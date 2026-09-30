using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameAudioPlaybackValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAudioFadePlayModeValidation
    {
        const string Pending="AreaBattle.AudioFadeNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Native AudioControl/UI/Parallel/concrete-factory/Fade actions and recovered1001/2001 playback. Countdown pulses use an explicit real-time test driver; original TimeModule, resource/update ownership and full AudioManager remain separate work. No recording or Player claim.";public List<string> checks=new List<string>();}
        sealed class Pulse{public int Id,Duration;public double Began;public Action<int> Finish;public Action<int,int> Every;}
        static readonly Dictionary<int,Pulse> pulses=new Dictionary<int,Pulse>();static int next,phase,started,ended;static Report report;static bool stopped,driveActions;static double began,phaseAt;static float pausedCursor,previousVolume;
        static Fixture fixture;static OutgameAudioControl control;static OutgameUiAudioManager ui;static OutgameAudioParallel node;static OutgameAudioFadeBase action;static AudioSource source;static GameObject sourceObject;
        static OutgameAudioFadePlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange s){if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;driveActions=true;phase=next=started=ended=0;pulses.Clear();began=phaseAt=EditorApplication.timeSinceStartup;
            try{
                new GameObject("FadeNativeListener",typeof(AudioListener));fixture=new Fixture(true);fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/1001");fixture.Provider.AssetObject=fixture.Clip;fixture.NewResources=true;
                fixture.Data=new OutgameAudioData{id=1001,Atype=1,Ptype=4,VoiceType=OutgameVoiceType.Music,Vol=.4f,ResPath="Audio/Loop/hcrzd_GameBGM.wav"};
                fixture.Services.SetCountDownByMillisecond=(ms,finish,every)=>{int id=++next;pulses.Add(id,new Pulse{Id=id,Duration=ms,Began=EditorApplication.timeSinceStartup,Finish=finish,Every=every});return id;};fixture.Services.RemoveTime=id=>pulses.Remove(id);
                var factory=new OutgameAudioActionFactory(fixture.Services);ui=new OutgameUiAudioManager((mode,parent)=>{if(mode!=1)throw new Exception("Unexpected node mode");return node=new OutgameAudioParallel(fixture.Root,17,"native-controller-audio",factory.GetAudioType,factory.Create,()=>fixture.Messages);},args=>{},()=>{});
                control=new OutgameAudioControl(null,()=>fixture.Settings,()=>ui);fixture.Messages.AddListener("AudioPlayStart",args=>started++);fixture.Messages.AddListener("AudioPlayEnd",args=>ended++);
                control.OnGamePlayState(2);foreach(var a in node.Actions.Values)action=(OutgameAudioFadeBase)a;source=action.AudioSource;
                Check(action is OutgameAudioLoopFadeAction&&source&&source.clip==fixture.Clip&&source.loop&&source.volume==0&&started==1,"State2 -> UI parallel -> source Ptype4 factory -> real original1001 loop-fade source and global start");EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Pump(double now){foreach(var p in new List<Pulse>(pulses.Values)){if(!pulses.ContainsKey(p.Id))continue;int remaining=p.Duration-(int)((now-p.Began)*1000);if(remaining<=0)p.Finish(p.Id);else p.Every(p.Id,remaining);}if(driveActions)fixture.Tick();}
        static void Poll()
        {
            if(stopped)return;try{
                double now=EditorApplication.timeSinceStartup;if(now-began>30)throw new TimeoutException("Fade native phase"+phase);Pump(now);
                if(phase==0){if(source.time<.15f)return;Check(source.isPlaying&&source.volume>0&&source.volume<.4f,"Native cursor advances while production countdown callback raises volume from zero");control.OnGamePlayState(7);pausedCursor=source.time;previousVolume=source.volume;phaseAt=now;phase=1;return;}
                if(phase==1){if(now-phaseAt<.2)return;Check(!source.isPlaying&&Mathf.Abs(source.time-pausedCursor)<.02f&&source.volume>previousVolume,"State7 pauses native playback but source empty pause hook leaves fade callbacks active");phase=2;return;}
                if(phase==2){if(pulses.Count!=0)return;Check(Mathf.Abs(source.volume-.4f)<.0001f&&action.TimerId!=0,"Fade-in completion reaches configured volume and retains timer id after removal");control.OnGamePlayState(6);phase=3;return;}
                if(phase==3){if(source.time<pausedCursor+.1f)return;Check(source.isPlaying,"State6 resumes original native music after fade-in completed while paused");control.IsMusicEffect=false;Check(source.volume==0&&action.FadeVolume==0,"Controller music toggle reaches settings, global message and live fade target");fixture.Settings.SetMusicGameVolume(OutgameVoiceType.Music,.5f);Check(Mathf.Abs(source.volume-.2f)<.0001f,"Fractional setting refresh uses original .4 configured volume");control.OnGamePlayState(8);Check(action.Stopping&&Mathf.Abs(action.FadeVolume-.1f)<.0001f&&node.Actions.Count==1,"State8 music stop starts fade-out with source-volume times setting; node remains until completion");phaseAt=now;phase=4;return;}
                if(phase==4){if(now-phaseAt<.3)return;Check(source&&source.isPlaying&&source.volume>0&&source.volume<.1f&&ended==0,"Native loop keeps playing while fade-out lowers volume before end event");sourceObject=source.gameObject;phase=5;return;}
                if(phase==5){if(ended<1||sourceObject)return;Check(node.Actions.Count==0&&!action.AudioSource&&action.UpdateId==0&&fixture.Provider.Releases==1&&pulses.Count==0,"Fade completion sends end, removes GUID, releases handle/update and destroys native child");
                    fixture.NewResources=false;fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/2001");fixture.Data=new OutgameAudioData{id=2001,Atype=1,Ptype=3,VoiceType=OutgameVoiceType.Sound,Vol=1,ResPath="Audio/once/ui/button_normal.mp3"};ui.Play(1,2001);foreach(var a in node.Actions.Values)action=(OutgameAudioFadeBase)a;source=action.AudioSource;
                    Check(action is OutgameAudioFadeAction&&source&&!source.loop&&source.isPlaying&&started==2,"Same UI node resolves Ptype3 into native original2001 one-shot fade action");phase=6;return;
                }
                if(phase==6){if(!action.Stopping)return;Check(action.EndTime==0&&action.ClipLength<1&&source&&node.Actions.Count==1,"Short original clip enters source length-minus-one-second automatic fade-out");phase=7;return;}
                if(phase==7){if(ended<2)return;Check(!action.AudioSource&&node.Actions.Count==0&&fixture.Updates.Count==0&&pulses.Count==0,"One-shot fade ends through countdown completion and same node removes final action");Finish(null);}
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/audio-fade-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
