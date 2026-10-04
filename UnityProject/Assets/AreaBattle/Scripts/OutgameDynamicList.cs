using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameDynamicListProvider
    {
        OutgameDynamicList DynamicList {set;}
        int GetListCount();
        OutgameDynamicRenderItem CreateRenderer();
    }
    // Source4552: the list is public and mutable; notifications do not change its contents.
    public class OutgameDynamicListProvider<T>:IOutgameDynamicListProvider
    {
        public List<T> Data=new List<T>();
        public virtual OutgameDynamicList DynamicList {get;set;}
        public T GetData(int index)=>Data[index];
        public int IndexOf(T value)=>Data.IndexOf(value);
        public virtual int GetListCount()=>Data.Count;
        public void UpdateItemData(int index)=>DynamicList.UpdateItemData(index);
        public void UpdateList()=>DynamicList.UpdateList(this,false);
        public void Centered2Top(float duration=0)=>DynamicList.CenteredWithIndex(0,duration);
        public virtual OutgameDynamicRenderItem CreateRenderer()=>new OutgameDynamicRenderItem{Provider=this};
    }
    public interface IOutgameDynamicItem
    {
        void OnCreate(IOutgameDynamicListProvider provider);
        void InstantiateNoNewItem(GameObject root);
        void OnRenderer(int index);
        void Dispose();
    }
    public interface IOutgameDynamicHidden {void OnHidden();}
    public sealed class OutgameDynamicRegion
    {
        public readonly Rect Rect;public readonly int Index;
        public float X=>Rect.x;public float Y=>Rect.y;
        public OutgameDynamicRegion(float x,float y,float width,float height,int index)
        {Index=index;Rect=new Rect(x,y,width,height);}
        public bool Overlaps(Rect rect)=>Rect.Overlaps(rect);
        public override string ToString()=>string.Format("index:{0},x:{1},y:{2},w:{3},h:{4}",Index,Rect.x,Rect.y,Rect.width,Rect.height);
    }
    // Source4557 keeps the native object, item and region as separate references.
    public class OutgameDynamicRenderItem
    {
        public IOutgameDynamicListProvider Provider;
        public GameObject Root;
        public OutgameDynamicRegion Region;
        IOutgameDynamicItem item;IOutgameDynamicHidden hidden;
        public IOutgameDynamicItem BaseItem {get=>item;set{item=value;hidden=value as IOutgameDynamicHidden;OnBaseItemChanged();}}
        protected virtual void OnBaseItemChanged(){}
        public virtual void Refresh(){if(Region!=null)BaseItem.OnRenderer(Region.Index);}
        public void OnHidden(){if(hidden!=null)hidden.OnHidden();}
    }
    // Source3868. BindAwake supplies recovered prefab references before replaying Awake30101.
    public sealed class OutgameDynamicList:MonoBehaviour
    {
        public bool AutoMask,KeepScroll,Recycle=true,AutoAdapt=true,Inverse,IsNormalList;
        public int Direction,ColumnAlignment,RowOrColumnCount=1;
        public Vector2 SpacingSize;
        public float[] SpaceList;
        public Action OnNewItemRender;
        public ScrollRect Scroll {get;private set;}
        public GameObject Prefab {get;private set;}
        public int ShowMinIdx {get;private set;}=-1;
        public int ShowMaxIdx {get;private set;}=-1;
        public int SlotCount=>slotCount;
        public int Columns=>columns;
        public bool IsDirty=>dirty;
        public bool IsInitialized=>initialized;
        public IReadOnlyList<OutgameDynamicRegion> Regions=>regions;
        public IReadOnlyList<OutgameDynamicRenderItem> Renderers=>renderers;
        IOutgameDynamicListProvider provider;
        Func<OutgamePrefabPoolControl> pool;
        RectTransform content;Vector2 itemSize,viewportSize;Rect visibleRect;
        bool sizeCached,initialized,dirty;
        Coroutine positionCoroutine;
        int columns,slotCount;
        GameObject[] objects;IOutgameDynamicItem[] items;OutgameDynamicRenderItem[] renderers;
        OutgameDynamicRegion[] regions;
        readonly Dictionary<int,OutgameDynamicRegion> visible=new Dictionary<int,OutgameDynamicRegion>();
        readonly List<OutgameDynamicRenderItem> pending=new List<OutgameDynamicRenderItem>();
        public void BindAwake(ScrollRect scroll,GameObject prefab,Func<OutgamePrefabPoolControl> pool)
        {
            Scroll=scroll;Prefab=prefab;this.pool=pool;
            for(int i=0;i<transform.childCount;i++)
            {var child=transform.GetChild(i);if(child.name==Prefab.name)child.gameObject.SetActive(false);}
            Scroll.onValueChanged.AddListener(OnScroll);
            if(Scroll.horizontalScrollbar!=null)Scroll.horizontalScrollbar.onValueChanged.AddListener(OnScrollbar);
            if(Scroll.verticalScrollbar!=null)Scroll.verticalScrollbar.onValueChanged.AddListener(OnScrollbar);
            if(Prefab==null)Debug.LogError("DynamicList组件prefabItem为空");
        }
        void OnScroll(Vector2 value)=>dirty=true;
        void OnScrollbar(float value)=>dirty=true;
        Vector2 ItemSize
        {
            get{if(sizeCached)return itemSize;sizeCached=true;itemSize=Prefab.GetComponent<RectTransform>().sizeDelta;return itemSize;}
        }
        float StepX=>ItemSize.x+SpacingSize.x;
        float StepY=>ItemSize.y+SpacingSize.y;
        // Source WASM conversion returns int.MinValue for NaN, infinity and out-of-range values.
        static int Ceil(float value){double result=Math.Ceiling(value);return Math.Abs(result)<2147483648d?(int)result:int.MinValue;}
        float Space(int line)
        {
            float result=0;if(SpaceList==null||SpaceList.Length==0)return result;
            for(int i=0;i<=line&&i<SpaceList.Length;i++)result+=SpaceList[i];return result;
        }
        public void SetSpaceList(float[] values)=>SpaceList=values;
        public int GetSpaceListLength()=>SpaceList==null?0:SpaceList.Length;
        void Adapt()
        {
            viewportSize=transform.parent.GetComponent<RectTransform>().rect.size;
            visibleRect=Direction==0?new Rect(0,-viewportSize.y,viewportSize.x,viewportSize.y):new Rect(0,0,viewportSize.x,viewportSize.y);
            if(!AutoAdapt){columns=RowOrColumnCount;return;}
            int axis=Direction==1?1:0,count=Ceil(viewportSize[axis]/ItemSize[axis]),fit=0;float used=0;
            for(int i=0;i<count;i++){used+=ItemSize[axis];if(used<=viewportSize[axis])fit++;used+=SpacingSize[axis];}
            columns=Math.Max(fit,1);
        }
        public void InitRendererList(IOutgameDynamicListProvider data,Func<IOutgameDynamicItem> createItem)
        {
            provider=data;provider.DynamicList=this;
            if(initialized)return;
            content=transform as RectTransform;content.pivot=new Vector2(0,Direction==0&&Inverse?0:1);
            Adapt();slotCount=unchecked((Ceil(Direction==0?viewportSize.y/StepY:viewportSize.x/StepX)+2)*columns);
            pool().CreatePool(Prefab,0);CalculateRegions(slotCount);
            renderers=new OutgameDynamicRenderItem[slotCount];items=new IOutgameDynamicItem[slotCount];objects=new GameObject[slotCount];
            for(int i=0;i<slotCount;i++)
            {items[i]=createItem();items[i].OnCreate(data);renderers[i]=data.CreateRenderer();renderers[i].Region=regions[i];}
            Resize(0);initialized=true;dirty=true;
        }
        void CalculateRegions(int count)
        {
            if(count<columns)columns=Math.Max(count,1);
            regions=new OutgameDynamicRegion[count];
            for(int i=0;i<count;i++)
            {
                int row=i/columns,column=i-row*columns;
                if(Direction==0)
                {
                    float x;
                    switch(ColumnAlignment)
                    {
                        case 0:x=0;break;
                        case 1:x=columns%2==0?viewportSize.x*.5f-(columns/2)*StepX+SpacingSize.x*.5f:viewportSize.x*.5f-(columns/2)*StepX-ItemSize.x*.5f;break;
                        case 2:x=viewportSize.x-columns*StepX-SpacingSize.x;break;
                        default:throw new Exception("错误");
                    }
                    regions[i]=new OutgameDynamicRegion(x+column*StepX,Inverse?row*StepY+Space(row):-row*StepY-ItemSize.y-Space(row),ItemSize.x,ItemSize.y,i);
                }
                else regions[i]=new OutgameDynamicRegion(row*StepX+ItemSize.x+Space(row),column*StepY,ItemSize.x,ItemSize.y,i);
            }
        }
        void Resize(int count)
        {
            viewportSize=transform.parent.GetComponent<RectTransform>().rect.size;
            int lines=Ceil(count*1f/columns);var size=content.sizeDelta;
            if(Direction==0)size.y=lines*StepY+Space(lines);else size.x=lines*StepX+Space(lines);
            content.sizeDelta=size;
            if(Direction==0){Scroll.horizontal=false;Scroll.vertical=KeepScroll||content.sizeDelta.y>viewportSize.y;}
            else{Scroll.horizontal=KeepScroll||content.sizeDelta.x>viewportSize.x;Scroll.vertical=false;}
            for(int i=0;i<Math.Min(count,slotCount);i++)PrepareSlot(i);
        }
        void PrepareSlot(int index)
        {
            if(renderers[index].Root!=null)return;
            var root=pool().Spawn(Prefab,transform);objects[index]=root;
            var rect=root.GetComponent<RectTransform>();rect.anchorMax=new Vector2(0,1);rect.anchorMin=new Vector2(0,1);
            if(Direction==0){if(Inverse){rect.anchorMax=Vector2.zero;rect.anchorMin=Vector2.zero;}rect.pivot=Vector2.zero;}
            else rect.pivot=Vector2.one;
            var renderer=renderers[index];
            if(renderer.BaseItem==null){renderer.BaseItem=items[index];renderer.BaseItem.InstantiateNoNewItem(root);}
            root.SetActive(false);renderer.Root=root;
        }
        void ClearRegions(){foreach(var renderer in renderers)renderer.Region=null;}
        public void UpdateList()
        {
            if(IsNormalList){Debug.LogError("普通列表模式只能使用UpdateList<>泛型方法");return;}
            if(AutoMask)Adapt();int count=provider.GetListCount();CalculateRegions(count);Resize(count);ClearRegions();dirty=true;
        }
        // Generic30086: keepBindings suppresses the explicit clearing step, including stale region references.
        public void UpdateList(IOutgameDynamicListProvider data,bool keepBindings)
        {
            if(AutoMask)Adapt();int count=provider.GetListCount();CalculateRegions(count);
            if(IsNormalList&&count>slotCount)
            {
                Array.Resize(ref renderers,count);Array.Resize(ref items,count);Array.Resize(ref objects,count);
                for(int i=slotCount;i<count;i++)
                {items[i]=(IOutgameDynamicItem)Activator.CreateInstance(items[0].GetType());items[i].OnCreate(data);renderers[i]=data.CreateRenderer();renderers[i].Region=regions[i];}
                slotCount=count;
            }
            Resize(count);if(!keepBindings)ClearRegions();dirty=true;
        }
        public void ForceRefreshDataProvider()
        {
            if(provider==null||provider.GetListCount()==0)return;
            if(AutoMask)Adapt();int count=provider.GetListCount();CalculateRegions(count);Resize(count);ClearRegions();dirty=true;
        }
        public void RefreshSoft(){if(renderers==null)return;for(int i=0;i<renderers.Length;i++)renderers[i].Refresh();}
        public void UpdateItemData(int index)
        {if(provider==null||provider.GetListCount()==0||index<0)return;var renderer=GetItem(index);if(renderer!=null)renderer.Refresh();}
        public OutgameDynamicRenderItem GetItem(int index)
        {if(index<0||index>regions.Length-1)return null;return Find(regions[index]);}
        OutgameDynamicRenderItem Find(OutgameDynamicRegion region)
        {foreach(var renderer in renderers)if(renderer.Region!=null&&region.Index==renderer.Region.Index)return renderer;return null;}
        OutgameDynamicRenderItem FreeSlot()
        {
            foreach(var renderer in renderers)if(renderer.Region==null)return renderer;
            string message="Error,"+gameObject.name+",len:"+renderers.Length;Debug.LogError(message);throw new Exception(message);
        }
        void HideUnbound()
        {foreach(var renderer in renderers)if(renderer.Region==null&&renderer.Root!=null)renderer.Root.SetActive(false);}
        void LateUpdate(){if(initialized&&dirty){dirty=false;RefreshVisible();}}
        void RefreshVisible()
        {
            if(provider==null||provider.GetListCount()==0){ShowMinIdx=-1;ShowMaxIdx=-1;HideUnbound();return;}
            visible.Clear();pending.Clear();int count=provider.GetListCount();
            if(Direction==0)visibleRect.y=-viewportSize.y-content.anchoredPosition.y;
            else visibleRect.x=-content.anchoredPosition.x+ItemSize.x;
            foreach(var region in regions)if(IsNormalList||region.Overlaps(visibleRect))visible.Add(region.Index,region);
            foreach(var renderer in renderers)if(renderer.Region!=null&&!visible.ContainsKey(renderer.Region.Index))renderer.Region=null;
            ShowMinIdx=int.MaxValue;ShowMaxIdx=-1;
            foreach(var region in visible.Values){ShowMinIdx=Math.Min(ShowMinIdx,region.Index);ShowMaxIdx=Math.Max(ShowMaxIdx,region.Index);}
            foreach(var pair in visible)
            {
                if(Find(pair.Value)!=null)continue;
                var renderer=FreeSlot();renderer.Region=pair.Value;
                if(pair.Value.Index>=count)continue;
                pending.Add(renderer);
            }
            HideUnbound();
            for(int i=0;i<pending.Count;i++)
            {
                var renderer=pending[i];renderer.Root.SetActive(true);
                var rect=(RectTransform)renderer.Root.transform;rect.anchoredPosition3D=Vector3.zero;
                rect.anchoredPosition=new Vector2(renderer.Region.X,Direction!=0?-renderer.Region.Y:renderer.Region.Y);
                renderer.Refresh();OnNewItemRender?.Invoke();
            }
        }
        public void GetVisibleItems(List<IOutgameDynamicItem> result)
        {result.Clear();foreach(var renderer in renderers)if(renderer.Region!=null)result.Add(renderer.BaseItem);}
        public void Centered2Top(float duration=0)=>CenteredWithIndex(0,duration);
        public void Centered2Bottom(float duration=0)=>CenteredWithIndex(unchecked(provider.GetListCount()-1),duration);
        // Source30097 warns on invalid indices and still clamps/positions, including an empty provider.
        public void CenteredWithIndex(int index,float duration=0)
        {
            int count=provider.GetListCount();
            if(index<0||index>unchecked(count-1))Debug.LogWarning("Locate Index Error "+index);
            Ceil(Direction==0?viewportSize.y/StepY:viewportSize.x/StepX); // Source evaluates this even though the result is unused.
            index=Math.Max(Math.Min(index,unchecked(count-1)),0);
            Vector2 from=content.anchoredPosition,to=from;int row=index/columns;
            if(Direction==0)
            {
                float center=(row+.5f)*StepY+Space(row),half=viewportSize.y*.5f;
                to.y=Inverse?-(center+half):center-half;
                to.y=Inverse?Mathf.Clamp(to.y,-content.sizeDelta.y,-viewportSize.y):Mathf.Clamp(to.y,0,Mathf.Max(content.sizeDelta.y-viewportSize.y,0));
            }
            else
            {
                to.x=-((row+.5f)*StepX+Space(row)-viewportSize.x*.5f);
                to.x=Mathf.Clamp(to.x,-Mathf.Max(content.sizeDelta.x-viewportSize.x,0),0);
            }
            SetCenteredPosition(from,to,duration);dirty=true;
        }
        // Source30078 is intentionally distinct: invalid indices throw; space/inverse and horizontal sign differ.
        public void CenteredWithTwoIndex(int first,int second,float duration)
        {
            int count=provider.GetListCount();
            if(first<0||first>unchecked(count-1))throw new Exception("Locate Index Error "+first);
            if(second<0||second>unchecked(count-1))throw new Exception("Locate Index Error "+second);
            int visibleLines=Ceil(Direction==0?viewportSize.y/StepY:viewportSize.x/StepX);
            first=Math.Max(Math.Min(first,unchecked(provider.GetListCount()-1)),0);
            second=Math.Max(Math.Min(second,unchecked(provider.GetListCount()-1)),0);
            Vector2 from=content.anchoredPosition,a=from,b=from;
            int row=first/columns;
            if(Direction==0)a.y=(row+.5f)*StepY-viewportSize.y*.5f;else a.x=(row+.5f)*StepX-viewportSize.x*.5f;
            row=second/columns;
            if(Direction==0)b.y=(row+.5f)*StepY-viewportSize.y*.5f;else b.x=(row+.5f)*StepX-viewportSize.x*.5f;
            float half=visibleLines*.5f;Vector2 to;
            if(half>first&&half<second){to=from;if(Direction==0)to.y=0;else to.x=0;}
            else
            {
                to=(a+b)*.5f;
                if(first>provider.GetListCount()-half&&second>provider.GetListCount()-half)
                {if(Direction==0)to.y=content.sizeDelta.y-viewportSize.y;else to.x=content.sizeDelta.x-viewportSize.x;}
            }
            SetCenteredPosition(from,to,duration); // Unlike30097, no explicit dirty assignment here.
        }
        void SetCenteredPosition(Vector2 from,Vector2 to,float duration)
        {
            if(duration<=0){content.anchoredPosition=to;Scroll.StopMovement();}
            else positionCoroutine=StartCoroutine(MovePosition(from,to,duration));
        }
        // Iterator3867/30108 first yields, then advances with scaled deltaTime. Each frame allocates its own yield object.
        // A finishing older routine stops the currently stored handle; starting another does not cancel existing routines.
        IEnumerator MovePosition(Vector2 from,Vector2 to,float duration)
        {
            bool running=true;float elapsed=0;
            while(running)
            {
                yield return new WaitForEndOfFrame();elapsed+=Time.deltaTime;Vector2 next;
                if(duration<=elapsed){running=false;next=to;if(positionCoroutine!=null)StopCoroutine(positionCoroutine);positionCoroutine=null;}
                else next=Vector2.Lerp(from,to,elapsed/duration);
                content.anchoredPosition=next;dirty=true;
            }
        }
        public void Dispose()
        {
            if(renderers!=null)
            {
                foreach(var renderer in renderers)if(renderer.Region!=null)renderer.OnHidden();
                foreach(var renderer in renderers){if(renderer.BaseItem!=null)renderer.BaseItem.Dispose();renderer.Root=null;}
            }
            if(objects!=null&&Recycle)
                for(int i=objects.Length-1;i>=0;i--){if(objects[i]==null)continue;pool().Recycle(objects[i]);objects[i]=null;}
        }
        void OnDestroy()=>Dispose();
    }
}
