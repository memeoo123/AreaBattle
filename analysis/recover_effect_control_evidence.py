"""Audit original EffectControl ownership, NormalPool creation and sequential reward effects."""
from pathlib import Path
import hashlib,json,struct
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';index=json.loads((out/'method-map.json').read_text())
selected=set(range(31209,31244))|{28679,28680,28681,28682,28683,28684,28685,32715,32736,33920}
methods=[dict(r,sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest())for r in index if r['metadata']in selected]
assert len(methods)==len(selected),(len(methods),len(selected),selected-{r['metadata']for r in methods})
fields=[]
for owner in [4052,4053,4054,4058,3667]:
 for k in range(c['ts'][owner][18]):
  name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
  fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
usages=[]
for address in [3931300,4092760,3922532,4002680,3945316,4027836,3953376,3953576,3928508]:
 v=c['u'](address);kind=v>>29;ix=(v&0x1ffffffe)>>1;r=dict(address=address,kind=kind,index=ix)
 if kind==5:
  length,off=struct.unpack_from('<II',c['b'],c['pairs'][0][0]+8*ix);r['literal']=c['b'][c['pairs'][1][0]+off:c['pairs'][1][0]+off+length].decode('utf8')
 elif kind==6:r['spec']=list(struct.unpack_from('<iii',c['mem'],483008+12*ix))
 elif kind==3:r['method']=dict(metadata=ix,name=c['ms'](c['md'][ix][0]),owner=c['md'][ix][1])
 usages.append(r)
report=dict(status='source-audited-effect-control-ownership-and-sequence',metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,usages=usages,
 findings=[
 'EffectControl4058 OnInit31213 replaces coroutine/time dictionaries first. Only when held NormalPool is managed-null does it assign a newly constructed NormalPool and call CreateObjectPool(moneyPool,300,5f,null,false). Source fourth option is Transform root, not boolean. Reinitialization retains pool, pending cleanup ids, NextCheck timestamp, both WaitForSecondsRealtime instances and UIMessage name.',
 'NormalPool28682 resolves IObjectPoolManager; absent manager returns without mutating any fields. With manager it stores name/cacheResources/root before creating single-spawn pool, then assigns AutoReleaseInterval, Capacity, ExpireTime, Priority0 in order. Only after successful settings it creates legacy resource dictionary then modern handle dictionary. Factory/setter failures leave source partial initialization; no synthetic root created. Existing injected-pool constructor retained as explicit compatibility composition.',
 'NormalPool28679 destroys by stored source pool-name field24, not by dereferencing held pool.Name. Then destroys Unity-alive root, releases modern cached handles+UnloadUnusedAssets or unloads legacy resources, and clears only legacy dictionary. Noncached mode selected by EffectControl has no cache-handle ownership; final fixture releases externally retained test handles separately, not via invented production cleanup.',
 'EffectControl31215 calls DestoryPool before clearing ControlBase<EffectControl> singleton. Failure prevents clear; success retains pool/coroutine/time/pending fields. Stale instance disposal clears current slot and repeated disposal repeats resource calls, matching source.',
 '31214/31216 play voice before resolving current UIControl target Text. Money uses2019, diamond voice comes from original static provider endpoint.31217 Unity-null Text returns-1 before id, root, award or coroutine. Otherwise shared id is allocated then Unity-null root resolves UIMessage; optional ToolChange happens before UpdateManager.StartCoroutine, dictionary.Add and Time.time read. Save failure may leave economic mutation but no animation registration. Audio/config/target providers remain required.',
 'Existing recovered31209/31219/31221/31223 cleanup is now controller-owned and driven through LogicModule.Updata. Pending duplicate ids, kill-complete reentrancy, first-failure propagation, delayed timeout enqueue and additive NextCheck+=Time.time+1 remain unchanged. Initialize only recreates coroutine/time maps; stale pending ids can raise KeyNotFound after reinit, as original.',
 'Nested4052/31224 sequence owns original ids/counts array references, index, position and callbacks. Empty/exhausted array resets index0 before completed callback. Nonempty invokes Func<int,Vector3> with current index then Nullable<Vector3>.Value; absent function throws rather than using default position. Reads ids/current index AFTER callback, resolves GoodsType, reads counts, dispatches gold(kind1)/diamond(kind2) with rootnull, applyInventoryfalse, completionPlay, updateDisplayedValuetrue, then increments index. Other kinds advance index once but never schedule continuation. Callback or array/dispatch failure precedes index increment.',
 'Actual FlyToolAnimation/Collection/IDs/Scatter/tween native implementations are reused. Controller owns shared realtime waits by retaining one animation object for lifetime. Native binding supplies existing tween runner and actual UpdateManager coroutines; standalone older FlyRuntime remains available to prior callers and tests.',
 'Source UIControl32715/32736 lazily retry cached Unity-null Text when TopInfo managed reference exists, reading original fields132/152. OutgameUiControl now exposes those recovered getters and retains caches on source disposal. TopInfo original serialized outlets provide txt_goldNum/txt_DiamondNum. BindUi resolves current UI controller each request; native uses real UIControl/TopInfo getters and recovered UIMessage root. Complete TopInfo account/buttons lifecycle and full production composition remain pending.',
 'Native account evidence distinguishes ItemManager award path from direct ToolChange: task reward50 reaches both records, then FlyMoney7(applyInventory=true) updates LocalData to57 while ItemManager stays50; diamonds direct path3 likewise uses local inventory. Sequence has applyInventory=false and changes neither balance. Independent file restart asserts these exact source values; an initial test incorrectly expecting both gold records57 was retained and corrected without changing production rules.',
 'Pool-manager acquisition and original asset acquisition are explicit local validation hosts, exercising actual restored OutgameObjectPool and real OutgameAssetProvider/handles/original assets. Full original ObjectPoolManager module startup/resource catalog/network/SDK, production Main/account/platform/all business and final Player/original audiovisual equivalence are not claimed.'
 ])
p=out/'EFFECT_CONTROL_SOURCE_EVIDENCE.json';p.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),usages=len(usages),indexedMethods=len(index))))
