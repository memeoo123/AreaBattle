using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameAudioData
    {public int id,Atype,Ptype,StartTime,EndTime;public OutgameVoiceType VoiceType;public float Vol;public string ResPath;}
    // Original UpdateManager/resource/config/static-service boundaries are explicit; no successful fallback.
    public sealed class OutgameAudioActionServices
    {
        public Func<int,OutgameAudioData> GetData;public Func<Action,int> AddUpdate;public Action<int> RemoveUpdate;
        public Func<OutgameMessageDispatcher> Messages;public Func<OutgameAudioSettings> Settings;public Action<string> UnloadClip;
        public Func<bool> UseNewResourceLoader;
        public Func<string,Task<OutgameAssetHandle>> LoadAudioClipForNewResource;
        public Func<string,Task<AudioClip>> LoadAudioClip;
        public Action<object[]> Error;public Action<int> StopSdkEffect;
        public Func<int,Action<int>,Action<int,int>,int> SetCountDownByMillisecond;public Action<int> RemoveTime;
        public Func<float> Realtime=()=>Time.realtimeSinceStartup;
        public Action<AudioSource> DestroySource=OutgameAudioSourceObjects.Destroy;
    }
    public abstract class OutgameAudioAction:IOutgameAudioAction
    {
        protected readonly OutgameAudioActionServices Services;
        public int UpdateId{get;private set;}public int AudioId{get;protected set;}public int Guid{get;protected set;}
        public float Volume{get;protected set;}=1;public OutgameVoiceType VoiceType{get;protected set;}
        public GameObject Root{get;protected set;}public OutgameAudioData Data{get;protected set;}public AudioSource AudioSource{get;protected set;}
        public Action<int> StartCall,EndCall;
        protected OutgameAudioAction(GameObject root,int audioId,Action<int> started,Action<int> ended,OutgameAudioActionServices services)
        {
            Services=services;Root=root;AudioId=audioId;int guid=GetHashCode();StartCall=started;Guid=guid;EndCall=ended;
            OnAwake();UpdateId=Services.AddUpdate(OnUpdate);RemoveVolumeListener();Services.Messages().AddListener("GlobalVolumeChange",OnGlobalVolumeChange);
        }
        void RemoveVolumeListener()=>Services.Messages().RemoveListener("GlobalVolumeChange",OnGlobalVolumeChange);
        void OnGlobalVolumeChange(object[] args){if(args!=null&&args.Length>=2)RefreshAudioVolume((OutgameVoiceType)args[0],(float)args[1]);}
        public void OnStartCall()=>StartCall?.Invoke(Guid);public void OnEndCall()=>EndCall?.Invoke(Guid);
        public float GetSettingVolume(){switch(VoiceType){case OutgameVoiceType.Music:return Services.Settings().MusicGameVolume;case OutgameVoiceType.Sound:return Services.Settings().SoundGameVolume;default:return 1;}}
        // Source26271/nested26284: captures root before data lookup; no cancel/dispose generation guard.
        public async void Play()
        {
            var root=Root;Data=Services.GetData(AudioId);if(Data==null||!root)return;
            Volume=Data.Vol<=0?1:Data.Vol;VoiceType=Data.VoiceType;AudioSource=await OnPlay(Data);if(AudioSource)OnStartCall();
        }
        public void Pause(bool paused){if(AudioSource){if(paused)AudioSource.Pause();else AudioSource.UnPause();OnPause(paused);}}
        public void Stop()=>OnStop();
        public void Dispose()
        {
            if(UpdateId!=0){Services.RemoveUpdate(UpdateId);UpdateId=0;}
            RemoveVolumeListener();OnDispose();if(Data!=null){Services.UnloadClip(Data.ResPath);Data=null;}
        }
        public virtual void RefreshAudioVolume(OutgameVoiceType voice,float value){if(VoiceType==voice&&AudioSource)AudioSource.volume=value*Volume;}
        protected abstract void OnAwake();protected abstract Task<AudioSource> OnPlay(OutgameAudioData data);
        protected abstract void OnPause(bool paused);protected abstract void OnStop();protected abstract void OnUpdate();protected abstract void OnDispose();
    }
}
