# Original smash audio

CR-035 uses original mathematical synthesis in `Assets/Scripts/SmashAudio.cs`. No recordings, sample packs, third-party music, or external sound assets are included. The generated PCM clips are created at runtime and carry the same project usage rights as the original game source; there are no third-party attribution obligations.

Wood combines a sharp noise/transient with decaying lower resonance and several quieter splinter ticks. Chain-link, mailbox and sign variants use different inharmonic resonances and decaying clatter pulses. Three deterministic timbres and five pitch offsets prevent identical repeated hits. Collision speed controls gain. Four shared spatial voices and a 75ms global onset cooldown bound repeated fence impacts. Each prop emits once per break; body-impact handling excludes breakables. Master listener volume and the existing vehicle slider apply. Pause preserves paused voices; quit/countdown stops them.

The original yielding trigger, 24 moving-prop limit, four-second cleanup, occupied restoration wait and race-restart restoration remain in place. Listening satisfaction still requires Dan's review.
