using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameProfileValidation
    {
        static string CommanderJson=>BattleView.ReadText("Data/Outgame/CommanderConfig");
        static string SkillJson=>BattleView.ReadText("Data/AllSkillConfig");
        static void Init(OutgameProfile p)=>OutgameProfileInitialization.Initialize(p,CommanderJson,SkillJson);
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source commander record defaults and reconstruction disk envelope only; complete login/guide defaults and original save migration pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            string root=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"));
            check("outgame-profile-source-commander-defaults",()=>{
                var p=new OutgameProfile();Init(p);Require(p.usedCommanderId==1&&p.commanders.Count>0,"selection default");
                foreach(var c in p.commanders){Require(c.level==0,"selected does not imply unlocked");foreach(int level in c.skillLevels)Require(level==1,"skill seed one");}
            });
            check("outgame-profile-initialize-preserves-held-progress",()=>{
                var p=new OutgameProfile();Init(p);int n=p.commanders.Count;
                p.usedCommanderId=2;p.commanders.Find(c=>c.id==2).level=4;p.commanders.Find(c=>c.id==2).skillLevels[0]=5;
                new OutgameLocalInventory(p.inventory,SkillJson).Change(2001,-2);Init(p);
                Require(p.commanders.Count==n&&p.usedCommanderId==2&&p.commanders.Find(c=>c.id==2).skillLevels[0]==5,"held progress retained");
                Require(new OutgameLocalInventory(p.inventory,SkillJson).Count(2001)==0,"no replenishment on init");
            });
            check("outgame-profile-disk-upgrade-restart",()=>{
                string path=Path.Combine(root,"account.json");var store=new OutgameProfileStore(path);var p=store.Load();Init(p);
                p.inventory.goldNum=10250;store.Save(p);
                var inventory=new OutgameLocalInventory(p.inventory,SkillJson);
                var rules=new OutgameCommanderProgression(CommanderJson,BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                var c=p.commanders.Find(x=>x.id==2);
                Action<int,int> debit=(id,delta)=>Require(inventory.Change(id,delta),"debit");
                Require(rules.TryUpgrade(c,inventory.Count,debit,out _)&&rules.TryUpgrade(c,inventory.Count,debit,out _),"unlock and upgrade");
                inventory.Change(2001,-2);p.usedCommanderId=2;store.Save(p);
                var reloaded=new OutgameProfileStore(path).Load();Init(reloaded);
                Require(reloaded.inventory.goldNum==0&&reloaded.usedCommanderId==2&&reloaded.commanders.Find(x=>x.id==2).level==2&&reloaded.commanders.Find(x=>x.id==2).skillLevels[0]==2,"durable transaction");
                Require(new OutgameLocalInventory(reloaded.inventory,SkillJson).Count(2001)==0,"durable depletion");
                Require(File.Exists(path+".backup")&&!File.Exists(path+".pending"),"committed replacement and backup");
            });
            check("outgame-profile-corrupt-save-preserved",()=>{
                string path=Path.Combine(root,"corrupt.json");Directory.CreateDirectory(root);File.WriteAllText(path,"{broken");
                bool rejected=false;try{new OutgameProfileStore(path).Load();}catch{rejected=true;}
                Require(rejected&&File.ReadAllText(path)=="{broken","do not silently reset corruption");
            });
            check("outgame-profile-future-version-preserved",()=>{
                string path=Path.Combine(root,"future.json");string data="{\"schemaVersion\":99}";File.WriteAllText(path,data);
                bool rejected=false;try{new OutgameProfileStore(path).Load();}catch{rejected=true;}
                Require(rejected&&File.ReadAllText(path)==data,"do not downgrade unknown schema");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-profile-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
