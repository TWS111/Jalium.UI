using System.Text;

namespace Jalium.UI.Tests;

public class WebViewDependencyTests
{
    [Fact]
    public void Repository_ShouldNotReference_MicrosoftWebWebView2_NuGetPackage()
    {
        var root = FindRepoRoot();
        var allowedRoots = new[]
        {
            Path.Combine(root, "src"),
            Path.Combine(root, "tests")
        };

        var projectFiles = allowedRoots
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "*.csproj", SearchOption.AllDirectories))
            .Where(path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains("\\.tmp\\", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(projectFiles);

        var offenders = new List<string>();
        foreach (var file in projectFiles)
        {
            var content = File.ReadAllText(file, Encoding.UTF8);
            if (content.Contains("Microsoft.Web.WebView2", StringComparison.OrdinalIgnoreCase))
            {
                offenders.Add(Path.GetRelativePath(root, file));
            }
        }

        Assert.True(
            offenders.Count == 0,
            $"Found forbidden Microsoft.Web.WebView2 package references: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void CurrentObjAssets_ShouldNotContain_MicrosoftWebWebView2Package()
    {
        var root = FindRepoRoot();
        var allowedRoots = new[]
        {
            Path.Combine(root, "src"),
            Path.Combine(root, "tests")
        };

        var assetsFiles = allowedRoots
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "project.assets.json", SearchOption.AllDirectories))
            .Where(path => path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains("\\.tmp\\", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var offenders = new List<string>();
        foreach (var file in assetsFiles)
        {
            var content = File.ReadAllText(file, Encoding.UTF8);
            if (content.Contains("microsoft.web.webview2", StringComparison.OrdinalIgnoreCase))
            {
                offenders.Add(Path.GetRelativePath(root, file));
            }
        }

        Assert.True(
            offenders.Count == 0,
            $"Found forbidden Microsoft.Web.WebView2 assets entries: {string.Join(", ", offenders)}");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "src", "managed", "Jalium.UI.Controls")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root.");
    }
}
