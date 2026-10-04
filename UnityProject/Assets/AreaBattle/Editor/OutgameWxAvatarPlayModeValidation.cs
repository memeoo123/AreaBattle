using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTopInfoPageValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameWxAvatarPlayModeValidation
    {
        const string Pending="AreaBattle.WxAvatarNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error,capture,restoredNickname,restoredAvatar;public bool restoredAuthorization;public int restoredPreference;
            public string scope="Real UnityWebRequestTexture local-file success/missing-file failure, original default sprite/cache/dedup/callback, actual TopInfo/Match/UserDataPrefs/shared account file save and independent restart. Local URLs are explicit native transport fixtures, not remote server/WeChat authorization. Main/platform/Rank/UI destinations remain incomplete.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static OutgameMatchControl match;static OutgameWxAvatarRuntime runtime;static OutgameUserPreferences prefs;static OutgamePlatformStringPreferences platform;
        static readonly List<OutgameUnityAvatarRequest> requests=new List<OutgameUnityAvatarRequest>();
        static Report report;static string path,url,bad;static int phase,frame,refreshes,sharedCallback;static double started;static bool stopped;static Camera camera;
        static OutgameWxAvatarPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){OutgameAccountRewardsValidation.PrepareConfig();var bundle=OutgameTopInfoAssetsValidation.Bundle;SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"),false,"Game").Show();EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool b,string why){if(!b)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int v){phase=v;frame=Time.frameCount;}
        static void Compose()
        {
            f=new Fixture(true,path);f.Effects.Camera=camera;match=OutgameMatchValidation.AttachTopInfo(f);
            var canvas=f.Effects.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
            var scaler=f.Effects.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;
            f.Effects.Root.AddComponent<GraphicRaycaster>();Canvas.ForceUpdateCanvases();
            var roots=new OutgameUiRootInitialization(f.Effects.Root,()=>true,()=>false,()=>{});roots.Loaded((n,a)=>{var g=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));g.SetActive(a);return g;});f.Layer.SetParent(roots.UiRoot.Find("UIPopup"),false);
            var rect=(RectTransform)f.Layer;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            platform=new OutgamePlatformStringPreferences(f.Account.Statistics.Strings.GetString,f.Account.Statistics.Strings.SetString);
            var file=new OutgameWebGlFileStorage(f.Account.Statistics.Strings,()=>false,s=>{});
            prefs=file.CreatePreferences(()=>f.Account.Statistics.Pool.SaveDisabled,s=>{},s=>{},(c,s)=>{if(c!=1001)throw new Exception(s);},s=>f.Trace.Add("actual-userdata-saved"));prefs.OnInit(false);
            var services=new OutgameWxAvatarServices{StartCoroutine=e=>f.Runner.StartCoroutine(e),Messages=()=>f.Messages,Log=s=>f.Trace.Add(s),Request=u=>{var request=new OutgameUnityAvatarRequest(u);requests.Add(request);return request;}};
            runtime=new OutgameWxAvatarRuntime(services,platform.GetString);runtime.BindTopInfo(f.Services,()=>prefs);runtime.InitAtStartGame();
            f.Messages.AddListener("Avatar_Refresh",a=>refreshes++);
            f.Services.SetSprite=(im,n,a,m)=>{string id=n=="Tx_man01"?"2720706472938528916":n=="Tx_frame01_small"?"-5557301240740203688":throw new Exception("default profile sprite fixture");im.sprite=Resources.Load<Sprite>("Recovered/TopInfo/Sprites/CAB-a79877fee73a97ed0be4eacb186a645b_"+id);};
            var ui=new OutgameUiControl(null,f.Account.Page.Data.Registry,()=>f.Messages,new OutgameUiControlGlobals(),null,()=>f.Open(),null,null,null);ui.ShowTopInfoUI();
        }
        static void Start()
        {
            stopped=false;report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                OutgameAccountRewardsValidation.LoadPreparedConfig();OutgameTopInfoAssetsValidation.LoadPrepared();
                camera=new GameObject("UICamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.farClipPlane=3000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.08f,.12f);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleWxAvatar-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(path);
                string png=Path.Combine(path,"avatar.png");File.Copy(Path.Combine(Application.dataPath,"AreaBattle/Resources/EnterGameUI/defaultIconTexture.png"),png);url=new Uri(png).AbsoluteUri;bad=new Uri(Path.Combine(path,"missing-avatar.png")).AbsoluteUri;
                Compose();Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static bool Ready()=>f.Trace.Contains("open")&&f.Page.IconInitialization!=null&&f.Page.IconInitialization.IsCompleted&&f.Effects.Module.Effects.Values.All(e=>e.IsLoaded);
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>60)throw new TimeoutException("avatar phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0){
                    if(!Ready())return;
                    Sprite cached=null;runtime.Instance.GetAvatar("default",s=>cached=s);
                    Check(cached==Resources.Load<Sprite>("EnterGameUI/defaultIcon")&&cached.vertices.Length==7&&cached.texture.width==128&&requests.Count==0,"Original default resource is cached and returned synchronously without network");
                    platform.SetString("myNickName","头像恢复验证");platform.SetString("myAvatarUrl",url);prefs.SetInt("AvatarValidation",47);
                    f.Messages.SendMessage("Avatar_Refresh_OnAuth",new object[]{"explicit-test-auth","explicit-test-url"});
                    runtime.Instance.GetAvatar(url,s=>sharedCallback++);
                    Check(requests.Count==1&&runtime.Instance.Downloading.Count==1&&runtime.Instance.SpriteCallbacks[url].GetInvocationList().Length==2,"Two native avatar consumers share one real UnityWebRequestTexture request");
                    Check(match.Manager.Data.rankData.nick=="头像恢复验证"&&match.Manager.Data.rankData.url==url&&match.Manager.Data.rankData.isGetWechatInfo&&f.Page.MatchNameText.text=="头像恢复验证","Actual preference wrapper and TopInfo auth update real Match fields before download completion");
                    Check(f.Trace.Contains("actual-userdata-saved")&&f.Account.Statistics.Strings.GetString("UserData.txt","").Contains("AvatarValidation")&&LitJson.JsonMapper.ToObject<OutgameMatchData>(f.Account.Stored("MatchManager")).rankData.url==url,"Actual UserDataPrefs and Match pool files are saved in source auth path");Phase(1);return;
                }
                if(phase==1){
                    if(!requests[0].Request.isDone||!runtime.Instance.Sprites.ContainsKey(url))return;
                    var sprite=runtime.Instance.Sprites[url];
                    Check(requests[0].Succeeded&&sprite.texture.width==128&&sprite.rect.width==128&&sprite.pixelsPerUnit==100&&f.Page.WechatHead.sprite==sprite&&sharedCallback==1&&refreshes==1,"Real native texture decode creates source100-PPU sprite and delivers both callbacks before refresh");
                    Check(!runtime.Instance.SpriteCallbacks.ContainsKey(url)&&runtime.Instance.Downloading.Contains(url),"Successful callbacks are removed while original download marker remains");
                    Sprite again=null;runtime.Instance.GetAvatar(url,s=>again=s);Check(again==sprite&&requests.Count==1,"Subsequent lookup is synchronous from actual native sprite cache");
                    runtime.Instance.GetAvatar(bad,s=>throw new Exception("failed request must not call back"));Phase(2);return;
                }
                if(phase==2){
                    if(!requests[1].Request.isDone||!f.Trace.Any(s=>s.StartsWith("Error downloading image: ")))return;
                    Check(!requests[1].Succeeded&&!runtime.Instance.Sprites.ContainsKey(bad)&&runtime.Instance.SpriteCallbacks.ContainsKey(bad)&&refreshes==1,"Actual missing-file transport failure retains waiters and emits no fallback or refresh");
                    runtime.Instance.GetAvatar(bad,null);Check(requests.Count==2&&runtime.Instance.Downloading.Contains(bad),"A later failed-URL request is deduplicated rather than automatically retried");
                    Preview("Default",runtime.Instance.Sprites["default"],-200);Preview("Downloaded",runtime.Instance.Sprites[url],200);Canvas.ForceUpdateCanvases();
                    report.capture=Path.Combine(BattleBuild.Workspace,"analysis/captures/wx-avatar-native.png");Directory.CreateDirectory(Path.GetDirectoryName(report.capture));ScreenCapture.CaptureScreenshot(report.capture);Phase(3);return;
                }
                if(phase==3){
                    if(!File.Exists(report.capture))return;
                    f.CloseRegistry.CloseForName(OutgameTopInfoPage.SourceName);Phase(4);return;
                }
                if(phase==4){
                    if(f.Page.Closing==null||!f.Page.Closing.IsCompleted||f.Page.GameObject)return;f.Page.Closing.GetAwaiter().GetResult();
                    Check(f.Effects.Module.Effects.Count==0&&f.Provider.RefCount==0,"Native page close releases owned effects/page handle after actual avatar callback");
                    Cleanup();Compose();Phase(5);return;
                }
                if(phase==5){
                    if(!Ready())return;report.restoredNickname=match.Manager.Data.rankData.nick;report.restoredAvatar=match.Manager.Data.rankData.url;report.restoredAuthorization=match.Manager.Data.rankData.isGetWechatInfo;report.restoredPreference=prefs.GetInt("AvatarValidation");
                    Check(report.restoredNickname=="头像恢复验证"&&report.restoredAvatar==url&&report.restoredAuthorization&&report.restoredPreference==47,"Independent native account restart restores real Match authorization and separate UserDataPrefs record");
                    Check(runtime.GetMyNickname()=="头像恢复验证"&&runtime.GetMyAvatarUrl()==url&&runtime.Instance.Sprites.Count==1&&runtime.Instance.Downloading.Count==0,"Fresh platform preference/cache owner restores raw preference values without persisting download caches");
                    Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        // Diagnostic previews belong to this validation scene, outside the original page.
        static void Preview(string label,Sprite sprite,float x)
        {
            var image=new GameObject("ValidationAvatar"+label,typeof(RectTransform),typeof(Image)).GetComponent<Image>();image.transform.SetParent(f.Layer,false);var canvas=image.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=32000;image.sprite=sprite;image.preserveAspect=true;image.rectTransform.sizeDelta=new Vector2(256,256);image.rectTransform.anchoredPosition=new Vector2(x,0);
            var text=new GameObject("Caption",typeof(RectTransform),typeof(Text)).GetComponent<Text>();text.transform.SetParent(image.transform,false);text.font=f.Page.NameText.font;text.text=label;text.fontSize=30;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;text.rectTransform.sizeDelta=new Vector2(300,80);text.rectTransform.anchoredPosition=new Vector2(0,-170);
        }
        static void Cleanup()
        {
            f?.Dispose();f=null;
            // Validation cleanup owns native requests/textures; original runtime intentionally has no Dispose.
            if(runtime!=null){foreach(var row in runtime.Instance.Sprites)if(row.Key!="default"&&row.Value)UnityEngine.Object.Destroy(row.Value);foreach(var row in runtime.Instance.Textures)if(row.Key!="default"&&row.Value&&row.Value!=runtime.Instance.Textures["default"])UnityEngine.Object.Destroy(row.Value);}
            foreach(var request in requests)request.Request.Dispose();requests.Clear();runtime=null;
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=error==null;report.error=error?.ToString();
            try{Cleanup();}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/wx-avatar-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_WX_AVATAR_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
