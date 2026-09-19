# Racer 0.7.0 local radio

Fresh settings use **Music beside Racer.exe**. Existing explicit source, custom-folder and Off preferences are retained. Settings > Music / local radio > Music source / collection setup selects Bundled or a custom collection root.

Each immediate child folder is one channel, named exactly after that folder. Songs in nested artist/album folders remain in that channel. Songs directly in the selected root form General. Empty folders are omitted; unreadable tracks are skipped and a channel with no playable tracks is skipped. Channels sort by folder name, case-insensitively; root General comes first when present. A child folder named General remains a distinct folder channel.

Examples:

```
Music/Rock/Artist/Album/song.mp3    -> Rock
Music/Country/song.mp3            -> Country
Music/song.wav                   -> General
```

During gameplay, **D-pad Down / M** cycles channel 1 -> channel 2 -> ... -> Off -> channel 1. **D-pad Right / ]** selects the next shuffled song; **Left / [** goes back through that channel's history; **Up / I** displays channel and artist/title. D-pad navigation in menus does not operate the gameplay radio. Settings provides the same channel-cycle, previous/next, rescan and independent music-volume controls. Off stops only radio playback. Master volume still affects all audio.

Supported formats: DRM-free MP3, PCM WAV, Ogg Vorbis. Scans run asynchronously and exclude directory links. Limits remain 100,000 files, 250,000 directory entries, 20,000 folders, 30 seconds per scan; WAV 64 MiB and MP3/Ogg 256 MiB per file. A blocked OS filesystem call can delay cancellation until it returns. Playback loads one clip with a 15-second request timeout, and metadata reads are bounded. A malformed codec stream can produce a Unity decoder diagnostic while the player skips it; this is not an unhandled game exception.

## Repackage without Unity

1. Copy only the music you deliberately want in the package into project **BundleMusic**, using channel folders (for example `BundleMusic/Rock/Artist/Album/song.mp3`). Do not select an entire personal collection unless you intend to stage it.
2. Double-click **Package-Racer.cmd** at the project root. Equivalent PowerShell command from the project root:

   `pwsh -NoProfile -File Tools/Package-Racer.ps1 -Version 0.7.0-review1`

3. The script copies supported staged files into **Music** in `Builds/Latest`, `Builds/Racer-0.7.0-review1-Windows`, and its ZIP, preserving folder names and hierarchy. It verifies every extracted/runtime file by SHA256. No Unity compilation is required to add songs.
4. Previous complete Latest, versioned runtime and ZIP are preserved under the printed **Builds/Preserved/<timestamp>** directory. Songs added directly to Latest/Music are preserved there, but are not silently included in new packages. To include selected songs, copy them from that preserved Music folder into BundleMusic, resolve any filename conflicts yourself, and package again.

Custom collection folders are never copied automatically. Original songs are not modified. BundleMusic, Music and Builds are excluded from Git. This workflow does not upload anything.

Extract the entire ZIP before launching Racer.exe. Bundled paths are resolved relative to the executable's installation, not the original project or a drive letter. An explicit custom source remains a custom source after moving the game; use Settings to return to Bundled if desired.

Automated generated-tone playback and virtual input tests are separate from human listening and physical-controller testing. Dan's review remains required for sound balance and everyday use.
