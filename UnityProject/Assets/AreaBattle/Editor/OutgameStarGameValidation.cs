using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameStarGameValidation
    {
        sealed class EntryHost:IOutgameStartupEntryHost
        {
            public readonly List<string> Trace=new List<string>();public Action Loaded;
            public void ShowMenu(){Trace.Add("menu");}public void SetMenuVisible(bool v){Trace.Add("visible:"+v);}public void ShowCommonReward(){Trace.Add("reward");}
            public void InitializePrefabs(){Trace.Add("prefabs");}public void LoadScene(string n,Action c,bool o){Trace.Add(n);Loaded=c;}
            public bool EnterGame {get;set;}public void LateInitializeModule(){}public bool RedDotSourceFlag8 {get;set;}public int RedDotSourceValue16 {get;set;}
            public void SendLoadGameScreen(){Trace.Add("screen");}public int CurrentLevel=>3;public void ReportActivityEnter(string a,string l){}public void InitializeLevelRank(){}public void InitializeSevenDayActivity(){}public void SetPlayState(int s){}public void CloseLoading(){Trace.Add("close");}public void ReportGameInteractive(string m){}
        }
        sealed class Exit:IOutgameFsmState<IOutgameProcedureManager>
        {
            public bool Entered;public void OnInit(OutgameFsm<IOutgameProcedureManager> f){}public void OnEnter(OutgameFsm<IOutgameProcedureManager> f){Entered=true;}public void OnEnter(OutgameFsm<IOutgameProcedureManager> f,object[] a){}public void OnUpdate(OutgameFsm<IOutgameProcedureManager> f,float a,float b){}public void OnLeave(OutgameFsm<IOutgameProcedureManager> f,bool s){}public void OnDestroy(OutgameFsm<IOutgameProcedureManager> f){}
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-star-game-entry-mapping-listeners-and-scene-composition",()=>{
                var h=new EntryHost();var trace=h.Trace;var messages=new OutgameMessageDispatcher();var runtime=new OutgameLegacyBundleRuntime(()=>null,s=>{},s=>{},s=>{},e=>{});var registrations=new List<int>();int touches=0;bool exiting=false;
                messages.AddListener("MineGameExitLogic",a=>{exiting=true;trace.Add("exit-message");});
                var state=new OutgameProcedureStarGame(()=>new[]{new WXAMSConfig{ReportLable="reward",PlacementId=7}},map=>{Require(map["reward"]==7&&!runtime.DisableUnload,"mapping before unload flag");trace.Add("mapping");},runtime,()=>messages,(label,id)=>{Require(runtime.DisableUnload,"flag before registrations");registrations.Add(id);},trace.Add,()=>trace.Add("report"),()=>trace.Add("day"),()=>touches++,()=>{},new OutgameStartupEntry(h,()=>trace.Add("migrate")),typeof(Exit));
                var manager=new OutgameFsmManager();var procedure=new OutgameProcedureManager(()=>manager);var exit=new Exit();procedure.Register(state,exit);procedure.StartProcedure<OutgameProcedureStarGame>();
                Require(registrations.Count==18&&registrations[0]==34155&&registrations[8]==30171&&registrations[17]==34145,"original ordered18 callbacks");Require(trace.IndexOf("mapping")<trace.IndexOf("report")&&trace.IndexOf("report")<trace.IndexOf("day")&&trace.IndexOf("day")<trace.IndexOf("menu")&&trace.IndexOf("migrate")<trace.IndexOf("GamePlay"),"entry composition order");h.Loaded();Require(trace.Contains("close"),"scene completion uses existing entry");
                manager.Update(65.25f,99);Require(state.Elapsed==35.25f&&touches==1,"subtract30 only once per update");messages.SendMessage("GamePause",new object[]{true});Require(state.Elapsed==.25f&&touches==2,"pause truncates whole seconds");messages.SendMessage("ReadyExitGame");Require(exiting&&exit.Entered,"exit message precedes transition");messages.SendMessage("GamePause",new object[]{true});messages.SendMessage("ReadyExitGame");Require(touches==2&&runtime.DisableUnload,"leave unregisters both listeners and retains unload flag");
            });
            test("original-star-game-duplicate-ad-label-stops-before-side-effects",()=>{
                var runtime=new OutgameLegacyBundleRuntime(()=>null,s=>{},s=>{},s=>{},e=>{});int published=0;
                var state=new OutgameProcedureStarGame(()=>new[]{new WXAMSConfig{ReportLable="x"},new WXAMSConfig{ReportLable="x"}},m=>published++,runtime,null,null,s=>{},null,null,null,null,null,typeof(Exit));bool failed=false;try{state.OnEnter(null);}catch(ArgumentException){failed=true;}Require(failed&&published==0&&!runtime.DisableUnload,"duplicate Add propagates before publishing mapping or changing flags");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
