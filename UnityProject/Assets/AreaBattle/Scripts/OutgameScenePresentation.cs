using System;
using UnityEngine;
namespace AreaBattle
{
    // GameControl.MoveCamera f3946 and InitScene f12187 (viewport branch).
    public sealed class OutgameScenePresentation
    {
        readonly OutgameModelRoots roots;
        public OutgameScenePresentation(OutgameModelRoots roots){this.roots=roots!=null?roots:throw new ArgumentNullException(nameof(roots));}
        public void MoveCamera(bool shop,float offset)
        {
            if(shop)
            {
                var target=Vector3.up*offset;var current=roots.SceneHome.position;
                // Source compares exact components of Scene_home WORLD position, not soldierRoot.
                if(current.x==target.x&&current.y==target.y&&current.z==target.z)return;
                roots.ModelBackdrop.gameObject.SetActive(true);roots.SoldierRoot.localPosition=target;roots.HomeBackdrop.gameObject.SetActive(false);
            }
            else
            {
                roots.ModelBackdrop.gameObject.SetActive(false);roots.SoldierRoot.localPosition=Vector3.zero;roots.HomeBackdrop.gameObject.SetActive(true);
            }
        }
        public void ApplyViewport(int width,int height,float sourceHeightAdjustment)
        {
            float ratio=width/(float)(height+sourceHeightAdjustment);
            if(ratio>.5625f)
            {
                float scale=ratio/.5625f;roots.HomeBackdrop.localScale*=scale;roots.ModelBackdrop.localScale*=scale;
            }
            else if(ratio<.5625f)
            {
                float scale=.5625f/ratio;roots.HomeCamera.orthographicSize=scale*.89f;
                roots.HomeBackdrop.localScale*=scale;roots.ModelBackdrop.localScale=roots.HomeBackdrop.localScale;
            }
        }
    }
}
