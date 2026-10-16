"""1.0 trailer: the contact sheet (Trailer/contact-sheet.jpg): three frames of every clip in Trailer/Clips (at a quarter, half and
three quarters of it), labeled with the clip's name and length, so Dan can review the takes fast.
Usage: python sheet.py [--clips Trailer/Clips] [--out Trailer/contact-sheet.jpg]"""
import argparse, json, pathlib, subprocess, tempfile
from PIL import Image, ImageDraw, ImageFont

ROOT = pathlib.Path(__file__).resolve().parents[3]
FFMPEG = next((ROOT / 'Tools/Trailer/pylib/imageio_ffmpeg/binaries').glob('ffmpeg*.exe'))
W, H, LABEL = 480, 270, 34


def length(clip):
    # ffmpeg prints the duration on stderr when given no output
    err = subprocess.run([str(FFMPEG), '-hide_banner', '-i', str(clip)], capture_output=True, text=True).stderr
    for line in err.splitlines():
        if 'Duration:' in line:
            h, m, s = line.split('Duration:')[1].split(',')[0].strip().split(':')
            return int(h) * 3600 + int(m) * 60 + float(s)
    return 0.0


def frame(clip, at, tmp):
    out = pathlib.Path(tmp) / 'f.png'
    subprocess.run([str(FFMPEG), '-y', '-loglevel', 'error', '-ss', f'{at:.2f}', '-i', str(clip), '-frames:v', '1', '-vf', f'scale={W}:{H}', str(out)], check=True)
    return Image.open(out).convert('RGB')


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--clips', type=pathlib.Path, default=ROOT / 'Trailer/Clips')
    p.add_argument('--out', type=pathlib.Path, default=ROOT / 'Trailer/contact-sheet.jpg')
    a = p.parse_args()
    clips = sorted(a.clips.glob('*.mp4'))
    font = ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf', 22)
    sheet = Image.new('RGB', (W * 3, (H + LABEL) * len(clips)), (16, 18, 22))
    draw = ImageDraw.Draw(sheet)
    with tempfile.TemporaryDirectory() as tmp:
        for i, clip in enumerate(clips):
            d = length(clip); y = i * (H + LABEL)
            draw.text((8, y + 6), f'{clip.stem}   ({d:.1f} s)', font=font, fill=(235, 235, 235))
            for k, f in enumerate((.25, .5, .75)):
                sheet.paste(frame(clip, d * f, tmp), (k * W, y + LABEL))
    sheet.save(a.out, quality=85)
    print(json.dumps(dict(sheet=str(a.out), clips=len(clips))))


if __name__ == '__main__':
    main()
