using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameAudioPlaybackValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameAudioPlaybackPlayModeValidation
    {
        const string Pending="AreaBattle.AudioPlaybackNative";
        [Serializable] sealed class Report{public bool passed;public string error;public string scope="Native AudioSource playback/time/pause/end/destruction using recovered original1002 and2001 clips. Resource tasks and update ownership are explicit fixture boundaries. No device recording, full AudioManager/resource host, Fade/LoopFade or Player-build claim.";public List<string> checks=new List<string>();}
        static Report report;static Fixture fixture;static OutgameAudioLoopAction loop;static OutgameAudioOnceAction once;static AudioSource source;static GameObject sourceObject;
        static double began,phaseAt;static float pausedTime;static int phase;static bool stopped;
        static OutgameAudioPlaybackPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string text){if(!yes)throw new Exception(text);report.checks.Add(text);}
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=phaseAt=EditorApplication.timeSinceStartup;
            try{
                new GameObject("NativeAudioListener",typeof(AudioListener));fixture=new Fixture(true);fixture.NewResources=true;fixture.Services.Realtime=()=>Time.realtimeSinceStartup;
                fixture.Data.StartTime=1;loop=fixture.Loop();loop.Play();source=loop.AudioSource;
                Check(source&&source.clip==fixture.Clip&&source.clip.samples>0&&source.clip.loadState==AudioDataLoadState.Loaded&&fixture.Started==1,"Original main-music1002 decoded/imported and native loop source created with start callback");
                EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;try{
                double now=EditorApplication.timeSinceStartup;if(now-began>25)throw new TimeoutException("Native audio playback timed out phase"+phase+" source="+(source?source.isPlaying+"/"+source.time:"null"));
                if(phase==0){
                    if(source.time<1.1f)return;Check(source.isPlaying&&source.loop&&source.time>1.1f,"Unity audio engine advances original1002 from configured start time");
                    loop.Pause(true);pausedTime=source.time;phaseAt=now;phase=1;return;
                }
                if(phase==1){
                    if(now-phaseAt<.15)return;Check(!source.isPlaying&&Mathf.Abs(source.time-pausedTime)<.02f,"Native pause freezes playback cursor across editor frames");
                    fixture.Settings.SetMusicGameVolume(OutgameVoiceType.Music,.25f);Check(Mathf.Abs(source.volume-.125f)<.0001f,"Production settings/message/action chain adjusts live source volume during pause");
                    loop.Pause(false);phase=2;return;
                }
                if(phase==2){
                    if(source.time<pausedTime+.1f)return;Check(source.isPlaying,"Native unpause resumes loop cursor");
                    source.Stop();fixture.Tick();Check(loop.AudioSource&&fixture.Ended==0&&fixture.Updates.Count==1,"Loop source external stop does not invent auto-completion in empty original Update");
                    sourceObject=source.gameObject;loop.Stop();Check(!loop.AudioSource&&loop.Data==null&&fixture.Provider.Releases==1&&fixture.Ended==1&&fixture.Updates.Count==0,"Loop Stop ends, releases actual handle and disposes source ownership");phase=3;return;
                }
                if(phase==3){
                    if(sourceObject)return;Check(!sourceObject,"Production DestroyAudioSource destroys native child GameObject after frame boundary");
                    fixture.NewResources=false;fixture.Data=new OutgameAudioData{id=1002,Vol=.5f,VoiceType=OutgameVoiceType.Music,Atype=1,Ptype=1,StartTime=0,EndTime=1,ResPath="Audio/Loop/hcrzd_MainBGM.wav"};
                    once=fixture.Once();once.Play();source=once.AudioSource;phase=4;return;
                }
                if(phase==4){
                    if(source.time<.1f)return;Check(source.isPlaying&&!source.loop&&fixture.Started==2,"Concrete once action runs native original clip without looping");
                    source.time=1;once.Pause(true);fixture.Tick();Check(once.Paused&&!once.AudioSource&&fixture.Ended==2&&fixture.Updates.Count==0,"Positive EndTime stops even while paused when native cursor reaches boundary, matching original branch");
                    fixture.Clip=Resources.Load<AudioClip>("Recovered/Audio/2001");Check(fixture.Clip&&fixture.Clip.length>0&&fixture.Clip.length<10,"Original button2001 available for natural completion");
                    fixture.Data=new OutgameAudioData{id=2001,Vol=1,VoiceType=OutgameVoiceType.Sound,Atype=1,Ptype=1,ResPath="Audio/once/ui/button_normal.mp3"};
                    once=fixture.Once();once.Play();source=once.AudioSource;Check(source&&source.isPlaying&&fixture.Started==3,"Natural-completion case starts real original sound clip");phase=5;return;
                }
                if(phase==5){
                    fixture.Tick();if(fixture.Ended<3)return;Check(!once.AudioSource&&once.Data==null&&fixture.Updates.Count==0&&once.StopAtRealtime==-1,"Once Update observes native clip completion and removes update/data/source");Finish(null);
                }
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/audio-playback-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
