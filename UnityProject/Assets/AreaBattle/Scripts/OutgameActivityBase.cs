using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameActivityPopEvent
    {public OutgameActivityConfigRow Config;public Type UiType;public object[] Args;}
    public interface IOutgameActivityUiHost
    {OutgameUiPage Open(string nameSpace,string name,object[] args);}
    public sealed class OutgameActivityServices
    {
        public Func<int,OutgameActivityItemData> GetItemData;
        public Func<int,OutgameActivityConfigRow> GetConfig;
        public Func<OutgameFsmManager> FsmManager;
        public Func<OutgameCommonMessageDispatcher> Common;
        public OutgameStatisticsExpansion Statistics;
        public Action<bool> SetDirty;
        public Action<OutgameActivityBase,int> Launch,Notice,Over;
        public Action<int> Closed;
        public Action<OutgameActivityPopEvent> QueueNotice,QueueLaunch;
        public Func<IOutgameActivityUiHost> Ui;
        public Func<string,object[],string> Format;
        public Func<OutgameItemConfigManager> ItemConfig;
        public Action<string> ItemFactoryError;
    }
    // ActivityBase4629 lifecycle, item factory and generic child lookup.
    public class OutgameActivityBase
    {
        public readonly OutgameActivityServices Services;
        public int ActivityId;
        public OutgameFsmManager FsmManager;
        public OutgameFsm<OutgameActivityBase> Fsm;
        public OutgameActivityItemData Data;
        public List<OutgameActivityBase> Children=new List<OutgameActivityBase>();
        public Dictionary<int,OutgameActivityBase> ChildrenById=new Dictionary<int,OutgameActivityBase>();
        public bool FsmInit=true,IsInit=true;
        public List<Button> Buttons=new List<Button>();public List<Text> Descriptions=new List<Text>();
        public Type NoticePopUi,LaunchPopUi;
        public OutgameActivityPopEvent NoticePopEvent,LaunchPopEvent;
        public OutgameActivityConfigRow Config;
        public UnityAction ButtonClickAction;
        public OutgameActivityBase(OutgameActivityServices services){Services=services;}
        public virtual OutgameActivityFactory ActivityFactoryBase(int itemId)=>new OutgameActivityFactory(itemId,Services.ItemConfig,Services.ItemFactoryError);
        public T GetChildActivity<T>(int id)where T:OutgameActivityBase
        {return ChildrenById.ContainsKey(id)?(T)ChildrenById[id]:default;}
        public virtual void Refresh(int activityId)
        {
            ActivityId=activityId;Data=Services.GetItemData(activityId);
            if(Data==null)return;
            Config=Services.GetConfig(Data.id);
            if(IsInit){IsInit=false;OnInit();}
            RefreshFsm();
            for(int i=0;i<Children.Count;i++){var child=Children[i];child.Refresh(Children[i].ActivityId);}
        }
        public void BaseOnInit()=>OnInit();
        public virtual void OnInit(){}
        public void RefreshFsm()
        {
            if(Fsm==null)
            {
                FsmManager=Services.FsmManager();
                var states=new OutgameActivityStateBase[]{new OutgameActivityCloseState(),new OutgameActivityNoticeState(),new OutgameActivityLaunchState(),new OutgameActivityOverState()};
                Fsm=FsmManager.CreateFsm<OutgameActivityBase>(Data.id.ToString(),this,states);
                if(Config.open==1)
                {
                    switch(Data.state)
                    {
                        case 1:Fsm.Start<OutgameActivityCloseState>();break;
                        case 2:Fsm.Start<OutgameActivityNoticeState>();break;
                        case 3:Fsm.Start<OutgameActivityLaunchState>();break;
                        case 4:Fsm.Start<OutgameActivityOverState>();break;
                    }
                }
                else Fsm.Start<OutgameActivityCloseState>();
                return;
            }
            if(Config.open==1)FireEvent(1);
        }
        public void FireEvent(int id){if(Fsm!=null)Fsm.FireEvent(this,id);}
        public void BaseUpdate(){for(int i=0;i<Children.Count;i++)Children[i].Update();Update();}
        public virtual void Update(){}
        public virtual void ResetProgress(){}
        public virtual void LaunchStateEnter()=>Services.Common().SendMessage("CommonModule_Activity_LaunchOnceOnEnterState",new object[]{ActivityId});
        public virtual void WarmStateEnter()=>Services.Common().SendMessage("CommonModule_Activity_WarmOnceOnEnterState",new object[]{ActivityId});
        public virtual void Launch(){}
        public virtual void Warm(){}
        public virtual void Over(){}
        public void NoticePop()
        {
            if(NoticePopUi!=null)
            {
                NoticePopEvent=new OutgameActivityPopEvent{Config=Config,UiType=NoticePopUi,Args=Array.Empty<object>()};
                Services.QueueNotice(NoticePopEvent);
            }
            WarmStateEnter();
        }
        public void UnlockPop()
        {
            if(LaunchPopUi!=null)
            {
                LaunchPopEvent=new OutgameActivityPopEvent{Config=Config,UiType=LaunchPopUi,Args=Array.Empty<object>()};
                Services.QueueLaunch(LaunchPopEvent);
            }
            LaunchStateEnter();
        }
        public virtual OutgameUiPage OpenBindNoticeUi(OutgameActivityPopEvent ignored)
        {var ui=Services.Ui();return ui.Open(NoticePopEvent.UiType.Namespace,NoticePopEvent.UiType.Name,NoticePopEvent.Args);}
        public void NoticeClick(){if(NoticePopUi!=null){var ui=Services.Ui();ui.Open(NoticePopUi.Namespace,NoticePopUi.Name,Array.Empty<object>());}}
        public void LaunchClick(){if(LaunchPopUi!=null){var ui=Services.Ui();ui.Open(LaunchPopUi.Namespace,LaunchPopUi.Name,Array.Empty<object>());}}
        public void DestroyFsm(){if(FsmManager!=null)FsmManager.DestroyFsm(Fsm);}
        public void OnDispose()
        {DestroyFsm();for(int i=0;i<Children.Count;i++)Children[i].OnDispose();Children.Clear();ChildrenById.Clear();Dispose();}
        public virtual void Dispose(){}
    }
}
