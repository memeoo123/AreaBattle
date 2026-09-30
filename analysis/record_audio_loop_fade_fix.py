"""Register a source-backed production audio fix and fresh validation."""
import json,subprocess
from datetime import datetime,timezone
from pathlib import Path
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';g=t/'generated';v=g/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def write(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
now=datetime.now(timezone.utc).isoformat();audit=read(g/'audio-loop-fade-fix-audit.json');checks=read(w/'analysis/unity-integrated-validation.json');assert checks['passed'] and len(checks['checks'])==265
assert audit['numericReplayUnchanged'] and read(w/'analysis/unity-build-report.json')['result']=='Succeeded' and read(w/'analysis/player-smoke.json')['passed']
assert read(g/'inlevel-resource-audit.json')['passedIntegrity']
p=g/'RESTORE_SPEC.json';s=read(p);s['originalVideoReference']['lightningOnsetAudit']='generated/video-20260928/lightning-onset-audit.json';s['originalVideoReference']['audioLoopFadeAudit']='generated/audio-loop-fade-fix-audit.json';write(p,s)
p=t/'BATTLEFIELD_RESTORE_STATE.json';s=read(p);m=dict(id='source-music-loop-fade-restored',status=audit['status'],atUtc=now,evidence=['generated/audio-loop-fade-fix-audit.json']);s['milestones']=[x for x in s.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for key,val in old.items():
 if key not in s:s[key]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}))
s['latestValidation']={'scope':'Production music loop fade repair, source-derived wall-time/stop tests, lightning onset measurement and ice reuse inspection.','checksPassed':265,'freshPlayerBuild':True,'freshPlayerSmoke':True,'smokeFrames':600,'commands':['BattleBuild.BuildAndValidate','AreaBattle.exe -battle-smoke','OriginalVideoReplayDiagnostic.Run','EffectLifecycleAudit.Run','BattleBuild.ValidateMechanicsOnly','python analysis/asset_inlevel_audit.py'],'numericReplayUnchanged':True,'sourceAudioAudit':'generated/audio-loop-fade-fix-audit.json','notClaimed':'Matched complete original visual/audio replay; exact async load latency.'}
s['audioLoopFadeAudit']=audit;s['pendingFullGoal']=['Complete original visual/audio replay with source viewport and random session uncertainty','Resource callback delivery order and observed lightning latency','Unverified special-mode original captures and daily pool initialization'];write(p,s)
note='''\n\n## 音乐循环淡入淡出修复与雷击首帧审计（2026-09-29）

[已确认并修复] 原AudioConfig1001为Ptype4、Vol0.4、StartTime/EndTime均0，音轨约40.00798秒。AudioLoopFadeAction f20576在播放位置到达音轨长度减1秒时，经f13128启动1000ms淡出，f20574回调随后启动1000ms淡入；AudioSource.loop保持true，不重启或seek。此前BattleAudio只实现首次淡入和结算淡出，遗漏每次自然循环的尾首淡变。现AudioLoopFadeEnvelope补齐该行为，并保留暂停期间的墙钟计时、自然淡出期间Stop直接完成、普通Stop从当前音量乘设置值淡出。源码摘录在generated/audio-loop-fade-disassembly，结论见audio-loop-fade-fix-audit.json。

新增3项源分支测试：多轮尾首淡变、暂停前后墙钟推进、自然淡出及初始淡入期间终止。265项集成检查全部通过，重新构建Windows成功（0错误、5条已有Spine警告），新二进制600帧smoke通过、重试前后各9次出兵；240文件/1363 GUID资源审计通过。完整录像数值报告仍与full-replay-before-skin102.json相同。未修改伤害、兵力、随机种子或移动速度。

[已测量，尚不改固定时序] 原片62.29858秒首次显示雷击扣兵，62.46562秒仍无闪电，62.48236秒首次出现闪电，观察间隔0.18378秒。原f14822先EffectModule.Show再立即ChangeScore，资源回调允许后续才出现画面；一次录像不能证明恒定0.184秒加载耗时。见video-20260928/lightning-onset-audit.json、lightning-onset-contact.png。没有把这个差值加入生产常数。

[未发现复用缺陷] 冰晶在停用后复用，0.3秒的Animator状态hash、归一化时间、全部子节点缩放与新实例一致；未改相机HDR、材质或Animator重置。粒子随机性和全部过渡帧不在该验证结论内。完整视觉/听觉验收仍未通过。
'''
marker='\n\n## 音乐循环淡入淡出修复与雷击首帧审计'
for p in [w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',w/'analysis/VIDEO_VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf8').split(marker)[0]+note,encoding='utf8')
p=w/'AREA_BATTLE_HANDOFF.md';s=p.read_text(encoding='utf8');s+='''
## 最新补充：音乐循环修复（当前265项检查）
- 生产BattleAudio.cs新增AudioLoopFadeEnvelope：每次40.00798秒BGM循环的最后1秒淡出，下一轮首秒淡入；源f20576/13128/20574及其嵌套回调提供证据，不改AudioSource.loop、不seek。完整证据G/audio-loop-fade-fix-audit.json和audio-loop-fade-disassembly/。
- 新增3项音频分支检查，总265项通过；Windows已重新构建，新二进制600帧smoke通过，240文件/1363 GUID审计通过。旧262项仅是历史数量。完整数值回放与full-replay-before-skin102.json完全一致。
- V/lightning-onset-audit.json记录扣兵62.29858、末个无闪电62.46562、首次闪电62.48236，间隔0.18378秒。属当前录像观察，资源回调耗时未知；禁止直接加入固定0.184秒常数。
- V/effect-lifecycle-audit.json确认冰晶停用复用后0.3秒Animator/子节点缩放匹配新实例。不是粒子随机性或全部动画验收。
- 更新脚本analysis/record_audio_loop_fade_fix.py保留manifest历史扩展；本轮生产音频改动已有新构建和smoke，无待轮询进程。
- 下一步可继续追踪EffectModule异步资源实际交付规则；仍缺完整原始视觉/听觉验证与特殊模式、每日池证据。不要重复处理已确认文字缓存或BGM循环淡变。
''';p.write_text(s,encoding='utf8')
p=v/'visual-review.html';s=p.read_text(encoding='utf8');mark='<p>本次新增截图没有改变数值回放结果。';s=s.replace(mark,'<p>雷击首帧：原片扣兵后约0.184秒才出现闪电。本地资源同步就绪；未将这次观察值写成固定延迟。<a href="lightning-onset-contact.png">逐帧证据</a> · <a href="lightning-onset-audit.json">测量与限制</a>。音乐已按原始源码补齐每轮末尾淡出及开头淡入，尚未通过完整听觉验收。</p>'+mark);p.write_text(s,encoding='utf8')
print('Registered production audio repair with265 fresh checks, new build and600-frame smoke.')
