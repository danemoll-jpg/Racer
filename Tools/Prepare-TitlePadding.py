"""Dan's final CR-118 attempt: preserve source PCM and prepend one second of silence."""
from pathlib import Path
import hashlib
import json
import wave

root = Path(__file__).resolve().parent.parent
source = root / "Assets/Resources/Title/Voice.wav"
target = source.with_name("VoicePadded.wav")
with wave.open(str(source), "rb") as audio:
    params = audio.getparams()
    pcm = audio.readframes(audio.getnframes())
assert params.sampwidth == 2, "Expected signed 16-bit source PCM"
silence = bytes(params.framerate * params.nchannels * params.sampwidth)
with wave.open(str(target), "wb") as audio:
    audio.setparams(params)
    audio.writeframes(silence + pcm)
with wave.open(str(target), "rb") as audio:
    assert audio.readframes(audio.getnframes()) == silence + pcm
report = {
    "source": str(source.relative_to(root)),
    "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
    "derivative": str(target.relative_to(root)),
    "leading_silence_frames": params.framerate,
    "original_frames": params.nframes,
    "original_pcm_preserved_exactly": True,
    "audible_completeness": "Awaiting Dan; sample identity is not a listening verdict",
}
(root / "Docs/CR121-followup/title-padding.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
