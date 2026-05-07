using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Postgirl.Presentation.Behaviors;

public static class WindowChromeBehavior
{
    public static readonly DependencyProperty IsDragRegionProperty = DependencyProperty.RegisterAttached(
        "IsDragRegion",
        typeof(bool),
        typeof(WindowChromeBehavior),
        new PropertyMetadata(false, OnIsDragRegionChanged));

    public static readonly DependencyProperty IsCloseButtonProperty = DependencyProperty.RegisterAttached(
        "IsCloseButton",
        typeof(bool),
        typeof(WindowChromeBehavior),
        new PropertyMetadata(false, OnIsCloseButtonChanged));

    public static bool GetIsDragRegion(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsDragRegionProperty);
    }

    public static void SetIsDragRegion(DependencyObject obj, bool value)
    {
        obj.SetValue(IsDragRegionProperty, value);
    }

    public static bool GetIsCloseButton(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsCloseButtonProperty);
    }

    public static void SetIsCloseButton(DependencyObject obj, bool value)
    {
        obj.SetValue(IsCloseButtonProperty, value);
    }

    private static void OnIsDragRegionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
        {
            return;
        }

        if ((bool)e.OldValue)
        {
            element.MouseLeftButtonDown -= OnDragRegionMouseLeftButtonDown;
        }

        if ((bool)e.NewValue)
        {
            element.MouseLeftButtonDown += OnDragRegionMouseLeftButtonDown;
        }
    }

    private static void OnIsCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ButtonBase button)
        {
            return;
        }

        if ((bool)e.OldValue)
        {
            button.Click -= OnCloseButtonClick;
        }

        if ((bool)e.NewValue)
        {
            button.Click += OnCloseButtonClick;
        }
    }

    private static void OnDragRegionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DependencyObject dependencyObject)
        {
            return;
        }

        var window = Window.GetWindow(dependencyObject);

        if (window == null)
        {
            return;
        }

        if (e.ClickCount == 2 && (window.ResizeMode == ResizeMode.CanResize || window.ResizeMode == ResizeMode.CanResizeWithGrip))
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            window.DragMove();
        }
    }

    private static void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not DependencyObject dependencyObject)
        {
            return;
        }

        var window = Window.GetWindow(dependencyObject);

        if (window != null)
        {
            window.Close();
        }
    }
}
