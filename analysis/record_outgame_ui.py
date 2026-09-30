from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path.cwd();t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-ui-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==306
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='recovered-ui-runtime-binding';s['validation'].update(integratedChecksPassed=306,uiHierarchyChecksPassed=4);s['nextActions']=['Wire original MenuTabUI/Proj_xqzdStartUI/CommanderUI/ShopUI prefabs to production navigation/profile/actions; avoid invented layout.','Map original button outlets to runtime fields; preserve duplicate sibling names with explicit binding identity.','Finish stats/tool effects consumers and lifecycle saving, then actual UI runtime capture/build.'];s['artifacts'].update(uiEvidence='generated/outgame/ui-evidence.json',uiImport='generated/outgame/ui-import.json');write(p,s)
p=o/'golden-cases.json';s=read(p);s['cases']=[c for c in s['cases'] if c['id'] not in {c['id'] for c in v['checks']}]+[dict(c,sourceContract='ui-import.json original node hierarchy') for c in v['checks']];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Four original outgame UI prefabs imported,426 nodes101sprites; original topology/anchors/pivots/active states validated','checksPassed':306,'outgameChecksPassed':39,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Runtime page binding, visual parity, effects/controllers or full outgame'};write(p,s)
note='''\n\n## 原始关外UI Prefab导入（306项）
analysis/outgame_ui_evidence.py从原始asset-evidence提取MenuTabUI68节点/17绑定，Proj_xqzdStartUI78/33，CommanderUI115/43，ShopUI165/59；共426节点。outgame_ui_prepare.py生成outgame/ui-import.json：101sprites、1font、2explicit material unknown。原始图像和字体哈希校验后由共用RecoveredHudImporter.ImportOutgame导入Resources/Recovered/Outgame四个Prefab。旧战斗HUD仍用Import，参数化输入和目标，未替换旧prefab。
修复导入器Text空字体引用：原MenuTabUI btnMask两个Text确为无font，保留null，不猜字体。原始同名兄弟节点保持同名，校验按节点顺序/父映射解析，不强迫改名为内部唯一path。OutgameUiImportValidation核对426节点名称/层级顺序、锚点/pivot及active状态；4项+原302=306通过，编译退出0。仅静态导入；未做屏幕视觉对照、动态列表、事件接线或新玩家构建，仍非可玩大厅。
关键下一步：复用这些原始Prefab接OutgameMenuNavigation和CommanderActions，避免继续只写模型。按钮原始outlet路径在ui-import.json bindings。完整统计/奖励/商店/活动/平台/lifecycle仍待完成，完整目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','306 integrated checks pass;4 original UI prefab topology checks; page runtime/visual parity incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded306checks; original outgame prefabs imported; full objective incomplete.')
