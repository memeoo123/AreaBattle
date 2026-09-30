using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameInventoryValidation
    {
        static OutgameLocalInventory Open(OutgameLocalInventoryState state)=>new OutgameLocalInventory(state,BattleView.ReadText("Data/AllSkillConfig"));
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="LocalDataManager inventory core only; complete account initialization, rewards, events and durable store pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("outgame-inventory-seeds-only-configured-missing-records",()=>{
                var held=new OutgameLocalInventoryState();var inv=Open(held);
                Require(held.toolCounts.Count==18,"18 AllSkillConfig gameItem rows");
                for(int id=2001;id<=2018;id++)Require(inv.Count(id)==2,"source missing-row seed2");
                Require(inv.Count(1001)==0&&inv.Count(1002)==0&&inv.Count(1005)==0,"inventory initializer does not grant scalar balances");
            });
            check("outgame-inventory-reload-does-not-refill-exhausted-tools",()=>{
                var held=new OutgameLocalInventoryState();held.toolCounts.Add(new OutgameToolCount{id=2001,count=0});held.toolCounts.Add(new OutgameToolCount{id=2002,count=7});
                var inv=Open(held);Require(inv.Count(2001)==0&&inv.Count(2002)==7&&inv.Count(2003)==2,"held rows override missing-row seed");
                Require(inv.Change(2003,-2),"spend exact balance");
                var loaded=JsonUtility.FromJson<OutgameLocalInventoryState>(JsonUtility.ToJson(held));var reloaded=Open(loaded);
                Require(reloaded.Count(2003)==0&&reloaded.Count(2001)==0&&loaded.toolCounts.Count==18,"reload keeps exhausted rows and creates no duplicates");
            });
            check("outgame-inventory-source-currency-routing",()=>{
                var inv=Open(new OutgameLocalInventoryState());
                foreach(int id in new[]{1001,1002,1004,1005})Require(inv.Change(id,13)&&inv.Count(id)==13,"original scalar currency "+id);
                Require(!inv.Change(1003,13)&&inv.Count(1003)==0,"1003 is not strength; package routing is outside local setter");
                Require(!inv.Change(7999,13)&&inv.Count(7999)==0,"unknown item is not silently created");
            });
            check("outgame-inventory-insufficient-debit-is-atomic",()=>{
                var inv=Open(new OutgameLocalInventoryState{goldNum=3,diamondsNum=3,strengthsNum=3,ToolValue=3});
                foreach(int id in new[]{1001,1002,1004,1005,2001})
                {
                    int count=inv.Count(id);Require(!inv.Change(id,-count-1)&&inv.Count(id)==count,"no partial debit or clamping");
                    Require(inv.Change(id,-count)&&inv.Count(id)==0,"exact debit succeeds");
                }
            });
            check("outgame-inventory-source-overflow-rejected-before-store",()=>{
                var held=new OutgameLocalInventoryState{goldNum=int.MaxValue};var inv=Open(held);
                Require(!inv.Change(1001,1)&&held.goldNum==int.MaxValue,"WASM i32 overflow goes negative and is rejected");
            });
            check("outgame-inventory-integrates-commander-spend-and-roundtrip",()=>{
                var held=new OutgameLocalInventoryState{goldNum=10250};var inv=Open(held);
                var commander=new OutgameCommanderState{id=2,level=0,skillLevels=new[]{1,1,1}};
                var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                Action<int,int> debit=(id,delta)=>Require(inv.Change(id,delta),"validated debit");
                Require(rules.TryUpgrade(commander,inv.Count,debit,out _)&&rules.TryUpgrade(commander,inv.Count,debit,out int slot)&&slot==0,"unlock and upgrade with production inventory");
                var restored=Open(JsonUtility.FromJson<OutgameLocalInventoryState>(JsonUtility.ToJson(held)));
                Require(restored.Count(1001)==0&&commander.level==2&&commander.skillLevels[0]==2,"full debit and skill increment survive inventory roundtrip");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-inventory-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
