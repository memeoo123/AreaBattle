using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameLimitTaskItemServices
    {
        public Func<OutgameActivityControl> Activities;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Func<string,object[],string> LanguageFormat;
        public OutgameLimitTaskRewardItemServices Rewards;
        public Action<GameObject> Destroy=go=>UnityEngine.Object.Destroy(go);
    }
    // CommonLimitTimeTaskItem4337, source33149..33159. The source goto handler is empty.
    public sealed class OutgameLimitTaskView:IOutgameDynamicItem
    {
        readonly OutgameLimitTaskItemServices services;
        OutgameDynamicListProviderSelection<OutgameLimitTaskItemData> provider;
        public OutgameUiLifetime Lifetime {get;private set;}
        public OutgameLimitTaskItemData Data {get;private set;}
        public OutgameLimitTimeTaskActivity Parent {get;private set;}
        public OutgameChildLimitTimeTaskActivity Child {get;private set;}
        public readonly List<OutgameLimitTaskProgressItem> ProgressItems=new List<OutgameLimitTaskProgressItem>();
        public readonly List<OutgameLimitTaskRewardItem> RewardItems=new List<OutgameLimitTaskRewardItem>();
        public Text Description {get;private set;}
        public Image Progress {get;private set;}
        public Button Claim {get;private set;}
        public Button Goto {get;private set;}
        public Button Claimed {get;private set;}
        public GameObject RewardLayout {get;private set;}
        public Button Tip {get;private set;}
        public GameObject ProgressLayout {get;private set;}
        public GameObject ProgressTemplate {get;private set;}
        public GameObject RewardTemplate {get;private set;}
        public OutgameLimitTaskView(OutgameLimitTaskItemServices services)=>this.services=services;
        public void OnCreate(IOutgameDynamicListProvider data)=>provider=(OutgameDynamicListProviderSelection<OutgameLimitTaskItemData>)data;
        public void InstantiateNoNewItem(GameObject root)
        {
            Lifetime=new OutgameUiLifetime(root,()=>{},services.Destroy);
            T At<T>(string path)where T:Component=>root.transform.Find(path).GetComponent<T>();
            Description=At<Text>("bg/imgTitle/des");Progress=At<Image>("taskProgressItem/progress");
            Claim=At<Button>("bg/btnGet");Goto=At<Button>("bg/btnGoto");Claimed=At<Button>("bg/btnHaveGet");
            RewardLayout=root.transform.Find("bg/tiplayout").gameObject;Tip=At<Button>("bg/TipItem");
            ProgressLayout=root.transform.Find("bg/progressLayout").gameObject;
            ProgressTemplate=root.transform.Find("taskProgressItem").gameObject;RewardTemplate=root.transform.Find("CommonLimitTimeTaskRewardItem").gameObject;
            Claim.onClick.RemoveAllListeners();Claim.onClick.AddListener(ClaimTask);
            Goto.onClick.RemoveAllListeners();Goto.onClick.AddListener(GotoTask);
        }
        public void InitializeSkin(){}
        void GotoTask(){}
        void ClaimTask()
        {
            Child.TaskComplete(Data.id);
            services.Messages().SendMessage("SevendayFinishTask",new object[]{Data.Config.day,Data.id});
        }
        GameObject Clone(GameObject template,Transform parent)
        {var root=UnityEngine.Object.Instantiate(template);root.transform.SetParent(parent,false);root.transform.localScale=Vector3.one;root.SetActive(true);return root;}
        void ClearRewards()
        {
            for(int i=0;i<RewardItems.Count;i++){RewardItems[i].Dispose();services.Destroy(RewardItems[i].Lifetime.GameObject);}
            RewardItems.Clear();
        }
        void RenderProgress()
        {
            for(int i=0;i<ProgressItems.Count;i++){ProgressItems[i].Dispose();services.Destroy(ProgressItems[i].Lifetime.GameObject);}
            ProgressItems.Clear();
            for(int i=0;i<Data.conditions.Count;i++)
            {
                var item=new OutgameLimitTaskProgressItem(Clone(ProgressTemplate,ProgressLayout.transform),services.Destroy);
                var condition=Data.conditions[i];var targets=Data.Config.conditionParams[i].datas;
                int last=Data.Config.conditionParams[i].datas.Length-1;item.SetData(condition,targets[last]);ProgressItems.Add(item);
            }
        }
        public void OnRenderer(int index)
        {
            ClearRewards();Data=provider.GetData(index);
            Parent=services.Activities().GetActivity<OutgameLimitTimeTaskActivity>(1301001);Child=Parent.TaskViewActivities[Data.ActivityId];
            for(int i=0;i<Data.RewardsData.Count;i++)
            {
                var item=new OutgameLimitTaskRewardItem(Clone(RewardTemplate,RewardLayout.transform),services.Rewards,services.Destroy);
                item.SetData(Data.RewardsData[i]);RewardItems.Add(item);
            }
            var config=Data.Config;RenderProgress();
            var values=new object[config.conditionParams.Count];
            for(int i=0;i<config.conditionParams.Count;i++)
            {int last=config.conditionParams[i].datas.Length-1;values[i]=config.conditionParams[i].datas[last];}
            Description.text=services.LanguageFormat(config.des.key,values);
            var claim=Claim.gameObject;claim.SetActive(Data.BtnState==0);
            var go=Goto.gameObject;go.SetActive(Data.BtnState==1);
            var claimed=Claimed.gameObject;claimed.SetActive(Data.BtnState==2);
        }
        public void Dispose(){Lifetime.Dispose();ClearRewards();}
    }
}
