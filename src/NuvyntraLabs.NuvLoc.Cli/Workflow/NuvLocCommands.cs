namespace NuvyntraLabs.NuvLoc.Cli.Workflow;

public static class NuvLocCommands
{
    public static readonly IReadOnlyList<string> Ids = ["status", "translate"];

    public static string Description(string id) => id switch
    {
        "status" => "Show i18n coverage from nuvloc status. Does not translate. Completeness is not correctness.",
        "translate" => "Translate missing and stale keys from nuvloc plan. Review with a native speaker before ship.",
        _ => $"NuvLoc /nuvloc.{id}",
    };
}
