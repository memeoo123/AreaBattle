using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameRankScoreValidation
    {
        [Serializable] sealed class OracleCase {public float input;public int integer,decimals;}
        [Serializable] sealed class Oracle {public OracleCase[] cases;}
        sealed class LowRandom:System.Random {public override int Next(int min,int max)=>min;}
        public sealed class Fixture
        {
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameDataManagerPool Pool;public readonly OutgameSdkStringStorage Storage;
            public readonly OutgameRankScoreServices Services;public readonly OutgameRankScoreRuntime Runtime;
            public readonly OutgameAccountRewardsValidation.AccountHost Host;
            public OutgameRankControl Current;public readonly OutgameRankManager Manager;
            public string PathName;public long Now=123456789;
            public Fixture(string path=null,bool initialize=true)
            {
                PathName=path??Path.Combine(Path.GetTempPath(),"AreaBattleRankScore-"+Guid.NewGuid().ToString("N"));
                Storage=new OutgameSdkStringStorage(new OutgameFileStorageBackend(PathName,a=>a()),s=>{throw new Exception(s);});
                Host=new OutgameAccountRewardsValidation.AccountHost(new OutgameStatisticsOffNetValidation.Host());
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>{throw new Exception(s);});Pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>());
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                ReadConfig(Config);var symbols=new OutgameBigNumberSymbols();symbols.Set(OutgameBigNumberSymbols.FromConfig(Config.dicLargeNum));
                Services=new OutgameRankScoreServices{Pool=()=>Pool,Config=()=>Config,Now=()=>Now,Current=()=>Current,Symbols=symbols,Warning=a=>{},UnityRange=(a,b)=>a,Random=new GameRandomSource(new LowRandom())};
                Current=new OutgameRankControl(Services);
                Runtime=new OutgameRankScoreRuntime(Services,Host,Storage,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}),Host.Downloads.Add);
                var reg=Runtime.Registration;Manager=(OutgameRankManager)reg.Create();Manager.ParticipatesInSync=reg.AutoSyn;Manager.CompressData=reg.CompressData;
                if(initialize)Manager.OnInit();Pool.Managers[4137]=Manager;
            }
            public string Stored=>Storage.GetString(Host.MineGameName+"RankManager","");
        }
        public static void ReadConfig(OutgameLegacyConfigManager config)
        {
            var reader=new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);});
            if(config.dicLargeNum.Count==0)reader.ReadTable(config.dicLargeNum);if(config.dicRankSub.Count==0)reader.ReadTable(config.dicRankSub);config.newRankSettingConfig=reader.ReadValue<NewRankSettingConfig>();
        }
        public static OutgameRankControl AttachTopInfo(OutgameTopInfoPageValidation.Fixture f)
        {
            ReadConfig(f.Config);var symbols=new OutgameBigNumberSymbols();symbols.Set(OutgameBigNumberSymbols.FromConfig(f.Config.dicLargeNum));OutgameRankControl current=null;
            var services=new OutgameRankScoreServices{Pool=()=>f.Account.Statistics.Pool,Config=()=>f.Config,Now=()=>123456789,Current=()=>current,Symbols=symbols,Warning=a=>f.Trace.Add("rank-warning"),UnityRange=(a,b)=>a,Random=new GameRandomSource(new LowRandom())};current=new OutgameRankControl(services);
            var runtime=new OutgameRankScoreRuntime(services,f.Account.Host,f.Account.Services.Storage,f.Account.Services.Versions,f.Account.Host.Downloads.Add);var reg=runtime.Registration;
            var manager=reg.Create();manager.ParticipatesInSync=reg.AutoSyn;manager.CompressData=reg.CompressData;manager.OnInit();f.Account.Statistics.Pool.Managers[reg.SourceTypeIndex]=manager;runtime.BindTopInfo(f.Services);return current;
        }
        static void Require(bool b,string why){if(!b)throw new Exception(why);}
        static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original RankManager/RankData, RankControl score/add/reduce/offline-rank portion, ConfigHelper rank-step/magnitude and source lossy score serialization with actual TopInfo/account storage. Full RankControl home/AI-list lifecycle, RanklistTransmitter/server/Main/platform are still pending; roster not incremented."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="rank-score-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="rank-score-"+id,result="fail",detail=e.ToString()});}};
            check("original-registration-config-and-fresh-record",()=>{var f=new Fixture();Require(f.Runtime.Registration.SourceTypeIndex==4137&&f.Runtime.Registration.GameName=="Proj_hdzd"&&f.Manager.ParticipatesInSync&&!f.Manager.CompressData&&f.Manager.PlayerRank==20000&&f.Manager.LastTime==f.Now&&f.Manager.Data.curPlayerScore==""&&f.Current.CurPlayerScore==0,"source config and fresh empty score");});
            check("parse-defaults-malformed-score-and-corrupt-json",()=>{var f=new Fixture();f.Current.CurPlayerScore=999;f.Manager.UpdateDataCallBack("{}");Require(f.Manager.PlayerRank==0&&f.Manager.LastTime==0&&f.Current.CurPlayerScore==0,"nonempty empty object is not repaired");f.Manager.UpdateDataCallBack("{\"playerCurRank\":12,\"curPlayerScore\":\"not-number\"}");Require(f.Current.CurPlayerScore==0&&f.Manager.Data.curPlayerScore=="not-number","failed TryParse sets zero without changing string");var old=f.Manager.Data;Throws<LitJson.JsonException>(()=>f.Manager.UpdateDataCallBack("invalid"));Require(f.Manager.Data==old,"JSON failure retains old record");f.Current.CurPlayerScore=15;f.Manager.UpdateDataCallBack(null);Require(f.Manager.PlayerRank==20000&&f.Current.CurPlayerScore==0,"null input creates defaults, unlike MatchManager");});
            check("bigint-and-current-owner-reload",()=>{var f=new Fixture();var old=f.Current;var newer=new OutgameRankControl(f.Services);f.Services.Current=()=>{f.Current=newer;return newer;};string digits="123456789012345678901234567890";f.Manager.UpdateDataCallBack("{\"curPlayerScore\":\""+digits+"\"}");Require(newer.CurPlayerScore==BigInteger.Parse(digits)&&old.CurPlayerScore==0,"real BigInteger and current owner target");});
            check("create-new-record-before-config-clock-and-reentry",()=>{var f=new Fixture();var old=f.Manager.Data;f.Services.Config=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(f.Manager.CreateNewData);Require(f.Manager.Data!=old&&f.Manager.Data.playerCurRank==0,"new record published before config failure");f.Services.Config=()=>f.Config;var replacement=new OutgameRankData{playerCurRank=91};f.Services.Now=()=>{f.Manager.Data=replacement;return 44;};f.Manager.CreateNewData();Require(f.Manager.Data==replacement&&replacement.lastTime==44&&replacement.curPlayerScore==null,"timestamp assignment re-resolves data after getter");});
            check("rank-upper-only-clamp-and-timestamp-target",()=>{var f=new Fixture();f.Manager.SetRank(30000);Require(f.Manager.PlayerRank==22000,"upper cap");f.Manager.SetRank(-7);Require(f.Manager.PlayerRank==-7,"no lower repair");var captured=f.Manager.Data;f.Services.Now=()=>{f.Manager.Data=new OutgameRankData();return 88;};f.Manager.SetRank(123);Require(captured.playerCurRank==123&&captured.lastTime==88&&f.Manager.Data.playerCurRank==0,"SetRank captures data before clock callback");});
            check("magnitude-first-match-unknown-last-and-last-character",()=>{var f=new Fixture();f.Config.dicLargeNum.Clear();f.Config.dicLargeNum.Add(3,new LargeNumConfig{mag=3,magName="K"});f.Config.dicLargeNum.Add(6,new LargeNumConfig{mag=6,magName="M"});Require(f.Services.Magnitude("K")==3&&f.Services.Magnitude("unknown")==6&&f.Services.Magnitude("")==-1,"unknown suffix returns last visited value");Require(f.Manager.GetLargeNumZero("7",out bool n)=="00"&&!n&&f.Manager.GetLargeNumZero("K",out bool k)=="00000"&&k,"numeric digit vs magnitude");Require(f.Manager.GetLargeNumStr("1.23K")=="123000"&&f.Manager.GetLargeNumStr("1.23aa")=="000000000","only final character removed; failed float prefix becomes zero with last magnitude padding");});
            check("float-decimal-count-and-source-excess-division",()=>{var f=new Fixture();Require(OutgameRankManager.FloatToInt(1.25f,out int n)==125&&n==2,"float repeated multiplication");var oracle=JsonUtility.FromJson<Oracle>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/rank-float-oracle.json")));foreach(var item in oracle.cases)Require(OutgameRankManager.FloatToInt(item.input,out int places)==item.integer&&places==item.decimals,"unaltered original WASM arithmetic oracle "+item.input);Require(f.Manager.GetLargeNumStr("1.28K")=="639999","source f32 repeated multiplication reaches12799999 at7 decimals then divides by20");Require(f.Manager.GetLargeNumStr("1.2345")=="617"&&f.Manager.GetLargeNumStr("-1.25")=="-125"&&f.Manager.GetLargeNumStr("")=="","source divides by excess-times-ten, retains negative and empty");});
            check("lossy-save-does-not-change-live-score-and-file-restart",()=>{var f=new Fixture();f.Current.CurPlayerScore=123456;f.Manager.SetRank(50);f.Pool.SaveData();Require(f.Current.CurPlayerScore==123456&&f.Manager.Data.curPlayerScore=="123000","display shortening is expanded for storage, live value retained");var g=new Fixture(f.PathName);Require(g.Current.CurPlayerScore==123000&&g.Manager.PlayerRank==50&&g.Manager.LastTime==f.Now,"independent actual backend restores stored value");g.Current.CurPlayerScore=150;g.Manager.OnSave();Require(g.Manager.Data.curPlayerScore=="100","original final-fraction-zero display rule also affects storage");});
            check("save-mutation-precedes-storage-login-and-disabled-gates",()=>{var f=new Fixture();f.Current.CurPlayerScore=123456;f.Host.Progress=9;f.Manager.OnSave();Require(f.Stored==""&&f.Manager.Data.curPlayerScore=="123000","record formatting before storage login gate");f.Host.Progress=10;f.Manager.OnSave();Require(f.Stored=="","source digest recorded before denied write");f.Current.CurPlayerScore=125456;f.Pool.SetSaveDisabled(true);f.Pool.SaveData();Require(f.Manager.Data.curPlayerScore=="123000","pool gate stops manager itself");f.Pool.SetSaveDisabled(false);f.Pool.SaveData();Require(f.Stored!=""&&f.Manager.Data.curPlayerScore=="125000","changed score crosses both gates");});
            check("server-download-remains-pending-and-release-retains",()=>{var f=new Fixture(initialize:false);f.Host.Server=true;f.Manager.OnInit();Require(f.Host.Downloads.Count==1&&f.Manager.Data==null,"no synthetic server completion");f.Manager.UpdateData(false);var d=f.Manager.Data;f.Manager.OnRelease();Require(f.Manager.Data==d,"empty release");});
            check("config-rank-step-boundaries-contiguous-keys-and-default",()=>{var f=new Fixture();f.Config.dicRankSub.Clear();Require(f.Services.RandomRankDown(5)==1,"empty table fallback1..2");f.Config.dicRankSub.Add(1,new RankSubConfig{id=1,areaValue=10,randValue=new[]{3,4}});f.Config.dicRankSub.Add(2,new RankSubConfig{id=2,areaValue=20,randValue=new[]{7,9}});Require(f.Services.RandomRankDown(10)==3&&f.Services.RandomRankDown(11)==7&&f.Services.RandomRankDown(99)==7,"first inclusive area and last fallback");f.Config.dicRankSub.Remove(1);Throws<KeyNotFoundException>(()=>f.Services.RandomRankDown(5));});
            check("add-score-and-rank-update-save-is-explicit",()=>{var f=new Fixture();f.Manager.SetRank(1);f.Current.CurPlayerScore=10;int rank=f.Current.AddScore();Require(rank==1&&f.Current.CurPlayerScore==15010&&f.Stored=="","Unity150..399 score range, min rank1, no save");f.Services.UnityRange=(a,b)=>{Require(a==150&&b==400,"source add range");return a;};f.Services.Config=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(()=>f.Current.AddScore());Require(f.Current.CurPlayerScore==30010,"score increment precedes rank helper failure");});
            check("reduce-reresolves-current-owner-after-comparison",()=>{var f=new Fixture();var first=f.Current;first.CurPlayerScore=15000;var second=new OutgameRankControl(f.Services){CurPlayerScore=2000};int lookups=0;f.Services.Current=()=>++lookups==1?first:second;first.ReduceScore();Require(lookups==2&&first.CurPlayerScore==15000&&second.CurPlayerScore==-8000,"comparison owner and subtraction owner differ under reentry; no extra clamp");f.Services.Current=()=>second;second.ReduceScore();Require(second.CurPlayerScore==0,"reduction >= current clears to zero");});
            check("offline-rank-gates-hours-and-current-manager",()=>{var f=new Fixture();f.Manager.Data.playerCurRank=1000;f.Manager.Data.lastTime=0;f.Now=3*3600000;f.Current.InitPlayerRank();Require(f.Manager.PlayerRank==1000,"rank1000 bypass");f.Manager.Data.playerCurRank=999;f.Now=7199999;f.Current.InitPlayerRank();Require(f.Manager.PlayerRank==999,"less than2 whole hours bypass");f.Now=7200000;f.Current.InitPlayerRank();Require(f.Manager.PlayerRank==1001&&f.Manager.LastTime==f.Now,"2 hours times Unity1..9 then timestamp update");});
            check("manager-getter-rereads-pool-without-cache",()=>{var f=new Fixture();var old=f.Current.Manager;f.Pool.Managers.Remove(4137);Require(f.Current.Manager==null,"missing source lookup does not retain old manager");f.Pool.Managers[4137]=old;Require(f.Current.Manager==old,"new lookup uses live pool");});
            check("current-culture-float-parse-preserved",()=>{var f=new Fixture();var before=CultureInfo.CurrentCulture;try{CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("fr-FR");Require(f.Manager.GetLargeNumStr("1.23")=="000","dot rejected in French parse, source float becomes zero");}finally{CultureInfo.CurrentCulture=before;}});
            check("top-info-real-rank-binding-and-account-restart",()=>{string path;using(var f=new OutgameTopInfoPageValidation.Fixture()){var score=AttachTopInfo(f);score.CurPlayerScore=123456;f.Start();Require(f.Page.ScoreText.text=="1.23K","actual live rank score display");f.Account.Statistics.Pool.SaveData();path=f.Account.PathName;}using(var f=new OutgameTopInfoPageValidation.Fixture(path:path)){var score=AttachTopInfo(f);f.Start();Require(score.CurPlayerScore==123000&&f.Page.ScoreText.text=="1.23K","fresh actual RankManager and page display");}});
            return r;
        }
    }
}
