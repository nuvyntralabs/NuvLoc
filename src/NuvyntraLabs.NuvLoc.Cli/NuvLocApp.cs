using System.CommandLine;
using NuvLoc;
using NuvLoc.Config;
using NuvLoc.Diff;
using NuvLoc.Reporting;
using NuvyntraLabs.NuvLoc.Cli.Agents;
using NuvyntraLabs.NuvLoc.Cli.Infrastructure;
using NuvyntraLabs.NuvLoc.Cli.Scaffolding;

namespace NuvyntraLabs.NuvLoc.Cli;

public static class NuvLocApp
{
    public const string Usage =
        """
        nuvloc — agent-driven localization (MAUI .resx)

          nuvloc init        Check i18n.json and install /nuvloc.status + /nuvloc.translate
          nuvloc update      Refresh skills and .nuvloc/reference
          nuvloc agent add   Install a second coding agent
          nuvloc plan        JSON / human worklist (missing, stale, extra)
          nuvloc status      Coverage table
          nuvloc check       CI gate (exit 1 on missing, stale, or broken placeholders)
          nuvloc version     Tool version

        Options
          --configfile <path>   Default i18n.json
          --path <dir>          Project directory (default: cwd)
          --agent <id>          Coding agent (cursor, copilot, claude, …)
          --format human|json   plan / status / check
          --lang <code>         Repeatable culture filter
          --ci                  No prompts
          --force               Overwrite skill files (never culture .resx)
          --write-cache         After check, record source hashes for present keys
          --no-update-check     Skip the nuget.org self-update prompt

        There is no nuvloc translate command. Translation runs in the agent:
          /nuvloc.status
          /nuvloc.translate

        """ + Disclaimer.Text;

    public static Task<int> RunAsync(IReadOnlyList<string> args, TextWriter stdout, TextWriter stderr) =>
        RunAsync(args, stdout, stderr, Console.In, payloadRoot: null);

    public static async Task<int> RunAsync(
        IReadOnlyList<string> args,
        TextWriter stdout,
        TextWriter stderr,
        TextReader stdin,
        string? payloadRoot)
    {
        var remaining = ToolUpdateCheck.ConsumeNoUpdateCheckFlag(args, out var noUpdateCheck);
        var update = ToolUpdateCheck.Run(new UpdateCheckOptions
        {
            ToolKey = "nuvloc",
            PackageId = "NuvyntraLabs.NuvLoc.Cli",
            CurrentVersion = ToolUpdateCheck.ReadAssemblyVersion(typeof(NuvLocApp)),
            Args = remaining,
            Stdout = stdout,
            Stderr = stderr,
            Stdin = stdin,
            AllowPrompt = ToolUpdateCheck.IsInteractive(stdout) && !noUpdateCheck,
        });
        if (update == UpdateCheckOutcome.UpdatedExit)
            return ExitCodes.Success;

        if (remaining.Length == 0 || IsHelp(remaining[0]))
        {
            stdout.WriteLine(Usage);
            return ExitCodes.Success;
        }

        var root = BuildRoot(stdout, stderr, stdin, payloadRoot);
        var parse = root.Parse(remaining);
        if (parse.Errors.Count > 0 && !LooksLikeKnownCommand(remaining[0]))
        {
            stderr.WriteLine($"Unknown command '{remaining[0]}'.");
            stderr.WriteLine();
            stderr.WriteLine(Usage);
            return ExitCodes.Usage;
        }

        var configuration = new InvocationConfiguration
        {
            Output = stdout,
            Error = stderr,
        };
        return await parse.InvokeAsync(configuration).ConfigureAwait(false);
    }

