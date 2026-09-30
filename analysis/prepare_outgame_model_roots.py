from pathlib import Path
import json,hashlib
r=Path(__file__).resolve().parent/'targets/wxcf1394487200e48f/43';a=r/'generated/unity-assets'
def read(p):return json.loads(p.read_text(encoding='utf8'))
ts={int(p.stem.split('__')[-1]):(p,read(p)) for p in (a/'Transform').glob('*BuildPlayer-GamePlay*')};gos={int(p.stem.split('__')[-1]):(p,read(p)) for p in (a/'GameObject').glob('*BuildPlayer-GamePlay*')}
cp=a/'Camera/unnamed__BuildPlayer-GamePlay__47.json';c=read(cp);cameraTransform=next(i for i,(_,t) in ts.items() if t['m_GameObject']==c['m_GameObject'])
extraCameras=[]
for cameraId in [46,48,45]:
 ep=a/f'Camera/unnamed__BuildPlayer-GamePlay__{cameraId}.json';ec=read(ep)
 et=next(i for i,(_,t) in ts.items() if t['m_GameObject']==ec['m_GameObject'])
 extraCameras.append((ep,ec,et))
selected=set()
monoPath=a/'MonoBehaviour/unnamed__BuildPlayer-GamePlay__59.json'
mono=read(monoPath)
sourceTransforms=['Scene_home','Scene_game','startui_root','soldierRoot','NormalGroup','DefenseGroup','AttackGroup','objCommanderRoot','objUnlockCommanderRoot','objCameraParent','objCameraCacheParent','objUIModelCacheParent']
homeBackdrop=next(i for i,(_,t) in ts.items() if t["m_GameObject"]["m_PathID"]==4)
for i in [44,41,42,40,homeBackdrop,cameraTransform,28]+[x[2] for x in extraCameras]+[mono[k]["m_PathID"] for k in sourceTransforms]:
 while i:
  selected.add(i);i=ts[i][1]['m_Father']['m_PathID']
nodes=[];sources=[]
for i in sorted(selected):
 p,t=ts[i];gp,g=gos[t['m_GameObject']['m_PathID']]
 nodes.append(dict(id=i,name=g['m_Name'],parent=t['m_Father']['m_PathID'],position=t['m_LocalPosition'],rotation=t['m_LocalRotation'],scale=t['m_LocalScale'],layer=g['m_Layer'],active=bool(g['m_IsActive']),siblingIndex=next((j for j,c in enumerate(ts[t["m_Father"]["m_PathID"]][1]["m_Children"]) if c["m_PathID"]==i),0) if t["m_Father"]["m_PathID"] else 0))
 for q in [p,gp]:sources.append(dict(path=q.relative_to(r).as_posix(),sha256=hashlib.sha256(q.read_bytes()).hexdigest()))
sources.append(dict(path=cp.relative_to(r).as_posix(),sha256=hashlib.sha256(cp.read_bytes()).hexdigest()))
out=dict(nodes=nodes,sources=sources,normal=44,defense=41,attack=42,sceneHome=31,soldierRoot=35,modelBackdrop=40,homeBackdrop=homeBackdrop,cameraNode=cameraTransform,camera=dict(near=c['near clip plane'],far=c['far clip plane'],fov=c['field of view'],orthographic=c['orthographic'],size=c['orthographic size'],mask=c['m_CullingMask']['m_Bits']-(1<<32),clear=c['m_ClearFlags'],background=c['m_BackGroundColor'],depth=c['m_Depth'],enabled=bool(c['m_Enabled']),hdr=c['m_HDR'],msaa=c['m_AllowMSAA']),scope='Original model group ancestor transforms and HomeCamera only; remaining scene content and aspect adaptation pending')
out['sceneGame']=28
out['gameCameraNode']=extraCameras[0][2];out['commanderCameraNode']=extraCameras[1][2]
for key,(ep,ec,et) in zip(['gameCamera','commanderCamera','upgradeCamera'],extraCameras):
 mask=ec['m_CullingMask']['m_Bits'];mask=mask-(1<<32) if mask>=(1<<31) else mask
 out[key]=dict(near=ec['near clip plane'],far=ec['far clip plane'],fov=ec['field of view'],orthographic=ec['orthographic'],size=ec['orthographic size'],mask=mask,clear=ec['m_ClearFlags'],background=ec['m_BackGroundColor'],depth=ec['m_Depth'],enabled=bool(ec['m_Enabled']),hdr=ec['m_HDR'],msaa=ec['m_AllowMSAA'])
 sources.append(dict(path=ep.relative_to(r).as_posix(),sha256=hashlib.sha256(ep.read_bytes()).hexdigest()))
out['upgradeCameraNode']=extraCameras[2][2]
out['sceneMono']={k:mono[k]['m_PathID'] for k in sourceTransforms};out['sceneMono']['rootNode']=37
sources.append(dict(path=monoPath.relative_to(r).as_posix(),sha256=hashlib.sha256(monoPath.read_bytes()).hexdigest()))
out['scope']='Original serialized GameSceneMono references, model-group ancestors, four cameras and selected transforms in source sibling order. Unreferenced scene content remains outside this prefab.' 
(r/'generated/outgame/model-roots.json').write_text(json.dumps(out,indent=2)+'\n',encoding='utf8');print(len(nodes),'source nodes')
