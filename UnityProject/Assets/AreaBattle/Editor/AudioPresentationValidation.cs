using System;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class AudioPresentationValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<bool,string> require=(ok,message)=>{if(!ok)throw new Exception(message);};
            try{
                var fade=new AudioFadeEnvelope(100000,.8f);
                require(Mathf.Abs(fade.Evaluate(100500)-.4f)<.00001f,"source half-second fade-in");
                // Battle time need not advance for a wall-clock audio action to progress.
                require(Mathf.Abs(fade.Evaluate(101000)-.8f)<.00001f,"battle pause cannot freeze wall-clock fade");
                fade.Stop(101250,.5f);
                require(Mathf.Abs(fade.Evaluate(101750)-.2f)<.00001f&&!fade.Finished,"stop fades from current volume times setting");
                require(fade.Evaluate(102250)==0&&fade.Finished,"one-second cleanup boundary");
                var interrupted=new AudioFadeEnvelope(0,1);interrupted.Stop(250);require(Mathf.Abs(interrupted.Evaluate(750)-.125f)<.00001f,"stop during fade-in starts from actual current volume");
                interrupted.Stop(750);require(interrupted.Finished&&interrupted.Evaluate(750)==0,"second source stop finishes instead of restarting countdown");
                report.checks.Add(new BattleBuild.Check{id="audio2041-source-wall-clock-fade-and-stop",result="pass"});
                var loop=new AudioLoopFadeEnvelope(0,.4f);
                require(Mathf.Abs(loop.Advance(500,.5f,10,true)-.2f)<.00001f,"loop initial half-second fade-in");
                require(Mathf.Abs(loop.Advance(8999,8.999f,10,true)-.4f)<.00001f&&!loop.Stopping,"no natural fade before last second");
                require(Mathf.Abs(loop.Advance(9000,9,10,true)-.4f)<.00001f&&loop.Stopping,"source last-second boundary starts natural fade");
                require(Mathf.Abs(loop.Advance(9500,9.5f,10,true)-.2f)<.00001f,"loop tail half-fade");
                require(loop.Advance(10000,0,10,true)==0&&!loop.Finished&&!loop.Stopping,"natural wrap starts fade-in without ending music action");
                require(Mathf.Abs(loop.Advance(10500,.5f,10,true)-.2f)<.00001f,"next loop head half-fade");
                require(Mathf.Abs(loop.Advance(11000,1,10,true)-.4f)<.00001f,"next loop full volume");
                loop.Advance(19000,9,10,true);require(loop.Stopping,"subsequent loops also fade");
                report.checks.Add(new BattleBuild.Check{id="music-source-natural-loop-tail-and-head-fades",result="pass"});
                var pausedLoop=new AudioLoopFadeEnvelope(0,.4f);
                require(Mathf.Abs(pausedLoop.Advance(9000,9,10,false)-.4f)<.00001f&&!pausedLoop.Stopping,"paused playback does not start tail fade");
                pausedLoop.Advance(10000,9,10,true);
                require(Mathf.Abs(pausedLoop.Advance(10500,9,10,false)-.2f)<.00001f,"existing tail fade advances on wall time during pause");
                require(pausedLoop.Advance(11000,9,10,false)==0&&!pausedLoop.Stopping&&!pausedLoop.Finished,"pause does not block loop fade callback");
                require(Mathf.Abs(pausedLoop.Advance(11500,9,10,false)-.2f)<.00001f,"next fade-in continues during pause");
                report.checks.Add(new BattleBuild.Check{id="music-source-loop-fades-use-wall-time-through-pause",result="pass"});
                var stoppedLoop=new AudioLoopFadeEnvelope(0,.4f);stoppedLoop.Advance(9000,9,10,true);stoppedLoop.Stop(9250);
                require(stoppedLoop.Finished&&stoppedLoop.Evaluate(9250)==0,"source stop during natural fade disposes immediately");
                var stoppedHead=new AudioLoopFadeEnvelope(0,.4f);stoppedHead.Stop(500,.5f);
                require(Mathf.Abs(stoppedHead.Evaluate(1000)-.05f)<.00001f,"ordinary stop uses current head volume times setting");
                require(stoppedHead.Advance(1500,9,10,true)==0&&stoppedHead.Finished,"terminal stop never restarts loop fade-in");
                report.checks.Add(new BattleBuild.Check{id="music-source-stop-during-loop-fade-and-initial-fade",result="pass"});
                foreach(int id in new[]{2016,2018,2021,2026,2027,2028,2029,2030,2031,2032,2033,2034,2035,2036,2037,2038,2039,2040,2041,2042,2043,3121,3122,4401,4501})
                {var clip=Resources.Load<AudioClip>("Recovered/Audio/"+id);require(clip!=null&&clip.samples>0,"original configured skill clip "+id);}
                report.checks.Add(new BattleBuild.Check{id="audio25-source-skill-clips-imported",result="pass"});
                var host=new GameObject("Audio phase replay");
                try{
                    var audio=host.AddComponent<BattleAudio>();audio.Initialize();
                    require(audio.MusicSource!=null&&audio.MusicSource.clip!=null&&audio.MusicSource.clip.name=="1001"&&audio.MusicSource.loop,"original battle loop1001");
                    audio.ApplyPhase(BattlePhase.Pause);require(audio.ParallelPaused,"pause parallel node");
                    audio.ApplyPhase(BattlePhase.Running);require(!audio.ParallelPaused,"resume parallel node");
                    audio.ApplyPhase(BattlePhase.Victory);require(audio.MusicStopping,"victory stops music with original fade");
                    audio.BeginBattleMusic();require(!audio.MusicStopping&&!audio.ParallelPaused,"retry creates new music action");
                    audio.SetSound(false);require(!audio.SoundEnabled&&audio.EffectSettingVolume==0,"settings muting also affects direct UI audio");
                    audio.SetMusic(false);require(audio.MusicSource.mute,"original music setting");
                }finally{UnityEngine.Object.DestroyImmediate(host);}
                report.checks.Add(new BattleBuild.Check{id="audio-source-pause-result-retry-and-settings",result="pass"});
            }catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id="audio-presentation",result="fail",detail=ex.ToString()});}
            return report;
        }
    }
}
