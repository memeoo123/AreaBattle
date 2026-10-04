using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameEffectControlValidation
    {
        public sealed class PoolHost:IOutgameSingleSpawnPoolFactory,IOutgameNormalPoolLifetime,IOutgameNormalPoolResources
        {
            public readonly Dictionary<string,OutgameObjectPool> Pools=new Dictionary<string,OutgameObjectPool>();
            public readonly List<OutgameAssetHandle> Handles=new List<OutgameAssetHandle>();public readonly List<GameObject> Icons=new List<GameObject>();
            public readonly List<string> Trace=new List<string>();public bool Modern=true,FailCreate,FailDestroy;public int Unloads,Created,Loaded;public bool Native;
            public bool UseNewResourceLoader=>Modern;
            public OutgameObjectPool CreateSingleSpawnObjectPool(string name){Trace.Add("create:"+name);Created++;if(FailCreate)throw new InvalidOperationException("create");var p=new OutgameObjectPool(name,false,int.MaxValue,float.MaxValue,0);Pools.Add(name,p);return p;}
            public void DestroyRegisteredPoolIfManagerExists(string name){Trace.Add("destroy:"+name);if(FailDestroy)throw new InvalidOperationException("destroy");if(Pools.TryGetValue(name,out var p)){Pools.Remove(name);p.Shutdown();}}
            public void DestroyRoot(GameObject root){Trace.Add("root");Destroy(root);}
            public void ReleaseHandle(IOutgamePoolAssetHandle handle){Trace.Add("release");((OutgameAssetHandle)handle).Release();}
            public void UnloadUnusedAssets(){Trace.Add("unload");Unloads++;}
            public void UnloadUnusedBundle(IOutgamePoolResourceInfo info)=>Trace.Add("legacy-unload");
            public void LoadOriginal(string path,Action<IOutgamePoolAssetHandle> complete)
            {
                Loaded++;var provider=new OutgameAssetProvider(()=>false,Debug.LogWarning){AssetObject=OutgameFlyCurrencyAssets.Load(path),Status=4};var handle=provider.CreateHandle(path,()=>false);Handles.Add(handle);complete(handle);
            }
            public void LoadResource(string path,Action<IOutgamePoolResourceInfo> complete)=>throw new InvalidOperationException("Legacy resource provider not selected");
            public void LoadSynchronousPrefab(string path,Action<GameObject> complete){Loaded++;var go=UnityEngine.Object.Instantiate(OutgameFlyCurrencyAssets.Load(path));Icons.Add(go);complete(go);}
            public string GetIntactBundleName(string asset,string bundle)=>bundle+asset;
            public void ReleaseUIForm(GameObject go){Trace.Add("release-object");Destroy(go);}
            public void Warning(string message)=>Trace.Add(message);
            void Destroy(GameObject go){if(Native)UnityEngine.Object.Destroy(go);else UnityEngine.Object.DestroyImmediate(go);}
            public void Cleanup(){foreach(var p in Pools.Values)p.Shutdown();Pools.Clear();foreach(var h in Handles)h.Release();foreach(var go in Icons)if(go)Destroy(go);}
        }
        sealed class Effects:IOutgameToolEffects
        {
            public int Saves;public bool Fail;
            public void RefreshTopInfo(){}public void GoldSpent(int i,long n){}public void ToolChanged(int i){}public void UnlockScene(int i){}public void UnlockSoldier(int i){}
            public bool ApplyItemEntity(int i,long n)=>false;public void MissingItemEntity(int i){}public void ReportGet(int i,int k,int n,int b,string s){}public void ReportCost(int i,int k,int n,int b){}
            public void Save(){Saves++;if(Fail)throw new InvalidOperationException("save");}
        }
        public sealed class Fixture:IDisposable
        {
            public readonly PoolHost Host=new PoolHost();public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly List<string> Trace=new List<string>();public readonly List<IEnumerator> Routines=new List<IEnumerator>();
            public readonly GameObject Root=new GameObject("effect-control-fixture");public readonly GameObject TopRoot;
            public readonly Text Gold,Diamonds;public readonly OutgameLocalInventoryState State=new OutgameLocalInventoryState();
            readonly Effects effects=new Effects();public readonly OutgameToolDispatcher Tools;public readonly OutgameEffectControlServices Services;public readonly OutgameEffectControl Control;
            public readonly OutgameLogicModule Logic;public readonly OutgameFrameEntry Frame;public readonly OutgameObjectPoolManager Manager;public float Now;
            public int Saves=>effects.Saves;public bool FailSave{set=>effects.Fail=value;}
            public Fixture(bool native=false,bool frameManager=false)
            {
                Host.Native=native;TopRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"),Root.transform,false);TopRoot.SetActive(true);
                Gold=TopRoot.transform.Find("objTopInfo/goldInfo/txt_goldNum").GetComponent<Text>();Diamonds=TopRoot.transform.Find("objTopInfo/diamondInfo/txt_DiamondNum").GetComponent<Text>();Gold.text=Diamonds.text="0";
                var inventory=new OutgameLocalInventory(State,"{\"Datas\":[]}");Tools=new OutgameToolDispatcher(inventory,effects,"{\"Datas\":[]}","{\"Datas\":[]}","{\"Datas\":[]}");
                Services=new OutgameEffectControlServices{PoolManager=()=>Host,Resources=Host,PoolLifetime=Host,Tools=()=>Tools,InventoryCount=inventory.Count,
                    GoldText=()=>{Trace.Add("gold");return Gold;},DiamondsText=()=>{Trace.Add("diamonds");return Diamonds;},UiNode=n=>{Trace.Add(n);return Root.transform;},Voice=(kind,id)=>Trace.Add("voice:"+kind+":"+id),DiamondVoice=()=>2099,
                    CreateTargetTween=t=>new object(),PlayTargetTween=t=>{},Move=(t,p,d,e,id,u,a)=>{},KillTween=(id,c)=>Trace.Add("kill:"+id),Scatter=new OutgameFlyScatter((min,max)=>min,Mathf.Sin,Mathf.Cos)};
                if(!native){Services.StartCoroutine=r=>{Trace.Add("start");Routines.Add(r);return r;};Services.StopCoroutine=r=>Trace.Add("stop");Services.Time=()=>Now;}
                else Services.BindNativeTweens(Root.AddComponent<OutgameFlyTweenRunner>());
                if(frameManager){
                    Frame=new OutgameFrameEntry(new OutgameFrameServices{CreateModule=type=>type==typeof(OutgameObjectPoolManager)?(IOutgameFrameModule)new OutgameObjectPoolManager():type==typeof(OutgameLogicModule)?Logic:throw new InvalidOperationException(type.Name),Log=s=>Trace.Add(s),Warning=s=>Trace.Add(s),SetCulture=()=>{},ShutdownInput=()=>{},ClearPermissions=()=>{},SetConfigReadInitialized=b=>{}});
                    Manager=Frame.GetModule<OutgameObjectPoolManager>();Frame.Initialize(Manager,null);
                    OutgameNormalPoolLifetime.Bind(Services,Frame,Host,Host.UnloadUnusedAssets,Host.UnloadUnusedBundle,native?(Action<GameObject>)UnityEngine.Object.Destroy:UnityEngine.Object.DestroyImmediate);
                }
                OutgameCoreControllerBindings.BindEffects(Registry,Services);Control=(OutgameEffectControl)Registry.Resolve(4058);
                Logic=new OutgameLogicModule(b=>{},()=>{},()=>{},s=>{},s=>{});Logic.Initialize();if(frameManager)Frame.GetModule<OutgameLogicModule>();Logic.RegisterLogicCtr(Control,true);if(frameManager)Frame.Start();
            }
            public void Dispose(){Host.FailDestroy=false;if(Frame!=null)Frame.Shutdown();Host.Cleanup();if(Host.Native)UnityEngine.Object.Destroy(Root);else UnityEngine.Object.DestroyImmediate(Root);}
        }
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original EffectControl and sequence/NormalPool creation with actual existing pool/fly/account services. Pool manager acquisition, resource host and presentation/report remain explicit; complete Main/platform/full roster/Player pending."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="effect-control-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="effect-control-"+id,result="fail",detail=e.ToString()});}};
            check("registry-init-original-pool-options-and-reinit-retains-owner",()=>{using(var f=new Fixture()){var p=f.Control.Pool;Require(p.Name=="moneyPool"&&p.Root==null&&!p.CacheResources&&p.Pool.Capacity==300&&p.Pool.ExpireTime==5&&p.Pool.AutoReleaseInterval==5&&p.Pool.Priority==0,"source options");f.Control.OnInit();Require(ReferenceEquals(p,f.Control.Pool)&&f.Host.Created==1&&f.Control.ActiveEffectCount==0,"reinit same pool");}});
            check("ui-target-source-lazy-cache-and-dispose-retained-text",()=>{using(var f=new Fixture()){var messages=new OutgameMessageDispatcher();var top=OutgameTopInfoPage.FromOriginal(f.TopRoot);var ui=new OutgameUiControl(()=>null,f.Registry,()=>messages,new OutgameUiControlGlobals(),null,()=>top,()=>null,()=>null,(a,b)=>{});Require(ui.GoldText==null&&ui.DiamondsText==null,"no top page means missing target");ui.ShowTopInfoUI();Require(ReferenceEquals(ui.GoldText,f.Gold)&&ReferenceEquals(ui.DiamondsText,f.Diamonds),"original field outlets");f.Services.BindUi(()=>ui,n=>f.Root.transform);ui.OnDispose();Require(ReferenceEquals(f.Services.GoldText(),f.Gold)&&ReferenceEquals(f.Services.DiamondsText(),f.Diamonds),"source retains Unity-live text after TopInfo cleared");UnityEngine.Object.DestroyImmediate(f.Gold.gameObject);Require(f.Services.GoldText()==null,"destroyed cached Unity text remains native-null without owner");}});
            check("normal-pool-manager-absence-and-creation-failure-field-order",()=>{var h=new PoolHost();var p=new OutgameNormalPool(h);p.CreateObjectPool(()=>null,"absent",3,4,null,true);Require(p.Name==null&&p.Pool==null&&!p.CacheResources,"absent manager untouched");h.FailCreate=true;Throws<InvalidOperationException>(()=>p.CreateObjectPool(()=>h,"failed",3,4,null,true));Require(p.Name=="failed"&&p.CacheResources&&p.Pool==null,"fields published before factory failure");});
            check("normal-pool-setter-failure-retains-partial-created-pool",()=>{var h=new PoolHost();var p=new OutgameNormalPool(h);Throws<InvalidOperationException>(()=>p.CreateObjectPool(()=>h,"negative",-1,7,null,false));Require(p.Pool==h.Pools["negative"]&&p.Pool.AutoReleaseInterval==7&&p.Pool.ExpireTime==float.MaxValue,"interval set before capacity failure");h.Cleanup();});
            check("audio-before-null-target-no-id-root-or-coroutine",()=>{using(var f=new Fixture()){f.Services.GoldText=()=>{f.Trace.Add("gold");return null;};int id=f.Control.FlyMoney(4,null,Vector3.zero,true,null,true);Require(id==-1&&f.Trace.SequenceEqual(new[]{"voice:1:2019","gold"})&&f.Control.ActiveEffectCount==0&&f.Saves==0,"null target returns after audio/read");}});
            check("shared-id-fallback-root-economic-save-before-registration",()=>{using(var f=new Fixture()){int id=f.Control.FlyMoney(50,null,new Vector3(1,2,3),true,null,true);Require(id>=0&&f.State.goldNum==50&&f.Saves==1&&f.Control.ActiveEffectCount==1&&f.Trace.SequenceEqual(new[]{"voice:1:2019","gold","UIMessage","start"}),"actual award and original fallback");}});
            check("save-failure-preserves-economic-write-without-coroutine",()=>{using(var f=new Fixture()){f.FailSave=true;Throws<InvalidOperationException>(()=>f.Control.FlyMoney(8,null,Vector3.zero,true,null,true));Require(f.State.goldNum==8&&f.Saves==1&&f.Control.ActiveEffectCount==0&&f.Routines.Count==0,"no invented transaction/continuation");}});
            check("direct-fly-toolchange-preserves-source-local-and-global-record-split",()=>{using(var account=new OutgameAccountRewardsValidation.Fixture())using(var f=new Fixture()){f.Services.Tools=()=>account.Account.Dispatcher;f.Services.InventoryCount=id=>account.Account.Inventory.Count(id);f.Control.FlyMoney(7,null,Vector3.zero,true,null,true);Require(account.Account.Local.GoldNum==7&&account.Global.GetItemCount(1001)==0,"direct ToolChange modifies legacy local record only");Require(OutgameLocalRecord.Read(account.Stored("LocalDataManager")).goldNum==7,"real shared pool save precedes animation");}});
            check("logic-update-delayed-cleanup-and-additive-check-time",()=>{using(var f=new Fixture()){int id=f.Control.FlyMoney(1,null,Vector3.zero,false,null,true);f.Now=5;f.Logic.Update(0,0);Require(f.Control.PendingCleanupCount==1&&f.Control.ActiveEffectCount==1&&f.Control.NextCleanupCheck==6,"timeout queues for next frame");f.Logic.Update(0,0);Require(f.Control.ActiveEffectCount==0&&f.Control.PendingCleanupCount==0&&f.Trace.Contains("stop"),"logic native ownership drains pending");int next=f.Control.FlyMoney(1,null,Vector3.zero,false,null,true);f.Now=7;f.Logic.Update(0,0);Require(f.Control.NextCleanupCheck==14,"source additive time, not now+1");f.Control.StopEffect(next);f.Logic.Update(0,0);}});
            check("reinit-retains-pending-and-clock-then-source-key-failure",()=>{using(var f=new Fixture()){f.Control.FlyMoney(1,null,Vector3.zero,false,null,true);f.Now=5;f.Control.Updata(0,0);f.Control.OnInit();Require(f.Control.ActiveEffectCount==0&&f.Control.PendingCleanupCount==1&&f.Control.NextCleanupCheck==6,"pending/check retained");Throws<KeyNotFoundException>(()=>f.Control.Updata(0,0));Require(f.Control.PendingCleanupCount==1,"failure precedes pending clear");}});
            check("dispose-failure-retains-registry-success-retains-fields",()=>{using(var f=new Fixture()){f.Control.FlyMoney(1,null,Vector3.zero,false,null,true);var p=f.Control.Pool;f.Host.FailDestroy=true;Throws<InvalidOperationException>(()=>f.Control.OnDispose());Require(f.Registry.HasInstance(4058),"failed destruction stops clear");f.Host.FailDestroy=false;f.Control.OnDispose();Require(!f.Registry.HasInstance(4058)&&ReferenceEquals(p,f.Control.Pool)&&f.Control.ActiveEffectCount==1&&f.Host.Unloads==1,"no invented coroutine/pool reset");f.Control.OnDispose();Require(f.Host.Unloads==2,"repeat source disposal");}});
            check("empty-sequence-completion-and-required-position-nullable",()=>{using(var f=new Fixture()){int done=0;f.Control.PlaySequenceEffect(new int[0],null,()=>done++,null);Require(done==1,"empty completes without counts/callback");Throws<InvalidOperationException>(()=>f.Control.PlaySequenceEffect(new[]{1001},new[]{1},null,null));Require(f.Routines.Count==0,"null position callback throws before fly");}});
            check("sequence-supported-order-single-step-and-shared-array-reread",()=>{using(var f=new Fixture()){int done=0;var seen=new List<int>();var ids=new[]{1001,1002};var counts=new[]{2,3};var s=new OutgameSequenceEffect(()=>f.Control,f.Tools.GoodsType,ids,counts,()=>done++,i=>{seen.Add(i);return new Vector3(i,0,0);});s.Play();Require(s.Index==1&&f.Routines.Count==1&&done==0&&f.State.goldNum==0,"first animation no economic mutation");s.Play();Require(s.Index==2&&f.Routines.Count==2&&f.Trace.Contains("voice:1:2099"),"second diamonds");s.Play();Require(s.Index==0&&done==1&&seen.SequenceEqual(new[]{0,1}),"reset before complete");ids[0]=1005;s.Play();Require(s.Index==1&&f.Routines.Count==2&&done==1,"unsupported goods stop with index advanced");}});
            check("sequence-callback-rereads-id-before-dispatch-and-failure-before-index",()=>{using(var f=new Fixture()){var ids=new[]{1001};var s=new OutgameSequenceEffect(()=>f.Control,f.Tools.GoodsType,ids,new[]{4},null,i=>{ids[i]=1002;return Vector3.one;});s.Play();Require(f.Trace.Contains("diamonds")&&!f.Trace.Contains("gold"),"array reread after callback");var bad=new OutgameSequenceEffect(()=>f.Control,f.Tools.GoodsType,new[]{1001},new int[0],null,i=>Vector3.zero);Throws<IndexOutOfRangeException>(bad.Play);Require(bad.Index==0,"count failure before increment");}});
            check("uncached-modern-pool-clone-return-reuse-and-source-handle-retention",()=>{using(var f=new Fixture()){GameObject a=null,b=null;f.Control.Spawn("model/entity/golditem",go=>a=go);Require(a&&a.name=="goldItem"&&f.Host.Loaded==1,"real original prefab clone");f.Control.Unspawn(a);f.Control.Spawn("model/entity/golditem",go=>b=go);Require(ReferenceEquals(a,b)&&f.Host.Loaded==1,"same pool reuse");f.Control.DestoryPool();Require(!a&&f.Host.Handles[0].MainObject!=null,"uncached source handle not in normal pool release dictionary");}});
            check("uncached-legacy-prefab-route-and-clone-name-retained",()=>{using(var f=new Fixture()){f.Host.Modern=false;GameObject go=null;f.Control.Spawn("model/entity/diamonditem",r0=>go=r0);Require(go&&go.name=="diamondItem(Clone)"&&f.Host.Handles.Count==0,"source legacy route no modern rename");f.Control.Unspawn(go);f.Control.DestoryPool();Require(!go&&f.Host.Unloads==0,"legacy no modern unused call");}});
            return r;
        }
    }
}
