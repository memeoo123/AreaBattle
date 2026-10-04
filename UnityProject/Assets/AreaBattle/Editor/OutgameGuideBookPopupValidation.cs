using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using AreaBattle.OriginalConfig;
using RewardFixture=AreaBattle.EditorTools.OutgameGuideBookRewardsValidation.Fixture;
namespace AreaBattle.EditorTools
{
    public static class OutgameGuideBookPopupValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly RewardFixture Rewards;
            public readonly OutgameGuideBookPopupBinding Popup;public readonly OutgameGuideBookBrowseBinding Browse;
            public readonly OutgameGuideBookPopupServices Services;public readonly OutgameGuideBookItemServices ItemServices;
            public readonly OutgameGuideBookSprites Sprites=new OutgameGuideBookSprites();
            public readonly OutgameLocalization Language=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));
            public readonly List<string> Trace=new List<string>();public int Commander=1;public Action SpriteCallback;
            public Fixture(string path=null)
            {
                Rewards=new RewardFixture(path);
                Popup=Rewards.Binding.gameObject.AddComponent<OutgameGuideBookPopupBinding>();
                Services=new OutgameGuideBookPopupServices{Config=()=>Rewards.Config,Control=()=>Rewards.Control,CurrentCommanderId=()=>Commander,
                    Language=k=>{Trace.Add("lang:"+k);return Language.Chinese(k);},Voice=id=>Trace.Add("voice:"+id),ClosePage=()=>Trace.Add("close"),Messages=()=>Rewards.Messages,
                    SetSprite=(image,key,atlas,size)=>{Require(atlas=="GuideSprite"&&!size,"source atlas request flags");Trace.Add("sprite:"+key);image.sprite=Sprites.Find(key);SpriteCallback?.Invoke();}};
                Popup.Bind(Rewards.Binding,Services);
                Browse=Rewards.Binding.gameObject.AddComponent<OutgameGuideBookBrowseBinding>();
                ItemServices=new OutgameGuideBookItemServices{Config=()=>Rewards.Config,Control=()=>Rewards.Control,Page=()=>Browse,LangValue=l=>Language.Chinese(l.key),
                    Language=Language.Chinese,LanguageFormat=(k,a)=>string.Format(Language.Chinese(k),a),Toast=s=>Trace.Add("toast:"+s),Voice=Services.Voice,Messages=()=>Rewards.Messages};
                Browse.Bind(ItemServices,Rewards.Binding,Popup.OnItemClick);
                Rewards.Messages.AddListener("GF_UIButtonClick",a=>Trace.Add("button:"+((Button)a[0]).name));
            }
            public OutgameGuideBookItem Item(int id)
            {
                var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/GuideBook/GuideBookItem"),Popup.transform.Find("guideSV/Viewport/dynamicList"),false);
                var item=new OutgameGuideBookItem(root,ItemServices);var data=new List<OutgameGuideBookRow>();foreach(var pair in Rewards.Config.dicGuidebook)data.Add(new OutgameGuideBookRow{Config=pair.Value});
                item.OnCreate(data);item.OnRenderer(data.FindIndex(x=>x.Config.id==id));return item;
            }
            public void Dispose()=>Rewards.Dispose();
        }
        public static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        static string Pitch(Fixture f,int index)=>f.Popup.Pitch.transform.GetChild(index).GetChild(2).GetComponent<Text>().text;
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Actual original popup/config/language strings/sprite payloads/Spine assets and item->browse->popup->reward state. Imported local sprite lookup is an explicit adapter, not proof of source asynchronous atlas service; SkillControl/voice/close are fixture endpoints. Full DynamicList/Main/page lifecycle and matched Player visuals remain pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("guide-popup-original-item-open-content-spine-and-reward-state",()=>{
                using(var f=new Fixture()){
                    var item=f.Item(1);f.Trace.Clear();item.UnlockButton.onClick.Invoke();
                    Require(f.Rewards.Binding.SelectedBook==f.Rewards.Config.dicGuidebook[1]&&f.Rewards.Binding.SelectedIndex==0&&f.Popup.Popup.activeSelf,"actual row opens selected popup");
                    Require(f.Trace[0]=="voice:2001"&&f.Trace[1]=="lang:guide/11"&&f.Trace[2]=="lang:guide/12"&&f.Trace[3]=="sprite:guideUI_icon1","source popup call order");
                    Require(f.Popup.Title.text==f.Language.Chinese("guide/11")&&f.Popup.Content.text==f.Language.Chinese("guide/12"),"original Chinese title and explanation");
                    var spine=f.Popup.Demonstration.GetComponent<SkeletonAnimation>();Require(spine&&spine.AnimationName=="YD"&&spine.loop&&spine.timeScale==1&&spine.enabled&&f.Popup.Demonstration.activeSelf,"own source Spine subtree attached");
                    Require(f.Popup.Icon.enabled&&f.Popup.Icon.sprite==null&&!f.Popup.Pitch.activeSelf&&f.Rewards.Button.gameObject.activeSelf,"guide1 clears sprite while retaining Image enabled");
                    f.Rewards.Button.onClick.Invoke();Require(f.Rewards.Manager.ContainsGuide(1)&&!f.Rewards.Button.gameObject.activeSelf,"real claim updates popup button");
                    item.UnlockButton.onClick.Invoke();Require(!f.Rewards.Button.gameObject.activeSelf,"reopening rereads claim state");
                    f.Popup.OnItemClick(1,f.Rewards.Config.dicGuidebook[2]);Require(!f.Popup.Demonstration.activeSelf&&f.Popup.Icon.sprite==f.Sprites.Find("guideUI_icon3"),"regular page selects original picture and hides Spine");
                }
            });
            check("guide-popup-commander-skill-format-reread-and-minus-one-sentinel",()=>{
                using(var f=new Fixture()){
                    int reads=0;f.Services.CurrentCommanderId=()=>++reads==1?2:1;f.Popup.OnItemClick(5,f.Rewards.Config.dicGuidebook[6]);
                    Require(reads==2&&f.Popup.Title.text==f.Language.Chinese("Commander.SkillName.4")&&f.Popup.Icon.sprite==f.Sprites.Find("guideUI_icon9_1"),"name uses captured calculated id, picture rereads commander");
                    f.Services.CurrentCommanderId=()=>0;f.Trace.Clear();f.Popup.OnItemClick(8,f.Rewards.Config.dicGuidebook[9]);
                    Require(f.Trace.Contains("lang:Commander.SkillName.{0}")&&f.Trace.Contains("sprite:guideUI_icon10_{0}")&&f.Popup.Icon.sprite==null,"-1 calculated id preserves unformatted source strings");
                    f.Services.CurrentCommanderId=()=>int.MaxValue;f.Popup.OnItemClick(9,f.Rewards.Config.dicGuidebook[10]);Require(f.Trace.Contains("lang:Commander.SkillName.2147483645"),"source unchecked32-bit arithmetic");
                }
            });
            check("guide-popup-pitch-empty-segments-retained-children-and-hint-alignment",()=>{
                using(var f=new Fixture()){
                    f.Popup.AdditionalTip.alignment=TextAnchor.UpperLeft;f.Popup.OnItemClick(6,f.Rewards.Config.dicGuidebook[7]);
                    Require(f.Popup.Pitch.activeSelf&&f.Popup.Content.text==""&&Pitch(f,0)=="2"&&Pitch(f,2)=="1"&&f.Popup.AdditionalTip.alignment==TextAnchor.UpperLeft,"defense pitch and retained alignment");
                    f.Popup.OnItemClick(7,f.Rewards.Config.dicGuidebook[8]);Require(Pitch(f,2)=="2"&&Pitch(f,3)=="1","attack source values");
                    f.Rewards.Config.dicGuide[7].param="x;";f.Popup.OnItemClick(6,f.Rewards.Config.dicGuidebook[7]);Require(Pitch(f,0)=="x"&&Pitch(f,1)==""&&Pitch(f,2)=="2"&&Pitch(f,3)=="1","split keeps empty trailing segment and untouched children");
                    foreach(var pair in new[]{(6,"max"),(7,"defanse"),(8,"attack"),(9,"ice"),(10,"fire"),(11,"lighting"),(12,"arrow")}){
                        f.Popup.AdditionalTip.alignment=TextAnchor.UpperRight;OutgameGuideBookPopupBinding.RefreshAdditionalTip(f.Popup.AdditionalTip,pair.Item1,f.Services.Language);
                        Require(f.Popup.AdditionalTip.text==f.Language.Chinese("GuideUI.tipAdd."+pair.Item2),"exact additional key "+pair.Item1);
                        bool retain=pair.Item1==7||pair.Item1==8||pair.Item1==12;Require(f.Popup.AdditionalTip.alignment==(retain?TextAnchor.UpperRight:TextAnchor.MiddleCenter),"source switch return "+pair.Item1);
                    }
                    OutgameGuideBookPopupBinding.RefreshAdditionalTip(f.Popup.AdditionalTip,99,f.Services.Language);Require(f.Popup.AdditionalTip.text==""&&f.Popup.AdditionalTip.alignment==TextAnchor.UpperRight,"default clears text only");
                }
            });
            check("guide-popup-missing-config-and-failure-preserve-source-partial-state",()=>{
                using(var f=new Fixture()){
                    f.Popup.OnItemClick(1,f.Rewards.Config.dicGuidebook[2]);string prior=f.Popup.Title.text;var missing=new GuidebookConfig{id=999,guideId=999};f.Popup.OnItemClick(99,missing);
                    Require(f.Popup.Popup.activeSelf&&f.Rewards.Binding.SelectedBook==missing&&f.Popup.Title.text==prior,"missing config retains visible prior fields");
                    Throws<NullReferenceException>(()=>f.Popup.OnItemClick(88,null));Require(f.Rewards.Binding.SelectedBook==null&&f.Rewards.Binding.SelectedIndex==88&&f.Popup.Popup.activeSelf,"null fails only after voice/select/show");
                    f.Rewards.Button.gameObject.SetActive(false);f.Rewards.Config.dicGuide[7].param=null;Throws<NullReferenceException>(()=>f.Popup.OnItemClick(6,f.Rewards.Config.dicGuidebook[7]));
                    Require(f.Popup.Pitch.activeSelf&&f.Popup.Icon.sprite==f.Sprites.Find("guideUI_icon4")&&!f.Rewards.Button.gameObject.activeSelf,"bad pitch fails after image/show before final reward visibility");
                }
            });
            check("guide-popup-native-close-button-mask-and-voice-failure-order",()=>{
                using(var f=new Fixture()){
                    f.Popup.OnItemClick(4,f.Rewards.Config.dicGuidebook[5]);f.Trace.Clear();f.Popup.transform.Find("guidePop/mainbg/OKBtn").GetComponent<Button>().onClick.Invoke();
                    Require(string.Join("|",f.Trace)=="voice:2001|button:OKBtn"&&!f.Popup.Popup.activeSelf&&f.Rewards.Binding.SelectedBook==null&&f.Rewards.Binding.SelectedIndex==4,"OK hides popup clears book retains index then global message");
                    f.Popup.OnItemClick(4,f.Rewards.Config.dicGuidebook[5]);f.Trace.Clear();f.Popup.transform.Find("guidePop/imgPopMask").GetComponent<OutgameUiPointerClick>().OnPointerClick(null);Require(string.Join("|",f.Trace)=="voice:2001"&&!f.Popup.Popup.activeSelf,"mask has no global Button message");
                    f.Trace.Clear();f.Popup.transform.Find("btnClose").GetComponent<Button>().onClick.Invoke();Require(string.Join("|",f.Trace)=="voice:2001|close|button:btnClose","page close uses required owner callback");
                    f.Popup.OnItemClick(3,f.Rewards.Config.dicGuidebook[4]);var book=f.Rewards.Binding.SelectedBook;f.Services.Voice=id=>throw new InvalidOperationException("voice fixture failure");Throws<InvalidOperationException>(f.Popup.ClosePopup);Require(f.Popup.Popup.activeSelf&&f.Rewards.Binding.SelectedBook==book,"voice failure prevents state changes");
                }
            });
            check("guide-popup-deferred-sprite-callback-and-source-guide-one-null",()=>{
                using(var f=new Fixture()){
                    Action completed=null;f.Services.SetSprite=(image,key,atlas,size)=>completed=()=>image.sprite=f.Sprites.Find("guideUI_icon3");
                    f.Popup.OnItemClick(0,f.Rewards.Config.dicGuidebook[1]);Require(f.Popup.Demonstration.activeSelf&&f.Popup.Icon.sprite==null,"source clears guide1 sprite after starting request");
                    completed();Require(f.Popup.Icon.sprite==f.Sprites.Find("guideUI_icon3")&&f.Popup.Demonstration.activeSelf,"popup does not invent cancellation or async callback gate");
                }
            });
            return report;
        }
        public static void Validate(){var report=Run();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-book-popup-validation.json"),JsonUtility.ToJson(report,true));EditorApplication.Exit(report.passed?0:1);}
    }
}
