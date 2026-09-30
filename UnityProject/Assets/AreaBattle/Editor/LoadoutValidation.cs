using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class LoadoutValidation
    {
        static void Require(bool value,string message) {if(!value)throw new Exception(message);}
        static BattleSimulation World()
        {
            var b=new BattleSimulation(new LevelLayout{StarInfoCfgs=new[]{
                new StarInfoCfg{CampID=1,ShipID=1,StartScore=10,pos=new IntVector3{z=100}},
                new StarInfoCfg{CampID=2,ShipID=1,StartScore=20,pos=new IntVector3{x=100,z=200}}}},BattleView.ReadConfig(),43,(a,c)=>true){AIEnabled=false};
            BattleView.ConfigureSkills(b);return b;
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Local reconstruction held-state contract only. Tests neither read nor modify original accounts and infer no purchase/ad/win rewards."};
            string root=Path.Combine(BattleBuild.Workspace,"analysis/loadout-validation");Directory.CreateDirectory(root);
            string run=Guid.NewGuid().ToString("N");var files=new List<string>();
            Func<string,string> path=name=>{string p=Path.Combine(root,run+"-"+name+".json");files.Add(p);return p;};
            string rules=BattleView.ReadText("Data/AllSkillConfig");
            Action<string,Action> check=(id,fn)=>{try{fn();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("loadout-first-run-only-fixture",()=>{
                string p=path("new");var fixture=BattleLoadout.CreateFixture(14,3,2,3,2);
                var held=BattleLoadout.LoadOrCreate(p,fixture);Require(held.CreatedFromFixture&&held.NormalLevel==14&&held.CommanderId==3,"explicit initial state");
                fixture.skillItems[0].count=99;Require(held.ItemCount(2001)==3,"fixture deep copied");
                var loaded=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(99,6,10,99,99));
                Require(!loaded.CreatedFromFixture&&loaded.NormalLevel==14&&loaded.CommanderId==3&&loaded.ItemCount(2001)==3&&loaded.GenericStock==2,"existing file ignores fixture");
            });
            check("loadout-legacy-level-preserved-zero-inventory",()=>{
                string p=path("legacy");File.WriteAllText(p,"{\"normalLevel\":17}");
                var held=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(0,6,10,3,4));
                Require(held.MigratedLegacyProfile&&held.NormalLevel==17&&held.CommanderId==1,"legacy progress retained");
                Require(held.ItemCount(2001)==0&&held.GenericStock==0&&held.SkillLevel(1)==1,"legacy absent stock is zero, no fixture refill");
                var again=BattleLoadout.LoadOrCreate(p);Require(!again.MigratedLegacyProfile&&again.NormalLevel==17,"migration persisted once");
            });
            check("loadout-empty-held-inventory-not-refilled",()=>{
                string p=path("empty");var fixture=BattleLoadout.CreateFixture(21);fixture.skillItems.Clear();
                BattleLoadout.LoadOrCreate(p,fixture);var held=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(21,1,1,3));
                Require(held.ItemCount(2001)==0&&!held.CreateSkillInput(World(),rules,21).TryUse(0),"empty means empty");
            });
            check("loadout-independent-three-skill-levels",()=>{
                string p=path("levels");var fixture=BattleLoadout.CreateFixture(30,2,1,1);
                fixture.skillLevels.Find(x=>x.skillId==4).level=2;fixture.skillLevels.Find(x=>x.skillId==5).level=3;fixture.skillLevels.Find(x=>x.skillId==6).level=4;
                var held=BattleLoadout.LoadOrCreate(p,fixture);var b=World();var input=held.CreateSkillInput(b,rules,30);
                Require(input.TryUse(0)&&input.TryUse(1)&&input.TryUse(2,1),"three configured casts");
                Require(b.FindSkill(4).Level==2&&b.FindSkill(5).Level==3&&b.FindSkill(6).Level==4,"per-slot levels reached production CastSkill");
                var again=BattleLoadout.LoadOrCreate(p);Require(again.SkillLevel(4)==2&&again.SkillLevel(5)==3&&again.SkillLevel(6)==4,"levels persist");
            });
            check("loadout-specific-before-generic-persists-across-battles",()=>{
                string p=path("debit");var held=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(21,1,1,1,1));
                var first=held.CreateSkillInput(World(),rules,21);Require(first.TryUse(0),"first accepted");
                var reloaded=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(21,1,1,9,9));
                Require(reloaded.ItemCount(2001)==0&&reloaded.GenericStock==1,"specific persisted");
                var second=reloaded.CreateSkillInput(World(),rules,21);Require(second.TryUse(0),"generic fallback");
                var final=BattleLoadout.LoadOrCreate(p);Require(final.ItemCount(2001)==0&&final.GenericStock==0,"generic persisted");
                Require(!final.CreateSkillInput(World(),rules,21).TryUse(0),"next battle does not replenish");
            });
            check("loadout-rejections-do-not-debit-or-save",()=>{
                string p=path("reject");var held=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(21,1,1,2));string before=File.ReadAllText(p);var b=World();
                Require(!held.CreateSkillInput(b,rules,5).TryUse(0),"locked rejected");
                var input=held.CreateSkillInput(b,rules,21);Require(!input.TryUse(2,1),"friendly lightning rejected");
                b.Pause(true);Require(!input.TryUse(0),"pause rejected");Require(File.ReadAllText(p)==before,"no save on rejected cast");
            });
            check("loadout-consumption-callback-once-with-details",()=>{
                var b=World();int calls=0;SkillConsumption result=null;
                var input=new BattleSkillInput(b,rules,1,21,new[]{2,3,4},new Dictionary<int,int>{{2001,1}},1,c=>{calls++;result=c;});
                Require(input.TryUse(0)&&!input.TryUse(0),"accepted then active-rejected");
                Require(calls==1&&result.ItemId==2001&&result.SkillId==1&&result.SkillLevel==2&&result.RemainingItemStock==0&&result.RemainingGenericStock==1,"specific callback details");
                b.Tick(10);Require(input.TryUse(0),"generic accepted");Require(calls==2&&result.ItemId==1005&&result.RemainingGenericStock==0,"generic callback");
            });
            check("loadout-progress-and-explicit-commander-persist-with-stock",()=>{
                string p=path("progress");var held=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(5,1,1,1));held.SetCommander(6);held.SetNormalLevel(6);
                var again=BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(0,1));
                Require(again.CommanderId==6&&again.NormalLevel==6&&again.ItemCount(2018)==1,"updating progress does not overwrite loadout");
            });
            check("loadout-level-range-and-invalid-file-no-reset",()=>{
                bool rejected=false;try{BattleLoadout.CreateFixture(0,1,11);}catch(InvalidDataException){rejected=true;}Require(rejected,"out-of-source-range rejected");
                string p=path("bad");File.WriteAllText(p,"{\"unrelated\":5}");string before=File.ReadAllText(p);rejected=false;
                try{BattleLoadout.LoadOrCreate(p,BattleLoadout.CreateFixture(0,1,1,3));}catch(InvalidDataException){rejected=true;}
                Require(rejected&&File.ReadAllText(p)==before,"invalid existing profile not overwritten with fixture");
            });
            foreach(string p in files)if(File.Exists(p))File.Delete(p);
            return report;
        }
    }
}
