using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameMenuValidation
    {
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Runtime route identities and transition state only; page views, tween presentation, overlays and complete lifecycle integration pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("outgame-menu-runtime-routes-override-legacy-table",()=>{
                Require(OutgameMenuNavigation.SourceUI((OutgameMenuPage)4)=="CommanderUI","runtime4 must not be disabled by legacy HerosDetailUI row");
                Require(OutgameMenuNavigation.SourceUI((OutgameMenuPage)2)=="ShopUI"&&OutgameMenuNavigation.SourceUI((OutgameMenuPage)3)=="Proj_xqzdStartUI","source getter offsets28/32/36");
            });
            check("outgame-menu-first-entry-and-repeated-selection",()=>{
                var nav=new OutgameMenuNavigation();int shown=0,moved=0;nav.FirstPageShown+=p=>shown++;nav.TransitionStarted+=(a,b,c,d)=>moved++;
                Require(nav.ReturnToMain()&&nav.Current==OutgameMenuPage.Main&&nav.CanSwitch,"first entry immediate");
                Require(!nav.ReturnToMain()&&shown==1&&moved==0,"same page ignored");
            });
            check("outgame-menu-transition-lock-direction-and-return",()=>{
                var nav=new OutgameMenuNavigation();nav.ReturnToMain();int direction=0,transitions=0;float duration=0;
                nav.TransitionStarted+=(a,b,c,d)=>{direction=c;duration=d;transitions++;};
                Require(nav.CheckUI(OutgameMenuPage.Skins)&&direction==-1&&duration==0.3f&&!nav.CanSwitch,"left slide locks navigation");
                Require(!nav.CheckUI(OutgameMenuPage.Commander)&&nav.Current==OutgameMenuPage.Skins,"rapid click must not replace destination");
                nav.CompleteTransition();Require(nav.ReturnToMain()&&direction==1&&transitions==2,"return slides right");
                nav.CompleteTransition();Require(nav.CanSwitch&&nav.Current==OutgameMenuPage.Main,"completion unlocks");
            });
            check("outgame-menu-commander-overlay-boundary",()=>{
                var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                Require(OutgameMenuNavigation.CommanderLocked(5,rules)&&!OutgameMenuNavigation.CommanderLocked(6,rules),"config commander1 unlockLevel6");
            });
            check("outgame-menu-native-view-scene-switch-request-timing",()=>{
                var canvas=new GameObject("menu-binding-validation",typeof(RectTransform));
                var roots=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots")).GetComponent<OutgameModelRoots>();
                try
                {
                    var parent=canvas.GetComponent<RectTransform>();parent.sizeDelta=new Vector2(1080,1920);
                    var rules=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                    var menu=canvas.AddComponent<OutgameMenuView>();menu.Initialize(parent,6,rules);
                    using(var binding=new OutgameMenuSceneBinding(menu,roots))
                    {
                        int accepted=0;menu.PageRequestAccepted+=page=>accepted++;
                        Require(menu.RequestPage(OutgameMenuPage.Skins)&&!menu.Navigation.CanSwitch,"shop request starts source transition");
                        Require(roots.ModelBackdrop.gameObject.activeSelf&&!roots.HomeBackdrop.gameObject.activeSelf&&roots.SoldierRoot.localPosition==Vector3.up*.5f,"shop presentation changes before tween completion");
                        Require(!menu.RequestPage(OutgameMenuPage.Main)&&accepted==1&&!roots.HomeBackdrop.gameObject.activeSelf,"rejected transition cannot reset background");
                        menu.AdvanceTransition(.3f);
                        Require(!menu.RequestPage(OutgameMenuPage.Skins)&&accepted==1,"same-page request has no scene side effect");
                        Require(menu.RequestPage(OutgameMenuPage.Commander)&&!roots.HomeBackdrop.gameObject.activeSelf,"commander route does not invoke source MoveCamera");
                        menu.AdvanceTransition(.3f);
                        Require(menu.RequestPage(OutgameMenuPage.Main)&&roots.HomeBackdrop.gameObject.activeSelf&&!roots.ModelBackdrop.gameObject.activeSelf&&roots.SoldierRoot.localPosition==Vector3.zero,"home presentation resets immediately on accepted request");
                        menu.AdvanceTransition(.3f);menu.SetCommanderLevel(5,rules);
                        Require(!menu.RequestPage(OutgameMenuPage.Commander)&&accepted==3,"locked overlay has no accepted event");
                    }
                    menu.RequestPage(OutgameMenuPage.Skins);
                    Require(roots.HomeBackdrop.gameObject.activeSelf,"disposed scene binding releases event subscription");
                }
                finally{UnityEngine.Object.DestroyImmediate(canvas);UnityEngine.Object.DestroyImmediate(roots.gameObject);}
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-menu-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
