# CR-119 — 0.21.0-launcher1 local review delivery

Complete install: `Builds/LauncherRelease-21002/assets/WoodstockRush-0.21.0-launcher1-Full.zip`.
Unpacked launcher: `Builds/LauncherRelease-21002/starter/WoodstockRushLauncher.exe`.
Verified extraction: `Builds/LauncherRelease-21002/verified-extraction/WoodstockRush`.
Standalone cover: `Builds/LauncherRelease-21002/starter/WoodstockRush-Cover.png`.

Silent native controls, quick play, optional signed game/soundtrack updates, progress/cancel, offline Play, immutable runtime selection, save backups, previous-version rollback, running-child protection, startup handshake, and parent lifetime implemented. Shared managed/personal music retains folder stations and custom preference. Publisher tooling prepares full starter or independent component releases; deliberate publishing stages/verifies assets before latest.

Safety checkpoint `7cc2f1da14b16b136746c575355c73067708860f`; completion commit reported in the delivery response. Full validation and limitations are in VALIDATION.md; player/migration instructions in README-LAUNCHER.md; publishing instructions in PUBLISHING.md.

Local tests pass. Hosted update remains blocked pending explicit publishing approval after automatic review rejected an authenticated access probe; no credentials accessed or files uploaded. Physical Deck/controller/prefix acceptance remains pending. Existing feedback build0.20.1 and Latest remain preserved. Dan's accepted people/turkeys/mountain layouts and ghosts remain accepted; no launcher work changes their logic. Spoken-title human acceptance remains open from the prior feedback phase; no new audio tests here.