    static RootCommand BuildRoot(TextWriter stdout, TextWriter stderr, TextReader stdin, string? payloadRoot)
    {
        var configOption = new Option<string>("--configfile") { DefaultValueFactory = _ => I18nConfig.DefaultFileName };
        var pathOption = new Option<string?>("--path");
        var agentOption = new Option<string?>("--agent");
        var formatOption = new Option<string>("--format") { DefaultValueFactory = _ => "human" };
        var langOption = new Option<string[]>("--lang") { AllowMultipleArgumentsPerToken = true, DefaultValueFactory = _ => [] };
        var ciOption = new Option<bool>("--ci");
        var forceOption = new Option<bool>("--force");
        var writeCacheOption = new Option<bool>("--write-cache");

        var init = new Command("init", "Check i18n.json and install agent skills");
        init.Options.Add(configOption);
        init.Options.Add(pathOption);
        init.Options.Add(agentOption);
        init.Options.Add(forceOption);
        init.Options.Add(ciOption);
        init.SetAction(parse => Init(
            parse.GetValue(configOption)!,
            parse.GetValue(pathOption),
            parse.GetValue(agentOption),
            parse.GetValue(forceOption),
            parse.GetValue(ciOption),
            stdout,
            stderr,
            stdin,
            payloadRoot));

        var update = new Command("update", "Refresh skills and reference");
        update.Options.Add(configOption);
        update.Options.Add(pathOption);
        update.Options.Add(forceOption);
        update.SetAction(parse => Update(
            parse.GetValue(configOption)!,
            parse.GetValue(pathOption),
            parse.GetValue(forceOption),
            stdout,
            stderr,
            payloadRoot));

        var agentIdArg = new Argument<string>("id");
        var agentAdd = new Command("add", "Install a second coding agent");
        agentAdd.Arguments.Add(agentIdArg);
        agentAdd.Options.Add(pathOption);
        agentAdd.SetAction(parse => AgentAdd(
            parse.GetValue(agentIdArg)!,
            parse.GetValue(pathOption),
            stdout,
            stderr,
            payloadRoot));

        var agent = new Command("agent", "Manage coding-agent integrations");
        agent.Subcommands.Add(agentAdd);

        var plan = new Command("plan", "Worklist of missing / stale keys");
        plan.Options.Add(configOption);
        plan.Options.Add(pathOption);
        plan.Options.Add(formatOption);
        plan.Options.Add(langOption);
        plan.SetAction(parse => Report(
            parse.GetValue(configOption)!,
            parse.GetValue(pathOption),
            parse.GetValue(formatOption)!,
            parse.GetValue(langOption)!,
            writeCache: false,
            failOnGaps: false,
            stdout,
            stderr));

        var status = new Command("status", "Coverage table");
        status.Options.Add(configOption);
        status.Options.Add(pathOption);
        status.Options.Add(formatOption);
        status.Options.Add(langOption);
        status.SetAction(parse => Report(
            parse.GetValue(configOption)!,
            parse.GetValue(pathOption),
            parse.GetValue(formatOption)!,
            parse.GetValue(langOption)!,
            writeCache: false,
            failOnGaps: false,
            stdout,
            stderr));

        var check = new Command("check", "CI: fail on missing, stale, or broken placeholders");
        check.Options.Add(configOption);
        check.Options.Add(pathOption);
        check.Options.Add(formatOption);
        check.Options.Add(langOption);
        check.Options.Add(ciOption);
        check.Options.Add(writeCacheOption);
        check.SetAction(parse => Report(
            parse.GetValue(configOption)!,
            parse.GetValue(pathOption),
            parse.GetValue(ciOption) ? "json" : parse.GetValue(formatOption)!,
            parse.GetValue(langOption)!,
            parse.GetValue(writeCacheOption),
            failOnGaps: true,
            stdout,
            stderr));

        var version = new Command("version", "Tool version");
        version.SetAction(_ =>
        {
            stdout.WriteLine(ToolUpdateCheck.ReadAssemblyVersion(typeof(NuvLocApp)));
            return ExitCodes.Success;
        });

        var root = new RootCommand("nuvloc — agent-driven localization")
        {
            init, update, agent, plan, status, check, version,
        };
        return root;
    }

    static int Init(
        string configFile,
        string? path,
        string? agentId,
        bool force,
        bool ci,
        TextWriter stdout,
        TextWriter stderr,
        TextReader stdin,
        string? payloadRoot)
    {
        var projectDir = ProjectRoot.Resolve(path);
        var configPath = ResolveConfig(projectDir, configFile);
        var loaded = ConfigLoader.Load(configPath);
        if (!loaded.Success)
            return WriteConfigErrors(stderr, loaded);

        var agent = ResolveAgent(agentId, ci, stdout, stderr, stdin);
        if (agent is null)
            return ExitCodes.Usage;

        string payload;
        try
        {
            payload = PayloadRoot.Find(payloadRoot);
        }
        catch (DirectoryNotFoundException ex)
        {
            stderr.WriteLine(ex.Message);
            return ExitCodes.Usage;
        }

        var files = new List<string>();
        files.AddRange(PayloadInstaller.InstallRules(payload, projectDir, force));
        files.AddRange(AgentInstaller.Install(payload, projectDir, agent));

        stdout.WriteLine($"Checked {configPath}");
        stdout.WriteLine($"Platform {loaded.Config!.Platform}; source {loaded.Config.Source}; languages {string.Join(", ", loaded.Config.Languages)}");
        stdout.WriteLine($"Agent {agent.Id} ({agent.DisplayName})");
        foreach (var file in files)
            stdout.WriteLine($"  wrote {file}");
        stdout.WriteLine();
        stdout.WriteLine("Next: /nuvloc.status then /nuvloc.translate");
        stdout.WriteLine(Disclaimer.Text);
        return ExitCodes.Success;
    }

