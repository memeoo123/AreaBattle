"""Static PVP/daily evidence and explicit implementation gate. No target execution."""
import json,subprocess,sys
from pathlib import Path
G=Path(__file__).parent/'targets/wxcf1394487200e48f/43/generated'
def load(p):return json.loads(p.read_text(encoding='utf-8'))
def ref(a,b,m='wasmcode'):return f'generated/wasm/{m}.wasm:0x{a}-0x{b}'
def rule(i,v,*e):return dict(id=i,status='confirmed-static',value=v,evidence=list(e))
rules=[
rule('pvp-setup','PVP flag disables ordinary result handling. PVP.Init reads opponent AI grade -> AgentGradeConfig SkillUseId, AgentName and clamped ActionRate; chooses weighted CommanderConfig.usePercent>=1. Enemy camp is the LAST serialized CampInfoCfg whose ID is2..4. Creates Commander with that camp and three AgentSkillUse slots. Implementation accepts explicitly supplied commander, saved skill levels and configuration; arena matching/model inference is not synthesized.',ref('540026','540308'),ref('7e107e','7e1487'),ref('163a82','163a90')),
rule('pvp-agent-init','Init copies slot/camp, sets active, total, delay, corresponding rate/space and timer0. Clear only sets inactive. Init DOES NOT reset usedCount; reused pooled slots preserve it.',ref('3d2b76','3d2c07'),ref('e070f','e0716')),
rule('pvp-agent-clock','Only Running: dt*GameTimeScale. Delay>0 subtracts delta then returns, including a crossing frame. Timer-=delta; strict timer<0 and usedCount<=total gate. Adds space once (no catch-up). Random.Next(0,100)<trunc(rate) then Execute(slot+(commander-1)*3), usedCount+=1 regardless whether skill was already active. Three independent per-slot budgets; count0 permits total+1 probability successes.',ref('901021','9010eb'),ref('199779','199798'),ref('7e0ad0','7e0af9')),
rule('pvp-null-callback','AgentSkillUse.Execute checks matching Skill.inSkill and only when false invokes CommanderBase.Execute(id,null). CommanderBase writes callback40=null then invokes skill virtual Execute. No UI inventory/cooldown/target argument is passed.',ref('623f50','623f6b'),ref('3abaee','3abb26')),
rule('pvp-targets','AI skill3 and12 choose random active Camp1 towers (not nearest, not arbitrary enemy);9 random active same-skill-camp tower;15 random ANY active tower including neutral. Skill6 only copies UI selected target if camp1: fresh enemy skill6 activates lifetime but has no target/heal. Skill18 chooses random Camp1 tower then x/z += inclusive Random(-20,20)/100, y unchanged; AI Execute bypasses player drag bounds.',ref('6e7388','6e756a'),ref('6b1fa7','6b1fce'),ref('5127ef','512b9f'),ref('5406ed','540ae0'),ref('6dd2c6','6dd372'),'combat-evidence.json:commander3-drain'),
rule('pvp-result','CampInfo refresh sums trunc_toward_zero(Score) per active tower. While Running and PvP renderer exists, enemy SumScore<=0 => Victory first; else player SumScore<=0 => Defeat. Thus bothzero => Victory, fractional0.5 tower contributes0; ownership of all neutral towers is not required. Kernel fixture assumes initialized PVP renderer/camp records.',ref('1638ca','1639a2'),ref('7e0c5d','7e0d44')),
rule('pvp-order','MineGameMain registers PVPController before AICampController, LevelControl, SkillControl and WayLineControl. PVP first updates AgentSkillUse scaleddt, then enemy Commander rawdt. Kernel invokes agent clocks before its unified skill update; all SkillRuntime camps receive rawdt once.',ref('5516d1','5516fd'),ref('55179c','551859'),ref('7e0b94','7e0bc1')),
rule('daily-map-consumption','GetChallengeLevel uses selected index only when unlockAll debug flag is enabled, else current challenge; GetLevel returns todayLeveldict[index].level. RandomLevels iterates saved pools: lookup todayLeveldict[pool.id], pops LAST pool entry and stores level. Pop helper uses Enumerable.Last then RemoveAt(Count-1), no RNG. Empty pools are not refilled in this method. Pool initialization/shuffle remains unknown; legacy raw challengeLevel JSON has no corresponding typed DailyChallengeConfig field.',ref('b07dac','b07dd3','wasmcode1'),ref('4c52a6','4c52bf','wasmcode1'),ref('b07bce','b07c4c','wasmcode1'),ref('140c807','140c83e','wasmcode1'),ref('1aab50','1aab78'),ref('39a653','39a67f'))]
cases=[
 {'id':'pvp-budget-off-by-one','pre':{'total':0,'used':0,'timer':0,'dt':.1,'space':1,'chanceSample':0,'rate':100},'expected':{'used':1,'timer':.9,'nextAttemptAtBudgetExhausted':False},'rules':['pvp-agent-clock']},
 {'id':'pvp-delay-crossing','pre':{'delay':.05,'dt':.1,'timer':0},'expected':{'delay':-.05,'timer':0,'attempts':0},'rules':['pvp-agent-clock']},
 {'id':'pvp-chance-strict','pre':{'rate':15,'samples':[14,15]},'expected':{'success':[True,False]},'rules':['pvp-agent-clock']},
 {'id':'pvp-active-budget','pre':{'skillActive':True,'used':0,'total':6,'chanceSuccess':True},'expected':{'used':1,'newExecution':False},'rules':['pvp-agent-clock','pvp-null-callback']},
 {'id':'pvp-both-zero','pre':{'enemyTowerScores':[.5,.5],'playerTowerScores':[0]},'expected':{'enemySum':0,'result':'Victory'},'rules':['pvp-result']},
 {'id':'pvp-enemy-skill6','pre':{'skill':6,'camp':2,'friendlyScore':10,'freshRuntime':True},'expected':{'active':True,'friendlyScore':10,'target':None},'rules':['pvp-targets']},
 {'id':'pvp-poison-discrete-offset','pre':{'playerTowerPosition':[2.4,0,4.9],'samples':[20,20]},'expected':{'point':[2.6,0,5.1],'passesPlayerDragBounds':False,'AIExecuteAllowed':True},'rules':['pvp-targets']},
 {'id':'daily-pop-last','pre':{'pools':{'1':[10,20,30]},'today':{'1':5}},'expected':{'pools':{'1':[10,20]},'today':{'1':30}},'rules':['daily-map-consumption']}
]
for c in cases:c['status']='derived-not-runtime-verified'
selected=[(11175,'wasmcode'),(18346,'wasmcode'),(8338,'wasmcode'),(20924,'wasmcode'),(4876,'wasmcode'),(13316,'wasmcode'),(8120,'wasmcode'),(18343,'wasmcode'),(18344,'wasmcode'),(14822,'wasmcode'),(14258,'wasmcode'),(42680,'wasmcode1'),(10983,'wasmcode'),(11177,'wasmcode'),(14647,'wasmcode'),(4384,'wasmcode'),(30227,'wasmcode1'),(66166,'wasmcode1'),(14264,'wasmcode1'),(30228,'wasmcode1'),(5096,'wasmcode'),(7920,'wasmcode')]
functions=[]
for f,m in selected:
 r=subprocess.run([sys.executable,str(Path(__file__).parent/'flow_disassemble.py'),'@'+str(f),m],capture_output=True,encoding='utf-8',check=True)
 assert 'offset=' in r.stdout
 functions.append(dict(module=m,function=f,text=r.stdout))
