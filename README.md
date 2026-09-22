# NuvLoc

Agent-driven localization CLI for .NET MAUI (other platforms later).

**Package:** `NuvyntraLabs.NuvLoc.Cli` (`PackAsTool`, command `nuvloc`)  
**Version:** 1.0.0  
**License:** MIT  
**Author:** [Niladri Prasad Padhy](https://github.com/NiladriPadhy) / Nuvyntra Labs

The CLI checks `i18n.json`, diffs culture `.resx` files, and installs `/nuvloc.status` and `/nuvloc.translate` for your coding agent. **The agent translates.** NuvLoc does not call OpenAI, Azure, or any vendor API and does not take an API key.

Usual alternatives: Visual Studio Multilingual App Toolkit, [ResXResourceManager](https://github.com/dotnet/ResXResourceManager), Crowdin / Phrase.

> NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.

## Install

```bash
dotnet tool install -g NuvyntraLabs.NuvLoc.Cli --source https://api.nuget.org/v3/index.json
nuvloc version
```

This is a global tool, not an app `PackageReference`.

## Config

Put `i18n.json` at the project root:

```json
{
  "platform": "maui",
  "source": "Resources/Strings/AppResources.resx",
  "languages": ["es", "fr"]
}
```

## Use

```bash
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
