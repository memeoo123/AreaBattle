using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameMainPreGameValidation
    {
        [Serializable] sealed class Roster{public Row[] controllers;}
        [Serializable] sealed class Row{public int typeIndex;}
        sealed class Control:IOutgameLogicControl
        {public Action Init;public void OnInit(){Init();}public void Updata(float a,float b){}public void OnDispose(){}}
        sealed class Host:IOutgamePreLoadHost
        {
            public List<string> Trace;public Action Callback;public void LogProcedure(string s){Trace.Add("preload");}public void InitializeUserNetModule(){Trace.Add("net");}public int ArenaRankEvent=>1;public void RegisterStatisticValue(int k,Func<object[],long> p){Trace.Add("statistics-register");}public long EventCount(int k)=>0;public void InitializeStatistics(Action a){Callback=a;Trace.Add("statistics-init");}public void InitializeActivityWithNullData(){throw new Exception("statistics callback must remain pending");}public bool LoginSourceFlag64=>false;public bool LoginSourceFlag65=>false;public void LogWarning(string s){}public void Handle103VersionBug(){}public void ClearStateEvents(){}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            foreach(bool fail in new[]{false,true})test(fail?"original-main-pre-game-controller-failure-retains-earlier-registration":"original-main-pre-game-full-order-to-pending-preload",()=>{
                var trace=new List<string>();var state=new OutgameMainLifecycleState{ResourcesOnly=true};var fm=new OutgameFsmManager();var pm=new OutgameProcedureManager(()=>fm);int initialized=0,lookups=0;var ids=new List<int>();
                var lm=new OutgameLogicModule(v=>trace.Add("auto:"+v),()=>trace.Add("pool"),()=>{},s=>{},s=>{});lm.Initialize();var host=new Host{Trace=trace};var messages=new OutgameMessageDispatcher();
                var settings=new OutgamePreGameSettings(v=>trace.Add("settings"),()=>new Dictionary<int,string>{{3,"K"}},new OutgameBigNumberSymbols(),v=>{},v=>{});
                var cleanup=new OutgameMainExit(null,null,null,null,null,null,null,null);
                var startup=new OutgameMainPreGameStartup(state,settings,()=>{trace.Add("procedure-service");return pm;},()=>{lookups++;return lm;},id=>{ids.Add(id);if(fail&&ids.Count==2)throw new InvalidOperationException("controller unavailable");return new Control{Init=()=>initialized++};},()=>{trace.Add("new-preload");return new OutgameProcedurePreLoad(host,typeof(OutgameProcedureStarGame));},()=>{trace.Add("new-star");return new OutgameProcedureStarGame(null,null,null,null,null,null,null,null,null,null,null,typeof(OutgameProcedureExitGame));},()=>{trace.Add("new-exit");return new OutgameProcedureExitGame(null,null,null,null);},()=>{Require(initialized==0&&lm.ControllerCount==38,"all registered before audio/init");trace.Add("audio");},()=>{Require(!state.ResourcesOnly&&state.ModulesReady&&initialized==38,"flags after control init before language");trace.Add("language");},()=>trace.Add("common"),()=>trace.Add("report"),trace.Add,()=>messages,cleanup);
                if(fail){bool failed=false;try{startup.Begin();}catch(InvalidOperationException){failed=true;}Require(failed&&lm.ControllerCount==1&&fm.Count==1&&state.ResourcesOnly&&!state.ModulesReady&&initialized==0,"failure preserves prior registration without audio/init/flags");return;}
                startup.Begin();var roster=JsonUtility.FromJson<Roster>(File.ReadAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../analysis/targets/wxcf1394487200e48f/43/generated/outgame/PRE_GAME_REGISTRATION_ROSTER.json"))));Require(ids.Count==roster.controllers.Length,"source registration count");for(int i=0;i<ids.Count;i++)Require(ids[i]==roster.controllers[i].typeIndex,"source registration order");
                Require(lookups==39&&initialized==38&&pm.ProcedureFsm.CurrentState is OutgameProcedurePreLoad&&host.Callback!=null,"per-registration service lookups and genuine preload pending callback");Require(trace.IndexOf("new-preload")<trace.IndexOf("new-star")&&trace.IndexOf("new-star")<trace.IndexOf("new-exit")&&trace.IndexOf("audio")<trace.IndexOf("pool")&&trace.IndexOf("language")<trace.IndexOf("common")&&trace.IndexOf("common")<trace.IndexOf("report")&&trace.IndexOf("report")<trace.IndexOf("preload"),"startup stage order");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
