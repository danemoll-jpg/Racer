# Publication verified

Implementation commit: b0390d4a953be970a498e6d764affd894e6d405c — Smooth Forest Loop Reverse opening hill while preserving nearby routes.

Published version 0.29.0-review1, tag game-29000, as latest at https://github.com/danemoll-jpg/woodstock-rush-releases/releases/tag/game-29000.

The existing Windows release build succeeded with zero errors and two warnings (existing mesh collision pre-baking advisory and intentionally disabled runtime Pipeline). Packaging verified 451 files by SHA256. All three required release assets were uploaded and checked against GitHub hashes: game.zip, game-manifest.json and update-catalog.json. Existing publisher identity and soundtrack pointer were retained.

The public signed manifest matches all 261 game files in Builds/Latest. The existing updater downloaded, verified, extracted and activated the public release in an isolated verification install; the startup check exited successfully. Play-Racer.cmd launched Builds/Latest/Racer.exe with version 0.29.0-review1 and a responsive game window. No gameplay races were run.

After successful publication and startup verification, disposable package staging, duplicate current runtime/ZIP, obsolete 0.27 build, temporary preserved build copies and hosted verification install were removed. Existing music was checked before cleanup. Builds now occupies 4.091 GiB (4,392,290,382 bytes), retaining Latest and the immediately previous 0.28 build, plus launcher, SDK, publisher tools/keys and release metadata. All source and Git history remain intact.
