using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameOriginalLocalDataValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original local scalar/tool payload import only. Raw payload retained; skin/commander/other migrations and live login still pending."};
            try
            {
                string skills=BattleView.ReadText("Data/AllSkillConfig");var p=new OutgameProfile();
                string legacy="{\"LevelID\":14,\"goldNum\":123,\"bankNum\":9,\"bankAdNum\":3,\"iceNum\":0,\"supportNum\":7,\"toolnum_4\":8,\"fireNum\":9,\"upgradeNum\":10,\"toolnum_5\":11,\"unknownFutureField\":77}";
                OutgameOriginalLocalData.Apply(p,legacy,skills);var inventory=new OutgameLocalInventory(p.inventory,skills);
                int[] expected={0,7,8,9,10,11};for(int i=0;i<expected.Length;i++)Require(inventory.Count(2001+i)==expected[i],"original old tool mapping "+i);
                Require(inventory.Count(2007)==2&&p.levelID==14&&p.inventory.collectNum==9&&p.inventory.collectAdNum==3&&p.sourceLocalDataJson==legacy,"source rename and untouched original payload");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-local-legacy-tool-mapping-and-rename",result="pass"});
                OutgameOriginalLocalData.Apply(p,"{\"toolCounts\":[{\"id\":2001,\"count\":0}],\"supportNum\":99}",skills);inventory=new OutgameLocalInventory(p.inventory,skills);
                Require(inventory.Count(2001)==0&&inventory.Count(2002)==2,"nonempty modern list suppresses legacy fields");
                OutgameOriginalLocalData.Apply(p,"{\"iceNum\":-1,\"supportNum\":-2}",skills);inventory=new OutgameLocalInventory(p.inventory,skills);
                Require(inventory.Count(2001)==2&&inventory.Count(2002)==-2&&inventory.Count(2003)==2,"only -1 skips; missing old fields use ctor2");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-local-modern-precedence-and-legacy-sentinel",result="pass"});
                foreach(string empty in new string[]{null,"","null","{}"}){OutgameOriginalLocalData.Apply(p,empty,skills);Require(p.levelID==0&&p.inventory.goldNum==0&&new OutgameLocalInventory(p.inventory,skills).Count(2001)==2,"no invented first-level/currency grant");}
                report.checks.Add(new BattleBuild.Check{id="outgame-original-local-empty-defaults",result="pass"});
                OutgameOriginalLocalData.Apply(p,legacy,skills);string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"original.json");var store=new OutgameProfileStore(path);store.Save(p);var loaded=store.Load();
                Require(loaded.sourceLocalDataJson==legacy&&loaded.levelID==14&&loaded.inventory.goldNum==123&&loaded.inventory.collectNum==9,"raw unknown fields and migrated scalars survive reconstruction restart");
                bool threw=false;try{OutgameOriginalLocalData.Apply(loaded,"{broken",skills);}catch(ArgumentException){threw=true;}
                Require(threw&&loaded.sourceLocalDataJson==legacy&&loaded.inventory.goldNum==123&&store.Load().sourceLocalDataJson==legacy,"failed import leaves held and disk generation intact");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-local-restart-and-failed-import-preservation",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-original-local-import",result="fail",detail=e.ToString()});}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-original-local-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
