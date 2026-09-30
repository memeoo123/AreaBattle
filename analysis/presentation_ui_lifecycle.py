"""Resolve source UI framework lifecycle helpers without running target code."""
import json,struct
from pathlib import Path
s=Path('analysis/controls_disassemble.py').read_text(encoding='utf8').split('control_type=')[0]
ctx={'__file__':str(Path('analysis/controls_disassemble.py').resolve())};exec(compile(s,'metadata_only','exec'),ctx)
memory,meta,pairs,md,ts,ms,table=[ctx[k] for k in ('memory','meta','pairs','md','ts','ms','table')]
images=[struct.unpack_from('<10i',meta,pairs[20][0]+i*40) for i in range(pairs[20][1]//40)]
resolved=[]
for im in images:
 name=ms(im[0]);start=memory.find(name.encode()+b'\0')
 if start<0:continue
 at=0
 while (at:=memory.find(struct.pack('<I',start),at))>=0:
  if at%4==0 and at+12<len(memory):
   _,count,ptr=struct.unpack_from('<III',memory,at)
   if 1<count<200000 and ptr>0 and ptr+count*4<len(memory):
    values=struct.unpack_from('<'+str(count)+'I',memory,ptr)
    if max(values)<1000000:
     for m in md:
      if not im[2]<=m[1]<im[2]+im[3]:continue
      rid=m[6]&0xffffff
      if not 1<=rid<=count:continue
      tid=values[rid-1];loc=table.get(tid)
      if not loc:continue
      cls=ms(ts[m[1]][0]);method=ms(m[0])
      if tid in (1318,2825) or loc==('wasmcode',6251) or cls in ('UIModule','UIBase','BaseUI','UI','UIView','UIWindow','UIGroup','UIItem','UIObject','UINode') or ('UI' in cls and method in ('Close','CloseSelf','Open','Awake','Dispose','Destroy')):
       body=ctx['fm'][loc[0]]['bodies'].get(str(loc[1]))
       if body:resolved.append({'assembly':name,'class':cls,'method':method,'token':m[6],'module':loc[0],'function':loc[1],'tableIndex':tid,'body':body,'signature':ctx['fm'][loc[0]]['signatures'][body['typeIndex']]})
  at+=1
(ctx['ROOT']/'generated/ui-lifecycle-method-map.json').write_text(json.dumps(resolved,ensure_ascii=False,indent=2),encoding='utf8')
print('\n'.join(f"{r['assembly']} {r['class']}.{r['method']} {r['module']} f{r['function']} table{r['tableIndex']}" for r in resolved if r['tableIndex'] in (1318,2825)))

source=Path('analysis/guide_disassemble.py').read_text(encoding='utf8')
a=source.index('classes=');b=source.index('functions=[]')
source=source[:a]+"selected=json.loads((ROOT/'generated/ui-lifecycle-method-map.json').read_text(encoding='utf8')); selected=[m for m in selected if m['assembly']=='GFRunning.dll']\nfor m in selected:names.setdefault((m['module'],m['function']),[]).append(m['class']+'.'+m['method'])\n"+source[b:]
source=source.replace('generated/guide-evidence.json','generated/ui-lifecycle-evidence.json')
exec(compile(source,'ui_lifecycle_disassembly','exec'),{'__file__':str(Path('analysis/guide_disassemble.py').resolve())})
