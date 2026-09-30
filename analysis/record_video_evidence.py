"""Register the supplied original recording and bounded checkpoint validation."""
import hashlib,json
from datetime import datetime,timezone
from pathlib import Path

W=Path(__file__).resolve().parent.parent
T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
stamp=datetime.now(timezone.utc).isoformat()
checks=read(W/'analysis/unity-integrated-validation.json');build=read(W/'analysis/unity-build-report.json');smoke=read(W/'analysis/player-smoke.json')
video_checks=[c for c in checks['checks'] if c['id'].startswith('video871-')]
assert checks['passed'] and len(video_checks)>=4 and all(c['result']=='pass' for c in video_checks)
assert build['result']=='Succeeded' and smoke['passed']
assert (W/'analysis/player-smoke.json').stat().st_mtime>=(W/'Build/Windows/AreaBattle.exe').stat().st_mtime
video=T/'evidence/original-reference/video-20260928/original.mp4'
digest=hashlib.sha256(video.read_bytes()).hexdigest()
assert digest=='439e6d33828fb2b98be78f2085ce28dbbbcc06f4706be8e9f0ac5ba2a5493bd0'
pts=read(V/'frame-pts.json')
source=dict(path=str(video),sha256=digest,bytes=video.stat().st_size,durationSeconds=90.52,
 dimensions=[720,1334],decodedFrames=len(pts),gameViewport=dict(x=0,y=54,width=720,height=1280,status='inferred from video titlebar and portrait viewport'),
 suppliedPath='C:/Users/jiachengwei/Desktop/AreaBattle/QQ20260928-163219-HD.mp4',
 initialState='Already paused, with blue tower scores19/19; not a fresh entry recording.',
 userSkillLevelAnswer='Unknown; no original account profile was accessed.')
timeline=[
 dict(id='resume',ptsApprox=.806,status='visual-and-audio-supported',detail='Initial paused UI closes; source button sound2001 has correlation0.50613. Not exact engine input time.'),
 dict(id='ice',frame=2160,pts=pts[2160],stockBefore=27,stockAfter=26,detail='Enemy towers3/4/6/7 frozen; blue towers1/2/5 unaffected.'),
 dict(id='ice-end',ptsInterval=[50.05872,50.09187],status='visual-interval-and-audio-supported',detail='Ice skill active blue button outline disappears across these frames; ice geometry continues melting. Sound2018 candidate at50.086, correlation0.0497.'),
 dict(id='fire',frame=3137,pts=pts[3137],stockBefore=26,stockAfter=25,detail='Fireballs launch from the fire button; both friendly healing and enemy hits are visible. Level unresolved.'),
 dict(id='lightning',beforeFrame=3731,frame=3732,pts=pts[3732],stockBefore=25,stockAfter=24,tower=7,scoreBefore=45,scoreAfter=0,campBefore='green',campAfter='green',detail='Tower model downgrades; no immediate capture. Strong level10 candidate for an isolated cast; coincident other damage is not independently excluded.'),
 dict(id='result-transition',frame=5156,pts=pts[5156],detail='Commander hidden while skill icons remain. Play HUD closes and skill graphics fade afterward; this is transition onset, not exact win simulation time.'),
 dict(id='result-fading',frame=5162,pts=pts[5162],detail='Tower labels gone; skill panel partially transparent; result UI entering.'),
 dict(id='result-banner',frame=5200,pts=pts[5200],detail='Victory artwork/ranking/reward UI visible; banner text still blank during source clip entry. Commercial rewards remain excluded from reconstruction scope.')]
inferences=[
 dict(skill=1,level=10,status='strong-inference',basis='Visible active-state end interval implies13.97350..14.00665 seconds from cast. Config110=14s; all lower levels<=13s. Audio2016/2018 candidates are14.000s apart; low melt correlation alone would be insufficient.'),
 dict(skill=2,level=None,status='unknown',basis='Source data1 is projectile count10..25, not damage. All levels last5s and hit for2 damage or2 healing. Mixed-audio launch correlations contain aliases, so no reliable projectile-count or level inference.'),
 dict(skill=3,level=10,status='strong-conditional-inference',basis='Adjacent original frames45to0 without capture agree with config310 damage45. Lower levels max43; simultaneous other hits cannot be excluded solely by the score transition.')]
