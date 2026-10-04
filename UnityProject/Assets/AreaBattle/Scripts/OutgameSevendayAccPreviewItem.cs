using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameSevendayAccPreviewServices
    {
        public OutgameLimitTaskRewardItemServices Rewards;
        public Func<GameObject> SelectedObject=()=>EventSystem.current.currentSelectedGameObject;
        public Func<Action,int> AddUpdate=OutgameUpdateManager.AddHandle;
        public Action<Action> RemoveUpdate=OutgameUpdateManager.RemoveHandle;
        public Func<WaitForEndOfFrame,Task> EndOfFrame=WaitFrame;
        public Action<GameObject> Destroy=go=>UnityEngine.Object.Destroy(go);
        static async Task WaitFrame(WaitForEndOfFrame instruction){await OutgameUnityAwait.Await(instruction);}
    }
    // SevendayAccPreviewItem4361 and async state machine4360.
    public sealed class OutgameSevendayAccPreviewItem
    {
        readonly OutgameSevendayAccPreviewServices services;
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameSevendayAccPreviewItem> OnClick;
        public List<OutgameItemReward> Data;
        public readonly List<OutgameLimitTaskRewardItem> Items=new List<OutgameLimitTaskRewardItem>();
        public RectTransform BackgroundLeft {get;}public RectTransform BackgroundRight {get;}
        public Image Touch {get;}public GameObject RewardTemplate {get;}public RectTransform RewardParent {get;}
        public bool Visible {get;private set;}=true;
        public OutgameSevendayAccPreviewItem(GameObject root,OutgameSevendayAccPreviewServices services)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{},services.Destroy);root.transform.localScale=Vector3.one;root.SetActive(true);
            BackgroundLeft=root.transform.Find("img_bgLeft").GetComponent<RectTransform>();BackgroundRight=root.transform.Find("img_bgRight").GetComponent<RectTransform>();
            Touch=root.transform.Find("img_touch").GetComponent<Image>();RewardTemplate=root.transform.Find("CommonLimitTimeTaskRewardItem").gameObject;RewardParent=root.transform.Find("rewardP").GetComponent<RectTransform>();
            SetVisible(Visible);OutgameUiClick.Add(root,Click);
        }
        public void InitializeSkin(){}
        public OutgameSevendayAccPreviewItem SetData(List<OutgameItemReward> data){Data=data;return this;}
        public void SetVisible(bool value)
        {
            if(value)services.AddUpdate(CheckSelection);else services.RemoveUpdate(CheckSelection);
            if(Lifetime.GameObject)OutgameUiPage.ApplyObjectVisibility(Lifetime.GameObject,value);
            Visible=value; // UIObject visibility has no BaseUI GF_VisibleUI message.
        }
        bool KeepVisible(string name)=>name==Touch.name||name.Contains(RewardTemplate.name)||name.Contains("Node");
        void CheckSelection()
        {var selected=services.SelectedObject();if(selected!=null){string name=selected.name;if(name!=null&&KeepVisible(name))return;}SetVisible(false);}
        void Click(){var callback=OnClick;if(callback!=null)callback(this);}
        void ClearItems()
        {for(int i=0;i<Items.Count;i++){Items[i].Dispose();services.Destroy(Items[i].Lifetime.GameObject);}Items.Clear();}
        public async void Refresh()=>await RefreshAsync();
        public async Task RefreshAsync()
        {
            ClearItems();await services.EndOfFrame(new WaitForEndOfFrame());
            if(Lifetime.GameObject==null)return;
            for(int i=0;i<Data.Count;i++)
            {
                var root=UnityEngine.Object.Instantiate(RewardTemplate);root.transform.SetParent(RewardParent.transform,false);root.transform.localScale=Vector3.one;root.SetActive(true);
                var item=new OutgameLimitTaskRewardItem(root,services.Rewards,services.Destroy);item.SetData(Data[i]);Items.Add(item);
            }
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
}
