namespace NuvyntraLabs.NuvLoc.Cli.Agents;

public sealed record AiAgent(
    string Id,
    string DisplayName,
    string CommandsDir,
    AgentFormat Format,
    IReadOnlyList<string>? Aliases = null,
    bool IncludeName = false)
{
    public string PickerLabel => $"{Id} ({DisplayName})";

    public static readonly AiAgent Agy = Skill("agy", "Antigravity", ".agents/skills");
    public static readonly AiAgent Alquimia = Skill("alquimia", "Alquimia AI", ".alquimia/skills");
    public static readonly AiAgent Amp = Markdown("amp", "Amp", ".agents/commands");
    public static readonly AiAgent Auggie = Markdown("auggie", "Auggie CLI", ".augment/commands");
    public static readonly AiAgent Bob = Skill("bob", "IBM Bob", ".bob/skills");
    public static readonly AiAgent Claude = Markdown("claude", "Claude Code", ".claude/commands");
    public static readonly AiAgent Cline = Markdown("cline", "Cline", ".clinerules/workflows");
    public static readonly AiAgent CodeBuddy = Markdown("codebuddy", "CodeBuddy CLI", ".codebuddy/commands");
    public static readonly AiAgent Codex = Skill("codex", "Codex CLI", ".agents/skills");
    public static readonly AiAgent CommandCode = Skill("command-code", "Command Code", ".commandcode/skills");
    public static readonly AiAgent Copilot = Skill("copilot", "GitHub Copilot", ".github/skills");
    public static readonly AiAgent Cursor = Skill("cursor", "Cursor", ".cursor/skills", "cursor-agent");
    public static readonly AiAgent Devin = Skill("devin", "Devin for Terminal", ".devin/skills");
    public static readonly AiAgent DockerAgent = Skill("docker-agent", "Docker Agent", ".agents/skills");
    public static readonly AiAgent Droid = Skill("droid", "Factory Droid", ".factory/skills");
    public static readonly AiAgent Dsh = Skill("dsh", "DeepSeek Harness", ".dsh/skills");
    public static readonly AiAgent Firebender = Markdown("firebender", "Firebender", ".firebender/commands");
    public static readonly AiAgent Forge = new("forge", "Forge", ".forge/commands", AgentFormat.Markdown, IncludeName: true);
    public static readonly AiAgent Gemini = new("gemini", "Gemini CLI", ".gemini/commands", AgentFormat.Toml);
    public static readonly AiAgent Generic = Markdown("generic", "Generic", ".agents/commands");
    public static readonly AiAgent Goose = new("goose", "Goose", ".goose/recipes", AgentFormat.Yaml);
    public static readonly AiAgent Grok = Skill("grok", "Grok Build", ".grok/skills");
    public static readonly AiAgent Hermes = Skill("hermes", "Hermes", ".hermes/skills");
    public static readonly AiAgent Iflow = Markdown("iflow", "iFlow CLI", ".iflow/commands");
    public static readonly AiAgent Junie = Markdown("junie", "Junie", ".junie/commands");
    public static readonly AiAgent KiloCode = Markdown("kilocode", "Kilo Code", ".kilo/commands");
    public static readonly AiAgent Kimi = Skill("kimi", "Kimi Code", ".kimi-code/skills");
    public static readonly AiAgent KiroCli = Markdown("kiro-cli", "Kiro CLI", ".kiro/prompts", "kiro");
    public static readonly AiAgent Lingma = Skill("lingma", "Lingma", ".lingma/skills");
    public static readonly AiAgent Muse = Skill("muse", "Muse Code", ".agents/skills");
    public static readonly AiAgent Omp = Markdown("omp", "Oh My Pi", ".omp/commands");
    public static readonly AiAgent OpenCode = Markdown("opencode", "opencode", ".opencode/commands");
    public static readonly AiAgent Pi = Markdown("pi", "Pi Coding Agent", ".pi/prompts");
    public static readonly AiAgent QoderCli = Skill("qodercli", "Qoder CLI", ".qoder/skills");
    public static readonly AiAgent Qwen = Markdown("qwen", "Qwen Code", ".qwen/commands");
    public static readonly AiAgent Roo = Markdown("roo", "Roo Code", ".roo/commands");
    public static readonly AiAgent RovoDev = Skill("rovodev", "RovoDev", ".rovodev/skills");
    public static readonly AiAgent Shai = Markdown("shai", "SHAI", ".shai/commands");
    public static readonly AiAgent Tabnine = Markdown("tabnine", "Tabnine CLI", ".tabnine/agent/commands");
    public static readonly AiAgent Trae = Skill("trae", "Trae", ".trae/skills");
    public static readonly AiAgent Vibe = Skill("vibe", "Mistral Vibe", ".vibe/skills");
    public static readonly AiAgent Windsurf = Markdown("windsurf", "Windsurf", ".windsurf/workflows");
    public static readonly AiAgent Zcode = Skill("zcode", "ZCode", ".zcode/skills");
    public static readonly AiAgent Zed = Skill("zed", "Zed", ".agents/skills");

    public static readonly IReadOnlyList<AiAgent> All =
    [
        Agy, Alquimia, Amp, Auggie, Bob, Claude, Cline, CodeBuddy, Codex, CommandCode,
        Copilot, Cursor, Devin, DockerAgent, Droid, Dsh, Firebender, Forge, Gemini, Generic,
        Goose, Grok, Hermes, Iflow, Junie, KiloCode, Kimi, KiroCli, Lingma, Muse, Omp,
        OpenCode, Pi, QoderCli, Qwen, Roo, RovoDev, Shai, Tabnine, Trae, Vibe, Windsurf,
        Zcode, Zed,
    ];

    public static AiAgent? Find(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return All.FirstOrDefault(a =>
            a.Id.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            a.DisplayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            a.PickerLabel.Equals(trimmed, StringComparison.OrdinalIgnoreCase) ||
            (a.Aliases is not null && a.Aliases.Any(alias =>
                alias.Equals(trimmed, StringComparison.OrdinalIgnoreCase))));
    }

    static AiAgent Skill(string id, string displayName, string dir, params string[] aliases) =>
        new(id, displayName, dir, AgentFormat.Skill, aliases.Length == 0 ? null : aliases, IncludeName: true);

    static AiAgent Markdown(string id, string displayName, string dir, params string[] aliases) =>
        new(id, displayName, dir, AgentFormat.Markdown, aliases.Length == 0 ? null : aliases);
}
