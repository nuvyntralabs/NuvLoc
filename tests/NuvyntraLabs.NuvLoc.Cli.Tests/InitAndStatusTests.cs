using NuvLoc;
using NuvLoc.Config;
using NuvLoc.Resources;

namespace NuvyntraLabs.NuvLoc.Cli.Tests;

public sealed class InitAndStatusTests
{
    [Fact]
    public async Task Init_without_config_is_usage()
    {
        using var dir = new TempDir();
        var (exit, _, err) = await Run(["init", "--path", dir.Path, "--agent", "cursor", "--ci"]);
        Assert.Equal(ExitCodes.Usage, exit);
        Assert.Contains("not found", err, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Init_writes_cursor_skills()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);

        var (exit, output, err) = await Run(
            ["init", "--path", dir.Path, "--configfile", "i18n.json", "--agent", "cursor", "--ci"]);

        Assert.Equal(ExitCodes.Success, exit);
        Assert.True(string.IsNullOrWhiteSpace(err), err);
        Assert.True(File.Exists(Path.Combine(dir.Path, ".cursor", "skills", "nuvloc-translate", "SKILL.md")));
        Assert.True(File.Exists(Path.Combine(dir.Path, ".cursor", "skills", "nuvloc-status", "SKILL.md")));
        Assert.True(File.Exists(Path.Combine(dir.Path, ".nuvloc", "rules.md")));
        Assert.True(File.Exists(Path.Combine(dir.Path, ".nuvloc", "reference", "dotnet-resx.md")));
        Assert.False(File.Exists(Path.Combine(dir.Path, ".nuvloc", "reference", "maui-resx.md")));
        Assert.Contains("/nuvloc.translate", output, StringComparison.Ordinal);
        Assert.Contains(Disclaimer.Text, output, StringComparison.Ordinal);
        Assert.Contains(Disclaimer.Text, File.ReadAllText(Path.Combine(dir.Path, ".cursor", "skills", "nuvloc-translate", "SKILL.md")), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_lists_missing_keys()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);

        var (exit, output, _) = await Run(
            ["status", "--path", dir.Path, "--configfile", "i18n.json", "--format", "json", "--no-update-check"]);

        Assert.Equal(ExitCodes.Success, exit);
        Assert.Contains("ItemsLeft", output, StringComparison.Ordinal);
        Assert.Contains(Disclaimer.Text, output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Status_rejects_invalid_lang_flag()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);

        var (exit, _, err) = await Run(
            ["status", "--path", dir.Path, "--configfile", "i18n.json", "--lang", "foo", "--no-update-check"]);

        Assert.Equal(ExitCodes.Usage, exit);
        Assert.Contains("Invalid language code 'foo'", err, StringComparison.Ordinal);
        Assert.Contains("not a BCP-47 culture", err, StringComparison.Ordinal);
        Assert.Contains(LanguageCode.SkipFile, err, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(dir.Path, "AppResources.foo.resx")));
    }

    [Fact]
    public async Task Invalid_language_in_config_skips_culture_file()
    {
        using var dir = new TempDir();
        File.Copy(Fixture("AppResources.resx"), Path.Combine(dir.Path, "AppResources.resx"));
        File.WriteAllText(Path.Combine(dir.Path, "i18n.json"), """{"platform":"maui","source":"AppResources.resx","languages":["es","foo"]}""");

        var (exit, _, err) = await Run(
            ["status", "--path", dir.Path, "--configfile", "i18n.json", "--no-update-check"]);

        Assert.Equal(ExitCodes.Usage, exit);
        Assert.Contains("not a BCP-47 culture", err, StringComparison.Ordinal);
        Assert.Contains(LanguageCode.SkipFile, err, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(dir.Path, "AppResources.foo.resx")));
    }

    [Fact]
    public async Task Status_rejects_lang_not_in_config()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);

        var (exit, _, err) = await Run(
            ["status", "--path", dir.Path, "--configfile", "i18n.json", "--lang", "de", "--no-update-check"]);

        Assert.Equal(ExitCodes.Usage, exit);
        Assert.Contains("Unknown language 'de'", err, StringComparison.Ordinal);
        Assert.Contains("Configured: es", err, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Check_fails_when_keys_are_missing()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);

        var (exit, _, _) = await Run(
            ["check", "--path", dir.Path, "--configfile", "i18n.json", "--ci", "--no-update-check"]);

        Assert.Equal(ExitCodes.Failed, exit);
    }

    [Fact]
    public async Task Check_passes_when_complete()
    {
        using var dir = new TempDir();
        SeedProject(dir.Path);
        var adapter = new ResxAdapter();
        var source = Path.Combine(dir.Path, "AppResources.resx");
        adapter.Write(
            Path.Combine(dir.Path, "AppResources.es.resx"),
            [
                new ResourceEntry("Save", "Guardar", null),
                new ResourceEntry("Cancel", "Cancelar", null),
                new ResourceEntry("ItemsLeft", "Tienes {0} artículos", "count"),
            ],
            source);

        var (exit, output, _) = await Run(
            ["check", "--path", dir.Path, "--configfile", "i18n.json", "--format", "human", "--no-update-check"]);

        Assert.Equal(ExitCodes.Success, exit);
        Assert.Contains(Disclaimer.Text, output, StringComparison.Ordinal);
    }

    static void SeedProject(string dir)
    {
        File.Copy(Fixture("AppResources.resx"), Path.Combine(dir, "AppResources.resx"));
        File.WriteAllText(Path.Combine(dir, "i18n.json"), """{"platform":"maui","source":"AppResources.resx","languages":["es"]}""");
    }

    static async Task<(int Exit, string Output, string Error)> Run(string[] args)
    {
        var stdout = new StringWriter();
        var stderr = new StringWriter();
        var exit = await NuvLocApp.RunAsync(args, stdout, stderr, new StringReader(""), Payload());
        return (exit, stdout.ToString(), stderr.ToString());
    }

    static string Payload() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "payload"));

    static string Fixture(string name) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "fixtures", name));
}

public sealed class TempDir : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "nuvloc-cli-tests", Guid.NewGuid().ToString("N"));

    public TempDir() => Directory.CreateDirectory(Path);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
