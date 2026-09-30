using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    // Isolated observed checkpoints, not a replay of the unknown original RNG/initial clocks.
    // Provenance: generated/video-20260928/original-video-evidence.json.
    public static class OriginalVideoValidation
    {
        [Serializable] sealed class VideoSteps { public float[] deltas; }
        [Serializable] sealed class VideoInput { public float pts; public string kind; public int source,target,expectedOutgoing; }
        [Serializable] sealed class VideoInputs { public VideoInput[] events; }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        static BattleSimulation Checkpoint()
        {
            var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_120"));
            layout.StarInfoCfgs[4].CampID=BattleSimulation.PlayerCampID;
            layout.StarInfoCfgs[6].StartScore=45;
            var sim=new BattleSimulation(layout,BattleView.ReadConfig(),4305,(a,b)=>true){AIEnabled=false};
            BattleView.ConfigureSkills(sim);
            foreach(var tower in sim.Towers)tower.AutoAddScore=false;
            return sim;
        }
        static BattleSkillInput Input(BattleSimulation sim,int fireLevel,int stock)
        { return new BattleSkillInput(sim,BattleView.ReadText("Data/AllSkillConfig"),1,871,
            new[]{10,fireLevel,10},new Dictionary<int,int>(),stock); }

        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Video checkpoint tests; skill levels are inferred candidates. No original profile is read or changed; no synchronized full-run claim."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("video871-ice-four-enemies-fourteen-second-candidate",()=>{
                var sim=Checkpoint();var input=Input(sim,1,27);var events=new List<SkillVisualEvent>();sim.SkillVisual+=events.Add;
                Require(input.TryUse(0)&&input.GenericStock==26,"PTS36.08522: ice spends one generic point");
                Require(sim.Towers.Where(t=>t.Mode==1).Select(t=>t.Id).SequenceEqual(new[]{3,4,6,7}),"video freezes the four enemy towers; captured blue tower5 stays unfrozen");
                sim.Tick(13.99f);Require(sim.IsSkillActive(1)&&sim.Tower(7).Mode==1,"inferred14-second candidate remains active before boundary");
                sim.Tick(.02f);Require(!sim.IsSkillActive(1)&&sim.Towers.All(t=>t.Mode==0),"mechanical freeze ends near inferred audio melt onset50.086");
                Require(events.Count(e=>e.Kind=="tower-ice-melt")==4&&events.Count(e=>e.Kind=="audio-play"&&e.AudioId==2018)==1,"four separate melt animations with one source sound");
            });
            check("video871-lightning-f3731-f3732-zero-without-capture",()=>{
                var sim=Checkpoint();var input=Input(sim,1,25);var tower=sim.Tower(7);int camp=tower.Camp;
                Require(tower.Score==45&&camp!=BattleSimulation.PlayerCampID,"original pre-cast checkpoint");
                Require(input.TryUse(2,7)&&input.GenericStock==24,"PTS62.29858: accepted drag spends one point");
                Require(tower.Score==0&&tower.Camp==camp,"45 to0 immediately, same green camp; damage does not capture");
                Require(tower.Grade==0&&sim.State==BattlePhase.Running,"source tower model downgrades without ending battle");
                Require(!input.TryUse(2,7)&&input.GenericStock==24,"active skill cannot consume a duplicate point");
            });
            check("video871-three-tool-costs-independent-of-unknown-fire-level",()=>{
                for(int fireLevel=1;fireLevel<=10;fireLevel++)
                {
                    var sim=Checkpoint();var input=Input(sim,fireLevel,27);
                    Require(input.TryUse(0)&&input.GenericStock==26,"ice27to26");
                    Require(input.TryUse(1,projectileOrigin:Vector3.zero)&&input.GenericStock==25,"fire26to25 at every unresolved level");
                    Require(input.TryUse(2,7)&&input.GenericStock==24,"lightning25to24");
                }
            });
            check("video871-result-commander-hides-before-skill-panel-fade",()=>{
                var root=new GameObject("Original video result transition");
                try
                {
                    var view=root.AddComponent<BattleView>();view.InitializeNormalLevel(871);
                    var panel=view.Hud.Canvas.transform.Find("SkillUI").gameObject;
                    var group=panel.GetComponent<CanvasGroup>();var prior=view.Hud.Commander;
                    foreach(var tower in view.Simulation.Towers.Where(t=>t.Camp!=BattleSimulation.PlayerCampID).ToArray())
                        view.Simulation.ChangeScore(tower.Id,BattleSimulation.PlayerCampID,-tower.Score-1);
                    view.Hud.Synchronize();
                    Require(!prior.gameObject.activeInHierarchy&&panel.activeInHierarchy,"original frame5156: commander hidden while skill graphics remain");
                    Require(!group.blocksRaycasts&&!group.interactable,"closing graphics cannot accept input");
                    view.Hud.AdvancePresentation(.1f);
                    Require(panel.activeInHierarchy&&Mathf.Abs(group.alpha-4f/9)<.001f,"original .3-second OutQuad fade, distinct from instant closure");
                    view.Hud.AdvancePresentation(.21f);Require(!panel.activeInHierarchy,"skill panel closes after source fade duration");
                    view.RestartCurrentLevel();view.Hud.Synchronize();
                    Require(panel.activeInHierarchy&&group.alpha==1&&group.blocksRaycasts&&group.interactable,"retry resets fade and input state");
                    Require(!ReferenceEquals(prior,view.Hud.Commander)&&view.Hud.Commander.gameObject.activeInHierarchy,"retry still creates fresh visible commander");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
            });
            check("video871-fire25-recorded-frame-step-candidate",()=>{
                var sim=Checkpoint();var input=Input(sim,10,26);int launches=0;
                sim.SkillVisual+=e=>{if(e.Kind=="projectile-spawn"&&e.SkillId==2)launches++;};
                Require(input.TryUse(1,projectileOrigin:Vector3.zero),"recorded fire input accepted");
                var steps=JsonUtility.FromJson<VideoSteps>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/video-20260928/fire-replay-steps.json")));
                foreach(float dt in steps.deltas){sim.TickSkillCoroutines(dt);sim.Tick(dt);}
                Require(launches==25,"25 distinct original fire heads, candidate level10; production launches="+launches);
                Require(input.GenericStock==25,"one point spent for entire burst");
            });
            check("video871-seven-committed-line-inputs-capacity-sequence",()=>{
                var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_120"));
                layout.StarInfoCfgs[0].StartScore=layout.StarInfoCfgs[1].StartScore=19;
                var sim=new BattleSimulation(layout,BattleView.ReadConfig(),4305,(a,b)=>true){AIEnabled=false};
                var trace=JsonUtility.FromJson<VideoInputs>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/video-20260928/early-input-trace.json")));
                Require(trace.events.Length==7,"seven observed committed operations");
                // Validate topology/counts only. Do not invent the missing pre-recording soldiers or clocks.
                foreach(var action in trace.events)
                {
                    bool accepted=action.kind=="connect"?sim.Connect(action.source,action.target):sim.CutPlayerLine(sim.FindLine(action.source,action.target).Id);
                    Require(accepted,"observed operation accepted at PTS"+action.pts);
                    Require(sim.Tower(action.source).OutgoingCount==action.expectedOutgoing,"capacity agrees with native-frame filled circles at PTS"+action.pts);
                }
                Require(sim.FindLine(1,2).IsFrom(2)&&!sim.FindLine(1,2).IsFrom(1),"last horizontal connection points2to1");
            });
            check("video871-soldier102-cosmetic-binding-preserves-mechanics",()=>{
                var root=new GameObject("Video soldier cosmetic validation");
                try
                {
                    var view=root.AddComponent<BattleView>();view.OrdinarySoldierSkinId=102;
                    view.RandomSourceOverride=new System.Random(4305);view.InitializeNormalLevel(871);
                    view.Simulation.AIEnabled=false;
                    Require(view.Simulation.Connect(1,5),"controlled legal line must be accepted");
                    Require(view.Simulation.Connect(7,3,true),"controlled green line must be accepted");
                    for(int i=0;i<120;i++)view.AdvanceFrame(1f/60,1f/60);
                    int visible=0;
                    foreach(var soldier in view.Simulation.Soldiers.Where(s=>s.Active&&s.ShipType==1))
                    {
                        var obj=view.SoldierPresentationTransform(soldier.Id);Require(obj!=null,"active soldier visual missing");
                        Require(obj.GetComponent<MeshFilter>().sharedMesh.name==(soldier.Camp==3?"soldier_100":"soldier_102"),"video green soldiers retain round heads while blue/red use102");
                        Require(soldier.Attack==1&&soldier.HP==1&&soldier.Occupy==1&&soldier.Reinforce==1,"skin must not change ordinary combat attributes");visible++;
                    }
                    Require(visible>0,"fixture must exercise actual spawned soldiers");
                    view.RestartCurrentLevel();Require(view.OrdinarySoldierSkinId==102,"retry preserves local cosmetic selection");
                }
                finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
            });
            return report;
        }
    }
}
