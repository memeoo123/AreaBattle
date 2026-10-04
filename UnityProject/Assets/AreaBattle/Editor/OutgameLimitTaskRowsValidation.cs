using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskRowsValidation
    {
        [Serializable] sealed class ItemRows {public List<SharedItemConfig.GameItemConfig> Datas;}
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTaskDaysValidation.Fixture Days;
            public readonly OutgameLimitTaskListBinding Binding;
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameLimitTaskItemServices Services;
            public readonly List<string> Trace=new List<string>();
            public Action SpriteCallback,LanguageCallback;public Action<object[]> Finished;public object[] LastFormat;
            public int Day=1;public bool AutoRefresh;
            public OutgameChildLimitTimeTaskActivity Child=>Days.Child;
            public OutgameLimitTaskItemData Task=>Child.FindTask(1301101);
            public Fixture(bool native=false,string path=null)
            {
                Days=new OutgameLimitTaskDaysValidation.Fixture(native,path,native);
                foreach(var row in JsonUtility.FromJson<ItemRows>(Resources.Load<TextAsset>("Recovered/FirstPack/Config/GameItemConfig").text).Datas)Days.Tasks.Items.Config.Instance.Items[row.id]=row;
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);}).ReadTable(Config.dicGameItem);
                Services=new OutgameLimitTaskItemServices{Activities=()=>Days.Tasks.Runtime.Control,Messages=()=>Days.Messages,
                    LanguageFormat=(key,args)=>{Trace.Add("language");LastFormat=args;LanguageCallback?.Invoke();return key+":"+string.Join(",",args);},
                    Rewards=new OutgameLimitTaskRewardItemServices{Config=()=>Config,SetSprite=(image,icon,atlas,size)=>{Trace.Add("sprite");SpriteCallback?.Invoke();},PopItemInfo=(rect,id)=>Trace.Add("popup:"+id)},
                    Destroy=native?(Action<GameObject>)(go=>UnityEngine.Object.Destroy(go)):(go=>UnityEngine.Object.DestroyImmediate(go))};
                Binding=new OutgameLimitTaskListBinding(Days.Page,Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskItem"),()=>Days.Pool,Services);
                Days.Messages.AddListener("SevendayFinishTask",args=>{Trace.Add("finished:"+args[0]+":"+args[1]);Finished?.Invoke(args);});
                Days.Common.AddListener("CommonModule_RefreshNoviceTaskList",args=>{if(AutoRefresh&&(int)args[0]==1301&&(int)args[1]==Day)RenderDay(Day);});
                RenderDay(1);if(!native)Tick();
            }
            public void RenderDay(int day){Day=day;Binding.RenderDay(Child,Days.Tasks.Runtime.Statistics.Expansion,day);}
            public void Tick()=>OutgameDynamicListValidation.Tick(Binding.List);
            public OutgameLimitTaskView At(int index)=>(OutgameLimitTaskView)Binding.List.GetItem(index).BaseItem;
            public OutgameLimitTaskView ViewTask(int id)
            {int i=Binding.Data.Data.FindIndex(t=>t.id==id);Binding.List.CenteredWithIndex(i);Tick();return At(i);}
            public void Ready(){Days.Tasks.Runtime.Statistics.Expansion.SetEventCount(10015,10);RenderDay(1);Tick();}
            public void Dispose()=>Days.Dispose();
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original task row/list component, source show filtering and BtnState sorting, actual task activity/reward model, native Button handlers and reentry/failure prefixes. Sprite/localization/detail delivery are observed required endpoints; full page lifecycle/accumulator/preview/menu/Main/Player/audiovisual acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="limit-task-rows-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="limit-task-rows-"+id,result="fail",detail=e.ToString()});}};
            check("original-row-children-targets-and-independent-scroll-binding",()=>{
                using(var f=new Fixture()){
                    var item=f.ViewTask(1301101);
                    Require(ReferenceEquals(item.Data,f.Task)&&ReferenceEquals(item.Child,f.Child)&&ReferenceEquals(item.Parent,f.Days.Tasks.Parent),"actual task/child/parent ownership");
                    Require(item.ProgressItems.Count==f.Task.conditions.Count&&item.RewardItems.Count==f.Task.RewardsData.Count,"original config creates all reward and progress children");
                    Require(item.ProgressItems[0].Lifetime.Transform.parent==item.ProgressLayout.transform&&item.RewardItems[0].Lifetime.Transform.parent==item.RewardLayout.transform,"original outlet layout parents");
                    Require(item.ProgressItems[0].Value.text=="0 / 10"&&item.Goto.gameObject.activeSelf&&!item.Claim.gameObject.activeSelf&&!item.Claimed.gameObject.activeSelf,"original progress and incomplete button state");
                    Require(item.Description.text.StartsWith(f.Task.Config.des.key)&&f.Binding.List.ColumnAlignment==1&&f.Binding.List.SpacingSize==new Vector2(0,8)&&f.Binding.List.SpaceList[0]==10&&!f.Binding.List.Recycle,"description and original task list geometry contract");
                    Require(f.Binding.List.Scroll!=f.Days.Binding.List.Scroll,"task and day columns bind distinct same-name scroll owners");
                }
            });
            check("actual-original-reward-claim-finish-order-and-duplicate-guard",()=>{
                using(var f=new Fixture()){
                    f.Ready();var item=f.ViewTask(1301101);f.Trace.Clear();bool observed=false;
                    f.Finished=args=>{Require(f.Task.state==1&&f.Days.Tasks.Items.Global.GetItemCount(1301)==10&&(int)args[0]==1&&(int)args[1]==1301101,"actual award/state before original finish message");observed=true;};
                    item.Claim.onClick.Invoke();Require(observed&&f.Child.ModuleData.ext.AccProgress==10&&item.Claim.gameObject.activeSelf,"claim does not itself rerender row visuals");
                    item.Claim.onClick.Invoke();Require(f.Days.Tasks.Items.Global.GetItemCount(1301)==10&&f.Trace.Count==2,"duplicate activity guard skips award but UI still sends finish message");
                    f.RenderDay(1);f.Tick();item=f.ViewTask(1301101);Require(item.Claimed.gameObject.activeSelf&&!item.Claim.gameObject.activeSelf,"subsequent source list render displays claimed state");
                }
            });
            check("rejected-task-still-notifies-and-goto-handler-is-empty",()=>{
                using(var f=new Fixture()){
                    var item=f.ViewTask(1301101);f.Trace.Clear();item.Goto.onClick.Invoke();Require(f.Trace.Count==0&&f.Task.state==0,"source goto handler is no-op");
                    item.Claim.onClick.Invoke();Require(f.Task.state==0&&f.Days.Tasks.Items.Global.GetItemCount(1301)==0&&f.Trace.Count==1&&f.Trace[0]=="finished:1:1301101","programmatic unready claim returns from activity then sends finish");
                }
            });
            check("claim-report-failure-retains-award-but-prevents-finish-message",()=>{
                using(var f=new Fixture()){
                    f.Ready();var item=f.ViewTask(1301101);f.Trace.Clear();f.Days.Tasks.ReportHost.Sending=(kind,report)=>{if(kind==OutgameLimitTaskReportKind.Reward)throw new InvalidOperationException("report failure");};
                    Throws<InvalidOperationException>(()=>item.Claim.onClick.Invoke());
                    Require(f.Task.state==1&&f.Days.Tasks.Items.Global.GetItemCount(1301)==10&&!f.Trace.Exists(s=>s.StartsWith("finished:")),"source activity prefix persists while UI message not reached");
                }
            });
            check("row-rebuild-before-data-fetch-and-source-disposal-asymmetry",()=>{
                using(var f=new Fixture()){
                    var item=f.ViewTask(1301101);var data=item.Data;var reward=item.RewardItems[0];var progress=item.ProgressItems[0];var root=item.Lifetime.GameObject;
                    Throws<ArgumentOutOfRangeException>(()=>item.OnRenderer(999));
                    Require(item.RewardItems.Count==0&&reward.Lifetime.IsDisposed&&ReferenceEquals(item.Data,data)&&!progress.Lifetime.IsDisposed,"old rewards cleared before failed data lookup; progress untouched");
                    int index=f.Binding.Data.Data.IndexOf(data);item.OnRenderer(index);var kept=item.ProgressItems[0];Require(progress.Lifetime.IsDisposed,"successful rerender subsequently clears old progress");
                    item.Dispose();Require(root&&item.Lifetime.IsDisposed&&item.RewardItems.Count==0&&item.ProgressItems.Count==1&&!kept.Lifetime.IsDisposed&&kept.Lifetime.GameObject,"source disposal clears only rewards, retains progress and native row");
                }
            });
            check("sprite-failure-leaves-untracked-clone-and-published-new-data",()=>{
                using(var f=new Fixture()){
                    var item=f.ViewTask(1301101);int before=item.RewardLayout.transform.childCount;var progress=item.ProgressItems[0];
                    f.SpriteCallback=()=>throw new InvalidOperationException("sprite failure");Throws<InvalidOperationException>(()=>item.OnRenderer(f.Binding.Data.Data.IndexOf(f.Task)));
                    Require(item.RewardItems.Count==0&&item.RewardLayout.transform.childCount==before&&item.Data==f.Task&&!progress.Lifetime.IsDisposed,"old clone removed, failing new clone exists before list append; progress rebuild not reached");
                }
            });
            check("language-callback-precedes-three-live-button-state-queries",()=>{
                using(var f=new Fixture()){
                    var item=f.ViewTask(1301101);f.LanguageCallback=()=>f.Task.state=1;item.OnRenderer(f.Binding.Data.Data.IndexOf(f.Task));
                    Require(item.Claimed.gameObject.activeSelf&&!item.Goto.gameObject.activeSelf&&item.ProgressItems[0].Value.text=="0 / 10","language side effect is observed by later button eligibility reads");
                    f.LanguageCallback=()=>throw new InvalidOperationException("language failure");f.Task.state=0;Throws<InvalidOperationException>(()=>item.OnRenderer(f.Binding.Data.Data.IndexOf(f.Task)));
                    Require(item.RewardItems.Count==f.Task.RewardsData.Count&&item.ProgressItems.Count==f.Task.conditions.Count&&item.Claimed.gameObject.activeSelf,"children rebuilt before failed description; previous button state retained");
                }
            });
            check("finish-event-rereads-current-row-after-activity-callback",()=>{
                using(var f=new Fixture()){
                    f.Ready();var item=f.ViewTask(1301101);var original=f.Task;int second=f.Binding.Data.Data.FindIndex(t=>t.id!=original.id);int expected=f.Binding.Data.Data[second].id;
                    f.Days.Tasks.ReportHost.Sending=(kind,report)=>{if(kind==OutgameLimitTaskReportKind.Reward)item.OnRenderer(second);};
                    item.Claim.onClick.Invoke();Require(original.state==1&&item.Data.id==expected&&f.Trace.Contains("finished:"+item.Data.Config.day+":"+expected),"claim captures original id; finish message reads row rebound during report callback");
                }
            });
            check("page-filter-reads-live-threshold-and-sorts-only-button-state",()=>{
                using(var f=new Fixture()){
                    var first=f.Task;first.ShowCondition.Clear();first.ShowCondition.Add(new OutgameActivityCondition{key=880,value=5,arg=Array.Empty<object>()});
                    f.Days.Tasks.Runtime.Statistics.Owner.ValueProviders[880]=args=>{var condition=first.ShowCondition[0];condition.value=7;first.ShowCondition[0]=condition;return 6;};
                    f.RenderDay(1);Require(!f.Binding.Data.Data.Contains(first),"filter rereads threshold changed by GameValue callback");
                    first.ShowCondition.Clear();f.Ready();Require(f.Binding.Data.Data[0].BtnState==0,"claimable button state sorts before incomplete");
                    foreach(var task in f.Child.DayTasks[1])task.state=1;f.RenderDay(1);Require(f.Binding.Data.Data.Count==f.Child.DayTasks[1].Count,"filter does not drop claimed rows");
                }
            });
            return r;
        }
    }
}
