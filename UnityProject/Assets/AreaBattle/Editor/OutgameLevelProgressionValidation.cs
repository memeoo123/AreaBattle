using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLevelProgressionValidation
    {
        public sealed class Effects:IOutgameLevelEffects
        {
            public readonly List<string> calls=new List<string>();public Action save;
            public void SetStatistic(int id,long count){calls.Add(id+":"+count);}
            public void SaveLocalData(){calls.Add("save");save?.Invoke();}
        }
        static void Require(bool ok,string text){if(!ok)throw new Exception(text);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Held LevelID and mode getters/setter only; no full first-account/login/level-entry implementation claim."};
            try
            {
                string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"level.json");
                var store=new OutgameProfileStore(path);var profile=new OutgameProfile{levelID=6};
                var effects=new Effects{save=()=>store.Save(profile)};var levels=new OutgameLevelProgression(profile,effects);
                levels.CurrentLevel=14;
                Require(levels.CurrentLevel==14&&levels.RealCurrentLevel==14&&store.Load().levelID==14,"normal level survives restart");
                Require(string.Join(",",effects.calls)=="10015:13,save","source statistic then save");
                report.checks.Add(new BattleBuild.Check{id="outgame-level-normal-progress-persist-and-statistic",result="pass"});
                effects.calls.Clear();levels.SpecialState=1;levels.SpecialLevel=999;levels.CurrentLevel=21;
                Require(levels.CurrentLevel==999&&levels.RealCurrentLevel==14&&store.Load().levelID==14&&effects.calls.Count==0,"special setter ignored, real level preserved");
                levels.SpecialState=0;Require(levels.CurrentLevel==14,"return restores normal level");
                report.checks.Add(new BattleBuild.Check{id="outgame-level-special-mode-keeps-real-progress",result="pass"});
                var bankTrace=new List<string>();
                var bank=new OutgamePiggyBank(profile,levels,(activity,level,old,value)=>{Require(profile.inventory.collectNum.ToString()==old,"report before mutation");bankTrace.Add(activity+":"+level+":"+old+":"+value);},(activity,level,a,b)=>{Require(profile.inventory.collectNum==0&&profile.inventory.collectAdNum==0&&a==null&&b==null,"reset reporting after both fields clear");bankTrace.Add("reset:"+level);});
                levels.CurrentLevel=29;profile.inventory.collectNum=990;bank.OnGamePlayState(8);Require(bank.GetBankState()==0&&bank.CollectNum==990&&bankTrace.Count==0,"bank gated below30");
                levels.CurrentLevel=30;Require(bank.GetBankState()==1,"normal bank at threshold");bank.OnGamePlayState(9);Require(bankTrace.Count==0,"loss does not add bank funds");bank.OnGamePlayState(8);
                Require(bank.CollectNum==1000&&bank.GetBankState()==2&&bankTrace.Count==1&&bankTrace[0].EndsWith(":30:990:1010"),"win reports requested1010 then caps1000");
                store.Save(profile);Require(store.Load().inventory.collectNum==1000,"bank shares persisted original inventory field");
                bank.OnGM_SetPiggyNum("-2");Require(bank.CollectNum==-2&&!bank.IsFull,"source has upper cap only");
                profile.inventory.collectAdNum=7;bank.Reset();Require(bankTrace[bankTrace.Count-1]=="reset:30","reset source report order");
                levels.SpecialState=1;levels.SpecialLevel=12;Require(bank.GetBankState()==0,"bank uses current special level, not real level");levels.SpecialState=0;
                report.checks.Add(new BattleBuild.Check{id="outgame-piggy-original-inventory-win-cap-report-reset",result="pass"});
                var messages=new OutgameMessageDispatcher();int eventReports=0;
                var subscribed=new OutgamePiggyBank(profile,levels,(a,b,c,d)=>eventReports++,(a,b,c,d)=>{},()=>messages);
                profile.inventory.collectNum=0;subscribed.OnInit();Require(subscribed.ActiveUpdate,"source initialization enables updates");
                messages.SendMessage("GamePlayState",new object[]{8});Require(profile.inventory.collectNum==20&&eventReports==1,"source message reaches subscribed bank");
                subscribed.OnDispose();messages.SendMessage("GamePlayState",new object[]{8});Require(profile.inventory.collectNum==20,"dispose removes bank listener");
                subscribed.OnInit();messages.SendMessage("GamePlayState",new object[]{8});Require(profile.inventory.collectNum==40,"new init/dispose lifecycle grants once");
                subscribed.OnInit();messages.SendMessage("GamePlayState",new object[]{8});Require(profile.inventory.collectNum==80,"source duplicate registration retains duplicate delegates");
                subscribed.OnDispose();messages.SendMessage("GamePlayState",new object[]{8});Require(profile.inventory.collectNum==100,"remove deletes one duplicate");
                levels.CurrentLevel=29;messages.SendMessage("GamePlayState",null);Require(profile.inventory.collectNum==100,"level gate precedes event payload read");subscribed.OnDispose();
                report.checks.Add(new BattleBuild.Check{id="outgame-piggy-source-message-subscribe-dispose",result="pass"});
                long sdk=OutgameItemTimestamp.FromDateTime(new DateTime(2026,9,30,23,59,59));int sdkReads=0,net=1,cleared=0;
                var clockMessages=new OutgameMessageDispatcher();var clockEvents=new List<string>();
                clockMessages.AddListener("RefreshNetTime",args=>clockEvents.Add("refresh"));clockMessages.AddListener("Time_NewDay",args=>clockEvents.Add("day"));
                var clock=new OutgameServerClock(()=>{sdkReads++;return sdk;},()=>net,()=>false,()=>new DateTime(2026,10,2),()=>clockMessages,()=>cleared++);
                clock.OnInit();Require(sdkReads==4,"source init performs two refreshes with two positive SDK reads each");clock.OnInit();Require(sdkReads==4,"clock init is guarded");
                clock.Updata(100,1);Require(sdkReads==4,"strictly greater than one realtime second");clock.Updata(0,.1f);Require(sdkReads==6&&clockEvents.Count==0,"first observed day does not emit new-day");
                sdk+=2000;clock.Updata(0,1);Require(string.Join(",",clockEvents)=="refresh,day"&&clock.GetNowDateTime().Day==1,"midnight emits source messages in order");
                clock.DebugOffset=3600000;Require(clock.GetNowTimestampLong()==sdk+3600000&&clock.GetNowTimes()==(int)(sdk/1000),"debug offset affects long getter only");
                int readBefore=sdkReads;clockMessages.SendMessage("GamePause",new object[]{true});Require(sdkReads==readBefore,"pause does not refresh");clockMessages.SendMessage("GamePause",new object[]{false});Require(sdkReads==readBefore+2,"resume refreshes");
                sdk=0;net=0;clock.Updata(0,1);Require(!clock.ConnectedNext&&clock.GetNowDateTime()==new DateTime(2026,10,2),"nonpositive SDK time uses original local epoch conversion");
                var countdown=new OutgameActivityCountdown(()=>4,clock.GetNowTimestampLong){EndTimeStamp=clock.GetNowTimestampLong()+61000};countdown.RefreshRemainingTime();Require(countdown.MinTime==1&&countdown.SecTime==1,"activity countdown reads concrete server clock");
                readBefore=sdkReads;clock.OnDispose();clockMessages.SendMessage("GamePause",new object[]{false});clock.Updata(0,100);Require(sdkReads==readBefore&&cleared==1,"dispose removes event and stops ticking");
                report.checks.Add(new BattleBuild.Check{id="outgame-server-clock-source-init-refresh-rollover",result="pass"});
                long activityNow=50;int activityNetwork=1;bool activityEnabled=true;
                var activityClock=new OutgameServerClock(()=>activityNow,()=>activityNetwork,()=>true);activityClock.OnInit();
                var activityData=OutgameFestActivityData.Read(null);var activityReports=new List<string>();
                var activity=new OutgameFestActivityState(()=>activityData,()=>activityEnabled,activityClock,levels,(kind,name,level,a,b)=>activityReports.Add(kind+":"+activityData.status)){NoticeTime=100,StartTimeStamp=200,EndTimeStamp=300,UnlockLevel=30,ActivityName="source-name-fixture"};
                levels.CurrentLevel=31;activity.Refresh();Require(activity.Status==1,"before notice inactive");
                activityNow=100;activityClock.Updata(0,1.1f);activity.Refresh();Require(activity.Status==3&&string.Join(",",activityReports)=="warmup:1","notice boundary warms up then unlocked prestart3");
                levels.CurrentLevel=30;activity.Refresh();Require(activity.Status==2,"level equality remains gated");
                activityNow=200;activityClock.Updata(0,1);activity.Refresh();Require(activity.Status==2,"start boundary does not bypass level gate");
                levels.CurrentLevel=31;activity.Refresh();Require(activity.Status==4&&activityReports[1]=="unlock:2","active unlock reports prior stored state");
                activityNetwork=0;activityClock.Updata(0,1);activity.Refresh();Require(activity.Status==6,"disconnect converts active state to6");
                activityNetwork=1;activityClock.Updata(0,1);activity.Refresh();Require(activity.Status==4&&activityReports.Count==2,"reconnect from6 does not repeat unlock report");
                activityNow=300;activityClock.Updata(0,1);activity.Refresh();Require(activity.Status==5&&activityReports[2]=="complete:4","end boundary completes");
                activityEnabled=false;activityData.status=3;Require(activity.Status==5&&activityData.status==3,"disabled config masks persisted state");
                activityData.openActTime=100;activityData.rewardId=9;activityData.CheckInit(100,300,()=>999);Require(activityData.rewardId==9,"inclusive cycle boundaries preserve data");
                activityData.openActTime=99;activityData.CheckInit(100,300,()=>150);var restored=OutgameFestActivityData.Read(activityData.ToOriginalJson());
                Require(restored.status==1&&restored.rewardId==1&&restored.limetSkinStatus==0&&restored.nextGetAwardTime==0&&restored.isShowPanel&&restored.openActTime==150,"new cycle resets original serialized fields");activityClock.OnDispose();
                report.checks.Add(new BattleBuild.Check{id="outgame-fest-state-time-level-network-cycle-boundaries",result="pass"});
                var configErrors=new List<string>();var activityConfigs=new OutgameActivityConfig(BattleView.ReadText("Data/Outgame/PubActivityConfig"),configErrors.Add);
                var original=activityConfigs.Read(103002);
                Require(original.UnlockLevel==6&&OutgameItemTimestamp.ToDateTime(original.Start)==new DateTime(2022,2,13)&&OutgameItemTimestamp.ToDateTime(original.End)==new DateTime(2022,2,27)&&OutgameItemTimestamp.ToDateTime(original.Notice)==new DateTime(2023,2,10),"original inconsistent historical activity dates preserved");
                var configuredCountdown=new OutgameActivityCountdown(()=>activity.Status,activityClock.GetNowTimestampLong);activityConfigs.Apply(103002,activity,configuredCountdown);
                Require(activity.ActivityName=="情人节活动"&&activity.NoticeTime>activity.EndTimeStamp&&configuredCountdown.StartTimeStamp==original.Start,"local config connects state and countdown without fixing source date order");
                Require(activityConfigs.Read(-1).Start==0&&configErrors.Count==0,"absent config returns zero tuple");
                var row=activityConfigs.Get(103002);row.launchParams[0]="2022-02-13";Require(activityConfigs.Read(103002).Start==0&&configErrors.Count==1,"strict original timestamp parse rejects alternative formatting");
                report.checks.Add(new BattleBuild.Check{id="outgame-activity-original-config-dates-and-binding",result="pass"});
                effects.calls.Clear();levels.CurrentLevel=0;Require(levels.CurrentLevel==0&&effects.calls[0]=="10015:-1","source setter does not silently clamp");
                report.checks.Add(new BattleBuild.Check{id="outgame-level-source-zero-not-clamped",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-level-progress",result="fail",detail=e.ToString()});}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-level-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
