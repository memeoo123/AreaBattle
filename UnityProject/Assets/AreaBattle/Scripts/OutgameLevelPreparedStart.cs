using System;
using System.Collections;
namespace AreaBattle
{
    public interface IOutgamePreparedPlayPage {void Refresh();}
    public interface IOutgamePreparedPlayUi {IOutgamePreparedPlayPage GetPlayPage();void ShowPlayPage();}
    public interface IOutgameLevelWayLines {void Clear();void InitPrePrefabLoad();void InitStarLine();}
    // Source31535 post-resource callback and4106.MoveNext. No fake completion for missing services.
    public sealed class OutgameLevelPreparedStart
    {
        readonly OutgameLevelControl level;readonly Func<int,IOutgameLevelTower> normal;readonly Func<IOutgameLevelTower> boss;
        readonly Func<OutgameGameControl> game;readonly Func<IOutgamePreparedPlayUi> ui;readonly Action<int,bool> setState;
        readonly Func<int> currentLevel;readonly Action<IEnumerator> start;readonly Func<IOutgameLevelWayLines> lines;readonly Action initializeAi;
        public OutgameLevelPreparedStart(OutgameLevelControl level,Func<int,IOutgameLevelTower> normal,Func<IOutgameLevelTower> boss,Func<OutgameGameControl> game,Func<IOutgamePreparedPlayUi> ui,Action<int,bool> setState,Func<int> currentLevel,Action<IEnumerator> start,Func<IOutgameLevelWayLines> lines,Action initializeAi)
        {this.level=level;this.normal=normal;this.boss=boss;this.game=game;this.ui=ui;this.setState=setState;this.currentLevel=currentLevel;this.start=start;this.lines=lines;this.initializeAi=initializeAi;}
        public void Complete(bool noSetBefore)
        {
            level.InitializeTowers(normal,boss);level.InitializeObstacles();game().PrepareGameScene();
            var existing=ui().GetPlayPage();var next=ui();if(existing==null)next.ShowPlayPage();else next.GetPlayPage().Refresh();
            level.RefreshCampInfo();if(!noSetBefore)setState(4,true);
            var cameraOwner=game();currentLevel();cameraOwner.SetCameraSize();start(InitializeLines());
        }
        public IEnumerator InitializeLines()
        {
            lines().Clear();yield return new UnityEngine.WaitForSeconds(.2f);
            lines().InitPrePrefabLoad();lines().InitStarLine();yield return new UnityEngine.WaitForSeconds(.2f);
            initializeAi();
        }
    }
}
