using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OriginalReferenceValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,action)=>{try{action();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            for(int commander=1;commander<=6;commander++)
            {
                int selected=commander;
                check("original-commander-"+selected+"-animation-and-pause",()=>{
                    var root=new GameObject("Original commander validation");
                    try {
                        var view=root.AddComponent<BattleView>();view.CommanderMode=selected;view.InitializeNormalLevel(871);
                        Require(view.Initialized,"production view initialized");var animation=view.Hud.Commander;
                        Require(animation!=null&&animation.Skeleton!=null,"original skeleton bound");
                        Require(animation.Skeleton.Data.FindAnimation("idle")!=null&&animation.Skeleton.Data.FindAnimation("skill")!=null,"both source animations present");
                        Require(animation.GetComponent<Renderer>().sortingLayerName=="UIPopup","original renderer sorting layer retained");
                        var initialIdle=animation.AnimationState.GetCurrent(0);
                        float idleDuration=animation.Skeleton.Data.FindAnimation("idle").Duration;
                        view.Hud.AdvancePresentation(idleDuration*2+.05f);
                        Require(ReferenceEquals(initialIdle,animation.AnimationState.GetCurrent(0)),"idle track survives loops before the first source UseSkill event");
                        Require(view.Simulation.CastSkill(1,1),"production skill event accepted");
                        Require(animation.AnimationState.GetCurrent(0).Animation.Name=="skill","production event starts the original cast animation");
                        var firstCast=animation.AnimationState.GetCurrent(0);
                        Require(view.Simulation.CastSkill(4,1),"overlapping production skill accepted");
                        Require(ReferenceEquals(firstCast,animation.AnimationState.GetCurrent(0)),"busy commander does not restart its current cast");
                        float duration=animation.Skeleton.Data.FindAnimation("skill").Duration;
                        view.Hud.AdvancePresentation(duration+.05f);
                        Require(animation.AnimationState.GetCurrent(0).Animation.Name=="idle","source completion returns to idle");
                        float elapsed=animation.AnimationState.GetCurrent(0).TrackTime;
                        view.Simulation.Pause(true);view.Hud.AdvancePresentation(0);
                        Require(animation.AnimationState.GetCurrent(0).TrackTime==elapsed,"scaled presentation freezes on pause");
                    }finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
                });
            }
            check("original871-initial-layout-and-entry-presentation",()=>{
                var root=new GameObject("Original screenshot validation");
                var rt=new RenderTexture(723,1282,24);
                try {
                    var view=root.AddComponent<BattleView>();view.InitializeNormalLevel(871);Require(view.Initialized&&view.LevelId==120,"871 binds recovered layout120");
                    Require(!view.BattleCamera.allowHDR&&!view.BattleCamera.allowMSAA,"source GameCamera46 disables HDR/MSAA; HDR clips transparent ice highlights after blending");
                    view.BattleCamera.targetTexture=rt;view.Hud.UICamera.targetTexture=rt;
                    view.BattleCamera.aspect=723f/1282;view.BattleCamera.orthographicSize=2.1f*.5625f/view.BattleCamera.aspect;
                    view.SynchronizeBackground(view.BattleCamera.aspect);Canvas.ForceUpdateCanvases();view.Hud.Synchronize();
                    // Pixel anchors read from user entry screenshot, excluding 55px WeChat title bar.
                    var positions=new[]{new Vector2(139,384),new Vector2(585,384),new Vector2(276,600),new Vector2(139,730),new Vector2(453,600),new Vector2(591,730),new Vector2(361,778)};
                    for(int i=0;i<positions.Length;i++)
                    {
                        var score=view.Hud.Canvas.transform.Find("StarInfoRoot/TowerCanvas_"+(i+1)+"/ScoreNum");
                        var p=RectTransformUtility.WorldToScreenPoint(view.Hud.UICamera,score.position);p.y=1282-p.y;
                        Require(Vector2.Distance(p,positions[i])<4,"reference score anchor "+(i+1)+" differs: "+p);
                    }
                    Require(view.SkillPresentation.ActiveVisualCount==2,"entry shows both player tower highlights without player input");
                    Require(view.Simulation.Towers.Count()==7,"seven source towers");
                }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(rt);Time.timeScale=1;}
            });
            check("original871-retry-replays-player-camp-highlight",()=>{
                var root=new GameObject("Original retry presentation validation");
                try {
                    var view=root.AddComponent<BattleView>();view.InitializeNormalLevel(871);
                    view.SkillPresentation.Step(3.1f);
                    Require(view.SkillPresentation.ActiveVisualCount==0,"initial source highlight expires");
                    var simulation=view.Simulation;
                    view.Simulation.Pause(true);view.RestartCurrentLevel();
                    Require(ReferenceEquals(simulation,view.Simulation),"retry retains source simulation and tower entities");
                    Require(view.Simulation.State==BattlePhase.Running,"retry resumes non-guide combat");
                    Require(view.SkillPresentation.ActiveVisualCount==2,"retry recreates both player highlights");
                    view.SkillPresentation.Step(3.1f);
                    Require(view.SkillPresentation.ActiveVisualCount==0,"retry highlights retain original three-second lifetime");
                }finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
            });
            foreach(bool win in new[]{true,false})
            {
                bool victory=win;
                check("original-result-"+(win?"victory":"defeat")+"-closes-battle-hud-and-reopens",()=>{
                    var root=new GameObject("Original result UI lifecycle validation");
                    try {
                        var view=root.AddComponent<BattleView>();view.InitializeNormalLevel(871);
                        var sim=view.Simulation;sim.AIEnabled=false;
                        Require(sim.CastSkill(1,1),"source skill starts before result");
                        var prior=view.Hud.Commander;var track=prior.AnimationState.GetCurrent(0);
                        int camp=victory?BattleSimulation.PlayerCampID:2;
                        foreach(var tower in sim.Towers.Where(t=>t.Camp!=camp).ToArray())sim.ChangeScore(tower.Id,camp,-tower.Score-1);
                        Require(sim.State==(victory?BattlePhase.Victory:BattlePhase.Defeat),"production capture produces expected result");
                        view.Hud.AdvancePresentation(.35f);
                        foreach(string name in new[]{"PlayTopBar","StarInfoRoot","SkillUI"})
                            Require(!view.Hud.Canvas.transform.Find(name).gameObject.activeInHierarchy,"source result closes "+name);
                        Require(!prior.gameObject.activeInHierarchy&&track.TrackTime==0,"closed commander is neither visible nor advancing");
                        Require(view.Hud.Canvas.transform.Find(victory?"VictoryUI":"DefeatUI").gameObject.activeInHierarchy,"correct result is visible");
                        if(victory)
                        {
                            var title=view.Hud.Canvas.transform.Find("VictoryUI/go_victory/Image/Text").gameObject;
                            Require(!title.activeInHierarchy,"source victory title remains hidden during its initial clip segment");
                            view.Hud.AdvancePresentation(.35f);
                            Require(title.activeInHierarchy,"source clip reveals victory title after .6333333 seconds");
                        }
                        view.RestartCurrentLevel();view.Hud.Synchronize();
                        foreach(string name in new[]{"PlayTopBar","StarInfoRoot","SkillUI"})
                            Require(view.Hud.Canvas.transform.Find(name).gameObject.activeInHierarchy,"retry reopens "+name);
                        Require(!ReferenceEquals(prior,view.Hud.Commander),"closed source SkillUI gets a fresh commander on retry");
                        var idle=view.Hud.Commander.AnimationState.GetCurrent(0);
                        Require(idle.Animation.Name=="idle","new commander returns to source initial idle");
                        view.Hud.AdvancePresentation(idle.Animation.Duration+.1f);
                        Require(ReferenceEquals(idle,view.Hud.Commander.AnimationState.GetCurrent(0)),"old Complete subscriptions do not survive reopened SkillUI");
                        Require(sim.CastSkill(1,1),"skill can be used after retry");
                        Require(view.Hud.Commander.AnimationState.GetCurrent(0).Animation.Name=="skill","retry cast is not blocked by old busy state");
                    }finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
                });
            }
            check("original-guide-ui-closes-on-result-and-reopens-on-retry",()=>{
                var root=new GameObject("Original guide result validation");
                try {
                    var view=root.AddComponent<BattleView>();view.InitializeNormalLevel(6);
                    Require(view.Initialized&&view.Guide.Stage>0,"source guided level initialized");
                    var panel=view.Hud.Canvas.transform.Find("GuideUI").gameObject;
                    Require(panel.activeInHierarchy,"guide initially visible");
                    foreach(var tower in view.Simulation.Towers.Where(t=>t.Camp==BattleSimulation.PlayerCampID).ToArray())
                        view.Simulation.ChangeScore(tower.Id,2,-tower.Score-1);
                    Require(view.Simulation.State==BattlePhase.Defeat,"source outcome event reached");
                    view.Hud.Synchronize();Require(!panel.activeInHierarchy,"explicit Close<GuideUI> removes tutorial overlay at result");
                    view.RestartCurrentLevel();Require(panel.activeInHierarchy,"guide reappears on guided retry");
                }finally{UnityEngine.Object.DestroyImmediate(root);Time.timeScale=1;}
            });
            return report;
        }
    }
}
