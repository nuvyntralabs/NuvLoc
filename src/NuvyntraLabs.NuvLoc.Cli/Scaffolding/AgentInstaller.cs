using NuvLoc;
using NuvyntraLabs.NuvLoc.Cli.Agents;
using NuvyntraLabs.NuvLoc.Cli.Workflow;

namespace NuvyntraLabs.NuvLoc.Cli.Scaffolding;

public static class AgentInstaller
{
    public static IReadOnlyList<string> Install(string payloadRoot, string projectDir, AiAgent agent)
    {
        var written = new List<string>();
        var commandsDir = Path.Combine(payloadRoot, "commands");

        foreach (var id in NuvLocCommands.Ids)
        {
            var bodyPath = Path.Combine(commandsDir, $"{id}.md");
            if (!File.Exists(bodyPath))
                throw new FileNotFoundException($"Missing command payload: {id}.md", bodyPath);

            var dest = Destination(projectDir, agent, id);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.WriteAllText(dest, Wrap(agent, id, File.ReadAllText(bodyPath)));
            written.Add(Path.GetRelativePath(projectDir, dest));
        }

        return written;
    }

    public static AiAgent? DetectInstalled(string projectDir)
    {
        foreach (var agent in AiAgent.All)
        {
            if (File.Exists(Destination(projectDir, agent, "translate")))
                return agent;
        }

        return null;
    }

    internal static string Destination(string projectDir, AiAgent agent, string id)
    {
        var folder = Path.Combine(projectDir, agent.CommandsDir.Replace('/', Path.DirectorySeparatorChar));
        return agent.Format switch
        {
            AgentFormat.Skill => Path.Combine(folder, $"nuvloc-{id}", "SKILL.md"),
            AgentFormat.Markdown => Path.Combine(folder, $"nuvloc.{id}.md"),
            AgentFormat.Toml => Path.Combine(folder, $"nuvloc.{id}.toml"),
            AgentFormat.Yaml => Path.Combine(folder, $"nuvloc.{id}.yaml"),
            _ => throw new ArgumentOutOfRangeException(nameof(agent), agent.Format, "Unknown agent format."),
        };
    }

    internal static string Wrap(AiAgent agent, string id, string body)
    {
        var slash = $"/nuvloc.{id}";
        var trimmed = body.Trim();
        var description = NuvLocCommands.Description(id);

        return agent.Format switch
        {
            AgentFormat.Toml => WrapToml(description, trimmed),
            AgentFormat.Yaml => WrapYaml(id, description, trimmed),
            _ => WrapMarkdown(agent, id, description, trimmed, slash),
        };
    }

    static string WrapToml(string description, string body)
    {
        var escaped = body.Replace("\\", "\\\\");
        return $"description = \"{description.Replace("\"", "'")}\"{Environment.NewLine}prompt = \"\"\"{Environment.NewLine}{escaped}{Environment.NewLine}\"\"\"{Environment.NewLine}";
    }

    static string WrapYaml(string id, string description, string body)
    {
        var indented = string.Join(Environment.NewLine, body.Split('\n').Select(line => "  " + line.TrimEnd('\r')));
        return
            $"""
            version: 1.0.0
            title: nuvloc.{id}
            description: "{description.Replace("\"", "'")}"
            author:
              contact: nuvloc
            parameters:
              - key: args
                input_type: string
                requirement: optional
                default: ""
                description: User input passed to the command.
            prompt: |2
            {indented}

            """;
    }

    static string WrapMarkdown(AiAgent agent, string id, string description, string body, string slash)
    {
        var nameLine = agent.IncludeName || agent.Format == AgentFormat.Skill
            ? $"name: nuvloc-{id}{Environment.NewLine}"
            : "";

        return
            $"""
            ---
            {nameLine}description: {description}
            ---

            # {slash}

            {body}

            {Disclaimer.Text}

            """;
    }
}
