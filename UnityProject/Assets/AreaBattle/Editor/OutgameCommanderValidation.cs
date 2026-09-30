using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameCommanderValidation
    {
        static OutgameCommanderProgression Rules()=>new OutgameCommanderProgression(
            BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
        static OutgameCommanderState State(int id=2,int level=0)=>new OutgameCommanderState{id=id,level=level,skillLevels=new[]{1,1,1}};
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Commander core only; UI gates, first-run defaults, persistence and statistics integration pending."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("outgame-commander-original-cost-boundaries",()=>{
                var rules=Rules();var s=State();int[] levels={0,1,5,6,14,15,27,28};int[] costs={10000,250,250,500,500,1000,1000,10000};
                for(int i=0;i<levels.Length;i++){s.level=levels[i];Require(rules.NextCost(s)==costs[i],"source next-cost at level "+levels[i]);}
            });
            check("outgame-commander-level-gate-distinct-from-affordability",()=>{
                var rules=Rules();var s=State(3);
                Require(!rules.IsLevelUnlockable(s,21)&&rules.IsLevelUnlockable(s,22),"original unlockLevel22 inclusive");
                Require(rules.CanAffordUnlock(s,id=>10000),"resource query does not add an unproven level guard");
                Require(!rules.IsUnlocked(s),"level0 locked");s.level=1;Require(rules.IsUnlocked(s),"level1 unlocked");
            });
            check("outgame-commander-unlock-insufficient-and-exact-cost",()=>{
                var rules=Rules();var s=State();int coins=9999,changes=0;
                Func<int,int> count=id=>id==1001?coins:0;Action<int,int> change=(id,delta)=>{Require(id==1001,"source coin currency");coins+=delta;changes++;};
                Require(!rules.TryUpgrade(s,count,change,out int slot)&&slot==-1&&s.level==0&&coins==9999&&changes==0,"failed unlock leaves inventory and progression untouched");
                coins=10000;Require(rules.TryUpgrade(s,count,change,out slot)&&coins==0&&changes==1&&s.level==1&&slot==-1,"exact unlock spends once and starts level1");
                Require(s.skillLevels[0]==1&&s.skillLevels[1]==1&&s.skillLevels[2]==1,"unlock does not increment a skill");
            });
            check("outgame-commander-upgrade-rotates-source-skills",()=>{
                var rules=Rules();var s=State(2,1);int coins=750;
                for(int i=0;i<3;i++){Require(rules.TryUpgrade(s,id=>coins,(id,delta)=>coins+=delta,out int slot)&&slot==i,"source skill rotation slot"+i);}
                Require(s.level==4&&coins==0&&s.skillLevels[0]==2&&s.skillLevels[1]==2&&s.skillLevels[2]==2,"three upgrades advance each skill once");
            });
            check("outgame-commander-full-28-level-cap-and-price-total",()=>{
                var rules=Rules();var s=State(2,1);int coins=18750,changes=0;
                for(int i=0;i<27;i++)Require(rules.TryUpgrade(s,id=>coins,(id,delta)=>{coins+=delta;changes++;},out _),"source full progression step"+i);
                Require(coins==0&&changes==27&&s.level==28&&s.skillLevels[0]==10&&s.skillLevels[1]==10&&s.skillLevels[2]==10,"source 27 upgrade rows total18750; three level10 skills");
                coins=10000;Require(rules.CanAffordUpgrade(s,id=>coins),"original affordability query uses unlock cost at max");
                Require(!rules.TryUpgrade(s,id=>coins,(id,delta)=>coins+=delta,out int slot)&&slot==-1&&coins==10000,"actual upgrade still rejects max without spending");
            });
            check("outgame-commander-diamond-unlock-and-upgrade-currency",()=>{
                var rules=Rules();var s=State(5);var wallet=new Dictionary<int,int>{{1001,100000},{1002,499}};
                Func<int,int> count=id=>wallet[id];Action<int,int> change=(id,delta)=>wallet[id]+=delta;
                Require(!rules.TryUpgrade(s,count,change,out _),"gold cannot substitute for diamond499");
                wallet[1002]=750;Require(rules.TryUpgrade(s,count,change,out _)&&wallet[1002]==250,"unlock diamond500");
                Require(rules.TryUpgrade(s,count,change,out int slot)&&slot==0&&wallet[1002]==0&&wallet[1001]==100000,"upgrade uses first unlockPrice currency too");
            });
            check("outgame-commander-upgrade-rejection-does-not-mutate",()=>{
                var rules=Rules();var s=State(2,1);int coins=249,changes=0;
                Require(!rules.TryUpgrade(s,id=>coins,(id,delta)=>{coins+=delta;changes++;},out int slot),"one coin short rejected");
                Require(slot==-1&&s.level==1&&s.skillLevels[0]==1&&coins==249&&changes==0,"no partial upgrade or debit");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-commander-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
    }
}
