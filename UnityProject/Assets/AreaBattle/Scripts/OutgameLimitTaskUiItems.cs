using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameLimitTaskRewardItemServices
    {
        public Func<OutgameLegacyConfigManager> Config;
        public Action<Image,string,string,bool> SetSprite;
        public Action<RectTransform,int> PopItemInfo;
    }
    // CommonLimitTimeTaskProgressItem4339, source methods33173..33180.
    public sealed class OutgameLimitTaskProgressItem
    {
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameLimitTaskProgressItem> OnClick;
        public Image Progress {get;}
        public Text Value {get;}
        public OutgameLimitTaskProgressItem(GameObject root,Action<GameObject> destroy=null)
        {
            Lifetime=new OutgameUiLifetime(root,()=>{},destroy);
            Progress=root.transform.Find("progress").GetComponent<Image>();
            Value=root.transform.Find("progValue").GetComponent<Text>();
            OutgameUiClick.Add(root,Click);
        }
        public void Refresh(){}
        public void InitializeSkin(){}
        void Click(){var callback=OnClick;if(callback!=null)callback(this);}
        public void SetData(OutgameLimitTaskCondition condition,long target)
        {
            long value=condition.value;
            if(target<value)value=target;
            Progress.fillAmount=(float)value/(float)target;
            Value.text=string.Format("{0} / {1}",value,target);
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
    // CommonLimitTimeTaskRewardItem4340, source methods33181..33188.
    public sealed class OutgameLimitTaskRewardItem
    {
        readonly OutgameLimitTaskRewardItemServices services;
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameLimitTaskRewardItem> OnClick;
        public OutgameItemReward Data {get;private set;}
        public Text Count {get;}
        public Image Icon {get;}
        public OutgameLimitTaskRewardItem(GameObject root,OutgameLimitTaskRewardItemServices services,Action<GameObject> destroy=null)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{},destroy);
            Count=root.transform.Find("itemCount").GetComponent<Text>();
            Icon=root.transform.Find("itemIcon").GetComponent<Image>();
            OutgameUiClick.Add(root,Click);
        }
        public void Refresh(){}
        public void InitializeSkin(){}
        public void SetData(OutgameItemReward reward)
        {
            Data=reward;
            var items=services.Config().dicGameItem;
            if(items.TryGetValue(reward.itemId,out var row))
            {
                services.SetSprite(Icon,row.icon,row.atlasName,false);
                Count.text=reward.itemCount.ToString();
            }
        }
        void Click()
        {
            var callback=OnClick;if(callback!=null)callback(this);
            services.PopItemInfo(Lifetime.RectTransform,Data.itemId);
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
}
