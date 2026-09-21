# Delivered: Racer 0.18.0-review1

Safety checkpoint: `a4dfebb23ba65d2e5d9227b2b5c18d0cbaea578b`.
Implementation commit: `752ca2f19c04cdc180b943f46c79634cca0bc880`.
The following completion commit records this final packaging/startup evidence.

Launch: `C:/Users/danmo/Racer/Builds/Latest/Racer.exe` or `Play-Racer.cmd`.
Complete ZIP: `C:/Users/danmo/Racer/Builds/Racer-0.18.0-review1-Windows.zip`.
ZIP SHA256: `4A6D164B0264A19CCF47EB11466E6579E0AB9E3A0199594FD9D532BC5B950FC8`.

All 440 files in versioned runtime, Latest and extracted ZIP match SHA256.
187 playable staged songs and two original M4A files are preserved. Older builds
and the previous Latest are preserved; no external collection, saved preference
or shipped volume setting was changed. No upload was performed.

The single extracted-package startup ran from the portable executable directory,
using an isolated muted save, and passed menu loading plus embedded speech access.
It exited normally without timeout. Its assembly SHA256 is
`F9DB15DF86530B8AAD7DA49E4164F9C7B8FB66460EA438155428221A38ADBB12`, identical to
all three final delivery flight checks. See `package/extracted-startup`.

A shell wrapper initially reported failure after the successful packager returned,
because it inspected an inherited native LASTEXITCODE instead of PowerShell script
success. The packager's verified final report was intact; no repackaging or duplicate
startup was required for that wrapper issue. An earlier packaging pass was preserved
when release documentation was finalized.

See VALIDATION.md and accepted-flight-evidence.json for the eight accepted physical
runs, final bike/ATV results and all retained limitations. Existing jump scoring still
gives zero awards. No subjective listening, physical-controller or Deck validation
is claimed. The baseline approach did not reproduce the pit and omitted flight.
Dan's human acceptance remains pending. No other features were newly accepted.
