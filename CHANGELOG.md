# Changelog

## 1.1.2

- Reject invalid language codes in `i18n.json` and `--lang` (must be a BCP-47 culture such as `es` or `pt-BR`).
- Non-BCP-47 codes skip creating that localized `.resx` and print the error plus reason.
- Canonicalize culture names (`PT-br` → `pt-BR`). Underscores (`es_MX`) fail with a hyphen hint.
- `--lang` that is not in `i18n.json` exits 2 instead of silently reporting no languages.

## 1.1.1

- CI matches other Labs CLIs: version alignment → NuGet check → tests → pack → publish (`NUGET_KEY_NUVLOC` only).
- `PackageProjectUrl` points at https://nuvyntralabs.github.io/toolkits/nuvloc/

## 1.1.0

- `platform` accepts `maui`, `wpf`, `winui`, `avalonia`, and `uno` for sibling `.resx` hosts.
- Shared `.nuvloc/reference/dotnet-resx.md` replaces the MAUI-only reference.
- WinUI / Uno PRI `.resw` folder layout remains out of scope.
- Docs: local pack / install / uninstall (`nuget.config` + `--ignore-failed-sources`); sample uses `AppResources.resx`.

## 1.0.0

- `nuvloc init --configfile i18n.json --agent <id>` checks the config and installs `/nuvloc.status` and `/nuvloc.translate` for the Nuvyn Spec Kit agent set.
- `plan` / `status` / `check` diff MAUI `.resx` culture files. No vendor API key.
- Disclaimer: NuvLoc does not give a 100% guarantee on translated text. Review with a native speaker before ship.
