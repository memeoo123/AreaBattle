import json,sys
from pathlib import Path
sys.path.insert(0,'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/unitypy-1.25.2')
import UnityPy
from native_unity_serialization import native_tree,document
R=Path('analysis/targets/wxcf1394487200e48f/43');E=json.loads((R/'generated/asset-evidence.json').read_text(encoding='utf8'));source=next(o for o in E['objects'] if o['id']=='CAB-1c54ab3ba4798d190cd8c8add407659d:3730588714688692163')
env=UnityPy.load(str(R/source['source']));reader=next(o for o in env.objects if o.path_id==source['pathId'])
def pointer(value,path):
 if path=='.m_GameObject':return {'fileID':1000000000001}
 if value['m_PathID']:raise ValueError('Probe requires a pointer-free particle '+path)
 return {'fileID':0}
particle=native_tree(reader.serialized_type.node,reader.read_typetree(),pointer)
game={'m_ObjectHideFlags':0,'m_CorrespondingSourceObject':{'fileID':0},'m_PrefabInstance':{'fileID':0},'m_PrefabAsset':{'fileID':0},'serializedVersion':6,'m_Component':[{'component':{'fileID':1000000000002}},{'component':{'fileID':1000000000003}}],'m_Layer':0,'m_Name':'NativeParticleProbe','m_TagString':'Untagged','m_Icon':{'fileID':0},'m_NavMeshLayer':0,'m_StaticEditorFlags':0,'m_IsActive':1}
transform={'m_ObjectHideFlags':0,'m_GameObject':{'fileID':1000000000001},'serializedVersion':2,'m_LocalRotation':{'x':0,'y':0,'z':0,'w':1},'m_LocalPosition':{'x':0,'y':0,'z':0},'m_LocalScale':{'x':1,'y':1,'z':1},'m_ConstrainProportionsScale':0,'m_Children':[],'m_Father':{'fileID':0},'m_LocalEulerAnglesHint':{'x':0,'y':0,'z':0}}
dest=Path('UnityProject/Assets/AreaBattle/Experimental/NativeParticleProbe.prefab');dest.parent.mkdir(exist_ok=True)
dest.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n'+document(1,1000000000001,'GameObject',game)+document(4,1000000000002,'Transform',transform)+document(198,1000000000003,'ParticleSystem',particle),encoding='utf8')
print(dest)
