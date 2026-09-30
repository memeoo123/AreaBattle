using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // One explicit projection of ConfigMgr static fields4..40, initialized by30435.
    public sealed class OutgameConfigGlobalValues
    {
        public float BaseGameTimeScale,GameTimeScale,ShipTimeScale,StarScale;
        public float LineRendererWide,BaseLineRendererMoveSpeed,LineRendererMoveSpeed,LineRendererTilingScale,HalfLineRendererWide;
        public string ShareURL;
        public void Initialize(Dictionary<object,GlobalValueConfig> values,Func<int> shareUrlKey)
        {
            GameTimeScale=float.Parse(values[101].Content1);BaseGameTimeScale=GameTimeScale;
            ShipTimeScale=float.Parse(values[102].Content1);StarScale=float.Parse(values[110].Content1);
            var parts=values[113].Content1.Split(';');
            LineRendererWide=float.Parse(parts[0]);LineRendererMoveSpeed=float.Parse(parts[1])*GameTimeScale;
            BaseLineRendererMoveSpeed=LineRendererMoveSpeed;LineRendererTilingScale=float.Parse(parts[2]);
            HalfLineRendererWide=LineRendererWide*.5f;ShareURL=values[shareUrlKey()].Content1;
        }
    }
    // ConfigMgr30410 colors and material dispatch; completion30407 is a separate resource boundary.
    public static class OutgameConfigCampPresentation
    {
        public static void Initialize(Dictionary<object,CampConfig> camps,Func<int> campSumNum,
            Dictionary<int,Color> lineColors,Dictionary<int,Color[]> soldierColors,Action<string,object[]> loadMaterial)
        {
            for(int i=0;i<=campSumNum();i++){
                var colors=new Color[3];Color color;
                ColorUtility.TryParseHtmlString("#"+camps[i].LineColor,out color);lineColors[i]=color;
                ColorUtility.TryParseHtmlString("#"+camps[i].soldierColor0,out color);colors[0]=color;
                ColorUtility.TryParseHtmlString("#"+camps[i].soldierColor1,out color);colors[1]=color;
                ColorUtility.TryParseHtmlString("#"+camps[i].soldierColor2,out color);colors[2]=color;
                soldierColors[i]=colors;
            }
            for(int i=0;i<=campSumNum();i++){
                string path=camps[i].BasitionMaterial;
                loadMaterial(path,new object[]{i,camps[i].BasitionMaterial});
            }
        }
    }
}
