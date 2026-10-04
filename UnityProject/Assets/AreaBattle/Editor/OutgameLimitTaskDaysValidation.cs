using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskDaysValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTimeTaskValidation.Fixture Tasks;
            public readonly GameObject Root,Page;public readonly OutgamePrefabPoolControl Pool;
            public readonly OutgameLimitTaskDayListBinding Binding;
            public readonly List<int> SelectedDays=new List<int>();public readonly List<string> LanguageCalls=new List<string>();
            public Action<object[]> Selection;public Action LanguageCallback;
            public OutgameChildLimitTimeTaskActivity Child=>Tasks.Parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(1301);
            public OutgameCommonMessageDispatcher Common=>Tasks.Runtime.Statistics.Common;
            public OutgameMessageDispatcher Messages=>Tasks.Runtime.Statistics.Messages;
            public Fixture(bool native=false,string path=null,bool nativeActivities=false)
            {
                Tasks=new OutgameLimitTimeTaskValidation.Fixture(path,nativeActivities);
                Child.ModuleData.ext.dayId=2;
                Root=new GameObject("OriginalSevenDayList",typeof(RectTransform));Root.SetActive(false);((RectTransform)Root.transform).sizeDelta=new Vector2(1080,1920);
                Page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskUI"),Root.transform,false);Page.SetActive(true);
                var registry=new OutgameControllerRegistry();Pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));
                registry.Bind(4561,()=>Pool);registry.Resolve(4561);Pool.OnInit();
                var services=new OutgameLimitTaskPageItemServices{Activities=()=>Tasks.Runtime.Control,Common=()=>Common,Messages=()=>Messages,
                    LanguageFormat=(key,args)=>{LanguageCalls.Add(key+":"+args[0]);LanguageCallback?.Invoke();return "localized placeholder";}};
                Binding=new OutgameLimitTaskDayListBinding(Page,Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskPageItem"),()=>Pool,services);
                for(int day=1;day<=7;day++)Binding.Data.Data.Add(new OutgameLimitTaskPageItem(day,1301));Binding.Data.UpdateList();
                Messages.AddListener(OutgameLimitTaskPageView.SelectPage,args=>{SelectedDays.Add((int)args[1]);Selection?.Invoke(args);});
                if(native){var canvas=Root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;Root.AddComponent<GraphicRaycaster>();Root.SetActive(true);}
                else Tick();
            }
            public void Tick()=>OutgameDynamicListValidation.Tick(Binding.List);
            public OutgameLimitTaskPageView At(int index)=>(OutgameLimitTaskPageView)Binding.List.GetItem(index).BaseItem;
            public void Dispose()
            {
                if(Application.isPlaying)UnityEngine.Object.Destroy(Root);else UnityEngine.Object.DestroyImmediate(Root);
                Pool.OnDispose();Tasks.Dispose();
            }
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original seven-day page/day-item prefabs, source shared selection provider and actual limited-task child1301. Day buttons/messages/red updates/disposal verified; full task list, accumulator/preview, page lifecycle and production menu/Main remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="limit-task-days-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="limit-task-days-"+id,result="fail",detail=e.ToString()});}};
            check("duplicate-source-scroll-names-resolve-distinct-outlets",()=>{
                using(var f=new Fixture()){
                    var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/LimitTask/hud-import").text,"CommonLimitTimeTaskUI");var map=new Dictionary<string,GameObject>();foreach(var pair in outlets.Read(f.Page))map.Add(pair.Key,(GameObject)pair.Value);
                    Require(map["dynamicList"]==f.Binding.List.gameObject&&map["Content"].transform.parent.parent!=f.Binding.List.transform.parent.parent,"source identity distinguishes two same-name Scroll Views");
                    Require(map["Content"].transform.parent.parent.name==f.Binding.List.transform.parent.parent.name&&f.Binding.List.Scroll.content==f.Binding.List.transform,"both original names retained and day scroll owns correct content");
                    Require(f.Binding.List.Regions.Count==7&&f.Binding.List.SpacingSize==new Vector2(0,30)&&f.Binding.List.SpaceList[0]==30&&!f.Binding.List.Recycle,"original seven day rows and source scroll configuration");
                }
            });
            check("original-seven-day-label-lock-and-selection-message",()=>{
                using(var f=new Fixture()){
                    var one=f.At(0);var two=f.At(1);var three=f.At(2);
                    Require(one.Normal.gameObject.activeSelf&&two.Normal.gameObject.activeSelf&&three.Locked.gameObject.activeSelf&&!three.Normal.gameObject.activeSelf,"original current day2 gates normal versus locked display");
                    Require(one.NormalName.text=="1天"&&one.SelectedName.text=="1天"&&one.LockedName.text=="1天"&&f.LanguageCalls[0].EndsWith(":1"),"language call executes before source Chinese labels overwrite it");
                    one.Normal.onClick.Invoke();two.Normal.onClick.Invoke();
                    Require(f.SelectedDays.Count==2&&f.SelectedDays[0]==1&&f.SelectedDays[1]==2&&!f.Binding.Data.Data[0].Selected&&f.Binding.Data.Data[1].Selected,"actual native buttons use provider selection and original page event");
                    Require(one.Normal.gameObject.activeSelf&&!one.Selected.gameObject.activeSelf&&two.Selected.gameObject.activeSelf&&!two.Normal.gameObject.activeSelf,"deselect and select update source three graphics");
                    two.Normal.onClick.Invoke();Require(f.SelectedDays.Count==2,"repeated programmatic button invocation does not repeat already-selected event");
                }
            });
            check("actual-task-red-refresh-filters-and-render-selection-replay",()=>{
                using(var f=new Fixture()){
                    foreach(var tasks in f.Child.DayTasks.Values)foreach(var entry in tasks)entry.state=1;
                    f.Binding.List.RefreshSoft();Require(!f.At(0).Red.activeSelf,"no unclaimed task leaves no red");
                    var task=f.Child.FindTask(1301101);task.state=0;foreach(var condition in task.conditions)condition.value=long.MaxValue;
                    f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[]{1301,2});Require(!f.At(0).Red.activeSelf,"other-day refresh does not update first day's stale red");
                    f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[]{1301,1});Require(f.At(0).Red.activeSelf,"matching message queries actual task CanComplete");
                    task.state=1;f.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301});Require(!f.At(0).Red.activeSelf,"activity-wide refresh clears claimed task red");
                    f.Binding.Data.SetSelect(1);f.SelectedDays.Clear();f.Binding.Data.UpdateItemData(1);Require(f.SelectedDays.Count==1&&f.SelectedDays[0]==2&&f.At(1).Selected.gameObject.activeSelf,"selected row rerender resets visuals then replays page message");
                }
            });
            check("language-callback-rereads-day-and-source-partial-failure",()=>{
                using(var f=new Fixture()){
                    var one=f.At(0);f.LanguageCallback=()=>{f.Binding.Data.Data[0].day=3;f.Child.ModuleData.ext.dayId=3;};one.OnRenderer(0);
                    Require(one.Data.day==3&&one.NormalName.text=="3天"&&one.Normal.gameObject.activeSelf,"later day/extension reads observe callback mutation");
                    f.LanguageCallback=()=>throw new InvalidOperationException("language failure");Throws<InvalidOperationException>(()=>one.OnRenderer(1));
                    Require(ReferenceEquals(one.Data,f.Binding.Data.Data[1])&&ReferenceEquals(one.Child,f.Child)&&one.NormalName.text=="3天","new data and child published before failed language request");
                }
            });
            check("malformed-message-retains-original-short-array-and-unbox-failures",()=>{
                using(var f=new Fixture()){
                    f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",null);f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[0]);
                    Throws<IndexOutOfRangeException>(()=>f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[]{999}));
                    Throws<InvalidCastException>(()=>f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[]{999,"not an Int32"}));
                    Throws<InvalidCastException>(()=>f.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301L}));
                }
            });
            check("select-callback-single-row-hiding-and-no-dirty-write",()=>{
                using(var f=new Fixture()){
                    var one=f.At(0);f.Tasks.Parent.Dirty=false;
                    f.Selection=args=>f.Binding.Data.Data.RemoveRange(1,6);f.Binding.Data.SetSelect(0);
                    Require(!one.Lifetime.GameObject.activeSelf&&one.Data.Selected&&!f.Tasks.Parent.Dirty,"after page message, live provider count1 hides row without dirtying activity");
                }
            });
            check("programmatic-locked-selection-and-deselection-have-no-extra-gate",()=>{
                using(var f=new Fixture()){
                    var locked=f.At(2);Require(locked.Locked.gameObject.activeSelf,"day3 originally locked");f.Binding.Data.SetSelect(2);
                    Require(locked.Selected.gameObject.activeSelf&&f.SelectedDays[0]==3,"provider permits direct selected locked day; gating belongs to visible button");
                    f.Binding.Data.KillSelect();Require(locked.Normal.gameObject.activeSelf&&!locked.Locked.gameObject.activeSelf,"source deselect shows normal unconditionally until next render");
                    f.Binding.Data.UpdateItemData(2);Require(locked.Locked.gameObject.activeSelf&&!locked.Normal.gameObject.activeSelf,"subsequent render restores actual day gate");
                }
            });
            check("dispose-keeps-native-row-removes-subscriptions-and-runtime-button-listeners",()=>{
                using(var f=new Fixture()){
                    var one=f.At(0);var root=one.Lifetime.GameObject;var rect=one.Lifetime.RectTransform;bool foreign=false;one.Normal.onClick.AddListener(()=>foreign=true);
                    one.Dispose();one.Normal.onClick.Invoke();Require(root&&one.Lifetime.GameObject==null&&one.Lifetime.IsDisposed&&ReferenceEquals(rect,one.Lifetime.RectTransform)&&!foreign,"DynamicBaseItem preserves native object and removes every runtime button listener");
                    one.Data.day=999;f.Common.SendMessage("CommonModule_RefreshNoviceTaskList",new object[]{1301,999});f.Common.SendMessage("CommonModule_NoviceExtRefresh",new object[]{1301});
                    Require(one.Data.day==999&&ReferenceEquals(one.Child,f.Child),"removed subscriptions do not dereference missing day while held data/child remain");
                }
            });
            return r;
        }
    }
}
