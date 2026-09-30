using System;
namespace AreaBattle
{
    public interface IOutgameLevelStartLoading
    {
        bool SourceFlag48 {set;}
        float SourceValue40 {set;}
    }
    public interface IOutgameLevelStartHost
    {
        IOutgameLevelStartLoading OpenLoading();
        void InitializeEnemySkin();
        bool SourceFlag53 {set;}
        Action FindMenuClose();
        void AppendDelayedCallback(float seconds,Action callback);
        int CurrentLevel {get;}
        void LoadLevel(int level);
        void InitializeSpecialScene();
    }
    // Only the state3 branch of LevelControl.OnGamePlayState31525 and its delayed callback.
    // Caller owns shared state notifications and special-mode bypass before dispatching this branch.
    public sealed class OutgameLevelStartTransition
    {
        readonly IOutgameLevelStartHost host;
        public OutgameLevelStartTransition(IOutgameLevelStartHost host){this.host=host;}
        public void Enter()
        {
            var loading=host.OpenLoading();loading.SourceFlag48=true;loading.SourceValue40=5f;
            host.InitializeEnemySkin();host.SourceFlag53=false;
            host.FindMenuClose()?.Invoke();
            host.AppendDelayedCallback(.5f,()=>{host.LoadLevel(host.CurrentLevel);host.InitializeSpecialScene();});
        }
    }
}
