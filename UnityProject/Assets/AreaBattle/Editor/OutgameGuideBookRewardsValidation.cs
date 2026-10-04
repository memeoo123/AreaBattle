using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameGuideBookRewardsValidation
    {
        sealed class Host:IOutgameDataStorageHost
        {
            public int SourceLoginProgress=>10; // Explicit test account endpoint.
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>false;
            public string MineGameName=>"Proj_hdzd";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){throw new Exception(s);}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;
            public void QueueUpload(string k,string s){throw new Exception("Unexpected upload");}
        }
        public sealed class Effects:IOutgameToolEffects,IOutgameShopCurrencyEffects
        {
            public readonly List<string> Trace=new List<string>();public OutgameDataManagerPool Pool;
            public Action FlyCallback,ChangeCallback;public bool FailFly,FailSave;
            public Transform LastRoot;public Vector3 LastPosition;public int ParticleCount;
            public void RefreshTopInfo()=>Trace.Add("top");public void GoldSpent(int id,long n)=>Trace.Add("spent");
            public void ToolChanged(int id){Trace.Add("changed:"+id);ChangeCallback?.Invoke();}
            public void UnlockScene(int id){throw new Exception("Unexpected scene");}public void UnlockSoldier(int id){throw new Exception("Unexpected soldier");}
            public bool ApplyItemEntity(int id,long n){throw new Exception("Unexpected item");}public void MissingItemEntity(int id){throw new Exception("Unexpected item");}
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>Trace.Add("get:"+reason);
            public void ReportCost(int id,int category,int amount,int balance)=>Trace.Add("cost");
            public void Save(){Trace.Add("save");if(FailSave)throw new IOException("Explicit save failure");Pool.SaveData();}
            public void FlyMoney(int n,Transform root,Vector3 position,bool applyInventory,Action completed,bool display)=>Fly(1001,n,root,position,applyInventory,completed,display);
            public void FlyDiamonds(int n,Transform root,Vector3 position,bool applyInventory,Action completed,bool display)=>Fly(1002,n,root,position,applyInventory,completed,display);
            void Fly(int id,int n,Transform root,Vector3 position,bool applyInventory,Action completed,bool display)
            {
                Require(!applyInventory&&completed==null&&display,"source presentation must not apply reward twice");
                LastRoot=root;LastPosition=position;ParticleCount=n;Trace.Add("fly:"+id+":"+n);
                if(FailFly)throw new InvalidOperationException("Explicit effect failure");FlyCallback?.Invoke();
            }
        }
        public sealed class Fixture:IDisposable
        {
            public readonly string Path;public readonly OutgameFileStorageBackend Backend;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly OutgameProfile Profile=new OutgameProfile();public readonly OutgameLocalInventory Inventory;
            public readonly OutgameDataManagerPool Pool;public readonly OutgameLocalDataManager Local;
            public readonly OutgameGuideBookControl Control;public readonly OutgameGuideBookManager Manager;
            public readonly OutgameToolControl Tools;
            public readonly OutgameLegacyConfigManager Config;public readonly Effects Effects=new Effects();
            public readonly OutgameGuideBookRewardBinding Binding;public readonly GameObject Container;public Action<int> RefreshItem;
            public Button Button=>Binding.BookRewardButton.GetComponent<Button>();
            public Fixture(string path=null)
            {
                Path=path??System.IO.Path.Combine(System.IO.Path.GetTempPath(),"AreaBattleBookRewards-"+Guid.NewGuid().ToString("N"));
                var host=new Host();Backend=new OutgameFileStorageBackend(Path,a=>a());
                var storage=new OutgameSdkStringStorage(Backend,s=>{throw new Exception(s);});
                var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{});
                var messages=Messages;var registry=new OutgameControllerRegistry();
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});Effects.Pool=Pool;
                var book=new OutgameGuideBookRuntime(host,storage,versions,s=>{throw new Exception("Unexpected download");});
                Local=new OutgameLocalDataManager(Profile,BattleView.ReadText("Data/AllSkillConfig"),BattleView.ReadText("Data/Outgame/GameProductConfig"),()=>DateTime.Now,
                    new OutgameDataManagerStorage(()=>"LocalDataManager",host,storage,versions),host,s=>{throw new Exception();},(m,s)=>{});
                Pool.OnInit(true,"Proj_hdzd",new[]{new OutgameManagerRegistration(4119,"Proj_hdzd",true,false,()=>Local),book.Registration});
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});
                reader.ReadTable(Config.dicGuide);reader.ReadTable(Config.dicGuidebook);reader.ReadTable(Config.dicGuideTips);
                book.BindController(registry,()=>Pool,()=>Config,()=>1000,()=>messages);Control=(OutgameGuideBookControl)registry.Resolve(4076);Control.OnInit();Manager=Control.Manager;
                Inventory=new OutgameLocalInventory(Profile.inventory,BattleView.ReadText("Data/AllSkillConfig"));
                var dispatcher=new OutgameToolDispatcher(Inventory,Effects,BattleView.ReadText("Data/Outgame/GameItemConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/SkinConfig"));
                var tools=new OutgameToolControl(()=>dispatcher,()=>Inventory,id=>throw new Exception(),()=>"unused");Tools=tools;
                Container=new GameObject("isolated-ui-root");var canvas=new GameObject("Canvas",typeof(RectTransform));canvas.transform.SetParent(Container.transform,false);
                var layer=new GameObject("Normal",typeof(RectTransform));layer.transform.SetParent(canvas.transform,false);
                var prefab=Resources.Load<GameObject>("Recovered/GuideBook/GuideBookUI");Require(prefab,"source page imported");
                var root=UnityEngine.Object.Instantiate(prefab,layer.transform,false);root.SetActive(true);Binding=root.AddComponent<OutgameGuideBookRewardBinding>();
                Binding.Bind(()=>Control,()=>tools,()=>Effects,id=>Effects.Trace.Add("voice:"+id),index=>{Effects.Trace.Add("refresh:"+index);RefreshItem?.Invoke(index);},()=>Messages);
                messages.AddListener("GetGuidBookReward",a=>Effects.Trace.Add("claim"));
            }
            public void Dispose()=>UnityEngine.Object.DestroyImmediate(Container);
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static GuidebookConfig Book(int id,int currency,int amount)=>new GuidebookConfig{id=id,reward=new[]{currency,amount}};
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Actual imported UGUI button plus recovered ToolControl/LocalData/GuideBook disk records. Effects/audio/report endpoints are observed test sinks; full list/tab/popup/Spine/Main and native-frame acceptance remain pending."};
            Action<string,Action> check=(id,run)=>{try{run();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("guide-book-native-button-original-reward-order-and-real-record-restart",()=>{
                using(var f=new Fixture()){
                    f.Binding.SelectedBook=f.Config.dicGuidebook[1];int held=f.Inventory.Count(1001);
                    f.Effects.FlyCallback=()=>{Require(f.Inventory.Count(1001)==held+50&&!f.Manager.ContainsGuide(1),"inventory before effect, claim after effect");
                        using(var before=new Fixture(f.Path))Require(before.Inventory.Count(1001)==held+50&&!before.Manager.ContainsGuide(1),"source economic save precedes claim save on actual disk");};
                    f.Button.onClick.Invoke();
                    Require(string.Join(",",f.Effects.Trace)=="voice:2001,changed:1001,get:guidebook,save,fly:1001:50,claim,refresh:0","exact source call order: "+string.Join(",",f.Effects.Trace));
                    Require(f.Manager.ContainsGuide(1)&&!f.Button.gameObject.activeSelf&&ReferenceEquals(f.Effects.LastRoot,f.Binding.transform),"claim, UI hiding and animation root");
                    f.Effects.Trace.Clear();f.Button.onClick.Invoke();Require(string.Join(",",f.Effects.Trace)=="voice:2001"&&f.Inventory.Count(1001)==held+50,"duplicate click plays voice only");
                    using(var next=new Fixture(f.Path))Require(next.Manager.ContainsGuide(1)&&next.Inventory.Count(1001)==held+50,"independent actual LocalData and GuideBook records restore");
                    Require(f.Binding.transform.Find("guideSV/Viewport").GetComponent<RectMask2D>()&&f.Binding.transform.Find("tipSV/Viewport").GetComponent<RectMask2D>(),"native source viewport masks imported");
                }
            });
            check("guide-book-reward-effect-clamp-preserves-economic-amount-and-position",()=>{
                using(var f=new Fixture()){
                    f.Binding.SelectedBook=Book(701,1002,250);f.Binding.BookRewardButton.position=new Vector3(4,5,6);int held=f.Inventory.Count(1002);f.Button.onClick.Invoke();
                    Require(f.Inventory.Count(1002)==held+250&&f.Effects.ParticleCount==100&&f.Effects.LastPosition==new Vector3(4,5,6),"diamonds clamp only particles");
                    f.Binding.SelectedBook=Book(702,1001,2);f.Button.onClick.Invoke();Require(f.Effects.ParticleCount==10,"minimum particle count");
                }
            });
            check("guide-book-tip-original-reward-no-page-voice-and-parent-traversal",()=>{
                using(var f=new Fixture()){
                    var tip=f.Config.dicGuideTips[1];int held=f.Inventory.Count(1001);var position=new Vector3(2,3,4);
                    f.Binding.Rewards.ClaimTip(tip,position);
                    Require(f.Inventory.Count(1001)==held+50&&f.Manager.ContainsTip(1)&&f.Effects.LastPosition==position,"tip original reward and source passed origin");
                    Require(string.Join(",",f.Effects.Trace)=="changed:1001,get:guidetip,save,fly:1001:50,claim","tip page has no extra click sound or list-item refresh");
                    f.Effects.Trace.Clear();f.Binding.Rewards.ClaimTip(tip,position);f.Binding.Rewards.ClaimTip(null,position);Require(f.Effects.Trace.Count==0,"null/already claimed tip returns");
                    f.Binding.transform.SetParent(null);try{Throws<NullReferenceException>(()=>f.Binding.Rewards.ClaimTip(f.Config.dicGuideTips[2],position));
                        Require(f.Inventory.Count(1001)==held+100&&!f.Manager.ContainsTip(2),"discarded parent traversal still fails after reward before claim");}
                    finally{f.Binding.transform.SetParent(f.Container.transform);}
                }
            });
            check("guide-book-source-ignores-rejected-toolchange-and-does-not-recheck-unlock",()=>{
                using(var f=new Fixture()){
                    f.Profile.inventory.goldNum=0;f.Binding.SelectedBook=Book(888,1001,-3);f.Button.onClick.Invoke();
                    Require(f.Inventory.Count(1001)==0&&f.Manager.ContainsGuide(888)&&f.Effects.ParticleCount==10,"false debit result still followed by effect and claim for unknown config");
                    f.Effects.Trace.Clear();f.Binding.SelectedBook=null;f.Button.onClick.Invoke();Require(string.Join(",",f.Effects.Trace)=="voice:2001","null book guard after audio");
                }
            });
            check("guide-book-source-save-and-effect-failure-keep-earlier-economic-change",()=>{
                using(var f=new Fixture()){
                    int held=f.Inventory.Count(1001);f.Binding.SelectedBook=Book(991,1001,7);f.Effects.FailSave=true;
                    Throws<IOException>(()=>f.Binding.Rewards.ClaimBook());Require(f.Inventory.Count(1001)==held+7&&!f.Manager.ContainsGuide(991)&&!f.Effects.Trace.Exists(x=>x.StartsWith("fly:")),"save exception prevents later effects/claim without reverting economic mutation");
                }
                using(var f=new Fixture()){
                    int held=f.Inventory.Count(1001);f.Binding.SelectedBook=Book(992,1001,7);f.Effects.FailFly=true;
                    Throws<InvalidOperationException>(()=>f.Binding.Rewards.ClaimBook());
                    using(var next=new Fixture(f.Path))Require(next.Inventory.Count(1001)==held+7&&!next.Manager.ContainsGuide(992),"effect failure leaves saved currency before unsaved claim, matching source");
                }
            });
            check("guide-book-reward-rereads-selection-after-external-callbacks",()=>{
                using(var f=new Fixture()){
                    int held=f.Inventory.Count(1001);f.Binding.SelectedBook=Book(801,1001,3);f.Binding.SelectedIndex=5;
                    f.Effects.FlyCallback=()=>{f.Binding.SelectedBook=Book(802,1002,99);f.Binding.SelectedIndex=9;};
                    f.Button.onClick.Invoke();Require(f.Inventory.Count(1001)==held+3&&!f.Manager.ContainsGuide(801)&&f.Manager.ContainsGuide(802)&&f.Effects.Trace.Contains("refresh:9"),"source field reread after effect callback");
                }
            });
            check("guide-book-noncurrency-reward-skips-effects-and-red-dots-follow-current-claims",()=>{
                using(var f=new Fixture()){
                    int held=f.Inventory.Count(1005);f.Binding.SelectedBook=Book(900,1005,2);f.Button.onClick.Invoke();
                    Require(f.Inventory.Count(1005)==held+2&&f.Manager.ContainsGuide(900)&&!f.Effects.Trace.Exists(x=>x.StartsWith("fly:")),"noncurrency dispatch with no visual currency effect");
                    foreach(var row in f.Config.dicGuidebook.Values)f.Manager.GetGuideReward(row.id);
                    foreach(var row in f.Config.dicGuideTips.Values)f.Manager.GetTipReward(row.id);
                    f.Binding.RefreshRedDots();
                    foreach(string path in new[]{"tabBtnG/btnBookG0/imgRedG0","tabBtnG/btnBookG1/imgRedG1","tabBtnT/btnBookT0/imgRedT0","tabBtnT/btnBookT1/imgRedT1"})
                        Require(!f.Binding.transform.Find("tabBtnGroup/"+path).gameObject.activeSelf,"all source red dots clear after all original claims");
                }
            });
            return r;
        }
        public static void Validate()
        {var r=Run();File.WriteAllText(System.IO.Path.Combine(BattleBuild.Workspace,"analysis/guide-book-rewards-validation.json"),JsonUtility.ToJson(r,true));Debug.Log("GUIDE_BOOK_REWARDS_"+(r.passed?"PASS":"FAIL")+" checks="+r.checks.Count);if(Application.isBatchMode)EditorApplication.Exit(r.passed?0:1);}
    }
}
