using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Task-list component from original CommonLimitTimeTaskUI. Page owns filtering, sorting and refresh events.
    public sealed class OutgameLimitTaskListBinding
    {
        public readonly OutgameDynamicListProviderSelection<OutgameLimitTaskItemData> Data;
        readonly OutgameLimitTaskItemServices services;
        public OutgameDynamicList List {get;}
        public OutgameLimitTaskListBinding(GameObject page,GameObject prefab,Func<OutgamePrefabPoolControl> pool,
            OutgameLimitTaskItemServices services,OutgameDynamicListProviderSelection<OutgameLimitTaskItemData> data=null,bool initializeRenderer=true)
        {
            this.services=services;Data=data??new OutgameDynamicListProviderSelection<OutgameLimitTaskItemData>();
            var outlets=new OutgameImportedUiOutlets(Resources.Load<TextAsset>("Recovered/LimitTask/hud-import").text,"CommonLimitTimeTaskUI");
            var nodes=new Dictionary<string,GameObject>();foreach(var binding in outlets.Read(page))nodes.Add(binding.Key,(GameObject)binding.Value);
            List=nodes["Content"].AddComponent<OutgameDynamicList>();
            // Original component-1916771998422009902, distinct from the adjacent day list.
            List.AutoMask=false;List.KeepScroll=false;List.Recycle=false;List.Direction=0;List.ColumnAlignment=1;List.Inverse=false;
            List.SpacingSize=new Vector2(0,8);List.AutoAdapt=false;List.RowOrColumnCount=1;List.SpaceList=new[]{10f};List.IsNormalList=false;
            List.BindAwake(List.transform.parent.parent.GetComponent<ScrollRect>(),prefab,pool);
            if(initializeRenderer)InitializeRenderer();
        }
        public void InitializeRenderer()=>List.InitRendererList(Data,()=>new OutgameLimitTaskView(services));
        // Source CommonLimitTimeTaskUI32880. Keep live element reads around GameValue callbacks.
        public void RenderDay(OutgameChildLimitTimeTaskActivity child,OutgameStatisticsExpansion statistics,int day)
        {
            var tasks=child.GetDayTaskList(day);Data.Data.Clear();
            for(int i=0;i<tasks.Count;i++)
            {
                bool shown=true;
                for(int j=0;j<tasks[i].ShowCondition.Count;j++)
                {
                    int key=tasks[i].ShowCondition[j].key;var args=tasks[i].ShowCondition[j].arg;
                    long value=statistics.GameValue(key,args);
                    if(value<tasks[i].ShowCondition[j].value){shown=false;break;}
                }
                if(shown)Data.Data.Add(tasks[i]);
            }
            Data.Data.Sort((a,b)=>a.BtnState.CompareTo(b.BtnState));
            List.CenteredWithIndex(0,0);List.UpdateList();
        }
    }
}
