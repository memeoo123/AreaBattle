using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTimeToRefreshPlayModeValidation
    {
        const string Pending="AreaBattle.TimeToRefreshNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Actual source refresh singleton/Unity Update, offnet readiness coroutine, compressed file save/restart, ExitGame and scaled await cleanup. SDK clock values/result messages are explicit test inputs; no production Main, platform response or Player claim.";
            public List<string> checks=new List<string>();
        }
        static Report report;static OutgameStatisticsOffNetValidation.Fixture f;static OutgameTimeToRefreshControl refresh;
        static int phase,frame;static double started,phaseAt;static bool stopped;static string path;static long first,day3,day4;
        static OutgameTimeToRefreshPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string why){if(!value)throw new Exception(why);report.checks.Add(why);}
        static void SetPhase(int next){phase=next;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Bind()
        {
            var owner=f;
            OutgameTimeToRefreshControl.Services=new OutgameRefreshServices{IsReleaseVersion=()=>owner.Release,ServerTime=()=>owner.ServerTime,LocalNow=()=>owner.LocalNow,
                Messages=()=>owner.Messages,GetDisposableActions=()=>owner.Disposables,SetDisposableActions=a=>owner.Disposables=a};
            OutgameTimeToRefreshControl.BindStatistics(owner.Services);
        }
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;
            try
            {
                day3=OutgameTimeToRefreshValidation.Stamp(new DateTime(2026,10,3));day4=day3+86400000L;
                Time.timeScale=0;f=new OutgameStatisticsOffNetValidation.Fixture(native:true);path=f.PathName;f.ServerTime=day3+12*3600000L;Bind();
                f.Pool.SourceReadyFlag=false;f.Init();first=f.Strategy.Data.firstStartTimeStamp;SetPhase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try
            {
                if(EditorApplication.timeSinceStartup-started>45)throw new TimeoutException("refresh phase "+phase);
                if(Time.frameCount<=frame+2||EditorApplication.timeSinceStartup-phaseAt<.2)return;
                if(phase==0)
                {
                    Check(OutgameTimeToRefreshControl.Groups.Count==0&&f.Manager.GetEventStatistics(10902)==0,"Actual pool readiness prevents scheduler registration and launch count");
                    f.Pool.SourceReadyFlag=true;SetPhase(1);return;
                }
                if(phase==1)
                {
                    if(OutgameTimeToRefreshControl.Groups.Count==0||f.Owner.IsDirty)return;
                    refresh=OutgameTimeToRefreshControl.Instance;
                    Check(ReferenceEquals(refresh,OutgameTimeToRefreshControl.Instance)&&refresh.name=="TimeToRefreshControl"&&f.Disposables.GetInvocationList().Length==2,"Native singleton root reuses one component and one lifecycle subscription");
                    Check(f.Manager.GetEventStatistics(10902)==1&&f.Manager.GetEventStatistics(10901)==0&&refresh.Elapsed==0,"Registration finishes during pause but scheduler waits for scaled time");
                    f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{true});Time.timeScale=1;SetPhase(2);return;
                }
                if(phase==2)
                {
                    if(f.Manager.GetEventStatistics(10901)!=1||f.Owner.IsDirty)return;
                    Check(f.Strategy.Data.lastRefreshTimeStamp==day3&&f.Manager.GetEventStatistics(10005)==0&&f.Manager.GetEventStatistics(10002)==1,"First actual scheduler Update aligns midnight and resets daily count while retaining lifetime count");
                    var data=JsonUtility.FromJson<OutgameStatisticsJsonData>(OutgameStatisticsCodec.DecompressString(f.Stored,s=>throw new Exception(s)));
                    Check(data.lastRefreshTimeStamp==day3&&data.firstStartTimeStamp==first,"Automatic initial refresh persists through native owner Update and gzip file storage");
                    f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{true});f.Messages.SendMessage("Item_ItemChange",new object[]{71,9L});
                    f.ServerTime=day4;f.Messages.SendMessage("GF_ReceiveServerTime",new object[]{day4});SetPhase(3);return;
                }
                if(phase==3)
                {
                    if(f.Manager.GetEventStatistics(10901)!=2||f.Owner.IsDirty)return;
                    Check(f.Strategy.Data.lastRefreshTimeStamp==day4&&f.Manager.GetEventStatistics(10005)==0&&f.Manager.GetEventStatistics(10002)==2,"Actual next-day scheduler tick resets daily data exactly at the duration boundary");
                    var data=JsonUtility.FromJson<OutgameStatisticsJsonData>(OutgameStatisticsCodec.DecompressString(f.Stored,s=>throw new Exception(s)));
                    Check(data.lastRefreshTimeStamp==day4,"Cross-day timestamp committed without invoking refresh callback from the harness");
                    f.Messages.SendMessage("ExitGame");Check(OutgameTimeToRefreshControl.Groups.Count==0,"Real ordinary ExitGame message clears scheduler registry immediately");
                    f.Dispose();UnityEngine.Object.DestroyImmediate(refresh.gameObject);Check(f.Disposables==null,"Native destruction removes scheduler lifecycle delegate after owner release");
                    f=new OutgameStatisticsOffNetValidation.Fixture(path,native:true);f.ServerTime=day4;Bind();f.Pool.SourceReadyFlag=false;f.Init();
                    Check(f.Strategy.Data.lastRefreshTimeStamp==day4&&f.Strategy.Data.firstStartTimeStamp==first&&f.Manager.GetEventStatistics(10008,71)==9&&f.Manager.GetEventStatistics(10901)==2,"Independent owner graph restores automatic refresh timestamps and counts from compressed file");
                    f.Pool.SourceReadyFlag=true;SetPhase(4);return;
                }
                if(phase==4)
                {
                    if(EditorApplication.timeSinceStartup-phaseAt<2.3||f.Manager.GetEventStatistics(10902)!=2||f.Owner.IsDirty)return;
                    refresh=OutgameTimeToRefreshControl.Instance;
                    Check(f.Manager.GetEventStatistics(10901)==2&&OutgameTimeToRefreshControl.Groups.Count==1,"Restart registers fresh launch but actual scheduler ticks do not duplicate same-day reset");
                    Time.timeScale=0;f.Disposables();Check(OutgameTimeToRefreshControl.Groups.Count==1,"Disposal callback begins native scaled wait before clearing registry");SetPhase(5);return;
                }
                if(phase==5)
                {
                    if(EditorApplication.timeSinceStartup-phaseAt<.5)return;
                    Check(OutgameTimeToRefreshControl.Groups.Count==1&&f.Updates.HandleList.Count==0,"Scaled cleanup stays pending during pause while queued owner removal completes");
                    Time.timeScale=1;SetPhase(6);return;
                }
                if(phase==6)
                {
                    if(OutgameTimeToRefreshControl.Groups.Count!=0)return;
                    Check(refresh!=null&&f.Disposables!=null,"Native await clears registry after resume and retains source singleton/lifecycle subscription");
                    f.Manager.OnRelease();UnityEngine.Object.DestroyImmediate(refresh.gameObject);
                    Check(f.Disposables==null&&OutgameTimeToRefreshControl.Groups.Count==0,"Final destruction clears records and unregisters cleanup");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/time-refresh-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
