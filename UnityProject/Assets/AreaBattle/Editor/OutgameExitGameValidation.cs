using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameExitGameValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-exit-game-saves-before-notify-and-reloads",()=>{
                var trace=new List<string>();string saved=null;var messages=new OutgameMessageDispatcher();
                var prefs=new OutgameUserPreferences(n=>null,(n,v)=>{Require(n=="UserData.txt","original file route");saved=v;trace.Add("write");},()=>false,s=>{},s=>{},(i,s)=>{},s=>trace.Add("report:"+s));prefs.OnInit(false);prefs.SetString("GF_LoginUserID","test-user");prefs.SetInt("Day",13);
                var exit=new OutgameProcedureExitGame(prefs,()=>messages,trace.Add,()=>trace.Add("destroy"));var manager=new OutgameFsmManager();var procedure=new OutgameProcedureManager(()=>manager);procedure.Register(exit);
                messages.AddListener("ExitGame",a=>{Require(saved!=null&&a==null,"saved data and null message arguments");Require(procedure.ProcedureFsm.CurrentState==exit,"exit state published before callback");trace.Add("exit");});procedure.StartProcedure<OutgameProcedureExitGame>();
                Require(trace.Count==4&&trace[1]=="write"&&trace[2]=="report:test-user"&&trace[3]=="exit","save/report precede exit");
                var reloaded=new OutgameUserPreferences(n=>saved,(n,v)=>{},()=>false,s=>{},s=>{},(i,s)=>{},s=>{});reloaded.OnInit(false);Require(reloaded.GetInt("Day")==13&&reloaded.GetString("GF_LoginUserID")=="test-user","fresh preference instance reloads saved values");procedure.Shutdown();Require(trace[4]=="Leave 'Proj_hdzd.Procedure.ProcedureExitGame' procedure."&&trace[5]=="destroy"&&exit.Owner!=null,"original leave and event cleanup; owner retained");
            });
            test("original-exit-game-save-failure-keeps-current-and-stops-notification",()=>{
                int events=0,writes=0;var messages=new OutgameMessageDispatcher();messages.AddListener("ExitGame",a=>events++);
                var prefs=new OutgameUserPreferences(n=>null,(n,v)=>{writes++;throw new InvalidOperationException("write failed");},()=>false,s=>{},s=>{},(i,s)=>{},s=>{});prefs.OnInit(false);
                var exit=new OutgameProcedureExitGame(prefs,()=>messages,s=>{},()=>{});var manager=new OutgameFsmManager();var procedure=new OutgameProcedureManager(()=>manager);procedure.Register(exit);bool failed=false;try{procedure.StartProcedure<OutgameProcedureExitGame>();}catch(InvalidOperationException){failed=true;}
                Require(failed&&events==0&&writes==1&&procedure.ProcedureFsm.CurrentState==exit,"save failure aborts notification after FSM current write");
            });
            test("original-exit-game-disabled-save-still-notifies",()=>{
                int events=0,warnings=0;var messages=new OutgameMessageDispatcher();messages.AddListener("ExitGame",a=>events++);
                var prefs=new OutgameUserPreferences(n=>null,(n,v)=>throw new Exception("unexpected write"),()=>true,s=>{},s=>warnings++,(i,s)=>{},s=>{});var exit=new OutgameProcedureExitGame(prefs,()=>messages,s=>{},()=>{});exit.OnEnter(null);Require(events==1&&warnings==1,"normal early return in disabled save still sends exit");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
