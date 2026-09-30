using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Soldier f5663/f10316/f7123; Tags.cctor f15702; MPBExtension f11448.
    public sealed class OutgameSoldierColor
    {
        readonly IReadOnlyList<Color> colors;readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
        public OutgameSoldierColor(IReadOnlyList<Color> colors){this.colors=colors??throw new ArgumentNullException(nameof(colors));}
        public static OutgameSoldierColor FromRecovered()
        {
            var source=Resources.Load<GameObject>("Recovered/Soldiers/soldier_100").GetComponent<RecoveredSoldierVisual>().CampColors;
            var values=new Color[6];Array.Copy(source,1,values,0,6);return new OutgameSoldierColor(values);
        }
        public static string CampTag(int camp)=>camp>=1&&camp<=6?"Tag"+(camp+4):"Tag9";
        public void Apply(GameObject model,int camp)
        {
            string tag=CampTag(camp);if(model.CompareTag(tag))return;
            model.tag=tag;Apply(model.GetComponentInChildren<MeshRenderer>(),camp);
            var renderer=model.GetComponent<Renderer>();renderer.sortingLayerName="Default";renderer.sortingOrder=-5;
        }
        public void Apply(MeshRenderer renderer,int camp)
        {
            if(renderer==null)return;
            var color=colors[Math.Max(camp,1)-1];renderer.GetPropertyBlock(block);block.SetColor("_Color",color);renderer.SetPropertyBlock(block);
        }
    }
}
