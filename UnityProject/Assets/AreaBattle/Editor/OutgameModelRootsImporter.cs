using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameModelRootsImporter
    {
        [Serializable] sealed class Node {public int id,parent,layer,siblingIndex;public string name;public Vector3 position,scale;public Quaternion rotation;public bool active;}
        [Serializable] sealed class Evidence {public string path,sha256;}
        [Serializable] sealed class Cam {public float near,far,fov,size,depth;public int mask,clear;public Color background;public bool orthographic,enabled,hdr,msaa;}
        [Serializable] sealed class SceneMono {public int rootNode,Scene_home,Scene_game,startui_root,soldierRoot,NormalGroup,DefenseGroup,AttackGroup,objCommanderRoot,objUnlockCommanderRoot,objCameraParent,objCameraCacheParent,objUIModelCacheParent;}
        [Serializable] sealed class Data {public SceneMono sceneMono;public int upgradeCameraNode;public Cam upgradeCamera;public Node[] nodes;public Evidence[] sources;public int normal,defense,attack,sceneHome,soldierRoot,modelBackdrop,homeBackdrop,cameraNode,sceneGame,gameCameraNode,commanderCameraNode;public Cam camera,gameCamera,commanderCamera;}
        static Camera ImportCamera(Transform node,Cam c)
        {
            var cam=node.gameObject.AddComponent<Camera>();
            cam.nearClipPlane=c.near;cam.farClipPlane=c.far;cam.fieldOfView=c.fov;cam.orthographic=c.orthographic;cam.orthographicSize=c.size;cam.depth=c.depth;cam.cullingMask=c.mask;cam.clearFlags=(CameraClearFlags)c.clear;cam.backgroundColor=c.background;cam.enabled=c.enabled;cam.allowHDR=c.hdr;cam.allowMSAA=c.msaa;return cam;
        }
        public static void ImportBatch()
        {
            try
            {
                RecoveredBossEmbeddedImporter.ImportOutgameBackground();
                var d=JsonUtility.FromJson<Data>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/model-roots.json")));
                foreach(var source in d.sources)using(var sha=System.Security.Cryptography.SHA256.Create())
                    if(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(BattleBuild.Target,source.path)))).Replace("-","").ToLowerInvariant()!=source.sha256)throw new InvalidDataException(source.path);
                var root=new GameObject("OriginalModelRoots");
                try
                {
                    var nodes=new Dictionary<int,Transform>();foreach(var n in d.nodes)
                    {
                        string background=n.id==d.homeBackdrop?"meshHomeScene_4":n.id==d.modelBackdrop?"meshHomeScene_18":null;
                        var go=background==null?new GameObject(n.name):UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/BossEmbedded/"+background));
                        go.name=n.name;nodes.Add(n.id,go.transform);
                    }
                    foreach(var n in d.nodes)
                    {
                        var t=nodes[n.id];t.SetParent(n.parent==0?root.transform:nodes[n.parent],false);t.localPosition=n.position;t.localRotation=n.rotation;t.localScale=n.scale;t.gameObject.layer=n.layer;t.gameObject.SetActive(n.active);
                    }
                    foreach(var group in d.nodes.GroupBy(n=>n.parent))foreach(var n in group.OrderBy(n=>n.siblingIndex))nodes[n.id].SetAsLastSibling();
                    var cam=ImportCamera(nodes[d.cameraNode],d.camera);
                    var gameCam=ImportCamera(nodes[d.gameCameraNode],d.gameCamera);
                    var commanderCam=ImportCamera(nodes[d.commanderCameraNode],d.commanderCamera);
                    var upgradeCam=ImportCamera(nodes[d.upgradeCameraNode],d.upgradeCamera);
                    var source=d.sceneMono;var owner=nodes[source.rootNode].gameObject.AddComponent<OutgameGameSceneMono>();owner.gameObject.tag="Tag10";
                    owner.GameCamera=gameCam;owner.HomeCamera=cam;owner.UpgradeCamera=upgradeCam;owner.CommanderCamera=commanderCam;
                    owner.Scene_home=nodes[source.Scene_home];owner.Scene_game=nodes[source.Scene_game];owner.Scene_home_CJroot=nodes[d.homeBackdrop].GetComponent<Renderer>();
                    owner.startui_root=nodes[source.startui_root];owner.soldierRoot=nodes[source.soldierRoot];owner.NormalGroup=nodes[source.NormalGroup];owner.DefenseGroup=nodes[source.DefenseGroup];owner.AttackGroup=nodes[source.AttackGroup];
                    owner.objCommanderRoot=nodes[source.objCommanderRoot];owner.objUnlockCommanderRoot=nodes[source.objUnlockCommanderRoot];owner.objCameraParent=nodes[source.objCameraParent];owner.objCameraCacheParent=nodes[source.objCameraCacheParent];owner.objUIModelCacheParent=nodes[source.objUIModelCacheParent];
                    var binding=root.AddComponent<OutgameModelRoots>();binding.Normal=nodes[d.normal];binding.Defense=nodes[d.defense];binding.Attack=nodes[d.attack];binding.HomeCamera=cam;binding.GameCamera=gameCam;binding.CommanderCamera=commanderCam;binding.SceneGame=nodes[d.sceneGame];binding.SceneHome=nodes[d.sceneHome];binding.SoldierRoot=nodes[d.soldierRoot];binding.ModelBackdrop=nodes[d.modelBackdrop];binding.HomeBackdrop=nodes[d.homeBackdrop];
                    PrefabUtility.SaveAsPrefabAsset(root,"Assets/AreaBattle/Resources/Recovered/Outgame/OriginalModelRoots.prefab",out bool ok);if(!ok)throw new InvalidDataException("roots prefab");AssetDatabase.SaveAssets();
                }
                finally{UnityEngine.Object.DestroyImmediate(root);}
                EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
