# Sibling `.resx` (NuvLoc 1.1)

- Source: path in `i18n.json` → `source`. Typical paths:
  - MAUI: `Resources/Strings/AppResources.resx`
  - WPF: `Properties/Resources.resx` (or a Strings folder if the host already uses one)
  - WinUI 3 (`.resx` hosts only): same sibling pattern as MAUI
  - Avalonia: host-chosen `.resx` (often under `Assets` or `Resources`)
  - Uno: shared `.resx` in the class library
- Target: sibling `{stem}.{lang}.resx` next to the English source (example: `AppResources.es.resx`).
- Copy `name` and optional `<comment>` from the source. Translate `<value>` only.
- Preserve `resheader`, schema, comments, and `xml:space`.
- Do not edit the source file, the `.csproj`, or `NeutralResourcesLanguage`.
- WinUI / Uno PRI `.resw` files under `Strings/{lang}/` are out of scope. Use sibling `.resx` only.

NuvLoc does not give a 100% guarantee on translated text. Review every culture file with a native speaker of that language before you ship.
