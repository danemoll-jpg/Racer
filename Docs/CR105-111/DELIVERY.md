# CR-105–111 delivery — 0.19.0-review3

Launch C:\Users\danmo\Racer\Play-Racer.cmd or Builds/Latest/Racer.exe.
Complete versioned runtime: Builds/Racer-0.19.0-review3-Windows.
Complete ZIP: Builds/Racer-0.19.0-review3-Windows.zip. No upload.

Safety checkpoint: 542a61629c4a8ea0ef0763064b7c01c9c8d2c33f.
Implementation checkpoint: a74119641fcd0fe81916ea47176ccbd566b97486.
VERSION.txt identifies this source and compiled Assembly-CSharp hash. The following delivery-only commit records packaging/startup evidence and TODO; it changes no runtime code.

All 447 files match SHA256 across the versioned runtime, whole Latest and extracted ZIP. ZIP SHA256: C1C09EEEFEDAA4ED8EC16DC101263CC08B5B789DF1087B86716F6B6AE4AECBD3.
187 playable staged songs and two original audio files are included. External music collections and personal saves/settings were untouched.
Prior complete Latest and prepackage runtime are preserved at Builds/Preserved/20260921-033119-027-a75956.

Approved artwork is included as WoodstockRush-Cover.png beside Racer.exe in the runtime, Latest and ZIP. All copies match Assets/Resources/Title/Artwork.png (SHA256 2070F4D996567CD3ACE865C52FD30E8EDCBCC34F519DE80C677BAF2AED0C32B4). Tools/Package-Racer.ps1 copies and verifies it for every future version, independently of version-specific README selection. File inclusion only was checked; no additional gameplay for the cover.

The single extracted-folder portable startup passed and exited with code 0, without timeout. It used an isolated Temp save and temporary test mute; ordinary launches do not enable the test mute. No test game remains running. The only audible verification in the entire pass was the brief new turkey call, automatically remuted.

See VALIDATION.md for targeted outcomes, failed candidates, affected reruns and precise limitations. Four new-course profile/direction runs, centered/off-center giant flights, local approaches/returns/recovery, two short AI events and the requested UI/world/turkey cases are documented. No broad regression or ghost suites were run. Automated runs include ordinary gate misses and safe recoveries; they are not claimed as clean laps or physical controller/Deck testing. Ghost HUMAN review and Dan's subjective acceptance remain pending.

README-player.txt contains the seven-item checklist for new work. PROJECT_TODO.md records actual completion and preserves the vehicle/stat/unlock/people/multiplayer backlog.
