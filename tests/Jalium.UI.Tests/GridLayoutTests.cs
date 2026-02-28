using Jalium.UI;
using Jalium.UI.Controls;

namespace Jalium.UI.Tests;

public class GridLayoutTests
{
    [Fact]
    public void Grid_AutoRow_ShouldTrackChildHeight_AfterFinalCellWidthMeasure()
    {
        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto, MaxWidth = 120 });

        var text = new TextBlock
        {
            Text = string.Join(" ", Enumerable.Repeat("longword", 40)),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14
        };

        grid.Children.Add(text);
        grid.Measure(new Size(500, double.PositiveInfinity));

        Assert.True(text.DesiredSize.Height > 30, $"Expected wrapped text height, got {text.DesiredSize.Height}");
        Assert.True(
            grid.DesiredSize.Height + 0.01 >= text.DesiredSize.Height,
            $"Grid height {grid.DesiredSize.Height} should include wrapped text height {text.DesiredSize.Height}");
    }
}
