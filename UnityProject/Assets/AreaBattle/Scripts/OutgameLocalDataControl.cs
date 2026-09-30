using System;
namespace AreaBattle
{
    // LocalDataControl31584..31594. Source Event.static4 is GamePlayState.
    public sealed class OutgameLocalDataControl:IOutgameLogicControl
    {
        readonly Func<OutgameDataManagerPool> pool;
        readonly OutgameControllerRegistry registry;
        readonly Func<OutgameMessageDispatcher> messages;
        readonly Func<int> todayOfYear;
        readonly Func<long> nowTimestamp;
        readonly OutgameLegacyCommanderRepair repair;
        public OutgameLocalDataManager Manager {get;private set;}
        public OutgameLocalDataControl(Func<OutgameDataManagerPool> pool,OutgameControllerRegistry registry,Func<OutgameMessageDispatcher> messages,Func<int> todayOfYear,Func<long> nowTimestamp,OutgameLegacyCommanderRepair repair)
        {this.pool=pool;this.registry=registry;this.messages=messages;this.todayOfYear=todayOfYear;this.nowTimestamp=nowTimestamp;this.repair=repair;}
        public void OnInit()
        {
            Manager=pool().GetModel<OutgameLocalDataManager>(4119,"LocalDataManager");
            messages().AddListener("IapSuccess",OnIapSuccess);
            messages().AddListener("GamePlayState",OnGamePlayState);
        }
        void OnIapSuccess(object[] args)=>Manager.SuccessFirstCharge(nowTimestamp);
        void OnGamePlayState(object[] args)
        {
            switch((int)args[0])
            {
                case 3:
                    int today=todayOfYear();
                    if(today!=Manager.Record.jumpData.jumpDayOfYear)
                    {
                        Manager.Record.jumpData.jumpNum=0;
                        Manager.Record.jumpData.jumpDayOfYear=todayOfYear();
                    }
                    return;
                case 9:Manager.Record.jumpData.defeatNum=unchecked(Manager.Record.jumpData.defeatNum+1);return;
                case 8:Manager.Record.jumpData.defeatNum=0;return;
            }
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Original31585.
        public void OnDispose()
        {
            messages().RemoveListener("GamePlayState",OnGamePlayState);
            messages().RemoveListener("IapSuccess",OnIapSuccess);
            registry.Clear(4118);
        }
        public int GetCoinNum()=>Manager==null?0:Manager.GoldNum;
        public int GetJewelNum()=>Manager==null?0:Manager.DiamondsNum;
        public bool GetJewelPackFirstPurchaseStatus(string key)=>Manager!=null&&Manager.HasPurchasedJewelKey(key);
        public void SetJewelPackFirstPurchaseStatus(string key){if(Manager!=null)Manager.SetPurchasedJewelKey(key);}
        public void Handle103VersionBug()=>repair.Handle103VersionBug();
    }
}
