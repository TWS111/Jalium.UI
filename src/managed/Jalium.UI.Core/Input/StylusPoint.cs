using System.Collections.ObjectModel;

namespace Jalium.UI.Input;

/// <summary>
/// Represents a single stylus sample point in the input pipeline.
/// </summary>
public struct StylusPoint
{
    public StylusPoint(double x, double y) : this(x, y, 0.5f) { }

    public StylusPoint(double x, double y, float pressureFactor)
    {
        X = x;
        Y = y;
        PressureFactor = Math.Clamp(pressureFactor, 0f, 1f);
    }

    public double X { get; set; }
    public double Y { get; set; }
    public float PressureFactor { get; set; }
}

/// <summary>
/// Collection of stylus sample points in the input pipeline.
/// </summary>
public sealed class StylusPointCollection : Collection<StylusPoint>
{
    public StylusPointCollection() { }

    public StylusPointCollection(IEnumerable<StylusPoint> points)
        : base(new List<StylusPoint>(points ?? throw new ArgumentNullException(nameof(points))))
    {
    }
}
