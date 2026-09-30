using System;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    // Expectations are source-derived; these checks do not claim original live replay.
    public static class ArrowValidation
    {
        static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
        static void Near(float actual, float expected) { Require(Mathf.Abs(actual - expected) < .00001f, actual + " != " + expected); }
        static BattleSimulation Fixture(bool extra = false)
        {
            var layout = new LevelLayout { CampInfoCfgs = Array.Empty<CampInfoCfg>(), StarInfoCfgs = extra ? new[] {
                new StarInfoCfg { ShipID=4,CampID=1,StartScore=5 },
                new StarInfoCfg { ShipID=1,CampID=2,StartScore=5,pos=new IntVector3{x=69} },
                new StarInfoCfg { ShipID=1,CampID=3,StartScore=5,pos=new IntVector3{x=-69} }
            } : new[] {
                new StarInfoCfg { ShipID=4,CampID=1,StartScore=5 },
                new StarInfoCfg { ShipID=1,CampID=2,StartScore=5,pos=new IntVector3{x=69} }
            }};
            var b = new BattleSimulation(layout, BattleView.ReadConfig(), 43, (a,c)=>true);
            b.AIEnabled=false; foreach(var t in b.Towers)t.AutoAddScore=false;
            return b;
        }
        public static BattleBuild.Report Run()
        {
            var report = new BattleBuild.Report { unityVersion=Application.unityVersion, passed=true,
                limitations="Arrow source-derived checks; original visual replay, async prefab latency and exact cross-retry pool reuse remain unverified." };
            Action<string,Action> check = (id,test) => {
                try { test(); report.checks.Add(new BattleBuild.Check{id=id,result="pass"}); }
                catch(Exception e) { report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message}); }
            };
            check("arrow-config-and-line-rejection",()=>{
                var b=Fixture();Require(b.Tower(1).IsArrow && !b.Tower(1).CanAddLine,"Ship4 must have no outgoing line capacity");
                Require(!b.Connect(1,2),"Player must not originate arrow line");Near(BattleSimulation.GetArrowRate(0),1.8f);
                Near(BattleSimulation.GetArrowRate(1),1.3f);Near(BattleSimulation.GetArrowRate(2),.8f);Near(BattleSimulation.GetArrowRegionScale(0),.308f);
            });
            check("arrow-range-inclusive-and-source-sqrt",()=>{
                foreach(float d in new[]{.2f,.5f,.69f,.7f,.71f}) {
                    bool soldier=d<=.7f, tower=d>=.69f && d<=.7f;
                    Require(BattleSimulation.ArrowSoldierInRange(Vector3.zero,new Vector3(d,0,0),.7f)==soldier,"soldier d="+d);
                    Require(BattleSimulation.ArrowTowerInRange(Vector3.zero,new Vector3(d,0,0),.7f)==tower,"tower d="+d);
                }
            });
            check("arrow-clock-strict-threshold-and-no-catchup",()=>{
                var b=Fixture();b.Tick(1.8f);Require(b.Arrows.Count==0,"strict >, not >=");
                b.Tick(.001f);Require(b.Arrows.Count==1,"one attack after crossing threshold");Near(b.Tower(1).ArrowAccumulator,0);
                b.Tick(8);Require(b.Arrows.Count==2,"large dt must attempt only once");Near(b.Tower(1).ArrowAccumulator,0);
            });
            check("arrow-no-target-still-consumes-clock",()=>{
                var b=Fixture();b.Tower(2).Position=new Vector3(3,0,0);b.Tick(2);
                Require(b.Arrows.Count==0,"out of range");Near(b.Tower(1).ArrowAccumulator,0);
            });
            check("arrow-first-soldier-not-nearest-and-priority",()=>{
                var b=Fixture(true);Require(b.Connect(2,3,true),"fixture line");
                var far=b.SpawnSoldier(2,3);var near=b.SpawnSoldier(2,3);far.Position=new Vector3(.69f,0,0);near.Position=new Vector3(.3f,0,0);
                Require(b.FindArrowSoldier(b.Tower(1))==far,"first, not nearest");
                Require(b.FireArrow(1).TargetSoldierId==far.Id,"soldier must precede eligible tower");
            });
            check("arrow-large-origin-before-small-and-endpoint-camp",()=>{
                var b=Fixture(true);b.Connect(2,3,true);b.Connect(3,2,true);
                var small=b.SpawnSoldier(2,3);var large=b.SpawnSoldier(3,2);
                small.Position=new Vector3(.3f,0,0);large.Position=new Vector3(.6f,0,0);
                Require(b.FindArrowSoldier(b.Tower(1))==large,"large-origin list is first");
                b.Tower(3).Camp=1;Require(large.Camp==3,"retained old soldier camp");
                Require(b.FindArrowSoldier(b.Tower(1))==small,"filter uses current endpoint camp");
            });
            check("arrow-cut-line-inflight-excluded",()=>{
                var b=Fixture(true);b.Connect(2,3,true);var soldier=b.SpawnSoldier(2,3);soldier.Position=new Vector3(.4f,0,0);
                b.RemoveOutgoing(2);Require(soldier.Active,"cut retains soldier");Require(b.FindArrowSoldier(b.Tower(1))==null,"direction0 excluded from arrow collector");
            });
            check("arrow-tower-selection-sampled-list-index",()=>{
                var b=Fixture(true);b.ArrowTargetIndex=count=>{Require(count==2,"two ordered candidates");return 1;};
                Require(b.FindArrowTower(b.Tower(1)).Id==3,"explicit second candidate");
            });
            check("arrow-direct-kill-ignores-hp-and-fixed-end",()=>{
                var b=Fixture(true);b.Connect(2,3,true);var soldier=b.SpawnSoldier(2,3);soldier.Position=new Vector3(.5f,0,0);soldier.HP=100000;
                var a=b.FireArrow(1);Near(a.End.x,.3f);Near(a.Start.y,.2f);
                soldier.Position=new Vector3(9,0,0);b.Tick(.2f);
                Require(!soldier.Active,"retained target killed regardless moved range/HP");Near(a.End.x,.3f);
                Require(b.FindLine(2,3).SmallSoldiers.Count==0,"immediate list removal");
            });
            check("arrow-damage-clamps-without-capture",()=>{
                var b=Fixture();b.Tower(2).Score=.5f;b.FireArrow(1);b.Tick(.2f);
                Near(b.Tower(2).Score,0);Require(b.Tower(2).Camp==2,"arrow cannot capture");
            });
            check("arrow-pause-freezes-flight-and-resume",()=>{
                var b=Fixture();var a=b.FireArrow(1);b.Tick(.1f);b.Pause(true);b.Tick(20);
                Near(a.Elapsed,.1f);Near(b.Tower(2).Score,5);b.Pause(false);b.Tick(.1f);Near(b.Tower(2).Score,4);
            });
            check("arrow-victory-does-not-cancel-retained-hit",()=>{
                var b=Fixture();b.FireArrow(1);b.ChangeScore(2,1,-6);Require(b.State==BattlePhase.Victory,"fixture victory");
                b.Tick(.2f);Near(b.Tower(2).Score,0);Require(b.Tower(2).Camp==1,"same-camp after launch is not rechecked");
            });
            check("arrow-hit-reads-current-source-camp",()=>{
                var b=Fixture();b.FireArrow(1);b.ChangeScore(1,3,-6);int hitCamp=0;
                b.Event+=e=>{if(e.Kind=="arrow-hit")hitCamp=e.Camp;};b.Tick(.2f);
                Require(hitCamp==3,"callback reads source current camp");Near(b.Tower(2).Score,4);
            });
            check("arrow-neutral-and-tree-still-attack",()=>{
                var b=Fixture(true);b.Tower(1).Camp=0;b.Tower(2).Camp=1;b.Tower(1).Mode=1;b.Tick(2);
                Require(b.Arrows.Count==1,"no extra neutral/Tree attack gate");
            });
            check("arrow-clear-retry-clock-and-retained-tower-reference",()=>{
                var b=Fixture();var old=b.Tower(2);var a=b.FireArrow(1);b.Tower(1).ArrowAccumulator=.5f;b.Restart();
                Require(ReferenceEquals(old,b.Tower(2)),"retry entity identity retained");Near(b.Tower(1).ArrowAccumulator,.5f);
                b.Tick(.2f);Require(!a.Active,"existing tween completes after retry");Near(b.Tower(2).Score,4);
            });
            return report;
        }
    }
}
