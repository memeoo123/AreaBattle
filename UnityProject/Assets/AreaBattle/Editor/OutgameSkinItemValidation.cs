using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameSkinItemValidation
    {
        public sealed class Effects:IOutgameToolEffects,IOutgameSceneSkinEffects
        {
            public List<string> trace=new List<string>();public OutgameSkinCatalog skins;public OutgameSkinActions unlock;
            public OutgameProfile profile;public OutgameProfileStore store;
            public void RefreshTopInfo()=>trace.Add("top");public void GoldSpent(int id,long amount)=>trace.Add("spent:"+amount);
            public void ToolChanged(int id)=>trace.Add("changed:"+id);
            public void UnlockScene(int id){trace.Add("unlock:"+id);unlock.UnlockScene(id,true);}
            public void UnlockSoldier(int id){trace.Add("unlock:"+id);unlock.UnlockSoldier(id,true);}
            public bool ApplyItemEntity(int id,long delta)=>throw new Exception("unexpected entity");
            public void MissingItemEntity(int id)=>throw new Exception("unexpected missing entity");
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>trace.Add("get:"+id+":"+reason);
            public void ReportCost(int id,int category,int amount,int balance)=>trace.Add("cost:"+id);
            public void Save(){trace.Add("save");skins.PrepareSave();store.Save(profile);}
            public void PlayVoice(int id)=>trace.Add("voice:"+id);
            public void AddStatistic(int eventId,long delta)=>trace.Add("stat:"+eventId+":"+delta);
            public void ReportToolUse(int id,int category)=>trace.Add("use:"+id+":"+category);
            public void ChooseSkin(int id){trace.Add("choose:"+id);skins.SetUsedSkin(skins.Skin(id).skinType,id);}
            public void ShowMissingCurrency(int id)=>trace.Add("missing:"+id);
            public void SetRedDot(int id,bool visible)=>trace.Add("dot:"+id+":"+visible);
            public void ChangeSceneStyle(int id,bool flag){if(skins.UsedSkin(4)!=id)throw new Exception("scene style before equip");trace.Add("scene-style:"+id+":"+flag);}
            public void RefreshSceneCard(int id)=>trace.Add("scene-refresh:"+id);
        }
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original SkinItem callbacks integrated with real inventory/skin/disk handlers; live UI, ad SDK and full account lifecycle pending. ChooseSkin listener uses an observed sink for equipment; PlayerControl visual effects pending."};
            try
            {
                string soldier=BattleView.ReadText("Data/Outgame/SkinConfig"),scene=BattleView.ReadText("Data/Outgame/SceneSkinConfig"),stats=BattleView.ReadText("Data/Outgame/StatisticEventConfig");
                var skins=OutgameSkinCatalog.FromOriginal(null,soldier,scene);var profile=new OutgameProfile{skins=skins.State};
                var store=new OutgameProfileStore(Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"skin-buy.json"));
                var e=new Effects{skins=skins,profile=profile,store=store};e.unlock=new OutgameSkinActions(skins,(id,value)=>e.trace.Add("set:"+id+":"+value),stats);
                var tools=new OutgameToolDispatcher(new OutgameLocalInventory(profile.inventory,BattleView.ReadText("Data/AllSkillConfig")),e,BattleView.ReadText("Data/Outgame/GameItemConfig"),scene,soldier);
                var actions=new OutgameSkinItemActions(tools,skins,e,soldier,scene,stats);
                Require(actions.Price(101)==350&&actions.Price(102)==140,"original gold/diamond pricing");
                Require(!actions.Buy(101)&&!skins.SoldierSkin(101).u,"failed debit cannot unlock");
                Require(string.Join("|",e.trace)=="top|changed:1001|save|missing:1001","failed debit preserves dispatcher save/notification");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-buy-insufficient-source-order",result="pass"});
                e.trace.Clear();profile.inventory.goldNum=350;Require(actions.Buy(101)&&profile.inventory.goldNum==0,"exact price succeeds");
                Require(string.Join("|",e.trace)=="top|spent:350|changed:1001|cost:1001|save|voice:2017|stat:110001:1|use:101:3|unlock:101|set:210001:4|changed:101|get:101:|save|choose:101|dot:101:False","purchase callback order: "+string.Join("|",e.trace));
                var persisted=new OutgameSkinCatalog(store.Load().skins,soldier,scene);
                Require(persisted.SoldierSkin(101).u&&persisted.SoldierSkin(101).isNew&&persisted.UsedSkin(1)==100,"source dispatcher save precedes choose and clear-new; no invented final save");
                Require(skins.UsedSkin(1)==101&&!skins.SoldierSkin(101).isNew,"live post-purchase equipment/new state");
                e.Save();persisted=new OutgameSkinCatalog(store.Load().skins,soldier,scene);Require(persisted.UsedSkin(1)==101&&!persisted.SoldierSkin(101).isNew,"later lifecycle save persists final state");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-buy-dispatch-save-before-equip-restart",result="pass"});
                e.trace.Clear();actions.VideoCompleted(102,false,"fixture-ad-reason");Require(e.trace.Count==0&&!skins.SoldierSkin(102).u,"failed video is inert");
                actions.VideoCompleted(102,true,"fixture-ad-reason");Require(skins.SoldierSkin(102).u&&profile.inventory.diamondsNum==0,"successful video grants without currency debit");
                Require(string.Join("|",e.trace)=="use:102:3|unlock:102|set:210001:5|changed:102|get:102:fixture-ad-reason|save|choose:102|dot:102:False","video sequence");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-video-success-failure-source-callbacks",result="pass"});
                e.trace.Clear();actions.Select(101,1,()=>e.trace.Add("callback"));Require(e.trace.Count==0,"nonzero item state suppresses selection");
                actions.Select(101,0,()=>e.trace.Add("callback"));Require(string.Join("|",e.trace)=="voice:2001|choose:101|use:101:3|callback|dot:101:False","selection order");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-select-state-and-callback-order",result="pass"});
                var sceneActions=new OutgameSceneSkinActions(actions,tools,skins,e,stats);
                profile.inventory.goldNum=0;e.trace.Clear();Require(!sceneActions.Buy(2)&&skins.UsedSkin(4)==1,"scene insufficient keeps equip");
                Require(string.Join("|",e.trace)=="top|changed:1001|save|missing:1001","scene insufficient dispatcher effects");
                profile.inventory.goldNum=1000;e.trace.Clear();Require(sceneActions.Buy(2)&&profile.inventory.goldNum==0,"source scene price");
                Require(string.Join("|",e.trace)=="top|spent:1000|changed:1001|cost:1001|save|voice:2017|stat:110002:1|scene-style:2:False|unlock:2|changed:2|get:2:|save|dot:2:False|scene-refresh:2","scene purchase order: "+string.Join("|",e.trace));
                persisted=new OutgameSkinCatalog(store.Load().skins,soldier,scene);
                Require(persisted.UsedSkin(4)==2&&persisted.SceneSkin(2).u&&persisted.SceneSkin(2).isNew&&!skins.SceneSkin(2).isNew,"scene purchase saves equipped/unlocked state before clearing new flag");
                report.checks.Add(new BattleBuild.Check{id="outgame-scene-purchase-equip-before-unlock-save",result="pass"});
                e.trace.Clear();sceneActions.Select(2);Require(e.trace.Count==0,"same scene selection is inert");
                sceneActions.Select(3);Require(!skins.SceneSkin(3).u&&skins.UsedSkin(4)==3,"source scene handler does not test ownership/item state");
                Require(string.Join("|",e.trace)=="voice:2001|scene-style:3:False|dot:3:False|use:3:3","scene selection order/no ChooseSkin event/save/callback");
                Require(new OutgameSkinCatalog(store.Load().skins,soldier,scene).UsedSkin(4)==2,"scene selection defers persistence");
                report.checks.Add(new BattleBuild.Check{id="outgame-scene-selection-distinct-source-semantics",result="pass"});
                var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                try
                {
                    var card=page.transform.Find("bottom/SkinItem");card.gameObject.SetActive(true);var view=card.gameObject.AddComponent<OutgameSkinItemView>();
                    bool seven=true;int status=5,activityClicks=0;var original=actions.Configuration(103);int special=original.special,actNo=original.actNo;
                    view.Bind(103,1,actions,skins,key=>key??"",name=>null,()=>seven,()=>status,(videoId,callback)=>callback(false),"fixture",()=>activityClicks++);
                    Require(card.Find("btn_goldUnlock").gameObject.activeSelf&&view.CurrentItemState==3,"locked normal native card shows price button");
                    profile.inventory.goldNum=100000;profile.inventory.diamondsNum=100000;
                    card.Find("btn_goldUnlock").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(skins.SoldierSkin(103).u&&view.CurrentItemState==1&&card.Find("img_using").gameObject.activeSelf&&!card.Find("btn_goldUnlock").gameObject.activeSelf,"native buy button grants/equips and refreshes used badge");
                    Require(!card.Find("imgRedDot").gameObject.activeSelf,"purchase clears native red dot");
                    report.checks.Add(new BattleBuild.Check{id="outgame-skin-native-buy-button-and-used-badge",result="pass"});
                    skins.SoldierSkin(103).u=false;original.special=2;original.actNo=1301;view.Refresh();
                    Require(view.CurrentItemState==4&&card.Find("btn_actLimit").gameObject.activeSelf,"active seven-day lock routes to activity");
                    card.Find("btn_actLimit").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Require(activityClicks==1,"native activity route");
                    seven=false;view.Refresh();Require(view.CurrentItemState==3,"expired seven-day skin falls back to currency");
                    original.actNo=999;status=4;view.Refresh();Require(view.CurrentItemState==4,"other activity status !=5 remains activity");status=5;view.Refresh();Require(view.CurrentItemState==3,"other activity status5 falls back");
                    original.special=3;view.Refresh();Require(view.CurrentItemState==5&&card.Find("btn_spceilLimit").gameObject.activeSelf&&card.Find("btn_spceilLimit/txt_getway").GetComponent<UnityEngine.UI.Text>().text=="ShopUI.ActivityGetWay","special source label/button");
                    original.special=99;view.Refresh();Require(view.CurrentItemState==5,"unknown special retains previous state");original.special=special;original.actNo=actNo;
                    report.checks.Add(new BattleBuild.Check{id="outgame-skin-native-activity-state-transitions",result="pass"});
                    // ShopUI coroutine f13861 creates SceneSkinItem using field172 SkinItem, not field176 sceneSkinItem.
                    var sceneCard=UnityEngine.Object.Instantiate(page.transform.Find("bottom/SkinItem"),page.transform.Find("bottom/skinGroup_scene/Viewport/Content_scene"),false);
                    sceneCard.name="SceneSkinItem_2";UnityEngine.Object.DestroyImmediate(sceneCard.GetComponent<OutgameSkinItemView>());
                    var sceneView=sceneCard.gameObject.AddComponent<OutgameSkinItemView>();
                    skins.SceneSkin(2).u=false;skins.SetUsedSkin(4,1);profile.inventory.goldNum=1000;
                    sceneView.Bind(2,4,actions,skins,key=>key??"",name=>null,()=>false,()=>5,(videoId,callback)=>callback(false),"fixture",()=>activityClicks++,sceneActions);
                    Require(sceneView.CurrentItemState==3&&sceneCard.Find("btn_goldUnlock/img_goldUnlock/txt_gold").GetComponent<UnityEngine.UI.Text>().text=="1000","source shared template binds scene price");
                    e.trace.Clear();sceneCard.Find("btn_goldUnlock").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(skins.UsedSkin(4)==2&&skins.SceneSkin(2).u&&sceneView.CurrentItemState==1&&sceneCard.Find("img_using").gameObject.activeSelf,"native scene buy routes through scene override");
                    Require(e.trace.Contains("stat:110002:1")&&!e.trace.Contains("choose:2"),"scene override does not accidentally use soldier event");
                    skins.SetUsedSkin(4,1);sceneView.Refresh();e.trace.Clear();sceneCard.Find("btn_choose").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Require(skins.UsedSkin(4)==2&&sceneView.CurrentItemState==1&&e.trace.Contains("scene-style:2:False"),"native scene choose updates scene and badge");
                    report.checks.Add(new BattleBuild.Check{id="outgame-scene-native-shared-template-buttons",result="pass"});
                    UnityEngine.Object.DestroyImmediate(sceneCard.gameObject);UnityEngine.Object.DestroyImmediate(view);
                    var skinSprites=new OutgameSkinSprites();var lists=page.AddComponent<OutgameShopSkinLists>();var buildOrder=new List<int>();
                    lists.Build(skins,2,(item,id,type)=>{buildOrder.Add(type);item.Bind(id,type,actions,skins,key=>key??"",name=>skinSprites.ForSkin(id),()=>false,()=>5,(videoId,callback)=>callback(false),"fixture",()=>{},type==4?sceneActions:null);});
                    Require(lists.Count==skins.OrderedSoldiers.Count+skins.OrderedScenes.Count&&buildOrder[0]==2,"all configured cards created, selected soldier category first");
                    Require(lists.Card(100).transform.parent.name=="Content_normal"&&lists.Card(200).transform.parent.name=="Content_defense"&&lists.Card(300).transform.parent.name=="Content_attack"&&lists.Card(1).transform.parent.name=="Content_scene","source category content mapping");
                    Require(lists.Card(2).transform.Find("btn_actLimit")!=null,"scene list uses shared SkinItem template");
                    skins.SetUsedSkin(4,1);lists.Refresh();Require(lists.Card(1).CurrentItemState==1&&lists.Card(2).CurrentItemState==0,"list refresh updates previous and next equipment badges");
                    foreach(var skin in skins.OrderedSoldiers.Concat(skins.OrderedScenes))Require(lists.Card(skin.s).transform.Find("img_poster").GetComponent<UnityEngine.UI.Image>().sprite==skinSprites.ForSkin(skin.s),"native poster uses atlas-qualified original sprite");
                    Require(skinSprites.ForSkin(1)!=null&&skinSprites.ForSkin(100)!=null,"both source atlas families loaded");
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-all-original-skin-posters",result="pass"});
                    int before=lists.Count;lists.Build(skins,1,(item,id,type)=>item.Bind(id,type,actions,skins,key=>key??"",name=>skinSprites.ForSkin(id),()=>false,()=>5,(videoId,callback)=>callback(false),"fixture",()=>{},type==4?sceneActions:null));
                    Require(lists.Count==before&&lists.Card(100).transform.parent.childCount==skins.OrderedSoldiers.Count(skin=>skin.skinType==1),"rebuild releases old cards instead of duplicating");
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-skin-list-native-grouping-and-refresh",result="pass"});
                    var menuRoot=new GameObject("Skin lifecycle integration",typeof(RectTransform));OutgameShopMenuBinding menuBinding=null;
                    try{
                        var menu=menuRoot.AddComponent<OutgameMenuView>();OutgameShopSkinLists nativeCards=null;int resets=0;
                        var progression=new OutgameCommanderProgression(BattleView.ReadText("Data/Outgame/CommanderConfig"),BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                        menu.Initialize((RectTransform)menuRoot.transform,6,progression,(kind,target)=>{
                            if(kind!=OutgameMenuPage.Skins)return;
                            nativeCards=target.gameObject.AddComponent<OutgameShopSkinLists>();
                            nativeCards.Build(skins,1,(item,id,type)=>item.Bind(id,type,actions,skins,key=>key??"",name=>skinSprites.ForSkin(id),()=>false,()=>5,(videoId,callback)=>callback(false),"fixture",()=>{},type==4?sceneActions:null));
                            menuBinding=new OutgameShopMenuBinding(menu,nativeCards,type=>{Require(type==0,"refresh resets all first-ad slots");resets++;});
                        });
                        Require(resets==1,"initial shop hide invokes source refresh after binding");
                        skins.SoldierSkin(101).isNew=true;skins.SceneSkin(2).isNew=true;
                        menu.RequestPage(OutgameMenuPage.Skins);menu.AdvanceTransition(.3f);
                        Require(resets==2&&nativeCards.Card(101).transform.Find("imgRedDot").gameObject.activeSelf&&nativeCards.Card(2).transform.Find("imgRedDot").gameObject.activeSelf,"actual menu show refreshes soldier and scene data");
                        skins.SoldierSkin(101).isNew=false;skins.SceneSkin(2).isNew=false;
                        menu.RequestPage(OutgameMenuPage.Main);menu.AdvanceTransition(.3f);
                        Require(resets==3&&!nativeCards.Card(101).transform.Find("imgRedDot").gameObject.activeSelf&&!nativeCards.Card(2).transform.Find("imgRedDot").gameObject.activeSelf,"actual shop hide refreshes both card families while inactive");
                        menuBinding.Dispose();menu.RequestPage(OutgameMenuPage.Skins);menu.AdvanceTransition(.3f);Require(resets==3,"disposing removes only owned refresh subscription");
                        report.checks.Add(new BattleBuild.Check{id="outgame-shop-menu-lifecycle-native-data-refresh",result="pass"});
                    }finally{menuBinding?.Dispose();UnityEngine.Object.DestroyImmediate(menuRoot);}
                    var equipmentTrace=new List<string>();var equipment=new OutgameShopEquipment(skins,soldier,type=>{Require(skins.UsedSkin(type)==101,"equipment precedes reset");equipmentTrace.Add("reset:"+type);},type=>{lists.RefreshCategory(type);equipmentTrace.Add("refresh:"+type);},(type,id)=>{Require(lists.Card(101).CurrentItemState==1&&lists.Card(100).CurrentItemState==0,"all same-type cards refreshed before model change");equipmentTrace.Add("model:"+type+":"+id);});
                    skins.SoldierSkin(100).u=true;skins.SoldierSkin(101).u=true;skins.SetUsedSkin(1,100);lists.Refresh();
                    equipment.ChooseSkin(101);Require(string.Join("|",equipmentTrace)=="reset:1|refresh:1|model:1:101","source equipment listener sequence");
                    equipmentTrace.Clear();equipment.ChooseSkin(2);equipment.ChooseSkin(99999);Require(equipmentTrace.Count==0,"scene/unknown IDs excluded by soldier table lookup");
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-equipment-listener-category-refresh",result="pass"});

                    var tabs=page.AddComponent<OutgameShopTabs>();var tabTrace=new List<string>();var lang=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));
                    Require(lang.Chinese("")==""&&lang.Chinese("missing-source-key")=="missing-source-key","source missing localization falls back to key");
                    tabs.Bind(skins,1,lang.Chinese,id=>tabTrace.Add("voice:"+id),type=>tabTrace.Add("rotate:"+type));
                    skins.SoldierSkin(201).isNew=true;skins.SceneSkin(2).isNew=true;
                    var defense=page.transform.Find("bottom/ToggleArr/tog_defense").GetComponent<UnityEngine.UI.Toggle>();defense.onValueChanged.Invoke(true);
                    Require(tabs.SelectedSoldierType==2&&string.Join("|",tabTrace)=="voice:2001|rotate:2","native toggle callback effects");
                    Require(!defense.transform.Find("objDefenseRedDot").gameObject.activeSelf&&skins.SoldierSkin(201).isNew,"active tab hides badge without consuming skin new flag");
                    tabTrace.Clear();page.transform.Find("bottom/ToggleArr/tog_MapSkin").GetComponent<UnityEngine.UI.Toggle>().onValueChanged.Invoke(true);
                    Require(tabs.SelectedTab==4&&tabs.SelectedSoldierType==2&&string.Join("|",tabTrace)=="voice:2001|rotate:2","scene tab retains last soldier rotation type");
                    Require(page.transform.Find("bottom/bg_top/txt_shopinfo").GetComponent<UnityEngine.UI.Text>().text==lang.Chinese("ShopUI.Scene")&&defense.transform.Find("objDefenseRedDot").gameObject.activeSelf,"scene title and other-category red dot restored");
                    tabTrace.Clear();tabs.Changed(4,false);Require(tabTrace.Count==0,"toggle false is inert");
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-native-tabs-title-rotation-red-dots",result="pass"});
                    var mapToggle=page.transform.Find("bottom/ToggleArr/tog_MapSkin").GetComponent<UnityEngine.UI.Toggle>();
                    Require(defense.group!=null&&defense.group==mapToggle.group&&!defense.group.allowSwitchOff&&mapToggle.graphic!=null,"source group and graphic references imported");
                    Require(mapToggle.onValueChanged.GetPersistentEventCount()==2&&mapToggle.onValueChanged.GetPersistentTarget(0)==page.transform.Find("bottom/skinGroup_scene").gameObject&&mapToggle.onValueChanged.GetPersistentMethodName(0)=="SetActive","source scene content persistent target");
                    // RuntimeOnly source events are temporarily enabled in the isolated editor fixture.
                    foreach(var toggle in page.GetComponentsInChildren<UnityEngine.UI.Toggle>(true))
                        for(int i=0;i<toggle.onValueChanged.GetPersistentEventCount();i++)toggle.onValueChanged.SetPersistentListenerState(i,UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
                    page.SetActive(true);defense.gameObject.SetActive(true);mapToggle.gameObject.SetActive(true);
                    defense.isOn=true;mapToggle.isOn=true;
                    Require(mapToggle.isOn&&!defense.isOn&&page.transform.Find("bottom/skinGroup_scene").gameObject.activeSelf&&!page.transform.Find("bottom/skinGroup_defense").gameObject.activeSelf,"native grouped value change switches actual source content");
                    Require(mapToggle.transform.Find("Background/Label_on").gameObject.activeSelf&&!defense.transform.Find("Background/Label_on").gameObject.activeSelf,"source selected labels follow real toggle state");
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-imported-toggle-group-content-events",result="pass"});
                    foreach(int initial in new[]{2,3,99})
                    {
                        var fresh=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/ShopUI"));
                        try
                        {
                            foreach(var toggle in fresh.GetComponentsInChildren<UnityEngine.UI.Toggle>(true))
                                for(int i=0;i<toggle.onValueChanged.GetPersistentEventCount();i++)toggle.onValueChanged.SetPersistentListenerState(i,UnityEngine.Events.UnityEventCallState.EditorAndRuntime);
                            var initialTabs=fresh.AddComponent<OutgameShopTabs>();initialTabs.Bind(skins,initial,lang.Chinese,id=>{},type=>{});initialTabs.InitializeSourceSelection();
                            int expected=initial==99?1:initial;string group=expected==1?"normal":expected==2?"defense":"attack";
                            Require(initialTabs.SelectedSoldierType==expected&&fresh.transform.Find("bottom/skinGroup_"+group).gameObject.activeSelf,"source initial category/fallback");
                            Require(!fresh.transform.Find("bottom/skinGroup_scene").gameObject.activeSelf&&!fresh.transform.Find("bottom/skinGroup_Shop").gameObject.activeSelf,"scene and shop start hidden");
                        }
                        finally{UnityEngine.Object.DestroyImmediate(fresh);}
                    }
                    report.checks.Add(new BattleBuild.Check{id="outgame-shop-original-initial-category-selection",result="pass"});





                }
                finally{UnityEngine.Object.DestroyImmediate(page);}

            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-skin-item-actions",result="fail",detail=e.ToString()});}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-skin-item-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
