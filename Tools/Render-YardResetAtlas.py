from pathlib import Path
from html import escape
root=Path(__file__).resolve().parents[1];out=root/'Docs/YardReset'
anchors=[(463.6,8,'START / FINISH'),(422.7,9,'HILL START'),(409.5,9.3,'PARKING START'),(391.9,9.7,'DIRT PATH START'),(331.4,16.2,'FUTURE DUMP LAUNCH'),(258.6,8.2,'FUTURE DUMP LANDING'),(96.1,-108.6,'FUTURE BIG GULLY'),(456.4,67.4,'FOREST RETURN')]
parts=['<svg xmlns="http://www.w3.org/2000/svg" width="1900" height="1120" viewBox="0 0 1900 1120">','<rect width="1900" height="1120" fill="#12221c"/>','<text x="28" y="42" fill="white" font-size="29" font-family="sans-serif">RESTORED WORLD · ANCHOR VALIDATION ONLY</text>','<text x="28" y="77" fill="#e6eddf" font-size="20" font-family="sans-serif">Dan’s Backyard rejected / rolled back. No route connects these points. North (+Z) up.</text>','<image href="restored-world-overhead.png" x="0" y="105" width="1500" height="1000"/>']
for i,(x,z,label) in enumerate(anchors,1):
    px=(x-(-5))/570*1500;py=105+(165-z)/380*1000
    parts.extend([f'<circle cx="{px:.2f}" cy="{py:.2f}" r="15" fill="#f843a2" stroke="white" stroke-width="2"/>',f'<text x="{px:.2f}" y="{py+6:.2f}" text-anchor="middle" fill="white" font-size="18" font-weight="bold" font-family="sans-serif">{i}</text>',f'<text x="1530" y="{150+i*93}" fill="#ff7dbe" font-size="20" font-family="sans-serif">{i} · {escape(label)}</text>',f'<text x="1530" y="{177+i*93}" fill="white" font-size="19" font-family="sans-serif">X={x:.1f} / Z={z:.1f}</text>'])
parts.extend(['<text x="1530" y="990" fill="white" font-size="19" font-family="sans-serif">Temporary · non-colliding</text>','<text x="1530" y="1022" fill="white" font-size="19" font-family="sans-serif">Awaiting Dan’s validation</text>','</svg>'])
(out/'anchor-validation.svg').write_text('\n'.join(parts),encoding='utf-8')
print('Anchor atlas generated from actual world capture; no connecting lines.')
