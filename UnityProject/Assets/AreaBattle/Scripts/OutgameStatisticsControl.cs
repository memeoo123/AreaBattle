using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public sealed class OutgameStatisticsControlServices
    {
        public Func<OutgameDataManagerPool> Pool;
        public Func<OutgameStatisticsManager> CreateManager;
        public Func<Action,int> AddUpdate;
        public Action<Action> RemoveUpdate;
        // GameFrameEntry3433.disposableActions, not an invented application-quit event.
        public Func<Action> GetDisposableActions;
        public Action<Action> SetDisposableActions;
        public Action ClearCommonMessages;
    }
    // Source4620 is a separate singleton, not one of the 38 LogicModule controllers.
    public sealed class OutgameStatisticsControl
    {
        static OutgameStatisticsControl shared;
        public static OutgameStatisticsControl Shared=>shared??(shared=new OutgameStatisticsControl());
        public OutgameStatisticsManager Manager;
        public Dictionary<int,Func<object[],long>> ValueProviders;
        public Action InitOver; // Source35047/35060 access field16: Action, not field20: bool.
        public bool SourceFlag20=true,IsDirty;
        OutgameStatisticsControlServices services;
        public void OnInit(OutgameStatisticsControlServices services,Action complete)
        {
            this.services=services;
            if(ValueProviders==null)ValueProviders=new Dictionary<int,Func<object[],long>>();
            InitOver=complete;
            Manager=services.Pool().GetModel<OutgameStatisticsManager>(4617,"GameStatisticsManager");
            if(Manager==null)
            {
                Manager=services.CreateManager();
                services.Pool().AddModel(4617,Manager,true);
            }
            Manager.InitStrategy();
            services.AddUpdate(Update);
            services.SetDisposableActions(services.GetDisposableActions()+Dispose);
        }
        public void Update()
        {
            Manager.Update();
            if(services.Pool().IsEnableSaveData&&IsDirty)Manager.OnSave();
        }
        public void Dispose()
        {
            ValueProviders=null;
            services.RemoveUpdate(Update);
            services.SetDisposableActions(services.GetDisposableActions()-Dispose);
            services.ClearCommonMessages();
            InitOver=null;
        }
        public void AddEventCount(int id,long value)=>Manager.AddEventStatistics(id,value);
        public void AddEventCount(int id,int itemId,long value)=>Manager.AddEventStatistics(id,itemId,value);
        public void SetEventCount(int id,long value)=>Manager.SetEventStatistics(id,value);
        public void SetEventCount(int id,int itemId,long value)=>Manager.SetEventStatistics(id,itemId,value);
        public void ResetEventCount(int id)=>Manager.ResetEventStatistics(id);
        public long GetEventCount(int id)=>Manager.GetEventStatistics(id);
        public long GetEventCount(int id,int itemId)=>Manager.GetEventStatistics(id,itemId);
    }
    // Source4618: resolve the owner again after callbacks; do not latch a session across reentry.
    public sealed class OutgameStatisticsExpansion
    {
        readonly Func<OutgameStatisticsControl> control;
        readonly Action<object[]> error;
        public OutgameStatisticsExpansion(Func<OutgameStatisticsControl> control,Action<object[]> error)
        {this.control=control;this.error=error;}
        public void RegisteredValueFunc(int id,Func<object[],long> value)
        {
            if(control()==null)return;
            var values=control().ValueProviders;var owner=control();
            if(values==null)owner.ValueProviders=new Dictionary<int,Func<object[],long>>();
            else if(owner.ValueProviders.ContainsKey(id))return;
            control().ValueProviders.Add(id,value);
        }
        public void AddEventCount(int id,long value){if(control()==null)return;control().AddEventCount(id,value);control().IsDirty=true;}
        public void AddEventCount(int id,int itemId,long value){if(control()==null)return;control().AddEventCount(id,itemId,value);control().IsDirty=true;}
        public void SetEventCount(int id,long value){if(control()==null)return;control().SetEventCount(id,value);if(id!=10000)control().IsDirty=true;}
        public void SetEventCount(int id,int itemId,long value){if(control()==null)return;control().SetEventCount(id,itemId,value);control().IsDirty=true;}
        public void ResetEventCount(int id){if(control()==null)return;control().ResetEventCount(id);control().IsDirty=true;}
        public long EventCount(int id)=>control()==null?0:control().GetEventCount(id);
        public long EventCount(int id,int itemId)=>control()==null?0:control().GetEventCount(id,itemId);
        public long GameValue(int id,params object[] args)
        {
            if(control()==null||control().ValueProviders==null)
            {error(new object[]{"GameStatisticsControl未初始化"});return 0;}
            if(control().ValueProviders.TryGetValue(id,out var provider))
            {
                try{return provider(args);}
                catch(Exception ex){error(new object[]{"GameValue注册函数执行异常: "+ex.Message});return 0;}
            }
            if(args==null||args.Length==0)return EventCount(id);
            if(args.Length==1)
            {
                if(args[0]!=null&&int.TryParse(args[0].ToString(),out int itemId))
                    return itemId==0?EventCount(id):EventCount(id,itemId);
                error(new object[]{string.Format("GameValue参数解析失败，无法将'{0}'转换为int类型",args[0])});return 0;
            }
            error(new object[]{string.Format("GameValue未注册自定义函数，不支持{0}个参数的调用",args.Length)});return 0;
        }
    }
}
