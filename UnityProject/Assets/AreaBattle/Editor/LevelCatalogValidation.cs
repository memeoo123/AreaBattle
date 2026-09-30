using System;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class LevelCatalogValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}
                catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            var catalog=new BattleLevelCatalog(BattleView.ReadText("Data/LevelConfig"));
            check("pvp-view-special-map-normal-identity-and-ui-origin",()=>{
                var obj=new GameObject("PVP view fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializePvPMap(1,30,1,1,new[]{1,2,3});
                    if(!view.Initialized||view.LevelId!=2001||!view.Simulation.PvPEnabled)throw new Exception("PVP source map route");
                    if(view.SkillInput.NormalLevel!=30||!view.Progress.Special)throw new Exception("PVP ordinary identity or save guard");
                    var screen=view.BattleCamera.WorldToScreenPoint(view.Simulation.PvPFireballOrigin());
                    if(Vector2.Distance(view.SkillScreenCenter(1),new Vector2(screen.x,screen.y))>.01f||Mathf.Abs(screen.z-5)>.001f)throw new Exception("PVP fireball must originate from player second UI slot");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("special-entry-after-normal-preserves-normal-save",()=>{
                var obj=new GameObject("Special entry fixture");
                try{
                    var view=obj.AddComponent<BattleView>();view.InitializeNormalLevel(25);
                    view.InitializeSpecialScene(99998);
                    if(!view.Initialized||view.RequestedNormalLevel!=-1||!view.Progress.Special)throw new Exception("Special entry retained normal save identity");
                    if(view.LevelId!=99998||view.SkillInput.NormalLevel!=99998)throw new Exception("Special CurLevel must remain direct layout identity");
                    if(view.Progress.OpenResult(BattlePhase.Victory))throw new Exception("Special victory writes normal progress");
                    if(!view.Simulation.Towers.Exists(t=>t.IsBoss))throw new Exception("Source boss layout not loaded");
                }finally{UnityEngine.Object.DestroyImmediate(obj);Time.timeScale=1f;Physics.SyncTransforms();}
            });
            check("normal-progress-advances-on-result-once-special-preserved",()=>{
                var normal=new BattleProgress(25);
                if(normal.OpenResult(BattlePhase.Defeat)||normal.SavedLevel!=25)throw new Exception("Defeat must not advance");
                if(!normal.OpenResult(BattlePhase.Victory)||normal.SavedLevel!=26)throw new Exception("Victory panel open increments");
                if(normal.OpenResult(BattlePhase.Victory)||normal.SavedLevel!=26)throw new Exception("Result redraw must not double increment");
                var special=new BattleProgress(99998,true);
                if(special.OpenResult(BattlePhase.Victory)||special.SavedLevel!=99998)throw new Exception("Special cannot write normal progress");
            });
            check("all-639-layout-data-initialization",()=>{
                var configs=BattleView.ReadConfig();int count=0,towers=0;
                foreach(var text in Resources.LoadAll<TextAsset>("Data/Levels"))
                {
                    var layout=JsonUtility.FromJson<LevelLayout>(text.text);
                    var sim=new BattleSimulation(layout,configs,4305,(a,b)=>true);BattleView.ConfigureSkills(sim);
                    if(sim.Towers.Count!=layout.StarInfoCfgs.Length)throw new Exception(text.name+" tower count");
                    for(int i=0;i<sim.Towers.Count;i++)
                    {
                        var tower=sim.Towers[i];var source=layout.StarInfoCfgs[i];
                        if(tower.Position!=source.pos.WorldPosition || tower.Score!=source.StartScore || tower.Camp!=source.CampID)
                            throw new Exception(text.name+" initial serialized state at tower "+i);
                    }
                    count++;towers+=sim.Towers.Count;
                }
                if(count!=639 || towers!=5752)throw new Exception("Source traversal counts "+count+"/"+towers);
            });
            check("all-normal-level-scene-bindings",()=>{
                if(catalog.MaximumLevel!=560)throw new Exception("Expected original 561-row table");
                for(int id=0;id<=560;id++)if(Resources.Load<TextAsset>("Data/Levels/level_"+catalog.Resolve(id).SceneId)==null)
                    throw new Exception("Missing SceneId binding for normal level "+id);
            });
            check("post-table-parity-wrap-boundaries",()=>{
                int[] requested={560,561,822,823,1084,1085},expected={560,37,559,38,560,37};
                for(int i=0;i<requested.Length;i++)if(catalog.ResolveIdentity(requested[i])!=expected[i])
                    throw new Exception("Wrap boundary "+requested[i]);
            });
            return report;
        }
    }
}
