using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameFrameLaunchValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class Ads:IOutgameFrameAds
        {public Action<string> Set;public Action Hide;public string SourceName12{set=>Set(value);}public void HideBanner()=>Hide();}
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();public readonly List<string> Trace=new List<string>();
            public readonly OutgameFrameWorkMono Mono;public readonly OutgameFrameMonoServices Services;
            public int Sample=10,RandomCalls,Saves;public string Config="20";public bool Release=true;
            public Fixture()
            {
                Services=new OutgameFrameMonoServices{Messages=()=>Messages,Log=a=>Trace.Add((string)a[0]),ReleaseLog=a=>Trace.Add((string)a[0]),IsReleaseVersion=()=>Release,
                    CheckSdkLogSupport=()=>Trace.Add("sdk-log-support"),GetNetworkSample=()=>Sample,SetNetworkSample=v=>{Sample=v;Trace.Add("sample-set");},SavePreferences=()=>{Saves++;Trace.Add("save");},
                    OnlineConfig=key=>{Require(key=="net_status_game_statistic_filter","source online key");return Config;},RandomInclusive=(a,b)=>{Require(a==1&&b==100,"inclusive source bounds");RandomCalls++;return 50;}};
                OutgameFrameWorkMono.Services=Services;Mono=new GameObject("GameFrameWorkMono").AddComponent<OutgameFrameWorkMono>();
            }
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Mono.gameObject);Time.timeScale=1;}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original launch/settings/pause/focus and sampling paths; native singleton and actual TimeModule checked separately. Transition/banner/SDK/scene endpoints explicit; real Main/GameFrameworkLoad/platform and audiovisual acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("frame-launch-pause-priority-escalation-and-retained-resume-source",()=>{
                using(var f=new Fixture())
                {
                    var payloads=new List<object[]>();f.Messages.AddListener("GF_NewGamePause",a=>payloads.Add(a));f.Mono.IsInitialized=true;
                    f.Mono.OnApplicationFocus(false);f.Mono.OnApplicationPause(true);f.Mono.ActivePause(true);
                    Require(payloads.Count==1&&(bool)payloads[0][0]&&payloads[0][1] is OutgamePauseSource&&f.Mono.PauseSource==OutgamePauseSource.ActiveTriggerPause,"higher pause source updates without duplicate notification");
                    f.Mono.OnApplicationFocus(true);f.Mono.OnApplicationPause(false);Require(f.Mono.PauseState==1&&payloads.Count==1,"lower resume cannot clear active pause");
                    f.Mono.ActivePause(false);Require(f.Mono.PauseState==0&&payloads.Count==2&&(OutgamePauseSource)payloads[1][1]==OutgamePauseSource.ActiveTriggerPause,"resume retains old source in payload");
                    f.Mono.SetPause(true,OutgamePauseSource.AppFocus);f.Mono.SetPause(false,OutgamePauseSource.ActiveTriggerPause);
                    Require((OutgamePauseSource)payloads[3][1]==OutgamePauseSource.AppFocus&&f.Trace[f.Trace.Count-1].Contains("当前状态来源[AppFocus]"),"higher resume logs both sources but publishes old source");
                }
            });
            check("frame-launch-focus-before-pause-gate-and-callback-failure",()=>{
                using(var f=new Fixture())
                {
                    f.Messages.AddListener("GF_GameFocus",a=>f.Trace.Add("focus:"+a[0]));f.Messages.AddListener("GF_NewGamePause",a=>f.Trace.Add("pause:"+a[0]));
                    f.Mono.OnApplicationFocus(false);f.Mono.OnApplicationPause(true);f.Mono.ActivePause(true);Require(f.Trace.Count==0&&f.Mono.HasFocus,"uninitialized Unity entry points do nothing");
                    f.Mono.IsInitialized=true;f.Mono.OnApplicationFocus(false);f.Mono.OnApplicationFocus(false);Require(string.Join(",",f.Trace)=="focus:False,pause:True","focus transition precedes pause and repeated inputs suppressed");
                    f.Messages.AddListener("GF_GameFocus",a=>throw new InvalidOperationException());Throws<InvalidOperationException>(()=>f.Mono.OnApplicationFocus(true));Require(f.Mono.HasFocus&&f.Mono.PauseState==1,"focus commits before callback error prevents pause handling");
                }
            });
            check("frame-launch-pause-log-reentry-and-invalid-state",()=>{
                using(var f=new Fixture())
                {
                    var values=new List<bool>();f.Messages.AddListener("GF_NewGamePause",a=>values.Add((bool)a[0]));f.Mono.SetPause(true,OutgamePauseSource.AppFocus);
                    f.Services.Log=a=>f.Mono.SetPause(true,OutgamePauseSource.ActiveTriggerPause);f.Mono.SetPause(false,OutgamePauseSource.AppFocus);
                    Require(values.Count==3&&values[2]&&f.Mono.PauseState==1,"resume publishes live state after log reentry");
                    f.Mono.PauseState=7;f.Mono.PauseSource=OutgamePauseSource.AppFocus;f.Mono.SetPause(true,OutgamePauseSource.AppPause);f.Mono.SetPause(false,OutgamePauseSource.ActiveTriggerPause);
                    Require(f.Mono.PauseState==7&&f.Mono.PauseSource==OutgamePauseSource.AppPause&&values.Count==3,"nonzero unknown state can change source without state notification");
                }
            });
            check("frame-launch-network-sample-initialization-threshold-and-sticky-disable",()=>{
                using(var f=new Fixture())
                {
                    f.Sample=-1;f.Config=null;f.Mono.ConfigureNetworkSampling();Require(f.Sample==50&&f.Saves==1&&f.RandomCalls==1&&!f.Mono.NetworkSamplingEnabled,"new inclusive sample saved once and compared with default20");
                    Require(string.Join(",",f.Trace)=="sample-set,save,网络随机上报1:50,网络随机上报2:20","source sampler save/log order");
                    f.Config="100";f.Mono.ConfigureNetworkSampling();Require(!f.Mono.NetworkSamplingEnabled&&f.Saves==1&&f.RandomCalls==1,"later high threshold does not re-enable");
                    f.Mono.NetworkSamplingEnabled=true;f.Config="bad";f.Mono.ConfigureNetworkSampling();Require(!f.Mono.NetworkSamplingEnabled&&f.Trace[f.Trace.Count-1]=="网络随机上报2:0","nonempty invalid config TryParse sets threshold0");
                    f.Sample=-2;f.Config="";f.Mono.NetworkSamplingEnabled=true;f.Mono.ConfigureNetworkSampling();Require(f.RandomCalls==1&&f.Mono.NetworkSamplingEnabled,"only exact-1 triggers sampling; empty config keeps20");
                }
            });
            check("frame-launch-network-save-failure-preserves-selected-value",()=>{
                using(var f=new Fixture())
                {
                    f.Sample=-1;f.Services.SavePreferences=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Mono.ConfigureNetworkSampling);
                    Require(f.Sample==50&&f.Trace.Count==1&&f.Mono.NetworkSamplingEnabled,"selection assigned before failed save; no logs or comparison afterward");
                }
            });
            check("frame-launch-common-settings-order-dpi-and-source-ads-reference",()=>{
                using(var f=new Fixture())
                {
                    // CommonSettings obtains the actual singleton; native ownership rules tested separately.
                    var frame=new OutgameFrameEntry(new OutgameFrameServices());var ads=new Ads{Set=s=>f.Trace.Add("ads-set:"+s),Hide=()=>{}};
                    frame.Launch=new OutgameFrameLaunchServices{InitializeSdk=()=>f.Trace.Add("sdk"),ReadDpi=()=>0,SetTargetFrameRate=v=>f.Trace.Add("fps:"+v),SetSleepTimeout=v=>f.Trace.Add("sleep:"+v),SetCulture=()=>f.Trace.Add("culture"),
                        Ads=()=>{f.Trace.Add("ads-get");return ads;},MineGameName=()=>{f.Trace.Add("game");return "Proj_hdzd";},SettingString48=()=>{f.Trace.Add("setting");return "source";}};
                    // Arrange existing singleton through its native accessor; production settings always call it.
                    frame.CommonSettings();var singleton=OutgameFrameWorkMono.Instance;
                    Require(OutgameFrameEntry.Dpi==96&&string.Join(",",f.Trace)=="sdk,fps:60,sleep:-1,culture,ads-get,game,setting,ads-set:Proj_hdzd,source,网络随机上报1:10,网络随机上报2:20","full source common-settings order");
                    frame.Launch.ReadDpi=()=>float.NaN;frame.CommonSettings();Require(float.IsNaN(OutgameFrameEntry.Dpi),"NaN is not normalized by <=0");UnityEngine.Object.DestroyImmediate(singleton);
                }
            });
            check("frame-launch-common-settings-null-ads-keeps-earlier-side-effects",()=>{
                using(var f=new Fixture())
                {
                    var frame=new OutgameFrameEntry(new OutgameFrameServices());frame.Launch=new OutgameFrameLaunchServices{InitializeSdk=()=>{},ReadDpi=()=>100,SetTargetFrameRate=v=>{},SetSleepTimeout=v=>{},SetCulture=()=>{},Ads=()=>null,MineGameName=()=>{f.Trace.Add("game");return null;},SettingString48=()=>{f.Trace.Add("setting");return null;}};
                    Throws<NullReferenceException>(frame.CommonSettings);Require(string.Join(",",f.Trace)=="game,setting"&&OutgameFrameEntry.Dpi==100,"reads both strings before null ads assignment; sampler never runs");
                }
            });
            check("frame-launch-start-game-deferred-transition-and-state-order",()=>{
                var frame=new OutgameFrameEntry(new OutgameFrameServices()){GameMenuSceneName="old",IsGoGameMenu=true};var messages=new OutgameMessageDispatcher();var order=new List<string>();Action done=null;
                var ads=new Ads{Set=s=>{},Hide=()=>{Require(frame.IsGoGameMenu,"go-menu still true at banner hide");order.Add("banner");}};
                messages.AddListener("ReadyExitGame",a=>{Require(a==null&&frame.GameMenuSceneName=="menu","menu committed before null-payload ReadyExitGame");order.Add("ready");});
                frame.Launch=new OutgameFrameLaunchServices{HideTransition=a=>done=a,Messages=()=>messages,Ads=()=>ads,SetGlobalGameName=s=>{Require(!frame.IsGoGameMenu&&s=="game","flag cleared before gameName assignment");order.Add("name");},LoadScene=s=>{Require(s=="GameFrameworkLoad","exact source load scene");order.Add("scene");}};
                frame.StartGame("game","menu");Require(frame.GameMenuSceneName=="old"&&order.Count==0,"transition callback genuinely pending");done();Require(string.Join(",",order)=="ready,banner,name,scene","source transition completion order");
                frame.Launch.Ads=()=>null;frame.StartGame("game",null);done();Require(frame.GameMenuSceneName=="menu"&&order.Count==7,"empty menu retained, missing banner owner skipped");
            });
            check("frame-launch-start-game-message-failure-preserves-menu-only",()=>{
                var frame=new OutgameFrameEntry(new OutgameFrameServices()){GameMenuSceneName="old",IsGoGameMenu=true};var messages=new OutgameMessageDispatcher();messages.AddListener("ReadyExitGame",a=>throw new InvalidOperationException());
                frame.Launch=new OutgameFrameLaunchServices{HideTransition=a=>a(),Messages=()=>messages,Ads=()=>throw new Exception("must not reach ads")};
                Throws<InvalidOperationException>(()=>frame.StartGame("game","new"));Require(frame.GameMenuSceneName=="new"&&frame.IsGoGameMenu,"callback exception keeps committed menu and skips flag/scene");
            });
            check("frame-launch-domain-public-lookup-last-match-and-null-values",()=>{
                var data=new OutgameDomainData();Require(data.Datas.Count==0&&data.GetValue("g",2,"b")=="","source empty list constructor/default string");
                data.Datas.Add(new OutgameDomainConfig{GN="g",BN="b",D="unused",TD="first",RD="release"});data.Datas.Add(new OutgameDomainConfig{GN="g",BN="b",TD=null,RD="last"});
                Require(data.GetValue("g",2,"b")==null&&data.GetValue("g",3,"b")=="last"&&data.GetValue("g",1,"b")=="","only TD/RD public types2/3; last match including null wins");
                data.Datas.Add(null);Throws<NullReferenceException>(()=>data.GetValue("other",2,"b"));
            });
            return report;
        }
    }
}
