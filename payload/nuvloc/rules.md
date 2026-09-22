# NuvLoc rules

- English `source` in `i18n.json` is the source of truth. Do not invent keys.
- Translate missing and stale keys from `nuvloc plan` only.
- Preserve `{0}`, `{name}`, `%s`, and `%d` exactly.
- Do not call a third-party translation HTTP API. You are the translator.
- Do not overwrite a current key unless the user asks.

NuvLoc does not give a 100% guarantee on translated text. Agent wording can be inaccurate or culturally off. Review every culture file with a native speaker of that language before you ship. Coverage and placeholder checks only prove completeness, not correctness.
