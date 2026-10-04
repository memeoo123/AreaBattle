using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameDynamicListValidation
    {
        public sealed class Item:IOutgameDynamicItem,IOutgameDynamicHidden
        {
            public static List<string> Trace;
            public OutgameDynamicListProvider<int> Provider;public GameObject Root;
            public int Index=-1,Value,RenderCount;public bool FailRender;
            public void OnCreate(IOutgameDynamicListProvider provider){Provider=(OutgameDynamicListProvider<int>)provider;Trace?.Add("create");}
            public void InstantiateNoNewItem(GameObject root){Root=root;Trace?.Add("attach");}
            public void OnRenderer(int index){Index=index;Value=Provider.GetData(index);RenderCount++;Trace?.Add("render:"+index);if(FailRender)throw new InvalidOperationException("Explicit render failure");}
            public void OnHidden()=>Trace?.Add("hidden:"+Index);
            public void Dispose()=>Trace?.Add("dispose:"+Index);
        }
        public sealed class Fixture:IDisposable
        {
            public readonly GameObject Root,Prefab;public readonly OutgameDynamicList List;
            public readonly OutgameDynamicListProvider<int> Data=new OutgameDynamicListProvider<int>();
            public readonly OutgamePrefabPoolControl Pool;
            public Fixture(float width=100,float height=50,float itemWidth=50,float itemHeight=25)
            {
                Root=new GameObject("dynamic-list-fixture",typeof(RectTransform));Root.SetActive(false);
                var viewport=new GameObject("Viewport",typeof(RectTransform));viewport.transform.SetParent(Root.transform,false);((RectTransform)viewport.transform).sizeDelta=new Vector2(width,height);
                var content=new GameObject("Content",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);((RectTransform)content.transform).sizeDelta=new Vector2(width,0);
                var scroll=Root.AddComponent<ScrollRect>();scroll.viewport=(RectTransform)viewport.transform;scroll.content=(RectTransform)content.transform;
                Prefab=new GameObject("Row",typeof(RectTransform));((RectTransform)Prefab.transform).sizeDelta=new Vector2(itemWidth,itemHeight);
                var registry=new OutgameControllerRegistry();Pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));
                registry.Bind(4561,()=>Pool);registry.Resolve(4561);Pool.OnInit();
                List=content.AddComponent<OutgameDynamicList>();List.BindAwake(scroll,Prefab,()=>Pool);
            }
            public void Init(int count){List.InitRendererList(Data,()=>new Item());for(int i=0;i<count;i++)Data.Data.Add(i+100);Data.UpdateList();}
            public void ScrollTo(float y){((RectTransform)List.transform).anchoredPosition=new Vector2(0,y);List.Scroll.onValueChanged.Invoke(Vector2.zero);Tick(List);}
            public void Dispose(){UnityEngine.Object.DestroyImmediate(Root);Pool.OnDispose();UnityEngine.Object.DestroyImmediate(Prefab);Item.Trace=null;}
        }
        public static void Tick(OutgameDynamicList list)
        {try{typeof(OutgameDynamicList).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(list,null);}catch(TargetInvocationException e){throw e.InnerException;}}
        public static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source DynamicList scroll/recycle/provider path and recovered GuideBook rows; edit checks drive the LateUpdate entry explicitly. Native frame checks are separate. Centering/tween APIs, full page/Main lifecycle and all external services remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("dynamic-list-lazy-slots-native-scroll-reuse-and-visible-refresh",()=>{
                using(var f=new Fixture()){
                    Item.Trace=new List<string>();f.Init(40);Require(f.List.SlotCount==8&&f.List.Columns==2,"viewport capacity plus two rows");
                    Require(f.List.transform.childCount==8&&f.List.ShowMinIdx==-1&&f.List.IsDirty,"native slots prepared but rendering deferred");
                    f.ScrollTo(0);Require(f.List.ShowMinIdx==0&&f.List.ShowMaxIdx==3,"strict overlap excludes touching edge");
                    var slot=f.List.GetItem(0);var root=slot.Root;var item=(Item)slot.BaseItem;Require(item.Value==100&&root.activeSelf,"first data bound");
                    f.Data.Data[0]=999;f.Data.UpdateItemData(0);Require(item.Value==999&&item.RenderCount==2,"provider forwards immediate selected refresh");
                    f.Data.UpdateItemData(30);Require(f.List.GetItem(30)==null&&f.List.transform.childCount==8,"offscreen update creates no native object");
                    f.ScrollTo(50);Require(f.List.ShowMinIdx==4&&f.List.ShowMaxIdx==7&&f.List.GetItem(0)==null,"scroll changes visible interval");
                    Require(f.List.GetItem(4)==slot&&slot.Root==root&&item.Index==4&&item.Value==104,"same slot/root rebound to new row");
                    Require(!Item.Trace.Exists(x=>x.StartsWith("hidden:")),"OnHidden is disposal notification, not scroll notification");
                    int rendered=item.RenderCount;f.List.RefreshSoft();Require(item.RenderCount==rendered+1,"soft refresh only bound regions");
                }
            });
            check("dynamic-list-source-spacing-alignment-horizontal-and-column-shrink",()=>{
                using(var f=new Fixture(150,50,50,25)){
                    f.List.ColumnAlignment=1;f.List.SpacingSize=new Vector2(10,5);f.List.SetSpaceList(new[]{3f,7f,11f});f.Init(8);
                    Require(f.List.Columns==2&&f.List.Regions[0].X==20&&f.List.Regions[0].Y==-28&&f.List.Regions[2].Y==-65,"even center and inclusive cumulative spaces");
                    Require(((RectTransform)f.List.transform).sizeDelta.y==141,"trailing line includes space list prefix");
                    f.Data.Data.RemoveRange(1,7);f.Data.UpdateList();Require(f.List.Columns==1&&f.List.Regions[0].X==50,"small data shrinks persistent column count");
                    f.Data.Data.AddRange(new[]{2,3,4});f.Data.UpdateList();Require(f.List.Columns==1,"AutoMask false retains reduced columns");
                    f.List.AutoMask=true;f.Data.UpdateList();Require(f.List.Columns==2,"AutoMask reruns adaptation");
                    f.List.ColumnAlignment=2;f.Data.UpdateList();Require(f.List.Regions[0].X==20,"source right alignment includes extra spacing subtraction");
                    f.List.Inverse=true;f.Data.UpdateList();Require(f.List.Regions[0].Y==3&&f.List.Regions[2].Y==40,"inverse rows use positive space");
                }
                using(var f=new Fixture(100,100,50,25)){
                    f.List.Direction=1;f.List.SpacingSize=new Vector2(3,5);f.Init(12);Tick(f.List);
                    Require(f.List.Columns==3&&f.List.Regions[0].X==50&&f.List.Regions[1].Y==30&&f.List.Regions[3].X==103,"horizontal region coordinates");
                    Require(((RectTransform)f.List.GetItem(0).Root.transform).pivot==Vector2.one&&f.List.Scroll.horizontal&&!f.List.Scroll.vertical,"horizontal pivot and scroll axis");
                }
            });
            check("dynamic-list-empty-provider-force-refresh-and-source-sentinels",()=>{
                using(var f=new Fixture()){
                    f.List.RefreshSoft();f.Init(40);f.ScrollTo(0);var root=f.List.GetItem(0).Root;
                    f.Data.Data.Clear();f.List.ForceRefreshDataProvider();Require(root.activeSelf&&!f.List.IsDirty,"empty force refresh is no-op");
                    f.Data.UpdateList();Tick(f.List);Require(!root.activeSelf&&f.List.ShowMinIdx==-1&&f.List.ShowMaxIdx==-1,"explicit update clears and hides empty list");
                    f.Data.Data.Add(7);f.Data.UpdateList();f.ScrollTo(500);Require(f.List.ShowMinIdx==int.MaxValue&&f.List.ShowMaxIdx==-1,"nonempty entirely offscreen preserves distinct min sentinel");
                    f.Data.UpdateItemData(-1);f.Data.UpdateItemData(100);Require(f.List.GetItem(-1)==null&&f.List.GetItem(100)==null,"invalid requested row does not render");
                }
            });
            check("dynamic-list-dispose-notifications-before-reverse-pool-recycle",()=>{
                using(var f=new Fixture()){
                    f.Init(40);f.ScrollTo(0);var roots=new List<GameObject>();foreach(var slot in f.List.Renderers)roots.Add(slot.Root);
                    Item.Trace=new List<string>();f.List.Dispose();Require(Item.Trace.Count==12&&Item.Trace[3]=="hidden:3"&&Item.Trace[4]=="dispose:0","all hidden callbacks before any item disposal");
                    Require(f.Pool.Root.transform.childCount==8&&f.Pool.Root.transform.GetChild(0).gameObject==roots[7],"recycle in reverse slot order");
                    foreach(var root in roots)Require(root!=null&&!root.activeSelf&&root.transform.parent==f.Pool.Root.transform,"pool owns surviving native objects");
                    Require(f.List.Renderers[0].Region!=null&&f.List.Renderers[0].BaseItem!=null&&f.List.IsInitialized,"source does not clear region/item/initialized state");
                }
            });
            check("dynamic-list-render-failure-and-rebinding-preserve-partial-state",()=>{
                using(var f=new Fixture()){
                    f.Init(40);((Item)f.List.Renderers[0].BaseItem).FailRender=true;
                    Throws<InvalidOperationException>(()=>Tick(f.List));Require(!f.List.IsDirty&&f.List.GetItem(0).Root.activeSelf&&!f.List.GetItem(1).Root.activeSelf,"dirty cleared before render, later pending slots not shown on failure");
                    ((Item)f.List.Renderers[0].BaseItem).FailRender=false;Tick(f.List);Require(!f.List.GetItem(1).Root.activeSelf,"no automatic retry introduced");
                    f.Data.UpdateList();Tick(f.List);Require(f.List.GetItem(1).Root.activeSelf,"explicit data refresh rebuilds pending bindings");
                    var replacement=new OutgameDynamicListProvider<int>();replacement.Data.Add(33);int created=0;f.List.InitRendererList(replacement,()=>{created++;return new Item();});
                    Require(created==0&&replacement.DynamicList==f.List,"second init rebinds provider only");
                    f.Data.Data[0]=444;replacement.UpdateItemData(0);Require(((Item)f.List.GetItem(0).BaseItem).Value==444,"original item retains original provider reference");
                }
            });
            check("dynamic-list-normal-mode-grow-and-keep-bindings-source-behavior",()=>{
                using(var f=new Fixture()){
                    f.List.IsNormalList=true;f.Init(20);Tick(f.List);Require(f.List.SlotCount==20&&f.List.ShowMaxIdx==19&&f.List.transform.childCount==20,"generic normal update grows slots and shows all rows");
                    var slot=f.List.GetItem(0);int calls=((Item)slot.BaseItem).RenderCount;var region=slot.Region;
                    f.Data.Data[0]=777;f.List.UpdateList(f.Data,true);Tick(f.List);Require(slot.Region==region&&((Item)slot.BaseItem).RenderCount==calls,"keepBindings leaves original region and does not re-render");
                    f.Data.UpdateItemData(0);Require(((Item)slot.BaseItem).Value==777,"explicit single refresh reads current value");
                    var visible=new List<IOutgameDynamicItem>{null};f.List.GetVisibleItems(visible);Require(visible.Count==20&&visible[0]==slot.BaseItem,"visible query clears output then uses slot order");
                }
            });
            check("guide-book-original-dynamic-rows-popup-and-pool-reopen",()=>{
                using(var f=new OutgameGuideBookPopupValidation.Fixture()){
                    var registry=new OutgameControllerRegistry();var pool=new OutgamePrefabPoolControl(registry,s=>{throw new Exception(s);},s=>{throw new Exception(s);},g=>{},g=>UnityEngine.Object.DestroyImmediate(g));
                    registry.Bind(4561,()=>pool);registry.Resolve(4561);pool.OnInit();
                    try{
                        var page=f.Popup.transform;var viewport=(RectTransform)page.Find("guideSV/Viewport");viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,660);viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,300);
                        var list=new OutgameGuideBookListBinding(page,Resources.Load<GameObject>("Recovered/GuideBook/GuideBookItem"),()=>pool,f.ItemServices);f.Rewards.RefreshItem=list.Data.UpdateItemData;Tick(list.List);
                        Require(list.Data.Data.Count==f.Rewards.Config.dicGuidebook.Count,"original dictionary rows all included");
                        var first=(OutgameGuideBookDynamicItem)list.List.GetItem(0).BaseItem;first.Item.UnlockButton.onClick.Invoke();Require(f.Rewards.Binding.SelectedIndex==0&&f.Rewards.Binding.SelectedBook==list.Data.Data[0].Config,"real row opens original popup");
                        f.Rewards.Button.onClick.Invoke();Require(!first.Item.UnlockButton.transform.Find("imgRed").gameObject.activeSelf,"real saved claim refreshes visible red dot through provider");
                        var initial=list.List.GetItem(0).Root;((RectTransform)list.List.transform).anchoredPosition=new Vector2(0,300);list.List.Scroll.onValueChanged.Invoke(Vector2.zero);Tick(list.List);
                        int index=list.List.ShowMinIdx;Require(index>0,"actual recovered row dimensions support scrolling");
                        var row=(OutgameGuideBookDynamicItem)list.List.GetItem(index).BaseItem;row.Item.UnlockButton.onClick.Invoke();Require(f.Rewards.Binding.SelectedIndex==index&&f.Rewards.Binding.SelectedBook==list.Data.Data[index].Config,"recycled button uses newly rendered index and config");
                        list.Data.Data=new List<OutgameGuideBookRow>(list.Data.Data);list.Data.Data[index]=new OutgameGuideBookRow{Config=f.Rewards.Config.dicGuidebook[2]};list.Data.UpdateItemData(index);
                        row.Item.UnlockButton.onClick.Invoke();Require(f.Rewards.Binding.SelectedBook==f.Rewards.Config.dicGuidebook[2],"item retains provider and observes replacement public data list");
                        list.List.Dispose();Require(initial!=null&&initial.transform.parent==pool.Root.transform,"source page disposal returns root to existing pool");
                    }finally{UnityEngine.Object.DestroyImmediate(f.Popup.transform.Find("guideSV/Viewport/dynamicList").GetComponent<OutgameDynamicList>());pool.OnDispose();}
                }
            });
            return report;
        }
        public static void Validate()
        {
            var r=Run();var path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../analysis/dynamic-list-validation.json"));
            System.IO.File.WriteAllText(path,JsonUtility.ToJson(r,true));Debug.Log("AREABATTLE_DYNAMIC_LIST_"+(r.passed?"PASS":"FAIL")+" cases="+r.checks.Count);EditorApplication.Exit(r.passed?0:1);
        }
    }
}
