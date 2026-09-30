using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    // A reconstruction integration replay, not an original-game oracle or visual match.
    // Every state transition below is produced by normal public battle inputs and Tick.
    public static class RepresentativeReplayValidation
    {
        [Serializable] public sealed class InputRecord { public float time; public string kind; public int source,target,line; public bool accepted; }
        [Serializable] public sealed class EventRecord
        {
            public float time,value;public string kind,phase;public int tower,line,soldier,arrow,camp,previousCamp;
            public EventRecord(BattleSimulation b,BattleEvent e){time=b.Elapsed;value=e.Value;kind=e.Kind;phase=e.Phase.ToString();tower=e.TowerId;line=e.LineId;soldier=e.SoldierId;arrow=e.ArrowId;camp=e.Camp;previousCamp=e.PreviousCamp;}
        }
        [Serializable] public sealed class TowerRecord { public int id,camp; public float score; }
        [Serializable] public sealed class PairRecord { public int id,a,b; public float length; }
        [Serializable] public sealed class Replay
        {
            public string scenario,result;
            public int layout=5,seed=4305,policy;
            public float dt=.05f,duration;
            public List<InputRecord> inputs=new List<InputRecord>();
            public List<EventRecord> events=new List<EventRecord>();
            public List<TowerRecord> initial=new List<TowerRecord>(),final=new List<TowerRecord>();
            public List<PairRecord> physicsPairs=new List<PairRecord>();
            public string scope="Production BattleView.InitializeScene(5), Physics.OverlapCapsule topology, normal legal input only. Source-derived reconstruction, not original replay.";
        }
        [Serializable] sealed class Bundle { public bool passed; public List<Replay> replays=new List<Replay>(); public string[] limitations={"No live-original synchronized trace or image comparison.","One ordinary layout validates a connected slice, not full gameplay coverage."}; }
        static void Require(bool condition,string message) { if(!condition)throw new Exception(message); }
        static List<TowerRecord> Snapshot(BattleSimulation b)
        {var s=new List<TowerRecord>();foreach(var t in b.Towers)s.Add(new TowerRecord{id=t.Id,camp=t.Camp,score=t.Score});return s;}
        static Replay Begin(BattleSimulation b,string name,int policy=0)
        {
            var r=new Replay{scenario=name,policy=policy,initial=Snapshot(b)};
            foreach(var line in b.Lines)r.physicsPairs.Add(new PairRecord{id=line.Id,a=line.SmallTowerId,b=line.LargeTowerId,length=line.Length});
            return r;
        }
        static void Finish(Replay r,BattleSimulation b) {r.result=b.State.ToString();r.duration=b.Elapsed;r.final=Snapshot(b);}
        static void RecordConnect(BattleSimulation b,Replay r,int a,int c)
        {r.inputs.Add(new InputRecord{time=b.Elapsed,kind="connect",source=a,target=c,accepted=b.Connect(a,c)});}
        static void RecordCut(BattleSimulation b,Replay r,LineState line)
        {r.inputs.Add(new InputRecord{time=b.Elapsed,kind="cut",line=line.Id,accepted=b.CutPlayerLine(line.Id)});}
        // Input policy chooses a single frontier tower, then routes friendly towers
        // along valid shortest paths to it. This is test automation, not game AI.
        static int Plan(BattleSimulation b,Replay r,int priorFocus,int policy)
        {
            var candidates=new List<TowerState>();
            foreach(var t in b.Towers)if(t.Active&&t.Camp!=BattleSimulation.PlayerCampID)candidates.Add(t);
            if(candidates.Count==0)return 0;
            var focus=b.Tower(priorFocus);
            if(focus==null||!focus.Active||focus.Camp==BattleSimulation.PlayerCampID)
            {
                candidates.Sort((a,c)=>{
                    float sa=Priority(b,a,policy),sc=Priority(b,c,policy);
                    int d=sa.CompareTo(sc);return d!=0?d:a.Id.CompareTo(c.Id);
                });
                focus=candidates[0];
            }
            var distance=new Dictionary<int,float>();var next=new Dictionary<int,int>();var done=new HashSet<int>();
            foreach(var t in b.Towers)if(t.Active&&(t.Camp==BattleSimulation.PlayerCampID||t.Id==focus.Id))distance[t.Id]=float.PositiveInfinity;
            distance[focus.Id]=0;
            while(true)
            {
                int nearest=0;float best=float.PositiveInfinity;
                foreach(var entry in distance)if(!done.Contains(entry.Key)&&entry.Value<best){nearest=entry.Key;best=entry.Value;}
                if(nearest==0)break;done.Add(nearest);
                foreach(var line in b.GetPotentialLines(nearest))
                {
                    int other=line.Other(nearest);if(!line.Active||!distance.ContainsKey(other)||done.Contains(other))continue;
                    float d=best+line.Length;if(d<distance[other]){distance[other]=d;next[other]=nearest;}
                }
            }
            // Cut old player directions through the normal cut operation, preserving
            // any opposing half-direction and all soldiers already travelling.
            foreach(var line in b.Lines)
            {
                bool unwanted=false;
                foreach(int source in new[]{line.SmallTowerId,line.LargeTowerId})
                    if(b.Tower(source).Camp==BattleSimulation.PlayerCampID&&line.IsFrom(source)&&
                        (!next.TryGetValue(source,out int destination)||destination!=line.Other(source)))unwanted=true;
                if(unwanted)RecordCut(b,r,line);
            }
            foreach(var t in b.Towers)
                if(t.Active&&t.Camp==BattleSimulation.PlayerCampID&&next.TryGetValue(t.Id,out int target))
                {var line=b.FindLine(t.Id,target);if(line!=null&&!line.IsFrom(t.Id))RecordConnect(b,r,t.Id,target);}
            return focus.Id;
        }
        static float Priority(BattleSimulation b,TowerState target,int policy)
        {
            float nearest=float.PositiveInfinity;
            foreach(var t in b.Towers)if(t.Active&&t.Camp==BattleSimulation.PlayerCampID)
                foreach(var line in b.GetPotentialLines(t.Id))if(line.Other(t.Id)==target.Id)nearest=Mathf.Min(nearest,line.Length);
            if(float.IsPositiveInfinity(nearest))return 100000+target.Id;
            if(policy==1)return nearest*30+target.Score;
            if(policy==2)return (target.Camp==0?100:0)+nearest*10+target.Score;
            return target.Score+nearest*5;
        }
        static void Advance(BattleView view,float dt)
        {view.AdvanceFrame(dt,dt);}
        static bool Win(BattleView view,Replay r)
        {
            var b=view.Simulation;
            int focus=0;
            for(int step=0;step<12000&&b.State==BattlePhase.Running;step++)
            {if(step%10==0)focus=Plan(b,r,focus,r.policy);Advance(view,r.dt);}
            Finish(r,b);return b.State==BattlePhase.Victory;
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Natural-input connected replay on layout5 and production Physics topology. Does not establish live-original temporal or visual equivalence, special modes or all layouts."};
            var bundle=new Bundle();var obj=new GameObject("Representative replay validation");
            var priorRandomState=UnityEngine.Random.state;
            UnityEngine.Random.InitState(4305); // Explicit fixture seed for the separate Unity stream.
            try
            {
                var view=obj.AddComponent<BattleView>();view.Seed=4305;view.RandomSourceOverride=new System.Random(4305);view.CommanderMode=1;view.RequestedNormalLevel=-1;
                view.InitializeScene(5);Require(view.Initialized,"Production layout5 scene failed initialization");
                var b=view.Simulation;Require(b.AIEnabled,"AI must remain enabled");Require(b.Lines.Count>0,"Physics topology empty");
                for(int slot=0;slot<3;slot++)Require(!view.SkillInput.Unlocked(slot),"Normal5 commander1 skills must be locked");
                report.checks.Add(new BattleBuild.Check{id="representative-production-topology-and-locked-skills",result="pass",detail="layout5; physicsPairs="+b.Lines.Count});
                var smoke=Begin(b,"legal-cut-pause");bundle.replays.Add(smoke);
                Action<BattleEvent> observe=e=>smoke.events.Add(new EventRecord(b,e));b.Event+=observe;
                var friendly=b.FindLine(3,4);Require(friendly!=null,"fixture friendly pair absent");
                RecordConnect(b,smoke,3,4);Require(friendly.IsFrom(3),"normal connect rejected");
                RecordCut(b,smoke,friendly);Require(!friendly.IsFrom(3),"normal cut rejected");
                var before=Snapshot(b);float elapsed=b.Elapsed;b.Pause(true);smoke.inputs.Add(new InputRecord{kind="pause",accepted=true,time=b.Elapsed});
                view.AdvanceFrame(0,3);Require(b.State==BattlePhase.Pause&&b.Elapsed==elapsed,"Pause advanced battle");
                for(int i=0;i<before.Count;i++)Require(before[i].score==b.Towers[i].Score,"Pause changed score");
                b.Pause(false);smoke.inputs.Add(new InputRecord{kind="resume",accepted=true,time=b.Elapsed});Finish(smoke,b);b.Event-=observe;
                report.checks.Add(new BattleBuild.Check{id="representative-legal-cut-and-pause",result="pass"});
                Replay victory=null;
                for(int policy=0;policy<3;policy++)
                {
                    if(policy>0)view.RestartCurrentLevel();
                    var attempt=Begin(b,"natural-victory",policy);bundle.replays.Add(attempt);
                    Action<BattleEvent> log=e=>attempt.events.Add(new EventRecord(b,e));b.Event+=log;
                    bool won=Win(view,attempt);b.Event-=log;if(won){victory=attempt;break;}
                }
                Require(victory!=null,"No legal input policy achieved natural victory within600 seconds");
                report.checks.Add(new BattleBuild.Check{id="representative-natural-victory",result="pass",detail="policy="+victory.policy+";seconds="+victory.duration+";inputs="+victory.inputs.Count});
                for(int frame=0;frame<120;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                BattleBuild.Capture(view,"ordinary5-natural-victory.png");
                view.RestartCurrentLevel();var retry=Begin(b,"retry-then-idle-observation");bundle.replays.Add(retry);
                Require(b.State==BattlePhase.Running&&b.Elapsed==0,"Retry did not restore Running clock");
                for(int i=0;i<retry.initial.Count;i++)Require(retry.initial[i].camp==victory.initial[i].camp&&retry.initial[i].score==victory.initial[i].score,"Retry changed serialized initial state");
                foreach(var line in b.Lines)Require(line.Direction==0,"Retry retained direction");
                Require(b.Soldiers.Count==0,"Retry retained active soldier list");
                report.checks.Add(new BattleBuild.Check{id="representative-retry-original-state",result="pass"});
                Action<BattleEvent> idle=e=>retry.events.Add(new EventRecord(b,e));b.Event+=idle;
                for(int step=0;step<3600&&b.State==BattlePhase.Running;step++)Advance(view,retry.dt);
                b.Event-=idle;Finish(retry,b);
                report.checks.Add(new BattleBuild.Check{id="representative-idle-observation",result="pass",detail="state="+b.State+";seconds="+retry.duration+"; AIType0 camp3 excludes player targets, so idle defeat is not assumed"});
                view.RestartCurrentLevel();var defeat=Begin(b,"legal-eliminate-camp3-then-stop-defending");bundle.replays.Add(defeat);
                Action<BattleEvent> loss=e=>defeat.events.Add(new EventRecord(b,e));b.Event+=loss;
                bool stopped=false;
                for(int step=0;step<18000&&b.State==BattlePhase.Running;step++)
                {
                    if(step%10==0&&!stopped)
                    {
                        TowerState target=null;
                        foreach(var t in b.Towers)if(t.Active&&t.Camp==3&&(target==null||t.Score<target.Score))target=t;
                        if(target!=null)Plan(b,defeat,target.Id,0);
                        else
                        {
                            foreach(var line in b.Lines)
                                if((b.Tower(line.SmallTowerId).Camp==1&&line.IsFrom(line.SmallTowerId))||(b.Tower(line.LargeTowerId).Camp==1&&line.IsFrom(line.LargeTowerId)))RecordCut(b,defeat,line);
                            stopped=true;
                        }
                    }
                    Advance(view,defeat.dt);
                }
                b.Event-=loss;Finish(defeat,b);Require(b.State==BattlePhase.Defeat,"Legal non-defending strategy did not reach defeat within900 seconds");
                report.checks.Add(new BattleBuild.Check{id="representative-natural-defeat-after-legal-input",result="pass",detail="seconds="+defeat.duration+";inputs="+defeat.inputs.Count});
                for(int frame=0;frame<120;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                BattleBuild.Capture(view,"ordinary5-natural-defeat.png");
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="representative-connected-replay",result="fail",detail=e.ToString()});}
            finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;UnityEngine.Random.state=priorRandomState;}
            bundle.passed=report.passed;
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/representative-replay-trace.json"),JsonUtility.ToJson(bundle,true));
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/representative-replay-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
    }
}
