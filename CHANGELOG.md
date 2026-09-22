# Changelog

## 1.1.0

- `platform` accepts `maui`, `wpf`, `winui`, `avalonia`, and `uno` for sibling `.resx` hosts.
- Shared `.nuvloc/reference/dotnet-resx.md` replaces the MAUI-only reference.
- WinUI / Uno PRI `.resw` folder layout remains out of scope.
- Docs: local pack / install / uninstall (`nuget.config` + `--ignore-failed-sources`); sample uses `AppResources.resx`.

## 1.0.0

- `nuvloc init --configfile i18n.json --agent <id>` checks the config and installs `/nuvloc.status` and `/nuvloc.translate` for the Nuvyn Spec Kit agent set.
- `plan` / `status` / `check` diff MAUI `.resx` culture files. No vendor API key.
- Disclaimer: NuvLoc does not give a 100% guarantee on translated text. Review with a native speaker before ship.
