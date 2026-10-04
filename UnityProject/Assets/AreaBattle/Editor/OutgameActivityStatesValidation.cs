using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityStatesValidation
    {
        public static void Require(bool ok,string text){if(!ok)throw new Exception(text);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public sealed class Probe:OutgameActivityBase
        {
            public Action InitAction,ResetAction,UpdateAction,DisposeAction;public Action<int> RefreshAction;
            public int Resets,Inits;
            public Probe(OutgameActivityServices services):base(services){}
            public override void OnInit(){Inits++;InitAction?.Invoke();}
            public override void ResetProgress(){Resets++;ResetAction?.Invoke();}
            public override void Update()=>UpdateAction?.Invoke();
            public override void Dispose()=>DisposeAction?.Invoke();
            public override void Refresh(int id){if(RefreshAction!=null)RefreshAction(id);else base.Refresh(id);}
        }
        public sealed class Ui:IOutgameActivityUiHost
        {
            public int Calls;public string LastNamespace,LastName;public object[] Args;public Action Called;
            public OutgameUiPage Open(string ns,string name,object[] args){Calls++;LastNamespace=ns;LastName=name;Args=args;Called?.Invoke();return null;}
        }
        public sealed class Fixture:IDisposable
        {
            readonly Func<int,OutgameActivityConfigRow> previous;
            public readonly OutgameStatisticsOffNetValidation.Fixture Statistics;
            public readonly OutgameFsmManager Manager=new OutgameFsmManager();
            public readonly List<string> Trace=new List<string>();public readonly Ui UiHost=new Ui();
            public OutgameActivityConfigRow Config;
            public OutgameActivityItemData Data;
            public readonly OutgameActivityServices Services;public readonly Probe Activity;
            public readonly GameObject Root;public readonly Button Button;public readonly Text Text;
            public int Dirty,NoticePop,LaunchPop,Closed;public Action DirtyAction;
            public Fixture(bool native=false)
            {
                Statistics=new OutgameStatisticsOffNetValidation.Fixture(native:native);Statistics.Init();
                Config=new OutgameActivityConfigRow{id=7,uniqueId="one",open=1,noticeType=new[]{701},noticeParams=new[]{"1"},launchType=new[]{702},launchParams=new[]{"1"},overType=new[]{703},overParams=new[]{"1"},closeType=new[]{704},closeParams=new[]{"1"},noticeDes=new OutgameActivityConfigRow.Name{key="Need {0}"}};
                Data=new OutgameActivityItemData{id=7,uniqueId="one"};previous=OutgameActivityItemData.ConfigurationResolver;OutgameActivityItemData.ConfigurationResolver=id=>Config;
                Services=new OutgameActivityServices{GetItemData=id=>Data,GetConfig=id=>Config,FsmManager=()=>Manager,Common=()=>Statistics.Common,Statistics=Statistics.Expansion,
                    SetDirty=b=>{Dirty++;Trace.Add("dirty");DirtyAction?.Invoke();},Notice=(a,id)=>Trace.Add("notice:"+id),Launch=(a,id)=>Trace.Add("launch:"+id),Over=(a,id)=>Trace.Add("over:"+id),Closed=id=>{Closed++;Trace.Add("closed:"+id);},
                    QueueNotice=e=>{NoticePop++;Trace.Add("notice-pop");},QueueLaunch=e=>{LaunchPop++;Trace.Add("launch-pop");},Ui=()=>UiHost,Format=(key,args)=>string.Format(key,args)};
                Activity=new Probe(Services){NoticePopUi=typeof(Fixture),LaunchPopUi=typeof(Probe)};
                Root=new GameObject("activity-state-widget-fixture",typeof(RectTransform));Button=new GameObject("button",typeof(RectTransform),typeof(Button)).GetComponent<Button>();Button.transform.SetParent(Root.transform,false);
                Text=new GameObject("description",typeof(RectTransform),typeof(Text)).GetComponent<Text>();Text.transform.SetParent(Root.transform,false);
                Activity.Buttons.Add(Button);Activity.Descriptions.Add(Text);
            }
            public void Set(int id,long value)=>Statistics.Expansion.SetEventCount(id,value);
            public void Start(int state=1){Data.state=state;Activity.Refresh(7);}
            public void Dispose()
            {
                try{Activity.OnDispose();}finally{OutgameActivityItemData.ConfigurationResolver=previous;Statistics.Dispose();UnityEngine.Object.DestroyImmediate(Root);}
            }
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Original activity base/four states/widgets through actual statistics/common messages/FSM. ActivityControl state bookkeeping/pop queues/config/Main/UI registry remain explicit required hosts. Separate native suite validates runtime buttons/frames; no original activity page/Player claim."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("activity-states-real-statistics-close-notice-launch-over-close",()=>{
                using(var f=new Fixture())
                {
                    f.Start();Require(f.Activity.Fsm.CurrentState is OutgameActivityCloseState&&!f.Button.gameObject.activeSelf&&f.Closed==1,"initial close and hide");
                    f.Set(701,1);Require(f.Activity.Fsm.CurrentState is OutgameActivityNoticeState&&f.Data.state==2&&f.Button.gameObject.activeSelf&&f.Text.text=="Need 1"&&f.NoticePop==1&&f.Data.WarmTimeStamp>0,"actual statistics enters notice with widgets/pop/time");
                    f.Set(702,1);Require(f.Activity.Fsm.CurrentState is OutgameActivityLaunchState&&f.Data.state==3&&!f.Text.gameObject.activeSelf&&f.LaunchPop==1,"actual launch transition");
                    f.Set(703,1);Require(f.Activity.Fsm.CurrentState is OutgameActivityOverState&&f.Data.state==4&&f.Data.launchPop&&!f.Button.gameObject.activeSelf,"over flag and hidden button");
                    f.Set(704,1);Require(f.Activity.Fsm.CurrentState is OutgameActivityCloseState&&f.Activity.Resets==1&&!f.Data.launchPop&&!f.Data.noticePop,"over closes/reset; original popup flags clear");
                }
            });
            check("activity-states-entry-priorities-and-transition-hook-order",()=>{
                using(var f=new Fixture())
                {
                    f.Set(701,1);f.Set(702,1);f.Set(703,1);f.Start();Require(f.Activity.Fsm.CurrentState is OutgameActivityOverState,"notice then launch then over via nested OnEnter");
                    int notice=f.Trace.IndexOf("notice:7"),launch=f.Trace.IndexOf("launch:7"),over=f.Trace.IndexOf("over:7"),closed=f.Trace.IndexOf("closed:7");
                    Require(notice>=0&&notice<launch&&launch<over&&over<closed&&f.NoticePop==0&&f.LaunchPop==0,"close chooses notice first; original close callback still runs after nested transition");
                }
                using(var f=new Fixture())
                {
                    f.Set(702,1);f.Start();Require(f.Activity.Fsm.CurrentState is OutgameActivityLaunchState&&f.Trace.IndexOf("launch-pop")<f.Trace.IndexOf("launch:7"),"initial direct launch hook happens after state entry/popup");
                }
            });
            check("activity-states-initial-disabled-noncanonical-open-and-invalid-state",()=>{
                using(var f=new Fixture()){f.Config.open=0;f.Set(701,1);f.Start(3);Require(f.Data.state==1&&f.Activity.Fsm.CurrentState is OutgameActivityCloseState,"disabled config forces close; conditions gated");}
                using(var f=new Fixture()){f.Config.open=2;f.Set(702,1);f.Start(4);Require(f.Data.state==3&&f.Activity.Fsm.CurrentState is OutgameActivityLaunchState,"open2 initially starts close but nonzero conditions can launch");}
                using(var f=new Fixture()){f.Start(99);Require(f.Activity.Fsm!=null&&!f.Activity.Fsm.IsRunning&&f.Data.state==99,"unknown saved state leaves created FSM unstarted");Throws<OutgameFrameworkException>(()=>f.Activity.Refresh(7));}
            });
            check("activity-states-unique-id-reset-order-and-dirty-failure",()=>{
                using(var f=new Fixture())
                {
                    f.Data.uniqueId="old";f.Data.LaunchTimeStamp=9;f.Data.WarmTimeStamp=8;f.Activity.ResetAction=()=>Require(f.Data.uniqueId=="one"&&f.Data.LaunchTimeStamp==0&&f.Data.WarmTimeStamp==8,"ID changes and only LaunchTimeStamp clears before reset");
                    f.Start();Require(f.Activity.Resets==1&&f.Dirty==2,"identity reset dirty then state entry dirty");
                }
                using(var f=new Fixture())
                {
                    f.Data.uniqueId="old";f.Data.LaunchTimeStamp=9;f.DirtyAction=()=>throw new InvalidOperationException("dirty");Throws<InvalidOperationException>(()=>f.Start());
                    Require(f.Data.uniqueId=="one"&&f.Data.LaunchTimeStamp==9&&f.Activity.Resets==0&&f.Activity.Fsm.CurrentState is OutgameActivityCloseState,"dirty failure retains ID change and current FSM but aborts reset/listeners");f.DirtyAction=null;
                }
            });
            check("activity-states-refresh-replacement-and-init-failure-gate",()=>{
                using(var f=new Fixture())
                {
                    f.Activity.InitAction=()=>throw new InvalidOperationException("init");Throws<InvalidOperationException>(()=>f.Start());Require(!f.Activity.IsInit&&f.Activity.Fsm==null&&ReferenceEquals(f.Activity.Data,f.Data),"clear init flag before virtual callback");
                    f.Activity.InitAction=null;f.Activity.Refresh(7);Require(f.Activity.Inits==1&&f.Activity.Fsm.IsRunning,"failed init is not automatically retried");
                    var held=f.Activity.Config;f.Data=null;f.Activity.Refresh(88);Require(f.Activity.ActivityId==88&&f.Activity.Data==null&&ReferenceEquals(f.Activity.Config,held),"missing data returns retaining config/FSM");f.Data=new OutgameActivityItemData{id=7,uniqueId="one"};f.Activity.Data=f.Data;
                }
            });
            check("activity-states-refresh-event-source-flags-and-double-launch-ui",()=>{
                using(var f=new Fixture())
                {
                    f.Start(2);Require(f.NoticePop==1,"saved notice displays even notice threshold not reached");
                    f.Data.state=3;f.Activity.Refresh(7);Require(f.Activity.Fsm.CurrentState is OutgameActivityLaunchState&&f.LaunchPop==2&&!f.Data.launchPop,"notice change event enters launch, then clears flag and refreshes launch widgets again");
                    f.Data.state=4;f.Activity.Refresh(7);Require(f.Activity.Fsm.CurrentState is OutgameActivityOverState&&f.Data.launchPop&&f.Data.noticePop,"launch event marks both flags before over");
                }
            });
            check("activity-states-native-button-listener-rebind-and-pop-payload",()=>{
                using(var f=new Fixture())
                {
                    int stale=0;f.Button.onClick.AddListener(()=>stale++);f.Start(2);f.Button.onClick.Invoke();Require(stale==0&&f.UiHost.LastName==nameof(Fixture)&&ReferenceEquals(f.UiHost.Args,Array.Empty<object>()),"widget replaces listener and uses notice UI type");
                    var stored=f.Activity.NoticePopEvent;var payload=new object[]{4};stored.Args=payload;f.Activity.OpenBindNoticeUi(new OutgameActivityPopEvent());Require(ReferenceEquals(f.UiHost.Args,payload),"OpenBindNoticeUi ignores its parameter and reads stored event");
                    f.Data.noticePop=true;f.Activity.FireEvent(2);Require(f.NoticePop==1,"existing popup flag suppresses queued notice");
                    f.Set(702,1);f.Button.onClick.Invoke();Require(f.UiHost.LastName==nameof(Probe)&&f.LaunchPop==1,"launch click rebound to launch UI type");
                }
            });
            check("activity-states-countdown-format-live-text-and-sticky-description-array",()=>{
                Require(OutgameActivityStateBase.FormatDuration(90061)=="1d01h01m"&&OutgameActivityStateBase.FormatDuration(3661)=="01h01m01s"&&OutgameActivityStateBase.FormatDuration(-1)=="00h00m-1s","original first-three segments and negative remainder");
                using(var f=new Fixture())
                {
                    f.Config.noticeType=new[]{10000};f.Config.noticeParams=new[]{"20261003120000"};long target=OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0));f.Statistics.Owner.ValueProviders[10000]=a=>target-3661000;f.Start(2);
                    Require(f.Text.text=="Need 01h01m01s","initial actual Text countdown");var state=(OutgameActivityNoticeState)f.Activity.Fsm.CurrentState;
                    f.Statistics.Owner.ValueProviders[10000]=a=>target-1000;state.DateUpdate(null);Require(f.Text.text=="Need 00h00m01s"&&state.DescriptionArgs.Length==1,"clock callback updates cached description array");
                    f.Config.noticeType=new[]{701,701};f.Config.noticeParams=new[]{"1","2"};Throws<IndexOutOfRangeException>(()=>state.DateUpdate(null));Require(state.DescriptionArgs.Length==1,"description cache does not resize when config grows");
                }
            });
            check("activity-states-original-launch-unsubscribe-array-mismatch",()=>{
                using(var f=new Fixture())
                {
                    f.Start(3);var state=(OutgameActivityLaunchState)f.Activity.Fsm.CurrentState;f.Set(703,1);Require(state.IsLeave,"source exit flag set after removal");
                    var map=(Dictionary<string,Action<object[]>>)typeof(OutgameCommonMessageDispatcher).GetField("listeners",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(f.Statistics.Common);
                    Require(Array.Exists(map["703"].GetInvocationList(),d=>ReferenceEquals(d.Target,state))&&Array.Exists(map["704"].GetInvocationList(),d=>ReferenceEquals(d.Target,state)),"crossed source removal leaves dormant launch callbacks on original keys");
                    int count=f.Trace.Count;state.OverEvent(null);state.CloseEvent(null);Require(f.Trace.Count==count,"retained callbacks short-circuit after leave");
                }
            });
            check("activity-states-child-update-refresh-dispose-and-retained-fsm",()=>{
                using(var f=new Fixture())
                {
                    var child=new Probe(f.Services){ActivityId=8};var grandchild=new Probe(f.Services);child.Children.Add(grandchild);var trace=new List<string>();
                    child.RefreshAction=id=>trace.Add("refresh:"+id);child.UpdateAction=()=>trace.Add("child");grandchild.UpdateAction=()=>trace.Add("grandchild");f.Activity.UpdateAction=()=>trace.Add("parent");
                    child.DisposeAction=()=>trace.Add("dispose-child");grandchild.DisposeAction=()=>trace.Add("dispose-grandchild");f.Activity.DisposeAction=()=>trace.Add("dispose-parent");
                    f.Activity.Children.Add(child);f.Activity.ChildrenById.Add(8,child);f.Start();f.Activity.BaseUpdate();var held=f.Activity.Fsm;f.Activity.OnDispose();
                    Require(string.Join(",",trace)=="refresh:8,child,parent,dispose-grandchild,dispose-child,dispose-parent","refresh/update/dispose source child order; BaseUpdate does not recursively tick grandchildren");
                    Require(f.Activity.Children.Count==0&&f.Activity.ChildrenById.Count==0&&ReferenceEquals(held,f.Activity.Fsm)&&held.IsDestroyed&&!f.Activity.IsInit,"dispose clears collections but retains source lifecycle fields/FSM pointer");
                }
            });
            return report;
        }
    }
}
