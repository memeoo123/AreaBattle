using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Native scalar subset used by the source loading-page DOTween.To (one loop, scaled time).
    public sealed class OutgameScalarTweenRunner:MonoBehaviour
    {
        sealed class Tween {public Func<float> Read;public Action<float> Write;public Action Complete;public float End,Duration,Elapsed,Start;public bool Started;public int Ease;}
        readonly List<Tween> active=new List<Tween>();
        public int DefaultEase=6; // Recovered DOTween defaultEaseType = OutQuad.
        public int Count=>active.Count;
        public void To(Func<float> read,Action<float> write,float end,float duration,Action completed)
        {To(read,write,end,duration,completed,DefaultEase);}
        public void To(Func<float> read,Action<float> write,float end,float duration,Action completed,int ease)
        {active.Add(new Tween{Read=read,Write=write,End=end,Duration=duration,Complete=completed,Ease=ease});}
        public void Advance(float scaledDelta)
        {
            if(scaledDelta>-.000001f&&scaledDelta<.000001f)return;
            foreach(var tween in active.ToArray())
            {
                try
                {
                    if(!tween.Started){tween.Start=tween.Read();tween.Started=true;}
                    tween.Elapsed+=scaledDelta;float t=tween.Duration<=0?1f:Mathf.Min(tween.Elapsed/tween.Duration,1f);
                    float eased=OutgameFlyEasing.Evaluate(tween.Ease,t,1f);tween.Write(tween.Start+(tween.End-tween.Start)*eased);
                    if(tween.Elapsed>=tween.Duration){active.Remove(tween);tween.Complete?.Invoke();}
                }
                catch(Exception ex){active.Remove(tween);Debug.LogException(ex);}
            }
        }
        void Update()=>Advance(Time.deltaTime);
    }
}
