---
name: nuvloc-translate
description: Translate missing and stale keys from nuvloc plan. Review with a native speaker before ship.
---

# /nuvloc.translate

Translate missing and stale localization keys. You are the translator. Do not call a third-party translation HTTP API.

1. Read `i18n.json` and `.nuvloc/rules.md`.
2. Run `nuvloc plan --configfile <i18n.json> --format json` from the project root. Forward `--lang` if the user named cultures.
3. For each language in the plan, translate only `missing` and `stale` items. Keep the key and comment. Preserve `{0}`, `{name}`, `%s`, and `%d` exactly.
4. Write sibling `{stem}.{lang}.resx` files next to the English source (example: `AppResources.es.resx`). Create the file if it is missing. Do not rename keys. Do not add keys that are not in the worklist. Do not overwrite `current` keys.
5. Run `nuvloc check --configfile <i18n.json> --write-cache`. If check fails, fix placeholder mismatches and stop.

End the turn by repeating that NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.

NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.
