using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
 public static class ArtDynamicReview
 {
  [Serializable] public class Report { public bool passed; public string scope="Controlled editor battle replays using actual simulation and presentation; 540x960, not mobile performance certification."; public List<Case> cases=new List<Case>(); public string error; }
  [Serializable] public class Case {public int level,peakSoldiers,deathEvents,visibleDeaths,frameChanges;public float simulatedSeconds;public double medianAdvanceMs;public bool pausePassed;public string frames;}
  public static void RunBatch(){var report=new Report();try{foreach(int level in new[]{116,30,99001})report.cases.Add(Run(level));report.passed=true;}catch(Exception e){report.error=e.ToString();Debug.LogException(e);}finally{Time.timeScale=1;File.WriteAllText(Path.Combine(BattleBuild.Workspace,"art-style/style-a/dynamic-review.json"),JsonUtility.ToJson(report,true));}Debug.Log("ART_DYNAMIC_"+(report.passed?"PASS":"FAIL"));EditorApplication.Exit(report.passed?0:1);}
  static void Need(bool ok,string message){if(!ok)throw new Exception(message);}
  static void Resolve(BattleView view){for(int n=0;view.Hud.EvolutionOpen&&n<30;n++){var sheet=view.Hud.Canvas.transform.Find("Evolution modal/Sheet");sheet.Find("Route_"+(n%3)).GetComponent<Button>().onClick.Invoke();sheet.Find("Confirm").GetComponent<Button>().onClick.Invoke();}Need(!view.Hud.EvolutionOpen,"modal unresolved");}
  static Case Run(int level){var host=new GameObject("Dynamic art review");var result=new Case{level=level,frames="analysis/captures/art-dynamic-"+level};try{
   var view=host.AddComponent<BattleView>();view.EvolutionCampaign=true;if(level==99001)view.InitializeScene(level);else view.InitializeNormalLevel(level);Need(view.Initialized,"init");var w=view.Simulation;w.AIEnabled=false;
   // Deliberately dense opposed armies, without touching saved layout/profile.
   for(int i=0;i<w.Towers.Count;i++){var tower=w.Towers[i];if(tower.IsBoss)continue;tower.Camp=1;w.ChangeScore(tower.Id,1,60-tower.Score);if(w.CanAdvance(tower.Id)&&i%4!=0)w.ChooseAdvancement(tower.Id,w.AdvancementOptions(tower.Id)[(i-1)%3].Id);tower.Camp=i%2+1;}
   view.RefreshPresentation();Resolve(view);
   foreach(var tower in w.Towers.Where(t=>!t.IsArrow&&!t.IsBoss))foreach(var line in w.Lines.Where(l=>l.SmallTowerId==tower.Id||l.LargeTowerId==tower.Id).OrderBy(l=>Vector3.Distance(tower.Position,w.Tower(l.Other(tower.Id)).Position))){var other=w.Tower(line.Other(tower.Id));if(other.Camp==tower.Camp||!w.Connect(tower.Id,other.Id,true))continue;for(int n=1;n<=5;n++){var s=w.SpawnSoldier(tower.Id,other.Id);if(s!=null)s.Position=Vector3.Lerp(tower.Position,other.Position,n*.09f);}}
   w.Event+=e=>{if(e.Kind=="soldier-clear")result.deathEvents++;};view.RefreshPresentation();Resolve(view);
   Need(w.Soldiers.Any(s=>s.Active),"no soldiers");
   var dir=Path.Combine(BattleBuild.Workspace,result.frames);Directory.CreateDirectory(dir);
   var previous=new Dictionary<int,Sprite>();var timings=new List<double>();var timer=new System.Diagnostics.Stopwatch();
   for(int f=0;f<180;f++){
    Resolve(view);timer.Restart();view.AdvanceFrame(1f/30,1f/30);timer.Stop();timings.Add(timer.Elapsed.TotalMilliseconds);
    result.peakSoldiers=Math.Max(result.peakSoldiers,w.Soldiers.Count(s=>s.Active));
    foreach(var s in w.Soldiers){var tr=view.SoldierPresentationTransform(s.Id);if(tr==null)continue;var sr=tr.GetComponent<SpriteRenderer>();if(sr==null)continue;if(!s.Active&&s.PlayDeathAnimation&&sr.color.a>0)result.visibleDeaths++;if(previous.TryGetValue(s.Id,out var sprite)&&sprite!=sr.sprite)result.frameChanges++;previous[s.Id]=sr.sprite;}
    if(f%3==0)BattleBuild.Capture(view,"art-dynamic-"+level+"/"+(f/3).ToString("D2")+".png");
   }
   result.simulatedSeconds=view.VisualClock;Resolve(view);
   var visible=w.Soldiers.Select(s=>view.SoldierPresentationTransform(s.Id)).Where(t=>t!=null&&t.GetComponent<ClearSoldierVisual>()!=null).ToArray();
   var poses=visible.Select(t=>t.localToWorldMatrix).ToArray();var sprites=visible.Select(t=>t.GetComponent<SpriteRenderer>().sprite).ToArray();float clock=view.VisualClock;
   w.Pause(true);view.AdvanceFrame(0,.5f);result.pausePassed=view.VisualClock==clock&&visible.Select((t,i)=>t.localToWorldMatrix==poses[i]&&t.GetComponent<SpriteRenderer>().sprite==sprites[i]).All(b=>b);w.Pause(false);
   Need(result.pausePassed,"pause changed soldier pose");Need(result.frameChanges>0,"no animation changes");Need(result.simulatedSeconds>5,"clock did not progress");
   timings.Sort();result.medianAdvanceMs=timings[timings.Count/2];return result;
  }finally{UnityEngine.Object.DestroyImmediate(host);Time.timeScale=1;}}
 }
}
