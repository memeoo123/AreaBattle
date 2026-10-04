using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameRankListSupportValidation
    {
        sealed class Draws:System.Random
        {
            public readonly List<int> Bounds=new List<int>();public Queue<int> Values=new Queue<int>();
            public override int Next(int min,int max){Bounds.Add(max);return Values.Count>0?Values.Dequeue():min;}
        }
        sealed class Weight {public int W;}
        public sealed class ThrowingReference:IOutgameReference
        {public int Calls;public bool Fail;public void Clear(){Calls++;if(Fail)throw new InvalidOperationException();}}
        static void Need(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source RankItemData/reference pool, weighted selection and country helper prerequisites. Full RankControl home/lifecycle is not wired; app country transport is an explicit port, no platform success claimed."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="rank-list-support-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="rank-list-support-"+id,result="fail",detail=e.ToString()});}};
            check("row-defaults-clear-and-fifo-reuse",()=>{
                var p=new OutgameReferencePool();var a=p.Acquire<OutgameRankItemData>();var b=p.Acquire<OutgameRankItemData>();Need(a.name==null&&a.countryN==null&&a.score==null,"new row null strings");a.rankIndex=44;a.name="name";a.countryN="country";a.score="100K";a.headBoxId=15;p.Release(a);p.Release(b);Need(a.rankIndex==0&&a.name==""&&a.countryN==""&&a.score==""&&a.headBoxId==0,"all original fields cleared");Need(p.Acquire<OutgameRankItemData>()==a&&p.Acquire<OutgameRankItemData>()==b,"FIFO identity preserved");});
            check("default-double-release-and-strict-clear-order",()=>{
                var p=new OutgameReferencePool();var a=p.Acquire<ThrowingReference>();p.Release(a);p.Release(a);Need(a.Calls==2&&p.Acquire<ThrowingReference>()==a&&p.Acquire<ThrowingReference>()==a,"original default permits duplicate queue references");p.StrictChecks=true;p.Release(a);Throws<Exception>(()=>p.Release(a));Need(a.Calls==4&&p.Acquire<ThrowingReference>()==a&&p.Acquire<ThrowingReference>()!=a,"strict duplicate check follows Clear");});
            check("release-failure-does-not-enqueue",()=>{
                var p=new OutgameReferencePool();var a=p.Acquire<ThrowingReference>();a.Fail=true;Throws<InvalidOperationException>(()=>p.Release(a));Need(p.Acquire<ThrowingReference>()!=a&&a.Calls==1,"clear failure preserves queue");Throws<Exception>(()=>p.Release(null));a.Fail=false;p.Release(a);Need(p.Acquire<ThrowingReference>()==a,"retry follows actual queue");});
            check("pool-isolation-by-concrete-type",()=>{
                var p=new OutgameReferencePool();var row=p.Acquire<OutgameRankItemData>();var other=p.Acquire<ThrowingReference>();p.Release(other);p.Release(row);Need(p.Acquire<OutgameRankItemData>()==row&&p.Acquire<ThrowingReference>()==other,"separate concrete queues");Need(new OutgameReferencePool().Acquire<OutgameRankItemData>()!=row,"separate app ownership in fixtures");});
            check("weighted-invalid-and-all-alias-consume-no-random",()=>{
                var d=new Draws();var random=new GameRandomSource(d);var rows=new List<Weight>{new Weight{W=2},new Weight{W=5}};Func<Weight,int> w=v=>v.W;
                Need(OutgameRankWeightedRandom.SelectMany<Weight>(null,1,w,random)==null&&OutgameRankWeightedRandom.SelectMany(rows,0,w,random)==null&&OutgameRankWeightedRandom.SelectMany(rows,3,w,random)==null,"invalid counts");var all=OutgameRankWeightedRandom.SelectMany(rows,2,w,random);Need(all==rows&&d.Bounds.Count==0,"all aliases input without sampling");all.Reverse();Need(rows[0].W==5,"caller sorting can mutate original list");});
            check("weighted-priority-with-zero-and-negative-weights",()=>{
                var d=new Draws{Values=new Queue<int>(new[]{0,7,4})};var random=new GameRandomSource(d);var a=new Weight{W=4};var b=new Weight{W=0};var c=new Weight{W=1};var rows=new List<Weight>{a,b,c};var got=OutgameRankWeightedRandom.SelectMany(rows,2,v=>v.W,random);Need(got[0]==b&&got[1]==c&&rows[0]==a&&d.Bounds.Count==3&&d.Bounds.TrueForAll(n=>n==8),"weight+1 sum and descending sampled priorities");
                d.Values=new Queue<int>();rows=new List<Weight>{new Weight{W=-1},new Weight{W=0}};Need(OutgameRankWeightedRandom.SelectMany(rows,1,v=>v.W,random)[0]==rows[1],"negative weights are not repaired");});
            check("single-weight-boundaries-and-empty-failure",()=>{
                var d=new Draws{Values=new Queue<int>(new[]{2})};var random=new GameRandomSource(d);var rows=new List<Weight>{new Weight{W=2},new Weight{W=3}};Need(OutgameRankWeightedRandom.SelectOne(rows,v=>v.W,random,s=>{})==rows[1]&&d.Bounds[0]==5,"strict roll less than cumulative boundary");int logs=0;Need(OutgameRankWeightedRandom.SelectOne<Weight>(null,v=>v.W,random,s=>{logs++;Need(s=="权重随机集合不能为空","original error");})==null&&logs==1,"null reports then returns");Throws<ArgumentOutOfRangeException>(()=>OutgameRankWeightedRandom.SelectOne(new List<Weight>(),v=>v.W,random,s=>{}));});
            check("country-pending-repeat-and-synchronous-callback",()=>{
                var f=new OutgameRankScoreValidation.Fixture();f.Config.dicCountryConfig.Add(1,new CountryConfigConfig{id=1,countryCode=new[]{"CN","cn"}});var callbacks=new List<Action<string>>();var country=new OutgamePlayerCountry(()=>f.Config,callbacks.Add,(color,args)=>Need(color==Color.green&&args.Length==1,"source log color/shape"));Need(country.GetPlayerCountry()==-1&&country.Requested,"pending returns unknown");callbacks[0]("CN");Need(country.PlayerCountryID==1,"actual callback updates country");Need(country.GetPlayerCountry()==-1&&callbacks.Count==2,"every call resets and requests again");var sync=new OutgamePlayerCountry(()=>f.Config,a=>a("cn"),(color,args)=>{});Need(sync.GetPlayerCountry()==1,"synchronous platform completion is observable");});
            check("country-first-match-case-and-late-callback-state",()=>{
                var f=new OutgameRankScoreValidation.Fixture();f.Config.dicCountryConfig.Add(1,new CountryConfigConfig{id=7,countryCode=new[]{"CN"}});f.Config.dicCountryConfig.Add(2,new CountryConfigConfig{id=8,countryCode=new[]{"CN","US"}});var callbacks=new List<Action<string>>();var country=new OutgamePlayerCountry(()=>f.Config,callbacks.Add,(color,args)=>{});country.GetPlayerCountry();callbacks[0]("cn");Need(country.PlayerCountryID==-1,"case sensitive unknown");callbacks[0]("CN");Need(country.PlayerCountryID==7,"first insertion order match uses row ID");country.GetPlayerCountry();callbacks[1]("US");Need(country.PlayerCountryID==8,"second country matches");callbacks[0]("CN");Need(country.PlayerCountryID==7,"late callback can overwrite; no generation filter");callbacks[1]("US");Need(country.PlayerCountryID==7,"existing ID stops unmatched first row before later match");});
            check("country-request-log-and-row-failure-prefix",()=>{
                var f=new OutgameRankScoreValidation.Fixture();var bad=new OutgamePlayerCountry(()=>f.Config,a=>throw new InvalidOperationException(),(c,a)=>{});Throws<InvalidOperationException>(()=>bad.GetPlayerCountry());Need(bad.Requested&&bad.PlayerCountryID==-1,"flag assigned before transport throw");Action<string> callback=null;var country=new OutgamePlayerCountry(()=>f.Config,a=>callback=a,(c,a)=>throw new InvalidOperationException());country.GetPlayerCountry();Throws<InvalidOperationException>(()=>callback("CN"));Need(country.PlayerCountryID==-1,"log failure precedes config access");});
            return r;
        }
    }
}
