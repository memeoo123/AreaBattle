using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameAiNamesValidation
    {
        sealed class MinimumRandom:System.Random
        {
            public readonly List<string> Calls=new List<string>();
            public override int Next(int min,int max){Calls.Add(min+":"+max);if(min>max)throw new ArgumentOutOfRangeException();return min;}
        }
        sealed class StorageHost:IOutgameDataStorageHost
        {
            public bool Server;public int SourceLoginProgress=>10; // Test-only established account state.
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>Server;
            public string MineGameName=>"Proj_hdzd";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){throw new Exception(s);}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;
            public void QueueUpload(string k,string s){throw new Exception("Unexpected test upload");}
        }
        static OutgameLegacyConfigManager Config(bool original=false)
        {
            var c=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
            if(original){var r=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});r.ReadTable(c.dicAIName);r.ReadTable(c.dicCountryConfig);}
            return c;
        }
        static OutgameLegacyConfigManager Small()
        {
            var c=Config();foreach(int id in new[]{1,2,10,11})c.dicCountryConfig[id]=new CountryConfigConfig{id=id,countryFlagName="c"+id};
            c.dicAIName[1]=new AINameConfig{id=1,zh_cn="A"};c.dicAIName[2]=new AINameConfig{id=2,zh_cn="B"};c.dicAIName[3]=new AINameConfig{id=3,zh_cn="excluded"};return c;
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("source-randoms-exclusive-range-distinct-selection-and-error-order",()=>{
                var rng=new MinimumRandom();var random=new GameRandomSource(rng);var errors=new List<object[]>();
                Require(string.Join(",",random.DistinctRange(3,2,5,errors.Add))=="2,3,4"&&string.Join(",",rng.Calls)=="0:3,0:2,0:1","source half-open candidates and shrinking random index bounds");
                Require(random.DistinctRange(4,2,5,errors.Add)==null&&Equals(errors[0][0],"随机次数[4]大于区间值[3]"),"count overflow precedes selection");
                Require(random.DistinctRange(0,5,4,errors.Add)==null&&Equals(errors[1][0],"随机次数[0]大于区间值[-1]"),"source count check precedes reversed-range check");
                Require(random.DistinctRange(-2,5,4,errors.Add)==null&&Equals(errors[2][0],"随机区间值错误[4]小于[5]"),"negative count can reach reversed-range diagnostic");
                Require(random.DistinctRange(-1,2,5,errors.Add).Length==0&&rng.Calls.Count==3,"valid negative draw count performs no RNG calls");
            });
            check("source-ai-names-remap-exhaustion-group-order-and-insertion",()=>{
                var c=Small();var unityBounds=new List<int>();var rng=new MinimumRandom();
                var names=new OutgameAiNames(()=>c,a=>{throw new Exception("unexpected log");},(min,max)=>{Require(min==0,"unity lower bound");unityBounds.Add(max);return 0;},new GameRandomSource(rng));
                Require(string.Join(",",names.Generate(4,true))=="c11;B,c11;A,c2;B,c2;A","country1/10 remaps, source semicolon, grouping and no insertion at tail");
                Require(string.Join(",",unityBounds)=="4,4,4,3,2,2","saturated selected keys removed individually before retry");
                Require(string.Join(",",rng.Calls)=="0:2,0:1,0:1,0:2,0:1,0:2,0:3","managed sample/insertion sequence distinct from Unity country stream");
            });
            check("source-ai-names-unprefixed-still-looks-up-country-and-captured-name",()=>{
                var c=Small();c.dicCountryConfig.Remove(2);c.dicAIName[1]=null;var rng=new MinimumRandom();
                var names=new OutgameAiNames(()=>c,a=>{},(min,max)=>0,new GameRandomSource(rng));
                Throws<KeyNotFoundException>(()=>names.Generate(1,false));Require(rng.Calls.Count==1,"country lookup after name lookup, before dereference even without prefix");
                c.dicCountryConfig[2]=new CountryConfigConfig{id=2};Throws<NullReferenceException>(()=>names.Generate(1,false));
            });
            check("source-ai-names-captures-tables-but-rereads-third-config-count",()=>{
                var first=Small();var second=Config();var third=Small();second.dicAIName[1]=new AINameConfig{id=1,zh_cn="captured"};
                var rng=new MinimumRandom();int reads=0;
                var names=new OutgameAiNames(()=>{reads++;return reads==1?first:reads==2?second:third;},a=>{},(min,max)=>0,new GameRandomSource(rng));
                Require(names.Generate(1,false)[0]=="captured"&&reads==3&&rng.Calls[0]=="0:2","first country table, second name table, third count determines range");
            });
            check("source-ai-names-zero-count-still-resolves-config-and-exhaustion-fails",()=>{
                var c=Small();int reads=0,calls=0;var rng=new MinimumRandom();
                var names=new OutgameAiNames(()=>{reads++;return c;},a=>{},(min,max)=>{calls++;return 0;},new GameRandomSource(rng));
                Require(names.Generate(0,true).Count==0&&reads==3&&calls==0&&rng.Calls.Count==0,"zero count does config work but no RNG");
                Throws<ArgumentOutOfRangeException>(()=>names.Generate(5,false));Require(rng.Calls.Count==0,"country capacity exhausted before managed name draws");
                Throws<NullReferenceException>(()=>new OutgameAiNames(()=>null,a=>{}).Generate(0,false));
            });
            check("source-ai-names-original-tables-exclude-final-name-row",()=>{
                var c=Config(true);Require(c.dicAIName.Count==98&&c.dicCountryConfig.Count==23,"original table sizes");
                var names=new OutgameAiNames(()=>c,a=>{throw new Exception("unexpected log");},(min,max)=>0,new GameRandomSource(new MinimumRandom()));
                var rows=names.Generate(97,false);var seen=new HashSet<string>(rows);
                Require(rows.Count==97&&seen.Contains(c.dicAIName[1].zh_cn)&&!seen.Contains(c.dicAIName[98].zh_cn),"source [1,98) sampling with one country at capacity");
                Require(rows[0]==c.dicAIName[97].zh_cn&&rows[96]==c.dicAIName[1].zh_cn,"fixed-minimum insertion reverses generated order");
            });
            check("source-user-info-runtime-original-names-registration-items-and-disk-restart",()=>{
                var randomState=UnityEngine.Random.state;
                try{
                    var c=Config(true);var host=new StorageHost();var messages=new OutgameMessageDispatcher();var registry=new OutgameControllerRegistry();var items=new OutgameVirtualItemServices();
                    string path=Path.Combine(Path.GetTempPath(),"AreaBattleNamedUser-"+Guid.NewGuid().ToString("N"));
                    var pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});
                    var backend=new OutgameFileStorageBackend(path,a=>a());
                    var versions=new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{});
                    var runtime=new OutgameUserInfoRuntime(()=>pool,()=>c,host,new OutgameSdkStringStorage(backend,s=>{throw new Exception(s);}),versions,
                        s=>{throw new Exception("Unexpected download");},()=>456,()=>false,()=>messages,s=>{},a=>{throw new Exception("Unexpected random log");});
                    runtime.BindController(registry,items);Throws<NullReferenceException>(()=>items.HeadBoxes());
                    pool.OnInit(true,"Proj_hdzd",new[]{runtime.Registration});var control=(OutgameUserInfoControl)registry.Resolve(4228);control.OnInit();
                    var manager=control.Manager;bool valid=false;for(int id=1;id<98;id++)valid|=c.dicAIName[id].zh_cn==manager.Name;
                    Require(valid&&manager.InstallTime==456&&manager.ParticipatesInSync&&!manager.CompressData,"real name provider and source registration defaults");
                    Require(ReferenceEquals(items.HeadBoxes(),manager.HeadBoxes),"entity services follow actual controller manager");
                    items.HeadBoxes().UnlockHeadBox(8);pool.SaveData();string name=manager.Name;
                    var nextPool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});
                    var nextRuntime=new OutgameUserInfoRuntime(()=>nextPool,()=>c,host,
                        new OutgameSdkStringStorage(new OutgameFileStorageBackend(path,a=>a()),s=>{throw new Exception(s);}),versions,
                        s=>{throw new Exception();},()=>throw new Exception("Held timestamp must survive"),()=>false,()=>messages,s=>{},a=>{});
                    nextPool.OnInit(true,"Proj_hdzd",new[]{nextRuntime.Registration});var next=(OutgameUserInfoManager)nextPool.Managers[4229];
                    Require(next.Name==name&&next.HaveHeadBox(8)&&next.InstallTime==456,"independent production runtime reloads exact generated name and unlocked frame");
                }finally{UnityEngine.Random.state=randomState;}
            });
            return report;
        }
        public static void Validate()
        {
            var r=Run();var existing=OutgameUserInfoValidation.Run();r.checks.AddRange(existing.checks);r.passed&=existing.passed;
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/ai-name-validation.json"),JsonUtility.ToJson(r,true));
            Debug.Log("AI_NAMES_"+(r.passed?"PASS":"FAIL")+" checks="+r.checks.Count);if(Application.isBatchMode)EditorApplication.Exit(r.passed?0:1);
        }
    }
}
