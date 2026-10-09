using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class SingleTowerProbe
    {
        [Serializable] public sealed class Sample
        {
            public string scenario; public int fps, score, basicCount, singleCount, basicHP, singleHP, basicAttack, singleAttack;
            public float basicInterval, singleInterval, basicMoveSpeed, singleMoveSpeed, basicMeasuredInterval, singleMeasuredInterval;
        }
        [Serializable] public sealed class Report { public bool passed=true;public List<Sample> samples=new List<Sample>(); }
        static Sample Measure(BattleSimulation w,int basic,int single,int fps,int score,string name)
        {
            var a=w.Tower(basic);var b=w.Tower(single);
            var sample=new Sample{scenario=name,fps=fps,score=score,basicInterval=a.SpawnTime,singleInterval=b.SpawnTime};
            var firstA=-1f;var lastA=0f;var firstB=-1f;var lastB=0f;
            w.Event+=e=>{if(e.Kind!="spawn")return;var s=w.Soldiers.Find(x=>x.Id==e.SoldierId);
                if(e.TowerId==basic){sample.basicCount++;if(firstA<0)firstA=w.Elapsed;lastA=w.Elapsed;sample.basicHP=s.HP;sample.basicAttack=s.Attack;sample.basicMoveSpeed=s.Speed;}
                if(e.TowerId==single){sample.singleCount++;if(firstB<0)firstB=w.Elapsed;lastB=w.Elapsed;sample.singleHP=s.HP;sample.singleAttack=s.Attack;sample.singleMoveSpeed=s.Speed;}
            };
            for(int i=0;i<30*fps;i++)w.Tick(1f/fps);
            sample.basicMeasuredInterval=(lastA-firstA)/Math.Max(1,sample.basicCount-1);
            sample.singleMeasuredInterval=(lastB-firstB)/Math.Max(1,sample.singleCount-1);
            if(sample.singleCount<=sample.basicCount||sample.basicHP!=sample.singleHP||sample.basicAttack!=sample.singleAttack||sample.basicMoveSpeed!=sample.singleMoveSpeed)
                throw new Exception("Unexpected single-line comparison: "+JsonUtility.ToJson(sample));
            return sample;
        }
        [Serializable] public sealed class RateSample {
            public string route;public int lanes,fps,totalCount,laneOneCount,laneTwoCount;
            public float seconds=60,configuredInterval,perLanePerSecond,totalPerSecond;
        }
        [Serializable] public sealed class RateReport {public List<RateSample> samples=new List<RateSample>();}
        public static void RunCurrentRates()
        {
            try{
                var report=new RateReport();
                foreach(int fps in new[]{60,120})foreach(int scenario in new[]{0,1,2,3}){
                    var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                        new StarInfoCfg{CampID=1,ShipID=1,StartScore=10},new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=100000}},
                        new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{z=100000}}
                    }},BattleView.ReadConfig(),1,(x,y)=>true){AIEnabled=false};
                    BattleView.ConfigureSkills(w);foreach(var t in w.Towers)t.AutoAddScore=false;
                    if(scenario>0&&!w.AdvanceTower(1,scenario==1?TowerSpecialization.Single:TowerSpecialization.Split))throw new Exception("route setup");
                    w.Connect(1,2);if(scenario==3)w.Connect(1,3);
                    var sample=new RateSample{route=scenario==0?"basic":scenario==1?"single":"split",lanes=scenario==3?2:1,fps=fps,configuredInterval=w.Tower(1).SpawnTime};
                    sample.perLanePerSecond=1/sample.configuredInterval;sample.totalPerSecond=sample.perLanePerSecond*sample.lanes;
                    w.Event+=e=>{if(e.Kind!="spawn"||e.TowerId!=1)return;sample.totalCount++;var soldier=w.Soldiers.Find(x=>x.Id==e.SoldierId);if(soldier.TargetTowerId==2)sample.laneOneCount++;else sample.laneTwoCount++;};
                    for(int i=0;i<60*fps;i++)w.Tick(1f/fps);
                    report.samples.Add(sample);
                }
                File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/current-level10-rates.json"),JsonUtility.ToJson(report,true));
                Debug.Log("CURRENT_RATE_PROBE_PASS");EditorApplication.Exit(0);
            }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
        }
        public static void RunBatch() { RunCurrentRates(); }
    }
}
