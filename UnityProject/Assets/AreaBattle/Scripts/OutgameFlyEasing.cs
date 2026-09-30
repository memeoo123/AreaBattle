using System;
using UnityEngine;
namespace AreaBattle
{
    // Exact float expression order of DOTween EaseManager68862 circular branches.
    public static class OutgameFlyEasing
    {
        public static float Evaluate(int ease,float time,float duration)
        {
            float t=time/duration;
            switch(ease)
            {
                case 6:return -t*(t-2f);
                case 20:return -(Mathf.Sqrt(1f-t*t)-1f);
                case 21:t=t-1f;return Mathf.Sqrt(1f-t*t);
                default:throw new ArgumentOutOfRangeException(nameof(ease),ease,"Unrestored fly movement easing");
            }
        }
    }
}
