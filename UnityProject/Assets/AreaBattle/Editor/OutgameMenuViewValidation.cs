using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameMenuViewValidation
    {
        sealed class VisibilityProbe:OutgameUiPage
        {
            readonly System.Collections.Generic.List<string> trace;
            public VisibilityProbe(GameObject target,OutgameMessageDispatcher messages,System.Collections.Generic.List<string> trace):base(target,()=>messages){this.trace=trace;}
            protected override void VisibleBefore(bool value){trace.Add("before:"+value+":"+Visible);}
            protected override void VisibleImp(bool value){trace.Add("imp:"+value+":"+Visible);base.VisibleImp(value);}
        }
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Real imported UI button and page transition integration with explicit level fixture. Dynamic content/actions/economy and source easing still pending."};
            var root=new GameObject("Outgame menu integration",typeof(RectTransform));
            try
            {
                var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(1080,1920);
                var view=root.AddComponent<OutgameMenuView>();
                var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                int bound=0;var startTrace=new System.Collections.Generic.List<string>();OutgameMainStartBinding start=null;
                view.Initialize(rect,5,rules,(page,target)=>{
                    Require(!target.gameObject.activeInHierarchy,"page binds before initial activation");bound++;
                    if(page==OutgameMenuPage.Main)start=new OutgameMainStartBinding(target,(state,option)=>startTrace.Add("state:"+state+":"+option),(group,id)=>startTrace.Add("audio:"+group+":"+id));
                });
                Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
                check("outgame-main-source-activity-countdown",()=>{
                    int state=3,reads=0;long now=100000;
                    var time=new OutgameActivityCountdown(()=>state,()=>{reads++;return now;});
                    var text=view.Page(OutgameMenuPage.Main).Find("RigthBar/btn_Easter/Image/textEasterTime").GetComponent<Text>();
                    var language=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));
                    var timer=new OutgameMainActivityTimer(text,time,language.Chinese);
                    time.StartTimeStamp=now+(86400+2*3600+61)*1000L;timer.Refresh();Require(text.text=="1天2小时"&&time.MinTime==1&&time.SecTime==1,"state3 countdown uses start timestamp and original day template");
                    state=4;time.EndTimeStamp=now+(3600+120)*1000L;timer.Refresh();Require(text.text=="1小时2分","state4 countdown uses end timestamp");
                    time.EndTimeStamp=now+59999;timer.Refresh();Require(text.text=="0分59秒","milliseconds truncate before time decomposition");
                    time.EndTimeStamp=now-61000;timer.Refresh();Require(text.text=="-1分-1秒","negative source remaining values are not clamped");
                    state=2;int before=reads;timer.Refresh();Require(text.text=="0分0秒"&&reads==before,"state2 renders zero without reading server clock");
                    state=1;text.text="retained";timer.Refresh();Require(text.text=="retained"&&reads==before,"status below2 leaves existing label untouched");
                });
                check("outgame-main-native-data-refresh-lifecycle",()=>{
                    int status=2,mode=0,timers=0,loaded=0;
                    var held=new OutgameProfile{levelID=0};
                    var progression=new OutgameLevelProgression(held,new OutgameLevelProgressionValidation.Effects());
                    var bank=new OutgamePiggyBank(held,progression,(a,b,c,d)=>{},(a,b,c,d)=>{});
                    var main=view.Page(OutgameMenuPage.Main);var messages=new OutgameMessageDispatcher();
                    messages.AddListener("LoadStartingUI",args=>loaded++);
                    using(var binding=new OutgameMainInfoBinding(view,main,progression,key=>"关卡",bank,()=>status,()=>timers++,()=>mode,()=>messages))
                    {
                        binding.Refresh();
                        Require(main.Find("objNewStartpanel/btnNewStart/TextGuide").gameObject.activeSelf&&main.Find("objStartpanel/btnStart/txt_startLevel").GetComponent<Text>().text=="","tutorial level clears label and selects guide text");
                        Require(!main.Find("RigthBar/pigBank").gameObject.activeSelf&&!main.Find("RigthBar/btn_Easter").gameObject.activeSelf,"zero bank and inactive Easter hidden");
                        progression.CurrentLevel=30;bank.CollectNum=1000;status=3;mode=1;
                        view.RequestPage(OutgameMenuPage.Skins);view.AdvanceTransition(.3f);view.RequestPage(OutgameMenuPage.Main);view.AdvanceTransition(.3f);
                        Require(loaded==2&&timers==1&&main.Find("objStartpanel/btnStart/txt_startLevel").GetComponent<Text>().text=="关卡 30","actual return to main refreshes level and active timer");
                        Require(main.Find("RigthBar/pigBank/btnPiggy_full").gameObject.activeSelf&&!main.Find("RigthBar/pigBank/btnPiggy_normal").gameObject.activeSelf,"full bank icon selected");
                        Require(!main.Find("objNewStartpanel").gameObject.activeSelf&&main.Find("objStartpanel").gameObject.activeSelf,"design mode1 preserves source ordinary panel choice");
                        status=6;progression.CurrentLevel=29;binding.Refresh();Require(main.Find("RigthBar/btn_Easter").gameObject.activeSelf&&timers==1,"status above5 preserves existing Easter visibility without timer refresh");
                        status=5;binding.Refresh();Require(!main.Find("RigthBar/btn_Easter").gameObject.activeSelf,"status5 hides Easter");
                    }
                    int before=loaded;view.RequestPage(OutgameMenuPage.Skins);view.AdvanceTransition(.3f);view.RequestPage(OutgameMenuPage.Main);view.AdvanceTransition(.3f);Require(loaded==before,"disposed main binding no longer refreshes");
                });
                check("outgame-page-specific-visibility-overrides",()=>{
                    var trace=new System.Collections.Generic.List<string>();
                    view.PageRefreshRequested+=page=>trace.Add("refresh:"+page);
                    view.SelectedCommanderRefreshRequested+=()=>trace.Add("selected");
                    Require(view.Page(OutgameMenuPage.Main).gameObject.activeSelf&&!view.Page(OutgameMenuPage.Skins).gameObject.activeSelf&&!view.Page(OutgameMenuPage.Commander).gameObject.activeSelf,"main uses scale while shop/commander use active state");
                    var main=view.Page(OutgameMenuPage.Main);
                    Require(!main.Find("btnPermit").gameObject.activeSelf&&!main.Find("RigthBar/btn_ads").gameObject.activeSelf,"main disables bound permit and ads outlets before refresh");
                    Require(main.Find("RigthBar/btn_ads/btn_ads").gameObject.activeSelf,"same-name nested artwork is not the bound button");
                    view.RequestPage(OutgameMenuPage.Skins);view.AdvanceTransition(.3f);
                    Require(string.Join(",",trace)=="refresh:Skins","shop show refreshes, main hide does not");trace.Clear();
                    view.SetCommanderLevel(6,rules);view.RequestPage(OutgameMenuPage.Commander);view.AdvanceTransition(.3f);
                    Require(string.Join(",",trace)=="selected,refresh:Commander,refresh:Skins","commander refresh order and outgoing shop refresh on hide");trace.Clear();
                    var detail=view.Page(OutgameMenuPage.Commander).Find("objCommander/objCommanderInfo/objSkills/objSkillDetail");detail.gameObject.SetActive(true);
                    view.RequestPage(OutgameMenuPage.Main);view.AdvanceTransition(.3f);
                    Require(!detail.gameObject.activeSelf&&!view.Page(OutgameMenuPage.Commander).gameObject.activeSelf&&string.Join(",",trace)=="refresh:Main","commander hide closes detail without refreshing");
                    view.SetCommanderLevel(5,rules);
                });
                check("outgame-ui-visibility-source-order-and-active-object",()=>{
                    var target=new GameObject("UI visibility source probe");
                    try{
                        target.SetActive(false);target.transform.localScale=new Vector3(2,3,4);
                        var messages=new OutgameMessageDispatcher();var trace=new System.Collections.Generic.List<string>();
                        var item=new VisibilityProbe(target,messages,trace);
                        messages.AddListener("GF_VisibleUI",args=>{
                            Require(ReferenceEquals(args[0],item)&&args.Length==2,"source UI identity and boolean payload");
                            trace.Add("message:"+args[1]+":"+item.Visible);
                        });
                        item.SetVisible(false);item.SetVisible(false);item.SetVisible(true);
                        Require(string.Join(",",trace)=="before:False:True,imp:False:True,message:False:False,before:False:False,imp:False:False,message:False:False,before:True:False,imp:True:False,message:True:True","before/virtual impl observe old flag; message observes committed flag, including repeated calls");
                        Require(target.activeSelf&&target.transform.localScale==Vector3.one,"show restores source unit scale, not previous custom scale");
                        item.SetVisible(false);Require(target.activeSelf&&target.transform.localScale==Vector3.zero,"hidden UI remains active");
                        UnityEngine.Object.DestroyImmediate(target);item.SetVisible(true);
                        Require(item.Visible,"destroyed Unity object still updates logical flag and broadcasts");
                    }finally{if(target)UnityEngine.Object.DestroyImmediate(target);}
                });
                check("outgame-view-main-start-binding-before-activation",()=>{
                    Require(bound==3&&view.Navigation.Current==OutgameMenuPage.Main,"all three pages bound before first main display");
                    foreach(var button in view.Page(OutgameMenuPage.Main).GetComponentsInChildren<Button>(true))
                        if(button.name=="btnStart"||button.name=="btnNewStart")button.onClick.Invoke();
                    Require(string.Join(",",startTrace)=="state:3:False,audio:1:2001,state:3:False,audio:1:2001","both original buttons preserve source state/audio order");
                    start.Dispose();startTrace.Clear();
                    foreach(var button in view.Page(OutgameMenuPage.Main).GetComponentsInChildren<Button>(true))
                        if(button.name=="btnStart"||button.name=="btnNewStart")button.onClick.Invoke();
                    Require(startTrace.Count==0,"binding teardown removes owned callbacks");
                });
                check("outgame-view-original-button-skin-main-roundtrip",()=>{
                    view.Menu.Find("objBottomTab/TabContent/objSkinUnSelected").GetComponent<Button>().onClick.Invoke();
                    Require(view.Navigation.Current==OutgameMenuPage.Skins&&!view.Navigation.CanSwitch,"original skin button starts transition");
                    Require(view.Page(OutgameMenuPage.Skins).parent==view.Page(OutgameMenuPage.Main),"incoming nested during transition");
                    view.AdvanceTransition(0.15f);Require(!view.Navigation.CanSwitch,"midpoint remains locked");
                    view.AdvanceTransition(0.15f);Require(view.Navigation.CanSwitch&&view.Page(OutgameMenuPage.Skins).parent==rect&&view.Page(OutgameMenuPage.Main).gameObject.activeSelf&&view.Page(OutgameMenuPage.Main).localScale==Vector3.zero,"completion restores sibling and hides outgoing by scale while keeping it active");
                    view.Menu.Find("objBottomTab/TabContent/objMainUnSelected").GetComponent<Button>().onClick.Invoke();view.AdvanceTransition(0.3f);
                    Require(view.Page(OutgameMenuPage.Main).gameObject.activeSelf&&view.Page(OutgameMenuPage.Main).localScale==Vector3.one&&view.Page(OutgameMenuPage.Main).anchoredPosition==Vector2.zero,"return main at origin");
                });
                check("outgame-view-commander-overlay-and-rapid-click",()=>{
                    int locked=0;view.CommanderLockedClicked+=()=>locked++;
                    view.Menu.Find("objBottomTab/TabContent/objOtherUnSelected/imgCommanderLock").GetComponent<Button>().onClick.Invoke();
                    Require(locked==1&&view.Navigation.Current==OutgameMenuPage.Main,"lock overlay handles click");
                    view.SetCommanderLevel(6,rules);view.Menu.Find("objBottomTab/TabContent/objOtherUnSelected").GetComponent<Button>().onClick.Invoke();
                    Require(!view.RequestPage(OutgameMenuPage.Skins)&&view.Navigation.Current==OutgameMenuPage.Commander,"pending transition cannot be replaced");
                    view.AdvanceTransition(0.3f);Require(view.Page(OutgameMenuPage.Commander).gameObject.activeSelf&&view.Page(OutgameMenuPage.Commander).localScale==new Vector3(1.01f,1,1)&&view.Page(OutgameMenuPage.Main).gameObject.activeSelf&&view.Page(OutgameMenuPage.Main).localScale==Vector3.zero,"commander visible with original 1.01 horizontal scale after completion");
                    Require(!view.Menu.Find("objBottomTab/TabContent/objShopUnSelected").gameObject.activeSelf&&!view.Menu.Find("objBottomTab/TabContent/objItemUnSelected").gameObject.activeSelf,"source Awake hides obsolete tabs");
                });
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-view-initialize",result="fail",detail=e.ToString()});}
            finally{UnityEngine.Object.DestroyImmediate(root);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-menu-view-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
