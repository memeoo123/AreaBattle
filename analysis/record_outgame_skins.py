from pathlib import Path
import json,subprocess,struct
from datetime import datetime,timezone
w=Path(__file__).resolve().parent.parent;t=w/'analysis/targets/wxcf1394487200e48f/43';o=t/'generated/outgame'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
r=read(w/'analysis/unity-integrated-validation.json');v=read(w/'analysis/outgame-skin-validation.json');assert r['passed'] and v['passed'] and len(r['checks'])==330
b=(t/'work/webdata/Il2CppData/Metadata/global-metadata.dat').read_bytes();pairs=[struct.unpack_from('<II',b,8+8*i) for i in range(21)]
def string(i):
 a=pairs[2][0]+i;return b[a:b.index(0,a)].decode('utf8')
fields=[]
for ix in range(4144,4151):
 row=struct.unpack_from('<16i8H2I',b,pairs[19][0]+ix*88);fields.append({'typeIndex':ix,'name':string(row[0]),'fields':[string(struct.unpack_from('<i',b,pairs[11][0]+fi*12)[0]) for fi in range(row[8],row[8]+row[18])]})
write(o/'skin-record-fields.json',fields)
p=t/'OUTGAME_RESTORE_STATE.json';s=read(p);s['lastUpdatedAtUtc']=datetime.now(timezone.utc).isoformat();s['currentStage']='skin-records-and-legacy-migration';s['validation'].update(integratedChecksPassed=330,skinRecordChecksPassed=4)
s['nextActions']=['Finish skin acquisition/equip mutations, original save projection and source ShopUI binding.','Bind startup migrations and providers to real account/UI lifecycle with concrete effects consumers.','Complete all remaining outgame subsystems and verify a new player build/end-to-end flows.']
for sub in s['subsystems']:
 if sub['id']=='skins-cosmetics':sub.update(evidenceStatus='SkinManager record init and old ownership migration confirmed; mutation/UI flow pending',implementation='Source record/defaults and old payload migration implemented; page/actions pending',validation='4 skin record/legacy/restart checks pass; full subsystem incomplete')
write(p,s)
p=o/'OUTGAME_RESTORE_SPEC.json';s=read(p);gate={'implementationReady':True,'status':'confirmed','scope':'Source skin record loading/default metadata and legacy ownership flags; ordering/acquisition/equip/save projection and page/startup excluded.','rules':[
{'rule':'Missing/empty/decoded-null manager data creates four used records types1..4 selecting first matching lockState2 (type4 scene table, others soldier table by skinType). Existing {} does not auto-repair empty used list.','source':['disassembly/Type4150-31729.txt','disassembly/Type4150-31734.txt','disassembly/Type4145-31755.txt']},
{'rule':'Index held valid skin records; preserve u and set isNew from newSkins membership. Missing configured skins append with u=(lockState==2). Refresh skinType/sortNo/special/castType from ISkin config. Keep invalid saved records outside lookup.','source':['disassembly/Type4150-31737.txt','disassembly/Type4150-31738.txt','disassembly/Type4150-31740.txt','disassembly/Type4150-31741.txt','skin-record-fields.json']},
{'rule':'OldData.list_playerskin(skinId,isUnlock) overwrites matching soldier u; sceneMapDatas(sceneId,isUnlock) overwrites matching scene u. Unknown IDs ignored. False values relock; no equip normalization/new-flag/save effect.','source':'disassembly/Type4150-31730.txt'}],
'implementation':['OutgameSkinCatalog.cs','OutgameProfileStore.cs'],'limitations':['Original ordered lookup and save projection pending','No acquisition/equip/ShopUI or live login claim']};s['subsystemGates']['skin-record-initialization-and-legacy']=gate;write(p,s)
p=o/'ACCOUNT_STARTUP_AUDIT.json';s=read(p);s['sourceMethodsIndexed']=688;s['confirmed'] += [{'fact':x['rule'],'source':x['source']} for x in gate['rules']];write(p,s)
p=o/'golden-cases.json';s=read(p);ids={c['id'] for c in s['cases']};s['cases'] += [dict(c,sourceContract='SkinManager.UpdateDataCallBack/InitSoliderSkinData/InitSceneMapData/DealOldData') for c in v['checks'] if c['id'] not in ids];write(p,s)
p=w/'analysis/VALIDATION_MANIFEST.json';old=read(p);subprocess.run(['python',str(w/'analysis/record_validation_manifest.py')],check=True);s=read(p)
for k,val in old.items():
 if k not in s:s[k]=val
s.setdefault('validationHistory',[]).append(old.get('latestValidation',{}));s['latestValidation']={'scope':'Source skin defaults, held record initialization, old ownership migration and restart','checksPassed':330,'outgameChecksPassed':63,'freshPlayerBuild':False,'freshPlayerSmoke':False,'notClaimed':'Skin acquisition/equip/UI, full account startup or playable lobby'};write(p,s)
note='''

## 皮肤记录与旧数据迁移（330项）
新增OutgameSkinCatalog与OutgameSkinState，提供原SkinManager载入、缺失记录补齐与DealOldData。缺省manager数据初始装备按types1..4从lockState2选择：100/200/300/1；存在但空的{}不擅自补装备。持有的u保留、isNew取newSkins成员关系，缺失配置记录才按lockState2赋初始解锁，config刷新type/sort/special/castType，未知存档记录保留但不进有效索引。元数据SkinManagerData/SkinData/OldData等字段证据写入skin-record-fields.json。
旧list_playerskin(skinId,isUnlock)/sceneMapDatas(sceneId,isUnlock)只覆盖对应皮肤解锁标记，包含true→false，未知ID忽略，不改装备或新获得标记，不额外保存。原方法索引688。
4新用例覆盖缺省与已有空数据差异、持有记录与配置刷新、旧标记双向覆盖与装备不归一化、隔离磁盘重启。Unity退出0，330集成检查通过（原267+关外63）。完整皮肤排序/获取/装备/原save投影、ShopUI和账户启动仍待完成，未构建新玩家，完整目标active/incomplete。
'''
for p in [w/'AREA_BATTLE_HANDOFF.md',w/'RESTORE_PROGRESS.md',w/'analysis/VALIDATION_REPORT.md',t/'REVERSE_PROGRESS.md']:p.write_text(p.read_text(encoding='utf8')+note,encoding='utf8')
b=['python','C:/Users/jiachengwei/.codex/skills/wechat-minigame-reconstruction-orchestrator/scripts/orchestrate.py'];a=['--project-root',str(w),'--target','wxcf1394487200e48f/43']
for kind,p in [('unityProject',w/'UnityProject'),('validationManifest',w/'analysis/VALIDATION_MANIFEST.json'),('validationReport',w/'analysis/VALIDATION_REPORT.md')]:subprocess.run(b+['record-artifact']+a+['--kind',kind,'--path',str(p)],check=True,capture_output=True)
for name in ['unityCompile','outgameCommanderCore','outgameInventoryCore','outgameToolDispatchCore','outgameProfileStore','outgameMenuNavigation','outgameCommanderActions','outgameUiImport','outgameMenuView','outgameCommanderView','outgameLevelProgression','outgameOriginalLocalData','outgameOriginalCommanders','outgameSkinRecords']:
 subprocess.run(b+['set-check']+a+['--name',name,'--result','pass','--evidence','330 integrated checks passed including source skin records/legacy migration and isolated restart; full outgame incomplete.','--depends-on','unityProject','--depends-on','validationManifest'],check=True,capture_output=True)
print('Recorded 330 checks and skin initialization/migration source gate.')
