using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameUiControlValidation
    {
        sealed class MenuPage:OutgameUiPage,IOutgameMenuTabPage
        {public int Closed;public MenuPage(GameObject root,Func<OutgameMessageDispatcher> bus):base(root,bus){}public void DoClose(){Closed++;SetVisible(false);}}
        [Serializable] sealed class Import {public Prefab[] prefabs;}
        [Serializable] sealed class Prefab {public Node[] nodes;public Binding[] bindings;}
        [Serializable] sealed class Node {public string path,name;public Rect transform;}
        [Serializable] sealed class Rect {public Vector2 m_AnchorMin,m_AnchorMax,m_SizeDelta,m_AnchoredPosition,m_Pivot;}
        [Serializable] sealed class Binding {public string name,path;}
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        static OutgameUiModuleInitialization Module(GameObject canvas,GameObject root)
        {
            return new OutgameUiModuleInitialization(()=>true,(path,loaded)=>loaded((name,active)=>{root.SetActive(active);return root;}),null,new OutgameUiDisplaySettings(),s=>{},s=>{throw new Exception(s);},tag=>canvas,g=>{});
        }
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("source-top-info-original-layout-and-outlets",()=>{
                var prefab=Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI");Require(prefab!=null,"imported original prefab");
                var data=JsonUtility.FromJson<Import>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/top-info-ui-import.json"))).prefabs[0];
                Require(data.nodes.Length==36&&data.bindings.Length==25,"source inventory");
                foreach(var node in data.nodes){var t=(RectTransform)(node.path==""?prefab.transform:prefab.transform.Find(node.path));Require(t&&t.name==node.name&&t.anchorMin==node.transform.m_AnchorMin&&t.anchorMax==node.transform.m_AnchorMax&&t.sizeDelta==node.transform.m_SizeDelta&&t.anchoredPosition==node.transform.m_AnchoredPosition&&t.pivot==node.transform.m_Pivot,"source node geometry: "+node.path);}
                foreach(var binding in data.bindings)Require(prefab.transform.Find(binding.path)!=null,"source outlet "+binding.name);
            });
            test("source-top-info-bitmask-native-canvas-and-preopen-guard",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"));
                try{
                    var page=OutgameTopInfoPage.FromOriginal(root,()=>new OutgameMessageDispatcher());page.SetVisible(false);page.RendererPart(7,"UIMessage");Require(!page.Visible,"unopened dictionary returns before visibility change");page.OpenParts();
                    var gold=root.transform.Find("objTopInfo/goldInfo").gameObject;var diamond=root.transform.Find("objTopInfo/diamondInfo").gameObject;var stamina=root.transform.Find("objTopInfo/spInfo").gameObject;stamina.SetActive(false);
                    page.RendererPart(2,"UIMessage");Require(page.Visible&&gold.activeSelf&&!diamond.activeSelf&&!stamina.activeSelf,"bit2 selects gold; dictionary5 not visited");
                    Require(gold.GetComponent<Canvas>().sortingLayerName=="UIMessage"&&gold.GetComponent<Canvas>().sortingOrder==9&&gold.GetComponent<GraphicRaycaster>().enabled,"selected part external layer and raycaster");
                    Require(diamond.GetComponent<Canvas>().sortingLayerName=="UIPopup"&&diamond.GetComponent<Canvas>().sortingOrder==9&&!diamond.GetComponent<GraphicRaycaster>().enabled,"unselected part default layer but external-order9");
                    page.RendererPart(-1,"");Require(gold.activeSelf&&diamond.activeSelf&&gold.GetComponent<Canvas>().sortingOrder==0,"arithmetic mask shift and empty layer order0");
                    page.RendererPart(0,null);Require(!gold.activeSelf&&!diamond.activeSelf&&gold.GetComponent<Canvas>().enabled,"zero mask keeps canvases enabled while deactivating groups");
                    Require(OutgameUiLayerNames.Get(6)=="UIMessage"&&OutgameUiLayerNames.Get(7)==null,"original enum names");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            });
            test("source-ui-control-init-shared-fields-playstate-and-disposal",()=>{
                var canvas=new GameObject("canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var camera=new GameObject("UICamera",typeof(Camera));camera.transform.SetParent(canvas.transform);
                var root=new GameObject("UIRoot",typeof(RectTransform));((RectTransform)canvas.transform).sizeDelta=new Vector2(900,1600);
                var topRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"));var menuRoot=new GameObject("menu");var itemRoot=new GameObject("item");
                try{
                    var module=Module(canvas,root);module.Initialize();var globals=new OutgameUiControlGlobals();var registry=new OutgameControllerRegistry();var messages=new OutgameMessageDispatcher();int moduleLookups=0,showTop=0,roles=0;
                    var page=OutgameTopInfoPage.FromOriginal(topRoot,()=>messages);page.OpenParts();var menu=new MenuPage(menuRoot,()=>messages);var item=new OutgameUiPage(itemRoot,()=>messages);
                    var items=new OutgameMenuItems((name,visible)=>item,()=>item,frame=>Task.CompletedTask){Main=item,Shop=item,Commander=item,ItemInfo=item};
                    OutgameCoreControllerBindings.BindUi(registry,()=>{moduleLookups++;return module;},()=>messages,globals,items,()=>{showTop++;return page;},()=>menu,()=>menu,(a,b)=>{Require(a==0,"source role origin0");roles++;});
                    var control=(OutgameUiControl)registry.Resolve(4296);control.CurrentPage=4;control.ActiveUpdate=true;control.OnInit();Require(moduleLookups==3&&globals.UiRoot==module.UiRoot&&globals.UiCamera==module.UiCamera&&control.UiWidth==module.UiRoot.rect.width&&control.CurrentPage==0&&control.ActiveUpdate,"three live module reads and exact resets");
                    messages.SendMessage("GamePlayState",new object[]{2});Require(showTop==1&&ReferenceEquals(control.TopInfo,page)&&ReferenceEquals(control.MenuTab,menu)&&roles==1&&control.CurrentPage==0,"first home display preserves current-page0");
                    messages.SendMessage("GamePlayState",new object[]{3});Require(menu.Closed==1&&!page.Visible,"battle start closes queried menu then hides cached header");
                    messages.SendMessage("GamePlayState",new object[]{8});Require(page.Visible&&topRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingOrder==9,"result moves source mask6 to popup ordering");
                    messages.SendMessage("GamePlayState",new object[]{11});Require(showTop==1&&topRoot.transform.Find("objTopInfo/goldInfo").GetComponent<Canvas>().sortingOrder==0&&roles==1,"state11 restores top alone");
                    messages.SendMessage("GamePlayState",new object[]{2});Require(control.CurrentPage==3&&roles==2,"cached menu branch opens items then sets current3");
                    control.OnDispose();messages.SendMessage("GamePlayState",new object[]{2});Require(!registry.HasInstance(4296)&&control.TopInfo==null&&ReferenceEquals(control.MenuTab,menu)&&globals.UiRoot==module.UiRoot&&roles==2&&!control.ActiveUpdate,"source clear order retains menu/global roots but removes listener");
                }finally{UnityEngine.Object.DestroyImmediate(canvas);if(root)UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(topRoot);UnityEngine.Object.DestroyImmediate(menuRoot);UnityEngine.Object.DestroyImmediate(itemRoot);}
            });
            test("source-ui-control-init-failure-and-current-singleton-reset",()=>{
                var canvas=new GameObject("canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var camera=new GameObject("UICamera",typeof(Camera));camera.transform.SetParent(canvas.transform);var root=new GameObject("UIRoot",typeof(RectTransform));
                try{
                    var module=Module(canvas,root);module.Initialize();var unready=Module(canvas,null);var globals=new OutgameUiControlGlobals();var registry=new OutgameControllerRegistry();var messages=new OutgameMessageDispatcher();int reads=0;bool failing=true;
                    registry.Bind(4296,()=>new OutgameUiControl(()=>{reads++;return failing&&reads==3?unready:module;},registry,()=>messages,globals,null,null,null,null,null));
                    var first=(OutgameUiControl)registry.Resolve(4296);first.CurrentPage=4;bool failed=false;try{first.OnInit();}catch(NullReferenceException){failed=true;}Require(failed&&first.CurrentPage==4&&first.UiWidth==1080&&globals.UiRoot==module.UiRoot&&globals.UiCamera==module.UiCamera,"third module failure preserves prior global assignments and default width");
                    failing=false;first.OnDispose();var replacement=(OutgameUiControl)registry.Resolve(4296);replacement.CurrentPage=3;first.OnInit();Require(replacement.CurrentPage==0&&first.CurrentPage==4,"old instance init resets current singleton rather than itself");
                    first.OnDispose();Require(!registry.HasInstance(4296),"old disposal resets current slot");
                }finally{UnityEngine.Object.DestroyImmediate(canvas);if(root)UnityEngine.Object.DestroyImmediate(root);}
            });
            test("source-native-menutab-visibility-close-message-order",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/MenuTabUI"));var child=new GameObject("child");
                try{
                    var messages=new OutgameMessageDispatcher();var registry=new OutgameControllerRegistry();var control=new OutgameUiControl(null,registry,()=>messages,new OutgameUiControlGlobals(),null,null,null,null,null);control.CurrentPage=4;
                    var order=new List<string>();var item=new OutgameUiPage(child,()=>messages);
                    var items=new OutgameMenuItems((name,visible)=>throw new Exception("Unexpected missing native child"),()=>throw new Exception("Unexpected missing item-info")){Main=item,Shop=item,Commander=item,ItemInfo=item};
                    var page=new OutgameMenuTabPage(root,()=>control,items,()=>messages,()=>order.Add("main"),()=>order.Add("start"),()=>order.Add("commander"),()=>order.Add("level"),()=>order.Add("skins"));
                    messages.AddListener("MenuTabDispose",args=>{Require(args==null&&page.Visible&&control.CurrentPage==0&&root.transform.localScale==Vector3.zero,"dispose observes object hidden before Visible commit");order.Add("dispose");});
                    messages.AddListener("GF_VisibleUI",args=>{if(ReferenceEquals(args[0],page)){Require(page.Visible==(bool)args[1],"global visibility follows commit");order.Add("visible:"+args[1]);}});
                    page.SetVisible(true);Require(string.Join(",",order)=="main,start,commander,level,skins,visible:True"&&child.transform.localScale==Vector3.one,"native show order and child activation");
                    order.Clear();page.DoClose();Require(string.Join(",",order)=="main,dispose,visible:False"&&!page.Visible&&root.activeSelf&&child.transform.localScale==Vector3.zero,"close hides children and preserves root active");
                    order.Clear();page.DoClose();Require(order.Count==0,"already-hidden guard suppresses all work");
                }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(child);}
            });
            test("source-native-menutab-refresh-failure-partial-visibility",()=>{
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/MenuTabUI"));var child=new GameObject("child");
                try{
                    var messages=new OutgameMessageDispatcher();var registry=new OutgameControllerRegistry();var control=new OutgameUiControl(null,registry,()=>messages,new OutgameUiControlGlobals(),null,null,null,null,null);
                    var item=new OutgameUiPage(child,()=>messages);var items=new OutgameMenuItems(null,null){Main=item,Shop=item,Commander=item,ItemInfo=item};bool fail=true;int later=0,commits=0;
                    var page=new OutgameMenuTabPage(root,()=>control,items,()=>messages,()=>{},()=>{if(fail)throw new InvalidOperationException("source refresh failure");},()=>later++,()=>later++,()=>later++);
                    page.SetVisible(false);messages.AddListener("GF_VisibleUI",args=>{if(ReferenceEquals(args[0],page))commits++;});
                    bool caught=false;try{page.SetVisible(true);}catch(InvalidOperationException){caught=true;}
                    Require(caught&&!page.Visible&&root.transform.localScale==Vector3.one&&later==0&&commits==0,"native base visibility precedes failed callback, logical flag/message not committed");
                    fail=false;page.SetVisible(true);Require(page.Visible&&later==3&&commits==1,"subsequent call can finish normally");
                }finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(child);}
            });
            return report;
        }
    }
}
