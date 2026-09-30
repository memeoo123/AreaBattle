"""Record source-backed retry/commander fixes without raising fidelity acceptance."""
import hashlib, json, shutil
from datetime import datetime, timezone
from pathlib import Path
W=Path(__file__).resolve().parent.parent
R=W/'analysis/targets/wxcf1394487200e48f/43'; G=R/'generated'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
stamp=datetime.now(timezone.utc).isoformat()
checks=read(W/'analysis/unity-integrated-validation.json');build=read(W/'analysis/unity-build-report.json');smoke=read(W/'analysis/player-smoke.json')
assert checks['passed'] and build['result']=='Succeeded' and smoke['passed']
count=len(checks['checks'])
dest=G/'dynamic-ui-disassembly';dest.mkdir(exist_ok=True)
for name in ['skillui-awake','skillui-close','event-cctor','overui-ctor','failui-ctor','daily-pool66164','daily-pool66165']:
    shutil.copyfile(W/f'analysis/{name}.txt',dest/f'{name}.txt')
rules=[
 dict(id='retry-player-camp-highlight',status='confirmed',
      rule='Again10 enters Enter3 after clearing data. The normal loading/StartPlay5 path calls ShowMyCampEffect before Running6 whenever guide does not take over. Retry therefore repeats the three-second effect107 for current player towers; local adapter triggers it after restored towers and presentation.',
      evidence=['original871-disassembly/wasmcode-7630.txt:0x37a510-0x37a555','original871-disassembly/wasmcode-7630.txt:0x37a2fb-0x37a339','original871-disassembly/wasmcode-4973.txt'],
      limitation='Original asynchronous reload latency remains unmeasured; this correction establishes ordering, not a matched wall-clock delay.'),
 dict(id='commander-complete-subscription',status='confirmed',
      rule='SkillUI.Awake subscribes to Event.UseSkill and initializes its busy flag false. Its use handler f15918 returns while busy, otherwise adds an AnimationState.Complete delegate and starts the non-looping skill animation. No completion delegate is registered by the prefab load callback f15922. Before the first cast the idle TrackEntry must survive loop completion. f15919 clears busy and returns to looping idle. Original per-accepted-cast delegate addition is retained.',
      evidence=['dynamic-ui-disassembly/skillui-awake.txt:0x74cf19-0x74cf5f','dynamic-ui-disassembly/event-cctor.txt:0x8704a2-0x8704b5','original871-disassembly/wasmcode-15918.txt:0x74c8fc-0x74c9c9','original871-disassembly/wasmcode-15919.txt','original871-disassembly/wasmcode-15922.txt']),
]
evidence=dict(target=dict(appId='wxcf1394487200e48f',version='43'),atUtc=stamp,rules=rules,
    dailyPoolAudit='daily-pool-all-wasm-callsite-audit.json',
    dailyPoolConclusion='Expanded from mapped gameplay methods to all 90641 local defined WASM bodies. No direct constructor calls/table constants found; initial fill/refill remains unknown. Reflection, unavailable module bodies and persisted/server inputs are not ruled out.',
    validation=dict(caseCount=count,passed=True,build=build,smokeFrames=smoke['frames'],sourceWarnings='Five CS0618 obsolete Unity API warnings in original Spine4.1 runtime; no compile errors.'),
    fullGoalComplete=False)
write(G/'dynamic-ui-evidence.json',evidence)
p=G/'RESTORE_SPEC.json';v=read(p);v['dynamicPresentationCorrection']={'status':'confirmed-with-async-timing-limits','evidence':'generated/dynamic-ui-evidence.json'};write(p,v)
p=R/'BATTLEFIELD_RESTORE_STATE.json';v=read(p)
milestone=dict(id='retry-commander-dynamic-correction',atUtc=stamp,status='source-derived-integration-pass',evidence=['generated/dynamic-ui-evidence.json'],caseCount=count)
v['milestones']=[m for m in v.get('milestones',[]) if m.get('id')!=milestone['id']]+[milestone];write(p,v)
p=W/'analysis/GOAL_COMPLETION_AUDIT.json';v=read(p);v['atUtc']=stamp
v['latestBuildSha256']=hashlib.sha256((W/'Build/Windows/AreaBattle.exe').read_bytes()).hexdigest()
for row in v['requirements']:
    if row['requirement']=='Complete in-level mechanics':row['scope']=f'{count} source-derived production cases; all639 layouts initialize;18 skills,Boss,PvP,guide,ordinary lifecycle fixtures; retry highlight and six commander idle/cast/pause cases'
v['blockerAudit']['independentProgressCompleted']=f'Retry highlight and commander lifecycle corrected; {count} checks and fresh600-frame binary smoke pass. Daily constructor search expanded to all90641 local WASM bodies without inventing a fill algorithm.'
write(p,v)
note=f'\n\n## Retry and commander lifecycle correction\n\n{stamp}. Source-backed retry now repeats player tower highlight after restoration. Commander completion is registered on accepted skill use, retaining the original pre-cast idle loop and busy-cast guard. All {count} integrated checks and a fresh {smoke["frames"]}-frame standalone smoke pass; build has {build["errors"]} errors and {build["warnings"]} original Spine obsolete-API warnings. Updated entry capture retains its prior single-frame pixel metrics. Daily pool initialization/refill remains unknown after expanding constructor callsite search to all 90,641 local defined WASM bodies. Evidence: `generated/dynamic-ui-evidence.json`; full synchronized replay remains pending.\n'
for p in [R/'REVERSE_PROGRESS.md',W/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md']:
    with p.open('a',encoding='utf-8') as f:f.write(note)
print(f'Recorded {count} passing checks, {len(rules)} source corrections; full fidelity remains incomplete.')
