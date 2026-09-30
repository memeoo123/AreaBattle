"""Register actual original scheduler execution without claiming original RNG equality."""
import json
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
oracle=read(G/'original-ai-timer-cases.json');checks=read(W/'analysis/unity-integrated-validation.json')
ids=['original-ai-timer-'+c['id'] for c in oracle['cases']]
assert len(ids)==9 and checks['passed'] and all(any(r['id']==i and r['result']=='pass' for r in checks['checks']) for i in ids)
cfg=next(r for r in read(G/'tables/AIConfig.json')['Datas'] if r['id']==99)
e=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='source-scheduler-boundaries-verified',oracle='generated/original-ai-timer-cases.json',checks=ids,
 sourceFunction=oracle['source'],AI99=dict(delayMs=cfg['DelayTime'],actionTimeCandidatesMs=cfg['ActionTime']),
 conclusion='Production scheduler agrees with all9 isolated original-WASM boundary cases; no evidence-backed timer correction needed. Do not force a0.15s offset from two visible AI responses.',
 limitations=oracle['limitations']+['Does not establish original pre-recording clock/RNG state or random calls consumed by initialization/skin selection.'])
write(G/'ai-timer-oracle-evidence.json',e)
p=G/'golden-cases.json';v=read(p);v['originalAITimingOracle']=e;write(p,v)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalAITimingVerification']=e;write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='original-ai-timer-oracle9',status='bounded-original-code-checks-pass',atUtc=e['atUtc'],evidence=['generated/ai-timer-oracle-evidence.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';v=read(p);v['originalAITimingOracle']=e;write(p,v)
marker='\n\n## 原始 AI 计时函数直接执行验证'
note=marker+f'''\n\n直接执行未改动的原始 WASM `AICamp.Updata`（函数21038，offset9492148，149字节），通过独立内存与明确的配置/动作桩隔离运行；不执行原始初始化、网络或账号代码。配置桩只返回指定间隔，动作桩只计数，因此此验证覆盖计时逻辑，不覆盖随机选择和AI策略。\n\n9组原始结果与生产 BattleSimulation 调度器逐项一致：正延迟、延迟跨零、延迟恰好归零、行动计时恰好归零、首个运行帧、负延迟后的帧、剩余计时、大帧只执行一次、负计时遇到零dt。原始边界是延迟>0则扣减后直接返回，行动计时<0才执行，保留剩余量，每帧最多一次。\n\n第871关 AI99 的原始启动延迟为400毫秒，行动间隔从[200,400]毫秒随机取值，不能理解成固定0.2秒。这排除了已测试的计时公式/边界错误，尚不能解释原片与固定种子回放的初始相位差。生产AI逻辑未被人为提前0.15秒。\n\n目前 {len(checks['checks'])} 项集成检查通过，包含这9项新加入的原始函数对照。完整录像仍未匹配；后续应检查初始会话状态、随机调用顺序及火球目标，不应凭录像误差调整原始计时公式。\n'''
for p in [W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'analysis/VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print('Original AI scheduler9 cases agree;',len(checks['checks']),'integrated checks. No production timer change.')
