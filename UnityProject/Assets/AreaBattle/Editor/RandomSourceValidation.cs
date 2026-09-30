using System;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class RandomSourceValidation
    {
        sealed class CountingRandom : System.Random
        {
            public int Calls;
            public override int Next(int min,int max) { Calls++;return min; }
        }
        static BattleSimulation World(System.Random random=null)
        {
            return new BattleSimulation(new LevelLayout { StarInfoCfgs=new[] {
                new StarInfoCfg{CampID=1,ShipID=1,StartScore=20},
                new StarInfoCfg{CampID=1,ShipID=1,StartScore=20},
                new StarInfoCfg{CampID=1,ShipID=1,StartScore=20},
                new StarInfoCfg{CampID=2,ShipID=1,StartScore=20}
            }},BattleView.ReadConfig(),4305,(a,b)=>false,random);
        }
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,action)=>{
                var saved=UnityEngine.Random.state;
                try { action();r.checks.Add(new BattleBuild.Check{id=id,result="pass"}); }
                catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}
                finally { UnityEngine.Random.state=saved; }
            };
            var cfg=new AIConfig{AIType=0,ActionNum=1,Protect=new[]{0,1}};
            check("ai-shuffle-consumes-separate-unity-stream",()=>{
                var w=World();UnityEngine.Random.InitState(1743);
                for(int i=0;i<3;i++)UnityEngine.Random.Range(0,3);
                var expected=UnityEngine.Random.state;
                UnityEngine.Random.InitState(1743);w.RunAI(1,cfg);
                if(!expected.Equals(UnityEngine.Random.state))throw new Exception("Expected exactly one Unity Range draw per eligible source, including self swaps.");
            });
            check("ai-shuffle-recorded-index-range-and-stream-isolation",()=>{
                var w=World();int calls=0;w.AIShuffleIndex=count=>{if(count!=3)throw new Exception("Shuffle must use full list each iteration");calls++;return 2;};
                var before=UnityEngine.Random.state;w.RunAI(1,cfg);
                if(calls!=3||!before.Equals(UnityEngine.Random.state))throw new Exception("Recorded samples consumed live Unity state or skipped a source.");
            });
            check("ai-shuffle-retry-does-not-reset-unity-stream",()=>{
                var w=World();w.RunAI(1,cfg);var before=UnityEngine.Random.state;w.Restart();
                if(!before.Equals(UnityEngine.Random.state))throw new Exception("Restart reset shared Unity state.");
            });
            check("managed-random-retry-retains-stream",()=>{
                var random=new CountingRandom();var w=World(random);
                w.GetAIAction(cfg,w.Tower(1));w.Restart();w.GetAIAction(cfg,w.Tower(1));
                if(random.Calls!=2)throw new Exception("Retry replaced the provided RandomHelper stream.");
            });
            check("managed-random-shared-between-battles",()=>{
                var random=new CountingRandom();var first=World(random);first.GetAIAction(cfg,first.Tower(1));
                var second=World(random);second.GetAIAction(cfg,second.Tower(1));
                if(random.Calls!=2)throw new Exception("A new battle did not continue the session stream.");
            });
            check("managed-random-seeded-fixture-continues-after-retry",()=>{
                var w=World();var expected=new System.Random(4305);
                var weighted=new AIConfig{AIType=0,ActionNum=1,Protect=new[]{0,50},Defend=new[]{0,50}};
                for(int i=0;i<20;i++){
                    if(i==10)w.Restart();int sample=expected.Next(0,101);int action=sample<50?1:sample<100?2:0;
                    if(w.GetAIAction(weighted,w.Tower(1))!=action)throw new Exception("Seeded fixture rewound or changed its managed sequence.");
                }
            });
            return r;
        }
    }
}
