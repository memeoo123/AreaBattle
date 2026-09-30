using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAdsPlayModeValidation
    {
        const string Pending="AreaBattle.AdsPlayModeValidation";
        [Serializable] sealed class Report
        {
            public bool passed;public string scope="Actual Unity Play Mode, original ShopUI/config, native Button, real coroutine await; explicit SDK/report fixture, no account or user data";
            public List<string> checks=new List<string>();public string error;public int frames;public float elapsed,requestToPlatform;
        }
        sealed class Host:IOutgameVideoButtonHost
        {
            public OutgameSdkFunctionStates States;public OutgameSdkButtonRegistry Registry;public OutgameAdsVideoFlow Flow;public int starts,creates,readies,completions;public float platformAt;
            readonly OutgameVideoButtonManager config=new OutgameVideoButtonManager(()=>true,()=>false,OutgameVideoButtonManager.ReadRecoveredJson,x=>{},x=>{},x=>{});
            public OutgameVideoButtonData GetData(int id)=>config.GetData(id);
            public bool IsOpen(int function,bool refresh)=>States.IsOpen(function,refresh);
            public void BindFunctionButton(int function,OutgameVideoButton button){starts++;Registry.BindFunctionBtn(function,new UnityEngine.UI.Button[]{button});}
            public void CheckVideoIsReady()=>Registry.CheckVideoIsReady();
            public bool IsVideoReady()=>true;
            public void PlayAudio(int group,int audio){}
            public void Log(string text){}
            public void TrackVideo(int state){if(state==1)creates++;if(state==2)readies++;}
            public void Report(string kind,string label,string p1,string p2){}
            public string GameName=>"fixture";
            public void ReportEvent(string name,string value,bool once){}
            public void ShowVideo(int flag,Action<string,bool> complete,int id)=>Flow.ShowVide(flag,complete,id);
        }
        static Report report;static Host host;static OutgameVideoButton gold;static EventSystem events;
        static int phase,firstFrame;static float began,scaledBegan,clicked,unpaused;static bool showMessage;
        static OutgameAdsPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {
            if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");
            SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange change)
        {if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);report.checks.Add(message);}
        static void Start()
        {
            report=new Report();phase=0;showMessage=false;
            try{
                Time.timeScale=0;began=Time.realtimeSinceStartup;scaledBegan=Time.time;firstFrame=Time.frameCount;
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));page.SetActive(false);
                events=new GameObject("Validation EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
                host=new Host();var capabilities=new OutgameSdkCapabilityResolver(method=>method=="AdsManager.isVideoReady"?(host.IsVideoReady()?1:0):throw new InvalidOperationException("Unexpected platform fixture capability: "+method));host.States=new OutgameSdkFunctionStates(capabilities.GetState,id=>id.ToString(),text=>{});host.Registry=new OutgameSdkButtonRegistry(new Dictionary<int,UnityEngine.Events.UnityAction>(),host.IsOpen,host.States.Set,host.IsVideoReady,null);var messages=new OutgameMessageDispatcher();messages.AddListener("GF_ShowAdsVideo",args=>showMessage=true);
                var platformBridge=new OutgameXyxVideoBridge(host.IsVideoReady,done=>done(true),text=>{},text=>host.Flow.AfterVideo(text));
                host.Flow=new OutgameAdsVideoFlow(()=>Time.unscaledTime,OutgameUnityAwait.WaitRealtime,(flag,id)=>{
                    Check(showMessage&&id==1001&&flag==0,"Real await resumes to source show message then SDK call with original videoID");
                    host.platformAt=Time.realtimeSinceStartup;platformBridge.ShowVideoStatic(flag);
                },()=>messages,flag=>{},text=>{},playing=>{},text=>OutgameCallbackJson.DataParse(text,x=>{}));
                OutgameShopVideoBindings.Bind(page,host,c=>{},()=>{},()=>messages,new OutgameVideoPlayCooldown());
                gold=page.transform.Find("btn_addGold").GetComponent<OutgameVideoButton>();gold.AddVideoPlayCallBack(success=>{if(success)host.completions++;});
                page.SetActive(true);EditorApplication.update+=Poll;
            }catch(Exception error){Finish(error);}
        }
        static void Poll()
        {
            try{
                report.frames=Time.frameCount-firstFrame;report.elapsed=Time.realtimeSinceStartup-began;
                if(report.elapsed>15)throw new TimeoutException("Native advertisement timing validation timed out");
                if(phase==0&&report.frames>=2){
                    Check(host.starts>0&&gold.ButtonState==2,"Actual Start binds SDK function and readiness before pointer click");
                    clicked=Time.realtimeSinceStartup;gold.OnPointerClick(new PointerEventData(events){button=PointerEventData.InputButton.Left});
                    Check(host.completions==0&&!showMessage,"Native pointer schedules real wait without immediate SDK completion");phase=1;
                }
                if(phase==1&&Time.realtimeSinceStartup-clicked>=.7f){
                    Check(host.completions==1,"Raw SDK result resumes native completion callback while timeScale is zero");
                    report.requestToPlatform=host.platformAt-clicked;
                    Check(report.requestToPlatform>=.19f&&report.requestToPlatform<.7f,"Realtime SDK request delay is approximately0.2seconds");
                    Check(host.creates==0&&Time.time==scaledBegan,"Scaled button report coroutine stays suspended during pause");
                    Time.timeScale=1;unpaused=Time.time;phase=2;
                }
                if(phase==2&&Time.time-unpaused>=1.3f){
                    Check(host.creates>0&&host.readies>0,"Scaled one-second delayed report runs after game time resumes");
                    Finish(null);
                }
            }catch(Exception error){Finish(error);}
        }
        static void Finish(Exception error)
        {
            EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-ads-playmode-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
