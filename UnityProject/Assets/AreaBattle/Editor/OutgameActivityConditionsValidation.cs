using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameActivityConditionsValidation
    {
        static void Require(bool ok,string text){if(!ok)throw new Exception(text);}
        static void Throws<T>(Action action,string message=null)where T:Exception
        {try{action();}catch(T error){if(message!=null)Require(error.Message==message,"source error text");return;}throw new Exception("Expected "+typeof(T).Name);}
        sealed class First:OutgameFsmState<object>{}
        sealed class Second:OutgameFsmState<object>{}
        sealed class PayloadState:OutgameFsmState<object>
        {
            public int Bare,Args;public object[] Last;
            public override void OnEnter(OutgameFsm<object> fsm){Bare++;}
            public override void OnEnter(OutgameFsm<object> fsm,object[] args){Args++;Last=args;}
        }
        static OutgameActivityConfigRow Row()=>new OutgameActivityConfigRow{open=1,noticeType=Array.Empty<int>(),noticeParams=Array.Empty<string>(),
            launchType=Array.Empty<int>(),launchParams=Array.Empty<string>(),overType=Array.Empty<int>(),overParams=Array.Empty<string>(),closeType=Array.Empty<int>(),closeParams=Array.Empty<string>()};
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Source activity-item lazy config/condition caches and runtime comparison plus recovered generic FSM event dispatch. Editor state scenarios verify the framework bridge; actual ActivityBase/state/UI/config owner/SevenDay/Main remain pending. No new native or Player."};
            Action<string,Action> check=(id,body)=>{var resolver=OutgameActivityItemData.ConfigurationResolver;try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}finally{OutgameActivityItemData.ConfigurationResolver=resolver;}};
            check("activity-condition-four-lazy-caches-and-typed-arguments",()=>{
                var config=Row();config.noticeType=new[]{10,10000,11};config.noticeParams=new[]{"3_bad_2147483648_-9223372036854775808","20261003120000","9223372036854775807"};
                config.launchType=new[]{21};config.launchParams=new[]{"9_2"};config.overType=new[]{31};config.overParams=new[]{"invalid"};config.closeType=new[]{41};config.closeParams=new[]{"__"};
                int resolves=0;OutgameActivityItemData.ConfigurationResolver=id=>{resolves++;Require(id==7,"lookup current record ID");return config;};var item=new OutgameActivityItemData{id=7};
                var notice=item.NoticeCondition;Require(notice.Length==3&&notice[0].value==long.MinValue&&notice[0].arg.Length==3&&(int)notice[0].arg[0]==3&&(int)notice[0].arg[1]==0&&(int)notice[0].arg[2]==0,"int arguments, invalid/overflow zero, long target");
                Require(notice[1].arg==null&&notice[1].value==OutgameItemTimestamp.FromDateTime(new DateTime(2026,10,3,12,0,0))&&notice[2].value==long.MaxValue&&notice[2].arg.Length==0,"date has null args; number alone gets empty array");
                Require(item.LaunchCondition[0].key==21&&(int)item.LaunchCondition[0].arg[0]==9&&item.OverCondition[0].value==0&&item.CloseCondition[0].arg.Length==2,"all four independent source offsets");
                Require(resolves==1&&ReferenceEquals(notice,item.NoticeCondition),"one cached config and same condition array reference");
            });
            check("activity-condition-null-config-retry-and-cached-config-retention",()=>{
                int reads=0;var config=Row();config.launchType=new[]{10000};config.launchParams=new[]{"bad-date"};
                OutgameActivityItemData.ConfigurationResolver=id=>++reads<2?null:config;var item=new OutgameActivityItemData{id=1};Require(item.Config==null,"null resolution not fabricated");
                Require(item.LaunchCondition[0].value==0&&item.LaunchCondition[0].arg==null&&reads==2,"null config retried and invalid date target zero");
                item.id=2;OutgameActivityItemData.ConfigurationResolver=id=>throw new Exception("unexpected resolver");Require(ReferenceEquals(item.Config,config),"ID change does not invalidate config cache");
                config.launchParams[0]="20261003120000";Require(item.LaunchCondition[0].value==0,"already parsed conditions do not refresh with config edits");
            });
            check("activity-condition-malformed-input-retains-partial-cache",()=>{
                var config=Row();config.noticeType=new[]{7};config.noticeParams=new[]{"3","4"};OutgameActivityItemData.ConfigurationResolver=id=>config;var item=new OutgameActivityItemData();
                Throws<IndexOutOfRangeException>(()=>{_=item.NoticeCondition;});config.noticeType=new[]{8,9};
                var partial=item.NoticeCondition;Require(partial.Length==2&&partial[0].key==7&&partial[0].value==3&&partial[1].key==0&&partial[1].arg==null,"cache committed before parse; repeat access returns partial array");
                config.launchType=new[]{12};config.launchParams=new string[]{null};Throws<NullReferenceException>(()=>{_=item.LaunchCondition;});
                Require(item.LaunchCondition[0].key==12&&item.LaunchCondition[0].arg==null,"key assigned before null text split failure");
                config.overParams=null;Throws<NullReferenceException>(()=>{_=item.OverCondition;});config.overParams=Array.Empty<string>();Require(item.OverCondition.Length==0,"failure before allocation permits retry");
            });
            check("activity-condition-runtime-open-gate-live-target-and-source-type",()=>{
                var owner=new OutgameStatisticsControl{ValueProviders=new Dictionary<int,Func<object[],long>>()};var stats=new OutgameStatisticsExpansion(()=>owner,a=>{});var config=Row();
                var args=new object[]{7};var conditions=new[]{new OutgameActivityCondition{key=99,value=10,arg=args}};int calls=0;
                owner.ValueProviders[5]=a=>{calls++;Require(ReferenceEquals(a,args),"pass cached argument array without copy");conditions[0].value=8;return 8;};
                Require(OutgameActivityConditions.Met(config,new[]{5},conditions,stats)&&calls==1,"supplied type ID used; target read after provider mutation");
                config.open=0;Require(!OutgameActivityConditions.Met(config,null,null,null),"disabled config returns before arrays/providers");config.open=-2;
                Require(OutgameActivityConditions.Met(config,Array.Empty<int>(),null,null),"any nonzero open and no conditions true");
                owner.ValueProviders[5]=a=>-1;conditions[0].value=0;Require(!OutgameActivityConditions.Met(config,new[]{5,999},conditions,stats),"first unmet stops before mismatched later array");
                Throws<IndexOutOfRangeException>(()=>OutgameActivityConditions.Met(config,new[]{5},Array.Empty<OutgameActivityCondition>(),stats));
            });
            check("activity-condition-original-config-and-json-cache-reconstruction",()=>{
                var catalog=new OutgameActivityConfig(Resources.Load<TextAsset>("Data/Outgame/PubActivityConfig").text,s=>throw new Exception(s));
                OutgameActivityItemData.ConfigurationResolver=catalog.Get;var item=new OutgameActivityItemData{id=103002};var config=item.Config;
                Require(config!=null&&item.LaunchCondition.Length==config.launchParams.Length&&item.NoticeCondition.Length==config.noticeParams.Length&&item.OverCondition.Length==config.overParams.Length&&item.CloseCondition.Length==config.closeParams.Length,"actual original local config supports all four arrays");
                string json=JsonUtility.ToJson(item);Require(!json.Contains("cachedConfig")&&!json.Contains("Condition")&&!json.Contains("ConfigurationResolver"),"private caches and static resolver excluded from persistent JSON");
                var restored=JsonUtility.FromJson<OutgameActivityItemData>(json);Require(!ReferenceEquals(item.LaunchCondition,restored.LaunchCondition)&&restored.LaunchCondition[0].value==item.LaunchCondition[0].value,"restart rebuilds private caches from configuration");
            });
            check("activity-fsm-event-current-state-identity-null-and-state-change",()=>{
                var first=new First();var second=new Second();var owner=new object();var fsm=new OutgameFsm<object>("activity",owner,new IOutgameFsmState<object>[] {first,second});var sender=new object();var payload=new object();int a=0,b=0;
                first.SubscribeEvent(4,(f,s,d)=>{a++;Require(ReferenceEquals(f,fsm)&&ReferenceEquals(s,sender)&&d==null,"two-arg overload supplies null userData");f.ChangeState<Second>();});
                second.SubscribeEvent(4,(f,s,d)=>{b++;Require(ReferenceEquals(d,payload),"three-arg overload preserves payload");});
                Throws<OutgameFrameworkException>(()=>fsm.FireEvent(sender,4),"Current state is invalid.");fsm.Start<First>();fsm.FireEvent(sender,4);fsm.FireEvent(sender,4,payload);
                Require(a==1&&b==1&&ReferenceEquals(fsm.CurrentState,second),"dispatch follows active state and committed transition");fsm.FireEvent(sender,99);Require(a==1&&b==1,"unknown event no-op");
            });
            check("activity-fsm-event-duplicates-remove-one-reentry-and-snapshot",()=>{
                var state=new First();var fsm=new OutgameFsm<object>("activity",new object(),new[]{state});fsm.Start<First>();var trace=new List<string>();bool nested=false;
                OutgameFsmEventHandler<object> tail=(f,s,d)=>trace.Add("tail");
                OutgameFsmEventHandler<object> first=(f,s,d)=>{trace.Add("first");state.UnsubscribeEvent(7,tail);if(!nested){nested=true;f.FireEvent(null,7);}};
                state.SubscribeEvent(7,first);state.SubscribeEvent(7,tail);state.SubscribeEvent(7,tail);fsm.FireEvent(null,7);
                Require(string.Join(",",trace)=="first,first,tail,tail,tail","Remove removes one last match; each dispatch holds multicast snapshot");
                state.UnsubscribeEvent(7,first);trace.Clear();fsm.FireEvent(null,7);Require(trace.Count==0,"null-valued subscription key is silent");
                state.SubscribeEvent(7,tail);fsm.FireEvent(null,7);Require(trace.Count==1,"subscribe works after retained null slot");
                Throws<OutgameFrameworkException>(()=>state.SubscribeEvent(8,null),"Event handler is invalid.");Throws<OutgameFrameworkException>(()=>state.UnsubscribeEvent(999,null),"Event handler is invalid.");
            });
            check("activity-fsm-event-exception-and-destroyed-state-subscriptions",()=>{
                var state=new First();var fsm=new OutgameFsm<object>("activity",new object(),new[]{state});fsm.Start<First>();int calls=0;
                state.SubscribeEvent(3,(f,s,d)=>{calls++;throw new InvalidOperationException("handler");});state.SubscribeEvent(3,(f,s,d)=>calls++);
                Throws<InvalidOperationException>(()=>fsm.FireEvent(null,3));Require(calls==1&&ReferenceEquals(fsm.CurrentState,state),"failure stops multicast tail without changing FSM");
                fsm.Shutdown();state.OnEvent(fsm,null,3,null);Require(calls==1,"base OnDestroy clears state dictionary");Throws<OutgameFrameworkException>(()=>fsm.FireEvent(null,3),"Current state is invalid.");
                state.SubscribeEvent(3,(f,s,d)=>calls++);state.OnEvent(null,null,3,null);Require(calls==2,"OnEvent has no independent FSM validity guard");
            });
            check("activity-fsm-state-base-overloads-and-change-state-forwarding",()=>{
                var first=new First();var next=new PayloadState();var fsm=new OutgameFsm<object>("activity",new object(),new IOutgameFsmState<object>[] {first,next});fsm.Start<First>();var args=new object[]{8};
                Throws<OutgameFrameworkException>(()=>first.ChangeState<PayloadState>(null),"FSM is invalid.");first.ChangeState<PayloadState>(fsm,args);
                Require(next.Args==1&&next.Bare==0&&ReferenceEquals(next.Last,args),"change helper preserves args array");first.ChangeState<PayloadState>(fsm,(object[])null);Require(next.Bare==1,"null args calls bare overload");
                var plain=new OutgameFsmState<object>();plain.OnEnter(null,new object[]{1});plain.OnInit(null);plain.OnUpdate(null,1,2);plain.OnLeave(null,false);plain.OnDestroy(null);
            });
            check("activity-fsm-conditioned-event-uses-recovered-statistics-and-cache",()=>{
                var config=Row();config.launchType=new[]{10700};config.launchParams=new[]{"8_4"};OutgameActivityItemData.ConfigurationResolver=id=>config;var item=new OutgameActivityItemData();
                using(var statistics=new OutgameStatisticsOffNetValidation.Fixture())
                {
                    statistics.Init();statistics.Expansion.SetEventCount(10700,8,3);var first=new First();var second=new Second();var manager=new OutgameFsmManager();var fsm=manager.CreateFsm<object>(new object(),first,second);
                    first.SubscribeEvent(1,(f,s,d)=>{if(OutgameActivityConditions.Met(config,config.launchType,item.LaunchCondition,statistics.Expansion))f.ChangeState<Second>();});fsm.Start<First>();fsm.FireEvent(item,1);
                    Require(ReferenceEquals(fsm.CurrentState,first),"item-specific statistics below parsed target stays in current state");statistics.Expansion.AddEventCount(10700,8,1);fsm.FireEvent(item,1);
                    Require(ReferenceEquals(fsm.CurrentState,second),"actual recovered statistics reaches target and real FSM transitions");manager.Shutdown();Require(fsm.IsDestroyed,"actual manager tears down state subscriptions");
                }
            });
            return report;
        }
    }
}
