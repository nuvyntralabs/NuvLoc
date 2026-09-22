using NuvLoc.Config;

namespace NuvLoc.Tests;

public sealed class ConfigLoaderTests
{
    [Fact]
    public void Missing_file_fails()
    {
        var result = ConfigLoader.Load(Path.Combine(Path.GetTempPath(), "nuvloc-missing-i18n.json"));
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Message.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Valid_config_loads()
    {
        using var dir = new TempDir();
        var source = Path.Combine(dir.Path, "AppResources.resx");
        File.WriteAllText(source, "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, """{"platform":"maui","source":"AppResources.resx","languages":["es","fr"]}""");

        var result = ConfigLoader.Load(config);
        Assert.True(result.Success, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal("maui", result.Config!.Platform);
        Assert.Equal(["es", "fr"], result.Config.Languages);
        Assert.Equal(Path.GetFullPath(source), result.Config.SourceFullPath);
        Assert.EndsWith("AppResources.es.resx", result.Config.TargetPath("es"));
    }

    [Fact]
    public void English_in_languages_fails()
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, """{"platform":"maui","source":"AppResources.resx","languages":["en","es"]}""");

        var result = ConfigLoader.Load(config);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Message.Contains("English", StringComparison.OrdinalIgnoreCase));
    }
}
