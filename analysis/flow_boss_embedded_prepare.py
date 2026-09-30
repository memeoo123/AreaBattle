from pathlib import Path
s=Path('analysis/asset_embedded_effects_prepare.py').read_text(encoding='utf8')
s=s.replace("O=S/'prepared-embedded'","O=R/'generated/resource-snapshots/boss-entities-20260928/prepared-embedded'")
s=s.replace('(-2107951418625338322,832913686255133012,2117273487600732731)','(-7484581989977586475,-2700617283758054626,8217516532788768465,5543199552207237062,8825091481322354176,-5049064904441721189,-1609345259139015766)')
s=s.replace("and not clip['m_ConstantClip']['data']", "")
s=s.replace("assert (binding['attribute'],binding['customType']) in ((3,0),(4,4));props=[('m_LocalScale.' if binding['attribute']==3 else 'localEulerAnglesRaw.')+c for c in 'xyz'];typ='Transform'", "assert (binding['attribute'],binding['customType']) in ((1,0),(2,0),(3,0),(4,4));props=[{1:'m_LocalPosition.',2:'m_LocalRotation.',3:'m_LocalScale.',4:'localEulerAnglesRaw.'}[binding['attribute']]+c for c in ('xyzw' if binding['attribute']==2 else 'xyz')];typ='Transform'")
s=s.replace("for a,b in zip(keys,keys[1:]):", "if index>=clip['m_StreamedClip']['curveCount']:\n    value=clip['m_ConstantClip']['data'][index-clip['m_StreamedClip']['curveCount']]\n    keys=[{'time':t,'value':value,'coefficients':[0,0,0,value],'inTangent':0,'outTangent':0,'step':False} for t in (0.0,t['m_MuscleClip']['m_StopTime'])]\n   for a,b in zip(keys,keys[1:]):")
s=s.replace("len(curves)==clip['m_StreamedClip']['curveCount']", "len(curves)==clip['m_StreamedClip']['curveCount']+len(clip['m_ConstantClip']['data'])")
s=s.replace("'Animator','MeshRenderer'", "'Animator','SkinnedMeshRenderer','MeshRenderer'")
s=s.replace("'Animator','SkinnedMeshRenderer'", "'Animation','Animator','SkinnedMeshRenderer'")
s=s.replace("if o['type'] not in ('AnimatorController','AnimationClip'):return source_resource(ident)", "if o['type'] not in ('AnimatorController','AnimationClip') or o['type']=='AnimationClip' and tree(o)['m_Legacy']:return source_resource(ident)")
exec(compile(s,'boss_embedded_prepare','exec'))
import json
manifest=O/'native-import.json'
data=json.loads(manifest.read_text(encoding='utf8'))
for row in data['resources']:
 if row.get('shaderName')=='tex':row['restoredShaderName']='AreaBattle/Recovered Boss Ice tex'
manifest.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')

