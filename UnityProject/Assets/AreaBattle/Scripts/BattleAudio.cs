using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Source AudioFadeAction uses a wall-millisecond countdown, independent of battle pause.
    public sealed class AudioFadeEnvelope
    {
        double started;float basis;
        public bool Stopping {get;private set;}
        public bool Finished {get;private set;}
        public AudioFadeEnvelope(double now,float volume){started=now;basis=volume;}
        public float Evaluate(double now)
        {
            if(Finished)return 0;
            float progress=Mathf.Clamp01((float)((now-started)/1000));
            if(Stopping&&progress>=1)Finished=true;
            return basis*(Stopping?1-progress:progress);
        }
        public void Stop(double now,float settingVolume=1)
        {
            if(Stopping){Finished=true;return;}
            basis=Evaluate(now)*settingVolume;started=now;Stopping=true;
        }
    }
    // Source AudioLoopFadeAction f20576/f13128/f20574: native clip looping
    // continues while wall-time fades run across each last-second boundary.
    public sealed class AudioLoopFadeEnvelope
    {
        readonly float loopVolume;
        double started;float basis;
        bool fadingOut,terminalStop;
        public bool Stopping=>fadingOut;
        public bool Finished {get;private set;}
        public AudioLoopFadeEnvelope(double now,float volume){started=now;basis=loopVolume=volume;}
        public float Evaluate(double now)
        {
            if(Finished)return 0;
            float progress=Mathf.Clamp01((float)((now-started)/1000));
            if(fadingOut&&progress>=1)
            {
                if(terminalStop){Finished=true;return 0;}
                // Fade-out callback starts a fresh one-second fade-in. It never
                // seeks or restarts the AudioSource; its native loop wraps time.
                fadingOut=false;started=now;basis=loopVolume;return 0;
            }
            return basis*(fadingOut?1-progress:progress);
        }
        public float Advance(double now,float playbackTime,float clipLength,bool isPlaying)
        {
            float volume=Evaluate(now);
            if(!Finished&&!fadingOut&&isPlaying&&playbackTime>=clipLength-1f)
            {
                // Natural loop fade uses saved volume48, not current source volume.
                fadingOut=true;terminalStop=false;started=now;basis=loopVolume;volume=basis;
            }
            return volume;
        }
        public void Stop(double now,float settingVolume=1)
        {
            if(Finished)return;
            // Source field60 also guards the natural loop fade. A stop during it
            // completes/disposes immediately instead of launching a second fade.
            if(fadingOut){Finished=true;return;}
            basis=Evaluate(now)*settingVolume;started=now;fadingOut=true;terminalStop=true;
        }
    }
    public sealed class BattleAudio : MonoBehaviour
    {
        [Serializable] sealed class Manifest {public Entry[] clips;}
        [Serializable] sealed class Entry {public int id;public float volume;}
        readonly Dictionary<int,float> volumes=new Dictionary<int,float>();
        readonly Dictionary<int,long> deadlines=new Dictionary<int,long>();
        readonly Dictionary<TowerState,long> captureDeadlines=new Dictionary<TowerState,long>();
        sealed class OwnedClip {public AudioSource Source;public AudioFadeEnvelope Fade;}
        readonly Dictionary<int,OwnedClip> owned=new Dictionary<int,OwnedClip>();
        public float EffectSettingVolume=1;
        AudioSource music,effects,single;
        AudioLoopFadeEnvelope musicFade;
        public bool ParallelPaused {get;private set;}
        public bool MusicStopping=>musicFade!=null&&musicFade.Stopping;
        public AudioSource MusicSource=>music;
        public bool SoundEnabled=true,MusicEnabled=true;
        public void Initialize()
        {
            if(effects!=null)return;
            var manifest=Resources.Load<TextAsset>("Recovered/Audio/audio-runtime");if(manifest==null)return;
            foreach(var row in JsonUtility.FromJson<Manifest>(manifest.text).clips)volumes[row.id]=row.volume;
            effects=gameObject.AddComponent<AudioSource>();effects.playOnAwake=false;effects.spatialBlend=0;
            single=gameObject.AddComponent<AudioSource>();single.playOnAwake=false;single.spatialBlend=0;
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.spatialBlend=0;music.loop=true;
            BeginBattleMusic();
        }
        public void BeginBattleMusic()
        {
            if(music==null)return;
            music.clip=Resources.Load<AudioClip>("Recovered/Audio/1001");music.volume=0;music.mute=!MusicEnabled;
            musicFade=new AudioLoopFadeEnvelope(OriginalNowMilliseconds(),volumes.TryGetValue(1001,out float value)?value:.4f);
            if(music.clip!=null)music.Play();
            ParallelPaused=false;
        }
        public void ApplyPhase(BattlePhase phase)
        {
            // AudioControl.OnGamePlayState pauses only its parallel node (field8).
            // Result states StopParallelNode(VoiceType.Music=1), preserving sound effects.
            if(phase==BattlePhase.Victory||phase==BattlePhase.Defeat)
            {if(musicFade!=null)musicFade.Stop(OriginalNowMilliseconds(),MusicEnabled?1:0);return;}
            bool paused=phase==BattlePhase.Pause;ParallelPaused=paused;
            foreach(var source in ParallelSources())if(source!=null){if(paused)source.Pause();else source.UnPause();}
        }
        IEnumerable<AudioSource> ParallelSources()
        {yield return music;yield return effects;foreach(var item in owned.Values)yield return item.Source;}
        public void SetMusic(bool enabled){MusicEnabled=enabled;if(music!=null)music.mute=!enabled;}
        public void SetSound(bool enabled){SoundEnabled=enabled;EffectSettingVolume=enabled?1:0;}
        public static long OriginalNowSeconds()=> (long)((DateTime.Now-new DateTime(1970,1,1,8,0,0)).Ticks*1e-7);
        public static double OriginalNowMilliseconds()=> (DateTime.Now-new DateTime(1970,1,1,8,0,0)).Ticks*.0001;
        public void OnSkillAudio(SkillVisualEvent e)
        {
            double now=OriginalNowMilliseconds();
            if(e.Kind=="audio-stop")
            {if(owned.TryGetValue(e.HandleId,out var old))old.Fade.Stop(now,EffectSettingVolume);return;}
            if(e.Kind!="audio-play")return;
            bool direct=e.AudioRoute=="ui-audio";
            if(!direct&&!SoundEnabled)return;
            if(e.AudioId!=2041){Play(e.AudioId,null,direct);return;}
            var clip=Resources.Load<AudioClip>("Recovered/Audio/2041");if(clip==null||effects==null)return;
            var source=gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;source.clip=clip;source.volume=0;source.time=0;source.Play();
            owned[e.HandleId]=new OwnedClip{Source=source,Fade=new AudioFadeEnvelope(now,(volumes.TryGetValue(2041,out float volume)&&volume>0?volume:1)*EffectSettingVolume)};
        }
        void Update()
        {
            double now=OriginalNowMilliseconds();var done=new List<int>();
            if(music!=null&&musicFade!=null)
            {music.volume=musicFade.Advance(now,music.time,music.clip!=null?music.clip.length:0,music.isPlaying);if(musicFade.Finished){music.Stop();musicFade=null;}}
            foreach(var pair in owned)
            {
                var item=pair.Value;var source=item.Source;
                if(source==null){done.Add(pair.Key);continue;}
                source.volume=item.Fade.Evaluate(now);
                if(source.isPlaying&&!item.Fade.Stopping&&source.time>=source.clip.length-1)item.Fade.Stop(now,EffectSettingVolume);
                if(item.Fade.Finished){source.volume=0;source.Stop();Destroy(source);done.Add(pair.Key);}
            }
            foreach(int id in done)owned.Remove(id);
        }
        public void OnBattleEvent(BattleEvent e,BattleSimulation sim)
        {
            if(e.Kind=="phase")ApplyPhase(e.Phase);
            if(e.Kind=="restart")BeginBattleMusic();
            int id=e.AudioId;
            long now=OriginalNowSeconds();
            if(e.Kind=="capture")
            {
                var tower=sim.Tower(e.TowerId);if(tower==null)return;
                captureDeadlines.TryGetValue(tower,out long deadline);
                if(deadline>=now)return;captureDeadlines[tower]=now+1;
            }
            if(id!=0)Play(id,now);
        }
        public void Play(int id,long? clock=null,bool bypassSoundGuard=false,bool singleMode=false)
        {
            if((!SoundEnabled&&!bypassSoundGuard)||effects==null)return;
            long now=clock??OriginalNowSeconds();
            if(id==2012||id==2013)
            {deadlines.TryGetValue(id,out long deadline);if(deadline>now)return;deadlines[id]=now+1;}
            var clip=Resources.Load<AudioClip>("Recovered/Audio/"+id);
            if(clip!=null)
            {
                float volume=(volumes.TryGetValue(id,out float level)?level:1)*EffectSettingVolume;
                if(singleMode||id==2012||id==2013){single.Stop();single.clip=clip;single.volume=volume;single.Play();}
                else effects.PlayOneShot(clip,volume);
            }
        }
    }
}
