using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // BaseUI.addCanvas27325. Only original layer1 gains an independent canvas.
    public sealed class OutgameUiCanvas
    {
        readonly Func<int> maximumWindowIndex;readonly Func<bool> sourceFlag28;
        public int WindowIndex {get;private set;}
        public OutgameUiCanvas(Func<int> maximumWindowIndex,Func<bool> sourceFlag28)
        {this.maximumWindowIndex=maximumWindowIndex;this.sourceFlag28=sourceFlag28;}
        public void Add(GameObject root,int layer)
        {
            if(layer!=1)return;
            var canvas=root.GetComponent<Canvas>();if(canvas==null)canvas=root.AddComponent<Canvas>();
            canvas.overrideSorting=true;
            WindowIndex=unchecked(maximumWindowIndex()+1);
            bool wide=sourceFlag28();
            canvas.sortingOrder=unchecked((wide?100:10)+(WindowIndex-1)*(wide?200:3));
            canvas.sortingLayerID=SortingLayer.NameToID("UIWindow");
            if(root.GetComponent<GraphicRaycaster>()==null)root.AddComponent<GraphicRaycaster>();
        }
    }
}
