using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle.EditorTools
{
    public static class HudPresentationValidation
    {
        public static void ImportAndRunBatch()
        {
            try
            {
                RecoveredHudImporter.Import();
                var report=Run();
                System.IO.File.WriteAllText(System.IO.Path.Combine(BattleBuild.Workspace,"analysis/unity-hud-presentation-validation.json"),JsonUtility.ToJson(report,true));
                Debug.Log("AREABATTLE_HUD_"+(report.passed?"PASS":"FAIL")+" cases="+report.checks.Count);
                if(Application.isBatchMode)EditorApplication.Exit(report.passed?0:1);
            }
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        static bool Near(Color a,Color b)=>Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)+Mathf.Abs(a.a-b.a)<.00001f;
        static void WithView(int level,Action<BattleView> test)
        {
            var host=new GameObject("HUD production validation");
            try{var view=host.AddComponent<BattleView>();view.InitializeScene(level,30);Require(view.Initialized,"production view initialized");view.Simulation.AIEnabled=false;view.Simulation.BossAIEnabled=false;test(view);}
            finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;Physics.SyncTransforms();}
        }
        static Transform Label(BattleView view,int id)=>view.Hud.Canvas.transform.Find("StarInfoRoot/TowerCanvas_"+id);
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source consumer and native UGUI binding checks; original matched-frame visual comparison remains pending."};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("hud-original-sliders-resolve-pptr-fill-rects",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/Hud/PlayTopBar");Require(prefab!=null,"topbar imported");
                var sliders=prefab.GetComponentsInChildren<Slider>(true);Require(sliders.Length==6,"five inactive camp sliders and one Boss slider retained");
                foreach(var slider in sliders){Require(slider.fillRect!=null&&slider.fillRect.IsChildOf(slider.transform),"original fillRect PPtr must resolve within slider");Require(slider.handleRect==null,"original sliders have no handle");Require(slider.direction==Slider.Direction.LeftToRight&&slider.navigation.mode==(slider.name=="slider_boss"?Navigation.Mode.Automatic:Navigation.Mode.None),"source slider direction/navigation");}
                var boss=prefab.transform.Find("ProgressSlider/slider_boss").GetComponent<Slider>();Require(boss.wholeNumbers&&boss.minValue==0&&boss.maxValue==100&&boss.value==100,"source Boss serialized range");
                Require(boss.fillRect.GetComponent<Image>().sprite!=null,"Boss fill uses original sprite");
            });
            check("hud-three-original-capacity-families-and-layout",()=>WithView(10,view=>{
                foreach(int ship in new[]{1,2,3})
                {
                    var tower=view.Simulation.Towers.First(t=>t.ShipID==ship&&!t.IsBoss);var label=Label(view,tower.Id);string name=ship==1?"normal":ship==2?"defense":"attack";
                    Require(label!=null&&label.Find(name).gameObject.activeSelf,"ship-specific original family");
                    foreach(string other in new[]{"normal","defense","attack"})Require(label.Find(other).gameObject.activeSelf==(other==name),"only matching family active");
                    var family=label.Find(name);Require(family.childCount==3&&family.GetComponent<HorizontalLayoutGroup>()!=null,"source three-slot layout group retained");
                    for(int i=0;i<3;i++)
                    {
                        var slot=family.GetChild(i);Require(slot.gameObject.activeSelf==(i<tower.MaxLines),"unused capacity visibility");Require(Near(slot.GetComponent<Image>().color,Color.white),"unused outer icon white");
                        if(ship!=1)Require(Near(slot.GetChild(0).GetComponent<Image>().color,new Color(.42f,.46f,.48f,1)),"unused inset source shadow color");
                    }
                }
                Require(!view.Hud.Canvas.transform.Find("PlayTopBar/ProgressSlider/go_normal").gameObject.activeSelf,"source ordinary faction bar stays disabled");
            }));
            check("hud-production-line-color-and-over-capacity-preserved",()=>WithView(10,view=>{
                var sim=view.Simulation;var tower=sim.Towers.First(t=>t.Camp==1&&t.ShipID==1);sim.ChangeScore(tower.Id,tower.Camp,30-tower.Score,true);
                var targets=sim.Towers.Where(t=>t.Id!=tower.Id&&t.Active).Take(3).ToArray();Require(targets.Length==3,"fixture targets");
                foreach(var target in targets)Require(sim.Connect(tower.Id,target.Id,true),"production connect");view.Hud.Synchronize();
                var family=Label(view,tower.Id).Find("normal");Require(tower.OutgoingCount==3,"fixture three source lines");
                for(int i=0;i<3;i++)Require(family.GetChild(i).gameObject.activeSelf&&Near(family.GetChild(i).GetComponent<Image>().color,BattleView.CampColor(1)),"used slot source camp color");
                sim.ChangeScore(tower.Id,tower.Camp,1-tower.Score,true);view.Hud.Synchronize();
                Require(tower.MaxLines==1&&tower.OutgoingCount==3,"source downgrade keeps existing outgoing lines");
                for(int i=0;i<3;i++)Require(family.GetChild(i).gameObject.activeSelf,"used slots remain visible above lower capacity");
                // Keep another player tower so this capture does not end the battle before reconnect.
                if(targets[2].Camp!=1)sim.ChangeScore(targets[2].Id,1,-targets[2].Score-1);
                sim.ChangeScore(tower.Id,2,-2);view.Hud.Synchronize();Require(tower.Camp==2&&tower.OutgoingCount==0,"production capture removes outgoing lines");
                Require(sim.Connect(tower.Id,targets[0].Id,true),"new camp connect");view.Hud.Synchronize();
                Require(Near(family.GetChild(0).GetComponent<Image>().color,BattleView.CampColor(2)),"capture refresh updates cached camp before next line use");
            }));
            check("hud-boss-score-event-truncation-and-retry",()=>WithView(99999,view=>{
                var sim=view.Simulation;var boss=sim.Towers.Last(t=>t.IsBoss);var slider=view.Hud.Canvas.transform.Find("PlayTopBar/ProgressSlider/slider_boss").GetComponent<Slider>();
                Require(Label(view,boss.Id)==null,"Boss never receives ordinary TowerCanvas");Require(slider.gameObject.activeSelf&&slider.maxValue==boss.MaxScore&&slider.value==boss.MaxScore,"source Refresh initializes max and full value");
                float target=Mathf.Min(123.9f,boss.Score-.1f);sim.ChangeScore(boss.Id,1,target-boss.Score,true);view.Hud.Synchronize();
                Require(slider.value==(int)boss.Score,"BossHealthChange uses truncation, not rounding or ratio");
                sim.Restart();view.Hud.Synchronize();Require(slider.value==boss.MaxScore&&slider.maxValue==boss.MaxScore,"same simulation retry reinitializes full source slider");Require(Label(view,boss.Id)==null,"retry still excludes Boss canvas");
            }));
            return report;
        }
    }
}
