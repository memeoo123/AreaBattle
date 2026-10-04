"""Record source4502, concrete commander sum and existing startup ordering evidence."""
from pathlib import Path
import hashlib,json,struct
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';index=json.loads((out/'method-map.json').read_text())
selected=set(range(34354,34366))|{30989,31020,31743,31495}|set(range(34119,34134))|{34142,34154,34161}
methods=[dict(r,sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected]
assert {r['metadata'] for r in methods}==selected
b,mem,ts,md,ms,u,pairs=[c[x] for x in ('b','mem','ts','md','ms','u','pairs')];fields=[]
for owner in (4502,4643,3969,3907):
 for k in range(ts[owner][18]):
  off=u(u(3823136+4*owner)+4*k)
  if owner==3907 and off!=260:continue
  name,typ,token=struct.unpack_from('<3i',b,pairs[11][0]+12*(ts[owner][8]+k));ptr=u(200288+4*typ)
  fields.append(dict(owner=owner,name=ms(name),offset=off,typeCode=(u(ptr+4)>>16)&255,typeData=u(ptr),attributes=u(ptr+4)&65535))
usages=[]
for address in (3988816,3988864,3953440,3953344,3916956,3953548,3989352,3975340,3975344,3955468,3965688,3965692,3970252):
 encoded=u(address);kind=encoded>>29;idx=(encoded&0x1ffffffe)>>1;row=dict(address=address,kind=kind)
 if kind==6:
  spec=struct.unpack_from('<3i',mem,483008+12*idx);method=md[spec[0]];row.update(spec=list(spec),sourceClass=ms(ts[method[1]][0]),method=ms(method[0]))
 else:row['index']=idx
 usages.append(row)
report=dict(metadataSha256=hashlib.sha256(b).hexdigest(),memorySha256=hashlib.sha256(mem).hexdigest(),methods=methods,fields=fields,usages=usages,
 findings=[
  'SevendayActivityControl4502 owns child4698 at8, cached Int64 start16/end24. OnInit34358 and Updata34361 are empty; OnDispose34357 clears generic singleton slot only and leaves old fields intact. Constructor only generic-base metadata touch. Core registry binding is source4502, original controller order35.',
  'EnterGameInit34359 resolves ActivityControl.GetActivity<LimitTimeTaskActivity>(1301001), nullable parent.GetChildActivity<ChildLimitTimeTaskActivity>(1301), assigns child and returns if null. Original config registers1301001 as a child of1300001, so activity lookup may resolve through ChildActivities. Dates are always recomputed when child exists, before statistics seeding.',
  'Date calculation reads low signed Int32 of first OverCondition struct target(value offset8 in array first element), subtracts1 unchecked, converts child.Data.LaunchTimeStamp to source TimeHelper date, constructs unspecified midnight for its Y/M/D, converts back to timestamp. End=start+sign-extended Int32(days*86400000); multiplication overflows before Int64 addition. It does not use module.ext.launchTime or task-group maximum.',
  'Seeding order: capture current legacy ConfigMgr.statisticEventConfig.HaveSkinNum(field56), count current SkinManager unlocked soldier skins, SetEventCount; read current LevelControl.CurLevel, subtract1 unchecked then clamp to nonnegative and set10015; reread current config CommanderUpgradeTotal(field84), current CommanderControl.GetCommanderTotalLv, set event. Callbacks can change later keys/levels. Failure preserves published dates and prior seeds.',
  'CommanderControl30989 delegates to held manager; CommanderManager31020 enumerates live held dictionary and adds every CommanderData.level field16 with unchecked Int32 addition. No unlock filter, count substitution or clamp. Existing SkinCatalog.UnlockedSoldierCount and current-level owner endpoint are used by concrete seven-day services.',
  'IsInActivity34355 recomputes cached dates only when child nonnull and start0, then always reads GameValue10000 and tests strictly start<now<end. IsUnlock requires nonnull child first. GetActDate34365 returns start for true and end for false; it performs no refresh. Direct IsInActivity can still use old bounds after child removed.',
  'IsHaveAnyRed34362 gates on IsUnlock then bitwise-ORs both IsAnyDayHaveRed and IsAccHaveRed, so the latter always executes even when day red true. Day range is1 through lowInt32(firstOverTarget)-1, not dictionary keys, and each query delegates ExitRewardWaitGet. Missing days retain underlying failures. Acc query captures display list once, rereads current child ext.AccProgress each iteration, compares reward cached config.accValue then rereads list item state; exact state0 only, distinct from reward.CanComplete.',
  'ProcedurePreLoad original order remains network module, arena provider registration, statistics initialization. Concrete host now uses recovered StatisticsRuntime/Expansion and ActivityRuntime.Init(null), with account/network/repair flags supplied by required app endpoints. Existing native WaitUntil tests flag64 then65; after both, LoginComplete log/legacy repair/FSM transition. Arena provider rereads current legacy configured key at invocation.',
  'StartupEntry scene callback retains source34161 order: load-game-screen event, enter-home report, LevelRank initialization, concrete Sevenday.EnterGameInit via registry, play state1, loading close, interactive report. New constructor binds concrete controller while existing explicit application-host overload remains supported. It does not call seven-day logic during controller OnInit or before scene completion.'
 ],boundaries=['Binding4502 increases implemented lifecycle count to19/38; it does not establish complete production Main or all business completion.','Native startup uses actual pre-load FSM/WaitUntil/statistics/activity/task graph and native save with explicit pending server response/account flags/scene endpoint. It does not assert real network authentication, successful platform reports, original task UI, full scene assets or Player/audiovisual acceptance.'])
p=out/'SEVENDAY_STARTUP_SOURCE_EVIDENCE.json';p.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),index=len(index))))
