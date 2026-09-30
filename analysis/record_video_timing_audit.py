"""Persist original pixel timing versus instrumented replay, without retuning mechanics."""
import json
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
events=read(V/'full-replay-events.json')['events'];original=read(V/'first-ai-response-pixels.json')['changes'];pairs=[]
for o,line in zip(original,[6,4]):
 e=next(e for e in events if e['kind']=='connect' and e['tower']==5 and e['line']==line and e['pts']>=o['pts'])
 pairs.append(dict(originalInterval=[o['previousPts'],o['pts']],replayPts=e['pts'],lateSeconds=e['pts']-o['pts'],source=5,target=2 if line==6 else 1))
capture=read(V/'tower6-capture-pixels.json')['changes'][0];e=next(e for e in events if e['kind']=='capture' and e['tower']==6 and e['camp']==1)
result=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='timing-differences-measured',originalAIResponses=pairs,
 tower6Capture=dict(originalInterval=[capture['previousPts'],capture['pts']],replayPts=e['pts'],lateSeconds=e['pts']-capture['pts']),
 caveats=['Visible original changes include rendering latency. Replay callbacks are instrumented simulation events at recording frame intervals.',
 'Two similar AI delays support investigating timer phase; they do not prove AI code incorrect. Original pre-recording AI/RNG/soldiers remain unknown.',
 'Fire target choices and initial scores differ; no movement-speed or collision-formula defect has been established.'],
 visualCorrection='Original green camp3 has round-headed default soldiers in half-second/012.png. Restrict soldier102 option to camps1/2; preserve green100. Previous all-camp102 fixture was too broad.',
 evidence=['first-ai-response-pixels.json','tower6-capture-pixels.json','full-replay-events.json','half-second/012.png'])
write(V/'timing-audit.json',result)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['timingAudit']='generated/video-20260928/timing-audit.json';v['originalVideoReference']['soldierSkin']['campIds']=[1,2];v['originalVideoReference']['soldierSkin']['greenCampSkinId']=100;write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='video-ai-phase-capture-timing',status='mismatch-measured',atUtc=result['atUtc'],evidence=['generated/video-20260928/timing-audit.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['videoTimingAudit']=result;write(p,v)
marker='\n\n## AI 回应及占领时刻追查'
note=marker+'\n\n新增完整模拟事件时间轴（出兵、碰撞、抵达、分数、占领），与原片像素变化对照。\n\n| 事件 | 原片首个可见帧 | 还原事件 | 差值 |\n|---|---|---|---|\n'
for p in pairs:note+=f"| 红塔5→{p['target']}回连 | {p['originalInterval'][1]:.5f} | {p['replayPts']:.5f} | +{p['lateSeconds']:.3f}秒 |\n"
c=result['tower6Capture'];note+=f"| 右下6号塔变蓝 | {c['originalInterval'][1]:.5f} | {c['replayPts']:.5f} | +{c['lateSeconds']:.3f}秒 |\n"
note+='\n两次 AI 回应均约晚0.15秒，间隔与原片相同，值得继续核对计时相位；尚不能归因为兵速或碰撞公式。右下塔变蓝约晚3秒，明确导致75.639秒玩家连线被拒绝。未为了配录像调整原始数值，也未搜索随机种子。\n\n纠正上一轮皮肤范围：原片5.520秒清晰帧显示绿色兵仍是圆头，102尖帽皮肤现在只应用于蓝、红阵营。生产场景验证同时生成蓝方102与绿方100，完整数值回放仍与更换皮肤前一致。后续仍需分离原版随机火球落点、开局士兵与AI计时状态。\n'
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print(json.dumps(result,ensure_ascii=False))
