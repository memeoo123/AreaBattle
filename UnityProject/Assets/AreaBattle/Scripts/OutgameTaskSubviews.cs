using System;
namespace AreaBattle
{
    // TaskSingleton3990 publishes the instance before invoking constrained ITaskSubUI.OnInit.
    // One owner is shared by all task pages in the same application composition.
    public sealed class OutgameTaskSingleton<T> where T:class
    {
        readonly Func<T> create;readonly Action<T> initialize;
        public T Instance;
        public OutgameTaskSingleton(Func<T> create,Action<T> initialize){this.create=create;this.initialize=initialize;}
        public T I {get{if(Instance==null){Instance=create();initialize(Instance);}return Instance;}}
        public void Clear()=>Instance=null;
    }
    public sealed class OutgameTaskSubviews
    {
        public readonly OutgameTaskSingleton<OutgameDailyTaskView> Daily;
        public readonly OutgameTaskSingleton<OutgameAchievementTaskView> Achievement;
        public OutgameTaskSubviews(OutgameDailyTaskViewServices daily,OutgameAchievementTaskViewServices achievement)
        {
            Daily=new OutgameTaskSingleton<OutgameDailyTaskView>(()=>new OutgameDailyTaskView(daily),view=>view.OnInit());
            Achievement=new OutgameTaskSingleton<OutgameAchievementTaskView>(()=>new OutgameAchievementTaskView(achievement),view=>view.OnInit());
            daily.ClearSingleton=Daily.Clear;achievement.ClearSingleton=Achievement.Clear;
        }
    }
}
