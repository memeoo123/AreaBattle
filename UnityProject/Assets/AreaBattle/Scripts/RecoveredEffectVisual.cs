using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Original native emitters with an explicit scaled clock shared by player and replay.
    // Source seeds, curves, modules and renderer parameters stay on the imported prefab.
    public sealed class RecoveredEffectVisual : MonoBehaviour
    {
        sealed class Particle {public ParticleSystem System;public bool WasActive;}
        sealed class Legacy {public Animation Source;public AnimationClip Clip;public float Time;public WrapMode Wrap;public bool WasActive,Auto;}
        readonly List<Particle> particles=new List<Particle>();
        readonly List<Legacy> animations=new List<Legacy>();
        Animator[] animators;
        bool initialized;
        public void Initialize()
        {
            if(initialized)return;initialized=true;
            foreach(var ps in GetComponentsInChildren<ParticleSystem>(true))
            {ps.Pause(false);particles.Add(new Particle{System=ps});}
            foreach(var source in GetComponentsInChildren<Animation>(true))
            {
                var clip=source.clip;
                animations.Add(new Legacy{Source=source,Clip=clip,Auto=source.playAutomatically,Wrap=clip!=null&&clip.wrapMode!=WrapMode.Default?clip.wrapMode:source.wrapMode});
                source.enabled=false;
            }
            animators=GetComponentsInChildren<Animator>(true);
            foreach(var animator in animators)animator.enabled=false;
        }
        public void Step(float scaledDelta)
        {
            if(scaledDelta<0||float.IsNaN(scaledDelta)||float.IsInfinity(scaledDelta))throw new ArgumentOutOfRangeException(nameof(scaledDelta));
            Initialize();
            foreach(var item in particles)
            {
                var ps=item.System;if(ps==null)continue;
                bool active=ps.gameObject.activeInHierarchy;
                if(!active){item.WasActive=false;continue;}
                if(!item.WasActive)
                {
                    item.WasActive=true;
                    if(ps.main.playOnAwake){ps.Play(false);ps.Pause(false);}
                }
                if(ps.isPaused||ps.isPlaying)ps.Simulate(scaledDelta,false,false,false);
                // Simulate leaves the emitter paused, excluding the engine's automatic second tick.
            }
            foreach(var item in animations)
            {
                if(item.Source==null||item.Clip==null||!item.Auto)continue;
                bool active=item.Source.gameObject.activeInHierarchy;
                if(!active){item.WasActive=false;continue;}
                if(!item.WasActive){item.Time=0;item.WasActive=true;}
                item.Time+=scaledDelta;float time=item.Time,length=item.Clip.length;
                if(item.Wrap==WrapMode.Loop&&length>0)time=Mathf.Repeat(time,length);
                else if(item.Wrap==WrapMode.PingPong&&length>0)time=Mathf.PingPong(time,length);
                else time=Mathf.Min(time,length);
                item.Clip.SampleAnimation(item.Source.gameObject,time);
            }
            foreach(var animator in animators)if(animator!=null&&animator.gameObject.activeInHierarchy&&animator.runtimeAnimatorController!=null)animator.Update(scaledDelta);
        }
        public static RecoveredEffectVisual Attach(GameObject obj)
        {var visual=obj.GetComponent<RecoveredEffectVisual>()??obj.AddComponent<RecoveredEffectVisual>();visual.Initialize();return visual;}
    }
}
