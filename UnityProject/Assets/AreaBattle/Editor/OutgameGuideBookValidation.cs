using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameGuideBookValidation
    {
        sealed class Host:IOutgameDataStorageHost
        {
            public bool Server;public int Progress=10;public int SourceLoginProgress=>Progress;
            public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;public bool IsUseServer=>Server;
            public string MineGameName=>"Proj_hdzd";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){throw new Exception(s);}public void Toast(string s){}
            public string Compress(string k,string s)=>s;public string Decompress(string k,string s)=>s;
            public void QueueUpload(string k,string s){throw new Exception("Unexpected test upload");}
        }
        sealed class Backend:IOutgameStorageBackend
        {
            public readonly OutgameFileStorageBackend File;public int Writes;public bool Fail;
            public Backend(string path){File=new OutgameFileStorageBackend(path,a=>a());}
            public string Get(string k)=>File.Get(k);
            public void Set(string k,string v,Action<string> fail,Action complete)
            {Writes++;if(Fail)throw new IOException("Explicit write failure");File.Set(k,v,fail,complete);}
            public void Remove(string k,Action<string> fail,Action complete)=>File.Remove(k,fail,complete);
            public void Clear(Action<string> fail,Action complete)=>File.Clear(fail,complete);
        }
        sealed class Fixture
        {
            public readonly Host Host=new Host();public readonly Backend Backend;
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameGuideBookRuntime Runtime;
            public readonly List<string> Downloads=new List<string>();
            public int Level,LevelReads;
            public OutgameGuideBookManager Manager=>(OutgameGuideBookManager)Pool.Managers[4078];
            public OutgameGuideBookControl Control=>(OutgameGuideBookControl)Registry.Resolve(4076);
            public Fixture(string path=null,bool server=false)
            {
                Host.Server=server;Backend=new Backend(path??Path.Combine(Path.GetTempPath(),"AreaBattleBook-"+Guid.NewGuid().ToString("N")));
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});
                Runtime=new OutgameGuideBookRuntime(Host,new OutgameSdkStringStorage(Backend,s=>{throw new Exception(s);}),
                    new OutgameDataVersionState(()=>{},()=>{},()=>{},n=>{},s=>{}),Downloads.Add);
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                Runtime.BindController(Registry,()=>Pool,()=>Config,()=>{LevelReads++;return Level;},()=>Messages);
                Pool.OnInit(true,"Proj_hdzd",new[]{Runtime.Registration});Control.OnInit();
            }
            public void ReadOriginal()
            {
                var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});
                reader.ReadTable(Config.dicGuide);reader.ReadTable(Config.dicGuidebook);reader.ReadTable(Config.dicGuideTips);
            }
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("guide-book-original-record-construction-and-unrepaired-null-lists",()=>{
                var empty=new OutgameGuideBookData();Require(empty.GuideRewards==null&&empty.TipRewards==null,"source empty constructor");
                var f=new Fixture();var data=f.Manager.BookData;
                Require(data.GuideRewards.Count==0&&data.TipRewards.Count==0&&f.Backend.Writes==0,"fresh lists without implicit save");
                Require(!ReferenceEquals(f.Manager.CreateNewData(),data)&&ReferenceEquals(f.Manager.BookData,data),"CreateNewData returns without publishing");
                f.Manager.UpdateDataCallBack("{\"GuideRewards\":[4,4],\"TipRewards\":[8]}");
                Require(f.Manager.BookData.GuideRewards.Count==2&&f.Manager.ContainsTip(8),"loaded duplicates retained");
                f.Manager.BookData.TipRewards=null;Throws<NullReferenceException>(()=>f.Manager.ContainsTip(8));
                f.Manager.OnRelease();Require(f.Manager.BookData!=null,"source OnRelease does not clear record");
            });
            check("guide-book-production-registration-server-wait-and-local-override",()=>{
                var f=new Fixture(server:true);Require(f.Downloads.Count==1&&f.Downloads[0]=="GuideBookDataManager"&&f.Manager.BookData==null,"real server branch waits without fresh data");
                Require(f.Manager.ParticipatesInSync&&!f.Manager.CompressData&&ReferenceEquals(f.Control.Manager,f.Manager),"original registration and pool owner");
                f.Manager.UpdateDataCallBack("{\"GuideRewards\":[2],\"TipRewards\":[3]}");Require(f.Control.IsGetBookReward(2)&&f.Control.IsGetTipReward(3),"explicit download callback");
                f.Manager.UpdateData(false);Require(f.Downloads.Count==1&&!f.Manager.ContainsGuide(2),"local override reloads empty storage without second request");
            });
            check("guide-book-claims-save-before-message-and-deduplicate-record-only",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleBookRestart-"+Guid.NewGuid().ToString("N"));var f=new Fixture(path);int events=0;
                f.Messages.AddListener("GetGuidBookReward",args=>{events++;Require(args==null&&f.Backend.Get("Proj_hdzdGuideBookDataManager").Contains("91"),"save precedes source null-argument message");});
                f.Control.GetBookRrward(91);f.Control.GetBookRrward(91);f.Control.GetTipReward(92);
                Require(events==3&&f.Backend.Writes==2&&f.Manager.BookData.GuideRewards.Count==1,"repeat controller claim still notifies, manager avoids duplicate save");
                var next=new Fixture(path);Require(next.Control.IsGetBookReward(91)&&next.Control.IsGetTipReward(92),"independent production registration reloads both lists from disk");
            });
            check("guide-book-save-and-message-failure-retain-source-mutation-order",()=>{
                var f=new Fixture();int events=0;f.Messages.AddListener("GetGuidBookReward",a=>events++);f.Backend.Fail=true;
                Throws<IOException>(()=>f.Control.GetTipReward(3));Require(f.Manager.ContainsTip(3)&&events==0,"append survives save failure and notification is skipped");
                f.Control.GetTipReward(3);Require(events==1&&f.Backend.Writes==1,"duplicate skips failed save but sends controller message");
                var next=new Fixture();next.Messages.AddListener("GetGuidBookReward",a=>{throw new InvalidOperationException();});
                Throws<InvalidOperationException>(()=>next.Control.GetBookRrward(9));Require(next.Backend.Get("Proj_hdzdGuideBookDataManager").Contains("9"),"event failure follows successful disk persistence");
            });
            check("guide-book-tip-inclusive-book-strict-and-recursive-guide-unlock",()=>{
                var f=new Fixture();f.Config.dicGuideTips[7]=new GuideTipsConfig{id=7,unlockLevel=5};
                f.Config.dicGuide[1]=new GuideConfig{id=1,guildLv=5};f.Config.dicGuide[2]=new GuideConfig{id=2,guildLv=-1};f.Config.dicGuide[3]=new GuideConfig{id=3,guildLv=-2};
                f.Config.dicGuidebook[9]=new GuidebookConfig{id=9,guideId=3};f.Level=5;
                Require(f.Control.IsTipUnlock(7)&&!f.Control.IsBookUnlock(9)&&f.Control.GetGuideUnlockLv(3)==5,"tip >=5 versus book > inherited5");
                f.Level=6;Require(f.Control.IsBookUnlock(9),"strict threshold after progression");
                Require(f.Control.GetGuideUnlockLv(99)==0&&!f.Control.IsBookUnlock(99)&&!f.Control.IsTipUnlock(99),"source missing guide default and missing rows");
                f.Config.dicGuidebook[8]=new GuidebookConfig{id=8,guideId=99};f.Level=0;Require(!f.Control.IsBookUnlock(8),"missing guide returns0, still strict");
                f.Level=1;Require(f.Control.IsBookUnlock(8),"missing guide and positive level");
                f.Config.dicGuide.Remove(1);Require(f.Control.GetGuideUnlockLv(3)==0,"negative guide chain stops on missing predecessor");
            });
            check("guide-book-source-null-and-level-query-order",()=>{
                var f=new Fixture();f.Config.dicGuideTips[1]=null;f.Config.dicGuidebook[1]=null;
                Throws<NullReferenceException>(()=>f.Control.IsTipUnlock(1));Require(f.LevelReads==1,"tip queries current level before null row dereference");
                Throws<NullReferenceException>(()=>f.Control.IsBookUnlock(1));Require(f.LevelReads==2,"book queries current level before null row dereference");
                f.Control.IsTipUnlock(2);f.Control.IsBookUnlock(2);Require(f.LevelReads==2,"missing rows avoid level owner access");
                f.Config.dicGuide[1]=null;Throws<NullReferenceException>(()=>f.Control.GetGuideUnlockLv(1));
            });
            check("guide-book-original-config-availability-and-claim-status",()=>{
                var f=new Fixture();f.ReadOriginal();Require(f.Config.dicGuidebook.Count>0&&f.Config.dicGuideTips.Count>0,"actual original configs loaded");
                f.Level=int.MaxValue;Require(f.Control.HaveAnyUnGetGuideReward()&&f.Control.HaveAnyUnGetTipReward(),"original unlocked entries are claimable");
                foreach(var row in f.Config.dicGuidebook.Values)f.Control.GetBookRrward(row.id);
                foreach(var row in f.Config.dicGuideTips.Values)f.Control.GetTipReward(row.id);
                Require(!f.Control.HaveAnyUnGetGuideReward()&&!f.Control.HaveAnyUnGetTipReward(),"all original entries claimed");
                int reads=f.LevelReads;f.Control.HaveAnyUnGetGuideReward();f.Control.HaveAnyUnGetTipReward();Require(f.LevelReads==reads,"claimed entries short circuit before progression query");
            });
            check("guide-book-dispose-disables-update-retains-registry-and-data",()=>{
                var f=new Fixture();var control=f.Control;var manager=control.Manager;control.ActiveUpdate=true;control.Updata(100,100);control.OnDispose();
                Require(!control.ActiveUpdate&&ReferenceEquals(f.Registry.Resolve(4076),control)&&ReferenceEquals(control.Manager,manager),"source dispose only clears its boolean");
                control.GetTipReward(-4);Require(manager.ContainsTip(-4),"claim path has no config/unlock validation or item grant");
            });
            return report;
        }
        public static void Validate()
        {
            var r=Run();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-book-validation.json"),JsonUtility.ToJson(r,true));
            Debug.Log("GUIDE_BOOK_"+(r.passed?"PASS":"FAIL")+" checks="+r.checks.Count);if(Application.isBatchMode)EditorApplication.Exit(r.passed?0:1);
        }
    }
}
