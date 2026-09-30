using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLoginSyncValidation
    {
        [Serializable] sealed class FirstPackRawAudit { public FirstPackRawAsset[] assets; }
        [Serializable] sealed class FirstPackRawAsset { public string type,resourceKey,destination,sha256;public int bytes; }
        sealed class LegacyAssetRequestProbe:IOutgameLegacyAssetRequest
        {public bool Done;public float Value;public UnityEngine.Object Result;public bool IsDone=>Done;public float Progress=>Value;public UnityEngine.Object Asset=>Result;}
        sealed class BundleServicesProbe:IOutgameLegacyBundleServices
        {public Func<string,OutgameLegacyBundleLocation> Lookup;public OutgameLegacyBundleLocation GetAssetBundleInfo(string name)=>Lookup(name);}
        sealed class MenuItemProbe:IOutgameMenuItem
        {readonly Action<bool> changed;public MenuItemProbe(Action<bool> changed){this.changed=changed;}public void SetVisible(bool visible)=>changed(visible);}
        sealed class MenuVisibilityHostProbe:IOutgameMenuVisibilityHost
        {
            public readonly List<string> Calls=new List<string>();public bool Active=true;public OutgameMenuVisibility Owner;
            public bool Visible=>Active;public int CurrentPage{set{Calls.Add("page:"+value);}}
            public void ApplyBaseVisibility(bool visible){Active=visible;Calls.Add("base:"+visible);}public void SetVisibility(bool visible){Calls.Add("set:"+visible);Owner.VisibleImp(visible);}
            public void RequestMainPage(){Calls.Add("main");}public void CloseAllMenuItems(){Calls.Add("close-items");}public void OpenAllMenuItems(){Calls.Add("open-items");}
            public void CheckStartPanels(){Calls.Add("panels");}public void RefreshCommanderLockAndRedDots(){Calls.Add("commander");}public void RefreshSourceLevelGate(){Calls.Add("level-gate");}public void RefreshNewSkins(){Calls.Add("skins");}public void SendMenuTabDispose(){Calls.Add("dispose-message");}
        }
        sealed class LevelStateHostProbe:IOutgameLevelStateHost
        {
            public readonly List<string> Calls=new List<string>();public int Level=6,Local=1;public bool Insert,Checked,Guide,BossFlag;
            public int CurrentLevel=>Level;public int LocalSourceValue16{get=>Local;set{Local=value;Calls.Add("local:"+value);}}public int SmallLevelIndex{set{Calls.Add("small:"+value);}}
            public bool InInsertPause=>Insert;public bool HaveCheckedFailReward=>Checked;public bool GuideActive=>Guide;public bool SourceFlag53=>BossFlag;
            public float SourceSpeed104=>2;public int MinimumCommanderUnlockLevel=>6;
            public Action FindBossRainSound()=>()=>Calls.Add("rain");public Action<bool> FindParallelAudioPause()=>value=>Calls.Add("pause:"+value);
            public void SetAdHelperLevel(int level){Calls.Add("adlevel:"+level);}public void InitializeChannelData(){Calls.Add("channel");}public void ShowUpdateDialog(){Calls.Add("update");}
            public void PrepareHomeScene(){Calls.Add("home");}public void LoadHomeLevel(int level){Calls.Add("homeload:"+level);}public void SetPlayState(int state,bool option){Calls.Add("state:"+state+":"+option);}
            public void CloseLoading(){Calls.Add("close-loading");}public void ShowUi(string name){Calls.Add("show:"+name);}public void CloseUi(string name){Calls.Add("close:"+name);}public void GetPlayUi(){Calls.Add("get-play");}
            public void ReportLevelState(int state,int level){Calls.Add("report:"+state+":"+level);}public void EnterGuideNextStage(){Calls.Add("guide-next");}public void ReportTutorialStart(){Calls.Add("tutorial-start");}public void ReportTutorialFinish(){Calls.Add("tutorial-finish");}
            public void ShowMyCampEffect(){Calls.Add("effect");}public void SetLevelSpeed(float value){Calls.Add("speed:"+value);}public void PlayAudio(int channel,int[] ids){Calls.Add("play:"+channel+":"+ids[0]);}public void StopParallelAudio(int channel){Calls.Add("stop:"+channel);}
            public void ClearModelEntityCache(){Calls.Add("clear-model");}public void LevelRankWin(){Calls.Add("rank-win");}public void DiceLevelWin(){Calls.Add("dice-win");}
        }
        sealed class LevelContinuationHostProbe:IOutgameLevelContinuationHost
        {
            public readonly List<string> Calls=new List<string>();public Action Hidden;public int Level=5,Small=2;
            public int LocalSourceValue16{set{Calls.Add("local:"+value);}}public int SpecialState{set{Calls.Add("special:"+value);}}public bool AdsSourceFlag28{set{Calls.Add("ads:"+value);}}
            public int CurrentLevel{get=>Level;set{Level=value;Calls.Add("level:"+value);}}public int SmallLevelIndex{get=>Small;set{Small=value;Calls.Add("small:"+value);}}
            public void StopParallelAudio(int channel){Calls.Add("stop:"+channel);}public void CloseUi(string name){Calls.Add("close:"+name);}
            public void InitializeGameData(){Calls.Add("init");}public void SetPlayState(int state,bool option){Calls.Add("state:"+state+":"+option);}
            public void HideTransition(Action callback){Calls.Add("hide");Hidden=callback;}public void ShowTransition(Action callback){Require(callback==null,"source show null callback");Calls.Add("show");}
        }
        sealed class PlayStateHostProbe:IOutgamePlayStateHost
        {
            public readonly List<string> Calls=new List<string>();public Action<int> AudioHook;
            public bool Pvp,Dice;public bool PvpActive{get{Calls.Add("pvp");return Pvp;}}public bool DiceActive{get{Calls.Add("dice");return Dice;}}
            public void PauseGame(bool paused){Calls.Add("pause:"+paused);}
            public void AudioState(int state){Calls.Add("audio:"+state);AudioHook?.Invoke(state);}
            public void WayLineState(int state){Calls.Add("line:"+state);}public void AiState(int state){Calls.Add("ai:"+state);}public void SkillState(int state){Calls.Add("skill:"+state);}
            public void PublishGamePlayState(int state){Calls.Add("message:"+state);}public void SetCameraSize(){Calls.Add("camera");}public void DispatchStateBranch(int state){Calls.Add("branch:"+state);}
        }
        sealed class LevelStartHostProbe:IOutgameLevelStartHost,IOutgameLevelStartLoading
        {
            public readonly List<string> Calls=new List<string>();public Action Pending;public int Level=4;public bool HasMenu=true;
            public bool SourceFlag48{set{Calls.Add("loading-flag:"+value);}}public float SourceValue40{set{Calls.Add("loading-value:"+value);}}
            public bool SourceFlag53{set{Calls.Add("level-flag:"+value);}}
            public IOutgameLevelStartLoading OpenLoading(){Calls.Add("loading");return this;}
            public void InitializeEnemySkin(){Calls.Add("enemy");}
            public Action FindMenuClose(){Calls.Add("menu-query");return HasMenu?(Action)(()=>Calls.Add("menu-close")):null;}
            public void AppendDelayedCallback(float seconds,Action callback){Require(seconds==.5f,"original half-second sequence interval");Calls.Add("schedule");Pending=callback;}
            public int CurrentLevel{get{Calls.Add("level-query");return Level;}}
            public void LoadLevel(int level){Calls.Add("load:"+level);}
            public void InitializeSpecialScene(){Calls.Add("scene");}
        }
        sealed class SdkActionManagersProbe:IOutgameSdkActionManagers
        {
            public string Last;
            public void LoginComStatic(bool show){Last="login:"+show;}public void ShowToast(string text){Last="toast:"+text;}
            public void ShowFeedback(){Last="ShowFeedback";}
            public void ShowGDPRDialogStatic(){Last="ShowGDPRDialogStatic";}
            public void GotoPrivacyPolicyStatic(){Last="GotoPrivacyPolicyStatic";}
            public void GotoTermsServiceStatic(){Last="GotoTermsServiceStatic";}
            public void ShowGameBanHao(){Last="ShowGameBanHao";}
            public void StartRestoreStatic(){Last="StartRestoreStatic";}
            public void OpenOppoGameCenterStatic(){Last="OpenOppoGameCenterStatic";}
            public void ShowDrawVideoStatic(){Last="ShowDrawVideoStatic";}
            public void OpenPrivacyRecallActStatic(){Last="OpenPrivacyRecallActStatic";}
        }
        sealed class SdkActionProbe:IOutgameSdkFunctionActions
        {
            public int Last;
            public void AdsVideoFunction(){Last=5;}public void ShowShareFunction(){Last=7;}public void ShowLoginFunction(){Last=8;}
            public void FeedbackFunction(){Last=9;}public void GDPRUserFunction(){Last=10;}public void ShowPolicyFunction(){Last=11;}
            public void ShowUserProtocolFunction(){Last=12;}public void ShowGameBanHaoFunction(){Last=17;}public void StartRestore(){Last=19;}
            public void ShowOppoGameCenter(){Last=20;}public void DrawVideo(){Last=21;}public void OpenPrivacyRecall(){Last=22;}
        }
        sealed class Host:IOutgameLoginSyncHost
        {
            public event Action SynDataOver,DataUploaded;
            public readonly List<string> Calls=new List<string>();
            string user="prior-user";public string LoginUserId{get=>user;set{user=value;Calls.Add("user:"+value);}}
            public long Uid=>42;public bool IsNewDevice=>true;public bool IsNewPlayer{get;set;}
            public float Timer{get;set;}public bool LoginSuccess{get;set;}
            public void EnterBase(){Calls.Add("enter");Timer=0;}
            public void UpdateBase(float elapsed,float realElapsed){Calls.Add("update");}
            public void LeaveBase(bool shutdown){Calls.Add("leave:"+shutdown);}
            public void UserLogin(long uid,bool device,bool player){Calls.Add("login:"+uid+":"+device+":"+player);user="current-user";}
            public void ReportSyncSuccess(){Calls.Add("report");}
            public void RequestDataUploaded(){Calls.Add("upload");}
            public void ChangeToLoginSuccess(){if(!LoginSuccess)throw new Exception("success flag must precede transition");Calls.Add("success");}
            public void RefreshUserDataPath(){Calls.Add("path");}
            public void CloseSyncData(){Calls.Add("close");}
            public void HandleFailure(int code,string message){Calls.Add("failure:"+code+":"+message);}
            public void Sync()=>SynDataOver?.Invoke();public void Uploaded()=>DataUploaded?.Invoke();
        }
        sealed class FailureHost:IOutgameLoginFailureHost
        {
            public readonly List<string> Calls=new List<string>();
            public int SourceLoginMode{get;set;}public bool LoginSuccess{get;set;}public float Timer{get;set;}
            public void SendLoginProgress(int progress){Calls.Add("progress:"+progress);}
            public void LogFailure(bool switching,int error){Calls.Add("log:"+switching+":"+error);}
            public void ChangeToLoginSuccess(){Require(!LoginSuccess,"source success flag cleared before change");Calls.Add("success-state");}
            public void ChangeToIdle(){Require(Timer==0,"source timer reset before idle transition");Calls.Add("idle");}
            public void LoginFail(int error,string message){Calls.Add("fail:"+error+":"+message);}
        }
        sealed class SuccessHost:IOutgameLoginSuccessHost
        {
            public readonly List<string> Calls=new List<string>();public readonly Dictionary<int,Action> Events=new Dictionary<int,Action>();
            public bool LoginSuccess{get;set;}public Action Completing;
            public void EnterBase(){Calls.Add("enter");}
            public void LeaveBase(bool shutdown){Calls.Add("leave:"+shutdown);}
            public void Subscribe(int id,Action callback){Calls.Add("add:"+id);Events.Add(id,callback);}
            public void Unsubscribe(int id,Action callback){Require(Events[id]==callback,"same delegate removed");Calls.Add("remove:"+id);Events.Remove(id);}
            public void ChangeToSdkXyx(){Calls.Add("sdk");}
            public void ChangeToIdle(){Calls.Add("idle");}
            public void LoginComplete(bool success,string message){Require(success&&message==string.Empty,"source completion arguments");Calls.Add("complete");Completing?.Invoke();}
        }
        sealed class StorageBackend:IOutgameStorageBackend
        {
            public readonly Dictionary<string,string> Disk=new Dictionary<string,string>();public readonly List<string> Calls=new List<string>();
            public Action Complete;public Action<string> Fail;Action commit;public int Reads;
            public string Get(string key){Reads++;return Disk.TryGetValue(key,out var value)?value:"";}
            public void Set(string key,string value,Action<string> fail,Action complete){Calls.Add("set:"+key);commit=()=>Disk[key]=value;Fail=fail;Complete=complete;}
            public void Remove(string key,Action<string> fail,Action complete){Calls.Add("remove:"+key);commit=()=>Disk.Remove(key);Fail=fail;Complete=complete;}
            public void Clear(Action<string> fail,Action complete){Calls.Add("clear");commit=()=>Disk.Clear();Fail=fail;Complete=complete;}
            public void Finish(bool success){var next=Complete;if(success)commit();else Fail("disk failure");next();}
        }
        sealed class StartupHost:IOutgameStartupEntryHost
        {
            public readonly List<string> Calls=new List<string>();public Action Completed;public bool Synchronous,FailLoad;
            public int CurrentLevel{get;set;}=17;
            public bool EnterGame{get=>entered;set{entered=value;Calls.Add("enter:"+value);}}bool entered;
            public bool RedDotSourceFlag8{get=>red;set{red=value;Calls.Add("red:"+value);}}bool red;
            public int RedDotSourceValue16{get=>redState;set{redState=value;Calls.Add("red-state:"+value);}}int redState;
            public void ShowMenu()=>Calls.Add("menu");
            public void SetMenuVisible(bool value)=>Calls.Add("visible:"+value);
            public void ShowCommonReward()=>Calls.Add("reward");
            public void InitializePrefabs()=>Calls.Add("prefabs");
            public void LoadScene(string name,Action complete,bool option){Calls.Add("load:"+name+":"+option);if(FailLoad)throw new InvalidOperationException("load");Completed=complete;if(Synchronous)complete();}
            public void LateInitializeModule()=>Calls.Add("late");
            public void SendLoadGameScreen()=>Calls.Add("event");
            public void ReportActivityEnter(string activity,string level)=>Calls.Add("activity:"+activity+":"+level);
            public void InitializeLevelRank()=>Calls.Add("rank");
            public void InitializeSevenDayActivity()=>Calls.Add("seven");
            public void SetPlayState(int state)=>Calls.Add("play:"+state);
            public void CloseLoading()=>Calls.Add("close");
            public void ReportGameInteractive(string message)=>Calls.Add("interactive:"+message);
        }
        sealed class DataStorageHost:IOutgameDataStorageHost
        {
            public readonly List<string> Calls=new List<string>();
            public int SourceLoginProgress {get;set;}=10;
            public bool LoginProcedureFlag8 {get;set;}
            public bool LoginStaticFlag4 {get;set;}
            public bool IsUseServer {get;set;}
            public string MineGameName=>"Proj_hdzd";
            public bool HasToast=>true;
            public void Log(string text)=>Calls.Add("log:"+text);
            public void Error(string text)=>Calls.Add("error:"+text);
            public void Toast(string text)=>Calls.Add("toast:"+text);
            public string Compress(string key,string text){Calls.Add("compress:"+key);return "compressed:"+text;}
            public string Decompress(string key,string text){Calls.Add("decompress:"+key);return "decoded:"+text;}
            public void QueueUpload(string key,string text)=>Calls.Add("upload:"+key+":"+text);
        }
        sealed class PoolManager:IOutgameDataManager
        {
            public string DataKey {get;set;}
            public bool ParticipatesInSync {get;set;}
            public bool CompressData {get;set;}
            public Action Init,Save;
            public void OnInit()=>Init?.Invoke();
            public void OnSave()=>Save?.Invoke();
        }
        sealed class VersionManager:IOutgameVersionManager
        {
            public string DataKey {get;set;}
            public bool ParticipatesInSync {get;set;}
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        sealed class RewardVideoHost:IOutgameRewardVideoHost,IOutgameRewardVideoEnvironment
        {
            public string ClickLockParameter {get;set;}="1";public float Realtime {get;set;}=5;public bool NativeVideoReady {get;set;}=true;
            public readonly List<string> Calls=new List<string>();public Action<bool> Shown,Closed;public System.Collections.IEnumerator Routine;
            public void ResetNativeSourceFlag64(){Calls.Add("reset");}
            public void ShowNative(Action<bool> shown,Action<bool> closed){Shown=shown;Closed=closed;Calls.Add("show");}
            public object StartRoutine(System.Collections.IEnumerator routine){Routine=routine;Calls.Add("start");routine.MoveNext();return routine;}
            public void StopRoutine(object routine){Calls.Add("stop");}
            public void Log(string text){Calls.Add("log:"+text);}public void Warn(string text){Calls.Add("warn:"+text);}
            public void ReportNoAds(string label){Calls.Add("noads:"+label);}public void Send(string name,params object[] args){Calls.Add(name+(args.Length==0?"":":"+args[0]));}
        }
        sealed class AdController:IOutgameAdController
        {
            public bool Ready;public bool SourceFlag20 {get;set;}public Action OnClose;public Action<bool> Shown,Closed;public int ShowCount,CloseCount,ReadyCount;
            public bool IsReady(){ReadyCount++;return Ready;}public void Close(){CloseCount++;OnClose?.Invoke();}
            public void Show(Action<bool> shown,Action<bool> closed){ShowCount++;Shown=shown;Closed=closed;}
        }
        sealed class AdAdapterState:OutgameAdAdapterState
        {
            public bool Cache=true,Guarantee;public Action Clearing;
            public AdAdapterState(Func<float> now,Func<float> last,Action<string> log):base(now,last,log){}
            public override bool IsCacheRequest=>Cache;public override bool IsGuarantee=>Guarantee;
            protected override void OnFinishClearCache(){Clearing?.Invoke();}
        }
        sealed class CacheCandidate:IOutgameAdCacheCandidate
        {
            public int Priority {get;set;}
            public int PlatformId {get;set;}
            public int Price;
            public Func<int> ReadPrice;
            public bool IsGuarantee {get;set;}
            public int GetRealPrice()=>ReadPrice==null?Price:ReadPrice();
        }
        sealed class RequestCandidate:IOutgameAdRequestCandidate
        {
            public bool IsCacheRequest {get;set;}
            public bool IsHighPriority {get;set;}
            public Action Request;
            public void Handle()=>Request?.Invoke();
        }
        sealed class ShowCandidate:IOutgameVideoCandidate
        {
            public bool WaitCloseReportShow {get;set;}
            public int SourceState {get;set;}=2;
            public bool IsCacheRequest {get;set;}=true;
            public bool IsHighPriority {get;set;}
            public bool IsGuarantee {get;set;}
            public int Priority {get;set;}
            public int PlatformId {get;set;}
            public int Price=1;
            public Action Request,Show,Finishing;
            public void Finish(){Finishing?.Invoke();SourceState=0;}
            public Func<bool> Allowed=()=>true;
            public bool CanShow()=>Allowed();
            public int GetRealPrice()=>Price;
            public void Handle()=>Request?.Invoke();
            public void ShowAd()=>Show?.Invoke();
        }
        sealed class ShowEnvironment:IOutgameAdShowEnvironment
        {
            public string Channel {get;set;}="weixin";
            public bool InterVideoShowing {get;set;}
            public readonly List<string> Calls=new List<string>();
            public void Log(string text)=>Calls.Add(text);
            public void ReportIntersRequest()=>Calls.Add("inters-request");
            public void ReportRotaRequestAd()=>Calls.Add("rota-request");
            public void ReportRotaRequestAdFail()=>Calls.Add("rota-fail");
        }
        class WxLoadHandle:IOutgameWxVideoLoadHandle
        {
            public Action Loaded,Destroying;public Action<OutgameWxAdError> Failed;public int Loads;
            public void OnLoad(Action callback)=>Loaded=callback;
            public void OnError(Action<OutgameWxAdError> callback)=>Failed=callback;
            public void Load()=>Loads++;
            public void Destroy()=>Destroying?.Invoke();
        }
        class NotificationHost:IOutgameAdNotificationHost
        {
            public readonly List<string> Calls=new List<string>();public bool InterVideoShowing {get;set;}
            public void StopLoadTimeout()=>Calls.Add("stop-load");public void StopShowTimeout()=>Calls.Add("stop-show");public void ReportRequest()=>Calls.Add("request");
            public int PlatformId;public int GetPlatformId()=>PlatformId;
            public void ReportClick()=>Calls.Add("click");public void ReportNewClick(bool repeated)=>Calls.Add("new-click:"+repeated);
            public void ReportNewShowError(int code,string message)=>Calls.Add("new-error");public void ReportShowFail(int code,string message)=>Calls.Add("show-fail");
            public double GetPrice()=>12;public void ReportShow(bool value)=>Calls.Add("show:"+value);public void ReportNewAdShow()=>Calls.Add("new-show");public void ReportVideoComplete()=>Calls.Add("complete");public void HandleAdsLevel(string key)=>Calls.Add(key);public void StartShow()=>Calls.Add("start");
        }
        class NotifyingAdapter:OutgameAdNotifyingAdapter
        {
            public bool Cache=true;public Action Clearing;
            public override bool IsCacheRequest=>Cache;
            public NotifyingAdapter(NotificationHost host):base(host,()=>0,()=>0,x=>{}){}
            protected override void OnFinishClearCache()=>Clearing?.Invoke();
        }
        sealed class SerialLifecycleHost:IOutgameSerialControllerHost
        {
            public readonly List<string> Calls=new List<string>();public Action Loading;
            public void CloseBanner()=>Calls.Add("close-banner");public void ChangeTimerReport(int type)=>Calls.Add("timer:"+type);public void ReportPlatformBack()=>Calls.Add("platform-back");public void ReportRotaRequestAdSuccess()=>Calls.Add("rota-success");public void VideoShowLoad()=>Calls.Add("video-preload");
            public Action Checking;public float Realtime {get;set;}=10;public bool CanAutoShowInter4 {get;set;}
            public void CheckRequest(){Calls.Add("check");Checking?.Invoke();}public void SetCanAutoShowInter4(bool value){CanAutoShowInter4=value;Calls.Add("auto:"+value);}public void OnlyShowInter4(bool value)=>Calls.Add("inter4:"+value);public void VideoLoadSuccessEvent(bool value)=>Calls.Add("video-load:"+value);
            public void Log(string text)=>Calls.Add(text);public void ResetTimerReport()=>Calls.Add("timer-reset");public void SetInterVideoShowing(bool value)=>Calls.Add("shared:"+value);
            public void SetIntersClose(string type)=>Calls.Add("inters-close:"+type);public void ShowIntersAfterIntersClose()=>Calls.Add("inters-after-inters");public void ReportInsertClose()=>Calls.Add("report-insert-close");public void ShowIntersAfterVideoClose()=>Calls.Add("inters-after-video");public void StopVideoShowLoad()=>Calls.Add("stop-video-load");public int GetPlatListCount(){Calls.Add("count");return 3;}public void Load(){Calls.Add("load");Loading?.Invoke();}public void RestoreBanner()=>Calls.Add("restore-banner");
        }
        sealed class PreloadCandidate:IOutgameAdPreloadCandidate
        {
            public bool IsCacheRequest {get;set;}=true;
            public int SourceState {get;set;}
            public OutgameAdListener Listener {get;set;}
            public Action Request;
            public void Handle()=>Request?.Invoke();
        }
        sealed class ListeningShowAdapter:NotifyingAdapter,IOutgameAdShowCandidate
        {
            public ListeningShowAdapter(NotificationHost host):base(host){}
            public int Priority=>0;public int PlatformId=>0;public int GetRealPrice()=>1;
            public bool WaitCloseReportShow=>false;
            public void Handle(){}public void ShowAd()=>NotifyShowAd(true,true);
        }
        sealed class WxPresentationHandle:WxLoadHandle,IOutgameWxVideoHandle
        {
            public Action<OutgameWxVideoCloseResult> Closed;
            public Action<OutgameWxAdError> Shown,ShowFailed;public int OffCount;
            public void OnClose(Action<OutgameWxVideoCloseResult> callback)=>Closed=callback;
            public void OffClose(Action<OutgameWxVideoCloseResult> callback){Require(callback==Closed,"source removes same close delegate");OffCount++;}
            public void Show(Action<OutgameWxAdError> success,Action<OutgameWxAdError> fail){Shown=success;ShowFailed=fail;}
        }
        sealed class WxPresentationHost:IOutgameWxVideoPresentationHost
        {
            public bool MiniGameCommonPlugin {get;set;}
            public int PlayCount {get;set;}public int SuccessCount {get;set;}
            public readonly List<string> Calls=new List<string>();
            public void ReportRewardShowEvent()=>Calls.Add("partial-report");public void RewardAdShow()=>Calls.Add("ams-show");
            public void RequestRewardPrediction(int total,int successes)=>Calls.Add("prediction:"+total+":"+successes);
            public string StateText=>"test-state";public void Log(string text){}
        }
        sealed class WxAdapterRuntime:NotificationHost,IOutgameWxVideoRuntime
        {
            public readonly WxPresentationHandle Native=new WxPresentationHandle();
            public readonly List<System.Collections.IEnumerator> Routines=new List<System.Collections.IEnumerator>();
            public readonly List<object> Stopped=new List<object>();
            public string CreatedId;public bool Multiton;public int Destroyed;
            public IOutgameWxVideoHandle CreateVideo(string id,bool multiton){CreatedId=id;Multiton=multiton;Native.Destroying=()=>Destroyed++;return Native;}
            public int? ConfiguredIdCount=>1;
            public float Realtime=>10;public float LastShowTime=>0;
            public DateTime LocalNow=>new DateTime(2026,9,29);
            public string LastPriceKey;
            public int ReadInt(string key,int fallback){LastPriceKey=key;return 17;}
            public object StartRoutine(System.Collections.IEnumerator routine){Routines.Add(routine);return routine;}
            public void StopRoutine(object handle)=>Stopped.Add(handle);
            public bool AudioInterrupted {get;set;}
            public void ReportRequestAd()=>Calls.Add("request-ad");public void ReportTimeout()=>Calls.Add("timeout");
            public bool MiniGameCommonPlugin=>false;
            public int PlayCount {get;set;}public int SuccessCount {get;set;}
            public void ReportRewardShowEvent()=>Calls.Add("reward-show");public void RewardAdShow()=>Calls.Add("ams-show");
            public void RequestRewardPrediction(int total,int successes)=>Calls.Add("prediction:"+total+":"+successes);
            public string StateText=>"fixture-state";public void Log(string text){}
        }
        sealed class FlyAnimationProbe:IOutgameFlyAnimationHost
        {
            public readonly List<string> Trace=new List<string>();public readonly List<Action> Complete=new List<Action>();public readonly List<GameObject> Icons=new List<GameObject>();int sub;
            public object CreateTargetTween(Transform target){Trace.Add("target");return this;}
            public void PlayTargetTween(object tween)=>Trace.Add("pulse");
            public void Spawn(string path,Action<GameObject> ready){var icon=UnityEngine.Object.Instantiate(OutgameFlyCurrencyAssets.Load(path));Icons.Add(icon);Trace.Add("spawn");ready(icon);}
            public void Unspawn(GameObject item){Trace.Add("pool");item.SetActive(false);}
            public Vector2 ScatterPosition(Vector2 origin)=>origin+Vector2.one;
            public string NextSubId(int id)=>"fly_"+id+"_"+sub++;
            public void Move(Transform item,Vector3 destination,float duration,int ease,string id,bool independent,Action completed){Require(independent,"source independent tween update");Trace.Add("move:"+ease+":"+duration);Complete.Add(completed);}
            public void StopEffect(int id)=>Trace.Add("stop:"+id);
            public void KillTween(string id,bool complete)=>Trace.Add("kill:"+id+":"+complete);
        }
        sealed class FlyCleanupProbe:IOutgameFlyCleanupHost
        {
            public float Now;public float Time=>Now;public bool ThrowKill;public readonly List<string> Trace=new List<string>();
            public int SubIdCount(int id){Trace.Add("count:"+id);return 2;}
            public void KillTween(string id,bool complete){Trace.Add("kill:"+id+":"+complete);if(ThrowKill)throw new InvalidOperationException("kill");}
            public void StopCoroutine(object coroutine)=>Trace.Add("stop:"+coroutine);
            public void RemoveSubIds(int id)=>Trace.Add("remove:"+id);
        }
        sealed class ShopFlyProbe:IOutgameFlyToolHost,IOutgameShopCurrencyEffects
        {
            public readonly List<string> Trace=new List<string>();public OutgameFlyToolRequest Request;public Transform Target,Root;public OutgameFlyToolStart Start;public bool FailAnimation;
            public int NextId(){Trace.Add("id");return 7;}public Transform DefaultRoot(){Trace.Add("root");return Root;}
            public object StartAnimation(OutgameFlyToolRequest request){Trace.Add("animate");Request=request;if(FailAnimation)throw new InvalidOperationException("animation");return this;}
            public void TrackCoroutine(int id,object coroutine)=>Trace.Add("track:"+id);
            public float Time{get{Trace.Add("time");return 12.5f;}}
            public void TrackTime(int id,float time)=>Trace.Add("stamp:"+id);
            public void FlyMoney(int amount,Transform root,Vector3 position,bool apply,Action completion,bool updateDisplayedValue)=>Start.Begin(1001,Target,amount,root,position,apply,completion,updateDisplayedValue);
            public void FlyDiamonds(int amount,Transform root,Vector3 position,bool apply,Action completion,bool updateDisplayedValue)=>Start.Begin(1002,Target,amount,root,position,apply,completion,updateDisplayedValue);
        }
        sealed class VideoButtonHostProbe:IOutgameVideoButtonHost
        {
            public readonly List<string> Trace=new List<string>();
            public OutgameVideoButtonData Data=new OutgameVideoButtonData{id=8,showType=0,AudioId=42,VideFlag=0,ReportLable="original",Video_param1="a",Video_param2="b"};
            public bool Allowed=true,Ready=true;public bool? OpenOverride;public Action<string,bool> Pending;public Action<int> TrackAction;public Action<int,Action<string,bool>,int> ShowOverride;
            public Func<int,OutgameVideoButtonData> DataLookup;
            public OutgameVideoButtonData GetData(int id){Trace.Add("data:"+id);return DataLookup==null?Data:DataLookup(id);}
            public bool IsOpen(int function,bool fallback){Trace.Add("open:"+function+":"+fallback);return function==5?Allowed:(OpenOverride??Ready);}
            public void BindFunctionButton(int function,OutgameVideoButton button)=>Trace.Add("bind:"+function);
            public void CheckVideoIsReady()=>Trace.Add("check-ready");
            public bool IsVideoReady(){Trace.Add("ready");return Ready;}
            public void PlayAudio(int group,int audio)=>Trace.Add("audio:"+group+":"+audio);
            public void Log(string message)=>Trace.Add("log:"+message);
            public void TrackVideo(int state){Trace.Add("track:"+state);TrackAction?.Invoke(state);}
            public void Report(string kind,string label,string p1,string p2)=>Trace.Add(kind+":"+label+":"+p1+":"+p2);
            public string GameName=>"fixture-game";
            public void ReportEvent(string name,string value,bool once)=>Trace.Add("event:"+name+":"+value+":"+once);
            public void ShowVideo(int flag,Action<string,bool> callback,int id){Trace.Add("ad:"+flag+":"+id);if(ShowOverride!=null)ShowOverride(flag,callback,id);else Pending=callback;}
        }
        sealed class CleanupLoaderProbe:IOutgameCleanupLoader
        {
            public string RegistryKey {get;set;}public bool Allowed;public readonly List<string> Trace;public Action OnDestroy,OnProviders;
            public CleanupLoaderProbe(string key,List<string> trace){RegistryKey=key;Trace=trace;}
            public void TryDestroyAllProviders(){Trace.Add("providers:"+RegistryKey);OnProviders?.Invoke();}
            public bool CanDestroy(){Trace.Add("can:"+RegistryKey);return Allowed;}
            public void Destroy(){Trace.Add("destroy:"+RegistryKey);OnDestroy?.Invoke();}
        }
        sealed class UiOpenProbe:IOutgameUiOpenHost
        {
            public bool IsDisposed {get;set;}public bool Cached {get;set;}public object LegacyResource {get;set;}
            public int OpenAnimation {get;set;}public float OpenAnimationTime=>.2f;public object PageIdentity=>this;
            public readonly List<string> Trace=new List<string>();public readonly Queue<System.Collections.IEnumerator> Routines=new Queue<System.Collections.IEnumerator>();public Action Later;
            public void InitGameObject(GameObject root)=>Trace.Add("init");public void AddCanvas()=>Trace.Add("canvas");public void CloseLoading()=>Trace.Add("loading");public void Refresh()=>Trace.Add("refresh");
            public System.Collections.IEnumerator CustomOpenAnimation(){Trace.Add("custom-body");yield return null;}
            public System.Collections.IEnumerator StandardOpenAnimation(GameObject root,int kind,float duration){Require(kind==3&&duration==.2f,"source open animation arguments");Trace.Add("standard-body");yield return null;}
            public object StartCoroutine(System.Collections.IEnumerator routine){Trace.Add("start");Routines.Enqueue(routine);return routine;}
            public void OpenLater(){Trace.Add("later");Later?.Invoke();}public void Log(string value)=>Trace.Add(value);
        }
        sealed class UiAsyncCloseProbe:IOutgameUiAsyncCloseHost
        {
            public readonly List<string> Trace=new List<string>();public int CloseAnimation {get;set;}public float CloseAnimationTime=>.3f;public GameObject GameObject=>null;
            public TaskCompletionSource<bool> Animation=new TaskCompletionSource<bool>();public readonly Queue<TaskCompletionSource<bool>> Frames=new Queue<TaskCompletionSource<bool>>();
            public bool UsesNewResources {get;set;}public string UIPath=>"FestActUI/ValentineUI";public object PageIdentity=>this;
            public void CloseBefore()=>Trace.Add("before");public Task CustomCloseAnimation(){Trace.Add("custom");return Animation.Task;}
            public Task StandardCloseAnimation(GameObject root,int animation,float duration){Require(animation==2&&duration==.3f,"source standard animation arguments");Trace.Add("standard");return Animation.Task;}
            public void MarkDisposed()=>Trace.Add("disposed");public Task EndOfFrame(WaitForEndOfFrame instruction){Trace.Add("frame");var pending=new TaskCompletionSource<bool>();Frames.Enqueue(pending);return pending.Task;}
            public void SetVisible(bool value){Require(!value,"close hides");Trace.Add("hide");}public void Destroy(GameObject root)=>Trace.Add("destroy");public void Dispose()=>Trace.Add("cleanup");
            public void ReleaseMainHandle()=>Trace.Add("release");public void UnloadUnusedAssets()=>Trace.Add("unused");public void UnloadUnusedBundle(string path){Require(path=="UI/FestActUI/ValentineUI.prefab","original bundle path");Trace.Add("bundle");}
            public void ReleaseDynamicResources()=>Trace.Add("dynamic");public void ReleaseCustomResources()=>Trace.Add("custom-res");
        }
        sealed class VideoClickProbe:IOutgameVideoClickHost
        {
            public bool HasData {get;set;}=true;
            public int ButtonState {get;set;}=2;
            public float UnscaledTime {get;set;}
            public readonly List<string> Trace=new List<string>();
            public Action BaseAction,ShowAction,ReportAction;
            public OutgameVideoButtonCompletion Completion;
            public void PlayAudio()=>Trace.Add("audio");
            public void Log(string message)=>Trace.Add(message);
            public void VideoClickReport()=>Trace.Add("click-report");
            public void InvokeClick(bool ready){Trace.Add("click:"+ready);Completion?.InvokeClick(ready);}
            public void BasePointerClick(){Trace.Add("base");BaseAction?.Invoke();}
            public void ReportVideoPlay(){Trace.Add("play-report");ReportAction?.Invoke();}
            public void ShowVideo(){Trace.Add("show");ShowAction?.Invoke();}
        }
        sealed class ShopEventsProbe:IOutgameShopEventHost
        {
            public List<string> Trace=new List<string>();public Dictionary<string,Action<object[]>> Listeners=new Dictionary<string,Action<object[]>>();public Action<bool> Gold,Diamonds;
            public void AddListener(string name,Action<object[]> callback){Trace.Add("add:"+name);Listeners.Add(name,callback);}
            public void RemoveListener(string name,Action<object[]> callback){Trace.Add("remove:"+name);Require(Listeners[name]==callback,"same listener identity during dispose");Listeners.Remove(name);}
            public void StopInitializationCoroutine(object coroutine)=>Trace.Add("stop");
            public void EnsureSelectCardControl()=>Trace.Add("select-card");
            public void AddGoldVideoCallback(Action<bool> callback){Trace.Add("gold");Gold=callback;}
            public void AddDiamondVideoCallback(Action<bool> callback){Trace.Add("diamonds");Diamonds=callback;}
        }
        sealed class AssetProviderProbe:IOutgameAssetProvider
        {
            public bool Done;public bool IsDone=>Done;
            public bool Destroyed;public UnityEngine.Object Asset;public int Releases;public bool Fail;
            public bool IsDestroyed=>Destroyed;public UnityEngine.Object AssetObject=>Asset;
            public void ReleaseHandle(OutgameAssetHandle handle){Releases++;if(Fail)throw new InvalidOperationException("provider release");}
        }
        sealed class NormalPoolLifetimeProbe:IOutgameNormalPoolLifetime
        {
            public List<string> Trace=new List<string>();public bool FailRelease;
            public void DestroyRegisteredPoolIfManagerExists(string name)=>Trace.Add("manager:"+name);
            public void DestroyRoot(GameObject root)=>Trace.Add("root:"+root.name);
            public void ReleaseHandle(IOutgamePoolAssetHandle handle){Trace.Add("handle");if(FailRelease)throw new InvalidOperationException("release");}
            public void UnloadUnusedAssets()=>Trace.Add("unused");
            public void UnloadUnusedBundle(IOutgamePoolResourceInfo info)=>Trace.Add("legacy");
        }
        sealed class NormalPoolHandleProbe:IOutgamePoolAssetHandle
        {public UnityEngine.Object Source;public Func<GameObject> Create;public UnityEngine.Object MainObject=>Source;public GameObject Instantiate()=>Create();}
        sealed class NormalPoolResourcesProbe:IOutgameNormalPoolResources
        {
            public bool UseNewResourceLoader=>true;
            public List<Action<IOutgamePoolAssetHandle>> Pending=new List<Action<IOutgamePoolAssetHandle>>();
            public List<GameObject> Released=new List<GameObject>();
            public void LoadOriginal(string path,Action<IOutgamePoolAssetHandle> complete)=>Pending.Add(complete);
            public void LoadResource(string path,Action<IOutgamePoolResourceInfo> complete)=>throw new InvalidOperationException("Unexpected legacy resource path");
            public void LoadSynchronousPrefab(string path,Action<GameObject> complete)=>throw new InvalidOperationException("Unexpected synchronous path");
            public string GetIntactBundleName(string assetName,string bundleName)=>throw new InvalidOperationException("Unexpected legacy name resolution");
            public void ReleaseUIForm(GameObject target)=>Released.Add(target);
            public void Warning(string message){}
        }
        sealed class ProjectShopProbe:IOutgameProjectShopItemView,IOutgameToolEffects
        {
            public readonly List<string> Trace=new List<string>();
            public void SetActive(bool value)=>Trace.Add("active:"+value);
            public void SetIcon(string icon,string atlas)=>Trace.Add("icon:"+atlas+":"+icon);
            public void SetPriceText(string value)=>Trace.Add("price:"+value);
            public void SetQuantityText(string value)=>Trace.Add("quantity:"+value);
            public void PlayVoice(int group,int id)=>Trace.Add("voice:"+group+":"+id);
            public void ReportToolGet(int item,int category,int amount,int balance,string reason)=>Trace.Add("extra:"+item+":"+amount+":"+balance+":"+reason);
            public void Refresh()=>Trace.Add("refresh");
            public void RefreshTopInfo()=>Trace.Add("top");public void GoldSpent(int id,long amount)=>Trace.Add("spent");
            public void ToolChanged(int id)=>Trace.Add("changed:"+id);
            public void UnlockScene(int id)=>throw new Exception("unexpected scene");public void UnlockSoldier(int id)=>throw new Exception("unexpected soldier");
            public bool ApplyItemEntity(int id,long delta)=>false;public void MissingItemEntity(int id)=>Trace.Add("missing");
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>Trace.Add("get:"+id);
            public void ReportCost(int id,int category,int amount,int balance)=>Trace.Add("cost:"+id);
            public void Save()=>Trace.Add("save");
        }
        sealed class PriceRandomProbe:System.Random
        {
            public int Min,Max,Count,RangeCalls,ListCalls;
            public override int Next(int min,int max){Min=min;Max=max;RangeCalls++;return min;}
            public override int Next(int max){Count=max;ListCalls++;return 0;}
        }
        sealed class ItemLifecycleHost:IOutgameGlobalItemLifecycleHost
        {
            public List<string> Events;public Action Update,OnRegister;public bool FailAdd,FailRemove;
            public string Message;public int EventId;public Func<object[],long> Query;
            public int AddUpdate(Action action){Events.Add("add");if(FailAdd)throw new InvalidOperationException();Update=action;return 0;}
            public void RemoveUpdate(int id){Events.Add("remove:"+id);if(FailRemove)throw new InvalidOperationException();}
            public void DisposeItemConfiguration(){Events.Add("dispose");}
            public void ClearGlobalInstance(){Events.Add("clear");}
            public void SendStatisticsRegistration(string message,int eventId,Func<object[],long> query){Events.Add("register");Message=message;EventId=eventId;Query=query;OnRegister?.Invoke();}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source LoginSynData state with injected inherited FSM and platform/data services. Real SDK login, data transfer and production startup are not validated."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("outgame-login-sync-old-player-success-and-unsubscribe",()=>{
                var h=new Host();var state=new OutgameLoginSync(h);state.Enter();h.Sync();
                Require(string.Join("|",h.Calls)=="enter|login:42:True:False|report|success","old player exact enter and completion order");
                state.Leave(false);int count=h.Calls.Count;h.Uploaded();h.Sync();Require(h.Calls.Count==count,"leave removes both subscriptions");
            });
            check("outgame-login-sync-new-player-awaits-upload",()=>{
                var h=new Host{IsNewPlayer=true};var state=new OutgameLoginSync(h);state.Enter();h.Sync();
                Require(!h.LoginSuccess&&h.Calls[h.Calls.Count-1]=="upload","sync over alone cannot complete new player login");
                h.Uploaded();Require(h.LoginSuccess&&h.Calls[h.Calls.Count-1]=="success","upload message completes login");state.Leave(false);
            });
            check("outgame-login-sync-timeout-boundary-restores-user",()=>{
                var h=new Host();var state=new OutgameLoginSync(h);state.Enter();state.Update(29.5f,100);
                Require(h.LoginUserId=="current-user"&&h.Timer==29.5f,"uses elapsed not realElapsed and waits below30");
                h.Calls.Clear();state.Update(.5f,0);
                Require(string.Join("|",h.Calls)=="update|user:prior-user|path|close|failure:4:同步数据超时","30 second boundary restores user then path, closes sync, reports failure");
                Require(!h.LoginSuccess,"timeout does not grant login success");state.Leave(false);
            });
            check("outgame-login-sync-callback-reads-current-new-player-flag",()=>{
                var h=new Host{IsNewPlayer=true};var state=new OutgameLoginSync(h);state.Enter();h.IsNewPlayer=false;h.Sync();
                Require(h.LoginSuccess&&!h.Calls.Contains("upload"),"new player flag is reread on callback");state.Leave(false);
            });
            check("outgame-login-failure-normal-reset-idle-notification-order",()=>{
                foreach(int mode in new[]{1,2,-1})
                {
                    var h=new FailureHost{SourceLoginMode=mode,Timer=30,LoginSuccess=true};new OutgameLoginFailure(h).Handle(4,"timeout");
                    Require(string.Join("|",h.Calls)=="progress:4|log:False:4|idle|fail:4:timeout","normal failure resets then changes idle before original error callback");
                    Require(h.LoginSuccess,"normal failure does not modify static success flag");
                }
            });
            check("outgame-login-failure-account-switch-preserves-timer",()=>{
                var h=new FailureHost{SourceLoginMode=0,Timer=30,LoginSuccess=true};new OutgameLoginFailure(h).Handle(4,"timeout");
                Require(string.Join("|",h.Calls)=="progress:4|log:True:4|success-state|fail:5:timeout","account switching uses source LoginSuccess state then error5");
                Require(!h.LoginSuccess&&h.Timer==30,"switch branch clears flag but does not reset timer");
            });
            check("outgame-login-success-registers-before-reentrant-completion",()=>{
                var h=new SuccessHost{LoginSuccess=true};var state=new OutgameLoginSuccess(h);
                h.Completing=()=>{Require(h.Events.Count==3,"all source events present during completion");h.Events[3]();state.Leave(false);};state.Enter();
                Require(string.Join("|",h.Calls)=="enter|add:3|add:5|add:7|complete|sdk|remove:3|remove:5|leave:False","reentrant transition retains source subscription and completion ordering");
                Require(h.Events.ContainsKey(7),"source does not explicitly unsubscribe7; owner FSM lifetime remains external");
            });
            check("outgame-login-success-false-flag-does-not-complete",()=>{
                var h=new SuccessHost{LoginSuccess=false};var state=new OutgameLoginSuccess(h);state.Enter();
                Require(!h.Calls.Contains("complete"),"switch-failure entry must not announce successful login");
                h.Events[5]();h.Events[7]();Require(h.Calls[h.Calls.Count-1]=="idle"&&h.Calls[h.Calls.Count-2]=="idle","both source event IDs route idle");state.Leave(true);
            });
            check("outgame-login-data-new-player-upload-precedes-reload",()=>{
                foreach(bool changed in new[]{false,true})foreach(bool device in new[]{false,true})
                {
                    var plan=OutgameLoginDataPlan.Resolve(changed?"previous":"42",42,device,true,0);
                    Require(plan.UserId=="42"&&plan.ChangedUser==changed&&plan.ForceDownload==device,"original identity/device flags retained");
                    Require(plan.ForceUpload&&!plan.ReloadManagers,"new-player upload branch must preserve held memory even for changed user/new device");
                }
            });
            check("outgame-login-data-existing-user-and-device-reload-matrix",()=>{
                foreach(int syn in new[]{0,1,-1})foreach(bool changed in new[]{false,true})foreach(bool device in new[]{false,true})
                {
                    var plan=OutgameLoginDataPlan.Resolve(changed?"previous":"42",42,device,false,syn);
                    Require(!plan.ForceUpload&&plan.ReloadManagers==(changed||device),"old-player routing distinguishes same-device relogin from new-device/account change");
                }
                foreach(int syn in new[]{1,-1})
                {var plan=OutgameLoginDataPlan.Resolve("previous",42,false,true,syn);Require(!plan.ForceUpload&&plan.ReloadManagers,"nonzero local sync flag disables force-upload exception");}
                Require(OutgameLoginDataPlan.Resolve("042",42,false,false,0).ChangedUser,"source compares user identity strings without numeric normalization");
            });
            check("outgame-data-version-increment-first-zero-and-overflow",()=>{
                int saves=0,errors=0;var state=new OutgameDataVersionState(()=>saves++,()=>{},()=>{},x=>{},x=>errors++);
                state.AddGameVersion(null);state.AddGameVersion("");Require(errors==2&&saves==0&&state.Local.Count==0,"empty version keys log and do not mutate/save");
                state.AddGameVersion("local");Require(state.Local["local"].versionNumber==0&&saves==1,"first source version is zero");
                state.AddGameVersion("local");Require(state.Local["local"].versionNumber==1&&saves==2,"existing source version increments once");
                state.Local["local"].versionNumber=int.MaxValue;state.AddGameVersion("local");Require(state.Local["local"].versionNumber==int.MinValue,"source i32 addition wraps");
            });
            check("outgame-data-version-completion-queues-save-before-message",()=>{
                var calls=new List<string>();OutgameDataVersionState state=null;
                state=new OutgameDataVersionState(()=>{Require(!state.SourceFlag16,"flag cleared before completion save");calls.Add("save");},()=>calls.Add("report"),()=>calls.Add("message"),x=>calls.Add("localflag:"+x),x=>{});
                state.SourceFlag16=true;state.ForceUpload=true;state.Pending20.Add(new OutgameReportDataVersion());state.CheckCompletion();Require(calls.Count==0,"first queue delays completion");
                state.Pending20.Clear();state.Pending24.Add(new OutgameReportDataVersion());state.CheckCompletion();Require(calls.Count==0,"second queue independently delays completion");
                state.Pending24.Clear();state.CheckCompletion();Require(string.Join("|",calls)=="localflag:1|report|save|message","forced-upload flag update precedes report/save/event");
                Require(state.ForceUpload,"completion does not silently clear force-upload flag");
            });
            check("outgame-data-version-close-preserves-records-no-completion",()=>{
                int effects=0;var state=new OutgameDataVersionState(()=>effects++,()=>effects++,()=>effects++,x=>effects++,x=>effects++);
                state.Local.Add("held",new OutgameDataVersion{dataKey="held",versionNumber=8});state.Server.Add("held",new OutgameDataVersion{dataKey="held",versionNumber=9});
                state.Pending20.Add(new OutgameReportDataVersion());state.Pending24.Add(new OutgameReportDataVersion());state.SourceFlag16=state.SourceFlag18=state.ForceUpload=true;state.Close();
                Require(state.Closed&&!state.SourceFlag16&&!state.SourceFlag18&&!state.ForceUpload&&state.Pending20.Count==0&&state.Pending24.Count==0,"source close clears queues and exact flags");
                Require(effects==0&&state.Local["held"].versionNumber==8&&state.Server["held"].versionNumber==9,"close neither persists nor signals completion nor discards versions");
            });
            check("outgame-sdk-storage-async-failure-cache-and-restart",()=>{
                var disk=new StorageBackend();var errors=new List<string>();var store=new OutgameSdkStringStorage(disk,errors.Add);
                store.SetString("a","one");store.SetString("b","two");Require(disk.Calls.Count==1&&disk.Disk.Count==0&&store.GetString("b")=="two","cache updates before queued backend write");
                disk.Finish(false);Require(errors.Count==1&&disk.Calls.Count==2&&store.GetString("a")=="one","failure keeps cache and complete dispatches next write");disk.Finish(true);
                var restarted=new OutgameSdkStringStorage(disk,errors.Add);Require(restarted.GetString("a", "missing")=="missing"&&restarted.GetString("b")=="two","restart reveals actual persisted results, no fabricated failed write");
            });
            check("outgame-sdk-storage-missing-cache-delete-and-default-key",()=>{
                var disk=new StorageBackend();disk.Disk["old"]="held";var store=new OutgameSdkStringStorage(disk,x=>{});
                Require(store.GetString("absent","fallback")=="fallback"&&store.GetString("absent","next")=="next"&&disk.Reads==1,"missing cache sentinel reuses caller fallback without backend reread");
                store.SetString("","value");Require(disk.Calls[0]=="set:defaultKey"&&store.GetString("")=="value","empty key changes cache at empty name but backend uses defaultKey");disk.Finish(true);
                store.DeleteAll();Require(store.GetString("old","fallback")=="fallback"&&!store.HasKey("old"),"delete-all prevents reads of uncached old backend keys immediately");
                store.SetString("new","fresh");disk.Finish(true);disk.Finish(true);Require(store.GetString("new")=="fresh"&&disk.Disk.Count==1,"new writes survive delete-all queue in order");
                store.DeleteKey("new");Require(!store.HasKey("new"),"delete-key cache changes before backend");disk.Finish(true);Require(disk.Disk.Count==0,"queued deletion persisted");
            });
            check("outgame-version-json-original-records-defaults-duplicate-keys",()=>{
                var rows=OutgameDataVersionStorage.Decode("[{\"dataKey\":\"held\",\"versionNumber\":2147483647},{\"dataKey\":\"default\"}]");
                Require(rows.Count==2&&rows[0].versionNumber==int.MaxValue&&rows[1].versionNumber==-1,"integer precision and original constructor default survive decoding");
                var backend=new StorageBackend();backend.Disk["GameGameDataVer"]="[{\"dataKey\":\"duplicate\",\"versionNumber\":1},{\"dataKey\":\"duplicate\",\"versionNumber\":2}]";
                var state=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});state.Local.Add("old",new OutgameDataVersion());bool failed=false,completed=false;
                try{new OutgameDataVersionStorage(state,new OutgameSdkStringStorage(backend,x=>{}),"Game").Load(()=>completed=true);}catch(ArgumentException){failed=true;}
                Require(failed&&!completed&&state.Local.Count==1&&state.Local["duplicate"].versionNumber==1,"source clears first then Add rejects duplicate, retaining parsed prefix");
            });
            check("outgame-version-storage-real-file-restart-and-completion-order",()=>{
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"sdk");var work=new Queue<Action>();var backend=new OutgameFileStorageBackend(root,work.Enqueue);var sdk=new OutgameSdkStringStorage(backend,x=>{throw new Exception(x);});
                OutgameDataVersionStorage file=null;var state=new OutgameDataVersionState(()=>file.Save(),()=>{},()=>{},x=>{},x=>{});file=new OutgameDataVersionStorage(state,sdk,"Game");
                state.AddGameVersion("LocalData");Require(state.SaveObserved&&work.Count==1&&Directory.GetFiles(root).Length==0,"save marks observed and queues actual persistence");work.Dequeue()();
                var reloaded=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});bool ready=false;
                new OutgameDataVersionStorage(reloaded,new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),"Game").Load(()=>{Require(reloaded.Local["LocalData"].versionNumber==0,"versions restored before missing-manager initializer");ready=true;});
                Require(ready,"fresh storage instance reloads persisted original array format");
                var unrelated=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});new OutgameDataVersionStorage(unrelated,sdk,"AnotherGame").Load(()=>{});Require(unrelated.Local.Count==0,"source game-name key namespace isolates games");
            });
            check("outgame-manager-versions-add-prune-preserve-and-save-once",()=>{
                int saves=0;var state=new OutgameDataVersionState(()=>saves++,()=>{},()=>{},x=>{},x=>{});
                var held=new OutgameDataVersion{dataKey="held",versionNumber=17};state.Local.Add("held",held);
                state.Local.Add("disabled",new OutgameDataVersion{dataKey="disabled",versionNumber=5});state.Local.Add("removed",new OutgameDataVersion{dataKey="removed"});
                var managers=new[]{new VersionManager{DataKey="held",ParticipatesInSync=true},new VersionManager{DataKey="new",ParticipatesInSync=true},new VersionManager{DataKey="new",ParticipatesInSync=true},new VersionManager{DataKey="disabled",ParticipatesInSync=false}};
                state.InitializeManagerVersions(managers);
                Require(saves==1&&state.Local.Count==2&&ReferenceEquals(state.Local["held"],held)&&held.versionNumber==17,"source preserves retained rows, prunes removed and disabled keys, saves once");
                Require(state.Local["new"].versionNumber==-1,"new synchronized manager starts at -1, duplicate manager key does not increment");
                state.InitializeManagerVersions(managers);Require(saves==2&&state.Local["new"].versionNumber==-1,"unchanged initialization still saves without version increment");
                state.InitializeManagerVersions(new VersionManager[0]);Require(saves==3&&state.Local.Count==0,"empty registry removes all old records and persists");
            });
            check("outgame-manager-versions-load-reconcile-persisted-restart",()=>{
                var backend=new StorageBackend();backend.Disk["GameGameDataVer"]="[{\"dataKey\":\"held\",\"versionNumber\":7},{\"dataKey\":\"orphan\",\"versionNumber\":9}]";
                OutgameDataVersionStorage file=null;var state=new OutgameDataVersionState(()=>file.Save(),()=>{},()=>{},x=>{},x=>{});file=new OutgameDataVersionStorage(state,new OutgameSdkStringStorage(backend,x=>{}),"Game");
                file.LoadManagers(new[]{new VersionManager{DataKey="held",ParticipatesInSync=true},new VersionManager{DataKey="added",ParticipatesInSync=true}});
                Require(state.SaveObserved&&backend.Calls.Count==1&&state.Local.Count==2&&state.Local["held"].versionNumber==7,"load restores and reconciles before one queued save");backend.Finish(true);
                var rows=OutgameDataVersionStorage.Decode(backend.Disk["GameGameDataVer"]);
                Require(rows.Count==2&&rows.Exists(x=>x.dataKey=="held"&&x.versionNumber==7)&&rows.Exists(x=>x.dataKey=="added"&&x.versionNumber==-1),"backend receives cleaned source-version list");
            });
            check("outgame-manager-pool-registration-order-filter-replacement",()=>{
                var calls=new List<string>();var pool=new OutgameDataManagerPool(()=>calls.Add("queue"),calls.Add,calls.Add,calls.Add);
                var first=new PoolManager{DataKey="first"};var second=new PoolManager{DataKey="second"};
                first.Init=()=>{Require(pool.Managers.Count==0&&first.ParticipatesInSync&&!first.CompressData,"source flags applied before initialization, manager not registered yet");calls.Add("first");};
                second.Init=()=>{Require(ReferenceEquals(pool.Managers[4119],first)&&!second.ParticipatesInSync&&second.CompressData,"replacement initializes while old manager remains registered");calls.Add("second");};
                pool.OnInit(true,"Proj_hdzd",new[]{new OutgameManagerRegistration(4119,"Proj_hdzd",true,false,()=>first),new OutgameManagerRegistration(4617,"CommonGameModule",true,false,()=>throw new Exception("wrong namespace")),new OutgameManagerRegistration(4119,"Proj_hdzd",false,true,()=>second)});
                Require(string.Join("|",calls)=="queue|first|second|初始化DataManagerPool完成"&&pool.Managers.Count==1&&ReferenceEquals(pool.Managers[4119],second),"source namespace filter, callback ordering and dictionary assignment replacement");
                Require(pool.IsEnableSaveData,"ready after initialization");pool.SetSaveDisabled(true);Require(!pool.IsEnableSaveData,"disable reflected in readiness query");pool.OnInit(false,"Proj_hdzd",null);Require(pool.IsEnableSaveData&&pool.Managers.Count==0,"reinit resets disable and dictionary, manual mode does not enumerate registrations");
            });
            check("outgame-manager-pool-save-guards-and-exception-continuation",()=>{
                var warnings=new List<string>();var pool=new OutgameDataManagerPool(()=>{},warnings.Add,x=>{},warnings.Add);int saves=0;
                Require(!pool.IsEnableSaveData,"uninitialized registry is not ready");pool.SaveData();
                pool.OnInit(false,"Proj_hdzd",null);pool.Managers[1]=null;pool.Managers[2]=new PoolManager{DataKey="broken",Save=()=>throw new InvalidOperationException("bad data")};pool.Managers[3]=new PoolManager{DataKey="held",Save=()=>saves++};
                pool.SourceReadyFlag=false;Require(!pool.IsEnableSaveData,"query observes source ready flag");pool.SaveData();
                Require(saves==1&&warnings.Count==1&&warnings[0].StartsWith("保存数据[broken]时发生异常="),"SaveData does not test ready flag; individual failure logs and continues");
                pool.SetSaveDisabled(true);pool.SaveData();Require(saves==1&&warnings[warnings.Count-1]=="数据存储功能被禁用了","explicit disable suppresses all manager saves");
            });
            check("outgame-manager-pool-initialization-failure-retains-prefix",()=>{
                int logCount=0;var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>logCount++,x=>{});bool failed=false;
                var held=new PoolManager{DataKey="held"};
                try{pool.OnInit(true,"Proj_hdzd",new[]{new OutgameManagerRegistration(1,"Proj_hdzd",true,false,()=>held),new OutgameManagerRegistration(2,"Proj_hdzd",true,false,()=>new PoolManager{Init=()=>throw new InvalidOperationException("init failed")})});}catch(InvalidOperationException){failed=true;}
                Require(failed&&logCount==0&&pool.Managers.Count==1&&ReferenceEquals(pool.Managers[1],held),"failed initialization does not register failing manager or claim completion, prior registrations remain");
            });
            check("outgame-manager-pool-original-metadata-catalog-selection",()=>{
                var asset=Resources.Load<TextAsset>("Data/OutgameManagerRegistration");Require(asset!=null,"source metadata registry resource present");
                var created=new List<int>();var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>{});
                var entries=OutgameManagerRegistrationCatalog.Read(asset.text,id=>{created.Add(id);return new PoolManager{DataKey="fixture:"+id};});
                pool.OnInit(true,"Proj_hdzd",entries);
                Require(created.Count==16&&pool.Managers.Count==16&&pool.Managers.ContainsKey(4119)&&pool.Managers.ContainsKey(4028)&&pool.Managers.ContainsKey(4150),"original project roster includes local, commander and skins");
                Require(!pool.Managers.ContainsKey(4085)&&!pool.Managers.ContainsKey(4092)&&!pool.Managers.ContainsKey(4513),"unmarked classes are excluded");
                foreach(var manager in pool.Managers.Values)Require(manager.ParticipatesInSync&&!manager.CompressData,"original marked managers sync uncompressed");
                created.Clear();pool.OnInit(true,"CommonGameModule",entries);Require(created.Count==5&&pool.Managers.ContainsKey(4617)&&pool.Managers.ContainsKey(4692),"public module registry is selected only by its game namespace");
            });
            check("outgame-manager-storage-login-gate-version-and-upload-order",()=>{
                var h=new DataStorageHost{IsUseServer=true,SourceLoginProgress=9};var backend=new StorageBackend();var sdk=new OutgameSdkStringStorage(backend,x=>{});
                var versions=new OutgameDataVersionState(()=>h.Calls.Add("version-save"),()=>{},()=>{},x=>{},x=>{});var storage=new OutgameDataManagerStorage(()=>"LocalDataManager",h,sdk,versions);
                storage.SaveLocalData("abc");Require(storage.LastDigest=="900150983cd24fb0d6963f7d28e17f72"&&backend.Calls.Count==0&&versions.Local["LocalDataManager"].versionNumber==0,"below progress10 suppresses local write only, digest and version advance");
                Require(h.Calls[h.Calls.Count-2]=="version-save"&&h.Calls[h.Calls.Count-1]=="upload:LocalDataManager:abc","version persists before queued upload");
                h.SourceLoginProgress=10;storage.SaveLocalData("abc");Require(backend.Calls.Count==0&&versions.Local["LocalDataManager"].versionNumber==0,"unchanged digest remains skipped after login progresses");
                storage.SaveLocalData("abcd");Require(backend.Calls.Count==1&&backend.Calls[0]=="set:Proj_hdzdLocalDataManager"&&versions.Local["LocalDataManager"].versionNumber==1,"changed payload writes exact namespace key at progress10");
                backend.Finish(true);Require(storage.ReadLocalData()=="abcd","original string storage read connected");
            });
            check("outgame-manager-storage-local-disable-force-and-compression",()=>{
                var h=new DataStorageHost{LoginProcedureFlag8=true,LoginStaticFlag4=true};var backend=new StorageBackend();var sdk=new OutgameSdkStringStorage(backend,x=>{});int saves=0;
                var versions=new OutgameDataVersionState(()=>saves++,()=>{},()=>{},x=>{},x=>{});var storage=new OutgameDataManagerStorage(()=>"SkinManager",h,sdk,versions){SaveLocal=false,CompressData=true};
                storage.SaveLocalData("same");storage.SaveLocalData("same");
                Require(storage.LastDigest==""&&saves==0&&backend.Calls.Count==0&&h.Calls.FindAll(x=>x=="upload:SkinManager:compressed:same").Count==2,"source login flag bypasses digest; local disable still allows forced upload with compressed data");
                storage.SaveLocal=true;storage.AutoSyn=false;storage.SaveLocalData("same");Require(backend.Calls.Count==1&&saves==0,"non-sync managers still save locally without version/upload");backend.Finish(true);
                Require(storage.ReadLocalData()=="decoded:compressed:same","read uses matching required decompression host");
            });
            check("outgame-manager-storage-identifier-warning-does-not-abort",()=>{
                foreach(string value in new[]{"LocalDataManager","_data2","@name","中文",new string('a',511)})Require(OutgameDataManagerStorage.IsValidDataKey(value),"source valid identifier: "+value);
                foreach(string value in new[]{null,"","@","2data","a-b","a b","a.b",new string('a',512)})Require(!OutgameDataManagerStorage.IsValidDataKey(value),"source invalid identifier");
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});var storage=new OutgameDataManagerStorage(()=>"bad-key",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                storage.SaveLocalData("abc");Require(backend.Calls.Count==1&&h.Calls.Exists(x=>x.StartsWith("error:"))&&h.Calls.Exists(x=>x.StartsWith("toast:")),"original invalid key warns but does not abort saving");
                int errors=h.Calls.FindAll(x=>x.StartsWith("error:")).Count;storage.SaveLocalData("abc");Require(h.Calls.FindAll(x=>x.StartsWith("error:")).Count==errors+1&&backend.Calls.Count==1,"invalid name revalidated before digest skip");
            });
            check("outgame-cleanup-loader-concrete-providers-and-dependencies",()=>{
                var reference=new OutgameBundleReference{RefCount=2};var a=new OutgameAssetProvider(()=>false,x=>{}){Status=4,OwnerBundle=reference};var b=new OutgameAssetProvider(()=>false,x=>{}){Status=4,OwnerBundle=reference};
                var held=b.CreateHandle("held",()=>false);var providers=new List<OutgameAssetProvider>{a,b};var trace=new List<string>();bool dependencyReady=false;
                var globalProviders=new List<OutgameAssetProvider>{a,b};var providerIndex=new Dictionary<string,OutgameAssetProvider>{{"a",a},{"b",b}};
                var registry=new OutgameProviderRegistry(globalProviders,providerIndex,provider=>{Require(!globalProviders.Contains(provider),"source key lookup follows list removal");return ReferenceEquals(provider,a)?"a":"b";});
                var loader=new OutgameCleanupLoader("bundle",reference,providers,new[]{7,8},id=>{trace.Add("dependency:"+id);return dependencyReady;},list=>{Require(ReferenceEquals(list,providers)&&list.Count==2&&a.IsDestroyed&&b.IsDestroyed&&reference.RefCount==0,"unregister after destroying every provider before clear");registry.Unregister(list);trace.Add("unregister");},()=>trace.Add("destroy"));
                loader.TryDestroyAllProviders();Require(!a.IsDestroyed,"unfinished loader skips provider destruction");loader.Status=1;loader.TryDestroyAllProviders();Require(!a.IsDestroyed&&providers.Count==2,"one held provider blocks entire collection destruction");
                held.Release();reference.RefCount=3;loader.TryDestroyAllProviders();Require(!a.IsDestroyed,"extra bundle reference beyond provider count blocks release");reference.RefCount=2;
                loader.TryDestroyAllProviders();Require(providers.Count==0&&reference.RefCount==0&&string.Join(",",trace)=="unregister","concrete provider destruction releases owner refs and clears list after unregister");
                Require(globalProviders.Count==0&&providerIndex.Count==0,"concrete source registry removes both destroyed providers before held collection clear");
                trace.Clear();Require(!loader.CanDestroy()&&string.Join(",",trace)=="dependency:7","dependent gate stops at first live entry");trace.Clear();dependencyReady=true;
                Require(loader.CanDestroy()&&string.Join(",",trace)=="dependency:7,dependency:8","all dependent entries must be released");loader.Status=2;Require(loader.CanDestroy(),"source status2 also terminal");loader.Status=3;Require(!loader.CanDestroy(),"other status excluded");
            });
            check("outgame-resource-cleanup-reverse-scans-and-index-order",()=>{
                var trace=new List<string>();var a=new CleanupLoaderProbe("a",trace){Allowed=true};var b=new CleanupLoaderProbe("b",trace);var c=new CleanupLoaderProbe("c",trace){Allowed=true};
                var list=new List<IOutgameCleanupLoader>{a,b,c};var index=new Dictionary<string,IOutgameCleanupLoader>{{"a",a},{"b",b},{"c",c}};
                c.OnDestroy=()=>{Require(list.Count==3&&index.ContainsKey("c"),"destroy sees original list and dictionary entries");c.RegistryKey="mutated";};
                a.OnDestroy=()=>Require(list.Count==2&&!index.ContainsKey("c")&&index.ContainsKey("a"),"earlier reverse removal committed before next destroy");
                new OutgameResourceCleanupPass(list,index).Run();Require(string.Join(",",trace)=="providers:c,providers:b,providers:a,can:c,destroy:c,can:b,can:a,destroy:a","two complete reverse scans and stable destroy ordering");
                Require(list.Count==1&&ReferenceEquals(list[0],b)&&index.Count==1&&index.ContainsKey("b"),"key captured before destroy; only eligible entries removed");
                trace.Clear();b.Allowed=true;b.OnDestroy=()=>throw new InvalidOperationException("fixture-destroy");bool threw=false;
                try{new OutgameResourceCleanupPass(list,index).Run();}catch(InvalidOperationException){threw=true;}Require(threw&&list.Count==1&&index.ContainsKey("b"),"destroy failure leaves registry entries in place");
                b.OnDestroy=null;new OutgameResourceUnusedCleanup(()=>true,new OutgameResourceCleanupPass(list,index).Run,x=>{}).UnloadUnusedAssets();Require(list.Count==0&&index.Count==0,"ten-pass entry consumes concrete cleanup traversal");
            });
            check("outgame-resource-unload-package-order-and-ten-passes",()=>{
                bool present=false,allowed=false;var trace=new List<string>();
                var system=new OutgameResourceUnusedCleanup(()=>allowed,()=>trace.Add("pass"),message=>{Require(message=="Can not unload unused assets when processing resource loading !","source warning text");trace.Add("warning");});
                var package=new OutgameResourcePackageUnload(()=>present,()=>trace.Add("update"),system.UnloadUnusedAssets);
                package.UnloadUnusedAssets();Require(trace.Count==0,"uninitialized package skips request");
                present=true;package.UnloadUnusedAssets();Require(string.Join(",",trace)=="update,warning","package updates before source flag-gated warning");
                trace.Clear();allowed=true;package.UnloadUnusedAssets();Require(trace.Count==11&&trace[0]=="update"&&trace.FindAll(x=>x=="pass").Count==10,"ten cleanup passes after update");
                package.UnloadUnusedAssets();Require(trace.Count==22,"repeated request is not coalesced by this source layer");
                int passes=0;var failing=new OutgameResourceUnusedCleanup(()=>true,()=>{passes++;throw new InvalidOperationException("fixture-pass");},x=>{});bool threw=false;
                try{failing.UnloadUnusedAssets();}catch(InvalidOperationException){threw=true;}Require(threw&&passes==1,"pass exception propagates without invented retry");
            });
            check("outgame-ui-resource-dictionaries-release-held-handles",()=>{
                var first=new GameObject("dynamic-owner");var second=new GameObject("custom-owner");var empty=new GameObject("null-handle-owner");
                try{
                    var warnings=new List<string>();var provider=new OutgameAssetProvider(()=>false,warnings.Add);var dynamic=provider.CreateHandle("dynamic",()=>false);var custom=provider.CreateHandle("custom",()=>false);
                    var heldDynamic=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{first,dynamic},{empty,null}};var heldCustom=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{second,custom}};
                    int unloads=0;OutgameUiResourceLists lists=null;
                    lists=new OutgameUiResourceLists(()=>{unloads++;Require(unloads==1?lists.Dynamic!=null&&provider.RefCount==1:lists.Custom!=null&&provider.RefCount==0,"unload after handle releases and before dictionary field reset");}){Dynamic=heldDynamic,Custom=heldCustom};
                    lists.ReleaseDynamic();Require(lists.Dynamic==null&&ReferenceEquals(lists.Custom,heldCustom)&&heldDynamic.Count==2,"dynamic resets field without clearing external dictionary or touching custom");
                    lists.ReleaseCustom();Require(lists.Custom==null&&provider.RefCount==0&&warnings.Count==0&&unloads==2,"actual provider ref counts drop once per owned handle");
                    lists.ReleaseDynamic();lists.ReleaseCustom();Require(unloads==2,"null collections skip unload entirely");
                    var emptyList=new Dictionary<UnityEngine.Object,OutgameAssetHandle>();var failure=new OutgameUiResourceLists(()=>throw new InvalidOperationException("fixture-unload")){Dynamic=emptyList};
                    bool threw=false;try{failure.ReleaseDynamic();}catch(InvalidOperationException){threw=true;}Require(threw&&ReferenceEquals(failure.Dynamic,emptyList),"empty nonnull dictionary still unloads; exception retains field before reset");
                }finally{UnityEngine.Object.DestroyImmediate(first);UnityEngine.Object.DestroyImmediate(second);UnityEngine.Object.DestroyImmediate(empty);}
            });
            check("outgame-ui-open-registry-original-reuse-and-failure-state",()=>{
                var pages=new Dictionary<string,object>();var errors=new List<string>();var resolutions=new List<string>();int created=0,opened=0;var args=new object[]{17};
                OutgameUiOpenRegistry<object> registry=null;
                registry=new OutgameUiOpenRegistry<object>(pages,name=>{resolutions.Add(name);return new OutgameUiType<object>("FestActUI",()=>{created++;return new object();});},(page,arguments)=>{
                    opened++;Require(ReferenceEquals(pages["FestActUI"],page)&&ReferenceEquals(arguments,args),"new page registered before original argument dispatch");
                    Require(ReferenceEquals(registry.Open("OtherNamespace","Alias",null),page),"reentrant Open returns registered instance without reloading");
                },errors.Add);
                var first=registry.Open("Game","FestActUI",args);Require(created==1&&opened==1&&resolutions[0]=="Game.FestActUI"&&resolutions[1]=="OtherNamespace.Alias","type resolution before simple-name registry lookup");
                Require(ReferenceEquals(registry.Open("Game","FestActUI",new object[]{99}),first)&&created==1&&opened==1,"duplicate open does not replace arguments, refresh or show again");
                object replacement=null;var close=new OutgameUiCloseRegistry<object>(pages,page=>replacement=registry.Open("Game","FestActUI",args));close.CloseForName("FestActUI");
                Require(!ReferenceEquals(first,replacement)&&ReferenceEquals(pages["FestActUI"],replacement)&&created==2&&opened==2,"shared close registry removal permits new instance during old close");
                pages["FestActUI"]=null;var unregistered=registry.Open("Game","FestActUI",args);
                Require(unregistered!=null&&pages["FestActUI"]==null&&created==3&&opened==2&&errors.Count==1,"null entry creates instance but duplicate Add is caught; source does not repair dictionary");
                pages.Clear();var failed=new OutgameUiOpenRegistry<object>(pages,name=>new OutgameUiType<object>("FestActUI",()=>new object()),(page,arguments)=>throw new InvalidOperationException("fixture-open"),errors.Add);
                var retained=failed.Open("Game","FestActUI",args);Require(ReferenceEquals(retained,pages["FestActUI"])&&errors[1].StartsWith("fixture-open"),"open error logs message plus stack and retains registered instance");
                pages.Clear();var badCtor=new OutgameUiOpenRegistry<object>(pages,name=>new OutgameUiType<object>("FestActUI",()=>throw new InvalidOperationException("fixture-constructor")),(page,arguments)=>{},errors.Add);
                Require(badCtor.Open("Game","FestActUI",args)==null&&pages.Count==0&&errors[2].StartsWith("fixture-constructor"),"constructor failure returns null without registering");
                var badType=new OutgameUiOpenRegistry<object>(pages,name=>throw new InvalidOperationException("fixture-type"),(page,arguments)=>{},errors.Add);bool propagated=false;
                try{badType.Open("Missing","Page",null);}catch(InvalidOperationException){propagated=true;}
                Require(propagated&&errors.Count==3,"type lookup failure remains outside source creation catch");
            });
            check("outgame-ui-window-maximum-index-not-entry-count",()=>{
                var parent=new GameObject("UI root",typeof(RectTransform),typeof(Canvas));var low=new GameObject("low",typeof(RectTransform));low.transform.SetParent(parent.transform,false);
                var high=new GameObject("hidden-high",typeof(RectTransform));high.transform.SetParent(parent.transform,false);high.SetActive(false);
                var next=new GameObject("next",typeof(RectTransform));next.transform.SetParent(parent.transform,false);
                try{
                    var pages=new Dictionary<string,Tuple<GameObject,int,int>>{{"low",Tuple.Create(low,1,3)},{"high",Tuple.Create(high,1,7)},{"not-window",Tuple.Create(low,2,99)},{"not-loaded",Tuple.Create((GameObject)null,1,100)}};
                    Func<int> maximum=()=>OutgameUiWindowIndex.Maximum(pages.Values,p=>p.Item2,p=>p.Item1,p=>p.Item3);
                    Require(maximum()==7,"maximum sequence includes inactive loaded windows and ignores non-window/unloaded entries");
                    var canvas=new OutgameUiCanvas(maximum,()=>false);canvas.Add(next,1);Require(canvas.WindowIndex==8&&next.GetComponent<Canvas>().sortingOrder==31,"new canvas follows highest existing sequence, not dictionary count");
                    UnityEngine.Object.DestroyImmediate(high);Require(maximum()==3,"Unity destroyed object compares null and no longer contributes");
                    pages.Remove("low");Require(maximum()==0,"no loaded windows yields zero despite other entries");
                }finally{UnityEngine.Object.DestroyImmediate(parent);}
            });
            check("outgame-concrete-load-request-through-open-host",()=>{
                var parent=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));GameObject root=null;
                Require(parent.CompareTag("GFUICanvas")&&parent.GetComponent<Canvas>().worldCamera==parent.transform.Find("UICamera").GetComponent<Camera>(),"original startup tag and camera binding");
                var moduleRoot=new OutgameUiRootInitialization(parent,()=>false,()=>throw new Exception("portrait short circuit"),null);
                moduleRoot.Loaded((name,active)=>{var instance=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));instance.name=name;instance.SetActive(active);return instance;});
                var layerNode=moduleRoot.UiRoot.Find("UIWindow").gameObject;
                var nodeCache=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>moduleRoot.UiRoot,()=>parent,message=>throw new Exception(message));
                try{
                    var trace=new List<string>();var lifetime=new OutgameUiLifetime(null,()=>{});var events=new OutgameMessageDispatcher();var page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);
                    var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FestActivity/hud-import").text,"ValentineUI");OutgameUiObjectInitialization init=null;
                    init=new OutgameUiObjectInitialization(lifetime,page,outlets.Read,()=>{Require(init.Objects.Count==13&&init.Objects["Claim"]==root.transform.Find("go_Main/Area1/Main/Claim").gameObject,"all original outlets bound");trace.Add("components");},()=>trace.Add("init"),()=>trace.Add("skin"),()=>trace.Add("awake"));
                    events.AddListener("GF_VisibleUI",args=>trace.Add("visible"));events.AddListener("OpenUI",args=>throw new Exception("cached page must not open"));
                    var host=new OutgameUiOpenHost(lifetime,page,init,new OutgameUiCanvas(()=>0,()=>false),null,()=>{Require(root.GetComponent<Canvas>().sortingOrder==10,"canvas before closeLoading");trace.Add("loading");},()=>trace.Add("refresh"),()=>throw new Exception("cached"),()=>throw new Exception("cached"),message=>trace.Add(message)){Cached=true,Layer=1};
                    var provider=new OutgameAssetProvider(()=>false,Debug.LogWarning){Status=4,AssetObject=Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI")};var handle=provider.CreateHandle("UI/FestActUI/ValentineUI",()=>false);OutgameAssetHandle main=null;System.Collections.IEnumerator waiting=null;
                    var args=new object[]{"original-args"};
                    var loader=new OutgameUiModernLoader((path,ready)=>{Require(path=="UI/FestActUI/ValentineUI"&&ReferenceEquals(lifetime.Arguments,args)&&main==null,"source prefix and arguments before loading");trace.Add("load");ready(handle);Require(main==null&&!init.IsInitialized,"synchronous asset completion precedes returned handle assignment");return handle;},layer=>{Require(layer=="UIWindow","source layer name");trace.Add("node");return nodeCache.Get(layer);},routine=>{waiting=routine;root=layerNode.transform.GetChild(0).gameObject;Require(!root.activeSelf,"modern instance hidden before scheduling completion");trace.Add("schedule");},(path,layer,ready)=>throw new Exception("legacy not expected"));
                    var request=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(host,()=>events),()=>{Require(ReferenceEquals(lifetime.Arguments,args),"arguments stored before Loading");trace.Add("show");},()=>{trace.Add("flag");return true;},()=>{trace.Add("module");return loader;},()=>{trace.Add("path");return "FestActUI/ValentineUI";},()=>{trace.Add("layer");return "UIWindow";},value=>{main=value;trace.Add("handle");});
                    request.Open(args);var rect=root.GetComponent<RectTransform>();
                    Require(ReferenceEquals(main,handle)&&!init.IsInitialized&&rect.anchorMin==Vector2.zero&&rect.anchorMax==Vector2.one&&rect.offsetMin==Vector2.zero&&rect.offsetMax==Vector2.zero,"real instance stretched and handle assigned before page initialization");
                    Require(string.Join(",",trace)=="show,flag,module,path,layer,load,node,schedule,handle","source request/module callback order");
                    Require(waiting.MoveNext()&&waiting.Current==null&&!init.IsInitialized&&waiting.MoveNext()&&waiting.Current==null&&!init.IsInitialized,"two independent null frame yields before completion");
                    Require(!waiting.MoveNext()&&init.IsInitialized&&root.activeSelf&&ReferenceEquals(host.PageIdentity,page),"after two frames concrete initializer activates same page");
                    Require(string.Join(",",trace).EndsWith("components,init,visible,skin,awake,loading,refresh,缓存模式，不播放动画 不抛事件"),"load connects full cached lifecycle in original order");main.Release();Require(provider.RefCount==0,"main handle retains actual provider ownership");
                }finally{UnityEngine.Object.DestroyImmediate(parent);}
            });
            check("outgame-legacy-manifest-operation-asset-request-and-publication",()=>{
                var request=new LegacyAssetRequestProbe{Value=.5f};int queries=0,loads=0,publishes=0;bool available=false;
                OutgameLegacyBundleLookup lookup=(string name,out string error,out int missing)=>{queries++;error=null;missing=0;return available?new OutgameLegacyBundleResult():null;};
                var operation=new OutgameLegacyManifestOperation("manifest",lookup,value=>{Require(value==null,"null manifest result is published without guard");publishes++;},message=>throw new Exception(message),bundle=>{loads++;return request;});
                Require(operation.Update()&&operation.GetAsset()==null&&operation.Progress()==0&&operation.MoveNext()&&loads==0,"unavailable wrapper stays queued");available=true;
                Require(operation.Update()&&operation.Progress()==.5f&&operation.MoveNext()&&loads==1&&queries==2,"asset request keeps derived operation queued");
                request.Done=true;Require(!operation.Update()&&!operation.MoveNext()&&publishes==1&&loads==1&&queries==2,"done request publishes and leaves manager queue");
                Require(!operation.Update()&&publishes==2,"explicit update after done repeats publication as source");
                var wrongType=new TextAsset("wrong-type");try{request.Result=wrongType;Require(operation.GetAsset()==null,"source safe generic cast returns null for a different Unity object type");}finally{UnityEngine.Object.DestroyImmediate(wrongType);}

            });
            check("outgame-legacy-manifest-error-coroutine-and-manager-diverge",()=>{
                string failure=null;var logs=new List<string>();int loads=0;
                OutgameLegacyBundleLookup lookup=(string name,out string error,out int missing)=>{error=failure;missing=0;return null;};
                var operation=new OutgameLegacyManifestOperation("manifest",lookup,value=>throw new Exception("unexpected publish"),logs.Add,bundle=>{loads++;return null;});
                Require(operation.Update()&&operation.MoveNext(),"no error yet");failure="0";
                Require(operation.Update()&&!operation.MoveNext()&&logs[0]=="0"&&loads==0,"manifest treats literal zero error as completion, while Update still retains operation");
                failure="";Require(operation.Update()&&operation.IsDone()&&logs[1]=="","empty error also logs and completes");
            });
            check("outgame-legacy-unified-runtime-cached-request-and-unload-state",()=>{
                var messages=new List<string>();var runtime=new OutgameLegacyBundleRuntime(()=>throw new Exception("cached path must not query service"),messages.Add,messages.Add,messages.Add,ex=>throw ex);
                Require(runtime.UnloadInterval==60&&!runtime.DisableUnload&&runtime.ActiveVariants.Length==0&&runtime.Manifest==null&&runtime.GetRequest("absent")==null,"source initial state and absent-request lookup");
                var item=new OutgameLegacyBundleResult{BundleName="bundle",ReferenceCount=1};runtime.Loaded.Add("bundle",item);
                var operation=runtime.LoadAsync("bundle");Require(runtime.Operations.Count==1&&operation.MoveNext()&&messages.Count==1,"manifest gate reports error but source still enqueues operation");
                runtime.Update();Require(runtime.Operations.Count==0&&!operation.MoveNext()&&ReferenceEquals(operation.GetAssetBundle(),item),"unified manager pump resolves cached wrapper");
                runtime.Unload("bundle");Require(item.ReferenceCount==0&&item.UnloadRemaining==60&&runtime.PendingUnload.Count==1,"runtime unload uses original initial delay");
                runtime.Acquisition.LoadInternal("bundle",true);Require(item.ReferenceCount==1&&runtime.PendingUnload.Count==0,"shared reuse cancels pending unload");
            });
            check("outgame-original-tipui-native-animation-sampling",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FirstPack/TipUI/TipUI"));
                try{
                    var content=root.transform.Find("Content");var animation=content.GetComponent<Animation>();
                    Require(animation!=null&&!animation.enabled&&animation.playAutomatically&&animation.GetClipCount()==2&&animation.clip.name=="hdzd_setPanel_ani","source disabled native component and default clip references");
                    var open=animation.GetClip("hdzd_setPanel_ani");var close=animation.GetClip("hdzd_setPanelClose_ani");Require(open.legacy&&close.legacy&&open.length==1&&close.length==1,"source legacy clip duration retained");
                    open.SampleAnimation(content.gameObject,0);Require(content.localScale==Vector3.zero,"source open initial scale");
                    open.SampleAnimation(content.gameObject,.3f);Require(Vector3.Distance(content.localScale,new Vector3(1.2f,1.2f,1))<.00001f,"source open overshoot key");
                    open.SampleAnimation(content.gameObject,1);Require(content.localScale==Vector3.one,"source open final scale");
                    close.SampleAnimation(content.gameObject,0);Require(content.localScale==Vector3.one,"source close initial scale");
                    close.SampleAnimation(content.gameObject,1);Require(content.localScale==new Vector3(0,0,1),"source close preserves Z one while hiding XY");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-original-tipui-hierarchy-and-outlet-bindings",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/FirstPack/TipUI/TipUI");Require(prefab!=null,"source TipUI imported");
                var root=UnityEngine.Object.Instantiate(prefab);
                try{
                    Require(root.GetComponentsInChildren<RectTransform>(true).Length==13&&root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Length==3&&root.GetComponentsInChildren<UnityEngine.UI.Text>(true).Length==4,"source node and control topology");
                    var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/FirstPack/TipUI/hud-import").text,"TipUI");var bindings=new Dictionary<string,object>();foreach(var pair in outlets.Read(root))bindings.Add(pair.Key,pair.Value);
                    Require(bindings.Count==6&&ReferenceEquals(bindings["textContent"],root.transform.Find("Content/bg1/textContent").gameObject)&&ReferenceEquals(bindings["btnClose"],root.transform.Find("Content/btnClose").gameObject),"source object IDs resolve all six original outlet paths");
                    foreach(var text in root.GetComponentsInChildren<UnityEngine.UI.Text>(true))Require(text.font!=null,"original font resolves");
                    foreach(var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))Require(button.targetGraphic!=null,"source button target reference resolves");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-original-firstpack-config-and-font-byte-import",()=>{
                var audit=JsonUtility.FromJson<FirstPackRawAudit>(File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/FIRSTPACK_RAW_ASSETS_AUDIT.json")));
                int texts=0,fonts=0;using(var hash=System.Security.Cryptography.SHA256.Create())foreach(var item in audit.assets){
                    byte[] bytes;
                    if(item.type=="TextAsset"){
                        var asset=Resources.Load<TextAsset>(item.resourceKey);Require(asset!=null,"original config resolves: "+item.resourceKey);bytes=asset.bytes;texts++;
                    }else{
                        Require(Resources.Load<Font>(item.resourceKey)!=null,"original font imports: "+item.resourceKey);bytes=File.ReadAllBytes(Path.Combine(BattleBuild.Workspace,item.destination));fonts++;
                    }
                    Require(bytes.Length==item.bytes&&BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-","").ToLowerInvariant()==item.sha256,"source byte equality: "+item.resourceKey);
                }
                Require(texts==85&&fonts==2,"all original firstpack config and font payloads covered");
            });
            check("outgame-original-pack-lists-native-resources-and-initialization",()=>{
                var first=Resources.Load<TextAsset>("firstpack");var second=Resources.Load<TextAsset>("pack2");Require(first!=null&&second!=null,"source ResourceManager keys resolve imported TextAssets");
                using(var hash=System.Security.Cryptography.SHA256.Create()){
                    Require(BitConverter.ToString(hash.ComputeHash(first.bytes)).Replace("-","").ToLowerInvariant()=="ac856dc6f4d61f9f9a62c5327b515524884e2a568e54597d91cd0fe9be8dd57e","firstpack matches original bytes");
                    Require(BitConverter.ToString(hash.ComputeHash(second.bytes)).Replace("-","").ToLowerInvariant()=="cdaabe628692073ac0eb10642b420880d9ec4e8765155058b80573f246c0f9b1","pack2 matches original bytes");
                }
                var aliases=OutgameLegacyPackListJson.Deserialize(first.text);var later=OutgameLegacyPackListJson.Deserialize(second.text);
                Require(aliases.Count==7&&aliases[0]=="ui/uiroot.prefab.unity3d"&&aliases[1]=="data/config.unity3d"&&aliases[6]=="ui/mainmenu/tipui.prefab.unity3d","original firstpack ordering");
                Require(later.Count==85&&later.Contains("ui/mainmenu/proj_xqzdstartui.prefab.unity3d")&&later.Contains("ui/mainmenu/menutabui.prefab.unity3d"),"original second pack contains production start/menu resources");
                List<string> received=null;int started=0;var service=new BundleServicesProbe{Lookup=name=>new OutgameLegacyBundleLocation{LocalPath="fixture"}};
                var init=new OutgameLegacyResourceInitialization(()=>{},()=>null,()=>service,null,names=>{received=names;return null;},routine=>started++,message=>{},createMono:()=>null);
                init.Initialize();Require(started==1&&received.Count==7&&received[1]==aliases[1],"default Resources reader and valid-list adapter feed original firstpack names into initialization");
            });
            check("outgame-legacy-resource-initialize-early-flag-and-live-callback",()=>{
                var trace=new List<string>();var owner=new object();var list=new List<string>();System.Collections.IEnumerator pending=null;var packs=new Dictionary<string,AssetBundle>();var token=new object();GameObject mono=null;
                OutgameLegacyResourceInitialization init=null;var service=new BundleServicesProbe{Lookup=name=>{Require(init.IsInitialized&&name=="firstpack.unity3d","initialized before service lookup");trace.Add("service");return new OutgameLegacyBundleLocation{LocalPath="fixture"};}};
                var first=new OutgameLegacyFirstPack(name=>new LegacyBundleOperationProbe(()=>new OutgameLegacyBundleResult()),routine=>token,new OutgameLegacyPackRegistry(packs,trace.Add),trace.Add,trace.Add,()=>init.Initialized?.Invoke());
                try{
                    init=new OutgameLegacyResourceInitialization(()=>trace.Add("route"),()=>{trace.Add("manager");return owner;},()=>service,text=>{Require(text=="fixture-json","source text is passed unchanged");trace.Add("json");return list;},first.Run,routine=>{trace.Add("start");pending=routine;},trace.Add,name=>{Require(name=="firstpack","Resources key");trace.Add("text");return "fixture-json";},()=>{trace.Add("mono");return mono=new GameObject("ResourcesMono");});
                    int initial=0,replacement=0;init.Initialized=()=>initial++;init.Initialize();
                    Require(init.IsInitialized&&ReferenceEquals(init.BundleManager,owner)&&ReferenceEquals(init.MonoObject,mono)&&pending!=null&&initial==0,"flag becomes true before asynchronous first pack completion");
                    Require(string.Join(",",trace)=="route,manager,mono,service,text,json,start","native initialization dependency order");
                    init.Initialized=()=>replacement++;init.Initialize();Require(replacement==1&&initial==0&&trace.Count==7,"repeat initialize immediately invokes current callback without reloading");
                    Require(pending.MoveNext()&&ReferenceEquals(pending.Current,token),"firstpack awaits operation");Require(!pending.MoveNext()&&replacement==2&&initial==0,"firstpack completion invokes current callback again");
                }finally{if(mono!=null)UnityEngine.Object.DestroyImmediate(mono);}
            });
            check("outgame-legacy-resource-initialize-no-pack-and-failure-state",()=>{
                int complete=0,loads=0;var trace=new List<string>();var record=new OutgameLegacyBundleLocation();var service=new BundleServicesProbe{Lookup=name=>record};
                Func<OutgameLegacyResourceInitialization> create=()=>new OutgameLegacyResourceInitialization(()=>trace.Add("route"),()=>null,()=>service,text=>new List<string>(),names=>null,routine=>loads++,trace.Add,name=>throw new InvalidOperationException("missing text"),()=>null);
                var init=create();init.Initialized=()=>complete++;init.Initialize();Require(init.IsInitialized&&complete==1&&loads==0&&trace[1]=="ResourcesModule初始化完成","no local firstpack path logs and completes synchronously");
                record.LocalPath=" ";init=create();init.Initialized=()=>complete++;bool threw=false;try{init.Initialize();}catch(InvalidOperationException){threw=true;}
                Require(threw&&init.IsInitialized&&complete==1&&loads==0,"whitespace path attempts Resources text, failure preserves early flag");init.Initialize();Require(complete==2,"retry after load exception takes initialized callback branch");
                record=null;init=create();threw=false;try{init.Initialize();}catch(NullReferenceException){threw=true;}Require(threw&&init.IsInitialized,"null record has no fallback and leaves initialized flag true");
            });
            check("outgame-legacy-firstpack-await-register-and-complete-order",()=>{
                var packs=new Dictionary<string,AssetBundle>();var trace=new List<string>();object token=new object();int completed=0;
                var operation=new LegacyBundleOperationProbe(()=>{trace.Add("result");return new OutgameLegacyBundleResult();});
                var first=new OutgameLegacyFirstPack(name=>{Require(name=="firstpack.unity3d","exact first-pack key");trace.Add("load");return operation;},routine=>{Require(ReferenceEquals(routine,operation),"start actual operation");trace.Add("start");return token;},new OutgameLegacyPackRegistry(packs,trace.Add),trace.Add,trace.Add,()=>{completed++;Require(packs.ContainsKey("a")&&packs.ContainsKey("b"),"register all aliases before notifying");trace.Add("complete");});
                var names=new List<string>{"a"};var run=first.Run(names);Require(trace.Count==0,"iterator body stays deferred");
                Require(run.MoveNext()&&ReferenceEquals(run.Current,token)&&packs.Count==0&&completed==0,"await coroutine token before result/registration");names.Add("b");
                Require(!run.MoveNext()&&completed==1&&packs.Count==2&&packs["a"]==null,"list is live and null native bundle is registered without invented guard");
                Require(string.Join(",",trace)=="加载firstpack,load,start,result,ResourcesModule初始化完成,complete","source order");
            });
            check("outgame-legacy-firstpack-failure-does-not-notify",()=>{
                var packs=new Dictionary<string,AssetBundle>();var trace=new List<string>();int completed=0;IOutgameLegacyBundleOperation operation=null;
                var first=new OutgameLegacyFirstPack(name=>operation,routine=>null,new OutgameLegacyPackRegistry(packs,trace.Add),trace.Add,trace.Add,()=>completed++);
                var run=first.Run(new List<string>());Require(!run.MoveNext()&&completed==0&&trace[1]=="FirstPackError，请检测首包资源是否存在","missing operation stops before initialization callback");
                operation=new LegacyBundleOperationProbe(()=>null);run=first.Run(new List<string>());Require(run.MoveNext(),"await before result access");bool threw=false;try{run.MoveNext();}catch(NullReferenceException){threw=true;}Require(threw&&completed==0&&packs.Count==0,"null result propagates before registration and completion");
                operation=new LegacyBundleOperationProbe(()=>new OutgameLegacyBundleResult());run=first.Run(new List<string>{"one","one"});run.MoveNext();threw=false;try{run.MoveNext();}catch(NullReferenceException){threw=true;}Require(threw&&completed==0&&packs.ContainsKey("one"),"duplicate null pack fails on original name access and retains prior insertion");
            });
            check("outgame-legacy-pack-registry-retains-first-owner",()=>{
                var path=Path.GetFullPath("../analysis/native-legacy-ui-bundles/valentine");var bundle=AssetBundle.LoadFromFile(path);
                Require(bundle!=null,"existing native validation bundle fixture");
                try{
                    var packs=new Dictionary<string,AssetBundle>();var errors=new List<string>();var registry=new OutgameLegacyPackRegistry(packs,errors.Add);
                    registry.Add("Case/asset",bundle);registry.Add("Case/asset",bundle);
                    Require(errors.Count==1&&errors[0]=="资源[{aseetBundleName}]已经添加到pack["+bundle.name+"]中，无需添加"&&packs.Count==1,"same-pack source diagnostic preserves literal placeholder");
                    packs.Add("other",null);registry.Add("other",bundle);
                    Require(packs["other"]==null&&errors.Count==2&&errors[1].StartsWith("错误的资源引用：资源[{aseetBundleName}]存在于多个pack中:"),"conflicting owner is retained with diagnostic");
                }finally{bundle.Unload(true);}
            });
            check("outgame-legacy-patch-manifest-format-and-version-boundaries",()=>{
                var manifest=new OutgameLegacyPatchManifest("7| date |gf,build, 1 |seed\r\na|obf|hash|18446744073709551615|9\nb|two|h|2\nc|three|h|3|99|extra\nshort\n",message=>throw new Exception(message));
                Require(manifest.ResourceVersion==7&&manifest.ResourceVersionData==" date "&&manifest.GFVersion=="gf,build, 1 "&&manifest.IsEncrypAB&&manifest.RandomSeed=="seed\r","header retains whitespace and CR, fourth column enables encryption");
                Require(manifest.TryGetValue("a",out var a)&&a.Size==ulong.MaxValue&&a.Version==9&&a.ObfuscatorName=="obf"&&a.MD5=="hash","five-column override and unsigned size");
                Require(manifest.TryGetValue("b",out var b)&&b.Version==7&&manifest.TryGetValue("c",out var c)&&c.Version==7&&!manifest.TryGetValue("short",out _),"four and six columns inherit header, short lines ignored");
                bool duplicate=false;try{new OutgameLegacyPatchManifest("1|d|g\nx|a|h|1\nx|b|h|2",message=>{});}catch(ArgumentException){duplicate=true;}Require(duplicate,"source Add rejects duplicate keys");
                bool malformed=false;try{new OutgameLegacyPatchManifest("1|d",message=>{});}catch(IndexOutOfRangeException){malformed=true;}Require(malformed,"malformed header propagates indexing failure");
                malformed=false;try{new OutgameLegacyPatchManifest("1|d|g\nx|a|h|-1",message=>{});}catch(OverflowException){malformed=true;}Require(malformed,"negative unsigned size is not silently accepted");
            });
            check("outgame-legacy-patch-services-record-resolution-and-missing-entry",()=>{
                var messages=new List<string>();var service=new OutgameLegacyPatchBundleServices("7|d|gf,build, 1 |seed\na|obf|hash|5|9","file:///root/",2,messages.Add,messages.Add,()=>true,messages.Add);
                var record=service.GetAssetBundleInfo("a");Require(record.LocalPath=="file:///root//obf"&&record.RemoteURL==""&&record.Version==9&&record.IsEncrypAB&&record.RandomSeed=="seed"&&!record.IsInApp&&record.BundleName=="a","source concatenation preserves duplicate slash and all constructor fields");
                Require(messages.Count==1&&messages[0].Contains("isInApp[False]")&&service.GetGFVersion()=="gf"&&service.GetUACBuildID()=="build"&&service.GetIsDeepObf(),"source debug record and framework split/trim");
                ulong offset=99;var resolver=new OutgameLegacyBundleResolver(()=>service,value=>offset=value,messages.Add);
                Require(resolver.ResolveUrl("unknown")==null&&offset==0&&messages.Count==3&&messages[1]=="Not found element in patch manifest : unknown"&&messages[2]=="[unknown]本地资源未找到 offset=0","missing service record and resolver warnings occur in source order");
                var local=new OutgameLegacyPatchBundleServices("1|d|g\na|obf|h|1",null,1,messages.Add,messages.Add,()=>false,messages.Add);Require(local.GetAssetBundleInfo("a").LocalPath=="/obf"&&local.GetAssetBundleInfo("a").IsInApp,"null root concatenates slash, mode exactly one is in-app");
            });
            check("outgame-legacy-streaming-service-reload-and-empty-manifest",()=>{
                var messages=new List<string>();string prefix="first/";int reads=0;
                var service=new OutgameLegacyStreamingBundleServices(()=>{reads++;return prefix;},messages.Add,messages.Add,()=>false,messages.Add);
                Require(service.GetGFVersion()=="empty"&&service.GetUACBuildID()=="empty"&&!service.GetIsDeepObf(),"uninitialized metadata defaults");
                Require(service.GetAssetBundleInfo("a").LocalPath==""&&reads==0,"missing manifest avoids streaming path query");
                service.ReadStreamingPatchManifest("1|d|g\na|obf|h|1");Require(service.GetAssetBundleInfo("a").LocalPath=="first/obf","streaming path used without slash insertion");prefix="second";
                Require(service.GetAssetBundleInfo("a").LocalPath=="secondobf"&&reads==2,"streaming property re-read on each success");
                bool failed=false;try{service.ReadStreamingPatchManifest("bad");}catch(FormatException){failed=true;}Require(failed&&service.GetAssetBundleInfo("a").LocalPath=="secondobf","failed replacement preserves previous manifest");
                service.ReadStreamingPatchManifest("");Require(messages.Contains("清单文件为空")&&service.GetAssetBundleInfo("a").LocalPath=="","empty replacement installs initialized empty dictionary");
                failed=false;try{service.GetGFVersion();}catch(NullReferenceException){failed=true;}Require(failed,"empty object differs from absent manifest: GFVersion remains null");
            });
            check("outgame-legacy-source-resolver-offset-path-and-service-requery",()=>{
                var trace=new List<string>();ulong offset=99;int queries=0;var record=new OutgameLegacyBundleLocation{LocalPath="file:///fixture",IsEncrypAB=true};
                var service=new BundleServicesProbe{Lookup=name=>{trace.Add(name);return record;}};
                var resolver=new OutgameLegacyBundleResolver(()=>{queries++;return service;},value=>{offset=value;trace.Add("offset");},trace.Add);
                Require(ReferenceEquals(resolver.Resolve("Case/Bundle"),record)&&offset==16&&queries==1&&trace[0]=="Case/Bundle","preserve original name, exact record and encryption offset");
                record.LocalPath="";Require(resolver.ResolveUrl("missing")==null&&offset==16&&trace[trace.Count-1]=="[missing]本地资源未找到 offset=16","empty local path warns after offset write");
                record.LocalPath=null;record.IsEncrypAB=false;Require(resolver.Resolve("null")==null&&offset==0,"null path resets offset before warning");
                record.LocalPath=" ";Require(resolver.ResolveUrl("space")==" "&&queries==4,"whitespace accepted and current service queried for each request");
                Require(OutgameLegacyResourceDefaults.InitialAssetUnloadInterval==60,"source Utility static constructor initializes 60 seconds");
            });
            check("outgame-legacy-source-resolver-exception-boundaries",()=>{
                ulong offset=7;var service=new BundleServicesProbe{Lookup=name=>null};int warnings=0;
                var resolver=new OutgameLegacyBundleResolver(()=>service,value=>offset=value,message=>warnings++);
                bool threw=false;try{resolver.Resolve("missing-record");}catch(NullReferenceException){threw=true;}
                Require(threw&&offset==7&&warnings==0,"source has no null record guard, offset remains untouched");
                service.Lookup=name=>new OutgameLegacyBundleLocation{IsEncrypAB=true};
                resolver=new OutgameLegacyBundleResolver(()=>service,value=>offset=value,message=>throw new InvalidOperationException("logger"));
                threw=false;try{resolver.Resolve("missing-path");}catch(InvalidOperationException){threw=true;}
                Require(threw&&offset==16,"warning exception propagates after offset mutation");
            });
            check("outgame-legacy-dependency-cache-remap-and-duplicate-accounting",()=>{
                var cache=new Dictionary<string,string[]>();int queries=0,remaps=0;bool available=false;var calls=new List<string>();var original=new[]{"a","a"};
                var deps=new OutgameLegacyBundleDependencies(cache,()=>available,name=>{queries++;return name=="empty"?new string[0]:original;},name=>{remaps++;return name+".hd";},(name,suppress)=>{Require(!suppress&&cache.ContainsKey("root"),"cache registered before dependency acquisition with duplicate accounting enabled");calls.Add(name);return true;},()=>true,message=>calls.Add(message),message=>calls.Add("error"));
                deps.Load("root");Require(queries==0&&calls.Count==1&&calls[0]=="error","manifest gate before cached or fresh lookup");available=true;calls.Clear();deps.Load("root");
                Require(queries==1&&remaps==2&&ReferenceEquals(cache["root"],original)&&original[0]=="a.hd"&&calls.Count==4,"source mutates and caches exact manifest array, retains duplicate dependency entries");
                deps.Load("root");Require(queries==1&&remaps==2&&calls.Count==8,"cached dependency list reloads without querying/remapping");
                deps.Load("empty");deps.Load("empty");Require(queries==3&&!cache.ContainsKey("empty"),"empty results are not cached");
            });
            check("outgame-legacy-variant-first-dot-priority-ties-and-fallback",()=>{
                var warnings=new List<string>();string[] candidates={"ui.sd.pack","ui.hd.pack","ui.hd.other","other.hd"};string[] active={"hd","sd"};
                var variants=new OutgameLegacyBundleVariants(()=>candidates,()=>active,warnings.Add);
                Require(variants.Remap("ui.any.suffix")=="ui.hd.pack"&&warnings.Count==0,"source first two dot segments and active-array order choose earliest equal-rank candidate");
                active=new string[0];Require(variants.Remap("ui.request")=="ui.sd.pack"&&warnings.Count==1&&warnings[0].EndsWith("ui.sd.pack"),"no active match warns and uses first same-base variant");
                Require(variants.Remap("absent.request")=="absent.request"&&warnings.Count==1,"no same-base candidate returns unchanged without warning");
                candidates=null;Require(variants.Remap("ui.request")=="ui.request","absent manifest variants return unchanged");
                candidates=new[]{"ui"};bool threw=false;try{variants.Remap("ui.request");}catch(IndexOutOfRangeException){threw=true;}Require(threw,"malformed same-base candidate is not silently ignored");
            });
            check("outgame-legacy-acquisition-cache-inflight-and-manifest-routes",()=>{
                var loaded=new Dictionary<string,OutgameLegacyBundleResult>();var downloading=new Dictionary<string,IOutgameLegacyDownload>();var counts=new Dictionary<string,int>();var dependencies=new Dictionary<string,string[]>();var pending=new List<OutgameLegacyBundleResult>();var registry=new OutgameLegacyBundleRegistry(loaded,new Dictionary<string,string>(),dependencies);var trace=new List<string>();bool manifest=false;int creates=0;
                using(var request=new UnityEngine.Networking.UnityWebRequest()){
                    var acquisition=new OutgameLegacyBundleAcquisition(loaded,downloading,counts,new OutgameLegacyBundleReuse(dependencies,pending,registry.GetLoadedAssetBundle),name=>{trace.Add("resolve:"+name);return name=="absent"?"":"fixture";},()=>manifest,name=>trace.Add("dependencies:"+name),trace.Add,url=>{creates++;trace.Add("create");return request;},req=>{Require(downloading.Count==1,"register request before sending");trace.Add("send");});
                    acquisition.Load("asset",false);Require(creates==0&&trace[0].StartsWith("Please initialize"),"manifest missing logs before acquisition");trace.Clear();manifest=true;
                    acquisition.Load("asset",false);Require(string.Join(",",trace)=="resolve:asset,create,send,dependencies:asset","request sent before dependency loading");
                    trace.Clear();acquisition.Load("asset",false);Require(trace.Count==0&&counts.Count==0&&creates==1,"public duplicate in-flight request bypasses extra reference count and dependency load");
                    acquisition.LoadInternal("asset",false);acquisition.LoadInternal("asset",false);Require(counts["asset"]==2,"explicit internal duplicate accounting accumulates");
                    acquisition.Load("absent",false);Require(string.Join(",",trace)=="resolve:absent,dependencies:absent","missing URL returns false and still allows dependency phase");
                    var cached=new OutgameLegacyBundleResult{BundleName="asset",ReferenceCount=0};loaded["asset"]=cached;pending.Add(cached);acquisition.Load("asset",false);
                    Require(cached.ReferenceCount==1&&pending.Count==0&&creates==1,"loaded wrapper has priority over in-flight entry and cancels pending unload");
                }
            });
            check("outgame-legacy-bundle-reuse-cancels-unload-for-ready-dependencies",()=>{
                var root=new OutgameLegacyBundleResult{BundleName="root",ReferenceCount=1};var child=new OutgameLegacyBundleResult{BundleName="child",ReferenceCount=1};var blocked=new OutgameLegacyBundleResult{BundleName="blocked",ReferenceCount=0};
                var loaded=new Dictionary<string,OutgameLegacyBundleResult>{{"root",root},{"child",child},{"blocked",blocked}};
                var dependencies=new Dictionary<string,string[]>{{"root",new[]{"child","blocked"}},{"blocked",new[]{"missing"}}};var pending=new List<OutgameLegacyBundleResult>();var registry=new OutgameLegacyBundleRegistry(loaded,new Dictionary<string,string>(),dependencies);
                int destroyed=0;var unload=new OutgameLegacyBundleUnload(loaded,dependencies,registry.GetLoadedAssetBundle,pending,()=>false,()=>1,()=>10,()=>false,message=>{},(bundle,all)=>destroyed++);
                unload.Unload("root");Require(root.ReferenceCount==0&&child.ReferenceCount==0&&pending.Count==2,"close queues root and child");
                new OutgameLegacyBundleReuse(dependencies,pending,registry.GetLoadedAssetBundle).Retain(root);
                Require(root.ReferenceCount==1&&child.ReferenceCount==1&&root.UnloadRemaining==2147483648f&&child.UnloadRemaining==2147483648f&&pending.Count==0,"reuse increments references, restores source f32 sentinel and cancels both queued unloads");
                Require(blocked.ReferenceCount==-1,"dependency not ready is skipped on retain without raw-registry fallback");
                unload.Update();Require(destroyed==0&&loaded.Count==3,"subsequent cleanup does not destroy reopened resources");
                dependencies["root"]=new[]{"child","child"};new OutgameLegacyBundleReuse(dependencies,pending,registry.GetLoadedAssetBundle).Retain(root);
                Require(root.ReferenceCount==2&&child.ReferenceCount==3,"source dependency list duplicates increment twice without deduplication");
            });
            check("outgame-legacy-bundle-unload-references-dependencies-and-scaled-delay",()=>{
                var parent=new OutgameLegacyBundleResult{BundleName="parent",ReferenceCount=1};var child=new OutgameLegacyBundleResult{BundleName="child",ReferenceCount=2};
                var loaded=new Dictionary<string,OutgameLegacyBundleResult>{{"parent",parent},{"child",child}};var dependencies=new Dictionary<string,string[]>{{"parent",new[]{"child","missing"}}};var pending=new List<OutgameLegacyBundleResult>();var trace=new List<string>();float dt=0;bool disabled=true;
                var registry=new OutgameLegacyBundleRegistry(loaded,new Dictionary<string,string>(),dependencies);
                var unload=new OutgameLegacyBundleUnload(loaded,dependencies,registry.GetLoadedAssetBundle,pending,()=>disabled,()=>2,()=>dt,()=>true,trace.Add,(bundle,all)=>{Require(all,"original unload destroys loaded objects");trace.Add("unload");});
                unload.Unload("parent");Require(parent.ReferenceCount==1&&child.ReferenceCount==2&&pending.Count==0,"disable gate preserves each reference");disabled=false;
                unload.Unload("parent");Require(parent.ReferenceCount==0&&child.ReferenceCount==1&&pending.Count==1&&parent.UnloadRemaining==2,"missing dependency lookup falls back to loaded root and still releases existing dependency");
                unload.Update();Require(pending.Count==1&&parent.UnloadRemaining==2,"scaled zero delta pauses delayed destruction");
                unload.Unload("parent");Require(parent.ReferenceCount==-1&&child.ReferenceCount==0&&pending.Count==2,"repeated decrement goes negative, queues only exact zero");
                dt=2;unload.Update();Require(loaded.Count==0&&pending.Count==0&&string.Join(",",trace)=="unload,child has been unloaded successfully,unload,parent has been unloaded successfully","reverse pending scan determines native unload and removal order");
            });
            check("outgame-legacy-bundle-unload-exception-and-no-refcount-recheck",()=>{
                var item=new OutgameLegacyBundleResult{BundleName="x",ReferenceCount=1};var loaded=new Dictionary<string,OutgameLegacyBundleResult>{{"x",item}};var dependencies=new Dictionary<string,string[]>();var pending=new List<OutgameLegacyBundleResult>();
                var registry=new OutgameLegacyBundleRegistry(loaded,new Dictionary<string,string>(),dependencies);bool fail=true;int calls=0;
                var unload=new OutgameLegacyBundleUnload(loaded,dependencies,registry.GetLoadedAssetBundle,pending,()=>false,()=>0,()=>0,()=>false,message=>{},(bundle,all)=>{calls++;if(fail)throw new InvalidOperationException("native unload");});
                unload.Unload("x");item.ReferenceCount=9;bool threw=false;try{unload.Update();}catch(InvalidOperationException){threw=true;}
                Require(threw&&pending.Count==1&&loaded.Count==1,"native unload exception precedes dictionary/pending removal");fail=false;unload.Update();
                Require(calls==2&&pending.Count==0&&loaded.Count==0&&item.ReferenceCount==9,"cleanup rebuilds expired list and does not recheck references; cancellation belongs to acquisition path");
            });
            check("outgame-legacy-download-registration-removal-and-operation-order",()=>{
                var downloads=new Dictionary<string,IOutgameLegacyDownload>();var loaded=new Dictionary<string,OutgameLegacyBundleResult>();var errors=new Dictionary<string,string>();var trace=new List<string>();
                var existing=new OutgameLegacyBundleResult{ReferenceCount=7};loaded["existing"]=existing;errors["failed"]="old-error";
                var success=new LegacyDownloadProbe{Done=true,Content=()=>{Require(downloads.Count==4,"removal deferred until enumeration completes");trace.Add("content");return null;}};
                downloads.Add("success",success);downloads.Add("existing",success);downloads.Add("failed",new LegacyDownloadProbe{Done=true,Network=true,Content=()=>throw new Exception("error must not read bundle")});downloads.Add("pending",new LegacyDownloadProbe());
                var update=new OutgameLegacyDownloadUpdate(downloads,loaded,errors,()=>{trace.Add("time");return 12.5f;},trace.Add,()=>{Require(downloads.Count==1&&downloads.ContainsKey("pending")&&loaded.ContainsKey("success"),"registry updates/removals before operation pump");trace.Add("operations");},()=>trace.Add("cleanup"));
                update.Update();Require(loaded["success"].Bundle==null&&loaded["success"].ReferenceCount==1&&loaded["success"].BundleName=="success"&&loaded["success"].LoadedAt==12.5f&&ReferenceEquals(loaded["existing"],existing)&&errors["failed"]=="old-error","source registers even null content wrapper and preserves existing resource/error");
                Require(string.Join(",",trace)=="content,time,content,WebGl加载资源错误fixture-error,operations,cleanup","source download/registration and manager phase ordering");
                downloads.Clear();downloads["new-failure"]=new LegacyDownloadProbe{Done=true,Http=true};
                new OutgameLegacyDownloadUpdate(downloads,loaded,errors,()=>0,message=>{},()=>{},()=>{}).Update();
                Require(errors["new-failure"]=="new-failure is not a valid asset bundle."&&downloads.Count==0,"new failure stores fixed source error, not request.error");
            });
            check("outgame-legacy-download-exception-keeps-pending-removals",()=>{
                var downloads=new Dictionary<string,IOutgameLegacyDownload>{{"first",new LegacyDownloadProbe{Done=true}},{"throw",new LegacyDownloadProbe{Done=true,Content=()=>throw new InvalidOperationException("content")}}};
                var loaded=new Dictionary<string,OutgameLegacyBundleResult>();int later=0;
                var update=new OutgameLegacyDownloadUpdate(downloads,loaded,new Dictionary<string,string>(),()=>0,message=>{},()=>later++,()=>later++);bool threw=false;
                try{update.Update();}catch(InvalidOperationException){threw=true;}
                Require(threw&&loaded.ContainsKey("first")&&downloads.Count==2&&later==0,"source extraction exception propagates with prior registration retained, no removals or later phases");
            });
            check("outgame-legacy-bundle-operation-pump-removal-and-current-list-growth",()=>{
                var operations=new List<IOutgameLegacyManagerOperation>();var visited=new List<string>();bool appended=false;
                OutgameLegacyBundleLookup lookup=(string name,out string error,out int missing)=>{
                    visited.Add(name);error=null;missing=0;
                    if(name=="first"&&!appended){appended=true;operations.Add(new OutgameLegacyBundleOperation("appended",key=>null,(string key,out string err,out int count)=>{visited.Add(key);err=null;count=0;return new OutgameLegacyBundleResult();},message=>{},ex=>throw ex));}
                    return name=="pending"?null:new OutgameLegacyBundleResult();
                };
                foreach(var name in new[]{"first","second","pending"})operations.Add(new OutgameLegacyBundleOperation(name,key=>null,lookup,message=>{},ex=>throw ex));
                var pending=operations[2];var requests=new OutgameLegacyBundleRequests(()=>false,message=>{},name=>name,(name,flag)=>{},name=>null,operations);
                requests.UpdateOperations();Require(string.Join(",",visited)=="first,second,pending,appended"&&operations.Count==1&&ReferenceEquals(operations[0],pending),"source index loop removes consecutive completed entries, retains pending and observes same-pass appended operations");
                visited.Clear();requests.UpdateOperations();Require(string.Join(",",visited)=="pending"&&operations.Count==1,"pending entry is queried again on next manager pump");
            });
            check("outgame-legacy-bundle-registry-dependency-readiness-through-operation",()=>{
                var root=new OutgameLegacyBundleResult();var child=new OutgameLegacyBundleResult();
                var loaded=new Dictionary<string,OutgameLegacyBundleResult>();var errors=new Dictionary<string,string>();var dependencies=new Dictionary<string,string[]>();
                var registry=new OutgameLegacyBundleRegistry(loaded,errors,dependencies);
                Require(registry.GetLoadedAssetBundle("root",out var error,out var missing)==null&&error==null&&missing==0,"absent root is pending without dependency marker");
                loaded["root"]=root;dependencies["root"]=new[]{"child"};
                Require(registry.GetLoadedAssetBundle("root",out error,out missing)==null&&missing==1,"missing dependency writes integer marker1, not a reference count");
                errors["child"]="child failure";Require(registry.GetLoadedAssetBundle("root",out error,out missing)==null&&error==null&&missing==1,"source does not consult dependency error key");
                var operation=new OutgameLegacyBundleOperation("root",name=>null,registry.GetLoadedAssetBundle,message=>{},ex=>throw ex);
                Require(operation.Update()&&operation.MoveNext(),"operation waits for dependency registry readiness");
                loaded["child"]=child;dependencies["child"]=new[]{"grandchild"};
                Require(!operation.Update()&&operation.IsDone()&&ReferenceEquals(operation.GetAssetBundle(),root),"direct dependency wrapper suffices without recursive traversal or native bundle check");
                errors["root"]=null;Require(registry.GetLoadedAssetBundle("root",out error,out missing)==null&&error==null&&missing==0,"error key existence blocks result even with null value");
                errors["root"]="0";Require(registry.GetLoadedAssetBundle("root",out error,out missing)==null&&error=="0"&&missing==0,"sentinel value still suppresses registry result");
                errors.Remove("root");dependencies["root"]=null;bool threw=false;try{registry.GetLoadedAssetBundle("root",out error,out missing);}catch(NullReferenceException){threw=true;}
                Require(threw,"null dependency array is not silently normalized");
            });
            check("outgame-legacy-bundle-operation-source-completion-and-request-capture",()=>{
                int gets=0,lookups=0;string status=null;OutgameLegacyBundleResult result=null;var errors=new List<string>();
                OutgameLegacyBundleLookup lookup=(string name,out string error,out int count)=>{Require(name=="source.bundle","original mapped key");lookups++;error=status;count=37;return result;};
                using(var request=new UnityEngine.Networking.UnityWebRequest()){
                    var operation=new OutgameLegacyBundleOperation("source.bundle",name=>{gets++;return request;},lookup,errors.Add,ex=>throw ex);
                    Require(operation.Current==null&&operation.MoveNext()&&gets==1&&lookups==0,"IEnumerator only polls IsDone; constructor captures request once");
                    Require(operation.Update()&&!operation.IsDone()&&operation.Progress()==request.downloadProgress,"pending lookup retains native download progress");
                    status="0";operation.Update();Require(!operation.IsDone(),"literal zero error sentinel remains pending");
                    status="";operation.Update();Require(operation.IsDone()&&errors.Count==1&&errors[0]=="","empty non-null error is terminal and logged");
                    result=new OutgameLegacyBundleResult();Require(!operation.Update()&&operation.IsDone()&&!operation.MoveNext()&&operation.Progress()==1&&ReferenceEquals(result,operation.GetAssetBundle()),"wrapper completion ignores its native bundle nullness");operation.Reset();Require(operation.IsDone()&&gets==1,"Reset is empty and request is never re-resolved");
                }
                status="failed";result=null;var withoutRequest=new OutgameLegacyBundleOperation("source.bundle",name=>null,lookup,errors.Add,ex=>throw ex);withoutRequest.Update();Require(!withoutRequest.IsDone()&&withoutRequest.Progress()==0,"error without captured request remains pending");
            });
            check("outgame-legacy-bundle-request-registration-order-and-error-log-catch",()=>{
                var trace=new List<string>();var operations=new List<IOutgameLegacyManagerOperation>();
                OutgameLegacyBundleLookup lookup=(string name,out string error,out int count)=>{error="failed";count=0;return null;};
                using(var request=new UnityEngine.Networking.UnityWebRequest()){
                    var requests=new OutgameLegacyBundleRequests(()=>true,trace.Add,name=>{trace.Add("remap:"+name);return "mapped";},(name,manifest)=>{Require(!manifest,"source false manifest flag");trace.Add("load:"+name);},name=>{trace.Add("create:"+name);return new OutgameLegacyBundleOperation(name,key=>request,lookup,message=>throw new InvalidOperationException("logging"),ex=>trace.Add(ex.Message));},operations);
                    var op=(OutgameLegacyBundleOperation)requests.LoadAsync("original");
                    Require(string.Join(",",trace)=="Loading original bundle,remap:original,load:mapped,create:mapped"&&operations.Count==1&&ReferenceEquals(op,operations[0]),"load bundle precedes request capture and operation registration");
                    op.Update();Require(!op.IsDone()&&trace[trace.Count-1]=="logging","original IsDone catches logger exception, logs exception and reports pending");
                }
            });
            check("outgame-legacy-package-operation-failure-source-control-flow",()=>{
                var events=new OutgameMessageDispatcher();var notices=new List<string>();var errors=new List<string>();var warnings=new List<string>();
                events.AddListener("GF_ResLoadError",args=>notices.Add((string)args[0]));
                System.Collections.IEnumerator pending=null;var token=new object();int runs=0,callbacks=0;
                IOutgameLegacyBundleOperation operation=null;
                var package=new OutgameLegacyPackageLoad(new Dictionary<string,AssetBundle>(),name=>operation,routine=>{runs++;if(routine is IOutgameLegacyBundleOperation)return token;pending=routine;return null;},message=>{},errors.Add,events);
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(package.Start,name=>{}),warnings.Add);
                var missing=(OutgameLegacyAssetLoader)scheduler.LoadAsset("missing",res=>{Require(res==null,"missing operation supplies null callback");callbacks++;});scheduler.Update(0,0);
                Require(!pending.MoveNext()&&missing.State==0&&callbacks==1&&scheduler.RequestRemain==100,"null operation errors and exits without completion");
                operation=new LegacyBundleOperationProbe(()=>new OutgameLegacyBundleResult());
                var empty=(OutgameLegacyAssetLoader)scheduler.LoadAsset("empty",res=>{Require(res==null,"null native bundle errors before resource creation");callbacks++;});scheduler.Update(0,0);
                Require(pending.MoveNext()&&ReferenceEquals(pending.Current,token)&&empty.Resource==null,"yield scheduler token for native operation before reading result");
                Require(!pending.MoveNext()&&empty.State==3&&empty.Resource.IsReady&&empty.Resource.Bundle==null&&callbacks==2&&scheduler.RequestRemain==101,"source null-bundle Error is followed by Complete, yielding two accounting increments but one callback");
                operation=new LegacyBundleOperationProbe(()=>throw new InvalidOperationException("fixture-result"));
                OutgameLegacyPrefabResource recovered=null;var broken=(OutgameLegacyAssetLoader)scheduler.LoadAsset("broken",res=>recovered=res);scheduler.Update(0,0);pending.MoveNext();
                Require(!pending.MoveNext()&&notices.Count==1&&notices[0]=="broken.unity3d"&&errors.Count==1&&errors[0].Contains("fixture-result")&&ReferenceEquals(recovered,broken.Resource)&&recovered.IsReady,"caught result exception sends source message/log then still completes");
            });
            check("outgame-legacy-package-firstpack-failure-is-outside-catch",()=>{
                int errors=0,operations=0;System.Collections.IEnumerator pending=null;
                var package=new OutgameLegacyPackageLoad(new Dictionary<string,AssetBundle>{{"packed.unity3d",null}},name=>{operations++;return null;},routine=>{pending=routine;return null;},message=>{},message=>errors++,new OutgameMessageDispatcher());
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(package.Start,name=>{}),message=>{});
                var loader=(OutgameLegacyAssetLoader)scheduler.LoadAsset("packed",null);scheduler.Update(0,0);bool threw=false;
                try{pending.MoveNext();}catch(NullReferenceException){threw=true;}
                Require(threw&&errors==0&&operations==0&&loader.Resource==null&&scheduler.CurrentCount==1,"firstpack bundle.name failure does not fall back to async operation or enter result catch");
            });
            check("outgame-legacy-asset-loader-completion-unload-and-reload",()=>{
                int acquisitions=0;var unloads=new List<string>();var warnings=new List<string>();
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(loader=>acquisitions++,unloads.Add),warnings.Add);
                OutgameLegacyPrefabResource first=null;var args=new object[]{"source"};
                var loader=(OutgameLegacyAssetLoader)scheduler.LoadPrefab("UI/Panel",res=>first=res,args);scheduler.Update(0,0);
                Require(acquisitions==1&&loader.State==1&&first==null,"source start enqueues pending acquisition");loader.Complete();
                Require(loader.State==3&&first.IsReady&&!first.IsUnloaded&&ReferenceEquals(first.Arguments,args)&&ReferenceEquals(scheduler.GetBundleInfo("ui/panel.prefab.unity3d"),first),"completion creates ready registered resource before callbacks");
                Require(scheduler.GetBundleInfo("UI/PANEL.PREFAB.UNITY3D")==null,"resource lookup does not lowercase caller");
                var secondArgs=new object[]{"replacement"};OutgameLegacyPrefabResource cached=null;
                scheduler.LoadPrefab("UI/Panel",res=>cached=res,secondArgs);scheduler.Update(0,0);
                Require(acquisitions==1&&ReferenceEquals(first,cached)&&ReferenceEquals(cached.Arguments,secondArgs),"cached loader completion preserves resource and replaces arguments");
                scheduler.RemoveBundleInfo(first);
                Require(first.IsUnloaded&&first.IsReady&&first.Arguments==null&&first.Bundle==null&&first.OnUnloaded==null&&loader.Resource==null&&loader.State==0&&unloads.Count==1&&scheduler.GetBundleInfo(first.BundleName)==null,"unload resets loader, not readiness, and removes module record");
                scheduler.RemoveBundleInfo(first);Require(unloads.Count==1&&warnings.Count==1,"double module removal warns without repeated unload");
                scheduler.LoadPrefab("UI/Panel",res=>cached=res);scheduler.Update(0,0);loader.Complete();
                Require(acquisitions==2&&!ReferenceEquals(first,cached)&&cached.IsReady,"unloaded cached loader acquires new resource on next request");
                var delayed=new OutgameLegacyPrefabResource(null);int notices=0;delayed.OnUnloaded=res=>{Require(res.Arguments==null&&!res.IsUnloaded,"references cleared before unloaded flag");notices++;};delayed.Arguments=args;
                delayed.Unload(false);Require(delayed.IsUnloaded&&delayed.OnUnloaded!=null&&notices==0,"no-notify unload retains callback");
            });
            check("outgame-legacy-asset-loader-error-and-unload-exceptions",()=>{
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>new OutgameLegacyAssetLoader(loader=>{},name=>throw new InvalidOperationException("unload")),message=>{});
                int called=0;var loader=(OutgameLegacyAssetLoader)scheduler.LoadAsset("bad",res=>{Require(res==null,"failed load callback receives absent resource");called++;});scheduler.Update(0,0);loader.Error();
                Require(called==1&&loader.State==0&&!scheduler.IsCurrentLoading,"base failure callback then source state reset");
                scheduler.LoadAsset("bad",null);scheduler.Update(0,0);loader.Complete();var resource=loader.Resource;bool threw=false;
                try{scheduler.RemoveBundleInfo(resource);}catch(InvalidOperationException){threw=true;}
                Require(threw&&loader.Resource==null&&loader.State==0&&!resource.IsUnloaded&&resource.OnUnloaded!=null&&ReferenceEquals(scheduler.GetBundleInfo(resource.BundleName),resource),"unload exception propagates after loader reset but before unloaded flag/callback clear/dictionary removal");
                var immediate=(OutgameLegacyAssetLoader)scheduler.CreateLoader("immediate",true);immediate.Start(true);
                Require(immediate.State==3&&immediate.Resource.IsReady,"source immediate branch completes directly without enqueue");
            });
            check("outgame-legacy-resource-scheduler-coalescing-and-reentrant-completion",()=>{
                bool modern=false;int guards=0,created=0;var warnings=new List<string>();var seen=new List<string>();
                var scheduler=new OutgameLegacyResourceScheduler(()=>modern,()=>guards++,asset=>{Require(asset,"asset route factory");created++;return new LegacyLoaderProbe();},warnings.Add);
                var a1=new object[]{1};var a2=new object[]{2};OutgameLegacyResLoader follow=null;
                var first=(LegacyLoaderProbe)scheduler.LoadPrefab("UI/Panel",res=>{
                    seen.Add("first");Require(scheduler.CurrentCount==1,"callback occurs before module completion");
                    follow=scheduler.LoadPrefab("UI/Next",r=>seen.Add("next"));
                },a1);
                var again=scheduler.LoadPrefab("UI/Panel",res=>seen.Add("second"),a2);
                Require(ReferenceEquals(first,again)&&created==1&&guards==4&&first.BundleName=="ui/panel.prefab.unity3d"&&ReferenceEquals(first.Arguments,a2)&&scheduler.PendingCount==1,"same canonical path merges callbacks, overwrites arguments and deduplicates start set");
                modern=true;scheduler.Update(0,0);Require(first.Starts==0&&scheduler.PendingCount==1,"modern mode skips legacy pump");modern=false;
                scheduler.Update(0,0);Require(first.Starts==1&&first.Loads==1&&scheduler.RequestRemain==99&&scheduler.CurrentCount==1,"one source request starts with initial capacity100");
                first.Progress=value=>{};first.Finish();
                Require(string.Join(",",seen)=="first,second"&&first.Completed==null&&first.Progress==null&&scheduler.IsCurrentLoading&&scheduler.PendingCount==1&&scheduler.RequestRemain==100,"callbacks clear first, reentrant request keeps pump active");
                scheduler.Update(0,0);((LegacyLoaderProbe)follow).Finish();Require(!scheduler.IsCurrentLoading&&scheduler.RequestRemain==100,"next update consumes new callback request and ends batch");
                var retained=scheduler.LoadPrefab("UI/Panel",res=>seen.Add("cached"));scheduler.Update(0,0);
                Require(ReferenceEquals(retained,first)&&first.Starts==2&&first.Loads==1&&seen[seen.Count-1]=="cached"&&scheduler.RequestRemain==101,"completed cached start invokes completion without consuming a bundle slot; source counter increases");
            });
            check("outgame-legacy-resource-capacity-errors-and-callback-failure",()=>{
                var all=new List<LegacyLoaderProbe>();var warnings=new List<string>();
                var scheduler=new OutgameLegacyResourceScheduler(()=>false,()=>{},asset=>{var p=new LegacyLoaderProbe();all.Add(p);return p;},warnings.Add);
                for(int i=0;i<101;i++)scheduler.LoadAsset("item"+i,null);
                scheduler.Update(0,0);Require(scheduler.CurrentCount==101&&scheduler.QueuedCount==1&&scheduler.RequestRemain==0&&all[100].Loads==0,"all starts registered but only100 bundle loads admitted");
                all[0].Error();Require(warnings.Count==1&&warnings[0]=="Cant load AB : item0.unity3d"&&scheduler.RequestRemain==1,"failure frees a slot using module completion path");
                scheduler.Update(0,0);Require(all[100].Loads==1&&scheduler.QueuedCount==0&&scheduler.RequestRemain==0,"next request admitted after failure");
                var failed=all[1];failed.Completed=res=>throw new InvalidOperationException("source callback failure");failed.Progress=value=>{};bool threw=false;
                try{failed.Finish();}catch(InvalidOperationException){threw=true;}
                Require(threw&&failed.Completed==null&&failed.Progress==null&&scheduler.CurrentCount==100&&scheduler.RequestRemain==0,"callback exception propagates after clearing delegates and before freeing slot");
            });
            check("outgame-legacy-resource-prefab-type-name-and-activation",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/UiRoot/UIRoot");
                var loadedNames=new List<string>();var resource=new OutgameLegacyPrefabResource(null,name=>{loadedNames.Add(name);return prefab;});
                GameObject first=null,second=null;var wrong=new TextAsset("not a prefab");
                try{
                    first=resource.Instantiate("requested-alias",false);second=resource.Instantiate("another-alias",true);
                    Require(first!=second&&first.name==prefab.name&&second.name==prefab.name&&!first.activeSelf&&second.activeSelf,"fresh copies preserve asset name and honor caller activation");
                    Require(string.Join(",",loadedNames)=="requested-alias,another-alias","every call loads the requested asset rather than caching mainObject");
                    Require(new OutgameLegacyPrefabResource(null,name=>wrong).Instantiate("text",true)==null&&new OutgameLegacyPrefabResource(null,name=>null).Instantiate("missing",false)==null,"non-GameObject and missing assets return null");
                    var destroyed=new GameObject("destroyed");UnityEngine.Object.DestroyImmediate(destroyed);
                    Require(new OutgameLegacyPrefabResource(null,name=>destroyed).Instantiate("destroyed",true)==null,"Unity destroyed asset returns null");
                }finally{UnityEngine.Object.DestroyImmediate(first);UnityEngine.Object.DestroyImmediate(second);UnityEngine.Object.DestroyImmediate(wrong);}
            });
            check("outgame-legacy-root-and-page-share-concrete-resource-instantiation",()=>{
                var canvas=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                var rootAsset=Resources.Load<GameObject>("Recovered/UiRoot/UIRoot");var pageAsset=Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI");
                var trace=new List<string>();var rootResource=new OutgameLegacyPrefabResource(null,name=>{Require(name=="UIRoot","original root asset name");trace.Add("root-asset");return rootAsset;});
                var pageResource=new OutgameLegacyPrefabResource(null,name=>{Require(name=="ValentineUI","original page basename");trace.Add("page-asset");return pageAsset;});
                Action<string,Action<OutgameLegacyPrefabResource>> acquire=(path,cb)=>{trace.Add(path);cb(path=="UI/UIRoot"?rootResource:pageResource);};
                var resources=new OutgameUiLegacyRootResources(acquire,(dt,udt)=>{Require(dt==0&&udt==0,"source bootstrap resource pump");trace.Add("update");});
                try{
                    var module=new OutgameUiModuleInitialization(()=>false,(path,cb)=>throw new Exception("modern"),()=>resources,new OutgameUiDisplaySettings(),message=>{},message=>throw new Exception(message),tag=>canvas,obj=>{});
                    module.Initialize();Require(module.IsInitialized&&module.UiRoot.gameObject.activeSelf&&string.Join(",",trace)=="UI/UIRoot,root-asset,update","legacy root uses concrete active instantiation before source resource pump");
                    var nodes=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>module.UiRoot,()=>canvas,message=>throw new Exception(message));
                    System.Collections.IEnumerator waiting=null;int completed=0;GameObject page=null;var logs=new List<string>();
                    var legacy=new OutgameUiLegacyLoader(acquire,nodes.Get,routine=>waiting=routine,logs.Add);
                    var loader=new OutgameUiModernLoader((path,cb)=>throw new Exception("modern"),nodes.Get,routine=>throw new Exception("modern"),legacy.Load);
                    loader.LoadLegacy("FestActUI/ValentineUI","UIWindow",(go,res)=>{Require(ReferenceEquals(res,pageResource),"completion preserves exact resource ownership identity");page=go;completed++;});
                    var child=module.UiRoot.Find("UIWindow").GetChild(0);var rect=child.GetComponent<RectTransform>();
                    Require(!child.gameObject.activeSelf&&child.name==pageAsset.name&&rect.anchorMin==Vector2.zero&&rect.anchorMax==Vector2.one&&rect.offsetMin==Vector2.zero&&rect.offsetMax==Vector2.zero,"legacy instance hidden and stretched under original window layer");
                    Require(logs.Count==1&&logs[0]=="LoadPrefabFestActUI/ValentineUI完成[]"&&completed==0,"source completion log precedes asynchronous UI completion");
                    Require(waiting.MoveNext()&&waiting.Current==null&&waiting.MoveNext()&&waiting.Current==null&&completed==0,"exact two null yields");
                    Require(!waiting.MoveNext()&&completed==1&&page==child.gameObject,"callback after two frames receives same instance");
                    legacy.Load("ValentineUI","UIWindow",null);while(waiting.MoveNext()){};
                    Require(module.UiRoot.Find("UIWindow").childCount==2,"basename without slash and optional completion supported");
                }finally{UnityEngine.Object.DestroyImmediate(canvas);}
            });
            check("outgame-main-module-startup-source-gates-and-order",()=>{
                var order=new List<string>();var modules=new Dictionary<string,StartupModuleProbe>();int ready=0;
                foreach(string name in new[]{"VersionMondule","AssetbundleModule","ResourcesModule","UIModule","LangModule","FsmManager","MineGameLogicModule","TimeModule","EffectModule","ObjectPoolManager","ProcedureManager"})modules.Add(name,new StartupModuleProbe(name,order));
                modules["LangModule"].Initialized=()=>throw new Exception("stale callback must be overwritten");
                var startup=new OutgameCoreModuleStartup(name=>modules[name],message=>order.Add("log:"+message),()=>ready++);
                startup.Begin();Require(string.Join(",",order)=="VersionMondule"&&ready==0,"begin starts only version gate");
                modules["VersionMondule"].Initialized();Require(order[order.Count-1]=="AssetbundleModule"&&ready==0,"version completion starts bundle module");
                modules["AssetbundleModule"].Initialized();Require(order[order.Count-1]=="ResourcesModule","bundle completion starts resource module");
                modules["ResourcesModule"].Initialized();Require(order[order.Count-1]=="UIModule","resource completion gates UI");
                modules["UIModule"].Initialized();
                Require(string.Join(",",order).EndsWith("UIModule,LangModule,FsmManager,MineGameLogicModule,TimeModule,EffectModule,ObjectPoolManager,ProcedureManager")&&ready==0,"secondary modules start in original order without waiting on six null callbacks");
                foreach(string name in new[]{"LangModule","FsmManager","MineGameLogicModule","TimeModule","EffectModule","ObjectPoolManager"})Require(modules[name].Initialized==null,"source clears callback on "+name);
                modules["ProcedureManager"].Initialized();modules["ProcedureManager"].Initialized();Require(ready==2,"procedure completion alone advances; duplicate callback not deduplicated");
            });
            check("outgame-ui-module-initialize-source-root-and-reentry",()=>{
                var canvas=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiBootstrap/SceneUICanvas/UICanvas"));
                try{
                    int completed=0,loads=0;var order=new List<string>();Action<Func<string,bool,GameObject>> ready=null;
                    var module=new OutgameUiModuleInitialization(()=>{order.Add("mode");return true;},(path,cb)=>{Require(path=="UI/UIRoot","root resource path");loads++;ready=cb;},()=>throw new Exception("legacy"),new OutgameUiDisplaySettings(),message=>order.Add(message),message=>throw new Exception(message),tag=>{Require(tag=="GFUICanvas","source tag lookup");order.Add("find");return canvas;},obj=>{Require(obj==canvas,"persist original canvas");order.Add("persist");});
                    module.Initialized=()=>{Require(module.IsInitialized&&module.UiRoot.parent==canvas.transform,"module ready after root attachment");completed++;};module.Initialize();
                    Require(module.UiCamera==canvas.transform.Find("UICamera").GetComponent<Camera>()&&!module.IsInitialized&&string.Join(",",order)=="开始加载UIModule,find,persist,mode","original discovery/persistence/mode order before load completion");
                    ready((name,active)=>{var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));go.name=name;go.SetActive(active);return go;});module.Initialize();Require(completed==2&&loads==1,"initialized reentry invokes current completion without reloading");
                    var calls=new List<string>();var legacy=new UiRootResourcesProbe(calls);var old=new OutgameUiModuleInitialization(()=>false,(path,cb)=>throw new Exception("modern"),()=>{calls.Add("module");return legacy;},new OutgameUiDisplaySettings(),message=>{},message=>{},tag=>canvas,obj=>{});old.Initialize();Require(string.Join(",",calls)=="module,load:UI/UIRoot,module,update:0:0","legacy lookup/load then second lookup/update0,0");
                    var missing=new OutgameUiModuleInitialization(()=>true,(path,cb)=>{},()=>legacy,new OutgameUiDisplaySettings(),message=>{},calls.Add,tag=>null,obj=>calls.Add("persist-null"));calls.Clear();missing.Initialize();Require(string.Join(",",calls)=="UIcanvas未找到,UI相机未找到,persist-null"&&!missing.IsInitialized,"missing canvas logs both errors then continues source load route");
                }finally{UnityEngine.Object.DestroyImmediate(canvas);}
            });
            check("outgame-atlas-cache-firstpack-priority-and-missing-bundles",()=>{
                var firstpack=new Dictionary<string,AssetBundle>();var warnings=new List<string>();int reads=0,debugReads=0;OutgameLoadedAtlasBundle loaded=null;
                var cache=new OutgameAtlasCache(firstpack,key=>{Require(key=="uiatlas/main.spriteatlas.unity3d","bundle manager receives exact key");reads++;return loaded;},()=>{debugReads++;return false;},warnings.Add);
                string key="uiatlas/main.spriteatlas.unity3d";
                Require(cache.Find(key,"Main")==null&&reads==1&&debugReads==0&&warnings[0]=="图集资源包未加载："+key,"missing wrapper logs bundle failure");
                loaded=new OutgameLoadedAtlasBundle();Require(cache.Find(key,"Main")==null&&reads==2&&warnings[1]=="图集资源加载失败：Main","present wrapper with null native bundle reports asset failure");
                firstpack.Add(key,null);bool threw=false;try{cache.Find(key,"Main");}catch(NullReferenceException){threw=true;}
                Require(threw&&reads==2&&debugReads==1&&warnings.Count==2,"firstpack membership takes priority; invalid bundle does not fall back or add manager warnings");
                firstpack.Clear();int reloads=0;var loader=new OutgameAtlasLoader(()=>false,()=>false,h=>{},h=>{},cache.Find,(path,done)=>{Require(path=="UIAtlas/Main.spriteatlas","source reload path after cache miss");reloads++;},message=>{},message=>{},warnings.Add);
                loader.Request("Main",atlas=>throw new Exception("async miss cannot complete yet"));Require(reads==3&&reloads==1,"real cache miss routes request into reload without fabricated result");
            });
            check("outgame-atlas-loader-source-subscription-and-shared-callback",()=>{
                var atlas=new UnityEngine.U2D.SpriteAtlas();
                try{
                    bool modern=true,disabled=false;int subscribed=0,unsubscribed=0,lookups=0;var callbacks=new List<Action<IOutgameAtlasResource>>();var paths=new List<string>();var deliveries=new List<string>();var logs=new List<string>();
                    Action<string,Action<UnityEngine.U2D.SpriteAtlas>> subscribedHandler=null;
                    var loader=new OutgameAtlasLoader(()=>modern,()=>disabled,h=>{subscribed++;subscribedHandler=h;},h=>{Require(h==subscribedHandler,"same delegate target for unsubscribe");unsubscribed++;},(path,tag)=>{lookups++;Require(path==("UIAtlas/"+tag+".spriteatlas.unity3d").ToLower(),"original lowercase bundle key");return tag=="Hit"?atlas:null;},(path,cb)=>{paths.Add(path);callbacks.Add(cb);},logs.Add,logs.Add,logs.Add);
                    loader.OnEnable();Require(subscribed==0,"modern branch does not register legacy atlas handler");modern=false;loader.OnEnable();modern=true;loader.OnDisable();Require(subscribed==1&&unsubscribed==0,"disable uses current mode instead of remembered subscription state");modern=false;loader.OnDisable();Require(unsubscribed==1,"legacy disable unsubscribes same handler");
                    loader.Request("Hit",a=>Require(a==atlas,"cached atlas delivered synchronously"));
                    disabled=true;loader.Request("Skipped",a=>Require(a==null,"disabled loader invokes callback with null"));Require(lookups==1,"disabled branch skips lookup");disabled=false;
                    loader.AutoLoadAtlas=false;loader.Request("Missing",a=>Require(a==null,"auto-load disabled returns null synchronously"));Require(callbacks.Count==0,"no missing load when auto disabled");loader.AutoLoadAtlas=true;
                    loader.Request("First",a=>deliveries.Add("first"));loader.Request("Second",a=>deliveries.Add("second"));
                    Require(paths[0]=="UIAtlas/First.spriteatlas"&&paths[1]=="UIAtlas/Second.spriteatlas","reload path preserves tag case");
                    callbacks[0](new AtlasResourceProbe(atlas));callbacks[1](new AtlasResourceProbe(atlas));Require(string.Join(",",deliveries)=="second,second","source shared slot routes both completions to latest callback");
                    bool failed=false;try{callbacks[0](null);}catch(NullReferenceException){failed=true;}Require(failed&&logs.Contains("图集加载失败"),"null reload warns then continues into original failing access");
                }finally{UnityEngine.Object.DestroyImmediate(atlas);}
            });
            check("outgame-adaptive-bangs-source-pixels-and-restored-baseline",()=>{
                var go=new GameObject("bangs",typeof(RectTransform));var canvas=new GameObject("source canvas",typeof(RectTransform));
                try{
                    var rect=(RectTransform)go.transform;var canvasRect=(RectTransform)canvas.transform;canvasRect.sizeDelta=new Vector2(720,1280);
                    rect.offsetMin=new Vector2(7,11);rect.offsetMax=new Vector2(-13,-17);
                    var settings=new OutgameUiDisplaySettings{IsPortrait=true};var state=new OutgameBangsState();var logs=new List<string>();int canvasReads=0;
                    var adapter=new OutgameBangsAdaptation(rect,settings,state,()=>{canvasReads++;return canvasRect;},logs.Add){IsNeedAdaptiveBangs=true};
                    adapter.Start(1000,2100);
                    Require(canvasReads==0&&settings.IsBangs&&settings.BangsPixel==85&&rect.offsetMax==new Vector2(-13,-102)&&rect.offsetMin==new Vector2(7,11),"long portrait defaults to85 without provider");
                    rect.offsetMax=new Vector2(500,500);adapter.Apply();Require(rect.offsetMax==new Vector2(-13,-102),"baseline restored before repeated adaptation");
                    adapter.IsDoubleEnded=true;adapter.Apply();Require(rect.offsetMin==new Vector2(7,96),"portrait double end adds bottom inset");
                    state.SetBangsPixel(100);adapter.Apply();Require(settings.BangsPixel==61&&canvasReads==1&&rect.offsetMax==new Vector2(-13,-78)&&rect.offsetMin==new Vector2(7,72),"ceil100*1280/2100 based on captured screen height");
                    adapter.IsNeedAdaptiveBangs=false;state.SetBangsPixel(0);adapter.Apply();Require(canvasReads==2&&!settings.IsBangs&&rect.offsetMax==new Vector2(-13,-17)&&rect.offsetMin==new Vector2(7,11),"zero supplied height still queries canvas; disabled adaptation restores base and publishes state");
                    adapter.IsNeedAdaptiveBangs=true;settings.IsPortrait=false;state.SetBangsPixel(101);adapter.Apply();Require(settings.BangsPixel==73&&rect.offsetMin==new Vector2(80,11)&&rect.offsetMax==new Vector2(-86,-17),"landscape uses captured width and both horizontal ends");
                    state.SetBangsPixel(-1);adapter.Start(2000,1000);Require(settings.BangsPixel==0,"ratio exactly2 has no fallback inset");
                    var prefab=Resources.Load<GameObject>("Recovered/UiRoot/UIRoot").GetComponent<OutgameAdaptiveBangs>();Require(prefab!=null&&prefab.IsNeedAdaptiveBangs&&!prefab.IsDoubleEnded&&prefab.enabled,"source serialized flags imported on original root");
                }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(canvas);}
            });
            check("outgame-canvas-adaptation-original-orientation-and-change",()=>{
                var go=new GameObject("source canvas",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.AspectRatioFitter));
                var second=new GameObject("missing fitter",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
                try{
                    var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.referenceResolution=new Vector2(1080,1920);
                    var fitter=go.GetComponent<UnityEngine.UI.AspectRatioFitter>();fitter.enabled=false;
                    var settings=new OutgameUiDisplaySettings();var events=new OutgameMessageDispatcher();var logs=new List<string>();var routines=new List<System.Collections.IEnumerator>();int width=1080,height=1920,changes=0;
                    var adapter=new OutgameCanvasAdaptation(go,settings,()=>width,()=>height,routines.Add,logs.Add,()=>events){WhRatioConst=99,IsHeightControlsWidthFixedWidth=false};
                    events.AddListener("GameAspectChange",args=>{Require(args==null&&Mathf.Abs(adapter.AspectWhRatio-(float)width/height)<.000001f&&logs[logs.Count-1].StartsWith("屏幕分辨率变动，触发适配"),"event after adaptation and log with null arguments: args="+(args==null)+", ratio="+adapter.AspectWhRatio+", expected="+((float)width/height)+", log="+logs[logs.Count-1]);changes++;});
                    adapter.Initialize();
                    Require(settings.IsPortrait&&!settings.HeightControlsWidthFixedWidth&&adapter.WhRatioConst==.5625f&&scaler.matchWidthOrHeight==0&&fitter.aspectMode==UnityEngine.UI.AspectRatioFitter.AspectMode.WidthControlsHeight&&!fitter.enabled,"design overrides serialized ratio; equality selects width; disabled fitter stays disabled");
                    width=2160;height=3840;adapter.Update();Require(changes==0&&routines.Count==1,"resolution change at same ratio ignored");
                    width=1920;height=1080;adapter.Update();Require(changes==1&&routines.Count==2&&scaler.matchWidthOrHeight==1&&fitter.aspectMode==UnityEngine.UI.AspectRatioFitter.AspectMode.HeightControlsWidth,"wide screen for portrait design uses height factor");
                    adapter.Update();Require(changes==1&&routines.Count==2,"unchanged nonterminating binary ratio does not retrigger");
                    width=500;height=2000;adapter.Update();Require(changes==2&&scaler.matchWidthOrHeight==0&&fitter.aspectRatio==.25f,"narrow screen returns to width factor");
                    var diagnostic=routines[0];Require(diagnostic.MoveNext()&&diagnostic.Current is WaitForSeconds,"delayed source diagnostic yields scaled wait");
                    scaler.referenceResolution=new Vector2(720,1280);width=800;height=1000;Require(!diagnostic.MoveNext()&&logs[logs.Count-1].Contains("designWidth=720")&&logs[logs.Count-1].Contains("scaleWidth=800"),"diagnostic reads current values after wait");
                    second.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution=new Vector2(1920,1080);
                    var landscape=new OutgameCanvasAdaptation(second,settings,()=>500,()=>2000,routines.Add,logs.Add,()=>events){WidthControlsHeightFactor=.25f,HeightControlsWidthFactor=.75f};landscape.Initialize();
                    Require(!settings.IsPortrait&&settings.HeightControlsWidthFixedWidth&&second.GetComponent<UnityEngine.UI.AspectRatioFitter>()!=null&&second.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight==.25f,"landscape design always uses width factor and creates missing fitter");
                }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(second);}
            });
            check("outgame-ui-root-source-width-branch-and-completion",()=>{
                var canvas=new GameObject("bootstrap",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));
                try{
                    canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
                    var rect=canvas.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(1000,1280);
                    canvas.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight=1f;
                    int fixedReads=0,completed=0;OutgameUiRootInitialization init=null;
                    init=new OutgameUiRootInitialization(canvas,()=>true,()=>{fixedReads++;return true;},()=>{Require(init.IsInitialized&&init.UiRoot.offsetMin==new Vector2(140,0)&&init.UiRoot.offsetMax==new Vector2(-140,0),"completion observes final geometry and initialized flag");completed++;});
                    init.Loaded((name,active)=>{Require(name=="UIRoot"&&active,"original Instantiate arguments");return UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/UiRoot/UIRoot"));});
                    Require(completed==1&&fixedReads==1&&init.UiRoot.parent==canvas.transform&&init.UiRoot.localScale==Vector3.one&&init.UiRoot.anchorMin==Vector2.zero&&init.UiRoot.anchorMax==Vector2.one,"original root attached and normalized");
                    Require(init.UiRoot.Find("UIWindow").GetComponent<Canvas>().overrideSorting,"serialized layer override becomes active under parent Canvas");
                    UnityEngine.Object.DestroyImmediate(init.UiRoot.gameObject);
                    foreach(float width in new[]{720f,600f}){
                        rect.sizeDelta=new Vector2(width,1280);var narrow=new OutgameUiRootInitialization(canvas,()=>true,()=>true,null);
                        narrow.Loaded((name,active)=>new GameObject(name,typeof(RectTransform)));
                        Require(narrow.UiRoot.offsetMin==Vector2.zero&&narrow.UiRoot.offsetMax==Vector2.zero,"width <=720 not expanded");UnityEngine.Object.DestroyImmediate(narrow.UiRoot.gameObject);
                    }
                    canvas.GetComponent<UnityEngine.UI.CanvasScaler>().matchWidthOrHeight=.999f;rect.sizeDelta=new Vector2(1000,1280);
                    var other=new OutgameUiRootInitialization(canvas,()=>true,()=>throw new Exception("match must equal exactly one"),null);other.Loaded((name,active)=>new GameObject(name,typeof(RectTransform)));
                    Require(other.UiRoot.offsetMin==Vector2.zero&&other.IsInitialized,"non-height-match skips fixed width query");UnityEngine.Object.DestroyImmediate(other.UiRoot.gameObject);
                    var failure=new OutgameUiRootInitialization(canvas,()=>false,()=>false,()=>throw new InvalidOperationException("callback"));
                    bool threw=false;try{failure.Loaded((name,active)=>new GameObject(name,typeof(RectTransform)));}catch(InvalidOperationException){threw=true;}
                    Require(threw&&failure.IsInitialized&&failure.UiRoot.parent==canvas.transform,"completion error does not roll back initialized module");
                }finally{UnityEngine.Object.DestroyImmediate(canvas);}
            });
            check("outgame-ui-node-cache-original-fallback-and-stale-reference",()=>{
                var root=new GameObject("module-root");var canvasObject=new GameObject("canvas",typeof(RectTransform),typeof(Canvas));canvasObject.transform.SetParent(root.transform,false);
                var layer=new GameObject("UIWindow",typeof(RectTransform));layer.transform.SetParent(canvasObject.transform,false);
                try{
                    var nodes=new Dictionary<string,Transform>();var errors=new List<string>();int canvasReads=0,rootReads=0;
                    var cache=new OutgameUiNodes(nodes,()=>{canvasReads++;return canvasObject.GetComponent<RectTransform>();},()=>{rootReads++;return root;},errors.Add);
                    Require(cache.Get("UIWindow")==layer.transform&&cache.Get("UIWindow")==layer.transform&&canvasReads==1&&rootReads==0,"exact canvas-relative lookup cached after first query");
                    Require(cache.Get("Absent")==root.transform&&errors.Count==1&&errors[0]=="未找到UI节点:Absent"&&rootReads==1,"missing node logs and returns module root, not canvas");
                    var late=new GameObject("Absent",typeof(RectTransform));late.transform.SetParent(canvasObject.transform,false);
                    Require(cache.Get("Absent")==root.transform&&errors.Count==1&&canvasReads==2,"fallback retained even when matching node appears later");
                    var old=nodes["UIWindow"];UnityEngine.Object.DestroyImmediate(layer);
                    Require(ReferenceEquals(cache.Get("UIWindow"),old)&&cache.Get("UIWindow")==null&&canvasReads==2,"cache does not validate or recreate destroyed Unity transform");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-ui-load-request-legacy-selection-and-main-handle",()=>{
                var lifetime=new OutgameUiLifetime(null,()=>{});var host=new UiOpenProbe();var trace=new List<string>();bool modern=false,stored=false;var args=new object[]{42};
                var loader=new OutgameUiModernLoader((path,ready)=>throw new Exception("modern branch must remain unselected"),layer=>null,routine=>{},(path,layer,ready)=>{Require(path=="Page"&&layer=="UIMain"&&ReferenceEquals(lifetime.Arguments,args),"legacy original arguments");trace.Add("legacy");ready(null,new object());});
                var request=new OutgameUiLoadRequest(lifetime,new OutgameUiOpenLifecycle(host),()=>trace.Add("show"),()=>{trace.Add("flag");return modern;},()=>{trace.Add("module");return loader;},()=>{modern=true;trace.Add("path");return "Page";},()=>{trace.Add("layer");return "UIMain";},value=>stored=true);
                request.Open(args);Require(!stored&&host.LegacyResource==null&&string.Join(",",trace)=="show,flag,module,path,layer,legacy","flag captured before module/path and legacy never overwrites main handle; null result ignored");
            });
            check("outgame-ui-object-initialization-outlets-and-virtual-order",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));root.SetActive(false);root.transform.localScale=new Vector3(2,3,4);root.transform.localPosition=new Vector3(7,8,9);
                try{
                    var events=new OutgameMessageDispatcher();var lifetime=new OutgameUiLifetime(null,()=>{});var page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);var trace=new List<string>();
                    var main=root.transform.Find("go_Main").gameObject;OutgameUiObjectInitialization init=null;
                    events.AddListener("GF_VisibleUI",args=>{Require(ReferenceEquals(args[0],page)&&init.IsInitialized,"visibility after initialization flag");trace.Add("visible");});
                    init=new OutgameUiObjectInitialization(lifetime,page,obj=>{
                        Require(lifetime.GameObject==root&&lifetime.Transform==root.transform&&lifetime.RectTransform==root.GetComponent<RectTransform>()&&root.transform.localScale==Vector3.one&&!root.activeSelf,"root, normalized transform and rect bound before outlet enumeration");
                        return new[]{new KeyValuePair<string,object>("go_Main",main),new KeyValuePair<string,object>("component",main.transform)};
                    },()=>{Require(init.IsInitialized&&root.activeSelf&&init.Objects["go_Main"]==main&&init.Objects["component"]==null,"outlets cast to GameObject before activation and component initialization");trace.Add("components");},()=>trace.Add("initialize"),()=>trace.Add("skin"),()=>trace.Add("awake"));
                    init.Init(root);Require(string.Join(",",trace)=="components,initialize,visible,skin,awake"&&root.transform.localPosition==new Vector3(7,8,9),"original virtual order; Normalize changes scale only");
                    lifetime.Dispose();Require(init.Objects==null&&page.GameObject==null,"initialization shares dictionary/object disposal ownership");
                    root.SetActive(false);var failedLife=new OutgameUiLifetime(null,()=>{});var failedPage=new OutgameUiPage(()=>failedLife.GameObject);
                    var duplicate=new OutgameUiObjectInitialization(failedLife,failedPage,obj=>new[]{new KeyValuePair<string,object>("same",main),new KeyValuePair<string,object>("same",main)},()=>throw new Exception("unreachable"),()=>{},()=>{},()=>{});
                    bool threw=false;try{duplicate.Init(root);}catch(ArgumentException){threw=true;}
                    Require(threw&&!duplicate.IsInitialized&&!root.activeSelf&&duplicate.Objects.Count==1,"duplicate original outlet throws before activation without invented clear or rollback");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-ui-window-canvas-original-sorting-and-reuse",()=>{
                var canvasRoot=new GameObject("source-ui-root",typeof(RectTransform),typeof(Canvas));
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"),canvasRoot.transform,false);
                try{
                    var trace=new List<string>();int count=2;bool wide=false;
                    var layout=new OutgameUiCanvas(()=>{Require(root.GetComponent<Canvas>().overrideSorting,"overrideSorting set before window query");trace.Add("count");return count;},()=>{trace.Add("flag");return wide;});
                    layout.Add(root,0);Require(trace.Count==0&&root.GetComponent<Canvas>()==null,"non-window layers skip canvas and module queries");
                    layout.Add(root,1);var canvas=root.GetComponent<Canvas>();var raycaster=root.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                    Require(canvas!=null&&raycaster!=null&&layout.WindowIndex==3&&canvas.sortingOrder==16&&canvas.sortingLayerName=="UIWindow"&&string.Join(",",trace)=="count,flag","original standard window index, sorting layer and query order");
                    raycaster.enabled=false;wide=true;count=1;layout.Add(root,1);
                    Require(root.GetComponents<Canvas>().Length==1&&root.GetComponents<UnityEngine.UI.GraphicRaycaster>().Length==1&&root.GetComponent<Canvas>()==canvas&&root.GetComponent<UnityEngine.UI.GraphicRaycaster>()==raycaster&&!raycaster.enabled,"existing root components reused without resetting raycaster enabled state");
                    Require(layout.WindowIndex==2&&canvas.sortingOrder==300,"source flag28 selects100 + count*200 spacing");
                    layout.Add(root,2);Require(layout.WindowIndex==2&&canvas.sortingOrder==300&&trace.Count==4,"non-window retry preserves prior index and sorting");
                }finally{UnityEngine.Object.DestroyImmediate(canvasRoot);}
            });
            check("outgame-ui-load-completion-cache-and-close-races",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));root.name="ValentineUI";
                try{
                    var host=new UiOpenProbe{Cached=true};var events=new OutgameMessageDispatcher();int opened=0;
                    events.AddListener("OpenUI",args=>{Require(ReferenceEquals(args[0],host),"source opened payload identity");opened++;host.Trace.Add("event");});
                    var open=new OutgameUiOpenLifecycle(host,()=>events);var resource=new object();
                    open.LoadedLegacy(null,resource);Require(host.Trace.Count==0&&host.LegacyResource==null,"null result neither binds resource nor initializes");
                    open.LoadedLegacy(root,resource);Require(ReferenceEquals(host.LegacyResource,resource)&&string.Join(",",host.Trace)=="init,canvas,loading,refresh,缓存模式，不播放动画 不抛事件"&&host.Routines.Count==0,"cached page initializes and refreshes but does not animate or emit open event");
                    host.Trace.Clear();host.Cached=false;host.OpenAnimation=3;open.LoadedModern(root);
                    Require(ReferenceEquals(host.LegacyResource,resource)&&string.Join(",",host.Trace)=="init,canvas,loading,refresh,非缓存模式，播放动画 抛事件,start","modern callback preserves legacy field and starts after all initialization");
                    var outer=host.Routines.Dequeue();Require(outer.MoveNext(),"open waits started animation coroutine");var animation=host.Routines.Dequeue();Require(ReferenceEquals(outer.Current,animation)&&animation.MoveNext(),"standard coroutine handed to source scheduler");animation.MoveNext();
                    host.IsDisposed=true;root.name="Renamed";Require(!outer.MoveNext()&&opened==0&&host.Trace[host.Trace.Count-1]=="ValentineUI 播放打开动画的完，就被销毁了","animation captures original object name and skips late open notification after close");
                    host.IsDisposed=false;host.OpenAnimation=5;host.Trace.Clear();outer=open.WaitOpenAnimation(root);Require(outer.MoveNext(),"custom animation also awaited");animation=host.Routines.Dequeue();animation.MoveNext();animation.MoveNext();host.Later=()=>host.IsDisposed=true;
                    Require(!outer.MoveNext()&&opened==1&&string.Join(",",host.Trace)=="start,custom-body,later,抛 OpenUI 事件,event","OpenLater precedes event; no invented second disposal check after callback");
                    var late=UnityEngine.Object.Instantiate(root);open.LoadedModern(late);Require(late==null,"load callback after disposal immediately destroys late native object");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-concrete-page-close-lifetime-and-resource-ownership",()=>{
                foreach(bool modern in new[]{false,true}){
                    var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));root.SetActive(false);
                    try{
                        var trace=new List<string>();var events=new OutgameMessageDispatcher();var frames=new Queue<TaskCompletionSource<bool>>();
                        var animation=new TaskCompletionSource<bool>();OutgameFestUiLifetime lifetime=null;OutgameUiPage page=null;
                        lifetime=new OutgameFestUiLifetime(root,()=>{Require(!lifetime.IsDisposed&&page.GameObject==root,"close-before owns live page");trace.Add("before");},()=>events,
                            obj=>{Require(obj==root&&lifetime.IsDisposed&&!page.Visible&&root.transform.localScale==Vector3.zero,"destroy follows disposed flag and concrete visibility");trace.Add("destroy");});
                        page=new OutgameUiPage(()=>lifetime.GameObject,()=>events);lifetime.ObjectList=new object();lifetime.Arguments=new object();
                        var provider=new OutgameAssetProvider(()=>false,message=>throw new Exception(message)){Status=4};
                        var main=provider.CreateHandle("main",()=>false);var dynamicHandle=provider.CreateHandle("dynamic",()=>false);var custom=provider.CreateHandle("custom",()=>false);
                        Action unload=()=>trace.Add("unload:"+provider.RefCount);
                        var lists=new OutgameUiResourceLists(unload){Dynamic=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,dynamicHandle}},Custom=new Dictionary<UnityEngine.Object,OutgameAssetHandle>{{root,custom}}};
                        events.AddListener("GF_VisibleUI",args=>{Require(ReferenceEquals(args[0],page)&&!(bool)args[1]&&lifetime.IsDisposed,"visibility message uses same page identity");trace.Add("hide");});
                        events.AddListener("CheckUISortAfterStartUIShow",args=>{Require(page.GameObject==null&&lifetime.ObjectList==null&&lifetime.Arguments==null,"activity dispose clears shared references before sorting message");trace.Add("sort");});
                        events.AddListener("CloseUI",args=>{Require(ReferenceEquals(args[0],page)&&lists.Dynamic==null&&lists.Custom==null,"final close identity matches visibility and resources released");trace.Add("close");});
                        var host=new OutgameUiCloseHost(lifetime,page,null,lists,"FestActUI/ValentineUI",()=>modern,()=>{trace.Add("animation");return animation.Task;},unload,
                            path=>{Require(path=="UI/FestActUI/ValentineUI.prefab","original legacy bundle path");trace.Add("bundle");},instruction=>{var pending=new TaskCompletionSource<bool>();frames.Enqueue(pending);trace.Add("frame");return pending.Task;}){CloseAnimation=5,MainHandle=main};
                        var task=new OutgameUiAsyncClose(host,()=>events).CloseAsync();
                        Require(string.Join(",",trace)=="before,animation"&&!lifetime.IsDisposed&&provider.RefCount==3,"animation completes before lifetime and handle changes");
                        animation.SetResult(true);Require(lifetime.IsDisposed&&page.GameObject==root&&page.Visible&&provider.RefCount==3,"first frame retains live object and all resource handles");
                        frames.Dequeue().SetResult(true);Require(page.GameObject==null&&provider.RefCount==3&&!task.IsCompleted,"dispose shares null object while resource ownership survives second wait");
                        frames.Dequeue().SetResult(true);task.GetAwaiter().GetResult();
                        Require(string.Join(",",trace)==(modern?"before,animation,frame,hide,destroy,sort,frame,unload:2,unload:1,unload:0,close":"before,animation,frame,hide,destroy,sort,frame,bundle,unload:2,unload:1,close"),"concrete close order and per-release reference counts");
                        if(!modern)main.Release();Require(provider.RefCount==0,"fixture releases legacy-only unused test main handle");
                    }finally{UnityEngine.Object.DestroyImmediate(root);}
                }
            });
            check("outgame-ui-original-animation-targets-easing-and-clock",()=>{
                var runnerObject=new GameObject("ui-animation-fixture");var root=new GameObject("page",typeof(RectTransform),typeof(CanvasGroup));
                var imageObject=new GameObject("graphic",typeof(RectTransform),typeof(UnityEngine.UI.Image));imageObject.transform.SetParent(root.transform,false);
                var inactiveObject=new GameObject("inactive",typeof(RectTransform),typeof(UnityEngine.UI.Image));inactiveObject.transform.SetParent(root.transform,false);inactiveObject.SetActive(false);
                try{
                    var runner=runnerObject.AddComponent<OutgameUiAnimation>();var group=root.GetComponent<CanvasGroup>();var graphic=imageObject.GetComponent<UnityEngine.UI.Image>();var inactive=inactiveObject.GetComponent<UnityEngine.UI.Image>();
                    Require(runner.ResolveDuration(2,0)==.3f&&runner.ResolveDuration(4,-1)==.2f&&runner.ResolveDuration(3,0)==.5f&&runner.ResolveDuration(2,.9f)==.9f,"source animation default durations and explicit override");
                    group.alpha=.4f;graphic.color=new Color(.2f,.3f,.4f,.6f);
                    var fade=runner.ObjectAnim(root,2,1);Require(fade.MoveNext()&&fade.Current is WaitForSeconds,"fade schedules scaled duration wait");
                    runner.Advance(0);Require(group.alpha==.4f,"paused tween does not initialize constant getter");
                    runner.Advance(.5f);Require(Mathf.Abs(group.alpha-.25f)<.00001f&&graphic.color.a==.6f,"group fade starts from source constant one and leaves child graphics untouched");
                    runner.Advance(.5f);Require(group.alpha==0&&!fade.MoveNext(),"fade endpoint and one wait");
                    UnityEngine.Object.DestroyImmediate(group);
                    inactive.color=new Color(1,1,1,.8f);var fadeIn=runner.ObjectAnim(root,1,1);Require(fadeIn.MoveNext()&&graphic.color.a==0&&inactive.color.a==.8f,"Graphic.From initializes immediately and excludes inactive descendants");
                    runner.Advance(.5f);Require(Mathf.Abs(graphic.color.a-.45f)<.00001f&&graphic.color.r==.2f,"fade-in returns to actual graphic alpha using OutQuad");runner.Advance(.5f);
                    root.transform.localScale=new Vector3(2,3,4);var scaleIn=runner.ObjectAnim(root,3,1);Require(scaleIn.MoveNext()&&root.transform.localScale==Vector3.zero,"source From scale starts immediately at zero");
                    runner.Advance(.5f);Require(Vector3.Distance(root.transform.localScale,new Vector3(2,3,4)*1.0876975f)<.00001f,"OutBack source overshoot preserves all original scale axes");runner.Advance(.5f);
                    Require(root.transform.localScale==new Vector3(2,3,4),"scale-in restores captured nonuniform target");
                    var scaleOut=runner.ObjectAnim(root,4,1);Require(scaleOut.MoveNext(),"scale-out duration wait");root.transform.localScale=new Vector3(3,4,5);
                    runner.Advance(.5f);Require(Vector3.Distance(root.transform.localScale,new Vector3(3,4,5)*1.0876975f)<.00001f,"scale-out captures start at first update and uses InBack");runner.Advance(.5f);Require(root.transform.localScale==Vector3.zero,"scale-out endpoint");
                    Require(!runner.ObjectAnim(root,0,1).MoveNext()&&!runner.ObjectAnim(null,2,1).MoveNext()&&!runner.ObjectAnim(root,5,1).MoveNext(),"none/null/custom handled without invented animation wait");
                }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(runnerObject);}
            });
            check("outgame-ui-close-registry-removal-and-reentrant-owner",()=>{
                var trace=new List<string>();var pages=new Dictionary<string,object>();var first=new object();var second=new object();
                OutgameUiCloseRegistry<object> registry=null;
                registry=new OutgameUiCloseRegistry<object>(pages,page=>{
                    Require(!pages.ContainsKey("FestActUI"),"registration removed before close starts");
                    trace.Add(ReferenceEquals(page,first)?"first":"second");registry.CloseForName("FestActUI");
                });
                pages.Add("FestActUI",first);registry.CloseForName("FestActUI");registry.CloseForName("absent");
                Require(string.Join(",",trace)=="first"&&pages.Count==0,"reentrant and missing closes do not restart page close");
                OutgameUiCloseRegistry<object>.CloseSelf(()=>{trace.Add("action");pages.Add("FestActUI",second);},
                    ()=>{trace.Add("module");return registry;},()=>{trace.Add("name");return "FestActUI";});
                Require(string.Join(",",trace)=="first,action,module,name,second","close action before module/name lookup targets current registered page");
                pages.Add("FestActUI",first);var failed=new OutgameUiCloseRegistry<object>(pages,page=>throw new InvalidOperationException("fixture-close"));
                bool threw=false;try{failed.CloseForName("FestActUI");}catch(InvalidOperationException){threw=true;}
                Require(threw&&pages.Count==0,"synchronous close failure does not restore removed registration");
                var host=new UiAsyncCloseProbe{CloseAnimation=5,UsesNewResources=true};var messages=new OutgameMessageDispatcher();
                var asyncClose=new OutgameUiAsyncClose(host,()=>messages);var pendingPages=new Dictionary<string,OutgameUiAsyncClose>{{"FestActUI",asyncClose}};Task pending=null;
                var actual=new OutgameUiCloseRegistry<OutgameUiAsyncClose>(pendingPages,page=>pending=page.CloseAsync());
                actual.CloseForName("FestActUI");actual.CloseForName("FestActUI");
                Require(pendingPages.Count==0&&!pending.IsCompleted&&string.Join(",",host.Trace)=="before,custom","registered close starts concrete state machine once before animation completes");
                host.Animation.SetResult(true);host.Frames.Dequeue().SetResult(true);host.Frames.Dequeue().SetResult(true);pending.GetAwaiter().GetResult();
                Require(string.Join(",",host.Trace)=="before,custom,disposed,frame,hide,destroy,cleanup,frame,release,unused,dynamic,custom-res","registered close preserves both frame and resource sequences");
            });
            check("outgame-ui-async-close-frame-and-resource-order",()=>{
                foreach(bool modern in new[]{false,true}){
                    var host=new UiAsyncCloseProbe{CloseAnimation=modern?5:2,UsesNewResources=modern};var messages=new OutgameMessageDispatcher();
                    messages.AddListener("CloseUI",args=>{Require(args.Length==1&&ReferenceEquals(args[0],host),"source close payload");host.Trace.Add("message");});
                    var task=new OutgameUiAsyncClose(host,()=>messages).CloseAsync();Require(string.Join(",",host.Trace)==(modern?"before,custom":"before,standard")&&!task.IsCompleted,"close waits animation before disposed flag");
                    host.Animation.SetResult(true);Require(host.Trace[host.Trace.Count-1]=="frame"&&host.Trace.Contains("disposed")&&!host.Trace.Contains("hide"),"first frame after disposed before hide/destroy");
                    host.Frames.Dequeue().SetResult(true);Require(string.Join(",",host.Trace).EndsWith("hide,destroy,cleanup,frame")&&!task.IsCompleted,"hide/destroy/dispose then second distinct frame");
                    host.Frames.Dequeue().SetResult(true);task.GetAwaiter().GetResult();Require(string.Join(",",host.Trace).EndsWith(modern?"release,unused,dynamic,custom-res,message":"bundle,dynamic,custom-res,message"),"source resource branch and final event order");
                }
                var failed=new UiAsyncCloseProbe{CloseAnimation=5};var pending=new OutgameUiAsyncClose(failed).CloseAsync();failed.Animation.SetException(new InvalidOperationException("fixture-animation"));
                Require(pending.IsFaulted&&string.Join(",",failed.Trace)=="before,custom","animation error does not invent finally cleanup or close notification");
            });
            check("outgame-fest-immediate-close-and-dispose-order",()=>{
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));page.SetActive(false);
                try{
                    var events=new OutgameMessageDispatcher();var trace=new List<string>();OutgameFestUiLifetime lifetime=null;var rect=page.GetComponent<RectTransform>();
                    lifetime=new OutgameFestUiLifetime(page,()=>{Require(!lifetime.IsDisposed&&lifetime.GameObject==page,"close-before sees live page");trace.Add("before");},()=>events,
                        root=>{Require(root==page&&lifetime.IsDisposed&&lifetime.Transform==page.transform,"destroy requested after flag but before reference cleanup");trace.Add("destroy");});
                    lifetime.ObjectList=new object();lifetime.Arguments=new object();
                    events.AddListener("CheckUISortAfterStartUIShow",args=>{Require(args==null&&lifetime.IsDisposed&&lifetime.GameObject==null&&lifetime.Transform==null&&lifetime.ObjectList==null&&lifetime.Arguments==null,"sorting notification after all source cleanup");trace.Add("sort");});
                    lifetime.CloseUINow();Require(string.Join(",",trace)=="before,destroy,sort"&&ReferenceEquals(lifetime.RectTransform,rect),"source immediate-close order and retained rectTransform field");
                    trace.Clear();lifetime.Dispose();Require(string.Join(",",trace)=="sort","source repeated Dispose repeats notification without invented guard");
                }finally{UnityEngine.Object.DestroyImmediate(page);}
            });
            check("outgame-fest-page-awake-dates-close-and-menu-route",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                var clock=new OutgameServerClock(()=>1,()=>1,()=>true);var levels=new OutgameLevelProgression(new OutgameProfile{levelID=31},new OutgameLevelProgressionValidation.Effects());
                string config=BattleView.ReadText("Data/Outgame/SummerRewardConfig");var manager=new OutgameFestActManager(config,storage,h,x=>{},clock,levels,(a,b,c,d,e)=>true,()=>"activity",(a,b,c,d)=>{});manager.OnInit();
                var state=new OutgameFestActivityState(()=>manager.Data,()=>true,clock,levels,(a,b,c,d,e)=>{});
                new OutgameActivityConfig(BattleView.ReadText("Data/Outgame/PubActivityConfig"),x=>{}).Apply(103002,state,new OutgameActivityCountdown(()=>state.Status,clock.GetNowTimestampLong));
                foreach(int status in new[]{3,4}){
                    manager.Data.status=status;var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));page.SetActive(false);
                    try{
                        page.transform.Find("go_Main/Area1/Main/ClaimDouble").GetComponent<OutgameVideoButton>().Bind(new VideoButtonHostProbe{Data=new OutgameVideoButtonData{showType=1}});
                        var trace=new List<string>();var selection=new OutgameFestRewardSelection(()=>config);
                        var art=new OutgameFestRewardArt(selection,()=>BattleView.ReadText("Data/Outgame/SkinConfig"),()=>BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/FestRewardArt"));
                        var items=new OutgameFestRewardItems(page.transform,manager,()=>config,()=>BattleView.ReadText("Data/Outgame/GameItemConfig"),key=>"第{0}天",(image,name,atlas)=>{trace.Add("image");art.SetImportedSprite(image,name,atlas);},art.GetAwardImage);
                        var visibility=new OutgameFestRewardVisibility(page.transform,manager,key=>"挑战");
                        var lifecycle=new OutgameFestPageLifecycle(page.transform,state,manager,levels,()=>config,items,visibility,null,(g,id)=>{Require(g==1&&id==2001,"close voice");trace.Add("voice");},()=>trace.Add("close"),()=>{trace.Add("find");return ()=>trace.Add("skin");},
                            (name,level,a,b)=>{Require(name==state.ActivityName&&level=="31"&&a==null&&b==null,"source report fields");trace.Add("enter");});
                        lifecycle.Awake();Require(trace.Count==6&&trace[0]=="enter","entry report precedes five concrete image updates");
                        Require(page.transform.Find("go_Notice").gameObject.activeSelf==(status<4)&&page.transform.Find("go_Main").gameObject.activeSelf==(status>3),"source notice/main status gates");
                        string dates=page.transform.Find("go_Main/Title/Time").GetComponent<UnityEngine.UI.Text>().text;Require(dates=="2022.02.13-2022.02.27"&&page.transform.Find("go_Notice/GameObject/txtNoticeTime").GetComponent<UnityEngine.UI.Text>().text==dates,"unaltered original historical date range copied to notice");
                        trace.Clear();page.transform.Find("go_Notice/btn_Read").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Require(string.Join(",",trace)=="close","notice read closes without added voice or main-page switch");
                        trace.Clear();page.transform.Find("go_Main/Area2/MoreSkin").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Require(string.Join(",",trace)=="voice,close,find,skin","more skins closes then finds menu then opens skin tab");
                    }finally{UnityEngine.Object.DestroyImmediate(page);}
                }
            });
            check("outgame-fest-native-daily-claim-and-source-visibility",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                long now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,9,30,12,0,0));var clock=new OutgameServerClock(()=>now,()=>1,()=>true);clock.OnInit();
                var levels=new OutgameLevelProgression(new OutgameProfile{levelID=31},new OutgameLevelProgressionValidation.Effects());var trace=new List<string>();
                string config=BattleView.ReadText("Data/Outgame/SummerRewardConfig");OutgameFestActManager manager=null;
                manager=new OutgameFestActManager(config,storage,h,x=>{},clock,levels,(id,count,a,b,c)=>{trace.Add("grant:"+id+":"+count);manager.OnSave();return true;},()=>"activity",(a,b,c,d)=>trace.Add("report"));manager.OnInit();
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));page.SetActive(false);
                try{
                    var normal=page.transform.Find("go_Main/Area1/Main/Claim").GetComponent<UnityEngine.UI.Button>();var video=page.transform.Find("go_Main/Area1/Main/ClaimDouble").GetComponent<OutgameVideoButton>();
                    video.Bind(new VideoButtonHostProbe{Data=new OutgameVideoButtonData{showType=1}});
                    var visibility=new OutgameFestRewardVisibility(page.transform,manager,key=>"挑战");
                    int iconCalls=0;
                    var art=new OutgameFestRewardArt(new OutgameFestRewardSelection(()=>config),()=>BattleView.ReadText("Data/Outgame/SkinConfig"),()=>BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/FestRewardArt"));
                    Require(art.GetAwardImage(4)==("qibing10","SkinAndToolIcon")&&art.GetAwardImage(5)==("tubiao06","SceneSkin")&&art.GetAwardImage(1)==("",""),"original award config icon lookup");
                    var rewardItems=new OutgameFestRewardItems(page.transform,manager,()=>config,()=>BattleView.ReadText("Data/Outgame/GameItemConfig"),key=>{Require(key=="ValentineUI.DayIndex","source day-index language key");return "第{0}天";},
                        (image,icon,atlas)=>{Require(image!=null&&!string.IsNullOrEmpty(icon),"original image outlet and config icon");Require(atlas=="PublicIcon"||atlas=="SkinAndToolIcon"||atlas=="SceneSkin","original atlas identity");iconCalls++;art.SetImportedSprite(image,icon,atlas);},art.GetAwardImage);
                    new OutgameFestDailyRewardBinding(page.transform,manager,()=>config,()=>{trace.Add("state");visibility.Refresh();},()=>{trace.Add("items");rewardItems.Refresh();});
                    rewardItems.Refresh();var daily=page.transform.Find("go_Main/Area1/Main/ItemGifts");
                    Require(iconCalls==5&&daily.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled&&!daily.GetChild(1).GetComponent<UnityEngine.UI.Image>().enabled,"initial day highlight and three item plus two skin image requests");
                    Require(daily.GetChild(2).GetChild(2).GetComponent<UnityEngine.UI.Text>().text=="第3天"&&daily.GetChild(1).GetChild(3).GetChild(0).GetComponent<UnityEngine.UI.Text>().text=="X2","original day and amount format");
                    Require(daily.GetChild(0).GetChild(1).GetComponent<UnityEngine.UI.Image>().sprite==Resources.Load<Sprite>("Recovered/Outgame/Sprites/CAB-68a3634b9a99c5a2a10a36b772e4e945_8460458951675254696"),"actual item sprite matches source identity");
                    Require(page.transform.Find("go_Main/Area2/Main/SkinGifts").GetChild(1).GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite==Resources.Load<Sprite>("Recovered/Outgame/Sprites/CAB-96e7bcb9149957f0c16b74c6f96f93a3_-8614821950332085612"),"actual scene sprite matches source identity");
                    visibility.Refresh();Require(normal.gameObject.activeSelf&&video.gameObject.activeSelf&&!page.transform.Find("go_Main/Area1/Main/Claimed").gameObject.activeSelf,"initial original claim controls eligible");
                    video.VideoCallBack("fixture-failure",false);Require(trace.Count==0&&manager.Data.rewardId==1,"failed ad does not grant or refresh");
                    normal.onClick.Invoke();Require(string.Join(",",trace)=="grant:2001:2,report,state,items"&&manager.Data.rewardId==2,"normal claim then manager report then state/items refresh");
                    backend.Finish(true);Require(OutgameFestActivityData.Read(backend.Disk["Proj_hdzdFestActManager"]).rewardId==1,"grant save precedes reward advance; UI adds no final save");
                    Require(!daily.GetChild(0).GetComponent<UnityEngine.UI.Image>().enabled&&daily.GetChild(1).GetComponent<UnityEngine.UI.Image>().enabled&&daily.GetChild(0).GetChild(4).gameObject.activeSelf&&daily.GetChild(0).GetChild(5).gameObject.activeSelf&&!daily.GetChild(1).GetChild(4).gameObject.activeSelf,"claim refresh advances highlight and completed overlays on actual page");
                    Require(!normal.gameObject.activeSelf&&!video.gameObject.activeSelf&&page.transform.Find("go_Main/Area1/Main/Claimed").gameObject.activeSelf,"claimed status shown after first grant");
                    trace.Clear();normal.onClick.Invoke();video.VideoCallBack("fixture-success",true);Require(trace.Count==0,"same-day normal/ad callbacks recheck eligibility");
                    now=manager.Data.nextGetAwardTime+1;clock.Updata(0,1.1f);video.VideoCallBack("fixture-success",true);
                    Require(string.Join(",",trace)=="grant:2002:4,report,state,items"&&manager.Data.rewardId==3,"late successful ad selects current reward and doubles original count");
                    var gifts=page.transform.Find("go_Main/Area2/Main/SkinGifts");manager.Data.limetSkinStatus=2;visibility.Refresh();
                    Require(gifts.GetChild(1).GetChild(2).gameObject.activeSelf&&!gifts.GetChild(0).GetChild(3).gameObject.activeSelf&&gifts.GetChild(1).GetChild(3).gameObject.activeSelf,"source second completion marks second received but hides first claim");
                    Require(gifts.GetChild(0).GetChild(4).gameObject.activeSelf&&gifts.GetChild(0).GetChild(5).gameObject.activeSelf&&!gifts.GetChild(1).GetChild(4).gameObject.activeSelf,"source first-row overlay asymmetry preserved");
                }finally{UnityEngine.Object.DestroyImmediate(page);clock.OnDispose();}
            });
            check("outgame-fest-selection-special-level-start-to-win",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                var clock=new OutgameServerClock(()=>1,()=>1,()=>true);var levels=new OutgameLevelProgression(new OutgameProfile{levelID=31},new OutgameLevelProgressionValidation.Effects());
                string config=BattleView.ReadText("Data/Outgame/SummerRewardConfig");var manager=new OutgameFestActManager(config,storage,h,x=>{},clock,levels,(a,b,c,d,e)=>true,()=>"activity",(a,b,c,d)=>{});manager.OnInit();
                var selection=new OutgameFestRewardSelection(()=>config);Require(selection.SelectedAward==-1&&selection.GetAwardId(4)==310&&selection.GetAwardId(5)==6&&selection.GetAwardId(3)==-1,"original selection default and type lookup");
                var messages=new OutgameMessageDispatcher();var trace=new List<string>();
                var combat=new OutgameFestCombatBinding(manager,()=>selection.SelectedAward,id=>id==310?4:5,()=>config,()=>trace.Add("listen"),()=>{},()=>{},(g,id)=>trace.Add("rewardVoice"),
                    (id,notify)=>trace.Add("soldier:"+id),(id,notify)=>trace.Add("scene:"+id),(a,b,c,d,e)=>trace.Add("report"),()=>messages);
                var start=new OutgameFestSpecialLevelStart(combat,selection,levels,(g,id)=>{Require(g==1&&id==2001,"start voice");trace.Add("voice");},
                    (camera,duration)=>{Require(camera==0&&duration==0&&levels.SpecialState==1,"camera follows special selection");trace.Add("camera:"+levels.CurrentLevel);},
                    state=>{Require(state==3,"direct level state3");trace.Add("level");},()=>trace.Add("close"));
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));
                try{
                var buttons=new OutgameFestSpecialLevelButtons(page.transform,manager,start,key=>{Require(key=="ValentineUI.Play","original play label key");return "挑战";});buttons.OpenLater();
                var gifts=page.transform.Find("go_Main/Area2/Main/SkinGifts");Require(gifts.childCount==2&&gifts.GetChild(0).GetChild(3).GetChild(0).GetComponent<UnityEngine.UI.Text>().text=="挑战","original prefab indices bound to recovered action");
                gifts.GetChild(0).GetChild(3).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Require(string.Join(",",trace)=="listen,voice,camera:99998,level,close"&&selection.SelectedAward==310&&levels.RealCurrentLevel==31,"source config chooses first special level without changing held normal progress");
                messages.SendMessage("WarWin");Require(trace[trace.Count-1]=="soldier:310"&&manager.Data.limetSkinStatus==1,"first selection drives actual combat binding");
                trace.Clear();gifts.GetChild(1).GetChild(3).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();messages.SendMessage("WarWin");Require(string.Join(",",trace)=="listen,voice,camera:99999,level,close,rewardVoice,scene:6,report"&&manager.Data.limetSkinStatus==3,"second source special level drives scene reward order");
                Require(backend.Calls.Count==0&&levels.RealCurrentLevel==31,"selection/start/win do not invent save or normal progression changes");
                var completedPage=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/FestActivity/ValentineUI"));
                try{trace.Clear();new OutgameFestSpecialLevelButtons(completedPage.transform,manager,start,key=>throw new Exception("completed rows must skip label and listener")).OpenLater();
                    completedPage.transform.Find("go_Main/Area2/Main/SkinGifts").GetChild(0).GetChild(3).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Require(trace.Count==0,"completed reward does not bind start callback on fresh page");}
                finally{UnityEngine.Object.DestroyImmediate(completedPage);}
                }finally{UnityEngine.Object.DestroyImmediate(page);}

            });
            check("outgame-fest-combat-win-return-and-dispose-lifecycle",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                var clock=new OutgameServerClock(()=>1,()=>1,()=>true);var levels=new OutgameLevelProgression(new OutgameProfile(),new OutgameLevelProgressionValidation.Effects());
                string config=BattleView.ReadText("Data/Outgame/SummerRewardConfig");var manager=new OutgameFestActManager(config,storage,h,x=>{},clock,levels,(a,b,c,d,e)=>true,()=>"activity",(a,b,c,d)=>{});manager.OnInit();
                var messages=new OutgameMessageDispatcher();var trace=new List<string>();int selected=123,kind=4,claims=0;
                var binding=new OutgameFestCombatBinding(manager,()=>selected,id=>{Require(id==123,"selected controller award classified");return kind;},()=>config,
                    ()=>claims++,()=>trace.Add("clear"),()=>trace.Add("statistics"),(group,id)=>{Require(group==1&&id==2017,"scene reward voice");trace.Add("voice");},
                    (id,notify)=>{Require(id==310&&notify&&manager.Data.limetSkinStatus==1,"soldier flag before unlock");trace.Add("soldier");messages.SendMessage("WarWin");},
                    (id,notify)=>{Require(id==6&&notify&&manager.Data.limetSkinStatus==3,"scene flag before unlock");trace.Add("scene");},
                    (id,a,b,c,reason)=>{Require(id==1001006&&a==3&&b==1&&c==1&&reason=="Activity","original reporting arguments and Global.Activity literal");trace.Add("report");},()=>messages);
                binding.OnInit();binding.ListenCombatWar(true);binding.ListenCombatWar(true);messages.SendMessage("WarWin");messages.SendMessage("WarWin");
                Require(claims==2&&string.Join(",",trace)=="soldier","repeated enabling replaces own callback; removed before reentrant award");
                kind=5;trace.Clear();binding.ListenCombatWar(true);messages.SendMessage("GamePlayState",new object[]{11});messages.SendMessage("WarWin");Require(trace.Count==0,"return-home state removes combat callback");
                binding.ListenCombatWar(true);messages.SendMessage("GamePlayState",new object[]{8});messages.SendMessage("WarWin",new object[]{false});
                Require(string.Join(",",trace)=="voice,scene,report"&&backend.Calls.Count==0,"payload ignored; scene order exact; no invented save");
                kind=0;trace.Clear();binding.ListenCombatWar(true);messages.SendMessage("WarWin");kind=5;messages.SendMessage("WarWin");Require(trace.Count==0,"unknown goods type still consumes subscription");
                binding.ListenCombatWar(true);binding.OnDispose();messages.SendMessage("GamePlayState",new object[]{11});messages.SendMessage("WarWin");
                Require(string.Join(",",trace)=="statistics,clear,voice,scene,report","source dispose removes state/statistic listeners but leaves pending WarWin callback");
            });
            check("outgame-fest-pool-and-late-init-shared-state",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                long now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,1));var clock=new OutgameServerClock(()=>now,()=>1,()=>true);clock.OnInit();
                var levels=new OutgameLevelProgression(new OutgameProfile{levelID=31},new OutgameLevelProgressionValidation.Effects());
                var manager=new OutgameFestActManager(BattleView.ReadText("Data/Outgame/SummerRewardConfig"),storage,h,x=>{},clock,levels,(a,b,c,d,e)=>true,()=>"activity",(a,b,c,d)=>{});
                var entries=new List<OutgameManagerRegistration>();foreach(var entry in OutgameManagerRegistrationCatalog.Read(Resources.Load<TextAsset>("Data/OutgameManagerRegistration").text,id=>{Require(id==4505,"source festival type identity");return manager;}))if(entry.SourceTypeIndex==4505)entries.Add(entry);
                var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>throw new Exception(x));pool.OnInit(true,"Proj_hdzd",entries);
                Require(ReferenceEquals(pool.Managers[4505],manager)&&manager.ParticipatesInSync&&!manager.CompressData,"original roster flags initialize concrete manager");
                bool enabled=true;var configs=new OutgameActivityConfig(BattleView.ReadText("Data/Outgame/PubActivityConfig"),x=>throw new Exception(x));
                var state=new OutgameFestActivityState(()=>manager.Data,()=>enabled,clock,levels,(a,b,c,d,e)=>{});var timer=new OutgameActivityCountdown(()=>state.Status,clock.GetNowTimestampLong);
                var trace=new List<string>();Action callback=null;
                var init=new OutgameFestActivityInitialization(()=>enabled,configs,manager,state,timer,()=>trace.Add("icon:"+state.Status),(id,action)=>{Require(id==10000,"time statistic subscription");trace.Add("subscribe");callback+=action;},(id,action)=>{trace.Add("unsubscribe");callback-=action;});
                init.OnLateInit();Require(state.Status==5&&trace.Count==1&&trace[0]=="icon:5"&&callback==null&&!init.ActiveUpdate,"historical source dates initialize completed without subscription");
                var config=configs.Get(103002);config.noticeParams[0]="20260930000000";config.launchParams[0]="20261002000000";config.overParams[0]="20261004000000";trace.Clear();
                init.OnLateInit();Require(state.Status==3&&string.Join(",",trace)=="icon:3,subscribe"&&init.ActiveUpdate&&timer.StartTimeStamp==state.StartTimeStamp,"explicit future-cycle fixture composes config/storage/state/countdown then subscribes");
                now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,4));clock.Updata(0,1.1f);trace.Clear();callback();Require(state.Status==5&&string.Join(",",trace)=="icon:3","callback refreshes icons before recomputing expired state");
                trace.Clear();callback();Require(callback==null&&string.Join(",",trace)=="icon:5,unsubscribe","next callback observes complete and removes listener");
                pool.SaveData();backend.Finish(true);manager.UpdateDataCallBack(storage.ReadLocalData());Require(manager.Data.status==5,"pool saves composed state through original manager storage");
                enabled=false;trace.Clear();init.OnLateInit();Require(trace.Count==0,"disabled feature performs no initialization");clock.OnDispose();
            });
            check("outgame-fest-manager-reward-order-storage-restart",()=>{
                var h=new DataStorageHost();var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var sdk=new OutgameSdkStringStorage(backend,x=>{});var storage=new OutgameDataManagerStorage(()=>"FestActManager",h,sdk,versions);
                long now=OutgameItemTimestamp.FromDateTime(new DateTime(2026,9,30,15,0,0));var clock=new OutgameServerClock(()=>now,()=>1,()=>true);clock.OnInit();
                var levels=new OutgameLevelProgression(new OutgameProfile{levelID=31},new OutgameLevelProgressionValidation.Effects());var trace=new List<string>();OutgameFestActManager manager=null;
                string config=BattleView.ReadText("Data/Outgame/SummerRewardConfig");
                manager=new OutgameFestActManager(config,storage,h,x=>throw new Exception("unexpected download"),clock,levels,(id,count,top,reason,notify)=>{Require(id==2001&&count==2&&top&&reason==""&&notify,"source tool flags");trace.Add("grant:"+manager.Data.rewardId);manager.OnSave();return false;},()=>"activity",(name,level,reward,extra)=>{Require(manager.Data.nextGetAwardTime>now,"next time set before report");trace.Add("report:"+reward);});
                manager.OnInit();Require(manager.SignRewardCount==3&&manager.LimitSkinCount==2&&manager.CanGetTodayReward(),"original reward row lengths and default data");
                manager.GetTodayReward(2001,2);backend.Finish(true);Require(string.Join(",",trace)=="grant:1,report:1"&&manager.Data.rewardId==2,"grant return ignored and index advances after report");
                Require(OutgameItemTimestamp.ToDateTime(manager.Data.nextGetAwardTime)==new DateTime(2026,10,1)&&!manager.CanGetTodayReward(),"next eligibility at next midnight");
                var saved=OutgameFestActivityData.Read(backend.Disk["Proj_hdzdFestActManager"]);Require(saved.rewardId==1&&saved.nextGetAwardTime==0,"grant-triggered save precedes manager progress; no invented final save");
                manager.CompleteSpecialLevel1();manager.CompleteSpecialLevel2();manager.OnSave();backend.Finish(true);manager.UpdateDataCallBack(storage.ReadLocalData());Require(manager.Data.rewardId==2&&manager.Data.limetSkinStatus==3,"later explicit save/reload persists progression and special flags");
                now=manager.Data.nextGetAwardTime;clock.Updata(0,1.1f);Require(!manager.CanGetTodayReward(),"strict equality at midnight is not eligible");now++;clock.Updata(0,1);Require(manager.CanGetTodayReward(),"one millisecond after boundary eligible");clock.OnDispose();
            });
            check("outgame-commander-manager-original-json-load-save-restart",()=>{
                const string configs="{\"Datas\":[{\"id\":1,\"skills\":[1001]},{\"id\":2,\"skills\":[2001]}]}";
                var h=new DataStorageHost();var backend=new StorageBackend();backend.Disk["Proj_hdzdCommanderManager"]="{\"UsedCommanderId\":2,\"commanderDatas\":[{\"Id\":2,\"curLevel\":4,\"isNew\":true,\"skillsData\":[{\"skillId\":2001,\"skillLevel\":3}]},{\"Id\":999,\"curLevel\":8,\"skillsData\":[]}]}";
                var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});var profile=new OutgameProfile();var storage=new OutgameDataManagerStorage(()=>"CommanderManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions);
                var manager=new OutgameCommanderManager(profile,configs,storage,h,x=>throw new Exception("offline must read local"));
                manager.OnInit();Require(profile.usedCommanderId==2&&profile.commanders.Find(x=>x.id==2).level==4&&profile.commanders.Find(x=>x.id==1).level==0,"held commander retained and missing config initialized locked");
                var selected=profile.commanders.Find(x=>x.id==2);selected.level=5;selected.skillLevels[0]=4;manager.OnSave();backend.Finish(true);
                Require(!profile.commanders.Exists(x=>x.id==999),"save list rebuilt from valid manager dictionary");
                var restart=new OutgameProfile();var next=new OutgameCommanderManager(restart,configs,new OutgameDataManagerStorage(()=>"CommanderManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions),h,x=>{});next.OnInit();
                var reloaded=restart.commanders.Find(x=>x.id==2);Require(restart.usedCommanderId==2&&reloaded.level==5&&reloaded.isNew&&reloaded.skillLevels[0]==4,"fresh manager reloads original schema and live state mutations");
                Require(!backend.Disk["Proj_hdzdCommanderManager"].Contains("schemaVersion"),"platform record is original manager data, not reconstruction envelope");
            });
            check("outgame-commander-manager-download-route-and-empty-reset",()=>{
                const string configs="{\"Datas\":[{\"id\":1,\"skills\":[1001]}]}";var h=new DataStorageHost{IsUseServer=true};var backend=new StorageBackend();var calls=new List<string>();var profile=new OutgameProfile{usedCommanderId=9};
                var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});var manager=new OutgameCommanderManager(profile,configs,new OutgameDataManagerStorage(()=>"CommanderManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions),h,calls.Add);
                manager.OnInit();Require(calls.Count==1&&calls[0]=="CommanderManager"&&backend.Reads==0&&profile.usedCommanderId==9,"server initialization waits for real callback without fabricating local completion");
                manager.UpdateDataCallBack("");Require(profile.usedCommanderId==1&&profile.commanders.Count==1&&profile.commanders[0].level==0&&profile.commanders[0].skillLevels[0]==1,"empty source record resets constructor defaults and fills config");
                manager.ParticipatesInSync=false;manager.OnInit();Require(calls.Count==1&&backend.Reads==1,"non-sync initialization reads local despite server setting");
            });
            check("outgame-commander-original-config-pool-file-restart",()=>{
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"commander");var work=new Queue<Action>();var h=new DataStorageHost();
                string config=Resources.Load<TextAsset>("Data/Outgame/CommanderConfig").text;var profile=new OutgameProfile();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var sdk=new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>throw new Exception(x));
                var manager=new OutgameCommanderManager(profile,config,new OutgameDataManagerStorage(()=>"CommanderManager",h,sdk,versions),h,x=>throw new Exception("unexpected download"));
                var entries=new List<OutgameManagerRegistration>();
                foreach(var entry in OutgameManagerRegistrationCatalog.Read(Resources.Load<TextAsset>("Data/OutgameManagerRegistration").text,id=>{Require(id==4028,"only selected original commander factory instantiated");return manager;}))if(entry.SourceTypeIndex==4028)entries.Add(entry);
                var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>throw new Exception(x));pool.OnInit(true,"Proj_hdzd",entries);
                Require(profile.commanders.Count==6&&profile.commanders.TrueForAll(x=>x.level==0),"six original commander configs initialize locked without invented grants");
                profile.commanders.Find(x=>x.id==3).level=7;profile.usedCommanderId=3;pool.SaveData();Require(work.Count==1,"pool writes original manager through SDK queue");work.Dequeue()();
                var next=new OutgameProfile();var restarted=new OutgameCommanderManager(next,config,new OutgameDataManagerStorage(()=>"CommanderManager",h,new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),versions),h,x=>{});restarted.OnInit();
                Require(next.commanders.Count==6&&next.usedCommanderId==3&&next.commanders.Find(x=>x.id==3).level==7,"fresh provider reconstructs source state from isolated actual file");
            });
            check("outgame-skin-original-wire-fields-new-flags-and-held-records",()=>{
                string soldiers=Resources.Load<TextAsset>("Data/Outgame/SkinConfig").text,scenes=Resources.Load<TextAsset>("Data/Outgame/SceneSkinConfig").text;
                var catalog=OutgameSkinCatalog.FromOriginal(null,soldiers,scenes);var skin=catalog.OrderedSoldiers[0];skin.u=true;skin.isNew=true;catalog.SetUsedSkin(skin.skinType,skin.s);
                catalog.State.skins.Add(new OutgameSkinData{s=999999,u=true});string json=catalog.ToOriginalJson();
                Require(!json.Contains("skinType")&&!json.Contains("sortNo")&&!json.Contains("special")&&!json.Contains("castType")&&!json.Contains("isNew"),"original wire excludes all NonSerialized skin fields");
                var next=OutgameSkinCatalog.FromOriginal(json,soldiers,scenes);Require(next.Skin(skin.s).u&&next.Skin(skin.s).isNew&&next.UsedSkin(skin.skinType)==skin.s,"original owned/new/equipped fields round-trip");
                Require(next.State.skins.Exists(x=>x.s==999999&&x.u),"source retains unknown held skin record in serialized list");
            });
            check("outgame-skin-manager-instance-download-and-legacy-save",()=>{
                string soldiers=Resources.Load<TextAsset>("Data/Outgame/SkinConfig").text,scenes=Resources.Load<TextAsset>("Data/Outgame/SceneSkinConfig").text;
                var h=new DataStorageHost{IsUseServer=true};var backend=new StorageBackend();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});var calls=new List<string>();OutgameSkinManager current=null;
                var profile=new OutgameProfile();var manager=new OutgameSkinManager(profile,soldiers,scenes,new OutgameDataManagerStorage(()=>"SkinManager",h,new OutgameSdkStringStorage(backend,x=>{}),versions),h,key=>{Require(current!=null,"source singleton assigned before update request");calls.Add(key);},value=>current=value);
                manager.OnInit();Require(current==manager&&calls.Count==1&&calls[0]=="SkinManager"&&backend.Reads==0&&manager.Catalog==null,"server route remains pending callback");
                manager.UpdateDataCallBack("");var skin=manager.Catalog.OrderedSoldiers[0];skin.isNew=true;int used=manager.Catalog.UsedSkin(skin.skinType);
                manager.ApplyLegacy("{\"list_playerskin\":[{\"skinId\":"+skin.s+",\"isUnlock\":false}]}");Require(!skin.u&&skin.isNew&&manager.Catalog.UsedSkin(skin.skinType)==used&&backend.Calls.Count==0,"legacy transfer changes ownership only, no hidden save/equip/new reset");
                manager.OnSave();backend.Finish(true);Require(versions.Local["SkinManager"].versionNumber==0&&backend.Disk.ContainsKey("Proj_hdzdSkinManager"),"original skin key saved with version tracking");
            });
            check("outgame-skin-manager-original-registry-real-file-restart",()=>{
                string soldiers=Resources.Load<TextAsset>("Data/Outgame/SkinConfig").text,scenes=Resources.Load<TextAsset>("Data/Outgame/SceneSkinConfig").text;
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"skins");var work=new Queue<Action>();var h=new DataStorageHost();var profile=new OutgameProfile();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                var sdk=new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>throw new Exception(x));var manager=new OutgameSkinManager(profile,soldiers,scenes,new OutgameDataManagerStorage(()=>"SkinManager",h,sdk,versions),h,x=>{},x=>{});
                var entries=new List<OutgameManagerRegistration>();foreach(var entry in OutgameManagerRegistrationCatalog.Read(Resources.Load<TextAsset>("Data/OutgameManagerRegistration").text,id=>manager))if(entry.SourceTypeIndex==4150)entries.Add(entry);
                var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>throw new Exception(x));pool.OnInit(true,"Proj_hdzd",entries);
                var selected=manager.Catalog.OrderedSoldiers[1];selected.u=true;selected.isNew=true;manager.Catalog.SetUsedSkin(selected.skinType,selected.s);pool.SaveData();Require(work.Count==1,"pool delegates original skin write");work.Dequeue()();
                var restarted=new OutgameSkinManager(new OutgameProfile(),soldiers,scenes,new OutgameDataManagerStorage(()=>"SkinManager",h,new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),versions),h,x=>{},x=>{});restarted.OnInit();
                Require(restarted.Catalog.Skin(selected.s).u&&restarted.Catalog.Skin(selected.s).isNew&&restarted.Catalog.UsedSkin(selected.skinType)==selected.s,"fresh manager restores original ownership/new/equipment from actual file");
            });
            check("outgame-local-record-complete-fields-and-long-precision",()=>{
                const string json="{\"buyRemoveAd\":true,\"dealOldComm\":255,\"LevelID\":44,\"defeatState\":2,\"goldNum\":99,\"diamondsNum\":7,\"strengthsNum\":8,\"ToolValue\":9,\"bankNum\":11,\"bankAdNum\":12,\"firstChargeData\":{\"hasCharge\":true,\"rewardState\":3,\"firstChargeTime\":9223372036854775800},\"jumpData\":{\"defeatNum\":4,\"jumpNum\":5,\"jumpDayOfYear\":270},\"PurchasedJewelKey\":[\"bankSKU\"],\"season_pass\":6,\"groupLevelIndex\":7,\"SpeedUpPerDay\":8}";
                var record=OutgameLocalRecord.Read(json);string saved=record.ToOriginalJson();var next=OutgameLocalRecord.Read(saved);
                Require(next.buyRemoveAd&&next.dealOldComm==255&&next.defeatState==2&&next.LevelID==44&&next.firstChargeData.hasCharge&&next.firstChargeData.rewardState==3&&next.firstChargeData.firstChargeTime==9223372036854775800L,"ad/migration/progression/first charge source fields and int64 precision retained");
                Require(next.jumpData.defeatNum==4&&next.jumpData.jumpNum==5&&next.jumpData.jumpDayOfYear==270&&next.PurchasedJewelKey[0]=="collectSKU"&&next.season_pass==6&&next.groupLevelIndex==7&&next.SpeedUpPerDay==8,"jump/purchase/season/group/daily fields retained; original bank replacement applies to values too");
                Require(next.collectNum==11&&next.collectAdNum==12&&!saved.Contains("dailyChallengeData"),"normalized collection fields and original NonSerialized daily field");
                var fresh=OutgameLocalRecord.Read(null);Require(fresh.LevelID==0&&fresh.season_pass==1&&fresh.firstChargeData!=null&&fresh.jumpData!=null&&fresh.toolCounts.Count==0&&fresh.PurchasedJewelKey.Count==0,"source constructor defaults without invented starting level or rewards");
            });
            check("outgame-local-limited-bag-source-clock-filter-and-duplicate",()=>{
                var epoch=new DateTime(1970,1,1,8,0,0);int reads=0;var bags=new OutgameLocalLimitedBags("{\"Datas\":[{\"id\":10,\"isActive\":1},{\"id\":20,\"isActive\":0}]}",()=>{reads++;return epoch.AddMilliseconds(1000);});
                var record=new OutgameLocalRecord();record.limitedBagInfos.AddRange(new[]{new OutgameLimitedBagRecord{id=1,ItemBagId=10,LimitedTime=1001,isActive=false},new OutgameLimitedBagRecord{id=2,ItemBagId=10,LimitedTime=1000},new OutgameLimitedBagRecord{id=3,ItemBagId=10,LimitedTime=0},new OutgameLimitedBagRecord{id=4,ItemBagId=20,LimitedTime=2000},new OutgameLimitedBagRecord{id=5,ItemBagId=999,LimitedTime=2000}});
                bags.Initialize(record);Require(bags.Held.Count==1&&bags.Held.ContainsKey(1)&&reads==2,"strict expiry comparison, config isActive filter, zero/missing skip, held isActive does not gate inclusion");
                Require(OutgameLocalLimitedBags.ToSourceDateTime(0)==epoch&&OutgameLocalLimitedBags.ToSourceDateTime(1001).Kind==DateTimeKind.Unspecified,"original eight-hour epoch and millisecond conversion");
                bags.PrepareSave(record);Require(record.limitedBagInfos.Count==1&&record.limitedBagInfos[0].id==1,"save prunes filtered records by dictionary values");
                record.limitedBagInfos.Add(new OutgameLimitedBagRecord{id=1,ItemBagId=10,LimitedTime=2000});bool duplicate=false;try{bags.Initialize(record);}catch(ArgumentException){duplicate=true;}Require(duplicate&&bags.Held.Count==1,"source Dictionary.Add rejects duplicate valid ids retaining prefix");
            });
            check("outgame-local-manager-full-record-actual-file-restart",()=>{
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"local");var work=new Queue<Action>();var h=new DataStorageHost();var profile=new OutgameProfile();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                string skills=Resources.Load<TextAsset>("Data/AllSkillConfig").text,products=Resources.Load<TextAsset>("Data/Outgame/GameProductConfig").text;var sdk=new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>throw new Exception(x));int registrations=0;
                var manager=new OutgameLocalDataManager(profile,skills,products,()=>new DateTime(2026,9,29),new OutgameDataManagerStorage(()=>"LocalDataManager",h,sdk,versions),h,x=>throw new Exception("unexpected server request"),(value,text)=>{Require(profile.inventory.toolCounts.Count>0,"tools initialized before save registration");registrations++;});
                manager.OnInit();Require(registrations==1&&manager.Record.season_pass==1,"source fresh initialization registers after defaults");
                manager.Record.buyRemoveAd=true;manager.Record.firstChargeData.firstChargeTime=9223372036854775800L;manager.Record.PurchasedJewelKey.Add("sku");manager.Record.jumpData.jumpNum=3;profile.levelID=17;profile.inventory.goldNum=4321;
                manager.LimitedBags.Held.Add(42,new OutgameLimitedBagRecord{id=42,ItemBagId=100101,LimitedTime=1893456000000L,isActive=true});manager.OnSave();Require(work.Count==1,"original local record queued");work.Dequeue()();
                var next=new OutgameProfile();var restarted=new OutgameLocalDataManager(next,skills,products,()=>new DateTime(2026,9,29),new OutgameDataManagerStorage(()=>"LocalDataManager",h,new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),versions),h,x=>{},(value,text)=>{});restarted.OnInit();
                Require(next.levelID==17&&next.inventory.goldNum==4321&&restarted.Record.buyRemoveAd&&restarted.Record.firstChargeData.firstChargeTime==9223372036854775800L&&restarted.Record.PurchasedJewelKey[0]=="sku"&&restarted.Record.jumpData.jumpNum==3,"progress/inventory and other original fields survive real file restart");
                Require(restarted.LimitedBags.Held.ContainsKey(42),"original active product and future limited bag survive restart");
            });
            check("outgame-save-registry-source-hash-null-and-replacement",()=>{
                Require(OutgameDataSaveRegistry.OriginalStringHash("")==371857150&&OutgameDataSaveRegistry.OriginalStringHash("abc")==1099313834&&OutgameDataSaveRegistry.OriginalStringHash("中文")==1842450162&&OutgameDataSaveRegistry.OriginalStringHash("\ud83d\ude00")==1126268081,"source hash golden values include UTF16 surrogate pair");
                Require(OutgameDataSaveRegistry.OriginalStringHash("a\0b")==372029373&&OutgameDataSaveRegistry.OriginalStringHash("a\0b")==OutgameDataSaveRegistry.OriginalStringHash("a"),"original hash stops at embedded zero");
                var registry=new OutgameDataSaveRegistry();var key=new VersionManager{DataKey="LocalDataManager"};Require(registry.GetRegStringValue(null)==null,"uninitialized singleton returns null before accessing manager");
                registry.RegisterSaveData(key,"abc");Require(registry.GetRegStringValue(key)=="abc"&&registry.Hashes[key.DataKey]==1099313834,"register records original text and source hash");
                registry.RegisterSaveData(key,"new");Require(registry.GetRegStringValue(key)=="new","duplicate key replaces original registered string");int prior=registry.Hashes[key.DataKey];bool failed=false;
                try{registry.RegisterSaveData(key,null);}catch(NullReferenceException){failed=true;}Require(failed&&registry.GetRegStringValue(key)==null&&registry.Hashes[key.DataKey]==prior,"null registration preserves source partial update: text changes before hash fails");
                bool missing=false;try{registry.GetRegStringValue(new VersionManager{DataKey="missing"});}catch(KeyNotFoundException){missing=true;}Require(missing,"initialized registry uses Dictionary indexer, not fallback");
            });
            check("outgame-local-skin-commander-registry-startup-migration",()=>{
                string commanderConfig=Resources.Load<TextAsset>("Data/Outgame/CommanderConfig").text,skills=Resources.Load<TextAsset>("Data/AllSkillConfig").text,products=Resources.Load<TextAsset>("Data/Outgame/GameProductConfig").text,soldiers=Resources.Load<TextAsset>("Data/Outgame/SkinConfig").text,scenes=Resources.Load<TextAsset>("Data/Outgame/SceneSkinConfig").text;
                int skinId=OutgameSkinCatalog.FromOriginal(null,soldiers,scenes).OrderedSoldiers[0].s;
                string original="{\"bankNum\":19,\"UsedCommanderId\":1,\"commanderDatas\":[{\"Id\":1,\"curLevel\":4,\"isNew\":true,\"skillsData\":[]}],\"list_playerskin\":[{\"skinId\":"+skinId+",\"isUnlock\":false}]}";
                var backend=new StorageBackend();backend.Disk["Proj_hdzdLocalDataManager"]=original;var sdk=new OutgameSdkStringStorage(backend,x=>{});var h=new DataStorageHost();var profile=new OutgameProfile();var registry=new OutgameDataSaveRegistry();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                Func<string,OutgameDataManagerStorage> storage=key=>new OutgameDataManagerStorage(()=>key,h,sdk,versions);
                var local=new OutgameLocalDataManager(profile,skills,products,()=>new DateTime(2026,9,29),storage("LocalDataManager"),h,x=>{},registry.RegisterSaveData);
                var commander=new OutgameCommanderManager(profile,commanderConfig,storage("CommanderManager"),h,x=>{});var skin=new OutgameSkinManager(profile,soldiers,scenes,storage("SkinManager"),h,x=>{},x=>{});
                var factories=new Dictionary<int,IOutgameDataManager>{{4028,commander},{4119,local},{4150,skin}};var entries=new List<OutgameManagerRegistration>();
                foreach(var entry in OutgameManagerRegistrationCatalog.Read(Resources.Load<TextAsset>("Data/OutgameManagerRegistration").text,id=>factories[id]))if(factories.ContainsKey(entry.SourceTypeIndex))entries.Add(entry);
                var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>throw new Exception(x));pool.OnInit(true,"Proj_hdzd",entries);
                Require(profile.commanders.Find(x=>x.id==1).level==0&&registry.GetRegStringValue(local)==original.Replace("bank","collect")&&profile.inventory.collectNum==19,"original local registration retains normalized legacy payload independently of local DTO");
                OutgameLegacyMigration.Apply(registry,local,skin,commander);Require(profile.commanders.Find(x=>x.id==1).level==4&&profile.commanders.Find(x=>x.id==1).isNew&&!skin.Catalog.Skin(skinId).u&&backend.Calls.Count==0,"source startup transfers skin then commander without implicit save");
                pool.SaveData();for(int i=0;i<3;i++)backend.Finish(true);
                Require(backend.Disk["Proj_hdzdCommanderManager"].Contains("\"curLevel\":4")&&registry.GetRegStringValue(local)==original.Replace("bank","collect"),"commander live dictionary reindexed after migration; saving does not replace registered original payload");
            });
            check("outgame-startup-entry-page-migration-scene-callback-order",()=>{
                var h=new StartupHost();new OutgameStartupEntry(h,()=>h.Calls.Add("migration")).Enter();
                Require(string.Join("|",h.Calls)=="menu|visible:False|reward|migration|prefabs|load:GamePlay:False|enter:True|late|red:True|red-state:1","source menu hidden before reward/migration, prefabs before scene request, flags after LoadScene returns");
                h.Calls.Clear();h.CurrentLevel=23;h.Completed();Require(string.Join("|",h.Calls)=="event|activity:EnterGameHome:23|rank|seven|play:1|close|interactive:","callback rereads current level and enters source state before closing loading/interactive report");
                h.Calls.Clear();h.Completed();Require(h.Calls.Count==7,"source has no added completion-once guard");
            });
            check("outgame-startup-entry-synchronous-callback-and-failure",()=>{
                var h=new StartupHost{Synchronous=true};new OutgameStartupEntry(h,()=>h.Calls.Add("migration")).Enter();
                Require(h.Calls.IndexOf("interactive:")<h.Calls.IndexOf("enter:True")&&h.Calls.IndexOf("enter:True")<h.Calls.IndexOf("late"),"source synchronous scene callback completes before post-request flags and late init");
                h=new StartupHost{FailLoad=true};bool failed=false;try{new OutgameStartupEntry(h,()=>{}).Enter();}catch(InvalidOperationException){failed=true;}
                Require(failed&&!h.EnterGame&&!h.RedDotSourceFlag8&&!h.Calls.Contains("close"),"thrown scene request does not claim entry or close loading");
                h=new StartupHost();failed=false;try{new OutgameStartupEntry(h,()=>throw new InvalidOperationException("migration")).Enter();}catch(InvalidOperationException){failed=true;}
                Require(failed&&h.Calls.Count==3&&h.Completed==null,"migration failure stops prefabs/scene entry after source page setup");
            });
            check("outgame-prefab-cache-original-ranges-and-object-lifetime",()=>{
                var made=new List<GameObject>();var destroyed=new List<GameObject>();
                var cache=new OutgamePrefabCache(x=>null,value=>{destroyed.Add(value);UnityEngine.Object.DestroyImmediate(value);});
                var selected=new HashSet<int>{1000,6999,9033,9034};
                try{
                    foreach(int id in new[]{int.MinValue,-1,0,100,999,1000,6999,7000,9032,9033,9034,9035,int.MaxValue}){
                        var root=new GameObject("source-root-"+id);made.Add(root);var child=new GameObject("pooled");made.Add(child);child.transform.SetParent(root.transform);
                        cache.Roots.Add(id,root.transform);cache.Entities.Add(id,new List<GameObject>{child});cache.Prefabs.Add(id,root);
                    }
                    var cleared=cache.Entities[1000];var retained=cache.Entities[100];cache.ClearModelEntityCache();
                    Require(destroyed.Count==4&&cleared.Count==0&&retained.Count==1,"only source model range and shadow roots destroyed; held list references cleared");
                    foreach(int id in selected)Require(!cache.Roots.ContainsKey(id)&&!cache.Entities.ContainsKey(id)&&cache.Prefabs.ContainsKey(id),"selective clear removes root and pool but retains prefab index");
                    Require(cache.Roots.Count==9&&cache.Entities.Count==9&&cache.Roots[100]!=null,"nonmatching objects remain alive and indexed");
                    cache.ClearModelEntityCache();Require(destroyed.Count==4,"repeat selective clear preserves retained models");
                }finally{foreach(var value in made)if(value!=null)UnityEngine.Object.DestroyImmediate(value);}
            });
            check("outgame-prefab-init-clear-order-and-missing-resource",()=>{
                var config=ScriptableObject.CreateInstance<OutgameLevelEditorConfig>();config.IsEditor=true;config.StartLevel=77;
                var retained=new GameObject("retained-original-root");var removed=new GameObject("removed-original-root");OutgamePrefabCache cache=null;int reads=0;
                try{
                    cache=new OutgamePrefabCache(path=>{reads++;Require(path=="LevelEditor"&&cache.Prefabs.Count==0&&cache.Entities.Count==0&&cache.Roots.Count==0,"all dictionaries clear before resource lookup");return config;},value=>UnityEngine.Object.DestroyImmediate(value));
                    cache.Roots.Add(100,retained.transform);cache.Roots.Add(1000,removed.transform);var outsidePool=new List<GameObject>{retained};cache.Entities.Add(100,outsidePool);cache.Prefabs.Add(100,retained);
                    cache.InitData();Require(reads==1&&!config.IsEditor&&config.StartLevel==77&&retained!=null&&removed==null&&outsidePool.Count==1,"InitData drops retained references without destroying outside-range roots or clearing their external lists; only IsEditor changes");
                    cache=new OutgamePrefabCache(path=>null);cache.Prefabs.Add(1,retained);bool failed=false;try{cache.InitData();}catch(NullReferenceException){failed=true;}
                    Require(failed&&cache.Prefabs.Count==0,"missing original resource throws after clear, without synthesized config");
                }finally{if(retained!=null)UnityEngine.Object.DestroyImmediate(retained);if(removed!=null)UnityEngine.Object.DestroyImmediate(removed);UnityEngine.Object.DestroyImmediate(config);}
            });
            check("outgame-prefab-cache-failure-preserves-source-partial-state",()=>{
                var root=new GameObject("failed-pool-root");var cache=new OutgamePrefabCache(path=>null,value=>UnityEngine.Object.DestroyImmediate(value));
                try{
                    cache.Roots.Add(1000,root.transform);cache.Entities.Add(1000,null);bool failed=false;
                    try{cache.ClearModelEntityCache();}catch(NullReferenceException){failed=true;}
                    Require(failed&&root==null&&cache.Roots.ContainsKey(1000)&&cache.Entities.ContainsKey(1000),"root destroyed before null list throws; final root removal pass has not run");
                }finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-user-preferences-original-fields-init-and-overwrite",()=>{
                int reads=0,errors=0,warnings=0,writes=0;string saved=null;
                var prefs=new OutgameUserPreferences(key=>{reads++;Require(key=="UserData.txt","original preference filename");return "{\"UserDataList\":[null,{\"Key\":\"\",\"IntVar\":9},{\"Key\":\"Day\",\"IntVar\":1},{\"Key\":\"Day\",\"IntVar\":2,\"StrVar\":\"held\",\"FloatVar\":1.25}]}";},(key,text)=>{writes++;saved=text;},()=>false,x=>{},x=>warnings++,(code,text)=>errors++,x=>{});
                prefs.SetInt("Day",99);Require(prefs.GetInt("Day",7)==7,"uninitialized writes ignored and getters use fallback");prefs.OnInit(true);Require(reads==0,"platform-owned initialization remains deferred");prefs.OnInit(false);prefs.OnInit(false);
                Require(reads==1&&warnings==1&&errors==0&&prefs.GetInt("Day")==2,"managed init once; null/empty records skipped and duplicate key overwritten");
                prefs.SetInt("Day",3);prefs.SetFloat("Day",2.5f);prefs.SetString("Day",null);Require(prefs.GetString("Day")=="held"&&prefs.GetFloat("Day")==2.5f,"typed columns coexist and null string setter ignored");
                prefs.SetInt("",5);prefs.OnSave();prefs.OnSave();Require(writes==1&&saved.Contains("UserDataList")&&saved.Contains("StrVar")&&saved.Contains("FloatVar"),"source record schema and MD5 duplicate suppression");
            });
            check("outgame-user-preferences-save-failure-and-restart",()=>{
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"prefs");Directory.CreateDirectory(root);string path=Path.Combine(root,"UserData.txt");var reports=new List<string>();int errors=0;
                Func<OutgameUserPreferences> create=()=>new OutgameUserPreferences(key=>File.Exists(path)?File.ReadAllText(path):null,(key,text)=>File.WriteAllText(path,text),()=>false,x=>{},x=>{},(code,text)=>{Require(code==1001,"source missing record report");errors++;},reports.Add);
                var prefs=create();prefs.OnInit(false);prefs.SetInt("Day",9);prefs.SetString("GF_LoginUserID","user-42");prefs.SetFloat("volume",0.25f);prefs.OnSave();var next=create();next.OnInit(false);
                Require(errors==1&&reports[0]=="user-42"&&next.GetInt("Day")==9&&next.GetFloat("volume")==0.25f&&next.GetString("GF_LoginUserID")=="user-42","original record actual isolated file restart");
                int attempts=0;bool disabled=true;var failing=new OutgameUserPreferences(key=>"{}",(key,text)=>{attempts++;throw new IOException("write");},()=>disabled,x=>{},x=>{},(code,text)=>{},x=>throw new Exception("must not report failed write"));failing.OnInit(false);failing.SetInt("Day",5);failing.OnSave();Require(attempts==0,"disabled save does not change digest");disabled=false;bool failed=false;try{failing.OnSave();}catch(IOException){failed=true;}failing.OnSave();Require(failed&&attempts==1,"source digest assigned before failed write, unchanged retry suppressed");
            });
            check("outgame-new-day-rereads-update-event-and-backward-days",()=>{
                var prefs=new OutgameUserPreferences(key=>"{}",(key,text)=>throw new Exception("check does not save"),()=>false,x=>{},x=>{},(code,text)=>{},x=>{});prefs.OnInit(false);prefs.SetInt("Day",4);
                var days=new Queue<int>(new[]{5,6});var events=new List<object[]>();var daily=new OutgameNewDay(prefs,()=>days.Dequeue(),(name,args)=>{Require(name=="GameEnterNewDay"&&prefs.GetInt("Day")==6,"stored day updated before event");events.Add(args);});daily.Check();
                Require(events.Count==1&&(int)events[0][0]==6&&(int)events[0][1]==4&&days.Count==0,"source reads SDK day again after comparison and emits new/old order");
                int reads=0;new OutgameNewDay(prefs,()=>{reads++;return 6;},(name,args)=>throw new Exception("unchanged")).Check();Require(reads==1,"unchanged day only one SDK read");
                new OutgameNewDay(prefs,()=>2,(name,args)=>Require((int)args[0]==2&&(int)args[1]==6,"rollback still emits changed day")).Check();Require(prefs.GetInt("Day")==2,"no monotonic guard invented");
            });
            check("outgame-new-day-event-failure-retains-write-and-no-repeat",()=>{
                var prefs=new OutgameUserPreferences(key=>"{}",(key,text)=>{},()=>false,x=>{},x=>{},(code,text)=>{},x=>{});prefs.OnInit(false);int sends=0;var daily=new OutgameNewDay(prefs,()=>3,(name,args)=>{sends++;throw new InvalidOperationException("subscriber");});bool failed=false;try{daily.Check();}catch(InvalidOperationException){failed=true;}daily.Check();
                Require(failed&&prefs.GetInt("Day")==3&&sends==1,"event error cannot roll back day; repeated check does not replay completed day update");
            });
            check("outgame-preference-webgl-sdk-queue-key-and-actual-restart",()=>{
                string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"webgl-prefs");var work=new Queue<Action>();var logs=new List<string>();var reports=new List<string>();
                var sdk=new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>throw new Exception(x));var files=new OutgameWebGlFileStorage(sdk,()=>true,logs.Add);
                var prefs=files.CreatePreferences(()=>false,x=>{},x=>{},(code,text)=>{},reports.Add);prefs.OnInit(false);prefs.SetInt("Day",12);prefs.SetString("GF_LoginUserID","local-user");prefs.OnSave();
                Require(work.Count==1&&reports.Count==1&&reports[0]=="local-user","save report occurs on enqueue return, not asynchronous completion");
                Require(files.Read("UserData.txt").Contains("local-user")&&files.Read("Proj_hdzdUserData.txt")=="","fixed original key and immediate SDK cache, no inferred game/account prefix");
                var preRestart=new OutgameWebGlFileStorage(new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),()=>false,x=>{});Require(preRestart.Read("UserData.txt")=="","fresh process cannot see queued write until backend runs");work.Dequeue()();
                var freshFiles=new OutgameWebGlFileStorage(new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),()=>false,x=>{});var next=freshFiles.CreatePreferences(()=>false,x=>{},x=>{},(code,text)=>{},x=>{});next.OnInit(false);
                Require(next.GetInt("Day")==12&&next.GetString("GF_LoginUserID")=="local-user"&&logs[0].StartsWith("调用小游戏读取数据：key[UserData.txt]"),"entire original filename->SDK queue->actual file->fresh preferences pipeline");
            });
            check("outgame-preference-webgl-async-failure-retains-cache-digest",()=>{
                var backend=new StorageBackend();int errors=0,reports=0;var files=new OutgameWebGlFileStorage(new OutgameSdkStringStorage(backend,x=>errors++),()=>false,x=>{});var prefs=files.CreatePreferences(()=>false,x=>{},x=>{},(code,text)=>{},x=>reports++);prefs.OnInit(false);prefs.SetInt("Day",3);prefs.OnSave();backend.Finish(false);
                Require(errors==1&&reports==1&&files.Read("UserData.txt").Contains("Day")&&!backend.Disk.ContainsKey("UserData.txt"),"async failure logs but does not roll back SDK cache or already-issued report");int calls=backend.Calls.Count;prefs.OnSave();Require(backend.Calls.Count==calls,"unchanged preferences remain deduplicated after backend failure");
                prefs.SetInt("Day",4);prefs.OnSave();backend.Finish(true);Require(backend.Disk.ContainsKey("UserData.txt")&&reports==2,"changed record can persist after failure and queue continues");
            });
            check("outgame-original-wx-preferences-inherited-zero-day-and-managed-init",()=>{
                var backend=new StorageBackend();backend.Disk["UserData.txt"]="{\"UserDataList\":[{\"Key\":\"Day\",\"IntVar\":12}]}";var events=new List<object[]>();var files=new OutgameWebGlFileStorage(new OutgameSdkStringStorage(backend,x=>{}),()=>false,x=>{});var session=new OutgameWxPreferenceSession(files,()=>false,x=>{},x=>{},(code,text)=>throw new Exception(text),x=>{},(name,args)=>{Require(name=="GameEnterNewDay","source event name");events.Add(args);});
                session.Initialize();Require(backend.Reads==1&&session.Preferences.GetInt("Day")==12,"concrete WX inherits CanReadLocalData true; managed load executes");session.CheckNewDay();
                Require(events.Count==1&&(int)events[0][0]==0&&(int)events[0][1]==12&&session.Preferences.GetInt("Day")==0&&backend.Calls.Count==0,"actual inherited SDK day is zero; old nonzero day transitions without automatic save");session.CheckNewDay();Require(events.Count==1,"same inherited day does not create repeated calendar rewards");session.Save();backend.Finish(true);
                var nextFiles=new OutgameWebGlFileStorage(new OutgameSdkStringStorage(backend,x=>{}),()=>false,x=>{});var next=new OutgameWxPreferenceSession(nextFiles,()=>false,x=>{},x=>{},(code,text)=>throw new Exception(text),x=>{},(name,args)=>throw new Exception("persisted zero day must not reemit"));next.Initialize();next.CheckNewDay();Require(next.Preferences.GetInt("Day")==0,"fresh composed session restores saved zero day from original SDK key");
            });
            check("outgame-video-mapping-original-table-and-reference-semantics",()=>{
                var rows=OutgameVideoMapping.ReadOriginalTable(Resources.Load<TextAsset>("Data/Outgame/WXAMSConfig").text);var map=OutgameVideoMapping.Build(rows);var errors=new List<string>();var mapping=new OutgameVideoMapping(errors.Add);mapping.SetVideoMapping(map);
                Require(rows.Length==59&&map.Count==59&&mapping.ResolvePlacement("ShopUI_sendGold")==4&&mapping.ResolvePlacement("ShopUI_sendSkin1")==67&&mapping.ResolvePlacement("Game_sendTool_skill18")==8,"all original source rows imported with label->placement direction");
                map["ShopUI_sendGold"]=11;Require(mapping.ResolvePlacement("ShopUI_sendGold")==11&&errors.Count==0,"bridge retains source dictionary reference instead of copying");
                bool duplicate=false;try{OutgameVideoMapping.Build(new[]{new OutgameVideoPlacement{ReportLable="same",PlacementId=1},new OutgameVideoPlacement{ReportLable="same",PlacementId=2}});}catch(ArgumentException){duplicate=true;}Require(duplicate,"duplicate labels reject startup build via original Dictionary.Add");
            });
            check("outgame-video-mapping-fallback-errors-and-missing-setup",()=>{
                var errors=new List<string>();var mapping=new OutgameVideoMapping(errors.Add);bool missing=false;try{mapping.ResolvePlacement("x");}catch(NullReferenceException){missing=true;}Require(missing&&errors.Count==0,"source unset dictionary throws, no synthesized empty mapping");
                mapping.SetVideoMapping(new Dictionary<string,int>{{"zero",0},{"negative",-3}});Require(mapping.ResolvePlacement("unknown")==68&&mapping.ResolvePlacement("zero")==68&&mapping.ResolvePlacement("negative")==68,"all missing/nonpositive placements fall back to source 68");
                Require(errors[0]=="WXAMS,未设置视频名称映射,video_name:unknown,placementId:0"&&errors[2]=="WXAMS,视频名称映射为0或负数,video_name:negative,placementId:-3","exact source error classification and original value retained");
                var failing=new OutgameVideoMapping(x=>throw new InvalidOperationException("report"));failing.SetVideoMapping(new Dictionary<string,int>());bool failed=false;try{failing.ResolvePlacement("unknown");}catch(InvalidOperationException){failed=true;}Require(failed,"error callback failure not swallowed into successful fallback");
            });
            check("outgame-reward-video-source-ready-lock-and-callback-replacement",()=>{
                var h=new RewardVideoHost();var video=new OutgameRewardVideo(h);Require(video.Ready&&video.LastClick==-1f,"source constructor readiness and initial click time");video.Ready=false;var results=new List<bool>();video.Show(results.Add);Require(results.Count==1&&!results[0]&&!video.Showing&&h.Shown==null,"not-ready path fails without native display");
                video.Ready=true;video.Show(results.Add);Require(video.Showing&&h.Calls[h.Calls.Count-1]=="VideoAdShow","native request before show event");int first=results.Count;var second=new List<bool>();video.Show(second.Add);Require(second.Count==1&&!second[0]&&video.Showing,"locked duplicate fails replacement callback and retains showing flag");h.Closed(true);Require(results.Count==first&&second.Count==1&&!video.Showing,"original pending callback was replaced/cleared by rejected duplicate, no invented reward");
                h.ClickLockParameter="0";h.Realtime=5.5f;video.Show(results.Add);Require(!results[results.Count-1]&&!video.Showing,"strict realtime delta less than one rejected");h.Realtime=6;video.Show(results.Add);Require(video.Showing,"exact one second accepted");
            });
            check("outgame-reward-video-timeout-show-failure-and-close-order",()=>{
                var h=new RewardVideoHost{NativeVideoReady=false};var video=new OutgameRewardVideo(h){Ready=true};var results=new List<bool>();video.Show(results.Add);Require(h.Routine.Current is WaitForSeconds&&video.Showing,"source scaled WaitForSeconds auto-close scheduled");h.Routine.MoveNext();Require(results.Count==1&&!results[0]&&!video.Showing,"timeout returns failure and clears showing");
                h.Realtime=7;video.Show(results.Add);h.Shown(false);Require(!video.Showing&&results.Count==1&&h.Calls.Contains("stop"),"show failure cancels timer and resets showing but retains callback");h.Closed(true);Require(results.Count==2&&results[1]&&h.Calls[h.Calls.Count-1]=="VideoAdOnClose:True","close forwards ended flag then close event");h.Closed(false);Require(results.Count==2,"repeat close emits event but callback already cleared");
            });
            check("outgame-reward-video-string-results-and-callback-throw",()=>{
                var h=new RewardVideoHost();var video=new OutgameRewardVideo(h){Ready=true};int calls=0;video.Show(value=>{Require(value,"only explicit string1 succeeds");calls++;});video.OnRewardADCallBack("-1");video.OnRewardADCallBack("other");Require(calls==0&&h.Calls.Contains("noads:unknow"),"noads and unknown leave callback pending without reward");video.OnRewardADCallBack("1");video.OnRewardADCallBack("1");Require(calls==1&&video.Showing,"string callback clears only callback, not showing flag");
                h.Closed(false);video.Show(value=>throw new InvalidOperationException("recipient"));bool failed=false;try{h.Closed(true);}catch(InvalidOperationException){failed=true;}int before=h.Calls.Count;try{video.OnRewardADCallBack("0");}catch(InvalidOperationException){}Require(failed&&h.Calls.Count==before&&!video.Showing,"throwing callback survives because source clears only after invoke; close event not emitted");
            });
            check("outgame-ad-router-interstitial-block-vs-missing-controller",()=>{
                var router=new OutgameAdRequestRouter(()=>"weixin","tag",x=>{});int shown=0,closed=0;Action<bool> onShow=value=>{Require(!value,"router failure callback");shown++;};Action<bool> onClose=value=>{Require(!value,"router close failure callback");closed++;};
                Require(!router.IsVideoReady(),"no controller not ready");router.ShowVideo(onShow,onClose);Require(shown==1&&closed==0,"missing controller uses shown(false), not reward close");
                router.InterVideoShowing=true;router.ShowVideo(onShow,onClose);Require(shown==1&&closed==1,"remaining interstitial flag uses closed(false) before missing controller test");
                var controller=new AdController{Ready=false};router.VideoController=controller;router.InterVideoShowing=false;router.ShowVideo(onShow,onClose,true);Require(controller.ShowCount==1&&controller.SourceFlag20&&controller.ReadyCount==0,"show forwards option and callbacks without adding readiness guard");Require(!router.IsVideoReady()&&controller.ReadyCount==1,"explicit readiness delegates only when asked");
            });
            check("outgame-ad-router-platform-close-reentrancy-and-order",()=>{
                string platform="weixin";var calls=new List<string>();var router=new OutgameAdRequestRouter(()=>platform,"tag",calls.Add){InterVideoShowing=true};var video=new AdController();var inter=new AdController();router.VideoController=video;router.InterstitialController=inter;
                video.OnClose=()=>calls.Add("video-close");inter.OnClose=()=>{calls.Add("inter-close");router.InterVideoShowing=false;};router.ShowVideo(x=>{},x=>{});Require(video.CloseCount==1&&inter.CloseCount==1&&calls.IndexOf("video-close")<calls.IndexOf("inter-close")&&video.ShowCount==1,"weixin fix closes both in order; callback mutation reread permits show");
                platform="alipay";router.InterVideoShowing=true;router.ShowVideo(x=>{},x=>{});Require(video.CloseCount==1&&inter.CloseCount==2&&video.ShowCount==2,"alipay recovery closes only interstitial then rereads flag");
                platform="unknown";router.InterVideoShowing=true;int blocked=0;router.ShowVideo(null,x=>blocked++);Require(blocked==1&&inter.CloseCount==2&&video.ShowCount==2,"other channels do not invent close repair");
            });
            check("outgame-ad-entry-source-module-gate-cache-and-warning-reread",()=>{
                var h=new RewardVideoHost();var video=new OutgameRewardVideo(h);int lookups=0,rewards=0;var warnings=new List<string>();var entry=new OutgameAdModuleEntry(()=>{lookups++;return video;},warnings.Add);
                entry.ShowRewardAd(value=>rewards++);Require(!entry.IsRewardAdReady()&&rewards==0&&lookups==1&&warnings.Count==3&&h.Shown==null,"inactive module warns without callback; ready check adds original secondary warning; lazy lookup cached");
                var reread=new OutgameAdModuleEntry(()=>video,text=>video.Active=true);Require(reread.IsRewardAdReady(),"CheckFunctionActive rereads flag after warning side effects");
                var missing=new OutgameAdModuleEntry(()=>null,text=>{});bool failed=false;try{missing.IsRewardAdReady();}catch(NullReferenceException){failed=true;}Require(failed,"missing registered module is not silently treated as inactive");
            });
            check("outgame-ad-entry-module-router-controller-full-callback-chain",()=>{
                var environment=new RewardVideoHost();var native=new AdController{Ready=true};var router=new OutgameAdRequestRouter(()=>"weixin","tag",environment.Log){VideoController=native,SourceFlag64=true};var video=new OutgameRewardVideo(new OutgameRewardVideoControllerHost(router,environment)){Active=true};var entry=new OutgameAdModuleEntry(()=>video,environment.Warn);var rewards=new List<bool>();
                Require(entry.IsRewardAdReady()&&native.ReadyCount==0,"entry ready reflects module flag, not lower controller readiness");entry.ShowRewardAd(rewards.Add);Require(native.ReadyCount==1&&native.ShowCount==1&&!native.SourceFlag20&&!router.SourceFlag64&&video.Showing,"entry reaches restored module/router/controller, preserving source flags and option");native.Shown(true);Require(rewards.Count==0&&environment.Calls.Contains("VideoAdOnShowSuccess"),"display success alone does not issue reward");native.Closed(false);Require(rewards.Count==1&&!rewards[0]&&!video.Showing,"early close propagates failure end-to-end");
                entry.ShowRewardAd(rewards.Add);native.Closed(true);Require(rewards.Count==2&&rewards[1]&&environment.Calls[environment.Calls.Count-1]=="VideoAdOnClose:True","ended callback propagates success then close event");
            });
            check("outgame-ad-entry-missing-controller-retains-source-pending-result",()=>{
                var environment=new RewardVideoHost();var router=new OutgameAdRequestRouter(()=>"weixin","tag",environment.Log);var video=new OutgameRewardVideo(new OutgameRewardVideoControllerHost(router,environment)){Active=true};var entry=new OutgameAdModuleEntry(()=>video,environment.Warn);int results=0;entry.ShowRewardAd(value=>results++);
                Require(results==0&&!video.Showing&&environment.Calls.Contains("start")&&environment.Calls.Contains("stop")&&environment.Calls[environment.Calls.Count-1]=="VideoAdShow","native absent: timer starts, shown(false) cancels timer without invoking reward, outer show event still follows");video.OnRewardADCallBack("0");Require(results==1,"pending callback can still be resolved by original string failure route");
            });
            check("outgame-ad-readiness-cache-alternative-and-interval-boundary",()=>{
                var logs=new List<string>();float now=10,last=5;int reads=0;var adapter=new AdAdapterState(()=>{reads++;return now;},()=>last,logs.Add){AdType="VIDEO",SourceState=0,Guarantee=true,Interval=6};
                Require(OutgameAdReadiness.IsReady(new[]{adapter},"prefix",logs.Add)&&reads==0,"cache request alternative marks ready independently of source state and canShow");adapter.Cache=false;adapter.SourceState=2;
                Require(!OutgameAdReadiness.IsReady(new[]{adapter},"prefix",logs.Add),"loaded non-cache guarantee blocked before interval");now=11;Require(OutgameAdReadiness.IsReady(new[]{adapter},"prefix",logs.Add),"exact interval accepted");adapter.AdType="BANNER";now=0;Require(adapter.CanShow(),"banner bypasses guarantee interval");adapter.AdType="VIDEO";adapter.Interval=float.NaN;Require(adapter.CanShow(),"original negated greater-than retains NaN behavior");
            });
            check("outgame-ad-adapter-clock-reread-finish-and-source-exceptions",()=>{
                int reads=0;var adapter=new AdAdapterState(()=>10,()=>++reads==1?0:9,x=>{}){Guarantee=true,Interval=2,SourceState=4,AdType="VIDEO"};Require(!adapter.CanShow()&&reads==2,"source last show time reread after logging affects comparison");adapter.Clearing=()=>Require(adapter.SourceState==4,"cleanup precedes state reset");adapter.Finish();Require(adapter.SourceState==0,"finish resets after clearing cache");
                adapter.SourceState=4;adapter.Clearing=()=>throw new InvalidOperationException("clear");bool failed=false;try{adapter.Finish();}catch(InvalidOperationException){failed=true;}Require(failed&&adapter.SourceState==4,"cleanup failure retains original state");
                bool nullFailed=false;try{OutgameAdReadiness.IsReady(new IOutgameAdReadinessCandidate[]{null},"",x=>{});}catch(NullReferenceException){nullFailed=true;}Require(nullFailed,"source does not skip null adapters");
            });
            check("outgame-ad-cache-priority-rotation-and-guarantee-fallback",()=>{
                CacheCandidate a=new CacheCandidate{Priority=1},b=new CacheCandidate{Priority=3},g=new CacheCandidate{Priority=2,IsGuarantee=true};
                var cache=new List<IOutgameAdCacheCandidate>{b,g,a};var selector=new OutgameAdCacheSelection();
                Func<IOutgameAdCacheCandidate> pick=()=>selector.Select(cache,null,false,0,3,x=>throw new Exception("unexpected bidding"),"",x=>{});
                Require(pick()==a&&cache[0]==a&&selector.LastShowPriority==1,"initial pick sorts actual cache and records priority");
                Require(pick()==b&&pick()==a,"rotation skips guarantee and wraps to first");
                var fallback=new CacheCandidate();cache.Clear();cache.Add(g);Require(selector.Select(cache,fallback,false,0,1,null,"",null)==fallback&&selector.LastShowPriority==1,"all guarantee retains priority and falls back");
                Require(OutgameAdCacheSelection.ComparePriority(null,a)==0&&OutgameAdCacheSelection.ComparePrice(a,null)==0,"original comparers equate null pair members");
            });
            check("outgame-ad-cache-bidding-gates-and-platform-preference",()=>{
                CacheCandidate low=new CacheCandidate{Priority=1,Price=int.MinValue},high=new CacheCandidate{Priority=2,Price=int.MaxValue,IsGuarantee=true};var cache=new List<IOutgameAdCacheCandidate>{high,low};
                var selector=new OutgameAdCacheSelection();int probes=0;var logs=new List<string>();
                Require(selector.Select(cache,null,false,1,2,x=>{probes++;return true;},"tag",logs.Add)==high&&selector.LastShowPriority==-1&&cache[1]==high,"highest signed price wins including guarantee; priority unchanged");
                Require(probes==1&&logs[0]=="tagshow RealPrice:2147483647","price report after sorting and selection");
                Require(selector.Select(cache,null,false,1,3,x=>{probes++;return true;},"",logs.Add)==low&&probes==1,"incomplete cache bypasses price probe and uses priority");
                Require(selector.Select(cache,high,true,1,2,x=>throw new Exception("bypass"),"",null)==high&&cache[0]==low,"preferred platform bypasses cache sorting and policies");
                cache.Clear();Require(selector.Select(cache,null,false,1,0,null,"",null)==null,"empty cache bypasses price policy");
            });
            check("outgame-ad-price-validity-reread-zero-and-negative",()=>{
                int reads=0;var a=new CacheCandidate{PlatformId=7,ReadPrice=()=>++reads==1?12:0};var b=new CacheCandidate{PlatformId=8,Price=-4};var logs=new List<string>();var cache=new List<IOutgameAdCacheCandidate>{a,b};
                Require(!OutgameAdCacheSelection.AllPlatHasPrice(cache,"tag",logs.Add)&&reads==2&&logs[0]=="tagshow allListStr:7 12 ","price is reread after text construction; zero short circuits before next adapter");
                cache.Remove(a);Require(OutgameAdCacheSelection.AllPlatHasPrice(cache,"",logs.Add),"negative nonzero price is valid in original source");
                var selector=new OutgameAdCacheSelection();Require(selector.Select(cache,null,false,1,1,"",logs.Add)==b&&selector.LastShowPriority==-1,"default selection integrates source price policy and bidding");
                cache.Clear();Require(OutgameAdCacheSelection.AllPlatHasPrice(cache,"",logs.Add)&&logs[logs.Count-1]=="show allListStr:","empty direct price check logs and returns true");
            });
            check("outgame-ad-price-storage-date-key-and-zero-cache",()=>{
                var keys=new List<string>();int value=0,clockReads=0;DateTime now=new DateTime(2026,9,29,23,59,0);var price=new OutgameAdPriceCache((key,fallback)=>{Require(fallback==0,"source missing price default");keys.Add(key);return value;},()=>{clockReads++;return now;}){ZoneKey="VIDEO",PlatformId=7,ClearNextDayEnable=1};
                Require(price.GetRealPrice()==0&&price.GetRealPrice()==0&&keys.Count==2&&keys[0]=="realPriceVIDEO72026-09-29","zero does not cache; original key has no separators before date");
                value=-4;Require(price.GetRealPrice()==-4,"nonzero negative cached");now=now.AddDays(1);value=8;Require(price.GetRealPrice()==-4&&clockReads==3,"nonzero memory price survives date change until explicitly reset");
                price.RealPrice=0;Require(price.GetRealPrice()==8&&keys[3]=="realPriceVIDEO72026-09-30","reset reuses current local date key");
                price.RealPrice=0;price.ClearNextDayEnable=2;Require(price.GetRealPrice()==8&&keys[4]=="realPriceVIDEO7"&&clockReads==4,"only flag exactly1 adds date");
            });
            check("outgame-ad-request-queue-two-pass-priority-dedup",()=>{
                int enumerations=0,reports=0;var a=new RequestCandidate();var b=new RequestCandidate{IsHighPriority=true};var c=new RequestCandidate{IsCacheRequest=true};var d=new RequestCandidate();
                var queue=new OutgameAdRequestQueue(()=>{enumerations++;return new[]{a,c,b,a,b,d};},()=>"INTERSTITAL4",()=>reports++);
                b.Request=()=>Require(queue.Pending.Count==3&&queue.Pending[0]==b&&queue.Pending[1]==a&&queue.Pending[2]==d,"two passes retain source order inside groups and deduplicate");
                Require(queue.TryRequest(false)&&enumerations==2&&reports==1&&queue.Pending.Count==2,"new request rebuilds then handles first then removes");
                Require(queue.TryRequest(true)&&enumerations==2&&reports==1&&queue.Pending[0]==d,"continuation does not rebuild or report");Require(queue.TryRequest(true)&&!queue.TryRequest(true),"exhausted queue falls through to selection");
            });
            check("outgame-ad-request-queue-callback-mutation-and-failure",()=>{
                var first=new RequestCandidate();var next=new RequestCandidate();var queue=new OutgameAdRequestQueue(()=>new[]{first},()=>"VIDEO",()=>throw new Exception("unexpected report"));
                first.Request=()=>{Require(queue.Pending[0]==first,"request remains queued during synchronous callback");queue.Pending=new List<IOutgameAdRequestCandidate>{next,first};};
                Require(queue.TryRequest(false)&&queue.Pending.Count==1&&queue.Pending[0]==next,"removal rereads queue after callback replacement and removes by identity");
                next.Request=()=>throw new InvalidOperationException("native request");bool failed=false;try{queue.TryRequest(true);}catch(InvalidOperationException){failed=true;}Require(failed&&queue.Pending.Count==1,"throwing request retains pending entry");
                int reads=0;var changing=new OutgameAdRequestQueue(()=>++reads==1?new[]{new RequestCandidate{IsHighPriority=true}}:new[]{next},()=>"VIDEO",()=>{});Require(changing.TryRequest(false)&&reads==2&&changing.Pending[0]==next,"second pass obtains a fresh platform list");
            });
            check("outgame-ad-show-flow-request-then-cache-display-order",()=>{
                var env=new ShowEnvironment();var flow=new OutgameAdShowFlow(env){AdType="VIDEO",VideoShowTime=9};var adapter=new ShowCandidate{IsCacheRequest=false};flow.PlatList.Add(adapter);
                adapter.Request=()=>{adapter.IsCacheRequest=true;flow.Cache.Add(adapter);};int shown=0;adapter.Show=()=>{Require(flow.VideoShowTime==-1&&flow.Cache.Contains(adapter),"video clock resets before display; cache retained during callback");shown++;};
                flow.ShowAd();Require(shown==0&&flow.Requests.Pending.Count==0&&flow.Cache.Count==1,"request returns without premature display");flow.ShowAd(true);Require(shown==1&&flow.Cache.Count==0&&env.Calls.Contains("rota-fail"),"continuation reports no platform fallback then still selects cached candidate");
                flow.Cache.Add(adapter);adapter.Show=()=>throw new InvalidOperationException("display");bool failed=false;try{flow.ShowAd(true);}catch(InvalidOperationException){failed=true;}Require(failed&&flow.Cache.Count==1,"display failure prevents removal");
            });
            check("outgame-ad-show-flow-weixin-guarantee-and-platform-preference",()=>{
                var env=new ShowEnvironment();var flow=new OutgameAdShowFlow(env){AdType="VIDEO"};var ordinary=new ShowCandidate{Priority=1};var guarantee=new ShowCandidate{IsGuarantee=true,Priority=2};int shown=0;ordinary.Show=()=>shown=1;guarantee.Show=()=>shown=2;flow.PlatList.Add(ordinary);flow.PlatList.Add(guarantee);flow.Cache.Add(ordinary);
                flow.ShowAd(true,true);Require(shown==2&&flow.Cache.Count==1&&!env.Calls.Contains("rota-fail"),"weixin fallback requires guarantee and preferred platform preserves cache");
                guarantee.Allowed=()=>false;flow.ShowAd(true,true);Require(shown==1&&flow.Cache.Count==0&&env.Calls.Contains("rota-fail"),"blocked guarantee allows cached ordinary candidate despite preference");
                env.Channel="other";shown=0;guarantee.Allowed=()=>true;flow.ShowAd(true);Require(shown==0,"other channels do not invent guarantee fallback");
            });
            check("outgame-ad-show-flow-busy-gates-and-interstitial4-failure",()=>{
                var env=new ShowEnvironment();var flow=new OutgameAdShowFlow(env){AdType="INTERSTITAL4"};var adapter=new ShowCandidate{SourceState=4};flow.PlatList.Add(adapter);int closed=0;flow.CloseCallback=value=>{Require(value,"empty INTERSTITAL4 continuation uses true");closed++;env.Calls.Add("close");};
                flow.ShowAd();Require(env.Calls[0]=="inters-request"&&closed==0,"initial interstitial report precedes busy gate; busy nonvideo has no close callback");flow.PlatList.Clear();flow.ShowAd(true);Require(closed==1&&env.Calls[env.Calls.Count-2]=="close"&&env.Calls[env.Calls.Count-1]=="rota-fail","close(true) precedes request failure report");
                flow.AdType="VIDEO";env.InterVideoShowing=true;flow.CloseCallback=value=>{Require(!value,"busy video fails");closed++;};flow.ShowAd();Require(closed==2&&flow.Requests.Pending.Count==0,"shared busy gate closes video before queue build");
            });
            check("outgame-video-controller-completion-flag-close-and-repeat",()=>{
                var env=new ShowEnvironment();var a=new ShowCandidate();var b=new ShowCandidate();var results=new List<bool>();var controller=new OutgameVideoController(env,()=>new[]{a,b},()=>9,value=>{Require(!value,"close clears shared showing");env.InterVideoShowing=value;});
                controller.Show(null,results.Add);Require(!controller.IsPlayerEnd&&controller.Flow.VideoShowTime==9,"show resets flag and reads clock before base selection");controller.Flow.CloseCallback(true);Require(results.Count==1&&!results[0],"close argument alone cannot imply completed playback");controller.RewardCallback(false);Require(controller.IsPlayerEnd,"source completion wrapper ignores bool parameter");a.SourceState=4;b.SourceState=4;env.InterVideoShowing=true;a.Finishing=()=>Require(env.InterVideoShowing,"finish before shared flag reset");controller.Close();Require(results.Count==3&&results[1]&&results[2]&&a.SourceState==0&&b.SourceState==0&&!env.InterVideoShowing,"each showing adapter finishes and invokes retained close wrapper");
                controller.Show(null,results.Add);controller.Flow.CloseCallback(true);Require(!results[3]&&!controller.IsPlayerEnd,"new show resets completion flag");
            });
            check("outgame-video-controller-close-cleanup-failure-order",()=>{
                var env=new ShowEnvironment();var a=new ShowCandidate();int sharedWrites=0,closed=0;var controller=new OutgameVideoController(env,()=>new[]{a},()=>5,value=>sharedWrites++);controller.Show(null,value=>closed++);a.SourceState=4;a.Finishing=()=>throw new InvalidOperationException("clear");bool failed=false;try{controller.Close();}catch(InvalidOperationException){failed=true;}Require(failed&&a.SourceState==4&&sharedWrites==0&&closed==0,"failed finish prevents shared reset and close callback");
            });
            check("outgame-video-entry-through-restored-controller-to-show-flow",()=>{
                var env=new ShowEnvironment();var runtime=new RewardVideoHost();var router=new OutgameAdRequestRouter(()=>"weixin","",runtime.Log);var adapter=new ShowCandidate();var controller=new OutgameVideoController(env,()=>new[]{adapter},()=>runtime.Realtime,value=>{router.InterVideoShowing=value;env.InterVideoShowing=value;});router.VideoController=controller;controller.Flow.PlatList.Add(adapter);controller.Flow.Cache.Add(adapter);
                var module=new OutgameRewardVideo(new OutgameRewardVideoControllerHost(router,runtime)){Active=true};var entry=new OutgameAdModuleEntry(()=>module,runtime.Warn);var results=new List<bool>();adapter.Show=()=>{adapter.SourceState=4;controller.ShowCallback(true);};
                entry.ShowRewardAd(results.Add);Require(module.Showing&&adapter.SourceState==4&&controller.Flow.Cache.Count==0&&results.Count==0,"entry reaches actual restored controller and show flow; shown callback is not reward");controller.RewardCallback(false);controller.Close();Require(results.Count==1&&results[0]&&!module.Showing&&adapter.SourceState==0,"completion flag passes through restored controller/router/module chain");
                adapter.SourceState=2;controller.Flow.Cache.Add(adapter);entry.ShowRewardAd(results.Add);controller.Close();Require(results.Count==2&&!results[1],"second video early close returns false after flag reset");
            });
            check("outgame-wx-video-load-reuse-autoload-and-delay",()=>{
                var native=new WxLoadHandle();int state=1,success=0,creates=0;System.Collections.IEnumerator routine=null;
                var loading=new OutgameWxVideoLoading((id,multi)=>{Require(id=="adunit-demo"&&!multi,"one configured id disables multiton");creates++;return native;},()=>1,()=>state,()=>success++,()=>{},r=>routine=r,x=>{}){IdValues="demo"};
                loading.StartRequestAd();Require(creates==1&&native.Loads==1,"create then attach callbacks then load");native.Loaded();Require(success==1&&!loading.AutoloadSuccess,"regular load notifies success");state=5;native.Loaded();Require(success==1&&loading.AutoloadSuccess,"closed state load caches automatic success only");loading.StartRequestAd();Require(!loading.AutoloadSuccess&&native.Loads==1&&routine.MoveNext()&&routine.Current is WaitForSeconds,"next request consumes automatic success and schedules scaled delay");Require(!routine.MoveNext()&&success==2,"success after one delay");loading.StartRequestAd();Require(native.Loads==2&&creates==1,"existing uncached native object loads again");
            });
            check("outgame-wx-video-load-error-and-destroy-exceptions",()=>{
                var native=new WxLoadHandle();int failures=0;var loading=new OutgameWxVideoLoading((id,multi)=>{Require(multi,"missing list defaults multiton true");return native;},()=>null,()=>1,()=>{},()=>failures++,r=>{},x=>{});loading.StartRequestAd();native.Failed(new OutgameWxAdError{Code=7,Message=null});Require(failures==1,"request error with null text still notifies failure");
                native.Destroying=()=>throw new InvalidOperationException("native destroy");bool failed=false;try{loading.ClearCache();}catch(InvalidOperationException){failed=true;}Require(failed&&loading.Native==native,"destroy exception retains native reference");native.Destroying=null;loading.ClearCache();Require(loading.Native==null,"successful destroy clears reference");
            });
            check("outgame-ad-notifications-request-listener-reread-and-busy-cleanup",()=>{
                var h=new NotificationHost();var a=new NotifyingAdapter(h){SourceState=1};a.Listener=new OutgameAdListener();a.Listener.Received=x=>{Require(x.SourceState==2&&h.Calls.Count==2,"state and stop/report before listener");a.Cache=false;h.InterVideoShowing=true;};a.Listener.Failed=x=>{Require(x.SourceState==3,"busy cleanup marks failed");h.Calls.Add("failed");};a.Clearing=()=>{Require(a.SourceState==2,"cleanup before failed state");h.Calls.Add("clear");};a.NotifyRequestAdSuccess();Require(string.Join(",",h.Calls)=="stop-load,request,clear,failed","listener mutation affects cache/busy branch");int before=h.Calls.Count;a.NotifyRequestAdFail();Require(h.Calls.Count==before,"duplicate fail state suppressed");a.SourceState=1;a.NotifyRequestAdFail();Require(h.Calls[h.Calls.Count-3]=="stop-load"&&h.Calls[h.Calls.Count-2]=="stop-show","request fail cancels both timers before listener");
            });
            check("outgame-ad-notifications-display-close-and-reward-gates",()=>{
                var h=new NotificationHost();var a=new NotifyingAdapter(h){SourceState=1,AdType="VIDEO",Clicked=true};int shown=0,closed=0,reward=0;a.Listener=new OutgameAdListener{Shown=x=>shown++,Closed=x=>closed++,Rewarded=()=>reward++};a.NotifyShowAd(true,true);Require(h.Calls.Count==0&&a.Clicked,"unloaded show ignored");a.SourceState=2;a.NotifyShowAd(false,true);Require(a.SourceState==4&&!a.Clicked&&shown==1&&string.Join(",",h.Calls)=="stop-show,new-show","false report flag still preserves non-banner new report");a.NotifyShowAd(true,true);a.NotifyCloseAd();a.NotifyCloseAd();Require(shown==1&&closed==1&&a.SourceState==5,"show/close duplicate gates reflect current state");a.NotifyVideoRewarded();a.NotifyVideoRewarded();Require(reward==2&&h.Calls[h.Calls.Count-1]=="video_click_level","reward notification has no state/dedup guard and retains analytics order");
            });
            check("outgame-ad-show-error-special-close-vs-listener-retry",()=>{
                var h=new NotificationHost{PlatformId=458};var a=new NotifyingAdapter(h){SourceState=4,AdType="VIDEO"};int closed=0,retries=0;a.Listener=new OutgameAdListener{Closed=x=>closed++,ShowFailed=x=>{retries++;return true;}};
                a.NotifyShowAdError(1,"problem");Require(a.SourceState==5&&closed==1&&retries==0&&string.Join(",",h.Calls)=="stop-show,new-error,show-fail","platform458 nonnull video error routes through close after both reports");a.NotifyShowAdError(2,"other");Require(closed==1,"state5 suppresses duplicate error");
                a.SourceState=4;a.NotifyShowAdError(1,null);Require(retries==1&&closed==1&&h.Calls[h.Calls.Count-1]=="new-error","null video message bypasses platform special case; retry consumes general failure");a.SourceState=4;a.Listener.ShowFailed=x=>false;a.NotifyShowAdError(1,null);Require(h.Calls[h.Calls.Count-1]=="show-fail","unhandled retry reports failure");
            });
            check("outgame-ad-banner-error-switch-and-click-repeat-report",()=>{
                var h=new NotificationHost();var a=new NotifyingAdapter(h){SourceState=4,AdType="BANNER"};int callbacks=0;a.Listener=new OutgameAdListener{ShowFailed=x=>{callbacks++;return false;}};a.NotifyShowAdError(7,"error");Require(callbacks==1&&string.Join(",",h.Calls)=="stop-show","disabled banner error report still notifies listener");a.SourceState=4;a.BannerErrorReportEnable=1;a.Listener=null;a.NotifyShowAdError(7,"error");Require(h.Calls[h.Calls.Count-1]=="new-error","general failure with absent listener does not reportShowFail");
                h.Calls.Clear();a.AdType="INTERSTITAL4";a.NotifyClickAd();a.NotifyClickAd();Require(string.Join(",",h.Calls)=="click,new-click:False,inters_click_level,new-click:True","repeat clicks still report with repeated flag; primary report and level only once");
            });
            check("outgame-serial-close-callback-before-preload-and-banner",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>1,x=>{});var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);controller.Show(null,result=>{host.Calls.Add("result:"+result);controller.SourceFlag20=true;});lifecycle.OnVideoRewarded();host.Calls.Clear();lifecycle.OnCloseAd();Require(string.Join(",",host.Calls)=="DAU- onCloseAd,result:True,timer-reset,shared:False,stop-video-load,count,load,restore-banner"&&controller.Flow.CacheAmount==3,"business callback mutation suppresses after-close interstitial; preload and banner follow source order");
                host.Calls.Clear();host.Loading=()=>throw new InvalidOperationException("load");bool failed=false;try{lifecycle.OnCloseAd();}catch(InvalidOperationException){failed=true;}Require(failed&&host.Calls[host.Calls.Count-1]=="load","load exception prevents banner restoration");
            });
            check("outgame-controller-show-failure-callback-controls-retry-route",()=>{
                var env=new ShowEnvironment();var candidate=new ShowCandidate{SourceState=0};var controller=new OutgameVideoController(env,()=>new[]{candidate},()=>1,x=>{});var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);int callbacks=0;lifecycle.ShowFailCallback=value=>{Require(value,"source failure callback uses true");callbacks++;};Require(lifecycle.OnShowFail()&&callbacks==1&&host.Calls.Contains("shared:False")&&!host.Calls.Contains("load"),"cache readiness alternative permits original retry even without loaded candidate");
                lifecycle.ShowFailCallback=value=>controller.Flow.AdType="BANNER";host.Calls.Clear();Require(!lifecycle.OnShowFail()&&host.Calls.Contains("timer-reset")&&!host.Calls.Contains("load"),"callback changes type before retry gate; banner closes without reload");
            });
            check("outgame-serial-preload-state-scan-and-remove-before-request",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment());var preload=new OutgameSerialAdPreload(flow,()=>2,r=>r,r=>{},()=>{});var listener=new OutgameAdListener();preload.Listener=listener;var busy=new PreloadCandidate{SourceState=1};var ready=new PreloadCandidate{SourceState=2,Listener=new OutgameAdListener()};var failed=new PreloadCandidate{SourceState=3};preload.RequestList.AddRange(new[]{busy,ready,failed});failed.Request=()=>{Require(!preload.RequestList.Contains(failed)&&busy.Listener==listener&&failed.Listener==listener&&ready.Listener!=listener,"bind missing listeners including skipped candidates; remove before handle");throw new InvalidOperationException("request");};bool threw=false;try{preload.SerialLoad();}catch(InvalidOperationException){threw=true;}Require(threw&&preload.RequestList.Count==2,"failed request remains removed, unlike real-request show queue");
            });
            check("outgame-serial-video-preload-count-reread-delay-and-stop",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment());int reads=0,loads=0,stops=0;System.Collections.IEnumerator routine=null;bool stopFails=false;var preload=new OutgameSerialAdPreload(flow,()=>++reads==1?2:4,r=>{routine=r;return r;},r=>{stops++;if(stopFails)throw new InvalidOperationException("stop");},()=>loads++);preload.VideoShowLoad();Require(flow.CacheAmount==3&&routine.MoveNext()&&routine.Current is WaitForSeconds&&loads==0,"count reread controls cache cap, coroutine waits before load");Require(!routine.MoveNext()&&loads==1&&preload.VideoShowRoutine!=null,"completed coroutine handle retained until explicit stop");stopFails=true;bool threw=false;try{preload.StopVideoShowLoad();}catch(InvalidOperationException){threw=true;}Require(threw&&preload.VideoShowRoutine!=null,"stop failure retains handle");stopFails=false;preload.StopVideoShowLoad();Require(preload.VideoShowRoutine==null&&stops==2,"successful stop clears handle");
            });
            check("outgame-serial-load-entry-rebuild-cap-and-listener-binding",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment()){AdType="VIDEO",CacheAmount=1};var preload=new OutgameSerialAdPreload(flow,()=>3,r=>r,r=>{},()=>{});preload.Listener=new OutgameAdListener();var live=new PreloadCandidate{IsCacheRequest=false};var a=new PreloadCandidate{SourceState=3};var b=new PreloadCandidate{SourceState=0};var calls=new List<string>();int enumerations=0;a.Request=()=>calls.Add("request");preload.Load(()=>{enumerations++;return new[]{live,a,b};},()=>calls.Add("report"),x=>calls.Add(x));Require(enumerations==2&&live.Listener==preload.Listener&&a.Listener==preload.Listener&&b.Listener==null&&preload.IsRequestEnd()&&string.Join(",",calls)=="DAU- load ,report,request","entry binds non-cache then builds one-item video queue and reports before request");
                flow.AdType="BANNER";calls.Clear();preload.Load(()=>new[]{a,b},()=>calls.Add("report"),x=>{});Require(preload.RequestList.Count==1&&preload.RequestList[0]==b&&!calls.Contains("report"),"banner ignores single-cache truncation and skips rota report");
            });
            check("outgame-serial-load-state-filter-and-cache-count",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment()){AdType="VIDEO",CacheAmount=2};var preload=new OutgameSerialAdPreload(flow,()=>0,r=>r,r=>{},()=>{});var all=new List<IOutgameAdPreloadCandidate>();for(int state=0;state<=5;state++)all.Add(new PreloadCandidate{SourceState=state});preload.InitRequestList(all);Require(preload.RequestList.Count==3&&preload.RequestList[0].SourceState==0&&preload.RequestList[1].SourceState==3&&preload.RequestList[2].SourceState==5,"only retryable cache states queued; cacheAmount2 is not an arbitrary truncation limit");Require(preload.GetNowCacheNum(all)==1,"cache count scans state2 independent of request queue");preload.RequestList=null;Require(preload.IsRequestEnd(),"source null queue treated as ended");
            });
            check("outgame-serial-receive-success-cache-callback-and-video-window",()=>{
                var env=new ShowEnvironment();var adapter=new ShowCandidate{PlatformId=883};var controller=new OutgameVideoController(env,()=>new[]{adapter},()=>7,x=>{});controller.Flow.VideoShowTime=7;var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);int requests=0,shown=0;lifecycle.RequestCallback=value=>{Require(value&&controller.Flow.Cache.Contains(adapter),"cache insertion before successful request callback");requests++;};adapter.Show=()=>shown++;lifecycle.OnReceiveAdSuccess(adapter);Require(requests==1&&shown==1&&controller.Flow.VideoShowTime==-1&&host.Calls.IndexOf("check")<host.Calls.IndexOf("video-load:True"),"check before video event; platform883 exact three-second window autoshows");
                controller.Flow.VideoShowTime=6;lifecycle.OnReceiveAdSuccess(adapter);lifecycle.OnReceiveAdSuccess(adapter);Require(shown==1&&controller.Flow.Cache.Count==1&&requests==3,"late load still emits callbacks but no autoshows, cache insertion deduplicated");
            });
            check("outgame-serial-receive-inter4-auto-and-noncache-priority",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>0,x=>{});controller.Flow.AdType="INTERSTITAL4";var host=new SerialLifecycleHost{CanAutoShowInter4=true};var lifecycle=new OutgameSerialControllerLifecycle(controller,host);var adapter=new ShowCandidate{IsCacheRequest=false,Priority=8};int requests=0;lifecycle.RequestCallback=value=>requests++;lifecycle.OnReceiveAdSuccess(adapter);Require(controller.Flow.Selection.LastShowPriority==8&&requests==0&&host.CanAutoShowInter4,"noncache updates priority but no request-success callback or auto4");adapter.IsCacheRequest=true;lifecycle.OnReceiveAdSuccess(adapter);Require(requests==1&&!host.CanAutoShowInter4&&host.Calls[host.Calls.Count-2]=="auto:False"&&host.Calls[host.Calls.Count-1]=="inter4:True","cached inter4 consumes auto flag before show");host.Calls.Clear();lifecycle.OnReceiveAdFailed(adapter);Require(requests==2&&host.Calls[host.Calls.Count-1]=="check","cached failed callback followed by request check");
            });
            check("outgame-serial-show-listener-notification-chain",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>1,x=>{});var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);var adapterHost=new NotificationHost();var adapter=new ListeningShowAdapter(adapterHost){AdType="VIDEO",SourceState=1};adapter.Listener=lifecycle.CreateListener();int shown=0;controller.ShowCallback=value=>{Require(value,"source onShow passes true");shown++;host.Calls.Add("business-shown");};adapter.NotifyRequestAdSuccess();Require(controller.Flow.Cache.Contains(adapter)&&adapter.SourceState==2,"base notification reaches real serial listener and cache");host.Calls.Clear();adapter.NotifyShowAd(true,true);Require(shown==1&&string.Join(",",host.Calls)=="DAU- onShowAd,business-shown,close-banner,timer:1,shared:True,video-preload","adapter notification reaches shown callback before banner, timer, shared flag and preload");
            });
            check("outgame-serial-show-report-delay-and-callback-type-reread",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>0,x=>{});var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);var adapter=new ShowCandidate{IsCacheRequest=false,WaitCloseReportShow=true};controller.Flow.AdType="INTERSTITAL";lifecycle.OnShowAd(adapter);Require(!host.Calls.Contains("platform-back")&&!host.Calls.Contains("rota-success"),"wait-close adapter defers source platform reporting");lifecycle.OnRealShowAd(adapter);Require(host.Calls[host.Calls.Count-1]=="platform-back","real show reports deferred platform result");host.Calls.Clear();controller.ShowCallback=value=>controller.Flow.AdType="BANNER";lifecycle.OnShowAd(adapter);Require(string.Join(",",host.Calls)=="DAU- onShowAd,timer:2","business callback type change respected before banner/shared/preload gates");
            });
            check("outgame-ad-timeout-config-source-defaults-and-invalid-values",()=>{
                Require(OutgameAdTimeouts.GetRotaTimeout(4,"BANNER")==5&&OutgameAdTimeouts.GetRotaTimeout(4,"VIDEO")==60&&OutgameAdTimeouts.GetRotaTimeout(5.9f,"VIDEO")==5,"source low-timeout defaults and truncation");Require(OutgameAdTimeouts.GetRotaTimeout(float.NaN,"VIDEO")==int.MinValue,"WASM nonfinite conversion sentinel");var logs=new List<string>();Require(OutgameAdTimeouts.GetShowTimeout(false,null,"",logs.Add)==5&&logs.Count==0,"absent config defaults silently");Require(OutgameAdTimeouts.GetShowTimeout(true,"bad","",logs.Add)==0&&OutgameAdTimeouts.GetShowTimeout(true,"","",logs.Add)==5&&OutgameAdTimeouts.GetShowTimeout(true,"-2","",logs.Add)==-2,"parse failure produces zero, empty defaults5, negative retained");
            });
            check("outgame-ad-timeout-native-request-and-expiry-order",()=>{
                var h=new NotificationHost();var a=new NotifyingAdapter(h){AdType="VIDEO"};var calls=new List<string>();System.Collections.IEnumerator routine=null;bool interrupted=false;var timer=new OutgameAdTimeouts(a,r=>{routine=r;calls.Add("start");return r;},r=>calls.Add("stop"),()=>calls.Add("clear"),()=>calls.Add("timeout"),()=>interrupted,v=>interrupted=v,()=>a.SourceState.ToString(),x=>{});a.Listener=new OutgameAdListener{Failed=x=>calls.Add("failed"),Closed=x=>calls.Add("closed")};timer.Handle(()=>5,()=>7,()=>calls.Add("report"),()=>calls.Add("native"));Require(a.SourceState==1&&string.Join(",",calls)=="start,report,native"&&routine.MoveNext()&&routine.Current is WaitForSecondsRealtime,"request marks loading before realtime timeout/report/native call");routine.MoveNext();Require(a.SourceState==3&&calls[calls.Count-2]=="clear"&&calls[calls.Count-1]=="failed","load timeout clears before failure notification");
                calls.Clear();a.SourceState=2;timer.ShowAd(()=>7,()=>calls.Add("show"));routine.MoveNext();routine.MoveNext();Require(a.SourceState==5&&string.Join(",",calls)=="start,show,clear,closed,timeout","normal show timeout clears, closes, then reports");calls.Clear();a.SourceState=2;interrupted=true;timer.ShowAd(()=>7,()=>{});routine.MoveNext();routine.MoveNext();Require(!interrupted&&a.SourceState==4&&!calls.Contains("clear")&&!calls.Contains("timeout"),"audio-interrupted VIDEO uses original shown recovery without timeout report");
            });
            check("outgame-wx-video-close-count-reward-and-late-show",()=>{
                var ah=new NotificationHost();var adapter=new NotifyingAdapter(ah){SourceState=2,AdType="VIDEO"};var host=new WxPresentationHost();var native=new WxPresentationHandle();var loading=new OutgameWxVideoLoading((id,multi)=>native,()=>1,()=>adapter.SourceState,adapter.NotifyRequestAdSuccess,adapter.NotifyRequestAdFail,r=>{},x=>{}){Native=native,AutoloadSuccess=true};var presentation=new OutgameWxVideoPresentation(loading,adapter,host);adapter.Listener=new OutgameAdListener{Rewarded=()=>{Require(host.SuccessCount==0&&native.OffCount==1&&!loading.AutoloadSuccess,"unregister and clear autoload before reward; count increments afterward");host.Calls.Add("reward");},Closed=x=>host.Calls.Add("closed")};presentation.StartShowAd();native.Closed(new OutgameWxVideoCloseResult{IsEnded=true});Require(adapter.SourceState==5&&host.PlayCount==1&&host.SuccessCount==1&&string.Join(",",host.Calls)=="reward,prediction:1:1,closed","reward before count/prediction before close");native.Shown(null);Require(adapter.SourceState==5&&host.Calls[host.Calls.Count-1]=="ams-show","late show logs AMS event but cannot revive closed state");
            });
            check("outgame-wx-video-null-partial-and-empty-show-errors",()=>{
                var ah=new NotificationHost();var adapter=new NotifyingAdapter(ah){SourceState=2,AdType="VIDEO"};var host=new WxPresentationHost{MiniGameCommonPlugin=true};var native=new WxPresentationHandle();var loading=new OutgameWxVideoLoading(null,null,null,null,null,null,null){Native=native};var presentation=new OutgameWxVideoPresentation(loading,adapter,host);int rewards=0;adapter.Listener=new OutgameAdListener{Rewarded=()=>rewards++};presentation.StartShowAd();native.ShowFailed(null);native.ShowFailed(new OutgameWxAdError{Message=""});Require(adapter.SourceState==2,"null/empty show failures only log");native.Closed(null);Require(host.PlayCount==1&&host.SuccessCount==0&&rewards==0&&host.Calls.Count==0,"null close counts only, without reward or partial analytics");adapter.SourceState=2;presentation.StartShowAd();native.Closed(new OutgameWxVideoCloseResult{IsEnded=false});Require(host.PlayCount==2&&rewards==0&&host.Calls[0]=="partial-report","partial close respects plugin reporting branch and never rewards");
            });
            check("outgame-wx-adapter-preload-native-controller-reward-chain",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime){IdValues="source-id",Priority=7};
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new[]{adapter},()=>10,x=>{});controller.Flow.VideoShowTime=-1;
                var lifecycleHost=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,lifecycleHost);
                var preload=new OutgameSerialAdPreload(controller.Flow,()=>1,runtime.StartRoutine,runtime.StopRoutine,()=>{}){Listener=lifecycle.CreateListener()};
                preload.Load(()=>new[]{adapter},()=>{},x=>{});
                Require(adapter.SourceState==1&&adapter.Listener==preload.Listener&&runtime.CreatedId=="adunit-source-id"&&!runtime.Multiton&&runtime.Native.Loads==1,"preload binds listener and starts native request on same adapter");
                runtime.Native.Loaded();Require(adapter.SourceState==2&&controller.Flow.Cache.Contains(adapter)&&adapter.Timeouts.LoadRoutine==null&&runtime.Stopped.Count==1,"native load stops real timer and inserts same cache identity");
                int shown=0;bool? result=null;controller.Show(value=>{Require(value,"native shown success");shown++;},value=>result=value);
                Require(adapter.Timeouts.ShowRoutine!=null&&!controller.Flow.Cache.Contains(adapter)&&result==null,"cache consumption starts actual show timeout without inventing reward");
                runtime.Native.Shown(null);Require(shown==1&&adapter.SourceState==4&&adapter.Timeouts.ShowRoutine==null&&runtime.Stopped.Count==2,"native show passes notification and serial listener; cancels actual timer");
                runtime.Native.Closed(new OutgameWxVideoCloseResult{IsEnded=true});
                Require(result==true&&adapter.SourceState==5&&runtime.SuccessCount==1&&runtime.PlayCount==1&&runtime.Calls.Contains("prediction:1:1")&&lifecycleHost.Calls.Contains("load"),"completed native close rewards through controller and reaches reload lifecycle");
                Require(adapter.PlatformId==883&&adapter.GetRealPrice()==17&&!adapter.WaitCloseReportShow,"source platform and cache-price behavior");
            });
            check("outgame-wx-adapter-load-timeout-failure-and-retry",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);int failures=0;
                adapter.Listener=new OutgameAdListener{Failed=x=>{Require(ReferenceEquals(x,adapter)&&x.SourceState==3,"failure uses same adapter after state mutation");failures++;}};
                adapter.Handle();var timeout=runtime.Routines[0];Require(timeout.MoveNext()&&timeout.Current is WaitForSecondsRealtime,"actual preload timer is realtime");timeout.MoveNext();
                Require(failures==1&&runtime.Destroyed==1&&adapter.Loading.Native==null&&adapter.Timeouts.LoadRoutine==null&&runtime.Stopped.Contains(timeout),"timeout destroys native handle and cancellation reaches owned timer");
                adapter.Handle();runtime.Native.Loaded();Require(adapter.SourceState==2&&runtime.Native.Loads==2&&adapter.Timeouts.LoadRoutine==null,"same adapter retries after failure");
                adapter.Finish();Require(adapter.SourceState==0&&adapter.Loading.Native==null&&runtime.Destroyed==2,"controller finish clears owned native cache");
            });
            check("outgame-wx-adapter-autoload-retains-native-and-delays-success",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);int loaded=0;
                adapter.Listener=new OutgameAdListener{Received=x=>loaded++};adapter.Handle();runtime.Native.Loaded();adapter.SourceState=5;runtime.Native.Loaded();
                Require(adapter.Loading.AutoloadSuccess&&loaded==1,"closed-state native load is retained without publishing receive");adapter.Handle();
                Require(adapter.SourceState==1&&!adapter.Loading.AutoloadSuccess&&runtime.Native.Loads==1,"autoload reuse does not call native load again");
                var delay=runtime.Routines[runtime.Routines.Count-1];Require(delay.MoveNext()&&delay.Current is WaitForSeconds,"autoload delivery uses scaled delay");delay.MoveNext();
                Require(loaded==2&&adapter.SourceState==2&&adapter.Timeouts.LoadRoutine==null,"delayed native reuse reaches shared notification state and stops timer");
            });
            check("outgame-video-source-initialization-order-and-defaults",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>0,x=>{});var calls=new List<string>();
                Require(controller.Flow.CacheAmount==1&&controller.Flow.VideoShowTime==-1&&controller.Flow.Selection.LastShowPriority==-1&&controller.Flow.Prefix=="DAU- ","source base constructor defaults");
                controller.Flow.CacheAmount=12;
                controller.InitializeSerial(c=>{Require(c==controller&&c.Flow.CacheAmount==1&&c.Flow.BiddingEnable==0,"cache overwritten before setAdz; bidding applied afterward");calls.Add("set-adz");},7,()=>{Require(controller.Flow.BiddingEnable==7,"global delay read follows bidding");calls.Add("delay");return false;},()=>calls.Add("load"));
                Require(string.Join(",",calls)=="set-adz,delay,load","source video startup ordering");
            });
            check("outgame-video-source-request-delay-skips-immediate-load",()=>{
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new IOutgameVideoCandidate[0],()=>0,x=>{});int loads=0,binds=0;
                controller.InitializeSerial(c=>binds++,1,()=>true,()=>loads++);
                Require(binds==1&&loads==0&&controller.Flow.BiddingEnable==1&&controller.Flow.CacheAmount==1,"delayed global setting still binds but does not immediately load");
                bool threw=false;try{controller.InitializeSerial(c=>throw new InvalidOperationException("bind failed"),9,()=>false,()=>loads++);}catch(InvalidOperationException){threw=true;}
                Require(threw&&loads==0&&controller.Flow.BiddingEnable==1,"binding failure prevents bidding update and load");
            });
            check("outgame-ad-platform-list-normalizes-inters-and-caches",()=>{
                var calls=new List<string>();var accepted=new ShowCandidate();var rejected=new ShowCandidate();
                var list=new OutgameAdPlatformList<int,ShowCandidate>(()=>new[]{1,2,3},()=>"INTERSTITAL4",(key,id)=>{calls.Add(key+":"+id);return id==1?null:id==2?rejected:accepted;},(a,id)=>calls.Add("init:"+id),a=>{calls.Add("can-request");return a==accepted;});
                Require(list.Get().Count==1&&list.Items[0]==accepted&&string.Join(",",calls)=="INTERSTITAL:1,INTERSTITAL:2,init:2,can-request,INTERSTITAL:3,init:3,can-request","source factory skips null and initializes before request eligibility");
                int count=calls.Count;Require(list.Get()==list.Items&&calls.Count==count,"nonempty platform list retains adapter identity without recreation");
            });
            check("outgame-ad-platform-list-empty-retries-and-retains-partial-work",()=>{
                int creates=0;var adapter=new ShowCandidate();
                var list=new OutgameAdPlatformList<int,ShowCandidate>(()=>new[]{1},()=>null,(key,id)=>{Require(key==null,"null zone forwarded unchanged");creates++;return null;},(a,id)=>{},a=>true);
                list.Get();list.Get();Require(creates==2,"empty list retries provider on every read");list.Items=null;list.Get();Require(creates==3&&list.Items!=null,"null list recreated before retry");
                var partial=new OutgameAdPlatformList<int,ShowCandidate>(()=>new[]{1,2},()=>"VIDEO",(key,id)=>{if(id==2)throw new InvalidOperationException("platform factory");return adapter;},(a,id)=>{},a=>true);
                bool threw=false;try{partial.Get();}catch(InvalidOperationException){threw=true;}
                Require(threw&&partial.Items.Count==1&&partial.Get()[0]==adapter,"source preserves prior inserted adapter after factory exception and subsequent reads use nonempty cache");
            });
            check("outgame-wx-adapter-source-init-config-gates-and-defaults",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);
                Require(adapter.LogPrefix=="DAU-WxVideo "&&adapter.Interval==20&&adapter.BannerErrorReportEnable==1&&adapter.CanRequest(),"source ctor and constant canRequest");
                var ids=new OutgameAdIdsInfo{platformId=142,priority=4,idVals="from-source"};
                var zone=new OutgameAdZoneConfig{zkey="VIDEO",customParam="{\"banner_error_report_enable\":\"0\"}"};adapter.Initialize(ids,zone);
                Require(adapter.PlatformId==142&&adapter.NativePlatformId==883&&adapter.BannerErrorReportEnable==1&&adapter.LogPrefix=="DAU-WxVideo 142 ","configured platform differs from native constant; video banner-only config does not parse");
                zone.customParam="{\"ad_bidding_cache_nextday_clear_enable\":\"1\",\"banner_error_report_enable\":\"0\"}";adapter.Initialize(ids,zone);
                Require(adapter.PriceCache.ClearNextDayEnable==1&&adapter.BannerErrorReportEnable==0&&adapter.LogPrefix=="DAU-WxVideo 142 142 ","day-clear key allows parsing both flags and repeated init appends prefix");
                zone.customParam="{\"ad_bidding_cache_nextday_clear_enable\":\"bad\",\"banner_error_report_enable\":\"\"}";adapter.Initialize(ids,zone);
                Require(adapter.PriceCache.ClearNextDayEnable==0&&adapter.BannerErrorReportEnable==0,"failed int parse writes zero; empty field preserves existing flag");
            });
            check("outgame-wx-adapter-retains-source-config-references",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);var ids=new OutgameAdIdsInfo{platformId=883,priority=1,idVals="a"};var zone=new OutgameAdZoneConfig{zkey="VIDEO",rotaTimeout=12,customParam="{}"};adapter.Initialize(ids,zone);
                ids.priority=9;ids.platformId=142;ids.idVals="b";zone.zkey="VIDEO2";
                Require(adapter.Priority==9&&adapter.PlatformId==142&&adapter.AdType=="VIDEO2","source getters observe retained objects");adapter.GetRealPrice();Require(runtime.LastPriceKey=="realPriceVIDEO2142","price key uses current zone and configured platform");
                zone.zkey="VIDEO";adapter.Handle();Require(runtime.CreatedId=="adunit-b","native request reads current id values");runtime.Native.Loaded();
                zone.customParam="{\"video_ad_timeout\":\"13\"}";adapter.ShowAd();var timer=runtime.Routines[runtime.Routines.Count-1];Require(timer.MoveNext()&&((WaitForSecondsRealtime)timer.Current).waitTime==13,"show parses current customParam rather than initialization snapshot");
            });
            check("outgame-wx-adapter-init-banner-and-parse-failure-order",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);var ids=new OutgameAdIdsInfo{platformId=883};var zone=new OutgameAdZoneConfig{zkey="BANNER",customParam="{\"banner_error_report_enable\":\"0\"}"};adapter.Initialize(ids,zone);
                Require(adapter.BannerErrorReportEnable==0,"banner-only flag parses for BANNER");zone.customParam="unrelated invalid json";adapter.Initialize(ids,zone);Require(adapter.BannerErrorReportEnable==0,"unrelated malformed config is skipped by source text gate");
                zone.customParam="{ad_bidding_cache_nextday_clear_enable broken";bool threw=false;try{adapter.Initialize(ids,zone);}catch(ArgumentException){threw=true;}
                Require(threw&&adapter.IdsInfo==ids&&adapter.Zone==zone&&adapter.LogPrefix=="DAU-WxVideo 883 883 883 ","JSON failure follows reference assignment and prefix append; no rollback");
            });
            check("outgame-ad-factory-source-platform-mapping",()=>{
                Func<OutgameAdAdapterKind,string> name=kind=>kind.ToString();
                Require(OutgameAdAdapterFactory.Create("BANNER",424,false,name)=="WxCpsBanner"&&OutgameAdAdapterFactory.Create("BANNER",484,false,name)=="WxGridBanner"&&OutgameAdAdapterFactory.Create("BANNER",883,false,name)=="WxBanner","source banner platforms");
                Require(OutgameAdAdapterFactory.Create("INTERSTITAL4",424,false,name)=="CpsInters"&&OutgameAdAdapterFactory.Create("INTERSTITAL3",425,false,name)=="ApiInters"&&OutgameAdAdapterFactory.Create("INTERSTITAL",883,false,name)=="WxInters","source interstitial platform table");
                Require(OutgameAdAdapterFactory.Create("VIDEO",88399,false,name)=="WxVideo"&&OutgameAdAdapterFactory.Create("VIDEO",88300,false,name)=="WxVideo","extended platform ids use integer division by100");
                foreach(var key in new[]{"NATIVE","NATIVE_BIG","NATIVE_START","NATIVE_OVER","NATIVE_SPLASH"})Require(OutgameAdAdapterFactory.Create(key,883,false,name)=="WxNative","source native zone:"+key);
                Require(OutgameAdAdapterFactory.Create("VIDEO",10000,false,name)==null&&OutgameAdAdapterFactory.Create("VIDEO",142,false,name)==null&&OutgameAdAdapterFactory.Create("VIDEO2",883,false,name)==null,"unsupported platforms and nonexact video zone stay absent");
            });
            check("outgame-ad-factory-first-day-and-cached-inactive-module",()=>{
                var video=new OutgameRewardVideo(new RewardVideoHost()){Active=false};int lookups=0,constructed=0,warnings=0;var entry=new OutgameAdModuleEntry(()=>{lookups++;return video;},x=>warnings++);
                Func<OutgameAdAdapterKind,string> make=kind=>{constructed++;return kind.ToString();};
                Require(entry.CreateAdAdapter("VIDEO",883,make)=="WxVideo"&&warnings==0,"source factory does not use function-active gate");
                video.FirstDayNoAd=true;Require(entry.CreateAdAdapter(null,883,make)==null&&lookups==1&&constructed==1,"cached module rereads first-day flag before zone dereference or construction");
                video.FirstDayNoAd=false;bool threw=false;try{entry.CreateAdAdapter(null,883,make);}catch(NullReferenceException){threw=true;}Require(threw,"null zone follows source Contains failure when not suppressed");
            });
            check("outgame-ad-factory-platform-list-source-init-to-native-request",()=>{
                var runtime=new WxAdapterRuntime();var video=new OutgameRewardVideo(new RewardVideoHost());var entry=new OutgameAdModuleEntry(()=>video,x=>{});
                var zone=new OutgameAdZoneConfig{zkey="VIDEO",customParam="{}"};var ids=new[]{new OutgameAdIdsInfo{platformId=142,idVals="skip"},new OutgameAdIdsInfo{platformId=88301,priority=3,idVals="configured"}};
                int constructed=0;var list=new OutgameAdPlatformList<OutgameAdIdsInfo,OutgameWxVideoAdapter>(()=>ids,()=>zone.zkey,
                    (key,id)=>entry.CreateAdAdapter(key,id.platformId,kind=>{Require(kind==OutgameAdAdapterKind.WxVideo,"source selects WxVideo constructor");constructed++;return new OutgameWxVideoAdapter(runtime);}),
                    (adapter,id)=>adapter.Initialize(id,zone),adapter=>adapter.CanRequest());
                video.FirstDayNoAd=true;Require(list.Get().Count==0&&constructed==0,"first-day gate yields no platform instances");video.FirstDayNoAd=false;
                var adapters=list.Get();Require(adapters.Count==1&&adapters[0].IdsInfo==ids[1]&&adapters[0].PlatformId==88301&&constructed==1,"empty-list retry creates normalized platform but retains original source id");
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>list.Get(),()=>10,x=>{});var lifecycle=new OutgameSerialControllerLifecycle(controller,new SerialLifecycleHost());
                var preload=new OutgameSerialAdPreload(controller.Flow,()=>list.Get().Count,runtime.StartRoutine,runtime.StopRoutine,()=>{}){Listener=lifecycle.CreateListener()};
                controller.InitializeSerial(c=>{},0,()=>false,()=>preload.Load(()=>list.Get(),()=>{},x=>{}));
                Require(runtime.CreatedId=="adunit-configured"&&adapters[0].SourceState==1,"factory-created initialized adapter reaches source serial load");runtime.Native.Loaded();
                Require(controller.Flow.Cache.Contains(adapters[0])&&constructed==1,"native receive caches original adapter without redundant factory calls");
            });
            check("outgame-serial-source-set-adz-cache-clamp-and-reference",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment()){CacheAmount=8};var logs=new List<string>();int made=0;
                var runtime=new WxAdapterRuntime();var binding=new OutgameSerialAdConfiguration(flow,(key,id)=>{made++;return new OutgameWxVideoAdapter(runtime);},(a,id,z)=>((OutgameWxVideoAdapter)a).Initialize(id,z),a=>true,logs.Add);
                var zone=new OutgameAdZoneConfig{zkey="VIDEO",customParam="{}",idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo{platformId=883}}};binding.SetAdz(zone);
                Require(flow.CacheAmount==1&&flow.Prefix=="DAU- VIDEO-SerialController "&&logs[0]=="DAU- VIDEO-Controller adjust cacheAmount:1","base count clamp/log precede serial tag replacement");
                Require(ReferenceEquals(binding.GetPlatforms(),flow.PlatList)&&made==1&&flow.SourceConfig==zone,"configuration and show share exact platform list");
                zone.zkey="VIDEO2";Require(flow.AdType=="VIDEO2","controller observes source configuration mutation");
                var replacement=new OutgameAdZoneConfig{zkey="VIDEO",idsInfo=new List<OutgameAdIdsInfo>()};binding.SetAdz(replacement);
                Require(made==1&&flow.PlatList.Count==1&&((OutgameWxVideoAdapter)flow.PlatList[0]).Zone==zone,"source rebind does not rebuild a nonempty adapter list or overwrite retained adapter config");
            });
            check("outgame-serial-source-set-adz-empty-retry-and-failure-order",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment()){CacheAmount=4};int creates=0;
                var zone=new OutgameAdZoneConfig{zkey="VIDEO",idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo()}};
                var binding=new OutgameSerialAdConfiguration(flow,(key,id)=>{creates++;return null;},(a,id,z)=>{},a=>true,x=>{});binding.SetAdz(zone);
                Require(creates==3&&flow.CacheAmount==4,"source rereads empty platform list three times; zero adapters do not clamp cache to zero");
                var failedFlow=new OutgameAdShowFlow(new ShowEnvironment());var failed=new OutgameSerialAdConfiguration(failedFlow,(key,id)=>throw new InvalidOperationException("factory"),(a,id,z)=>{},a=>true,x=>{});
                bool threw=false;try{failed.SetAdz(zone);}catch(InvalidOperationException){threw=true;}
                Require(threw&&failed.Zone==zone&&failedFlow.Prefix=="DAU- VIDEO-Controller ","factory failure retains base config/tag before serial suffix replacement");
            });
            check("outgame-serial-platform-list-reentrant-replacement",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment());var original=flow.PlatList;var replacement=new List<IOutgameAdShowCandidate>();var candidate=new ShowCandidate();
                var binding=new OutgameSerialAdConfiguration(flow,(key,id)=>{flow.PlatList=replacement;return candidate;},(a,id,z)=>{},a=>true,x=>{});
                binding.SetAdz(new OutgameAdZoneConfig{zkey="VIDEO",idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo()}});
                Require(original.Count==0&&replacement.Count==1&&replacement[0]==candidate&&ReferenceEquals(binding.GetPlatforms(),replacement),"source appends to current field after factory callback replaces list");
            });
            check("outgame-ad-request-completion-source-cache-and-reload",()=>{
                var env=new ShowEnvironment();var candidate=new ShowCandidate{SourceState=4};var flow=new OutgameAdShowFlow(env,()=>new[]{candidate}){SourceConfig=new OutgameAdZoneConfig{zkey="VIDEO",reqInterTime=22.9f},CacheAmount=2};int count=1;var calls=new List<string>();var routines=new List<System.Collections.IEnumerator>();
                var completion=new OutgameAdRequestCompletion(flow,()=>count,()=>calls.Add("success"),()=>calls.Add("fail"),v=>calls.Add("video:"+v),()=>calls.Add("load"),r=>{routines.Add(r);return r;},r=>calls.Add("stop"),x=>{});
                completion.CheckRequest();Require(string.Join(",",calls)=="success"&&routines.Count==0,"showing counts toward full capacity and suppresses reload");
                candidate.SourceState=0;completion.CheckRequest();Require(calls[calls.Count-1]=="success"&&routines.Count==1,"partial nonzero cache reports success and schedules reload");
                var routine=routines[0];Require(routine.MoveNext()&&((WaitForSecondsRealtime)routine.Current).waitTime==22,"reload truncates float and uses realtime wait");routine.MoveNext();Require(calls[calls.Count-1]=="load"&&completion.ReloadRoutine==routine,"completed source coroutine handle remains until explicit stop");
                count=0;completion.CheckRequest();Require(string.Join(",",calls).EndsWith("fail,video:False,stop")&&routines.Count==2,"zero cache reports failure/event before replacing previous reload");
                completion.StopReload();Require(completion.ReloadRoutine==null,"explicit stop clears reload handle");
            });
            check("outgame-ad-request-completion-banner-delay-and-reentrant-type",()=>{
                var flow=new OutgameAdShowFlow(new ShowEnvironment()){SourceConfig=new OutgameAdZoneConfig{zkey="BANNER",banRefreshTime=31,reqInterTime=45}};int reports=0;System.Collections.IEnumerator routine=null;
                var completion=new OutgameAdRequestCompletion(flow,()=>0,()=>reports++,()=>reports++,v=>reports++,()=>{},r=>{routine=r;return r;},r=>{},x=>{});
                completion.CheckRequest();Require(reports==0&&routine.MoveNext()&&((WaitForSecondsRealtime)routine.Current).waitTime==31,"banner skips report/event and uses banner refresh delay");
                Require(OutgameAdRequestCompletion.GetReloadDelay(float.NaN)==15&&OutgameAdRequestCompletion.GetReloadDelay(float.PositiveInfinity)==15&&OutgameAdRequestCompletion.GetReloadDelay(-9)==15&&OutgameAdRequestCompletion.GetReloadDelay(15.9f)==15,"WASM conversion and minimum15 preserved");
                flow.SourceConfig.zkey="VIDEO";var calls=new List<string>();completion=new OutgameAdRequestCompletion(flow,()=>0,()=>{},()=>{calls.Add("fail");flow.SourceConfig.zkey="BANNER";},v=>calls.Add("event"),()=>{},r=>{routine=r;return r;},r=>{},x=>{});completion.CheckRequest();
                Require(string.Join(",",calls)=="fail"&&routine.MoveNext()&&((WaitForSecondsRealtime)routine.Current).waitTime==31,"failed report callback type mutation is reread for video event and reload selection");
            });
            check("outgame-serial-native-failure-automatically-schedules-retry",()=>{
                var runtime=new WxAdapterRuntime();var adapter=new OutgameWxVideoAdapter(runtime);var zone=new OutgameAdZoneConfig{zkey="VIDEO",reqInterTime=18,customParam="{}"};adapter.Initialize(new OutgameAdIdsInfo{platformId=883,idVals="retry"},zone);
                var env=new ShowEnvironment();var controller=new OutgameVideoController(env,()=>new[]{adapter},()=>10,x=>{});controller.Flow.SourceConfig=zone;
                var host=new SerialLifecycleHost();var lifecycle=new OutgameSerialControllerLifecycle(controller,host);var preload=new OutgameSerialAdPreload(controller.Flow,()=>1,runtime.StartRoutine,runtime.StopRoutine,()=>{}){Listener=lifecycle.CreateListener()};
                Action load=()=>preload.Load(()=>new[]{adapter},()=>{},x=>{});int failedReports=0,falseEvents=0;
                var completion=new OutgameAdRequestCompletion(controller.Flow,()=>preload.GetNowCacheNum(new[]{adapter}),()=>{},()=>failedReports++,v=>{if(!v)falseEvents++;},load,runtime.StartRoutine,runtime.StopRoutine,x=>{});
                host.Checking=()=>preload.CheckRequest(()=>{},completion,x=>{});load();runtime.Native.Failed(new OutgameWxAdError{Code=1,Message="network"});
                Require(adapter.SourceState==3&&failedReports==1&&falseEvents==1&&completion.ReloadRoutine!=null,"native failure automatically traverses serial check into source retry scheduling");
                var retry=(System.Collections.IEnumerator)completion.ReloadRoutine;Require(retry.MoveNext()&&((WaitForSecondsRealtime)retry.Current).waitTime==18,"retry uses source config");retry.MoveNext();
                Require(adapter.SourceState==1&&runtime.Native.Loads==2,"retry timer invokes actual serial load on failed candidate");runtime.Native.Loaded();Require(adapter.SourceState==2&&controller.Flow.Cache.Contains(adapter),"retried native load reaches cache");
            });
            check("outgame-serial-video-session-native-reward-and-auto-reload",()=>{
                var runtime=new WxAdapterRuntime();var host=new SerialLifecycleHost();var module=new OutgameRewardVideo(new RewardVideoHost());
                var session=new OutgameSerialVideoSession(new ShowEnvironment(),host,new OutgameAdModuleEntry(()=>module,x=>{}),()=>runtime,runtime.StartRoutine,runtime.StopRoutine,()=>{});
                var zone=new OutgameAdZoneConfig{zkey="VIDEO",customParam="{}",idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo{platformId=883,idVals="session"}}};
                session.Initialize(zone,0,()=>false);Require(runtime.Native.Loads==1&&session.Controller.Flow.Prefix=="DAU- VIDEO-SerialController ","session binds config and starts source preload without host glue");runtime.Native.Loaded();
                bool? reward=null;int shown=0;session.Controller.Show(v=>shown++,v=>reward=v);runtime.Native.Shown(null);runtime.Native.Closed(new OutgameWxVideoCloseResult{IsEnded=true});
                var adapter=(OutgameWxVideoAdapter)session.Configuration.GetPlatforms()[0];
                Require(shown==1&&reward==true&&runtime.Native.Loads==2&&adapter.SourceState==1&&host.Calls.Contains("restore-banner"),"native completed close rewards then automatically reloads and restores banner");
                Require(!host.Calls.Contains("load")&&!host.Calls.Contains("check"),"internal lifecycle owns load/check instead of delegating them to stub host");
                runtime.Native.Loaded();session.Controller.Show(null,v=>reward=v);runtime.Native.Shown(null);runtime.Native.Closed(new OutgameWxVideoCloseResult{IsEnded=false});
                Require(reward==false&&runtime.SuccessCount==1&&runtime.PlayCount==2&&runtime.Native.Loads==3,"partial second watch does not inherit prior reward and still reloads");
            });
            check("outgame-serial-video-session-native-failure-retry",()=>{
                var runtime=new WxAdapterRuntime();var host=new SerialLifecycleHost();var module=new OutgameRewardVideo(new RewardVideoHost());
                var session=new OutgameSerialVideoSession(new ShowEnvironment(),host,new OutgameAdModuleEntry(()=>module,x=>{}),()=>runtime,runtime.StartRoutine,runtime.StopRoutine,()=>{});
                session.Initialize(new OutgameAdZoneConfig{zkey="VIDEO",customParam="{}",reqInterTime=19,idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo{platformId=883}}},0,()=>true);
                Require(runtime.Native.Loads==0,"session respects delayed initial request");session.Load();runtime.Native.Failed(new OutgameWxAdError{Message="failed"});
                var retry=(System.Collections.IEnumerator)session.Completion.ReloadRoutine;Require(retry!=null&&host.Calls.Contains("video-load:False"),"native failure automatically schedules source retry through session");retry.MoveNext();Require(((WaitForSecondsRealtime)retry.Current).waitTime==19,"source retry interval");retry.MoveNext();runtime.Native.Loaded();
                Require(runtime.Native.Loads==2&&session.Controller.Flow.Cache.Count==1,"retry callback rebuilds request queue and successful load enters shared cache");
            });
            check("outgame-serial-video-session-multiple-adapters-preload-during-show",()=>{
                var first=new WxAdapterRuntime();var second=new WxAdapterRuntime();int created=0;var module=new OutgameRewardVideo(new RewardVideoHost());
                var session=new OutgameSerialVideoSession(new ShowEnvironment(),new SerialLifecycleHost(),new OutgameAdModuleEntry(()=>module,x=>{}),()=>created++==0?first:second,first.StartRoutine,first.StopRoutine,()=>{});
                session.Initialize(new OutgameAdZoneConfig{zkey="VIDEO",customParam="{}",idsInfo=new List<OutgameAdIdsInfo>{new OutgameAdIdsInfo{platformId=883,priority=1},new OutgameAdIdsInfo{platformId=883,priority=2}}},0,()=>false);
                Require(first.Native.Loads==1&&second.Native.Loads==0,"initial source cache1 requests only first adapter");first.Native.Loaded();session.Controller.Show(null,v=>{});first.Native.Shown(null);
                var during=(System.Collections.IEnumerator)session.Preload.VideoShowRoutine;Require(during!=null&&during.MoveNext()&&during.Current is WaitForSeconds,"multiple adapters schedule scaled show-preload");during.MoveNext();Require(second.Native.Loads==1,"show-preload requests idle second adapter");second.Native.Loaded();first.Native.Closed(new OutgameWxVideoCloseResult{IsEnded=true});
                Require(session.Preload.VideoShowRoutine==null&&session.Controller.Flow.CacheAmount==2&&first.Native.Loads==2,"close cancels show-preload and restores capacity before reload");first.Native.Loaded();Require(session.Controller.Flow.Cache.Count==2&&created==2,"two source adapter identities survive display and refill cycle");
            });
            check("outgame-store-source-new-data-and-wire-roundtrip",()=>{
                var prior=OutgameStoreSettings.Resolve;
                try{
                    var settings=JsonUtility.FromJson<OutgameStoreSettings>(Resources.Load<TextAsset>("Data/SettingConfig").text);OutgameStoreSettings.Resolve=()=>settings;
                    var h=new DataStorageHost();var sdk=new OutgameSdkStringStorage(new StorageBackend(),x=>{});var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                    var manager=new OutgameStoreDataManager(new OutgameDataManagerStorage(()=>"StoreDataManager",h,sdk,versions),h,x=>{});manager.OnInit();
                    Require(OutgameStoreDataManager.Instance==manager&&manager.Data.coinData.count==5&&manager.Data.diamondData.count==3&&manager.Data.diamondData.MaxCount==3&&manager.Data.diamondData.CurrentReward==20,"source settings initialize original manager defaults");
                    manager.Data.coinData.count=2;manager.Data.diamondData.count=1;manager.Data.diamondData.lastRewardTime=1234567890123L;
                    string text=JsonUtility.ToJson(manager.Data);Require(!text.Contains("MaxCount")&&!text.Contains("numArray")&&text.Contains("1234567890123"),"only original public wire fields serialized with Int64 time");manager.UpdateDataCallBack(text);
                    Require(manager.Data.coinData.count==2&&manager.Data.diamondData.count==1&&manager.Data.diamondData.lastRewardTime==1234567890123L,"held data roundtrip does not reset counts");manager.OnRelease();Require(OutgameStoreDataManager.Instance==null,"release clears source singleton");
                }finally{OutgameStoreSettings.Resolve=prior;}
            });
            check("outgame-store-source-empty-decode-and-server-routing",()=>{
                var prior=OutgameStoreSettings.Resolve;
                try{
                    int reads=0;OutgameStoreSettings.Resolve=()=>{reads++;return new OutgameStoreSettings{storeAdDiamondCount=3,storeAdGoldCount=5,storeAdSendDiamond=new[]{7,8,9}};};
                    var h=new DataStorageHost{IsUseServer=true};var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});var requests=new List<string>();var manager=new OutgameStoreDataManager(new OutgameDataManagerStorage(()=>"StoreDataManager",h,new OutgameSdkStringStorage(new StorageBackend(),x=>{}),versions),h,key=>{Require(OutgameStoreDataManager.Instance!=null,"publish before download request");requests.Add(key);});manager.OnInit();
                    Require(requests.Count==1&&manager.Data==null&&reads==0,"server path defers data construction until callback");manager.UpdateDataCallBack("");Require(reads==6&&manager.Data.diamondData.CurrentReward==9,"empty source input creates twice, each reads max/rewards/gold in order");
                    manager.UpdateDataCallBack("{\"coinData\":{\"count\":2},\"diamondData\":{\"count\":1,\"lastRewardTime\":7,\"numArray\":[999],\"MaxCount\":99}}");
                    Require(manager.Data.diamondData.CurrentReward==20&&manager.Data.diamondData.MaxCount==3,"nonserialized/private fields are constructed from source runtime defaults, not injected JSON fields");
                    manager.Data.diamondData.count=0;Require(!manager.Data.diamondData.CanFree,"zero free count gate");bool threw=false;try{int reward=manager.Data.diamondData.CurrentReward;}catch(IndexOutOfRangeException){threw=true;}Require(threw,"source reward accessor does not clamp exhausted count");manager.OnRelease();
                }finally{OutgameStoreSettings.Resolve=prior;}
            });
            check("outgame-store-original-registration-exclusion-and-file-restart",()=>{
                var prior=OutgameStoreSettings.Resolve;
                try{
                    OutgameStoreSettings.Resolve=()=>JsonUtility.FromJson<OutgameStoreSettings>(Resources.Load<TextAsset>("Data/SettingConfig").text);
                    string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"store");var work=new Queue<Action>();var h=new DataStorageHost();var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},x=>{},x=>{});
                    var sdk=new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>throw new Exception(x));var manager=new OutgameStoreDataManager(new OutgameDataManagerStorage(()=>"StoreDataManager",h,sdk,versions),h,x=>throw new Exception("unexpected download"));
                    var entries=new List<OutgameManagerRegistration>();foreach(var entry in OutgameManagerRegistrationCatalog.Read(Resources.Load<TextAsset>("Data/OutgameManagerRegistration").text,id=>{Require(id==4092,"source store type identity");return manager;}))if(entry.SourceTypeIndex==4092)entries.Add(entry);
                    var pool=new OutgameDataManagerPool(()=>{},x=>{},x=>{},x=>throw new Exception(x));pool.OnInit(true,"Proj_hdzd",entries);Require(entries.Count==0&&!pool.Managers.ContainsKey(4092),"source store lacks registration attribute; do not activate automatically");manager.OnInit();
                    manager.Data.coinData.count=3;manager.Data.diamondData.count=2;manager.Data.diamondData.lastRewardTime=999;manager.OnSave();Require(work.Count==1,"store queues isolated persisted save");work.Dequeue()();
                    var restarted=new OutgameStoreDataManager(new OutgameDataManagerStorage(()=>"StoreDataManager",h,new OutgameSdkStringStorage(new OutgameFileStorageBackend(root,work.Enqueue),x=>{}),versions),h,x=>{});restarted.OnInit();
                    Require(restarted.Data.coinData.count==3&&restarted.Data.diamondData.count==2&&restarted.Data.diamondData.lastRewardTime==999,"fresh manager restores actual isolated file without grants/reset");restarted.OnRelease();
                }finally{OutgameStoreSettings.Resolve=prior;}
            });
            check("outgame-item-original-wire-preserves-int64-and-product-counters",()=>{
                const string text="{\"saveTimestamp\":9223372036854775806,\"lastExitDayOfYear\":271,\"itemUserDatas\":[{\"itemId\":1,\"itemCount\":4294967297,\"holdTime\":17}],\"productUserDatas\":[{\"m_uniqueId\":4,\"productId\":8,\"haveBuyTimes\":2,\"buyCount\":5,\"HaveBuyCounts\":[{\"key\":2,\"buyCount\":[{\"key\":1,\"value\":3}]},{\"key\":2,\"buyCount\":[{\"key\":1,\"value\":7}]}],\"lastResetTime\":11,\"nextRefreshTimeStamp\":1234567890123,\"consumeItemPrice\":4294967298,\"consumeItemPriceArray\":[4294967299,-1]}]}";
                var record=OutgameItemManagerData.ReadOriginal(text);var restored=OutgameItemManagerData.ReadOriginal(record.SerializeRecord());var product=restored.productUserDatas[0];
                Require(restored.saveTimestamp==9223372036854775806L&&restored.itemUserDatas[0].itemCount==4294967297L&&product.consumeItemPrice==4294967298L&&product.consumeItemPriceArray[0]==4294967299L,"original 64-bit fields survive without Int32 truncation");
                Require(product.HaveBuyCounts.Count==2&&product.HaveBuyCounts[0].buyCount[0].value==3&&product.HaveBuyCounts[1].buyCount[0].value==7&&product.nextRefreshTimeStamp==1234567890123L&&product.lastResetTime==11,"source record retains duplicate keyed rows, order and timestamps before runtime consumers");
                Require(product.m_uniqueId==4&&product.productId==8&&product.haveBuyTimes==2&&product.buyCount==5&&restored.itemUserDatas[0].holdTime==17,"independent product and item fields retained");
            });
            check("outgame-item-original-empty-versus-held-default-record",()=>{
                var empty=OutgameItemManagerData.ReadOriginal("");Require(empty.saveTimestamp==-1&&empty.lastExitDayOfYear==-1&&empty.itemUserDatas.Count==0&&empty.productUserDatas.Count==0,"null decoded source record gets sentinel times and fresh lists");
                var held=OutgameItemManagerData.ReadOriginal("{}");Require(held.saveTimestamp==0&&held.lastExitDayOfYear==0,"existing empty object does not receive new-player sentinel repairs");
                var product=new OutgameProductUserData();Require(product.HaveBuyCounts.Count==0&&product.consumeItemPriceArray==null,"source product ctor initializes buy-count list only");
                var zero=new OutgameItemManagerData();Require(zero.saveTimestamp==0&&zero.lastExitDayOfYear==0&&zero.itemUserDatas.Count==0,"record constructor and manager missing-record handling stay distinct");
            });
            check("outgame-global-item-index-filter-identity-and-shallow-snapshot",()=>{
                var data=new OutgameItemManagerData();var item=new OutgameItemUserData{itemId=1,itemCount=4294967297L,holdTime=4};data.itemUserDatas.Add(item);data.itemUserDatas.Add(new OutgameItemUserData{itemId=99});
                var product=new OutgameProductUserData{m_uniqueId=17,productId=2,buyCount=3,consumeItemPriceArray=new long[]{7}};product.HaveBuyCounts.Add(new OutgameProductBuyCount{key=1,buyCount=new List<OutgameItemGetTypeBuyCount>{new OutgameItemGetTypeBuyCount{key=2,value=3}}});data.productUserDatas.Add(product);data.productUserDatas.Add(new OutgameProductUserData{productId=99});
                var indexes=new OutgameGlobalItemIndexes();indexes.InitializeProducts(data,id=>id==2);indexes.InitializeItems(data,id=>id==1);
                Require(indexes.Items.Count==1&&indexes.Products.Count==1&&indexes.Items[1]==item&&indexes.Products[17]==product,"only configured held rows accepted, preserving original live records");
                Require(indexes.ItemSnapshot[1]!=item&&indexes.ItemSnapshot[1].itemCount==4294967297L&&indexes.ProductSnapshot[17]!=product&&ReferenceEquals(indexes.ProductSnapshot[17].HaveBuyCounts,product.HaveBuyCounts)&&ReferenceEquals(indexes.ProductSnapshot[17].consumeItemPriceArray,product.consumeItemPriceArray),"source memberwise clones preserve nested reference aliasing");
                Require(data.itemUserDatas.Count==2&&data.productUserDatas.Count==2&&indexes.IsDirty,"index construction does not filter original serialized lists in place");
            });
            check("outgame-global-item-index-duplicate-failure-retains-partial-state",()=>{
                var data=new OutgameItemManagerData();data.itemUserDatas.Add(new OutgameItemUserData{itemId=3,itemCount=1});data.itemUserDatas.Add(new OutgameItemUserData{itemId=3,itemCount=9});var indexes=new OutgameGlobalItemIndexes();indexes.Items.Add(99,new OutgameItemUserData());var dictionary=indexes.Items;var oldSnapshot=indexes.ItemSnapshot;
                bool threw=false;try{indexes.InitializeItems(data,id=>true);}catch(ArgumentException){threw=true;}
                Require(threw&&ReferenceEquals(dictionary,indexes.Items)&&indexes.Items.Count==1&&indexes.Items[3].itemCount==1&&!ReferenceEquals(oldSnapshot,indexes.ItemSnapshot)&&indexes.ItemSnapshot.ContainsKey(3),"source Clear retains live dictionary, replaces snapshot then Add duplicate throws after first row");
                var p=new OutgameProductUserData();int uid=p.UID;Require(p.m_uniqueId==uid&&p.UID==uid,"zero product UID lazily caches source object hash");
                data.productUserDatas.Add(new OutgameProductUserData{m_uniqueId=7,productId=1});data.productUserDatas.Add(new OutgameProductUserData{m_uniqueId=7,productId=2});threw=false;try{indexes.InitializeProducts(data,id=>true);}catch(ArgumentException){threw=true;}Require(threw&&indexes.Products.Count==1&&indexes.Products[7].productId==1,"duplicate unique id is not merged by product id");
            });
            check("outgame-global-item-snapshot-update-source-field-selection",()=>{
                var indexes=new OutgameGlobalItemIndexes();indexes.InitializeProducts(new OutgameItemManagerData(),id=>true);var product=new OutgameProductUserData{m_uniqueId=1,buyCount=2,haveBuyTimes=3,lastResetTime=4,nextRefreshTimeStamp=5,consumeItemPrice=6};indexes.Products.Add(1,product);indexes.SnapshotProduct(1);var snapshot=indexes.ProductSnapshot[1];
                product.buyCount=12;product.haveBuyTimes=13;product.lastResetTime=14;product.nextRefreshTimeStamp=15;product.consumeItemPrice=16;product.HaveBuyCounts=new List<OutgameProductBuyCount>();product.consumeItemPriceArray=new long[]{99};indexes.SnapshotProduct(1);
                Require(snapshot.buyCount==12&&snapshot.lastResetTime==14&&snapshot.HaveBuyCounts==product.HaveBuyCounts&&snapshot.consumeItemPriceArray==product.consumeItemPriceArray,"source selected snapshot fields refresh");
                Require(snapshot.haveBuyTimes==3&&product.haveBuyTimes==13&&snapshot.nextRefreshTimeStamp==5&&snapshot.consumeItemPrice==6,"source self-write and omitted timestamp/price updates preserved");indexes.IsDirty=false;indexes.SnapshotItem(999);Require(indexes.IsDirty,"missing id still marks source dirty");
            });
            check("outgame-global-item-save-rebuilds-live-lists-before-time",()=>{
                var indexes=new OutgameGlobalItemIndexes();indexes.InitializeItems(new OutgameItemManagerData(),id=>true);var item=new OutgameItemUserData{itemId=2,itemCount=13};var product=new OutgameProductUserData{m_uniqueId=7,productId=4,buyCount=5};indexes.Items.Add(2,item);indexes.Products.Add(7,product);indexes.SnapshotItem(2);item.itemCount=19;
                var data=new OutgameItemManagerData{lastExitDayOfYear=123};data.itemUserDatas.Add(new OutgameItemUserData{itemId=999});var oldItems=data.itemUserDatas;var oldProducts=data.productUserDatas;int calls=0;
                var clock=new OutgameGlobalItemClock(()=>throw new Exception("host present"));clock.HostNow=()=>{Require(data.itemUserDatas.Count==1&&data.itemUserDatas[0]==item&&data.productUserDatas[0]==product,"source lists rebuilt before provider time callbacks");return new DateTime(2026,1,1).AddSeconds(++calls);};
                var save=new OutgameGlobalItemPersistence(indexes,clock,date=>date.Second){Data=data};var result=OutgameItemManagerData.ReadOriginal(save.Save());
                Require(calls==2&&result.saveTimestamp==2&&result.itemUserDatas[0].itemCount==19&&result.lastExitDayOfYear==123,"second host time and live record persisted without day mutation");
                Require(ReferenceEquals(oldItems,data.itemUserDatas)&&ReferenceEquals(oldProducts,data.productUserDatas)&&indexes.ItemSnapshot[2].itemCount==13&&indexes.IsDirty,"source lists retain identity, snapshots/dirty state unchanged");
            });
            check("outgame-global-item-save-failure-and-reentrant-time-state",()=>{
                var indexes=new OutgameGlobalItemIndexes();indexes.Products.Add(1,new OutgameProductUserData{m_uniqueId=1});var old=new OutgameItemManagerData{saveTimestamp=7};old.itemUserDatas=null;var clock=new OutgameGlobalItemClock(()=>DateTime.MinValue);int times=0;
                var save=new OutgameGlobalItemPersistence(indexes,clock,date=>{times++;return 42;}){Data=old};bool threw=false;try{save.Save();}catch(NullReferenceException){threw=true;}Require(threw&&old.productUserDatas.Count==1&&old.saveTimestamp==7&&times==0,"product list already rebuilt when null item list aborts before time");
                old.itemUserDatas=new List<OutgameItemUserData>();var replacement=new OutgameItemManagerData{saveTimestamp=99};clock.HostNow=()=>{save.Data=replacement;return DateTime.MinValue;};var result=OutgameItemManagerData.ReadOriginal(save.Save());
                Require(old.saveTimestamp==42&&replacement.saveTimestamp==99&&result.saveTimestamp==99,"timestamp destination captured before clock callback, serialization rereads current Data");
            });
            check("outgame-global-item-clock-host-replacement-and-local-fallback",()=>{
                int local=0;var clock=new OutgameGlobalItemClock(()=>{local++;return new DateTime(2026,1,1);});Require(clock.GetNow().Year==2026&&local==1,"missing host uses one local time read");
                clock.HostNow=()=>{clock.HostNow=()=>new DateTime(2027,1,1);return DateTime.MinValue;};Require(clock.GetNow().Year==2027&&local==1,"second source host call rereads replaced provider");
            });
            check("outgame-item-time-original-eight-hour-epoch-and-milliseconds",()=>{
                var epoch=new DateTime(1970,1,1,8,0,0);Require(OutgameItemTimestamp.FromDateTime(epoch)==0&&OutgameItemTimestamp.FromDateTime(new DateTime(1970,1,1))==-28800000L,"source epoch is 08:00, not midnight");
                Require(OutgameItemTimestamp.FromDateTime(epoch.AddTicks(19999))==1&&OutgameItemTimestamp.FromDateTime(epoch.AddTicks(-19999))==-1,"millisecond conversion truncates toward zero");
                Require(OutgameItemTimestamp.FromDateTime(DateTime.SpecifyKind(epoch,DateTimeKind.Utc))==0&&OutgameItemTimestamp.FromDateTime(DateTime.SpecifyKind(epoch,DateTimeKind.Local))==0,"source subtraction does not normalize DateTime kind");
                long stamp=1234567890123L;var date=OutgameItemTimestamp.ToDateTime(stamp);Require(date.Kind==DateTimeKind.Unspecified&&OutgameItemTimestamp.FromDateTime(date)==stamp,"64-bit milliseconds round trip through source epoch");
                var data=new OutgameItemManagerData();var save=new OutgameGlobalItemPersistence(new OutgameGlobalItemIndexes(),new OutgameGlobalItemClock(()=>epoch.AddMilliseconds(1234))){Data=data};Require(OutgameItemManagerData.ReadOriginal(save.Save()).saveTimestamp==1234,"default persistence uses original converter");
            });
            check("outgame-global-item-lifecycle-init-order-and-reinitialize",()=>{
                var events=new List<string>();var h=new ItemLifecycleHost{Events=events};OutgameGlobalItemLifecycle manager=null;
                manager=new OutgameGlobalItemLifecycle(h,()=>events.Add("tick"),id=>{events.Add("product");Require(manager.Data.saveTimestamp==2&&manager.UpdateHandle==0,"time and zero-valued handle precede indexes");return true;},id=>{events.Add("item");return true;},()=>throw new Exception("host expected"));
                h.OnRegister=()=>Require(manager.Indexes.IsDirty&&manager.GetItemCount(3)==4294967297L,"registration follows both indexes and dirty flag");
                int times=0;manager.Initialize("{\"productUserDatas\":[{\"m_uniqueId\":4,\"productId\":2}],\"itemUserDatas\":[{\"itemId\":3,\"itemCount\":4294967297}]}",()=>OutgameItemTimestamp.Epoch.AddMilliseconds(++times));
                Require(string.Join(",",events)=="add,product,item,register"&&h.Message==OutgameGlobalItemLifecycle.StatisticsRegistration&&h.EventId==10020,"original init order and statistics event id");
                Require(h.Query(null)==0&&h.Query(new object[0])==0&&h.Query(new object[]{3})==4294967297L&&h.Query(new object[]{99})==0,"statistics returns live Int64 count or zero");
                bool threw=false;try{h.Query(new object[]{3L});}catch(InvalidCastException){threw=true;}Require(threw,"source unbox requires Int32 argument");
                h.OnRegister=null;events.Clear();manager.Initialize("",()=>OutgameItemTimestamp.Epoch.AddMilliseconds(8));Require(string.Join(",",events)=="register"&&manager.UpdateHandle==0&&manager.Data.lastExitDayOfYear==-1&&manager.GetItemCount(3)==0,"reinitialize rebuilds data but does not register another update");
                h.Update();Require(events[1]=="tick","registered source update callback preserved");
            });
            check("outgame-global-item-lifecycle-init-failure-retains-source-stage",()=>{
                var events=new List<string>();var h=new ItemLifecycleHost{Events=events};var manager=new OutgameGlobalItemLifecycle(h,()=>{},id=>true,id=>true,()=>OutgameItemTimestamp.Epoch);
                bool threw=false;try{manager.Initialize("{\"productUserDatas\":[{\"m_uniqueId\":1,\"productId\":2},{\"m_uniqueId\":1,\"productId\":3}],\"itemUserDatas\":[{\"itemId\":4}]}",null);}catch(ArgumentException){threw=true;}
                Require(threw&&manager.UpdateHandle==0&&manager.Indexes.Products.Count==1&&manager.Indexes.Items.Count==0&&string.Join(",",events)=="add","duplicate failure retains timestamp/handle/partial products and skips items/statistics");
                manager.Release();Require(string.Join(",",events)=="add,remove:0,dispose,clear"&&!manager.UpdateHandle.HasValue,"partial initialization still releases its registered update before config and singleton");
            });
            check("outgame-global-item-lifecycle-release-failure-and-time-resync",()=>{
                var events=new List<string>();var h=new ItemLifecycleHost{Events=events};var manager=new OutgameGlobalItemLifecycle(h,()=>{},id=>true,id=>true,()=>OutgameItemTimestamp.Epoch);manager.Initialize("",null);
                manager.SynchronizeServerTime(9876543210123L);Require(manager.Data.saveTimestamp==9876543210123L&&events.FindAll(x=>x=="add").Count==1,"resync updates full timestamp without duplicate handle");
                h.FailRemove=true;bool threw=false;try{manager.Release();}catch(InvalidOperationException){threw=true;}Require(threw&&manager.UpdateHandle.HasValue&&!events.Contains("dispose"),"remove failure preserves handle and prevents later release steps");
                h.FailRemove=false;manager.Release();events.Clear();manager.Release();Require(string.Join(",",events)=="dispose,clear","repeated release still disposes configuration and clears instance");
                h.FailAdd=true;threw=false;try{manager.SynchronizeServerTime(7);}catch(InvalidOperationException){threw=true;}Require(threw&&manager.Data.saveTimestamp==7&&!manager.UpdateHandle.HasValue,"add failure occurs after timestamp write without inventing a handle");
            });
            check("outgame-product-update-lifecycle-threshold-and-day-comparison",()=>{
                var events=new List<string>();var h=new ItemLifecycleHost{Events=events};DateTime now=new DateTime(2026,1,2);float delta=1;int resets=0,hostCalls=0;
                var manager=new OutgameGlobalItemLifecycle(h,id=>true,id=>true,()=>now,()=>delta,id=>new OutgameProductRefreshConfig{buyLimit=1},id=>{Require(id==7,"reset receives UID");resets++;},()=>hostCalls++,x=>{});
                manager.Initialize("{\"lastExitDayOfYear\":1,\"productUserDatas\":[{\"m_uniqueId\":7,\"productId\":3}]}",null);h.Update();Require(resets==0&&hostCalls==1,"exactly one second skips product pass but calls host Update");
                delta=0.25f;h.Update();Require(resets==1&&manager.Data.lastExitDayOfYear==2&&manager.ProductUpdates.AccumulatedTime==0.25f,"strict threshold triggers daily reset and retains remainder");
                now=new DateTime(2027,1,2);delta=4;h.Update();Require(resets==1&&manager.ProductUpdates.AccumulatedTime==3.25f&&hostCalls==3,"source ignores year and subtracts one second once per frame");
            });
            check("outgame-product-interval-refresh-boundary-catchup-and-overflow",()=>{
                var h=new ItemLifecycleHost{Events=new List<string>()};DateTime now=OutgameItemTimestamp.Epoch.AddMilliseconds(1000);var manager=new OutgameGlobalItemLifecycle(h,()=>{},id=>true,id=>true,()=>now);manager.Initialize("",null);
                var cfg=new OutgameProductRefreshConfig{refreshPeriod=new[]{10}};int resets=0;var updates=new OutgameProductUpdates(manager,id=>cfg,id=>resets++,()=>{},x=>{});var row=new OutgameProductUserData{m_uniqueId=7};
                updates.UpdateInterval(row);Require(row.nextRefreshTimeStamp==11000&&resets==1,"first interval sets next timestamp and resets immediately");now=OutgameItemTimestamp.Epoch.AddMilliseconds(11000);updates.UpdateInterval(row);Require(resets==1,"equal timestamp does not reset");
                now=OutgameItemTimestamp.Epoch.AddMilliseconds(35000);updates.UpdateInterval(row);Require(row.nextRefreshTimeStamp==41000&&resets==2,"missed periods advance from old schedule without multiple reset grants");
                cfg.refreshPeriod=new[]{2147484};row.nextRefreshTimeStamp=0;updates.UpdateInterval(row);Require(row.nextRefreshTimeStamp==unchecked(35000L+(long)unchecked(2147484*1000)),"seconds multiplication wraps Int32 before Int64 addition");
                cfg.refreshPeriod=new[]{0};row.nextRefreshTimeStamp=1;bool threw=false;try{updates.UpdateInterval(row);}catch(DivideByZeroException){threw=true;}Require(threw,"source zero interval is not repaired");
            });
            check("outgame-product-hour-and-week-refresh-original-calendar-boundaries",()=>{
                var h=new ItemLifecycleHost{Events=new List<string>()};DateTime now=new DateTime(2026,1,30,8,0,0);var manager=new OutgameGlobalItemLifecycle(h,()=>{},id=>true,id=>true,()=>now);manager.Initialize("",null);
                var cfg=new OutgameProductRefreshConfig{refreshPeriod=new[]{8,16}};int resets=0;var updates=new OutgameProductUpdates(manager,id=>cfg,id=>resets++,()=>{},x=>{});var row=new OutgameProductUserData{m_uniqueId=9};
                updates.UpdateHours(row);Require(OutgameItemTimestamp.ToDateTime(row.nextRefreshTimeStamp)==new DateTime(2026,1,30,16,0,0),"equal hour skips to next configured hour");now=new DateTime(2026,1,30,20,0,0);updates.UpdateHours(row);Require(OutgameItemTimestamp.ToDateTime(row.nextRefreshTimeStamp)==new DateTime(2026,1,31,8,0,0),"after final hour uses next day constructor");
                now=new DateTime(2026,1,31,20,0,0);long before=row.nextRefreshTimeStamp;bool threw=false;try{updates.UpdateHours(row);}catch(ArgumentOutOfRangeException){threw=true;}Require(threw&&row.nextRefreshTimeStamp==before&&resets==2,"original day+1 constructor throws at month end before timestamp/reset");
                now=new DateTime(2026,9,30,12,0,0);cfg.refreshPeriod=new[]{1,5};row.nextRefreshTimeStamp=0;updates.UpdateWeekdays(row);Require(OutgameItemTimestamp.ToDateTime(row.nextRefreshTimeStamp)==new DateTime(2026,10,2),"weekday mode uses AddDays and crosses month correctly");
                now=new DateTime(2026,10,2,12,0,0);updates.UpdateWeekdays(row);Require(OutgameItemTimestamp.ToDateTime(row.nextRefreshTimeStamp)==new DateTime(2026,10,5),"equal weekday skips to first configured day next week");
            });
            check("outgame-product-update-failure-and-source-parameter-gates",()=>{
                var h=new ItemLifecycleHost{Events=new List<string>()};var manager=new OutgameGlobalItemLifecycle(h,()=>{},id=>true,id=>true,()=>new DateTime(2026,1,1));manager.Initialize("",null);
                var row=new OutgameProductUserData{m_uniqueId=1,productId=2};manager.Indexes.Products.Add(1,row);var cfg=new OutgameProductRefreshConfig{buyLimit=3,refreshPeriod=new[]{-1}};int resets=0,hostCalls=0;var updates=new OutgameProductUpdates(manager,id=>cfg,id=>resets++,()=>hostCalls++,x=>{});
                updates.Update(2);Require(resets==0&&hostCalls==1,"disabled period sentinel skips reset while per-frame host runs");cfg.refreshPeriod=new int[0];bool threw=false;try{updates.Update(1);}catch(IndexOutOfRangeException){threw=true;}Require(threw&&hostCalls==1&&updates.AccumulatedTime==1,"original first parameter access precedes empty check; failure skips host callback");
            });
            check("outgame-product-reset-modes-price-order-and-original-snapshot-key",()=>{
                var indexes=new OutgameGlobalItemIndexes();indexes.InitializeProducts(new OutgameItemManagerData(),id=>true);var row=new OutgameProductUserData{m_uniqueId=7,productId=2,buyCount=9,haveBuyTimes=3};indexes.Products.Add(7,row);indexes.SnapshotProduct(7);
                var cfg=new OutgameProductRefreshConfig{id=2,buyLimit=1,buyLimitParam=5};var events=new List<string>();var reset=new OutgameProductResets(indexes,id=>cfg,p=>{Require(p.haveBuyTimes==0&&p.HaveBuyCounts.Count==0,"counters cleared before price recalculation");events.Add("price:"+p.buyCount);},(message,args)=>events.Add(message+":"+args[0]),args=>events.Add("warning"));
                reset.Reset(7);Require(string.Join(",",events)=="price:5,ItemUI_RefreshStore:7,ItemUI_ProductReset:7"&&row.buyCount==5&&indexes.ProductSnapshot[7].buyCount==9,"daily mode sets limit before price; source snapshot uses config id, not UID");
                events.Clear();row.buyCount=9;cfg.buyLimit=3;reset.Reset(7);Require(events[0]=="price:9"&&row.buyCount==5,"interval mode sets limit after price");
                events.Clear();row.buyCount=9;cfg.buyLimit=0;reset.Reset(7);Require(row.buyCount==9&&events.Count==3,"unlimited resets purchase histories without buyCount assignment");
                events.Clear();cfg.buyLimit=2;reset.Reset(7);Require(events.Count==0,"mode2 exits before notifications");cfg.buyLimit=99;reset.Reset(7);Require(events.Count==2,"unknown mode still sends source notifications without price mutation");
                events.Clear();reset.Reset(999);Require(events.Count==1&&events[0]=="warning","missing UID warns without UI events");
            });
            check("outgame-product-reset-integrated-schedule-and-failure-state",()=>{
                var events=new List<string>();var h=new ItemLifecycleHost{Events=events};var cfg=new OutgameProductRefreshConfig{id=2,buyLimit=1,buyLimitParam=4};OutgameProductResets reset=null;
                var manager=new OutgameGlobalItemLifecycle(h,id=>true,id=>true,()=>new DateTime(2026,1,2),()=>2f,id=>cfg,id=>reset.Reset(id),()=>events.Add("host"),x=>{});
                manager.Initialize("{\"lastExitDayOfYear\":1,\"productUserDatas\":[{\"m_uniqueId\":7,\"productId\":2,\"haveBuyTimes\":3,\"HaveBuyCounts\":[{\"key\":1,\"buyCount\":[{\"key\":2,\"value\":6}]}]}]}",null);
                var row=manager.Indexes.Products[7];Require(row.HaveBuyCounts[0].buyCount[0].value==6,"actual nested purchase wire decoded");
                reset=new OutgameProductResets(manager.Indexes,id=>cfg,p=>events.Add("price"),(message,args)=>events.Add(message),x=>{});events.Clear();h.Update();Require(string.Join(",",events)=="price,ItemUI_RefreshStore,ItemUI_ProductReset,host"&&row.buyCount==4&&row.HaveBuyCounts.Count==0,"scheduled daily pass drives actual reset and UI sequence before host update");
                row.haveBuyTimes=8;row.buyCount=9;cfg.buyLimit=3;manager.Indexes.IsDirty=false;events.Clear();reset=new OutgameProductResets(manager.Indexes,id=>cfg,p=>throw new InvalidOperationException(),(message,args)=>events.Add(message),x=>{});
                bool threw=false;try{reset.Reset(7);}catch(InvalidOperationException){threw=true;}Require(threw&&row.haveBuyTimes==0&&row.buyCount==9&&!manager.Indexes.IsDirty&&events.Count==0,"price failure leaves cleared counts, skips dirty flag, later limit assignment and messages");
            });
            check("outgame-product-prices-all-source-formulas-and-distinct-counts",()=>{
                var cfg=new OutgameProductPriceConfig{buyTypeOrder=new[]{1,1,1,1,1,1},costItemPriceTypes=new[]{0,1,2,3,4,5},costItemPriceParams=new[]{new[]{9,10},new[]{9,10,20},new[]{9,10,20,30},new[]{9,10,20,30},new[]{9,10,3},new[]{9,10,3}},getType=1,priceCalParam=new[]{9,10,20,30}};
                var row=new OutgameProductUserData{productId=2,haveBuyTimes=3};row.HaveBuyCounts.Add(new OutgameProductBuyCount{key=1,buyCount=new List<OutgameItemGetTypeBuyCount>{new OutgameItemGetTypeBuyCount{key=9,value=2}}});
                var prices=new OutgameProductPrices(id=>cfg,(min,max)=>max,values=>values[values.Count-1],(a,b)=>(float)Math.Pow(a,b),(id,error)=>throw error);prices.Update(row);
                Require(string.Join(",",row.consumeItemPriceArray)=="10,20,30,30,16,90","all six array price modes use per-item count2");
                int[] expected={10,3,3,3,19,270};cfg.priceCalParam=new[]{9,10,3};
                for(int mode=0;mode<6;mode++){cfg.priceType=mode;prices.Update(row);Require(row.consumeItemPrice==expected[mode],"scalar mode"+mode+" uses product haveBuyTimes3");}
                Require(row.HaveBuyCounts.Count==1&&row.HaveBuyCounts[0].buyCount.Count==1,"pricing reads counters without increments");
            });
            check("outgame-product-price-source-indexing-errors-and-held-values",()=>{
                var cfg=new OutgameProductPriceConfig{buyTypeOrder=new[]{2,1,2,1},costItemPriceTypes=new[]{0,4},costItemPriceParams=new[]{new[]{9,11},new[]{9,20,2}},getType=2,priceCalParam=new[]{9,7}};
                var row=new OutgameProductUserData{productId=2,consumeItemPrice=99};var errors=new List<Exception>();var prices=new OutgameProductPrices(id=>cfg,(min,max)=>min,values=>values[0],(a,b)=>(float)Math.Pow(a,b),(id,error)=>errors.Add(error));prices.Update(row);
                Require(string.Join(",",row.consumeItemPriceArray)=="11,20"&&row.consumeItemPrice==99&&row.HaveBuyCounts.Count==0,"compact index selects parameters; absent count stays detached; non-item scalar held");
                var old=row.consumeItemPriceArray;cfg.buyTypeOrder=new[]{1,1,1};cfg.costItemPriceTypes=new[]{0,0,0};cfg.costItemPriceParams=new[]{new int[0],new[]{9,33},new[]{9,44}};cfg.getType=1;cfg.priceType=2;cfg.priceCalParam=new[]{9};prices.Update(row);
                Require(ReferenceEquals(old,row.consumeItemPriceArray)&&old[0]==11&&old[1]==33&&errors.Count==3&&row.consumeItemPrice==99,"per-entry errors continue; no resizing; scalar failure retains previous value");
                cfg.priceCalParam=null;prices.Update(row);Require(row.consumeItemPrice==0,"missing scalar params clear value before getType gate");
                cfg.buyTypeOrder=null;cfg.getType=0;cfg.priceCalParam=new[]{9,8};prices.Update(row);Require(cfg.buyTypeOrder.Length==1&&cfg.buyTypeOrder[0]==0,"missing buy order mutates original config to getType singleton");
            });
            check("outgame-product-price-reset-composition-and-numeric-conversion",()=>{
                var indexes=new OutgameGlobalItemIndexes();var row=new OutgameProductUserData{m_uniqueId=7,productId=2,haveBuyTimes=3,buyCount=9};indexes.Products.Add(7,row);
                var cfg=new OutgameProductPriceConfig{buyTypeOrder=new int[0],getType=1,priceType=4,priceCalParam=new[]{9,10,5}};
                var prices=new OutgameProductPrices(id=>cfg,(a,b)=>a,values=>values[0],(a,b)=>float.NaN,(id,error)=>throw error);
                var reset=new OutgameProductResets(indexes,id=>new OutgameProductRefreshConfig{id=2,buyLimit=1,buyLimitParam=4},prices.Update,(message,args)=>Require(row.consumeItemPrice==10,"real price updated before UI event"),args=>{});reset.Reset(7);
                Require(row.haveBuyTimes==0&&row.buyCount==4&&row.consumeItemPrice==10&&indexes.IsDirty,"reset composes actual price formulas and dirty state");
                row.haveBuyTimes=2;cfg.priceCalParam=new[]{9,int.MaxValue,1};prices.Update(row);Require(row.consumeItemPrice==-2147483647L,"Int32 addition wraps before Int64 storage");
                cfg.priceType=5;cfg.priceCalParam=new[]{9,3,2};prices.Update(row);Require(row.consumeItemPrice==int.MinValue,"NaN power becomes Int32 minimum before wrapped multiply");
            });
            check("outgame-original-random-range-list-dispatch-and-boundaries",()=>{
                var probe=new PriceRandomProbe();var random=new GameRandomSource(probe);Require(random.Inclusive(4,8)==4&&probe.Min==4&&probe.Max==9&&probe.RangeCalls==1,"inclusive source range dispatches Next(min,max+1)");
                random.Inclusive(0,int.MaxValue);Require(probe.Max==int.MinValue,"source upper-bound addition wraps before Random.Next validation");
                Require(random.Element(new List<int>{7,9})==7&&probe.Count==2&&probe.ListCalls==1,"generic source list selection uses Next(count) overload");
                bool threw=false;try{random.Element(new List<int>());}catch(ArgumentOutOfRangeException){threw=true;}Require(threw&&probe.Count==0&&probe.ListCalls==2,"empty list still calls Next(0), then original index access fails");
                threw=false;try{new GameRandomSource(new System.Random(3)).Inclusive(0,int.MaxValue);}catch(ArgumentOutOfRangeException){threw=true;}Require(threw,"actual managed generator retains overflow range failure");
            });
            check("outgame-random-shared-stream-interleaves-price-and-battle-draws",()=>{
                var actual=new GameRandomSource(new System.Random(731));var expected=new System.Random(731);
                Require(actual.Managed.Next(100)==expected.Next(100),"battle-style draw consumes the same managed stream");
                var cfg=new OutgameProductPriceConfig{buyTypeOrder=new[]{1},costItemPriceTypes=new[]{1},costItemPriceParams=new[]{new[]{9,10,15}},getType=1,priceType=3,priceCalParam=new[]{9,20,40,60}};
                var row=new OutgameProductUserData();var prices=new OutgameProductPrices(id=>cfg,actual.Inclusive,actual.Element,(a,b)=>(float)Math.Pow(a,b),(id,error)=>throw error);prices.Update(row);
                Require(row.consumeItemPriceArray[0]==expected.Next(10,16)&&row.consumeItemPrice==new[]{20,40,60}[expected.Next(3)],"price range then list draws consume one existing stream in source order");
                Require(actual.Managed.Next(100)==expected.Next(100),"following battle-style draw continues same stream");
            });
            check("outgame-menu-items-creation-frame-order-and-reuse",()=>{
                var trace=new List<string>();var waits=new List<System.Threading.Tasks.TaskCompletionSource<bool>>();WaitForEndOfFrame first=null;
                var items=new OutgameMenuItems((name,option)=>{Require(!option,"original ShowUI false parameter");trace.Add("create:"+name);return new MenuItemProbe(visible=>trace.Add(name+":"+visible));},()=>{trace.Add("item-info");return new MenuItemProbe(visible=>trace.Add("info-visible"));},frame=>{
                    if(first==null)first=frame;else Require(ReferenceEquals(first,frame),"shared source wait object");var pending=new System.Threading.Tasks.TaskCompletionSource<bool>();waits.Add(pending);return pending.Task;
                });
                var opening=items.OpenAsync();Require(!opening.IsCompleted&&waits.Count==1&&items.Main!=null&&items.Shop==null,"first page stored before first frame wait");
                waits[0].SetResult(true);Require(waits.Count==2&&items.Shop!=null&&items.Commander==null,"second page creation follows first wait");
                waits[1].SetResult(true);Require(waits.Count==3&&items.Commander!=null&&items.ItemInfo==null,"third page creation follows second wait");
                waits[2].SetResult(true);opening.GetAwaiter().GetResult();Require(string.Join(",",trace)=="create:Proj_xqzdStartUI,create:ShopUI,create:CommanderUI,item-info","new pages are not given an extra Visible assignment after creation");
                trace.Clear();items.OpenAsync().GetAwaiter().GetResult();Require(waits.Count==3&&string.Join(",",trace)=="Proj_xqzdStartUI:True,ShopUI:True,CommanderUI:True","existing pages show immediately without new waits or item-info creation");
                trace.Clear();items.LegacyPage24=new MenuItemProbe(value=>trace.Add("legacy24:"+value));items.LegacyPage40=new MenuItemProbe(value=>trace.Add("legacy40:"+value));items.CloseAllMenuItemUI();
                Require(string.Join(",",trace)=="Proj_xqzdStartUI:False,legacy24:False,ShopUI:False,CommanderUI:False,legacy40:False","close visits original five slots without hiding ItemInfo");
            });
            check("outgame-menu-visibility-close-and-reopen-lifecycle",()=>{
                var host=new MenuVisibilityHostProbe();var lifecycle=new OutgameMenuVisibility(host);host.Owner=lifecycle;
                lifecycle.DoClose();Require(string.Join(",",host.Calls)=="main,close-items,page:0,set:False,base:False,dispose-message"&&!host.Active,"close returns to main before hiding pages, resets current page then uses virtual visibility path");
                host.Calls.Clear();lifecycle.DoClose();Require(host.Calls.Count==0,"already invisible menu close has no side effects");
                lifecycle.VisibleImp(true);Require(string.Join(",",host.Calls)=="base:True,open-items,main,panels,commander,level-gate,skins"&&host.Active,"show applies base visibility then ordered original menu refreshes");
                host.Calls.Clear();lifecycle.VisibleImp(false);lifecycle.VisibleImp(false);Require(string.Join(",",host.Calls)=="base:False,dispose-message,base:False,dispose-message","VisibleImp itself does not suppress repeated hide notifications");
            });
            check("outgame-home-scene-imported-owner-references",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots"));
                try{
                    var owner=root.GetComponent<OutgameModelRoots>();
                    Require(owner.SceneGame&&owner.GameCamera&&owner.CommanderCamera&&owner.HomeCamera,"original root prefab contains complete home/battle camera references");
                    Require(owner.SceneGame.name=="Scene_game"&&owner.GameCamera.name=="GameCamera"&&owner.CommanderCamera.name=="CommanderCamera","references identify recovered source objects");
                    owner.SceneGame.gameObject.SetActive(true);owner.SceneHome.gameObject.SetActive(false);owner.GameCamera.gameObject.SetActive(true);
                    owner.CreateHomeScenePresentation().PrepareHomeScene();
                    Require(!owner.SceneGame.gameObject.activeSelf&&owner.SceneHome.gameObject.activeSelf&&!owner.GameCamera.gameObject.activeSelf&&owner.HomeCamera.gameObject.activeSelf&&owner.CommanderCamera.gameObject.activeSelf,"home switch operates on actual imported scene/camera hierarchy");
                    Require(owner.Normal&&owner.Defense&&owner.Attack&&owner.HomeBackdrop&&owner.ModelBackdrop,"existing original model and backdrop references retained");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-home-scene-original-object-activation",()=>{
                var root=new GameObject("home-scene-owner");
                try{
                    var home=new GameObject("Scene_home");home.transform.SetParent(root.transform);var game=new GameObject("Scene_game");game.transform.SetParent(root.transform);
                    var homeCamera=new GameObject("HomeCamera",typeof(Camera));homeCamera.transform.SetParent(root.transform);
                    var commander=new GameObject("CommanderCamera",typeof(Camera));commander.transform.SetParent(root.transform);
                    var battle=new GameObject("GameCamera",typeof(Camera));battle.transform.SetParent(root.transform);
                    var prior=new GameObject("previous-source-object");prior.transform.SetParent(root.transform);
                    home.SetActive(false);homeCamera.SetActive(false);commander.SetActive(false);
                    var presentation=new OutgameHomeScenePresentation(home.transform,game.transform,homeCamera.GetComponent<Camera>(),commander.GetComponent<Camera>(),battle.GetComponent<Camera>())
                        {SourceObjects56=new Dictionary<int,GameObject>{{107,prior}},SourceOffset60=7};
                    presentation.PrepareHomeScene();Require(!prior.activeSelf&&!game.activeSelf&&home.activeSelf&&homeCamera.activeSelf&&commander.activeSelf&&!battle.activeSelf,"source keyed object and battle roots hidden; home and commander cameras enabled");
                    prior.SetActive(true);presentation.SourceOffset60=8;presentation.PrepareHomeScene();Require(prior.activeSelf,"missing source dictionary key leaves unrelated object untouched");
                    presentation.SourceObjects56=null;presentation.PrepareHomeScene();Require(home.activeSelf,"absent optional dictionary still allows home scene activation");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-level-full-state-branch-dispatch",()=>{
                var host=new LevelStateHostProbe();var start=new LevelStartHostProbe();var next=new LevelContinuationHostProbe();
                var states=new OutgameLevelStateBranches(host,new OutgameLevelStartTransition(start),new OutgameLevelContinuation(next));
                states.Dispatch(1);Require(string.Join(",",host.Calls)=="adlevel:6,channel,state:2:False,update","startup state enters home before update dialog");
                host.Calls.Clear();states.Dispatch(2);Require(string.Join(",",host.Calls)=="home,stop:0,homeload:6","home prepares presentation then loads current level");
                host.Calls.Clear();states.Dispatch(4);Require(string.Join(",",host.Calls)=="close-loading,show:FailRewardUI","eligible restart opens original fail reward");
                host.Calls.Clear();host.Checked=true;states.Dispatch(4);Require(string.Join(",",host.Calls)=="close-loading,state:5:False","checked reward advances without showing again");
                host.Calls.Clear();host.Checked=false;host.Level=5;states.Dispatch(4);Require(host.Calls[1]=="state:5:False","below commander unlock skips reward");
                host.Calls.Clear();host.Insert=true;states.Dispatch(4);Require(host.Calls[1]=="show:FailRewardUI","insert pause bypasses level threshold");
                host.Calls.Clear();host.Guide=false;states.Dispatch(5);Require(string.Join(",",host.Calls)=="local:0,report:0:5,effect,state:6:True,play:1:1002","normal start state precedes music");
                host.Calls.Clear();host.Guide=true;host.Level=0;states.Dispatch(5);Require(string.Join(",",host.Calls)=="local:0,report:0:0,guide-next,tutorial-start,play:1:1002","tutorial start retains separate guide flow");
                host.Calls.Clear();host.BossFlag=true;states.Dispatch(6);states.Dispatch(7);Require(string.Join(",",host.Calls)=="rain,speed:2,pause:False,pause:True","resume restores rain/speed/audio then pause only pauses audio");
                host.Calls.Clear();states.Dispatch(8);Require(string.Join(",",host.Calls)=="clear-model,close:Proj_xqzdPlayUI,show:Proj_xqzdOverUI,tutorial-finish,stop:1,close:GuideUI,small:0,rank-win,dice-win","win cleanup and side effects retain order");
                host.Calls.Clear();states.Dispatch(9);Require(string.Join(",",host.Calls)=="report:3:0,close:Proj_xqzdPlayUI,show:Proj_xqzdFailUI,stop:1,close:GuideUI","failure has distinct reporting and no win effects");
                host.Calls.Clear();states.Dispatch(13);states.Dispatch(-1);Require(string.Join(",",host.Calls)=="get-play","state13 only retrieves play UI; unknown branch no-op");
                states.Dispatch(3);states.Dispatch(11);Require(start.Pending!=null&&next.Hidden!=null,"full table delegates source delayed start and return paths");
            });
            check("outgame-level-retry-next-and-return-state-branches",()=>{
                var host=new LevelContinuationHostProbe();var flow=new OutgameLevelContinuation(host);
                Require(!flow.TryDispatch(3)&&host.Calls.Count==0,"unsupported branches left to full dispatcher");
                flow.TryDispatch(10);Require(string.Join(",",host.Calls)=="stop:0,close:GuideUI,local:1,init,small:0,state:3:True","retry resets small level after initialization");
                host.Calls.Clear();host.Small=4;flow.TryDispatch(15);Require(host.Small==4&&string.Join(",",host.Calls)=="stop:0,close:GuideUI,local:1,init,state:3:True","alternate retry preserves small index");
                host.Calls.Clear();flow.TryDispatch(12);Require(host.Level==6&&string.Join(",",host.Calls)=="stop:0,close:GuideUI,init,level:6,close:Proj_xqzdPauseUI,state:3:True","next level advances and closes pause before restart");
                host.Calls.Clear();flow.TryDispatch(14);Require(host.Small==5&&string.Join(",",host.Calls)=="stop:0,close:GuideUI,init,small:5,state:3:True","next small level does not mutate main level");
                host.Calls.Clear();flow.TryDispatch(11);Require(string.Join(",",host.Calls)=="ads:False,hide","return home waits for original hide completion");
                host.Calls.Clear();host.Hidden();Require(string.Join(",",host.Calls)=="close:Proj_xqzdPlayUI,close:GuideUI,special:0,init,state:2:True,show","home callback preserves cleanup, state change and reveal order");
            });
            check("outgame-play-state-common-notifications-and-special-mode",()=>{
                var host=new PlayStateHostProbe();var states=new OutgamePlayStateDispatcher(host);
                Require(states.SetPlaySate(3,true)&&states.State==3,"original option ignored and success return retained");
                Require(string.Join(",",host.Calls)=="pause:False,audio:3,line:3,ai:3,skill:3,message:3,pvp,dice,branch:3","source common notification order before branch");
                host.Calls.Clear();host.Pvp=true;states.SetPlaySate(7);Require(string.Join(",",host.Calls)=="pause:True,audio:7,line:7,ai:7,skill:7,message:7,pvp,camera","PVP short-circuits dice read and state-specific branch");
                host.Calls.Clear();host.Pvp=false;host.Dice=true;states.SetPlaySate(3);Require(host.Calls[host.Calls.Count-1]=="camera"&&!host.Calls.Contains("branch:3"),"dice mode also bypasses branch");
                host.Dice=false;host.Calls.Clear();host.AudioHook=state=>{if(state==3)states.SetPlaySate(7);};states.SetPlaySate(3);
                Require(states.State==7&&host.Calls.Contains("message:3")&&host.Calls[host.Calls.Count-1]=="branch:7","reentrant state changes affect later dispatch while original request remains message payload");
            });
            check("outgame-level-start-source-transition-order",()=>{
                var host=new LevelStartHostProbe();var transition=new OutgameLevelStartTransition(host);transition.Enter();
                Require(string.Join(",",host.Calls)=="loading,loading-flag:True,loading-value:5,enemy,level-flag:False,menu-query,menu-close,schedule","state3 loading and menu order before delay");
                host.Level=9;host.Calls.Clear();host.Pending();Require(string.Join(",",host.Calls)=="level-query,load:9,scene","delayed callback reads current level at invocation then initializes scene");
                host.HasMenu=false;host.Calls.Clear();transition.Enter();Require(host.Calls[host.Calls.Count-1]=="schedule"&&!host.Calls.Contains("menu-close"),"missing menu does not suppress source delayed load");
            });
            check("outgame-sdk-xyx-wx-module-controller-reward-chain",()=>{
                var environment=new RewardVideoHost();var native=new AdController{Ready=true};
                var router=new OutgameAdRequestRouter(()=>"weixin","tag",environment.Log){VideoController=native};
                var module=new OutgameRewardVideo(new OutgameRewardVideoControllerHost(router,environment)){Active=true};
                var entry=new OutgameAdModuleEntry(()=>module,environment.Warn);var messages=new OutgameMessageDispatcher();var results=new List<bool>();float now=10;
                OutgameAdsVideoFlow flow=null;var bridge=new OutgameXyxVideoBridge(entry,environment.Warn,text=>flow.AfterVideo(text));
                flow=new OutgameAdsVideoFlow(()=>now,wait=>System.Threading.Tasks.Task.CompletedTask,(flag,id)=>bridge.ShowVideoStatic(flag),()=>messages,label=>{},text=>{},playing=>{},text=>OutgameCallbackJson.Deserialize(text) as Dictionary<string,object>);
                Require(bridge.IsVideoReadyStatic(),"XYX readiness reaches recovered WX module");
                flow.ShowVideoAsync(2,(flag,ended)=>{Require(flag=="2","source flag survives full bridge chain");results.Add(ended);},1002).GetAwaiter().GetResult();
                Require(native.ShowCount==1&&module.Showing&&results.Count==0,"SDK request reaches WX module and native controller without synthetic completion");
                native.Shown(true);native.Closed(false);Require(results.Count==1&&!results[0]&&!module.Showing&&flow.ShowVideoInterval==now,"native cancellation maps through module/JSON and resets SDK gate");
                now=15;flow.ShowVideoAsync(2,(flag,ended)=>results.Add(ended),1002).GetAwaiter().GetResult();native.Shown(true);native.Closed(true);
                Require(results.Count==2&&results[1]&&flow.ShowVideoInterval==19,"native completion traverses restored layers and preserves successful gate");
            });
            check("outgame-xyx-video-platform-callback-conversion",()=>{
                bool ready=false;var callbacks=new List<Action<bool>>();var traces=new List<string>();
                var bridge=new OutgameXyxVideoBridge(()=>ready,callback=>{traces.Add("platform");callbacks.Add(callback);},traces.Add,text=>traces.Add(text));
                Require(!bridge.IsVideoReadyStatic(),"readiness comes from current platform bridge");ready=true;Require(bridge.IsVideoReadyStatic(),"bridge does not cache readiness");
                bridge.ShowVideoStatic(2);bridge.ShowVideoStatic(7);Require(string.Join(",",traces)=="ShowVideoStatic:2,platform,ShowVideoStatic:7,platform","warning precedes each platform call");
                traces.Clear();callbacks[1](false);callbacks[0](true);callbacks[0](false);
                Require(traces[0]=="{\"videoFlag\":\"7\",\"result\":\"1\"}"&&traces[1]=="{\"videoFlag\":\"2\",\"result\":\"0\"}"&&traces[2]=="{\"videoFlag\":\"2\",\"result\":\"1\"}","per-request flag retained, bool inverted to source numeric string, repeated callbacks forwarded");
            });
            check("outgame-sdk-actions-original-manager-routing",()=>{
                var manager=new SdkActionManagersProbe();bool close=true,ready=false;int queries=0;
                var actions=new OutgameSdkFunctionActions(manager,()=>close,(id,refresh)=>{Require(id==15&&!refresh,"ad hint reads cached feature15");queries++;return ready;},key=>{Require(key=="Sdk_NoAdsTips","source localization key");return "localized";});
                actions.AdsVideoFunction();Require(queries==0&&manager.Last==null,"CloseAdsTips short-circuits readiness and toast");
                close=false;ready=true;actions.AdsVideoFunction();Require(queries==1&&manager.Last==null,"ready ads do not display unavailable hint");
                ready=false;actions.AdsVideoFunction();Require(manager.Last=="toast:localized","unavailable ads display localized source hint");
                manager.Last=null;actions.ShowShareFunction();Require(manager.Last==null,"original shared action has empty body");
                actions.ShowLoginFunction();Require(manager.Last=="login:True","login passes original true argument");
                actions.FeedbackFunction();Require(manager.Last=="ShowFeedback","source FeedbackFunction routing");
                actions.GDPRUserFunction();Require(manager.Last=="ShowGDPRDialogStatic","source GDPRUserFunction routing");
                actions.ShowPolicyFunction();Require(manager.Last=="GotoPrivacyPolicyStatic","source ShowPolicyFunction routing");
                actions.ShowUserProtocolFunction();Require(manager.Last=="GotoTermsServiceStatic","source ShowUserProtocolFunction routing");
                actions.ShowGameBanHaoFunction();Require(manager.Last=="ShowGameBanHao","source ShowGameBanHaoFunction routing");
                actions.StartRestore();Require(manager.Last=="StartRestoreStatic","source StartRestore routing");
                actions.ShowOppoGameCenter();Require(manager.Last=="OpenOppoGameCenterStatic","source ShowOppoGameCenter routing");
                actions.DrawVideo();Require(manager.Last=="ShowDrawVideoStatic","source DrawVideo routing");
                actions.OpenPrivacyRecall();Require(manager.Last=="OpenPrivacyRecallActStatic","source OpenPrivacyRecall routing");
            });
            check("outgame-sdk-action-initialization-yields-and-binding",()=>{
                var actions=new Dictionary<int,UnityEngine.Events.UnityAction>();var owner=new SdkActionProbe();
                var init=OutgameSdkFunctionInitialization.InitSDKOpenFunction(actions,owner);
                Require(actions.Count==0&&init.MoveNext()&&init.Current is WaitForEndOfFrame&&actions.Count==0,"no actions registered before source end-of-frame boundary");
                Require(init.MoveNext()&&init.Current is CustomYieldInstruction wait&&!wait.keepWaiting&&actions.Count==12,"twelve actions registered before original WaitForUpdate");
                var page=new GameObject("sdk-action-binding");
                try{
                    var button=page.AddComponent<UnityEngine.UI.Button>();var registry=new OutgameSdkButtonRegistry(actions,(id,refresh)=>true,(id,value)=>{},()=>true,null);
                    registry.BindFunctionBtn(8,new[]{button});button.onClick.Invoke();Require(owner.Last==8,"registered native button invokes original login action slot");
                    foreach(var id in new[]{5,7,8,9,10,11,12,17,19,20,21,22}){actions[id]();Require(owner.Last==id,"each original function maps to corresponding owner action");}
                }finally{UnityEngine.Object.DestroyImmediate(page);}
                Require(!init.MoveNext(),"initialization terminates after second yield");
                var partial=new Dictionary<int,UnityEngine.Events.UnityAction>{{8,()=>{}}};init=OutgameSdkFunctionInitialization.InitSDKOpenFunction(partial,owner);init.MoveNext();bool failed=false;
                try{init.MoveNext();}catch(ArgumentException){failed=true;}
                Require(failed&&partial.Count==3&&partial.ContainsKey(5)&&partial.ContainsKey(7)&&!partial.ContainsKey(9),"duplicate Add preserves earlier registrations and stops at source collision");
            });
            check("outgame-sdk-button-refresh-cached-state-and-dead-entries",()=>{
                var a=new GameObject("sdk-refresh-a");var b=new GameObject("sdk-refresh-b");
                try{
                    var first=a.AddComponent<UnityEngine.UI.Button>();var second=b.AddComponent<UnityEngine.UI.Button>();
                    bool allowed=true;int queries=0;var states=new OutgameSdkFunctionStates(id=>{queries++;return allowed;},id=>id.ToString(),text=>{});
                    var registry=new OutgameSdkButtonRegistry(new Dictionary<int,UnityEngine.Events.UnityAction>(),states.IsOpen,states.Set,()=>true,null);
                    registry.ReshsdkFunctionBtnState(9);Require(queries==0,"unregistered feature does not resolve capability");
                    registry.BindFunctionBtn(9,new[]{first,second,first});Require(queries==1,"duplicate buttons share cached capability");
                    allowed=false;registry.ReshsdkFunctionBtnState(9);Require(a.activeSelf&&b.activeSelf&&queries==1,"refresh buttons does not force refresh platform capability");
                    states.Set(9,false);UnityEngine.Object.DestroyImmediate(b);registry.ReshsdkFunctionBtnState(9);
                    Require(!a.activeSelf&&queries==1,"destroyed button skipped; cached callback value applied to remaining buttons");
                    states.Set(9,true);registry.ReshsdkFunctionBtnState(9);Require(a.activeSelf,"refresh can reactivate existing inactive button");
                }finally{if(a)UnityEngine.Object.DestroyImmediate(a);if(b)UnityEngine.Object.DestroyImmediate(b);}
            });
            check("outgame-sdk-capabilities-numeric-rules-and-cache",()=>{
                int value=0;var calls=new List<string>();var resolver=new OutgameSdkCapabilityResolver(method=>{calls.Add(method);return value;});
                Require(resolver.GetState(5)&&!resolver.GetState(-1)&&!resolver.GetState(23)&&calls.Count==0,"source fixed video support and unsupported ids do not query platform");
                Require(resolver.GetState(6)&&resolver.GetState(17)&&!resolver.GetState(21),"design-mode and publication-type zero invert; draw status zero is unavailable");
                value=2;Require(!resolver.GetState(6)&&!resolver.GetState(17)&&!resolver.GetState(21),"draw video requires exactly1 rather than any nonzero");
                value=1;Require(resolver.GetState(21),"draw status1 enables source function");
                calls.Clear();resolver.GetState(11);resolver.GetState(12);Require(calls.Count==2&&calls[0]=="SDKToolManager.IsShowPolicy"&&calls[1]==calls[0],"policy and user-protocol functions share source policy query");
                var states=new OutgameSdkFunctionStates(resolver.GetState,id=>id.ToString(),text=>{});calls.Clear();value=0;
                Require(!states.IsOpen(15)&&calls.Count==1&&calls[0]=="AdsManager.isVideoReady","state cache resolves original advertisement readiness manager");
                value=1;Require(!states.IsOpen(15)&&states.IsOpen(15,true)&&calls.Count==2,"platform readiness changes visible only through forced refresh or callback state update");
            });
            check("outgame-sdk-function-state-refresh-not-fallback",()=>{
                int queries=0;bool current=false;var trace=new List<string>();
                var states=new OutgameSdkFunctionStates(id=>{queries++;return current;},id=>"feature"+id,trace.Add);
                Require(!states.IsOpen(15,true)&&queries==1,"refresh=true does not imply default true");
                Require(string.Join(",",trace)=="feature15==IsRefresh==True,feature15Add======False,feature15==Get==False","source diagnostics order before cache insertion and return");
                current=true;Require(!states.IsOpen(15,false)&&queries==1,"non-refresh retains cached false");
                Require(states.IsOpen(15,true)&&queries==2,"explicit refresh re-queries source");
                states.Set(15,false);Require(!states.IsOpen(15,false)&&queries==2,"readiness callback update becomes authoritative cached value");
                bool fail=false;var failing=new OutgameSdkFunctionStates(id=>true,id=>"feature",text=>{if(text.Contains("Add======"))throw new InvalidOperationException("trace");});
                try{failing.Set(5,true);}catch(InvalidOperationException){fail=true;}Require(fail,"source logging failure precedes state insertion");
            });
            check("outgame-sdk-button-registry-original-bind-and-readiness",()=>{
                var page=new GameObject("registry");page.SetActive(false);
                try{
                    var sdk=new VideoButtonHostProbe();var video=page.AddComponent<OutgameVideoButton>();video.Bind(sdk,new OutgameVideoPlayCooldown());
                    var calls=new List<string>();bool allowed=true;int clicks=0;var args=new object[]{"original"};
                    var registry=new OutgameSdkButtonRegistry(new Dictionary<int,UnityEngine.Events.UnityAction>{{5,()=>clicks++}},(function,fallback)=>{calls.Add("open:"+function);return allowed;},(function,state)=>calls.Add("state:"+function+":"+state),()=>{calls.Add("ready");return true;},(function,button,payload)=>{Require(ReferenceEquals(payload,args),"same platform args");calls.Add("bind");});
                    registry.BindFunctionBtn(5,new UnityEngine.UI.Button[]{video,video},args);video.onClick.Invoke();Require(clicks==2,"duplicate registrations retain duplicate native listeners");
                    calls.Clear();registry.CheckVideoIsReady();Require(string.Join(",",calls)=="open:5,ready,open:5,state:15:True"&&video.ButtonState==2,"readiness checks feature twice and updates state15 before all video buttons");
                    allowed=false;calls.Clear();registry.ChangeVideoBtnState(false);Require(video.ButtonState==2&&string.Join(",",calls)=="open:5","disabled feature prevents state changes");
                    allowed=true;UnityEngine.Object.DestroyImmediate(page);registry.ChangeVideoBtnState(false);Require(calls[calls.Count-1]=="state:15:False","destroyed native buttons cleaned before enumeration");
                }finally{if(page)UnityEngine.Object.DestroyImmediate(page);}
            });
            check("outgame-callback-json-original-permissive-semantics",()=>{
                var logs=new List<string>();var data=OutgameCallbackJson.DataParse("{\"videoFlag\":1,\"result\":1,\"result\":0,\"extra\":[true,false,null,2.5]}",logs.Add);
                Require((long)data["videoFlag"]==1&&(long)data["result"]==0&&logs.Count==1,"source dictionary duplicate overwrites and integral values are Int64");
                var array=(List<object>)data["extra"];Require((bool)array[0]&&!(bool)array[1]&&array[2]==null&&(double)array[3]==2.5,"nested source JSON values preserved");
                Require((long)OutgameCallbackJson.Deserialize("1e2")==0&&(long)OutgameCallbackJson.Deserialize("9999999999999999999999999")==0,"source ignores TryParse failure and chooses integer solely by absence of decimal point");
                Require((long)OutgameCallbackJson.Deserialize("7 trailing")==7&&((List<object>)OutgameCallbackJson.Deserialize("[1,,2,]")).Count==2,"source permits trailing input and redundant commas");
                Require(OutgameCallbackJson.DataParse("[]",x=>{})==null&&OutgameCallbackJson.Deserialize(null)==null&&OutgameCallbackJson.Deserialize("{\"x\" 1}")==null,"non-dictionary root cast and malformed-object source behavior");
            });
            check("outgame-callback-json-string-and-error-boundaries",()=>{
                Require((string)OutgameCallbackJson.Deserialize("\"a\\n\\u4e2d\\qz\"")=="a\n中z","source string escapes and ignored unknown escape");
                Require((string)OutgameCallbackJson.Deserialize("\"unfinished")=="unfinished","unterminated string returns collected content");
                bool failed=false;try{OutgameCallbackJson.Deserialize("\"\\u12\"");}catch(OverflowException){failed=true;}Require(failed,"truncated Unicode reads through Convert.ToChar and throws at end of stream");
                failed=false;try{OutgameCallbackJson.DataParse("{}",x=>throw new InvalidOperationException("log"));}catch(InvalidOperationException){failed=true;}Require(failed,"callback log failure precedes parsing");
            });
            check("outgame-ads-video-delay-gate-and-callback-replacement",()=>{
                float time=10;var trace=new List<string>();var pending=new System.Threading.Tasks.TaskCompletionSource<bool>();var messages=new OutgameMessageDispatcher();WaitForSecondsRealtime firstWait=null;int waits=0;
                messages.AddListener("GF_ShowAdsVideo",args=>{Require(args==null,"source show message has null payload");trace.Add("show-message");});
                messages.AddListener("GF_AdsPlayCallBack",args=>trace.Add("result-message:"+args[0]));
                var ads=new OutgameAdsVideoFlow(()=>time,delay=>{waits++;firstWait=delay;return pending.Task;},(flag,id)=>trace.Add("sdk:"+flag+":"+id),()=>messages,flag=>trace.Add("report:"+flag),text=>{},playing=>{},text=>null){IsOpenAdsReport=true};
                var started=ads.ShowVideoAsync(0,(flag,success)=>trace.Add("old"),1001);
                Require(!started.IsCompleted&&ads.ShowVideoInterval==14&&firstWait.waitTime==.2f&&trace.Count==0,"unscaled four-second gate set before shared real-time wait");
                ads.ShowVideoAsync(1,(flag,success)=>trace.Add("new:"+flag+":"+success),1006).GetAwaiter().GetResult();Require(waits==1,"gated request replaces callback but does not schedule another SDK call");
                pending.SetResult(true);started.GetAwaiter().GetResult();Require(string.Join(",",trace)=="show-message,sdk:0:1001","original request retains original arguments after delay");
                trace.Clear();ads.ParseAfterVideo(new Dictionary<string,object>{{"videoFlag",0},{"result",0}});
                Require(string.Join(",",trace)=="report:0,result-message:True,new:0:True"&&ads.ShowVideoInterval==14,"success reports then broadcasts then invokes replacement callback; retains deadline");
                time=14;ads.ShowVideoAsync(2,null,1002).GetAwaiter().GetResult();Require(waits==2,"equal manager deadline permits new request");
            });
            check("outgame-ads-video-result-failure-and-interruption",()=>{
                var trace=new List<string>();float time=20;var messages=new OutgameMessageDispatcher();
                var ads=new OutgameAdsVideoFlow(()=>time,delay=>System.Threading.Tasks.Task.CompletedTask,(flag,id)=>{},()=>messages,flag=>{},text=>trace.Add("log"),playing=>trace.Add("playing:"+playing),text=>throw new FormatException("parse"));
                ads.ShowVideoAsync(0,(flag,success)=>trace.Add("callback:"+success),1001).GetAwaiter().GetResult();
                bool failed=false;try{ads.AfterVideo("bad");}catch(FormatException){failed=true;}Require(failed&&trace[0]=="playing:False","platform playing flag cleared before potentially failing data parse");
                trace.Clear();time=21;ads.ParseAfterVideo(new Dictionary<string,object>());Require(ads.ShowVideoInterval==21&&string.Join(",",trace)=="log,callback:False","missing result means failure, resets interval to now and invokes callback");
                trace.Clear();ads.AfterVideoFailed("0");Require(string.Join(",",trace)=="log","afterVideoFailed only logs; does not invoke callback or reset state");
                messages.AddListener("GF_AdsPlayCallBack",args=>throw new InvalidOperationException("subscriber"));trace.Clear();failed=false;
                try{ads.ParseAfterVideo(new Dictionary<string,object>{{"result","0"}});}catch(InvalidOperationException){failed=true;}
                Require(failed&&string.Join(",",trace)=="log","broadcast exception prevents subsequent button callback");
                failed=false;try{ads.ParseAfterVideo(new Dictionary<string,object>{{"result",null}});}catch(NullReferenceException){failed=true;}Require(failed,"present null result is not converted to missing field");
            });
            check("outgame-video-manager-cache-reset-and-source-json",()=>{
                var rows=OutgameVideoButtonManager.ReadRecoveredJson();var traces=new List<string>();int reads=0;bool available=true;
                var manager=new OutgameVideoButtonManager(()=>available,()=>false,()=>{reads++;return rows;},traces.Add,traces.Add,traces.Add);
                var gold=manager.GetData(1001);var diamond=manager.GetData(1006);
                Require(reads==1&&manager.Initialized&&gold.ReportLable=="ShopUI_sendGold"&&gold.showType==1&&gold.AudioId==1&&diamond!=null,"original raw JSON supplies shared cached video configuration");
                Require(gold.Video_param1==null&&gold.Video_param2==null,"JsonUtility retains source field-name mismatch instead of invented aliases");
                manager.Reset();available=false;Require(ReferenceEquals(manager.GetData(1001),gold)&&!manager.Initialized&&reads==1,"reset leaves prior dictionary readable when bundle unavailable");
                available=true;manager.GetData(1001);Require(reads==2&&manager.Initialized,"subsequent available request reloads cache");
                var first=new OutgameVideoButtonData{id=7};var second=new OutgameVideoButtonData{id=7};traces.Clear();
                manager=new OutgameVideoButtonManager(()=>true,()=>false,()=>new List<OutgameVideoButtonData>{first,second},traces.Add,traces.Add,traces.Add);
                Require(ReferenceEquals(manager.GetData(7),first)&&traces.Exists(x=>x.Contains("相同键")),"first duplicate wins and error is reported");
                Require(manager.GetData(-1)==null&&traces.Exists(x=>x.Contains("未找到")),"missing id returns null and logs source diagnostic");
            });
            check("outgame-video-manager-null-and-failed-load",()=>{
                int reads=0;bool fail=true;var one=new OutgameVideoButtonData{id=1};
                var manager=new OutgameVideoButtonManager(()=>false,()=>true,()=>{reads++;return fail?new List<OutgameVideoButtonData>{one,null}:null;},x=>{},x=>{},x=>{});
                bool threw=false;try{manager.GetData(1);}catch(NullReferenceException){threw=true;}
                Require(threw&&!manager.Initialized,"invalid row leaves initialization incomplete after partial dictionary mutation");
                fail=false;Require(manager.GetData(1)==null&&reads==2&&manager.Initialized,"retry rebuilds dictionary; null parsed list still marks initialized");
                manager.GetData(1);Require(reads==2,"null table is cached until reset");
            });
            check("outgame-shop-original-imported-video-buttons",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/Outgame/ShopUI");
                var buttons=prefab.GetComponentsInChildren<OutgameVideoButton>(true);Require(buttons.Length==5,"all five source UIVideoBtn components imported");
                var gold=prefab.transform.Find("btn_addGold").GetComponent<OutgameVideoButton>();var diamonds=prefab.transform.Find("btn_addDiamond").GetComponent<OutgameVideoButton>();
                Require(gold.videoID==1001&&diamonds.videoID==1006&&gold.AutoCallBtnShow&&diamonds.AutoCallBtnShow,"source video ids and auto-show flags retained");
                foreach(var button in buttons)Require(button.targetGraphic==button.GetComponent<UnityEngine.UI.Image>()&&button.onClick.GetPersistentEventCount()==0,"source targetGraphic and empty persistent click event retained");
                Require(gold.animationTriggers.selectedTrigger=="Highlighted"&&gold.navigation.mode==UnityEngine.UI.Navigation.Mode.Automatic,"source animation trigger and navigation retained");
            });
            check("outgame-shop-prefab-pointer-through-video-host-to-inventory",()=>{
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));page.SetActive(false);
                var eventRoot=new GameObject("events",typeof(UnityEngine.EventSystems.EventSystem));
                try{
                    var state=new OutgameLocalInventoryState();var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var toolProbe=new ProjectShopProbe();
                    var tools=new OutgameToolDispatcher(inv,toolProbe,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                    var fly=new ShopFlyProbe{Target=page.transform,Root=page.transform};fly.Start=new OutgameFlyToolStart(tools,fly);
                    var feedback=page.AddComponent<OutgameShopFeedback>();feedback.Bind(inv,key=>"",fly);
                    var sdk=new VideoButtonHostProbe();var adsMessages=new OutgameMessageDispatcher();
                    var adsFlow=new OutgameAdsVideoFlow(()=>10,delay=>System.Threading.Tasks.Task.CompletedTask,(flag,id)=>sdk.Trace.Add("platform:"+flag+":"+id),()=>adsMessages,flag=>{},text=>{},playing=>{},text=>OutgameCallbackJson.DataParse(text,x=>{}));
                    sdk.ShowOverride=adsFlow.ShowVide;var sourceConfig=new OutgameVideoButtonManager(()=>true,()=>false,OutgameVideoButtonManager.ReadRecoveredJson,x=>{},x=>{},x=>{});sdk.DataLookup=sourceConfig.GetData;var host=OutgameShopVideoBindings.Bind(page,sdk,c=>{},()=>{},()=>new OutgameMessageDispatcher(),new OutgameVideoPlayCooldown());
                    var lifecycle=new OutgameShopLifecycle(host,args=>{},args=>{},args=>{},feedback);lifecycle.OpenLater();
                    var button=page.transform.Find("btn_addGold").GetComponent<OutgameVideoButton>();button.CheckCanShowVideo();button.SetButtonState(2);page.SetActive(true);sdk.Trace.Clear();
                    button.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(eventRoot.GetComponent<UnityEngine.EventSystems.EventSystem>()));
                    Require(sdk.Trace.Contains("ad:0:1001")&&sdk.Trace.Contains("platform:0:1001")&&state.goldNum==0,"real prefab pointer routes original videoID without pre-grant");
                    adsFlow.AfterVideo("{\"videoFlag\":0,\"result\":1}");Require(state.goldNum==0,"failed platform callback grants nothing");
                    adsFlow.AfterVideo("{\"videoFlag\":0,\"result\":0}");Require(state.goldNum==50&&fly.Request.ItemId==1001,"successful platform callback reaches shop lifecycle reward and dispatcher");
                    Require(string.Join(",",toolProbe.Trace)=="changed:1001,get:1001,save","source notify/report/save side effects retained");
                }finally{UnityEngine.Object.DestroyImmediate(page);UnityEngine.Object.DestroyImmediate(eventRoot);}
            });
            check("outgame-native-video-button-pointer-to-sdk-callback",()=>{
                var node=new GameObject("native video");node.SetActive(false);var eventRoot=new GameObject("events",typeof(UnityEngine.EventSystems.EventSystem));
                try{
                    var host=new VideoButtonHostProbe();var button=node.AddComponent<OutgameVideoButton>();button.videoID=8;button.Bind(host,new OutgameVideoPlayCooldown());
                    button.AddVideoBtnEvent(ready=>host.Trace.Add("click-event:"+ready));button.onClick.AddListener(()=>host.Trace.Add("native-click"));button.AddVideoPlayCallBack(success=>host.Trace.Add("completion:"+success));
                    Require(button.CheckCanShowVideo()&&button.ButtonState==2&&button.ReportState==2,"source manual-show readiness activates native button");host.Trace.Clear();
                    button.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(eventRoot.GetComponent<UnityEngine.EventSystems.EventSystem>()){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left});
                    Require(string.Join(",",host.Trace)=="audio:1:42,track:3,click:original:a:b,click-event:True,native-click,play:original:a:b,ad:0:8","native Button event runs between original click and play reports");
                    host.Trace.Clear();host.Pending("sdk-result",true);Require(string.Join(",",host.Trace)=="success:original:a:b,event:videoreward:fixture-game:True,event:videoreward_fixture-game:original:True,completion:True","SDK callback reports source success events before native completion listeners");
                }finally{UnityEngine.Object.DestroyImmediate(node);UnityEngine.Object.DestroyImmediate(eventRoot);}
            });
            check("outgame-native-video-button-config-lock-and-state",()=>{
                var node=new GameObject("native config");node.SetActive(false);
                try{
                    var host=new VideoButtonHostProbe();var button=node.AddComponent<OutgameVideoButton>();button.videoID=8;button.Bind(host,new OutgameVideoPlayCooldown());
                    button.SetVideoBtnData("override","x","y");host.Trace.Clear();var original=button.Data;host.Data=new OutgameVideoButtonData{id=9,ReportLable="new"};button.ResetVideoBtnData(9);
                    Require(button.videoID==9&&ReferenceEquals(button.Data,original)&&button.ReportLabel=="override"&&string.Join(",",host.Trace)=="open:5:False","locked report parameters also preserve existing source data while id changes");
                    button.CreateVideoReport();button.SetButtonState(2);Require(button.ReportState==2&&!node.activeSelf,"state2 emits ready report after creation without activating object");
                    host.TrackAction=state=>throw new InvalidOperationException("track");bool failed=false;try{button.CreateVideoReport();}catch(InvalidOperationException){failed=true;}Require(failed&&button.ReportState==2,"report state only changes after successful TrackVideo");
                    host.TrackAction=null;host.Trace.Clear();button.VideoBtnShow(false);Require(string.Join(",",host.Trace)=="ready,unable:override::","readiness queried even when show=false; unable report only uses label");
                    host.Trace.Clear();button.ReportVideoPlayOver();Require(host.Trace.Contains("success:override:x:y")&&host.Trace.Contains("event:videoreward_fixture-game:original:True"),"success transport uses overrides but event label comes from original data");
                }finally{UnityEngine.Object.DestroyImmediate(node);}
            });
            check("outgame-native-video-button-disabled-base-does-not-cancel-video",()=>{
                var node=new GameObject("native disabled");node.SetActive(false);var eventRoot=new GameObject("events",typeof(UnityEngine.EventSystems.EventSystem));
                try{
                    var host=new VideoButtonHostProbe();var button=node.AddComponent<OutgameVideoButton>();button.Bind(host,new OutgameVideoPlayCooldown());button.CheckCanShowVideo();button.interactable=false;
                    int native=0;button.onClick.AddListener(()=>native++);host.Trace.Clear();button.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(eventRoot.GetComponent<UnityEngine.EventSystems.EventSystem>()));
                    Require(native==0&&host.Pending!=null&&host.Trace.Contains("ad:0:0"),"base Button interactability blocks its own event, while original subclass still calls advertising host");
                }finally{UnityEngine.Object.DestroyImmediate(node);UnityEngine.Object.DestroyImmediate(eventRoot);}
            });
            check("outgame-video-click-shared-cooldown-and-source-order",()=>{
                var shared=new OutgameVideoPlayCooldown();var first=new VideoClickProbe{UnscaledTime=10};var second=new VideoClickProbe{UnscaledTime=10};
                var a=new OutgameVideoClick(first,shared);var b=new OutgameVideoClick(second,shared);
                a.OnPointerClick();Require(string.Join(",",first.Trace)=="audio,click-report,click:True,base,play-report,show"&&a.ClickUntil==11&&shared.Until==15,"audio, per-button lock, click event/base then global lock/report/ad order");
                b.OnPointerClick();Require(second.Trace.Count==2&&second.Trace[0]=="audio"&&second.Trace[1].Contains("播放CD"),"shared cooldown blocks another button after audio");
                first.UnscaledTime=11;a.Update();Require(a.Clicking,"equal click deadline remains locked");first.UnscaledTime=11.01f;a.Update();Require(!a.Clicking&&shared.Playing,"only per-button deadline expired");
                second.UnscaledTime=15;b.Update();Require(shared.Playing,"equal shared deadline remains locked");second.UnscaledTime=15.01f;b.Update();Require(!shared.Playing,"any button update can clear shared cooldown");
                second.Trace.Clear();b.OnPointerClick();Require(second.Trace.Contains("show")&&shared.Until==20.01f,"second button can play after strict deadline");
            });
            check("outgame-video-click-mutation-and-failure-boundaries",()=>{
                var shared=new OutgameVideoPlayCooldown();var h=new VideoClickProbe{HasData=false};var click=new OutgameVideoClick(h,shared);click.OnPointerClick();Require(h.Trace.Count==0&&!click.Clicking,"missing data exits before audio and locks");
                h.HasData=true;h.ButtonState=1;h.BaseAction=()=>h.ButtonState=2;click.OnPointerClick();Require(h.Trace.Contains("click:False")&&h.Trace.Contains("show"),"state re-read after native Button callback can enable video");
                shared=new OutgameVideoPlayCooldown();h=new VideoClickProbe();h.BaseAction=()=>h.ButtonState=1;click=new OutgameVideoClick(h,shared);click.OnPointerClick();Require(!shared.Playing&&!h.Trace.Contains("show"),"base callback can suppress video after ready click event");
                h.Trace.Clear();h.UnscaledTime=100;click.OnPointerClick();Require(h.Trace.Count==2&&h.Trace[1].Contains("点击CD"),"click does not expire cooldown itself; Update owns expiry");
                click.Update();h.BaseAction=null;h.ButtonState=2;h.ReportAction=()=>throw new InvalidOperationException("report");h.Trace.Clear();bool failed=false;try{click.OnPointerClick();}catch(InvalidOperationException){failed=true;}
                Require(failed&&click.Clicking&&shared.Playing&&shared.Until==105&&!h.Trace.Contains("show"),"play report failure retains both locks and never opens ad");
            });
            check("outgame-video-click-to-completion-event-chain",()=>{
                var node=new GameObject("video chain");
                try{
                    var completion=node.AddComponent<OutgameVideoButtonCompletion>();var trace=new List<string>();
                    completion.Bind(()=>1,force=>{},()=>trace.Add("success-report"),()=>trace.Add("failure-report"));
                    completion.AddVideoBtnEvent(ready=>trace.Add("click:"+ready));completion.AddVideoPlayCallBack(success=>trace.Add("complete:"+success));
                    var h=new VideoClickProbe{Completion=completion};h.ShowAction=()=>completion.VideoCallBack("fixture-ad-result",true);
                    var shared=new OutgameVideoPlayCooldown();var click=new OutgameVideoClick(h,shared);click.OnPointerClick();
                    Require(string.Join(",",trace)=="click:True,success-report,complete:True"&&shared.Playing,"ad callback reaches native event chain; completion does not reset playback cooldown");
                }finally{UnityEngine.Object.DestroyImmediate(node);}
            });
            check("outgame-video-delay-report-scaled-wait-and-sdk-gate",()=>{
                var trace=new List<string>();bool open=false;
                var routine=OutgameVideoButtonCompletion.DelayReport(1,()=>trace.Add("create"),()=>{trace.Add("sdk");return open;},()=>trace.Add("ready"));
                Require(routine.MoveNext()&&routine.Current is WaitForSeconds&&trace.Count==0,"source yields scaled wait before all reporting");
                Require(!routine.MoveNext()&&string.Join(",",trace)=="create,sdk","closed SDK omits ready report");
                trace.Clear();open=true;routine=OutgameVideoButtonCompletion.DelayReport(0,()=>trace.Add("create"),()=>{trace.Add("sdk");return open;},()=>trace.Add("ready"));routine.MoveNext();routine.MoveNext();
                Require(string.Join(",",trace)=="create,sdk,ready","report creation precedes SDK query and ready report");
            });
            check("outgame-video-completion-source-report-order",()=>{
                var parent=new GameObject("video parent");var node=new GameObject("video");node.transform.SetParent(parent.transform);
                try{
                    var video=node.AddComponent<OutgameVideoButtonCompletion>();var trace=new List<string>();int mode=2;
                    video.Bind(()=>mode,force=>trace.Add("delay:"+force),()=>trace.Add("success"),()=>trace.Add("failure"));
                    video.AddVideoPlayCallBack(success=>trace.Add("reward:"+success));video.ReportState=7;
                    parent.SetActive(false);video.VideoCallBack("unused",true);
                    Require(video.ReportState==0&&string.Join(",",trace)=="delay:1,success,reward:True","activeSelf, not activeInHierarchy, controls report restart before completion");
                    trace.Clear();node.SetActive(false);video.VideoCallBack(null,false);Require(string.Join(",",trace)=="failure,reward:False","inactive button still reports failure and invokes event");
                    trace.Clear();mode=1;video.ReportState=9;video.RemoveVideoBtnListeners();video.VideoCallBack(null,true);Require(video.ReportState==9&&string.Join(",",trace)=="success","other show type retains report state; remove listeners suppresses reward only");
                    video.AddVideoPlayCallBack(success=>trace.Add("unexpected"));video.Bind(()=>2,force=>{},()=>throw new InvalidOperationException("report"),()=>{});bool failed=false;
                    try{video.VideoCallBack(null,true);}catch(InvalidOperationException){failed=true;}Require(failed&&!trace.Contains("unexpected"),"report exception prevents reward event");
                }finally{UnityEngine.Object.DestroyImmediate(parent);}
            });
            check("outgame-shop-native-video-event-to-inventory",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try{
                    var state=new OutgameLocalInventoryState();var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var probe=new ProjectShopProbe();
                    var tools=new OutgameToolDispatcher(inv,probe,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                    var fly=new ShopFlyProbe{Target=root.transform,Root=root.transform};fly.Start=new OutgameFlyToolStart(tools,fly);
                    var feedback=root.AddComponent<OutgameShopFeedback>();feedback.Bind(inv,key=>"",fly);
                    var gold=root.transform.Find("btn_addGold").gameObject.AddComponent<OutgameVideoButtonCompletion>();
                    var diamonds=root.transform.Find("btn_addDiamond").gameObject.AddComponent<OutgameVideoButtonCompletion>();
                    gold.Bind(()=>1,force=>{},()=>{},()=>{});diamonds.Bind(()=>1,force=>{},()=>{},()=>{});
                    var host=new OutgameShopEventHost(cb=>gold.AddVideoPlayCallBack(cb.Invoke),cb=>diamonds.AddVideoPlayCallBack(cb.Invoke),c=>{},()=>{},()=>new OutgameMessageDispatcher());
                    var lifecycle=new OutgameShopLifecycle(host,args=>{},args=>{},args=>{},feedback);lifecycle.OpenLater();
                    gold.VideoCallBack(null,false);diamonds.VideoCallBack(null,false);Require(state.goldNum==0&&state.diamondsNum==0,"failed native video events grant nothing");
                    gold.VideoCallBack(null,true);diamonds.VideoCallBack(null,true);Require(state.goldNum==50&&state.diamondsNum==20,"native UnityEvents reach original reward amounts and inventory dispatcher");
                    lifecycle.Dispose();gold.VideoCallBack(null,true);Require(state.goldNum==100,"source shop Dispose does not remove video listeners");
                    gold.RemoveVideoBtnListeners();gold.VideoCallBack(null,true);Require(state.goldNum==100,"explicit video listener removal prevents further grant");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-message-dispatcher-original-multicast-semantics",()=>{
                var messages=new OutgameMessageDispatcher();var trace=new List<int>();
                Action<object[]> duplicate=args=>trace.Add((int)args[0]);messages.AddListener("x",duplicate);messages.AddListener("x",duplicate);
                messages.SendMessage("x",new object[]{2});Require(trace.Count==2,"duplicate listener is retained");messages.RemoveListener("x",duplicate);trace.Clear();messages.SendMessage("x",new object[]{3});Require(trace.Count==1&&trace[0]==3,"remove deletes only last matching occurrence");
                messages.RemoveListener("x",duplicate);messages.SendMessage("x");messages.RemoveListener("absent",null);
                messages.AddListener("null",null);bool threw=false;try{messages.SendMessage("null");}catch(NullReferenceException){threw=true;}Require(threw,"stored null callback is not silently ignored");messages.RemoveListener("null",null);messages.SendMessage("null");
                Action<object[]> later=args=>trace.Add(9);messages.AddListener("change",args=>messages.RemoveListener("change",later));messages.AddListener("change",later);messages.SendMessage("change");Require(trace[trace.Count-1]==9,"in-flight multicast retains removed callback");
                messages.AddListener("fail",args=>throw new InvalidOperationException("callback"));messages.AddListener("fail",later);int before=trace.Count;threw=false;try{messages.SendMessage("fail");}catch(InvalidOperationException){threw=true;}Require(threw&&trace.Count==before,"callback failure stops later callbacks");
            });
            check("outgame-shop-concrete-message-host-refresh",()=>{
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try{
                    var state=new OutgameLocalInventoryState{ToolValue=9};var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var feedback=page.AddComponent<OutgameShopFeedback>();feedback.Bind(inv,key=>"value:",null);
                    var messages=new OutgameMessageDispatcher();Action<bool> gold=null,diamonds=null;int selected=0;
                    var host=new OutgameShopEventHost(callback=>gold=callback,callback=>diamonds=callback,coroutine=>{},()=>{},()=>messages);
                    var lifecycle=new OutgameShopLifecycle(host,args=>selected++,args=>{},args=>{},feedback);lifecycle.OpenLater();messages.SendMessage("ToolChange");messages.SendMessage("ChooseSkin");
                    var label=page.transform.Find("bottom/skinGroup_Shop/Viewport/Content/Shop_Diamond/shopInfo/toolValueNum/txt_toolValue").GetComponent<UnityEngine.UI.Text>();Require(label.text=="value:9"&&selected==1&&gold!=null&&diamonds!=null,"real dispatcher reaches original page through concrete lifecycle host");
                    lifecycle.Dispose();state.ToolValue=10;messages.SendMessage("ToolChange");Require(label.text=="value:9","disposed page stops receiving original global message");
                }finally{UnityEngine.Object.DestroyImmediate(page);}
            });
            check("outgame-shop-page-lifecycle-feedback-connection",()=>{
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try{
                    var state=new OutgameLocalInventoryState{ToolValue=3};var inventory=new OutgameLocalInventory(state,"{\"Datas\":[]}");
                    var feedback=page.AddComponent<OutgameShopFeedback>();feedback.Bind(inventory,key=>"points:",null);
                    var host=new ShopEventsProbe();int selected=0,skinState=0;
                    var lifecycle=new OutgameShopLifecycle(host,args=>selected+=(int)args[0],args=>selected+=(int)args[0],args=>skinState++,feedback);
                    lifecycle.OpenLater();Require(string.Join(",",host.Trace)=="add:ChooseSkin,add:ChooseSoldier,add:ValnetineStatueChanged,add:ToolChange,gold,diamonds","original registration and video callback order");
                    host.Listeners["ChooseSkin"](new object[]{2});host.Listeners["ChooseSoldier"](new object[]{3});host.Listeners["ValnetineStatueChanged"](null);
                    state.ToolValue=7;host.Listeners["ToolChange"](null);
                    var label=page.transform.Find("bottom/skinGroup_Shop/Viewport/Content/Shop_Diamond/shopInfo/toolValueNum/txt_toolValue").GetComponent<UnityEngine.UI.Text>();
                    Require(selected==5&&skinState==1&&label.text=="points:7","registered listeners reach actual page feedback and selection callbacks");
                    host.Gold(false);host.Diamonds(false);lifecycle.InitializationCoroutine=new object();lifecycle.Dispose();
                    Require(host.Trace[host.Trace.Count-2]=="stop"&&host.Trace[host.Trace.Count-1]=="select-card"&&lifecycle.InitializationCoroutine!=null,"dispose stops saved coroutine without clearing it and then accesses SelectCardControl singleton");Require(host.Listeners.Count==0&&host.Gold!=null&&host.Diamonds!=null,"source disposal removes messages but does not remove video callbacks");
                }finally{UnityEngine.Object.DestroyImmediate(page);}
            });
            check("outgame-provider-bundle-destruction-and-partial-failure",()=>{
                var owner=new OutgameBundleReference{RefCount=1};var dependency=new OutgameBundleReference();
                var dependencies=new OutgameBundleDependencies(new List<OutgameBundleReference>{dependency,dependency});dependencies.Reference();Require(dependency.RefCount==2,"source does not deduplicate repeated dependencies");
                var provider=new OutgameAssetProvider(()=>false,message=>{}){OwnerBundle=owner,DependBundles=dependencies};var handle=provider.CreateHandle("active",()=>false);
                provider.Destroy();Require(provider.IsDestroyed&&owner.RefCount==0&&dependency.RefCount==0&&provider.RefCount==1&&provider.OwnerBundle==null&&provider.DependBundles==null,"destroy does not enforce CanDestroy or release asset handles; clears ownership after decrement");
                provider.Destroy();Require(owner.RefCount==0&&dependency.RefCount==0,"cleared ownership prevents repeated decrements");
                var list=new List<OutgameBundleReference>{dependency,null};var failing=new OutgameBundleDependencies(list);provider.OwnerBundle=owner;provider.DependBundles=failing;
                bool failed=false;try{provider.Destroy();}catch(NullReferenceException){failed=true;}
                Require(failed&&provider.IsDestroyed&&owner.RefCount==-1&&provider.OwnerBundle==null&&dependency.RefCount==-1&&ReferenceEquals(provider.DependBundles,failing),"dependency failure preserves earlier decrements and uncleared dependency group");
                list.RemoveAt(1);provider.Destroy();Require(dependency.RefCount==-2&&provider.DependBundles==null,"retry repeats remaining group release without counter normalization");
            });
            check("outgame-provider-completion-snapshot-release-and-task",()=>{
                var warnings=new List<string>();bool suppressed=false;var provider=new OutgameAssetProvider(()=>suppressed,warnings.Add);
                var first=provider.CreateHandle("a",()=>false);var second=provider.CreateHandle("b",()=>false);var trace=new List<string>();
                first.Completed+=h=>{trace.Add("a");second.Release();};second.Completed+=h=>trace.Add("b");var task=provider.Task;
                provider.Status=4;provider.InvokeCompletion();Require(string.Join(",",trace)=="a"&&provider.RefCount==1&&task.IsCompleted&&provider.Progress==1,"completion snapshot skips released next handle and completes task after callbacks");
                first.Release();Require(provider.CanDestroy,"completed zero-reference provider can destroy");
                bool failed=false;try{provider.ReleaseHandle(first);}catch(Exception e){failed=e.Message=="Should never get here !";}Require(failed&&provider.RefCount==0&&warnings.Count>0,"missing handle warns at zero then throws without decrement");
                suppressed=true;provider.Progress=.25f;provider.InvokeCompletion();Require(!provider.IsDone&&provider.Progress==.25f,"source global suppression gate prevents completion progress changes");
                var pending=new OutgameAssetProvider(()=>false,warnings.Add);var bad=pending.CreateHandle("bad",()=>false);Action<OutgameAssetHandle> throws=h=>throw new InvalidOperationException("callback");bad.Completed+=throws;var pendingTask=pending.Task;pending.Status=5;
                failed=false;try{pending.InvokeCompletion();}catch(InvalidOperationException){failed=true;}Require(failed&&!pendingTask.IsCompleted&&pending.Progress==1&&pending.IsDone,"callback exception leaves task pending despite terminal status and progress");
                bad.Completed-=throws;pending.InvokeCompletion();Require(pendingTask.IsCompleted,"later successful dispatch completes existing task");
            });
            check("outgame-asset-handle-completion-subscriptions",()=>{
                var provider=new AssetProviderProbe();var warnings=new List<string>();var handle=new OutgameAssetHandle("pending",provider,()=>false,warnings.Add);var trace=new List<string>();
                Action<OutgameAssetHandle> second=h=>trace.Add("second");
                Action<OutgameAssetHandle> first=h=>{trace.Add("first");h.Completed-=second;};
                handle.Completed+=first;handle.Completed+=second;handle.InvokeCompleted();
                Require(string.Join(",",trace)=="first,second","dispatch snapshots multicast invocation despite removal during callback");
                trace.Clear();handle.InvokeCompleted();Require(string.Join(",",trace)=="first","dispatch retains current subscriptions for subsequent invocation");
                handle.Completed-=first;provider.Done=true;handle.Completed+=second;Require(trace.Count==2&&trace[1]=="second","already done subscription runs immediately");
                trace.Clear();handle.InvokeCompleted();Require(trace.Count==0,"immediate callback is not stored in event list");
                provider.Done=false;Action<OutgameAssetHandle> failing=h=>throw new InvalidOperationException("callback");handle.Completed+=failing;handle.Completed+=second;
                bool failed=false;try{handle.InvokeCompleted();}catch(InvalidOperationException){failed=true;}Require(failed&&trace.Count==0,"callback error propagates and prevents later delegates");
                handle.Completed-=failing;handle.InvokeCompleted();Require(trace.Count==1,"failed dispatch did not clear subscriptions");
                handle.Release();bool invalid=false;try{handle.Completed-=second;}catch(Exception e){invalid=e.GetType()==typeof(Exception)&&e.Message=="RuntimeAssetHandle is invalid";}Require(invalid&&warnings.Count>0,"removal on released handle warns then throws original exception");
            });
            check("outgame-asset-handle-validity-release-and-native-instance",()=>{
                var prefab=new GameObject("original-name");GameObject instance=null;
                try{
                    prefab.SetActive(false);var warnings=new List<string>();var provider=new AssetProviderProbe{Asset=prefab};bool compatibility=false;
                    var handle=new OutgameAssetHandle("model/gold",provider,()=>compatibility,warnings.Add);
                    instance=handle.Instantiate();Require(instance.activeSelf&&instance.name==prefab.name&&ReferenceEquals(handle.MainObject,prefab),"source default instantiate activates and restores source name");
                    provider.Fail=true;bool failed=false;try{handle.Release();}catch(InvalidOperationException){failed=true;}
                    Require(failed&&ReferenceEquals(handle.MainObject,prefab),"failed provider release retains handle connection");
                    provider.Fail=false;handle.Release();handle.Release();Require(provider.Releases==2&&warnings[warnings.Count-1]=="Operation handle is released : model/gold"&&handle.MainObject==null,"successful release detaches; repeated release warns without provider call");
                    compatibility=true;handle.DirectMainObject=prefab;handle.Release();Require(ReferenceEquals(handle.MainObject,prefab)&&provider.Releases==2,"compatibility mode bypasses provider release and reads direct main object");
                    compatibility=false;provider.Destroyed=true;var dead=new OutgameAssetHandle("dead",provider,()=>false,warnings.Add);Require(dead.Instantiate()==null&&warnings[warnings.Count-1]=="Provider is destroyed : dead","destroyed provider suppresses instance creation");
                }finally{if(instance!=null)UnityEngine.Object.DestroyImmediate(instance);UnityEngine.Object.DestroyImmediate(prefab);}
            });
            check("outgame-normal-pool-destruction-order-and-retained-cache",()=>{
                var root=new GameObject("destroy-root");var prefab=new GameObject("destroy-prefab");var made=new List<GameObject>();
                try{
                    var resources=new NormalPoolResourcesProbe();var pool=new OutgameObjectPool("effects",false,20,float.MaxValue,0);
                    var normal=new OutgameNormalPool(pool,root.transform,true,resources);
                    var handle=new NormalPoolHandleProbe{Source=prefab,Create=()=>{var item=UnityEngine.Object.Instantiate(prefab);made.Add(item);return item;}};
                    normal.Spawn("gold",null);resources.Pending[0](handle);
                    var lifetime=new NormalPoolLifetimeProbe();normal.DestroyObjectPool(lifetime);
                    Require(string.Join(",",lifetime.Trace)=="manager:effects,root:destroy-root,handle,unused","source manager destruction precedes root, handle and global unused cleanup");
                    lifetime.Trace.Clear();normal.DestroyObjectPool(lifetime);
                    Require(string.Join(",",lifetime.Trace)=="manager:effects,root:destroy-root,handle,unused","source retains new-resource cache instead of inventing idempotent cleanup");
                    lifetime.Trace.Clear();lifetime.FailRelease=true;bool failed=false;
                    try{normal.DestroyObjectPool(lifetime);}catch(InvalidOperationException){failed=true;}
                    Require(failed&&string.Join(",",lifetime.Trace)=="manager:effects,root:destroy-root,handle","resource failure propagates before global cleanup");
                }finally{foreach(var item in made)UnityEngine.Object.DestroyImmediate(item);UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-normal-pool-deferred-cache-and-reuse",()=>{
                var root=new GameObject("normal-pool-root");var prefab=new GameObject("normal-pool-source");var made=new List<GameObject>();
                try{
                    var resources=new NormalPoolResourcesProbe();int firstCreates=0,secondCreates=0;
                    var first=new NormalPoolHandleProbe{Source=prefab,Create=()=>{firstCreates++;var go=UnityEngine.Object.Instantiate(prefab);made.Add(go);return go;}};
                    var second=new NormalPoolHandleProbe{Source=prefab,Create=()=>{secondCreates++;var go=UnityEngine.Object.Instantiate(prefab);made.Add(go);return go;}};
                    var pool=new OutgameObjectPool("normal",false,20,float.MaxValue,0);var normal=new OutgameNormalPool(pool,root.transform,true,resources);
                    GameObject a=null,b=null;normal.Spawn("gold",x=>a=x);normal.Spawn("gold",x=>b=x);
                    Require(resources.Pending.Count==2&&pool.Count==0,"concurrent misses are not merged into an invented single load");
                    resources.Pending[1](second);resources.Pending[0](first);
                    Require(a!=null&&b!=null&&firstCreates==1&&secondCreates==1,"each completion instantiates its own delivered handle");
                    normal.Spawn("gold",x=>{});Require(secondCreates==2&&resources.Pending.Count==2,"first completed handle remains cached when later completion arrives");
                    normal.Unspawn(b);GameObject reused=null;normal.Spawn("gold",x=>reused=x);
                    Require(ReferenceEquals(reused,b)&&secondCreates==2,"free object hit bypasses cache loader and instantiation");
                    int calls=0;normal.Spawn("missing",x=>calls++);resources.Pending[2](null);Require(calls==0,"null async resource completion does not invoke user callback");
                    pool.Shutdown();Require(resources.Released.Count==3,"all instantiated pooled objects reach resource release provider");
                }finally{foreach(var go in made)UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-pool-source-first-free-and-release-order",()=>{
                var a=new GameObject("first");var b=new GameObject("second");var released=new List<GameObject>();
                try{
                    var pool=new OutgameObjectPool("effects",false,10,float.MaxValue,0);
                    var first=new OutgamePooledPrefab("gold",null,a,null,item=>released.Add(item));var second=new OutgamePooledPrefab("gold",null,b,null,item=>released.Add(item));
                    pool.Register(first,false);pool.Register(second,false);
                    Require(ReferenceEquals(pool.Spawn("gold"),first)&&ReferenceEquals(pool.Spawn("gold"),second)&&pool.Spawn("gold")==null,"first free matching registered object; occupied entries skipped");
                    pool.Unspawn(a);Require(ReferenceEquals(pool.Spawn("gold"),first),"returned object stays at original registration position");
                    pool.Capacity=0;Require(pool.Count==2&&released.Count==0,"capacity never evicts active entries");
                    pool.Unspawn(b);Require(pool.Count==1&&released.Count==1&&ReferenceEquals(released[0],b),"return triggers excess capacity release");
                    pool.Shutdown();Require(pool.Count==0&&released.Count==2&&ReferenceEquals(released[1],a),"shutdown removes and releases active objects too");
                }finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
            });
            check("outgame-pool-expiry-filter-and-unscaled-update",()=>{
                var a=new GameObject("expiry");var b=new GameObject("future");int releases=0;
                try{
                    var first=new OutgamePooledPrefab("x",null,a,null,item=>releases++);var second=new OutgamePooledPrefab("x",null,b,null,item=>releases++);
                    var candidates=new LinkedList<OutgamePooledPrefab>();candidates.AddLast(first);candidates.AddLast(second);
                    var selected=OutgameObjectPool.DefaultReleaseObjectFilterCallback(candidates,0,second.LastUseTime);
                    Require(selected.Count==2&&candidates.Count==0,"expiration includes equality and ignores zero capacity release quota");
                    var pool=new OutgameObjectPool("expiry",false,10,float.MaxValue,0);pool.Register(first,true);
                    pool.ExpireTime=0;Require(pool.Count==1,"expiry excludes occupied objects");pool.Unspawn(a);Require(pool.Count==0&&releases==1,"zero expiry releases immediately after return");
                    pool.Register(second,false);Require(pool.Count==0&&releases==2,"register invokes release even below capacity");
                    pool.ExpireTime=float.MaxValue;pool.AutoReleaseInterval=1;pool.Register(second,false);
                    pool.Update(100,0);Require(pool.Count==1,"scaled delta is not automatic release clock");
                    pool.Release(1,(items,count,cutoff)=>items);Require(pool.Count==0&&releases==3,"custom release operates on source candidate references");
                    var throwing=new OutgamePooledPrefab("bad",null,a,null,item=>throw new InvalidOperationException("release failed"));pool.Register(throwing,false);
                    bool failed=false;try{pool.Shutdown();}catch(InvalidOperationException){failed=true;}Require(failed&&pool.Count==0,"release callback failure retains prior removal");
                }finally{UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);}
            });
            check("outgame-pool-entry-source-count-and-timestamp",()=>{
                var target=new GameObject("counted-pool-object");int releases=0;
                try{
                    target.SetActive(false);var item=new OutgamePooledPrefab("gold",null,target,null,value=>releases++);
                    var created=item.LastUseTime;var entry=new OutgamePoolEntry(item,true);
                    Require(entry.IsInUse&&entry.SpawnCount==1&&target.activeSelf&&item.LastUseTime==created,"initial spawned flag calls lifecycle without another timestamp assignment");
                    entry.Unspawn();Require(!entry.IsInUse&&entry.SpawnCount==0&&!target.activeSelf&&item.LastUseTime>=created,"return decrements and timestamps");
                    entry.Unspawn();Require(entry.SpawnCount==-1&&!entry.IsInUse,"source permits negative count after repeated return");
                    Require(ReferenceEquals(entry.Spawn(),item)&&entry.SpawnCount==0&&!entry.IsInUse&&target.activeSelf,"following spawn increments negative count without normalization");
                    entry.Spawn();Require(entry.IsInUse&&entry.SpawnCount==1&&ReferenceEquals(entry.Peek(),item)&&entry.Name=="gold"&&!entry.Locked,"wrapper reads original object and tracks use");
                    entry.Release();Require(releases==1&&entry.SpawnCount==1,"release forwards without resetting count");
                    bool rejected=false;try{new OutgamePoolEntry(null,false);}catch(InvalidOperationException e){rejected=e.Message=="Object is invalid.";}Require(rejected,"null pooled object rejected");
                }finally{UnityEngine.Object.DestroyImmediate(target);}
            });
            check("outgame-pooled-prefab-base-initialization",()=>{
                bool rejected=false;
                try{new OutgamePooledPrefab("missing",null,null,null,null);}catch(InvalidOperationException e){rejected=e.Message=="Target 'missing' is invalid.";}
                Require(rejected,"managed null target rejected with source message");
                var item=new GameObject("base-pool-object");
                try{
                    var before=DateTime.Now;var entry=new OutgamePooledPrefab(null,null,item,null,null);var after=DateTime.Now;
                    Require(entry.Name==string.Empty&&!entry.Locked&&entry.Priority==0&&entry.LastUseTime>=before&&entry.LastUseTime<=after,"source default name, flags and local creation timestamp");
                    UnityEngine.Object.DestroyImmediate(item);
                    var destroyed=new OutgamePooledPrefab("destroyed",null,item,null,null);
                    Require(ReferenceEquals(destroyed.Target,item),"destroyed Unity wrapper remains a managed target");
                    destroyed.OnSpawn();destroyed.OnUnspawn();
                }finally{if(item!=null)UnityEngine.Object.DestroyImmediate(item);}
            });
            check("outgame-original-pooled-prefab-parent-and-visibility-lifecycle",()=>{
                var parent=new GameObject("owner");var pool=new GameObject("pool");var item=UnityEngine.Object.Instantiate(OutgameFlyCurrencyAssets.Load("model/entity/golditem"));
                try{
                    parent.transform.position=new Vector3(10,0,0);pool.transform.position=new Vector3(-10,0,0);item.transform.SetParent(parent.transform,false);item.transform.localPosition=new Vector3(3,4,0);item.transform.localScale=new Vector3(2,2,2);
                    GameObject released=null;var entry=new OutgamePooledPrefab("gold",null,item,pool.transform,value=>released=value);
                    entry.OnUnspawn();Require(!item.activeSelf&&item.transform.parent==pool.transform&&item.transform.localPosition==new Vector3(3,4,0),"source hides then reparents without preserving world position");
                    entry.OnSpawn();Require(item.activeSelf&&item.transform.parent==parent.transform&&item.transform.localPosition==new Vector3(3,4,0)&&item.transform.localScale==new Vector3(2,2,2),"source restores previous parent without resetting local transform");
                    entry.Release();Require(released==item,"release forwards original object to actual resource releaser");
                    entry.OnUnspawn();UnityEngine.Object.DestroyImmediate(parent);entry.OnSpawn();Require(item.activeSelf&&item.transform.parent==pool.transform,"destroyed previous parent skips reparent but still activates");
                }finally{UnityEngine.Object.DestroyImmediate(item);UnityEngine.Object.DestroyImmediate(pool);if(parent!=null)UnityEngine.Object.DestroyImmediate(parent);}
            });
            check("outgame-fly-scatter-source-draw-order-and-runtime-missing-target",()=>{
                var trace=new List<string>();int call=0;float sinArg=0,cosArg=0;
                var scatter=new OutgameFlyScatter((min,max)=>{trace.Add(min+":"+max);return call++==0?360:20;},x=>{sinArg=x;trace.Add("sin");return .25f;},x=>{cosArg=x;trace.Add("cos");return .5f;});
                Vector2 point=scatter.Position(new Vector2(3,4));Require(point==new Vector2(4,4.5f)&&sinArg==360*.017f&&cosArg==sinArg,"source multiplier and radius without replacement by Deg2Rad");
                Require(string.Join(",",trace)=="0:360,10:20,sin,cos","original random draw and native trig evaluation order");
                var root=new GameObject("fly-runtime");try{
                    var state=new OutgameLocalInventoryState();var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var probe=new ProjectShopProbe();var tools=new OutgameToolDispatcher(inv,probe,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                    var runtime=root.AddComponent<OutgameFlyRuntime>();trace.Clear();runtime.Bind(tools,inv,null,null,root.transform,(group,id)=>trace.Add("voice:"+group+":"+id),()=>2099,(path,ready)=>{throw new Exception("unexpected spawn");},item=>{throw new Exception("unexpected unspawn");});
                    runtime.FlyMoney(50,root.transform,Vector3.zero,true,null,true);runtime.FlyDiamonds(20,root.transform,Vector3.zero,true,null,true);
                    Require(string.Join(",",trace)=="voice:1:2019,voice:1:2099"&&state.goldNum==0&&state.diamondsNum==0&&probe.Trace.Count==0,"source wrappers play voice before missing target suppresses grant/animation");
                    Require(root.GetComponent<OutgameFlyTweenRunner>()!=null,"native composition installs actual tween runner");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-fly-native-tween-forward-pulse-and-independent-movement",()=>{
                var root=new GameObject("fly-runner");var target=new GameObject("fly-target");
                try{
                    var runner=root.AddComponent<OutgameFlyTweenRunner>();int complete=0;
                    var move=runner.Move(target.transform,new Vector3(10,0,0),.4f,21,"move",true,()=>complete++);
                    runner.Advance(0,.2f);Require(Mathf.Abs(target.transform.position.x-8.660254f)<.00001f&&complete==0,"real Transform follows original OutCirc with scaled time paused");
                    runner.Advance(0,.2f);Require(target.transform.position==new Vector3(10,0,0)&&complete==1&&!move.IsActive,"movement reaches exact end and auto-kills");
                    target.transform.localScale=Vector3.one;var pulse=runner.CreateTargetTween(target.transform);runner.Advance(0,.1f);Require(target.transform.localScale==Vector3.one,"target pulse starts paused");
                    runner.PlayForward(pulse);runner.Advance(0,.2f);Require(pulse.IsComplete&&pulse.IsActive&&target.transform.localScale==new Vector3(1.2f,1.2f,0),"source target tween completes but stays active");
                    runner.PlayForward(pulse);runner.Advance(0,.2f);Require(target.transform.localScale==new Vector3(1,1,0),"return scale runs; completed forward pulse does not restart");
                    runner.Kill("Target_Tween",false);Require(!pulse.IsActive,"global target id kill");
                    var finish=runner.Move(target.transform,new Vector3(20,0,0),.5f,20,"forced",true,()=>complete++);runner.Kill("forced",true);
                    Require(target.transform.position==new Vector3(20,0,0)&&complete==2&&!finish.IsActive,"kill complete moves to end and invokes callback once");
                    runner.Kill("forced",true);Require(complete==2,"inactive tween not completed twice");
                }finally{UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-fly-global-id-registry-and-source-circular-easing",()=>{
                var ids=new OutgameFlyEffectIds();int first=ids.NextId();int second=ids.NextId();Require(first==1&&second==2,"original first id and monotonically allocated ids");
                Require(ids.NextSubId(first)=="fly_1_0"&&ids.NextSubId(first)=="fly_1_1"&&ids.NextSubId(second)=="fly_2_0","each animation stage consumes a unique sub id");
                Require(ids.SubIdCount(first)==2&&ids.SubIdCount(second)==1&&ids.SubIdCount(999)==-1,"source missing id sentinel");
                ids.RemoveSubIds(first);Require(ids.SubIdCount(first)==-1&&ids.NextId()==3,"removed ids are not recycled");
                bool threw=false;try{ids.NextSubId(first);}catch(KeyNotFoundException){threw=true;}Require(threw,"no implicit registration for removed id");
                Require(OutgameFlyEasing.Evaluate(20,0,.5f)==0&&OutgameFlyEasing.Evaluate(20,.5f,.5f)==1,"InCirc endpoints");
                Require(OutgameFlyEasing.Evaluate(21,0,.4f)==0&&OutgameFlyEasing.Evaluate(21,.4f,.4f)==1,"OutCirc endpoints");
                Require(Mathf.Abs(OutgameFlyEasing.Evaluate(20,.25f,.5f)-.1339746f)<.000001f&&Mathf.Abs(OutgameFlyEasing.Evaluate(21,.2f,.4f)-.8660254f)<.000001f,"source circular midpoint values");
                Require(float.IsNaN(OutgameFlyEasing.Evaluate(20,1,.5f)),"raw source evaluator does not clamp out-of-domain input");
            });
            check("outgame-fly-coroutine-native-icons-realtime-waits-and-arrivals",()=>{
                var target=new GameObject("currency",typeof(RectTransform),typeof(UnityEngine.UI.Text));var root=new GameObject("effects");var host=new FlyAnimationProbe();
                try{
                    var text=target.GetComponent<UnityEngine.UI.Text>();text.text="50";int callbacks=0;
                    var request=new OutgameFlyToolRequest{Id=9,ItemId=1001,Amount=50,Target=target.transform,Root=root.transform,Position=new Vector3(2,3,4),UpdateDisplayedValue=true,Completion=()=>callbacks++};
                    var animation=new OutgameFlyToolAnimation(host,id=>50);var routine=animation.Run(request);object wait=null;
                    for(int i=0;i<5;i++){
                        Require(routine.MoveNext()&&routine.Current is WaitForSecondsRealtime,"one real-time yield per original icon");
                        var current=(WaitForSecondsRealtime)routine.Current;Require(current.waitTime==.025f,"source spawn interval");if(wait!=null)Require(ReferenceEquals(wait,current),"original shared wait object");wait=current;
                        Require(host.Icons[i].transform.parent==root.transform&&host.Icons[i].transform.position==request.Position,"native prefab starts at source world position");
                    }
                    Require(routine.MoveNext()&&((WaitForSecondsRealtime)routine.Current).waitTime==1.1f,"source final realtime wait");
                    var scatter=host.Complete.ToArray();host.Complete.Clear();foreach(var callback in scatter)callback();Require(host.Complete.Count==5,"five scatter callbacks schedule five returns");
                    foreach(var callback in host.Complete.ToArray())callback();Require(text.text=="50"&&callbacks==1&&host.Icons.TrueForAll(x=>!x.activeSelf),"native label completes and icons return to pool");
                    Require(!routine.MoveNext()&&string.Join(",",host.Trace).EndsWith("stop:9,kill:Target_Tween:False"),"queue cleanup before global target tween kill without completion");
                    Require(host.Trace.FindAll(x=>x=="move:21:0.4").Count==5&&host.Trace.FindAll(x=>x=="move:20:0.5").Count==5,"original duration/easing commands");
                }finally{foreach(var icon in host.Icons)UnityEngine.Object.DestroyImmediate(icon);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(target);}
            });
            check("outgame-fly-cleanup-deferred-stop-strict-time-and-failure-state",()=>{
                var host=new FlyCleanupProbe();var cleanup=new OutgameFlyToolCleanup(host);cleanup.TrackCoroutine(1,"c1");cleanup.TrackTime(1,0);
                host.Now=4;cleanup.Update();Require(cleanup.PendingCount==0&&cleanup.NextCheck==5,"exact4 seconds does not queue; next check source expression");
                host.Now=5;cleanup.Update();Require(cleanup.PendingCount==0,"strict next-check threshold");
                host.Now=5.5f;cleanup.Update();Require(cleanup.PendingCount==1&&cleanup.ActiveCount==1&&cleanup.NextCheck==11.5f&&host.Trace.Count==0,"timeout only enqueues; next check accumulates previous value");
                cleanup.Update();Require(cleanup.ActiveCount==0&&cleanup.PendingCount==0&&string.Join(",",host.Trace)=="count:1,kill:fly_1_0:True,kill:fly_1_1:True,stop:c1,remove:1","kill-complete then stop/remove order on following update");
                cleanup.StopEffect(999);Require(cleanup.PendingCount==0,"unknown id ignored");
                cleanup.TrackCoroutine(2,"c2");cleanup.TrackTime(2,5.5f);cleanup.StopEffect(2);Require(cleanup.ActiveCount==1,"manual stop is deferred");host.ThrowKill=true;
                bool threw=false;try{cleanup.Update();}catch(InvalidOperationException){threw=true;}Require(threw&&cleanup.ActiveCount==1&&cleanup.PendingCount==1,"failed kill preserves queue and registrations");
                host.ThrowKill=false;cleanup.Update();Require(cleanup.ActiveCount==0&&cleanup.PendingCount==0,"retry drains retained stop");
                var duplicate=new OutgameFlyToolCleanup(host);duplicate.TrackCoroutine(3,"c3");duplicate.TrackTime(3,0);duplicate.StopEffect(3);duplicate.StopEffect(3);
                threw=false;try{duplicate.Update();}catch(KeyNotFoundException){threw=true;}Require(threw&&duplicate.ActiveCount==0&&duplicate.PendingCount==2,"source list does not deduplicate; second removal retains partial failure");
            });
            check("outgame-fly-currency-original-prefabs-and-sprite-bindings",()=>{
                string[] names={"golditem","diamonditem","strengthitem"};float[] sizes={70,64,70};
                for(int i=0;i<names.Length;i++){
                    var root=UnityEngine.Object.Instantiate(OutgameFlyCurrencyAssets.Load("model/entity/"+names[i]));
                    try{var rect=root.GetComponent<RectTransform>();var img=root.GetComponent<UnityEngine.UI.Image>();
                        Require(rect!=null&&rect.sizeDelta==new Vector2(sizes[i],sizes[i])&&rect.pivot==new Vector2(.5f,.5f)&&rect.localScale==Vector3.one,"original fly prefab geometry");
                        Require(img!=null&&img.sprite!=null&&img.sprite.texture!=null,"actual original image binding");
                        Require(root.transform.childCount==0,"original single-node prefab");
                    }finally{UnityEngine.Object.DestroyImmediate(root);}
                }
            });
            check("outgame-fly-collection-source-count-remainder-and-callback-order",()=>{
                int[] amounts={0,50,100,101,149,150,299,300,int.MinValue};int[] counts={5,5,5,6,6,7,9,10,10};
                for(int i=0;i<amounts.Length;i++)Require(OutgameFlyToolCollection.SourceCreateCount(amounts[i])==counts[i],"source particle count including unchecked overflow");
                var trace=new List<string>();var req=new OutgameFlyToolRequest{ItemId=1001,Amount=101,UpdateDisplayedValue=true,Completion=()=>trace.Add("complete")};
                var collection=new OutgameFlyToolCollection(req,"151",id=>151,v=>trace.Add("text:"+v),()=>trace.Add("pulse"));
                Require(collection.DisplayedValue==50&&collection.EndValue==151&&collection.Step==16&&collection.AssetPath=="model/entity/golditem","already refreshed label rolls back reward only for display");
                for(int i=0;i<6;i++)collection.Arrived(()=>trace.Add("pool"));
                Require(collection.DisplayedValue==151&&string.Join(",",trace).EndsWith("pool,pulse,text:151,complete"),"final particle corrects remainder before completion");
                Require(trace.FindAll(x=>x=="complete").Count==1,"callback at exact create count");
                var stale=new OutgameFlyToolCollection(req,"40",id=>151,v=>{},()=>{});Require(stale.DisplayedValue==40&&stale.EndValue==141,"stale label retains its own baseline");
                trace.Clear();req.UpdateDisplayedValue=false;var hidden=new OutgameFlyToolCollection(req,"151",id=>151,v=>trace.Add("unexpected-text"),()=>trace.Add("pulse"));
                for(int i=0;i<6;i++)hidden.Arrived(()=>trace.Add("pool"));Require(hidden.DisplayedValue==50&&!trace.Contains("unexpected-text")&&trace[trace.Count-1]=="complete","display flag does not suppress completion");
                bool threw=false;try{new OutgameFlyToolCollection(req,"1K",id=>151,v=>{},()=>{});}catch(FormatException){threw=true;}Require(threw,"source Int32.Parse is not replaced by formatted-number fallback");
                trace.Clear();var failure=new OutgameFlyToolCollection(req,"151",id=>151,v=>{},()=>trace.Add("pulse"));try{failure.Arrived(()=>{throw new InvalidOperationException();});}catch(InvalidOperationException){}Require(failure.Count==0&&trace.Count==0,"pool failure precedes count/pulse");
            });
            check("outgame-shop-native-video-feedback-and-immediate-fly-grant",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try{
                    var state=new OutgameLocalInventoryState{ToolValue=9};var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var toolProbe=new ProjectShopProbe();
                    var tools=new OutgameToolDispatcher(inv,toolProbe,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                    var fly=new ShopFlyProbe{Target=root.transform,Root=root.transform};fly.Start=new OutgameFlyToolStart(tools,fly);
                    var page=root.AddComponent<OutgameShopFeedback>();page.Bind(inv,key=>{Require(key=="ShopUI.toolvalue1","source label key");return "Cards:";},fly);
                    page.Refresh();var label=root.transform.Find("bottom/skinGroup_Shop/Viewport/Content/Shop_Diamond/shopInfo/toolValueNum/txt_toolValue").GetComponent<UnityEngine.UI.Text>();Require(label.text=="Cards:9","original tool value label");
                    state.ToolValue=10;page.OnToolChange(null);Require(label.text=="Cards:10","tool change ignores payload and refreshes current inventory");
                    page.GoldVideoCompleted(false);page.DiamondVideoCompleted(false);Require(fly.Trace.Count==0&&state.goldNum==0,"failed video no effect or grant");
                    page.GoldVideoCompleted(true);Require(state.goldNum==50&&fly.Request.ItemId==1001,"gold granted before animation completion");
                    Require(fly.Request.Root==root.transform.Find("effectRoot")&&fly.Request.Position==root.transform.Find("btn_addGold").position&&fly.Request.ApplyInventory&&fly.Request.Completion==null&&fly.Request.UpdateDisplayedValue,"original effect arguments");
                    Require(string.Join(",",toolProbe.Trace)=="changed:1001,get:1001,save","silent top refresh but notify/report/save enabled");
                    page.DiamondVideoCompleted(true);Require(state.diamondsNum==20&&fly.Request.ItemId==1002&&fly.Request.Position==root.transform.Find("btn_addDiamond").position,"diamond video path");
                    fly.Trace.Clear();fly.Target=null;page.GoldVideoCompleted(true);Require(fly.Trace.Count==0&&state.goldNum==50,"missing target suppresses grant and scheduling");
                    fly.Target=root.transform;fly.FailAnimation=true;bool threw=false;try{page.GoldVideoCompleted(true);}catch(InvalidOperationException){threw=true;}
                    Require(threw&&state.goldNum==100&&!fly.Trace.Exists(x=>x.StartsWith("track:")),"source grant precedes failing animation and is not rolled back");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-shop-native-four-product-buttons-and-rebuild",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try {
                    var table=OutgameProjectProductTable.Read(Resources.Load<TextAsset>("Data/Outgame/GameProductConfig").text);
                    var state=new OutgameLocalInventoryState{diamondsNum=1440};var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");
                    var probe=new ProjectShopProbe();var tools=new OutgameToolDispatcher(inv,probe,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                    var list=root.AddComponent<OutgameShopProductList>();int icons=0;
                    Action<OutgameProjectShopItemView,OutgameProjectProduct> bind=(view,data)=>view.Bind(data,tools,inv,(name,atlas)=>{Require(name==data.icon&&atlas=="ShopUI","original icon request");icons++;return null;},probe.PlayVoice,probe.ReportToolGet,()=>"coin-cost");
                    list.Build(table.Datas,bind);Require(list.Cards.Count==4&&icons==4,"only original four storeType11 rows");
                    Require(!root.transform.Find("bottom/ShopItem").gameObject.activeSelf,"template hidden");
                    int[] prices={40,200,400,800};int[] quantities={1,5,10,20};
                    for(int i=0;i<4;i++){
                        var card=list.Cards[i];Require(card.Controller.Data.id==10001+i,"original row order");
                        Require(card.transform.Find("img_gold/txt_gold").GetComponent<UnityEngine.UI.Text>().text==prices[i].ToString(),"native price");
                        Require(card.transform.Find("txt_info").GetComponent<UnityEngine.UI.Text>().text=="x"+quantities[i],"native quantity");
                        card.transform.Find("btn_choose").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    }
                    Require(state.diamondsNum==0&&state.ToolValue==36,"native clicks debit1440 and grant36");
                    probe.Trace.Clear();list.Cards[0].transform.Find("btn_choose").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(state.ToolValue==36&&!probe.Trace.Exists(x=>x.StartsWith("voice:")||x.StartsWith("extra:")),"insufficient diamonds do not grant/report reward");
                    table.Datas[12].isActive=0;list.Build(table.Datas,bind);
                    Require(list.Cards.Count==4&&list.Cards[0].transform.parent.childCount==4,"no invented activity filter; rebuild clears previous cards");
                    state.diamondsNum=40;list.Cards[0].transform.Find("btn_choose").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(state.diamondsNum==0&&state.ToolValue==37,"rebuilt button invokes one purchase");
                } finally {UnityEngine.Object.DestroyImmediate(root);}
            });
            check("outgame-project-shop-original-table-and-real-currency-purchase",()=>{
                var table=OutgameProjectProductTable.Read(Resources.Load<TextAsset>("Data/Outgame/GameProductConfig").text);Require(table.Datas.Length==16,"original project table has16 products");
                var row=Array.Find(table.Datas,x=>x.id==100101);var state=new OutgameLocalInventoryState{diamondsNum=20,goldNum=5};var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var h=new ProjectShopProbe();var tools=new OutgameToolDispatcher(inv,h,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");var card=new OutgameProjectShopItem(tools,inv,h,()=>"source-coin-cost");card.SetData(row);
                Require(card.ItemId==1001&&card.Amount==300&&card.Price==12&&h.Trace.Contains("icon:ShopUI:jibitubiao02")&&h.Trace.Contains("quantity:x300"),"source first reward and literal integer price displayed");h.Trace.Clear();card.Click();
                Require(state.diamondsNum==8&&state.goldNum==305&&string.Join(",",h.Trace)=="top,changed:1002,cost:1002,save,top,changed:1001,get:1001,save,voice:1:2017,extra:1001:300:305:source-coin-cost,refresh","real dispatcher debit then reward, save/report, voice and extra source report ordering");
                h.Trace.Clear();card.Click();Require(state.diamondsNum==8&&state.goldNum==305&&string.Join(",",h.Trace)=="top,changed:1002,save","insufficient balance retains dispatcher side effects but skips reward/audio/refresh");
            });
            check("outgame-project-shop-original-parse-failure-and-first-reward-only",()=>{
                var state=new OutgameLocalInventoryState{diamondsNum=10};var inv=new OutgameLocalInventory(state,"{\"Datas\":[]}");var h=new ProjectShopProbe();var tools=new OutgameToolDispatcher(inv,h,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");var card=new OutgameProjectShopItem(tools,inv,h,()=>"");
                var row=new OutgameProjectProduct{price="3",icon="a",itemConfig=new[]{new OutgameProjectShopReward{datas=new[]{1001,7}},new OutgameProjectShopReward{datas=new[]{1002,99}}}};card.SetData(row);card.Click();Require(state.diamondsNum==7&&state.goldNum==7,"ShopItem uses first reward only, not a package loop");
                row.price="bad";h.Trace.Clear();bool threw=false;try{card.SetData(row);}catch(FormatException){threw=true;}Require(threw&&card.Data==row&&card.Price==3&&string.Join(",",h.Trace)=="active:True,icon:ShopUI:a,price:bad","original partial display occurs before uncaught Parse failure; previous parsed price retained");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-login-sync-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
        sealed class LegacyDownloadProbe:IOutgameLegacyDownload
        {
            public bool Done,Network,Http;public Func<AssetBundle> Content=()=>null;
            public bool IsDone=>Done;public bool NetworkError=>Network;public bool HttpError=>Http;
            public string Error=>"fixture-error";public AssetBundle GetContent()=>Content();
        }

        sealed class LegacyBundleOperationProbe:IOutgameLegacyBundleOperation
        {
            readonly Func<OutgameLegacyBundleResult> get;
            public LegacyBundleOperationProbe(Func<OutgameLegacyBundleResult> get){this.get=get;}
            public OutgameLegacyBundleResult GetAssetBundle()=>get();
            public object Current=>null;public bool MoveNext()=>false;public void Reset(){}
        }

        sealed class LegacyLoaderProbe:OutgameLegacyResLoader
        {
            public int Starts,Loads;
            public override void Start(bool immediate){base.Start(immediate);Starts++;if(IsComplete)Complete();else{State=1;Module.Enqueue(this);}}
            public override void LoadBundle(){Loads++;}
            public void Finish(){State=3;Complete();}
        }

        sealed class AtlasResourceProbe:IOutgameAtlasResource
        {
            readonly UnityEngine.U2D.SpriteAtlas atlas;
            public AtlasResourceProbe(UnityEngine.U2D.SpriteAtlas atlas){this.atlas=atlas;}
            public UnityEngine.U2D.SpriteAtlas LoadAsset(string name)=>atlas;
        }

        sealed class UiRootResourcesProbe:IOutgameUiRootResources
        {
            readonly List<string> calls;
            public UiRootResourcesProbe(List<string> calls){this.calls=calls;}
            public void LoadPrefab(string path,Action<Func<string,bool,GameObject>> loaded){calls.Add("load:"+path);}
            public void Update(float deltaTime,float unscaledDeltaTime){calls.Add("update:"+deltaTime+":"+unscaledDeltaTime);}
        }

        sealed class StartupModuleProbe:IOutgameStartupModule
        {
            readonly string name;readonly List<string> order;
            public Action Initialized {get;set;}
            public StartupModuleProbe(string name,List<string> order){this.name=name;this.order=order;}
            public void Initialize()=>order.Add(name);
        }

    }
}
