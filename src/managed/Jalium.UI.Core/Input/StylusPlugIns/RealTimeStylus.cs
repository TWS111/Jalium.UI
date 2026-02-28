namespace Jalium.UI.Input.StylusPlugIns;

/// <summary>
/// Coordinates real-time stylus packet flow and plug-in execution.
/// </summary>
public sealed class RealTimeStylus
{
    private readonly UIElement _root;
    private readonly Dictionary<uint, StylusSession> _sessions = [];

    public RealTimeStylus(UIElement root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    public UIElement RootElement => _root;

    public RealTimeStylusProcessResult Process(
        uint pointerId,
        UIElement target,
        StylusInputAction action,
        StylusPointCollection stylusPoints,
        int timestamp,
        bool inAir,
        bool inRange,
        bool barrelButtonPressed,
        bool eraserPressed,
        bool inverted,
        bool pointerCanceled)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(stylusPoints);

        if (!_sessions.TryGetValue(pointerId, out var session))
        {
            session = new StylusSession();
            _sessions[pointerId] = session;
        }

        UIElement? previousTarget = session.Target;
        bool enteredRange = !session.InRange && inRange;
        bool exitedRange = session.InRange && !inRange;
        bool targetChanged = !ReferenceEquals(previousTarget, target);
        bool enteredElement = targetChanged;
        bool leftElement = targetChanged && previousTarget != null;
        bool barrelButtonDown = !session.BarrelButtonPressed && barrelButtonPressed;
        bool barrelButtonUp = session.BarrelButtonPressed && !barrelButtonPressed;

        var rawStylusInput = new RawStylusInput(
            pointerId,
            target,
            action,
            stylusPoints,
            timestamp,
            inAir,
            inRange,
            inverted,
            barrelButtonPressed,
            eraserPressed);

        ExecutePlugIns(rawStylusInput, target);

        if (pointerCanceled)
        {
            rawStylusInput.Cancel();
        }

        bool sessionEnded = rawStylusInput.IsCanceled || action == StylusInputAction.Up || !inRange;
        if (sessionEnded)
        {
            _sessions.Remove(pointerId);
        }
        else
        {
            session.Target = target;
            session.InRange = inRange;
            session.InAir = inAir;
            session.BarrelButtonPressed = barrelButtonPressed;
            session.Inverted = inverted;
            session.EraserPressed = eraserPressed;
        }

        return new RealTimeStylusProcessResult(
            rawStylusInput,
            previousTarget,
            enteredRange,
            exitedRange,
            enteredElement,
            leftElement,
            barrelButtonDown,
            barrelButtonUp,
            sessionEnded);
    }

    public void QueueProcessedCallbacks(RealTimeStylusProcessResult processResult)
    {
        ArgumentNullException.ThrowIfNull(processResult);

        var callbacks = processResult.RawStylusInput.DrainProcessedCallbacks();
        if (callbacks.Count == 0)
        {
            return;
        }

        var dispatcher = _root.Dispatcher;
        foreach (var stylusPlugIn in callbacks)
        {
            var plugIn = stylusPlugIn;
            dispatcher.BeginInvoke(() =>
            {
                try
                {
                    plugIn.InvokeProcessed(processResult.RawStylusInput);
                }
                catch
                {
                    // Processed-stage failures must not crash the input loop.
                }
            });
        }
    }

    public void CancelSession(uint pointerId)
    {
        _sessions.Remove(pointerId);
    }

    private void ExecutePlugIns(RawStylusInput rawStylusInput, UIElement target)
    {
        var path = BuildPathFromRootToTarget(target);
        for (int i = 0; i < path.Count; i++)
        {
            var plugIns = path[i].GetStylusPlugIns(createIfMissing: false);
            if (plugIns == null || plugIns.Count == 0)
            {
                continue;
            }

            foreach (var stylusPlugIn in plugIns.Snapshot())
            {
                if (!stylusPlugIn.ShouldProcess(rawStylusInput))
                {
                    continue;
                }

                try
                {
                    stylusPlugIn.InvokeInput(rawStylusInput);
                }
                catch
                {
                    rawStylusInput.Cancel();
                    return;
                }

                if (rawStylusInput.IsCanceled)
                {
                    return;
                }
            }
        }
    }

    private List<UIElement> BuildPathFromRootToTarget(UIElement target)
    {
        var path = new List<UIElement>(8);
        UIElement? current = target;
        while (current != null)
        {
            path.Add(current);
            if (ReferenceEquals(current, _root))
            {
                break;
            }

            current = current.VisualParent as UIElement;
        }

        path.Reverse();
        return path;
    }

    private sealed class StylusSession
    {
        public UIElement? Target { get; set; }
        public bool InRange { get; set; }
        public bool InAir { get; set; }
        public bool BarrelButtonPressed { get; set; }
        public bool Inverted { get; set; }
        public bool EraserPressed { get; set; }
    }
}

/// <summary>
/// Result object produced by RealTimeStylus processing.
/// </summary>
public sealed class RealTimeStylusProcessResult
{
    internal RealTimeStylusProcessResult(
        RawStylusInput rawStylusInput,
        UIElement? previousTarget,
        bool enteredRange,
        bool exitedRange,
        bool enteredElement,
        bool leftElement,
        bool barrelButtonDown,
        bool barrelButtonUp,
        bool sessionEnded)
    {
        RawStylusInput = rawStylusInput;
        PreviousTarget = previousTarget;
        EnteredRange = enteredRange;
        ExitedRange = exitedRange;
        EnteredElement = enteredElement;
        LeftElement = leftElement;
        BarrelButtonDown = barrelButtonDown;
        BarrelButtonUp = barrelButtonUp;
        SessionEnded = sessionEnded;
    }

    public RawStylusInput RawStylusInput { get; }
    public UIElement? PreviousTarget { get; }
    public bool EnteredRange { get; }
    public bool ExitedRange { get; }
    public bool EnteredElement { get; }
    public bool LeftElement { get; }
    public bool BarrelButtonDown { get; }
    public bool BarrelButtonUp { get; }
    public bool SessionEnded { get; }
    public bool Canceled => RawStylusInput.IsCanceled;
}
