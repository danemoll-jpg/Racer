# CR-119 launcher phase — local delivery complete, publication/physical acceptance pending

Safety checkpoint: `7cc2f1da14b16b136746c575355c73067708860f`, verified clean. This is also the completed small-feedback checkpoint. Preserve `Builds/Racer-0.20.1-review1-Windows`, its ZIP and normal portable distribution. Feedback outcomes are in Docs/CR120-121/DELIVERY.md.

FINAL LOCAL STATUS supersedes progress notes below: full starter build21002 packaged and hash-verified; 15 core fixtures pass; actual signed21001→21002 update, muted startup, rollback startup pass with save/settings/playlist preservation. Final runtime build job daccfc59b4334e1fb4e66b9dcd32dc67 completed0 errors/4 warnings. Native launcher SHA256 BC86641F192A950EA625E92577CDC0BC18EC8400473A788A58E3F7DFF6A182BB. Full ZIP418 files/1017500040 bytes/SHA74a2c78a0e00158d402ed8aedfb2ac377997ec03a78042d9fd6333fefa82048d. See DELIVERY/VALIDATION/README-LAUNCHER/PUBLISHING. No further local test expansion is needed absent a new defect. Publication authorization, authenticated access, hosted update and physical Deck/controller/prefix checks remain pending. No upload or credential access occurred. Completion commit to be reported in final response.

Authoritative approved requirements:

`C:/Users/danmo/Documents/Codex/2026-09-17/referenced-chatgpt-conversation-this-is-an/outputs/LAUNCHER_CODEX_PROMPT.md` and `LAUNCHER_SCOPE.md`.

No subagents authorized. All launcher/game fixtures muted. No further course/AI/audio tests. Physical Deck unavailable; keep verification pending. Build and validate locally before asking for concrete public publication approval: prompt explicitly says a build is not automatic upload authorization. Do not ask for a different repository.

Repository `danemoll-jpg/woodstock-rush-releases` checked anonymously via GitHub API: public, size 0, default main, no releases. Authenticated publishing access not yet checked. Web fetch of repo had cache miss; shell API worked. GitHub docs confirmed stable latest asset URL syntax. No uploads.

Native compiler available: Visual Studio 18 Community, MSVC14.51.36231; vcvars64.bat at `C:/Program Files/Microsoft Visual Studio/18/Community/VC/Auxiliary/Build/vcvars64.bat`. Use C++ native Win32/GDI+/XInput, static CRT, WinHTTP/BCrypt. No separately installed desktop runtime. Physical Proton/Deck remains unverified. Prototype current-game child lifetime first, then shared updater core. Source dependency release discovery in progress for official nlohmann/json and richgel999/miniz (pin source/hash/licenses).

Game integration audit:

- Company/Product `DefaultCompany/Racer`; preserve save identity and same Proton prefix.

- RaceFlow.Start uses Application.persistentDataPath/Phase7/street-loop-gates-v1-lapsN; isolated `-racerTestSave` override already exists.

- LocalRadio.BundledFolder is runtime sibling Music. MusicSource bundled/custom persists; custom folder must stay. Scan currently one root via MusicCollection.Scan; folder station IDs are root:/folder:Name. Add explicit launcher-managed/personal roots for bundled scan, merge channels by stable station ID. Ordinary portable sibling Music remains fallback. No other music tests.

- Need small ready handshake after startup; nonce/path flags, no sound or gameplay changes.

Planned structure: native launcher stable entry, versions/staging/shared directories, signed catalog with independent signed game/soundtrack manifests. RSA-SHA256 verification key pinned; private key only excluded publisher storage, not source/download. ZIP safe paths/link rejection, SHA/size/inventory, state atomic selection, previous version rollback, journal recovery, single updater, running-child defer, async/cancellable downloads/free-space checks. Managed soundtrack per-file sync, preserve personal/unknown/modified songs; positive path/hash ownership. Publisher dry run and explicit inventory; no automatic publish.

User deliverables include complete starter package with existing 187 playable songs + two originals, launcher/update/rollback/recovery checks, small soundtrack delta fixture, publisher helper and one-time Windows/Deck migration instructions. Existing Steam shortcut/AppID/prefix must be preserved; no delete/re-add. Consider keeping old executable path stable with backed-up original if appropriate. Real Deck checklist mandatory; no physical acceptance claim.

## Publishing-access approval block

Anonymous GitHub API proved the supplied repo public/empty/no releases. Automatic approval review REJECTED the non-interactive credential-based push-permission check, stating it considered the launcher repository unauthorized/credential probing. Do NOT bypass this rejection via another credential tool or indirect command. No credential command executed. Continue local prototype/core/publisher dry run. Authenticated check/publication needs explicit approval after concrete local package exists; quote/link prompt rule that builds are not automatic upload authorization. Report this block separately in final.

Dependencies downloaded: nlohmann/json v3.12.0 json.hpp + MIT; miniz3.1.2 miniz.c/.h + license, hashes Docs/CR119/dependency-hashes.json. Prototype Launcher/prototype.cpp and Tools/Build-LauncherPrototype.ps1 created. First compile failed missing windows.h: Visual Studio has compiler but no Windows SDK. SDK repair via official Microsoft.Windows.SDK.CPP and CPP.x64 NuGet10.0.28000.2705 into Builds/LauncherSDK (no system install). First flat .sha512 URL404; correct catalogEntry packageHash verification used instead. SDK download/extract currently running. Next point compiler INCLUDE/LIB to local SDK, compile/prototype muted current-game child lifetime; then complete signed updater/managed soundtrack implementation and bounded fixtures.

Implementation progress: prototype passed current 0.20.1 child lifetime; local SDK verified. Native core/UI compile. Initial core fixtures all 15 passed after fixing transient MoveFileEx sharing conflict (bounded retries) and path separator mismatch in running-process comparison. Final cleanup/reused-retained-version changes need affected fixture rerun. CLI fixture source and Python scripts in Launcher/Tools. Signing key is excluded Builds/PublisherPrivate, never ship. Native public key pinned. Initial runtime 0.21.0-launcher1 built successfully (job b1f1326c6ff14294a7641819fd8ac084). First draft Builds/LauncherRelease-21001 contains 220 runtime files,189 soundtrack files,225757495-byte game ZIP. Final runtime rebuild with test-only discovery logging job daccfc59b4334e1fb4e66b9dcd32dc67 pending; prepare final draft21002 after build. Native final GUI built SHA0F4E8FC45A403F3BF27C27A99E2F0DBD49F8B9AAB4DB990546E947E04A69ED61; CLI rebuilding session84855. Need full signed old21001->new21002 install + muted actual startup/rollback startup, starterZIP/extract verification, docs and completion commit. Publisher no uploads/auth attempts. Physical Deck/controller pending.

Publication follow-up: Dan explicitly approved releasebuild-21002. Safety HEADc73d74badd6ac7776783b03bace80b97f5dac4e2 was clean. GitHub limit confirmed per-file2GiB, no aggregate release cap. Official portable gh2.101.0 verified SHA256bc6c814367b193cd8e713611d61e36013c0ef843b8f516458fe3eda039192794. Authenticated push permission confirmed without printing credentials. Release393381093 published2026-09-22T00:46:30Z; all194 public asset digests/sizes match. Draft tag404 diagnosed and safely resumed using release ID with no asset overwrite/reupload. Public notes supersede packaged prepublication status. Hosted update and muted startup PASSED; see publication.json and hosted/result.json. No further automated checks required. Physical Deck/controller/existing-prefix acceptance remains pending.
