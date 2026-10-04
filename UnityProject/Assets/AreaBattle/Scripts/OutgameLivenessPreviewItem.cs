using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameLivenessPreviewServices
    {
        public Action<Image,string,string,bool> SetSprite;
        public Func<OriginalConfig.Lang,string> Language;
        public Action<RectTransform,int> PopItemInfo;
        public Func<GameObject> SelectedObject=()=>EventSystem.current.currentSelectedGameObject;
        public Func<Action,int> AddUpdate=OutgameUpdateManager.AddHandle;
        public Action<Action> RemoveUpdate=OutgameUpdateManager.RemoveHandle;
        public Func<WaitForEndOfFrame,Task> EndOfFrame=WaitFrame;
        public Action<GameObject> Destroy=go=>UnityEngine.Object.Destroy(go);
        static async Task WaitFrame(WaitForEndOfFrame instruction){await OutgameUnityAwait.Await(instruction);}
    }
    // LivenessPreviewItem4350 and original async state machine4349.
    public sealed class OutgameLivenessPreviewItem
    {
        readonly OutgameLivenessPreviewServices services;
        public readonly WaitForEndOfFrame FrameInstruction=new WaitForEndOfFrame();
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameLivenessPreviewItem> OnClick;
        public OriginalConfig.GameItemConfig Data;public string SpriteName,AtlasName;public OriginalConfig.Lang Name;public int Amount;
        public float PreviousNameWidth;
        public RectTransform BackgroundLeft {get;}public RectTransform BackgroundRight {get;}public RectTransform NameRect {get;}public RectTransform RootRect {get;}
        public Image Icon {get;}public Text CountText {get;}public Text NameText {get;}public Image Touch {get;}public GameObject IconBackground {get;}
        public bool Visible {get;private set;}=true;
        public OutgameLivenessPreviewItem(GameObject root,OutgameLivenessPreviewServices services)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{},services.Destroy);root.transform.localScale=Vector3.one;root.SetActive(true);
            var nodes=new Dictionary<string,GameObject>();foreach(string name in new[]{"img_bgLeft","img_bgRight","img_Icon","text_count","text_name","img_touch"})nodes.Add(name,root.transform.Find(name=="img_Icon"?"IconBg/img_Icon":name).gameObject);Lifetime.ObjectList=nodes;
            BackgroundLeft=nodes["img_bgLeft"].GetComponent<RectTransform>();BackgroundRight=nodes["img_bgRight"].GetComponent<RectTransform>();Icon=nodes["img_Icon"].GetComponent<Image>();
            CountText=nodes["text_count"].GetComponent<Text>();NameText=nodes["text_name"].GetComponent<Text>();Touch=nodes["img_touch"].GetComponent<Image>();
            SetVisible(Visible); // UIObject.initGameObject calls visibility before Awake.
            IconBackground=root.transform.Find("IconBg").gameObject;RootRect=root.transform as RectTransform;NameRect=NameText.GetComponent<RectTransform>();PreviousNameWidth=NameRect.sizeDelta.x;
            root.SetActive(false);OutgameUiClick.Add(root,Click);OutgameUiClick.Add(IconBackground,ShowInfo);
        }
        public void InitializeSkin(){}
        public OutgameLivenessPreviewItem SetData(OriginalConfig.GameItemConfig data,int amount)
        {Data=data;SpriteName=data.ItemIcon;AtlasName=data.atlasName;Name=data.name;Amount=amount;return this;}
        public void SetVisible(bool value)
        {
            if(value)services.AddUpdate(CheckSelection);else services.RemoveUpdate(CheckSelection);
            if(Lifetime.GameObject)OutgameUiPage.ApplyObjectVisibility(Lifetime.GameObject,value);Visible=value;
        }
        bool KeepVisible(string name)=>name==Touch.name||name==IconBackground.name||name.Contains("Node");
        void CheckSelection()
        {var selected=services.SelectedObject();if(!ReferenceEquals(selected,null)){string name=selected.name;if(name!=null&&KeepVisible(name))return;}SetVisible(false);}
        void Click(){var callback=OnClick;if(callback!=null)callback(this);}
        void ShowInfo()=>services.PopItemInfo(IconBackground.GetComponent<RectTransform>(),Data.id);
        public async void Refresh()=>await RefreshAsync();
        public async Task RefreshAsync()
        {
            services.SetSprite(Icon,SpriteName,AtlasName,false);NameText.text=services.Language(Name);CountText.text=string.Format("x{0}",Amount);
            await services.EndOfFrame(FrameInstruction);
            if(Lifetime.GameObject==null)return;
            float extra=NameRect.sizeDelta.x-PreviousNameWidth;
            if(!(extra>0))return;
            PreviousNameWidth=NameRect.sizeDelta.x;
            BackgroundLeft.sizeDelta=new Vector2(BackgroundLeft.sizeDelta.x+extra*.5f,BackgroundLeft.sizeDelta.y);
            BackgroundRight.sizeDelta=new Vector2(BackgroundRight.sizeDelta.x+extra*.5f,BackgroundRight.sizeDelta.y);
            RootRect.sizeDelta=new Vector2(RootRect.sizeDelta.x+extra,RootRect.sizeDelta.y);
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
}
