"""Generate the Woodstock Rush police radio clips with ElevenLabs.

Reads Tools/Audio/police-radio-lines.csv (id,text) and saves one MP3 per line
into SourceArt/Audio/PoliceRadio/<id>.mp3 using your ElevenLabs voice.
Lines that already have a file there (any extension, e.g. R01.wav or R01.mp3)
are skipped, so it is safe to run again; it only makes what is missing.

Usage (from the Racer folder, in a terminal):
    set ELEVENLABS_API_KEY=your_key_here
    python Tools/Audio/generate_police_radio.py
Options:
    --voice   voice name or voice ID (default 319AW1BlfYo8QKsZ2K9o, Sheriff John Paul)
    --only    comma-separated IDs to (re)make, e.g. --only P01,P02 (overwrites)
    --takes   number of takes per line (extra takes saved as R05a, R05b ...)
    --list    just list your voices and exit
Uses only Python's standard library.
"""
import argparse, csv, json, os, sys, time, urllib.request, urllib.error

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
LINES = os.path.join(ROOT, "Tools", "Audio", "police-radio-lines.csv")
OUT = os.path.join(ROOT, "SourceArt", "Audio", "PoliceRadio")
API = "https://api.elevenlabs.io/v1"

def call(path, key, body=None):
    req = urllib.request.Request(API + path, data=None if body is None else json.dumps(body).encode(),
                                 headers={"xi-api-key": key, "Content-Type": "application/json"},
                                 method="GET" if body is None else "POST")
    with urllib.request.urlopen(req, timeout=120) as r:
        return r.read()

def voices(key):
    return json.loads(call("/voices", key))["voices"]

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--voice", default="319AW1BlfYo8QKsZ2K9o")  # Sheriff John Paul
    ap.add_argument("--only", default="")
    ap.add_argument("--takes", type=int, default=1)
    ap.add_argument("--model", default="eleven_multilingual_v2")
    ap.add_argument("--list", action="store_true")
    a = ap.parse_args()
    key = os.environ.get("ELEVENLABS_API_KEY", "").strip()
    if not key:
        sys.exit("Set ELEVENLABS_API_KEY first (ElevenLabs > Profile > API keys).")
    if a.list:
        for v in voices(key): print(f'{v["voice_id"]}  {v["name"]}')
        return
    vid = a.voice
    if " " in vid or len(vid) != 20:  # a name, not an ID: look it up
        match = [v for v in voices(key) if v["name"].lower() == vid.lower()]
        if not match:
            sys.exit(f'Voice "{vid}" not found. Run with --list to see your voices.')
        vid = match[0]["voice_id"]
    os.makedirs(OUT, exist_ok=True)
    have = {os.path.splitext(f)[0].upper() for f in os.listdir(OUT)}
    have |= {h[:-1] for h in list(have) if len(h) > 2 and h[-1] in "ABCDEFGHIJ" and h[-2].isdigit()}  # R02a/R02b count as R02
    only = {s.strip().upper() for s in a.only.split(",") if s.strip()}
    rows = list(csv.DictReader(open(LINES, encoding="utf-8")))
    made = skipped = failed = 0
    for row in rows:
        lid, text = row["id"].strip(), row["text"].strip()
        if only and lid.upper() not in only: continue
        for t in range(a.takes):
            name = lid if t == 0 else lid + "abcdefghij"[t - 1]
            if not only and name.upper() in have:
                skipped += 1; continue
            body = {"text": text, "model_id": a.model,
                    "voice_settings": {"stability": 0.45, "similarity_boost": 0.8, "style": 0.2}}
            try:
                audio = call(f"/text-to-speech/{vid}?output_format=mp3_44100_128", key, body)
                open(os.path.join(OUT, name + ".mp3"), "wb").write(audio)
                made += 1; print(f"made {name}: {text}")
            except urllib.error.HTTPError as e:
                failed += 1; print(f"FAILED {name}: {e.code} {e.read()[:200]!r}")
                if e.code in (401, 402, 429): sys.exit("Stopping (key, credits or rate limit). Run again later; done files are kept.")
            time.sleep(0.4)
    print(f"Done: {made} made, {skipped} already there, {failed} failed. Files in {OUT}")

if __name__ == "__main__":
    main()
