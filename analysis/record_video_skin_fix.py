"""Register the source-preserving soldier102 cosmetic option and unchanged replay."""
import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
before=read(V/'full-replay-before-skin102.json');after=read(V/'full-replay-diagnostic.json');assert before==after,'Cosmetic change must preserve the complete diagnostic state/input trace'
checks=read(W/'analysis/unity-integrated-validation.json');assert checks['passed'] and any(c['id']=='video871-soldier102-cosmetic-binding-preserves-mechanics' and c['result']=='pass' for c in checks['checks'])
e=read(G/'video-soldier-skin-evidence.json');e.update(atUtc=datetime.now(timezone.utc).isoformat(),implementation='BattleView.OrdinarySoldierSkinId=102 or -battle-skin102 (separate CLI arguments); only ordinary type1 visuals change for blue/red camp mask6; green retains default100.',validation=dict(integratedCases=len(checks['checks']),mechanicsTraceIdentical=True,replayVictoryPts=after['victoryPts'],render='generated/video-20260928/full-replay-first-checkpoint.png',visualReview='Original pointed asymmetric hat and star staff appear in the reconstructed shot. No claim of whole-frame or pose equivalence.'))
write(G/'video-soldier-skin-evidence.json',e)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['soldierSkin']=dict(id=102,status='source-asset-confirmed-video-match-inferred',evidence='generated/video-soldier-skin-evidence.json',accountRead=False);write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='video-soldier102-cosmetic-binding',atUtc=e['atUtc'],status='implemented-and-validated',evidence=['generated/video-soldier-skin-evidence.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['videoSoldierSkin']=e['validation'];write(p,v)
marker='\n\n## 录像普通兵皮肤修正'
note=marker+f'''\n\n已从缓存原件导入 `soldier_102`：SkinConfig102→prefab1002，skin04 图集含尖帽和星形法杖，94 顶点原始网格，原始 RGBA32 GPU 动画（run 从266帧起、37帧、0.6秒）。原始包及导出数据保持不变。\n\n新增显式外观选项 `-battle-skin 102`；`Play-Level871.cmd` 用此选项打开第871关。只改变蓝、红两方普通 type1 士兵的显示；绿色方保留原片可见的圆头 soldier100，其他兵种与正常存档不变。录像诊断使用此选项；用户/敌方账号皮肤选择仍是画面推断。\n\n更换前后完整回放 JSON 完全一致，新增生产场景测试验证实际生成士兵使用 soldier102 网格、攻击/占领/增援值不变且重试保留选项。当前 {len(checks['checks'])} 项集成检查通过。后续仍需解决初始士兵、占领时机及画面时序差异，不能将外观修正视为整局验收。\n'''
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print('Registered soldier102 cosmetic fix;',len(checks['checks']),'checks; complete numeric trace unchanged.')
