using System;
namespace AreaBattle
{
    public sealed class OutgameActivityCloseState:OutgameActivityStateBase
    {
        public bool IsLeave=true;
        public override void OnEnter(OutgameFsm<OutgameActivityBase> fsm)
        {
            base.OnEnter(fsm);IsLeave=false;fsm.Owner.Data.state=1;Services.SetDirty(true);RefreshWidgets(fsm,null,null);
            if(!SelectInitialState()){fsm.Owner.Data.launchPop=false;fsm.Owner.Data.noticePop=false;}
            Services.Closed(fsm.Owner.ActivityId);
        }
        public override void OnLeave(OutgameFsm<OutgameActivityBase> fsm,bool shutdown){base.OnLeave(fsm,shutdown);IsLeave=true;}
        public override void RefreshWidgets(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)=>HideButtons(fsm);
        public override void ChangeStateEvent(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)
        {
            if(fsm.Owner.Data.state==2){ChangeState<OutgameActivityNoticeState>(fsm);fsm.Owner.Data.noticePop=false;}
            else if(fsm.Owner.Data.state==3){ChangeState<OutgameActivityLaunchState>(fsm);fsm.Owner.Data.launchPop=false;}
        }
        public void NoticeEvent(object[] args)
        {
            if(IsLeave||Met(Owner.Config.closeType,Owner.Data.CloseCondition)||!Met(Owner.Config.noticeType,Owner.Data.NoticeCondition))return;
            Services.Notice(Owner,Owner.Config.id);ChangeState<OutgameActivityNoticeState>(Fsm);
        }
        public void LaunchEvent(object[] args)
        {
            if(IsLeave||Met(Owner.Config.closeType,Owner.Data.CloseCondition)||!Met(Owner.Config.launchType,Owner.Data.LaunchCondition))return;
            Services.Launch(Owner,Owner.Config.id);ChangeState<OutgameActivityLaunchState>(Fsm);
        }
        public void OverEvent(object[] args)
        {
            if(IsLeave||Met(Owner.Config.closeType,Owner.Data.CloseCondition)||!Met(Owner.Config.overType,Owner.Data.OverCondition))return;
            Services.Over(Owner,Owner.Config.id);ChangeState<OutgameActivityOverState>(Fsm);
        }
        public override void AddStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.noticeType,NoticeEvent,false);Listen(fsm,c=>c.launchType,LaunchEvent,false);Listen(fsm,c=>c.overType,OverEvent,false);}
        public override void RemoveStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.noticeType,NoticeEvent,true);Listen(fsm,c=>c.launchType,LaunchEvent,true);Listen(fsm,c=>c.overType,OverEvent,true);}
    }
    public sealed class OutgameActivityLaunchState:OutgameActivityStateBase
    {
        public bool IsLeave=true;
        public override void OnEnter(OutgameFsm<OutgameActivityBase> fsm)
        {
            base.OnEnter(fsm);IsLeave=false;fsm.Owner.Data.state=3;Services.SetDirty(true);
            if(Met(Owner.Config.overType,Owner.Data.OverCondition)){Services.Over(Owner,Owner.Config.id);ChangeState<OutgameActivityOverState>(fsm);return;}
            if(Met(Owner.Config.closeType,Owner.Data.CloseCondition)){ChangeState<OutgameActivityCloseState>(fsm);return;}
            RefreshWidgets(fsm,null,null);
        }
        public override void OnLeave(OutgameFsm<OutgameActivityBase> fsm,bool shutdown){base.OnLeave(fsm,shutdown);IsLeave=true;}
        public override void RefreshWidgets(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)=>ShowLaunch(fsm);
        public override void ChangeStateEvent(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)
        {
            if(fsm.Owner.Data.state==1)
            {fsm.Owner.Data.launchPop=true;fsm.Owner.Data.noticePop=true;ChangeState<OutgameActivityCloseState>(fsm);}
            else if(fsm.Owner.Data.state==4)
            {fsm.Owner.Data.launchPop=true;fsm.Owner.Data.noticePop=true;ChangeState<OutgameActivityOverState>(fsm);}
        }
        public void OverEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.overType,Owner.Data.OverCondition))return;Services.Over(Owner,Owner.Config.id);ChangeState<OutgameActivityOverState>(Fsm);}
        public void CloseEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.closeType,Owner.Data.CloseCondition))return;ChangeState<OutgameActivityCloseState>(Fsm);}
        public override void AddStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.overType,OverEvent,false);Listen(fsm,c=>c.closeType,CloseEvent,false);}
        // Source35334 uses the opposite arrays when removing these two callbacks.
        public override void RemoveStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.closeType,OverEvent,true);Listen(fsm,c=>c.overType,CloseEvent,true);}
    }
    public sealed class OutgameActivityNoticeState:OutgameActivityStateBase
    {
        public bool IsLeave=true;public string[] DescriptionArgs;
        public override void OnEnter(OutgameFsm<OutgameActivityBase> fsm)
        {
            base.OnEnter(fsm);DescriptionArgs=null;IsLeave=false;fsm.Owner.Data.state=2;Services.SetDirty(true);
            if(Met(Owner.Config.launchType,Owner.Data.LaunchCondition))
            {Services.Launch(Owner,Owner.Config.id);ChangeState<OutgameActivityLaunchState>(fsm);}
            else if(Met(Owner.Config.overType,Owner.Data.OverCondition))
            {Services.Over(Owner,Owner.Config.id);ChangeState<OutgameActivityOverState>(fsm);}
            else if(Met(Owner.Config.closeType,Owner.Data.CloseCondition))ChangeState<OutgameActivityCloseState>(fsm);
            else
            {
                ShowNotice(fsm);Services.Common().AddListener(OutgameStatisticsMessageKey.Get(10000),DateUpdate);
            }
            if(fsm.Owner.Data.state==2&&fsm.Owner.Data.WarmTimeStamp==0)
            {var data=fsm.Owner.Data;data.WarmTimeStamp=Services.Statistics.GameValue(10000,Array.Empty<object>());}
        }
        public override void OnLeave(OutgameFsm<OutgameActivityBase> fsm,bool shutdown)
        {base.OnLeave(fsm,shutdown);Services.Common().RemoveListener(OutgameStatisticsMessageKey.Get(10000),DateUpdate);IsLeave=true;}
        public override void RefreshWidgets(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)=>ShowNotice(fsm);
        public override void ChangeStateEvent(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)
        {
            if(fsm.Owner.Data.state==1){fsm.Owner.Data.noticePop=true;ChangeState<OutgameActivityCloseState>(fsm);}
            else if(fsm.Owner.Data.state==3){ChangeState<OutgameActivityLaunchState>(fsm);fsm.Owner.Data.launchPop=false;ShowLaunch(fsm);}
        }
        public void LaunchEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.launchType,Owner.Data.LaunchCondition))return;Services.Launch(Owner,Owner.Config.id);ChangeState<OutgameActivityLaunchState>(Fsm);}
        public void OverEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.overType,Owner.Data.OverCondition))return;Services.Over(Owner,Owner.Config.id);ChangeState<OutgameActivityOverState>(Fsm);}
        public void CloseEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.closeType,Owner.Data.CloseCondition))return;ChangeState<OutgameActivityCloseState>(Fsm);}
        public void DateUpdate(object[] args)
        {
            if(DescriptionArgs==null)DescriptionArgs=new string[Owner.Config.noticeType.Length];
            FillDescription(Fsm,DescriptionArgs);
            for(int i=0;i<Owner.Descriptions.Count;i++)
            {
                if(Owner.Descriptions[i]==null||Owner.Config==null)continue;
                var description=Owner.Config.noticeDes;var text=Owner.Descriptions[i];
                text.text=description==null?string.Empty:Services.Format(Owner.Config.noticeDes.key,DescriptionArgs);
            }
        }
        public override void AddStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.launchType,LaunchEvent,false);Listen(fsm,c=>c.overType,OverEvent,false);Listen(fsm,c=>c.closeType,CloseEvent,false);}
        public override void RemoveStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)
        {Listen(fsm,c=>c.launchType,LaunchEvent,true);Listen(fsm,c=>c.overType,OverEvent,true);Listen(fsm,c=>c.closeType,CloseEvent,true);}
    }
    public sealed class OutgameActivityOverState:OutgameActivityStateBase
    {
        public bool IsLeave=true;
        public override void OnEnter(OutgameFsm<OutgameActivityBase> fsm)
        {
            base.OnEnter(fsm);IsLeave=false;fsm.Owner.Data.state=4;Services.SetDirty(true);fsm.Owner.Data.launchPop=true;
            if(Met(Owner.Config.closeType,Owner.Data.CloseCondition)){fsm.Owner.ResetProgress();ChangeState<OutgameActivityCloseState>(fsm);return;}
            RefreshWidgets(fsm,null,null);
        }
        public override void OnLeave(OutgameFsm<OutgameActivityBase> fsm,bool shutdown){base.OnLeave(fsm,shutdown);IsLeave=true;}
        public override void RefreshWidgets(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)=>HideButtons(fsm);
        public override void ChangeStateEvent(OutgameFsm<OutgameActivityBase> fsm,object sender,object args)
        {if(fsm.Owner.Data.state==1){fsm.Owner.ResetProgress();ChangeState<OutgameActivityCloseState>(fsm);}}
        public void CloseEvent(object[] args)
        {if(IsLeave||!Met(Owner.Config.closeType,Owner.Data.CloseCondition))return;Owner.ResetProgress();ChangeState<OutgameActivityCloseState>(Fsm);}
        public override void AddStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)=>Listen(fsm,c=>c.closeType,CloseEvent,false);
        public override void RemoveStatisticsListeners(OutgameFsm<OutgameActivityBase> fsm)=>Listen(fsm,c=>c.closeType,CloseEvent,true);
    }
}
