using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // GuideBookUI33031/33034/33047/33048: tip-list and tab ownership.
    // Popup rendering and DynamicList remain separate required page dependencies.
    public sealed class OutgameGuideBookBrowseBinding:MonoBehaviour,IOutgameGuideBookItemsPage
    {
        public readonly List<OutgameTipBookItem> Tips=new List<OutgameTipBookItem>();
        public OutgameTipBookItem SelectedTip {get;private set;}
        public OutgameTabButton GuideTab {get;private set;}
        public OutgameTabButton TipTab {get;private set;}
        public OutgameTabButtonGroup Tabs {get;private set;}
        OutgameGuideBookItemServices services;OutgameGuideBookRewardBinding rewards;
        Action<int,GuidebookConfig> openBook;
        public void Bind(OutgameGuideBookItemServices services,OutgameGuideBookRewardBinding rewards,Action<int,GuidebookConfig> openBook)
        {
            PrepareComponents();BindHandlers(services,rewards,openBook);
        }
        public void PrepareComponents()
        {
            GuideTab=AttachTab("tabBtnG","btnBookG0","btnBookG1");TipTab=AttachTab("tabBtnT","btnBookT0","btnBookT1");
            Tabs=transform.Find("tabBtnGroup").gameObject.AddComponent<OutgameTabButtonGroup>();
        }
        public void BindHandlers(OutgameGuideBookItemServices services,OutgameGuideBookRewardBinding rewards,Action<int,GuidebookConfig> openBook)
        {
            this.services=services;this.rewards=rewards;this.openBook=openBook;
            GuideTab.SelectChanged+=SelectGuide;TipTab.SelectChanged+=SelectTip;
        }
        OutgameTabButton AttachTab(string name,string unselected,string selected)
        {
            var root=transform.Find("tabBtnGroup/"+name);var tab=root.gameObject.AddComponent<OutgameTabButton>();
            tab.UnSelectGo=root.Find(unselected).gameObject;tab.SelectGo=root.Find(selected).gameObject;return tab;
        }
        public void PopulateTips()
        {
            Tips.Clear();
            foreach(var pair in services.Config().dicGuideTips)
            {
                // BaseItem.Instantiate273xx: clone without parent, SetParent(false), UIObject normalization/activation.
                var root=Instantiate(transform.Find("TipBookItem").gameObject);
                root.transform.SetParent(transform.Find("tipSV/Viewport/Content"),false);root.transform.localScale=Vector3.one;root.SetActive(true);
                var item=new OutgameTipBookItem(root,services);item.SetData(pair.Value);Tips.Add(item);
            }
        }
        void SelectGuide(bool selected){if(selected)services.Voice(2001);transform.Find("guideSV").gameObject.SetActive(selected);}
        void SelectTip(bool selected)
        {
            if(selected)services.Voice(2001);
            else if(SelectedTip!=null){SelectedTip.SetSelect(false);SelectedTip=null;}
            transform.Find("tipSV").gameObject.SetActive(selected);
        }
        public void OnItemClick(int index,GuidebookConfig config)=>openBook(index,config);
        public void OnTipItemClick(OutgameTipBookItem item)
        {if(SelectedTip!=null&&SelectedTip!=item)SelectedTip.SetSelect(false);SelectedTip=item;}
        public void GetTipReward(GuideTipsConfig config,Vector3 position)=>rewards.Rewards.ClaimTip(config,position);
        // Called after the page's base disposal and guide-data clear, as in33052.
        public void DisposeItemsAndTabs()
        {foreach(var item in Tips)item.Dispose();Tips.Clear();GuideTab.SelectChanged-=SelectGuide;TipTab.SelectChanged-=SelectTip;}
    }
}
