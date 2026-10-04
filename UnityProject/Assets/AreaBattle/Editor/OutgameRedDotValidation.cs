using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameRedDotValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameControllerRegistry Registry=new OutgameControllerRegistry();
            public readonly List<string> Logs=new List<string>();
            public readonly List<GameObject> Roots=new List<GameObject>();
            public readonly OutgameRedDotControl Control;
            public bool FailLog;
            public Fixture(bool initialize=true)
            {
                OutgameCoreControllerBindings.BindRedDots(Registry,(color,args)=>{Require(color==Color.green&&args.Length==1,"source log color/array");Logs.Add((string)args[0]);if(FailLog)throw new InvalidOperationException("explicit log failure");});
                Control=(OutgameRedDotControl)Registry.Resolve(4453);if(initialize)Control.OnInit();
            }
            public OutgameRedDotItem NewItem(bool withEvent=true)
            {
                var root=new GameObject("RedDot"+Roots.Count,typeof(RectTransform));Roots.Add(root);
                var item=root.AddComponent<OutgameRedDotItem>();item.BindController(()=> (OutgameRedDotControl)Registry.Resolve(4453));
                item.DotPrefab=new GameObject("dot");item.DotPrefab.transform.SetParent(root.transform,false);
                if(withEvent)item.CheckActionBool=new OutgameRedDotEvent();return item;
            }
            public void Dispose(){FailLog=false;foreach(var root in Roots)if(root)UnityEngine.Object.DestroyImmediate(root);}
        }
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Project RedDotControl4453/item4457 with original MenuTabUI serialized references. Generic framework RedDotModule3655, inactive obfuscated aliases, missing persistent target and full Main/activity/Player composition remain separate."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("red-dot-source-unscaled-period-and-mode-boundaries",()=>{using(var f=new Fixture()){
                var item=f.NewItem();int calls=0;item.CheckActionBool.AddListener(x=>{calls++;x.IsShowRedDot=true;});f.Control.AddItem(item);
                f.Control.Updata(100,10);Require(f.Control.Elapsed==0&&calls==0,"inactive skips both time and items");f.Control.InitRedDot(OutgameRedDotDetectType.PerSecond);
                f.Control.Updata(100,.5f);f.Control.Updata(0,.5f);Require(calls==1&&f.Control.Elapsed==0&&item.DotPrefab.activeSelf,"unscaled inclusive1 threshold");
                f.Control.Updata(0,3.5f);Require(calls==2&&f.Control.Elapsed==0,"one refresh per call and discard overshoot");
                f.Control.InitRedDot(OutgameRedDotDetectType.Update);f.Control.Updata(0,.25f);f.Control.Updata(0,.25f);Require(calls==4&&f.Control.Elapsed==.5f,"every update retains accumulated time");
                f.Control.InitRedDot(OutgameRedDotDetectType.Message);f.Control.Updata(0,3);Require(calls==4&&f.Control.Elapsed==3.5f,"message mode accumulates without automatic refresh");
                f.Control.InitRedDot((OutgameRedDotDetectType)(-1));f.Control.Updata(0,.5f);Require(calls==4&&f.Control.Elapsed==4,"unknown mode matches default");
                f.Control.InitRedDot(OutgameRedDotDetectType.PerSecond);f.Control.Updata(0,0);Require(calls==5&&f.Control.Elapsed==0,"mode change does not reset prior time");
            }});
            check("red-dot-native-component-check-before-show-and-live-listeners",()=>{using(var f=new Fixture()){
                var item=f.NewItem();var parameter=new object();item.InitRedDot(parameter);item.IsShowRedDot=true;item.DotPrefab.SetActive(false);var trace=new List<string>();
                item.CheckActionBool.AddListener(x=>{Require(!x.IsShowRedDot&&!x.DotPrefab.activeSelf&&ReferenceEquals(x.DotRedParam,parameter),"check resets only flag before event");trace.Add("one");x.IsShowRedDot=true;});
                item.CheckActionBool.AddListener(x=>{Require(x.IsShowRedDot,"second sees first listener flag");trace.Add("two");});
                f.Control.AddItem(item);f.Control.InitRedDot(OutgameRedDotDetectType.Update);f.Control.Updata(0,0);
                Require(item.DotPrefab.activeSelf&&string.Join("|",trace)=="one|two","show follows complete event invocation");
                item.CheckActionBool=null;f.Control.Updata(0,0);Require(!item.IsShowRedDot&&!item.DotPrefab.activeSelf&&ReferenceEquals(parameter,item.DotRedParam),"absent event resets flag and hides, preserving parameter");
                item.IsShowRedDot=true;Require(!item.DotPrefab.activeSelf,"property setter does not perform Show");
            }});
            check("red-dot-list-duplicates-remove-one-and-log-failure-order",()=>{using(var f=new Fixture()){
                var item=f.NewItem();f.Control.AddItem(item);f.Control.AddItem(item);Require(f.Control.Items.Count==2,"source Add does not deduplicate");
                f.Control.RemoveItem(item);Require(f.Control.Items.Count==1,"Contains then Remove only first");f.Control.RemoveItem(item);f.Control.RemoveItem(item);Require(f.Logs.Count==5,"absent item still logs");
                f.FailLog=true;Throws<InvalidOperationException>(()=>f.Control.AddItem(item));Require(f.Control.Items.Count==1,"list mutation precedes log failure");Throws<InvalidOperationException>(()=>f.Control.RemoveItem(item));Require(f.Control.Items.Count==0,"removal precedes log failure");
                f.FailLog=false;Throws<NullReferenceException>(()=>f.Control.AddItem(null));Require(f.Control.Items.Count==1&&f.Control.Items[0]==null,"null entry added before reading gameObject for log");
                Throws<NullReferenceException>(()=>f.Control.RemoveItem(null));Require(f.Control.Items.Count==0,"null removed before log dereference");
            }});
            check("red-dot-exception-and-enumeration-mutation-source-order",()=>{using(var f=new Fixture()){
                var first=f.NewItem();var next=f.NewItem();int later=0;first.DotPrefab.SetActive(false);next.CheckActionBool.AddListener(x=>later++);
                first.CheckActionBool.AddListener(x=>{x.IsShowRedDot=true;throw new InvalidOperationException("explicit event failure");});f.Control.AddItem(first);f.Control.AddItem(next);f.Control.InitRedDot(OutgameRedDotDetectType.PerSecond);
                Throws<InvalidOperationException>(()=>f.Control.Updata(0,1));Require(first.IsShowRedDot&&!first.DotPrefab.activeSelf&&later==0&&f.Control.Elapsed==0,"failure after timer reset skips show and remaining items");
                first.CheckActionBool.RemoveAllListeners();first.CheckActionBool.AddListener(x=>{x.IsShowRedDot=true;f.Control.RemoveItem(next);});
                Throws<InvalidOperationException>(()=>f.Control.Updata(0,1));Require(first.DotPrefab.activeSelf&&later==0&&f.Control.Items.Count==1,"current item still shown before enumerator detects list mutation");
            }});
            check("red-dot-start-null-event-inactive-and-registration-retention",()=>{using(var f=new Fixture()){
                var item=f.NewItem(false);item.SendMessage("Start");Require(f.Control.Items.Count==0,"null event suppresses Start registration");
                item.CheckActionBool=new OutgameRedDotEvent();item.SendMessage("Start");item.SendMessage("Start");Require(f.Control.Items.Count==2,"explicit repeated source Start retains duplicates");
                int checks=0;item.CheckActionBool.AddListener(x=>checks++);item.gameObject.SetActive(false);item.enabled=false;
                f.Control.InitRedDot(OutgameRedDotDetectType.Update);f.Control.Updata(0,0);Require(checks==2&&f.Control.Items.Count==2,"source checks registered items even if inactive or disabled");
                typeof(OutgameRedDotItem).GetMethod("OnDestroy",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(item,null);Require(f.Control.Items.Count==1,"one removal per destroy callback");
            }});
            check("red-dot-init-dispose-and-old-singleton-slot",()=>{using(var f=new Fixture()){
                var item=f.NewItem();f.Control.AddItem(item);var oldItems=f.Control.Items;f.Control.InitRedDot(OutgameRedDotDetectType.Message);f.Control.Updata(0,.75f);f.Control.OnInit();
                Require(!f.Control.ActiveUpdate&&f.Control.DetectType==OutgameRedDotDetectType.Message&&f.Control.Elapsed==.75f&&!ReferenceEquals(oldItems,f.Control.Items)&&oldItems.Count==1,"init resets active/list only");
                f.Control.AddItem(item);f.Control.InitRedDot(OutgameRedDotDetectType.Update);f.Control.OnDispose();Require(!f.Registry.HasInstance(4453)&&f.Control.Items.Count==1&&f.Control.ActiveUpdate&&f.Control.Elapsed==.75f,"dispose only clears singleton");
                var next=f.Registry.Resolve(4453);f.Control.OnDispose();Require(!ReferenceEquals(next,f.Control)&&!f.Registry.HasInstance(4453),"old instance clears new singleton");
            }});
            check("red-dot-uninitialized-null-list-and-nan-time",()=>{using(var f=new Fixture(false)){
                f.Control.AddItem(null);f.Control.RemoveItem(null);Require(f.Logs.Count==0,"uninitialized list suppresses add/remove entirely");f.Control.InitRedDot(OutgameRedDotDetectType.PerSecond);
                Throws<NullReferenceException>(()=>f.Control.Updata(0,1));Require(f.Control.Elapsed==0,"timer reset before missing-list failure");f.Control.OnInit();f.Control.InitRedDot(OutgameRedDotDetectType.PerSecond);f.Control.Updata(0,float.NaN);Require(float.IsNaN(f.Control.Elapsed),"NaN fails inclusive threshold");f.Control.Updata(0,1);Require(float.IsNaN(f.Control.Elapsed),"NaN timer not sanitized");
            }});
            check("red-dot-original-menu-component-and-null-persistent-target",()=>{using(var f=new Fixture()){
                var root=new GameObject("real-menu-owner",typeof(RectTransform));f.Roots.Add(root);var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(1080,1920);
                var view=root.AddComponent<OutgameMenuView>();var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                view.Initialize(rect,5,rules,redDots:()=>f.Control);var item=view.Menu.GetComponentInChildren<OutgameRedDotItem>(true);item.gameObject.SetActive(true);
                Require(item.CheckActionBool.GetPersistentEventCount()==1&&item.CheckActionBool.GetPersistentTarget(0)==null&&item.CheckActionBool.GetPersistentMethodName(0)=="Tower_CheckReddot","preserve serialized missing-target call metadata");
                Require(item.DotPrefab.name=="imgCommanderDot3"&&item.Scale==Vector2.one&&item.PosOffset==Vector2.zero&&item.RectAnchorType==OutgameRedDotAnchorType.RightTop&&item.CheckActionBool!=null,"original serialized references/defaults restored");
                item.SendMessage("Start");item.IsShowRedDot=true;item.DotPrefab.SetActive(true);f.Control.InitRedDot(OutgameRedDotDetectType.Update);f.Control.Updata(0,0);
                Require(!item.IsShowRedDot&&!item.DotPrefab.activeSelf&&f.Control.Items.Count==1,"missing original Tower_CheckReddot target does not fabricate a predicate");
            }});
            return report;
        }
    }
}
