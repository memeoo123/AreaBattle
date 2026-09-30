"""Original ordinary-tower red/blue target markers, preserved as independent subtrees."""
from pathlib import Path
source=(Path(__file__).parent/'asset_embedded_effects_prepare.py').read_text(encoding='utf8')
source=source.replace("O=S/'prepared-embedded'","O=S/'prepared-markers'")
source=source.replace('(-2107951418625338322,832913686255133012,2117273487600732731)','(6112431770738899461,7197884350226120440)')
source=source.replace('source_resource=resource',"root_ids.append('CAB-cfe20b2df2a68585abca3f6dfada43e5:404227533793537550')\nsource_resource=resource")
source=source.replace("if o['type'] not in ('AnimatorController','AnimationClip'):return source_resource(ident)","if o['type'] not in ('AnimatorController','AnimationClip') or o['type']=='AnimationClip' and tree(o).get('m_Legacy',False):return source_resource(ident)")
source=source.replace("'MeshFilter','SpriteRenderer')","'MeshFilter','SpriteRenderer','Animation')")
exec(compile(source,'source_tower_markers','exec'))
evidence={
 'target':'wxcf1394487200e48f/43',
 'ordinaryMarkers':{'blue':'CAB-68a3634b9a99c5a2a10a36b772e4e945:6112431770738899461','red':'CAB-68a3634b9a99c5a2a10a36b772e4e945:7197884350226120440','parent':'Bastion/objEffectRoot, parent local TRS identity','modeConsumer':'Tower.Update f6993@0x289f16..0x289fb1 reads SkillControl.field100. mode0 hides both; mode1 enables blue for player camp; mode2 enables red for nonplayer camp; other branches do not actively hide. SkillControl down f15938 targetType0→1,2→2,8→3; up f15937 clears0.','sourceAnimation':'Original Animation default/autoplay and hdzd_saojian_ani preserved; no inferred tween replacement.'},
 'showMyCampEffect':{'function':'LevelControl.ShowMyCampEffect f4973, generated/guide-presentation-tween-helpers.json','filter':'Enumerates LevelControl+56 dictionary; value.Camp(+20)==PlayerCampID and value.Transform(+16) is a nonnull Unity Object. No additional tower.Active filter appears in this caller.','position':'Transform.position','effect':107,'config':{'res':'hdzd_effect_yindao_03','duration':3,'type':3},'parent':None,'sortingLayer':'LayerMask.NameToLayer("Default") f7116','effectOrder':-20,'showSignature':{'class':'GFRunning.EffectModule','method':'Show','token':'0x06001166','function':2596,'parameterNames':['templId','pos','parent','effectSortingLayer','effectOrder']},'sourceRoot':'CAB-cfe20b2df2a68585abca3f6dfada43e5:404227533793537550','sourceRootTRS':'position(0,0,-1.5), identity rotation, scale1; passed world position overrides prefab position; caller does not override rotation/scale.','guideTriggers':[1,4,5,7,8,9,10,11],'guideCaller':'GuideUI.OnOK f11888@0x58bc61..0x58bd0b'},
 'manifest':(O/'native-import.json').relative_to(R).as_posix(),
 'runtimeKeys':['Recovered/SkillEffects/'+p['originalName']for p in prefabs],
 'importer':'RecoveredSkillEffectImporter.Import third prepared-markers manifest',
 'status':'prepared; Unity import and consumer runtime validation pending root unified build'}
(R/'generated/tower-marker-evidence.json').write_text(json.dumps(evidence,ensure_ascii=False,indent=2),encoding='utf8')
