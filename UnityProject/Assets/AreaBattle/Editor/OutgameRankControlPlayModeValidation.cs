using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTopInfoPageValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameRankControlPlayModeValidation
    {
        const string Pending="AreaBattle.RankControlNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error,capture,restoredName;public int restoredGold,restoredDiamonds;
            public string scope="Full original RankControl lifecycle and100-row home/settlement data with actual RankManager/UserInfo/config/pool and native TopInfo/account restart. Original RankUI/OverUI rendering and server transmitter/platform/Main remain pending; country transport stays pending and Match callbacks are explicit fixtures.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static Camera camera;static OutgameUiControl control;static string path;static int phase,frame;static double started,phaseAt;static bool stopped;
        static OutgameRankControl rank;static OutgameMatchControl match;static OutgameMatchValidation.Transmitter transmitter;
        static GameObject oldRoot;static OutgameBaseEffect gold,diamond;
        static OutgameRankControlPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){OutgameAccountRewardsValidation.PrepareConfig();var bundle=OutgameTopInfoAssetsValidation.Bundle;SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"),false,"Game").Show();EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool b,string why){if(!b)throw new Exception(why);report.checks.Add(why);}
        static void Phase(int value){phase=value;frame=Time.frameCount;phaseAt=EditorApplication.timeSinceStartup;}
        static void Click(GameObject go)=>ExecuteEvents.Execute(go,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Compose()
        {
            f=new Fixture(true,path);match=OutgameMatchValidation.AttachTopInfo(f);rank=OutgameRankControlValidation.AttachTopInfo(f,out var rankServices,out var rankHome);rank.OnInit();transmitter=new OutgameMatchValidation.Transmitter();match.Transmitter=transmitter;f.Effects.Camera=camera;
            var canvas=f.Effects.Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=10;
            var scaler=f.Effects.Root.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=1;f.Effects.Root.AddComponent<GraphicRaycaster>();Canvas.ForceUpdateCanvases();
            var roots=new OutgameUiRootInitialization(f.Effects.Root,()=>true,()=>false,()=>{});roots.Loaded((n,a)=>{var g=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));g.SetActive(a);return g;});
            f.Layer.SetParent(roots.UiRoot.Find("UIPopup"),false);var rect=(RectTransform)f.Layer;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            // Native fixture resolves the two default source sprites by their retained object IDs.
            f.Services.SetSprite=(im,name,atlas,mode)=>{
                if(atlas!="Headport"||mode!=0)throw new Exception("source sprite request");
                string id=name=="Tx_man01"?"2720706472938528916":name=="Tx_frame01_small"?"-5557301240740203688":throw new Exception("fixture covers default source head/frame only");
                var sprite=Resources.Load<Sprite>("Recovered/TopInfo/Sprites/CAB-a79877fee73a97ed0be4eacb186a645b_"+id);if(!sprite)throw new Exception("original default sprite missing");im.sprite=sprite;
            };
            control=new OutgameUiControl(null,f.Account.Page.Data.Registry,()=>f.Messages,new OutgameUiControlGlobals(),null,()=>f.Open(),null,null,null);
            control.ShowTopInfoUI();
        }
        static void Start()
        {
            stopped=false;report=new Report();started=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                OutgameAccountRewardsValidation.LoadPreparedConfig();OutgameTopInfoAssetsValidation.LoadPrepared();new GameObject("EventSystem",typeof(EventSystem));
                camera=new GameObject("UICamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=960;camera.farClipPlane=3000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.08f,.12f);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleRankControlNative-"+Guid.NewGuid().ToString("N"));Compose();Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-started>60)throw new TimeoutException("top page phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0){
                    if(!f.Trace.Contains("open")||f.Page.IconInitialization==null||!f.Page.IconInitialization.IsCompleted)return;
                    gold=f.Effects.Module.Get(f.Page.GoldEffectId);diamond=f.Effects.Module.Get(f.Page.DiamondEffectId);if(!gold.IsLoaded||!diamond.IsLoaded)return;
                    Check(f.Page.OutletCount==25&&f.Page.UserRoot.activeSelf&&!f.Page.MatchUserRoot.activeSelf&&!f.Page.StaminaRoot.activeSelf,"Native owned load binds25 source outlets and executes original Awake visibility");
                    Check(f.ReadyEvents==1&&f.Page.GoldEffectId!=-1&&f.Page.DiamondEffectId!=-1&&f.Effects.Module.Effects.Count==2,"Native BaseUI WaitUntil emits page-ready then starts both original persistent effects");
                    Check(gold.Parent==f.Page.GoldIcon.transform&&diamond.Parent==f.Page.DiamondIcon.transform&&f.Page.GoldRoot.GetComponent<Canvas>()&&f.Page.DiamondRoot.GetComponent<Canvas>(),"Owned page OpenLater installs source canvases and effects retain image parents");
                    Check(control.TopInfo==f.Page&&control.GoldText==f.Page.GoldText&&control.DiamondsText==f.Page.DiamondsText,"Actual UIControl resolves deferred currency outlets from the owned page");
                    f.Account.Account.Tool.ToolChange(1001,37,true,"",true);f.Account.Account.Tool.ToolChange(1002,5,true,"",true);
                    Check(f.Page.GoldText.text=="37"&&f.Page.DiamondsText.text=="5"&&f.Account.Account.Local.GoldNum==37&&f.Account.Account.Local.DiamondsNum==5,"Real account tool mutations immediately refresh native currency texts and persist through shared pool");
                    Check(rank.listRankItemData.Data.Count==100&&rank.playerListIndex==50&&rank.dic_userui.Count==0,"Full RankControl OnInit creates100 source home rows through actual controller registry");
                    Check(rank.GetCurPlayerCountryInfo()=="country_com"&&rank.listRankItemData.Data[50].countryN=="","Pending actual country port preserves generic player flag and source empty home-country field");
                    rank.CurPlayerScore=123456;f.Messages.SendMessage("LoadStartingUI");
                    Check(f.Page.ScoreText.text=="1.23K","Actual RankControl BigInteger drives native TopInfo score label");
                    f.Account.Statistics.Pool.SaveData();
                    Check(rank.CurPlayerScore==123456&&rank.Manager.Data.curPlayerScore=="123000","Real RankManager save follows original display-shortening expansion without mutating live score");
                    int oldRank=rank.CurPlayerRank;rank.AddScore();f.Messages.SendMessage("LoadStartingUI");
                    Check(rank.CurPlayerScore==138456&&f.Page.ScoreText.text=="1.38K"&&rank.CurPlayerRank<oldRank,"Source score reward and configured rank improvement reach actual native label");
                    rank.ReduceScore();f.Messages.SendMessage("LoadStartingUI");f.Account.Statistics.Pool.SaveData();
                    Check(rank.CurPlayerScore==128456&&f.Page.ScoreText.text=="1.28K"&&rank.Manager.Data.curPlayerScore=="639999","Source loss and explicit save retain original float-expansion record639999");
                    var oldRows=rank.listRankItemData.Data.ToArray();rank.InitHomeInfo();
                    Check(rank.listRankItemData.Data.SequenceEqual(oldRows)&&rank.listRankItemData.Data[50].score=="1.28K"&&rank.listRankItemData.Data[50].rankIndex==rank.CurPlayerRank,"Native home refresh reuses all100 FIFO row identities and reads live score/rank");
                    var ai=rank.GetOverUIRankAIData(3,false,rank.CurPlayerRank,rank.CurPlayerScore,0);
                    Check(ai.Count==3&&ai[0].rankIndex==rank.CurPlayerRank+1&&ai.All(x=>f.Config.dicHeadBox.ContainsKey(x.headBoxId)),"Native settlement data uses original AI names/gaps/head frames and real reference pool");
                    f.User.ApplyName("You");f.Messages.SendMessage("LoadStartingUI");Check(f.Page.NameText.text=="You"&&f.Page.NameRed.activeSelf&&f.Page.HeadIcon.sprite&&f.Page.HeadBoxIcon.sprite,"Real profile name event refreshes red dot and resolves original head/frame sprites");
                    Click(f.Page.UserRoot);Check(f.UserOpens==1,"Native profile pointer reaches source empty-argument UserInfoUI request endpoint");
                    f.Messages.SendMessage("Avatar_Refresh_OnAuth",new object[]{"test-event-name","test-event-url"});Check(match.Manager.Data.rankData.isGetWechatInfo&&match.Manager.Data.rankData.nick=="platform-nickname"&&f.Page.MatchNameText.text=="platform-nickname"&&f.AvatarRequested=="platform-avatar"&&f.UserSaves==1,"Explicit test authorization event uses separate platform endpoints and source Match/name/save order");
                    var sprite=f.Page.HeadIcon.sprite;f.AvatarCallback(sprite);Check(f.Page.WechatHead.sprite==sprite,"Deferred supplied avatar callback updates the exact native WeChat image");
                    Check(match.Manager.Data.rankData.url=="platform-avatar"&&LitJson.JsonMapper.ToObject<OutgameMatchData>(f.Account.Stored("MatchManager")).rankData.isGetWechatInfo,"Native auth saves real Match fields through the same account pool");
                    match.OnGamePlayerState(new object[]{8});
                    Check(transmitter.Requests.Count==3&&transmitter.Requests[0].rank==2&&transmitter.Requests[1].rank==3&&transmitter.Requests[2].rank==1&&match.Manager.Data.rankData.playNum==1&&match.Manager.Data.rankData.VN_t==5,"Native match emits day/week/total and increments play count before response");
                    transmitter.SendResponseCallback();
                    Check(match.Manager.Data.rankData.VN_t==6&&transmitter.SendResponseCallback==null&&LitJson.JsonMapper.ToObject<OutgameMatchData>(f.Account.Stored("MatchManager")).rankData.VN_t==5,"Explicit test response changes live wins, preserving source absence of automatic save");
                    match.OnGamePlayerState(new object[]{9});transmitter.SendResponseCallback();
                    Check(transmitter.Requests.Count==6&&match.Manager.Data.rankData.DN_t==1&&match.Manager.Data.rankData.playNum==2,"Explicit loss response updates all captured loss counters");
                    f.Account.Statistics.Pool.SaveData();
                    f.User.ApplyName("恢复测试");f.Messages.SendMessage("LoadStartingUI");Check(!f.Page.NameRed.activeSelf&&f.Page.NameText.text=="恢复测试","Saved nondefault nickname clears the original red dot");
                    report.capture=Path.Combine(BattleBuild.Workspace,"analysis/captures/rank-control-native.png");Directory.CreateDirectory(Path.GetDirectoryName(report.capture));ScreenCapture.CaptureScreenshot(report.capture);Phase(1);return;
                }
                if(phase==1){
                    if(!File.Exists(report.capture)||EditorApplication.timeSinceStartup-phaseAt<.4)return;
                    oldRoot=f.Page.GameObject;f.CloseRegistry.CloseForName(OutgameTopInfoPage.SourceName);Phase(2);return;
                }
                if(phase==2){
                    if(f.Page.Closing==null||!f.Page.Closing.IsCompleted||oldRoot)return;f.Page.Closing.GetAwaiter().GetResult();
                    Check(f.Page.Lifetime.IsDisposed&&f.Page.OutletCount==25&&f.Page.Lifetime.Arguments!=null&&ReferenceEquals(f.Page.GameObject,oldRoot),"Native close marks disposed but source TopInfo.Dispose retains base dictionary/arguments/destroyed-object reference");
                    Check(gold.IsDisposed&&diamond.IsDisposed&&!gold.GameObject&&!diamond.GameObject&&f.Effects.Module.Effects.Count==0&&f.Effects.Providers.Values.All(p=>p.RefCount==0)&&f.Provider.RefCount==0,"Source close releases both effects and main page handle after native frame boundaries");
                    int saves=f.UserSaves;f.Messages.SendMessage("LoadStartingUI");f.Messages.SendMessage("Avatar_Refresh_OnAuth",new object[]{"a","b"});Check(f.UserSaves==saves,"Closed page has removed both account and authorization listeners");
                    var heldRows=rank.listRankItemData.Data;rank.OnDispose();
                    Check(!f.Account.Page.Data.Registry.HasInstance(4134)&&heldRows.Count==100,"Native RankControl disposal clears registry singleton while retaining source row buffers");
                    f.Dispose();f=null;Compose();Phase(3);return;
                }
                if(phase==3){
                    if(!f.Trace.Contains("open")||!f.Page.IconInitialization.IsCompleted)return;gold=f.Effects.Module.Get(f.Page.GoldEffectId);diamond=f.Effects.Module.Get(f.Page.DiamondEffectId);if(!gold.IsLoaded||!diamond.IsLoaded)return;
                    report.restoredGold=f.Account.Account.Local.GoldNum;report.restoredDiamonds=f.Account.Account.Local.DiamondsNum;report.restoredName=f.User.Name;
                    Check(report.restoredGold==37&&report.restoredDiamonds==5&&report.restoredName=="恢复测试"&&f.Page.GoldText.text=="37"&&f.Page.DiamondsText.text=="5"&&f.Page.NameText.text=="恢复测试","Independent file-backed account/UserInfo restart restores actual currency/name and fresh page labels");
                    Check(match.Manager.Data.rankData.nick=="platform-nickname"&&match.Manager.Data.rankData.url=="platform-avatar"&&match.Manager.Data.rankData.isGetWechatInfo&&match.Manager.Data.rankData.VN_t==6&&match.Manager.Data.rankData.VN_d==1&&match.Manager.Data.rankData.VN_w==1&&match.Manager.Data.rankData.DN_d==1&&match.Manager.Data.rankData.DN_w==1&&match.Manager.Data.rankData.DN_t==1&&match.Manager.Data.rankData.playNum==2,"Independent native account restart restores Match authorization, day/week/total wins/losses and play count");
                    Check(rank.CurPlayerScore==639999&&rank.Manager.Data.curPlayerScore=="639999"&&f.Page.ScoreText.text=="6.39K"&&rank.CurPlayerRank<20000,"Independent native account restart restores saved RankManager score/rank and current TopInfo display");
                    Check(rank.listRankItemData.Data.Count==100&&rank.listRankItemData.Data[rank.playerListIndex].score=="6.39K"&&rank.listRankItemData.Data[rank.playerListIndex].name=="恢复测试","Independent native restart rebuilds full home list from restored RankManager and UserInfo records");
                    Check(f.Effects.Module.Effects.Count==2&&f.ReadyEvents==1&&f.Provider.RefCount==1&&f.Effects.Providers.Values.All(p=>p.RefCount==1)&&match.Manager.Data.rankData.isGetWechatInfo,"Reopened page owns fresh resource handles; real MatchManager restores saved authorization");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            try{f?.Dispose();}catch(Exception e){report.passed=false;report.error=(report.error??"")+e;}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/rank-control-native-validation.json"),JsonUtility.ToJson(report,true));Debug.Log("AREABATTLE_RANK_CONTROL_NATIVE_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);EditorApplication.Exit(report.passed?0:1);
        }
    }
}
