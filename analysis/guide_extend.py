"""Publish later original GuideConfig stages, exact localization and verified bytecode facts."""
import json
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
p=ROOT/'generated/guide-evidence.json';r=json.loads(p.read_text(encoding='utf8'))
cfg=json.loads((ROOT/'generated/tables/GuideConfig.json').read_text(encoding='utf8'))['Datas']
lang={x['id']:x['zh_cn'] for x in json.loads((ROOT/'generated/tables/LanguageConfig.json').read_text(encoding='utf8'))['Datas']}
r['configRows']=cfg;r['scope']='All nine main-table tutorial entries: levels 0,1,2,6,7,9,14,21,25; small-level index0'
keys={x[k] for x in cfg for k in ('title','explain','button') if x[k] and '{0}' not in x[k]}
keys|={k for k in lang if k.startswith(('GuideUI.tipAdd.','Commander.SkillName.','Commander.SkillDescribe.'))}
r['localization']={k:lang[k] for k in sorted(keys)}
def ev(fn):
 f=next(f for f in r['functions'] if f['function']==fn)
 return {k:f[k] for k in ('module','function','token','class','method','body')}
rules=[
 ('all-guide-entries','GuideConfig maps main levels0/1/2/6/7/9/14/21/25 to stages1/4/5/9/7/8/10/11/12 respectively; every row has guildsmallLv0. Stage7/8/9/10/11/12 nextGuildStage=0.',[12792]),
 ('later-tower-guides','Stage7 Defense and8 Attack acknowledge then immediately invoke inherited UI close (same slot15 as stages3/4). Stage12 Arrow maps to stage-action no-op, so remains until normal disposal, showing GuideUI.tipAdd.arrow. GuideLogicControl maps6 Max,7 Defense,8 Attack,9 Ice,10 Fire,11 Lightning,12 Arrow.',[11887,11891,11881,11882,11883]),
 ('skill-guide-prompt','For stages9..11, format title/explanation key with (stage-9)+3*SkillControl.mode-2, unless result==-1; format picture key with mode. SkillControl mode is +108. Do not always display ice/fire/lightning names: alternate commanders have different skills.',[7740]),
 ('skill-guide-hand','Stage9/10 show hand at SkillControl.items[0/1].GameObject.transform.position; scale to Vector3(1.2,1.2,1.2) over1s with15loops and completion deactivating hand GameObject. Stage11 starts at items[2] then calls GetEndPosInSkill, hides hand for zero endpoint else DOMove2s with15loops. GetEndPosInSkill walks active towers in list order: mode1 selects first nonplayer tower; mode2 selects first player tower; other modes returnzero. Endpoint is TowerCanvas GameObject world position minus Vector3.up. No automatic skill execution or forced battle-input lock in these stage actions.',[11887,11898]),
 ('skill-guide-exit','GuideUI subscribes Event static+36 UseIceOrUp,+40 UseFireOrDown,+44 UseLightOrRevert. Their callbacks only test current stage==9/10/11 respectively then EnterNextStage immediately. No payload, target, score, animation completion or delay condition. Wrong slot event leaves tutorial unchanged.',[19754,7739,19275,19279,19284,4406])]
ids={x[0] for x in rules};r['rules']=[x for x in r['rules'] if x['id'] not in ids]
for id,text,fns in rules:r['rules'].append(dict(id=id,status='confirmed-static',rule=text,evidence=[ev(f) for f in fns]))
r['laterEntryMap']={str(x['guildLv']):x['id'] for x in cfg if x['guildLv']>=0}
r['implementationGate']='all nine guide entry mechanics statically supported; live input/timing/visual acceptance not performed'
p.write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
md=ROOT/'generated/GUIDE_EVIDENCE.md'
s=md.read_text(encoding='utf8').split('\n# Later tutorial extension')[0]
s+='\n# Later tutorial extension\n\n'+r['scope']+'\n'
for id,text,_ in rules:s+='\n## '+id+'\n\n'+text+'\n'
md.write_text(s,encoding='utf8')
print(json.dumps({k:lang[k] for k in sorted(keys)},ensure_ascii=False,indent=2))

