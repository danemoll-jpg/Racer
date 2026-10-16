"""1.0 trailer: the first cut from Trailer/Clips, in shot-list order and timing (Docs/Trailer/shot-list.md).
Writes Trailer/WoodstockRush-trailer-v2.mp4 (the game's sound) and Trailer/WoodstockRush-trailer-v2-silent.mp4 (for music); if a track
is in Trailer/Music/, also Trailer/WoodstockRush-trailer-v2-music.mp4 with it mixed under the game sound.
Simple cuts, a short fade from black at the start, the title over the last shot, the cover art as the end card. 3840x2160, 60 fps.
Usage: python cut.py"""
import json, pathlib, subprocess, tempfile
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = pathlib.Path(__file__).resolve().parents[3]
FFMPEG = str(next((ROOT / 'Tools/Trailer/pylib/imageio_ffmpeg/binaries').glob('ffmpeg*.exe')))
CLIPS, OUT = ROOT / 'Trailer/Clips', ROOT / 'Trailer'
COVER = ROOT / 'Builds/Latest/WoodstockRush-Cover.png'
W, H, FPS = 3840, 2160, 60

# (shot, clip, in point s, length s): the shot list's order; in points chosen from the contact sheet
EDIT = [
    ('1', '01-dawn-road', 1.0, 4.0),
    ('2', '02-rider-looks', 0.2, 3.0),
    ('3', '03-garage-vehicles', 3.2, 3.0),
    ('4', '04-race-start', 1.5, 3.0),
    ('5', '05-hwy92', 2.0, 3.0),
    ('6N', '06-rider-close', 1.0, 3.0),
    ('7', '07-fence-smash', 1.0, 2.0),
    ('8', '08-mower-lawn', 0.5, 2.0),
    ('9N', '09-leviathan-summit', 3.0, 4.0),
    ('10', '10-cabin-jump', 3.0, 3.0),
    ('11', '11-lightning', 1.2, 3.0),
    ('13', '13a-sledders', 1.0, 2.0),
    ('13', '13b-broom-hockey', 1.0, 2.0),
    ('14', '14-hidden-cop', 2.0, 3.0),
    ('15', '15-getaway', 3.0, 4.0),
    ('S1', 'S1-split-race', 1.0, 2.0),
    ('S2', 'S2-split-police', 2.0, 2.0),
    ('S3', 'S3-split-stunt', 1.0, 3.0),
    ('16', '16-park-reveal', 1.5, 2.0),
    ('17', '17-buggy-backflip', 2.0, 3.0),
    ('18', '18a-bike-loop', 1.5, 1.5),
    ('18', '18b-bike-barrel-roll', 1.0, 1.5),
    ('18N', '12-stunt-race-tricks', 2.0, 3.0),
    ('19', '19-giant-jump', 3.0, 6.0),
    ('20', '20-cover', 1.0, 4.0),
]
EDIT_FILE = pathlib.Path(__file__).with_name('edit.json')  # the edit as used (in points may be tuned there without touching this file)


def run(*args):
    subprocess.run([FFMPEG, '-y', '-hide_banner', '-loglevel', 'error', *map(str, args)], check=True)


def has_audio(clip):
    err = subprocess.run([FFMPEG, '-hide_banner', '-i', str(clip)], capture_output=True, text=True).stderr
    return 'Audio:' in err


def title_png(path):
    img = Image.new('RGBA', (W, H), (0, 0, 0, 0)); font = ImageFont.truetype('C:/Windows/Fonts/ariblk.ttf', 300)
    text = 'WOODSTOCK RUSH'; d = ImageDraw.Draw(img); box = d.textbbox((0, 0), text, font=font); x = (W - (box[2] - box[0])) // 2; y = int(H * .14)
    shadow = Image.new('RGBA', (W, H), (0, 0, 0, 0)); ImageDraw.Draw(shadow).text((x + 12, y + 14), text, font=font, fill=(0, 0, 0, 170))
    img = Image.alpha_composite(img, shadow.filter(ImageFilter.GaussianBlur(10))); d = ImageDraw.Draw(img)
    d.text((x, y), text, font=font, fill=(255, 196, 40, 255), stroke_width=12, stroke_fill=(120, 30, 0, 255))
    img.save(path)


