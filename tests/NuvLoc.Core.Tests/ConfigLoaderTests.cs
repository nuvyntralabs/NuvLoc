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

    [Theory]
    [InlineData("maui")]
    [InlineData("wpf")]
    [InlineData("winui")]
    [InlineData("avalonia")]
    [InlineData("uno")]
    public void Known_platforms_load(string platform)
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, $$"""{"platform":"{{platform}}","source":"AppResources.resx","languages":["es"]}""");

        var result = ConfigLoader.Load(config);
        Assert.True(result.Success, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(platform, result.Config!.Platform);
        Assert.EndsWith("AppResources.es.resx", result.Config.TargetPath("es"));
    }

    [Fact]
    public void Unknown_platform_fails()
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, """{"platform":"android","source":"AppResources.resx","languages":["es"]}""");

        var result = ConfigLoader.Load(config);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Message.Contains("android", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, e => e.Message.Contains("Supported:", StringComparison.Ordinal));
    }

    [Fact]
    public void Non_resx_source_fails()
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "strings.xml"), "<resources></resources>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, """{"platform":"maui","source":"strings.xml","languages":["es"]}""");

        var result = ConfigLoader.Load(config);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Message.Contains("sibling .resx", StringComparison.OrdinalIgnoreCase));
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

    [Theory]
    [InlineData("foo")]
    [InlineData("english")]
    [InlineData("123")]
    [InlineData("es_MX")]
    [InlineData("xx")]
    public void Invalid_language_code_fails(string language)
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, $$"""{"platform":"maui","source":"AppResources.resx","languages":["{{language}}"]}""");

        var result = ConfigLoader.Load(config);
        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Message.Contains(language, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, e => e.Message.Contains("Invalid language code", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, e => e.Message.Contains(LanguageCode.SkipFile, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("es")]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("it")]
    [InlineData("nl")]
    [InlineData("ja")]
    [InlineData("ko")]
    [InlineData("zh-Hans")]
    [InlineData("pt-BR")]
    [InlineData("ar")]
    [InlineData("hi")]
    [InlineData("ru")]
    public void Popular_language_codes_load(string language)
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, $$"""{"platform":"maui","source":"AppResources.resx","languages":["{{language}}"]}""");

        var result = ConfigLoader.Load(config);
        Assert.True(result.Success, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(language, result.Config!.Languages.Single());
    }

    [Fact]
    public void Canonicalizes_language_codes()
    {
        using var dir = new TempDir();
        File.WriteAllText(Path.Combine(dir.Path, "AppResources.resx"), "<root></root>");
        var config = Path.Combine(dir.Path, "i18n.json");
        File.WriteAllText(config, """{"platform":"maui","source":"AppResources.resx","languages":["PT-br","ES","es"]}""");

        var result = ConfigLoader.Load(config);
        Assert.True(result.Success, string.Join("; ", result.Errors.Select(e => e.Message)));
        Assert.Equal(["pt-BR", "es"], result.Config!.Languages);
    }
}
