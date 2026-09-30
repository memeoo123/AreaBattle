from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-commander-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==311
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-dynamic-content-binding';s['validation'].update(integratedChecksPassed=311,commanderViewChecksPassed=3);s['nextActions']=['Complete commander skills/3D/acquisition descriptions and bind page to account startup.','Implement real effects consumers and lifecycle saves.','Run actual rendered UI capture and new lobby build after entry wiring.'];write(p,s)
p=o/'golden-cases.json';s=read(p);byid={c['id']:c for c in v['checks']}
for c in s['cases']:
 if c['id'] in byid:c.update(byid[c['id']])
s['cases'] += [dict(c,sourceContract='CommanderItem and CommanderUI source methods plus original prefab bindings') for c in v['checks'] if c['id'] not in {x['id'] for x in s['cases']}];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Actual offscreen UI rendering and source skill detail enter/leave callback','checksPassed':311,'outgameChecksPassed':44,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Skill/details presentation, real platform/stats consumers, account entry or full lobby'};write(p,s)
note='''

## 关外实际渲染检查与详情面板生命周期（311项）
新增OutgamePreviewCapture.Run编辑器独立渲染fixture，创建540x960屏幕空间Camera+原始Prefab主界面和指挥官页，输出analysis/captures/outgame-main-fixture.png、outgame-commander-fixture.png。不加载/创建用户存档；level6/10250金币仅预览夹具，不是新账号数据。已实际打开检查两图：主界面仍缺动态Logo（白块）、场景背景，Level100是原编辑文案尚未刷新；指挥官缺模型和技能内容，顶部/底部图标及动态可见性还未全接，不能称完整可玩大厅。
渲染揭示技能详情原Prefab默认打开显示New Text，源码CommanderUI.Awake f201?在008a60c7调用h^ZMRew f6196关闭field276=objSkillDetail，VisibleImp隐藏分支008a544c..008a5456同样关闭。已在Bind、OnDisable和成功升级关闭详情，并重新渲染确认占位详情不再覆盖页面。
新增一项初始化/显式disable回调检查：编辑器非PlayMode SetActive不自动运行普通MonoBehaviour的OnDisable，因此测试通过反射显式调用，不能当作真实玩家生命周期烟测。311集成通过、Unity退出0，截图反映当前仍不完整。下一步优先技能/模型与真实启动动态UI，随后实际Player生命周期验收。完整目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','311 integrated checks pass including real dynamic commander card and upgrade button/disk integration; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded311checks; dynamic commander cards and operations connected; full objective incomplete.')

