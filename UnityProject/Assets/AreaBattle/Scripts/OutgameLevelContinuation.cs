using System;
namespace AreaBattle
{
    public interface IOutgameLevelContinuationHost
    {
        void StopParallelAudio(int channel);void CloseUi(string name);
        int LocalSourceValue16 {set;}
        int CurrentLevel {get;set;}int SmallLevelIndex {get;set;}
        int SpecialState {set;}bool AdsSourceFlag28 {set;}
        void InitializeGameData();void SetPlayState(int state,bool option);
        void HideTransition(Action complete);void ShowTransition(Action complete);
    }
    // LevelControl31525 states10/11/12/14/15 and return-home callback31502.
    public sealed class OutgameLevelContinuation
    {
        readonly IOutgameLevelContinuationHost host;
        public OutgameLevelContinuation(IOutgameLevelContinuationHost host){this.host=host;}
        public bool TryDispatch(int state)
        {
            if(state==11)
            {
                host.AdsSourceFlag28=false;
                host.HideTransition(()=>{
                    host.CloseUi("Proj_xqzdPlayUI");host.CloseUi("GuideUI");host.SpecialState=0;
                    host.InitializeGameData();host.SetPlayState(2,true);host.ShowTransition(null);
                });return true;
            }
            if(state!=10&&state!=12&&state!=14&&state!=15)return false;
            host.StopParallelAudio(0);host.CloseUi("GuideUI");
            if(state==10||state==15)host.LocalSourceValue16=1;
            host.InitializeGameData();
            if(state==10)host.SmallLevelIndex=0;
            else if(state==12){host.CurrentLevel=unchecked(host.CurrentLevel+1);host.CloseUi("Proj_xqzdPauseUI");}
            else if(state==14)host.SmallLevelIndex=unchecked(host.SmallLevelIndex+1);
            host.SetPlayState(3,true);return true;
        }
    }
}
