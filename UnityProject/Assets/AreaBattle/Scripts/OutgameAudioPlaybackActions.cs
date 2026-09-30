using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Source helper26371/26372 and Utils.NewGameObject: a child GameObject owns the AudioSource.
    public static class OutgameAudioSourceObjects
    {
        public static AudioSource Create(GameObject root,string name)
        {
            if(!root)return null;
            var child=new GameObject();if(!string.IsNullOrEmpty(name))child.name=name;
            child.transform.SetParent(root.transform,false);child.transform.localPosition=Vector3.zero;
            child.transform.localRotation=Quaternion.identity;child.transform.localScale=Vector3.one;child.layer=root.layer;
            return child.AddComponent<AudioSource>();
        }
        public static void Destroy(AudioSource source){if(source)UnityEngine.Object.Destroy(source.gameObject);}
    }
    // AudioLoopAction3410, async state3409/26308. Required loaders preserve both resource branches.
    public sealed class OutgameAudioLoopAction:OutgameAudioAction
    {
        public OutgameAssetHandle ResourceHandle{get;private set;}
        public OutgameAudioLoopAction(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
        protected override void OnAwake(){}protected override void OnUpdate(){}protected override void OnPause(bool paused){}
        protected override async Task<AudioSource> OnPlay(OutgameAudioData data)
        {
            if(data==null){Stop();return null;}
            string path=data.ResPath;int start=data.StartTime,type=data.Atype;float volume=data.Vol<=0?1:data.Vol;
            float setting=GetSettingVolume();string name="loop_"+Guid;AudioClip clip;
            if(Services.UseNewResourceLoader()){ResourceHandle=await Services.LoadAudioClipForNewResource(path);clip=ResourceHandle.MainObject as AudioClip;}
            else clip=await Services.LoadAudioClip(path);
            if(!clip)return null;
            var source=OutgameAudioSourceObjects.Create(Root,name);source.playOnAwake=false;
            source.spatialBlend=type==1?0:1;if(type==2)source.dopplerLevel=0;
            source.clip=clip;source.volume=volume*setting;source.time=start;source.loop=true;source.Play();return source;
        }
        protected override void OnStop(){if(AudioSource)AudioSource.Stop();OnEndCall();ResourceHandle?.Release();Dispose();}
        protected override void OnDispose(){Services.DestroySource(AudioSource);AudioSource=null;}
    }
    // AudioOnceAction3415/26335, exported WeChat WASM's reachable loading path.
    // Its state48 SDK switch is initialized false and never written again; resume states0/1
    // have no scheduling predecessor. No Android or SDK-success branch is invented here.
    public sealed class OutgameAudioOnceAction:OutgameAudioAction
    {
        public float StartTime{get;private set;}public float EndTime{get;private set;}
        public bool Paused{get;private set;}public int SdkHandle{get;private set;}
        public float SdkDuration{get;private set;}public float StopAtRealtime{get;private set;}=-1;
        public OutgameAssetHandle ResourceHandle{get;private set;}
        public OutgameAudioOnceAction(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
        protected override void OnAwake(){}
        protected override async Task<AudioSource> OnPlay(OutgameAudioData data)
        {
            if(data==null){Stop();return null;}
            string path=data.ResPath;int start=data.StartTime,end=data.EndTime,type=data.Atype;float volume=data.Vol;
            float setting=GetSettingVolume();string name="once_"+Guid;EndTime=end;StartTime=start;
            float playbackVolume=(volume<=0?1:volume)*setting;AudioClip clip;
            if(Services.UseNewResourceLoader()){ResourceHandle=await Services.LoadAudioClipForNewResource(path);clip=ResourceHandle.MainObject as AudioClip;}
            else clip=await Services.LoadAudioClip(path);
            if(!clip)Services.Error(new object[]{"非Android平台加载Clip为空 Path=="+path});
            if(clip){
                var source=OutgameAudioSourceObjects.Create(Root,name);source.playOnAwake=false;
                source.spatialBlend=type==1?0:1;if(type==2)source.dopplerLevel=0;
                source.clip=clip;source.volume=playbackVolume;source.time=start;source.loop=false;source.Play();return source;
            }
            StopAtRealtime=Services.Realtime()+SdkDuration;return null;
        }
        protected override void OnPause(bool paused)=>Paused=paused;
        public override void RefreshAudioVolume(OutgameVoiceType voice,float value)=>base.RefreshAudioVolume(voice,value);
        protected override void OnUpdate()
        {
            if(AudioSource){
                if(EndTime<=0){if(!AudioSource.isPlaying&&!Paused)OnStop();}
                else if(AudioSource.time>=EndTime)OnStop();
            }else if(StopAtRealtime>0&&Services.Realtime()-StopAtRealtime>0&&!Paused)OnStop();
        }
        protected override void OnStop()
        {
            if(AudioSource){if(SdkHandle!=0)Services.StopSdkEffect(SdkHandle);AudioSource.Stop();}
            StopAtRealtime=-1;SdkHandle=0;OnEndCall();ResourceHandle?.Release();Dispose();
        }
        protected override void OnDispose(){Services.DestroySource(AudioSource);AudioSource=null;}
    }
}
