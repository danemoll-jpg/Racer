# Local and portable music

Settings > Music / local radio > Music source / collection setup selects **Bundled music** or **Custom folder**. Include subfolders defaults to On and is saved along with the source and custom root. A previously selected custom collection stays selected after upgrading. New installations default to Bundled music.

Bundled music resolves to `Music` beside `Racer.exe`, regardless of the working directory or where the ZIP is extracted. Open selected folder opens that location. Add songs, then Rescan; artist/album folders are supported. Custom folder uses the selected local root, also recursively. Neither source changes original files. There is no network access or uploading.

## Dan's shareable ZIP

1. Put only the songs you want to share in `BundleMusic` at the project root (`C:\Users\danmo\Racer\BundleMusic`). Artist/album subfolders are fine. This is the stable staging folder, separate from Unity Assets and all builds. It is ignored by Git and preserved through builds.
2. Double-click `Package-Racer.cmd` at the project root. Or run `pwsh -NoProfile -File Tools/Package-Racer.ps1 -Version 0.6.3-review1` from the project folder. **No Unity compile is needed after adding/removing songs.**
3. The command updates the versioned runtime, complete `Builds/Latest`, and `Builds/Racer-0.6.3-review1-Windows.zip`. Give your friend that ZIP manually. Extract it and start `Racer.exe`; select Bundled music if their settings previously used Custom folder.

Only supported audio deliberately placed in BundleMusic is copied. The selected external Custom folder is never read by the packaging command. No staged songs means an empty Music folder plus instructions, which works normally.

Before replacing a runtime, packaging moves its complete previous contents into a uniquely named folder under `Builds/Preserved`. This protects music added directly to Latest, and also preserves old ZIP revisions. The script prints the preservation location. To include songs previously added directly to Latest, copy your chosen files from its preserved `Music` folder into `BundleMusic`, resolving filename conflicts yourself, then rerun Package-Racer.cmd. **Preservation is not automatic inclusion**: nothing outside BundleMusic is silently added to a shareable ZIP. Leave preserved folders until you have checked their contents; the tool does not delete them. Close the game before packaging; locked files cause a clear error and previous runtime preservation is retained.

## Discovery, limits and controls

Supported formats remain DRM-free MP3, PCM WAV and Ogg Vorbis. FLAC, AAC/M4A, WMA, Opus, encrypted/DRM media and unusual WAV encodings are not supported. Renaming an extension does not convert a codec. Per-file caps remain MP3/Ogg 256 MiB and WAV 64 MiB. Corrupt/removed files are skipped at playback, with filename fallback for bad/missing metadata.

Discovery runs on one background worker and can be cancelled. Replacing the root, source, or recursion preference cancels the previous scan; stale results cannot replace the new collection. Rescan detects additions and removals. Scans do not decode audio or read all tags. Directory and file reparse points (junctions/symbolic links) are skipped; case-insensitive canonical paths prevent duplicate discovery. Hard-link filenames count as separate files. Inaccessible entries/folders are skipped rather than stopping the remaining tree.

The old silent 2,048-file cap is replaced with explicit bounds: **100,000 tracks, 250,000 visited entries, 20,000 folders or 30 seconds per scan**, whichever comes first. Counts show tracks and skipped unsupported, linked, inaccessible and oversized/empty entries. A limit message means the undiscovered remainder is unknown, not zero; choose a smaller artist/root folder to scan all of it. OS filesystem calls already in progress cannot be forcibly interrupted; cancellation takes effect between entries. The previous collection is retained on cancellation. Partial results from a bounded scan remain usable and clearly marked.

Shuffle covers the whole discovered collection, with no immediate repeat where possible. Previous has 32 entries of history. Only the selected track is opened for streaming; metadata has a separate bounded 4 MiB reader. Volume remains Music under Master. Music continues through menus, pause, results, race restart and Quit Race; Quit Game stops it.

Gameplay: D-pad right/left/up/down = next/previous/show/toggle; keyboard `]` / `[` / `I` / `M`. Menu D-pad navigation is unchanged. Music settings also provide Next, Previous, Radio toggle and volume. Open Folder, Rescan and Cancel scan are in collection setup.
