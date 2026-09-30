using UnityEngine;

namespace AreaBattle
{
    // Deterministic adapter for the source Animation component's default autoplay clip.
    // The original registered secondary clips are assets only: no chaining is inferred.
    public sealed class RecoveredUiAnimation : MonoBehaviour
    {
        public AnimationClip DefaultClip;
        public string SourceId;
        public bool SourcePlayAutomatically;
        public WrapMode SourceComponentWrapMode;
        public float SourceDuration;
        public bool SourceAnimatePhysics;
        public int SourceCullingType;
        public float Elapsed { get; private set; }
        public float SampleTime { get; private set; }
        bool wasVisible;
        public void ResetPlayback()
        {
            Elapsed=0;SampleTime=0;wasVisible=false;
            if(SourcePlayAutomatically && DefaultClip!=null && gameObject.activeInHierarchy)
            {wasVisible=true;DefaultClip.SampleAnimation(gameObject,0);}
        }
        public void Advance(float scaledDelta)
        {
            if(scaledDelta<0 || float.IsNaN(scaledDelta) || float.IsInfinity(scaledDelta))throw new System.ArgumentOutOfRangeException(nameof(scaledDelta));
            if(!gameObject.activeInHierarchy){wasVisible=false;return;}
            if(!SourcePlayAutomatically || DefaultClip==null)return;
            if(!wasVisible){Elapsed=0;wasVisible=true;}
            Elapsed+=scaledDelta;
            var mode=DefaultClip.wrapMode==WrapMode.Default?SourceComponentWrapMode:DefaultClip.wrapMode;
            SampleTime=mode==WrapMode.Loop && SourceDuration>0?Mathf.Repeat(Elapsed,SourceDuration):mode==WrapMode.PingPong && SourceDuration>0?Mathf.PingPong(Elapsed,SourceDuration):Mathf.Min(Elapsed,SourceDuration);
            DefaultClip.SampleAnimation(gameObject,SampleTime);
        }
    }
}
