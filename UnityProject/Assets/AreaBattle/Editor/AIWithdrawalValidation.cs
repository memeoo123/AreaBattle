using System;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    // Source: AIGoToAction protect branch, original WASM 0x6318d1..0x631bda.
    // Isolates the late-withdrawal mechanism observed in the level871 recording.
    // Fixture source order is explicit; it is not asserted to be original RNG state.
    public static class AIWithdrawalValidation
    {
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static BattleSimulation World(bool rightFirst)
        {
            var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_120"));
            var sim=new BattleSimulation(layout,BattleView.ReadConfig(),4305,(a,b)=>true);
            sim.AIEnabled=false;
            foreach(var t in sim.Towers)t.Camp=t.Id==3||t.Id==6?2:t.Id==7?3:1;
            int index=0;sim.AIShuffleIndex=count=>{Require(count==2,"exactly two candidate red towers");return index++==0?(rightFirst?1:0):1;};
            sim.Tower(6).RegenAccumulator=1.18333292f;
            Require(sim.Connect(6,7,true),"right-bottom outgoing fixture");
            return sim;
        }
        static void Act(BattleSimulation sim){sim.RunAI(2,new AIConfig{id=99,AIType=3,ActionNum=1});}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source-derived protect-branch fixtures; not original-session RNG or matched recording."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("ai-protect-empty-incoming-stops-before-later-source",()=>{
                var sim=World(false);Act(sim);
                Require(sim.FindLine(6,7).IsFrom(6),"earlier empty incoming branch must leave later tower unprocessed");
                Require(Mathf.Abs(sim.Tower(6).RegenAccumulator-1.18333292f)<1e-6f,"protected regen clock retained");
            });
            check("ai-protect-first-source-withdraws-and-resumes-preserved-regen",()=>{
                var sim=World(true);Act(sim);
                Require(!sim.FindLine(6,7).IsFrom(6)&&sim.Tower(6).OutgoingCount==0,"selected tower must withdraw without incoming enemy");
                float score=sim.Tower(6).Score;sim.Tick(.8f);Require(sim.Tower(6).Score==score,"not a fresh two-second timer and not immediate regen");
                sim.Tick(.02f);Require(sim.Tower(6).Score==score+1,"remaining accumulated regen resumes after withdrawal");
            });
            check("ai-protect-incoming-enemy-reconnects-and-blocks-regen",()=>{
                var sim=World(true);Require(sim.Connect(7,6,true),"incoming enemy fixture");Act(sim);
                Require(sim.FindLine(6,7).Direction==3,"incoming survives and protection reconnects opposite direction");
                sim.Tick(.1f);Require(Mathf.Abs(sim.Tower(6).RegenAccumulator-1.18333292f)<1e-6f,"reconnected tower cannot regenerate");
            });
            return report;
        }
    }
}
