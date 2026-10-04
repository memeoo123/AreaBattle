using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameMainFocusValidation
    {
        sealed class Manager:IOutgameDataManager
        {public bool ParticipatesInSync{get;set;}public bool CompressData{get;set;}public string DataKey=>"fixture";public Action Save;public void OnInit(){}public void OnRelease(){}public void OnSave(){Save();}}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-main-focus-pause-message-ui-and-save-order",()=>{
                var trace=new List<string>();var state=new OutgameMainLifecycleState{EnterGame=true,ExitRequested=true};var messages=new OutgameMessageDispatcher();messages.AddListener("GamePause",a=>trace.Add("pause:"+a[0]));
                var prefs=new OutgameUserPreferences(n=>null,(n,v)=>trace.Add("prefs"),()=>false,s=>{},s=>{},(i,s)=>{},s=>{});prefs.OnInit(false);var data=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});data.OnInit(true,"f",new[]{new OutgameManagerRegistration(1,"f",false,false,()=>new Manager{Save=()=>trace.Add("data")})});
                var focus=new OutgameMainFocus(state,()=>false,()=>true,()=>6,()=>false,a=>{Require(a.Length==0,"empty original Show args");trace.Add("ui");},()=>messages,prefs,data,trace.Add,(c,s)=>{Require(c==Color.red,"source red log");trace.Add("red");});
                focus.OnApplicationFocus(false);Require(string.Join(",",trace)=="OnApplicationFocus：False,pause:True,ui,red,prefs,data,数据存储完成","original loss sequence even when exit flag true");trace.Clear();focus.OnApplicationFocus(true);Require(string.Join(",",trace)=="OnApplicationFocus：True,pause:False","regain emits resume only");state.EnterGame=false;trace.Clear();focus.OnApplicationFocus(false);Require(trace.Count==1,"outside game logs only");
            });
            test("original-main-focus-global-suppression-keeps-save-and-ui-short-circuit",()=>{
                int messages=0,saves=0,ui=0;bool hasUi=false,pvp=true;int level=6;var bus=new OutgameMessageDispatcher();bus.AddListener("GamePause",a=>messages++);
                var prefs=new OutgameUserPreferences(n=>null,(n,v)=>{},()=>true,s=>{},s=>{},(i,s)=>{},s=>{});var data=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{});data.OnInit(true,"f",new[]{new OutgameManagerRegistration(1,"f",false,false,()=>new Manager{Save=()=>saves++})});
                var focus=new OutgameMainFocus(new OutgameMainLifecycleState{EnterGame=true},()=>true,()=>hasUi,()=>{Require(hasUi,"play state only read after UI exists");return level;},()=>pvp,a=>ui++,()=>bus,prefs,data,s=>{},(c,s)=>{});
                focus.OnApplicationFocus(false);hasUi=true;focus.OnApplicationFocus(false);pvp=false;level=5;focus.OnApplicationFocus(false);level=6;focus.OnApplicationFocus(false);Require(messages==0&&saves==4&&ui==1,"global gate only suppresses message, UI gates independent and saves always attempted");
            });
            test("original-main-focus-listener-failure-prevents-later-effects",()=>{
                var bus=new OutgameMessageDispatcher();bus.AddListener("GamePause",a=>throw new InvalidOperationException("listener"));var focus=new OutgameMainFocus(new OutgameMainLifecycleState{EnterGame=true},()=>false,()=>throw new Exception("unexpected UI read"),null,null,null,()=>bus,null,null,s=>{},null);bool failed=false;try{focus.OnApplicationFocus(false);}catch(InvalidOperationException){failed=true;}Require(failed,"source has no catch around message or saves");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
