using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class SkillInputValidation
    {
        static BattleSimulation World()
        {
            var w=new BattleSimulation(new LevelLayout{StarInfoCfgs=new[]{
                new StarInfoCfg{CampID=1,StartScore=30,ShipID=1,pos=new IntVector3{z=200}},
                new StarInfoCfg{CampID=2,StartScore=30,ShipID=1,pos=new IntVector3{x=100,z=200}}}},BattleView.ReadConfig(),43,(a,b)=>true);
            w.AIEnabled=false;BattleView.ConfigureSkills(w);return w;
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            Action<bool,string> require=(ok,msg)=>{if(!ok)throw new Exception(msg);};
            string rules=BattleView.ReadText("Data/AllSkillConfig");
            check("skill-input-specific-before-generic-and-active-guard",()=>{
                var w=World();var input=new BattleSkillInput(w,rules,1,21,1,new Dictionary<int,int>{{2001,1}},1);
                require(input.TryUse(0),"first use");require(input.Count(0)==0&&input.GenericStock==1,"specific first");
                require(!input.TryUse(0)&&input.GenericStock==1,"active guard preserves inventory");
                w.Tick(20);require(input.TryUse(0)&&input.GenericStock==0,"generic fallback after duration");
            });
            check("skill-input-unlock-and-invalid-target-preserve-stock",()=>{
                var w=World();var locked=new BattleSkillInput(w,rules,1,5,1,new Dictionary<int,int>{{2001,1}},0);
                require(!locked.TryUse(0)&&locked.Count(0)==1,"below original unlock level6");
                var input=new BattleSkillInput(w,rules,1,21,1,new Dictionary<int,int>{{2003,2}},0);
                require(!input.TryUse(2,1)&&input.Count(2)==2,"lightning rejects friendly target");
                require(input.TryUse(2,2)&&input.Count(2)==1,"valid enemy target spends once");
            });
            check("skill-input-all-six-commander-slot-mappings",()=>{
                for(int mode=1;mode<=6;mode++){
                    var input=new BattleSkillInput(World(),rules,mode,30,1,new Dictionary<int,int>(),0);
                    for(int slot=0;slot<3;slot++)require(input.SkillId(slot)==(mode-1)*3+slot+1,"slot binding");
                    require(!input.TryUse(0),"no inferred free inventory");
                    require(input.Rule(2).useType==1,"third slot drag");
                }
            });
            check("skill-drag-original-target-indicator-filter-and-clock",()=>{
                var obj=new GameObject("Original skill target indicator replay");
                try{
                    var visual=obj.AddComponent<BattleSkillTargetVisuals>();var w=World();
                    var friendly=w.Tower(1);var enemy=w.Tower(2);
                    require(visual.ShowTarget(0,friendly)&&visual.TargetId==1,"friendly indicator accepts player");
                    var ps=obj.GetComponentInChildren<ParticleSystem>();
                    require(ps!=null&&ps.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture!=null,"original indicator texture/module resolved");
                    require(ps.transform.root==obj.transform&&obj.transform.GetChild(0).position==friendly.Position+Vector3.up*.01f,"source tower world offset");
                    visual.Step(.1f);float clock=ps.time;visual.ShowTarget(0,friendly);visual.Step(.1f);
                    require(ps.time>clock,"continued drag must not restart particle clock every frame");
                    require(!visual.ShowTarget(0,enemy)&&visual.TargetId==0,"friendly rejects enemy");
                    require(visual.ShowTarget(2,enemy)&&visual.TargetId==2,"enemy red indicator");
                    enemy.Camp=0;require(visual.ShowTarget(2,enemy),"source enemy filter includes neutral");
                    require(!visual.ShowTarget(2,friendly),"enemy rejects friendly");
                    require(visual.ShowTarget(8,enemy)&&visual.ShowTarget(8,friendly),"any-tower blue indicator");
                    require(!visual.ShowTarget(4,enemy)&&!visual.ShowTarget(8,null),"ground/null target has no tower indicator");
                    friendly.Active=false;require(!visual.ShowTarget(8,friendly),"inactive tower rejected");
                    visual.Hide();require(visual.TargetId==0&&obj.GetComponentsInChildren<ParticleSystem>().Length==0,"release clears visual and cached target");
                }finally{UnityEngine.Object.DestroyImmediate(obj);}
            });
            check("skill-view-normal-level-binding-and-projected-fire-origin",()=>{
                var obj=new GameObject("skill UI integration fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeNormalLevel(14);
                    require(view.Initialized&&view.Guide.Stage==10,"normal14 guide10");
                    view.Guide.Tick(0,1.1f);require(view.Guide.Confirm(),"confirm");
                    require(view.TryUseSkillSlot(1),"production skill button API");
                    require(view.Simulation.SkillProjectiles.Count>0,"source fireball emitted");
                    var start=view.Simulation.SkillProjectiles[0].Start;var screen=view.BattleCamera.WorldToScreenPoint(start);
                    var center=view.SkillScreenCenter(1);
                    require(Vector2.Distance(center,new Vector2(screen.x,screen.y))<.01f&&Mathf.Abs(screen.z-5)<.001f,"double-camera equivalent projection at depth5");
                    view.InitializeNormalLevel(25);require(view.LevelId==501&&view.Guide.Stage==12,"Scene501 retains normal25 tutorial identity");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("skill-drag-original-all-tower-marker-modes",()=>{
                var obj=new GameObject("Tower skill marker fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeScene(5,30);view.Simulation.AIEnabled=false;
                    var friendly=view.Simulation.Towers.Find(t=>t.Camp==1);var enemy=view.Simulation.Towers.Find(t=>t.Camp!=1);
                    var markers=view.SkillTargets;markers.BeginDrag(2);markers.Step(.1f);
                    require(markers.TargetMode==2&&markers.TowerMark(enemy.Id,true).activeSelf&&markers.TowerMark(friendly.Id,true)==null,"enemy mode highlights every eligible tower without pointer hit");
                    markers.EndDrag();markers.Step(0);require(!markers.TowerMark(enemy.Id,true).activeSelf,"mode0 hides previous red markers");
                    markers.BeginDrag(0);markers.Step(.1f);require(markers.TowerMark(friendly.Id,false).activeSelf,"friendly mode");
                    markers.BeginDrag(8);markers.Step(0);require(markers.TargetMode==3&&markers.TowerMark(friendly.Id,false).activeSelf,"source mode3 leaves existing visibility unchanged");
                    markers.EndDrag();markers.Step(0);markers.BeginDrag(8);markers.Step(0);require(!markers.TowerMark(friendly.Id,false).activeSelf,"mode3 does not activate any marker");
                    markers.BeginDrag(0);markers.Step(0);view.RestartCurrentLevel();require(!markers.TowerMark(friendly.Id,false).activeSelf&&markers.TargetMode==0,"retry initialization hides markers");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("skill2-production-retry-retains-emitted-native-fireball",()=>{
                var obj=new GameObject("Fireball retry visual fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeScene(5,30);view.Simulation.AIEnabled=false;
                    require(view.TryUseSkillSlot(1),"production fireball cast");view.AdvanceFrame(.02f,.02f);
                    var flight=view.Simulation.SkillProjectiles.Find(p=>p.SkillId==2&&p.Active);
                    require(flight!=null,"emitted source Bullet");var original=view.SkillPresentation.VisualObject(flight.VisualId);
                    require(original!=null&&original.GetComponentInChildren<ParticleSystemRenderer>()!=null,"emitted native renderer");
                    view.RestartCurrentLevel();view.AdvanceFrame(.02f,.02f);
                    require(flight.Active&&ReferenceEquals(original,view.SkillPresentation.VisualObject(flight.VisualId))&&original.activeInHierarchy,"retry preserves same in-flight renderer");
                    require(Vector3.Distance(original.transform.position,view.Simulation.SkillProjectileVisualPosition(flight))<.0001f,"retained flight follows current core position after retry");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("guide-confirm-original-player-camp-effect107",()=>{
                var obj=new GameObject("Guide confirmed camp effect fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeNormalLevel(0);view.Guide.Tick(0,1.1f);
                    require(view.Guide.Confirm(),"source OnOK");int id=-100000,count=0;
                    foreach(var tower in view.Simulation.Towers)if(tower.Camp==1)
                    {
                        var effect=view.SkillPresentation.VisualObject(id--);require(effect!=null,"effect107 per player tower");
                        require(effect.transform.position==view.TowerPresentationTransform(tower.Id).position,"source world tower position");
                        var renderer=effect.GetComponentInChildren<ParticleSystemRenderer>();require(renderer!=null&&renderer.sortingLayerID==0&&renderer.sortingOrder==-20,"Default sorting layer/effectOrder-20");count++;
                    }
                    require(count>0&&view.SkillPresentation.VisualObject(id)==null,"no invented enemy camp effects");
                    view.SkillPresentation.Step(3.01f);require(view.SkillPresentation.VisualObject(-100000)==null,"original EffectConfig107 duration3");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("managed-effect-lifetime-starts-at-model-readiness",()=>{
                var obj=new GameObject("Managed readiness lifetime fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeScene(5,30);view.Simulation.AIEnabled=false;
                    SkillVisualEvent shown=null;view.Simulation.SkillVisual+=e=>{if(e.Kind=="effect-show"&&e.EffectId==413)shown=e;};
                    var enemy=view.Simulation.Towers.Find(t=>t.Active&&t.Camp!=0&&t.Camp!=1);
                    float before=enemy.Score;require(view.TryUseSkillSlot(2,enemy.Id),"lightning cast");
                    require(shown!=null&&enemy.Score<before,"damage occurs immediately before presentation readiness");
                    view.SkillPresentation.Step(shown.Duration+1);
                    require(view.SkillPresentation.VisualObject(shown.VisualId)!=null,"a long pre-readiness frame cannot consume the newly created model's lifetime");
                    view.Simulation.Pause(true);view.SkillPresentation.Step(0);view.SkillPresentation.Step(0);
                    require(view.SkillPresentation.VisualObject(shown.VisualId)!=null,"paused scaled time preserves effect");
                    view.Simulation.Pause(false);view.SkillPresentation.Step(shown.Duration-.1f);
                    require(view.SkillPresentation.VisualObject(shown.VisualId)!=null,"full source duration starts after model readiness");
                    view.SkillPresentation.Step(.11f);require(view.SkillPresentation.VisualObject(shown.VisualId)==null,"expires after source duration");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1;Physics.SyncTransforms();}
            });
            check("managed-effect-already-ready-counts-next-frame",()=>{
                var obj=new GameObject("Managed ready lifetime fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeScene(5,30);view.Simulation.AIEnabled=false;
                    SkillVisualEvent shown=null;view.Simulation.SkillVisual+=e=>{if(e.Kind=="effect-show"&&e.EffectId==413)shown=e;};
                    var enemy=view.Simulation.Towers.Find(t=>t.Active&&t.Camp!=0&&t.Camp!=1);
                    require(view.TryUseSkillSlot(2,enemy.Id),"lightning cast");view.SkillPresentation.Flush();
                    require(shown!=null&&view.SkillPresentation.VisualObject(shown.VisualId)!=null,"explicit ready model");
                    view.SkillPresentation.Step(shown.Duration+.01f);
                    require(view.SkillPresentation.VisualObject(shown.VisualId)==null,"already-ready effects must not receive an extra skipped frame");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1;Physics.SyncTransforms();}
            });
            for(int commander=1;commander<=6;commander++)
            {
                int selectedCommander=commander;
                check("skill-view-commander-"+commander+"-three-slots-runtime",()=>{
                    var obj=new GameObject("Commander runtime traversal");
                    try{
                        var view=obj.AddComponent<BattleView>();view.CommanderMode=selectedCommander;
                        for(int slot=0;slot<3;slot++)
                        {
                            // Original layout5 with explicitly injected unlocked progress and local stock.
                            // This isolates each input without modifying the battle's scores or outcome.
                            view.InitializeScene(5,30);require(view.Initialized&&view.Hud!=null,"production view and original HUD");
                            for(int iconSlot=0;iconSlot<3;iconSlot++)
                            {
                                var item=view.Hud.Canvas.transform.Find("SkillUI/main/layout/SkillItem_"+iconSlot);
                                var art=item.Find("img_cover").GetComponent<UnityEngine.UI.Image>();
                                var button=item.Find("btn_normal").GetComponent<UnityEngine.UI.Button>();
                                require(art.gameObject.activeInHierarchy&&art.sprite!=null&&art.color.a>.99f,"source skill art must be visible, not the transparent click target");
                                require(button.targetGraphic==item.Find("bg").GetComponent<UnityEngine.UI.Graphic>(),"serialized sibling targetGraphic retained");
                                var rect=(RectTransform)button.transform;
                                var edge=view.Hud.UICamera.WorldToScreenPoint(rect.TransformPoint(new Vector3(rect.rect.xMax-1,rect.rect.center.y,0)));
                                require(view.SkillContainsPointer(iconSlot,edge),"entire original190px button hitbox is clickable");
                            }
                            var dragArt=view.Hud.Canvas.transform.Find("SkillUI/main/layout/SkillItem_2/img_cover");
                            view.Hud.MoveSkillArtwork(2,new Vector2(100,200),true);
                            require(Vector3.Distance(view.Hud.UICamera.WorldToScreenPoint(dragArt.position),new Vector3(100,250,5))<.001f,"original skill artwork pointer offset/projection");
                            view.Hud.MoveSkillArtwork(2,Vector2.zero,false);require(dragArt.localPosition==Vector3.zero,"drag release returns artwork");
                            view.Simulation.Connect(3,1);
                            for(int frame=0;frame<60;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                            int id=view.SkillInput.SkillId(slot),before=view.SkillInput.Count(slot),target=0;
                            if(id==3||id==12)target=view.Simulation.Towers.Find(t=>t.Active&&t.Camp!=0&&t.Camp!=1).Id;
                            if(id==6||id==9||id==15)target=view.Simulation.Towers.Find(t=>t.Active&&t.Camp==1).Id;
                            require(view.TryUseSkillSlot(slot,target,new Vector3(0,0,2.5f)),"skill "+id+": "+view.SkillInput.Rejection);
                            require(view.SkillInput.Count(slot)==before-1,"consume exactly once through production input");
                            for(int frame=0;frame<180;frame++)view.AdvanceFrame(1f/60f,1f/60f);
                            require(view.SkillPresentation.MissingResourceCount==0,"all exercised native skill resources resolved");
                            var state=view.Simulation.FindSkill(id);var progress=view.Hud.Canvas.transform.Find("SkillUI/main/layout/SkillItem_"+slot+"/img_progress").GetComponent<UnityEngine.UI.Image>();
                            float expected=state.Active&&state.Parameters.duration>0?state.Elapsed/state.Parameters.duration:0;
                            require(Mathf.Abs(progress.fillAmount-Mathf.Clamp01(expected))<.0001f,"original elapsed/duration UI mask, cleared on end");
                            foreach(var tower in view.Simulation.Towers)require(!float.IsNaN(tower.Score)&&!float.IsInfinity(tower.Score),"finite runtime score");
                            view.Simulation.Pause(true);float clock=view.Simulation.Elapsed;
                            view.AdvanceFrame(0,1);require(view.Simulation.Elapsed==clock,"production pause clock");
                            view.Simulation.Pause(false);view.RestartCurrentLevel();
                            require(view.SkillInput.Count(slot)==before-1,"retry must retain spent inventory");
                        }
                    }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
                });
            }
            return report;
        }
    }
}
