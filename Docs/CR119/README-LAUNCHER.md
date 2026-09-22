# Woodstock Rush launcher — local review build

Extract the complete starter ZIP into a writable folder. Run **WoodstockRushLauncher.exe**. The game starts after three seconds; press any direction/button or use `--manage` to keep the update controls open. Use D-pad/arrows, A/Enter, or the mouse. B/Escape cancels a download or returns selection to Play. The launcher is silent. Updates are optional; an unavailable server never removes the installed game.

Check for updates shows the offered version, game download size, release notes, and separate soundtrack add/change/remove counts and download size. Update game and play installs a signed runtime after verification. Update soundtrack is a separate deliberate action. Cancel before activation leaves the current selection unchanged. Close the game before installing updates. Keep the launcher running while the child game runs; it maintains the Steam session lifetime.

**The public catalog is live for build-21002.** Offline Play remains available; Check can now retrieve the signed release. This is a Windows-tested native prototype for Deck/Proton review, not a claim of physical Deck acceptance.

## Files and recovery

The stable launcher sits beside `state.json`, `versions`, `staging`, and `shared`. Game updates replace the selected version, not saves. `shared/soundtracks` holds managed music; add your songs to `shared/PersonalMusic`. Immediate subfolders remain station names in both roots. Your existing custom-folder preference takes priority and is not overwritten. All 187 playable bundled songs and two supplied original files are included initially.

Restore previous version changes the selected runtime and keeps newer save data. It does not silently restore older progress. Pre-update and pre-rollback save snapshots are in `save-backups`; manual restoration is a separate deliberate decision, especially if future versions change save schemas. The normal Unity save identity remains `DefaultCompany/Racer` in the same Windows account or Proton prefix. No save migration is required.

`recovery.log` records activation and startup events. Interrupted activation uses the last atomically written `state.json`; unselected staging is retained for diagnosis. Downloads can restart; transfer resume is not implemented. One previous unmodified runtime and soundtrack are retained. Older versions containing unknown/modified files are preserved instead of automatically deleted. Failed staging may occupy disk; inspect it before manually removing it with the launcher and game closed. Never remove `shared/PersonalMusic`, current/previous versions, or saves as an update repair.

A startup timeout or early crash shows Retry/Play and Restore previous choices. Close any still-running child before retrying. An ordinary successful game exit closes the launcher without declaring failure. Bootstrap self-update is deliberately deferred: a manifest requiring a newer launcher tells you to replace the bootstrap manually while current Play remains available.

## Existing Windows installation

Close the game. Back up the existing Steam shortcut details if applicable. The supplied `Install-Launcher.ps1` takes `-Starter` and `-ExistingGameDirectory`. It backs up the existing Racer.exe, copies the starter, preserves legacy Music, copies unmatched/modified legacy songs to PersonalMusic, then places the bootstrap at the **same existing Racer.exe path**. Keep the existing Steam shortcut and launch options. The old executable is in `LauncherMigrationBackup-*`. Verify existing progress/settings on first use before accepting migration. Migration is explicit; merely extracting the starter does not modify an existing install.

If the script finds an existing launcher state or conflicting destination files, inspect those files rather than overwriting them. It never deletes the legacy Music collection. This conservative backup can temporarily use additional disk space.

## One-time Steam Deck migration — physical verification pending

1. In Desktop Mode with Steam and the game closed, back up `~/.local/share/Steam/userdata/<Steam ID>/config/shortcuts.vdf` and record the existing shortcut's exact Target, Start In, launch options, compatibility selection, non-Steam AppID, and `steamapps/compatdata/<AppID>` prefix. Back up the existing `DefaultCompany/Racer` save folder inside that prefix. Do not delete/re-add the Steam entry.
2. Extract the starter beside the existing installation. Copy its `versions`, `shared`, `state.json`, signed manifests, launcher and cover into the existing game's folder only if those launcher files do not already exist. Preserve the old Music folder. Put unmatched/personal songs in `shared/PersonalMusic`, preserving station subfolders; keep originals until verified. The PowerShell migration helper is for Windows, not a requirement to install PowerShell on Deck.
3. Back up the old Racer.exe, then copy WoodstockRushLauncher.exe over **that exact Racer.exe path**. Leave the existing Steam shortcut Target/Start In and compatibility override unchanged. This avoids deliberately changing the shortcut identity; actual prefix preservation still needs the check below.
4. In Gaming Mode, verify the same non-Steam AppID/prefix is used, existing progress/settings/playlists appear, controls work with Steam Input, and the session remains alive through launcher → game → exit. Use `--manage` temporarily to inspect controls; remove it for quick play. No separately installed .NET/desktop framework is required.
5. Try one published signed update and one rollback, checking the same save and music locations. Mark physical acceptance only after these checks. If startup fails, restore the backed-up original Racer.exe at the same path and retain launcher diagnostics; do not create a new Steam shortcut or prefix to hide the failure.

Physical Deck, controller hardware, and existing-user-prefix acceptance remain pending. Automated Windows fixtures are not substitutes for those checks.
