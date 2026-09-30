"""Complete active skill audio calls, based on local annotated WASM and AudioConfig."""
import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43';OUT=ROOT/'generated'
def read(p):return json.loads((OUT/p).read_text(encoding='utf8'))
def ref(typ,token,offset):
 p=f'generated/combat-presentation-disassembly/{typ}-{token}.txt';assert(ROOT/p).exists();return {'path':p,'offset':offset}
rows=[]
def add(skill,ids,trigger,typ,token,offset,route='player-voice',hook=None,**kw):
 rows.append({'skillId':skill,'audioIds':ids,'mode':1,'route':route,'trigger':trigger,'source':ref(typ,token,offset),'productionHook':hook,**kw})
add(1,[2016],'Execute after marking affected towers, before clearing enemy soldiers','Type4166',100665106,'0064addf',hook='audio-play already emitted')
add(1,[2018],'SkillEnd only if at least one active different-camp tower was still state1 and changed to0; once total, not per tower','Type4166',100665107,'0064a9f8..0064aa25 / 0064aad8',hook='audio-play already emitted with count>0 condition')
add(2,[3121],'Each fireball pooled-object launch callback after object SetActive(true)','Type4167',100665113,'006b4c07',hook='audio-play after projectile-spawn; source asset readiness may delay actual playback')
add(2,[2021,3122],'Arrival after target score/effect: current friendly=>2021; otherwise3122','Type4168',100665115,'00706707 / 00706845 / 00706856',hook='audio-play selected in projectile arrival callback')
add(3,[4401],'Execute only when selected target44 nonnull, before effect413','Type4171',100665124,'006e7577..006e7583',hook='audio-play before effect-show')
add(4,[2042],'Execute direct UIAudioManager.Play(1,[2042])','Type4173',100665128,'00707109..0070712d',route='ui-audio',hook='audio-play')
add(5,[2043],'Execute direct UIAudioManager.Play(1,[2043])','Type4174',100665132,'00705ede..00705f02',route='ui-audio',hook='audio-play')
add(6,[4501],'Execute only if target44 nonnull, after heal and target reset; fresh AI targetless cast is silent','Type4175',100665134,'006b2042..006b204e',hook='audio-play guarded by actual heal target')
for skill,aid,typ,token,off in [(7,2027,'Type4179',100665140,'00645732'),(8,2028,'Type4180',100665149,'006b0512'),(11,2031,'Type4189',100665184,'007088ff'),(13,2034,'Type4193',100665199,'006b1c26'),(14,2035,'Type4195',100665206,'0070a13d'),(16,2038,'Type4200',100665224,'00707a7b'),(17,2039,'Type4202',100665228,'0069d7ec')]:
 add(skill,[aid],'Execute once per accepted invocation',typ,token,off,hook='audio-play')
add(9,[2029],'End of Execute, direct UIAudioManager.Play(1,[2029]); not per draining projectile','Type4182',100665152,'00ea6e10..00ea6e22',route='ui-audio',hook='audio-play after StartDrain')
add(10,[2030],'Rain visual coroutine initial state before the first pair; once per coroutine invocation','Type4186',100665174,'006b1d86..006b1d92',hook='audio-play before StartArrowRain')
add(12,[2026],'OnDragBeginHandle; independent of whether player later completes or cancels the cast','Type4191',100665192,'0079882a..00798836',hook='View HandleSkillInput drag begin must play2026; do not trigger on CastSkill or AI Execute')
add(12,[2032,2033],'After WaitForSeconds0.8 and Show419: two consecutive mode1 PlayerVoice requests,2032 then2033, no extra delay','Type4190',100665194,'00512f25 / 00512f84..00512ff4',hook='two audio-play events after delayed419')
add(15,[2036,2037],'Selected friendly=>2036 before532; enemy=>2037 before531. One call before starting serial score job, not one per +/-1 tick','Type4196',100665216,'00540b8d..00540b9a / 00540d61..00540d6e',hook='audio-play before once-per-cast effect')
add(18,[3121],'Pooled bottle callback after SetActive(true), before Bullet.Init. Not2039 (which belongs to17)','Type4203',100665248,'00645ff4..00646003',hook='audio-play after projectile-spawn')
add(18,[2040],'Bottle arrival if bottle object still exists, before2041 and ground activation','Type4204',100665251,'006b2099..006b20a8',hook='audio-play after projectile-arrival')
add(18,[2041],'Same bottle arrival after2040: direct UIAudioManager.Play(1,[2041]); returned GUID array stored in skill field64','Type4204',100665251,'006b20b7..006b20e0',route='ui-audio',hook='audio-play with positive HandleId; adapter maps logical handle to playback instance',handle='Original stores returned GUID array; end/reset stops field64[0] on parallelNode8, not all2041 playbacks')
add(18,[2041],'Stop stored GUID at SkillEnd and Reset; stop invokes AudioFadeAction.OnStop, which fades out before disposal','Type4205',100665240,'006dcb1a..006dcb53',route='ui-audio',hook='audio-stop with same HandleId; Restart also emits stop before battle-reset',operation='stop',resetSource=ref('Type4205',100665243,'006dcc4b..006dcc84'))
ids=sorted({i for r in rows for i in r['audioIds']});cfg={r['id']:r for r in read('tables/AudioConfig.json')['Datas']};catalog=read('resource-catalog.json')['entries']
assets=[]
for i in ids:
 r=cfg[i];name=r['ResPath'].lower()+'.unity3d';matches=[x for x in catalog if x['name'].lower()==name]
 assets.append({'audioId':i,'config':r,'catalog':matches,'recoveredAudioClip':[p.relative_to(ROOT).as_posix() for p in (OUT/'unity-assets/AudioClip').glob('*') if p.name.lower().startswith(Path(r['ResPath']).stem.lower()+'__')]})
