using System;
namespace AreaBattle
{
    public interface IOutgameLevelStateHost
    {
        int CurrentLevel {get;}int LocalSourceValue16 {get;set;}int SmallLevelIndex {set;}
        bool InInsertPause {get;}bool HaveCheckedFailReward {get;}bool GuideActive {get;}bool SourceFlag53 {get;}
        float SourceSpeed104 {get;}int MinimumCommanderUnlockLevel {get;}
        Action FindBossRainSound();Action<bool> FindParallelAudioPause();
        void SetAdHelperLevel(int level);void InitializeChannelData();void ShowUpdateDialog();
        void PrepareHomeScene();void LoadHomeLevel(int level);void SetPlayState(int state,bool option);
        void CloseLoading();void ShowUi(string name);void CloseUi(string name);void GetPlayUi();
        void ReportLevelState(int state,int level);void EnterGuideNextStage();void ReportTutorialStart();void ReportTutorialFinish();
        void ShowMyCampEffect();void SetLevelSpeed(float speed);void PlayAudio(int channel,int[] ids);void StopParallelAudio(int channel);
        void ClearModelEntityCache();void LevelRankWin();void DiceLevelWin();
    }
    // Complete branch table of31525, after common notifications/special-mode bypass.
    // Host methods retain their own source semantics; this table does not synthesize external services.
    public sealed class OutgameLevelStateBranches
    {
        readonly IOutgameLevelStateHost host;readonly OutgameLevelStartTransition start;readonly OutgameLevelContinuation continuation;
        public OutgameLevelStateBranches(IOutgameLevelStateHost host,OutgameLevelStartTransition start,OutgameLevelContinuation continuation)
        {this.host=host;this.start=start;this.continuation=continuation;}
        public void Dispatch(int state)
        {
            if(continuation.TryDispatch(state))return;
            switch(state)
            {
                case 1:
                    host.SetAdHelperLevel(host.CurrentLevel);host.InitializeChannelData();host.SetPlayState(2,false);host.ShowUpdateDialog();break;
                case 2:
                    host.PrepareHomeScene();host.StopParallelAudio(0);host.LoadHomeLevel(host.CurrentLevel);break;
                case 3:start.Enter();break;
                case 4:
                    host.CloseLoading();
                    if((host.InInsertPause||(host.LocalSourceValue16==1&&host.CurrentLevel>=host.MinimumCommanderUnlockLevel))&&!host.HaveCheckedFailReward)
                        host.ShowUi("FailRewardUI");
                    else host.SetPlayState(5,false);
                    break;
                case 5:
                    host.LocalSourceValue16=0;host.ReportLevelState(0,host.CurrentLevel);
                    if(host.GuideActive){host.EnterGuideNextStage();if(host.CurrentLevel==0)host.ReportTutorialStart();}
                    else {host.ShowMyCampEffect();host.SetPlayState(6,true);}
                    host.PlayAudio(1,new[]{1002});break;
                case 6:
                    if(host.SourceFlag53)host.FindBossRainSound()?.Invoke();
                    host.SetLevelSpeed(host.SourceSpeed104);host.FindParallelAudioPause()?.Invoke(false);break;
                case 7:host.FindParallelAudioPause()?.Invoke(true);break;
                case 8:
                    host.ClearModelEntityCache();host.CloseUi("Proj_xqzdPlayUI");host.ShowUi("Proj_xqzdOverUI");
                    if(host.GuideActive&&host.CurrentLevel==0)host.ReportTutorialFinish();
                    host.StopParallelAudio(1);host.CloseUi("GuideUI");host.SmallLevelIndex=0;host.LevelRankWin();host.DiceLevelWin();break;
                case 9:
                    host.ReportLevelState(3,host.CurrentLevel);host.CloseUi("Proj_xqzdPlayUI");host.ShowUi("Proj_xqzdFailUI");host.StopParallelAudio(1);host.CloseUi("GuideUI");break;
                case 13:host.GetPlayUi();break;
            }
        }
    }
}
