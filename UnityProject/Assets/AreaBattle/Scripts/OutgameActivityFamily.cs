using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Original ChildActivityBase4639; EarlyOnInit only assigns the supplied module data.
    public class OutgameChildActivity<TData>:OutgameActivityBase
    {
        public TData ModuleData;
        public OutgameChildActivity(OutgameActivityServices services):base(services){}
        public void EarlyOnInit(TData data){ModuleData=data;}
        public override void Update(){}
        public override void OnInit(){}
        public override void Dispose(){}
    }
    public sealed class OutgameActivityFamilyServices<TChild>
    {
        public Func<OutgameActivityConfigManager> Config;
        public Func<OutgameActivityControl> Control;
        // Original Activator.CreateInstance<TChild>(); app factory supplies constructor services.
        public Func<TChild> CreateChild;
    }
    // FatherActivityBase4640, including shared generic ResetChildActivity35258.
    public class OutgameFatherActivity<TChild,TData>:OutgameActivityBase where TChild:OutgameChildActivity<TData>
    {
        public Dictionary<int,TData> SelfViewDatas=new Dictionary<int,TData>(),OtherViewDatas=new Dictionary<int,TData>();
        public Dictionary<int,TChild> SelfViewActivities=new Dictionary<int,TChild>(),OtherViewActivities=new Dictionary<int,TChild>();
        public readonly OutgameActivityFamilyServices<TChild> Family;
        public OutgameFatherActivity(OutgameActivityServices services,OutgameActivityFamilyServices<TChild> family):base(services){Family=family;}
        public override void Update(){}
        public override void OnInit(){}
        public override void Dispose(){}
        public virtual void SortDataDic(){}
        public void ResetChildActivity<TConfig>(Dictionary<object,TConfig> childActivityDic,Dictionary<int,TData> childDataDic)
        {
            SelfViewActivities=new Dictionary<int,TChild>();OtherViewActivities=new Dictionary<int,TChild>();
            foreach(var config in Family.Config().Activities.Values)
            {
                if(!childActivityDic.ContainsKey(config.id))continue;
                TChild child=default;
                bool attached=false;
                if(config.parentActivityID!=null&&config.parentActivityID.Length!=0)
                {
                    child=Family.CreateChild();
                    for(int i=0;i<config.parentActivityID.Length;i++)
                    {
                        if(config.parentActivityID[i]!=ActivityId)continue;
                        Children.Add(child);ChildrenById.Add(config.id,child);SelfViewActivities.Add(config.id,child);
                        Family.Control().ChildActivities.Add(config.id,child);
                        if(childDataDic.TryGetValue(config.id,out var data)){child.EarlyOnInit(data);if(child!=null)child.Refresh(config.id);}
                        attached=true;break;
                    }
                }
                if(attached)continue;
                if(childDataDic.TryGetValue(config.id,out var otherData))
                {
                    child=Family.CreateChild();Family.Control().ChildActivities.Add(config.id,child);OtherViewActivities.Add(config.id,child);
                    child.EarlyOnInit(otherData);if(child!=null)child.Refresh(config.id);
                }
            }
            foreach(var child in SelfViewActivities.Values)if(child.Data.state==3)SelfViewDatas[child.Data.id]=child.ModuleData;
            SortDataDic();
            foreach(var child in OtherViewActivities.Values)if(child.Data.state==3)OtherViewDatas[child.Data.id]=child.ModuleData;
        }
    }
}