    static int Update(
        string configFile,
        string? path,
        bool force,
        TextWriter stdout,
        TextWriter stderr,
        string? payloadRoot)
    {
        var projectDir = ProjectRoot.Resolve(path);
        var loaded = ConfigLoader.Load(ResolveConfig(projectDir, configFile));
        if (!loaded.Success)
            return WriteConfigErrors(stderr, loaded);

        string payload;
        try
        {
            payload = PayloadRoot.Find(payloadRoot);
        }
        catch (DirectoryNotFoundException ex)
        {
            stderr.WriteLine(ex.Message);
            return ExitCodes.Usage;
        }

        var files = PayloadInstaller.InstallRules(payload, projectDir, force).ToList();
        var agent = AgentInstaller.DetectInstalled(projectDir);
        if (agent is not null)
            files.AddRange(AgentInstaller.Install(payload, projectDir, agent));

        foreach (var file in files)
            stdout.WriteLine($"  wrote {file}");
        stdout.WriteLine(Disclaimer.Text);
        return ExitCodes.Success;
    }

    static int AgentAdd(
        string id,
        string? path,
        TextWriter stdout,
        TextWriter stderr,
        string? payloadRoot)
    {
        var agent = AiAgent.Find(id);
        if (agent is null)
        {
            stderr.WriteLine($"Unknown agent '{id}'.");
            stderr.WriteLine("Use one of: " + string.Join(", ", AiAgent.All.Select(a => a.Id)));
            return ExitCodes.Usage;
        }

        var projectDir = ProjectRoot.Resolve(path);
        string payload;
        try
        {
            payload = PayloadRoot.Find(payloadRoot);
        }
        catch (DirectoryNotFoundException ex)
        {
            stderr.WriteLine(ex.Message);
            return ExitCodes.Usage;
        }

        foreach (var file in AgentInstaller.Install(payload, projectDir, agent))
            stdout.WriteLine($"  wrote {file}");
        stdout.WriteLine(Disclaimer.Text);
        return ExitCodes.Success;
    }

    static int Report(
        string configFile,
        string? path,
        string format,
        string[] langs,
        bool writeCache,
        bool failOnGaps,
        TextWriter stdout,
        TextWriter stderr)
    {
        var projectDir = ProjectRoot.Resolve(path);
        var loaded = ConfigLoader.Load(ResolveConfig(projectDir, configFile));
        if (!loaded.Success)
            return WriteConfigErrors(stderr, loaded);

        var planner = new LocalizationPlanner();
        var plan = planner.Build(loaded.Config!, langs);
        if (writeCache)
            planner.WriteAcceptedHashes(loaded.Config!, plan);

        var json = format.Equals("json", StringComparison.OrdinalIgnoreCase);
        stdout.Write(json ? PlanReporter.Json(plan) : PlanReporter.Human(plan));
        if (json)
            stdout.WriteLine();

        if (failOnGaps && plan.HasGaps)
            return ExitCodes.Failed;

        return ExitCodes.Success;
    }

    static AiAgent? ResolveAgent(string? agentId, bool ci, TextWriter stdout, TextWriter stderr, TextReader stdin)
    {
        var found = AiAgent.Find(agentId);
        if (found is not null)
            return found;

        if (!string.IsNullOrWhiteSpace(agentId))
        {
            stderr.WriteLine($"Unknown agent '{agentId}'.");
            stderr.WriteLine("Use one of: " + string.Join(", ", AiAgent.All.Select(a => a.Id)));
            return null;
        }

        if (ci || !ToolUpdateCheck.IsInteractive(stdout))
        {
            stderr.WriteLine("Pass --agent <id> (example: --agent cursor).");
            return null;
        }

        stdout.WriteLine("Coding agents:");
        foreach (var agent in AiAgent.All)
            stdout.WriteLine($"  {agent.PickerLabel}");
        stdout.Write("Agent id: ");
        stdout.Flush();
        var answer = stdin.ReadLine();
        found = AiAgent.Find(answer);
        if (found is null)
            stderr.WriteLine("Unknown agent.");
        return found;
    }

    static string ResolveConfig(string projectDir, string configFile) =>
        Path.IsPathRooted(configFile)
            ? Path.GetFullPath(configFile)
            : Path.GetFullPath(Path.Combine(projectDir, configFile));

    static int WriteConfigErrors(TextWriter stderr, ConfigLoadResult loaded)
    {
        foreach (var error in loaded.Errors)
            stderr.WriteLine(error.Message);
        stderr.WriteLine();
        stderr.WriteLine("Example:");
        stderr.WriteLine(ConfigLoader.StubExample());
        return ExitCodes.Usage;
    }

    static bool IsHelp(string value) =>
        value is "-h" or "--help" or "-?" or "help";

    static bool LooksLikeKnownCommand(string value) =>
        value is "init" or "update" or "agent" or "plan" or "status" or "check" or "version";
}
