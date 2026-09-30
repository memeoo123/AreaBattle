using System;
using System.Collections.Generic;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameLevelControlValidation.Fixture;
using Tower=AreaBattle.EditorTools.OutgameLevelControlValidation.Tower;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelQueryValidation
    {
        sealed class Arrow:Tower,IOutgameLevelArrowTower{}
        sealed class Boss:Tower,IOutgameLevelBoss{}
        sealed class PickRandom:System.Random
        {public int Calls,Maximum;public override int Next(int maxValue){Calls++;Maximum=maxValue;return maxValue-1;}}
        sealed class Loads:IOutgameGameSceneResources
        {
            public readonly List<int> Ids=new List<int>();
            public void LoadAsset(int id,Action<GameObject> loaded,object[] arguments=null)=>Ids.Add(id);
            public void LoadTexture(string name,Action<Texture2D> loaded,object[] arguments=null)=>throw new Exception("unexpected texture completion");
            public void LoadPrefab(string name,Action<GameObject> loaded,object[] arguments=null)=>throw new Exception("unexpected effect completion");
        }
        static void Require(bool yes,string message){if(!yes)throw new Exception(message);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-level-camp-query-cache-alias-and-arrow-only-exclusion",()=>{using(var f=new Fixture()){
                var normal=f.Add(1,1);var arrow=new Arrow{Camp=1};var boss=new Boss{Camp=1};f.Level.Towers.Add(arrow);f.Level.Towers.Add(boss);f.Add(1,0,false);f.Add(2,1);
                var all=f.Level.GetCampAllTower(1);Require(all.Count==3&&all[0]==normal&&all[1]==arrow&&all[2]==boss,"ordered active same camp includes Boss/Arrow");
                var non=f.Level.GetCampAllTowerNonArrow(1);Require(ReferenceEquals(all,non)&&all.Count==2&&all[1]==boss,"same field76 buffer overwritten; only Arrow excluded");f.Level.GetCampAllTower(99);Require(all.Count==0&&f.Level.Towers.Count==5,"subsequent query clears previous returned list, source pool intact");
            }});
            check("source-level-other-enemy-share-cache-neutral-difference",()=>{using(var f=new Fixture()){
                f.Add(1,1);var neutral=f.Add(0,1);var enemy=f.Add(2,1);f.Add(3,1,false);var other=f.Level.GetOtherTower(1);Require(other.Count==2&&other[0]==neutral&&other[1]==enemy,"other includes neutral");var mine=f.Level.GetCampAllTower(1);var hostile=f.Level.GetEnemyTower(1);Require(ReferenceEquals(other,hostile)&&other.Count==1&&other[0]==enemy&&mine.Count==1,"enemy excludes neutral and shares field80 only");
            }});
            check("source-level-query-failure-keeps-partial-shared-results",()=>{using(var f=new Fixture()){
                var first=f.Add(1,1);f.Level.Towers.Add(null);f.Level.ScratchTowers76.Add(first);Throws<NullReferenceException>(()=>f.Level.GetCampAllTower(1));Require(f.Level.ScratchTowers76.Count==1&&f.Level.ScratchTowers76[0]==first,"clear before live foreach; no rollback or null filtering");
            }});
            check("source-level-entity-query-uses-native-null-and-first-active-match",()=>{using(var f=new Fixture()){
                var go=new GameObject("query-source-entity");try{f.Add(1,1,false).Entity=go;var first=f.Add(1,1);first.Entity=go;f.Add(2,1).Entity=go;Require(f.Level.GetTowerByGameObj(go)==first,"first active exact native object");UnityEngine.Object.DestroyImmediate(go);Require(f.Level.GetTowerByGameObj(null)==first,"destroyed Unity reference equals null; source has no nonnull gate");}finally{if(go)UnityEngine.Object.DestroyImmediate(go);}
            }});
            check("source-level-range-sqrt-minimum-squared-maximum-and-order",()=>{using(var f=new Fixture()){
                f.Add(1,1).SourcePosition68=new Vector3(1.9f,0,0);var inner=f.Add(1,1);inner.SourcePosition68=new Vector3(0,2,0);var outer=f.Add(1,1);outer.SourcePosition68=new Vector3(0,0,3);f.Add(1,1).SourcePosition68=new Vector3(3.1f,0,0);f.Add(2,1).SourcePosition68=new Vector3(2,0,0);f.Add(1,1,false).SourcePosition68=new Vector3(2,0,0);
                var found=f.Level.GetTowerInRange(1,true,Vector3.zero,3,16);Require(found.Count==2&&found[0]==inner&&found[1]==outer,"distance squared in [sqrt(16),3*3], inclusive, full xyz cached sourceposition");Require(!ReferenceEquals(found,f.Level.GetTowerInRange(1,true,Vector3.zero,3,16))&&f.Level.ScratchTowers76.Count==0,"range returns independent new list");Require(f.Level.GetTowerInRange(1,false,Vector3.zero,-3,16).Count==1,"opposite relation; negative outer range is squared");
            }});
            check("source-level-range-nan-comparison-direction-is-preserved",()=>{using(var f=new Fixture()){
                f.Add(1,1).SourcePosition68=Vector3.zero;f.Add(1,1).SourcePosition68=new Vector3(float.NaN,0,0);Require(f.Level.GetTowerInRange(1,true,Vector3.zero,1,-1).Count==2,"sqrt negative is NaN; greater-than checks both false for NaN coordinates");Require(f.Level.GetTowerInRange(1,true,Vector3.zero,float.NaN,float.NaN).Count==2,"NaN bounds do not become invented positive inclusion checks");
            }});
            check("source-level-range-single-pick-consumes-random-only-when-nonempty",()=>{using(var f=new Fixture()){
                var rng=new PickRandom();var source=new GameRandomSource(rng);Require(f.Level.GetOneTowerInRange(1,true,Vector3.zero,10,0,source)==null&&rng.Calls==0,"empty result no random read");var first=f.Add(1,1);Require(f.Level.GetOneTowerInRange(1,true,Vector3.zero,10,0,source)==first&&rng.Calls==1&&rng.Maximum==1,"singleton still consumes Next(count)");var last=f.Add(1,1);Require(f.Level.GetOneTowerInRange(1,true,Vector3.zero,10,0,source)==last&&rng.Calls==2&&rng.Maximum==2,"shared RandomHelper selection over ordered result");
            }});
            check("source-level-speed-native-write-and-refresh-flag-retain-timer",()=>{using(var f=new Fixture()){
                float previous=Time.timeScale;try{f.Level.CampRefreshRemaining=7;f.Level.SetScoreSumChangeOn();f.Level.SetLevelSpeed(.75f);Require(f.Level.NeedsCampRefresh&&f.Level.CampRefreshRemaining==7&&f.Level.LevelSpeed==.75f&&Time.timeScale==.75f,"flag does not force immediate refresh; speed writes sourcefield104 and Unity timescale");f.Level.SourceSpeed104=2;Require(Time.timeScale==.75f&&f.Level.LevelSpeed==2,"direct property storage does not implicitly call Time setter");}finally{Time.timeScale=previous;}
            }});
            check("source-level-boss-object-normalizes-only-scale-and-null-error",()=>{using(var f=new Fixture()){
                f.Loader.Cache.Prefabs[81]=UnityEngine.Resources.Load<GameObject>("Recovered/Obstacles/Entity_81");var go=f.Loader.GetEntityNow(81);go.transform.localScale=new Vector3(2,3,4);go.transform.position=new Vector3(7,8,9);go.SetActive(false);Require(f.Level.GetBossObj(81,s=>throw new Exception(s))==go&&go.transform.localScale==Vector3.one&&go.transform.position==new Vector3(7,8,9),"actual loader pool reused and only localScale reset");var errors=new List<string>();Require(f.Level.GetBossObj(-9,errors.Add)==null&&errors.Count==1&&errors[0]=="resObj == null  entityID=-9","missing object source diagnostic and null result");
            }});
            check("source-level-scene-special-value-captured-before-game-resolve",()=>{using(var f=new Fixture()){
                var progress=new OutgameLevelProgression(new OutgameProfile(),nullEffects());var calls=new List<string>();var loads=new Loads();var game=new OutgameGameControl(()=>f.Config,f.Registry,loads,null,null,null);var festival=new OutgameFestRewardSelection(()=>"{\"Datas\":[{\"id\":2,\"itemId\":[310,6],\"giftWay\":[1,2]}]}");var skins=OutgameSkinCatalog.FromOriginal(null,"{\"Datas\":[]}","{\"Datas\":[{\"id\":2,\"skinType\":4,\"lockState\":2}]}");
                var start=new OutgameLevelSceneStart(f.Resources,progress,()=>{calls.Add("game");progress.SpecialState=9;return game;},()=>{calls.Add("festival");return festival;},()=>{calls.Add("skins");return skins;});progress.SpecialState=1;start.InitSpecialScene();Require(game.CurrentSceneId==6&&string.Join(",",calls)=="game,festival"&&loads.Ids[0]==11001,"special exactly1 captured before receiver side effects; festival type5");calls.Clear();start.InitSpecialScene();Require(game.CurrentSceneId==2&&string.Join(",",calls)=="game,skins"&&loads.Ids.Count==2,"all other special states use equipped scene type4; actual GameControl dispatched");
            }});
            return report;
        }
        sealed class Effects:IOutgameLevelEffects {public void SetStatistic(int id,long count){}public void SaveLocalData(){}}
        static IOutgameLevelEffects nullEffects()=>new Effects();
    }
}
