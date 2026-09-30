using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // UIModule27424 + UIUtils27454. Scoped one-loop native tween adapter.
    // Coroutine waits and tween updates remain separate, as in the source.
    public sealed class OutgameUiAnimation:MonoBehaviour
    {
        sealed class Tween
        {
            public UnityEngine.Object Target;public Func<float> Read;public Action<float> Write;
            public float Start,End,Duration,Elapsed;public int Ease;public bool Started;
        }
        readonly List<Tween> tweens=new List<Tween>();
        public float DefaultDuration=.5f,FadeOutDuration=.3f,ScaleOutDuration=.2f;
        public float Overshoot=1.70158f;
        public float ResolveDuration(int animation,float duration)
            =>duration>0?duration:animation==2?FadeOutDuration:animation==4?ScaleOutDuration:DefaultDuration;
        public async Task Play(GameObject target,int animation,float duration)
        {await OutgameUnityAwait.Await(PlayRoutine(target,animation,duration));}
        public IEnumerator PlayRoutine(GameObject target,int animation,float duration)
        {yield return StartCoroutine(ObjectAnim(target,animation,ResolveDuration(animation,duration)));}
        void Add(UnityEngine.Object target,Func<float> read,Action<float> write,float end,float duration,int ease,bool from)
        {
            var tween=new Tween{Target=target,Read=read,Write=write,End=end,Duration=duration,Ease=ease};
            if(from){tween.Start=end;tween.End=read();tween.Started=true;write(end);}
            tweens.Add(tween);
        }
        public IEnumerator ObjectAnim(GameObject target,int animation,float duration)
        {
            if(animation==0||target==null)yield break;
            if(animation==1||animation==2)
            {
                var group=target.GetComponent<CanvasGroup>();
                if(group!=null)
                {
                    Debug.Log(string.Format("target[{0}]找到了CanvasGroup，优先使用CanvasGroup控制UI渐隐",target));
                    Add(group,()=>animation==1?0f:1f,value=>group.alpha=value,animation==1?1f:0f,duration,6,false);
                }
                else
                {
                    var graphics=target.GetComponentsInChildren<Graphic>();
                    for(int i=graphics.Length-1;i>=0;i--)
                    {
                        var graphic=graphics[i];
                        Add(graphic,()=>graphic.color.a,value=>{var color=graphic.color;color.a=value;graphic.color=color;},0,duration,6,animation==1);
                    }
                }
            }
            else if(animation==3||animation==4)
            {
                var transform=target.transform;Vector3 start=default;bool captured=false;
                // Scalar progress keeps the source vector components, including a nonuniform z.
                Func<float> read=()=>{start=transform.localScale;captured=true;return 1f;};
                Action<float> write=value=>{if(!captured){start=transform.localScale;captured=true;}transform.localScale=start*value;};
                Add(transform,read,write,0,duration,animation==3?27:26,animation==3);
            }
            else yield break;
            yield return new WaitForSeconds(duration);
        }
        float Ease(int ease,float t)
        {
            if(ease==6)return -t*(t-2f);
            if(ease==26)return t*t*((Overshoot+1f)*t-Overshoot);
            t-=1f;return t*t*((Overshoot+1f)*t+Overshoot)+1f;
        }
        public void Advance(float scaledDelta)
        {
            if(scaledDelta>-.000001f&&scaledDelta<.000001f)return;
            foreach(var tween in tweens.ToArray())
            {
                if(tween.Target==null){tweens.Remove(tween);continue;}
                if(!tween.Started){tween.Start=tween.Read();tween.Started=true;}
                tween.Elapsed+=scaledDelta;
                float time=tween.Duration<=0?1f:Mathf.Min(tween.Elapsed/tween.Duration,1f);
                float value=Ease(tween.Ease,time);tween.Write(tween.Start+(tween.End-tween.Start)*value);
                if(tween.Elapsed>=tween.Duration)tweens.Remove(tween);
            }
        }
        void Update()=>Advance(Time.deltaTime);
    }
}
