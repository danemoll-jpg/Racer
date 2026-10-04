"""Unity scene YAML helpers for the 0.76 restore: parse documents, resolve hierarchy paths, compare two versions."""
import re,sys,collections
HDR=re.compile(r'^--- !u!(\d+) &(-?\d+)( stripped)?',re.M)
def parse(text):
    """Returns (preamble, ordered list of (fileID, classID, header, body))."""
    ms=list(HDR.finditer(text));docs=[]
    pre=text[:ms[0].start()] if ms else text
    for i,m in enumerate(ms):
        end=ms[i+1].start() if i+1<len(ms) else len(text)
        docs.append((m.group(2),int(m.group(1)),text[m.start():m.end()],text[m.end():end]))
    return pre,docs
def field(body,name):
    m=re.search(r'^  '+name+r': (.*)$',body,re.M);return m.group(1).strip() if m else None
def fid(v):
    m=re.search(r'fileID: (-?\d+)',v or '');return m.group(1) if m else None
class Scene:
    def __init__(self,text):
        self.pre,self.docs=parse(text);self.by={d[0]:d for d in self.docs}
        self.go_name={};self.tr_go={};self.tr_father={};self.go_tr={};self.comp_go={}
        for f,c,h,b in self.docs:
            if c==1:self.go_name[f]=(field(b,'m_Name') or '')
            elif c in(4,224):
                g=fid(field(b,'m_GameObject'));self.tr_go[f]=g;self.tr_father[f]=fid(field(b,'m_Father'));self.go_tr[g]=f
            if c!=1:
                g=fid(field(b,'m_GameObject'))
                if g:self.comp_go[f]=g
        self._p={}
    def path_go(self,g):
        if g in self._p:return self._p[g]
        n=self.go_name.get(g,'?');t=self.go_tr.get(g);parts=[n];seen=0
        while t and self.tr_father.get(t) not in(None,'0') and seen<64:
            t=self.tr_father[t];parts.append(self.go_name.get(self.tr_go.get(t),'?'));seen+=1
        p='/'.join(reversed(parts));self._p[g]=p;return p
    def path_doc(self,f):
        d=self.by[f]
        if d[1]==1:return self.path_go(f)
        g=self.comp_go.get(f)
        return self.path_go(g) if g else '<'+str(d[1])+'>'
    def root_tr(self,g):
        t=self.go_tr.get(g)
        while t and self.tr_father.get(t) not in(None,'0'):t=self.tr_father[t]
        return t
def load(path):
    with open(path,encoding='utf-8',newline='') as fh:return Scene(fh.read())
if __name__=='__main__':
    a=load(sys.argv[1]);b=load(sys.argv[2]);depth=int(sys.argv[3]) if len(sys.argv)>3 else 2
    added=[f for f in b.by if f not in a.by];removed=[f for f in a.by if f not in b.by]
    changed=[f for f in b.by if f in a.by and a.by[f][3]!=b.by[f][3]]
    def key(s,f):return '/'.join(s.path_doc(f).split('/')[:depth])
    for label,lst,s in(('ADDED',added,b),('REMOVED',removed,a),('CHANGED',changed,b)):
        c=collections.Counter(key(s,f) for f in lst)
        print(f'== {label} {len(lst)}')
        for k,n in c.most_common():print(f'{n:7d}  {k}')
