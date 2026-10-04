using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameStatisticsRecordsValidation
    {
        static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameStatisticsRecords Records(OutgameCommonMessageDispatcher dispatcher)=>new OutgameStatisticsRecords(()=>dispatcher);
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Common dispatcher4569 and source mutable statistics records35015..35022/index loop35011/DTO/key cache. Full statistics manager/controller/offline strategy, CommonGameModule registration, actual persistence/day refresh, activities/Main/Player remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("common-msg-independent-domain-and-shared-reset",()=>{
                var common=OutgameCommonMessageDispatcher.Shared;int old=0,current=0,game=0;string key="statistics-domain-probe";
                Action<object[]> gameCallback=a=>game++;OutgameMessageDispatcher.Shared.AddListener(key,gameCallback);
                try{common.AddListener(key,a=>old++);common.SendMessage(key);Require(old==1&&game==0,"common send does not hit game listeners");
                    OutgameCommonMessageDispatcher.ClearEvent();var fresh=OutgameCommonMessageDispatcher.Shared;Require(!ReferenceEquals(common,fresh),"clear replaces singleton");fresh.AddListener(key,a=>current++);fresh.SendMessage(key);common.SendMessage(key);
                    Require(old==2&&current==1&&game==0,"old captured dispatcher remains usable after replacement");OutgameMessageDispatcher.Shared.SendMessage(key);Require(game==1,"game domain untouched");}
                finally{OutgameMessageDispatcher.Shared.RemoveListener(key,gameCallback);OutgameCommonMessageDispatcher.ClearEvent();}
            });
            check("common-msg-key-appended-copy-and-original-reference-path",()=>{
                var d=new OutgameCommonMessageDispatcher();object[] seen=null;d.AddListener("20",a=>seen=a);var marker=new object();var args=new object[]{7L,marker};
                d.SendMessage("20",args);Require(ReferenceEquals(args,seen),"ordinary send retains exact array");d.SendMessage("20");Require(seen==null,"no-argument send passes null");
                d.SendMessageGetKey("20",args);Require(!ReferenceEquals(args,seen)&&seen.Length==3&&(long)seen[0]==7L&&ReferenceEquals(marker,seen[1])&&(string)seen[2]=="20"&&args.Length==2,"key path shallow copies and appends key");
                d.SendMessageGetKey("absent",null);Throws<NullReferenceException>(()=>d.SendMessageGetKey("20",null));
                d.SendMessageGetKey("20",Array.Empty<object>());Require(seen.Length==1&&(string)seen[0]=="20","empty differs from null");
            });
            check("common-msg-multicast-reentry-removal-and-failure",()=>{
                var d=new OutgameCommonMessageDispatcher();var trace=new List<string>();Action<object[]> second=a=>trace.Add("second");
                Action<object[]> first=a=>{trace.Add("first");d.RemoveListener("x",second);};d.AddListener("x",first);d.AddListener("x",second);d.SendMessage("x");d.SendMessage("x");Require(string.Join("|",trace)=="first|second|first","current multicast invocation retains removed listener");
                d.AddListener("y",null);Throws<NullReferenceException>(()=>d.SendMessage("y"));d.RemoveListener("y",null);d.SendMessage("y");
                int later=0;d.AddListener("fail",a=>throw new InvalidOperationException("explicit handler failure"));d.AddListener("fail",a=>later++);Throws<InvalidOperationException>(()=>d.SendMessage("fail"));Require(later==0,"exceptions abort multicast");
                Throws<ArgumentNullException>(()=>d.AddListener(null,first));
            });
            check("statistics-record-parent-add-set-reset-and-notification-deltas",()=>{
                var d=new OutgameCommonMessageDispatcher();var r=Records(d);var values=new List<long>();d.AddListener("10",a=>{Require(a.Length==2&&(string)a[1]=="10"&&a[0] is long,"parent payload types/key");values.Add((long)a[0]);});
                r.AddEventStatistics(10,5L);r.SetEventStatistics(10,8L);r.SetEventStatistics(10,8L);r.SetEventStatistics(10,0L);r.ResetEventStatistics(10);r.AddEventStatistics(10,0L);
                Require(string.Join(",",values)=="5,3,0,-8,0,0"&&r.GetEventStatistics(10)==0&&r.Datas.ContainsKey(10),"set/reset always notify, add-zero creates a record");
                r.SetEventStatistics(10,long.MaxValue);r.AddEventStatistics(10,1L);Require(r.GetEventStatistics(10)==long.MinValue,"signed64 addition wraps");r.ResetEventStatistics(10);Require(values[values.Count-1]==long.MinValue&&!r.Datas.ContainsKey(10),"negation wraps and removes record");
            });
            check("statistics-record-child-first-match-and-total-fallback",()=>{
                var r=Records(new OutgameCommonMessageDispatcher());r.SetEventStatistics(12,40L);Require(r.GetEventStatistics(12,999)==40,"null child list falls back to total for any itemId");
                r.AddEventStatistics(12,3,7L);Require(r.GetEventStatistics(12)==40&&r.GetEventStatistics(12,3)==7&&r.GetEventStatistics(12,999)==0,"child creation does not alter total, removes fallback");
                var row=r.GetGameStatisticsData(12);row.items.Add(new OutgameStatisticsItemData{eventId=3,count=90});r.AddEventStatistics(12,3,2L);r.SetEventStatistics(12,3,11L);
                Require(row.items[0].count==11&&row.items[1].count==90,"only first duplicate child matched");row.items.Clear();Require(r.GetEventStatistics(12,3)==0&&r.GetEventStatistics(12)==40,"empty list does not use total fallback");
            });
            check("statistics-record-zero-child-id-has-source-asymmetry",()=>{
                var d=new OutgameCommonMessageDispatcher();var r=Records(d);var trace=new List<string>();d.AddListener("31",a=>trace.Add(string.Join(":",a)));
                r.SetEventStatistics(31,0,8L);Require(r.GetGameStatisticsData(31).items==null&&r.GetEventStatistics(31)==8&&r.GetEventStatistics(31,0)==8,"new record itemId0 writes total");
                r.SetEventStatistics(31,0,3L);Require(r.GetEventStatistics(31)==8&&r.GetEventStatistics(31,0)==3,"existing record itemId0 creates child0, retains total");r.SetEventStatistics(31,0,0L);
                Require(r.GetGameStatisticsData(31).items.Count==1&&r.GetEventStatistics(31)==8&&r.GetEventStatistics(31,0)==0&&string.Join("|",trace)=="8:0:31|3:0:31|-3:0:31","child-zero values remain stored with delta notifications");
                r.SetEventStatistics(31,0L);Require(r.GetGameStatisticsData(31)==null,"parent-zero deletes all indexed children");
            });
            check("statistics-record-listener-reentry-and-failure-after-mutation",()=>{
                var d=new OutgameCommonMessageDispatcher();var r=Records(d);var seen=new List<long>();bool nested=false;
                d.AddListener("5",a=>{seen.Add(r.GetEventStatistics(5));if(!nested){nested=true;r.SetEventStatistics(5,9L);}});r.AddEventStatistics(5,2L);Require(string.Join(",",seen)=="2,9"&&r.GetEventStatistics(5)==9,"listener observes committed count and nested mutation survives");
                d.AddListener("6",a=>throw new InvalidOperationException("explicit broadcast failure"));Throws<InvalidOperationException>(()=>r.AddEventStatistics(6,4,8L));Require(r.GetEventStatistics(6,4)==8,"child mutation is not rolled back");
                Throws<InvalidOperationException>(()=>r.ResetEventStatistics(6));Require(!r.Datas.ContainsKey(6),"deletion precedes failing broadcast");
            });
            check("statistics-record-index-overwrite-retains-old-keys-and-aliases",()=>{
                var r=Records(new OutgameCommonMessageDispatcher());r.SetEventStatistics(99,99L);
                var first=new OutgameGameStatisticsData(1,2);var last=new OutgameGameStatisticsData(1,3);var json=new OutgameStatisticsJsonData();json.datas.Add(first);json.datas.Add(last);r.IndexRecords(json);
                Require(ReferenceEquals(last,r.GetGameStatisticsData(1))&&r.GetEventStatistics(99)==99,"index assignment uses last duplicate without clearing old dictionary");last.count=7;Require(r.GetEventStatistics(1)==7,"index retains serialized row reference");
                json.datas=new List<OutgameGameStatisticsData>{new OutgameGameStatisticsData(2,8),null};Throws<NullReferenceException>(()=>r.IndexRecords(json));Require(r.GetEventStatistics(2)==8&&r.GetEventStatistics(1)==7,"index failure preserves earlier additions and old records");
            });
            check("statistics-record-null-dictionary-overload-differences",()=>{
                var r=Records(new OutgameCommonMessageDispatcher());r.Datas=null;Require(r.GetGameStatisticsData(1)==null&&r.GetEventStatistics(1,2)==0,"reads tolerate missing dictionary");r.SetEventStatistics(1,0L);r.ResetEventStatistics(1);Require(r.Datas==null,"zero parent/reset do not initialize dictionary");
                Throws<NullReferenceException>(()=>r.SetEventStatistics(1,2,8L));Require(r.Datas==null,"child set does not repair null dictionary");r.AddEventStatistics(1,2,8L);Require(r.Datas!=null&&r.GetEventStatistics(1,2)==8,"child add repairs null dictionary");
                r.Datas=null;r.SetEventStatistics(2,-7L);Require(r.GetEventStatistics(2)==-7,"nonzero parent set repairs dictionary");
            });
            check("statistics-record-original-json-and-culture-cached-key",()=>{
                var data=new OutgameStatisticsJsonData{firstStartTimeStamp=long.MinValue,lastRefreshTimeStamp=long.MaxValue};data.datas.Add(new OutgameGameStatisticsData(41,9){items=new List<OutgameStatisticsItemData>{new OutgameStatisticsItemData{eventId=-4,count=long.MaxValue}}});
                string text=JsonUtility.ToJson(data);var read=JsonUtility.FromJson<OutgameStatisticsJsonData>(text);
                Require(text.Contains("\"StatisticsEvent\":41")&&read.firstStartTimeStamp==long.MinValue&&read.lastRefreshTimeStamp==long.MaxValue&&read.datas[0].items[0].count==long.MaxValue,"original field names and signed64 JSON values");
                var old=CultureInfo.CurrentCulture;try{var culture=(CultureInfo)CultureInfo.InvariantCulture.Clone();culture.NumberFormat.NegativeSign="minus";CultureInfo.CurrentCulture=culture;string first=OutgameStatisticsMessageKey.Get(-1928394);CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;string second=OutgameStatisticsMessageKey.Get(-1928394);Require(first=="minus1928394"&&ReferenceEquals(first,second),"first formatting is cached across later culture changes");}
                finally{CultureInfo.CurrentCulture=old;}
            });
            return report;
        }
    }
}
