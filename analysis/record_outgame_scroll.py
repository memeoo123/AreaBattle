from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-ui-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==308
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-dynamic-content-binding';s['validation']['sourceUiScrollMaskFitComponents']=27;s['nextActions']=['Bind CommanderUI original CommanderItem template to dynamic profile list and selection/upgrade; scroll/mask/fit dependencies now restored.','Use original CommanderItem methods newly extracted (method-map337) to establish portraits/red-dot/level formatting.','Continue actual startup/lifecycle and effects consumers; no complete lobby/build yet.'];write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Restore and verify source ScrollRect/Mask/ContentSizeFitter for outgame UI:11 commander+16shop','checksPassed':308,'outgameChecksPassed':41,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Dynamic commander binding, full rendering parity or complete lobby'};write(p,s)
note='''\n\n## 原始列表滚动/遮罩/自适应修复（308项，扩大检查覆盖）
继续动态列表前发现RecoveredHudImporter跳过ScrollRect/Mask/ContentSizeFitter，会导致原始CommanderUI和ShopUI不能滚动/裁剪/扩展。已按原始序列化字段实现三类组件，ScrollRect第二遍解析原始content/viewport PPtr绑定，复制horizontal/vertical/movement/inertia/elasticity/deceleration/sensitivity及scrollbar设置；有非空scrollbar引用且未实现时明确报错，不默默丢弃。当前4Prefab全部重新导入退出0。
扩展OutgameUiImportValidation既有4项检查，校验原始引用及控制参数：CommanderUI11组件，ShopUI16组件，共27；仍308条但覆盖更广，不虚增计数。综合编译/回归通过。CommanderItem新提取14方法，总method-map337，定位了原始CommanderItem模板、CommanderContent和技能模板，待实际绑定动态数据。没有新玩家构建、没有完成的大厅。
下一步必须接原始CommanderItem模板数据与选择/升级交互，不再受缺少滚动组件阻碍。原始节点路径和bindings在outgame/ui-import.json；完整目标仍active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','308 integrated checks pass with expanded27 original scroll/mask/fitter component checks; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded308checks with expanded original UI behavior coverage.')
