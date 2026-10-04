using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // Source4369 and iterator4368. BaseItem owns destruction; UpdateManager owns coroutines.
    public sealed class OutgameTipBookItem
    {
        readonly OutgameGuideBookItemServices services;
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameTipBookItem> OnClick;
        public GuideTipsConfig Config {get;private set;}
        public bool IsSelected {get;private set;}
        public float InitialHeight {get;}
        public RectTransform Description {get;}
        public Button UnlockButton {get;}
        public Button LockButton {get;}
        public Image RewardButton {get;}
        readonly Text name,lockedName,info;readonly Image red,unselected,selected,background;
        public OutgameTipBookItem(GameObject root,OutgameGuideBookItemServices services)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{});
            T At<T>(string path)where T:Component=>root.transform.Find(path).GetComponent<T>();
            UnlockButton=At<Button>("btnUnlock");LockButton=At<Button>("btnLock");RewardButton=At<Image>("imgTextBg/bottomGo/btnBox0");
            name=At<Text>("btnUnlock/textName");lockedName=At<Text>("btnLock/textNameL");info=At<Text>("imgTextBg/textInfo");
            red=At<Image>("btnUnlock/imgRed");unselected=At<Image>("btnUnlock/imgUnSelect");selected=At<Image>("btnUnlock/imgSelect");background=At<Image>("imgTextBg");
            OutgameUiClick.Add(UnlockButton,Toggle,services.Messages);OutgameUiClick.Add(LockButton,ShowLocked,services.Messages);OutgameUiClick.Add(RewardButton,Claim);
            InitialHeight=Lifetime.RectTransform.rect.height;Description=background.GetComponent<RectTransform>();
        }
        public void SetData(GuideTipsConfig row){Config=row;Render(row);SetSelect(false);}
        void Render(GuideTipsConfig row)
        {
            name.text=string.Format("{0}.{1}",row.id,services.LangValue(row.name));
            lockedName.text=string.Format("{0}.{1}",row.id,services.LangValue(row.name));info.text=services.LangValue(row.des);
            bool claimed=services.Control().IsGetTipReward(row.id);bool unlocked=services.Control().IsTipUnlock(row.id);
            LockButton.gameObject.SetActive(!unlocked);UnlockButton.gameObject.SetActive(unlocked);red.gameObject.SetActive(unlocked&&!claimed);
            RewardButton.transform.parent.gameObject.SetActive(!claimed);
        }
        public void SetSelect(bool value)
        {
            IsSelected=value;selected.gameObject.SetActive(value);unselected.gameObject.SetActive(!value);background.gameObject.SetActive(value);
            LayoutRebuilder.ForceRebuildLayoutImmediate(Description);services.StartCoroutine(ResizeAfterFrame());
        }
        IEnumerator ResizeAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            float height=IsSelected?Description.rect.height:InitialHeight;
            Lifetime.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(Lifetime.Transform.parent.GetComponent<RectTransform>());
        }
        void Toggle(){SetSelect(!IsSelected);var page=services.Page();if(page!=null)page.OnTipItemClick(this);}
        void ShowLocked()
        {string text=services.Language("HeroDetailUI.NotOpened");text=string.Format(text,unchecked(Config.unlockLevel-1));services.Toast(text);}
        void Claim()
        {
            services.Voice(2001);var page=services.Page();if(page==null)return;
            var row=Config;var position=RewardButton.transform.position;page.GetTipReward(row,position);
            Render(Config);SetSelect(IsSelected);
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
}
