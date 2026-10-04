using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTaskBoxEffectValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskBoxEffectPlayModeValidation
    {
        const string Pending="AreaBattle.TaskBoxEffectNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error,capture;public int liveParticles,particlePixels;public long restoredGold;
            public string scope="Original TaskPanel and effect1016 particle prefab/materials through native AssetBundle and real EffectModule/provider; actual UGUI pointer claims, shared account reward/file persistence, tab-close, natural timer and independent restart. Local native bundle acquisition host; audio/currency-fly/report/platform fixtures and full production Main/EffectControl/original audiovisual equivalence remain pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static OutgameBaseEffect effect;static GameObject oldEffect;static string path;static long gold;static Camera camera;static int phase,frame;static double started,phaseAt;static bool stopped;
        static OutgameTaskBoxEffectPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){OutgameTaskBoxEffectValidation.Prepare();SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool b,string message){if(!b)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Click(GameObject target)=>ExecuteEvents.Execute(target,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Compose()
        {
            f=new Fixture(true,path){Camera=camera};var canvas=f.Account.Page.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
            var scaler=f.Account.Page.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;f.Account.Page.Root.AddComponent<GraphicRaycaster>();
            Canvas.ForceUpdateCanvases();
            var sourceRoot=new OutgameUiRootInitialization(f.Account.Page.Root,()=>true,()=>false,()=>{});
            sourceRoot.Loaded((name,active)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.SetActive(active);return go;});
            f.Account.Page.Layer.SetParent(sourceRoot.UiRoot.Find("UITip"),false);var layer=(RectTransform)f.Account.Page.Layer;layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;
            f.Services.UiRoot=()=>sourceRoot.UiRoot;
            var entry=new OutgameTaskEntryBinding(f.Account.Page.Data.Main.transform,new OutgameTaskEntryServices{Control=()=>f.Account.Page.Data.Control,Pages=()=>f.Account.Page.OpenRegistry,FindPage=()=>f.Account.Page.Pages.TryGetValue(OutgameTaskPage.SourceName,out var page)?page:null,Messages=f.Account.Page.Ui.Messages,PlayVoice=(kind,id)=>{}});entry.Initialize();
            Click(entry.Button.gameObject);
        }
        static void CaptureParticleDifference()
        {
            var renderers=effect.GameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);var flags=renderers.Select(r=>r.enabled).ToArray();
            var target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);var priorTarget=camera.targetTexture;var priorActive=RenderTexture.active;Texture2D before=null,after=null;
            try{
                camera.targetTexture=target;Canvas.ForceUpdateCanvases();
                foreach(var r in renderers)r.enabled=false;camera.Render();RenderTexture.active=target;before=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);before.ReadPixels(new Rect(0,0,target.width,target.height),0,0);before.Apply();
                for(int i=0;i<renderers.Length;i++)renderers[i].enabled=flags[i];camera.Render();RenderTexture.active=target;after=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);after.ReadPixels(new Rect(0,0,target.width,target.height),0,0);after.Apply();
                var a=before.GetPixels32();var b=after.GetPixels32();for(int i=0;i<a.Length;i++)if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>3)report.particlePixels++;
                string folder=Path.Combine(BattleBuild.Workspace,"analysis/captures");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,"task-box-effect-with-particles.png"),after.EncodeToPNG());File.WriteAllBytes(Path.Combine(folder,"task-box-effect-without-particles.png"),before.EncodeToPNG());
                Check(report.particlePixels>0,"Original particle renderers contribute visible native camera pixels");
            }finally{for(int i=0;i<renderers.Length;i++)renderers[i].enabled=flags[i];camera.targetTexture=priorTarget;RenderTexture.active=priorActive;if(before)UnityEngine.Object.Destroy(before);if(after)UnityEngine.Object.Destroy(after);target.Release();UnityEngine.Object.Destroy(target);Canvas.ForceUpdateCanvases();}
        }
        static void Start()
        {
            stopped=false;report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                OutgameTaskBoxEffectValidation.LoadPrepared();new GameObject("EventSystem",typeof(EventSystem));camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.08f,.12f);camera.farClipPlane=3000;
                path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskBoxEffect-"+Guid.NewGuid().ToString("N"));Compose();f.Account.Tasks.Child.ModuleData.ext.livenessValue=100;Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>50)throw new TimeoutException("task box effect phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0){
                    if(!f.Account.Page.Trace.Contains("open"))return;
                    Check(f.Account.Page.Page.OutletCount==18&&f.Account.Page.Subviews.Daily.I.Liveness==100,"Original main pointer opens complete task page and eligible liveness tiers");
                    Check(camera.WorldToScreenPoint(f.Account.Page.Page.Transform.position).z>camera.nearClipPlane,"Original UI root is in front of the actual UI camera");
                    Click(f.Box().gameObject);effect=f.Effect;
                    Check(effect!=null&&effect.Config.id==1016&&f.Module.Effects.Count==1,"Native tier pointer creates source1016 through actual EffectModule");Phase(1);return;
                }
                if(phase==1){
                    if(EditorApplication.timeSinceStartup-phaseAt<.7||!effect.IsLoaded)return;
                    Check(f.Account.Global.GetItemCount(1001)==50&&f.Account.Account.Local.GoldNum==50&&f.Account.Tasks.Child.ModuleData.ext.livenessAward==1&&!f.Box().GetComponent<Button>().enabled,"Real end-of-frame claim grants50 to both account records and saves first tier bit");
                    Check(effect.GameObject&&effect.GameObject.name=="hdzd_eff_bxGlow"&&effect.GameObject.transform.parent==f.Box()&&effect.GameObject.transform.localScale==Vector3.one*100&&f.Provider.RefCount==1,"Native bundle supplies original named prefab under source box with scale100 and owned handle");
                    var particles=effect.GameObject.GetComponentsInChildren<ParticleSystem>(true);report.liveParticles=particles.Sum(p=>p.particleCount);
                    Check(particles.Length==4&&report.liveParticles>0&&particles.All(p=>!p.main.useUnscaledTime),"All four original particle systems run in actual scaled native frames");
                    Check(effect.GameObject.GetComponentsInChildren<ParticleSystemRenderer>(true).All(r=>r.sharedMaterial&&r.sharedMaterial.mainTexture&&r.sortingLayerName=="UITip"),"Original textures/materials follow task UITip sorting layer");
                    CaptureParticleDifference();
                    var detail=new List<string>();for(var t=f.Account.Page.Page.Transform;t!=null;t=t.parent){var c=t.GetComponent<Canvas>();detail.Add(t.name+" active="+t.gameObject.activeInHierarchy+" pos="+t.position+" local="+t.localPosition+" scale="+t.lossyScale+" screen="+camera.WorldToScreenPoint(t.position)+" canvas="+(c?c.renderMode+"/"+c.sortingLayerName+"/"+c.sortingOrder+"/"+c.enabled:"none"));}
                    detail.Add("box="+f.Box().position+" screen="+camera.WorldToScreenPoint(f.Box().position));
                    detail.Add("effect="+effect.GameObject.transform.position+" scale="+effect.GameObject.transform.lossyScale);
                    File.WriteAllLines(Path.Combine(BattleBuild.Workspace,"analysis/task-box-effect-render-diagnostics.txt"),detail);
                    report.capture=Path.Combine(BattleBuild.Workspace,"analysis/captures/task-box-effect-native.png");Directory.CreateDirectory(Path.GetDirectoryName(report.capture));ScreenCapture.CaptureScreenshot(report.capture);Phase(2);return;
                }
                if(phase==2){
                    if(!File.Exists(report.capture))return;
                    oldEffect=effect.GameObject;Click(f.Account.Page.Page.AchievementTab.gameObject);
                    Check(f.Module.Get(effect.UID)==null&&f.Provider.RefCount==0&&f.Account.Page.Page.Liveness.EffectHandle==0,"Native achievement toggle closes and releases current box effect");Phase(3);return;
                }
                if(phase==3){
                    Check(!oldEffect,"Original task effect native object is destroyed after tab change");Click(f.Account.Page.Page.TaskTab.gameObject);Click(f.Box(1).gameObject);effect=f.Effect;Phase(4);return;
                }
                if(phase==4){
                    if(!effect.IsLoaded||f.Account.Tasks.Child.ModuleData.ext.livenessAward!=3)return;
                    if(f.Module.Get(effect.UID)!=null)return;
                    Check(effect.IsDisposed&&!effect.GameObject&&f.Provider.RefCount==0,"Second source box effect naturally expires after original five-second module duration");
                    Check(f.Account.Page.Page.Liveness.EffectHandle==effect.UID,"Natural module completion retains source page handle until a later tab callback");
                    gold=f.Account.Account.Local.GoldNum;f.Account.Tasks.Parent.Update();f.Account.Statistics.Pool.SaveData();Click(f.Account.Page.Page.CloseButton.gameObject);Phase(5);return;
                }
                if(phase==5){
                    if(f.Account.Page.Provider.RefCount!=0)return;f.Dispose();f=null;Compose();Phase(6);return;
                }
                if(phase==6){
                    if(!f.Account.Page.Trace.Contains("open"))return;
                    report.restoredGold=f.Account.Account.Local.GoldNum;
                    Check(f.Account.Global.GetItemCount(1001)==gold&&report.restoredGold==gold&&f.Account.Tasks.Child.ModuleData.ext.livenessAward==3,"Independent account/file restart restores both economic records and both claimed tier bits");
                    Check(f.Account.Page.Subviews.Daily.I.LivenessItems[0].state==1&&f.Account.Page.Subviews.Daily.I.LivenessItems[1].state==1&&f.Module.Effects.Count==0&&f.Provider.RefCount==0,"Reopened original task page shows both claimed boxes without recreating reward effects");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{f?.Dispose();}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-box-effect-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_TASK_BOX_EFFECT_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
