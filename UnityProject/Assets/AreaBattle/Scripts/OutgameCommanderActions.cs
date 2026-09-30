using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameCommanderEffects
    {
        void AddStatistic(int eventId,long delta);
        void SetCommanderStatistic(int eventId,int commanderId,int level);
        void SaveUserPreferences();
        void SaveManagers();
        void ReportUse(int commanderId,int level);
        void ReportUnlock(int commanderId,int level);
        void ReportLevelUp(int commanderId,int currentlyUsedCommanderLevel);
        void ShowUnlock(int commanderId);
    }
    public enum OutgameCommanderActionResult { None, LevelLocked, Insufficient, Maximum, Success }
    // CommanderUI selection + ClickUpgrade and CommanderControl wrapper around manager rules.
    // UI visuals/audio are separate. Required effects must be supplied, never silently discarded.
    public sealed class OutgameCommanderActions
    {
        [Serializable] sealed class Statistics { public int DailyCommUpgrade,CommanderUpgrade,CommanderUpgradeTotal; }
        readonly Statistics statistics;
        readonly OutgameProfile profile;
        readonly OutgameCommanderProgression rules;
        readonly Func<int,int> count;
        readonly Action<int,int> change;
        readonly IOutgameCommanderEffects effects;
        public int SelectedId { get; private set; }
        public OutgameCommanderActions(OutgameProfile profile,OutgameCommanderProgression rules,Func<int,int> count,Action<int,int> change,IOutgameCommanderEffects effects,string statisticsJson)
        {
            statistics=JsonUtility.FromJson<Statistics>(statisticsJson);
            this.profile=profile??throw new ArgumentNullException(nameof(profile));this.rules=rules??throw new ArgumentNullException(nameof(rules));
            this.count=count??throw new ArgumentNullException(nameof(count));this.change=change??throw new ArgumentNullException(nameof(change));this.effects=effects??throw new ArgumentNullException(nameof(effects));
        }
        OutgameCommanderState Get(int id)=>profile.commanders.Find(c=>c.id==id)??throw new ArgumentOutOfRangeException(nameof(id));
        public bool Select(int id)
        {
            if(id==SelectedId)return false;
            var state=Get(id);
            if(rules.IsUnlocked(state)){profile.usedCommanderId=id;effects.ReportUse(id,state.level);}
            SelectedId=id;return true;
        }
        public OutgameCommanderActionResult ClickUpgrade(int currentLevel,out int skillSlot)
        {
            skillSlot=-1;if(SelectedId==0)return OutgameCommanderActionResult.None;
            var state=Get(SelectedId);bool unlocked=rules.IsUnlocked(state);
            if(!unlocked&&!rules.IsLevelUnlockable(state,currentLevel))return OutgameCommanderActionResult.LevelLocked;
            if(unlocked&&rules.IsMax(state))return OutgameCommanderActionResult.Maximum;
            if(unlocked?!rules.CanAffordUpgrade(state,count):!rules.CanAffordUnlock(state,count))return OutgameCommanderActionResult.Insufficient;
            if(!rules.TryUpgrade(state,count,change,out skillSlot))return OutgameCommanderActionResult.Insufficient;
            // StatisticEventConfig fields16/72/84: DailyCommUpgrade, CommanderUpgrade, CommanderUpgradeTotal.
            effects.AddStatistic(statistics.DailyCommUpgrade,1);effects.SetCommanderStatistic(statistics.CommanderUpgrade,state.id,state.level);effects.AddStatistic(statistics.CommanderUpgradeTotal,1);
            effects.SaveUserPreferences();effects.SaveManagers();
            // Source wrapper reports the level of the currently USED commander, before unlock auto-equip.
            effects.ReportLevelUp(state.id,Get(profile.usedCommanderId).level);
            if(!unlocked&&state.level==1)
            {
                effects.ShowUnlock(state.id);profile.usedCommanderId=state.id;
                effects.ReportUnlock(state.id,state.level);effects.ReportUse(state.id,state.level);
            }
            return OutgameCommanderActionResult.Success;
        }
    }
}
