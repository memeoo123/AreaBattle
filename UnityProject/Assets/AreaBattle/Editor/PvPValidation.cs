using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class PvPValidation
    {
        static void Require(bool value,string message) { if(!value)throw new Exception(message); }
        static void Near(float value,float expected) { Require(Mathf.Abs(value-expected)<.00001f,value+" != "+expected); }
        static AgentSkillUseConfig Use(int rate=100,int total=0,float delay=0,float space=1)
        { return new AgentSkillUseConfig {total=total,DelayTime=delay,rate_skill1=rate,rate_skill2=rate,rate_skill3=rate,space_skill1=space,space_skill2=space,space_skill3=space}; }
        static BattleSimulation World(int commander=2,AgentSkillUseConfig use=null)
        {
            var layout=new LevelLayout {CampInfoCfgs=new[]{new CampInfoCfg {CampID=1},new CampInfoCfg {CampID=2}},StarInfoCfgs=new[]{
                new StarInfoCfg {CampID=1,ShipID=1,StartScore=65,pos=new IntVector3{x=0,z=100}},
                new StarInfoCfg {CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=100,z=300}},
                new StarInfoCfg {CampID=3,ShipID=1,StartScore=65,pos=new IntVector3{x=-100,z=300}},
                new StarInfoCfg {CampID=0,ShipID=1,StartScore=20,pos=new IntVector3{x=200,z=400}}
            }};
            var b=new BattleSimulation(layout,BattleView.ReadConfig(),43,(a,c)=>true){AIEnabled=false,BossAIEnabled=false};
            BattleView.ConfigureSkills(b);
            foreach(var t in b.Towers)t.AutoAddScore=false;
            b.ConfigurePvP(commander,new[]{1,1,1},use??Use());
            b.PvPChanceSample=()=>0;b.PvPTargetIndex=count=>0;
            return b;
        }
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report {passed=true,unityVersion=Application.unityVersion,
                limitations="PVP controlled fixtures validate source-derived clocks, target selection, score outcome and daily persisted mapping. Arena/model behavior, asynchronous asset latency and original random sequence remain unverified."};
            Action<string,Action> check=(id,fn)=>{try{fn();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("pvp-last-serialized-camp",()=>Require(BattleSimulation.SelectPvPEnemyCamp(new[]{new CampInfoCfg{CampID=4},new CampInfoCfg{CampID=0},new CampInfoCfg{CampID=2},new CampInfoCfg{CampID=6}})==2,"last eligible camp"));
            check("pvp-delay-crossing-returns",()=>{var b=World(use:Use(delay:.05f));b.Tick(.1f);Near(b.PvPSlots[0].Delay,-.05f);Near(b.PvPSlots[0].Timer,0);Near(b.PvPSlots[0].Used,0);b.Tick(.1f);Near(b.PvPSlots[0].Used,1);});
            check("pvp-zero-timer-strict",()=>{var b=World();b.Tick(0);Near(b.PvPSlots[0].Used,0);b.Tick(.01f);Near(b.PvPSlots[0].Used,1);});
            check("pvp-budget-total-plus-one-per-slot",()=>{var b=World();b.Tick(.1f);foreach(var s in b.PvPSlots)Near(s.Used,1);b.Tick(10);foreach(var s in b.PvPSlots)Near(s.Used,1);});
            check("pvp-active-skill-consumes-budget",()=>{var b=World(use:Use(total:6,space:.1f));b.Tick(.01f);var run=b.FindSkill(4,2);Near(run.Elapsed,.01f);b.Tick(.1f);Near(b.PvPSlots[0].Used,2);Near(run.Elapsed,.11f);});
            check("pvp-chance-strict-and-single-attempt",()=>{var b=World(use:Use(rate:15,total:100));b.PvPChanceSample=()=>15;b.Tick(8);Near(b.PvPSlots[0].Used,0);Near(b.PvPSlots[0].Timer,-7);b.PvPChanceSample=()=>14;b.Tick(.01f);Near(b.PvPSlots[0].Used,1);Near(b.PvPSlots[0].Timer,-6.01f);});
            check("pvp-scaled-agent-and-raw-skill-clocks",()=>{var b=World(use:Use(delay:1));b.Configs.GameTimeScale=2;b.Tick(.5f);Near(b.PvPSlots[0].Delay,0);b.Tick(.1f);Near(b.PvPSlots[0].Timer,.8f);Near(b.FindSkill(4,2).Elapsed,.1f);});
            check("pvp-pause-stops-agents",()=>{var b=World();b.Pause(true);b.Tick(10);Near(b.PvPSlots[0].Used,0);Near(b.PvPSlots[0].Timer,0);});
            check("pvp-reused-slot-count-not-reset",()=>{var b=World();b.Tick(.01f);b.Restart();Near(b.PvPSlots[0].Used,1);Near(b.PvPSlots[0].Timer,0);});
            check("pvp-score-truncation-and-victory-priority",()=>{var b=World();b.Tower(1).Score=0;b.Tower(2).Score=.5f;b.EvaluateOutcome();Require(b.State==BattlePhase.Victory,"enemy zero is checked before player zero");});
            check("pvp-score-defeat-despite-owned-tower",()=>{var b=World();b.Tower(1).Score=.5f;b.EvaluateOutcome();Require(b.State==BattlePhase.Defeat,"score depletion, not only loss of ownership");});
            check("pvp-victory-does-not-require-neutral",()=>{var b=World();b.Tower(2).Score=0;b.EvaluateOutcome();Require(b.State==BattlePhase.Victory&&b.Tower(4).Camp==0,"neutral irrelevant to PVP result");});
            check("pvp-strike-targets-player-only",()=>{var b=World(1);Require(b.CastAgentSkill(3,1),b.LastSkillRejection);Near(b.Tower(1).Score,45);Near(b.Tower(3).Score,65);});
            check("pvp-skill6-activation-without-heal",()=>{var b=World();b.Tower(2).Score=10;Require(b.CastAgentSkill(6,1),b.LastSkillRejection);Near(b.Tower(2).Score,10);Require(b.IsSkillActive(6,2),"base activation retained");});
            check("pvp-drain-selects-own-camp",()=>{var b=World(3);Require(b.CastAgentSkill(9,1),b.LastSkillRejection);Require(b.FindSkill(9,2).TargetId==2,"random friendly target");});
            check("pvp-dot-selects-player-camp",()=>{var b=World(4);Require(b.CastAgentSkill(12,1),b.LastSkillRejection);Require(b.FindSkill(12,2).TargetId==1,"camp1 target");});
            check("pvp-serial-can-target-neutral",()=>{var b=World(5);b.PvPTargetIndex=count=>count-1;Require(b.CastAgentSkill(15,1),b.LastSkillRejection);Near(b.Tower(4).Score,19);Require(b.Tower(4).Camp==0,"no capture");});
            check("pvp-poison-offset-bypasses-player-drag-bounds",()=>{var b=World(6);b.Tower(1).Position=new Vector3(2.4f,0,4.9f);b.PvPInclusiveInteger=(min,max)=>max;Require(b.CastAgentSkill(18,1),b.LastSkillRejection);var p=b.FindSkill(18,2).Point;Near(p.x,2.6f);Near(p.z,5.1f);});
            check("daily-mapping-pops-last-without-shuffle",()=>{var pools=new Dictionary<int,List<int>>{{1,new List<int>{10,20,30}}};var today=new Dictionary<int,int>{{1,5},{2,90}};DailyChallengeLayoutMap.Advance(pools,today);Require(today[1]==30&&pools[1].Count==2&&pools[1][1]==20,"pop last");Require(DailyChallengeLayoutMap.Resolve(today,1,2,false)==30&&DailyChallengeLayoutMap.Resolve(today,1,2,true)==90,"selector");});
            return r;
        }
    }
}
