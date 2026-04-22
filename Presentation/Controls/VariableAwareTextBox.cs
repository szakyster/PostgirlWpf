#nullable enable
using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Postgirl.Services;

namespace Postgirl.Presentation.Controls;

[TemplatePart(Name = PartEditor, Type = typeof(RichTextBox))]
public class VariableAwareTextBox : Control
{
    private const string PartEditor = "PART_Editor";

    private static readonly Regex VariablePattern =
        new(@"\{\{([\w\-\.]+)\}\}", RegexOptions.Compiled);

    private const double SingleLinePageWidth = 100_000;

    private VariablesService? _variablesService;
    private RichTextBox? _editor;
    private bool _isUpdating;
    private int _savedCaretOffset;
    private int? _pendingCaretOffset;

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(VariableAwareTextBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnTextPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VariableAwareTextBox tb && !tb._isUpdating)
            tb.RebuildDocument();
    }

    #endregion

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(VariableAwareTextBox),
            new PropertyMetadata(false, OnIsReadOnlyChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VariableAwareTextBox tb && tb._editor != null)
            tb._editor.IsReadOnly = (bool)e.NewValue;
    }

    #endregion

    #region AcceptsReturn

    public static readonly DependencyProperty AcceptsReturnProperty =
        DependencyProperty.Register(
            nameof(AcceptsReturn), typeof(bool), typeof(VariableAwareTextBox),
            new PropertyMetadata(false, OnAcceptsReturnChanged));

    public bool AcceptsReturn
    {
        get => (bool)GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    private static void OnAcceptsReturnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VariableAwareTextBox tb && tb._editor != null)
        {
            tb._editor.AcceptsReturn = (bool)e.NewValue;
            tb.ApplySingleLineConstraint();
        }
    }

    #endregion

    private void ApplySingleLineConstraint()
    {
        if (_editor == null) return;
        _editor.Document.PageWidth = AcceptsReturn
            ? double.NaN
            : SingleLinePageWidth;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_editor != null)
        {
            _editor.TextChanged -= OnEditorTextChanged;
            _editor.ContextMenuOpening -= OnContextMenuOpening;
        }

        if (_variablesService != null)
            _variablesService.Items.CollectionChanged -= OnVariablesChanged;

        _variablesService = App.AppHost?.Services.GetService<VariablesService>();

        if (_variablesService != null)
            _variablesService.Items.CollectionChanged += OnVariablesChanged;

        _editor = GetTemplateChild(PartEditor) as RichTextBox;

        if (_editor != null)
        {
            _editor.Document.PagePadding = new Thickness(0);
            _editor.IsReadOnly = IsReadOnly;
            _editor.AcceptsReturn = AcceptsReturn;
            _editor.ContextMenu = new ContextMenu();
            ApplySingleLineConstraint();
            _editor.ContextMenuOpening += OnContextMenuOpening;
            _editor.TextChanged += OnEditorTextChanged;
            RebuildDocument();
        }
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        _isUpdating = true;
        try
        {
            var raw = new TextRange(_editor!.Document.ContentStart, _editor.Document.ContentEnd).Text;
            // RichTextBox always appends \r\n after the last paragraph — strip it
            var text = raw.EndsWith("\r\n") ? raw[..^2] : raw;

            // Single-line mode: strip any stray newlines (e.g. from paste)
            if (!AcceptsReturn)
                text = text.ReplaceLineEndings(string.Empty);

            Text = text;
        }
        finally
        {
            _isUpdating = false;
        }

        RebuildDocument();
    }

    private void RebuildDocument()
    {
        if (_editor == null) return;

        _editor.TextChanged -= OnEditorTextChanged;
        try
        {
            var caretOffset = _pendingCaretOffset ?? GetCaretOffset();
            _pendingCaretOffset = null;

            _editor.Document.Blocks.Clear();

            var text = Text ?? string.Empty;
            var lines = AcceptsReturn
                ? text.Split(["\r\n", "\n"], StringSplitOptions.None)
                : [text];

            foreach (var line in lines)
            {
                var paragraph = new Paragraph { Margin = new Thickness(0) };
                AppendHighlightedInlines(paragraph, line);
                _editor.Document.Blocks.Add(paragraph);
            }

            RestoreCaretOffset(caretOffset);
        }
        finally
        {
            _editor.TextChanged += OnEditorTextChanged;
        }
    }

    private void AppendHighlightedInlines(Paragraph paragraph, string text)
    {
        var lastIndex = 0;

        foreach (Match match in VariablePattern.Matches(text))
        {
            if (match.Index > lastIndex)
                paragraph.Inlines.Add(new Run(text[lastIndex..match.Index]) { Foreground = Foreground });

            var variableName = match.Groups[1].Value;
            var brush = GetVariableBrush(variableName);
            paragraph.Inlines.Add(new Run(match.Value) { Foreground = brush });

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
            paragraph.Inlines.Add(new Run(text[lastIndex..]) { Foreground = Foreground });
    }

    private int GetCaretOffset()
    {
        if (_editor == null) return 0;
        return new TextRange(_editor.Document.ContentStart, _editor.CaretPosition).Text.Length;
    }

    private void RestoreCaretOffset(int offset)
    {
        if (_editor == null) return;

        foreach (var block in _editor.Document.Blocks)
        {
            if (block is not Paragraph paragraph) continue;

            var paraLength = new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.Length;

            if (offset <= paraLength)
            {
                RestoreCaretInParagraph(paragraph, offset);
                return;
            }

            offset -= paraLength + 2; // +2 for the \r\n paragraph separator in TextRange.Text
        }

        _editor.CaretPosition = _editor.Document.ContentEnd;
    }

    private void RestoreCaretInParagraph(Paragraph paragraph, int offset)
    {
        if (offset == 0)
        {
            _editor!.CaretPosition = paragraph.ContentStart;
            return;
        }

        foreach (var inline in paragraph.Inlines)
        {
            if (inline is not Run run) continue;

            if (offset <= run.Text.Length)
            {
                _editor!.CaretPosition = run.ContentStart.GetPositionAtOffset(offset);
                return;
            }

            offset -= run.Text.Length;
        }

        _editor!.CaretPosition = paragraph.ContentEnd;
    }

    private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        _savedCaretOffset = GetCaretOffset();

        var menu = _editor!.ContextMenu!;
        menu.Items.Clear();

        var variables = _variablesService?.Items;

        if (variables == null || variables.Count == 0)
        {
            menu.Items.Add(new MenuItem { Header = "(No variables defined)", IsEnabled = false });
            return;
        }

        foreach (var entry in variables)
        {
            var item = new MenuItem { Header = entry.Key };
            var captured = entry.Key;
            item.Click += (_, _) => InsertVariable(captured);
            menu.Items.Add(item);
        }
    }

    private void OnVariablesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => RebuildDocument();

    private Brush GetVariableBrush(string variableName)
    {
        var exists = _variablesService?.VariableExists(variableName) ?? false;
        var key = exists ? "Brush.Success" : "Brush.Warning";
        return Application.Current.Resources[key] as Brush ?? Brushes.Gold;
    }

    private void InsertVariable(string variableName)
    {
        var insertion = $"{{{{{variableName}}}}}";
        var text = Text ?? string.Empty;
        var insertPos = Math.Clamp(_savedCaretOffset, 0, text.Length);

        _pendingCaretOffset = insertPos + insertion.Length;
        Text = text[..insertPos] + insertion + text[insertPos..];
    }
}