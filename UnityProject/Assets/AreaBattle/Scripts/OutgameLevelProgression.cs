using System;
namespace AreaBattle
{
    public interface IOutgameLevelEffects
    {
        void SetStatistic(int eventId,long count);
        void SaveLocalData();
    }
    // Source LevelControl fields: eSpecialState at24, SpecialLevel at16.
    // A held LevelID is distinct from the first-account/guide initialization policy.
    public sealed class OutgameLevelProgression
    {
        readonly OutgameProfile profile;readonly IOutgameLevelEffects effects;
        public int SpecialState {get;set;}
        public int SpecialLevel {get;set;}
        public OutgameLevelProgression(OutgameProfile held,IOutgameLevelEffects effects)
        {profile=held??throw new ArgumentNullException(nameof(held));this.effects=effects??throw new ArgumentNullException(nameof(effects));}
        public int RealCurrentLevel=>profile.levelID;
        public int CurrentLevel
        {
            get=>SpecialState==0?profile.levelID:SpecialLevel;
            set
            {
                if(SpecialState!=0)return;
                profile.levelID=value;
                effects.SetStatistic(10015,unchecked(value-1));
                effects.SaveLocalData();
            }
        }
    }
}
