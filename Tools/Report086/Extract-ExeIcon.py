"""0.86 Part B (read only): save the largest icon image embedded in a Windows .exe as PNG (no extra packages).
Reads the PE resource directory, takes every RT_ICON (type 3) entry, keeps the largest; PNG-compressed icons are saved
as is, classic DIB icons are converted with Pillow. Prints what it found (or that the exe has no icon resource).
Usage: python Extract-ExeIcon.py <exe> <out.png>"""
import sys,struct,io
from PIL import Image
exe,outp=sys.argv[1:3];b=open(exe,'rb').read()
pe=struct.unpack_from('<I',b,0x3c)[0];assert b[pe:pe+4]==b'PE\0\0'
nsec=struct.unpack_from('<H',b,pe+6)[0];optsz=struct.unpack_from('<H',b,pe+20)[0];opt=pe+24
magic=struct.unpack_from('<H',b,opt)[0];dd=opt+(112 if magic==0x20b else 96)
rva,size=struct.unpack_from('<II',b,dd+2*8)
secs=[struct.unpack_from('<8sIIII',b,opt+optsz+i*40) for i in range(nsec)]
def off(r):
    for name,vsz,va,rsz,raw in secs:
        if va<=r<va+max(vsz,rsz):return r-va+raw
    raise ValueError(r)
if not rva:print(f'{exe}: no resource directory (no icon)');sys.exit(0)
base=off(rva)
def entries(d):
    n1,n2=struct.unpack_from('<HH',b,d+12);out=[]
    for i in range(n1+n2):
        name,ofs=struct.unpack_from('<II',b,d+16+i*8);out.append((name,ofs))
    return out
icons=[]
for name,ofs in entries(base):
    if name!=3 or not ofs&0x80000000:continue
    for n2,o2 in entries(base+(ofs&0x7fffffff)):
        for n3,o3 in entries(base+(o2&0x7fffffff)):
            drva,dsz=struct.unpack_from('<II',b,base+o3);icons.append(b[off(drva):off(drva)+dsz])
if not icons:print(f'{exe}: no RT_ICON resources (Windows shows the generic program icon)');sys.exit(0)
def size_of(d):
    if d[:8]==b'\x89PNG\r\n\x1a\n':return Image.open(io.BytesIO(d)).size[0]
    return struct.unpack_from('<i',d,4)[0]
best=max(icons,key=size_of)
if best[:8]==b'\x89PNG\r\n\x1a\n':open(outp,'wb').write(best)
else:
    # DIB icon: wrap as a one-image .ico and let Pillow decode it
    w=size_of(best);h=w;ico=struct.pack('<HHH',0,1,1)+struct.pack('<BBBBHHII',w%256,h%256,0,0,1,32,len(best),22)+best
    Image.open(io.BytesIO(ico)).save(outp)
print(f'{exe}: {len(icons)} icon images, sizes {sorted(size_of(d) for d in icons)}; saved the {size_of(best)} px one to {outp}')
