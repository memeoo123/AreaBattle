using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameMainExitValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-main-exit-two-saves-destruction-order-and-dispatcher-replacement",()=>{
                OutgameMessageDispatcher.ClearEvent();var oldMessages=OutgameMessageDispatcher.Shared;var trace=new List<string>();var main=new GameObject("exit-main-fixture");var update=new GameObject("exit-update-fixture");int writes=0;object iap=new object();bool exited=false,entered=true;
                try{
                    var prefs=new OutgameUserPreferences(n=>null,(n,v)=>{writes++;trace.Add("write");},()=>false,trace.Add,trace.Add,(i,s)=>{},s=>trace.Add("report"));prefs.OnInit(false);
                    var cleanup=new OutgameMainExit(trace.Add,()=>trace.Add("audio"),v=>{exited=v;trace.Add("flag37");},()=>{iap=null;trace.Add("iap");},()=>{trace.Add("get-main");return main;},()=>{Require(!main,"main destroy issued before update lookup in synchronous fixture");trace.Add("get-update");return update;},prefs,v=>{Require(!ReferenceEquals(oldMessages,OutgameMessageDispatcher.Shared),"message replacement before EnterGame reset");entered=v;trace.Add("enter:false");},g=>{trace.Add("destroy:"+g.name);UnityEngine.Object.DestroyImmediate(g);});
                    oldMessages.AddListener("ExitGame",cleanup.OnExitGame);var exit=new OutgameProcedureExitGame(prefs,()=>OutgameMessageDispatcher.Shared,trace.Add,()=>{});exit.OnEnter(null);
                    Require(writes==1&&exited&&!entered&&iap==null&&!main&&!update,"one physical write, exit state and destruction");
                    int audio=trace.IndexOf("audio");Require(trace[audio+1]=="flag37"&&trace[audio+2]=="iap"&&trace[audio+3]=="get-main"&&trace[audio+4]=="destroy:exit-main-fixture"&&trace[audio+5]=="get-update"&&trace[audio+6]=="destroy:exit-update-fixture"&&trace[audio+7]=="UserData数据没有发生变化，不存储"&&trace[audio+8]=="enter:false","exact cleanup order and second-save digest guard");
                    int count=trace.Count;OutgameMessageDispatcher.Shared.SendMessage("ExitGame");Require(trace.Count==count,"new shared dispatcher contains no old exit callback");
                }finally{if(main)UnityEngine.Object.DestroyImmediate(main);if(update)UnityEngine.Object.DestroyImmediate(update);OutgameMessageDispatcher.ClearEvent();}
            });
            test("original-main-exit-failure-stops-later-cleanup",()=>{
                OutgameMessageDispatcher.ClearEvent();var messages=OutgameMessageDispatcher.Shared;bool flag=false,cleared=false;var cleanup=new OutgameMainExit(s=>{},()=>{},v=>flag=v,()=>throw new InvalidOperationException("iap failure"),()=>throw new Exception("unexpected main destruction"),null,null,v=>cleared=true);
                bool failed=false;try{cleanup.OnExitGame(null);}catch(InvalidOperationException){failed=true;}
                Require(failed&&flag&&!cleared&&ReferenceEquals(messages,OutgameMessageDispatcher.Shared),"earlier flag persists; no destroy/save/event replacement/reset after failure");OutgameMessageDispatcher.ClearEvent();
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
