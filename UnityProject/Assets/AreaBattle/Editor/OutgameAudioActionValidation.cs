using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameAudioActionValidation
    {
        sealed class Context:SynchronizationContext
        {
            readonly Queue<Action> pending=new Queue<Action>();public readonly List<Exception> Errors=new List<Exception>();
            public override void Post(SendOrPostCallback callback,object state)=>pending.Enqueue(()=>callback(state));
            public void Pump(){int guard=100;while(pending.Count>0){if(--guard==0)throw new Exception("Continuation recursion");try{pending.Dequeue()();}catch(Exception ex){Errors.Add(ex);}}}
        }
        sealed class Probe:OutgameAudioAction
        {
            public static Action<Probe> Awakening;public Func<OutgameAudioData,Task<AudioSource>> Loading;public Action Disposing,Stopping;public int Updates,Pauses,Plays;
            public Probe(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
            public void SetSource(AudioSource source)=>AudioSource=source;public void SetRoot(GameObject root)=>Root=root;
            protected override void OnAwake()=>Awakening?.Invoke(this);protected override Task<AudioSource> OnPlay(OutgameAudioData data){Plays++;return Loading(data);}
            protected override void OnUpdate()=>Updates++;protected override void OnPause(bool paused)=>Pauses++;protected override void OnStop()=>Stopping?.Invoke();protected override void OnDispose()=>Disposing?.Invoke();
        }
        sealed class Fixture:IDisposable
        {
            readonly SynchronizationContext previous=SynchronizationContext.Current;public readonly Context Context=new Context();public readonly GameObject Root=new GameObject("audio-action-probe");
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();public readonly List<string> Calls=new List<string>();public readonly Dictionary<int,Action> Updates=new Dictionary<int,Action>();
            public OutgameAudioData Data=new OutgameAudioData{id=1001,Vol=.4f,VoiceType=OutgameVoiceType.Music,Ptype=4,ResPath="Audio/source.wav"};public readonly OutgameAudioActionServices Services;public readonly OutgameAudioSettings Settings;public Action Removed,Unloading;public int Started,Ended;
            public Fixture(){SynchronizationContext.SetSynchronizationContext(Context);var prefs=new OutgameUserPreferences(p=>null,(p,s)=>{},()=>false,s=>{},s=>{},(c,s)=>{},s=>{});prefs.OnInit(false);Settings=new OutgameAudioSettings(()=>prefs,()=>Messages);Services=new OutgameAudioActionServices{GetData=id=>{Calls.Add("data:"+id);return Data;},AddUpdate=a=>{Calls.Add("add-update");Updates.Add(7,a);return 7;},RemoveUpdate=id=>{Calls.Add("remove:"+id);Removed?.Invoke();Updates.Remove(id);},Messages=()=>{Calls.Add("messages");return Messages;},Settings=()=>Settings,UnloadClip=path=>{Calls.Add("unload:"+path);Unloading?.Invoke();}};Probe.Awakening=a=>Calls.Add("awake");}
            public Probe Create(){var a=new Probe(Root,1001,g=>{Started++;Calls.Add("start:"+g);},g=>Ended++,Services);a.Loading=d=>Task.FromResult(Source());a.Disposing=()=>Calls.Add("dispose");return a;}
            public AudioSource Source(){var go=new GameObject("native-source",typeof(AudioSource));go.transform.SetParent(Root.transform,false);return go.GetComponent<AudioSource>();}
            public void Dispose(){Probe.Awakening=null;SynchronizationContext.SetSynchronizationContext(previous);UnityEngine.Object.DestroyImmediate(Root);}
        }
        static void Require(bool b,string s){if(!b)throw new Exception(s);}static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("source-audio-action-construction-order-guid-and-update",()=>{using(var f=new Fixture()){var a=f.Create();Require(string.Join(",",f.Calls)=="awake,add-update,messages,messages"&&a.Guid==a.GetHashCode()&&a.UpdateId==7&&a.Volume==1,"OnAwake then update then remove/add volume listener; source hashcode identity");f.Updates[7]();Require(a.Updates==1,"registered wrapper dispatches update");a.OnEndCall();Require(f.Ended==1,"explicit end callback");}});
            check("source-audio-action-sync-play-data-volume-and-start",()=>{using(var f=new Fixture()){var a=f.Create();a.Play();Require(a.AudioSource&&a.Plays==1&&f.Started==1&&a.Data==f.Data&&a.Volume==.4f&&a.VoiceType==OutgameVoiceType.Music,"config -> virtual OnPlay -> assigned native source -> start");f.Context.Pump();Require(f.Context.Errors.Count==0,"no async failure");}});
            check("source-audio-action-missing-data-retains-update-subscription",()=>{using(var f=new Fixture()){f.Data=null;var a=f.Create();a.Play();Require(a.Plays==0&&a.UpdateId==7&&f.Started==0&&f.Updates.Count==1,"source early return does not dispose or signal end");}});
            check("source-audio-action-captures-root-before-config-getter",()=>{using(var f=new Fixture()){var a=f.Create();a.SetRoot(null);f.Services.GetData=id=>{a.SetRoot(f.Root);return f.Data;};a.Play();Require(a.Data==f.Data&&a.Root&&a.Plays==0,"captured null root still rejects after getter replaces live root");}});
            check("source-audio-action-volume-fallback-and-nan",()=>{using(var f=new Fixture()){var a=f.Create();f.Data.Vol=0;a.Play();Require(a.Volume==1,"nonpositive configured volume maps to1");f.Data.Vol=float.NaN;a.Play();Require(float.IsNaN(a.Volume)&&f.Started==2,"NaN passes source <=0 conditional unchanged, repeated Play allowed");}});
            check("source-audio-action-dispose-during-load-does-not-cancel-late-start",()=>{using(var f=new Fixture()){var a=f.Create();var pending=new TaskCompletionSource<AudioSource>();a.Loading=d=>pending.Task;a.Play();Require(a.Data==f.Data&&f.Started==0,"await suspended after config state writes");a.Dispose();Require(a.UpdateId==0&&a.Data==null&&f.Updates.Count==0,"release before delayed source");pending.SetResult(f.Source());f.Context.Pump();Require(a.AudioSource&&f.Started==1&&a.Data==null&&a.UpdateId==0,"late await still assigns source and invokes start without re-registering update or config");}});
            check("source-audio-action-async-fault-propagation-keeps-partial-state",()=>{using(var f=new Fixture()){var a=f.Create();a.Loading=d=>Task.FromException<AudioSource>(new InvalidOperationException("load probe"));a.Play();f.Context.Pump();Require(f.Context.Errors.Count==1&&f.Context.Errors[0] is InvalidOperationException&&a.Data==f.Data&&a.UpdateId==7&&f.Started==0,"async-void failure reaches synchronization context without invented dispose or end");}});
            check("source-audio-action-null-result-no-start-no-cleanup",()=>{using(var f=new Fixture()){var a=f.Create();a.Loading=d=>Task.FromResult<AudioSource>(null);a.Play();Require(!a.AudioSource&&f.Started==0&&a.Data==f.Data&&a.UpdateId==7,"null result preserves source state");}});
            check("source-audio-action-volume-message-type-and-native-multiplication",()=>{using(var f=new Fixture()){var a=f.Create();a.Play();a.AudioSource.volume=1;f.Settings.SetMusicGameVolume(OutgameVoiceType.Sound,.2f);Require(a.AudioSource.volume==1,"voice mismatch no write");f.Settings.SetMusicGameVolume(OutgameVoiceType.Music,.5f);Require(Mathf.Abs(a.AudioSource.volume-.2f)<.00001f,"matching event multiplies configured source volume");f.Messages.SendMessage("GlobalVolumeChange",new object[]{OutgameVoiceType.Music});Throws<InvalidCastException>(()=>f.Messages.SendMessage("GlobalVolumeChange",new object[]{"Music",.5f}));}});
            check("source-audio-action-pause-native-source-gate-and-unconditional-stop",()=>{using(var f=new Fixture()){var a=f.Create();a.Pause(true);Require(a.Pauses==0,"unloaded source skips virtual pause");a.SetSource(f.Source());a.Pause(true);a.Pause(false);Require(a.Pauses==2,"native Pause/UnPause then hook");UnityEngine.Object.DestroyImmediate(a.AudioSource);a.Pause(true);int stopped=0;a.Stopping=()=>stopped++;a.Stop();Require(a.Pauses==2&&stopped==1,"Unity destroyed-null gate, Stop has no source gate");}});
            check("source-audio-action-dispose-update-remove-failure-retains-rest",()=>{using(var f=new Fixture()){var a=f.Create();a.Play();f.Removed=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(a.Dispose);Require(a.UpdateId==7&&a.Data==f.Data&&f.Updates.Count==1&&!f.Calls.Contains("dispose"),"failed remove prevents id reset and remaining cleanup");}});
            check("source-audio-action-dispose-hook-failure-retains-clip-data",()=>{using(var f=new Fixture()){var a=f.Create();a.Play();a.Disposing=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(a.Dispose);Require(a.UpdateId==0&&a.Data==f.Data&&!f.Calls.Contains("unload:Audio/source.wav"),"update and listener removal commit before virtual failure skips unload");a.AudioSource.volume=1;f.Settings.SetMusicGameVolume(OutgameVoiceType.Music,0);Require(a.AudioSource.volume==1,"listener already removed");}});
            check("source-audio-action-unload-failure-retry-and-repeat-dispose",()=>{using(var f=new Fixture()){var a=f.Create();a.Play();f.Unloading=()=>throw new InvalidOperationException();Throws<InvalidOperationException>(a.Dispose);Require(a.Data==f.Data&&a.UpdateId==0,"clear data only after successful unload");f.Unloading=null;a.Dispose();a.Dispose();Require(a.Data==null&&f.Calls.FindAll(x=>x=="dispose").Count==3&&f.Calls.FindAll(x=>x=="remove:7").Count==1,"virtual dispose repeated, update removed only once");}});
            return r;
        }
    }
}