correction=dict(id='skill-ui-close-fade',status='source-confirmed-and-regression-tested',
 observed='Original commander disappears before its skill panel.',
 before='Local adapter instantly hid commander, skill graphics and other battle HUD together.',
 after='Hide commander immediately; fade skill graphics over0.3s with OutQuad; disable input while closing; restore alpha/input and fresh commander on retry.',
 source=['ui-close-source/15926.txt:SkillUI.CloseBefore hides command object',
 'ui-close-source/15920.txt:SkillUI constructor writes OpenAnim1/CloseAnim2',
 'ui-close-source/15244.txt:UIModule.UIAnim type2 selects default static offset4',
 'ui-close-source/15045.txt:static offset4 stores1050253722=float0.3',
 'ui-close-source/13447.txt:ObjectAnim type2 fades CanvasGroup/Graphics to0 using default tween ease',
 '../arrow-evidence.json:serialized DOTween defaultEaseType6=OutQuad'],
 remaining='PlayUI CloseAnim0 coroutine/end-of-frame scheduling and asynchronous result resource-load onset are not fully synchronized to this recording.')
limitations=['This is checkpoint validation, not a frame-synchronized90-second replay.',
 'Original pre-recording elapsed time, existing soldiers, per-line clocks, RNG state and prior input are unknown.',
 'Fire skill level remains unknown; ordinary saved loadout was not modified.',
 'Recording covers initial pause/resume, line operations, three skills and one victory; it does not establish fresh entry, retry or defeat behavior.',
 'Audio correlations identify candidates in a compressed mix, not complete audio fidelity.',
 'Original ranking/account/reward/ad UI is outside the user-authorized in-level scope.',
 'Visual acceptance remains pending; previous initial screenshot metric is not a whole-video metric.']
out=dict(schemaVersion='1.0',target=dict(appId='wxcf1394487200e48f',version='43',normalLevel=871,layout=120),atUtc=stamp,
 source=source,timeline=timeline,skillLevelInferences=inferences,productionCorrection=correction,
 measurements=['runtime-measurements.json','audio-matches.json','frame-pts.json','frame-index.json'],
 validation=dict(caseCount=len(checks['checks']),videoChecks=video_checks,build=build,smokeFrames=smoke['frames']),limitations=limitations,fullGoalComplete=False)
write(V/'original-video-evidence.json',out)
manifest=T/'evidence/original-reference/manifest.json';v=read(manifest)
entry=dict(id='video-20260928',**source,evidence='generated/video-20260928/original-video-evidence.json')
v['videos']=[r for r in v.get('videos',[]) if r.get('id')!=entry['id']]+[entry]
v['scope']='Entry screenshot plus supplied paused-to-victory recording; video supports bounded runtime checkpoints, not synchronized full replay.'
write(manifest,v)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']=dict(status='bounded-checkpoints-verified',evidence='generated/video-20260928/original-video-evidence.json',skillLevelInferences=inferences,correction='source SkillUI0.3-second close fade',fullReplayMatched=False);write(p,v)
p=G/'golden-cases.json';v=read(p);v['originalVideoCheckpoints']=dict(evidence='generated/video-20260928/original-video-evidence.json',validator='UnityProject/Assets/AreaBattle/Editor/OriginalVideoValidation.cs',checks=[c['id'] for c in video_checks],scope='Independent observed checkpoint fixtures; inferred levels and excluded original session state remain explicit.');write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p)
v['target']['representativeLevel']='layout5 mechanics replay; normal871/layout120 entry screenshot and original video checkpoints'
milestone=dict(id='original-video-20260928-checkpoints',atUtc=stamp,status='bounded-runtime-checkpoints-pass',caseCount=len(checks['checks']),evidence=['generated/video-20260928/original-video-evidence.json'])
v['milestones']=[m for m in v.get('milestones',[]) if m.get('id')!=milestone['id']]+[milestone];write(p,v)
p=W/'analysis/GOAL_COMPLETION_AUDIT.json';v=read(p);v['atUtc']=stamp
v['latestBuildSha256']=hashlib.sha256((W/'Build/Windows/AreaBattle.exe').read_bytes()).hexdigest()
v['blockerAudit'].update(priorCaptureBlocker='Supplied original screenshot and90.52-second video now provide usable reference evidence; native screenshot helper failure does not block offline recording analysis.',
 independentProgressCompleted=f'{len(checks["checks"])} integrated checks and fresh600-frame smoke pass. Video checkpoints verified; source SkillUI0.3-second result fade corrected.',
 remainingEvidence='Full synchronization of pre-recording state, original RNG, fire count/level and asynchronous UI scheduling; other unobserved modes retain prior gaps.')
