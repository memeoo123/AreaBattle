"""Publish verified result HUD lifecycle with static generic-call provenance."""
import json,hashlib,shutil
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;R=W/'analysis/targets/wxcf1394487200e48f/43';G=R/'generated'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
stamp=datetime.now(timezone.utc).isoformat()
checks=read(W/'analysis/unity-integrated-validation.json');build=read(W/'analysis/unity-build-report.json');smoke=read(W/'analysis/player-smoke.json')
assert checks['passed'] and build['result']=='Succeeded' and smoke['passed']
assert (W/'analysis/player-smoke.json').stat().st_mtime>=(W/'Build/Windows/AreaBattle.exe').stat().st_mtime
count=len(checks['checks']);dest=G/'dynamic-ui-disassembly';dest.mkdir(exist_ok=True)
shutil.copyfile(W/'analysis/playui-closebefore.txt',dest/'playui-closebefore.txt')
rules=[
 dict(id='result-closes-play-and-guide-ui',status='confirmed',
      rule='LevelControl.OnGamePlayState closes Proj_xqzdPlayUI before showing either result, and closes GuideUI on both branches. Metadata generic usage4018264 is UIModule.Close<Proj_xqzdPlayUI>,4018248 is Close<GuideUI>;4018476/4018472 are Show<Proj_xqzdOverUI>/Show<Proj_xqzdFailUI>. The local preloaded panel adapter now hides PlayTopBar, StarInfoRoot and GuideUI on result.',
      evidence=['ui-generic-call-resolution.json','original871-disassembly/wasmcode-7630.txt:0x37a407-0x37a50d']),
 dict(id='play-ui-closes-skill-ui',status='confirmed',
      rule='Proj_xqzdPlayUI.CloseBefore f18063 resets LevelSpeed to1 and closes SkillUI via generic usage4018272. Result therefore also hides the original skill buttons and commander. The local adapter stops manually advancing the hidden commander.',
      evidence=['ui-generic-call-resolution.json','dynamic-ui-disassembly/playui-closebefore.txt:0x7a2740-0x7a2762']),
 dict(id='result-retry-new-commander-state',status='confirmed-with-local-preload-adapter',
      rule='The source UIModule removes closed windows from its registry; subsequent Open constructs a fresh window. Reopened SkillUI has fresh Awake busy flag and new loaded skeleton. Local retry reuses preloaded UGUI wrappers but creates a fresh commander and clears prior Complete subscriptions/busy state. Pause-only retry retains existing commander state.',
      evidence=['ui-lifecycle-evidence.json','dynamic-ui-disassembly/skillui-awake.txt','original871-disassembly/wasmcode-15922.txt']),
 dict(id='victory-title-source-delay',status='confirmed',
      rule='Original result autoplay clip keeps go_victory/Image/Text inactive through .4333333373 and activates it at .6333333254 seconds. No immediate title visibility override is applied; tests verify the hidden and revealed segments.',
      evidence=['presentation-prepared/ui-result-animations.json:go_victory/Image/Text.m_IsActive'])
]
out=dict(target=dict(appId='wxcf1394487200e48f',version='43'),atUtc=stamp,rules=rules,
 validation=dict(caseCount=count,passed=True,build=build,smokeFrames=smoke['frames']),
 captures=[f'analysis/captures/original871-reconstruction-{s}.png' for s in ['victory-entry','victory','victory-retry','defeat','defeat-retry']],
 limitations=['Controlled score/capture inputs trigger production outcomes; these result captures are not original references.',
              'Original asynchronous window-close/resource-load latency and matched result recordings remain unobserved.',
              'Current ordinary victory continuation button is the previously authorized local no-reward adapter.'],fullGoalComplete=False)
write(G/'result-hud-lifecycle-evidence.json',out)
p=G/'RESTORE_SPEC.json';v=read(p);v['resultHudLifecycleCorrection']={'status':'source-confirmed-local-preload-adapter','evidence':'generated/result-hud-lifecycle-evidence.json'};write(p,v)
p=R/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);milestone=dict(id='result-hud-close-reopen-correction',atUtc=stamp,status='source-derived-integration-pass',caseCount=count,evidence=['generated/result-hud-lifecycle-evidence.json'])
v['milestones']=[m for m in v.get('milestones',[]) if m.get('id')!=milestone['id']]+[milestone];write(p,v)
p=W/'analysis/GOAL_COMPLETION_AUDIT.json';v=read(p);v['atUtc']=stamp;v['latestBuildSha256']=hashlib.sha256((W/'Build/Windows/AreaBattle.exe').read_bytes()).hexdigest()
for row in v['requirements']:
 if row['requirement']=='Complete in-level mechanics':row['scope']=f'{count} source-derived production cases; all639 layouts,18 skills,Boss,PvP,guide; result HUD closure and commander reinitialization on retry verified.'
v['blockerAudit']['independentProgressCompleted']=f'Generic UI targets resolved through26589 bounds-checked method specs. Result HUD/SkillUI/GuideUI closure and fresh commander on retry corrected; {count} checks and fresh600-frame smoke pass.';write(p,v)
note=f'\n\n## Result HUD lifecycle correction\n\n{stamp}. Resolved source generic UI targets using unique static metadata registration at1187060; all26589 method specs pass bounds validation. Win/loss closes PlayUI, whose CloseBefore closes SkillUI; both result branches also close GuideUI. Production HUD now hides those panels and stops the hidden commander clock. Reopening after a result creates a fresh commander without old busy state/completion callbacks. The original victory title activation at0.6333333254s is retained. All{count} integrated checks and fresh600-frame smoke pass; build{build["errors"]}errors/{build["warnings"]}Spine obsolete-API warnings. Controlled result screenshots are reconstruction-only, not matched original evidence. See `generated/result-hud-lifecycle-evidence.json`. Full replay acceptance remains open.\n'
for p in [R/'REVERSE_PROGRESS.md',W/'RESTORE_PROGRESS.md',W/'analysis/VALIDATION_REPORT.md']:
 with p.open('a',encoding='utf-8') as f:f.write(note)
print(f'Recorded result lifecycle: {count} cases; original synchronized replay still pending.')
