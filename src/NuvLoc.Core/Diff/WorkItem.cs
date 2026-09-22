namespace NuvLoc.Diff;

public sealed record WorkItem(string Key, string Value, string? Comment, string? PreviousSource = null);
