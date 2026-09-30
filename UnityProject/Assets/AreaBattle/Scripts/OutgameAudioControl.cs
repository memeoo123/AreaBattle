using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public enum OutgameVoiceType{Music=1,Sound=2}
    public interface IOutgameAudioNode
    {
        int[] Play(int[] audioIds);void Stop(OutgameVoiceType voice);void Pause(bool paused);void StopByGuid(int guid);
    }
    // Source GlobalData26850/26864/26867/26870 and AudioManager26354.
    public sealed class OutgameAudioSettings
    {
        readonly Func<OutgameUserPreferences> preferences;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameAudioSettings(Func<OutgameUserPreferences> preferences,Func<OutgameMessageDispatcher> messages){this.preferences=preferences;this.messages=messages;}
        public float SoundGameVolume{get=>preferences().GetFloat("SoundGameVolume",1);set=>preferences().SetFloat("SoundGameVolume",value);}
        public float MusicGameVolume{get=>preferences().GetFloat("MusicGameVolume",1);set=>preferences().SetFloat("MusicGameVolume",value);}
        public void SetMusicGameVolume(OutgameVoiceType voice,float value)
        {
            if(voice==OutgameVoiceType.Music)MusicGameVolume=value;else if(voice==OutgameVoiceType.Sound)SoundGameVolume=value;
            messages().SendMessage("GlobalVolumeChange",new object[]{voice,value});
        }
    }
    // Source UIAudioManager3431. Native node construction remains a required AudioManager service.
    public sealed class OutgameUiAudioManager
    {
        readonly Func<int,Transform,IOutgameAudioNode> createNode;readonly Action<object[]> log;readonly Action clearShared;
        readonly Dictionary<int,Func<int[],int[]>> routes=new Dictionary<int,Func<int[],int[]>>();
        public IOutgameAudioNode ParallelNode,SequenceNode,SingleNode;public bool Initialized{get;private set;}
        public OutgameUiAudioManager(Func<int,Transform,IOutgameAudioNode> createNode,Action<object[]> log,Action clearShared)
        {this.createNode=createNode;this.log=log;this.clearShared=clearShared;}
        void Initialize(){routes.Clear();routes.Add(1,PlayParallel);routes.Add(2,PlaySequence);routes.Add(3,PlaySingle);Initialized=true;}
        public int[] Play(int mode,params int[] audioIds){if(!Initialized)Initialize();return routes.TryGetValue(mode,out var play)&&play!=null?play(audioIds):null;}
        int[] PlayParallel(int[] ids){if(ParallelNode==null)ParallelNode=createNode(1,null);return ParallelNode?.Play(ids);}
        int[] PlaySequence(int[] ids){if(SequenceNode==null)SequenceNode=createNode(2,null);return SequenceNode?.Play(ids);}
        int[] PlaySingle(int[] ids){if(SingleNode==null)SingleNode=createNode(3,null);return SingleNode?.Play(ids);}
        public void StopParallelAudioByGUID(int guid)=>ParallelNode?.StopByGuid(guid);
        public void PauseParallerNode(bool paused)=>ParallelNode?.Pause(paused);
        public void StopParallelNode(OutgameVoiceType voice=0){log(new object[]{string.Format("StopParallelNode==StopParallelNode[{0}]",voice)});ParallelNode?.Stop(voice);}
        public void StopAll()
        {
            if(ParallelNode!=null){ParallelNode.Stop(0);ParallelNode=null;}
            if(SequenceNode!=null){SequenceNode.Stop(0);SequenceNode=null;}
            if(SingleNode!=null){SingleNode.Stop(0);SingleNode=null;}
        }
        public void Destroy()=>clearShared(); // Source26425 clears singleton only; no StopAll or node disposal.
    }
    // Instance slot provides source singleton semantics with explicit application-owned services.
    public sealed class OutgameUiAudioManagerSlot
    {
        readonly Func<int,Transform,IOutgameAudioNode> createNode;readonly Action<object[]> log;OutgameUiAudioManager instance;
        public OutgameUiAudioManagerSlot(Func<int,Transform,IOutgameAudioNode> createNode,Action<object[]> log){this.createNode=createNode;this.log=log;}
        public OutgameUiAudioManager Instance=>instance??(instance=new OutgameUiAudioManager(createNode,log,()=>instance=null));
    }
    // Complete AudioControl3904 public method surface; source ctor/init/update perform no work.
    public sealed class OutgameAudioControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;readonly Func<OutgameAudioSettings> settings;readonly Func<OutgameUiAudioManager> uiAudio;
        public OutgameAudioControl(OutgameControllerRegistry registry,Func<OutgameAudioSettings> settings,Func<OutgameUiAudioManager> uiAudio){this.registry=registry;this.settings=settings;this.uiAudio=uiAudio;}
        public bool IsSoundEffect{get=>settings().SoundGameVolume>.99f;set=>settings().SoundGameVolume=value?1:0;}
        public bool IsMusicEffect{get=>settings().MusicGameVolume>.99f;set=>settings().SetMusicGameVolume(OutgameVoiceType.Music,value?1:0);}
        public void OnInit(){}public void Updata(float deltaTime,float unscaledDeltaTime){}public void OnDispose()=>registry.Clear(3904);
        public void OnGamePlayState(int state)
        {
            switch(state){case 2:uiAudio().Play(1,1001);break;case 3:uiAudio().StopAll();break;case 5:uiAudio().Play(1,1002);break;case 6:uiAudio().ParallelNode?.Pause(false);break;case 7:uiAudio().ParallelNode?.Pause(true);break;case 8:case 9:uiAudio().StopParallelNode(OutgameVoiceType.Music);break;case 10:uiAudio().StopParallelNode(0);break;}
        }
    }
}
