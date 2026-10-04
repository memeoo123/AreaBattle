using System;
namespace AreaBattle
{
    public abstract class OutgameActivityStateBase:OutgameFsmState<OutgameActivityBase>
    {
        public OutgameFsm<OutgameActivityBase> Fsm;
        protected OutgameActivityBase Owner=>Fsm.Owner;
        protected OutgameActivityServices Services=>Owner.Services;
        public override void OnInit(OutgameFsm<OutgameActivityBase> fsm){Fsm=fsm;}
        public override void OnEnter(OutgameFsm<OutgameActivityBase> fsm)
        {
            if(!string.Equals(Owner.Config.uniqueId,Owner.Data.uniqueId))
            {
                Owner.Data.uniqueId=fsm.Owner.Config.uniqueId;Services.SetDirty(true);
                Owner.Data.LaunchTimeStamp=0;Owner.ResetProgress();
            }
            SubscribeEvent(1,ChangeStateEvent);SubscribeEvent(2,RefreshWidgets);AddStatisticsListeners(fsm);
        }
        public override void OnLeave(OutgameFsm<OutgameActivityBase> fsm,bool shutdown)
        {UnsubscribeEvent(1,ChangeStateEvent);UnsubscribeEvent(2,RefreshWidgets);RemoveStatisticsListeners(fsm);}
        public virtual void RefreshWidgets(OutgameFsm<OutgameActivityBase> fsm,object sender,object args){}
        public virtual void ChangeStateEvent(OutgameFsm<OutgameActivityBase> fsm,object sender,object args){}
        public abstract void AddStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm);
        public abstract void RemoveStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm);
        protected bool Met(int[] types,OutgameActivityCondition[] conditions)=>OutgameActivityConditions.Met(Owner.Config,types,conditions,Services.Statistics);
        protected void Listen(OutgameFsm<OutgameActivityBase> fsm,Func<OutgameActivityConfigRow,int[]> types,Action<object[]> handler,bool remove)
        {
            for(int i=0;i<types(fsm.Owner.Config).Length;i++)
            {
                string key=OutgameStatisticsMessageKey.Get(types(fsm.Owner.Config)[i]);
                if(remove)Services.Common().RemoveListener(key,handler);else Services.Common().AddListener(key,handler);
            }
        }
        protected bool SelectInitialState()
        {
            if(Met(Owner.Config.closeType,Owner.Data.CloseCondition))return false;
            if(Met(Owner.Config.noticeType,Owner.Data.NoticeCondition))
            {
                if(Fsm.CurrentState.GetType()==typeof(OutgameActivityNoticeState))return false;
                Services.Notice(Owner,Owner.Config.id);ChangeState<OutgameActivityNoticeState>(Fsm);return true;
            }
            if(Met(Owner.Config.launchType,Owner.Data.LaunchCondition))
            {
                if(Fsm.CurrentState.GetType()==typeof(OutgameActivityLaunchState))return false;
                ChangeState<OutgameActivityLaunchState>(Fsm);Services.Launch(Owner,Owner.Config.id);return true;
            }
            if(Met(Owner.Config.overType,Owner.Data.OverCondition))
            {
                if(Fsm.CurrentState.GetType()==typeof(OutgameActivityOverState))return false;
                ChangeState<OutgameActivityOverState>(Fsm);Services.Over(Owner,Owner.Config.id);return true;
            }
            return false;
        }
        protected void HideButtons(OutgameFsm<OutgameActivityBase> fsm)
        {for(int i=0;i<fsm.Owner.Buttons.Count;i++)fsm.Owner.Buttons[i].gameObject.SetActive(false);}
        protected void ShowLaunch(OutgameFsm<OutgameActivityBase> fsm)
        {
            fsm.Owner.ButtonClickAction=fsm.Owner.LaunchClick;
            for(int i=0;i<fsm.Owner.Buttons.Count;i++)
            {
                fsm.Owner.Buttons[i].gameObject.SetActive(true);fsm.Owner.Buttons[i].onClick.RemoveAllListeners();
                fsm.Owner.Buttons[i].onClick.AddListener(fsm.Owner.ButtonClickAction);
            }
            for(int i=0;i<fsm.Owner.Descriptions.Count;i++)
                if(fsm.Owner.Descriptions[i]!=null&&fsm.Owner.Descriptions[i].gameObject!=null)fsm.Owner.Descriptions[i].gameObject.SetActive(false);
            if(!fsm.Owner.Data.launchPop)fsm.Owner.UnlockPop();
        }
        protected void ShowNotice(OutgameFsm<OutgameActivityBase> fsm)
        {
            fsm.Owner.ButtonClickAction=fsm.Owner.NoticeClick;
            for(int i=0;i<fsm.Owner.Buttons.Count;i++)
            {
                fsm.Owner.Buttons[i].gameObject.SetActive(true);fsm.Owner.Buttons[i].onClick.RemoveAllListeners();
                fsm.Owner.Buttons[i].onClick.AddListener(fsm.Owner.ButtonClickAction);
            }
            var args=new string[fsm.Owner.Config.noticeType.Length];FillDescription(fsm,args);
            for(int i=0;i<fsm.Owner.Descriptions.Count;i++)
            {
                if(fsm.Owner.Descriptions[i]==null||fsm.Owner.Descriptions[i].gameObject==null)continue;
                fsm.Owner.Descriptions[i].gameObject.SetActive(true);
                var description=fsm.Owner.Config.noticeDes;var text=fsm.Owner.Descriptions[i];
                text.text=description==null?string.Empty:Services.Format(fsm.Owner.Config.noticeDes.key,args);
            }
            if(!fsm.Owner.Data.noticePop)fsm.Owner.NoticePop();
        }
        protected void FillDescription(OutgameFsm<OutgameActivityBase> fsm,string[] args)
        {
            for(int i=0;i<fsm.Owner.Config.noticeType.Length;i++)
            {
                if(fsm.Owner.Config.noticeType[i]==10000)
                {
                    long target=Owner.Data.NoticeCondition[i].value;
                    long remaining=unchecked(target-Services.Statistics.GameValue(10000,Array.Empty<object>()));
                    args[i]=FormatDuration(unchecked((int)(remaining/1000L)));
                }
                else args[i]=fsm.Owner.Config.noticeParams[i];
            }
        }
        // TimeHelper3531.ToDDHHMMSS returns only the first three segments: d/h/m or h/m/s.
        public static string FormatDuration(int seconds)
        {
            int days=seconds/86400,hours=(seconds-days*86400)/3600,minutes=(seconds-days*86400-hours*3600)/60,remainder=seconds%60;
            string h=hours.ToString().PadLeft(2,'0')+"h",m=minutes.ToString().PadLeft(2,'0')+"m";
            return seconds>=86400?days.ToString()+"d"+h+m:h+m+remainder.ToString().PadLeft(2,'0')+"s";
        }
    }
}
