# Part A: British spellings inside string literals (what can reach the screen), in game scripts, scenes/prefabs, data files, the launcher
# and the documents shipped with the game. Lists every hit; the fixes are made by hand (fixspell.py) so code names, files and save keys stay.
import re,os,sys,json
ROOT='C:/Users/danmo/Racer'
STEMS=(r"colour|grey|centre|metre|favourit|honour|licence|tyre|kerb|theatre|travell|cancell|practis|organis|realis|recognis|analys|defence|offence|"
       r"catalogue|programme|behaviour|neighbour|harbour|labour|armour|odour|rumour|flavour|humour|savour|endeavour|aluminium|manoeuvr|storey|whilst|"
       r"learnt|spelt|modell|levell|signall|fuell|labell|marvell|enrol|fulfil|skilful|ageing|artefact|kilometre|litre|tonne|customis|personalis|minimis|"
       r"maximis|optimis|finalis|prioritis|apologis|summaris|visualis|synchronis|normalis|stabilis|utilis|initialis|authoris|categoris|memoris|specialis|"
       r"cosy|mould|plough|draught|sceptic|jewell|pyjama|cheque|aeroplane|dialogue|analogue|instalment|enquir|amongst|dialled|counsell|totall|quarrell|"
       r"rivall|tunnell|channell|funnell|focuss|duell|panell|pedall|shovell|equall|fulfil")
RX=re.compile(r"\b[A-Za-z]*(?:%s)[A-Za-z]*\b"%STEMS,re.I)
LIT=re.compile(r'@"(?:[^"]|"")*"|\$?"(?:[^"\\\n]|\\.)*"')
hits=[]
def add(path,line,word,ctx):hits.append((path.replace(ROOT+'/',''),line,word,ctx))
def scan_cs(path):
    src=open(path,encoding='utf-8',errors='replace').read()
    for m in LIT.finditer(src):
        s=m.group(0)
        for w in RX.finditer(s):add(path,src.count('\n',0,m.start())+1,w.group(0),s[:200])
def walk(top,skip=()):
    for d,_,fs in os.walk(top):
        d=d.replace(os.sep,'/')
        if any(k in d for k in skip):continue
        for f in fs:yield d+'/'+f
for p in walk(ROOT+'/Assets',('_Recovery','/Editor/Report')):
    f=p.rsplit('/',1)[1]
    if f.endswith('.cs'):scan_cs(p)
    elif f.endswith(('.json','.txt','.md')) and ('/Resources/' in p or '/Track/' in p):
        t=open(p,encoding='utf-8',errors='replace').read()
        for w in RX.finditer(t):add(p,t.count('\n',0,w.start())+1,w.group(0),t[max(0,w.start()-60):w.end()+60].replace('\n',' '))
    elif f.endswith(('.unity','.prefab')):
        for i,l in enumerate(open(p,encoding='utf-8',errors='replace')):
            if 'm_Text:' in l or 'text:' in l or 'title:' in l or 'm_Name:' in l:
                for w in RX.finditer(l):add(p,i+1,w.group(0),l.strip()[:200])
for top in ('Launcher','LauncherSDK'):
    for p in walk(ROOT+'/'+top,('/bin','/obj')):
        if p.endswith('.cs'):scan_cs(p)
for f in ['Docs/DebugReporting/DEBUG_MODE.md','Docs/TrailerMode/TRAILER_MODE.md','Builds/Latest/RADIO.md']:
    if os.path.exists(ROOT+'/'+f):
        t=open(ROOT+'/'+f,encoding='utf-8').read()
        for w in RX.finditer(t):add(ROOT+'/'+f,t.count('\n',0,w.start())+1,w.group(0),t[max(0,w.start()-60):w.end()+60].replace('\n',' '))
if __name__=='__main__':
    from collections import Counter
    print(Counter(h[2].lower() for h in hits).most_common(90))
    json.dump(hits,open(ROOT+'/Tools/Report104/py/spelling-hits.json','w'),indent=0)
    print(len(hits),'hits in',len(set(h[0] for h in hits)),'files')
