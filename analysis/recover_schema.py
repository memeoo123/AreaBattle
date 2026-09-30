"""Recover factual JSON table schemas and IL2CPP v31 symbol records, not method bodies."""
import collections
import hashlib
import json
import re
import struct
from pathlib import Path

ROOT = Path(__file__).parent / 'targets/wxcf1394487200e48f/43'
def save(rel, value):
    p = ROOT / rel
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(json.dumps(value, ensure_ascii=False, indent=2), encoding='utf-8')

inv = json.loads((ROOT/'evidence/asset-inventory.json').read_text(encoding='utf-8'))
tables = []
for bundle in inv['bundles']:
    for obj in bundle.get('objects',[]):
        if obj['type'] != 'TextAsset' or not obj.get('output') or not obj.get('name','').endswith('Config'):
            continue
        try:
            data = json.loads((ROOT/obj['output']).read_text(encoding='utf-8-sig'))
        except (ValueError, UnicodeError):
            continue
        if not isinstance(data,dict):
            continue
        row_key = 'Datas' if 'Datas' in data else ('AudioInfos' if 'AudioInfos' in data else None)
        rows = data[row_key] if row_key else [data]
        if not isinstance(rows,list):
            continue
        fields = collections.defaultdict(set)
        for row in rows:
            for key,value in row.items(): fields[key].add(type(value).__name__)
        out = f'generated/tables/{obj["name"]}.json'
        save(out,data)
        ids = [r['id'] for r in rows if 'id' in r]
        tables.append({'name':obj['name'],'status':'confirmed','shape':'record-list' if row_key else 'singleton-object','rowKey':row_key,'rowCount':len(rows),'fields':{k:sorted(v) for k,v in fields.items()},'duplicateIds':[k for k,v in collections.Counter(ids).items() if v>1],'output':out,'source':bundle['source'],'assetFile':obj['assetFile'],'pathId':obj['pathId'],'textAsset':obj['output'],'sha256':obj['sha256'],'samples':rows[:2]})
save('generated/table-schemas.json',tables)

p=ROOT/'work/webdata/Il2CppData/Metadata/global-metadata.dat'
b=p.read_bytes()
magic,version=struct.unpack_from('<II',b)
assert magic==0xfab11baf and version==31,(magic,version)
pairs=[struct.unpack_from('<II',b,8+8*i) for i in range(21)]
for off,size in pairs: assert off+size<=len(b)
strings,strings_size=pairs[2]
def s(index):
    assert 0<=index<strings_size,index
    end=b.index(0,strings+index,strings+strings_size)
    return b[strings+index:end].decode('utf-8')
def records(pair,fmt):
    off,size=pairs[pair]
    stride=struct.calcsize(fmt)
    assert size%stride==0,(pair,size,stride)
    return [(i,off+i*stride,struct.unpack_from(fmt,b,off+i*stride)) for i in range(size//stride)]
types=records(19,'<16i8H2I')
methods=records(5,'<7i4H')
fields=records(11,'<3i')
params=records(10,'<3i')
images=records(20,'<10i')
type_images={}
for _,offset,t in images:
    for tid in range(t[2],t[2]+t[3]): type_images[tid]=s(t[0])
selected=[]
for index,offset,t in types:
    name,namespace=s(t[0]),s(t[1])
    if not namespace.startswith('Proj_hdzd'):
        continue
    members=[]
    for fi in range(t[8],t[8]+t[18]):
        _,foff,f=fields[fi]
        members.append({'name':s(f[0]),'typeIndex':f[1],'offset':foff,'token':f[2]})
    funcs=[]
    for mi in range(t[9],t[9]+t[16]):
        _,moff,m=methods[mi]
        assert m[1]==index,(name,mi,m[1],index)
        arguments=[]
        for pi in range(m[4],m[4]+m[10]):
            _,poff,param=params[pi]
            arguments.append({'name':s(param[0]),'typeIndex':param[2],'offset':poff})
        funcs.append({'name':s(m[0]),'index':mi,'offset':moff,'token':m[6],'returnTypeIndex':m[2],'parameters':arguments})
    selected.append({'name':name,'namespace':namespace,'assembly':type_images.get(index),'typeIndex':index,'offset':offset,'fields':members,'methods':funcs})
save('generated/gameplay-symbols.json',{'source':p.relative_to(ROOT).as_posix(),'sha256':hashlib.sha256(b).hexdigest(),'version':version,'structReference':'Il2CppDumper/Il2Cpp/MetadataClass.cs; reviewed local v31 layouts; bounds and declaring-type cross-checks passed','counts':{'types':len(types),'methods':len(methods),'fields':len(fields),'gameplayTypes':len(selected)},'methodBodiesRecovered':False,'types':selected})

catalog=ROOT/'work/cache/StreamingAssets/WebGL/Proj_hdzd/ABFiles_c7a33c47abddca27a160257cfbbf133e.txt'
entries=[]
for lineno,line in enumerate(catalog.read_text(encoding='utf-8-sig').splitlines()[1:],2):
    parts=line.split('|')
    if len(parts)!=4: continue
    name,file,md5,size=parts
    local=catalog.parent/file
    entries.append({'name':name,'file':file,'md5':md5,'size':int(size),'line':lineno,'cached':local.exists()})
levels=[x for x in entries if x['name'].startswith('data/levelcfg/')]
save('generated/resource-catalog.json',{'source':catalog.relative_to(ROOT).as_posix(),'entryCount':len(entries),'cachedCount':sum(x['cached'] for x in entries),'levelFileCount':len(levels),'cachedLevelFileCount':sum(x['cached'] for x in levels),'entries':entries})
keytables=['LevelConfig','LevelBConfig','SoldierConfig','AIConfig','CampConfig','GlobalValueConfig','EntityModelConfig']
print(json.dumps({'tables':len(tables),'keyTables':[{k:t[k] for k in ['name','rowCount','duplicateIds']} for t in tables if t['name'] in keytables],'metadataGameplayTypes':len(selected),'interestingClasses':[{'name':t['name'],'fields':len(t['fields']),'methods':len(t['methods'])} for t in selected if re.search('Tower|Bastion|Soldier|Level|Battle|SceneData|GamePlay',t['name'])][:60],'resourceCount':len(entries),'levelFiles':len(levels),'cachedLevelFiles':sum(x['cached'] for x in levels)},ensure_ascii=False,indent=2))
