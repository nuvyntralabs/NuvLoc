---
name: nuvloc-status
description: Show i18n coverage from nuvloc status. Does not translate. Completeness is not correctness.
---

# /nuvloc.status

Show localization coverage for this project. Do not translate and do not edit `.resx` files.

1. Find `i18n.json` (or `--configfile` if the user named another path).
2. Run `nuvloc status --configfile <that file> --format json` from the project root. Forward `--lang` if the user named cultures. If the CLI exits 2 (invalid or unknown language code), show the error and the reason, do not create a localized `.resx`, and stop.
3. Present a table per language: current / missing / stale / placeholder / extra.
4. If missing or stale keys exist, suggest `/nuvloc.translate`.
5. If coverage is complete, say the files are complete — not that the wording is correct. Do not invent counts.

NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.

NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.
