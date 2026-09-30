using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.SharedItemConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameGlobalItemSlotValidation
    {
        sealed class Fixture
        {
            public readonly OutgameItemConfigSlot Config=new OutgameItemConfigSlot(a=>{});
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly OutgameGlobalItemRewardsValidation.Reports Reports=new OutgameGlobalItemRewardsValidation.Reports();
            public readonly List<string> Trace=new List<string>();
            public readonly OutgameGlobalItemSlot Slot;
            public readonly OutgameItemEntityServices Services=new OutgameItemEntityServices();
            public Action Loading,Removing,Constructing,Tick;
            public int Loads,Constructs;public Action Update;
            public Fixture()
            {
                Slot=new OutgameGlobalItemSlot(h=>{Constructs++;Constructing?.Invoke();return new OutgameGlobalItemLifecycle(h,()=>Tick?.Invoke(),id=>Config.Instance.Products.ContainsKey(id),id=>Config.Instance.Items.ContainsKey(id),()=>new DateTime(2026,9,30));},
                    ()=>Config.Instance,()=>new OutgameLegacyConfigRead(n=>{Loads++;Trace.Add(n);Loading?.Invoke();return Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n);},s=>{}),
                    ()=>Messages,()=>Reports,s=>Trace.Add("error:"+s),a=>{Update=a;Trace.Add("update+");return 7;},id=>{Removing?.Invoke();Trace.Add("update-:"+id);});
                Slot.BindEntities(Services);
            }
        }
        sealed class Backend:IOutgameStorageBackend
        {
            public readonly Dictionary<string,string> Data=new Dictionary<string,string>();public int Writes;
            public string Get(string k)=>Data.TryGetValue(k,out var v)?v:null;
            public void Set(string k,string v,Action<string> f,Action c){Data[k]=v;Writes++;c();}
            public void Remove(string k,Action<string> f,Action c){Data.Remove(k);c();}
            public void Clear(Action<string> f,Action c){Data.Clear();c();}
        }
        sealed class StorageHost:IOutgameDataStorageHost
        {
            public bool Server;public int SourceLoginProgress=>10; // Test-only successful-login precondition.
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>Server;
            public string MineGameName=>"global-item-slot-fixture";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;public void QueueUpload(string k,string s){}
        }
        static OutgameItemManager Manager(Fixture f,Backend backend,StorageHost host,Action<string> download)
        {
            var storage=new OutgameDataManagerStorage(()=>"ItemManager",host,new OutgameSdkStringStorage(backend,s=>{}),new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{}));
            // This owner/startup probe uses model-only rewards; Tool effects are covered by the module chain suite.
            return new OutgameItemManager(storage,host,download,()=>f.Slot.Instance.Lifecycle,id=>f.Slot.CreateFactory(id,f.Services),()=>throw new Exception("Unexpected Tool access"),id=>6,()=>new DateTime(2026,9,30));
        }
        static void Require(bool b,string s){if(!b)throw new Exception(s);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-global-slot-config-getter-empty-global-getter-initializes",()=>{
                var f=new Fixture();Require(f.Config.Instance.Items.Count==0&&!f.Slot.HasInstance,"config getter alone must not load");var owner=f.Slot.Instance;
                Require(f.Loads==4&&f.Constructs==1&&f.Config.Instance.Items.Count==79&&f.Config.Instance.Packages.Count==4&&f.Config.Instance.Products.Count==16&&f.Config.Instance.Rewards.Count==10,"real four-table init from global getter");
                Require(ReferenceEquals(owner,f.Slot.Instance)&&f.Loads==4&&owner.Lifecycle.Data==null&&owner.Lifecycle.RewardHost==null&&!owner.Lifecycle.UpdateHandle.HasValue,"getter never initializes record or registers update");
                Require(owner.Lifecycle.Indexes.Items.Count==0&&owner.Lifecycle.Indexes.Products.Count==0&&owner.Lifecycle.Indexes.ItemSnapshot==null&&owner.Lifecycle.Indexes.ProductSnapshot==null&&!owner.Lifecycle.Indexes.IsDirty&&owner.Lifecycle.StatisticsEventId==10020,"ctor only live dictionaries and statistics id");
            });
            check("source-global-slot-published-before-config-reentrant-access",()=>{
                var f=new Fixture();OutgameGlobalItemInstance seen=null;f.Loading=()=>{Require(f.Slot.HasInstance,"published before resource read");var current=f.Slot.Instance;if(seen==null)seen=current;Require(ReferenceEquals(seen,current)&&current.Lifecycle.Data==null,"reentrant getter uses existing uninitialized owner");};
                Require(ReferenceEquals(f.Slot.Instance,seen)&&f.Constructs==1&&f.Loads==4,"no recursive initialization");
            });
            check("source-global-slot-config-failure-retains-partial-owner-without-retry",()=>{
                var f=new Fixture();f.Loading=()=>{if(f.Loads==2)throw new InvalidOperationException();};Throws<InvalidOperationException>(()=>{var o=f.Slot.Instance;});
                Require(f.Slot.HasInstance&&f.Config.Instance.Items.Count==79&&f.Config.Instance.Packages.Count==0&&f.Config.Instance.PackageRewards==null,"original prefix and published instance survive");
                f.Loading=null;var held=f.Slot.Instance;Require(f.Loads==2&&held.Lifecycle.Data==null,"nonnull slot does not retry failed config init");held.Lifecycle.Release();var fresh=f.Slot.Instance;
                Require(!ReferenceEquals(held,fresh)&&f.Constructs==2&&f.Loads==6&&f.Config.Instance.RewardItems.Count>0,"release permits fresh construction and init");
            });
            check("source-global-slot-constructor-failure-never-publishes",()=>{
                var f=new Fixture();f.Constructing=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>{var o=f.Slot.Instance;});Require(!f.Slot.HasInstance&&f.Loads==0,"constructor failure before publication");f.Constructing=null;Require(f.Slot.Instance!=null&&f.Constructs==2&&f.Loads==4,"next getter can retry construction");
            });
            check("source-global-slot-rereads-null-after-release-during-config",()=>{
                var f=new Fixture();OutgameItemConfigManager oldConfig=null;f.Loading=()=>{f.Loading=null;oldConfig=f.Config.Instance;f.Slot.Instance.Lifecycle.Release();};
                Require(f.Slot.Instance==null&&!f.Slot.HasInstance&&!f.Config.HasInstance&&oldConfig.Items.Count==79,"getter returns current null, captured old config still finishes init");
                Require(f.Slot.Instance!=null&&f.Loads==8&&f.Constructs==2&&!ReferenceEquals(oldConfig,f.Config.Instance),"subsequent getter constructs new config and global");
            });
            check("source-global-slot-rereads-replacement-after-reentrant-release",()=>{
                var f=new Fixture();OutgameGlobalItemInstance old=null,replacement=null;f.Loading=()=>{f.Loading=null;old=f.Slot.Instance;old.Lifecycle.Release();replacement=f.Slot.Instance;};
                Require(ReferenceEquals(f.Slot.Instance,replacement)&&!ReferenceEquals(old,replacement)&&f.Loads==8&&f.Constructs==2,"outer getter returns replacement published by callback");
                old.Lifecycle.Release();Require(!f.Slot.HasInstance&&!f.Config.HasInstance,"stale original release clears current replacement too");
            });
            check("source-global-slot-release-failure-prefix-and-current-config",()=>{
                var f=new Fixture();var owner=f.Slot.Instance;owner.Lifecycle.Initialize("",null);f.Removing=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(owner.Lifecycle.Release);
                Require(owner.Lifecycle.UpdateHandle==7&&f.Slot.HasInstance&&f.Config.HasInstance,"remove failure preserves handle and slots");f.Removing=null;var old=f.Config.Instance;old.Dispose();var current=f.Config.Instance;current.RewardRanges=null;
                Throws<NullReferenceException>(owner.Lifecycle.Release);Require(!owner.Lifecycle.UpdateHandle.HasValue&&f.Slot.HasInstance&&f.Config.HasInstance,"current config failure follows successful handle clear, before slot clear");
                current.RewardRanges=new Dictionary<int,List<OutgameRewardRangeData>>();owner.Lifecycle.Release();Require(!f.Slot.HasInstance&&!f.Config.HasInstance&&old.Items.Count==79,"release resolves current config and leaves old retained tables");
            });
            check("source-global-slot-preinit-reward-retains-mutation-before-null-snapshot",()=>{
                var f=new Fixture();var owner=f.Slot.Instance;int statistics=0;f.Messages.AddListener("CommonGameModule_StatisticsEventSet",a=>statistics++);
                Throws<NullReferenceException>(()=>owner.Rewards.AddRewardsModel(new List<OutgameItemReward>{new OutgameItemReward{itemId=1001,itemCount=3}}));
                Require(owner.Lifecycle.GetItemCount(1001)==3&&statistics==1&&!owner.Lifecycle.Indexes.IsDirty&&f.Reports.Changes.Count==0,"source constructor snapshot remains null; failure after live insert/statistics");
            });
            check("source-global-slot-manager-startup-statistics-persistence-restart",()=>{
                var f=new Fixture();var backend=new Backend();var host=new StorageHost();var manager=Manager(f,backend,host,s=>throw new Exception());int registrations=0;
                f.Messages.AddListener(OutgameGlobalItemLifecycle.StatisticsRegistration,a=>{registrations++;Require((int)a[0]==10020&&a[1] is Func<object[],long>,"actual source message payload");Require(f.Slot.Instance.Lifecycle.UpdateHandle==7&&f.Slot.Instance.Lifecycle.Indexes.ItemSnapshot!=null&&ReferenceEquals(f.Slot.Instance.Lifecycle.RewardHost,manager),"record, host, update, snapshots before statistics");});
                Require(!f.Slot.HasInstance,"manager constructor stays lazy");manager.OnInit();var held=f.Slot.Instance;
                manager.GetItem(f.Config.Instance.Items[1001]).AddItemOnlyModel(4294967298L);manager.OnSave();Require(registrations==1&&backend.Writes==1&&manager.GetItemNum(1001)==4294967298L,"startup and actual model reward save");
                manager.OnRelease();Require(!f.Slot.HasInstance&&!f.Config.HasInstance,"manager release uses owner slot");manager.UpdateData(false);Require(!ReferenceEquals(held,f.Slot.Instance)&&manager.GetItemNum(1001)==4294967298L&&registrations==2,"same manager reconstructs owner and restores original Int64 record");
                var restarted=new Fixture();var next=Manager(restarted,backend,host,s=>throw new Exception());next.OnInit();Require(next.GetItemNum(1001)==4294967298L&&restarted.Loads==4,"independent startup from isolated saved backend");
            });
            check("source-global-slot-manager-download-waits-for-real-callback",()=>{
                var f=new Fixture();var host=new StorageHost{Server=true};int downloads=0;var manager=Manager(f,new Backend(),host,s=>{Require(s=="ItemManager","download key");downloads++;});
                manager.OnInit();Require(downloads==1&&!f.Slot.HasInstance&&!f.Config.HasInstance,"server request must not fake data callback or force global init");
                manager.UpdateDataCallBack("{\"itemUserDatas\":[{\"itemId\":1001,\"itemCount\":5},{\"itemId\":-1,\"itemCount\":8}],\"productUserDatas\":[]}");
                Require(manager.GetItemNum(1001)==5&&manager.GetItemNum(-1)==0&&f.Loads==4,"callback initializes config before held-record filtering");
            });
            return r;
        }
    }
}
