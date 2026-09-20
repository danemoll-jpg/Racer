# Package verification

Windows x64, Unity 6000.6.1f1, `0.14.0-review2`, build succeeded with zero errors.

- Versioned runtime: `Builds/Racer-0.14.0-review2-Windows`.
- Complete regular runtime: `Builds/Latest`; unchanged `Play-Racer.cmd` launches it.
- Shareable local ZIP: `Builds/Racer-0.14.0-review2-Windows.zip`. No external upload.
- 436 runtime files; 187 songs copied only from deliberate `BundleMusic` staging. External/custom collections were not read or copied.
- Every file in the extracted ZIP, versioned runtime and Latest matched the staged SHA256 manifest. Old Latest, its music, the previous versioned runtime and any existing ZIP are preserved under `Builds/Preserved` by the packaging workflow.
- All 2,883 source-manifest files matched the working source (`source-verification.json`); compiled assembly matched all final tests and packaged smoke checks: `4C3CEB98C70F971631096357E788866D770CD9C2A75AF08C4166F3F3F5E83012`.
- Packaged Latest systems smoke: **474/474**, all four courses; `packaged/systems`.
- Packaged Latest normal-time combined driving/wildlife/radio smoke: **4/4**, `packaged/wildlife-radio`. Radio was playing at the beginning and end; ordinary route driving included wildlife sightings and more empty intervals than sightings. This is runtime evidence, not human listening. The fixture explicitly selects staged BundleMusic; package song hashes independently verify the bundled copies.
- Version metadata is stamped with the completion commit after committing. The same packaging workflow then propagates/verifies the stamp through runtime, complete Latest and ZIP. Final metadata, package date, counts, preserved paths and ZIP SHA are authoritative in `Builds/PACKAGE-LATEST.json`; this document deliberately does not freeze a pre-stamp ZIP hash.

No extra source build occurs during packaging. The compiled player/scene data tested in the 664-case final ramp matrix is copied intact, including the retained failures documented in `VALIDATION.md`. Metadata stamping cannot change those results. Human driving, art/readability acceptance, physical controller and listening tests remain pending.
