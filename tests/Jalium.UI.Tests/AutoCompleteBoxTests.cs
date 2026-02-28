using Jalium.UI;
using Jalium.UI.Controls;
using Jalium.UI.Input;
using Jalium.UI.Media;

namespace Jalium.UI.Tests;

public class AutoCompleteBoxTests
{
    [Fact]
    public void AutoCompleteBox_TextTrimming_DefaultsToCharacterEllipsis()
    {
        // Arrange & Act
        var autoComplete = new AutoCompleteBox();

        // Assert
        Assert.Equal(TextTrimming.CharacterEllipsis, autoComplete.TextTrimming);
    }

    [Fact]
    public void AutoCompleteBox_TabAccept_ShouldNotInsertTabCharacter()
    {
        // Arrange
        var autoComplete = new AutoCompleteBox
        {
            ItemsSource = new[] { "apple", "apricot", "banana" },
            MinimumPrefixLength = 1
        };

        autoComplete.Text = "ap";
        Assert.True(autoComplete.IsDropDownOpen);

        // Act: Tab accepts the current suggestion.
        autoComplete.RaiseEvent(new KeyEventArgs(UIElement.KeyDownEvent, Key.Tab, ModifierKeys.None, true, false, 0));
        // Simulate follow-up text input event for the same key stroke.
        autoComplete.RaiseEvent(new TextCompositionEventArgs(UIElement.TextInputEvent, "\t", 1));

        // Assert
        Assert.Equal("apple", autoComplete.Text);
        Assert.DoesNotContain('\t', autoComplete.Text);
    }
}
