"""Verify activity business schemas/roster and retain only used original method bodies."""
from pathlib import Path
import argparse,hashlib,json,struct,shutil
parser=argparse.ArgumentParser();parser.add_argument('validation_root',type=Path);validation=parser.parse_args().validation_root.resolve()
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py';c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';source=validation/'analysis/targets/wxcf1394487200e48f/43/generated/outgame'
owners=[4562,4563,4573,4575,4577,4579,4581,4583,4672]
getters={i for i,m in enumerate(c['md']) if m[1] in owners and c['ms'](m[0])=='get_UniqueID'};assert len(getters)==9
selected=getters|{35128,35372,35373,35374,35395,35396,35397,35602,35603,35604,35605,35606,35607}
index=json.loads((out/'method-map.json').read_text());known={r['metadata'] for r in index};added=[]
for row in json.loads((source/'method-map.json').read_text()):
    if row['metadata'] in selected and row['metadata'] not in known:
        shutil.copy2(source/row['path'],out/row['path']);index.append(row);known.add(row['metadata']);added.append(row['metadata'])
if added:
    path=out/'method-map.json';raw=path.read_bytes();text=json.dumps(index,ensure_ascii=False,indent=2)+('\n' if raw.endswith(b'\n') else '')
    path.write_bytes((text.replace('\n','\r\n') if b'\r\n' in raw else text).encode())
selected.update([35129,35130,35132,35133,35136,35137,35140,35141,35143,35144,35146,35147,35148,35150,35151,35152,35153,35154])
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest()) for r in index if r['metadata'] in selected]
fields=[];schemas={};schema_text=(helper.parent.parent/'UnityProject/Assets/AreaBattle/Scripts/OutgameActivityBusinessSchemas.cs').read_text()
for owner in owners+[4625,4626]:
    schema=[]
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ);code=(c['u'](ptr+4)>>16)&255;data=c['u'](ptr)
        row=dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=code,typeData=data);fields.append(row)
        if owner not in owners:continue
        if code in (8,10,14):cs={8:'int',10:'long',14:'string'}[code]
        elif code==18:assert data==3490;cs='Lang'
        elif code==21:assert data==3763980;cs='List<ListArrayInt>'
        elif code==29:cs={8:'int[]',14:'string[]'}[(c['u'](data+4)>>16)&255]
        else:raise ValueError(row)
        assert 'public '+cs+' '+row['name']+';' in schema_text
        schema.append(dict(name=row['name'],csharpType=cs))
    if schema:schemas[c['ms'](c['ts'][owner][0])]=schema
roster=[dict(typeIndex=i,name=c['ms'](t[0])) for i,t in enumerate(c['ts']) if i!=4631 and 4631 in c['chain'](i)]
assert [r['typeIndex'] for r in roster]==[4668,4675,4713]
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,schemas=schemas,customManagerRoster=roster,
    findings=[
        'All nine business DTO schemas are generated from metadata fields, with original names/Int64 and List<ListArrayInt>/Lang references. Getter bodies confirm boxed Int32 id (TaskGroup uses Id). Shared PubAchievementAccConfig has only id; rewards present in project JSON are deliberately not invented into shared schema. ConditionPriority4625 is a value type with private reserved field zeroed plus contentType/content; constructor35128 verified.',
        'ActivityConfigMgr constructor35144 initializes original dictionaries/lists; LivenessList stays null until TaskConfigMgr.OnInit. All remaining business getters/groups restored: task/task-group misses silent; most rows log only TryGetValue failure, novice task/acc row getters also log stored-null; liveness missing group warns, novice missing group errors, both return null without creating groups.',
        'BuildTaskGroups35130 and BuildTasks35143 append shared row references into existing lists, creating outer index via setter. BuildLiveness35133 and BuildNoviceAcc35152 add by row.id to inner dictionaries, duplicate ID throws. BuildNoviceTasks35147 first adds all task rows, then adds empty groups for every NT activity id; no empty accumulator group pass. Failures preserve prefix and block later passes.',
        'Full original metadata inheritance roster for ActivityConfigCustomMgr is NoviceTaskConfigMgr4668, TaskConfigMgr4675, AchievementConfigMgr4713; explicit runtime composition uses this exact order and complete set, with required current-owner/acquisition/report dependencies.',
        'Novice OnInit35373 captures binary flag once, reads novice tasks then accumulator then activity, builds task/acc groups, appends each accumulator to current owner list, then callback. Dispose35372 clears task rows, accumulator rows, task groups, accumulator groups, activities, accumulator list in order, resolving current owner each time.',
        'Task OnInit35396 captures binary mode once, reads task/liveness/group/activity rows, replaces LivenessList from current values, then builds group index/liveness index/task index and callbacks. Dispose35395 clears task group rows/tasks/liveness/list/group-by-activity/task activities/liveness-by-activity, deliberately retains TasksByGroup and its shared old-row references.',
        'Achievement OnInit35603 always uses ConfigRead Unity JSON for achievement then accumulator tables, never queries binary flag. Groups by row.type then value ConditionPriority(contentType,content), appends shared rows, sorts every existing group by signed priority comparator35607 (-1/0/1 without subtraction overflow), appends accumulator values and callbacks. Dispose35602 clears four original containers. No caught parse/group failures or automatic reset added.',
        'Original resource verification loads49 novice tasks/8 rewards/1 NT activity,8 tasks/3 liveness/1 group/1 task activity,127 achievements/3 accumulator rows, then9 common activities/settings. Real runtime reaches Completed==Expected==5 and releases/reloads; source retained task index appends8 new rows to8 old rows after restart of config owner lifecycle.',
        'Failure/reentry checks cover repeated read without dispose (six custom instances/Expected11, duplicate novice grouping stops completion), acquisition failure after first task table, current-owner replacement between reads, null dictionary entries/missing IDs, inner Add duplicate IDs, group mutation, signed64 values and retained nested reward aliases.'
    ],boundaries=[
        'This completes recovered configuration schema/query/group/custom-loader composition, not concrete activity gameplay or controller lifecycle binding. ActivityControl/SevenDay/Main and source business UI/rewards remain pending; controller registry remains18/38.',
        'Binary task/novice route uses a required generic acquisition interface; tests prove captured selection with explicit hosts, not a completed MemoryPack/platform service. Achievement always follows source Unity JSON route.',
        'No fresh native or Player in this configuration milestone. Existing activity state native10 remains prior evidence; no original-page audiovisual or full business acceptance claim.'
    ])
(out/'ACTIVITY_BUSINESS_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(added=len(added),indexedMethods=len(index),methods=len(methods),fields=len(fields),schemas=len(schemas),customManagers=len(roster))))
