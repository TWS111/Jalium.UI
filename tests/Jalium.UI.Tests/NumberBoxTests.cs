using Jalium.UI.Controls;
using Jalium.UI.Media;

namespace Jalium.UI.Tests;

public class NumberBoxTests
{
    [Fact]
    public void NumberBox_TextTrimming_DefaultsToCharacterEllipsis()
    {
        // Arrange & Act
        var numberBox = new NumberBox();

        // Assert
        Assert.Equal(TextTrimming.CharacterEllipsis, numberBox.TextTrimming);
    }
}
