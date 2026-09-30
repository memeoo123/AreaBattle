using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OriginalAITimingValidation
    {
        [Serializable] sealed class Case {public string id;public float delay,timer,dt,interval,expectedDelay,expectedTimer;public int expectedActions;}
        [Serializable] sealed class Oracle {public Case[] cases;}
        sealed class ClockRandom:System.Random
        {public int calls;public override int Next(int maxValue){calls++;return 0;}}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original unaltered AICamp.Updata body with explicit config/action stubs versus production scheduler; not original RNG/AI policy equivalence."};
            var oracle=JsonUtility.FromJson<Oracle>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/original-ai-timer-cases.json")));
            foreach(var c in oracle.cases)
            {
                try
                {
                    var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_120"));
                    layout.CampInfoCfgs=new[]{new CampInfoCfg{CampID=2,AIGrade=99}};
                    var config=BattleView.ReadConfig();config.AIs=new[]{new AIConfig{id=99,DelayTime=0,ActionTime=new[]{(int)Math.Round(c.interval*1000)},ActionNum=0}};
                    var random=new ClockRandom();var sim=new BattleSimulation(layout,config,4305,(a,b)=>true,random);
                    var clocks=(IList)typeof(BattleSimulation).GetField("aiClocks",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sim);
                    if(clocks.Count!=1)throw new Exception("Oracle fixture requires exactly one AI clock");
                    object clock=clocks[0];var delay=clock.GetType().GetField("Delay");var timer=clock.GetType().GetField("Timer");
                    delay.SetValue(clock,c.delay);timer.SetValue(clock,c.timer);random.calls=0;
                    sim.Tick(c.dt);
                    float actualDelay=(float)delay.GetValue(clock),actualTimer=(float)timer.GetValue(clock);
                    if(Math.Abs(actualDelay-c.expectedDelay)>1e-7f||Math.Abs(actualTimer-c.expectedTimer)>1e-7f||random.calls!=c.expectedActions)
                        throw new Exception($"Original scheduler mismatch: delay={actualDelay},timer={actualTimer},actions={random.calls}");
                    report.checks.Add(new BattleBuild.Check{id="original-ai-timer-"+c.id,result="pass"});
                }
                catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id="original-ai-timer-"+c.id,result="fail",detail=ex.ToString()});}
            }
            return report;
        }
    }
}
