using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    // Source scene and resource copies remain separate. AtlasLoader is pending; no substitute is attached.
    public static class RecoveredUiBootstrapImporter
    {
        const string Destination="Assets/AreaBattle/Resources/Recovered/UiBootstrap";
        [Serializable] class Manifest {public Prefab[] prefabs;}
        [Serializable] class Prefab
        {public string name,sourceId,tag,canvasJson,cameraJson;public TransformData rootTransform,cameraTransform;public Scaler scaler;public Raycaster raycaster;public Adaptive adaptive;public Aspect aspect;public bool aspectEnabled,atlasAutoLoad;}
        [Serializable] class TransformData
        {public Vector3 m_LocalPosition,m_LocalScale;public Quaternion m_LocalRotation;public Vector2 m_AnchorMin,m_AnchorMax,m_AnchoredPosition,m_SizeDelta,m_Pivot;}
        [Serializable] class Scaler
        {public int m_UiScaleMode,m_ScreenMatchMode,m_PhysicalUnit;public float m_ReferencePixelsPerUnit,m_ScaleFactor,referenceWidth,referenceHeight,m_MatchWidthOrHeight,m_FallbackScreenDPI,m_DefaultSpriteDPI,m_DynamicPixelsPerUnit;public bool m_PresetInfoIsWorld;}
        [Serializable] class Raycaster {public bool m_IgnoreReversedGraphics;public int m_BlockingObjects,m_BlockingMask;}
        [Serializable] class Adaptive {public float whRatioConst,widthControlsHeightFactor,heightControlsWidthFactor;public bool isHeightCtrWidthFixedWidth;}
        [Serializable] class Aspect {public int m_AspectMode;public float m_AspectRatio;}
        public static void ImportBatch()
        {
            try
            {
                string workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;
                string source=Path.Combine(workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/ui-bootstrap-import.json");
                var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(source));
                if(manifest.prefabs.Length!=2)throw new InvalidDataException("Bootstrap source count");
                EnsureTag();Directory.CreateDirectory(Destination);AssetDatabase.Refresh();
                foreach(var item in manifest.prefabs)Build(item);
                File.Copy(source,Destination+"/ui-bootstrap-import.json",true);AssetDatabase.ImportAsset(Destination+"/ui-bootstrap-import.json");AssetDatabase.SaveAssets();
                File.WriteAllText(Path.Combine(workspace,"analysis/unity-ui-bootstrap-import-validation.json"),"{\"passed\":true,\"prefabs\":2,\"scope\":\"Source native components, camera references, transforms, tags and serialized adaptive values; AtlasLoader and production startup pending\"}");
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static void EnsureTag()
        {
            var manager=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var tags=manager.FindProperty("tags");
            for(int i=0;i<tags.arraySize;i++)if(tags.GetArrayElementAtIndex(i).stringValue=="GFUICanvas")return;
            int index=tags.arraySize;tags.InsertArrayElementAtIndex(index);tags.GetArrayElementAtIndex(index).stringValue="GFUICanvas";manager.ApplyModifiedPropertiesWithoutUndo();
        }
        static void Build(Prefab p)
        {
            var root=new GameObject("UICanvas",typeof(RectTransform));root.SetActive(false);root.layer=5;root.tag=p.tag;
            try
            {
                var rt=(RectTransform)root.transform;var d=p.rootTransform;
                rt.anchorMin=d.m_AnchorMin;rt.anchorMax=d.m_AnchorMax;rt.pivot=d.m_Pivot;rt.sizeDelta=d.m_SizeDelta;rt.anchoredPosition=d.m_AnchoredPosition;
                rt.localPosition=d.m_LocalPosition;rt.localRotation=d.m_LocalRotation;rt.localScale=d.m_LocalScale;
                var child=new GameObject("UICamera");child.layer=5;child.transform.SetParent(rt,false);d=p.cameraTransform;
                child.transform.localPosition=d.m_LocalPosition;child.transform.localRotation=d.m_LocalRotation;child.transform.localScale=d.m_LocalScale;
                var camera=child.AddComponent<Camera>();EditorJsonUtility.FromJsonOverwrite("{\"Camera\":"+p.cameraJson+"}",camera);
                var canvas=root.AddComponent<Canvas>();EditorJsonUtility.FromJsonOverwrite("{\"Canvas\":"+p.canvasJson+"}",canvas);canvas.worldCamera=camera;
                var scaler=root.AddComponent<CanvasScaler>();var s=p.scaler;
                scaler.uiScaleMode=(CanvasScaler.ScaleMode)s.m_UiScaleMode;scaler.referencePixelsPerUnit=s.m_ReferencePixelsPerUnit;scaler.scaleFactor=s.m_ScaleFactor;
                scaler.referenceResolution=new Vector2(s.referenceWidth,s.referenceHeight);scaler.screenMatchMode=(CanvasScaler.ScreenMatchMode)s.m_ScreenMatchMode;scaler.matchWidthOrHeight=s.m_MatchWidthOrHeight;
                scaler.physicalUnit=(CanvasScaler.Unit)s.m_PhysicalUnit;scaler.fallbackScreenDPI=s.m_FallbackScreenDPI;scaler.defaultSpriteDPI=s.m_DefaultSpriteDPI;scaler.dynamicPixelsPerUnit=s.m_DynamicPixelsPerUnit;
                var ray=root.AddComponent<GraphicRaycaster>();ray.ignoreReversedGraphics=p.raycaster.m_IgnoreReversedGraphics;ray.blockingObjects=(GraphicRaycaster.BlockingObjects)p.raycaster.m_BlockingObjects;ray.blockingMask=p.raycaster.m_BlockingMask;
                var fit=root.AddComponent<AspectRatioFitter>();fit.enabled=p.aspectEnabled;fit.aspectMode=(AspectRatioFitter.AspectMode)p.aspect.m_AspectMode;fit.aspectRatio=p.aspect.m_AspectRatio;
                var adaptive=root.AddComponent<OutgameCanvasAdaptive>();adaptive.whRatioConst=p.adaptive.whRatioConst;adaptive.widthControlsHeightFactor=p.adaptive.widthControlsHeightFactor;adaptive.heightControlsWidthFactor=p.adaptive.heightControlsWidthFactor;adaptive.isHeightCtrWidthFixedWidth=p.adaptive.isHeightCtrWidthFixedWidth;
                // Keep the temporary authoring object inactive to avoid editor-driven geometry.
                // Canvas activation drives root geometry in the editor. Restore the serialized source snapshot before saving.
                d=p.rootTransform;var savedRect=new SerializedObject(rt);
                savedRect.FindProperty("m_LocalScale").vector3Value=d.m_LocalScale;savedRect.FindProperty("m_LocalPosition").vector3Value=d.m_LocalPosition;
                savedRect.FindProperty("m_AnchorMin").vector2Value=d.m_AnchorMin;savedRect.FindProperty("m_AnchorMax").vector2Value=d.m_AnchorMax;
                savedRect.FindProperty("m_Pivot").vector2Value=d.m_Pivot;savedRect.FindProperty("m_SizeDelta").vector2Value=d.m_SizeDelta;savedRect.FindProperty("m_AnchoredPosition").vector2Value=d.m_AnchoredPosition;savedRect.ApplyModifiedPropertiesWithoutUndo();
                Directory.CreateDirectory(Destination+"/"+p.name);AssetDatabase.Refresh();
                string path=Destination+"/"+p.name+"/UICanvas.prefab";PrefabUtility.SaveAsPrefabAsset(root,path,out bool success);if(!success)throw new InvalidDataException("Bootstrap save failed");
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);var serializedRoot=new SerializedObject(asset);serializedRoot.FindProperty("m_IsActive").boolValue=true;serializedRoot.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SavePrefabAsset(asset);
                var importer=AssetImporter.GetAtPath(path);importer.userData="wxcf1394487200e48f/43 | "+p.sourceId+" | AtlasLoader pending";importer.SaveAndReimport();
                Verify(p,AssetDatabase.LoadAssetAtPath<GameObject>(path));
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
        static void Verify(Prefab p,GameObject root)
        {
            var camera=root.transform.Find("UICamera").GetComponent<Camera>();var canvas=root.GetComponent<Canvas>();var scaler=root.GetComponent<CanvasScaler>();var adaptive=root.GetComponent<OutgameCanvasAdaptive>();var fit=root.GetComponent<AspectRatioFitter>();var ray=root.GetComponent<GraphicRaycaster>();
            var rt=(RectTransform)root.transform;var d=p.rootTransform;
            Require(rt.localScale==d.m_LocalScale&&rt.anchorMin==d.m_AnchorMin&&rt.anchorMax==d.m_AnchorMax&&rt.sizeDelta==d.m_SizeDelta&&rt.pivot==d.m_Pivot,"Root transform source fields");
            Require(root.name=="UICanvas"&&root.CompareTag("GFUICanvas")&&root.activeSelf&&root.layer==5&&root.transform.childCount==1,"Bootstrap root identity");
            Require(camera.transform.localPosition==p.cameraTransform.m_LocalPosition&&camera.transform.localScale==p.cameraTransform.m_LocalScale,"Camera transform");
            Require(canvas.worldCamera==camera&&canvas.renderMode==RenderMode.ScreenSpaceCamera&&canvas.sortingOrder==-1&&canvas.planeDistance==0f&&(int)canvas.additionalShaderChannels==25,"Canvas fields and reference");
            Require(camera.rect==new Rect(0,0,1,1)&&camera.orthographic&&Mathf.Abs(camera.orthographicSize-6.4f)<.00001f&&camera.cullingMask==32&&camera.depth==10&&camera.nearClipPlane==0&&camera.farClipPlane==50&&!camera.allowHDR&&!camera.allowMSAA&&camera.clearFlags==CameraClearFlags.Depth,"Source UI camera");
            Require(scaler.referenceResolution==new Vector2(p.scaler.referenceWidth,p.scaler.referenceHeight)&&scaler.matchWidthOrHeight==p.scaler.m_MatchWidthOrHeight&&scaler.uiScaleMode==CanvasScaler.ScaleMode.ScaleWithScreenSize,"CanvasScaler source selection");
            Require(adaptive.whRatioConst==p.adaptive.whRatioConst&&adaptive.isHeightCtrWidthFixedWidth==p.adaptive.isHeightCtrWidthFixedWidth&&adaptive.heightControlsWidthFactor==p.adaptive.heightControlsWidthFactor,"Adaptive source selection");
            Require(fit.enabled==p.aspectEnabled&&(int)fit.aspectMode==p.aspect.m_AspectMode&&Mathf.Abs(fit.aspectRatio-p.aspect.m_AspectRatio)<.000001f,"AspectRatioFitter source selection");
            Require(ray.blockingMask==32&&ray.blockingObjects==GraphicRaycaster.BlockingObjects.ThreeD&&ray.ignoreReversedGraphics,"Raycaster source fields");
        }
    }
}
