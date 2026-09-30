using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameAudioPlaybackValidation
    {
        public sealed class Context:SynchronizationContext
        {
            readonly Queue<Action> work=new Queue<Action>();public readonly List<Exception> Errors=new List<Exception>();
            public override void Post(SendOrPostCallback callback,object state)=>work.Enqueue(()=>callback(state));
            public void Pump(){int count=100;while(work.Count>0){if(--count==0)throw new Exception("Continuation recursion");try{work.Dequeue()();}catch(Exception ex){Errors.Add(ex);}}}
        }
        public sealed class Provider:IOutgameAssetProvider
        {
            public bool IsDestroyed=>false;public bool IsDone=>true;public UnityEngine.Object AssetObject{get;set;}public int Releases;public Action Releasing;
            public void ReleaseHandle(OutgameAssetHandle h){Releasing?.Invoke();Releases++;}
        }
        public sealed class Fixture:IDisposable
        {
            readonly SynchronizationContext previous;public readonly Context Context;public readonly GameObject Root=new GameObject("audio-playback-probe");
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();public readonly Dictionary<int,Action> Updates=new Dictionary<int,Action>();
            public readonly List<string> Calls=new List<string>();public readonly OutgameAudioActionServices Services;public readonly OutgameAudioSettings Settings;
            public OutgameAudioData Data=new OutgameAudioData{id=1002,Vol=.5f,VoiceType=OutgameVoiceType.Music,Atype=1,Ptype=2,ResPath="Audio/Loop/hcrzd_MainBGM.wav"};
            public AudioClip Clip;public Provider Provider;public bool NewResources;public int Started,Ended;public float Now=10;int next;public Action EndedAction;
            public Fixture(bool native=false)
            {
                previous=SynchronizationContext.Current;if(!native){Context=new Context();SynchronizationContext.SetSynchronizationContext(Context);}
                Clip=Resources.Load<AudioClip>("Recovered/Audio/1002");if(!Clip)throw new Exception("Recovered original1002 unavailable");
                var prefs=new OutgameUserPreferences(p=>null,(p,s)=>{},()=>false,s=>{},s=>{},(c,s)=>{},s=>{});prefs.OnInit(false);
                Settings=new OutgameAudioSettings(()=>prefs,()=>Messages);
                Provider=new Provider{AssetObject=Clip,Releasing=()=>Calls.Add("release")};
                Services=new OutgameAudioActionServices{
                    GetData=id=>Data,AddUpdate=a=>{int id=++next;Updates.Add(id,a);return id;},RemoveUpdate=id=>{Calls.Add("remove");Updates.Remove(id);},Messages=()=>Messages,Settings=()=>Settings,
                    UnloadClip=path=>Calls.Add("unload:"+path),UseNewResourceLoader=()=>NewResources,
                    LoadAudioClip=path=>{Calls.Add("legacy:"+path);return Task.FromResult(Clip);},LoadAudioClipForNewResource=path=>{Calls.Add("new:"+path);return Task.FromResult(Handle());},
                    Error=args=>Calls.Add("error:"+args[0]),Realtime=()=>Now,StopSdkEffect=id=>throw new Exception("Compiled source must not enable SDK branch")};
                if(!native)Services.DestroySource=s=>{Calls.Add("destroy");if(s)UnityEngine.Object.DestroyImmediate(s.gameObject);};
            }
            public OutgameAssetHandle Handle()=>new OutgameAssetHandle(Data.ResPath,Provider,()=>false,s=>Calls.Add("warning:"+s));
            public OutgameAudioLoopAction Loop()=>new OutgameAudioLoopAction(Root,Data.id,Start,End,Services);
            public OutgameAudioOnceAction Once()=>new OutgameAudioOnceAction(Root,Data.id,Start,End,Services);
            void Start(int id){Started++;Calls.Add("start");}void End(int id){Ended++;Calls.Add("end");EndedAction?.Invoke();}
            public void Tick(){foreach(var a in new List<Action>(Updates.Values))a();Context?.Pump();}
            public void Dispose(){if(Context!=null)SynchronizationContext.SetSynchronizationContext(previous);if(Root)UnityEngine.Object.DestroyImmediate(Root);}
        }
        static void Require(bool v,string message){if(!v)throw new Exception(message);}static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-audio-loop-original-main-clip-native-configuration",()=>{using(var f=new Fixture()){f.Root.layer=11;f.Settings.MusicGameVolume=.4f;var a=f.Loop();a.Play();var s=a.AudioSource;Require(s&&s.clip==f.Clip&&s.loop&&!s.playOnAwake&&s.spatialBlend==0&&Mathf.Abs(s.volume-.2f)<.0001f&&s.name=="loop_"+a.Guid,"original1002 clip and source properties");Require(s.transform.parent==f.Root.transform&&s.transform.localPosition==Vector3.zero&&s.transform.localScale==Vector3.one&&s.gameObject.layer==11&&f.Started==1,"source helper native parent/transform/layer/start");a.Stop();}});
            check("source-audio-loop-new-handle-stop-release-before-dispose",()=>{using(var f=new Fixture()){f.NewResources=true;f.Data.Atype=2;var a=f.Loop();a.Play();Require(a.AudioSource.spatialBlend==1&&a.AudioSource.dopplerLevel==0&&a.ResourceHandle!=null,"new resource cast and spatial rule");f.Calls.Clear();a.Stop();Require(string.Join(",",f.Calls)=="end,release,remove,destroy,unload:"+f.Data.ResPath&&f.Provider.Releases==1&&!a.AudioSource&&a.Data==null&&a.ResourceHandle!=null,"end before release before dispose; source handle reference retained");}});
            check("source-audio-loop-late-load-captures-configuration-and-setting",()=>{using(var f=new Fixture()){var pending=new TaskCompletionSource<AudioClip>();f.Services.LoadAudioClip=p=>pending.Task;f.Settings.MusicGameVolume=.6f;var a=f.Loop();a.Play();f.Data.Vol=.9f;f.Data.Atype=2;f.Settings.SetMusicGameVolume(OutgameVoiceType.Music,.1f);pending.SetResult(f.Clip);f.Context.Pump();Require(a.AudioSource&&Mathf.Abs(a.AudioSource.volume-.3f)<.0001f&&a.AudioSource.spatialBlend==0,"await preserves captured fields and ignores volume event before source exists");a.Stop();}});
            check("source-audio-loop-stop-during-load-still-starts-late",()=>{using(var f=new Fixture()){var pending=new TaskCompletionSource<AudioClip>();f.Services.LoadAudioClip=p=>pending.Task;var a=f.Loop();a.Play();a.Stop();pending.SetResult(f.Clip);f.Context.Pump();Require(f.Ended==1&&f.Started==1&&a.AudioSource&&a.UpdateId==0&&a.Data==null&&f.Updates.Count==0,"original has no cancellation; late source survives released action state");a.Stop();}});
            check("source-audio-loop-null-clip-keeps-unfinished-action",()=>{using(var f=new Fixture()){f.Clip=null;var a=f.Loop();a.Play();f.Tick();Require(!a.AudioSource&&f.Ended==0&&f.Started==0&&a.UpdateId!=0&&a.Data!=null&&!f.Calls.Exists(x=>x.StartsWith("error:")),"null clip returns without Stop/error or disposal; loop Update empty");a.Stop();}});
            check("source-audio-loop-release-failure-keeps-subscriptions-and-data",()=>{using(var f=new Fixture()){f.NewResources=true;var a=f.Loop();a.Play();f.Provider.Releasing=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(a.Stop);Require(f.Ended==1&&a.AudioSource&&a.Data!=null&&a.UpdateId!=0,"end committed, resource release failure skips Dispose");f.Provider.Releasing=null;a.Stop();}});
            check("source-audio-loop-end-failure-skips-resource-and-dispose",()=>{using(var f=new Fixture()){f.NewResources=true;var a=f.Loop();a.Play();f.EndedAction=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(a.Stop);Require(f.Provider.Releases==0&&a.Data!=null&&a.UpdateId!=0,"callback failure precedes all cleanup");f.EndedAction=null;a.Stop();}});
            check("source-audio-loop-destroyed-root-during-await-faults",()=>{using(var f=new Fixture()){var pending=new TaskCompletionSource<AudioClip>();f.Services.LoadAudioClip=p=>pending.Task;var a=f.Loop();a.Play();UnityEngine.Object.DestroyImmediate(f.Root);pending.SetResult(f.Clip);f.Context.Pump();Require(f.Context.Errors.Count==1&&f.Context.Errors[0] is NullReferenceException&&f.Started==0&&a.UpdateId!=0,"helper returns null; original unguarded source configuration faults");a.Stop();}});
            check("source-audio-loop-null-handle-faults-without-fake-clip",()=>{using(var f=new Fixture()){f.NewResources=true;f.Services.LoadAudioClipForNewResource=p=>Task.FromResult<OutgameAssetHandle>(null);var a=f.Loop();a.Play();f.Context.Pump();Require(f.Context.Errors.Count==1&&f.Context.Errors[0] is NullReferenceException&&a.Data!=null&&f.Ended==0,"null operation dereference propagates async-void error");a.Stop();}});
            check("source-audio-once-configuration-and-finished-update",()=>{using(var f=new Fixture()){f.Data.Ptype=1;f.Data.Atype=3;f.Data.Vol=0;var a=f.Once();a.Play();Require(a.AudioSource&&!a.AudioSource.loop&&a.AudioSource.volume==1&&a.AudioSource.spatialBlend==1&&a.AudioSource.dopplerLevel==1&&a.StopAtRealtime==-1,"once native config, only Atype2 suppresses doppler");a.AudioSource.Stop();f.Tick();Require(f.Ended==1&&a.Data==null&&!a.AudioSource&&f.Updates.Count==0,"nonplaying/end<=0 automatic stop and release");}});
            check("source-audio-once-paused-nonplaying-source-is-retained",()=>{using(var f=new Fixture()){var a=f.Once();a.Play();a.Pause(true);a.AudioSource.Stop();f.Tick();Require(a.Paused&&a.AudioSource&&f.Ended==0,"paused nonplaying source not completed");a.Pause(false);a.AudioSource.Stop();f.Tick();Require(f.Ended==1&&!a.AudioSource,"unpaused nonplaying source completes");}});
            check("source-audio-once-null-clip-deadline-strict-and-log-order",()=>{using(var f=new Fixture()){f.Clip=null;var a=f.Once();a.Play();Require(f.Calls.Contains("error:非Android平台加载Clip为空 Path=="+f.Data.ResPath)&&a.StopAtRealtime==10&&f.Started==0,"missing clip logs then records real-time deadline with zero SDK duration");f.Tick();Require(f.Ended==0,"deadline equality does not complete");f.Now=10.01f;f.Tick();Require(f.Ended==1&&a.StopAtRealtime==-1&&a.Data==null,"strict positive elapsed ends unloaded once");}});
            check("source-audio-once-missing-clip-at-time-zero-never-times-out",()=>{using(var f=new Fixture()){f.Clip=null;f.Now=0;var a=f.Once();a.Play();f.Now=100;f.Tick();Require(a.StopAtRealtime==0&&f.Ended==0&&a.UpdateId!=0,"original deadline>0 gate retains exact-zero case");a.Stop();}});
            check("source-audio-once-null-log-failure-prevents-deadline-write",()=>{using(var f=new Fixture()){f.Clip=null;f.Services.Error=args=>throw new InvalidOperationException();var a=f.Once();a.Play();f.Context.Pump();Require(f.Context.Errors.Count==1&&a.StopAtRealtime==-1&&a.UpdateId!=0&&f.Ended==0,"logging error propagates before timeout is assigned");a.Stop();}});
            check("source-audio-once-foreign-main-object-is-null-clip",()=>{using(var f=new Fixture()){f.NewResources=true;f.Provider.AssetObject=f.Root;var a=f.Once();a.Play();Require(a.ResourceHandle!=null&&a.StopAtRealtime==10&&!a.AudioSource,"as AudioClip cast gives null");f.Now=11;f.Tick();Require(f.Provider.Releases==1&&f.Ended==1,"failed cast still retains/releases original handle");}});
            return r;
        }
    }
}
