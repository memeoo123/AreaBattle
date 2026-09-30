"""Record capture-only corrections without claiming a new player build."""
import hashlib,json,subprocess
from datetime import datetime,timezone
from pathlib import Path
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';g=t/'generated';v=g/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat();a=read(v/'capture-registration-audit.json');checks=read(w/'analysis/unity-integrated-validation.json')
assert a['numericReplayUnchanged'] and checks['passed'] and len(checks['checks'])==262
assert read(g/'inlevel-resource-audit.json')['passedIntegrity']
manifestPath=w/'analysis/VALIDATION_MANIFEST.json';m=read(manifestPath)
changed=[x['path'] for x in m['sourceFingerprints'] if hashlib.sha256((w/x['path']).read_bytes()).hexdigest()!=x['sha256']]
assert all('/Editor/' in x for x in changed),changed
m['sourceFingerprints']=[dict(path=p.relative_to(w).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in sorted((w/'UnityProject/Assets/AreaBattle').rglob('*')) if p.is_file() and p.suffix in ['.cs','.shader','.json','.prefab','.mat','.unity']]
m['atUtc']=now;m['captureRegistrationAudit']=a
m['latestValidation']={'scope':'Editor diagnostic changes only; production runtime source fingerprints unchanged.','commands':['PresentationCaptureAudit.Run','OriginalVideoReplayDiagnostic.Run','BattleBuild.ValidateMechanicsOnly','python analysis/asset_inlevel_audit.py','node --check analysis/check-visual-review.js'],'checksPassed':262,'freshPlayerBuild':False,'freshPlayerSmoke':False,'playerEvidence':'Prior camera HDR/MSAA build and600-frame smoke retained; current changes are Editor-only.','changedExistingFiles':changed}
write(manifestPath,m)
p=t/'BATTLEFIELD_RESTORE_STATE.json';b=read(p);milestone={'id':'capture-text-mesh-and-viewport-audit','status':'capture-corrected-video-viewport-inferred','atUtc':now,'evidence':['generated/video-20260928/capture-registration-audit.json','generated/video-20260928/capture-registration-comparison.png']};b['milestones']=[x for x in b.get('milestones',[]) if x.get('id')!=milestone['id']]+[milestone];write(p,b)
p=g/'RESTORE_SPEC.json';s=read(p);s['originalVideoReference']['captureRegistrationAudit']='generated/video-20260928/capture-registration-audit.json';write(p,s)
p=v/'visual-review-audit.json';s=read(p);s['captureRegistrationAudit']='capture-registration-audit.json';s['currentCaptureNote']='Static Text mesh refreshed at target resolution. Alternate723x1282 source-viewport candidate explicitly marked as inferred; canonical720x1280 images retained.';write(p,s)
note='''\n\n## 离屏文字模糊修复与背景视口分支审计（2026-09-29）

已复现截图诊断器的静态Text缓存错误：连接720×1280 RenderTexture前生成的标题/道具点字形为12/10像素，切换后CanvasScaler已经正确但旧网格仍被放大；仅再次调用CanvasScaler.Handle不能修复。截图前SetAllDirty并ForceUpdateCanvases后生成32/26像素字形，原字体、字号和生产HUD均不变。实验见generated/video-20260928/capture-audit/report.json；修复在OriginalVideoReplayDiagnostic.Capture，12检查点已重新输出，旧图保存在before-capture-refresh。

背景f7819（0x391440～0x391490）按精确0.5625分支；较宽视口采用2*2.1*.5625/2.5875000953674316倍率。已有入口截图723×1282注册来自original871-visual-comparison.json。额外生成12张entry-aspect-candidate候选，标准720×1280回放保留。35.384秒上方空背景RGB MAE10.0073→2.8781，左侧装饰39.5625→6.1231；这支持宽屏分支假设，但不证明录像内部Framebuffer尺寸，不能据此改生产相机常数。对照页新增视口选择器。

完整数值报告与full-replay-before-skin102.json逐项一致。重新运行262项集成检查全部通过，资源完整性240文件/1363 GUID引用通过，编辑器编译及回放退出0，网页JS语法通过。只有编辑器源码改变，没有重新构建Windows或重跑600帧smoke；此前二进制及smoke有效保留。没有改分、搜索随机种子或修改生产数值。

视觉验收继续未通过。冰晶边缘/粒子、原始录像视口、随机会话与未验证特殊模式/异步资源/每日池仍待查。不能把截图工具修复算作新发现的生产游戏缺陷。
'''
for p in [w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',w/'analysis/VIDEO_VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 old=p.read_text(encoding='utf8');marker='\n\n## 离屏文字模糊修复与背景视口分支审计';p.write_text(old.split(marker)[0]+note,encoding='utf8')
p=w/'AREA_BATTLE_HANDOFF.md';s=p.read_text(encoding='utf8');s+='''
## 最新补充：截图工具与背景视口（优先于上文旧截图描述）
- 已修复OriginalVideoReplayDiagnostic.Capture的静态Text网格缓存：旧12/10px字形被放大，刷新后32/26px。12张标准回放已更新，旧图在V/before-capture-refresh。
- 新增Editor/PresentationCaptureAudit.cs可复现，报告V/capture-audit/report.json。
- 背景差异主要符合源f7819精确宽高比分支：标准9:16和略宽的723×1282会有不同背景缩放。V/entry-aspect-candidate下12张候选使用既有入口截图注册尺寸，未证明原片内部视口，不能把它默认为已确认值或修改生产相机。
- V/capture-registration-audit.json、capture-registration-comparison.png记录12点ROI与边界；visual-review.html新增视口选择器。
- 新重跑262项检查、资源审计、完整回放均通过；数值报告仍与旧基线完全相同。仅编辑器源码变化，本轮没重建游戏或跑smoke；此前构建和600帧测试保留。
- 更新指纹脚本analysis/record_capture_registration.py保留已有manifest扩展并标明本轮验证范围；所有功能门重新记录，visualBaseline仍未通过。
- 下一步使用刷新后的文字和显式视口候选区分真实渲染差异；不要再把文字模糊视为生产字体错误，不要再改冰晶HDR。
''';p.write_text(s,encoding='utf8')
print('Capture audit, source fingerprints and progress recorded; no full visual acceptance claimed.')
