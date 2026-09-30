"""Static guide bytecode extraction; embeds evidence in guide-evidence.json only."""
import json,re,struct,sys
from pathlib import Path
sys.stdout.reconfigure(encoding='utf8')
base=Path('analysis/controls_disassemble.py').read_text(encoding='utf8')
ctx={'__file__':str(Path('analysis/controls_disassemble.py').resolve())}
exec(compile(base.split('control_type=')[0], 'controls_metadata_only', 'exec'),ctx)
ROOT,methods,ts,md,ms,memory,meta,pairs,fm,R,ops,names,typenames,methodnames=[ctx[k] for k in ('ROOT','methods','ts','md','ms','memory','meta','pairs','fm','R','ops','names','typenames','methodnames')]
classes={'GuideControl','GuideUI','GuideLogicBase','GuideLogicControl','TextGuideLogic','Guide_TowerMax','Guide_TowerAttack','Guide_TowerDefense','Guide_TowerArrow','Guide_SkillIce','Guide_SkillFire','Guide_SkillLightning'}
type_ids={t[2] for t in ts if ms(t[0]) in classes}
tokens={md[j][6] for t in ts if t[3] in type_ids for j in range(t[9],t[9]+t[16])}
selected=[m for m in methods if m['class'] in classes or m['token'] in tokens or (m['class']=='ConfigMgr' and m['method'] in ('IsGuideLv','GetGuideId','GetGuideConfig')) or (m['class']=='Event' and m['method']=='.cctor')]
for fn in (949,10103,7029,7025,14988,14987,15568,14854,9901):
    body=fm['wasmcode']['bodies'][str(fn)]
    selected.append(dict(class_='GuideCoreProbe',method='function'+str(fn),token=fn,module='wasmcode',function=fn,body=body,signature=fm['wasmcode']['signatures'][body['typeIndex']]))
for m in selected:
    if 'class_' in m:m['class']=m.pop('class_')
functions=[]
for m in selected:
    if 'body' not in m:continue
    blob=(ROOT/f'generated/wasm/{m["module"]}.wasm').read_bytes();body=m['body'];r=R(blob,body['offset']);end=body['offset']+body['size']
    locals=[]
    for _ in range(r.leb()):
        count=r.leb();typ=blob[r.p];r.p+=1;locals.append((count,typ))
    lines=[];depth=0
    while r.p<end:
        pos=r.p;op=blob[r.p];r.p+=1;assert op in ops,(m['method'],pos,hex(op));name=ops[op];args=[];comment=''
        if op in (2,3,4):args=[r.leb(True)]
        elif op in (12,13,16,32,33,34,35,36,63,64):args=[r.leb()]
        elif op==14:args=[r.leb() for _ in range(r.leb()+1)]
        elif op==17:args=[r.leb(),r.leb()]
        elif 40<=op<=62:args=[r.leb(),r.leb()]
        elif op in (65,66):args=[r.leb(True)]
        elif op in (67,68):
            fmt='<f' if op==67 else '<d';args=[struct.unpack_from(fmt,blob,r.p)[0]];r.p+=struct.calcsize(fmt)
        if op==16:comment=' ; '+', '.join(names.get((m['module'],args[0]),[]))
        if op==65 and 0<=args[0]<len(memory)-4:
            v=struct.unpack_from('<I',memory,args[0])[0];kind=v>>29;idx=(v&0x1fffffff)>>1
            if kind==1 and idx in typenames:comment=' ; type: '+typenames[idx]
            if kind==3 and idx in methodnames:comment=' ; method: '+methodnames[idx]
            if kind==5 and idx<pairs[0][1]//8:
                sl,so=struct.unpack_from('<II',meta,pairs[0][0]+idx*8);comment=' ; literal: '+repr(meta[pairs[1][0]+so:pairs[1][0]+so+sl].decode('utf8',errors='replace'))
        if op==65 and args[0]>0:
            targets=[x['class']+'.'+x['method'] for x in methods if x.get('tableIndex')==args[0]]
            if targets:comment+=' ; table: '+', '.join(targets)
        if op in (5,11):depth=max(depth-1,0)
        lines.append(f'{pos:08x} '+('  '*depth)+name+' '+str(args)+comment)
        if op in (2,3,4,5):depth+=1
    functions.append({k:m[k] for k in ('class','method','token','module','function','signature','body')}|{'locals':locals,'disassembly':lines})
path=ROOT/'generated/guide-evidence.json'
report=json.loads(path.read_text(encoding='utf8')) if path.exists() else {'target':{'appId':'wxcf1394487200e48f','version':'43'},'rules':[],'unknowns':[]}
report['functions']=functions
path.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'functionCount':len(functions),'totalBytecode':sum(f['body']['size'] for f in functions)},ensure_ascii=False))

