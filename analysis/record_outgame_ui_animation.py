"""Record source UI animation restoration without upgrading historical Player evidence."""
import json,hashlib
from pathlib import Path
from datetime import datetime,timezone
P=Path(__file__).resolve().parent.parent
T=P/'analysis/targets/wxcf1394487200e48f/43';O=T/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(P/'analysis/unity-integrated-validation.json');assert r['passed'] and len(r['checks'])==594
m=read(P/'analysis/VALIDATION_MANIFEST.json');m.setdefault('validationHistory',[]).append(m['latestValidation']);m['atUtc']=datetime.now(timezone.utc).isoformat();m['caseCount']=594
m['latestValidation']={'scope':'Source UI animation targets, duration, scaled update, fade and Back easing','checksPassed':594,'outgameChecksPassed':327,'freshPlayerBuild':False,'freshPlayerSmoke':False,'freshPlayModeRun':False,'priorAdsPlayModeChecks':7,'priorFlyPlayModeChecks':8,'notClaimed':'Full DOTween scheduler parity, real frame/coroutine timing, production close host or complete playable outgame'}
stamp=(P/'analysis/unity-integrated-validation.json').stat().st_mtime
m['sourceFingerprints']=[]
for p in sorted((P/'UnityProject/Assets/AreaBattle').rglob('*')):
 if p.is_file() and p.suffix in ['.cs','.shader','.json','.prefab','.mat','.unity']:
  if p.suffix in ['.cs','.shader']:assert p.stat().st_mtime<=stamp,str(p)
  m['sourceFingerprints'].append({'path':p.relative_to(P).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
write(P/'analysis/VALIDATION_MANIFEST.json',m)
s=read(T/'OUTGAME_RESTORE_STATE.json');s['lastUpdatedAtUtc']=m['atUtc'];s['validation']['integratedChecksPassed']=594;s['validation']['loginSyncChecksPassed']=len(read(P/'analysis/outgame-login-sync-validation.json')['checks']);write(T/'OUTGAME_RESTORE_STATE.json',s)
write(O/'UI_ANIMATION_AUDIT.json',{'status':'source_animation_routes_with_scoped_native_tween_adapter','source':['disassembly/Type3557-27424.txt','disassembly/Type3558-27407.txt','disassembly/Type3562-27454.txt','disassembly/Type9871-68377.txt','disassembly/Type9974-68862.txt','ui-animation-generics.json'],'verified':['Default durations .5/.3/.2 and positive explicit override','Root CanvasGroup preferred; constant alpha getters0/1; child Graphic path excludes inactive descendants','Graphic.From writes zero immediately and restores captured alpha; reverse traversal','Scale.From zero to captured nonuniform vector; scale-out lazy capture','OutQuad and Back26/27 source expressions; default overshoot bits1071238496','Scaled update pause, midpoints and endpoints; original single WaitForSeconds enumerator'],'remaining':['Actual Play coroutine scheduling and two end-of-frame close sequence in PlayMode','Native adapter does not cover entire DOTween engine or competing tweens','Production page creation/close host and full Player flow'],'integratedChecks':594})
a=read(O/'UI_ANIMATION_AUDIT.json');a['source'] += [row['path'] for row in read(O/'method-map.json') if row['path'].startswith(('disassembly/Type3560-','disassembly/Type3561-'))]
assert all((O/x).exists() for x in a['source'])
write(O/'UI_ANIMATION_AUDIT.json',a)
note='\n\n## 原版页面动画运行组件（594项）\nOutgameUiAnimation恢复27424/27454及默认时长：CanvasGroup优先且getter固定0/1，缺失时倒序Graphic淡入淡出；缩放3/4保留原始各轴，分别OutBack/InBack。泛型和原始DOTween默认overshoot位值核实，组件接入Unity缩放时间Update与独立WaitForSeconds协程；直接驱动验证暂停、初值、默认时长、半程回弹和终点。594项通过，修正测试Image命名空间后重跑退出0。当前是有限原生Tween适配器，真实协程/帧序、完整关闭宿主与生产入口尚未验收，无新PlayMode/Player；索引3286。\n'
for p in [P/'AREA_BATTLE_HANDOFF.md',P/'RESTORE_PROGRESS.md',P/'analysis/VALIDATION_REPORT.md',T/'REVERSE_PROGRESS.md']:
 with p.open('a',encoding='utf8') as f:f.write(note)
print('Verified',len(r['checks']),'checks;',len(m['sourceFingerprints']),'fingerprints; full goal incomplete.')
