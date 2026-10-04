using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameMainLifecycleValidation
    {
        sealed class Manager:IOutgameDataManager
        {public bool ParticipatesInSync{get;set;}public bool CompressData{get;set;}public string DataKey=>"fixture";public Action Save;public void OnInit(){}public void OnRelease(){}public void OnSave(){Save();}}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-main-update-gates-live-read-and-time-resampling",()=>{
                var state=new OutgameMainLifecycleState{ResourcesOnly=true};var trace=new List<string>();int reads=0;
                var lifecycle=new OutgameMainLifecycle(state,(a,b)=>{Require(a==1&&b==2,"first clock samples");trace.Add("resources");state.ModulesReady=true;},(a,b)=>{Require(a==3&&b==4,"fresh clock samples for full update");trace.Add("frame");},e=>{throw e;},null,null,null,()=>++reads,()=>++reads);
                lifecycle.Update();Require(string.Join(",",trace)=="resources,frame"&&reads==4,"live ready gate after resource update");state.ExitRequested=true;lifecycle.Update();Require(reads==4,"exit flag skips all clocks");
            });
            test("original-main-update-catches-only-full-module-loop",()=>{
                var state=new OutgameMainLifecycleState{ResourcesOnly=true,ModulesReady=true};var expected=new InvalidOperationException("resource");int warnings=0;
                var lifecycle=new OutgameMainLifecycle(state,(a,b)=>throw expected,(a,b)=>throw new Exception("frame"),e=>warnings++,null,null,null,()=>1,()=>2);bool failed=false;try{lifecycle.Update();}catch(InvalidOperationException ex){failed=ReferenceEquals(ex,expected);}Require(failed&&warnings==0,"resource failure escapes catch");state.ResourcesOnly=false;lifecycle.Update();Require(warnings==1,"full update failure logged and swallowed");
            });
            test("original-main-quit-saves-preferences-before-manager-pool",()=>{
                var trace=new List<string>();var prefs=new OutgameUserPreferences(n=>null,(n,v)=>trace.Add("prefs"),()=>false,s=>{},s=>{},(i,s)=>{},s=>trace.Add("report"));prefs.OnInit(false);
                var data=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>trace.Add("manager-error"));data.OnInit(true,"fixture",new[]{new OutgameManagerRegistration(1,"fixture",false,false,()=>new Manager{Save=()=>{trace.Add("manager");throw new Exception("save");}})});var state=new OutgameMainLifecycleState{ExitRequested=true};
                var lifecycle=new OutgameMainLifecycle(state,null,null,null,prefs,data,trace.Add);lifecycle.OnApplicationQuit();Require(string.Join(",",trace)=="prefs,report,manager,manager-error,游戏退出，数据存储完成","quit ignores exit flag and uses pool per-manager failure handling");
                var failedPrefs=new OutgameUserPreferences(n=>null,(n,v)=>throw new InvalidOperationException("prefs"),()=>false,s=>{},s=>{},(i,s)=>{},s=>{});failedPrefs.OnInit(false);trace.Clear();lifecycle=new OutgameMainLifecycle(state,null,null,null,failedPrefs,data,trace.Add);bool failed=false;try{lifecycle.OnApplicationQuit();}catch(InvalidOperationException){failed=true;}Require(failed&&trace.Count==0,"preference failure prevents pool save and final log");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