for row in v['requirements']:
 if row['requirement']=='Complete in-level mechanics':row['scope']=f'{len(checks["checks"])} integrated checks, including original video checkpoint cases; complete runtime matching remains open.'
 if row['requirement']=='Representative matched visual comparison':
  row['status']='entry-frame-and-video-checkpoints-compared';row['evidence'].append('analysis/targets/wxcf1394487200e48f/43/generated/video-20260928/original-video-evidence.json') if not any('original-video-evidence' in x for x in row['evidence']) else None
  row['missing']=['Synchronized full-run state and visual/audio comparison, including original async window scheduling']
write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p)
v['mechanicsDataScope'].append('Original video871 ice/lightning/stock checkpoints and source0.3s skill HUD fade') if not any('video871' in x for x in v['mechanicsDataScope']) else None
v['pendingFullGoal']=[x for x in v['pendingFullGoal'] if not x.startswith('Single level871')]+['Original90.52-second video ingested; bounded checkpoints pass, complete synchronized replay still pending.']
v['originalVideoEvidence']='analysis/targets/wxcf1394487200e48f/43/generated/video-20260928/original-video-evidence.json';write(p,v)
note=f'''\n\n## Original recording validation — 2026-09-28\n\nPreserved the supplied90.52-second,720x1334 original recording and SHA256. Native PTS locate ice36.08522s, fire52.37694s, lightning62.29858s; generic stock27→26→25→24. Adjacent lightning frames show green45→0 without capture. Ice active-state duration supports level10; lightning10 is a strong isolated-cast candidate; fire remains unknown (data1 counts projectiles, every level hits for2). Source-confirmed SkillUI closes its commander immediately and fades graphics with0.3s OutQuad; production and retry reset corrected. {len(checks['checks'])} integrated checks, build0errors/{build['warnings']}warnings and fresh600-frame smoke pass. Recording starts already paused, so initial clocks/RNG and full matching remain open. See `analysis/VIDEO_VALIDATION_REPORT.md` and `generated/video-20260928/original-video-evidence.json`.\n'''
for p in [W/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md',T/'REVERSE_PROGRESS.md']:
 s=p.read_text(encoding='utf-8');marker='\n\n## Original recording validation — 2026-09-28';s=s.split(marker)[0];p.write_text(s+note,encoding='utf-8')
report=f'''# 第 871 关录像校验\n\n已分析你提供的 90.52 秒录像，并修正结算时技能栏的退出表现。当前 {len(checks['checks'])} 项集成检查通过，Windows 构建 0 错误、{build['warnings']} 警告，新的 600 帧运行测试通过。\n\n| 原版录像时间 | 观察与结论 |\n|---|---|\n| 约 0.81 秒 | 从暂停恢复；录像并非刚进入关卡 |\n| 36.085 秒 | 冰冻命中四座敌塔，道具点 27→26；已占领的蓝塔不受影响 |\n| 50.059–50.092 秒 | 冰技能的生效状态结束，冰壳继续消退；约 14 秒支持 10 级推断 |\n| 52.377 秒 | 使用火技能，道具点 26→25；可观察到敌方伤害与友方治疗 |\n| 62.299 秒 | 雷击绿色塔，数值 45→0，阵营仍为绿色，道具点 25→24 |\n| 86.044 秒起 | 进入结算；指挥官先隐藏，底部技能栏随后淡出 |\n\n## 已修正\n\n原版代码中 SkillUI 的 CloseBefore 先隐藏指挥官，CloseAnim=2 使用默认 0.3 秒渐隐。还原版此前将整个技能栏立即隐藏，现在按原始 OutQuad 曲线淡出，并在退出期间关闭点击。重试会恢复透明度、点击状态和新的指挥官动画状态。新增检查覆盖这一过程。\n\n## 技能等级与校验边界\n\n你不清楚技能等级，所以本次没有修改正常存档。冰冻 10 级有持续时间证据；雷击 10 级与单次 45 点扣减吻合，但单凭相邻帧不能完全排除同帧其他伤害。火技能等级暂不能确定：配置中的等级改变投射物数量，单发伤害与治疗各级均为 2，不能通过单次数值推算等级。\n\n录像已覆盖恢复、连线操作、三种技能和胜利；未覆盖刚入关、重试或失败。录像开始前的兵线、计时和随机状态未知，因此目前是独立检查点验证，尚未完成整局同步画面验收。\n\n原始视频已按哈希保留。逐帧时间、音频相关性与源码定位见 [机器可读证据](targets/wxcf1394487200e48f/43/generated/video-20260928/original-video-evidence.json)。\n'''
(W/'analysis/VIDEO_VALIDATION_REPORT.md').write_text(report,encoding='utf-8')
print(f'Registered original video,4 checkpoint cases,source fade correction;{len(checks["checks"])} checks. Full goal remains incomplete.')
