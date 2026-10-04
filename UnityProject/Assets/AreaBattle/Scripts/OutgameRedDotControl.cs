using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public enum OutgameRedDotDetectType {Update=0,PerSecond=1,Message=2}
    // Proj_hdzd.RedDot.RedDotControl4453. This is separate from GameFramework.RedDotModule3655.
    public sealed class OutgameRedDotControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;
        readonly Action<Color,object[]> log;
        List<OutgameRedDotItem> items;
        public bool ActiveUpdate {get;set;}
        public OutgameRedDotDetectType DetectType {get;private set;}=OutgameRedDotDetectType.PerSecond;
        public float Elapsed {get;private set;}
        public IReadOnlyList<OutgameRedDotItem> Items=>items;
        public OutgameRedDotControl(OutgameControllerRegistry registry,Action<Color,object[]> log)
        {this.registry=registry;this.log=log;}
        public void OnInit(){ActiveUpdate=false;items=new List<OutgameRedDotItem>();}
        public void InitRedDot(OutgameRedDotDetectType type){ActiveUpdate=true;DetectType=type;}
        public void AddItem(OutgameRedDotItem item)
        {
            if(items==null)return;
            items.Add(item);log(Color.green,new object[]{"有红点注册: "+item.gameObject.name});
        }
        public void RemoveItem(OutgameRedDotItem item)
        {
            if(items==null)return;
            if(items.Contains(item))items.Remove(item);
            log(Color.green,new object[]{"有红点退出: "+item.gameObject.name});
        }
        public void Updata(float deltaTime,float unscaledDeltaTime)
        {
            if(!ActiveUpdate)return;
            Elapsed+=unscaledDeltaTime;
            switch(DetectType)
            {
                case OutgameRedDotDetectType.Update:break;
                case OutgameRedDotDetectType.PerSecond:
                    if(!(Elapsed>=1f))return;
                    Elapsed=0;break;
                default:return;
            }
            Array.Empty<object>(); // Source34167 usage3989352 resolves Array.Empty<object>; return is discarded.
            Refresh();
        }
        void Refresh()
        {
            // Source reads component.gameObject before Unity null comparison, then check(slot5) before show(slot4).
            // Enumerate the live list; listener mutation and exceptions propagate through normal enumerator disposal.
            foreach(var item in items)if(item.gameObject!=null){item.CheackRedDot();item.ShowRedDot();}
        }
        public void OnDispose()=>registry.Clear(4453); // Source retains list, timer and ActiveUpdate.
    }
}
