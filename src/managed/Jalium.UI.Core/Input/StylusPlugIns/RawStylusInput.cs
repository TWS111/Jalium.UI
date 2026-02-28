namespace Jalium.UI.Input.StylusPlugIns;

/// <summary>
/// Carries mutable stylus packet data through the stylus plug-in chain.
/// </summary>
public sealed class RawStylusInput
{
    private readonly Queue<StylusPlugIn> _processedCallbacks = new();
    private readonly HashSet<StylusPlugIn> _processedCallbackSet = new();
    private StylusPointCollection _stylusPoints;

    internal RawStylusInput(
        uint pointerId,
        UIElement target,
        StylusInputAction action,
        StylusPointCollection stylusPoints,
        int timestamp,
        bool inAir,
        bool inRange,
        bool inverted,
        bool barrelButtonPressed,
        bool eraserPressed)
    {
        PointerId = pointerId;
        Target = target;
        Action = action;
        Timestamp = timestamp;
        InAir = inAir;
        InRange = inRange;
        Inverted = inverted;
        IsBarrelButtonPressed = barrelButtonPressed;
        IsEraserPressed = eraserPressed;
        _stylusPoints = new StylusPointCollection(stylusPoints ?? throw new ArgumentNullException(nameof(stylusPoints)));
    }

    public uint PointerId { get; }
    public UIElement Target { get; }
    public StylusInputAction Action { get; }
    public int Timestamp { get; }
    public bool InAir { get; }
    public bool InRange { get; }
    public bool Inverted { get; }
    public bool IsBarrelButtonPressed { get; }
    public bool IsEraserPressed { get; }
    public bool IsCanceled { get; private set; }

    public StylusPointCollection GetStylusPoints() => new(_stylusPoints);

    public void SetStylusPoints(StylusPointCollection stylusPoints)
    {
        ArgumentNullException.ThrowIfNull(stylusPoints);
        _stylusPoints = new StylusPointCollection(stylusPoints);
    }

    public void Cancel() => IsCanceled = true;

    public void NotifyWhenProcessed(StylusPlugIn stylusPlugIn)
    {
        ArgumentNullException.ThrowIfNull(stylusPlugIn);

        if (_processedCallbackSet.Add(stylusPlugIn))
        {
            _processedCallbacks.Enqueue(stylusPlugIn);
        }
    }

    internal IReadOnlyList<StylusPlugIn> DrainProcessedCallbacks()
    {
        if (_processedCallbacks.Count == 0)
        {
            return Array.Empty<StylusPlugIn>();
        }

        var result = new List<StylusPlugIn>(_processedCallbacks.Count);
        while (_processedCallbacks.Count > 0)
        {
            result.Add(_processedCallbacks.Dequeue());
        }

        _processedCallbackSet.Clear();
        return result;
    }
}
