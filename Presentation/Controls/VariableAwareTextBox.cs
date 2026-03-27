#nullable enable
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Postgirl.Presentation.Controls;

[TemplatePart(Name = PartEditor, Type = typeof(RichTextBox))]
public class VariableAwareTextBox : Control
{
    private const string PartEditor = "PART_Editor";

    private static readonly Regex VariablePattern =
        new(@"\{\{([\w\-\.]+)\}\}", RegexOptions.Compiled);

    private const double SingleLinePageWidth = 100_000;

    private RichTextBox? _editor;
    private bool _isUpdating;

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

    #region HighlightBrush

    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register(
            nameof(HighlightBrush), typeof(Brush), typeof(VariableAwareTextBox),
            new PropertyMetadata(Brushes.Gold, OnRenderPropertyChanged));

    /// <summary>
    /// The brush applied to <c>{{variable}}</c> tokens.
    /// Overridden per-token by <see cref="VariableBrushSelector"/> when set.
    /// </summary>
    public Brush HighlightBrush
    {
        get => (Brush)GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    #endregion

    #region VariableBrushSelector

    public static readonly DependencyProperty VariableBrushSelectorProperty =
        DependencyProperty.Register(
            nameof(VariableBrushSelector), typeof(Func<string, Brush>), typeof(VariableAwareTextBox),
            new PropertyMetadata(null, OnRenderPropertyChanged));

    /// <summary>
    /// Optional per-variable brush selector. Receives the variable name (without <c>{{ }}</c>)
    /// and returns the brush to use. Falls back to <see cref="HighlightBrush"/> when <c>null</c>.
    /// Raise <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> for this
    /// property whenever the underlying condition changes to trigger a re-render.
    /// </summary>
    public Func<string, Brush>? VariableBrushSelector
    {
        get => (Func<string, Brush>?)GetValue(VariableBrushSelectorProperty);
        set => SetValue(VariableBrushSelectorProperty, value);
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

    private static void OnRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is VariableAwareTextBox tb)
            tb.RebuildDocument();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_editor != null)
            _editor.TextChanged -= OnEditorTextChanged;

        _editor = GetTemplateChild(PartEditor) as RichTextBox;

        if (_editor != null)
        {
            _editor.Document.PagePadding = new Thickness(0);
            _editor.IsReadOnly = IsReadOnly;
            _editor.AcceptsReturn = AcceptsReturn;
            ApplySingleLineConstraint();
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
            var caretOffset = GetCaretOffset();

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
            var brush = VariableBrushSelector?.Invoke(variableName) ?? HighlightBrush;
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
}