using System;
namespace AreaBattle
{
    public interface IOutgamePlayStateHost
    {
        void PauseGame(bool paused);
        void AudioState(int state);void WayLineState(int state);void AiState(int state);void SkillState(int state);
        void PublishGamePlayState(int requestedState);
        bool PvpActive {get;}bool DiceActive {get;}
        void SetCameraSize();
        void DispatchStateBranch(int currentState);
    }
    // LevelControl31479 / common prefix and special-mode bypass of31525.
    public sealed class OutgamePlayStateDispatcher
    {
        readonly IOutgamePlayStateHost host;
        readonly OutgameLevelRuntimeState state;
        public int State=>state.PlayState;
        public OutgamePlayStateDispatcher(IOutgamePlayStateHost host,OutgameLevelRuntimeState state=null){this.host=host;this.state=state??new OutgameLevelRuntimeState();}
        public bool SetPlaySate(int state,bool ignoredSourceOption=false)
        {OnGamePlayState(state);return true;}
        public void OnGamePlayState(int state)
        {
            this.state.PlayState=state;host.PauseGame(state==7);
            host.AudioState(State);host.WayLineState(State);host.AiState(State);host.SkillState(State);
            host.PublishGamePlayState(state);
            if(host.PvpActive||host.DiceActive){host.SetCameraSize();return;}
            host.DispatchStateBranch(State);
        }
    }
}
