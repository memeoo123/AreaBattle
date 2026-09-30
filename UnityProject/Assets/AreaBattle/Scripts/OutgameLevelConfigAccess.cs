using System;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // ConfigMgr GetLevelRealID/GetLevelConfig/IsGuideLv/SetLevelSpeed, with live controller access.
    public sealed class OutgameLevelConfigAccess
    {
        readonly Func<OutgameLegacyConfigManager> config;readonly Func<int> maxLevel,smallLevel;readonly Action<string> log;
        public OutgameLevelConfigAccess(Func<OutgameLegacyConfigManager> config,Func<int> maxLevel,Func<int> smallLevel,Action<string> log)
        {this.config=config;this.maxLevel=maxLevel;this.smallLevel=smallLevel;this.log=log;}
        public int GetLevelRealId(int level)
        {
            if(level>maxLevel())
            {
                int count=unchecked(maxLevel()-36);int remainder=unchecked(level-maxLevel())%count;
                if(remainder==0)remainder=count;int half=unchecked(count+1)/2;int parity;
                if(remainder<=half)parity=1;else {remainder=unchecked(remainder-half);parity=2;}
                level=unchecked(parity+(remainder<<1)+34);
            }
            return level;
        }
        public LevelConfig GetLevelConfig(int level)
        {
            int real=GetLevelRealId(level);var owner=config();
            bool found=owner.dicLevel.TryGetValue(real,out owner.CurrentLevelConfig);
            if(!found)log(string.Format("LevelConfig 不存在 Id={0} 的数据",real));
            return owner.CurrentLevelConfig;
        }
        public bool IsGuideLevel(int level)
        {
            if(!config().dicLevel.ContainsKey(level))return false;
            int index=smallLevel();
            if(config().dicLevel[level].LevelType.Length<=index)index=config().dicLevel[level].LevelType.Length-1;
            return config().dicLevel[level].LevelType[index]==1;
        }
        public void SetLevelSpeed(int level)
        {
            var owner=config();owner.CurrentLevelConfig=GetLevelConfig(level);
            owner.Globals.GameTimeScale=owner.Globals.BaseGameTimeScale*1f;
            owner.Globals.LineRendererMoveSpeed=owner.Globals.BaseLineRendererMoveSpeed*1f;
        }
    }
    public sealed partial class OutgameLegacyConfigManager {public LevelConfig CurrentLevelConfig;}
}
