using System;
using UnityEngine;
namespace AreaBattle
{
    // EffectControl31210, native cosine1108 and sine1114. Preserve source .017f multiplier.
    public sealed class OutgameFlyScatter
    {
        readonly Func<int,int,int> random;readonly Func<float,float> sin,cos;
        public OutgameFlyScatter():this(GameRandomSource.Shared.Inclusive,Mathf.Sin,Mathf.Cos){}
        public OutgameFlyScatter(Func<int,int,int> random,Func<float,float> sin,Func<float,float> cos){this.random=random;this.sin=sin;this.cos=cos;}
        public Vector2 Position(Vector2 origin)
        {
            int angle=random(0,360);int radius=random(10,20);float a=angle*.017f;float r=radius/10f;
            float y=origin.y+r*sin(angle*.017f);float x=origin.x+r*cos(a);return new Vector2(x,y);
        }
    }
}
