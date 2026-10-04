using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameTimeToRefreshValidation
    {
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public sealed class Fixture:IDisposable
        {
            public long Now=Stamp(new DateTime(2026,10,3,12,0,0));public DateTime Local=new DateTime(2026,10,4,8,0,0);
            public bool Release=true;public int ServerReads,LocalReads;public Action Disposables;
            public OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public OutgameTimeToRefreshControl Control;
            public Fixture()
            {
                OutgameTimeToRefreshControl.Groups.Clear();
                OutgameTimeToRefreshControl.Services=new OutgameRefreshServices{IsReleaseVersion=()=>Release,ServerTime=()=>{ServerReads++;return Now;},LocalNow=()=>{LocalReads++;return Local;},
                    Messages=()=>Messages,GetDisposableActions=()=>Disposables,SetDisposableActions=a=>Disposables=a};
                Control=new GameObject("refresh-edit-fixture").AddComponent<OutgameTimeToRefreshControl>();Control.enabled=false;
            }
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Control.gameObject);}
        }
        public static long Stamp(DateTime value)=>OutgameItemTimestamp.FromDateTime(value);
        static OutgameRefreshData Row=>OutgameTimeToRefreshControl.Groups[0].Records[0];
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source4589 public scheduling and V1/V2 mutation contracts. Native singleton/scaled cleanup and offnet automatic save/restart verified separately; SDK/Main/production activities remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("time-refresh-duration-boundary-and-double-countdown",()=>{
                using(var f=new Fixture())
                {
                    var order=new List<string>();long stamp=0,countdown=0;
                    f.Control.AddRefreshHandle(6,f.Now-86400000,v=>{stamp=v;order.Add("refresh");},v=>{countdown=v;order.Add("countdown");},86400);
                    Require(order.Count==0,"registration does not invoke callback");f.Control.Advance(1);
                    Require(stamp==Stamp(new DateTime(2026,10,3,6,0,0))&&countdown==18*3600000&&string.Join(",",order)=="refresh,countdown,countdown","source boundary and duplicate due countdown");
                    f.Control.Advance(1);Require(order.Count==4,"not-due row gets only countdown");
                }
            });
            check("time-refresh-midnight-is-not-duration-and-hour-rolls-back",()=>{
                using(var f=new Fixture())
                {
                    int refresh=0;f.Now=Stamp(new DateTime(2026,10,4,0,0,1));f.Control.AddRefreshHandle(6,f.Now-2000,v=>refresh++,null,86400);f.Control.Advance(1);
                    Require(refresh==0,"crossing midnight alone does not satisfy duration");Row.LastRefreshTime=OutgameItemTimestamp.ToDateTime(f.Now-86400000);f.Control.Advance(1);
                    Require(refresh==1&&Row.LastRefreshTime==new DateTime(2026,10,3,6,0,0),"future refresh hour aligns previous day");
                    Row.Interval=TimeSpan.FromSeconds(1);f.Control.Advance(1);f.Control.Advance(1);Require(refresh==3,"short interval can repeatedly refresh same boundary");
                }
            });
            check("time-refresh-scaled-single-pass-release-clock-and-nan",()=>{
                using(var f=new Fixture())
                {
                    f.Control.Advance(.5f);Require(f.ServerReads==0,"subsecond does not consult clock");f.Control.Advance(2.5f);Require(f.ServerReads==1&&f.Control.Elapsed==2,"overshoot subtracts once");
                    f.Now=-1;f.Control.Advance(0);Require(f.LocalReads==1&&OutgameTimeToRefreshControl.Timestamp==Stamp(f.Local),"nonpositive release clock falls back");
                    f.Release=false;f.Control.Advance(0);Require(f.ServerReads==2&&f.LocalReads==2,"nonrelease skips SDK");f.Control.Advance(float.NaN);Require(float.IsNaN(f.Control.Elapsed)&&f.LocalReads==2,"NaN freezes threshold");
                }
            });
            check("time-refresh-group-merge-reference-keys-and-existing-v2-countdown-quirk",()=>{
                using(var f=new Fixture())
                {
                    var a=new object[]{1};var b=new object[]{1};var trace=new List<string>();Action<long,object[]> refresh=(v,args)=>trace.Add("r"),countdown=(v,args)=>trace.Add("c");
                    f.Control.AddRefreshHandleV2(0,f.Now,refresh,countdown,86400,a);f.Control.AddRefreshHandleV2(0,f.Now,refresh,countdown,86400,a);
                    f.Control.AddRefreshHandleV2(0,f.Now,refresh,countdown,86400,b);f.Control.AddRefreshHandle(0,f.Now,v=>{},null,86400);
                    Require(OutgameTimeToRefreshControl.Groups.Count==1&&OutgameTimeToRefreshControl.Groups[0].Records.Count==1&&Row.RefreshV2Dic.Count==2,"merge by date/interval and array identity");
                    Row.CountdownV2Dic[a](0,a);Require(string.Join(",",trace)=="c,r","existing countdown combines refresh delegate per34788");
                    f.Control.AddRefreshHandle(0,f.Now,null,null,-1);Require(OutgameTimeToRefreshControl.Groups[0].Records.Count==2,"negative interval retained as distinct record");
                    Throws<ArgumentOutOfRangeException>(()=>f.Control.AddRefreshHandle(8,long.MaxValue,null,null,1));Require(OutgameTimeToRefreshControl.Groups.Count==1,"invalid new group date does not publish group");
                }
            });
            check("time-refresh-v2-order-empty-payload-and-dictionary-gated-countdown",()=>{
                using(var f=new Fixture())
                {
                    var order=new List<string>();var key=new object[]{7};long last=f.Now-86400000;
                    f.Control.AddRefreshHandle(0,last,v=>order.Add("r1"),v=>order.Add("c1"),86400);
                    f.Control.AddRefreshHandleV2(0,last,(v,args)=>{Require(ReferenceEquals(args,Array.Empty<object>()),"null registration receives empty payload");order.Add("r2");},(v,args)=>order.Add("c2"),86400,null);
                    f.Control.AddRefreshHandleV2(0,last,(v,args)=>{Require(ReferenceEquals(args,key),"exact dictionary key payload");order.Add("rk");},(v,args)=>order.Add("ck"),86400,key);
                    Row.CountdownV2Dic.Add(new object[0],(v,args)=>throw new Exception("orphan countdown should not run"));f.Control.Advance(1);
                    Require(string.Join(",",order)=="r1,c1,r2,c2,rk,ck,c1,c2,ck","due V1/V2/keyed order and second countdown pass");
                }
            });
            check("time-refresh-v1-removal-prunes-only-already-null-row",()=>{
                using(var f=new Fixture())
                {
                    Action<long> callback=v=>{};f.Control.AddRefreshHandle(0,f.Now,callback,null,86400);f.Control.RemoveRefreshHandle(0,callback,null,null);
                    Require(OutgameTimeToRefreshControl.Groups[0].Records.Count==1&&Row.Refresh==null,"newly empty record survives removal");
                    f.Control.AddRefreshHandle(0,f.Now-1,callback,null,86400);f.Control.RemoveRefreshHandle(0,callback,null,null);
                    Require(OutgameTimeToRefreshControl.Groups[0].Records.Count==1&&Row.Refresh==callback,"next remove prunes first null and returns before later row");
                    f.Control.RemoveRefreshHandle(0,callback,null,null);f.Control.RemoveRefreshHandle(0,callback,null,null);Require(OutgameTimeToRefreshControl.Groups.Count==1&&OutgameTimeToRefreshControl.Groups[0].Records.Count==0,"empty group retained");
                }
            });
            check("time-refresh-v2-removal-whole-key-ignores-v1-and-countdowns",()=>{
                using(var f=new Fixture())
                {
                    var key=new object[0];Action<long,object[]> first=(v,a)=>{},second=(v,a)=>{};
                    f.Control.AddRefreshHandleV2(0,f.Now,first,first,86400,key);f.Control.AddRefreshHandleV2(0,f.Now,second,second,86400,key);
                    f.Control.AddRefreshHandle(0,f.Now,v=>{},v=>{},86400);var held=Row;f.Control.RemoveRefreshHandleV2(0,f.Now,null,null,86400,new object[0]);Require(held.RefreshV2Dic.Count==1,"equal-content distinct array does not match");
                    f.Control.RemoveRefreshHandleV2(0,f.Now,null,null,86400,key);Require(held.Refresh!=null&&held.RefreshV2Dic.Count==0&&OutgameTimeToRefreshControl.Groups[0].Records.Count==0,"whole key removed and row pruned despite V1 callbacks");
                }
            });
            check("time-refresh-live-callback-mutation-and-exception-partial-state",()=>{
                using(var f=new Fixture())
                {
                    var remaining=new List<long>();f.Control.AddRefreshHandle(0,f.Now-86400000,v=>{Row.Interval=TimeSpan.FromDays(2);},remaining.Add,86400);f.Control.Advance(1);
                    Require(remaining.Count==2&&remaining[0]==36*3600000L,"remaining rereads fields changed by refresh");
                    Row.LastRefreshTime=OutgameItemTimestamp.ToDateTime(f.Now-3*86400000L);Row.Refresh=v=>throw new InvalidOperationException("refresh failed");remaining.Clear();
                    Throws<InvalidOperationException>(()=>f.Control.Advance(1));Require(Row.LastRefreshTime==new DateTime(2026,10,3)&&remaining.Count==0,"boundary committed before failed callback; no catch or later countdown");
                }
            });
            check("time-refresh-live-list-and-dictionary-reentry",()=>{
                using(var f=new Fixture())
                {
                    int second=0;f.Control.AddRefreshHandle(0,f.Now-86400000,v=>f.Control.AddRefreshHandle(1,f.Now-86400000,x=>second++,null,86400),null,86400);f.Control.Advance(1);Require(second==1,"new group visited within same live iteration");
                    OutgameTimeToRefreshControl.Groups.Clear();var key=new object[0];
                    f.Control.AddRefreshHandleV2(0,f.Now-86400000,(v,args)=>Row.RefreshV2Dic.Add(new object[0],(x,a)=>{}),null,86400,key);
                    Throws<InvalidOperationException>(()=>f.Control.Advance(1));Require(Row.RefreshV2Dic.Count==2,"dictionary mutation propagates enumerator failure after insertion");
                }
            });
            check("time-refresh-null-keyed-callback-and-live-clear-failures",()=>{
                using(var f=new Fixture())
                {
                    f.Control.AddRefreshHandleV2(0,f.Now-86400000,null,null,86400,new object[0]);Throws<NullReferenceException>(()=>f.Control.Advance(1));
                    OutgameTimeToRefreshControl.Groups.Clear();f.Control.AddRefreshHandle(0,f.Now-86400000,v=>OutgameTimeToRefreshControl.Clear(null),null,86400);
                    Throws<ArgumentOutOfRangeException>(()=>f.Control.Advance(1));Require(OutgameTimeToRefreshControl.Groups.Count==0,"clear applies before next live group-index read fails");
                }
            });
            return report;
        }
    }
}
