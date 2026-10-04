using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Original abstract GameStatisticsStratrgyBase4624. OffNet4623 remains a required concrete implementation.
    public abstract class OutgameStatisticsStrategy
    {
        protected readonly Func<OutgameDataManagerPool> Pool;
        protected readonly Func<OutgameCommonMessageDispatcher> Common;
        protected readonly Func<OutgameStatisticsControl> Control;
        protected readonly OutgameStatisticsExpansion Expansion;
        protected Action<OutgameStatisticsJsonData> UpdateDataAction;
        protected OutgameStatisticsManager Manager;
        protected OutgameStatisticsStrategy(Func<OutgameDataManagerPool> pool,Func<OutgameCommonMessageDispatcher> common,
            Func<OutgameStatisticsControl> control,OutgameStatisticsExpansion expansion)
        {Pool=pool;Common=common;Control=control;Expansion=expansion;}
        public virtual void InitData(Action<OutgameStatisticsJsonData> update)
        {
            UpdateDataAction=update;
            Manager=Pool().GetModel<OutgameStatisticsManager>(4617,"GameStatisticsManager");
            Common().AddListener("CommonModule_Activity_Lunch",ActivityLunch);
        }
        public virtual void LoadData(string text){}
        protected void UpdateManagerData(bool allowServer)=>Manager.UpdateData(allowServer);
        public void Update()=>UpdateDate(Array.Empty<object>());
        public abstract void UpdateDate(object[] args);
        public abstract long Value10000(object[] args);
        public abstract long Value10015(object[] args);
        public abstract long Value10011(object[] args);
        public abstract long Value10901(object[] args);
        public abstract long Value10900(object[] args);
        public abstract long Value10800(object[] args);
        public abstract long Value10003(object[] args);
        public abstract long Value10002(object[] args);
        public abstract long Value10001(object[] args);
        public abstract long Value10902(object[] args);
        public abstract long Value10008(object[] args);
        public abstract long Value10007(object[] args);
        public abstract long Value20000(object[] args);
        public virtual long Value10500(object[] args)=>OutgameItemTimestamp.ToDateTime(Expansion.GameValue(10000,Array.Empty<object>())).Hour;
        public virtual long Value10501(object[] args)=>(int)OutgameItemTimestamp.ToDateTime(Expansion.GameValue(10000,Array.Empty<object>())).DayOfWeek;
        public virtual long Value10502(object[] args)=>OutgameItemTimestamp.ToDateTime(Expansion.GameValue(10000,Array.Empty<object>())).Day;
        public virtual long Value10600(object[] args)
        {if(args==null||args.Length==0)return 0;int id=(int)args[0];return Control().GetEventCount(10600,id);}
        public virtual long Value10700(object[] args)
        {if(args!=null&&args.Length==0)return 0;int id=(int)args[0];return Control().GetEventCount(10700,id);}
        public virtual long Value10013(object[] args)=>Control().GetEventCount(10013);
        void ActivityLunch(object[] args)
        {if(args!=null&&args.Length>0)Expansion.AddEventCount(20000,(int)args[0],1L);}
        public void Dispose(){Common().RemoveListener("CommonModule_Activity_Lunch",ActivityLunch);OnDispose();}
        protected abstract void OnDispose();
        public virtual void OnSave(OutgameStatisticsJsonData data,Dictionary<int,OutgameGameStatisticsData> records){}
    }
}
