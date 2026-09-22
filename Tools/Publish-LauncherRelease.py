"""Deliberate publisher operation. Default is dry run; never called by game builds."""
import argparse, hashlib, json, pathlib, subprocess, urllib.request
REPO='danemoll-jpg/woodstock-rush-releases'
def gh(*args):
    return subprocess.run(['gh',*args],check=True,capture_output=True,text=True).stdout
def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('draft',type=pathlib.Path);p.add_argument('--publish',action='store_true');p.add_argument('--confirm-repository');a=p.parse_args()
    inventory=json.loads((a.draft/'INVENTORY.json').read_text());assets=a.draft/'assets';tag=inventory.get('tag','build-'+str(inventory['game']['build']))
    for item in inventory['assets']:
        file=assets/item['path']
        if not file.is_file() or file.stat().st_size!=item['bytes']: raise ValueError('Incomplete staged asset')
        h=hashlib.sha256()
        with file.open('rb') as f:
            for block in iter(lambda:f.read(1024*1024),b''): h.update(block)
        if h.hexdigest()!=item['sha256']: raise ValueError('Staged asset changed after inventory')
    print(json.dumps(dict(repository=REPO,tag=tag,assets=len(inventory['assets']),bytes=sum(x['bytes'] for x in inventory['assets']),publish=a.publish)))
    if not a.publish:return
    if a.confirm_repository!=REPO:raise ValueError('Explicit repository confirmation required')
    # No credential extraction. gh uses the publisher\'s own deliberately configured session.
    repo=json.loads(gh('api','repos/'+REPO));
    if repo['private'] or not repo.get('permissions',{}).get('push'):raise ValueError('Public repository with publishing permission required')
    releases=json.loads(gh('api','repos/'+REPO+'/releases?per_page=100'))
    if any(r['tag_name']==tag for r in releases):raise ValueError('Immutable release already exists; do not overwrite its assets')
    gh('release','create',tag,'--repo',REPO,'--draft','--title','Woodstock Rush '+inventory['game']['version'],'--notes',inventory['game']['notes'])
    # Upload all immutable content into the draft. The catalog is uploaded last.
    for item in sorted(inventory['assets'],key=lambda x:x['path']=='update-catalog.json'):
        gh('release','upload',tag,str(assets/item['path']),'--repo',REPO)
    remote=json.loads(gh('api','repos/'+REPO+'/releases/tags/'+tag));by_name={x['name']:x for x in remote['assets']}
    for item in inventory['assets']:
        r=by_name.get(item['path'])
        if not r or r['size']!=item['bytes'] or r.get('digest')!='sha256:'+item['sha256']:raise ValueError('Uploaded asset verification failed; draft remains unpublished')
    gh('release','edit',tag,'--repo',REPO,'--draft=false','--latest')
    with urllib.request.urlopen('https://github.com/'+REPO+'/releases/latest/download/update-catalog.json',timeout=20) as response: catalog=response.read()
    if hashlib.sha256(catalog).hexdigest()!=hashlib.sha256((assets/'update-catalog.json').read_bytes()).hexdigest():raise ValueError('Latest catalog fetch mismatch; investigate before announcing rollout')
    print('Published and fetched latest signed catalog. Previous releases retained.')
if __name__=='__main__':main()
