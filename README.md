# NuvLoc

Agent-driven localization CLI for sibling `.resx` hosts (MAUI, WPF, WinUI, Avalonia, Uno).

**Package:** `NuvyntraLabs.NuvLoc.Cli` (`PackAsTool`, command `nuvloc`)  
**Version:** 1.1.0  
**License:** MIT  
**Author:** [Niladri Prasad Padhy](https://github.com/NiladriPadhy) / Nuvyntra Labs

The CLI checks `i18n.json`, diffs culture `.resx` files, and installs `/nuvloc.status` and `/nuvloc.translate` for your coding agent. **The agent translates.** NuvLoc does not call OpenAI, Azure, or any vendor API and does not take an API key. It does not bind strings to the UI — XAML, a markup extension, or code is the host’s choice.

Usual alternatives: Visual Studio Multilingual App Toolkit, [ResXResourceManager](https://github.com/dotnet/ResXResourceManager), Crowdin / Phrase.

> NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.

## Install

```bash
dotnet tool install -g NuvyntraLabs.NuvLoc.Cli --source https://api.nuget.org/v3/index.json
nuvloc version
```

This is a global tool, not an app `PackageReference`.

### Build, install, and uninstall locally

From this repo (or the hub root, adjusting paths):

```bash
dotnet test NuvLoc.slnx
dotnet pack src/NuvyntraLabs.NuvLoc.Cli/NuvyntraLabs.NuvLoc.Cli.csproj -c Release -o artifacts

dotnet tool uninstall -g NuvyntraLabs.NuvLoc.Cli
dotnet tool install -g NuvyntraLabs.NuvLoc.Cli \
  --add-source ./artifacts \
  --configfile ./nuget.config \
  --ignore-failed-sources \
  --version 1.1.0

nuvloc version
dotnet tool uninstall -g NuvyntraLabs.NuvLoc.Cli
```

`--configfile ./nuget.config` uses nuget.org only. `--ignore-failed-sources` keeps a broken extra feed (for example a 401 Azure Artifacts source in a machine-wide NuGet.config) from aborting the install. If you packed without `-o artifacts`, the nupkg is under `src/NuvyntraLabs.NuvLoc.Cli/bin/Release/`.

## Config

Put `i18n.json` at the project root. The host points `source` at an English sibling `.resx`. Culture files are `{stem}.{lang}.resx` next to that file.

`platform` is one of `maui`, `wpf`, `winui`, `avalonia`, or `uno`. WinUI / Uno PRI `.resw` files under `Strings/{lang}/` are out of scope.

```json
{
  "platform": "maui",
  "source": "Resources/Strings/AppResources.resx",
  "languages": ["es", "fr"]
}
```

WPF example:

```json
{
  "platform": "wpf",
  "source": "Properties/Resources.resx",
  "languages": ["es", "fr"]
}
```

A runnable MAUI host is in [`samples/NuvLocSample`](samples/NuvLocSample) (`i18n.json` + `AppResources.resx` already in place).

## Use

```bash
cd samples/NuvLocSample
nuvloc init --configfile i18n.json --agent cursor
```

`init` **checks** the file (valid JSON, source exists, languages set) and writes `.nuvloc/` plus agent skills. It does not translate.

In Cursor:

```
/nuvloc.status
/nuvloc.translate
```

`/nuvloc.status` runs `nuvloc status` and explains coverage. `/nuvloc.translate` runs `nuvloc plan`, writes culture `.resx` files, then `nuvloc check --write-cache`.

CI (no agent):

```bash
nuvloc check --configfile i18n.json --ci
```

There is no `nuvloc translate` CLI command.

## Commands

| Command | Purpose |
| --- | --- |
| `nuvloc init` | Check `i18n.json` and install slash commands |
| `nuvloc update` | Refresh skills and `.nuvloc/reference` |
| `nuvloc agent add <id>` | Second coding agent |
| `nuvloc plan` | Missing / stale worklist |
| `nuvloc status` | Coverage table |
| `nuvloc check` | Exit 1 on missing, stale, or broken placeholders |
| `nuvloc version` | Tool version |

`--agent` accepts the same Spec Kit set as Nuvyn (`cursor`, `copilot`, `claude`, `gemini`, `codex`, `windsurf`, …).

On an interactive terminal `nuvloc` asks every 4 hours whether to update from nuget.org (`--no-update-check` or `NUVYNTRA_NO_UPDATE_CHECK=1`). Cache: `~/.nuvyntra/cli-updates.json`. It does not phone home.

Publishing is pipeline-only. Never `dotnet nuget push` from a local clone.
