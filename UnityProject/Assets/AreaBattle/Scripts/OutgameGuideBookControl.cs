using System;
namespace AreaBattle
{
    // Source4076. Dispose only disables ActiveUpdate; it retains the singleton/model.
    public sealed class OutgameGuideBookControl:IOutgameLogicControl
    {
        readonly Func<OutgameDataManagerPool> pool;readonly Func<OutgameLegacyConfigManager> config;
        readonly Func<int> currentLevel;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameGuideBookManager Manager {get;private set;}
        public bool ActiveUpdate {get;set;}
        public OutgameGuideBookControl(Func<OutgameDataManagerPool> pool,Func<OutgameLegacyConfigManager> config,
            Func<int> currentLevel,Func<OutgameMessageDispatcher> messages)
        {this.pool=pool;this.config=config;this.currentLevel=currentLevel;this.messages=messages;}
        public void OnInit()=>Manager=pool().GetModel<OutgameGuideBookManager>(4078,"GuideBookDataManager");
        public void Updata(float deltaTime,float unscaledDeltaTime){} //31334 empty.
        public void OnDispose()=>ActiveUpdate=false;
        public bool IsGetBookReward(int id)=>Manager.ContainsGuide(id);
        public bool IsGetTipReward(int id)=>Manager.ContainsTip(id);
        public bool IsTipUnlock(int id)
        {return config().dicGuideTips.TryGetValue(id,out var row)&&currentLevel()>=row.unlockLevel;}
        public int GetGuideUnlockLv(int id)
        {
            if(!config().dicGuide.TryGetValue(id,out var row))return 0;
            int level=row.guildLv;if(level>=0)return level;
            int previous=unchecked(id-1);return GetGuideUnlockLv(previous>0?previous:0);
        }
        public bool IsBookUnlock(int id)
        {
            if(!config().dicGuidebook.TryGetValue(id,out var row))return false;
            int level=currentLevel();return level>GetGuideUnlockLv(row.guideId);
        }
        public bool HaveAnyUnGetTipReward()
        {foreach(var pair in config().dicGuideTips)if(!IsGetTipReward(pair.Value.id)&&IsTipUnlock(pair.Value.id))return true;return false;}
        public bool HaveAnyUnGetGuideReward()
        {foreach(var pair in config().dicGuidebook)if(!IsGetBookReward(pair.Value.id)&&IsBookUnlock(pair.Value.id))return true;return false;}
        public void GetTipReward(int id){Manager.GetTipReward(id);messages().SendMessage("GetGuidBookReward");}
        public void GetBookRrward(int id){Manager.GetGuideReward(id);messages().SendMessage("GetGuidBookReward");}
    }
}
