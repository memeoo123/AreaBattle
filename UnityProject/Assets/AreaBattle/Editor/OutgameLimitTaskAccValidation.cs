using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskAccValidation
    {
        public sealed class Effects:IOutgameToolEffects,IOutgameShopCurrencyEffects
        {
            public readonly List<string> Trace=new List<string>();public Action Completion,DuringFly;public bool FailFly;public int Amount;public Transform Root;public Vector3 Position;
            public void RefreshTopInfo()=>Trace.Add("top");public void GoldSpent(int id,long n)=>Trace.Add("spent");public void ToolChanged(int id)=>Trace.Add("change:"+id);
            public void UnlockScene(int id)=>Trace.Add("scene:"+id);public void UnlockSoldier(int id)=>Trace.Add("soldier:"+id);
            public bool ApplyItemEntity(int id,long n){Trace.Add("entity:"+id+":"+n);return true;}public void MissingItemEntity(int id)=>Trace.Add("missing:"+id);
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>Trace.Add("get:"+id+":"+amount+":"+reason);public void ReportCost(int id,int category,int amount,int balance)=>Trace.Add("cost");public void Save()=>Trace.Add("save");
            public void FlyMoney(int n,Transform root,Vector3 pos,bool apply,Action done,bool display)=>Fly(1001,n,root,pos,apply,done,display);
            public void FlyDiamonds(int n,Transform root,Vector3 pos,bool apply,Action done,bool display)=>Fly(1002,n,root,pos,apply,done,display);
            void Fly(int id,int n,Transform root,Vector3 pos,bool apply,Action done,bool display)
            {Require(!apply&&display&&done!=null,"source currency effect flags and completion");Amount=n;Root=root;Position=pos;Completion=done;Trace.Add("fly:"+id+":"+n);DuringFly?.Invoke();if(FailFly)throw new InvalidOperationException("effect failure");}
        }
        public sealed class Fixture:IDisposable,IOutgameLimitTaskAccPage
        {
            public readonly OutgameLimitTaskRowsValidation.Fixture Rows;
            public readonly Effects EffectsHost=new Effects();public readonly OutgameLocalInventory Inventory;
            public readonly OutgameLimitTaskAccItemServices Services;public readonly OutgameLimitTaskAccItem Item;
            public readonly GameObject ItemRoot;public int PreviewCount,CompletionMessages;public OutgameNoviceAccRewardItemData Previewed;public Transform Origin;public bool HasPage=true;
            public Action PreviewCallback;public Transform Transform=>Rows.Days.Page.transform;
            public OutgameChildLimitTimeTaskActivity Child=>Rows.Child;
            public Fixture(bool native=false,string path=null)
            {
                Rows=new OutgameLimitTaskRowsValidation.Fixture(native,path);Inventory=new OutgameLocalInventory(new OutgameProfile().inventory,BattleView.ReadText("Data/AllSkillConfig"));
                var dispatcher=new OutgameToolDispatcher(Inventory,EffectsHost,BattleView.ReadText("Data/Outgame/GameItemConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/SkinConfig"));
                var tools=new OutgameToolControl(()=>dispatcher,()=>Inventory,id=>0,()=>"fixture-purchase");
                Services=new OutgameLimitTaskAccItemServices{ActivityConfig=()=>Rows.Days.Tasks.Config,Config=()=>Rows.Config,Page=()=>HasPage?this:null,Effects=()=>EffectsHost,Tools=()=>tools,GoodsType=dispatcher.GoodsType,
                    CoinCost=()=>"fixture-CoinCost",ActivityReason=()=>"fixture-Activity",ShowSkinReward=args=>EffectsHost.Trace.Add("skin-ui:"+args[0]),SetSprite=(image,icon,atlas,size)=>EffectsHost.Trace.Add("sprite:"+icon+":"+atlas),Messages=()=>Rows.Days.Messages,Destroy=Rows.Services.Destroy};
                ItemRoot=UnityEngine.Object.Instantiate(Rows.Days.Page.transform.Find("LimitTimeTaskAccItem").gameObject,Rows.Days.Page.transform,false);ItemRoot.SetActive(true);Item=new OutgameLimitTaskAccItem(ItemRoot,Services);
                Item.SetData(Child.AccRewards[0],Child);Rows.Days.Messages.AddListener("SevendayGetAccReward",args=>CompletionMessages++);
            }
            public void PreviewAccReward(Transform origin,OutgameNoviceAccRewardItemData data){PreviewCount++;Previewed=data;Origin=origin;PreviewCallback?.Invoke();}
            public void Ready(){foreach(var list in Child.DayTasks.Values)foreach(var task in list)task.state=1;Item.Refresh();Require(Child.ModuleData.ext.AccProgress>=Item.Data.Config.accValue,"original claimed task rewards satisfy selected threshold");}
            public void Click()=>OutgameUiPointerClick.Get(ItemRoot).OnPointerClick(null);
            public void Dispose()=>Rows.Dispose();
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original accumulator item with actual activity reward engine and legacy ToolControl; currency/skin preview effects are observed required hosts. Preview async/native checks separate; full page/Main/menu/platform/Player/audiovisual acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="limit-task-acc-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="limit-task-acc-"+id,result="fail",detail=e.ToString()});}};
            check("original-positions-last-id-criterion-and-exact-state-red",()=>{
                using(var f=new Fixture()){
                    Require(!f.Item.IsLast&&f.Item.Count.text=="80"&&f.Item.BoxLocked.rectTransform.anchoredPosition==new Vector2(0,65)&&f.Item.Count.rectTransform.anchoredPosition==new Vector2(0,30),"odd original box layout");
                    f.Item.SetData(f.Child.AccRewards[1],f.Child);Require(f.Item.BoxLocked.rectTransform.anchoredPosition==new Vector2(0,-65)&&f.Item.Count.rectTransform.anchoredPosition==new Vector2(0,-100),"even original box layout");
                    f.Item.SetData(f.Child.AccRewards[7],f.Child);Require(f.Item.IsLast&&f.Item.RewardContent.activeSelf&&!f.Item.BoxContent.activeSelf&&f.EffectsHost.Trace.Exists(s=>s.StartsWith("sprite:qibing19:")),"id equals global accumulator-list count selects final skin icon branch");
                    f.Rows.Days.Tasks.Config.NoviceAccList.RemoveAt(0);f.Item.Refresh();Require(!f.Item.IsLast,"criterion is global list count, not last reward position");
                    f.Item.SetData(f.Child.AccRewards[0],f.Child);f.Ready();f.Item.Data.state=2;f.Item.Refresh();Require(!f.Item.BoxRed.gameObject.activeSelf&&!f.Item.BoxLocked.gameObject.activeSelf&&!f.Item.BoxUnlocked.gameObject.activeSelf,"state2 is neither unclaimed nor claimed for source UI");
                }
            });
            check("unready-preview-after-callback-and-missing-page",()=>{
                using(var f=new Fixture()){
                    var other=f.Child.AccRewards[1];f.Item.OnClick=item=>item.SetData(other,f.Child);f.Click();
                    Require(f.PreviewCount==1&&ReferenceEquals(f.Previewed,other)&&f.Origin==f.Item.BoxLocked.transform&&other.state==0,"callback runs before live preview eligibility/data/origin");
                    f.HasPage=false;f.Click();Require(f.PreviewCount==1,"missing page skips preview without award");
                    f.Item.Data.state=2;f.HasPage=true;f.Click();Require(f.PreviewCount==1,"nonzero unclaimed-like state does not enter preview");
                }
            });
            check("actual-gold-award-zero-extra-toolchange-and-delayed-completion",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.EffectsHost.Trace.Clear();long before=f.Rows.Days.Tasks.Items.Global.GetItemCount(1001);long oldEnergy=f.Inventory.Count(1005);
                    f.EffectsHost.DuringFly=()=>Require(f.Item.Data.state==1&&f.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==before+50,"actual award and claim precede display effect");f.Click();
                    Require(f.EffectsHost.Amount==50&&f.EffectsHost.Root==f.Transform&&f.EffectsHost.Position==f.Item.BoxLocked.transform.position&&f.Inventory.Count(1005)==oldEnergy,"source effect origin and zero delta for secondary item");
                    Require(string.Join(",",f.EffectsHost.Trace)=="fly:1001:50,change:1005,save"&&f.Item.BoxUnlocked.gameObject.activeSelf&&!f.Item.BoxLocked.gameObject.activeSelf&&f.CompletionMessages==0,"extra legacy ToolChange runs before final box visuals; callback deferred");
                    f.EffectsHost.Completion();Require(f.CompletionMessages==1,"currency completion sends original red-refresh event");f.Click();Require(f.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==before+50,"repeat claim no-op");
                }
            });
            check("diamond-low-int32-display-count-preserves-economic-long",()=>{
                using(var f=new Fixture()){
                    f.Item.Data.RewardsData.Clear();f.Item.Data.RewardsData.Add(new OutgameItemReward{itemId=1002,itemCount=(1L<<32)+7});f.Ready();f.Click();
                    Require(f.Rows.Days.Tasks.Items.Global.GetItemCount(1002)==(1L<<32)+7&&f.EffectsHost.Amount==7&&f.EffectsHost.Trace.Contains("fly:1002:7"),"source visual lowInt32 count without economic truncation");
                }
            });
            check("missing-page-still-awards-and-processes-secondary-items",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.HasPage=false;f.EffectsHost.Trace.Clear();f.Click();Require(f.Item.Data.state==1&&string.Join(",",f.EffectsHost.Trace)=="change:1005,save"&&f.Item.BoxUnlocked.gameObject.activeSelf,"page absence skips first display only");
                }
            });
            check("noncurrency-paramint-legacy-unlock-and-skin-popup",()=>{
                using(var f=new Fixture()){
                    f.Item.SetData(f.Child.AccRewards[1],f.Child);f.Ready();f.EffectsHost.Trace.Clear();f.Click();
                    Require(f.Item.Data.state==1&&f.EffectsHost.Trace.Contains("scene:10")&&f.EffectsHost.Trace.Contains("get:10:1:fixture-CoinCost")&&f.EffectsHost.Trace.Contains("skin-ui:10"),"first noncurrency uses original paramInt, delta1, CoinCost reason and SkinRewardUI argument");
                    Require(f.EffectsHost.Trace.IndexOf("skin-ui:10")<f.EffectsHost.Trace.LastIndexOf("top")&&f.Inventory.Count(1001)==0,"secondary gold ToolChange delta0 follows popup without adding50 again");
                }
            });
            check("effect-failure-preserves-claim-and-stops-later-visuals",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.EffectsHost.FailFly=true;f.EffectsHost.Trace.Clear();Throws<InvalidOperationException>(f.Click);
                    Require(f.Item.Data.state==1&&f.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==50&&f.Item.BoxLocked.gameObject.activeSelf&&!f.EffectsHost.Trace.Contains("change:1005"),"effect exception after award before secondary processing/visual flip");
                }
            });
            check("report-callback-changes-current-display-reward-after-award",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.Rows.Days.Tasks.ReportHost.Sending=(kind,report)=>{if(kind==OutgameLimitTaskReportKind.Reward)f.Item.Data.RewardsData[0]=new OutgameItemReward{itemId=1002,itemCount=3};};f.Click();
                    Require(f.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==50&&f.Rows.Days.Tasks.Items.Global.GetItemCount(1002)==0&&f.EffectsHost.Trace.Contains("fly:1002:3"),"effect rereads changed current rewards after actual original award");
                }
            });
            return r;
        }
    }
}
