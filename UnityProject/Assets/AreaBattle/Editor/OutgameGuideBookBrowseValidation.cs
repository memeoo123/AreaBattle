using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
using RewardFixture=AreaBattle.EditorTools.OutgameGuideBookRewardsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    public static class OutgameGuideBookBrowseValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly RewardFixture Rewards=new RewardFixture();
            public readonly List<string> Trace=new List<string>();public readonly List<IEnumerator> Coroutines=new List<IEnumerator>();
            public readonly OutgameGuideBookItemServices Services;public readonly OutgameGuideBookBrowseBinding Binding;
            public readonly OutgameMessageDispatcher Messages=new OutgameMessageDispatcher();public bool MissingPage;
            public Fixture(bool native=false)
            {
                Services=new OutgameGuideBookItemServices{Control=()=>Rewards.Control,Config=()=>Rewards.Config,
                    Page=()=>MissingPage?null:Binding,LangValue=l=>{Trace.Add("value:"+l.key);return "fixture:"+l.key;},
                    Language=k=>{Trace.Add("lang:"+k);return "locked {0}";},LanguageFormat=(k,a)=>{Trace.Add("format:"+k);return string.Format("locked {0}",a);},
                    Toast=s=>Trace.Add(s),Voice=id=>Trace.Add("voice:"+id),Messages=()=>Messages};
                if(!native)Services.StartCoroutine=r=>{Require(r.MoveNext()&&r.Current is WaitForEndOfFrame,"source frame yield");Coroutines.Add(r);};
                Binding=Rewards.Binding.gameObject.AddComponent<OutgameGuideBookBrowseBinding>();Binding.Bind(Services,Rewards.Binding,(i,c)=>Trace.Add("open:"+i+":"+c.id));
                if(!native){Binding.GuideTab.SendMessage("Awake");Binding.TipTab.SendMessage("Awake");Binding.Tabs.SendMessage("Awake");}
                Messages.AddListener("GF_UIButtonClick",args=>Trace.Add("button:"+((Button)args[0]).name));
            }
            public void Populate(){Binding.PopulateTips();Binding.Tabs.SetSelect(Binding.GuideTab);Rewards.Binding.RefreshRedDots();}
            public void Resume(){foreach(var r in Coroutines)Require(!r.MoveNext(),"one frame then completion");Coroutines.Clear();}
            public OutgameGuideBookItem Book(IList<OutgameGuideBookRow> data)
            {
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/GuideBook/GuideBookItem"),Rewards.Binding.transform,false);
                var item=new OutgameGuideBookItem(root,Services);item.OnCreate(data);return item;
            }
            public void Dispose()=>Rewards.Dispose();
        }
        public static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Equal(List<string> actual,params string[] expected)=>Require(string.Join("|",actual)==string.Join("|",expected),string.Join("|",actual));
        public static void Click(Image image)
        {ExecuteEvents.Execute(image.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original native UGUI assets, source tab/item/pointer dispatch and real reward records. Deterministic frame stepping here; separate PlayMode proves actual coroutine timing. Localization/voice/toast/open-popup endpoints are explicit fixtures; full DynamicList/popup/Spine/Main still pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("guide-book-item-render-index-language-order-and-claim-state",()=>{
                using(var f=new Fixture()){
                    var row=f.Rewards.Config.dicGuidebook[1];var data=new List<OutgameGuideBookRow>{new OutgameGuideBookRow{Config=row}};
                    var item=f.Book(data);item.OnRenderer(0);
                    Equal(f.Trace,"value:"+row.name.key,"value:"+row.name.key);Require(item.UnlockButton.gameObject.activeSelf&&!item.LockButton.gameObject.activeSelf,"unlocked state");
                    Require(item.Lifetime.Transform.Find("btnUnlock/imgRed").gameObject.activeSelf,"unclaimed dot");f.Trace.Clear();item.UnlockButton.onClick.Invoke();Equal(f.Trace,"open:0:1");
                    f.Rewards.Control.GetBookRrward(row.id);item.OnRenderer(0);Require(!item.Lifetime.Transform.Find("btnUnlock/imgRed").gameObject.activeSelf,"refresh reads actual claim manager");
                    f.MissingPage=true;f.Trace.Clear();item.UnlockButton.onClick.Invoke();Require(f.Trace.Count==0,"missing page does not open or emit button message");
                    item.Dispose();Require(item.Lifetime.IsDisposed&&item.Lifetime.GameObject==null&&item.Lifetime.RectTransform,"dynamic list owns gameobject, rect held by source");
                }
            });
            check("guide-book-vs-tip-locked-toast-source-level-rules",()=>{
                using(var f=new Fixture()){
                    var row=f.Rewards.Config.dicGuidebook[1];f.Rewards.Config.dicGuide[row.guideId].guildLv=1000;
                    var item=f.Book(new[]{new OutgameGuideBookRow{Config=row}});item.OnRenderer(0);Require(item.LockButton.gameObject.activeSelf,"strict book boundary");
                    f.Trace.Clear();item.LockButton.onClick.Invoke();Equal(f.Trace,"format:HeroDetailUI.NotOpened","locked 1000");
                    f.Rewards.Config.dicGuide.Remove(row.guideId);f.Trace.Clear();item.LockButton.onClick.Invoke();Require(f.Trace.Count==0,"missing guide ignores locked click");
                    f.Populate();var tip=f.Binding.Tips[0];tip.Config.unlockLevel=1001;tip.SetData(tip.Config);Require(tip.LockButton.gameObject.activeSelf,"tip threshold");
                    f.Trace.Clear();tip.LockButton.onClick.Invoke();Equal(f.Trace,"lang:HeroDetailUI.NotOpened","locked 1000","button:btnLock");
                    tip.Config.unlockLevel=int.MinValue;f.Trace.Clear();tip.LockButton.onClick.Invoke();Equal(f.Trace,"lang:HeroDetailUI.NotOpened","locked 2147483647","button:btnLock");
                }
            });
            check("guide-tip-original-dictionary-order-single-selection-and-latest-frame-state",()=>{
                using(var f=new Fixture()){
                    f.Populate();int i=0;foreach(var pair in f.Rewards.Config.dicGuideTips)Require(ReferenceEquals(pair.Value,f.Binding.Tips[i++].Config),"original config order");
                    f.Binding.Tabs.SetSelect(f.Binding.TipTab);var a=f.Binding.Tips[0];var b=f.Binding.Tips[1];f.Resume();float height=a.Lifetime.RectTransform.rect.height;
                    f.Trace.Clear();a.UnlockButton.onClick.Invoke();Require(a.IsSelected&&f.Binding.SelectedTip==a&&a.Lifetime.RectTransform.rect.height==height,"expand waits for frame");Equal(f.Trace,"button:btnUnlock");
                    b.UnlockButton.onClick.Invoke();Require(!a.IsSelected&&b.IsSelected&&f.Binding.SelectedTip==b,"single selection changes before global click message");f.Resume();
                    Require(Mathf.Approximately(a.Lifetime.RectTransform.rect.height,a.InitialHeight)&&Mathf.Approximately(b.Lifetime.RectTransform.rect.height,b.Description.rect.height),"pending coroutines use latest selection");
                    b.UnlockButton.onClick.Invoke();Require(!b.IsSelected&&f.Binding.SelectedTip==b,"page retains collapsed selected reference");
                    f.Binding.Tabs.SetSelect(f.Binding.GuideTab);Require(f.Binding.SelectedTip==null&&!f.Binding.transform.Find("tipSV").gameObject.activeSelf,"leaving tab clears selection");f.Resume();
                }
            });
            check("guide-tip-image-pointer-real-reward-record-and-duplicate-refresh",()=>{
                using(var f=new Fixture()){
                    f.Populate();f.Binding.Tabs.SetSelect(f.Binding.TipTab);var item=f.Binding.Tips[0];item.UnlockButton.onClick.Invoke();f.Resume();
                    int held=f.Rewards.Inventory.Count(item.Config.reward[0]);f.Trace.Clear();Click(item.RewardButton);
                    Require(f.Rewards.Inventory.Count(item.Config.reward[0])==held+item.Config.reward[1]&&f.Rewards.Manager.ContainsTip(item.Config.id),"real tools and persistent claim");
                    Require(item.IsSelected&&!item.RewardButton.transform.parent.gameObject.activeSelf,"claim retains expansion but removes reward row");
                    Require(f.Trace[0]=="voice:2001"&&f.Trace.FindIndex(s=>s.StartsWith("button:"))<0,"image overload has voice and no button message");
                    using(var restart=new RewardFixture(f.Rewards.Path))Require(restart.Manager.ContainsTip(item.Config.id)&&restart.Inventory.Count(item.Config.reward[0])==held+item.Config.reward[1],"actual independent file reload");
                    int expected=f.Rewards.Inventory.Count(item.Config.reward[0]);f.Trace.Clear();item.RewardButton.GetComponent<OutgameUiPointerClick>().OnPointerClick(null);Require(f.Rewards.Inventory.Count(item.Config.reward[0])==expected&&f.Trace.Count==4,"duplicate still voices and rereads names/description");
                    f.Resume();f.MissingPage=true;f.Trace.Clear();item.RewardButton.GetComponent<OutgameUiPointerClick>().OnPointerClick(null);Equal(f.Trace,"voice:2001");Require(f.Coroutines.Count==0,"missing page skips render and resize");
                }
            });
            check("guide-tab-programmatic-click-reentry-and-notify-order",()=>{
                using(var f=new Fixture()){
                    f.Populate();f.Binding.Tabs.SendMessage("Start");f.Trace.Clear();f.Binding.GuideTab.GetComponent<Button>().onClick.Invoke();Require(f.Trace.Count==0,"same native tab click does not refresh");
                    var group=f.Binding.Tabs;group.SelectionChanging=(a,b)=>{Require(a==f.Binding.GuideTab&&b==f.Binding.TipTab&&group.Previous==a,"before tab notifications");f.Trace.Add("changing");};
                    f.Binding.TipTab.GetComponent<Button>().onClick.Invoke();Equal(f.Trace,"changing","voice:2001");Require(f.Binding.TipTab.IsSelect&&!f.Binding.GuideTab.IsSelect,"clicked tab exclusive");
                    f.Trace.Clear();group.SetSelect(f.Binding.TipTab);Equal(f.Trace,"voice:2001");f.Trace.Clear();group.SetSelectWithoutNotify(f.Binding.GuideTab);Require(f.Trace.Count==0&&f.Binding.GuideTab.IsSelect,"silent mode renders only");
                    // SetSelect notification runs before visuals, and final Previous reads reentrant Current.
                    f.Binding.GuideTab.SelectChanged+=v=>{if(v){Require(!f.Binding.GuideTab.SelectGo.activeSelf,"visuals follow callbacks");group.Current=f.Binding.TipTab;}};
                    group.SetSelectWithoutNotify(f.Binding.TipTab);group.SetSelect(f.Binding.GuideTab);Require(group.Previous==f.Binding.TipTab&&f.Binding.GuideTab.IsSelect,"source rereads Current after iteration");
                }
            });
            check("guide-ui-click-overloads-replace-append-and-throw-boundaries",()=>{
                using(var f=new Fixture()){
                    f.Populate();f.Binding.Tabs.SetSelect(f.Binding.TipTab);var item=f.Binding.Tips[0];item.SetSelect(true);int first=0,second=0;OutgameUiClick.Add(item.RewardButton,()=>first++);OutgameUiClick.Add(item.RewardButton,()=>second++);Click(item.RewardButton);Require(first==0&&second==1,"image handler replaced");
                    f.Trace.Clear();OutgameUiClick.Add(item.UnlockButton,()=>f.Trace.Add("additional"),()=>f.Messages);item.UnlockButton.onClick.Invoke();Equal(f.Trace,"button:btnUnlock","additional","button:btnUnlock");
                    var button=item.LockButton;button.onClick.RemoveAllListeners();OutgameUiClick.Add(button,()=>throw new InvalidOperationException("explicit"),()=>f.Messages);f.Trace.Clear();try{button.onClick.Invoke();throw new Exception("expected");}catch(InvalidOperationException){}Require(f.Trace.Count==0,"throw skips global message");
                }
            });
            check("guide-book-reward-button-global-message-follows-return-only",()=>{
                using(var f=new RewardFixture()){
                    int clicks=0;f.Messages.AddListener("GF_UIButtonClick",args=>{Require(ReferenceEquals(args[0],f.Button),"original Button reference");clicks++;});
                    f.Binding.SelectedBook=f.Config.dicGuidebook[1];f.Button.onClick.Invoke();Require(clicks==1,"successful full reward handler returns then notifies");
                    f.Button.onClick.Invoke();Require(clicks==2,"duplicate guard still returns to click notification");
                    f.Binding.SelectedBook=f.Config.dicGuidebook[2];f.Effects.FailFly=true;try{f.Button.onClick.Invoke();throw new Exception("expected");}catch(InvalidOperationException){}Require(clicks==2,"effect failure suppresses click message");
                }
            });
            return report;
        }
        public static void Validate(){var report=Run();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-book-browse-validation.json"),JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1);}
    }
}
