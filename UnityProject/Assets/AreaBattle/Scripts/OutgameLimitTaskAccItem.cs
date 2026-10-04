using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameLimitTaskAccPage
    {
        Transform Transform {get;}
        void PreviewAccReward(Transform origin,OutgameNoviceAccRewardItemData reward);
    }
    public sealed class OutgameLimitTaskAccItemServices
    {
        public Func<OutgameActivityConfigManager> ActivityConfig;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<IOutgameLimitTaskAccPage> Page;
        public Func<IOutgameShopCurrencyEffects> Effects;
        public Func<OutgameToolControl> Tools;
        public Func<int,int> GoodsType;
        public Func<string> CoinCost,ActivityReason;
        public Action<object[]> ShowSkinReward;
        public Action<Image,string,string,bool> SetSprite;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Action<GameObject> Destroy=go=>UnityEngine.Object.Destroy(go);
    }
    // LimitTimeTaskAccItem4348, source33232..33241.
    public sealed class OutgameLimitTaskAccItem
    {
        readonly OutgameLimitTaskAccItemServices services;
        public OutgameUiLifetime Lifetime {get;}
        public Action<OutgameLimitTaskAccItem> OnClick;
        public OutgameNoviceAccRewardItemData Data {get;private set;}
        public OutgameChildLimitTimeTaskActivity Child {get;private set;}
        public bool IsLast {get;private set;}
        public Image BoxLocked {get;}public Image BoxUnlocked {get;}public Text Count {get;}public Image BoxRed {get;}
        public GameObject BoxContent {get;}public GameObject RewardContent {get;}
        public Image Got {get;}public Image RewardRed {get;}public Image Icon {get;}public Text RewardCount {get;}
        public OutgameLimitTaskAccItem(GameObject root,OutgameLimitTaskAccItemServices services)
        {
            this.services=services;Lifetime=new OutgameUiLifetime(root,()=>{},services.Destroy);
            T At<T>(string path)where T:Component=>root.transform.Find(path).GetComponent<T>();
            BoxLocked=At<Image>("boxContent/imgBoxL");BoxUnlocked=At<Image>("boxContent/imgBoxU");Count=At<Text>("boxContent/textAcc");BoxRed=At<Image>("boxContent/imgBoxL/imgRed");
            BoxContent=root.transform.Find("boxContent").gameObject;RewardContent=root.transform.Find("rewardContent").gameObject;
            Got=At<Image>("rewardContent/imgGet");RewardRed=At<Image>("rewardContent/imgRedR");Icon=At<Image>("rewardContent/imgIcon");RewardCount=At<Text>("rewardContent/textAccR");
            OutgameUiClick.Add(root,Click);
        }
        public void InitializeSkin(){}
        public void SetData(OutgameNoviceAccRewardItemData data,OutgameChildLimitTimeTaskActivity child){Data=data;Child=child;Refresh();}
        public void Refresh()
        {
            int id=Data.Config.id;IsLast=id==services.ActivityConfig().NoviceAccList.Count;
            int progress=Child.ModuleData.ext.AccProgress;bool red=progress>=Data.Config.accValue&&Data.state==0;
            Count.text=Data.Config.accValue.ToString();RewardCount.text=Data.Config.accValue.ToString();
            if(IsLast)
            {
                IsLast=true;BoxContent.SetActive(false);RewardContent.SetActive(true);
                RewardRed.gameObject.SetActive(red);Got.gameObject.SetActive(Data.state==1);
                var items=services.Config().dicGameItem;
                if(items.TryGetValue(Data.RewardsData[0].itemId,out var config))services.SetSprite(Icon,config.icon,config.atlasName,false);
            }
            else
            {
                BoxContent.SetActive(true);RewardContent.SetActive(false);BoxRed.gameObject.SetActive(red);
                BoxLocked.gameObject.SetActive(Data.state==0);BoxUnlocked.gameObject.SetActive(Data.state==1);
                var boxPosition=new Vector2(0,id%2>=1?65:-65);var textPosition=new Vector2(0,id%2>=1?30:-100);
                BoxLocked.rectTransform.anchoredPosition=boxPosition;BoxUnlocked.rectTransform.anchoredPosition=boxPosition;Count.rectTransform.anchoredPosition=textPosition;
            }
        }
        void Click()
        {
            var callback=OnClick;if(callback!=null)callback(this);
            if(Child.ModuleData.ext.AccProgress<Data.Config.accValue&&Data.state==0)
            {
                var page=services.Page();if(page!=null)page.PreviewAccReward(BoxLocked.transform,Data);
                return;
            }
            ClaimReward();
        }
        void RefreshRedInfo()=>services.Messages().SendMessage("SevendayGetAccReward");
        void ClaimReward()
        {
            if(Child.ModuleData.ext.AccProgress<Data.Config.accValue||Data.state!=0)return;
            Child.GetAccReward(Data.id);
            int id=Data.RewardsData[0].itemId;int count=unchecked((int)Data.RewardsData[0].itemCount);
            var page=services.Page();
            if(page!=null)
            {
                switch(services.GoodsType(id))
                {
                    case 1:services.Effects().FlyMoney(count,page.Transform,BoxLocked.transform.position,false,RefreshRedInfo,true);break;
                    case 2:services.Effects().FlyDiamonds(count,page.Transform,BoxLocked.transform.position,false,RefreshRedInfo,true);break;
                    default:
                        if(services.Config().dicGameItem.TryGetValue(id,out var item))
                        {int tool=item.paramInt;services.Tools().ToolChange(tool,1,true,services.CoinCost(),true);services.ShowSkinReward(new object[]{tool});}
                        break;
                }
            }
            if(Data.RewardsData.Count>=2)
                for(int i=1;i<Data.RewardsData.Count;i++)
                {int extra=Data.RewardsData[i].itemId;var unused=Data.RewardsData[i];services.Tools().ToolChange(extra,0,true,services.ActivityReason(),true);}
            if(IsLast){RewardRed.gameObject.SetActive(false);Got.gameObject.SetActive(true);}
            else{BoxLocked.gameObject.SetActive(false);BoxUnlocked.gameObject.SetActive(true);}
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();OnClick=null;}
    }
}