unknowns=['Exact Arena match/opponent selection and AgentPath model execution or tactical line policy are not implemented; no model behavior is inferred from a path string.','Skill2 camera-projected launch origin is an explicit host input.','Skill12/15 async asset-ready timing and original RNG sequence remain outside deterministic fixture equivalence.','Original empty-target-list exception behavior is not emulated; fixture reports rejection and still consumes the probability-success budget.','Daily pool initialization, replenishment and ordering provenance are unresolved; only persisted-data selection is implemented.']
out=dict(target={'appId':'wxcf1394487200e48f','version':'43'},mode='static-only',rules=rules,derivedCases=cases,functions=functions,unknowns=unknowns,fullGoalComplete=False)
(G/'pvp-evidence.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
# Root explicitly delegated these two artifact fields for this handoff.
spec=load(G/'RESTORE_SPEC.json')
spec['pvpImplementation']=dict(status='ready',scope='Controlled in-level fixture: recovered three-slot enemy auto-cast clocks, exact target branches and score-based outcome; persisted daily layout selector.',evidence='generated/pvp-evidence.json',rules=[r['id'] for r in rules],unknowns=unknowns,fullGoalComplete=False)
(G/'RESTORE_SPEC.json').write_text(json.dumps(spec,ensure_ascii=False,indent=2),encoding='utf-8')
golden=load(G/'golden-cases.json');ids={c['id'] for c in cases};golden['cases']=[c for c in golden['cases'] if c.get('id') not in ids]+cases
(G/'golden-cases.json').write_text(json.dumps(golden,ensure_ascii=False,indent=2),encoding='utf-8')
print('PVP evidence:',len(rules),'rules',len(cases),'cases',len(functions),'bodies')
