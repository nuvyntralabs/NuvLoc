# NuvLoc sample

A MAUI host with `i18n.json` and the usual satellite names:

| Role | File |
| --- | --- |
| Neutral (English) | `Resources/Strings/AppResources.resx` |
| Spanish / French | `AppResources.es.resx` / `AppResources.fr.resx` (written by `/nuvloc.translate`) |

NuvLoc only writes those culture files. XAML `x:Static` in this sample is the host’s choice, not part of the CLI.

```bash
# from the NuvLoc repo after a local tool install
cd samples/NuvLocSample
nuvloc init --configfile i18n.json --agent cursor
nuvloc status --configfile i18n.json
nuvloc check --configfile i18n.json
```

`check` exits 1 until the culture files exist. Then `/nuvloc.status` and `/nuvloc.translate`.

NuvLoc does not give a 100% guarantee on translated text. Review every culture file with a native speaker of that language before you ship.
