using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameStatisticsOffNetValidation
    {
        public static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public sealed class Host:IOutgameDataStorageHost
        {
            public bool Server;public int SourceLoginProgress=>10;public bool LoginProcedureFlag8=>false;public bool LoginStaticFlag4=>false;
            public bool IsUseServer=>Server;public string MineGameName=>"statistics-offnet-test";public bool HasToast=>false;
            public void Log(string s){}public void Error(string s){}public void Toast(string s){}
            public string Compress(string k,string s)=>throw new Exception("unexpected base storage compression");
            public string Decompress(string k,string s)=>throw new Exception("unexpected base storage decompression");
            public void QueueUpload(string k,string s){} // Explicit isolated upload endpoint; no success response.
        }
        public sealed class Fixture:IDisposable
        {
            public readonly string PathName;public readonly Host StorageHost=new Host();
            public readonly List<string> Trace=new List<string>(),Warnings=new List<string>(),Errors=new List<string>();
            public readonly OutgameDataManagerPool Pool;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();
            public OutgameCommonMessageDispatcher Common=new OutgameCommonMessageDispatcher();
            public readonly OutgameStatisticsControl Owner=new OutgameStatisticsControl();
            public readonly OutgameStatisticsExpansion Expansion;
            public readonly OutgameUpdateManager Updates;
            public readonly OutgameStatisticsOffNetServices Services;
            public readonly OutgameStatisticsManager Manager;
            public OutgameStatisticsOffNetStrategy Strategy;
            public readonly OutgameSdkStringStorage Strings;
            public Action Disposables;public Action<long> Refresh;public Action Writing,OnRegisterRefresh;
            public long ServerTime=1700000000000L,RegisteredLast;public DateTime LocalNow=new DateTime(2026,10,3,12,0,0);
            public float Realtime=10,Delta;public bool Release=true,HasHttp;
            public int Downloads,Requests,Registrations,Removals,Completions;
            public Fixture(string path=null,bool native=false)
            {
                PathName=path??System.IO.Path.Combine(System.IO.Path.GetTempPath(),"AreaBattleOffNet-"+Guid.NewGuid().ToString("N"));
                var backend=new OutgameFileStorageBackend(PathName,a=>{Writing?.Invoke();a();});
                Strings=new OutgameSdkStringStorage(backend,s=>Errors.Add(s));
                Pool=new OutgameDataManagerPool(()=>{},s=>{},s=>{},s=>Errors.Add(s));Pool.OnInit(false,"Proj_hdzd",Array.Empty<OutgameManagerRegistration>());
                Expansion=new OutgameStatisticsExpansion(()=>Owner,a=>Errors.Add((string)a[0]));
                Updates=new GameObject("offnet-source-update-manager").AddComponent<OutgameUpdateManager>();Updates.enabled=native;
                Services=new OutgameStatisticsOffNetServices{Messages=()=>Messages,ServerTime=()=>ServerTime,LocalNow=()=>LocalNow,
                    HasHttpHelper=()=>HasHttp,RequestServerTime=()=>{Requests++;Trace.Add("http-request");},IsReleaseVersion=()=>Release,
                    RealtimeSinceStartup=()=>native?Time.realtimeSinceStartup:Realtime,DeltaTime=()=>native?Time.deltaTime:Delta,Updates=()=>Updates,
                    AddRefreshHandle=(type,last,callback,otherCallback,seconds)=>{Require(type==0&&otherCallback==null&&seconds==86400,"source daily refresh registration arguments");Registrations++;RegisteredLast=last;Refresh=callback;Trace.Add("daily-register");OnRegisterRefresh?.Invoke();},
                    RemoveRefreshHandle=(type,callback,otherCallback,args)=>{Require(type==0&&otherCallback==null&&ReferenceEquals(args,Array.Empty<object>()),"source daily remove arguments");Removals++;if(Refresh==callback)Refresh=null;},
                    Warning=s=>Warnings.Add(s),Error=a=>Errors.Add((string)a[0])};
                var storage=new OutgameDataManagerStorage(()=>"CommonGameModuleGameStatisticsManager",StorageHost,Strings,new OutgameDataVersionState(()=>{},()=>{},()=>{},s=>{},s=>{}));
                Manager=new OutgameStatisticsManager(storage,StorageHost,s=>{Downloads++;Trace.Add("download");},
                    ()=>Strategy=new OutgameStatisticsOffNetStrategy(()=>Pool,()=>Common,()=>Owner,Expansion,Services),()=>Messages,()=>Common,()=>Owner,Expansion,a=>Trace.Add((string)a[0]));
            }
            public void Init(bool server=false)
            {
                StorageHost.Server=server;
                Owner.OnInit(new OutgameStatisticsControlServices{Pool=()=>Pool,CreateManager=()=>Manager,
                    AddUpdate=Updates.Register,RemoveUpdate=Updates.QueueRemove,GetDisposableActions=()=>Disposables,SetDisposableActions=a=>Disposables=a,
                    ClearCommonMessages=()=>Common=new OutgameCommonMessageDispatcher()},()=>{Completions++;Trace.Add("complete");});
            }
            public void Dispose()
            {
                if(Manager.Strategy!=null){Owner.Dispose();Manager.OnRelease();}
                UnityEngine.Object.DestroyImmediate(Updates.gameObject);
            }
            public string Stored=>Strings.GetString(StorageHost.MineGameName+Manager.DataKey,"");
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,
                limitations="Concrete offnet strategy, codec, real storage/manager/control path. Deterministic clock/http/daily scheduler endpoints are explicit fixtures. Native coroutine/update verification is separate; original TimeToRefreshControl and platform/Main composition are not claimed complete."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("statistics-offnet-codec-utf8-external-gzip-and-original-fallback",()=>{
                var warnings=new List<string>();string text="统计\n"+new string('x',4097);Require(OutgameStatisticsCodec.DecompressString(OutgameStatisticsCodec.CompressString(text),warnings.Add)==text,"UTF8 and multi-buffer payload");
                string known="H4sIAAAAAAAC/2WLsQ7CMAxE/8VzhpA0dZKdgRXYEEPUuiISgSo2LBX/TgoDA3fLk+7eAlOuLAdJVY65UIMyQ9wY27kefdCNFFwTy56mSnz5P4VPFIxJEkM8LdB2ySx54O2TbgIRUcFwf6wYjLEWjba9dx2i87ptWah8VVqF3QjR/ozXufUNmWrrGKkAAAA=";
                var data=JsonUtility.FromJson<OutgameStatisticsJsonData>(OutgameStatisticsCodec.DecompressString(known,warnings.Add));Require(data.datas[0].count==long.MaxValue&&data.datas[0].items[0].count==9,"independent Python gzip fixture accepted");
                string raw="{\"datas\":[]}";Require(ReferenceEquals(raw,OutgameStatisticsCodec.DecompressString(raw,warnings.Add))&&warnings.Count==1,"invalid gzip returns original input for old plain JSON");
                Require(OutgameStatisticsCodec.CompressString(null)==""&&OutgameStatisticsCodec.DecompressString(null,warnings.Add)=="","empty string guards");
                Throws<ArgumentNullException>(()=>OutgameStatisticsCodec.Decompress(null));
            });
            check("statistics-offnet-real-compressed-file-save-and-independent-restart",()=>{
                string path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"AreaBattleOffNetRestart-"+Guid.NewGuid().ToString("N"));long first;
                using(var f=new Fixture(path))
                {
                    f.Init();first=f.Strategy.Data.firstStartTimeStamp;Require(first==f.ServerTime&&f.Completions==1&&f.Owner.ValueProviders.Count==19,"actual strategy creates and publishes data");
                    f.Expansion.AddEventCount(77,3,long.MaxValue);f.Manager.OnSave();Require(!f.Owner.IsDirty&&f.Stored.Length>0&&!f.Stored.StartsWith("{"),"source compression reaches real file backend");
                    Require(ReferenceEquals(f.Manager.Data,f.Strategy.Data)&&ReferenceEquals(f.Manager.Data.datas[f.Manager.Data.datas.Count-1],f.Manager.GetGameStatisticsData(77)),"save list shares live records");
                }
                using(var next=new Fixture(path)){next.Init();Require(next.Strategy.Data.firstStartTimeStamp==first&&next.Manager.GetEventStatistics(77,3)==long.MaxValue&&next.Completions==1,"new owner graph decompresses and restores signed64 child count");}
            });
            check("statistics-offnet-save-pruning-partial-failure-and-dirty-cleared-before-io",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Manager.Datas.Clear();var zero=new OutgameGameStatisticsData(1,0){items=new List<OutgameStatisticsItemData>()};var child=new OutgameGameStatisticsData(2,0){items=new List<OutgameStatisticsItemData>{new OutgameStatisticsItemData{eventId=3,count=0}}};
                    f.Manager.Datas.Add(1,zero);f.Manager.Datas.Add(2,child);f.Owner.IsDirty=true;f.Manager.OnSave();Require(zero.items==null&&f.Manager.Datas.Count==2&&f.Manager.Data.datas.Count==1&&ReferenceEquals(child,f.Manager.Data.datas[0]),"empty list normalized; zero row omitted from JSON but not dictionary; zero child retained");
                    f.Owner.IsDirty=true;f.Manager.Datas.Add(3,null);Throws<NullReferenceException>(f.Manager.OnSave);Require(!f.Owner.IsDirty&&f.Manager.Data.datas.Count==1,"null-row failure retains prefix and cleared dirty");f.Manager.Datas.Remove(3);
                    f.Owner.IsDirty=true;f.Writing=()=>{Require(!f.Owner.IsDirty,"dirty already cleared at write");throw new InvalidOperationException("write failure");};child.count=7;Throws<InvalidOperationException>(f.Manager.OnSave);Require(!f.Owner.IsDirty,"source does not restore dirty on failed storage");
                }
            });
            check("statistics-offnet-deferred-server-load-raw-json-and-corrupt-reload",()=>{
                using(var f=new Fixture())
                {
                    f.Init(true);Require(f.Downloads==1&&f.Completions==0&&f.Manager.Data==null,"server data remains genuinely pending");
                    f.Manager.UpdateDataCallBack("{\"firstStartTimeStamp\":12,\"lastRefreshTimeStamp\":34,\"datas\":[{\"StatisticsEvent\":7,\"count\":8}]}");Require(f.Completions==1&&f.Manager.GetEventStatistics(7)==8&&f.Warnings.Count==1,"source plain JSON fallback loads after warning");
                    var held=f.Strategy.Data;Throws<ArgumentException>(()=>f.Manager.UpdateDataCallBack("not-json"));Require(ReferenceEquals(held,f.Strategy.Data)&&f.Completions==1,"both parse attempts fail without replacing existing data");
                }
            });
            check("statistics-offnet-clock-negative-zero-reentry-overflow-and-mode",()=>{
                using(var f=new Fixture())
                {
                    f.Init(true);f.ServerTime=-7;f.Realtime=10.125f;f.Strategy.RefreshClock();f.Realtime=11.25f;Require(f.Strategy.NowTimestamp()==1118,"negative server time is accepted by clock refresh");
                    f.ServerTime=0;f.Strategy.RefreshClock();Require(f.Strategy.ClockAnchor==OutgameItemTimestamp.FromDateTime(f.LocalNow),"only exact zero clock anchor falls back locally");
                    f.HasHttp=true;f.Strategy.RefreshClock();Require(f.Requests==1,"request only with existing HTTP helper");
                    f.Strategy.ReceiveServerTime(new object[]{long.MaxValue});f.Realtime+=1;Require(f.Strategy.NowTimestamp()==unchecked(long.MaxValue+1000L),"signed64 anchor addition wraps");
                    f.Realtime=float.NaN;f.Strategy.ReceiveServerTime(new object[]{9L});Require(f.Strategy.RealtimeAnchor==long.MinValue,"source NaN conversion sentinel");
                    f.Release=false;Require(f.Strategy.Value10000(null)==OutgameItemTimestamp.FromDateTime(f.LocalNow),"non-release query uses DateTime.Now provider");Throws<InvalidCastException>(()=>f.Strategy.ReceiveServerTime(new object[]{1}));
                }
            });
            check("statistics-offnet-frame-periods-single-pass-and-calendar-broadcast",()=>{
                using(var f=new Fixture())
                {
                    f.Init();int reads=0;f.Services.DeltaTime=()=>{reads++;return 61;};f.Owner.IsDirty=false;f.Strategy.UpdateDate(null);
                    Require(reads==3&&f.Strategy.SecondElapsed==60&&f.Strategy.OnlineElapsed==31&&f.Strategy.DirtyElapsed==31&&f.Manager.GetEventStatistics(10800)==30,"each timer reads delta independently and catches up at most once");
                    f.Services.DeltaTime=()=>0;f.Strategy.SecondElapsed=1;f.Strategy.OnlineElapsed=0;f.Strategy.DirtyElapsed=30;f.Owner.IsDirty=false;f.Strategy.UpdateDate(null);Require(!f.Owner.IsDirty&&f.Strategy.DirtyElapsed==30,"dirty threshold is strict greater-than30");
                    var seen=new List<string>();foreach(int id in new[]{10500,10501,10502}){int key=id;f.Owner.ValueProviders[id]=a=>-1;f.Common.AddListener(id.ToString(),a=>{Require(a.Length==1&&(string)a[0]==key.ToString(),"calendar payload is appended key only");seen.Add((string)a[0]);});}
                    f.Strategy.SecondElapsed=1;f.Strategy.UpdateDate(null);Require(string.Join(",",seen)=="10500,10501,10502","calendar query differences broadcast in source order");
                }
            });
            check("statistics-offnet-item-ad-product-source-events-and-unboxing",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Messages.SendMessage("GF_ShowInterst");f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{false});f.Messages.SendMessage("GF_AdsPlayCallBack",new object[]{true});
                    Require(f.Manager.GetEventStatistics(10001)==1&&f.Manager.GetEventStatistics(10004)==1&&f.Manager.GetEventStatistics(10002)==1&&f.Manager.GetEventStatistics(10005)==1,"observed actual result flags drive counts");
                    f.Messages.SendMessage("Item_ItemChange",new object[]{9,-4L});f.Messages.SendMessage("Item_ItemChange",new object[]{9,0L});Require(f.Expansion.GameValue(10007,9)==4&&f.Manager.GetGameStatisticsData(10008)!=null,"negative spending and zero acquisition follow different records");
                    Throws<InvalidCastException>(()=>f.Messages.SendMessage("Item_ItemChange",new object[]{9,1}));
                    f.Messages.SendMessage("ItemUI_ProductReward",new object[]{9,2,3});Require(f.Manager.GetEventStatistics(10017,3)==2&&f.Manager.GetEventStatistics(10018,9)==2&&f.Manager.GetEventStatistics(10019,9)==0,"three-argument product reward increments two records and resets original product");
                    f.Expansion.SetEventCount(10019,8,7);f.Messages.SendMessage("ItemUI_ProductReset",new object[]{0,8,99});Require(f.Manager.GetEventStatistics(10019,8)==7,"unsupported reset arity ignored");f.Messages.SendMessage("ItemUI_ProductReset",new object[]{0,8});Require(f.Manager.GetEventStatistics(10019,8)==0,"two-argument reset uses second id");
                    int errors=f.Errors.Count;Require(f.Strategy.Value10007(null)==0&&f.Errors.Count==errors+1,"source corrupt diagnostic literal preserved");Throws<IndexOutOfRangeException>(()=>f.Strategy.Value10008(Array.Empty<object>()));
                }
            });
            check("statistics-offnet-daily-reset-order-and-readiness-iterator",()=>{
                using(var f=new Fixture())
                {
                    f.Init();f.Pool.SetSaveDisabled(true);var wait=f.Strategy.WaitForRefresh();Require(wait.MoveNext()&&wait.Current is WaitUntil&&((WaitUntil)wait.Current).keepWaiting,"native WaitUntil predicate reads real pool readiness");
                    f.Pool.SetSaveDisabled(false);Require(!((WaitUntil)wait.Current).keepWaiting&&!wait.MoveNext()&&f.Registrations==1&&f.Manager.GetEventStatistics(10902)==1,"register after wait then increment launch count");
                    var order=new List<int>();foreach(int id in new[]{10901,10004,10005,10009,10010,10016,10801,10900}){int key=id;f.Common.AddListener(id.ToString(),a=>order.Add(key));}
                    f.Strategy.Data.firstStartTimeStamp=f.ServerTime-2*86400000L;f.Refresh(123);Require(f.Strategy.Data.lastRefreshTimeStamp==123&&f.Manager.GetEventStatistics(10900)==2&&string.Join(",",order)=="10901,10004,10005,10009,10010,10016,10801,10900","daily state commits before ordered notifications and lifetime-day set");
                }
            });
            check("statistics-offnet-fresh-negative-clock-and-disposal-listener-removal",()=>{
                using(var f=new Fixture())
                {
                    f.ServerTime=-1;f.Init();Require(f.Strategy.Data.firstStartTimeStamp==OutgameItemTimestamp.FromDateTime(f.LocalNow)&&f.Strategy.ClockAnchor==-1&&f.Manager.GetGameStatisticsData(10000)==null,"fresh first timestamp requires positive server time, refresh anchor only excludes zero");
                    var data=f.Strategy.Data;f.Manager.OnRelease();f.Messages.SendMessage("GF_ShowInterst");Require(f.Manager.Datas.Count==0&&ReferenceEquals(data,f.Strategy.Data)&&f.Removals==1,"release unhooks result listeners but strategy retains data and clock fields");
                }
            });
            return report;
        }
    }
}
