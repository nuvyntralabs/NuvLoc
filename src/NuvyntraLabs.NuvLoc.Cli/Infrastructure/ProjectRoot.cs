namespace NuvyntraLabs.NuvLoc.Cli.Infrastructure;

public static class ProjectRoot
{
    public static string Resolve(string? path) =>
        Path.GetFullPath(string.IsNullOrWhiteSpace(path) ? Directory.GetCurrentDirectory() : path);
}
