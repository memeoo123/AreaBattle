"""Resolve original pre-game registration order from executable calls, not warmup constants."""
from pathlib import Path
import json,struct,re
p=Path('analysis/recover_outgame_manager_registry.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('rows=[]')[0],n)
u,mem,md,ts,ms=[n[k] for k in ['u','mem','md','ts','ms']];out=n['p']/'generated/outgame';lines=(out/'disassembly/Type3974-30608.txt').read_text(encoding='utf8').splitlines();rows=[]
for i,line in enumerate(lines):
 if 'call [935]' not in line:continue
 addr=int(re.search(r'i32.const \[(\d+)\]',lines[i-2])[1]);v=u(addr);assert v>>29==6;idx=(v&0x1ffffffe)>>1;spec=struct.unpack_from('<3i',mem,483008+12*idx);method=md[spec[0]];assert ms(method[0])=='get_I'
 ptr=u(72592+4*spec[1]);count,args=struct.unpack_from('<II',mem,ptr);assert count==1;tid=u(u(args));assert 'call [1622]' in lines[i+3]
 rows.append(dict(order=len(rows),typeIndex=tid,name=ms(ts[tid][0]),usageAddress=addr,methodSpec=list(spec),registerExtraArgument=0))
result=dict(sourceMethod=30608,controllers=rows,procedures=['ProcedurePreLoad','ProcedureStarGame','ProcedureExitGame'],firstProcedure='ProcedurePreLoad',procedureRegistrationMethod=30622,procedureStartUsage=4009776,procedureStartMethod=28664,qualification='Registration inventory only; does not claim concrete controller instances or all lifecycle bodies restored')
(out/'PRE_GAME_REGISTRATION_ROSTER.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf8');print(json.dumps(result,ensure_ascii=False))
