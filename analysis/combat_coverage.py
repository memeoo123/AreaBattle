"""Skill coverage and actual serialized Boss references; never execute target code."""
import json, hashlib
from pathlib import Path
from collections import defaultdict
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
OUT=ROOT/'generated'
def read(p):return json.loads(p.read_text(encoding='utf8'))
def table(n):return read(OUT/'tables'/f'{n}.json')['Datas']
e=read(OUT/'combat-evidence.json');rules={r['id']:r for r in e['rules']}
names=['冰冻','火焰','闪电','加速','减速','发展','夜魔侵袭','血族契约','吸血','箭雨','迅捷','瞄准','鼓舞','募兵','降临','虚弱','恢复','毒药']
keys=['commander1-freeze','commander1-fireball','commander1-direct-damage','commander2-speed','commander2-speed','commander2-score','commander3-unlinkable','commander3-undead','commander3-drain','commander4-arrow-rain','commander4-speed','commander4-dot','commander5-inspiration','commander5-recruit','commander5-serial-score','commander6-fatigue','commander6-recovery','commander6-poison']
# Name of the skill6 rule is derived rather than duplicating a guessed ID.
keys[5]=next(k for k,r in rules.items() if 'skill6' in r['scope'].replace(' ',''))
commanders=table('CommanderConfig');allskills={r['id']:r for r in table('AllSkillConfig')};configs=table('SkillConfig')
unknown={2:['RandomHelper upper-bound convention and interruption timing not runtime-tested'],7:['Exact bat initial travel timing'],10:['Visual coroutine end-boundary versus active timer ordering'],12:['Async selected-target visual travel and exact duration start'],14:['randomPoint distribution, scene-unit step on pause'],15:['AI selection strategy and invalid selection flow'],18:['Bottle arrival versus duration start and pause timing']}
skills=[]
for sid in range(1,19):
 commander=next(c for c in commanders if sid in c['skills'])
 entries=[r for r in configs if r['id']//100==sid]
 r=rules[keys[sid-1]]
 skills.append({'skillId':sid,'name':names[sid-1],'commanderId':commander['id'],'branchStatus':'activebranch','branchMeaning':'Concrete Commander skill implementation and CommanderConfig binding exist; player unlock/save availability is separate.','configStatus':'confirmed','mechanicsStatus':'confirmed','ruleId':r['id'],'mechanics':r['rule'],'allSkillConfig':allskills[sid],'levels':entries,'representativeConfig':next(x for x in entries if x['id']==sid*100+1),'commanderUnlockType':commander['unlockType'],'commanderUsePercent':commander['usePercent'],'requiredForFullCommanderCoverage':True,'runtimeObserved':False,'remainingUnknowns':unknown.get(sid,[]),'evidence':r['evidence']})
stars=defaultdict(list);camprefs=defaultdict(list);levels=[]
for f in sorted((OUT/'all-levels').glob('*.json')):
 d=read(f);levels.append(f)
 for i,t in enumerate(d.get('StarInfoCfgs',[])):
  if t.get('isBoss'):
   stars[t.get('bossSkillId',0)].append({'level':f.relative_to(ROOT).as_posix(),'towerIndex':i,'campId':t['CampID'],'startScore':t['StartScore'],'shipId':t['ShipID']})
 for c in d.get('CampInfoCfgs',[]):
  if c.get('BossActionId',0):camprefs[c['BossActionId']].append({'level':f.relative_to(ROOT).as_posix(),'campId':c['CampID']})
boss=[]
for c in table('BossConfig'):
 cid=c['id'];active=stars.get(cid,[]);declarative=camprefs.get(cid,[])
 status='activebranch' if active else 'unknown' if declarative else 'unusedconfig'
 boss.append({'configId':cid,'status':status,'statusScope':'639 cached layouts through confirmed AICampController.InitInfo creation gate','serializedActiveStarReferences':active,'declarativeCampReferences':declarative,'requiredByObservedInitGate':bool(active),'reason':'isBoss=true Star references this bossSkillId' if active else 'Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.' if declarative else 'No isBoss Star or CampInfo references in cached layouts. Do not require solely because table row exists.','config':c})
actions=[]
mapping={5:[1,2],7:[3,4],6:[5,6],8:[7,8],9:[9,10]}
for camp,ids in mapping.items():
 for slot,a in enumerate(ids,1):
  cfg=[b['configId'] for b in boss if any(t['campId']==camp for t in b['serializedActiveStarReferences']) and b['config'][f'Skill{slot}'][1]>0]
  actions.append({'actionId':a,'campId':camp,'slot':slot,'implementation':'confirmed' if a in [1,2,5,6] else 'confirmed-noop','reachability':'activebranch' if cfg else 'unknown','activeConfigIds':cfg,'ruleIds':['boss-action-dispatch']+(['boss-single-strikes'] if a in [1,6] else ['boss-projectile-volley'] if a==2 else ['boss-rain-center','boss-rain-area'] if a==5 else []),'note':'BossUseSkill first stops prior coroutine even for unimplemented values.'})
report={'target':e['target'],'classificationDefinitions':{'activebranch':'Concrete implementation and binding are present; does not claim runtime observed.','confirmed':'Static evidence establishes the described kernel or config binding.','unknown':'Evidence insufficient to claim reachability/timing, never silently normalized.','unusedconfig':'No reference in explicitly scanned serialized scope; not a universal dead-code proof.'},'skillCount':len(skills),'skills':skills,'boss':{'scannedLayoutCount':len(levels),'serializedActiveStarCount':sum(map(len,stars.values())),'creationGateRule':'boss-schedule-creation','configs':boss,'actions':actions,'specialModePatchStatus':'unknown','ordinaryLoadPatchObservation':'Sibling level_flow_evidence inspected ordinary LevelControl creation f11599 and observed no isBoss/bossSkillId rewrite; not a proof for special modes.'},'towerBuff':{'status':'unknown','requiredByConfirmedCreationSource':False,'fieldsAndStacking':'confirmed','constructorDirectCaller':'none identified in current static scan','initialization':'Tower.InitBuff and BuffControl.Updata are nop. Tower.GetBuff generic specialization reads dictionary136; no creation in that method.','policy':'Keep confirmed empty-container default; do not invent active buff rates, X2/X3 chances, or new buff sources. If a runtime source is discovered, use confirmed stacking rules.','ruleIds':['tower-buff-fields','buff-lifecycle-stubs','tower-buff-read-only']},'bastion':{'status':'unknown','standaloneMechanicsClassFound':False,'note':'CampConfig.BasitionMaterial is a material field; it is insufficient evidence of an extra mechanic. ArrowTower and Boss kernels are separately confirmed.'},'tableHashes':{n:hashlib.sha256((OUT/'tables'/f'{n}.json').read_bytes()).hexdigest() for n in ['CommanderConfig','AllSkillConfig','SkillConfig','BossConfig']},'validation':{'all18SkillsHaveConfig':all(x['levels'] for x in skills),'all18SkillsHaveRule':all(x['ruleId'] in rules for x in skills),'allLevelFilesParsed':len(levels),'runtimeObserved':False}}
(OUT/'combat-skill-coverage.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
lines=['# 关内技能覆盖清单','','目标 wxcf1394487200e48f / 43；本文件是静态覆盖，不是运行验收。','', '|技能|角色|代表配置 duration/data1/data2/data3|分支|机制|残余未知|','|---|---|---|---|---|---|']
for s in skills:
 c=s['representativeConfig'];v='/'.join(str(c[k]) for k in ['duration','data1','data2','data3'])
 lines.append(f"|{s['skillId']} {s['name']}|{s['commanderId']}|{c['id']}: {v}|activebranch|confirmed|{'; '.join(s['remainingUnknowns']) or '运行验证'}|")
lines+=['','全部18技能都有具体代码与配置绑定。角色5/6 usePercent=0不能证明玩家不能使用，因此不据此删去技能13–18。','','## Boss 实际配置覆盖','',f'扫描 {len(levels)} 份布局；只有 {sum(map(len,stars.values()))} 个 Star.isBoss=true（配置 {sorted(stars)}）。CampInfo.BossActionId 不等于已激活调度。','']
for b in boss:lines.append(f"- Config {b['configId']}: **{b['status']}**；Star引用{len(b['serializedActiveStarReferences'])}，CampInfo引用{len(b['declarativeCampReferences'])}。{b['reason']}")
lines+=['','Boss action1/2/5/6有实现；3/4/7–10在当前BossUseSkill中只停止前一协程，不执行新动作。特殊模式是否动态改写isBoss/bossSkillId仍为unknown。','','## TowerBuff 与 Bastion','','TowerBuff字段/叠加公式confirmed，但创建源unknown，当前初始化方法为空、GetBuff只读字典。不得凭字段存在加上任意概率或倍率。Bastion仅发现材质字段，不能臆造独立机制。','','详细规则、精确二进制偏移和函数引用见 combat-evidence.json；本文件每个技能内已复制证据引用。']
(OUT/'combat-skill-coverage.md').write_text('\n'.join(lines)+'\n',encoding='utf8')
print(json.dumps({'skills':len(skills),'levels':len(levels),'activeBossStars':sum(map(len,stars.values())),'bossStatuses':{k:sum(x['status']==k for x in boss) for k in ['activebranch','unknown','unusedconfig']}}))