def main():
    edit = json.loads(EDIT_FILE.read_text()) if EDIT_FILE.exists() else [list(e) for e in EDIT]
    EDIT_FILE.write_text(json.dumps(edit, indent=1))
    vf = f'scale={W}:{H}:force_original_aspect_ratio=decrease,pad={W}:{H}:(ow-iw)/2:(oh-ih)/2,fps={FPS},setsar=1,format=yuv420p'
    enc = ['-c:v', 'libx264', '-preset', 'medium', '-crf', '16', '-r', FPS, '-c:a', 'aac', '-b:a', '256k', '-ar', '48000', '-ac', '2']
    with tempfile.TemporaryDirectory() as tmp:
        tmp = pathlib.Path(tmp); parts = []; total = 0.0
        title = tmp / 'title.png'; title_png(title)
        for i, (shot, name, start, dur) in enumerate(edit):
            clip = CLIPS / f'{name}.mp4'; part = tmp / f'{i:02d}.mp4'
            if not clip.exists(): print(f'missing {name}: skipped'); continue
            v = vf + (',fade=in:st=0:d=0.6' if i == 0 else '')
            af = f'afade=in:st=0:d=0.05,afade=out:st={dur - .05:.2f}:d=0.05' + (',afade=in:st=0:d=0.6' if i == 0 else '')
            src = ['-ss', start, '-t', dur, '-i', clip]
            if not has_audio(clip): src += ['-f', 'lavfi', '-t', dur, '-i', 'anullsrc=r=48000:cl=stereo']
            if i == len(edit) - 1:  # the title over the last shot, fading in after a second
                run(*src, '-loop', 1, '-t', dur, '-i', title, '-filter_complex', f'[0:v]{vf}[b];[{2 if not has_audio(clip) else 1}:v]format=rgba,fade=in:st=1:d=0.8:alpha=1[t];[b][t]overlay=0:0,format=yuv420p[v];[{1 if not has_audio(clip) else 0}:a]{af}[a]', '-map', '[v]', '-map', '[a]', *enc, part)
            else:
                run(*src, '-vf', v, '-af', af, '-map', '0:v', '-map', '1:a' if not has_audio(clip) else '0:a', *enc, part)
            parts.append(part); total += dur
        card = tmp / 'card.mp4'  # the end card: the cover art, 3 s, fading in and out, silent
        run('-loop', 1, '-t', 3, '-i', COVER, '-f', 'lavfi', '-t', 3, '-i', 'anullsrc=r=48000:cl=stereo', '-vf', vf + ',fade=in:st=0:d=0.3,fade=out:st=2.5:d=0.5', '-map', '0:v', '-map', '1:a', *enc, card)
        parts.append(card); total += 3
        lst = tmp / 'list.txt'; lst.write_text(''.join(f"file '{p.as_posix()}'\n" for p in parts))
        game = OUT / 'WoodstockRush-trailer-v2.mp4'; silent = OUT / 'WoodstockRush-trailer-v2-silent.mp4'
        run('-f', 'concat', '-safe', 0, '-i', lst, '-c', 'copy', '-movflags', '+faststart', game)
        run('-i', game, '-c:v', 'copy', '-an', '-movflags', '+faststart', silent)
        made = [str(game), str(silent)]
        music = sorted(p for p in (OUT / 'Music').glob('*') if p.suffix.lower() in ('.mp3', '.wav', '.m4a', '.flac', '.ogg', '.aac')) if (OUT / 'Music').exists() else []
        if music:  # the music under the game sound (game sound kept, music at about -6 dB, faded out with the end card)
            mixed = OUT / 'WoodstockRush-trailer-v2-music.mp4'
            run('-i', game, '-i', music[0], '-filter_complex', f'[1:a]volume=0.5,afade=out:st={total - 2:.2f}:d=2[m];[0:a][m]amix=inputs=2:duration=first:normalize=0[a]', '-map', '0:v', '-map', '[a]', '-c:v', 'copy', '-c:a', 'aac', '-b:a', '256k', '-movflags', '+faststart', mixed)
            made.append(str(mixed))
    print(json.dumps(dict(made=made, seconds=round(total, 1), shots=len(parts) - 1)))


if __name__ == '__main__':
    main()
