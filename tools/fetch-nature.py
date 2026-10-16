import re, pathlib, urllib.request, concurrent.futures, hashlib, json
base=pathlib.Path(__file__).parent
dest=base/'StylizedNature'
dest.mkdir(exist_ok=True)
def listing(html):
    decoded=re.sub(r'\\x([0-9a-fA-F]{2})',lambda m:chr(int(m[1],16)),html)
    return dict((name,ident) for ident,name in re.findall(r'\["([^"]+)",\["[^"]+"\],"([^"]+)"',decoded))
def request(url):
    with urllib.request.urlopen(url,timeout=60) as r:return r.read()
fbx=listing((base/'nature-fbx-drive.html').read_text(encoding='utf-8-sig'))
textures=listing(request('https://drive.google.com/drive/folders/19URsLKhVHqEiiTOG37NM-ypJrEb9HgOI').decode())
selected={n:i for n,i in fbx.items() if re.match(r'(NormalTree_[123]|BirchTree_[12]|MapleTree_[12]|Bush.*|Flower_[123]_Clump|Grass_Small)\.fbx$',n)}
selected.update({n:i for n,i in textures.items() if n.lower().endswith(('.png','.jpg','.txt'))})
selected['License.txt']='1W3YGLar9Ie0Lwfw7QJv1x8oBLKFPuOkW'
def download(item):
    name,ident=item;p=dest/name
    if not p.exists():
        data=request('https://drive.google.com/uc?export=download&id='+ident)
        if data.lstrip().startswith((b'<!DOCTYPE',b'<html')):raise ValueError(name+' returned HTML')
        p.write_bytes(data)
    return dict(name=name,source='https://drive.google.com/file/d/'+ident,sha256=hashlib.sha256(p.read_bytes()).hexdigest(),size=p.stat().st_size)
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    result=list(pool.map(download,selected.items()))
(dest/'provenance.json').write_text(json.dumps(result,indent=2))
print(json.dumps(result,indent=2))
