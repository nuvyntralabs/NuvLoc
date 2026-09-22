using NuvLoc.Config;
using NuvLoc.Diff;
using NuvLoc.Resources;

namespace NuvLoc.Tests;

public sealed class ResxAndPlanTests
{
    [Fact]
    public void Resx_roundtrip_preserves_keys()
    {
        using var dir = new TempDir();
        var source = Path.Combine(dir.Path, "AppResources.resx");
        File.Copy(Fixture("AppResources.resx"), source);

        var adapter = new ResxAdapter();
        var entries = adapter.Read(source);
        Assert.Equal(3, entries.Count);
        Assert.Contains(entries, e => e.Key == "ItemsLeft" && e.Value.Contains("{0}"));

        var target = Path.Combine(dir.Path, "AppResources.es.resx");
        adapter.Write(target, [new ResourceEntry("Save", "Guardar", null)], source);
        var written = adapter.Read(target);
        Assert.Contains(written, e => e.Key == "Save" && e.Value == "Guardar");
        Assert.DoesNotContain(written, e => e.Key == "Cancel");
    }

    [Fact]
    public void Plan_marks_missing_and_stale()
    {
        using var dir = new TempDir();
        var source = Path.Combine(dir.Path, "AppResources.resx");
        File.Copy(Fixture("AppResources.resx"), source);
        File.WriteAllText(Path.Combine(dir.Path, "i18n.json"), """{"platform":"maui","source":"AppResources.resx","languages":["es"]}""");

        var adapter = new ResxAdapter();
        adapter.Write(
            Path.Combine(dir.Path, "AppResources.es.resx"),
            [new ResourceEntry("Save", "Guardar", null), new ResourceEntry("OldKey", "x", null)],
            source);

        var config = ConfigLoader.Load(Path.Combine(dir.Path, "i18n.json")).Config!;
        var planner = new LocalizationPlanner(adapter);
        var plan = planner.Build(config);
        var es = plan.Languages.Single();

        Assert.Contains(es.Missing, i => i.Key == "Cancel");
        Assert.Contains(es.Missing, i => i.Key == "ItemsLeft");
        Assert.Contains(es.Extra, k => k == "OldKey");
        Assert.Equal(1, es.Current);

        planner.WriteAcceptedHashes(config, plan);
        File.WriteAllText(source, File.ReadAllText(source).Replace(">Save<", ">Store<"));
        var stale = planner.Build(config).Languages.Single();
        Assert.Contains(stale.Stale, i => i.Key == "Save");
    }

    [Fact]
    public void Placeholder_mismatch_is_reported()
    {
        using var dir = new TempDir();
        var source = Path.Combine(dir.Path, "AppResources.resx");
        File.Copy(Fixture("AppResources.resx"), source);
        File.WriteAllText(Path.Combine(dir.Path, "i18n.json"), """{"platform":"maui","source":"AppResources.resx","languages":["es"]}""");

        var adapter = new ResxAdapter();
        adapter.Write(
            Path.Combine(dir.Path, "AppResources.es.resx"),
            [
                new ResourceEntry("Save", "Guardar", null),
                new ResourceEntry("Cancel", "Cancelar", null),
                new ResourceEntry("ItemsLeft", "broken", null),
            ],
            source);

        var config = ConfigLoader.Load(Path.Combine(dir.Path, "i18n.json")).Config!;
        var es = new LocalizationPlanner(adapter).Build(config).Languages.Single();
        Assert.Contains(es.PlaceholderBroken, i => i.Key == "ItemsLeft");
        Assert.True(es.HasGaps);
    }

    [Theory]
    [InlineData("You have {0} items", "Tienes {0} artículos", true)]
    [InlineData("You have {0} items", "Tienes artículos", false)]
    [InlineData("Hello {name}", "Hola {name}", true)]
    [InlineData("Hello {name}", "Hola {Name}", false)]
    public void Placeholder_tokens(string source, string target, bool expected) =>
        Assert.Equal(expected, PlaceholderValidator.Matches(source, target));

    static string Fixture(string name) =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "fixtures", name));
}
