using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class AdvancementValidation
    {
        static void Need(bool b,string message){if(!b)throw new Exception(message);}
        static BattleSimulation World()=>new BattleSimulation(new LevelLayout{StarInfoCfgs=new[]{
            new StarInfoCfg{CampID=1,ShipID=1,StartScore=30},new StarInfoCfg{CampID=1,ShipID=1,StartScore=30},
            new StarInfoCfg{CampID=1,ShipID=1,StartScore=10},new StarInfoCfg{CampID=2,ShipID=1,StartScore=30}
        }},BattleView.ReadConfig(),1,(a,b)=>true){AIEnabled=false};
        static void Pick(BattleSimulation w,int id,int slot)
        {Need(w.ChooseAdvancement(id,w.AdvancementOptions(id)[slot].Id),"choice rejected");}
        static BattleSimulation Build(int route,int doctrine,int tier=2)
        {
            var w=World();w.ChangeScore(1,1,35);Pick(w,1,route);Pick(w,1,doctrine);
            while(w.Tower(1).AdvancementSpent<tier)Pick(w,1,0);
            return w;
        }
        public static void RunBatch()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Experimental progression and presentation; gameplay balance needs playtesting."};
            Action<string,Action> check=(id,test)=>{
                try{test();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}
            };
            check("10-point-threshold-retained",()=>{var w=World();Need(w.CanAdvance(3),"initial10");w.Tower(3).Score=9;w.Tower(3).AdvancementEarned=0;Need(!w.CanAdvance(3),"below10");w.ChangeScore(3,1,1);w.ChangeScore(3,2,-1);Need(w.CanAdvance(3),"earned retained");});
            check("invalid-state-and-route",()=>{var w=World();Need(!w.CanAdvance(4),"enemy");w.Pause(true);Need(!w.CanAdvance(1),"pause");w.Pause(false);w.Tower(1).IsBoss=true;Need(!w.CanAdvance(1),"boss");w.Tower(1).IsBoss=false;w.Tower(1).ShipID=4;Need(!w.CanAdvance(1),"arrow");w.Tower(1).ShipID=1;Pick(w,1,0);Need(!w.AdvanceTower(1,TowerSpecialization.Split)&&!w.ChooseAdvancement(1,"3_0"),"route locked");});
            check("every-route-stage-and-choice",()=>{var unique=new System.Collections.Generic.HashSet<string>();for(int route=0;route<3;route++)for(int doctrine=0;doctrine<3;doctrine++)for(int tier=2;tier<=6;tier++)for(int slot=0;slot<3;slot++){var w=World();w.ChangeScore(1,1,35);Pick(w,1,route);if(tier>2){Pick(w,1,doctrine);while(w.Tower(1).AdvancementSpent<tier-1)Pick(w,1,0);}var options=w.AdvancementOptions(1);Need(options.Length==3,"three choices");var id=options[slot].Id;Pick(w,1,slot);Need(w.Tower(1).AdvancementSpent==tier&&!w.ChooseAdvancement(1,id),"once per tier");unique.Add(id);if(tier==6)Need(!w.CanAdvance(1),"cap");}Need(unique.Count==117,"catalog coverage");});
            check("20-point-threshold",()=>{var w=World();Pick(w,3,0);w.ChangeScore(3,1,9);Need(!w.CanAdvance(3),"19");w.ChangeScore(3,1,1);Pick(w,3,0);Need(!w.CanAdvance(3),"one at20");});
            check("base-soldier-types-preserved-for-all-routes",()=>{for(int type=1;type<=3;type++)for(int route=0;route<3;route++){var w=World();w.Tower(1).ShipID=type;w.Connect(1,4);var old=w.SpawnSoldier(1,4);Pick(w,1,route);var unit=w.SpawnSoldier(1,4);Need(unit.HP==old.HP&&unit.Attack==old.Attack&&unit.Occupy==old.Occupy&&unit.Reinforce==old.Reinforce&&unit.ShipType==type,"combat identity unchanged");Need(old.Active&&old.VisualRoute==TowerSpecialization.None,"inflight snapshot");}});
            check("single-cap-and-speed",()=>{var w=World();w.Connect(1,4);w.Connect(1,2);w.Connect(1,3);var old=w.SpawnSoldier(1,3);Pick(w,1,0);var t=w.Tower(1);Need(t.MaxLines==1&&t.OutgoingCount==1&&old.Active,"prune only future dispatch");Need(Mathf.Abs(t.SpawnTime-w.GetSpawnTime(t.Grade,1)/1.35f)<.0001f,"single rate");Need(!w.Connect(1,3),"cannot exceed one");});
            check("split-three-lines-at10-and-dual-pruning",()=>{var w=World();Pick(w,3,1);Need(w.Tower(3).MaxLines==3,"three lines at10");w.Connect(3,1);w.Connect(3,2);w.Connect(3,4);var t=w.Tower(3);Need(t.OutgoingCount==3&&Mathf.Abs(t.SpawnTime-w.GetSpawnTime(t.Grade,1)/.7f)<.0001f,"weaker each line");w.ChangeScore(3,1,10);Pick(w,3,0);Need(t.MaxLines==2&&t.OutgoingCount==2,"dual prunes");Need(Mathf.Abs(t.SpawnTime-w.GetSpawnTime(t.Grade,1)/(.7f*1.35f))<.0001f,"dual rate");});
            check("relay-forward-fallback-and-loop-bound",()=>{var w=World();Pick(w,2,2);Pick(w,2,0);var t=w.Tower(2);Need(t.MaxLines==1,"direct route");w.Connect(1,2);w.Connect(2,4);var s=w.SpawnSoldier(1,2);w.ArriveSoldier(s,t);Need(s.Active&&s.TargetTowerId==4&&Mathf.Abs(s.Speed-1.3f)<.0001f&&t.Score==30,"forward without absorption");Need(s.VisualRoute==TowerSpecialization.None,"keeps original appearance");s=w.SpawnSoldier(1,2);s.Voyage=1;w.ArriveSoldier(s,t);Need(!s.Active,"loop bound");w.RemoveOutgoing(2);w.ArriveSoldier(w.SpawnSoldier(1,2),t);Need(t.Score==31,"receives when no output");});
            check("relay-own-production-penalty",()=>{var w=World();Pick(w,1,2);w.Connect(1,4);var t=w.Tower(1);Need(t.MaxLines==2&&Mathf.Abs(t.SpawnTime-w.GetSpawnTime(t.Grade,1)/.5f)<.0001f,"half production");});
            check("no-growth-while-dispatching-for-any-route",()=>{for(int route=0;route<3;route++){var w=Build(route,2,6);w.ChangeScore(1,2,-35);w.Connect(1,4);float before=w.Tower(1).Score;w.Tick(2.01f);Need(w.Tower(1).Score==before,"growth paused");w.RemoveOutgoing(1);w.Tick(2.01f);Need(w.Tower(1).Score>before,"growth resumes");}});
            check("capture-retry-and-no-farming",()=>{var w=Build(0,0,6);var t=w.Tower(1);w.ChangeScore(1,2,-60);w.ChangeScore(1,1,60);Need(!w.CanAdvance(1),"no farming");w.ChangeScore(1,2,-66);Need(t.AdvancementSpent==6&&t.Specialization==TowerSpecialization.Single,"capture keeps route");w.Restart();Need(t.Doctrine==-1&&t.AdvancementSpent==0&&t.AdvancementBonuses.Spawn==0,"retry reset");});
            check("production-hud-and-route-art",()=>{
                var host=new GameObject("Advancement preview");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeScene(120,30);Need(v.Initialized,"view");v.Simulation.AIEnabled=false;
                    var t=v.Simulation.Towers.First(x=>x.Camp==1&&!x.IsBoss&&x.ShipID<=3);v.Simulation.ChangeScore(t.Id,1,50);v.Hud.Synchronize();
                    var b=v.Hud.Canvas.transform.Find("StarInfoRoot/AdvanceTower_"+t.Id).GetComponent<Button>();
                    b.onClick.Invoke();v.Hud.Synchronize();BattleBuild.Capture(v,"advancement-choice.png");
                    v.Hud.Canvas.transform.Find("Tower advancement/Choice_0").GetComponent<Button>().onClick.Invoke();
                    b.onClick.Invoke();v.Hud.Synchronize();Need(v.Hud.Canvas.transform.Find("Tower advancement/Choice_2").gameObject.activeSelf,"three subroutes");
                    BattleBuild.Capture(v,"advancement-subroutes.png");
                    v.Hud.Canvas.transform.Find("Tower advancement/Choice_0").GetComponent<Button>().onClick.Invoke();
                    b.onClick.Invoke();v.Hud.Synchronize();Need(v.Hud.Canvas.transform.Find("Tower advancement/Title").GetComponentInChildren<Text>().text.Contains("30"),"next tier caption");
                    BattleBuild.Capture(v,"advancement-next-branch.png");v.Hud.Canvas.transform.Find("Tower advancement/Close").GetComponent<Button>().onClick.Invoke();
                    Need(t.Doctrine==0,"UI doctrine");
                    v.RefreshPresentation();Need(v.TowerPresentationTransform(t.Id).GetComponent<AdvancementVisual>()!=null,"tower equipment");
                    v.Simulation.Restart();v.RefreshPresentation();Need(!v.TowerPresentationTransform(t.Id).Find("Route equipment").gameObject.activeSelf,"retry clears artwork");
                    // Presentation fixture: six examples keep a seventh enemy alive. No save is written.
                    for(int i=0;i<6;i++){
                        var tower=v.Simulation.Towers[i];tower.Camp=1;tower.ShipID=i%3+1;v.Simulation.ChangeScore(tower.Id,1,65);
                        Pick(v.Simulation,tower.Id,i/2);Pick(v.Simulation,tower.Id,i%3);Pick(v.Simulation,tower.Id,0);
                        var target=v.Simulation.Towers.Last();
                        if(v.Simulation.FindLine(tower.Id,target.Id)==null)target=v.Simulation.Towers.First(x=>x.Id!=tower.Id&&v.Simulation.FindLine(tower.Id,x.Id)!=null);
                        v.Simulation.Connect(tower.Id,target.Id);
                        for(int n=1;n<=3;n++){var unit=v.Simulation.SpawnSoldier(tower.Id,target.Id);Need(unit!=null,"spawn showcase");unit.Position=Vector3.Lerp(tower.Position,target.Position,n*.19f);}
                    }
                    v.RefreshPresentation();Need(v.Simulation.Soldiers.All(s=>v.SoldierPresentationTransform(s.Id).GetComponent<AdvancementVisual>()!=null),"soldier equipment");
                    BattleBuild.Capture(v,"advancement-art-showcase.png");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("new-level-all-start-basic-and-can-evolve",()=>{
                var host=new GameObject("Basic tower experiment");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);Need(v.Initialized&&v.BasicTowerExperiment,"new layout loads");
                    var w=v.Simulation;w.AIEnabled=false;
                    Need(w.Towers.Count==9&&w.Towers.All(t=>t.ShipID==1&&!t.IsBoss&&t.Specialization==TowerSpecialization.None&&t.AdvancementSpent==0),"only basic towers initially");
                    Need(w.Towers.Count(t=>t.Camp==1)==3&&w.Towers.Count(t=>t.Camp==0)==3&&w.Towers.Count(t=>t.Camp==2)==3,"3 friendly / 3 neutral / 3 enemy");
                    Need(v.Progress.Special&&!v.Progress.OpenResult(BattlePhase.Victory),"experiment never advances normal save");
                    foreach(var t in w.Towers){
                        var renderer=v.TowerPresentationTransform(t.Id).Find("Tower Art").GetComponent<SpriteRenderer>();
                        Need(renderer.sprite!=null&&renderer.sprite.name=="基础塔·简朴","uniform compact base artwork");
                        var badge=v.Hud.Canvas.transform.Find("StarInfoRoot/AdvanceTower_"+t.Id);
                        Need(!badge.gameObject.activeSelf,"tower route caption hidden");
                    }
                    // Verify actual collider-based connectivity, rather than assuming all pairs connect.
                    var reached=new System.Collections.Generic.HashSet<int>{1};bool changed;
                    do{changed=false;foreach(var line in w.Lines){if(reached.Contains(line.SmallTowerId)&&reached.Add(line.LargeTowerId))changed=true;if(reached.Contains(line.LargeTowerId)&&reached.Add(line.SmallTowerId))changed=true;}}while(changed);
                    Need(reached.Count==9,"no isolated towers");
                    BattleBuild.Capture(v,"tower-lab-initial.png");
                    // Every location supports progression after ownership/score requirements are met.
                    foreach(var t in w.Towers){t.Camp=1;w.ChangeScore(t.Id,1,10);Need(w.CanAdvance(t.Id),"every tower can evolve");Pick(w,t.Id,(t.Id-1)%3);}
                    v.RefreshPresentation();BattleBuild.Capture(v,"tower-lab-evolved.png");
                    // Dense opposing lines make label ownership errors visible in the rendered check.
                    for(int id=1;id<=9;id++)w.Tower(id).Camp=id>=7?2:1;
                    foreach(int source in new[]{1,2,7,8}){
                        int target=source<=3?source+3:source-3;
                        Need(w.Connect(source,target,source>=7),"battle label fixture connection");
                        for(int n=1;n<=3;n++){var unit=w.SpawnSoldier(source,target);unit.Position=Vector3.Lerp(w.Tower(source).Position,w.Tower(target).Position,n*.22f);}
                    }
                    v.RefreshPresentation();BattleBuild.Capture(v,"tower-lab-labels-battle.png");
                    w.Restart();v.RefreshPresentation();Need(w.Towers.All(t=>t.AdvancementSpent==0&&t.Specialization==TowerSpecialization.None),"restart returns all to basic");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("captured-base-tower-must-reach10-under-new-owner",()=>{
                var host=new GameObject("Capture progression regression");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);var w=v.Simulation;w.AIEnabled=false;
                    foreach(int id in new[]{5,7}){
                        var t=w.Tower(id);Need(t.Score>=10&&t.AdvancementEarned==0,"neutral/enemy scores do not earn base advancement");
                        // Exercise the same arrival path as combat, down to exactly zero on capture.
                        for(int hit=0;hit<66&&t.Camp!=1;hit++)w.ArriveSoldier(new SoldierState{Camp=1,Occupy=1,Active=true},t);
                        Need(t.Camp==1,"arrival captures target");
                        v.RefreshPresentation();
                        Need(t.Score==0&&t.Specialization==TowerSpecialization.None&&!w.CanAdvance(id)&&!v.Hud.EvolutionOpen,"capture at zero does not prompt");
                        w.ChangeScore(id,1,9);v.RefreshPresentation();Need(!w.CanAdvance(id)&&!v.Hud.EvolutionOpen,"nine still basic");
                        w.ChangeScore(id,1,1);v.RefreshPresentation();Need(w.CanAdvance(id)&&v.Hud.EvolutionOpen,"own ten triggers modal");
                        Pick(w,id,0);v.RefreshPresentation();Need(t.AdvancementSpent==1,"no inherited higher tiers");
                        w.ChangeScore(id,2,-t.Score);Need(t.Specialization==TowerSpecialization.None&&t.AdvancementSpent==0,"capture resets route");
                    }
                    var baseTower=w.Tower(2);w.ChangeScore(2,1,5);Need(w.CanAdvance(2),"own pending ten");
                    w.ChangeScore(2,2,-10);w.ChangeScore(2,1,-1);v.RefreshPresentation();
                    Need(baseTower.Camp==1&&!w.CanAdvance(2)&&baseTower.AdvancementEarned==0&&!v.Hud.EvolutionOpen,"recapture clears unspent previous-owner eligibility");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("experiment-fixed-route-growth-and-budget",()=>{
                var w=new BattleSimulation(JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_99001")),BattleView.ReadConfig(),1,(a,b)=>true){AIEnabled=false};
                Need(w.Towers.All(t=>t.MaxLines==1),"all base towers one line regardless score");
                w.ChangeScore(1,1,57);Need(w.Tower(1).MaxLines==1,"base remains one at65");Pick(w,1,0);
                Need(w.Tower(1).AdvancementSpent==6&&!w.CanAdvance(1)&&w.Tower(1).MaxLines==1,"catchup automatic and locked");
                w.ChangeScore(2,1,5);Pick(w,2,1);Need(w.Tower(2).MaxLines==2,"split two at10");
                w.Connect(2,4);float one=w.Tower(2).SpawnTime;w.Connect(2,5);Need(Mathf.Abs(w.Tower(2).SpawnTime-one)<.001f,"adding split line preserves existing rate");
                Need(!w.Connect(2,6),"third locked at10");w.ChangeScore(2,1,10);Need(w.Tower(2).MaxLines==3&&w.Connect(2,6),"third unlocks automatically at20");
                w.ChangeScore(3,1,17);Pick(w,3,2);Need(w.Tower(3).AdvancementSpent==2&&w.Tower(3).MaxLines==0&&w.Tower(3).IsArrow&&w.Tower(3).ArrowRate<BattleSimulation.GetArrowRate(1),"arrow automatic rate at20");
                int spent=w.Tower(2).AdvancementSpent;w.ChangeScore(2,2,-5);w.ChangeScore(2,1,5);Need(w.Tower(2).AdvancementSpent==spent,"no duplicate growth");
            });
            check("mandatory-evolution-modal-clock-queue-and-restart",()=>{
                var host=new GameObject("Evolution modal preview");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);var w=v.Simulation;w.AIEnabled=false;
                    Need(!v.Hud.EvolutionOpen&&w.Towers.Where(t=>t.Camp==1).All(t=>t.Score<10),"no opening barrage");
                    Need(v.BeginTowerDrag(1),"drag starts before threshold");
                    w.Connect(1,4);var soldier=w.SpawnSoldier(1,4);var position=soldier.Position;
                    Need(w.CastSkill(2,1,1,0,null,Vector3.zero),"launch skill before pause");
                    var projectile=w.SkillProjectiles[0];float skillTime=projectile.Elapsed;
                    w.ChangeScore(1,1,2);w.ChangeScore(2,1,5);v.RefreshPresentation();
                    Need(v.Hud.EvolutionOpen&&Time.timeScale==0,"mandatory pause");
                    Need(!v.EndTowerDrag()&&!v.BeginTowerDrag(2)&&!v.TryUseSkillSlot(0),"input cancelled and blocked");
                    float elapsed=w.Elapsed,score=w.Tower(3).Score;v.AdvanceFrame(12,12);
                    Need(w.Elapsed==elapsed&&w.Tower(3).Score==score&&soldier.Position==position,"battle clock movement growth frozen");
                    Need(projectile.Elapsed==skillTime,"active skill flight clock frozen");
                    var sheet=v.Hud.Canvas.transform.Find("Evolution modal/Sheet");var confirm=sheet.Find("Confirm").GetComponent<Button>();
                    Need(!confirm.interactable,"must select first");confirm.onClick.Invoke();Need(w.Tower(1).AdvancementSpent==0,"cannot confirm empty");
                    BattleBuild.Capture(v,"evolution-modal.png");
                    sheet.Find("Route_1").GetComponent<Button>().onClick.Invoke();Need(confirm.interactable&&w.Tower(1).AdvancementSpent==0,"selection previews only");
                    BattleBuild.Capture(v,"evolution-modal-selected.png");confirm.onClick.Invoke();
                    Need(w.Tower(1).Specialization==TowerSpecialization.Split&&v.Hud.EvolutionOpen&&Time.timeScale==0&&!confirm.interactable,"queue stays frozen selection resets");
                    sheet.Find("Route_0").GetComponent<Button>().onClick.Invoke();confirm.onClick.Invoke();
                    Need(!v.Hud.EvolutionOpen&&Time.timeScale==1,"resumes after final confirmation");
                    v.AdvanceFrame(.1f,.1f);Need(w.Elapsed>elapsed,"clock resumes");
                    w.ChangeScore(1,1,10);v.RefreshPresentation();Need(w.Tower(1).MaxLines==3&&!v.Hud.EvolutionOpen,"20 automatic no interruption");
                    w.ChangeScore(3,1,10);v.RefreshPresentation();Need(v.Hud.EvolutionOpen,"later tower prompts");
                    v.RestartCurrentLevel();Need(!v.Hud.EvolutionOpen&&Time.timeScale==1&&v.Simulation.Towers.All(t=>t.Specialization==TowerSpecialization.None),"restart clears modal and pause");
                    v.Simulation.Pause(true);v.RefreshPresentation();Need(!v.Hud.EvolutionOpen&&Time.timeScale==0,"ordinary pause preserved");
                    v.Simulation.Pause(false);
                    for(int i=0;i<10&&!v.Hud.EvolutionOpen;i++)v.AdvanceFrame(3,3);
                    Need(v.Hud.EvolutionOpen,"natural growth triggers without manual badge click");
                    foreach(var t in v.Simulation.Towers)if(v.Simulation.CanAdvance(t.Id))Pick(v.Simulation,t.Id,0);
                    v.Simulation.Tower(7).Camp=1;v.Simulation.ChangeScore(7,1,1);v.RefreshPresentation();
                    Need(!v.Hud.Commander.gameObject.activeInHierarchy,"commander cannot cover choices");
                    var modal=v.Hud.Canvas.transform.Find("Evolution modal");
                    var compact=(RectTransform)modal.Find("Sheet");var highlight=(RectTransform)modal.Find("Selected tower");
                    Need(compact.sizeDelta.y<=450,"compact panel height");
                    var corners=new Vector3[4];compact.GetWorldCorners(corners);float sheetBottom=corners[0].y,sheetTop=corners[1].y;
                    highlight.GetWorldCorners(corners);Need(corners[1].y<sheetBottom||corners[0].y>sheetTop,"highlighted lower tower remains uncovered");
                    BattleBuild.Capture(v,"evolution-modal-lower-tower.png");

                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("enemy-evolution-threshold-choice-parity-and-multiline-ai",()=>{
                Func<int,bool,BattleSimulation> make=(score,multi)=>new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                    new StarInfoCfg{CampID=2,ShipID=1,StartScore=score},new StarInfoCfg{CampID=1,ShipID=1,StartScore=5},
                    new StarInfoCfg{CampID=0,ShipID=1,StartScore=5},new StarInfoCfg{CampID=0,ShipID=1,StartScore=5}
                }},BattleView.ReadConfig(),7,(x,y)=>x==1&&(multi||y==2));
                for(int slot=0;slot<2;slot++){
                    var w=make(9,slot==1);w.Tick(0);Need(w.Tower(1).Specialization==TowerSpecialization.None,"below10 stays base");
                    w.ChangeScore(1,2,1);w.Tick(0);var enemy=w.Tower(1);
                    Need(enemy.Specialization==(slot==0?TowerSpecialization.Single:TowerSpecialization.Split)&&!w.CanAdvance(1),"enemy selects route without player eligibility");
                    var player=make(10,slot==1);player.Tower(1).Camp=1;player.Tower(2).Camp=2;player.ChangeScore(1,1,.01f);Pick(player,1,slot);
                    Need(enemy.MaxLines==player.Tower(1).MaxLines&&Mathf.Abs(enemy.SpawnTime-player.Tower(1).SpawnTime)<.0001f,"same level10 values");
                    w.ChangeScore(1,2,30);player.ChangeScore(1,1,30);
                    Need(enemy.AdvancementSpent==4&&enemy.MaxLines==player.Tower(1).MaxLines&&Mathf.Abs(enemy.SpawnTime-player.Tower(1).SpawnTime)<.0001f,"same automatic level40 values");
                    if(slot==1){
                        var cfg=new AIConfig{AIType=3,ActionNum=1};w.AIShuffleIndex=n=>0;
                        for(int i=0;i<3;i++)w.RunAI(2,cfg);
                        Need(enemy.OutgoingCount==3,"existing AI fills unlocked branches");
                    }
                    w.ChangeScore(1,1,-enemy.Score);w.Tick(0);Need(enemy.Specialization==TowerSpecialization.None&&enemy.AdvancementSpent==0,"captured evolved route cleared");
                }
                var pressured=make(10,true);Need(pressured.Connect(2,1),"incoming player attack");pressured.Tick(0);
                Need(pressured.Tower(1).Specialization==TowerSpecialization.Single,"concentrates under pressure");
                var disabled=make(20,true);disabled.AIEnabled=false;disabled.Tick(0);Need(disabled.Tower(1).AdvancementSpent==0,"AI toggle honored");
                disabled.AIEnabled=true;disabled.Pause(true);disabled.Tick(0);Need(disabled.Tower(1).AdvancementSpent==0,"no progression while paused");
                disabled.Pause(false);disabled.Tick(0);Need(disabled.Tower(1).AdvancementSpent==2,"starting20 catches up");
                disabled.Restart();Need(disabled.Tower(1).Specialization==TowerSpecialization.None,"restart clears enemy route");
            });
            check("enemy-production-view-never-opens-player-modal",()=>{
                var host=new GameObject("Enemy evolution preview");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);v.AdvanceFrame(0,0);
                    Need(v.Simulation.Towers.Where(t=>t.Camp==2).All(t=>t.Specialization==TowerSpecialization.Single||t.Specialization==TowerSpecialization.Split),"all eligible enemy towers evolve");
                    Need(!v.Hud.EvolutionOpen&&Time.timeScale==1,"enemy upgrade does not pause or open modal");
                    foreach(var t in v.Simulation.Towers.Where(t=>t.Camp==2))Need(v.TowerPresentationTransform(t.Id).Find("Tower Art").GetComponent<SpriteRenderer>().sprite==CompactTowerVisual.ForTower(t)&&t.Specialization!=TowerSpecialization.None,"enemy evolved tower artwork visible");
                    BattleBuild.Capture(v,"enemy-evolution.png");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("current-score-downgrade-and-owner-route-memory",()=>{
                for(int route=0;route<3;route++){
                    var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                        new StarInfoCfg{CampID=1,ShipID=1,StartScore=60},new StarInfoCfg{CampID=2,ShipID=1,StartScore=8},
                        new StarInfoCfg{CampID=0,ShipID=1,StartScore=8},new StarInfoCfg{CampID=0,ShipID=1,StartScore=8}
                    }},BattleView.ReadConfig(),1,(a,b)=>true){AIEnabled=false};
                    Pick(w,1,route);var t=w.Tower(1);var chosen=t.Specialization;w.Connect(1,2);var unit=w.SpawnSoldier(1,2);
                    if(route==1){w.Connect(1,3);w.Connect(1,4);}
                    w.ChangeScore(1,2,-1);Need(t.AdvancementSpent==5,"60 to59 loses tier6");
                    w.ChangeScore(1,2,-39);Need(t.AdvancementSpent==2,"exact20 retains tier2");
                    w.ChangeScore(1,2,-1);Need(t.AdvancementSpent==1&&t.AdvancementBonuses.Spawn==0&&t.AdvancementBonuses.Speed==0,"19 loses level20 bonus");
                    if(route==1)Need(t.MaxLines==2&&t.OutgoingCount==2,"third output removed");
                    w.ChangeScore(1,2,-9);Need(t.AdvancementSpent==1,"exact10 retains route");
                    w.ChangeScore(1,2,-1);Need(route==2?t.IsArrow&&t.AdvancementSpent==1&&t.MaxLines==0:t.Specialization==TowerSpecialization.None&&t.AdvancementSpent==0&&t.MaxLines==1&&t.OutgoingCount<=1&&!w.CanAdvance(1),"nine retains arrow but deactivates troop routes");
                    Need(route==2?unit==null:unit.Active&&unit.VisualRoute!=TowerSpecialization.None,"arrow cannot dispatch; existing other units preserved");
                    Need(t.RememberedSpecialization==chosen,"route remembered while inactive");
                    w.ChangeScore(1,1,1);Need(!w.CanAdvance(1),"recovery does not require choice");
                    Need(t.AdvancementSpent==1&&t.Specialization==chosen,"same route automatically restored");
                    w.ChangeScore(1,1,10);w.ChangeScore(1,2,-1);w.ChangeScore(1,1,1);Need(t.AdvancementSpent==2,"oscillation does not stack bonuses");
                    Need(t.AdvancementBonuses.Spawn<=20&&t.AdvancementBonuses.Speed<=25,"bonus values rebuilt");
                    w.ChangeScore(1,2,-35);Need(t.Camp==2&&t.Score==15&&t.Specialization==TowerSpecialization.None&&t.RememberedSpecialization==TowerSpecialization.None,"overkill capture also clears route memory");
                }
            });
            check("route-memory-recovery-no-modal-and-capture-new-choice",()=>{
                var host=new GameObject("Route memory regression");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);var w=v.Simulation;w.AIEnabled=false;
                    w.ChangeScore(1,1,2);Pick(w,1,1);v.RefreshPresentation();w.ChangeScore(1,2,-1);v.RefreshPresentation();var t=w.Tower(1);
                    Need(t.Specialization==TowerSpecialization.None&&t.MaxLines==1,"nine uses base mechanics");
                    Need(v.TowerPresentationTransform(1).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name=="基础塔·简朴","nine restores plain basic artwork");
                    w.ChangeScore(1,1,1);v.RefreshPresentation();
                    Need(t.Specialization==TowerSpecialization.Split&&t.MaxLines==2&&!v.Hud.EvolutionOpen&&Time.timeScale==1,"recovery restores without interrupting");
                    Need(v.TowerPresentationTransform(1).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name=="分流塔","recovery restores split building artwork");
                    w.ChangeScore(1,2,-10);Need(t.RememberedSpecialization==TowerSpecialization.None,"enemy capture clears memory");
                    w.ChangeScore(1,1,-1);w.ChangeScore(1,1,9);v.RefreshPresentation();Need(v.Hud.EvolutionOpen&&w.CanAdvance(1),"recaptured tower needs a fresh choice");
                    Pick(w,1,2);v.RefreshPresentation();Need(t.Specialization==TowerSpecialization.Arrow,"new owner can change route");
                    w.ChangeScore(1,2,-1);w.Restart();w.ChangeScore(1,1,2);Need(w.CanAdvance(1)&&t.RememberedSpecialization==TowerSpecialization.None,"restart clears dormant memory");
                    var enemy=w.Tower(7);w.AIEnabled=true;w.Tick(0);var chosen=enemy.Specialization;
                    w.ChangeScore(7,1,-(enemy.Score-9));Need(enemy.Specialization==TowerSpecialization.None&&enemy.RememberedSpecialization==chosen,"enemy also remembers inactive route");
                    w.ChangeScore(7,2,1);Need(enemy.Specialization==chosen&&enemy.AdvancementSpent==1,"enemy recovers same route without new AI choice");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("arrow-evolution-reuses-targeting-no-dispatch-and-restoration",()=>{
                var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                    new StarInfoCfg{CampID=1,ShipID=1,StartScore=10},new StarInfoCfg{CampID=2,ShipID=1,StartScore=20,pos=new IntVector3{x=85}}
                }},BattleView.ReadConfig(),1,(a,c)=>true){AIEnabled=false,ArrowTargetIndex=n=>0};
                foreach(var t in w.Towers)t.AutoAddScore=false;
                Need(w.Connect(1,2),"base can dispatch before evolution");Pick(w,1,2);var tower=w.Tower(1);
                Need(tower.IsArrow&&tower.MaxLines==0&&tower.OutgoingCount==0&&!w.Connect(1,2)&&w.SpawnSoldier(1,2)==null,"arrow disables and removes dispatch");
                Need(w.Connect(2,1,true),"enemy incoming line remains");var soldier=w.SpawnSoldier(2,1);soldier.Position=new Vector3(.4f,0,0);soldier.Speed=0;w.SpawnMultiplier=t=>0;
                w.Tick(tower.ArrowRate+.001f);Need(w.Arrows.Count>0&&w.Arrows[0].TargetSoldierId==soldier.Id,"automatic shot prioritizes enemy soldier");
                w.Tick(.21f);Need(!soldier.Active,"original arrow kills targeted soldier");w.RemoveOutgoing(2);
                w.Tower(2).Score=1;var shot=w.FireArrow(1);Need(shot!=null&&shot.TargetTowerId==2,"fallback shoots tower");w.Tick(.21f);
                Need(w.Tower(2).Camp==2&&w.Tower(2).Score==0,"arrows damage without occupation");
                float initialRate=tower.ArrowRate;w.ChangeScore(1,1,10);Need(tower.ArrowRate<initialRate&&tower.ArrowGrade==1,"20 increases fire rate only");
                w.ChangeScore(1,2,-11);Need(tower.IsArrow&&w.FireArrow(1)!=null&&!w.Connect(1,2)&&tower.AdvancementSpent==1,"nine keeps base arrow firing without dispatch");
                w.ChangeScore(1,1,1);Need(tower.IsArrow&&tower.OutgoingCount==0&&!w.CanAdvance(1)&&Mathf.Abs(tower.ArrowRate-initialRate)<.001f,"ten restores same arrow and removes base output");
            });
            check("arrow-evolution-production-art-and-choice",()=>{
                var host=new GameObject("Arrow route preview");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeSpecialScene(99001);v.Simulation.AIEnabled=false;
                    v.Simulation.ChangeScore(1,1,2);v.RefreshPresentation();
                    var sheet=v.Hud.Canvas.transform.Find("Evolution modal/Sheet");sheet.Find("Route_2").GetComponent<Button>().onClick.Invoke();
                    BattleBuild.Capture(v,"arrow-evolution-choice.png");sheet.Find("Confirm").GetComponent<Button>().onClick.Invoke();
                    Need(v.Simulation.Tower(1).IsArrow&&!v.Hud.EvolutionOpen,"third choice makes arrow");
                    var sprite=v.TowerPresentationTransform(1).Find("Tower Art").GetComponent<SpriteRenderer>().sprite;
                    Need(sprite!=null&&sprite.name.Contains("箭"),"uses original arrow tower sprite");
                    BattleBuild.Capture(v,"arrow-evolution-tower.png");
                    Need(v.InspectArrowRange(1)&&v.ArrowRangeVisible,"click evolved arrow displays range");
                    BattleBuild.Capture(v,"arrow-evolution-range.png");
                    v.Simulation.ChangeScore(1,1,50);v.RefreshPresentation();BattleBuild.Capture(v,"arrow-evolution-range60.png");v.Simulation.ChangeScore(1,2,-50);
                    v.Simulation.ChangeScore(1,2,-1);v.RefreshPresentation();Need(v.ArrowRangeVisible&&v.Simulation.Tower(1).IsArrow,"below10 keeps arrow and its range");
                    v.Simulation.ChangeScore(1,1,1);v.RefreshPresentation();Need(v.InspectArrowRange(1),"restored arrow can be inspected");
                    v.RestartCurrentLevel();Need(!v.ArrowRangeVisible,"retry clears range");

                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("evolution-campaign-normal-layouts-and-progression",()=>{
                var catalog=new BattleLevelCatalog(BattleView.ReadText("Data/LevelConfig"));
                foreach(int level in new[]{0,2,7,9,25,30,100,871}){
                    var host=new GameObject("Campaign validation "+level);
                    try{
                        var v=host.AddComponent<BattleView>();v.EvolutionCampaign=true;v.InitializeNormalLevel(level);
                        Need(v.Initialized&&v.BasicTowerExperiment&&v.Simulation.BasicTowerExperiment&&!v.Progress.Special&&v.Progress.SelectedLevel==level,"normal identity with evolution enabled");
                        var original=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_"+catalog.Resolve(level).SceneId));
                        Need(v.Simulation.Towers.Count==original.StarInfoCfgs.Length,"original tower positions retained");
                        for(int i=0;i<v.Simulation.Towers.Count;i++){
                            var t=v.Simulation.Towers[i];var source=original.StarInfoCfgs[i];
                            Need(t.Position==source.pos.WorldPosition&&t.Camp==source.CampID,"positions and camps unchanged");
                            Need(source.isBoss||source.CampID==0?t.Score==source.StartScore:t.Score>=5&&t.Score<=8,"neutral and boss values preserved, owned towers below10");
                            Need(t.IsBoss==source.isBoss&&(t.IsBoss||t.ShipID==1&&t.Specialization==TowerSpecialization.None&&t.MaxLines==1),"ordinary towers start basic, bosses preserved");
                        }
                        Need(v.Guide.Stage==0&&v.Guide.CanDraw,"old scripted type guides do not block campaign");
                        Need(!v.Hud.EvolutionOpen&&v.Simulation.Towers.All(t=>t.Specialization==TowerSpecialization.None),"no initial progression or forced modal");
                        v.Simulation.AIEnabled=false;
                        foreach(var t in v.Simulation.Towers)if(v.Simulation.CanAdvance(t.Id))Pick(v.Simulation,t.Id,0);
                        v.RefreshPresentation();v.AdvanceFrame(.1f,.1f);Need(v.Simulation.Elapsed>0,"campaign clock runs after initial choices");
                        if(level==30)BattleBuild.Capture(v,"campaign-level30.png");
                    }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
                }
                string file=Path.Combine(BattleBuild.Workspace,"analysis/campaign-validation-profile.json");
                File.WriteAllText(file,JsonUtility.ToJson(BattleLoadout.CreateFixture(0,1,1,3)));
                var owner=new GameObject("Campaign save validation");
                try{
                    var v=owner.AddComponent<BattleView>();v.EvolutionCampaign=true;v.Loadout=BattleLoadout.LoadOrCreate(file);v.InitializeNormalLevel(v.Loadout.NormalLevel);
                    foreach(var t in v.Simulation.Towers)t.Camp=1;v.Simulation.EvaluateOutcome();v.RefreshPresentation();
                    Need(v.Progress.SavedLevel==1&&BattleLoadout.LoadOrCreate(file).NormalLevel==1,"normal victory persisted next level");
                    v.Hud.Canvas.transform.Find("VictoryUI/objBtn/go_common/btn_normalGold2").GetComponent<Button>().onClick.Invoke();Need(v.BasicTowerExperiment&&v.Progress.SelectedLevel==1,"next-level button keeps rules");
                    v.RestartCurrentLevel();Need(v.Progress.SelectedLevel==1&&v.BasicTowerExperiment,"retry keeps normal identity and rules");
                    v.InitializeSpecialScene(99001);Need(v.Progress.Special,"lab retains special identity");
                    foreach(var t in v.Simulation.Towers)t.Camp=1;v.Simulation.EvaluateOutcome();Need(BattleLoadout.LoadOrCreate(file).NormalLevel==1,"lab cannot advance campaign save");
                }finally{UnityEngine.Object.DestroyImmediate(owner);Time.timeScale=1;}
            });
            check("campaign-low-start-all-catalog-layouts",()=>{
                var catalog=new BattleLevelCatalog(BattleView.ReadText("Data/LevelConfig"));
                for(int level=0;level<=catalog.MaximumLevel;level++){
                    string json=BattleView.ReadText("Data/Levels/level_"+catalog.Resolve(level).SceneId);
                    var layout=JsonUtility.FromJson<LevelLayout>(json);var original=JsonUtility.FromJson<LevelLayout>(json);
                    BattleView.PrepareEvolutionCampaign(layout);
                    for(int i=0;i<layout.StarInfoCfgs.Length;i++){
                        var t=layout.StarInfoCfgs[i];var before=original.StarInfoCfgs[i];
                        Need(t.isBoss||t.CampID==0?t.StartScore==before.StartScore:t.StartScore>=5&&t.StartScore<=8,"start range for level "+level);
                        Need(t.isBoss?t.ShipID==before.ShipID:t.ShipID==1,"base type for level "+level);
                    }
                    foreach(var group in layout.StarInfoCfgs.Where(t=>!t.isBoss&&t.CampID!=0).GroupBy(t=>t.CampID))Need(group.Count(t=>t.StartScore==8)==1,"one lead tower per camp in level "+level);
                }
            });
            check("campaign-first-evolution-arrives-through-growth",()=>{
                var host=new GameObject("Campaign opening pace");
                try{
                    var v=host.AddComponent<BattleView>();v.EvolutionCampaign=true;v.InitializeNormalLevel(30);v.Simulation.AIEnabled=false;
                    Need(!v.Hud.EvolutionOpen&&Time.timeScale==1,"opening immediately playable");
                    for(int i=0;i<200&&!v.Hud.EvolutionOpen;i++)v.AdvanceFrame(.1f,.1f);
                    Need(v.Hud.EvolutionOpen&&v.Simulation.Elapsed>0&&v.Simulation.Elapsed<=20,"idle lead naturally reaches first evolution within20 seconds");
                    v.RestartCurrentLevel();Need(!v.Hud.EvolutionOpen&&v.Simulation.Towers.Where(t=>t.Camp!=0&&!t.IsBoss).All(t=>t.Score<10),"retry restores low opening");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("evolution-ai-counterattack-expansion-and-pause",()=>{
                Func<BattleSimulation> make=()=>new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,CampInfoCfgs=new[]{new CampInfoCfg{CampID=2,AIGrade=1000}},StarInfoCfgs=new[]{
                    new StarInfoCfg{CampID=2,ShipID=1,StartScore=5},new StarInfoCfg{CampID=1,ShipID=1,StartScore=8,pos=new IntVector3{x=100}},
                    new StarInfoCfg{CampID=0,ShipID=1,StartScore=4,pos=new IntVector3{z=100}}
                }},BattleView.ReadConfig(),1,(a,c)=>true);
                var w=make();foreach(var t in w.Towers)t.AutoAddScore=false;
                Need(w.Connect(1,3,true)&&w.Connect(2,1),"enemy busy expanding when attacked");
                w.Tick(.61f);Need(w.FindLine(1,2).IsFrom(1)&&!w.FindLine(1,3).IsFrom(1),"responds within reaction window and reallocates full output");
                for(int i=0;i<12;i++)w.Tick(.1f);
                Need(w.FindLine(1,2).IsFrom(1)&&w.Soldiers.Exists(t=>t.OriginTowerId==1),"counterattack persists and actually produces soldiers");
                var peaceful=make();foreach(var t in peaceful.Towers)t.AutoAddScore=false;
                for(int i=0;i<50;i++)peaceful.Tick(.1f);
                Need(peaceful.Tower(1).OutgoingCount==1&&peaceful.FindLine(1,3).IsFrom(1),"low-score enemy actively expands despite passive legacy config");
                var paused=make();paused.Connect(2,1);paused.Pause(true);paused.Tick(1);Need(paused.Tower(1).OutgoingCount==0,"manual pause blocks reaction");
                paused.Pause(false);paused.AIEnabled=false;paused.Tick(1);Need(paused.Tower(1).OutgoingCount==0,"AI toggle blocks reaction");
                paused.AIEnabled=true;paused.Tick(.61f);Need(paused.FindLine(1,2).IsFrom(1),"reacts after resumed");
            });
            check("lower-output-balance-and-measured-dispatch",()=>{
                foreach(int score in new[]{10,60}){
                    var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                        new StarInfoCfg{CampID=1,ShipID=1,StartScore=score},new StarInfoCfg{CampID=1,ShipID=1,StartScore=score,pos=new IntVector3{x=30}},
                        new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=10000}},new StarInfoCfg{CampID=0,ShipID=1,StartScore=65,pos=new IntVector3{z=10000}}
                    }},BattleView.ReadConfig(),1,(x,y)=>true){AIEnabled=false};
                    foreach(var t in w.Towers)t.AutoAddScore=false;
                    Pick(w,2,0);w.Connect(1,3);w.Connect(2,3);
                    float expected=score==10?1.2f:1.45f;
                    Need(Mathf.Abs(w.Tower(1).SpawnTime/w.Tower(2).SpawnTime-expected)<.001f,"single exact additive rate at "+score);
                    int basic=0,single=0;w.Event+=e=>{if(e.Kind=="spawn"){if(e.TowerId==1)basic++;if(e.TowerId==2)single++;}};
                    for(int i=0;i<3600;i++)w.Tick(1f/120);
                    Need(basic>0&&single>basic&&single/(float)basic<expected+.06f,"actual30-second dispatch respects reduced bonus");
                    Pick(w,1,1);w.Connect(1,4);
                    float splitExpected=score==10?1f:1.25f;
                    Need(Mathf.Abs(w.GetSpawnTime(w.Tower(1).Grade,1)/w.Tower(1).SpawnTime-splitExpected)<.001f,"split per-line output preserved");
                    if(score==60){w.ChangeScore(2,2,-41);Need(Mathf.Abs(w.GetSpawnTime(w.Tower(2).Grade,1)/w.Tower(2).SpawnTime-1.2f)<.001f,"downgrade removes later rate gains");w.ChangeScore(2,1,41);Need(Mathf.Abs(w.GetSpawnTime(w.Tower(2).Grade,1)/w.Tower(2).SpawnTime-1.45f)<.001f,"recovery restores without stacking");}
                }
            });
            check("split-independent-lanes-all-tiers-and-downgrade",()=>{
                foreach(int tier in new[]{1,2,3,4,5,6}){
                    var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                        new StarInfoCfg{CampID=1,ShipID=1,StartScore=tier*10},
                        new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=100000}},
                        new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{z=100000}},
                        new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=-100000}}
                    }},BattleView.ReadConfig(),1,(x,y)=>true){AIEnabled=false};
                    foreach(var t in w.Towers)t.AutoAddScore=false;
                    Pick(w,1,1);var source=w.Tower(1);w.Connect(1,2);float interval=source.SpawnTime;
                    Need(Mathf.Abs(w.GetSpawnTime(source.Grade,1)/interval-(1+(tier-1)*.05f))<.001f,"per-lane tier values");
                    w.Tick(.4f);float firstTimer=w.FindLine(1,2).SmallSpawnTimer;
                    w.Connect(1,3);if(tier>1)w.Connect(1,4);
                    Need(Mathf.Abs(source.SpawnTime-interval)<.001f&&w.FindLine(1,2).SmallSpawnTimer==firstTimer,"additional lanes do not slow or reset existing production");
                    int[] counts=new int[5];w.Event+=e=>{if(e.Kind=="spawn"&&e.TowerId==1)counts[w.Soldiers.Find(u=>u.Id==e.SoldierId).TargetTowerId]++;};
                    for(int i=0;i<3600;i++)w.Tick(1f/120);
                    Need(counts[2]>0&&Math.Abs(counts[2]-counts[3])<=1&&(tier==1||Math.Abs(counts[3]-counts[4])<=1),"each connected route produces independently");
                    if(tier>1){w.ChangeScore(1,2,-(source.Score-19));Need(source.MaxLines==2&&source.OutgoingCount==2&&source.AdvancementBonuses.Spawn==0,"drop below20 removes third line and level20 rate gain");w.ChangeScore(1,1,1);Need(source.MaxLines==3&&source.AdvancementBonuses.Spawn==5,"recover20 restores correct additive bonus");}
                }
            });
            check("arrow-growing-range-and-low-score-persistence",()=>{
                var w=new BattleSimulation(new LevelLayout{BasicTowerExperiment=true,StarInfoCfgs=new[]{
                    new StarInfoCfg{CampID=1,ShipID=1,StartScore=10},new StarInfoCfg{CampID=2,ShipID=1,StartScore=65,pos=new IntVector3{x=105}}
                }},BattleView.ReadConfig(),1,(a,c)=>true){AIEnabled=false};
                Pick(w,1,2);var t=w.Tower(1);Need(w.FindArrowTower(t)==null,"outside level10 range");
                for(int tier=2;tier<=6;tier++){
                    w.ChangeScore(1,1,10);Need(Mathf.Abs(t.ArrowRange-(1+(tier-1)*.1f))<.0001f,"range grows at every tier");
                    Need(w.FindArrowTower(t)!=null,"expanded range changes actual targeting");
                }
                w.ChangeScore(1,2,-41);Need(t.AdvancementSpent==1&&Mathf.Abs(t.ArrowRange-1)<.0001f&&w.FindArrowTower(t)==null,"range shrinks with lost tiers");
                w.ChangeScore(1,2,-18);Need(t.Score==1&&t.IsArrow&&t.MaxLines==0&&Mathf.Abs(t.ArrowRate-1.3f)<.0001f,"one hp retains baseline weapon");
                w.ChangeScore(1,2,-1);Need(t.Camp==2&&!t.IsArrow&&t.RememberedSpecialization==TowerSpecialization.None,"capture at zero resets arrow");
            });
            check("compact-art-state-camp-and-size",()=>{
                var host=new GameObject("Compact art validation");
                try{
                    var v=host.AddComponent<BattleView>();v.InitializeScene(99001);Need(v.Initialized,"compact scene");var w=v.Simulation;w.AIEnabled=false;
                    for(int i=0;i<4;i++){
                        var t=w.Towers[i];t.Camp=1;
                        if(i>0){w.ChangeScore(t.Id,1,10-t.Score);Pick(w,t.Id,i-1);}
                        t.Camp=i+1;
                    }
                    v.RefreshPresentation();
                    string[] expected={"基础塔·简朴","突击塔","分流塔","箭塔"};
                    for(int i=0;i<4;i++){
                        var t=w.Towers[i];var root=v.TowerPresentationTransform(t.Id);var sr=root.Find("Tower Art").GetComponent<SpriteRenderer>();
                        Need(sr.sprite.name==expected[i],"state artwork "+i);
                        Need(sr.bounds.size.x<=.263f,"width cap "+i);
                        Need(sr.sharedMaterial.shader.name=="AreaBattle/CompactTowerCamp"&&sr.sharedMaterial.shader.isSupported,"camp shader");
                        var block=new MaterialPropertyBlock();sr.GetPropertyBlock(block);Need(block.GetColor("_CampColor")==BattleView.CampColor(t.Camp),"camp color");
                        Need(root.GetComponent<AdvancementVisual>()==null,"no attached tower route equipment");
                        var dots=v.Hud.Canvas.transform.Find("StarInfoRoot/TowerCanvas_"+t.Id+"/normal");
                        Need(dots!=null&&dots.gameObject.activeInHierarchy,"capacity group visible");
                        int visibleDots=0;for(int slot=0;slot<3;slot++)if(dots.GetChild(slot).gameObject.activeInHierarchy)visibleDots++;
                        Need(visibleDots==t.MaxLines,"capacity dots reflect actual route capacity");
                    }
                    BattleBuild.Capture(v,"compact-art-four-routes.png");
                    var split=w.Towers[2];w.ChangeScore(split.Id,split.Camp==1?2:1,-1);v.RefreshPresentation();
                    Need(v.TowerPresentationTransform(split.Id).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name==expected[0],"split below10 basic art");
                    w.ChangeScore(split.Id,split.Camp,1);v.RefreshPresentation();Need(v.TowerPresentationTransform(split.Id).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name==expected[2],"remembered split art returns");
                    var arrow=w.Towers[3];w.ChangeScore(arrow.Id,1,-1);v.RefreshPresentation();Need(v.TowerPresentationTransform(arrow.Id).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name==expected[3],"arrow below10 retains art");
                    w.ChangeScore(arrow.Id,1,-20);v.RefreshPresentation();Need(v.TowerPresentationTransform(arrow.Id).Find("Tower Art").GetComponent<SpriteRenderer>().sprite.name==expected[0],"capture resets art");
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            check("compact-environment-all-wall-shapes-preserve-physics",()=>{
                var doc=JsonUtility.FromJson<BattleObstacles.Document>(BattleView.ReadText("Data/ObstacleGeometry"));
                Need(doc.prefabs.Length==15,"15 obstacle variants");
                foreach(var entry in doc.prefabs){
                    var host=new GameObject("Wall material fixture");
                    try{
                        var layout=new LevelLayout{ObstacleInfoCfgs=new[]{new ObstacleInfoCfg{EnityID=entry.entityId,scale=new IntVector3{x=100,y=100,z=100}}}};
                        var roots=BattleObstacles.Create(layout,host.transform);
                        var before=roots[0].GetComponentsInChildren<BoxCollider>(true);
                        var transforms=before.Select(c=>c.transform.localToWorldMatrix).ToArray();var sizes=before.Select(c=>c.size).ToArray();
                        var visual=RecoveredObstacleVisual.Attach(roots[0],entry.entityId);var renderers=visual.GetComponentsInChildren<MeshRenderer>(true);
                        var meshes=renderers.Select(m=>m.GetComponent<MeshFilter>().sharedMesh).ToArray();
                        CompactEnvironment.RestyleWall(visual);
                        Need(roots[0].GetComponentsInChildren<Collider>(true).Length==before.Length,"no new colliders");
                        for(int i=0;i<before.Length;i++)Need(before[i].transform.localToWorldMatrix==transforms[i]&&before[i].size==sizes[i],"collision geometry preserved");
                        for(int i=0;i<renderers.Length;i++)Need(renderers[i].GetComponent<MeshFilter>().sharedMesh==meshes[i]&&renderers[i].sharedMaterial.shader.name=="AreaBattle/WarmStoneWall"&&renderers[i].sharedMaterial.shader.isSupported,"original wall mesh, new supported material");
                    }finally{UnityEngine.Object.DestroyImmediate(host);}
                }
            });
            check("compact-ground-and-walls-runtime-render",()=>{
                var host=new GameObject("Environment render fixture");
                try{
                    var v=host.AddComponent<BattleView>();v.EvolutionCampaign=true;v.InitializeNormalLevel(116);
                    Need(v.Initialized&&v.BasicTowerExperiment&&v.BattleCamera.GetComponent<CompactEnvironment>()!=null,"new ground initialized");
                    v.Simulation.AIEnabled=false;v.RefreshPresentation();BattleBuild.Capture(v,"compact-environment-layout104.png");
                    var ground=v.BattleCamera.transform.Find("Compact battlefield ground").GetComponent<SpriteRenderer>();
                    Need(ground!=null&&ground.GetComponent<Collider>()==null,"background visual only");
                    v.BattleCamera.GetComponent<CompactEnvironment>().FitToCamera();
                    Need(ground.sprite.bounds.size.x*ground.transform.localScale.x>=4.2f*.5625f-.001f&&ground.sprite.bounds.size.y*ground.transform.localScale.y>=4.2f-.001f,"portrait viewport covered: "+ground.bounds.size);
                }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}
            });
            foreach(var baseline in new[]{BattleBuild.Validate(),SkillValidation.Run(),GuideValidation.Run(),ArrowValidation.Run()}){r.passed &= baseline.passed;r.checks.AddRange(baseline.checks);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/advancement-validation.json"),JsonUtility.ToJson(r,true));
            if(!r.passed){Debug.LogError("ADVANCEMENT_FAIL");EditorApplication.Exit(1);return;}
            Time.timeScale=1;
            var output=Path.Combine(BattleBuild.Workspace,"Build/Advancement/AreaBattle.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
            var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/AreaBattle/Scenes/Battle.unity"},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            Debug.Log("ADVANCEMENT_PASS checks="+r.checks.Count+" build="+build.summary.result);EditorApplication.Exit(build.summary.result==BuildResult.Succeeded?0:1);
        }
    }
}
