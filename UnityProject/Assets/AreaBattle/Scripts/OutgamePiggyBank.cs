using System;
namespace AreaBattle
{
    // PiggyBankControl30169/30172/30173/30178 and LocalDataManager CollectNum setter3899.
    public sealed class OutgamePiggyBank
    {
        public const string Activity="piggycank_猪猪存钱罐";
        readonly OutgameProfile profile;readonly OutgameLevelProgression levels;
        readonly Func<OutgameMessageDispatcher> messages;
        public bool ActiveUpdate {get;set;}
        readonly Action<string,string,string,string> reportSuccess,reportReset;
        public OutgamePiggyBank(OutgameProfile profile,OutgameLevelProgression levels,
            Action<string,string,string,string> reportSuccess,Action<string,string,string,string> reportReset,Func<OutgameMessageDispatcher> messages=null)
        {this.messages=messages??(()=>OutgameMessageDispatcher.Shared);this.profile=profile??throw new ArgumentNullException(nameof(profile));this.levels=levels??throw new ArgumentNullException(nameof(levels));this.reportSuccess=reportSuccess??throw new ArgumentNullException(nameof(reportSuccess));this.reportReset=reportReset??throw new ArgumentNullException(nameof(reportReset));}
        // Original OnInit/OnDispose use the shared string-event channel; repeated init is not deduplicated.
        public void OnInit(){ActiveUpdate=true;messages().AddListener("GamePlayState",OnStateMessage);}
        public void OnDispose(){messages().RemoveListener("GamePlayState",OnStateMessage);}
        void OnStateMessage(object[] args)
        {
            // Source checks the level before reading/unboxing the event's first argument.
            if(levels.CurrentLevel<30)return;
            OnGamePlayState((int)args[0]);
        }
        public int CollectNum
        {
            get=>profile.inventory.collectNum;
            set
            {
                reportSuccess(Activity,levels.CurrentLevel.ToString(),profile.inventory.collectNum.ToString(),value.ToString());
                profile.inventory.collectNum=Math.Min(value,1000);
            }
        }
        public bool IsFull=>CollectNum>999;
        public int GetBankState()=>levels.CurrentLevel>=30?(IsFull?2:1):0;
        public void OnGamePlayState(int state)
        {
            if(levels.CurrentLevel<30||state!=8)return;
            CollectNum=unchecked(CollectNum+20);
            if(CollectNum>=1001)CollectNum=1000;
        }
        public void OnGM_SetPiggyNum(string value){CollectNum=int.Parse(value);}
        public void PaySuccess(){}
        public void Reset()
        {
            CollectNum=0;profile.inventory.collectAdNum=0;
            reportReset(Activity,levels.CurrentLevel.ToString(),null,null);
        }
    }
}
