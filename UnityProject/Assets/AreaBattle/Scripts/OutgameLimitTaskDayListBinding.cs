using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Day-list component from original CommonLimitTimeTaskUI. Page business owns its data and selection.
    public sealed class OutgameLimitTaskDayListBinding
    {
        public readonly OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem> Data;
        readonly OutgameLimitTaskPageItemServices services;
        public OutgameDynamicList List {get;}
        public OutgameLimitTaskDayListBinding(GameObject page,GameObject prefab,Func<OutgamePrefabPoolControl> pool,
            OutgameLimitTaskPageItemServices services,OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem> data=null,bool initializeRenderer=true)
        {
            this.services=services;Data=data??new OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem>();
            var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/LimitTask/hud-import").text,"CommonLimitTimeTaskUI");
            var nodes=new Dictionary<string,GameObject>();foreach(var binding in outlets.Read(page))nodes.Add(binding.Key,(GameObject)binding.Value);
            List=nodes["dynamicList"].AddComponent<OutgameDynamicList>();
            // Original component6678087315481976053, including non-recycling and initial30 spacing.
            List.AutoMask=false;List.KeepScroll=false;List.Recycle=false;List.Direction=0;List.ColumnAlignment=0;List.Inverse=false;
            List.SpacingSize=new Vector2(0,30);List.AutoAdapt=false;List.RowOrColumnCount=1;List.SpaceList=new[]{30f};List.IsNormalList=false;
            List.BindAwake(List.transform.parent.parent.GetComponent<ScrollRect>(),prefab,pool);
            if(initializeRenderer)InitializeRenderer();
        }
        public void InitializeRenderer()=>List.InitRendererList(Data,()=>new OutgameLimitTaskPageView(services));
    }
}
