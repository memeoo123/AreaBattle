from pathlib import Path
import json,subprocess
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-menu-view-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==308
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='commander-page-content-and-profile-binding';s['validation'].update(integratedChecksPassed=308,menuViewChecksPassed=2);s['nextActions']=['Bind original CommanderUI dynamic list/detail/buttons to OutgameCommanderActions and durable profile.','Add real startup/lifecycle account provider and concrete stats/tool consumers.','Verify source tween default easing; current view curve is explicitly provisional linear.','Create runnable lobby scene only with actual startup data semantics; do not use fixture level as fresh-account truth.'];write(p,s)
p=o/'golden-cases.json';s=read(p);s['cases']=[c for c in s['cases'] if c['id'] not in {c['id'] for c in v['checks']}]+[dict(c,sourceContract='UIControl.OpenMenuItemUI, MenuTabUI.Awake/OpenLater plus ui-import bindings') for c in v['checks']];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Original prefab buttons and runtime page switching','checksPassed':308,'outgameChecksPassed':41,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Source tween easing, dynamic commander/shop content, account startup or full lobby'};write(p,s)
note='''\n\n## 原始菜单视图接线（308项）
OutgameMenuView.cs实例化四原始Prefab，绑定objSkin/Main/OtherUnSelected原按钮，主界面首次显示，incoming挂到outgoing下做页面过渡，0.3秒后恢复父级/关闭outgoing/解锁切页；选中和未选中底栏状态同步。imgCommanderLock按钮发送锁定通知，显式level<CommanderConfig[1].unlockLevel拦截指挥官入口；测试用level5/6，不当作新账号默认。MenuTabUI.Awake源码00801093..008010aa隐藏field148=ShopUnSelected和184=ItemUnSelected；运行视图保持相应无效页隐藏。
过渡使用可配置AnimationCurve，默认线性明确为临时呈现，原DOTween默认easing尚未验证，不宣称曲线一致。动态页面控制器从PageShown事件接入，当前尚未绑定指挥官列表、皮肤内容或账号。两项真实Prefab Button.onClick检查验证往返、父级/active状态、完成解锁、锁定覆盖、快速点击保护；308集成通过、编译退出0。尚无独立大厅场景/新Windows构建或端到端UI点击烟测。
下一步CommanderUI原列表/详情接OutgameCommanderActions和持久化账号，再启动入口/生命周期/消费者。关外完整目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','308 integrated checks pass including2 original-prefab button/page-switch checks; dynamic pages/startup/full goal remain incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded308 checks; runtime menu view added, full objective incomplete.')
