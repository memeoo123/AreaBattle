"""Focused static helpers for skill UI parent and ice virtual dispatch evidence."""
import json,runpy,contextlib,io
from pathlib import Path
class Sink(io.StringIO):
 def reconfigure(self,**kwargs):pass
with contextlib.redirect_stdout(Sink()): env=runpy.run_path(str(Path(__file__).with_name('combat_presentation_disassemble.py')))
ROOT=env['ROOT'];OUT=ROOT/'generated'
src=Path(__file__).with_name('combat_disassemble.py').read_text(encoding='utf8')
a=src.index('selected=');z=src.index('\nout=',a)
rows=[]
for name in ('wasmcode','wasmcode1'):
 mp=json.loads((ROOT/f'evidence/{name}-function-map.json').read_text())
 for f in ([14833,3877,2391,11497,5863,11494] if name=='wasmcode' else []):
  body=mp['bodies'][str(f)];rows.append(dict(class_='ParentRaw',method=str(f),token=f,module=name,function=f,body=body,signature=mp['signatures'][body['typeIndex']]))
src=src[:a]+"selected="+repr(rows)+"\nfor m in selected:m['class']=m.pop('class_')"+src[z:]
src=src.replace("out=ROOT/'generated/combat-disassembly'","out=ROOT/'generated/combat-parent-disassembly'")
with contextlib.redirect_stdout(Sink()):exec(compile(src,'combat_visual_parent_audit','exec'))
for p in (OUT/'combat-parent-disassembly').glob('*.txt'):
 import re
 s=p.read_text(encoding='utf8');s=re.sub(r'call \[(\d+)\] ; [^\n]*',lambda m:m[0].split(' ;')[0]+' ; '+(env['byfn'][('wasmcode',int(m[1]))]['cls']+'.'+env['byfn'][('wasmcode',int(m[1]))]['method'] if ('wasmcode',int(m[1])) in env['byfn'] else ''),s)
 p.write_text(s,encoding='utf8')
print('focused functions',len(rows))
report={'target':{'appId':'wxcf1394487200e48f','version':'43'},'status':'static source confirmed',
 'getSkillParent':{'function':'wasmcode f7147','offset':'002979e6..002979f5','expression':'SkillControl.items[SkillBase.slotIndex].transform','fields':{'SkillControl.items':92,'SkillBase.slotIndex':8,'UIObject.transform':16},'campBranch':False,'transformInitialization':'UIObject.initGameObject f4713 @0018dc28..0018dc38 stores gameObject.transform at +16'},
 'parents':[
  {'skill':2,'parent':'UI SkillItem index1','source':['Type4169-100665118 @0070767c..00707690','Type4167-100665113 @006b4b67'], 'localScale':[1,1,1]},
  {'skill':7,'parent':'NormalPool parent retained; skill never reparents','source':['Type4179-100665140 NormalPool.Spawn','Type4179-100665142 @003f76aa..003f76c0 only reads UI item0 origin'], 'correction':'Previous contract incorrectly called this GetSkillParent. Do not apply UI Canvas scale.'},
  {'skill':9,'parent':'GetSkillParent (slot2)','source':['Type4182-100665151 @007a85ea..007a85f7'],'localScale':[1,1,1]},
  {'skill':10,'parent':'GetSkillParent (slot0)','source':['Type4186-100665174 @006b1d2b','Type4184-100665169 @00816d2d,00816df2'],'localScale':[4,4,4]},
  {'skill':14,'parent':'GetEntityNow loader parent retained; no skill reparent','source':['Type4194-100665209 @00708181,007081f7,00708242'],'localScale':[0.05,0.05,0.05]},
  {'skill':18,'parent':'UI SkillItem index2 for both camps','source':['Type4205-100665242 @006dd3e7..006dd3f0','Type4203-100665248 @00645f63,00645fe8'],'localScale':[150,150,150]}
 ],
 'rendering':{'rule':'For UI-parented objects preserve localScale under the actual SkillItem transform and world-space position/rotation assignments. Do not treat source localScale as world scale.','layer':'No explicit layer mutation in the six skill Execute/spawn callbacks inspected; parenting does not itself change layers. Lower loader-wide layer policy is not claimed by this audit.','canvasScale':'Asset pipeline independently auditing original UI camera orthographicSize/referenceHeight; derive from actual Canvas, do not hardcode an inferred factor.'},
 'ice':{'execute':'Skill1 Execute @0064acfa passes virtual slot26 through table10363/f14831 => Tower.PlayIceEffect. It activates child and sets Animator skill=0; float6/cdTime unused.',
 'end':'Skill1 End @0064aa2f passes virtual slot8 through table1182/f14833 => Tower.PlayIceEffect2. Only active enemy towers still state1 get state0 and skill=1.',
 'boss':'Boss.PlayIceEffect f68531 calls table15535 => base Tower.PlayIceEffect. No suppression.',
 'stop':'Tower.Clear f10055@00499ca7 => helper f6992@00289e44..00289e61 calls virtual slot14 => StopIceEffect f10050 => ice GameObject.SetActive(false). No call observed in Tower.ChangeCamp.',
 'assetAudit':'asset_pipeline confirms disappear/grow clips have no AnimationEvents; melt naturally fades particles/material alpha, not a hidden Stop callback.',
 'productionEvents':['tower-ice-begin','tower-ice-melt']},
 'unknowns':['NormalPool and entity-loader full parent-chain identity beyond inspected callsites remains separate; neither7 nor14 may be assigned UI parent by inference.','Global managed-effect retry lifecycle remains unproven as documented in combat-skill-async-visual-audit.json.']}
(OUT/'combat-skill-parent-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
