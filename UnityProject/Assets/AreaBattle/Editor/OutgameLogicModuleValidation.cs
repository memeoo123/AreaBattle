using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLogicModuleValidation
    {
        sealed class Control:IOutgameLogicControl
        {
            public Action Init=()=>{},Dispose=()=>{};public Action<float,float> Tick=(a,b)=>{};
            public void OnInit()=>Init();public void Updata(float a,float b)=>Tick(a,b);public void OnDispose()=>Dispose();
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,action)=>{try{action();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-logic-registration-live-init-and-fast-mode",()=>{
                var trace=new List<string>();var warnings=new List<string>();var module=new OutgameLogicModule(value=>trace.Add("auto:"+value),()=>trace.Add("pool"),()=>trace.Add("release"),warnings.Add,trace.Add);
                module.Initialized=()=>{Require(module.IsInitialized&&module.ControllerCount==0,"init callback state");trace.Add("ready");};module.Initialize();
                var second=new Control{Init=()=>trace.Add("second")};var first=new Control{Init=()=>{trace.Add("first");module.RegisterLogicCtr(second,false);}};
                module.RegisterLogicCtr(first,false);module.RegisterLogicCtr(first,true);Require(warnings.Count==1&&warnings[0]=="逻辑控制类[Control]已经被注册"&&module.ControllerCount==1,"duplicate does not reinitialize");
                module.InitCtrl(false);Require(string.Join(",",trace)=="ready,auto:False,pool,框架急速模式启动","fast mode initializes data pool but skips controllers");
                trace.Clear();module.InitCtrl(true);Require(string.Join(",",trace)=="auto:True,pool,first,second"&&module.ControllerCount==2,"live init sees appended controller");
                module.Initialize();Require(module.ControllerCount==0&&module.IsInitialized&&module.Priority==12,"Initialize replaces registration list again");
            });
            test("original-logic-update-mutation-and-shutdown-order",()=>{
                var trace=new List<string>();var module=new OutgameLogicModule(value=>{},()=>{},()=>trace.Add("pool-release"),message=>{},message=>{});module.Initialize();
                var added=new Control();var first=new Control{Tick=(dt,udt)=>{Require(dt==.25f&&udt==.5f,"forward both deltas");module.RegisterLogicCtr(added,false);},Dispose=()=>trace.Add("first-dispose")};module.RegisterLogicCtr(first,false);
                bool failed=false;try{module.Update(.25f,.5f);}catch(InvalidOperationException){failed=true;}Require(failed&&module.ControllerCount==2,"foreach mutation propagates after registration");
                added.Dispose=()=>trace.Add("second-dispose");module.Shutdown();Require(string.Join(",",trace)=="pool-release,first-dispose,second-dispose"&&module.ControllerCount==0&&module.IsInitialized,"release precedes dispose and clear; initialized flag retained");
            });
            test("original-logic-failures-preserve-registration",()=>{
                var module=new OutgameLogicModule(value=>{},()=>{},()=>{},message=>{},message=>{});module.Initialize();var control=new Control{Init=()=>throw new InvalidOperationException("init"),Dispose=()=>throw new InvalidOperationException("dispose")};
                bool failed=false;try{module.RegisterLogicCtr(control,true);}catch(InvalidOperationException){failed=true;}Require(failed&&module.ControllerCount==1,"registered before immediate init failure");
                failed=false;try{module.Shutdown();}catch(InvalidOperationException){failed=true;}Require(failed&&module.ControllerCount==1,"failed disposal prevents final clear");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
