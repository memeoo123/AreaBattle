using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameRankControlValidation
    {
        public sealed class LowRandom:System.Random {public override int Next(int min,int max)=>min;}
        public sealed class FailRandom:System.Random {public override int Next(int min,int max)=>throw new InvalidOperationException();}
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameTopInfoPageValidation.Fixture Top=new OutgameTopInfoPageValidation.Fixture();
            public readonly OutgameRankControl Rank;public readonly OutgameRankScoreServices Score;public readonly OutgameRankHomeServices Home;
            public readonly List<Action<string>> Requests=new List<Action<string>>();
            public Fixture(bool initialize=true){Rank=AttachTopInfo(Top,out Score,out Home,Requests.Add);if(initialize)Rank.OnInit();}
            public void Dispose()=>Top.Dispose();
        }
        public static OutgameRankControl AttachTopInfo(OutgameTopInfoPageValidation.Fixture f,out OutgameRankScoreServices services,out OutgameRankHomeServices home,Action<Action<string>> request=null)
        {
            OutgameRankScoreValidation.ReadConfig(f.Config);var symbols=new OutgameBigNumberSymbols();symbols.Set(OutgameBigNumberSymbols.FromConfig(f.Config.dicLargeNum));
            services=new OutgameRankScoreServices{Pool=()=>f.Account.Statistics.Pool,Config=()=>f.Config,Now=()=>123456789,Symbols=symbols,Warning=a=>f.Trace.Add("rank-warning"),UnityRange=(a,b)=>a,Random=new GameRandomSource(new LowRandom())};
            home=new OutgameRankHomeServices{References=new OutgameReferencePool(),Log=(c,a)=>f.Trace.Add(string.Join(",",a)),Error=f.Trace.Add,
                Country=new OutgamePlayerCountry(()=>f.Config,request??(a=>f.Trace.Add("country-request-pending")),(c,a)=>f.Trace.Add(string.Join(",",a))),
                AiNames=new OutgameAiNames(()=>f.Config,a=>{throw new Exception("source AI name range");},(a,b)=>a,services.Random)};
            var registry=f.Account.Page.Data.Registry;OutgameCoreControllerBindings.BindRank(registry,services,home);var rank=(OutgameRankControl)registry.Resolve(4134);
            var runtime=new OutgameRankScoreRuntime(services,f.Account.Host,f.Account.Services.Storage,f.Account.Services.Versions,f.Account.Host.Downloads.Add);var reg=runtime.Registration;
            var manager=reg.Create();manager.ParticipatesInSync=reg.AutoSyn;manager.CompressData=reg.CompressData;manager.OnInit();f.Account.Statistics.Pool.Managers[reg.SourceTypeIndex]=manager;runtime.BindTopInfo(f.Services);return rank;
        }
        static void Need(bool b,string why){if(!b)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static List<string> Names(int n)=>Enumerable.Range(0,n).Select(i=>"country_test;ai"+i).ToList();
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Full source RankControl lifecycle, home/AI list generation and registry with real RankManager/UserInfo/config/reference pool. Actual RankUI/OverUI rendering, country platform request, RanklistTransmitter/Main/full business/final Player remain pending."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="rank-control-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="rank-control-"+id,result="fail",detail=e.ToString()});}};
            check("constructor-and-original-config-lifecycle",()=>{using(var f=new Fixture(false)){
                var c=f.Rank;Need(c.HeadWeights.Count==0&&c.defaultScoreRandList==null&&c.listRankItemData.Data.Count==0&&c.dic_userui.Count==0&&c.RememberedAIScore==0,"source ctor buffers only");c.OnInit();
                Need(c.HomeRankShowNum==100&&c.upPlayerScoreNum==6&&c.lowPlayerScoreNum==3&&c.rankGapMin==1&&c.rankGapMax==99&&c.minReGap==1&&c.maxReGap==5,"original config fields");Need(c.playerItemScaleT==.3f&&c.playerItemLargeScaleT==.4f&&c.playerItemSmallScaleT==.2f&&c.playerItemLargeScaleN==1.5f&&c.playerItemScaleV==1.1f,"source scale divisors");ColorUtility.TryParseHtmlString("#FCFFB6",out var color);Need(c.playerInfoColor==color&&c.HeadWeights.Count==f.Top.Config.dicHeadBox.Count&&c.defaultScoreRandList.Count==99,"source colors/weights");}});
            check("home-100-original-names-player-and-rank-span",()=>{using(var f=new Fixture()){
                var c=f.Rank;var rows=c.listRankItemData.Data;Need(rows.Count==100&&c.playerListIndex==50&&rows[0].rankIndex==19950&&rows[99].rankIndex==20049,"source50 above/49 below");Need(rows[50].name==f.Top.User.Name&&rows[50].countryN==""&&rows[50].score=="0"&&c.dic_userui.Count==0,"player row has empty country, dictionary cleared after rows");Need(rows.All(x=>f.Top.Config.dicHeadBox.ContainsKey(x.headBoxId))&&f.Requests.Count==1,"real head frame selection and pending country");}});
            check("home-refresh-fifo-identity-and-rank-one-alias-sort",()=>{using(var f=new Fixture()){
                var c=f.Rank;var before=c.listRankItemData.Data.ToArray();c.InitHomeInfo();Need(c.listRankItemData.Data.SequenceEqual(before)&&f.Requests.Count==2,"all100 references reused FIFO");c.Manager.SetRank(1);c.InitHomeInfo();Need(c.playerListIndex==0&&c.listRankItemData.Data.Count==100&&c.listRankItemData.Data[99].rankIndex==100&&c.defaultScoreRandList[0].Value==100,"rank1 uses all99 default weights with input alias");}});
            check("default-weights-actual-offset-and-stable-sort",()=>{using(var f=new Fixture(false)){
                var c=f.Rank;c.rankGapMin=3;c.rankGapMax=5;c.InitDefaultRankList();Need(c.defaultScoreRandList.Select(x=>x.Weight).SequenceEqual(new[]{1,0,-1})&&c.CalWeight(5)==-1,"weight uses actual i, not offset");var a=new OutgameRankRandomObject{Value=4};var b=new OutgameRankRandomObject{Value=4};var z=new OutgameRankRandomObject{Value=1};var list=new List<OutgameRankRandomObject>{a,z,b};c.SortScoreWeights(list,false);Need(list.SequenceEqual(new[]{z,a,b}),"ascending ties stable");c.SortScoreWeights(list,true);Need(list.SequenceEqual(new[]{a,b,z}),"descending ties stable");Need(OutgameRankControl.CompareAboveGap(2,2)==1&&OutgameRankControl.CompareAboveGap(int.MaxValue,int.MinValue)==-1,"source comparator has no subtraction and equality is1");}});
            check("init-repeated-appends-heads-but-refreshes-score-list",()=>{using(var f=new Fixture()){
                var c=f.Rank;int n=c.HeadWeights.Count;var defaults=c.defaultScoreRandList;c.OnInit();Need(c.HeadWeights.Count==n*2&&c.defaultScoreRandList==defaults&&defaults.Count==99&&c.listRankItemData.Data.Count==100,"source init appends frames without clearing and reuses defaults");}});
            check("init-partial-config-failure-prefix",()=>{using(var f=new Fixture(false)){
                var c=f.Rank;f.Top.Config.newRankSettingConfig.HM_RankGapRag=null;Throws<NullReferenceException>(c.OnInit);Need(c.upPlayerScoreNum==6&&c.lowPlayerScoreNum==3&&c.HomeRankShowNum==100&&c.HeadWeights.Count==0&&c.defaultScoreRandList==null,"earlier config fields and colors retained");}});
            check("home-failed-frame-selection-retains-generated-dictionary",()=>{using(var f=new Fixture()){
                var c=f.Rank;var old=c.listRankItemData.Data.ToArray();c.HeadWeights.Clear();Throws<ArgumentOutOfRangeException>(c.InitHomeInfo);Need(c.listRankItemData.Data.Count==0&&c.dic_userui.Count==100&&old[0].rankIndex==19950&&old[0].headBoxId==0&&old[1].name=="","old rows released; first acquired row partially written before frame failure");}});
            check("country-cache-pending-missing-config-and-disposed-slot",()=>{using(var f=new Fixture()){
                var c=f.Rank;Need(c.GetCurPlayerCountryInfo()=="country_com"&&f.Requests.Count==2,"unknown re-requests then generic flag");var row=f.Top.Config.dicCountryConfig.Values.First();f.Home.Country.PlayerCountryID=row.id;Need(c.GetCurPlayerCountryInfo()==row.countryFlagName&&f.Requests.Count==2,"cached lookup without transport");f.Home.Country.PlayerCountryID=int.MaxValue;Throws<KeyNotFoundException>(()=>c.GetCurPlayerCountryInfo());c.OnDispose();Need(c.GetCurPlayerCountryInfo()=="","cleared slot exits before config lookup");}});
            check("player-row-source-pool-and-no-frame-overwrite",()=>{using(var f=new Fixture()){
                var row=f.Rank.GetPlayerRankData(17,123456);Need(row.rankIndex==17&&row.countryN=="country_com"&&row.name==f.Top.User.Name&&row.score=="1.23K"&&row.headBoxId==0,"real player data composition");f.Home.References.Release(row);row.headBoxId=77;Need(f.Rank.GetPlayerRankData(18,123456)==row&&row.headBoxId==77,"source does not overwrite pooled frame field");}});
            check("settlement-gaps-sort-remembered-and-name-consumption",()=>{using(var f=new Fixture()){
                var c=f.Rank;c.lastRank=100;var names=Names(2);var got=c.RandomRankAIScore(true,names,2,100,1000,0,true);Need(got.Keys.SequenceEqual(new[]{98,99})&&got[98].Split(';')[2]==f.Score.Format(1800)&&got[99].Split(';')[2]==f.Score.Format(1400)&&c.RememberedAIScore==1800&&names.Count==0,"same-rank min increment and doubled running gaps, descending");var rows=c.GetOverUIRankAIData(2,false,100,50,0);Need(rows.Count==2&&rows[0].rankIndex==101&&rows[0].score=="0"&&rows[1].rankIndex==102,"below score floor and actual acquired rows");}});
            check("settlement-bound-reset-and-equal-gap-comparator",()=>{using(var f=new Fixture()){
                var c=f.Rank;c.lastRank=100;var got=c.RandomRankAIScore(true,Names(3),3,100,500,100,true);Need(got.Values.All(x=>x.Split(';')[2]==f.Score.Format(500))&&c.RememberedAIScore==500,"too-large bound candidate resets small running to zero; repeated equal gaps retained");c.RememberedAIScore=0;c.minReGap=5;c.maxReGap=5;c.lastRank=0;got=c.RandomRankAIScore(false,Names(2),2,100,2000,100,true);Need(got.Values.All(x=>x.Split(';')[2]==f.Score.Format(1500)),"large running rejected by bound resets into inclusive re-gap range");}});
            check("above-negative-preserved-below-zero-and-invalid-count",()=>{using(var f=new Fixture()){
                var c=f.Rank;c.defaultScoreRandList=new List<OutgameRankRandomObject>{new OutgameRankRandomObject{Weight=1,Value=100}};c.lastRank=20;c.RandomRankAIScore(true,Names(1),1,20,-500,0,false);Need(c.RememberedAIScore==-400,"above branch does not clamp negative");c.RememberedAIScore=0;c.RandomRankAIScore(false,Names(1),1,20,-500,0,false);Need(c.RememberedAIScore==0,"below branch clamps zero");var names=Names(3);Need(c.RandomRankAIScore(false,names,3,20,1000,0,false).Count==0&&names.Count==3,"count beyond default list yields empty output without consuming names");}});
            check("random-metadata-failure-after-remember-before-name-removal",()=>{using(var f=new Fixture()){
                var c=f.Rank;c.defaultScoreRandList=new List<OutgameRankRandomObject>{new OutgameRankRandomObject{Weight=1,Value=100}};c.lastRank=20;f.Score.Random=new GameRandomSource(new FailRandom());var names=Names(1);Throws<InvalidOperationException>(()=>c.RandomRankAIScore(true,names,1,20,1000,0,false));Need(names.Count==1&&c.RememberedAIScore==1100,"remembered first score written before metadata random throws, name retained");}});
            check("empty-name-error-then-original-index-failure",()=>{using(var f=new Fixture()){
                var c=f.Rank;c.defaultScoreRandList=new List<OutgameRankRandomObject>{new OutgameRankRandomObject{Weight=1,Value=100}};Throws<ArgumentOutOfRangeException>(()=>c.RandomRankAIScore(false,new List<string>(),1,20,1000,0,false));Need(f.Top.Trace.Any(x=>x=="随机的ai个数错误：0"),"source log precedes index exception");}});
            check("dispose-retains-data-clears-current-slot-unconditionally",()=>{using(var f=new Fixture()){
                var c=f.Rank;var rows=c.listRankItemData.Data;var defaults=c.defaultScoreRandList;c.Updata(99,99);c.OnDispose();Need(!f.Home.Registry.HasInstance(4134)&&c.listRankItemData.Data==rows&&rows.Count==100&&c.defaultScoreRandList==defaults,"no update work or disposal clearing lists");var next=f.Score.Current();Need(next!=c&&f.Home.Registry.HasInstance(4134),"actual lazy resolver creates replacement");c.OnDispose();Need(!f.Home.Registry.HasInstance(4134),"old owner can clear newer slot");}});
            check("home-current-owner-and-country-callback-slot-clear",()=>{using(var f=new Fixture()){
                var c=f.Rank;var old=c.listRankItemData.Data.ToArray();f.Home.Country=new OutgamePlayerCountry(()=>f.Top.Config,a=>f.Home.Registry.Clear(4134),(color,args)=>{});Throws<NullReferenceException>(c.InitHomeInfo);Need(c.dic_userui==null&&c.listRankItemData.Data.Count==0&&old.All(x=>x.name==""),"country callback cleared slot, generated null then old rows released before null dictionary traversal");}});
            return r;
        }
    }
}
