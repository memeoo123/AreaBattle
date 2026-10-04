using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public interface IOutgameGuideBookItemsPage
    {
        void OnItemClick(int index,GuidebookConfig config);
        void OnTipItemClick(OutgameTipBookItem item);
        void GetTipReward(GuideTipsConfig config,Vector3 position);
    }
    public sealed class OutgameGuideBookItemServices
    {
        public Func<OutgameGuideBookControl> Control;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<IOutgameGuideBookItemsPage> Page;
        public Func<Lang,string> LangValue;
        public Func<string,string> Language;
        public Func<string,object[],string> LanguageFormat;
        public Action<string> Toast;
        public Action<int> Voice;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Action<System.Collections.IEnumerator> StartCoroutine=r=>OutgameUpdateManager.Instance.StartCoroutine(r);
    }
    // Nested GuideBookUI.Data4344 carries a config reference, including null.
    public sealed class OutgameGuideBookRow {public GuidebookConfig Config;}
    public sealed class OutgameGuideBookItem
    {
        readonly OutgameGuideBookItemServices services;
        Func<int,OutgameGuideBookRow> getData;
        public GuidebookConfig Config {get;private set;}
        public int Index {get;private set;}
        public OutgameUiLifetime Lifetime {get;}
        public Button UnlockButton {get;}
        public Button LockButton {get;}
        readonly Text unlockedName,lockedName;readonly Image red;
        public OutgameGuideBookItem(GameObject root,OutgameGuideBookItemServices services)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{});
            UnlockButton=root.transform.Find("btnUnlock").GetComponent<Button>();LockButton=root.transform.Find("btnLock").GetComponent<Button>();
            unlockedName=root.transform.Find("btnUnlock/textNameU").GetComponent<Text>();lockedName=root.transform.Find("btnLock/textNameL").GetComponent<Text>();
            red=root.transform.Find("btnUnlock/imgRed").GetComponent<Image>();
            // ButtonExtension.AddRemoveListener30048 clears runtime listeners before adding each callback.
            UnlockButton.onClick.RemoveAllListeners();UnlockButton.onClick.AddListener(Open);
            LockButton.onClick.RemoveAllListeners();LockButton.onClick.AddListener(ShowLocked);
        }
        public void OnCreate(IList<OutgameGuideBookRow> rows)=>getData=index=>rows[index];
        // Source33208 retains the provider;33213 calls GetData so replacing its public list is observable.
        public void OnCreate(OutgameDynamicListProvider<OutgameGuideBookRow> provider)=>getData=index=>provider.GetData(index);
        public void OnRenderer(int index){var row=getData(index);Config=row.Config;Index=index;Render(row.Config);}
        void Render(GuidebookConfig row)
        {
            unlockedName.text=string.Format("{0}.{1}",row.id,services.LangValue(row.name));
            lockedName.text=string.Format("{0}.{1}",row.id,services.LangValue(row.name));
            bool unlocked=services.Control().IsBookUnlock(row.id);
            UnlockButton.gameObject.SetActive(unlocked);LockButton.gameObject.SetActive(!unlocked);
            bool claimed=services.Control().IsGetBookReward(row.id);red.gameObject.SetActive(unlocked&&!claimed);
        }
        void Open(){var page=services.Page();if(page!=null)page.OnItemClick(Index,Config);}
        void ShowLocked()
        {
            if(!services.Config().dicGuide.TryGetValue(Config.guideId,out var guide))return;
            int level=services.Control().GetGuideUnlockLv(Config.guideId);
            string text=services.LanguageFormat("HeroDetailUI.NotOpened",new object[]{level});services.Toast(text);
        }
        // Source33216 is UIObject.Dispose, not BaseItem.Dispose; DynamicList owns the native object.
        public void Dispose()=>Lifetime.Dispose();
    }
}
