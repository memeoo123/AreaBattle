using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameLimitTaskUiItemsValidation
    {
        [Serializable] sealed class Manifest {public Page[] prefabs;}
        [Serializable] sealed class Page {public string name;public Node[] nodes;}
        [Serializable] sealed class Node {public string path,name;public bool active;public RectData transform;public Part[] components;}
        [Serializable] sealed class RectData {public Vector2 m_AnchorMin,m_AnchorMax,m_AnchoredPosition,m_SizeDelta,m_Pivot;}
        [Serializable] sealed class Part {public string className;}
        public sealed class Fixture:IDisposable
        {
            public readonly GameObject Root,RewardRoot,ProgressRoot;
            public readonly OutgameLegacyConfigManager Config;
            public readonly OutgameLimitTaskRewardItemServices Services;
            public readonly OutgameLimitTaskRewardItem Reward;
            public readonly OutgameLimitTaskProgressItem Progress;
            public readonly List<string> Trace=new List<string>();
            public Action SpriteCallback;public Action<RectTransform,int> PopupCallback;
            public string IconName,AtlasName;public bool NativeSize;
            public Fixture(bool native=false)
            {
                Root=new GameObject("LimitTaskItemValidation",typeof(RectTransform));
                RewardRoot=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskRewardItem"),Root.transform,false);
                var task=Resources.Load<GameObject>("Recovered/LimitTask/CommonLimitTimeTaskItem");
                ProgressRoot=UnityEngine.Object.Instantiate(task.transform.Find("taskProgressItem").gameObject,Root.transform,false);
                RewardRoot.SetActive(true);ProgressRoot.SetActive(true);
                Config=new OutgameLegacyConfigManager(new OutgameLegacyConfigReadState(s=>{}),new OutgameConfigGlobalValues(),s=>null,()=>9,()=>200,()=>null,(s,a,o)=>{});
                new OutgameLegacyConfigRead(n=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+n),s=>{throw new Exception(s);}).ReadTable(Config.dicGameItem);
                Services=new OutgameLimitTaskRewardItemServices{
                    Config=()=>Config,
                    SetSprite=(image,icon,atlas,nativeSize)=>{Trace.Add("sprite");IconName=icon;AtlasName=atlas;NativeSize=nativeSize;SpriteCallback?.Invoke();},
                    PopItemInfo=(rect,id)=>{Trace.Add("popup:"+id);PopupCallback?.Invoke(rect,id);}
                };
                Action<GameObject> destroy=native?(Action<GameObject>)(go=>UnityEngine.Object.Destroy(go)):(go=>UnityEngine.Object.DestroyImmediate(go));
                Reward=new OutgameLimitTaskRewardItem(RewardRoot,Services,destroy);Progress=new OutgameLimitTaskProgressItem(ProgressRoot,destroy);
            }
            public void Dispose(){if(Application.isPlaying)UnityEngine.Object.Destroy(Root);else UnityEngine.Object.DestroyImmediate(Root);}
        }
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static void Click(GameObject root)=>Require(ExecuteEvents.Execute(root,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler),"actual pointer handler received event");
        static void CheckAsset(Page page)
        {
            var prefab=Resources.Load<GameObject>("Recovered/LimitTask/"+page.name);Require(prefab,"original prefab present");
            var resolved=new Dictionary<string,Transform>();var childIndices=new Dictionary<string,int>();int images=0,texts=0;
            foreach(var node in page.nodes)
            {
                Transform t;
                if(string.IsNullOrEmpty(node.path))t=prefab.transform;
                else
                {
                    int slash=node.path.LastIndexOf('/');string parent=slash<0?"":node.path.Substring(0,slash);
                    childIndices.TryGetValue(parent,out int i);t=resolved[parent].GetChild(i);childIndices[parent]=i+1;
                }
                resolved.Add(node.path,t);Require(t.name==node.name&&t.gameObject.activeSelf==node.active,"source name/order/activation "+node.path);
                var rect=(RectTransform)t;var d=node.transform;
                Require(Vector2.Distance(rect.anchorMin,d.m_AnchorMin)<.01f&&Vector2.Distance(rect.anchorMax,d.m_AnchorMax)<.01f&&Vector2.Distance(rect.pivot,d.m_Pivot)<.01f&&Vector2.Distance(rect.sizeDelta,d.m_SizeDelta)<.01f&&Vector2.Distance(rect.anchoredPosition,d.m_AnchoredPosition)<.01f,"source geometry "+node.path);
                foreach(var c in node.components)
                {
                    if(c.className=="Image"){Require(t.GetComponent<Image>(),"source Image "+node.path);images++;}
                    if(c.className=="Text"){Require(t.GetComponent<Text>()&&t.GetComponent<Text>().font,"source Text font "+node.path);texts++;}
                    if(c.className=="Button")Require(t.GetComponent<Button>(),"source Button "+node.path);
                    if(c.className=="RectMask2D")Require(t.GetComponent<RectMask2D>(),"source mask "+node.path);
                    if(c.className=="ScrollRect")Require(t.GetComponent<ScrollRect>()&&t.GetComponent<ScrollRect>().content&&t.GetComponent<ScrollRect>().viewport,"source scroll references "+node.path);
                }
            }
            Require(images>0&&texts>0,"source imagery and text");
        }
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original four limited-task UGUI prefabs and source progress/reward items. Sprite delivery and item-info popup are observed required host endpoints; full task/day/accumulator/preview/page binding, menu entry and visual/audio acceptance remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            var manifest=JsonUtility.FromJson<Manifest>(Resources.Load<TextAsset>("Recovered/LimitTask/hud-import").text);
            foreach(var page in manifest.prefabs)check("limit-task-ui-original-asset-"+page.name,()=>CheckAsset(page));
            check("limit-task-ui-reward-original-icon-atlas-int64-and-missing-row",()=>{
                using(var f=new Fixture()){
                    var reward=new OutgameItemReward{itemId=1001,itemCount=long.MaxValue};var row=f.Config.dicGameItem[1001];f.Reward.SetData(reward);
                    Require(f.IconName==row.icon&&f.AtlasName==row.atlasName&&!f.NativeSize&&f.Reward.Count.text==long.MaxValue.ToString(),"source icon/atlasName fields and long count, no native-size request");
                    f.Trace.Clear();var missing=new OutgameItemReward{itemId=-999,itemCount=4};f.Reward.SetData(missing);
                    Require(ReferenceEquals(f.Reward.Data,missing)&&f.Reward.Count.text==long.MaxValue.ToString()&&f.Trace.Count==0,"missing row publishes new data and retains old UI");
                    Click(f.RewardRoot);Require(string.Join(",",f.Trace)=="popup:-999","missing item still routes detail endpoint");
                }
            });
            check("limit-task-ui-reward-sprite-reentry-retains-input-count-and-live-selection",()=>{
                using(var f=new Fixture()){
                    var first=new OutgameItemReward{itemId=1001,itemCount=3};var second=new OutgameItemReward{itemId=1002,itemCount=8};
                    f.SpriteCallback=()=>{f.SpriteCallback=null;first.itemCount=11;f.Reward.SetData(second);};f.Reward.SetData(first);
                    Require(ReferenceEquals(f.Reward.Data,second)&&f.Reward.Count.text=="11","outer render reads mutated original argument after reentrant replacement");
                    f.Reward.OnClick=item=>{f.Trace.Add("callback");item.SetData(first);};f.Trace.Clear();
                    f.PopupCallback=(rect,id)=>Require(rect==f.Reward.Lifetime.RectTransform&&id==1001,"popup rereads current reward after callback");Click(f.RewardRoot);
                    Require(string.Join(",",f.Trace)=="callback,sprite,popup:1001","click callback before live detail request");
                }
            });
            check("limit-task-ui-reward-failures-preserve-source-prefix",()=>{
                using(var f=new Fixture()){
                    f.Reward.SetData(new OutgameItemReward{itemId=1001,itemCount=7});var next=new OutgameItemReward{itemId=1002,itemCount=9};
                    f.SpriteCallback=()=>throw new IOException("sprite endpoint failure");Throws<IOException>(()=>f.Reward.SetData(next));
                    Require(ReferenceEquals(f.Reward.Data,next)&&f.Reward.Count.text=="7","sprite exception after publication before count");
                    f.Trace.Clear();f.Reward.OnClick=item=>throw new IOException("click callback failure");
                    // Invoke the native delegate directly so Unity's event dispatcher cannot swallow the expected exception.
                    Throws<IOException>(()=>OutgameUiPointerClick.Get(f.RewardRoot).OnPointerClick(null));Require(f.Trace.Count==0,"callback exception prevents popup");
                    Throws<NullReferenceException>(()=>f.Reward.SetData(null));Require(f.Reward.Data==null&&f.Reward.Count.text=="7","null input published before source dereference");
                }
            });
            check("limit-task-ui-progress-signed-long-upper-cap-and-zero-target",()=>{
                using(var f=new Fixture()){
                    f.Progress.SetData(new OutgameLimitTaskCondition{value=long.MaxValue},long.MaxValue-1);
                    Require(f.Progress.Value.text==string.Format("{0} / {1}",long.MaxValue-1,long.MaxValue-1)&&f.Progress.Progress.fillAmount==1,"long upper cap before formatting/float conversion");
                    f.Progress.SetData(new OutgameLimitTaskCondition{value=-3},10);Require(f.Progress.Value.text=="-3 / 10"&&f.Progress.Progress.fillAmount==0,"no lower cap in text; UGUI clamps image");
                    f.Progress.SetData(new OutgameLimitTaskCondition{value=9},0);Require(f.Progress.Value.text=="0 / 0"&&float.IsNaN(f.Progress.Progress.fillAmount),"source zero denominator reaches native Image");
                    f.Progress.SetData(new OutgameLimitTaskCondition{value=3},-2);Require(f.Progress.Value.text=="-2 / -2"&&f.Progress.Progress.fillAmount==1,"negative target retained");
                }
            });
            check("limit-task-ui-gameobject-pointer-replaces-listener-and-disposal-clears-callback",()=>{
                using(var f=new Fixture()){
                    int first=0,second=0;OutgameUiClick.Add(f.ProgressRoot,()=>first++);OutgameUiClick.Add(f.ProgressRoot,()=>second++);Click(f.ProgressRoot);
                    Require(first==0&&second==1&&f.ProgressRoot.GetComponents<OutgameUiPointerClick>().Length==1,"gameobject overload replaces existing pointer handler");
                    var root=f.RewardRoot;var rect=f.Reward.Lifetime.RectTransform;f.Reward.OnClick=item=>{};f.Reward.Dispose();
                    Require(!root&&f.Reward.Lifetime.IsDisposed&&f.Reward.Lifetime.GameObject==null&&ReferenceEquals(rect,f.Reward.Lifetime.RectTransform)&&f.Reward.OnClick==null,"owned object destroyed; rect field retained; callback cleared");
                }
            });
            check("limit-task-ui-progress-native-pointer-and-destroy-failure-order",()=>{
                using(var f=new Fixture()){
                    int clicks=0;f.Progress.OnClick=item=>{Require(ReferenceEquals(item,f.Progress),"self callback");clicks++;};Click(f.ProgressRoot);Require(clicks==1,"source progress pointer callback");
                    var item=new OutgameLimitTaskProgressItem(f.ProgressRoot,go=>throw new IOException("destroy failure"));item.OnClick=value=>{};
                    Throws<IOException>(()=>item.Dispose());Require(!item.Lifetime.IsDisposed&&item.Lifetime.GameObject==f.ProgressRoot&&item.OnClick!=null,"destroy failure prevents field and callback cleanup");
                }
            });
            return r;
        }
    }
}
