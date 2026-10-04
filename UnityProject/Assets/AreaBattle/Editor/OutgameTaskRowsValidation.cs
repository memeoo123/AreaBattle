using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskRowsValidation
    {
        public sealed class Effects:IOutgameShopCurrencyEffects
        {
            public Action Call;public Action Completion;public int Kind,Amount;public Transform Root;public Vector3 Position;public bool Apply,Display;
            void Fly(int kind,int amount,Transform root,Vector3 position,bool apply,Action completion,bool display)
            {Kind=kind;Amount=amount;Root=root;Position=position;Apply=apply;Completion=completion;Display=display;Call?.Invoke();}
            public void FlyMoney(int amount,Transform root,Vector3 position,bool apply,Action completion,bool display)=>Fly(1,amount,root,position,apply,completion,display);
            public void FlyDiamonds(int amount,Transform root,Vector3 position,bool apply,Action completion,bool display)=>Fly(2,amount,root,position,apply,completion,display);
        }
        public sealed class Fixture:IDisposable,IOutgameDailyTaskPage
        {
            public readonly OutgameTaskActivityValidation.Fixture Tasks;
            public readonly GameObject Root,Page;public readonly OutgameScalarTweenRunner Motion;
            public readonly OutgameTaskRowServices Services;public readonly OutgameDailyTaskViewServices DailyServices;public readonly OutgameDailyTaskView Daily;
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly Effects Fx=new Effects();public readonly List<string> Trace=new List<string>();
            public readonly List<OutgameTaskRow> Created=new List<OutgameTaskRow>();public readonly List<object[]> Warnings=new List<object[]>(),Errors=new List<object[]>();
            public readonly List<string[]> Reported=new List<string[]>();
            public Action SpriteCallback,ProgressCallback;public Action<int,int> Award;public Action<string[]> Report;
            public int TopRefresh,RedRefresh,SingletonClear;public bool Empty;public string Countdown;public float Progress;
            public string IconName,AtlasName;public bool NativeSize;public float End,Duration;public int Ease;public Action Completion;
            public Func<Lang,string> Value;public Func<string,string> Language;
            readonly bool native;readonly string manifest;readonly Transform parent;readonly GameObject template;
            public Fixture(bool native=false,string path=null,bool achievement=false,Action<OutgameTaskActivityValidation.Fixture> configureBeforeInit=null)
            {
                this.native=native;Tasks=new OutgameTaskActivityValidation.Fixture(path,native,achievement:achievement,configureBeforeInit:configureBeforeInit);Root=new GameObject("TaskRowsValidation",typeof(RectTransform));
                Page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TaskPanel/TaskPanelUI"),Root.transform,false);Page.SetActive(true);
                manifest=File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-ui-import.json"));
                var nodes=new Dictionary<string,GameObject>();foreach(var p in new OutgameImportedUiOutlets(manifest,"TaskPanelUI").Read(Page))nodes.Add(p.Key,(GameObject)p.Value);
                parent=nodes["TaskItemContent"].transform;template=nodes["TaskItemItem"];Motion=Root.AddComponent<OutgameScalarTweenRunner>();Motion.enabled=native;
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});reader.ReadTable(Config.dicGameItem);Config.statisticEventConfig=reader.ReadValue<StatisticEventConfig>();
                Value=l=>"完成 {0}";Language=s=>s;Fx.Call=()=>Trace.Add("fly");
                Services=new OutgameTaskRowServices{Config=()=>Config,LanguageValue=l=>Value(l),Language=s=>Language(s),GoodsType=GoodsType,
                    RandomReward=(kind,count)=>new KeyValuePair<int,int>(1001,80),SetSprite=(image,icon,atlas,size)=>{IconName=icon;AtlasName=atlas;NativeSize=size;Trace.Add("sprite");SpriteCallback?.Invoke();},
                    Effects=()=>Fx,ShowAward=(id,count)=>{Trace.Add("award:"+id+":"+count);Award?.Invoke(id,count);},RefreshTopInfo=()=>{TopRefresh++;Trace.Add("top");},
                    Warning=Warnings.Add,Error=Errors.Add,ReportActivityReward=(a,b,c,d)=>{var row=new[]{a,b,c,d};Reported.Add(row);Trace.Add("report");Report?.Invoke(row);},
                    MoveX=(target,end,duration,ease,complete)=>{End=end;Duration=duration;Ease=ease;Completion=complete;Trace.Add("slide");Motion.To(()=>target.localPosition.x,v=>{var p=target.localPosition;p.x=v;target.localPosition=p;},end,duration,complete,ease);},
                    Messages=()=>Messages,Destroy=native?(Action<GameObject>)(go=>UnityEngine.Object.Destroy(go)):(go=>UnityEngine.Object.DestroyImmediate(go))};
                DailyServices=new OutgameDailyTaskViewServices{DailyActivity=()=>Tasks.Child,Config=()=>Tasks.Config,Common=()=>Tasks.Runtime.Statistics.Common,GameValue=(key,args)=>1,DailyActivityId=()=>130001,FormatHours=s=>"seconds:"+s,ClearSingleton=()=>SingletonClear++};
                Daily=new OutgameDailyTaskView(DailyServices){RootUI=this};Daily.OnInit();Daily.OnLateInit();
            }
            int GoodsType(int id){if(id==1001)return 1;if(id==1002)return 2;if(id==1005)return 7;if(id>=2001&&id<=2999)return 3;if(id>=100&&id<=399)return 4;if(id>0&&id<100)return 5;return Config.dicGameItem.ContainsKey(id)?6:0;}
            public OutgameTaskRow GetTaskItem()
            {var root=UnityEngine.Object.Instantiate(template,parent,false);root.SetActive(true);var item=new OutgameTaskRow(root,new OutgameImportedUiOutlets(manifest,"TaskItemItem"),Services);Created.Add(item);return item;}
            public void SetDailyProgress(float value){Progress=value;Page.transform.Find("Content/Content/TaskContent/ProgressBg/Prog").GetComponent<Slider>().value=value;ProgressCallback?.Invoke();}
            public void SetTaskEmpty(bool empty){Empty=empty;Page.transform.Find("Content/Content/TaskContent/Scroll View/Viewport/TaskItemContent/TipText").gameObject.SetActive(empty);}
            public void RefreshPointDailyLivenessBar(){RedRefresh++;Trace.Add("red");}
            public void SetDailyCountdown(string text){Countdown=text;}
            public OutgameTaskRow Ready(int id=1)
            {var task=Tasks.Child.ModuleData.tasks.Single(t=>t.id==id);foreach(var c in task.conditions)c.value=task.Config.conditionParams[task.conditions.IndexOf(c)].datas.Last();Daily.RefreshRows();return Daily.Rows[id];}
            public void Dispose(){Daily.OnDestroy();Tasks.Dispose();if(native)UnityEngine.Object.Destroy(Root);else UnityEngine.Object.DestroyImmediate(Root);}
        }
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameTaskRowData Data(int kind=123)=>new OutgameTaskRowData{id=17,Uid=88,liveness=12,number=10,prog=4,contentType=kind,dis=new Lang{key="row"},rewards=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,5}}}};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original task row with daily-subview projection/claim and original static page. Required effects/sprite/award/report/countdown-format endpoints are observed; full TaskPanelUI lifecycle/tabs/liveness preview, production Main, Achievement and audiovisual/Player acceptance remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="task-rows-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-rows-"+id,result="fail",detail=e.ToString()});}};
            check("original-page-outlets-eight-real-task-projections-and-graphics",()=>{
                using(var f=new Fixture()){
                    Require(f.Daily.Rows.Count==8&&f.Created.Count==8&&f.Progress==0&&!f.Empty,"original eight tasks create eight row projections");
                    var task=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);var row=f.Daily.Rows[1];
                    Require(ReferenceEquals(row.Data.rewards,task.Config.rewards)&&ReferenceEquals(row.Data.dis,task.Config.des)&&row.Data.Uid==task.uid&&row.Data.number==task.Config.conditionParams[0].datas[1],"shared source config lists and signed uid");
                    Require(row.Title.font&&row.Icon.sprite&&row.Target.text=="0/1"&&!row.Claim.gameObject.activeSelf,"original fonts/sprites and native progress fields");
                    var graphics=f.Page.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("RecoveredGraphic_")).ToArray();Require(graphics.Length==2&&graphics.All(i=>i.sprite&&i.rectTransform.anchorMin==Vector2.zero&&i.rectTransform.anchorMax==Vector2.one),"both serialized duplicate images preserved as full-rect child graphics");
                    Require(new OutgameImportedUiOutlets(File.ReadAllText(Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43/generated/outgame/task-panel-ui-import.json")),"TaskPanelUI").Read(f.Page).Count()==18,"source outlet identities remain valid after added graphic children/rows");
                }
            });
            check("daily-claim-waits-for-slide-real-award-and-liveness",()=>{
                using(var f=new Fixture()){
                    var row=f.Ready();long uid=row.Data.Uid;f.Trace.Clear();row.Claim.onClick.Invoke();
                    Require(!row.Claim.enabled&&f.Tasks.Child.TasksByUid[uid].state==0&&f.Tasks.Items.Global.GetItemCount(1001)==0,"click disables button but does not award");
                    Require(f.Fx.Kind==1&&f.Fx.Amount==50&&f.Fx.Root==null&&!f.Fx.Apply&&f.Fx.Display&&f.End==1600&&f.Duration==.7f&&f.Ease==3,"source effect and OutSine .7s slide contract");
                    f.Motion.Advance(.35f);Require(row.Lifetime.Transform.localPosition.x>1100&&f.Tasks.Child.TasksByUid[uid].state==0,"half-time sine motion precedes claim");
                    f.Motion.Advance(.36f);Require(f.Tasks.Child.TasksByUid[uid].state==1&&f.Tasks.Items.Global.GetItemCount(1001)==50&&f.Daily.Liveness==20&&f.RedRefresh==1,"animation completion invokes concrete task award/liveness/page refresh");
                    Require(row.Claim.enabled&&!row.Lifetime.GameObject.activeSelf&&f.Reported.Count==1&&f.Reported[0][0]=="DailyTask","source completion re-enables then hides and reports");
                    row.ClaimReward();Require(f.Tasks.Items.Global.GetItemCount(1001)==50&&f.Reported.Count==2,"UI IsClaim is not set; activity guard prevents duplicate award");
                    f.Fx.Completion();Require(f.TopRefresh==1,"currency completion updates top independently of task claim");
                }
            });
            check("ordinary-progress-clamp-negative-and-rank-binary-display",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();var d=Data();row.SetData(d,OutgameTaskType.DailyTask,null);d.prog=20;row.Refresh();Require(d.prog==10&&row.Target.text=="10/10"&&row.Claim.gameObject.activeSelf,"upper clamp mutates display data");
                    d.prog=-2;row.RefreshProgress();Require(d.prog==-2&&row.Target.text=="-2/10"&&row.Fill.fillAmount==0,"negative text retained native image clamps");
                    d.contentType=f.Config.statisticEventConfig.ArenaRank;d.number=-5;d.prog=8;row.RefreshProgress();Require(row.Target.text=="0/5"&&!row.Claim.gameObject.activeSelf,"rank not ready shows zero instead of partial progress");
                    d.prog=3;row.RefreshProgress();Require(row.Target.text=="5/5"&&row.Fill.fillAmount==1&&d.prog==3,"rank uses negative absolute comparison without mutating progress");
                    d.number=int.MinValue;d.prog=int.MinValue;row.RefreshProgress();Require(row.Target.text=="-2147483648/-2147483648","original wasm absolute-value overflow remains Int32 minimum");
                }
            });
            check("achievement-layout-retained-state-and-live-localization",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();var d=Data(f.Config.statisticEventConfig.CommanderUpgrade);d.ContentArgument=103;f.Value=l=>"{0}:{1}";
                    row.SetData(d,OutgameTaskType.Achievement,null);row.Refresh();Require(row.Title.text=="CommanderName.103:10"&&!row.Liveness.transform.parent.gameObject.activeSelf&&row.Title.rectTransform.sizeDelta==new Vector2(530,57)&&row.Fill.transform.parent.GetComponent<RectTransform>().sizeDelta==new Vector2(530,26),"commander description and achievement dimensions");
                    row.SetData(d,OutgameTaskType.DailyTask,null);d.contentType=f.Config.statisticEventConfig.UseProp;d.ContentArgument=2345;row.Refresh();Require(row.Title.text=="Commander.SkillName.345:10"&&!row.Liveness.transform.parent.gameObject.activeSelf,"reuse does not reset achievement layout; item skill modulo key");
                    d.contentType=123;f.Value=l=>"{1}";string old=row.Title.text;row.Refresh();Require(f.Warnings.Count==1&&row.Title.text==old&&row.Target.text=="4/10","ordinary format failure warns but continues reward/progress rendering");
                    d.contentType=f.Config.statisticEventConfig.UseProp;f.Value=l=>"{2}";Throws<FormatException>(row.Refresh);
                }
            });
            check("sprite-reentry-count-snapshot-and-published-data",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();var d=Data();row.SetData(d,0,null);f.SpriteCallback=()=>{d.rewards[0].datas[1]=99;d.prog=7;};row.Refresh();
                    Require(f.IconName=="Task_gold"&&f.AtlasName=="TaskUI"&&f.NativeSize&&row.Icon.transform.GetChild(0).GetComponent<Text>().text=="5"&&row.Target.text=="7/10","reward count captured before sprite callback, progress reads live data afterwards");
                    var next=Data();row.ReplaceData(next);f.SpriteCallback=()=>throw new InvalidOperationException("sprite");Throws<InvalidOperationException>(row.Refresh);Require(row.Data==next&&row.Target.text=="7/10","published data and title/liveness survive sprite failure before progress");
                }
            });
            check("random-reward-mutation-before-callback-and-live-report-after-reentry",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();var d=Data();d.rewards[0].datas[0]=0;bool claimed=false;var next=Data();next.id=22;next.rewards[0].datas[1]=7;
                    row.SetData(d,0,(id,uid)=>{Require(d.rewards[0].datas[0]==1001&&d.rewards[0].datas[1]==80&&id==17&&uid==88,"random substitution/max precedes callback");claimed=true;row.ReplaceData(next);row.ActionCache=true;});
                    row.Claim.onClick.Invoke();Require(d.rewards[0].datas[0]==0&&f.Fx.Amount==5,"random display effect uses old count before claim mutation");f.Completion();
                    Require(claimed&&row.Lifetime.GameObject.activeSelf&&f.Reported.Last().SequenceEqual(new[]{"DailyTask","22","1001","7"}),"report rereads callback-rebound Data while retaining captured reward id");
                }
            });
            check("effect-award-and-claim-failure-prefixes",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();row.SetData(Data(),0,null);f.Fx.Call=()=>throw new InvalidOperationException("effect");Throws<InvalidOperationException>(()=>row.Claim.onClick.Invoke());Require(!row.Claim.enabled&&f.Motion.Count==0,"effect throw leaves button disabled before slide setup");
                    row.Data.rewards[0].datas[0]=2001;f.Award=(id,n)=>throw new InvalidOperationException("award");row.Claim.onClick.Invoke();Require(f.Errors.Count==1&&f.Motion.Count==1,"award popup exception logged and slide still starts");
                    row.ClaimedCallback=(id,uid)=>throw new InvalidOperationException("claim");Throws<InvalidOperationException>(f.Completion);Require(row.Claim.enabled&&row.Lifetime.GameObject.activeSelf&&f.Reported.Count==0,"completion re-enable precedes callback failure; hide/report not reached");
                }
            });
            check("daily-filter-per-row-and-retained-hidden-row-state",()=>{
                using(var f=new Fixture()){
                    int calls=0;f.DailyServices.GameValue=(key,args)=>{Require(key==900001&&args.Length==0,"source query key/shape");calls++;return 0;};
                    var filtered=f.Daily.Filter(f.Tasks.Child.ModuleData.tasks);Require(calls==8&&filtered.Count==7&&!filtered.Any(t=>t.id==8),"live query per task and only id8 filtered");
                    var row=f.Daily.Rows[1];var task=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);task.state=1;f.Daily.RefreshRows();Require(!row.Lifetime.GameObject.activeSelf&&!row.ActionCache,"claimed row hides");
                    task.state=0;task.conditions[0].value=0x100000002L;f.Daily.RefreshRows();Require(row.Data.prog==1&&!row.Lifetime.GameObject.activeSelf,"Int64 truncates then Refresh clamps; reused row is not automatically reactivated");
                    foreach(var t in f.Tasks.Child.ModuleData.tasks)t.state=1;f.Daily.RefreshRows();Require(f.Empty,"no unclaimed filtered rows shows empty state");
                }
            });
            check("daily-projection-first-condition-and-retained-value-on-empty",()=>{
                using(var f=new Fixture()){
                    var row=f.Daily.Rows[1];var task=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);var cfg=task.Config;cfg.conditionParams[0].datas=new[]{71,9,15};task.conditions[0].value=0x100000002L;
                    f.Daily.RefreshRows();Require(row.Data.number==9&&row.Data.contentType==71&&row.Data.ContentArgument==0&&row.Data.prog==2,"source projection uses element1 as target even with filter arg; low32 progress only");
                    task.conditions.Clear();f.Daily.RefreshRows();Require(row.Data.prog==2,"no conditions retains prior row progress");
                }
            });
            check("daily-countdown-validation-order-and-listener-removal",()=>{
                using(var f=new Fixture()){
                    f.Daily.RootUI=null;f.Daily.RefreshCountdown(null);f.Daily.RootUI=f;
                    Throws<NullReferenceException>(()=>f.Daily.RefreshCountdown(null));Throws<InvalidCastException>(()=>f.Daily.RefreshCountdown(new object[]{7,1}));
                    f.Daily.RefreshCountdown(new object[]{130001,-1500L});Require(f.Countdown=="seconds:-1","signed milliseconds truncate toward zero");
                    f.Daily.OnDestroy();f.Countdown=null;f.Tasks.Runtime.Statistics.Common.SendMessage("CommonModule_ResetTimeRefresh",new object[]{130001,5000L});Require(f.SingletonClear==1&&f.Countdown==null,"singleton clear precedes removal of own listener");
                }
            });
            check("claim-save-and-independent-restart",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleTaskRow-"+Guid.NewGuid().ToString("N"));long uid;
                using(var f=new Fixture(false,path)){var row=f.Ready();uid=row.Data.Uid;row.Claim.onClick.Invoke();f.Completion();f.Tasks.Manager.OnSave();}
                using(var f=new Fixture(false,path)){var task=f.Tasks.Child.ModuleData.tasks.Single(t=>t.id==1);Require(task.uid==uid&&task.state==1&&f.Daily.Liveness==20&&!f.Daily.Rows.ContainsKey(1),"saved claimed record omitted from recreated row view");f.Daily.Claim(1,uid);Require(f.Tasks.Items.Host.Adds==0,"restart cannot reaward claimed task");}
            });
            check("disposal-retains-data-callback-and-source-destruction-order",()=>{
                using(var f=new Fixture()){
                    var row=f.GetTaskItem();var d=Data();Action<int,long> callback=(i,u)=>{};row.SetData(d,0,callback);var rect=row.Lifetime.RectTransform;row.Dispose();
                    Require(row.Lifetime.IsDisposed&&row.Lifetime.GameObject==null&&ReferenceEquals(row.Lifetime.RectTransform,rect)&&row.Data==d&&row.ClaimedCallback==callback,"BaseItem Dispose retains row fields/delegate and rect");
                    f.Services.Destroy=go=>throw new InvalidOperationException("destroy");var second=f.GetTaskItem();Throws<InvalidOperationException>(second.Dispose);Require(!second.Lifetime.IsDisposed&&second.Lifetime.GameObject,"destruction failure prevents owner clearing");
                }
            });
            return report;
        }
    }
}
