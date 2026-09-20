"""Summarize existing evidence without treating process completion as a test pass."""
from pathlib import Path
import csv,json,sys
root=Path(__file__).resolve().parent.parent/'Docs/CR097-100'
tag=sys.argv[1] if len(sys.argv)>1 else 'release'
base=root/tag
checks=[]; processes=[]; ramps=[]; flights=[]
for p in base.rglob('*checks.txt'):
    checks += [dict(file=str(p.relative_to(root)),text=s) for s in p.read_text().splitlines() if s.startswith(('PASS ','FAIL '))]
for p in base.rglob('process.json'):
    q=json.loads(p.read_text(encoding='utf-8-sig'));q['evidence']=str(p.parent.relative_to(root));processes.append(q)
for name,target in [('ramps.csv',ramps),('flight.csv',flights)]:
    for p in base.rglob(name):
        for q in csv.DictReader(p.open()):q['evidence']=str(p.parent.relative_to(root));target.append(q)
forward=[q for q in ramps if q.get('direction')=='1']
flights=[q for q in flights if float(q.get('launchY',0))>0]
result=dict(tag=tag,assertions=len(checks),passed=sum(q['text'].startswith('PASS ') for q in checks),
    failures=[q for q in checks if q['text'].startswith('FAIL ')],processes=len(processes),
    incompleteProcesses=[q for q in processes if q.get('Timeout') or q.get('Exit')!=0],
    summitCases=len(ramps),forwardCases=len(forward),
    summitNonScoring=sum(q.get('awards')=='0' for q in forward),
    summitFailedCompletion=[q for q in ramps if q.get('completed')!='True'],
    summitFailedReset=[q for q in ramps if q.get('recovered')!='True'],
    summitInstability=[q for q in ramps if float(q.get('minUp',1))<.65],
    flightRanges={k:[min(float(q[k]) for q in flights),max(float(q[k]) for q in flights)] for k in ['longestFlight','crestClearance','launchSpeed','landingSpeed'] } if flights else {})
(root/(tag+'-summary.json')).write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
