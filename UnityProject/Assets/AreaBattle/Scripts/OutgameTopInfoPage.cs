using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameTopInfoPage:IOutgameMenuItem
    {
        bool Visible {get;}
        void RendererPart(int mask,string layer);
    }
    public interface IOutgameCurrencyTopInfo
    {
        Text GoldText {get;}
        Text DiamondsText {get;}
    }
    // TopInfoUI33920 canvas setup and33928/33932 rendering; owned lifecycle in companion partial.
    public sealed partial class OutgameTopInfoPage:OutgameUiPage,IOutgameTopInfoPage,IOutgameCurrencyTopInfo
    {
        Dictionary<int,GameObject> sourceParts;
        Dictionary<int,GameObject> parts;
        Dictionary<int,(Canvas canvas,GraphicRaycaster raycaster)> renderers;
        public int Layer=2;
        public Text GoldText {get;private set;}
        public Text DiamondsText {get;private set;}
        public OutgameTopInfoPage(GameObject root,Dictionary<int,GameObject> parts,Func<OutgameMessageDispatcher> messages=null):base(root,messages){sourceParts=parts;}
        public static OutgameTopInfoPage FromOriginal(GameObject root,Func<OutgameMessageDispatcher> messages=null)=>new OutgameTopInfoPage(root,new Dictionary<int,GameObject>{
            {2,root.transform.Find("objTopInfo/goldInfo").gameObject},
            {4,root.transform.Find("objTopInfo/diamondInfo").gameObject},
            {5,root.transform.Find("objTopInfo/spInfo").gameObject}},messages){
                GoldText=root.transform.Find("objTopInfo/goldInfo/txt_goldNum").GetComponent<Text>(),
                DiamondsText=root.transform.Find("objTopInfo/diamondInfo/txt_DiamondNum").GetComponent<Text>()};
        public void OpenParts()
        {
            parts=new Dictionary<int,GameObject>(sourceParts);
            renderers=new Dictionary<int,(Canvas,GraphicRaycaster)>();
            foreach(var pair in parts)
            {
                var canvas=pair.Value.AddComponent<Canvas>();var raycaster=pair.Value.AddComponent<GraphicRaycaster>();
                canvas.overrideSorting=true;canvas.sortingLayerName=OutgameUiLayerNames.Get(Layer);
                renderers.Add(pair.Key,(canvas,raycaster));
            }
        }
        public void RendererPart(int mask,int layer)=>RendererPart(mask,OutgameUiLayerNames.Get(layer));
        public void RendererPart(int mask,string layer)
        {
            if(parts==null)return;
            if(!Visible)SetVisible(true);
            // Original ldexpf(1,index) yields1,2,4; source dictionary also contains5,
            // which is intentionally not visited by the three-bit loop.
            for(int remaining=7,index=0;remaining!=0;remaining>>=1,mask>>=1,index++)
            {
                int key=1<<index;
                if(!parts.ContainsKey(key))continue;
                var renderer=renderers[key];renderer.canvas.enabled=false;
                bool selected=(mask&1)!=0;
                renderer.canvas.sortingLayerName=selected&&!string.IsNullOrEmpty(layer)?layer:OutgameUiLayerNames.Get(Layer);
                renderer.raycaster.enabled=selected;
                renderer.canvas.sortingOrder=string.IsNullOrEmpty(layer)?0:9;
                renderer.canvas.enabled=true;parts[key].SetActive(selected);
            }
        }
    }
    public static class OutgameUiLayerNames
    {
        // EUINode3545 constants and EUINodeExtend32420. Unknown enum returns null.
        static readonly string[] Names={"UIMain","UIWindow","UIPopup","UITip","UIAlert","UIStory","UIMessage"};
        public static string Get(int value)=>value>=0&&value<Names.Length?Names[value]:null;
    }
}
