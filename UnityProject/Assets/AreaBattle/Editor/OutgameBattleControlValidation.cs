using System;
using System.Collections.Generic;
using UnityEngine;
using Row=AreaBattle.OriginalConfig.DispatchConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameBattleControlValidation
    {
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameLegacyConfigManager Config()
        {
            var config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),null,null,null,null,null,null);
            new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),s=>throw new Exception(s)).ReadTable(config.dicDispatch);
            return config;
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source BattleControl lifecycle/configuration with recovered records and existing battle-kernel parity; complete Main and other controllers remain pending. No new native or Player claim."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("battle-control-original-dispatch-and-existing-battle-parity",()=>{
                var config=Config();var c=new OutgameBattleControl(()=>config,new OutgameControllerRegistry());c.OnInit();
                var sim=new BattleSimulation(new LevelLayout{StarInfoCfgs=Array.Empty<StarInfoCfg>()},BattleView.ReadConfig(),1,(a,b)=>false);
                foreach(int grade in new[]{int.MinValue,-1,0,1,2,3,int.MaxValue}){
                    Require(c.GetDispatchLineNum(grade)==sim.GetDispatchLineNum(grade)&&c.GetDispatchScoreNum(grade)==sim.GetDispatchScoreNum(grade)&&c.GetDispatchAddScoreTime(grade)==sim.GetDispatchAddScoreTime(grade),"source rows agree with preserved battle kernel");
                    foreach(int lines in new[]{int.MinValue,-1,0,1,2,3,4,int.MaxValue})Require(c.GetSpawnTime(grade,lines)==sim.GetSpawnTime(grade,lines),"source default branch and millisecond conversion");
                }
                Require(c.GetDispatchScoreNum(0)==9&&c.GetDispatchScoreNum(1)==29&&c.GetDispatchScoreNum(2)==65&&c.GetSpawnTime(0,1)==1.111f,"original three ordinary records");
            });
            check("battle-control-captured-row-reference-and-live-field-mutation",()=>{
                var config=Config();int reads=0;var c=new OutgameBattleControl(()=>{reads++;return config;},new OutgameControllerRegistry());c.OnInit();Require(reads==3,"source rereads ConfigMgr for each row");
                var first=config.dicDispatch[1];first.scoreLimit=-11;first.swanpSpaceOne=int.MaxValue;first.addSpace=-25;
                config.dicDispatch[1]=new Row{scoreLimit=77};
                Require(c.GetDispatchScoreNum(0)==-11&&c.GetSpawnTime(0,1)==(float)int.MaxValue/1000f&&c.GetDispatchAddScoreTime(0)==-.025f&&reads==3,"no clamp, copy or dictionary reread in getters");
                c.OnInit();Require(reads==6&&c.GetDispatchScoreNum(0)==77,"explicit reinitialization replaces captured records");
            });
            check("battle-control-init-failure-keeps-source-partial-state",()=>{
                var config=Config();int reads=0;var c=new OutgameBattleControl(()=>{reads++;return config;},new OutgameControllerRegistry());c.OnInit();
                config.dicDispatch[1]=new Row{scoreLimit=123};config.dicDispatch.Remove(2);config.dicDispatch[3]=new Row{scoreLimit=456};
                Throws<KeyNotFoundException>(c.OnInit);Require(reads==5&&c.GetDispatchScoreNum(0)==123&&c.GetDispatchScoreNum(1)==29&&c.GetDispatchScoreNum(2)==65,"failure after first assignment retains old second/third rows");
                config.dicDispatch[2]=null;c.OnInit();Require(c.GetDispatchScoreNum(2)==456,"null row accepted at initialization");Throws<NullReferenceException>(()=>c.GetDispatchLineNum(1));
                var uninitialized=new OutgameBattleControl(()=>config,new OutgameControllerRegistry());Throws<NullReferenceException>(()=>uninitialized.GetSpawnTime(0,1));
            });
            check("battle-control-registry-logic-lifecycle-and-old-disposal",()=>{
                var config=Config();var registry=new OutgameControllerRegistry();OutgameCoreControllerBindings.BindBattle(registry,()=>config);
                var control=(OutgameBattleControl)registry.Resolve(4060);var trace=new List<string>();
                var logic=new OutgameLogicModule(b=>trace.Add("auto:"+b),()=>trace.Add("data-init"),()=>trace.Add("data-release"),s=>{},s=>{});
                logic.Initialize();logic.RegisterLogicCtr(control,false);logic.InitCtrl(true);logic.Update(float.NaN,float.PositiveInfinity);
                Require(ReferenceEquals(control,registry.Resolve(4060))&&control.GetDispatchLineNum(0)==1&&string.Join("|",trace)=="auto:True|data-init","real module initializes bound controller after data pool");
                logic.Shutdown();Require(!registry.HasInstance(4060)&&control.GetDispatchLineNum(0)==1&&trace[2]=="data-release","shutdown clears singleton after data release, retains row fields");
                var replacement=registry.Resolve(4060);control.OnDispose();Require(!ReferenceEquals(control,replacement)&&!registry.HasInstance(4060),"old instance unconditionally clears replacement singleton");
            });
            check("battle-control-init-resolves-live-config-three-times",()=>{
                var a=Config();var b=Config();var d=Config();a.dicDispatch[1].scoreLimit=101;b.dicDispatch[2].scoreLimit=202;d.dicDispatch[3].scoreLimit=303;
                int index=0;var configs=new[]{a,b,d};var control=new OutgameBattleControl(()=>configs[index++],new OutgameControllerRegistry());control.OnInit();
                Require(index==3&&control.GetDispatchScoreNum(0)==101&&control.GetDispatchScoreNum(1)==202&&control.GetDispatchScoreNum(2)==303,"provider replacement between assignments remains observable");
            });
            return report;
        }
    }
}
