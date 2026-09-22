# MAUI `.resx` (NuvLoc 1.0)

- Source: path in `i18n.json` → `source` (typically `Resources/Strings/AppResources.resx`).
- Target: sibling `AppResources.{lang}.resx`.
- Copy `name` and optional `<comment>` from the source. Translate `<value>` only.
- Preserve `resheader`, schema, comments, and `xml:space`.
- Do not edit the source file, the `.csproj`, or `NeutralResourcesLanguage`.

NuvLoc does not give a 100% guarantee on translated text. Review every culture file with a native speaker of that language before you ship.
