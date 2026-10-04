using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameAchievementTaskPage
    {
        OutgameTaskRow GetTaskItem();
        void SetAchTabReddot();
        void SetTaskEmpty(bool empty);
    }
    public sealed class OutgameAchievementTaskViewServices
    {
        public Func<OutgameAchievementActivity> Activity;
        public Func<OutgameCommonMessageDispatcher> Common;
        public OutgameStatisticsExpansion Statistics;
        public Action ClearSingleton;
        public Action<object[]> Warning;
    }
    // AchiTaskSubUI3986. The source page owns the shared singleton and RootUI assignment.
    public sealed class OutgameAchievementTaskView
    {
        readonly OutgameAchievementTaskViewServices services;
        public IOutgameAchievementTaskPage RootUI;
        public bool LateInited;
        public OutgameAchievementActivity Activity;
        public readonly Dictionary<int,OutgameTaskRow> Rows=new Dictionary<int,OutgameTaskRow>();
        public OutgameAchievementTaskView(OutgameAchievementTaskViewServices services){this.services=services;}
        public void OnInit(){Activity=services.Activity();services.Common().AddListener(OutgameAchievementStrategy.RefreshList,RefreshRows);}
        public void OnLateInit()=>Activity.RefreshSortList();
        public void OnDestroy(){services.ClearSingleton();services.Common().RemoveListener(OutgameAchievementStrategy.RefreshList,RefreshRows);}
        public List<OutgameAchievementItemData> Filter(List<OutgameAchievementItemData> input)
        {
            var result=new List<OutgameAchievementItemData>();var types=new Dictionary<int,OutgameAchievementItemData>();
            for(int i=0;i<input.Count;i++)
            {
                int type=input[i].Config.type;if(types.ContainsKey(type)||input[i].state==1)continue;
                types.Add(type,input[i]);bool visible=true;
                for(int j=0;j<input[i].ShowCondition.Count;j++)
                {long value=services.Statistics.GameValue(input[i].ShowCondition[j].key,Array.Empty<object>());if(value!=input[i].ShowCondition[j].value){visible=false;break;}}
                if(visible)result.Add(input[i]);
            }
            return result;
        }
        public void RefreshRows(object[] args)
        {
            RootUI.SetAchTabReddot();var list=Filter((List<OutgameAchievementItemData>)args[0]);list.Sort(CompareRows);
            if(Rows.Count==0)
                for(int i=0;i<list.Count;i++)if(list[i].state!=1){var row=CreateRow(list[i].Config.type);ApplyData(row,list[i]);row.Refresh();}
            for(int i=0;i<list.Count;i++)
            {try{Rows[list[i].Config.type].Lifetime.Transform.SetAsLastSibling();}catch(Exception){services.Warning(new object[]{list[i].Config.type});}}
            if(list.Count==0)RootUI.SetTaskEmpty(true);
        }
        public static int CompareRows(OutgameAchievementItemData a,OutgameAchievementItemData b)
        {bool ready=a.CanComplete(),other=b.CanComplete();if(ready&&!other)return -1;if(!ready&&other)return 1;return a.CompareTo(b);}
        public OutgameTaskRow CreateRow(int type)
        {
            var row=RootUI.GetTaskItem();Rows[type]=row;
            row.SetData(new OutgameTaskRowData(),OutgameTaskType.Achievement,Claim);row.IsClaim=false;return row;
        }
        public static void ApplyData(OutgameTaskRow row,OutgameAchievementItemData item)
        {
            var config=item.Config;row.Data.dis=config.des;row.Data.rewards=config.rewards;row.Data.liveness=0;
            row.Data.number=unchecked((int)config.number);row.Data.ContentArgument=config.content;row.Data.id=config.id;row.Data.contentType=config.contentType;row.Data.prog=unchecked((int)item.progress);row.ActionCache=true;
        }
        public OutgameAchievementItemData FirstUnclaimed(int type)
        {
            var list=Activity.TypeLists[type];for(int i=0;i<list.Count;i++)if(list[i].state==0)return list[i];return null;
        }
        public void HideType(int type)
        {if(Rows.TryGetValue(type,out var row)){row.ActionCache=false;row.Lifetime.GameObject.SetActive(false);Rows.Remove(type);}}
        public void Claim(int id,long uid)
        {
            Activity.Manager.Strategy.GetAchievementReward(id);int type=Activity.FindAchievement(id).Config.type;
            var next=FirstUnclaimed(type);bool exists=Rows.TryGetValue(type,out var row);
            if(next==null){if(exists)HideType(type);return;}
            if(!exists)row=CreateRow(type);ApplyData(row,next);row.Refresh();
        }
    }
}