fade={'audioId':2041,'class':'AudioFadeAction','Ptype':3,'loop':False,'fadeDurationMilliseconds':1000,
 'initialize':'AudioSource.volume=0, time=StartTime(0), loop=false, Play; store clip.length then start fade-in.',
 'fadeIn':'Linear volume=baseVolume*(1000-remainingMilliseconds)/1000; baseVolume=(AudioConfig.Vol>0?Vol:1)*GetSettingVolume.2041 Vol1.',
 'automaticFadeOut':'With EndTime0, while isPlaying, when AudioSource.time >= clip.length-1 and not already fading, call OnStop. For positive EndTime, threshold EndTime-1 instead.',
 'explicitStop':'First OnStop marks fading flag60=true, captures current AudioSource.volume*GetSettingVolume into base48, cancels previous fade countdown and starts a new1000ms fade-out.',
 'fadeOut':'Linear volume=baseVolume*remainingMilliseconds/1000. At completion force volume0 then AudioSource.Stop, cleanup source, completion callbacks and disposal. Repeated OnStop while already fading invokes completion/disposal branch; do not restart fade.',
 'clock':'TimeModule.SetCountDownByMillisecond -> f10104 schedules at f10093()+1000. f10093 reads DateTime-based elapsed ticks*.0001 plus clock offset; independent of Unity.timeScale. Exact poll cadence is not original-runtime verified.',
 'sources':[{'path':'generated/combat-disassembly/Raw-6276.txt','offset':'0022519e..0022521c (Ptype3 creates typeusage3928512 AudioFadeAction)'},ref('Type3408',100667231,'00225061..00225064'),ref('Type3406',100667240,'00480fea..004810fd'),{'path':'generated/combat-disassembly/Raw-20593.txt','offset':'008cd5af..008cd63d'},{'path':'generated/combat-disassembly/Raw-20594.txt','offset':'008cd675..008cd708'},{'path':'generated/combat-disassembly/Raw-14985.txt','offset':'006f4f35..006f4f80'},{'path':'generated/combat-disassembly/Raw-14986.txt','offset':'006f5016..006f5060'},{'path':'generated/combat-disassembly/Raw-10093.txt','offset':'0049c2f8..0049c39e'}]}
report={'target':{'appid':'wxcf1394487200e48f','version':'43'},'status':'18skills all audited;25 unique configured IDs; no inferred filenames','events':rows,'assets':assets,'fade2041':fade,
 'routeRules':{'allModes':1,'mode1':'AudioParallel, overlapping independent instances; none of these skill callsites uses Single3.','player-voice':'Calls Extension.PlayerVoice, which checks AudioControl.IsSoundEffect first. No2012/2013 throttle applies to these IDs.','ui-audio':'Direct UIAudioManager.Play; bypasses Extension.PlayerVoice IsSoundEffect guard. Shared GetSettingVolume may mute audio; do not claim equivalent callsite gate.'},
 'auditCoverage':{'checked':'Execute, generated coroutine/delegate callbacks, SkillEnd, Reset and SkillBase callbacks across skill types4166..4205 plus SkillBase.','noAdditionalAudioFound':'Except1 conditional end2018 and18 explicit handle stop, no other direct PlayerVoice/UIAudioManager audio requests were found in skill end/arrival bodies.7/8/9/10/11/13/14/15/16/17 have Execute audio as listed; do not add arrival sounds based on asset names.','unknown':'Asset-load completion-to-audible-frame latency, countdown polling jitter and DSP runtime synchronization remain unverified.'},
 'unityEventApi':{'source':'UnityProject/Assets/AreaBattle/Scripts/BattleSkills.cs','event':'SkillVisual','kinds':['audio-play','audio-stop'],'fields':['SkillId','Camp','AudioId','VoiceMode','HandleId','AudioRoute'],'dragInput':'Skill12 DragBegin2026 belongs to View input hook. All other listed calls are now audio-play/stop events in BattleSkills; do not also play from effect-show or skill-start.'}}
(OUT/'skill-audio-contract.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
(OUT/'skill-audio-minimum-resources.json').write_text(json.dumps({'target':report['target'],'audioIds':ids,'assets':assets},ensure_ascii=False,indent=2),encoding='utf8')
presentation=read('skill-presentation-contract.json');presentation['audioContract']='generated/skill-audio-contract.json'
for s in presentation['skills']:
 s['audioIds']=sorted({i for r in rows if r['skillId']==s['skillId'] for i in r['audioIds']})
 if s['skillId']==18:s['notes']=[x.replace('Launch2039','Launch3121') for x in s.get('notes',[])]
(OUT/'skill-presentation-contract.json').write_text(json.dumps(presentation,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'skills':len({r['skillId']for r in rows}),'audioIds':len(ids),'calls':len(rows),'catalogMissing':[a['audioId']for a in assets if not a['catalog']]}))
