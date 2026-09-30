using System;
using System.Collections.Generic;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameLevelControlValidation.Fixture;
using Tower=AreaBattle.EditorTools.OutgameLevelControlValidation.Tower;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelReuseValidation
    {
        sealed class Boss:Tower,IOutgameLevelBoss{}
        sealed class Arrow:Tower,IOutgameLevelArrowTower{}
        sealed class Soldier:IOutgameLevelSoldier
        {
            public bool Enabled,Ready;public OutgameEffectCollection Held;public Action<bool> Cleared;
            public bool Active=>Enabled;public bool SourceFlag112=>Ready;public OutgameEffectCollection Effects=>Held;
            public void Clear(bool option){Cleared?.Invoke(option);Enabled=false;}
        }
        sealed class Effect:IOutgameCollectionEffect{public readonly List<string> Calls;public Effect(List<string> c){Calls=c;}public void SetActive(bool active)=>Calls.Add("active:"+active);}
        sealed class Module:IOutgameCollectionEffectModule
        {
            public readonly List<string> Calls=new List<string>();public Action<int> Closed;public int Handle=7;public Transform Parent;public Vector3 Position;public bool Flag;public int Option;
            public IOutgameCollectionEffect Get(int id){Calls.Add("get:"+id);return new Effect(Calls);}public void Close(int id){Calls.Add("close:"+id);Closed?.Invoke(id);}
            public int Show(int id,Vector3 p,Transform parent,bool flag,int option){Calls.Add("show:"+id);Parent=parent;Position=p;Flag=flag;Option=option;return Handle;}
        }
        static void Require(bool yes,string message){if(!yes)throw new Exception(message);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameEffectCollection Collection(Module module,List<string> destroyed=null)=>new OutgameEffectCollection(()=>module,go=>{destroyed?.Add(go.name);UnityEngine.Object.DestroyImmediate(go);});
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-level-reset-order-retention-and-all-soldiers-effects",()=>{using(var f=new Fixture()){
                var go=new GameObject("source-obstacle");try{
                    f.Level.ObstacleObjects.Add(go);f.Level.CurrentBoss=new Boss();f.Level.HaveCheckedGuide=f.Level.HaveCheckedSerialActivity=f.Level.HaveCheckedFailReward=true;f.Level.HasBoss=true;f.Level.ObstaclesInitialized=true;f.Level.NeedsCampRefresh=true;f.Level.CampRefreshRemaining=9;f.State.PlayState=6;f.Level.TotalScore=50;f.Level.TotalTowerCount=8;
                    var camp=new OutgameCampInfo();camp.Init(1,2,Color.red);f.Level.Camps.Add(camp);var inactive=new OutgameCampInfo{Score=99};f.Level.Camps.Add(inactive);
                    var tower=f.Add(1,5);tower.OnClear=()=>{Require(!go.activeSelf&&f.Level.ObstacleObjects.Count==0&&!camp.IsActive&&!f.Level.HaveCheckedFailReward&&f.Level.CurrentBoss==null,"prior reset phases completed");f.Calls.Add("tower");};f.Add(2,2,false).OnClear=()=>throw new Exception("inactive tower must skip");
                    var module=new Module();var effects=Collection(module);effects.Handles.Add(1,10); // Missing native root: source Clear retains handles.
                    f.Level.Soldiers.Add(new Soldier{Enabled=false,Held=effects,Cleared=b=>f.Calls.Add("soldier:"+b)});
                    f.Level.InitGameData(value=>f.Calls.Add("line:"+value));Require(string.Join(",",f.Calls)=="tower,soldier:True,line:0"&&f.Level.Towers.Count==2&&f.Level.Soldiers.Count==1&&f.Level.Camps.Count==2,"source callback order with retained pools and inactive soldier clear");
                    Require(f.Level.HasBoss&&f.Level.ObstaclesInitialized&&f.Level.NeedsCampRefresh&&f.Level.CampRefreshRemaining==9&&f.State.PlayState==6&&f.Level.TotalScore==50&&f.Level.TotalTowerCount==8&&f.Resources.CurrentLevel==f.Asset&&inactive.Score==99&&effects.Handles.Count==1,"unmentioned source flags/data and inactive camp remain intact");
                }finally{UnityEngine.Object.DestroyImmediate(go);}
            }});
            check("source-level-reset-obstacle-null-aborts-before-flags-and-list-clear",()=>{using(var f=new Fixture()){
                var go=new GameObject("first");try{f.Level.ObstacleObjects.Add(go);f.Level.ObstacleObjects.Add(null);f.Level.HaveCheckedGuide=true;f.Level.CurrentBoss=new Boss();Throws<NullReferenceException>(()=>f.Level.InitGameData(v=>throw new Exception("tail")));Require(!go.activeSelf&&f.Level.CurrentBoss==null&&f.Level.ObstacleObjects.Count==2&&f.Level.HaveCheckedGuide,"prefix writes survive native obstacle failure");}finally{UnityEngine.Object.DestroyImmediate(go);}
            }});
            check("source-level-reset-foreach-tower-mutation-and-soldier-effect-failure",()=>{using(var f=new Fixture()){
                f.Add(1,5).OnClear=()=>f.Add(2,3);Throws<InvalidOperationException>(()=>f.Level.InitGameData(v=>throw new Exception("tail")));Require(f.Level.Towers.Count==2&&!f.Level.Towers[0].Active,"source foreach mutation after first Clear");f.Level.Towers.Clear();var soldier=new Soldier{Enabled=true,Held=null,Cleared=b=>f.Calls.Add("clear")};f.Level.Soldiers.Add(soldier);Throws<NullReferenceException>(()=>f.Level.InitGameData(v=>throw new Exception("tail")));Require(!soldier.Enabled&&f.Calls.Count==1,"Clear true occurs before null Effects access; tail not executed");
            }});
            check("source-level-soldier-pool-requires-inactive-and-flag112",()=>{using(var f=new Fixture()){
                var busy=new Soldier{Enabled=true,Ready=true};var waiting=new Soldier{Enabled=false,Ready=false};var ready=new Soldier{Enabled=false,Ready=true};f.Level.Soldiers.AddRange(new[]{busy,waiting,ready});Require(ReferenceEquals(f.Level.GetEmptySoldier(()=>throw new Exception("create")),ready),"inactive without readiness is skipped");ready.Enabled=true;var fresh=new Soldier();Require(ReferenceEquals(f.Level.GetEmptySoldier(()=>fresh),fresh)&&f.Level.Soldiers.Count==4&&!fresh.Enabled&&!fresh.Ready,"new source object added without invented activation/readiness");
            }});
            check("source-level-tower-pool-type-filter-and-source-order",()=>{using(var f=new Fixture()){
                var boss=new Boss{Enabled=false};var arrow=new Arrow{Enabled=false};var ordinary=new Tower{Enabled=false};f.Level.Towers.AddRange(new IOutgameLevelTower[]{boss,arrow,ordinary});Require(ReferenceEquals(f.Level.GetEmptyTower(1,()=>throw new Exception(),()=>throw new Exception()),ordinary),"normal ignores inactive boss and arrow");Require(ReferenceEquals(f.Level.GetEmptyTower(4,null,null),arrow)&&ReferenceEquals(f.Level.GetEmptyBoss(null),boss),"source subtype-specific reuse");ordinary.Enabled=arrow.Enabled=boss.Enabled=true;var created=new Tower();Require(ReferenceEquals(f.Level.GetEmptyTower(99,()=>created,null),created)&&f.Level.Towers.Count==4,"any non4 type creates plain Tower");var addedBoss=new Boss();Require(ReferenceEquals(f.Level.GetEmptyBoss(()=>addedBoss),addedBoss)&&f.Level.Towers.Count==5,"active boss cannot reuse");
            }});
            check("source-effect-setentity-one-child-duplicates-two-child-reuses",()=>{
                var root=new GameObject("entity");try{var first=new GameObject("EffectCellection");first.transform.SetParent(root.transform,false);var collection=Collection(new Module());collection.SetEntity(root.transform);Require(root.transform.childCount==2&&collection.Root!=first.transform&&collection.Root.localPosition==Vector3.zero&&collection.Root.localScale==Vector3.one,"source only searches when at least two children, preserving duplicate-name case");collection.SetEntity(root.transform);Require(collection.Root==first.transform&&root.transform.childCount==2,"two-child Find returns original first child");Throws<NullReferenceException>(()=>collection.SetEntity(null));Require(collection.Root==null&&collection.Entity==null,"null entity assignment clears heldroot before childCount failure");}finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("source-effect-spawn-handle-add-existing-display-and-remove",()=>{
                var root=new GameObject("entity");try{var module=new Module();var collection=Collection(module);collection.Spawn(10,3);Require(module.Calls.Count==0&&collection.Handles.Count==0,"missing native parent does not fabricate effect");collection.SetEntity(root.transform);collection.Spawn(10,3);Require(collection.Handles[10]==7&&module.Parent==collection.Root&&module.Position==Vector3.zero&&!module.Flag&&module.Option==3,"source Show parameters and handle mapping");collection.Spawn(10,99);Require(string.Join(",",module.Calls)=="show:10,get:7,active:True","existing key only reactivates, ignores new option");Throws<ArgumentException>(()=>collection.Add(10,8));collection.Remove(10);Require(!collection.Handles.ContainsKey(10)&&module.Calls[3]=="close:7","remove closes before deleting mapping");collection.Remove(10);Require(module.Calls.Count==4,"missing remove is no-op");}finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("source-effect-display-resolves-module-before-missing-key",()=>{int reads=0;var module=new Module();var collection=new OutgameEffectCollection(()=>{reads++;return module;});Throws<KeyNotFoundException>(()=>collection.Display(99));Require(reads==1&&module.Calls.Count==0,"receiver resolved before dictionary index evaluation");});
            check("source-effect-remove-callback-failure-preserves-handle",()=>{var module=new Module();var collection=Collection(module);collection.Add(1,3);module.Closed=id=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>collection.Remove(1));Require(collection.Handles[1]==3,"close failure prevents remove");});
            check("source-effect-clear-handles-then-children-in-reverse-order",()=>{
                var root=new GameObject("entity");try{var module=new Module();var destroyed=new List<string>();var collection=Collection(module,destroyed);collection.SetEntity(root.transform);for(int i=0;i<3;i++)new GameObject("child"+i).transform.SetParent(collection.Root,false);collection.Add(2,20);collection.Add(1,10);module.Closed=id=>Require(collection.Handles.Count==2&&collection.Root.childCount==3,"no early dictionary or child clear");collection.Clear();Require(string.Join(",",module.Calls)=="close:20,close:10"&&string.Join(",",destroyed)=="child2,child1,child0"&&collection.Handles.Count==0&&collection.Root,"insertion-order close then full mapping clear then reverse nativechild deletion");collection.Destory();Require(collection.Root==null&&collection.Entity==root.transform&&destroyed[3]=="EffectCellection","Destory clears heldroot but retains entity reference");}finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            check("source-effect-clear-missing-native-root-preserves-stale-handles",()=>{
                var module=new Module();var collection=Collection(module);collection.Add(1,5);collection.Clear();collection.Destory();Require(collection.Handles.Count==1&&module.Calls.Count==0,"source root gate precedes dictionary enumeration and clear");
            });
            check("source-effect-clear-mutation-fails-before-bookkeeping-and-destroy",()=>{
                var root=new GameObject("entity");try{var module=new Module();var collection=Collection(module);collection.SetEntity(root.transform);collection.Add(1,10);module.Closed=id=>collection.Add(2,20);Throws<InvalidOperationException>(()=>collection.Destory());Require(collection.Root&&collection.Handles.Count==2,"live dictionary foreach exception stops before clear and native parent destruction");}finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            return report;
        }
    }
}
