using System;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Common source bodies of3408/3413. Countdown arguments are (id, remainingMilliseconds).
    public abstract class OutgameAudioFadeBase:OutgameAudioAction
    {
        public float FadeVolume{get;private set;}public int FadeMilliseconds{get;private set;}=1000;
        public float ClipLength{get;private set;}public bool Stopping{get;private set;}
        public float StartTime{get;private set;}public float EndTime{get;private set;}public int TimerId{get;private set;}
        public OutgameAssetHandle ResourceHandle{get;private set;}
        protected abstract bool Loops{get;}
        protected OutgameAudioFadeBase(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
        protected override void OnAwake(){}protected override void OnPause(bool paused){}
        protected override async Task<AudioSource> OnPlay(OutgameAudioData data)
        {
            if(data==null){Stop();return null;}
            string path=data.ResPath;int start=data.StartTime,end=data.EndTime,type=data.Atype;float volume=data.Vol;
            float setting=GetSettingVolume();string name=(Loops?"loopFade_":"fade_")+Guid;
            EndTime=end;StartTime=start;FadeVolume=(volume<=0?1:volume)*setting;AudioClip clip;
            if(Services.UseNewResourceLoader()){ResourceHandle=await Services.LoadAudioClipForNewResource(path);clip=ResourceHandle.MainObject as AudioClip;}
            else clip=await Services.LoadAudioClip(path);
            if(!clip)return null;
            var source=OutgameAudioSourceObjects.Create(Root,name);source.playOnAwake=false;
            source.spatialBlend=type==1?0:1;if(type==2)source.dopplerLevel=0;
            source.clip=clip;source.volume=0;source.time=start;source.loop=Loops;source.Play();ClipLength=clip.length;
            Fade(source,true,null);return source;
        }
        void Fade(AudioSource source,bool fadeIn,Action completed)
        {
            if(!source)return;
            Action<int> finish=id=>{
                Services.RemoveTime(id);
                if(!source){completed?.Invoke();return;}
                if(fadeIn){source.volume=FadeVolume;return;}
                source.volume=0;completed?.Invoke();
            };
            Action<int,int> every=(id,remaining)=>{if(source)source.volume=FadeVolume*(fadeIn?((float)FadeMilliseconds-remaining)/FadeMilliseconds:(float)remaining/FadeMilliseconds);};
            if(TimerId!=0)Services.RemoveTime(TimerId);
            TimerId=Services.SetCountDownByMillisecond(FadeMilliseconds,finish,every);
        }
        void CompleteStop()
        {
            if(AudioSource)AudioSource.Stop();OnEndCall();ResourceHandle?.Release();Dispose();Stopping=false;
        }
        protected override void OnStop()
        {
            if(!Stopping){Stopping=true;if(AudioSource)FadeVolume=AudioSource.volume*GetSettingVolume();Fade(AudioSource,false,CompleteStop);return;}
            ResourceHandle?.Release();Dispose();
        }
        protected override void OnDispose(){if(TimerId!=0)Services.RemoveTime(TimerId);Services.DestroySource(AudioSource);AudioSource=null;}
        public override void RefreshAudioVolume(OutgameVoiceType voice,float value){if(VoiceType==voice)FadeVolume=value*Volume;base.RefreshAudioVolume(voice,value);}
        protected override void OnUpdate()
        {
            if(!AudioSource)return;
            if(EndTime<=0){bool playing=AudioSource.isPlaying;float time=AudioSource.time;float threshold=ClipLength-FadeMilliseconds/1000f;if(!playing||!(time>=threshold)||Stopping)return;}
            else if(!(AudioSource.time>=EndTime-FadeMilliseconds/1000f))return;
            if(Loops){Stopping=true;Fade(AudioSource,false,()=>{Stopping=false;Fade(AudioSource,true,null);});}
            else OnStop();
        }
    }
    public sealed class OutgameAudioFadeAction:OutgameAudioFadeBase
    {
        protected override bool Loops=>false;
        public OutgameAudioFadeAction(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
    }
    public sealed class OutgameAudioLoopFadeAction:OutgameAudioFadeBase
    {
        protected override bool Loops=>true;
        public OutgameAudioLoopFadeAction(GameObject root,int id,Action<int> start,Action<int> end,OutgameAudioActionServices services):base(root,id,start,end,services){}
    }
    public enum OutgameAudioActionType {Once=1,Loop=2,Fade=3,LoopFade=4}
    // AudioCompositeBase26381/26382: optional global override, then exact four-way constructor selection.
    public sealed class OutgameAudioActionFactory
    {
        public static Func<GameObject,OutgameAudioActionType,int,Action<int>,Action<int>,OutgameAudioAction> Override;
        readonly OutgameAudioActionServices services;
        public OutgameAudioActionFactory(OutgameAudioActionServices services){this.services=services;}
        public int GetAudioType(int id){var data=services.GetData(id);return data==null?1:data.Ptype;}
        public IOutgameAudioAction Create(GameObject root,int type,int id,Action<int> start,Action<int> end)
        {
            var overridden=Override?.Invoke(root,(OutgameAudioActionType)type,id,start,end);if(overridden!=null)return overridden;
            switch(type){
                case 1:return new OutgameAudioOnceAction(root,id,start,end,services);
                case 2:return new OutgameAudioLoopAction(root,id,start,end,services);
                case 3:return new OutgameAudioFadeAction(root,id,start,end,services);
                case 4:return new OutgameAudioLoopFadeAction(root,id,start,end,services);
                default:return null;
            }
        }
    }
}
