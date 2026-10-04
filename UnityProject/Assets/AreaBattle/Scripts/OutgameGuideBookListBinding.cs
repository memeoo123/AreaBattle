using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Delays native UIObject creation until source DynamicList.PrepareSlot30104 spawns a root.
    public sealed class OutgameGuideBookDynamicItem:IOutgameDynamicItem
    {
        readonly OutgameGuideBookItemServices services;
        OutgameDynamicListProvider<OutgameGuideBookRow> provider;
        public OutgameGuideBookItem Item {get;private set;}
        public OutgameGuideBookDynamicItem(OutgameGuideBookItemServices services)=>this.services=services;
        public void OnCreate(IOutgameDynamicListProvider data)=>provider=(OutgameDynamicListProvider<OutgameGuideBookRow>)data;
        public void InstantiateNoNewItem(GameObject root){Item=new OutgameGuideBookItem(root,services);Item.OnCreate(provider);}
        public void OnRenderer(int index)=>Item.OnRenderer(index);
        public void Dispose()=>Item.Dispose();
    }
    // GuideBookUI33033/33046 list initialization/population portions.
    public sealed class OutgameGuideBookListBinding
    {
        public readonly OutgameDynamicListProvider<OutgameGuideBookRow> Data;
        readonly OutgameGuideBookItemServices services;
        public OutgameDynamicList List {get;}
        public OutgameGuideBookListBinding(Transform root,GameObject prefab,Func<OutgamePrefabPoolControl> pool,OutgameGuideBookItemServices services,
            OutgameDynamicListProvider<OutgameGuideBookRow> data=null,bool initialize=true)
        {
            this.services=services;Data=data??new OutgameDynamicListProvider<OutgameGuideBookRow>();
            List=root.Find("guideSV/Viewport/dynamicList").gameObject.AddComponent<OutgameDynamicList>();
            // Original MonoBehaviour -641384949970309176, not layout defaults inferred from a screenshot.
            List.AutoMask=false;List.KeepScroll=false;List.Recycle=true;List.Direction=0;List.ColumnAlignment=0;List.Inverse=false;
            List.SpacingSize=Vector2.zero;List.AutoAdapt=true;List.RowOrColumnCount=1;List.SpaceList=new float[0];List.IsNormalList=false;
            List.BindAwake(root.Find("guideSV").GetComponent<ScrollRect>(),prefab,pool);
            if(initialize){InitializeRenderer();Populate();}
        }
        public void InitializeRenderer()=>List.InitRendererList(Data,()=>new OutgameGuideBookDynamicItem(services));
        public void Populate()
        {
            Data.Data.Clear();
            foreach(var pair in services.Config().dicGuidebook)Data.Data.Add(new OutgameGuideBookRow{Config=pair.Value});
            Data.UpdateList();
        }
    }
}
