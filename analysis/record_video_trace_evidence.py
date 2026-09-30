"""Append the fire-level and committed-input evidence refinement, preserving prior evidence."""
import json,hashlib
from datetime import datetime,timezone
from pathlib import Path
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
checks=read(W/'analysis/unity-integrated-validation.json');build=read(W/'analysis/unity-build-report.json');smoke=read(W/'analysis/player-smoke.json')
assert checks['passed'] and build['result']=='Succeeded' and smoke['passed']
assert (W/'analysis/player-smoke.json').stat().st_mtime>=(W/'Build/Windows/AreaBattle.exe').stat().st_mtime
ids=['video871-fire25-recorded-frame-step-candidate','video871-seven-committed-line-inputs-capacity-sequence']
assert all(any(c['id']==i and c['result']=='pass' for c in checks['checks']) for i in ids)
pulses=read(V/'fire-tip-upper-pulses.json');assert len(pulses['candidates'])==24
trace=read(V/'early-input-trace.json');assert len(trace['events'])==7
inference=dict(skill=2,level=10,status='strong-visual-inference',
 basis='24 red-head crossing runs in screen band y1035..1080, visually reviewed. The first run contains two distinct ascending heads (both visible at52.74325); subsequent23 runs each show one distinct head, total25. Recovered config210 is the only level with25 projectiles. Native recording frame deltas drive25 production launches. This is not an original account profile read.',
 caveat='Threshold pulses alone are not projectile identities; manual image review disambiguates the first merged run and rejects trailing embers. Cross-band times are not launch times.')
out=dict(atUtc=datetime.now(timezone.utc).isoformat(),fireLevelInference=inference,
 originalCount=dict(detectorRuns=24,firstRunDistinctHeads=2,otherRunsDistinctHeads=23,total=25,contactSheet='fire-tip-upper-candidates.png',rawMeasurements='fire-tip-upper-pulses.json',recordingSha256='439e6d33828fb2b98be78f2085ce28dbbbcc06f4706be8e9f0ac5ba2a5493bd0'),
 committedInputs=trace,tests=ids,passed=True,
 limitations=['Recorded video PTS includes render/input latency; exact engine input timestamp remains bounded by adjacent frames.',
 'Seven-input test verifies acceptance and outgoing capacity order, not unobserved initial soldiers/RNG or complete battle timing.',
 'Fire replay uses a controlled static tower fixture and native video frame intervals; target choices are not claimed to match original random choices.'])
write(V/'fire-and-input-evidence.json',out)
p=V/'original-video-evidence.json';v=read(p);v['skillLevelInferences']=[inference if r['skill']==2 else r for r in v['skillLevelInferences']]
v['refinementEvidence']='fire-and-input-evidence.json';v['limitations']=[x for x in v['limitations'] if not x.startswith('Fire skill level remains')];v['limitations'].append('All three equipped skill levels have inferred candidates; original account levels are not directly read.');write(p,v)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['skillLevelInferences']=[inference if r['skill']==2 else r for r in v['originalVideoReference']['skillLevelInferences']];v['originalVideoReference']['inputTrace']='generated/video-20260928/early-input-trace.json';write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='video-fire25-seven-input-refinement',atUtc=out['atUtc'],status='bounded-checkpoints-pass',evidence=['generated/video-20260928/fire-and-input-evidence.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/GOAL_COMPLETION_AUDIT.json';v=read(p);v['blockerAudit']['remainingEvidence']='Full synchronization of pre-recording state, original RNG and asynchronous UI scheduling; fire25 count now supports level10. Other unobserved modes retain prior gaps.';write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['mechanicsDataScope'].append('Video fire25 production launches using original native-frame intervals; seven committed line operations and capacity changes verified.');write(p,v)
marker='\n\n## Video fire and input refinement'
note=f'''{marker}\n\n{out['atUtc']}. Fire level previously unknown is now a strong level10 inference:25 distinct heads visually reviewed from24 detector runs (first run contains two), with config210 and25 production launches at recorded frame steps. Seven committed line operations identified through filled capacity circles, separated from drag previews. All{len(checks['checks'])} integrated checks pass; fresh600-frame smoke; build{build['errors']}errors/{build['warnings']}warnings. No ordinary save modified. This does not establish original RNG, pre-recording clocks or full matched replay. Evidence:`generated/video-20260928/fire-and-input-evidence.json`.\n'''
for p in [W/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md',T/'REVERSE_PROGRESS.md']:
 s=p.read_text(encoding='utf-8').split(marker)[0];p.write_text(s+note,encoding='utf-8')
p=W/'analysis/VIDEO_VALIDATION_REPORT.md';s=p.read_text(encoding='utf-8')
s=s.replace('火技能等级暂不能确定：配置中的等级改变投射物数量，单发伤害与治疗各级均为 2，不能通过单次数值推算等级。','火技能此前未知；进一步逐帧计数得到 25 个独立火球，现支持 10 级推断。像素检测得到 24 段火球头通过区间，其中第一段包含两个不同火球，已通过画面复核。使用录像逐帧时间间隔，还原版同样发射 25 个。等级依据来自数量，而非单发伤害。')
s+='\n\n## 前半段已提交操作\n\n提交时间依据塔顶实心容量圆点变化；与拖动预览出现时间分开记录，误差约一帧，尚不等于引擎内部输入时间。塔编号：1 左上、2 右上、5 中右。\n\n| 视频秒数 | 操作 |\n|---|---|\n'
for e in trace['events']:
 action='连接' if e['kind']=='connect' else '切断';s+=f"| {e['pts']:.3f} | {action} {e['source']} → {e['target']} |\n"
s+='\n这 7 次操作已通过还原版的连线接受与容量顺序检查。未把未知的初始士兵、计时或随机状态补写成原版事实。\n';p.write_text(s,encoding='utf-8')
print('Recorded fire25/level10 inference and7 committed inputs;',len(checks['checks']),'checks.')
