using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameStatisticsOffNetPlayModeValidation
    {
        const string Pending="AreaBattle.StatisticsOffNetNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Concrete source offnet/manager/control, native UpdateManager/WaitUntil/coroutine handle, compressed file restart and explicit result messages. SDK time/http and daily-refresh scheduler endpoints are isolated fixtures; no source TimeToRefreshControl, production Main, actual platform response or Player claim.";
            public List<string> checks=new List<string>();public int readyFrame,restartFrame;public long originalFirst;
        }
        static Report report;static OutgameStatisticsOffNetValidation.Fixture f;
        static int phase,frame;static double started,phaseAt;static bool stopped;static long pausedNow;static string path;
        static OutgameStatisticsOffNetPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange value){if(value==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool ok,string why){if(!ok)throw new Exception(why);report.checks.Add(why);}
        static void SetPhase(int next){phase=next;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Start()
        {
            report=new Report();stopped=false;started=EditorApplication.timeSinceStartup;
            try{Time.timeScale=0;f=new OutgameStatisticsOffNetValidation.Fixture(native:true);path=f.PathName;f.Pool.SourceReadyFlag=false;f.Init(true);SetPhase(0);EditorApplication.update+=Poll;}
            catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;try{
                if(EditorApplication.timeSinceStartup-started>40)throw new TimeoutException("statistics phase "+phase);
                if(Time.frameCount<=frame+2||EditorApplication.timeSinceStartup-phaseAt<.15)return;
                if(phase==0)
                {
                    Check(f.Downloads==1&&f.Completions==0&&f.Manager.Data==null,"Native update does not fabricate pending server-data completion");
                    f.Manager.UpdateDataCallBack("");report.originalFirst=f.Strategy.Data.firstStartTimeStamp;
                    Check(f.Completions==1&&f.Owner.ValueProviders.Count==19&&f.Strategy.RefreshCoroutine!=null,"Actual load publishes manager/provides completion and starts native readiness coroutine");
                    pausedNow=f.Strategy.NowTimestamp();SetPhase(1);return;
                }
                if(phase==1)
                {
                    Check(f.Registrations==0&&f.Manager.GetEventStatistics(10902)==0,"Actual WaitUntil remains pending while pool save readiness is false");
                    Check(f.Strategy.NowTimestamp()>pausedNow&&f.Strategy.SecondElapsed==0&&f.Strategy.OnlineElapsed==0,"Unscaled anchored time advances during timeScale0 while scaled statistics timers remain frozen");
                    f.Pool.SourceReadyFlag=true;SetPhase(2);return;
                }
                if(phase==2)
                {
                    if(f.Registrations==0||f.Owner.IsDirty)return;
                    report.readyFrame=Time.frameCount;
                    Check(f.Registrations==1&&f.Manager.GetEventStatistics(10902)==1&&f.Strategy.RefreshCoroutine!=null,"Native coroutine registers once, increments launch count and retains completed handle");
                    Check(f.Stored.Length>0&&!f.Stored.StartsWith("{"),"Native control Update saves compressed data through real file backend");
                    f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{false});Check(f.Manager.GetEventStatistics(10002)==0,"Explicit failed ad result does not grant successful-ad count");
                    f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{true});f.Messages.SendMessage("Item_ItemChange",new object[]{71,9L});
                    f.ServerTime+=2*86400000L;f.Messages.SendMessage("GF_ReceiveServerTime",new object[]{f.ServerTime});f.Refresh(f.ServerTime);
                    Check(f.Manager.GetEventStatistics(10002)==1&&f.Manager.GetEventStatistics(10005)==0&&f.Manager.GetEventStatistics(10900)==2,"Explicit refresh endpoint runs concrete daily reset after result messages");
                    SetPhase(3);return;
                }
                if(phase==3)
                {
                    if(f.Owner.IsDirty)return;
                    var saved=JsonUtility.FromJson<OutgameStatisticsJsonData>(OutgameStatisticsCodec.DecompressString(f.Stored,s=>throw new Exception(s)));
                    Check(saved.lastRefreshTimeStamp==f.ServerTime&&saved.firstStartTimeStamp==report.originalFirst,"Native save commits both source timestamps");
                    f.Manager.UpdateDataCallBack(f.Stored);Check(f.Completions==1&&f.Registrations==1,"Reload retains completed coroutine handle and does not repeat initialization or daily registration");
                    f.Dispose();f=new OutgameStatisticsOffNetValidation.Fixture(path,native:true);f.Pool.SourceReadyFlag=false;f.Init();report.restartFrame=Time.frameCount;
                    Check(f.Manager.GetEventStatistics(10008,71)==9&&f.Manager.GetEventStatistics(10901)==1&&f.Strategy.Data.firstStartTimeStamp==report.originalFirst,"Independent native owner graph restores file statistics after teardown");
                    SetPhase(4);return;
                }
                if(phase==4)
                {
                    Check(f.Registrations==0&&f.Completions==1,"Restart creates its own pending native readiness coroutine");
                    f.Pool.SourceReadyFlag=true;Time.timeScale=1;SetPhase(5);return;
                }
                if(phase==5)
                {
                    if(EditorApplication.timeSinceStartup-phaseAt<1.3||f.Manager.GetEventStatistics(10902)!=2)return;
                    Check(f.Registrations==1&&f.Strategy.SecondElapsed<1.1f&&f.Manager.GetEventStatistics(10000)>f.ServerTime,"Native scaled update resumes periodic clock write and fresh launch count");
                    f.Owner.Dispose();f.Manager.OnRelease();Check(f.Refresh==null&&f.Strategy.RefreshCoroutine==null&&f.Updates.HandleList.Count==1,"Release removes daily endpoint/stops retained coroutine and queues controller removal");
                    SetPhase(6);return;
                }
                if(phase==6){Check(f.Updates.HandleList.Count==0,"Next actual UpdateManager frame removes controller delegate");Finish(null);}
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/statistics-offnet-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
