# Racer local radio

No songs are shipped. Open Settings > Music / local radio > Open Music folder. Drag your own DRM-free songs into Explorer, then select Rescan. Choose local music folder changes the library location. Only the selected folder is scanned, not subfolders.

Default: %USERPROFILE%/AppData/LocalLow/DefaultCompany/Racer/Music (the exact path appears in Settings). Music is outside the install folder, so replacing Builds/Latest does not remove it.

Gameplay controls: D-pad Right next; Left previous; Up show current song; Down radio on/off. Keyboard: ] next, [ previous, I show song, M toggle. D-pad navigation remains available in menus. Settings offers playback and volume controls; its current-song text stays visible.

Music continues through pause, Settings, garage, results, restart and Quit Race. Radio off pauses the current song. Quit Game stops playback. Master governs all sound; Music is independent of Vehicle and Race/UI volume. Music gain is limited to retain space for vehicle cues.

MP3, PCM WAV and Ogg Vorbis are supported by the Windows runtime decoder. DRM, AAC/M4A, FLAC and Ogg Opus are not advertised as supported. Limits: 2048 top-level files, 256 MiB per MP3/Ogg file or 64 MiB per WAV, one current clip and one pending request; metadata reads are bounded to 4 MiB. Playback uses one asynchronously opened streaming clip; the library is never decoded in advance. Corrupt/removed files are skipped until Rescan. Very large tags may fall back to the filename. Unicode glyph availability depends on the bundled legacy font. Network/UNC folders are not accepted.

Artist/title metadata is read with ATL 6.26.0 (the upstream .NET Standard 2.1 release compatible with Unity); absent tags use the filename. Metadata is rendered as plain text, including angle brackets. A newer ATL package requires .NET 6 and is not compatible with this player runtime.

The game never uploads songs or writes tags. No personal music or library manifest belongs in the repository or package. Development fixtures are generated tones under Temp, never shipped.

Redistribution: ATL is MIT. Its unchanged Ude.NetStandard 1.2.0 dependency is used under MPL 1.1. Licenses and exact upstream source links are in Licenses/THIRD-PARTY.txt.
