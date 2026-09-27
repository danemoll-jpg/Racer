# Published route cleanup and atlas — 0.30.0-review1

Public latest: https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-30000

Safety checkpoint: `aa96293f564eb99d100864cb503351be44cd8495`.
Implementation completion: `6e71800616dbf30390004cf2047a6bb063ef735d`, pushed to source main before publication. The subsequent documentation commit records delivery evidence and final atlas road landmarks; it does not change gameplay.

Unity build succeeded in 2m24.67s with zero errors and five warnings (existing obsolete APIs, collider pre-bake notice, absent optional Pipeline config). Packaging verified every extracted ZIP/runtime/Latest file by SHA256. Free disk space before build was 314.65 GiB; Builds occupied 5.047 GiB before task packaging.

The existing component publisher used the existing private signing identity and project-local GitHub CLI. A briefly stale GitHub release list caused the first draft lookup to fail; the empty draft was inspected and the publisher's supported `--resume-draft` completed without overwriting content. All three assets were verified before publication:

| Asset | Bytes | SHA256 |
|---|---:|---|
| game.zip | 246397237 | 537fb44f8392050c53f64a5d13cfa5b2915ef95c0ba40649c7d49e0e0bbfcf15 |
| game-manifest.json | 56445 | 5c0c1d8846ffcec7e67e74cb823eb766d14a8d8d89e0014b13d0623f6a0a719e |
| update-catalog.json | 993 | 3a7cadc1ecf88118ef1a5517f5ba81865c2bd1032e95b6de665cee251fb22d93 |

The repository is public; the release is published, not a draft, and is latest. The signed catalog was fetched back and matched. The previous soundtrack catalog pointer is unchanged. A fresh public installation through the existing updater verified the pinned signature and all 276 game files, then passed one isolated startup check without NullReferenceException or MissingReferenceException. See public-release.json and hosted/result.json.

The normal `Play-Racer.cmd` entry point now calls the existing launcher in stable `Builds/Latest`. The standard Install-Launcher migration retained the executable shortcut path, existing saves and personal music. Starter preparation initially rejected the flat packaged folder's extra music files; it succeeded using the public installation's exact signed game inventory instead. No launcher security or signing changes were made. The actual command launched the responsive signed `Builds/Latest/versions/30000/Racer.exe`; the existing updater then reported no newer game or soundtrack. See play-racer-launch.json and launcher-catalog-check.json. The verification game/launcher processes were closed after checking; no additional races were run.

Cleanup ran only after publication, public download/startup and entry-point verification. Builds fell from **14.552 GiB immediately before cleanup to 4.862 GiB**. Removed packaging/extraction staging, temporary Preserved copies, 0.28 obsolete build, duplicate current runtime and ZIP, remote-verified upload ZIP, the old 0.29 full ZIP after verifying all 451 files against the retained previous build, temporary hosted/starter installs, migration executable backup and signed duplicate flat engine files. Retained current signed install and one previous 0.29 complete build. PublisherPrivate/key, PublisherTools, Launcher/LauncherSDK, source, Git, Docs/atlas, BundleMusic, current signed metadata, personal music and user saves were preserved. PACKAGE-LATEST now records the retained runtime and published release; the original package report remains in package-verification.json. See cleanup.json.

Scope/remaining review: Granite Saddle is main only in Forest Reverse, per Dan's explicit choice. Street Reverse House 3 driveway remains unchanged because Laurel protection takes priority. The other five straight driveways remain steep to preserve house/road positions. Laurel's old physical jump and route metadata differ; both are mapped and untouched. Existing unrelated ambient-traffic errors appeared only during targeted Editor checks; the public startup check did not reproduce them. Detailed driving/visual acceptance remains Dan's review. No vehicle physics, general AI or recovery redesign occurred. The replacement Forest shortcut remains explicitly deferred until atlas review.
