namespace Jalium.UI.Tests;

using ControlsApplication = Jalium.UI.Controls.Application;
using RootApplication = Jalium.UI.Application;

public class ApplicationInheritanceTests
{
    [Fact]
    public void ControlsApplication_ShouldBeInheritable()
    {
        Assert.False(typeof(ControlsApplication).IsSealed);
        Assert.True(typeof(DerivedControlsApplication).IsAssignableTo(typeof(ControlsApplication)));
    }

    [Fact]
    public void RootApplicationFacade_ShouldBeInheritable()
    {
        Assert.False(typeof(RootApplication).IsSealed);
        Assert.True(typeof(DerivedRootApplication).IsAssignableTo(typeof(RootApplication)));
    }

    private sealed class DerivedControlsApplication : ControlsApplication
    {
    }

    private sealed class DerivedRootApplication : RootApplication
    {
    }
}
