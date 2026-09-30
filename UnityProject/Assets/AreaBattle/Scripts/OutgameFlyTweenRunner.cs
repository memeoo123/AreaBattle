using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Scoped native runner for the forward, one-loop vector tweens used by fly currency.
    // Timing/PlayForward and easing come from recovered DOTween; other tween features are not exposed.
    public sealed class OutgameFlyTweenRunner:MonoBehaviour
    {
        public sealed class Handle
        {
            internal Transform Target;internal Vector3 Start,End;internal float Duration,Elapsed;internal int Ease;
            internal bool Scale,Independent,AutoKill,Started,Playing,Complete,Active=true;internal string Id;internal Action Callback;
            public bool IsComplete=>Complete;public bool IsActive=>Active;
        }
        readonly List<Handle> active=new List<Handle>();
        public int DefaultScaleEase=6; // Original DOTween .cctor defaultEaseType, overridable by recovered settings.
        public Handle Move(Transform target,Vector3 end,float duration,int ease,string id,bool independent,Action completed)
            =>Create(target,end,duration,ease,id,independent,false,true,true,completed);
        Handle Create(Transform target,Vector3 end,float duration,int ease,string id,bool independent,bool scale,bool autoKill,bool playing,Action completed)
        {
            var h=new Handle{Target=target,End=end,Duration=duration,Ease=ease,Id=id,Independent=independent,Scale=scale,AutoKill=autoKill,Playing=playing,Callback=completed};active.Add(h);return h;
        }
        public Handle CreateTargetTween(Transform target)
        {
            return Create(target,new Vector3(1.2f,1.2f,0),.2f,DefaultScaleEase,"Target_Tween",true,true,false,false,
                ()=>Create(target,new Vector3(1,1,0),.2f,DefaultScaleEase,null,true,true,true,true,null));
        }
        public void PlayForward(Handle h)
        {
            if(!h.Active)return;
            if(h.Complete){h.Playing=false;return;}
            h.Playing=true;
        }
        void Apply(Handle h,float time)
        {
            if(!h.Started){h.Start=h.Scale?h.Target.localScale:h.Target.position;h.Started=true;}
            float value=OutgameFlyEasing.Evaluate(h.Ease,time,h.Duration);
            Vector3 point=h.Start+(h.End-h.Start)*value;
            if(h.Scale)h.Target.localScale=point;else h.Target.position=point;
        }
        void Complete(Handle h)
        {
            if(h.Complete)return;
            Apply(h,h.Duration);h.Elapsed=h.Duration;h.Complete=true;h.Playing=false;
            try{h.Callback?.Invoke();}catch(Exception ex){Debug.LogException(ex);}
            if(h.AutoKill)h.Active=false;
        }
        public void Kill(string id,bool complete)
        {
            foreach(var h in active.ToArray())if(h.Active&&h.Id==id){if(complete)Complete(h);h.Active=false;h.Playing=false;}
            active.RemoveAll(h=>!h.Active);
        }
        public void Advance(float scaledDelta,float unscaledDelta)
        {
            foreach(var h in active.ToArray())
            {
                if(!h.Active||!h.Playing)continue;
                if(h.Target==null){h.Active=false;continue;}
                float dt=h.Independent?unscaledDelta:scaledDelta;
                if(dt>-0.000001f&&dt<0.000001f)continue;
                h.Elapsed+=dt;
                if(h.Elapsed>=h.Duration)Complete(h);else Apply(h,h.Elapsed);
            }
            active.RemoveAll(h=>!h.Active);
        }
        void Update()=>Advance(Time.deltaTime,Time.unscaledDeltaTime);
    }
}
