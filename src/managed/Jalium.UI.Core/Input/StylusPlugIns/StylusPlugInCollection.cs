using System.Collections.ObjectModel;

namespace Jalium.UI.Input.StylusPlugIns;

/// <summary>
/// Collection of stylus plug-ins attached to a specific UIElement owner.
/// </summary>
public sealed class StylusPlugInCollection : Collection<StylusPlugIn>
{
    private readonly UIElement _owner;

    internal StylusPlugInCollection(UIElement owner)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
    }

    public UIElement Owner => _owner;

    protected override void InsertItem(int index, StylusPlugIn item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateAttach(item);
        base.InsertItem(index, item);
        item.Attach(_owner);
    }

    protected override void SetItem(int index, StylusPlugIn item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var previous = this[index];
        if (ReferenceEquals(previous, item))
        {
            return;
        }

        ValidateAttach(item);

        previous.Detach();
        base.SetItem(index, item);
        item.Attach(_owner);
    }

    protected override void RemoveItem(int index)
    {
        var item = this[index];
        item.Detach();
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            item.Detach();
        }

        base.ClearItems();
    }

    internal StylusPlugIn[] Snapshot() => [.. this];

    private void ValidateAttach(StylusPlugIn item)
    {
        if (Contains(item))
        {
            throw new InvalidOperationException("StylusPlugIn cannot be added to a collection more than once.");
        }

        if (item.Element != null && !ReferenceEquals(item.Element, _owner))
        {
            throw new InvalidOperationException("StylusPlugIn is already attached to another element.");
        }
    }
}
