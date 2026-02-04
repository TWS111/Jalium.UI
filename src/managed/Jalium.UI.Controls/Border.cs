using Jalium.UI.Media;

namespace Jalium.UI.Controls;

/// <summary>
/// Draws a border, background, or both around another element.
/// </summary>
[ContentProperty("Child")]
public class Border : FrameworkElement
{
    private UIElement? _child;

    #region Dependency Properties

    /// <summary>
    /// Identifies the Background dependency property.
    /// </summary>
    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Border),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    /// <summary>
    /// Identifies the BorderBrush dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(Border),
            new PropertyMetadata(null, OnVisualPropertyChanged));

    /// <summary>
    /// Identifies the BorderThickness dependency property.
    /// </summary>
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(Border),
            new PropertyMetadata(new Thickness(0), OnLayoutPropertyChanged));

    /// <summary>
    /// Identifies the CornerRadius dependency property.
    /// </summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Border),
            new PropertyMetadata(new CornerRadius(0), OnVisualPropertyChanged));

    /// <summary>
    /// Identifies the Padding dependency property.
    /// </summary>
    public static readonly DependencyProperty PaddingProperty =
        DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(Border),
            new PropertyMetadata(new Thickness(0), OnLayoutPropertyChanged));

    #endregion

    #region CLR Properties

    /// <summary>
    /// Gets or sets the background brush.
    /// </summary>
    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush.
    /// </summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness.
    /// </summary>
    public Thickness BorderThickness
    {
        get => (Thickness)(GetValue(BorderThicknessProperty) ?? new Thickness(0));
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)(GetValue(CornerRadiusProperty) ?? new CornerRadius(0));
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding inside the border.
    /// </summary>
    public Thickness Padding
    {
        get => (Thickness)(GetValue(PaddingProperty) ?? new Thickness(0));
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the child element.
    /// </summary>
    public UIElement? Child
    {
        get => _child;
        set
        {
            if (_child == value) return;

            if (_child != null)
            {
                RemoveVisualChild(_child);
            }

            _child = value;

            if (_child != null)
            {
                AddVisualChild(_child);
            }

            InvalidateMeasure();
        }
    }

    #endregion

    #region Layout

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var border = BorderThickness;
        var padding = Padding;
        var totalHorizontal = border.Left + border.Right + padding.Left + padding.Right;
        var totalVertical = border.Top + border.Bottom + padding.Top + padding.Bottom;

        var childAvailable = new Size(
            Math.Max(0, availableSize.Width - totalHorizontal),
            Math.Max(0, availableSize.Height - totalVertical));

        var childSize = Size.Empty;

        if (_child != null)
        {
            _child.Measure(childAvailable);
            childSize = _child.DesiredSize;
        }

        return new Size(
            childSize.Width + totalHorizontal,
            childSize.Height + totalVertical);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var border = BorderThickness;
        var padding = Padding;

        if (_child != null)
        {
            var childRect = new Rect(
                border.Left + padding.Left,
                border.Top + padding.Top,
                Math.Max(0, finalSize.Width - border.Left - border.Right - padding.Left - padding.Right),
                Math.Max(0, finalSize.Height - border.Top - border.Bottom - padding.Top - padding.Bottom));

            _child.Arrange(childRect);
            // Note: Do NOT call SetVisualBounds here - ArrangeCore already handles margin
        }

        return finalSize;
    }

    #endregion

    #region Rendering

    /// <inheritdoc />
    protected override void OnRender(object drawingContext)
    {
        if (drawingContext is not DrawingContext dc)
            return;

        var rect = new Rect(RenderSize);
        var border = BorderThickness;
        var cornerRadius = CornerRadius;
        var borderWidth = Math.Max(border.Left, Math.Max(border.Top, Math.Max(border.Right, border.Bottom)));
        var halfBorder = borderWidth / 2;

        // Draw backdrop effect (if any)
        var backdropEffect = BackdropEffect;
        if (backdropEffect != null && backdropEffect.HasEffect)
        {
            dc.DrawBackdropEffect(rect, backdropEffect, cornerRadius);
        }

        // Draw background - fill the entire area inside the border
        if (Background != null)
        {
            // Background fills the area inside the border stroke
            var backgroundRect = new Rect(
                halfBorder,
                halfBorder,
                Math.Max(0, rect.Width - borderWidth),
                Math.Max(0, rect.Height - borderWidth));

            // Adjust corner radius for inner background
            var innerRadius = new CornerRadius(
                Math.Max(0, cornerRadius.TopLeft - halfBorder),
                Math.Max(0, cornerRadius.TopRight - halfBorder),
                Math.Max(0, cornerRadius.BottomRight - halfBorder),
                Math.Max(0, cornerRadius.BottomLeft - halfBorder));

            dc.DrawRoundedRectangle(Background, null, backgroundRect, innerRadius);
        }

        // Draw border - stroke is centered on the edge
        if (BorderBrush != null && borderWidth > 0)
        {
            var pen = new Pen(BorderBrush, borderWidth);

            var borderRect = new Rect(
                halfBorder,
                halfBorder,
                Math.Max(0, rect.Width - borderWidth),
                Math.Max(0, rect.Height - borderWidth));

            // Adjust corner radius for border stroke position
            var strokeRadius = new CornerRadius(
                Math.Max(0, cornerRadius.TopLeft - halfBorder),
                Math.Max(0, cornerRadius.TopRight - halfBorder),
                Math.Max(0, cornerRadius.BottomRight - halfBorder),
                Math.Max(0, cornerRadius.BottomLeft - halfBorder));

            dc.DrawRoundedRectangle(null, pen, borderRect, strokeRadius);
        }
    }

    #endregion

    #region Property Changed Callbacks

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Border border)
        {
            border.InvalidateVisual();
        }
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Border border)
        {
            border.InvalidateMeasure();
        }
    }

    #endregion
}
