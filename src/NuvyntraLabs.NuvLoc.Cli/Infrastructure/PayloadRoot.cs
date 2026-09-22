namespace NuvyntraLabs.NuvLoc.Cli.Infrastructure;

public static class PayloadRoot
{
    public static string Find(string? overrideRoot = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideRoot) && Directory.Exists(Path.Combine(overrideRoot, "commands")))
            return Path.GetFullPath(overrideRoot);

        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, "payload");
            if (Directory.Exists(Path.Combine(candidate, "commands")))
                return candidate;

            var parent = Directory.GetParent(dir);
            if (parent is null)
                break;
            dir = parent.FullName;
        }

        throw new DirectoryNotFoundException("NuvLoc payload/commands was not found next to the tool or in the repo.");
    }
}
