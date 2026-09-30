using System;
using System.IO;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    // Static-derived golden cases, distinct from the extracted original WASM oracle.
    public static class SkillValidation
    {
        [Serializable] private sealed class SkillRows { public SkillParameters[] Datas; }
        [Serializable] private sealed class BossRows { public BossParameters[] Datas; }
        [Serializable] private sealed class ShipRows { public ShipConfig[] Datas; }
        [Serializable] private sealed class DispatchRows { public DispatchConfig[] Datas; }
        private static string Table(string name) { return File.ReadAllText(Path.Combine(BattleBuild.Target, "generated/tables/" + name + ".json")); }
        private static BattleSimulation World(int bossCamp = 0)
        {
            var layout = new LevelLayout { StarInfoCfgs = new[] {
                new StarInfoCfg { CampID=1,ShipID=1,StartScore=10,pos=new IntVector3{x=0,z=100} },
                new StarInfoCfg { CampID=1,ShipID=1,StartScore=10,pos=new IntVector3{x=100,z=100} },
                new StarInfoCfg { CampID=bossCamp==0?2:bossCamp,ShipID=1,StartScore=65,pos=new IntVector3{x=0,z=300},isBoss=bossCamp!=0,bossSkillId=bossCamp==6?998:999 },
                new StarInfoCfg { CampID=0,ShipID=1,StartScore=3,pos=new IntVector3{x=200,z=400} }
            } };
            var cfg = new BattleConfigData { Ships=JsonUtility.FromJson<ShipRows>(Table("SoldierConfig")).Datas,
                Dispatch=JsonUtility.FromJson<DispatchRows>(Table("DispatchConfig")).Datas };
            var w = new BattleSimulation(layout,cfg,4305,(a,b)=>true) { AIEnabled=false,BossAIEnabled=false };
            w.ConfigureSkills(JsonUtility.FromJson<SkillRows>(Table("SkillConfig")).Datas,JsonUtility.FromJson<BossRows>(Table("BossConfig")).Datas);
            foreach(var t in w.Towers)t.AutoAddScore=false;
            if(bossCamp==0)w.Tower(3).Score=10;
            return w;
        }
        private static void Require(bool value,string detail) { if(!value)throw new InvalidOperationException(detail); }
        private static void Near(float actual,float expected,string detail) { Require(Mathf.Abs(actual-expected)<0.00001f,detail+": "+actual+" != "+expected); }
        private static void Cast(BattleSimulation w,int id,int target=0,Vector3? point=null,Vector3? origin=null)
        { Require(w.CastSkill(id,1,1,target,point,origin),w.LastSkillRejection); }
        private static SoldierState Soldier(BattleSimulation w,int source=1,int target=3)
        { var line=w.FindLine(source,target); if(!line.IsFrom(source))Require(w.Connect(source,target,true),"fixture connect");var s=w.SpawnSoldier(source,target);Require(s!=null,"fixture spawn");return s; }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report { passed=true,unityVersion=Application.unityVersion,
                limitations="Skill tests are deterministic cases derived from recovered functions. They do not prove original UI launch geometry, frame-order fidelity, asynchronous restart behavior, or full visual equivalence. TowerBuff creation remains unknown." };
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("skill01-freeze-and-remove-enemy-soldiers",()=>{
                var w=World();var enemy=Soldier(w,3,1);var visuals=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=visuals.Add;Cast(w,1);
                Require(w.Tower(3).Mode==1&&!enemy.Active&&!enemy.PlayDeathAnimation&&w.Tower(4).Mode==0,"freeze excludes neutral and immediately clears enemy soldier without death animation");
                Require(visuals.FindAll(e=>e.Kind=="tower-ice-begin").Count==1&&visuals.Exists(e=>e.Kind=="tower-ice-begin"&&e.TowerId==3),"only frozen enemy receives original embedded ice begin");
                w.Tick(5);Require(w.Tower(3).Mode==0&&!w.IsSkillActive(1),"duration cleanup");
                Require(visuals.FindAll(e=>e.Kind=="tower-ice-melt").Count==1&&visuals.Exists(e=>e.Kind=="tower-ice-melt"&&e.TowerId==3),"actual unfreeze requests source skill=1 melt");
            });
            check("skill-visual-aim-delay-is-scaled-and-independent-of-mechanic-tick",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,12,3);Require(events.FindAll(e=>e.EffectId==417).Count==1,"initial aim effect once");
                w.TickSkillCoroutines(0f);w.TickSkillCoroutines(0.79f);
                Require(!events.Exists(e=>e.EffectId==419),"no early delayed hit effect");
                foreach(var t in w.Towers)t.Camp=1;w.EvaluateOutcome();Require(w.State==BattlePhase.Victory,"fixture result");w.TickSkillCoroutines(0.02f);
                Require(events.FindAll(e=>e.EffectId==419).Count==1,"scaled wait continues on result without mechanic tick");
                Near(w.Tower(3).Score,10,"visual callback does not apply damage");
            });
            check("skill02-retry-stops-launches-but-retains-flying-bullet",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                w.Tower(1).Active=w.Tower(2).Active=w.Tower(4).Active=false;
                Cast(w,2,origin:Vector3.zero);var flight=w.SkillProjectiles[0];var run=w.FindSkill(2);w.TickSkillProjectiles(.1f);
                w.Restart();float score=w.Tower(3).Score;events.Clear();
                Require(w.SkillProjectiles.Count==1&&ReferenceEquals(flight,w.SkillProjectiles[0])&&flight.Active,"already launched Bullet identity retained");
                Require(ReferenceEquals(run,w.FindSkill(2))&&!run.Active,"same cached player skill reset inactive");
                Near(flight.Elapsed,.1f,"retry retains flight clock");w.TickSkillCoroutines(10);
                Require(w.SkillProjectiles.Count==1&&!events.Exists(e=>e.Kind=="projectile-spawn"),"Reset cancels remaining launch coroutine");
                w.Tower(3).Camp=1;w.TickSkillProjectiles(10);
                Near(w.Tower(3).Score,Mathf.Min(w.Tower(3).MaxScore,score+(int)run.Parameters.data3),"old callback reads current target camp and restored score");
                Require(!flight.Active&&w.SkillProjectiles.Count==0&&events.Exists(e=>e.Kind=="effect-show"&&e.EffectId==404),"one friendly arrival then source pooling");
            });
            check("skill12-retry-retains-await-and-reuses-current-target",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,12,3);var original=w.FindSkill(12);w.TickSkillCoroutines(.5f);w.Restart();
                Require(ReferenceEquals(original,w.FindSkill(12))&&!original.Active&&original.TargetId==0,"player commander keeps skill object while Reset clears target and active");
                Cast(w,12,4);events.Clear();w.TickSkillCoroutines(.31f);
                var oldShot=events.Find(e=>e.EffectId==419);
                Require(oldShot!=null&&oldShot.TowerId==4&&oldShot.Position==w.Tower(4).Position,"old await keeps remaining delay and reads replacement target");
                Require(events.FindAll(e=>e.Kind=="audio-play"&&(e.AudioId==2032||e.AudioId==2033)).Count==2,"old valid continuation preserves its two voices");
                w.Tower(4).Active=false;w.Tower(4).Camp=1;w.TickSkillCoroutines(.50f);
                Require(events.FindAll(e=>e.EffectId==419).Count==2,"new await also resumes without rechecking active/camp");
                Near(w.Tower(4).Score,3,"both continuations are presentation only");
            });
            check("skill12-retry-null-target-is-explicit-unresolved-continuation",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,12,3);w.TickSkillCoroutines(.4f);w.Restart();events.Clear();
                w.TickSkillCoroutines(0);Require(events.Count==0,"pause does not resume scaled await");w.TickSkillCoroutines(.41f);
                Require(events.FindAll(e=>e.Kind=="async-target-unresolved").Count==1,"retained await reaches cleared target instead of being cancelled");
                Require(!events.Exists(e=>e.EffectId==419||e.Kind=="audio-play"),"no invented target position, voice or runtime fault");
            });
            check("skill12-reset-closes-current-handles-after-overlapping-awaits",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,12,3);w.TickSkillCoroutines(.5f);w.Restart();Cast(w,12,4);events.Clear();
                w.TickSkillCoroutines(.31f);int first=events.Find(e=>e.EffectId==419).VisualId;
                w.TickSkillCoroutines(.50f);var shots=events.FindAll(e=>e.EffectId==419);int latest=shots[1].VisualId;
                events.Clear();w.Restart();
                Require(events.Exists(e=>e.Kind=="effect-close"&&e.VisualId==latest),"Reset closes current shot field56");
                Require(!events.Exists(e=>e.Kind=="effect-close"&&e.VisualId==first),"overwritten shot handle is not in source Reset field56");
            });
            check("skill-visual-serial-effect-once-and-subscriber-failure-isolated",()=>{
                var w=World();int effects=0;
                w.SkillVisual+=e=>{if(e.Kind=="effect-show"&&e.SkillId==15)effects++;};
                w.SkillVisual+=e=>{throw new InvalidOperationException("intentional broken presentation subscriber");};
                Cast(w,15,3);float afterFirst=w.Tower(3).Score;
                w.TickSkillCoroutines(1f);
                Require(effects==1&&w.Tower(3).Score<afterFirst,"serial ticks continue without re-showing effect or subscriber interruption");
            });
            check("skill-visual-rain-outquad-does-not-change-mechanical-arrival",()=>{
                var w=World();Cast(w,10);var p=w.SkillProjectiles[0];
                w.TickSkillProjectiles(0.5f);
                Near(p.Position.y,5,"mechanical reference position retained");
                Near(w.SkillProjectileVisualPosition(p).y,2.5f,"source DOMoveY OutQuad presentation");
                Require(p.Active,"not arrived early");w.TickSkillProjectiles(0.5f);Require(!p.Active,"lands at original one second");
            });
            check("skill-visual-poison-projectile-ground-order-no-mechanical-projectile",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,18,point:new Vector3(0,0,3),origin:new Vector3(2,1,0));
                Require(w.SkillProjectiles.Count==0,"poison visual does not join mechanical projectile list");
                var spawn=events.Find(e=>e.Kind=="projectile-spawn");
                Require(spawn!=null&&spawn.SkillId==18&&spawn.LocalScale==Vector3.one*150,"bottle original scale");
                Near(spawn.Duration,1,"bottle duration");Near(spawn.ControlPoint.y,2,"midpoint plus arc1.5");
                w.TickSkillCoroutines(1);
                int arrival=events.FindIndex(e=>e.Kind=="projectile-arrival"),ground=events.FindIndex(e=>e.Kind=="poison-ground-show"),hide=events.FindIndex(e=>e.Kind=="projectile-hide");
                Require(arrival>=0&&ground>arrival&&hide>ground,"arrival activates ground before hiding bottle");
                Near(w.Tower(3).Score,10,"visual-only arrival does not tick poison");
            });
            check("skill-visual-recruit-and-attached-effects-source-binding",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,14);var recruit=events.Find(e=>e.Kind=="recruit-spawn");
                Require(recruit!=null&&recruit.SkinType==1&&recruit.Orientation=="camera-tilt-flip"&&!recruit.LookAt&&recruit.Parent=="world","recruit selected skin, world parent and source billboard orientation");
                Require(events.FindAll(e=>e.EffectId==521).Count==1,"one summon effect for initial recruit");
                Cast(w,13);var s=Soldier(w);
                Require(events.Exists(e=>e.EffectId==511&&e.SoldierId==s.Id&&e.PositionIsLocal),"new friendly soldier gets local attached511");
            });
            check("skill-audio-fireball-order-and-conditional-unfreeze",()=>{
                var w=World();var order=new System.Collections.Generic.List<string>();
                w.Event+=e=>{if(e.Kind=="score")order.Add("score");};
                w.SkillVisual+=e=>{if(e.Kind=="effect-show")order.Add("effect"+e.EffectId);if(e.Kind=="audio-play")order.Add("audio"+e.AudioId);};
                w.Tower(1).Active=w.Tower(2).Active=w.Tower(4).Active=false;
                Cast(w,2,origin:Vector3.zero);Require(order.Contains("audio3121"),"fireball launch uses3121");order.Clear();
                w.TickSkillProjectiles(10);
                Require(order.IndexOf("score")>=0&&order.IndexOf("effect408")>order.IndexOf("score")&&order.IndexOf("audio3122")>order.IndexOf("effect408"),"source order score then impact effect then voice");
                Near(w.Tower(3).Score,8,"presentation ordering retains damage");
                w=World();int unfreeze=0;w.SkillVisual+=e=>{if(e.Kind=="audio-play"&&e.AudioId==2018)unfreeze++;};
                Cast(w,1);w.Tower(3).Mode=0;w.Tick(5);Require(unfreeze==0,"no2018 if no enemy tower remains frozen at end");
                Cast(w,1);w.Tick(5);Require(unfreeze==1,"one2018 when an active enemy is unfrozen");
            });
            check("skill-audio-poison-handles-end-and-restart",()=>{
                var w=World();var events=new System.Collections.Generic.List<SkillVisualEvent>();w.SkillVisual+=events.Add;
                Cast(w,18,point:new Vector3(0,0,3),origin:Vector3.zero);w.TickSkillCoroutines(1);
                var play=events.Find(e=>e.Kind=="audio-play"&&e.AudioId==2041);
                Require(play!=null&&play.HandleId>0&&play.VoiceMode==1&&play.AudioRoute=="ui-audio","2041 is independent parallel handle, direct UI route");
                Require(events.FindIndex(e=>e.Kind=="audio-play"&&e.AudioId==2040)<events.IndexOf(play),"landing2040 precedes2041");
                w.Tick(7);Require(events.Exists(e=>e.Kind=="audio-stop"&&e.HandleId==play.HandleId),"SkillEnd requests fade stop of exact playback");
                w=World();events.Clear();w.SkillVisual+=events.Add;
                Cast(w,18,point:new Vector3(0,0,3),origin:Vector3.zero);
                Require(w.CastSkill(18,1,2,groundPoint:new Vector3(0,0,3),projectileOrigin:Vector3.zero,agentCast:true),"enemy poison fixture");
                w.TickSkillCoroutines(1);var plays=events.FindAll(e=>e.Kind=="audio-play"&&e.AudioId==2041);
                Require(plays.Count==2&&plays[0].HandleId!=plays[1].HandleId,"opposing commanders retain distinct handles");
                w.Restart();int reset=events.FindIndex(e=>e.Kind=="battle-reset");
                foreach(var p in plays){int stop=events.FindIndex(e=>e.Kind=="audio-stop"&&e.HandleId==p.HandleId);Require(stop>=0&&stop<reset,"each retained audio handle stops before reset presentation cleanup");}
            });
            check("soldier-death-contact-detaches-and-cannot-hit-again",()=>{
                var w=World();var a=Soldier(w);var b=Soldier(w,3,1);
                w.ResolveSoldierContact(a,b);
                Require(!a.Active&&!b.Active&&a.PlayDeathAnimation&&b.PlayDeathAnimation,"contact death requests visual retention only");
                Require(a.LineId==0&&b.LineId==0,"dying soldiers leave combat lists immediately");
                var survivor=Soldier(w);int hp=survivor.HP;w.ResolveSoldierContact(a,survivor);
                Require(survivor.HP==hp,"dying soldier cannot damage another soldier");
            });
            check("soldier-arrival-removes-without-death-animation",()=>{
                var w=World();var s=Soldier(w);w.ArriveSoldier(s,w.Tower(3));
                Require(!s.Active&&!s.PlayDeathAnimation&&s.LineId==0,"arrival immediate removal");
            });
            check("score-presentation-keeps-pre-capture-camp-and-requested-delta",()=>{
                var w=World();BattleEvent score=null;w.Event+=e=>{if(e.Kind=="score")score=e;};
                w.ChangeScore(3,1,-11);
                Require(score!=null&&score.PreviousCamp==2&&score.SourceCamp==1&&score.Camp==1,"hit effect uses original target camp even after capture");
                Near(score.Amount,-11,"unclamped requested delta");Near(score.Value,1,"resulting captured score");
            });
            check("ordinary-audio-player-contact-and-wayline-arrival-only",()=>{
                var w=World();int contacts=0,arrivals=0;w.Event+=e=>{if(e.AudioId==2013)contacts++;if(e.AudioId==2012)arrivals++;};
                var a=Soldier(w);var b=Soldier(w,3,1);a.Camp=2;b.Camp=3;w.ResolveSoldierContact(a,b);
                Require(contacts==0,"nonplayer contact death is silent");
                a=Soldier(w);b=Soldier(w,3,1);w.ResolveSoldierContact(a,b);Require(contacts==1,"player contact pair requests one sound");
                a=Soldier(w);w.ArriveSoldier(a,w.Tower(3));Require(arrivals==0,"direct arrival API does not issue WayLine sound");
                b=Soldier(w,3,1);b.Position=w.Tower(1).Position;w.Tick(.001f);
                Require(arrivals==1,"WayLine arrival at player tower requests2012 after processing");
            });
            check("skill02-fireball-damage-and-required-origin",()=>{
                var w=World();Require(!w.CastSkill(2,1),"missing launch position must be explicit");
                w.Tower(1).Active=w.Tower(2).Active=w.Tower(4).Active=false;
                Cast(w,2,origin:new Vector3(0,0,1));Require(w.SkillProjectiles.Count==1,"first fireball immediate");
                w.TickSkillProjectiles(10);Near(w.Tower(3).Score,8,"2 damage");Require(w.Tower(3).Camp==2,"no capture");
                w.TickSkillCoroutines(0.5f);Require(w.SkillProjectiles.Count==1,"next ball at duration/count");
            });
            check("skill02-original-bezier-and-up-offset",()=>{
                var w=World();w.Tower(1).Active=w.Tower(2).Active=w.Tower(4).Active=false;
                Cast(w,2,origin:new Vector3(0,0,1));var p=w.SkillProjectiles[0];
                Near(p.End.y,0.2f,"target vertical offset");Near(p.End.z,3f,"no invented forward offset");
                Near(p.Duration,0.5f,"duration measures to unoffset tower position");
                Near(p.ControlPoint.y,1.6f,"midpoint+up*1.5");p.Elapsed=p.Duration*0.5f;Near(p.Position.y,0.85f,"quadratic midpoint arc/2");
            });
            check("bullet-zero-arc-random-side-control-point",()=>{
                Vector3 a=new Vector3(0,1,0),b=new Vector3(1,0,0);
                var positive=BattleSimulation.BulletControlPoint(a,b,0,true);var negative=BattleSimulation.BulletControlPoint(a,b,0,false);
                Near(positive.x,0.5f,"midpoint x");Near(positive.y,0.5f,"midpoint y");Near(positive.z,0.5f,"cross normal positive");Near(negative.z,-0.5f,"cross normal negative");
            });
            check("skill03-strike-cannot-capture",()=>{var w=World();Cast(w,3,3);Near(w.Tower(3).Score,0,"20 damage clamped");Require(w.Tower(3).Camp==2,"ownership unchanged");});
            check("skill04-own-production-multiplier",()=>{var w=World();Cast(w,4);Near(w.SpawnMultiplier(w.Tower(1)),1.8f,"own");Near(w.SpawnMultiplier(w.Tower(3)),1,"enemy");});
            check("enemy-commander-multipliers-are-not-consumed",()=>{
                var w=World();Require(w.CastSkill(4,1,2)&&w.CastSkill(5,1,2)&&w.CastSkill(11,1,2),"enemy skills active");
                Near(w.SpawnMultiplier(w.Tower(1)),1,"player dispatch consults player commander only");
                Near(w.SpawnMultiplier(w.Tower(3)),1,"enemy dispatch consults player commander only");
                var s=Soldier(w,3,1);Near(w.SpeedMultiplier(s),1,"enemy11 speed override not consulted");
                Require(w.Tower(1).Mode==6,"enemy11 explicit player-tower visual state side effect preserved");
            });
            check("player-skill-expires-after-tower-update-before-lines",()=>{
                var w=World();w.Tower(3).AutoAddScore=true;w.Tower(3).RegenAccumulator=1.9f;
                Cast(w,1);w.Tick(5f);
                Near(w.Tower(3).Score,10,"tower remains frozen for its final active frame");
                Require(!w.IsSkillActive(1)&&w.Tower(3).Mode==0,"skill ends after tower pass");
                w.Tick(.11f);Near(w.Tower(3).Score,11,"regen resumes next frame with retained accumulator");
            });
            check("skill05-other-production-multiplier",()=>{var w=World();Cast(w,5);Near(w.SpawnMultiplier(w.Tower(3)),0.7f,"enemy");Near(w.SpawnMultiplier(w.Tower(1)),1,"own");});
            check("skill06-development-score",()=>{var w=World();Cast(w,6,1);Near(w.Tower(1).Score,30,"+20");Require(w.Tower(1).Grade==2,"upgraded");});
            check("skill07-bat-outgoing-only-and-repeated-callback",()=>{
                var w=World();Require(w.Connect(3,4,true)&&w.Connect(1,3),"fixture lines");Cast(w,7);
                w.TickSkillProjectiles(1.5f);Require(w.Tower(3).Mode==4&&w.Tower(3).OutgoingCount==0&&w.Tower(1).OutgoingCount==1,"only outgoing removed");
                w.Tower(3).Camp=1;w.TickSkillProjectiles(0);Require(w.Tower(3).Mode==0,"landed bat rechecks ownership on later Update");
            });
            check("skill08-existing-new-and-restored-hp",()=>{
                var w=World();var old=Soldier(w);Cast(w,8);Near(old.HP,10000,"existing finite HP");var fresh=Soldier(w);Near(fresh.HP,100000,"new finite HP");
                w.Tick(6);Near(old.HP,1,"restored baseline");Near(fresh.HP,1,"fresh baseline");Require(w.Tower(1).Mode==0,"tower state cleared");
            });
            check("skill09-clamped-drain-not-conserved",()=>{
                var w=World();w.Tower(3).Score=2;Cast(w,9,1);Near(w.Tower(3).Score,0,"source clamp");w.TickSkillProjectiles(0.7f);Near(w.Tower(1).Score,15,"heals full5 even source had2");Require(w.Tower(3).Camp==2,"source cannot capture");
            });
            check("skill10-arrow-rain-tower-channel",()=>{
                var w=World();Cast(w,10);Near(w.Tower(3).Score,10,"wait0.3");w.TickSkillCoroutines(0.3f);Near(w.Tower(3).Score,8,"enemy2");Near(w.Tower(4).Score,1,"other includes neutral");Near(w.Tower(1).Score,10,"friendly excluded");
            });
            check("skill10-arrow-clears-first-soldier-regardless-hp",()=>{
                var w=World();var a=Soldier(w,3,1);var b=Soldier(w,3,1);Cast(w,10);var arrow=w.SkillProjectiles[0];
                for(int i=1;i<w.SkillProjectiles.Count;i++)w.SkillProjectiles[i].Active=false;
                a.Position=b.Position=arrow.End;a.HP=b.HP=100000;w.TickSkillProjectiles(1);
                Require(!a.Active&&b.Active,"one arrow clears only first candidate");
            });
            check("skill11-spawn-and-speed",()=>{
                var w=World();var s=Soldier(w);Cast(w,11);Near(w.SpawnMultiplier(w.Tower(1)),1.8f,"dispatch");Near(w.SpeedMultiplier(s),1.8f,"soldier speed");Require(w.Tower(1).Mode==6,"effect state");
            });
            check("skill12-tick-no-catchup",()=>{
                var w=World();w.Tower(3).Score=20;Cast(w,12,3);w.Tick(2.5f);Near(w.Tower(3).Score,15,"only one5damage");Near(w.FindSkill(12).DamageTimer,1.5f,"retain remainder");
            });
            check("skill13-new-soldier-only",()=>{
                var w=World();var old=Soldier(w);Cast(w,13);var fresh=Soldier(w);Require(old.Attack==1&&old.Occupy==1,"existing unaffected");Require(fresh.Attack==100&&fresh.Occupy==2,"new +99attack andx2occupy");
            });
            check("skill14-recruit-count-and-can-capture",()=>{
                var w=World();Cast(w,14);Require(w.RecruitedSoldiers.Count==1,"first immediate");
                for(int i=0;i<24;i++)w.TickSkillCoroutines(0.20001f);Require(w.RecruitedSoldiers.Count==25,"25 config units");
                var unit=w.RecruitedSoldiers[0];w.Tower(3).Score=1;unit.Position=w.Tower(3).Position+Vector3.right*0.05f;unit.Direction=Vector3.zero;
                w.Tick(0);Require(w.Tower(3).Camp==1,"recruit captures at exact0");Near(w.Tower(3).Score,0,"score0");
            });
            check("skill15-serial-count-cannot-capture",()=>{
                var w=World();w.Tower(3).Score=40;Cast(w,15,3);Near(w.Tower(3).Score,39,"first immediate");
                for(int i=0;i<29;i++)w.TickSkillCoroutines(1.7f/30f+0.000001f);Near(w.Tower(3).Score,10,"30 total");
                Require(w.Tower(3).Camp==2,"owner");
            });
            check("skill15-owner-change-stops-sequence",()=>{
                var w=World();Cast(w,15,3);w.Tower(3).Camp=1;w.TickSkillCoroutines(1);Near(w.Tower(3).Score,9,"no later tick after camp change");
            });
            check("skill15-restart-retains-reference-delay-and-remaining-count",()=>{
                var w=World();var target=w.Tower(3);Cast(w,15,3);w.TickSkillCoroutines(.02f);w.Restart();
                Require(ReferenceEquals(target,w.Tower(3)),"restart reuses tower identity");
                Require(!w.IsSkillActive(15),"commander active state reset independently");
                Near(target.Score,65,"layout initial score restored");
                w.TickSkillCoroutines(.03f);Near(target.Score,65,"retained wait has not elapsed");
                w.TickSkillCoroutines(.007f);Near(target.Score,64,"remaining wait continues across restart");
                for(int i=0;i<28;i++)w.TickSkillCoroutines(1.7f/30f+.000001f);
                Near(target.Score,36,"only29 old ticks remained after immediate pre-restart tick");
                w.TickSkillCoroutines(10);Near(target.Score,36,"sequence does not restart its count");
            });
            check("skill15-inactive-at-resume-terminates-before-restart",()=>{
                var w=World();var target=w.Tower(3);Cast(w,15,3);target.Active=false;w.TickSkillCoroutines(.06f);
                target.Active=true;w.Restart();w.TickSkillCoroutines(10);
                Near(target.Score,65,"observed inactive terminates old sequence permanently");
            });
            check("skill15-different-camp-at-resume-terminates-before-restart",()=>{
                var w=World();var target=w.Tower(3);Cast(w,15,3);target.Camp=1;w.TickSkillCoroutines(.06f);
                w.Restart();w.TickSkillCoroutines(10);
                Near(target.Score,65,"observed camp mismatch with null callback terminates old sequence");
            });
            check("skill15-restart-restores-camp-before-continuation",()=>{
                var w=World();var target=w.Tower(3);Cast(w,15,3);target.Camp=1;
                w.Restart();w.TickSkillCoroutines(.06f);
                Near(target.Score,64,"unobserved camp change does not cancel reference-held job");
                Require(target.Camp==2,"restart restored original camp");
            });
            check("skill15-old-job-does-not-retarget-replacement-id",()=>{
                var w=World();var original=w.Tower(3);Cast(w,15,3);w.Restart();
                var replacement=new TowerState{Id=original.Id,Camp=2,Score=23,MaxScore=65,ShipID=1,Active=true};
                w.Towers[2]=replacement;w.TickSkillCoroutines(.06f);
                Near(original.Score,64,"old active tower reference receives its tick");
                Near(replacement.Score,23,"same numeric ID does not redirect continuation");
            });
            check("skill16-new-enemy-attack-reinforce-only",()=>{
                var w=World();var old=Soldier(w,3,1);Cast(w,16);var fresh=Soldier(w,3,1);
                Require(old.Attack==1&&old.Reinforce==1,"existing unaffected");Require(fresh.Attack==0&&fresh.Reinforce==0&&fresh.Occupy==1&&fresh.HP==1,"occupy and HP preserved");
            });
            check("skill17-initial-friendly-snapshot",()=>{
                var w=World();Cast(w,17);Near(w.Tower(1).Score,13,"first immediate");w.Tower(3).Camp=1;w.TickSkillCoroutines(1);
                Near(w.Tower(1).Score,16,"second pass");Near(w.Tower(3).Score,10,"later friendly excluded");
            });
            check("skill18-independent-tower-and-soldier-channels",()=>{
                var w=World();var enemy=Soldier(w,3,1);enemy.HP=100000;enemy.Position=w.Tower(3).Position;
                Cast(w,18,point:w.Tower(3).Position);w.Tick(2.5f);Near(w.Tower(3).Score,8,"one2damage despite2.5s");Require(!enemy.Active,"poison clears regardlessHP");Near(w.FindSkill(18).DamageTimer,1.5f,"damage remainder");
            });
            check("skill-clocks-pause-freezes-scaled-wait",()=>{
                var w=World();Cast(w,15,3);w.Pause(true);w.Tick(10);w.TickSkillCoroutines(0);Near(w.Tower(3).Score,9,"paused");w.Pause(false);w.TickSkillCoroutines(0.06f);Near(w.Tower(3).Score,8,"resumed");
            });
            check("skill-result-fireball-callback-has-no-running-gate",()=>{
                var w=World();w.Tower(1).Active=w.Tower(2).Active=w.Tower(4).Active=false;Cast(w,2,origin:Vector3.zero);w.EvaluateOutcome();Require(w.State==BattlePhase.Defeat,"fixture result");w.TickSkillProjectiles(10);Near(w.Tower(3).Score,8,"unconditional callback continues at Result");
            });
            check("skill-result-drain-callback-is-gated",()=>{
                var w=World();Cast(w,9,1);w.Tower(1).Active=w.Tower(2).Active=false;w.EvaluateOutcome();w.TickSkillProjectiles(1);Near(w.Tower(1).Score,10,"no heal outside Running");
            });
            check("boss999-init-no-capture-and-strike1",()=>{
                var w=World(5);var b=w.Tower(3);Require(b.IsBoss&&!b.AutoAddScore&&b.ShipID==11,"boss init");Near(b.MaxScore,65,"initial maximum");Near(b.CollisionRadius,0.15f,"radius");
                w.Tower(2).Active=false;Require(w.ExecuteBossAction(3,1),"action1");w.TickSkillCoroutines(1);Near(w.Tower(1).Score,0,"20 damage clamp");Require(w.Tower(1).Camp==1,"strike cannot capture");
                w.ChangeScore(3,1,-100);Require(b.Camp==5,"boss itself cannot capture");Near(b.Score,0,"boss clamp");
            });
            check("boss999-volley-friendly-heal",()=>{
                var w=World(5);w.Tower(1).Active=w.Tower(4).Active=false;w.Tower(2).Camp=5;
                Require(w.ExecuteBossAction(3,2),"action2");w.TickSkillCoroutines(1);Require(w.SkillProjectiles.Count==1,"first after1s");
                w.TickSkillProjectiles(10);Near(w.Tower(2).Score,12,"camp5 healed2");w.TickSkillCoroutines(0.5f);Require(w.SkillProjectiles.Count==1,"second at5/10");
            });
            check("boss102-native-bullet-spawn-and-hit-presentation-order",()=>{
                var w=World(5);w.Tower(2).Active=w.Tower(4).Active=false;
                var events=new System.Collections.Generic.List<SkillVisualEvent>();var order=new System.Collections.Generic.List<string>();
                w.SkillVisual+=e=>{events.Add(e);order.Add(e.Kind=="audio-play"?"audio"+e.AudioId:e.Kind=="effect-show"?"effect"+e.EffectId:e.Kind);};
                w.Event+=e=>{if(e.Kind=="score")order.Add("score");};
                Require(w.ExecuteBossAction(3,2),"source volley");w.TickSkillCoroutines(1);
                var spawn=events.Find(e=>e.Kind=="projectile-spawn");
                Require(spawn!=null&&spawn.SkillId==102&&spawn.SourceTowerId==3&&spawn.Parent=="boss-bullet-root"&&spawn.EntityId==0,"native boss child clone uses owning bulletRoot, not invented entity102");
                Require(!spawn.LookAt&&!spawn.OverrideScale&&!spawn.OverrideEuler&&spawn.RotationSpeed==50,"source Bullet rotation50 and pool reuse preserve local transforms");
                Near(spawn.Duration,1,"distance2/2 flight");Near(spawn.ControlPoint.y,.5f,"source arc half unit");Require(events.Exists(e=>e.AudioId==3121),"launch voice");
                events.Clear();order.Clear();w.TickSkillProjectiles(10);
                Require(order.IndexOf("projectile-hide")>=0&&order.IndexOf("audio3122")>order.IndexOf("projectile-hide")&&order.IndexOf("score")>order.IndexOf("audio3122")&&order.IndexOf("effect408")>order.IndexOf("score"),"source hide/pool then unconditional voice then score then impact effect");
                Require(events.FindAll(e=>e.AudioId==3122).Count==2,"enemy hit keeps both unconditional and branch3122 calls");Near(w.Tower(1).Score,8,"presentation does not change original hit2");
            });
            check("boss998-rain-radius-damage-and-cleanup",()=>{
                var w=World(6);w.Tower(2).Active=false;Require(w.ExecuteBossAction(3,5),"action5");w.Tick(0.51f);Require(w.Tower(1).Mode==3,"strict halfsecond scan");
                Near(w.SpawnMultiplier(w.Tower(1)),0.5f,"rain production");w.Tick(0.5f);Near(w.Tower(1).Score,10,"earlier tower first accumulates rain next frame");
                w.Tick(0.5f);Near(w.Tower(1).Score,7,"3 damage after rain timer1");
                w.Tick(3.51f);Require(w.Tower(1).Mode==0,"duration>5 cleanup");
            });
            check("boss998-rain-and-regen-share-score-update",()=>{
                var w=World(6);var t=w.Tower(1);t.AutoAddScore=true;t.Mode=3;t.RegenAccumulator=1f;
                int scoreEvents=0;w.Event+=e=>{if(e.Kind=="score"&&e.TowerId==1)scoreEvents++;};w.Tick(1);
                Near(t.Score,8,"regen1 plus rain-3");Require(scoreEvents==1,"combined delta generates one score update");
            });
            check("boss998-strike6",()=>{
                var w=World(6);w.Tower(2).Active=false;w.Tower(1).Score=30;Require(w.ExecuteBossAction(3,6),"action6");w.TickSkillCoroutines(1);Near(w.Tower(1).Score,10,"20 damage");
            });
            check("boss-noop-stops-prior-coroutine",()=>{
                var w=World(5);w.Tower(2).Active=false;w.ExecuteBossAction(3,1);w.ExecuteBossAction(3,3);w.TickSkillCoroutines(2);Near(w.Tower(1).Score,10,"action3 stops pending action1 without new behavior");
            });
            check("boss-outcome-truncation-boundary",()=>{
                var w=World(5);w.Tower(3).Score=1f;w.EvaluateOutcome();Require(w.State==BattlePhase.Running,"score1 remains running");
                w.Tower(3).Score=0.5f;w.EvaluateOutcome();Require(w.State==BattlePhase.Victory,"trunc0.5==0 wins");
            });
            check("boss-outcome-player-loss-precedes-boss-death",()=>{
                var w=World(5);w.Tower(1).Camp=w.Tower(2).Camp=2;w.Tower(3).Score=0;w.EvaluateOutcome();Require(w.State==BattlePhase.Defeat,"player absence takes precedence");
            });
            check("boss-delay-crossing-drops-overshoot",()=>{
                var w=World(5);w.BossAIEnabled=true;int actions=0;w.Event+=e=>{if(e.Kind=="boss-action")actions++;};
                w.Tick(8.1f);Require(actions==0,"delay branch returns");w.Tick(0.01f);Require(actions==1,"next frame begins action");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/unity-skill-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
    }
}
