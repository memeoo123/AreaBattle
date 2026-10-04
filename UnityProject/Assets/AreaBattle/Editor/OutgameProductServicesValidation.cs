using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using AreaBattle.SharedItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameProductServicesValidation
    {
        sealed class Host:IOutgameGlobalItemLifecycleHost
        {
            public Action Tick;public int Registrations;
            public int AddUpdate(Action callback){Tick=callback;Registrations++;return 0;}
            public void RemoveUpdate(int id){Tick=null;}
            public void DisposeItemConfiguration(){}
            public void ClearGlobalInstance(){}
            public void SendStatisticsRegistration(string name,int id,Func<object[],long> query){}
        }
        sealed class RewardHost:IOutgameGlobalItemRewardHost
        {
            public DateTime Now=new DateTime(2026,10,3,12,0,0);public int Updates;
            public void Update(){Updates++;}
            public DateTime GetNowDateTime()=>Now;
            public IOutgameItemEntity GetItem(GameItemConfig config)=>throw new NotSupportedException();
            public void AddRewards(List<OutgameItemReward> rows)=>throw new NotSupportedException();
            public void ExpendReward(List<OutgameItemReward> rows)=>throw new NotSupportedException();
        }
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        public static BattleBuild.Report Run()
        {
            var result=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();result.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){result.passed=false;result.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("product-provider-shared-row-defaults-and-current-slot",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});var provider=new OutgameProductConfigProvider(()=>slot.Instance);
                var config=new GameProductConfig{id=1,getType=1,priceType=0,priceCalParam=new[]{1001,17},
                    costItemPriceTypes=new[]{0},costItemPriceParams=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,9}}}};
                slot.Instance.Products[1]=config;
                var held=provider.Price(1);var prices=new OutgameProductPrices(provider.Price,Mathf.Pow,(i,e)=>{throw e;});
                var row=new OutgameProductUserData{productId=1};prices.Update(row);
                Require(config.buyTypeOrder.Length==1&&config.buyTypeOrder[0]==1,"default must write to source shared row");
                Require(row.consumeItemPrice==17&&row.consumeItemPriceArray[0]==9,"real shared nested parameters");
                config.priceCalParam=new[]{1001,23};Require(held.priceCalParam[1]==23,"held wrapper retains live source object");
                slot.Instance.Dispose();slot.Instance.Products[1]=new GameProductConfig{id=1,buyLimit=4,buyLimitParam=8};
                Require(provider.Refresh(1).buyLimit==4&&provider.Refresh(1).buyLimitParam==8,"new call resolves replacement singleton");
                Require(held.priceCalParam[1]==23,"old reference remains original row");
            });
            check("product-provider-malformed-nested-price-catches-and-continues",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});var errors=new List<Exception>();
                slot.Instance.Products[1]=new GameProductConfig{id=1,buyTypeOrder=new[]{1,1},costItemPriceTypes=new[]{0,0},
                    costItemPriceParams=new List<ListArrayInt>{null,new ListArrayInt{datas=new[]{1001,42}}},
                    getType=1,priceType=0,priceCalParam=new[]{1001,7}};
                var provider=new OutgameProductConfigProvider(()=>slot.Instance);
                var row=new OutgameProductUserData{productId=1};
                new OutgameProductPrices(provider.Price,Mathf.Pow,(id,e)=>errors.Add(e)).Update(row);
                Require(errors.Count==1&&errors[0] is NullReferenceException,"nested error stays in original per-entry catch");
                Require(row.consumeItemPriceArray[0]==0&&row.consumeItemPriceArray[1]==42&&row.consumeItemPrice==7,"later prices still execute");
            });
            check("product-services-original-config-daily-reset-message-order-and-restart",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});
                slot.Instance.InitLegacyUnityJson(new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);}));
                var host=new Host();var rewardHost=new RewardHost();var bus=new OutgameMessageDispatcher();
                var errors=new List<object[]>();var events=new List<string>();float delta=1;
                var services=new OutgameProductServices(host,()=>slot.Instance,()=>rewardHost.Now,()=>delta,Mathf.Pow,()=>bus,a=>{},errors.Add);
                var global=services.Lifecycle;var config=slot.Instance.Products[100101];
                var saved=new OutgameItemManagerData{lastExitDayOfYear=rewardHost.Now.DayOfYear-1};
                saved.productUserDatas.Add(new OutgameProductUserData{m_uniqueId=77,productId=100101,haveBuyTimes=2,buyCount=0,consumeItemPrice=99});
                global.InitializeWithHost(saved.SerializeRecord(),rewardHost);
                bus.AddListener("ItemUI_RefreshStore",args=>{
                    var row=global.Indexes.Products[77];Require(row.buyCount==2&&row.haveBuyTimes==0&&row.consumeItemPrice==0,"reset completes before UI");
                    Require((int)args[0]==77&&global.Indexes.IsDirty,"refresh uses UID and dirty state");events.Add("refresh");});
                bus.AddListener("ItemUI_ProductReset",args=>{Require((int)args[0]==77&&(int)args[1]==100101,"reset UID and config id");events.Add("reset");});
                host.Tick();Require(events.Count==0&&rewardHost.Updates==1,"strict one-second threshold, host every frame");
                delta=.01f;host.Tick();
                Require(string.Join(",",events)=="refresh,reset"&&rewardHost.Updates==2,"source reset notifications before host update");
                Require(config.buyTypeOrder[0]==1&&errors.Count==1,"original missing shared price arrays logged; no invented price");
                Require(global.Indexes.ProductSnapshot[77].buyCount==0,"source snapshot uses config id, not differing UID");
                var restored=OutgameItemManagerData.ReadOriginal(global.Save());
                Require(restored.productUserDatas[0].buyCount==2&&restored.productUserDatas[0].haveBuyTimes==0,"save uses live reset row");
                global.InitializeWithHost(restored.SerializeRecord(),rewardHost);delta=1.1f;host.Tick();
                Require(host.Registrations==1&&events.Count==2,"restart same day neither repeats reset nor registers duplicate handle");
            });
            check("product-services-interval-reset-prices-and-notification-failure-prefix",()=>{
                var slot=new OutgameItemConfigSlot(a=>{});var config=new GameProductConfig{id=7,buyLimit=3,buyLimitParam=5,refreshPeriod=new[]{10},
                    buyTypeOrder=new[]{1},costItemPriceTypes=new[]{4},costItemPriceParams=new List<ListArrayInt>{new ListArrayInt{datas=new[]{1001,3,2}}},
                    getType=1,priceType=4,priceCalParam=new[]{1001,4,2}};slot.Instance.Products[7]=config;
                var host=new Host();var reward=new RewardHost();var bus=new OutgameMessageDispatcher();
                var services=new OutgameProductServices(host,()=>slot.Instance,()=>reward.Now,()=>1.01f,Mathf.Pow,()=>bus,a=>{},a=>{throw new Exception("unexpected price error");});
                var saved=new OutgameItemManagerData{lastExitDayOfYear=reward.Now.DayOfYear};
                saved.productUserDatas.Add(new OutgameProductUserData{m_uniqueId=7,productId=7,haveBuyTimes=9,buyCount=0,consumeItemPriceArray=new long[]{99}});
                services.Lifecycle.InitializeWithHost(saved.SerializeRecord(),reward);
                int secondEvent=0;bus.AddListener("ItemUI_RefreshStore",a=>{throw new InvalidOperationException("listener failure");});
                bus.AddListener("ItemUI_ProductReset",a=>secondEvent++);
                try{host.Tick();throw new Exception("expected listener failure");}catch(InvalidOperationException){}
                var row=services.Lifecycle.Indexes.Products[7];
                Require(row.buyCount==5&&row.haveBuyTimes==0&&row.consumeItemPrice==4&&row.consumeItemPriceArray[0]==3,"reset prices use cleared counts");
                Require(row.nextRefreshTimeStamp==OutgameItemTimestamp.FromDateTime(reward.Now)+10000,"interval timestamp written before reset");
                Require(secondEvent==0&&reward.Updates==0&&services.Lifecycle.Indexes.ProductSnapshot[7].buyCount==5,"failure retains prefix and skips host update");
            });
            return result;
        }
        public static void Validate()
        {
            var report=Run();
            foreach(var suite in new Func<BattleBuild.Report>[] {OutgameLoginSyncValidation.Run,OutgameItemConfigValidation.Run,
                OutgameGlobalItemSlotValidation.Run,OutgameGlobalItemRewardsValidation.Run,OutgameItemModuleValidation.Run,
                OutgameVirtualItemsValidation.Run,OutgamePackageItemsValidation.Run})
            {var previous=suite();report.checks.AddRange(previous.checks);report.passed&=previous.passed;}
            report.limitations="Product composition and existing item/storage regressions. Full Main/account/menu, native frame loop and Player remain separate acceptance work.";
            Directory.CreateDirectory(Path.Combine(BattleBuild.Workspace,"analysis"));
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/product-services-validation.json"),JsonUtility.ToJson(report,true));
            Debug.Log("PRODUCT_SERVICES_"+(report.passed?"PASS":"FAIL")+" checks="+report.checks.Count);
            if(Application.isBatchMode)EditorApplication.Exit(report.passed?0:1);
        }
    }
}
