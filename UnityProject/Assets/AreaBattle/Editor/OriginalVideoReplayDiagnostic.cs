using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle.EditorTools
{
    // Diagnostic continuation, deliberately not an acceptance test. The recording
    // omits the initial simulation/RNG state; observed scores are never injected.
    public static class OriginalVideoReplayDiagnostic
    {
        [Serializable] public sealed class Input { public float pts; public string kind; public int source,target,expectedOutgoing; }
        [Serializable] public sealed class Inputs { public Input[] events; }
        [Serializable] public sealed class Reference { public float pts; public string image; public int[] scores,camps; }
        [Serializable] public sealed class Fixture { public float preRecordingSeconds,endPts; public float[] frames; public Reference[] checkpoints; }
        [Serializable] public sealed class ObservedInput { public float pts; public string kind; public bool accepted; public int source,target,sourceCamp,targetCamp,outgoing,expectedOutgoing; }
        [Serializable] public sealed class LineSnapshot { public int a,b,direction; public float smallClock,largeClock; }
        [Serializable] public sealed class SoldierSnapshot { public int source,target,camp; public Vector3 position; }
        [Serializable] public sealed class TimelineEvent { public float pts,value;public string kind;public int tower,line,camp,previousCamp,soldier; }
        [Serializable] public sealed class Timeline {public List<TimelineEvent> events=new List<TimelineEvent>();}
        [Serializable] public sealed class TowerTimingSample { public float pts,score,regen; public int outgoing,lineDirection,targetCamp; }
        [Serializable] public sealed class TowerTimingTrace { public List<TowerTimingSample> samples=new List<TowerTimingSample>(); }
        [Serializable] public sealed class Checkpoint { public float pts; public string image; public int[] originalScores,originalCamps,camps; public float[] scores; public bool exact; }
        [Serializable] public sealed class Report
        {
            public string classification="diagnostic-only",unityVersion;
            public int normalLevel=871,layout=120,seed=4305;
            public float assumedPreRecordingSeconds,assumedResumePts=.806f,victoryPts=-1;
            public string finalPhase;
            public string[] limitations={"The original begins paused after a pre-recording simulation interval.","Pre-recording duration is inferred from first blue regeneration and initial15, conditional on no earlier blue input. See regen-phase-evidence.json; rendering latency remains unknown.","Both RNG streams use fixture seed4305; original RNG, soldiers, line clocks and AI history are unknown.","Skills10/10/10 are evidence-based candidates, not original account data.","Recorded native presentation frame intervals approximate original simulation intervals; inputs are applied at first observed committed frame.","Stops before unknown later player inputs. A mismatch does not isolate a mechanics defect."};
            public List<LineSnapshot> firstCheckpointLines=new List<LineSnapshot>();
            public List<SoldierSnapshot> firstCheckpointSoldiers=new List<SoldierSnapshot>();
            public List<ObservedInput> inputs=new List<ObservedInput>();
            public List<Checkpoint> checkpoints=new List<Checkpoint>();
        }
        public static void Run()
        {
            var prior=UnityEngine.Random.state; GameObject root=null;
            try
            {
                string dir=Path.Combine(BattleBuild.Target,"generated/video-20260928");
                var fixture=JsonUtility.FromJson<Fixture>(File.ReadAllText(Path.Combine(dir,"full-replay-fixture.json")));
                var trace=JsonUtility.FromJson<Inputs>(File.ReadAllText(Path.Combine(dir,"full-input-trace.json")));
                var report=new Report{unityVersion=Application.unityVersion,assumedPreRecordingSeconds=fixture.preRecordingSeconds};
                RecoveredSoldierImporter.Import();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                root=new GameObject("Video timed replay diagnostic");var view=root.AddComponent<BattleView>();
                view.OrdinarySoldierSkinId=102;
                string profile=Path.Combine(dir,"diagnostic-loadout.json");
                File.WriteAllText(profile,JsonUtility.ToJson(BattleLoadout.CreateFixture(871,1,10,0,27),true));
                view.Loadout=BattleLoadout.LoadOrCreate(profile);
                UnityEngine.Random.InitState(report.seed);view.RandomSourceOverride=new System.Random(report.seed);
                view.InitializeNormalLevel(871);
                view.BattleCamera.aspect=.5625f;view.BattleCamera.orthographicSize=2.1f;
                view.SynchronizeBackground(.5625f);Canvas.ForceUpdateCanvases();
                if(!view.Initialized||!view.Simulation.AIEnabled)throw new Exception("Production scene or AI unavailable");
                var timeline=new Timeline();float eventPts=-report.assumedPreRecordingSeconds;
                view.Simulation.Event+=e=>timeline.events.Add(new TimelineEvent{pts=eventPts,kind=e.Kind,tower=e.TowerId,line=e.LineId,camp=e.Camp,previousCamp=e.PreviousCamp,soldier=e.SoldierId,value=e.Value});
                float warmRemaining=report.assumedPreRecordingSeconds;
                while(warmRemaining>0){float step=Mathf.Min(1f/60,warmRemaining);eventPts=-warmRemaining+step;view.AdvanceFrame(step,step);warmRemaining-=step;}
                int nextInput=0,nextCheckpoint=0,nextSkill=0;
                var tower6Timing=new TowerTimingTrace();
                bool resultCaptured=false;
                float[] casts={36.08522f,52.37694f,62.29858f};
                float previous=0;
                foreach(float pts in fixture.frames)
                {
                    if(pts>fixture.endPts+.0001f)break;
                    eventPts=pts;
                    float dt=Mathf.Max(0,pts-Mathf.Max(previous,report.assumedResumePts));previous=pts;
                    while(nextInput<trace.events.Length&&trace.events[nextInput].pts<=pts+.00001f)
                    {
                        var e=trace.events[nextInput++];var line=view.Simulation.FindLine(e.source,e.target);
                        bool accepted=e.kind=="connect"?view.Simulation.Connect(e.source,e.target):line!=null&&view.Simulation.CutPlayerLine(line.Id);
                        report.inputs.Add(new ObservedInput{pts=pts,kind=e.kind,source=e.source,target=e.target,sourceCamp=view.Simulation.Tower(e.source).Camp,targetCamp=view.Simulation.Tower(e.target).Camp,accepted=accepted,outgoing=view.Simulation.Tower(e.source).OutgoingCount,expectedOutgoing=e.expectedOutgoing});
                    }
                    while(nextSkill<casts.Length&&casts[nextSkill]<=pts+.00001f)
                    {
                        int slot=nextSkill++;bool accepted=view.TryUseSkillSlot(slot,slot==2?7:0);
                        report.inputs.Add(new ObservedInput{pts=pts,kind="skill"+(slot+1),target=slot==2?7:0,accepted=accepted});
                    }
                    if(dt>0)view.AdvanceFrame(dt,dt);
                    if(pts>=58f&&pts<=70f)
                    {
                        var t=view.Simulation.Tower(6);var line=view.Simulation.FindLine(6,7);
                        tower6Timing.samples.Add(new TowerTimingSample{pts=pts,score=t.Score,regen=t.RegenAccumulator,outgoing=t.OutgoingCount,lineDirection=line.Direction,targetCamp=view.Simulation.Tower(7).Camp});
                    }
                    if(report.victoryPts<0&&view.Simulation.State==BattlePhase.Victory){report.victoryPts=pts;Capture(view,Path.Combine(dir,"full-replay-victory.png"));}
                    if(!resultCaptured&&report.victoryPts>=0&&pts>=report.victoryPts+.7f){Capture(view,Path.Combine(dir,"full-replay-result-plus700ms.png"));resultCaptured=true;}
                    while(nextCheckpoint<fixture.checkpoints.Length&&fixture.checkpoints[nextCheckpoint].pts<=pts+.00001f)
                    {
                        var reference=fixture.checkpoints[nextCheckpoint++];
                        var c=new Checkpoint{pts=pts,image=reference.image,originalScores=reference.scores,originalCamps=reference.camps,scores=new float[7],camps=new int[7],exact=true};
                        for(int i=0;i<7;i++){var t=view.Simulation.Tower(i+1);c.scores[i]=t.Score;c.camps[i]=t.Camp;if(t.Camp!=c.originalCamps[i]||Mathf.Abs(t.Score-c.originalScores[i])>.001f)c.exact=false;}
                        report.checkpoints.Add(c);
                        Capture(view,Path.Combine(dir,"full-replay-checkpoint-"+report.checkpoints.Count.ToString("00")+".png"));
                        // Independent diagnostic hypothesis: the supplied entry capture has a
                        // 723x1282 viewport. Video encoding does not establish its source viewport.
                        // Preserve the canonical720x1280 replay and the original gameplay fixture.
                        string aspectDir=Path.Combine(dir,"entry-aspect-candidate");Directory.CreateDirectory(aspectDir);
                        Capture(view,Path.Combine(aspectDir,"checkpoint-"+report.checkpoints.Count.ToString("00")+".png"),723,1282);
                        if(report.checkpoints.Count==1)
                        {
                            Capture(view,Path.Combine(dir,"full-replay-first-checkpoint.png"));
                            foreach(var line in view.Simulation.Lines)if(line.Direction!=0)
                                report.firstCheckpointLines.Add(new LineSnapshot{a=line.SmallTowerId,b=line.LargeTowerId,direction=line.Direction,smallClock=line.SmallSpawnTimer,largeClock=line.LargeSpawnTimer});
                            foreach(var soldier in view.Simulation.Soldiers)if(soldier.Active)
                                report.firstCheckpointSoldiers.Add(new SoldierSnapshot{source=soldier.SourceTowerId,target=soldier.TargetTowerId,camp=soldier.Camp,position=soldier.Position});
                        }
                    }
                }
                Capture(view,Path.Combine(dir,"full-replay-final-frame.png"));
                report.finalPhase=view.Simulation.State.ToString();
                report.limitations[5]="All22 observed line-state changes and3 skills replayed through the video end. Unknown initial session state means a mismatch does not isolate a mechanics defect.";
                File.WriteAllText(Path.Combine(dir,"full-replay-diagnostic.json"),JsonUtility.ToJson(report,true));
                File.WriteAllText(Path.Combine(dir,"full-replay-events.json"),JsonUtility.ToJson(timeline,true));
                File.WriteAllText(Path.Combine(dir,"tower6-replay-timing.json"),JsonUtility.ToJson(tower6Timing,true));
                Debug.Log("VIDEO_TIMED_DIAGNOSTIC_COMPLETE checkpoints="+report.checkpoints.Count);
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception e){Debug.LogException(e);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);UnityEngine.Random.state=prior;Time.timeScale=1;}
        }
        static void Capture(BattleView view,string path,int width=720,int height=1280)
        {
            var rt=new RenderTexture(width,height,24);var prior=RenderTexture.active;
            var camera=view.BattleCamera;float priorAspect=camera.aspect,priorSize=camera.orthographicSize;
            camera.targetTexture=rt;view.Hud.UICamera.targetTexture=rt;
            camera.aspect=width/(float)height;camera.orthographicSize=2.1f*.5625f/camera.aspect;
            view.SynchronizeBackground(camera.aspect);
            Canvas.ForceUpdateCanvases();view.Hud.Synchronize();
            // Offscreen resizing updates CanvasScaler, but unchanged Text can retain
            // a glyph mesh generated at the editor's old GameView resolution.
            // This is capture-only; source font assets and runtime HUD settings are retained.
            foreach(var text in view.Hud.Canvas.GetComponentsInChildren<Text>())text.SetAllDirty();
            Canvas.ForceUpdateCanvases();camera.Render();view.Hud.UICamera.Render();
            RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            camera.targetTexture=null;view.Hud.UICamera.targetTexture=null;RenderTexture.active=prior;
            camera.aspect=priorAspect;camera.orthographicSize=priorSize;view.SynchronizeBackground(priorAspect);
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
