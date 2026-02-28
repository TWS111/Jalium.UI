using Jalium.UI;

namespace Jalium.UI.Controls;

/// <summary>
/// Exposes editor view metrics and navigation hooks for advanced editor hosts.
/// </summary>
public interface IEditorViewMetrics
{
    double VerticalOffset { get; }

    double HorizontalOffset { get; }

    double LineHeight { get; }

    double ViewportWidth { get; }

    double LineNumberAreaLeft { get; }

    double GutterWidth { get; }

    double FoldingLaneLeft { get; }

    double TextAreaLeft { get; }

    int FirstVisibleLineNumber { get; }

    Rect MinimapRect { get; }

    Rect VerticalScrollTrackRect { get; }

    Point GetPointFromOffset(int offset, bool showLineNumbers);

    double GetAbsoluteLineTop(int lineNumber);

    void SetVerticalOffset(double verticalOffset, bool allowAnimation, bool userInitiated);
}
