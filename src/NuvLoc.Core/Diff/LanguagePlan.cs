namespace NuvLoc.Diff;

public sealed class LanguagePlan
{
    public required string Language { get; init; }
    public required string TargetPath { get; init; }
    public required IReadOnlyList<WorkItem> Missing { get; init; }
    public required IReadOnlyList<WorkItem> Stale { get; init; }
    public required IReadOnlyList<WorkItem> PlaceholderBroken { get; init; }
    public required IReadOnlyList<string> Extra { get; init; }
    public required int Current { get; init; }

    public int Total => Missing.Count + Stale.Count + Extra.Count + Current;

    public bool HasGaps => Missing.Count > 0 || Stale.Count > 0 || PlaceholderBroken.Count > 0;
}
