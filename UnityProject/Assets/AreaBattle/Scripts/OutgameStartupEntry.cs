using System;
namespace AreaBattle
{
    public interface IOutgameStartupEntryHost
    {
        void ShowMenu();
        void SetMenuVisible(bool visible);
        void ShowCommonReward();
        void InitializePrefabs();
        // Original ResLoadHelper.LoadScene uses false and a null progress handler.
        void LoadScene(string name,Action complete,bool sourceOption);
        bool EnterGame {get;set;}
        void LateInitializeModule();
        bool RedDotSourceFlag8 {get;set;}
        int RedDotSourceValue16 {get;set;}
        void SendLoadGameScreen();
        int CurrentLevel {get;}
        void ReportActivityEnter(string activity,string level);
        void InitializeLevelRank();
        void InitializeSevenDayActivity();
        void SetPlayState(int state);
        void CloseLoading();
        void ReportGameInteractive(string message);
    }
    // ProcedureStarGame f11032 -> GotoGameScene f11033 -> completion f18161.
    // This is the post-data startup phase; it does not invent successful account/config loading.
    public sealed class OutgameStartupEntry
    {
        readonly IOutgameStartupEntryHost host;
        readonly Action migrate,initializeSevenDay;
        public OutgameStartupEntry(IOutgameStartupEntryHost host,Action migrate)
        {this.host=host??throw new ArgumentNullException(nameof(host));this.migrate=migrate??throw new ArgumentNullException(nameof(migrate));initializeSevenDay=host.InitializeSevenDayActivity;}
        public OutgameStartupEntry(IOutgameStartupEntryHost host,Action migrate,OutgameControllerRegistry controllers):this(host,migrate)
        {initializeSevenDay=()=>((OutgameSevendayActivityControl)controllers.Resolve(4502)).EnterGameInit();}
        public OutgameStartupEntry(IOutgameStartupEntryHost host,OutgameDataSaveRegistry registry,OutgameLocalDataManager local,OutgameSkinManager skins,OutgameCommanderManager commanders)
            :this(host,()=>OutgameLegacyMigration.Apply(registry,local,skins,commanders)){}
        public void Enter()
        {
            host.ShowMenu();host.SetMenuVisible(false);host.ShowCommonReward();
            migrate();host.InitializePrefabs();GotoGameScene();
        }
        public void GotoGameScene()
        {
            host.LoadScene("GamePlay",SceneComplete,false);
            host.EnterGame=true;host.LateInitializeModule();host.RedDotSourceFlag8=true;host.RedDotSourceValue16=1;
        }
        void SceneComplete()
        {
            host.SendLoadGameScreen();host.ReportActivityEnter("EnterGameHome",host.CurrentLevel.ToString());
            host.InitializeLevelRank();initializeSevenDay();host.SetPlayState(1);
            host.CloseLoading();host.ReportGameInteractive("");
        }
    }
}
