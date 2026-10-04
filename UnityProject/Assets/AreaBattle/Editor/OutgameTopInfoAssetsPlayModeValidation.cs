using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTopInfoAssetsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTopInfoAssetsPlayModeValidation
    {
        const string Pending="AreaBattle.TopInfoAssetsNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error,capture;public int goldParticles,diamondParticles,goldPixels,diamondPixels;
            public string scope="Original TopInfo prefab with effect1007/1019 via native AssetBundle, real EffectModule/provider and source image parents. Native rendering, persistent lifetime and explicit close/shutdown are exercised. Account/profile/avatar actions, complete page lifecycle/Main/Player and original-frame visual equivalence remain pending. Resource acquisition is a local validation bundle.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static Camera camera;static OutgameBaseEffect gold,diamond;static GameObject oldGold,oldDiamond;static double started,phaseAt;static int phase,frame;static bool stopped;
        static OutgameTopInfoAssetsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){var bundle=OutgameTopInfoAssetsValidation.Bundle;SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"),false,"Game").Show();EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool b,string message){if(!b)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int p){phase=p;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Start()
        {
            stopped=false;report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                OutgameTopInfoAssetsValidation.LoadPrepared();camera=new GameObject("UICamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.08f,.12f);camera.farClipPlane=3000;
                f=new Fixture(true){Camera=camera};var canvas=f.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
                var scaler=f.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;f.Root.AddComponent<GraphicRaycaster>();
                Canvas.ForceUpdateCanvases();
                var sourceRoot=new OutgameUiRootInitialization(f.Root,()=>true,()=>false,()=>{});sourceRoot.Loaded((name,active)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.SetActive(active);return go;});
                f.Top.transform.SetParent(sourceRoot.UiRoot.Find("UIPopup"),false);Canvas.ForceUpdateCanvases();
                gold=f.Module.Get(f.Module.Show(1007,f.GoldParent));diamond=f.Module.Get(f.Module.Show(1019,f.DiamondParent));Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static int Difference(OutgameBaseEffect effect,string label)
        {
            var renderers=effect.GameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);var flags=renderers.Select(r=>r.enabled).ToArray();
            var rt=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);var target=camera.targetTexture;var active=RenderTexture.active;Texture2D before=null,after=null;
            try{
                camera.targetTexture=rt;Canvas.ForceUpdateCanvases();foreach(var r in renderers)r.enabled=false;camera.Render();RenderTexture.active=rt;
                before=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);before.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);before.Apply();
                for(int i=0;i<renderers.Length;i++)renderers[i].enabled=flags[i];camera.Render();RenderTexture.active=rt;
                after=new Texture2D(rt.width,rt.height,TextureFormat.RGBA32,false);after.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);after.Apply();
                var a=before.GetPixels32();var b=after.GetPixels32();int n=0;for(int i=0;i<a.Length;i++)if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>3)n++;
                string folder=Path.Combine(BattleBuild.Workspace,"analysis/captures");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,"top-info-"+label+"-visible.png"),after.EncodeToPNG());File.WriteAllBytes(Path.Combine(folder,"top-info-"+label+"-hidden.png"),before.EncodeToPNG());return n;
            }finally{for(int i=0;i<renderers.Length;i++)renderers[i].enabled=flags[i];camera.targetTexture=target;RenderTexture.active=active;if(before)UnityEngine.Object.Destroy(before);if(after)UnityEngine.Object.Destroy(after);rt.Release();UnityEngine.Object.Destroy(rt);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>35)throw new TimeoutException("top info assets phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0){
                    if(!gold.IsLoaded||!diamond.IsLoaded||EditorApplication.timeSinceStartup-phaseAt<1.5)return;
                    report.goldParticles=gold.GameObject.GetComponentsInChildren<ParticleSystem>(true).Sum(p=>p.particleCount);report.diamondParticles=diamond.GameObject.GetComponentsInChildren<ParticleSystem>(true).Sum(p=>p.particleCount);
                    var diagnostics=new List<string>();foreach(var e in new[]{gold,diamond}){
                        for(var t=e.GameObject.transform;t!=null;t=t.parent)diagnostics.Add(t.name+" active="+t.gameObject.activeInHierarchy+" pos="+t.position+" scale="+t.lossyScale+" screen="+camera.WorldToScreenPoint(t.position));
                        foreach(var ps in e.GameObject.GetComponentsInChildren<ParticleSystem>(true))diagnostics.Add(ps.name+" time="+ps.time+" count="+ps.particleCount+" playing="+ps.isPlaying+" paused="+ps.isPaused+" stopped="+ps.isStopped+" emission="+ps.emission.enabled+" culling="+ps.main.cullingMode);
                    }File.WriteAllLines(Path.Combine(BattleBuild.Workspace,"analysis/top-info-assets-render-diagnostics.txt"),diagnostics);
                    if(report.goldParticles==0||report.diamondParticles==0)return;
                    if(report.goldPixels==0)report.goldPixels=Difference(gold,"gold");
                    if(report.diamondPixels==0)report.diamondPixels=Difference(diamond,"diamond");
                    if(report.goldPixels==0||report.diamondPixels==0){frame=Time.frameCount;return;}
                    Check(gold.GameObject&&diamond.GameObject&&gold.GameObject.transform.parent==f.GoldParent&&diamond.GameObject.transform.parent==f.DiamondParent,"Original1007/1019 attach to the two original TopInfo image parents after native async bundle loads");
                    Check(f.Loads.SequenceEqual(new[]{"Effect/UI/hdzd_effect_jinbiglow","Effect/UI/hdzd_eff_Diamond02"})&&f.Providers.Values.All(p=>p.RefCount==1)&&f.Errors.Count==0,"Original resource names/case and one live provider handle per effect");
                    Check(gold.Config.duration==0&&diamond.Config.duration==0&&gold.GameObject.transform.localScale==Vector3.one&&diamond.GameObject.transform.localScale==Vector3.one,"Source zero-duration configuration and root scale retained");
                    Check(report.goldParticles>0&&report.diamondParticles>0,"Both original currency effects emit live particles in native frames");
                    Check(new[]{gold,diamond}.SelectMany(e=>e.GameObject.GetComponentsInChildren<ParticleSystemRenderer>(true)).Where(r=>r.enabled).All(r=>r.sharedMaterial&&r.sharedMaterial.mainTexture&&r.sortingLayerName=="UIPopup"&&r.sortingOrder==1),"Native materials and parent UIPopup sorting inherited by all original renderers");
                    Check(report.goldPixels>0&&report.diamondPixels>0,"Each effect independently contributes visible camera pixels");
                    report.capture=Path.Combine(BattleBuild.Workspace,"analysis/captures/top-info-effects-native.png");ScreenCapture.CaptureScreenshot(report.capture);Phase(1);return;
                }
                if(phase==1){
                    if(EditorApplication.timeSinceStartup-phaseAt<2||!File.Exists(report.capture))return;
                    Check(f.Module.Effects.Count==2&&!gold.IsDisposed&&!diamond.IsDisposed,"Zero-duration effects stay registered across later native frames");
                    oldGold=gold.GameObject;f.Module.Close(gold.UID);Check(f.Module.Get(gold.UID)==null&&f.Providers[OutgameTopInfoAssetsValidation.Gold].RefCount==0&&f.Providers[OutgameTopInfoAssetsValidation.Diamond].RefCount==1,"Explicit gold close releases its handle while diamond remains live");Phase(2);return;
                }
                if(phase==2){
                    Check(!oldGold&&diamond.GameObject&&!diamond.IsDisposed,"Deferred native destruction removes gold only");oldDiamond=diamond.GameObject;f.Module.Shutdown();Phase(3);return;
                }
                if(phase==3){Check(!oldDiamond&&diamond.IsDisposed&&f.Module.Effects.Count==0&&f.Providers.Values.All(p=>p.RefCount==0),"Module shutdown releases the remaining persistent diamond and its native object");Finish(null);}
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{f?.Dispose();}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/top-info-assets-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_TOP_INFO_ASSETS_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
