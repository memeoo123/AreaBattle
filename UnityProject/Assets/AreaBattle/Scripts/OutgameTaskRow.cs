using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public enum OutgameTaskType {DailyTask=0,Achievement=1}
    // TaskItemItemData4366 is a display projection; its progress is Int32, unlike saved task progress.
    public sealed class OutgameTaskRowData
    {
        public int liveness,number,prog,contentType,id;
        public Lang dis;public List<ListArrayInt> rewards;public int ContentArgument;public long Uid;
    }
    public sealed class OutgameTaskRowServices
    {
        public Func<OutgameLegacyConfigManager> Config;
        public Func<Lang,string> LanguageValue;public Func<string,string> Language;
        public Func<int,int> GoodsType;
        public Func<int,int,KeyValuePair<int,int>> RandomReward;
        public Action<Image,string,string,bool> SetSprite;
        public Func<IOutgameShopCurrencyEffects> Effects;
        public Action<int,int> ShowAward;
        public Action RefreshTopInfo;
        public Action<string,string,string,string> ReportActivityReward;
        public Action<object[]> Warning,Error;
        public Action<Transform,float,float,int,Action> MoveX;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Action<GameObject> Destroy=go=>UnityEngine.Object.Destroy(go);
    }
    // TaskItemItem4367, source33373..33386. The animation completion owns the claim callback.
    public sealed class OutgameTaskRow
    {
        readonly OutgameTaskRowServices services;
        public OutgameUiLifetime Lifetime {get;private set;}
        public OutgameTaskRowData Data {get;private set;}
        public bool IsClaim,ActionCache;public Action<int,long> ClaimedCallback;
        public OutgameTaskType Type;public int RewardId,MinimumCount;
        public Text Title {get;private set;}public Text Target {get;private set;}public Text Liveness {get;private set;}
        public Button Claim {get;private set;}public Image Icon {get;private set;}public Image Fill {get;private set;}
        internal OutgameTaskRow(OutgameTaskRowServices services)
        {this.services=services;Lifetime=new OutgameUiLifetime(null,()=>{},services.Destroy);}
        public OutgameTaskRow(GameObject root,OutgameImportedUiOutlets outlets,OutgameTaskRowServices services):this(services)
        {Instantiate(root,outlets);}
        internal void Instantiate(GameObject root,OutgameImportedUiOutlets outlets)
        {
            Lifetime=new OutgameUiLifetime(root,()=>{},services.Destroy);
            var nodes=new Dictionary<string,GameObject>();foreach(var pair in outlets.Read(root))nodes.Add(pair.Key,(GameObject)pair.Value);Lifetime.ObjectList=nodes;
            Title=nodes["Title"].GetComponent<Text>();Target=nodes["Traget"].GetComponent<Text>();Claim=nodes["GotoBtn"].GetComponent<Button>();
            Liveness=nodes["LivenessCount"].GetComponent<Text>();Icon=nodes["ItemIcon"].GetComponent<Image>();Fill=nodes["imgFull"].GetComponent<Image>();
            OutgameUiClick.Add(Claim,Click,services.Messages);
        }
        public void InitializeSkin(){}
        public void ReplaceData(OutgameTaskRowData data)=>Data=data;
        public void SetData(OutgameTaskRowData data,OutgameTaskType type,Action<int,long> claimed)
        {
            Data=data;ClaimedCallback=claimed;Type=type;
            if(type==OutgameTaskType.Achievement)
            {
                Liveness.transform.parent.gameObject.SetActive(false);
                Fill.transform.parent.GetComponent<RectTransform>().sizeDelta=new Vector2(530,26);
                Title.GetComponent<RectTransform>().sizeDelta=new Vector2(530,57);
            }
        }
        public void Refresh()
        {
            Liveness.text=Data.liveness.ToString();int content=Data.contentType;
            if(content==services.Config().statisticEventConfig.CommanderUpgrade)
            {
                string key=string.Format("CommanderName.{0}",Data.ContentArgument);
                string format=services.LanguageValue(Data.dis);string name=services.Language(key);
                Title.text=string.Format(format,name,Data.number);
            }
            else if(content==services.Config().statisticEventConfig.UseProp)
            {
                string key=string.Format("Commander.SkillName.{0}",Data.ContentArgument%1000);
                string format=services.LanguageValue(Data.dis);string name=services.Language(key);
                Title.text=string.Format(format,name,Data.number);
            }
            else
            {
                try{Title.text=string.Format(services.LanguageValue(Data.dis),SourceAbs(Data.number));}
                catch(Exception){services.Warning(new object[]{services.LanguageValue(Data.dis),Data.id});}
            }
            int id=Data.rewards[0].datas[0];int count=Data.rewards[0].datas[1];
            switch(services.GoodsType(id))
            {
                case 1:services.SetSprite(Icon,"Task_gold","TaskUI",true);break;
                case 2:services.SetSprite(Icon,"Pulbic_diamond","PublicIcon",true);break;
                case 6:if(services.Config().dicGameItem.TryGetValue(id,out var row))services.SetSprite(Icon,row.icon,row.atlasName,false);break;
            }
            Icon.transform.GetChild(0).GetComponent<Text>().text=count==1?"":count.ToString();RefreshProgress();
        }
        public void RefreshProgress()
        {
            bool rank=Data.contentType==services.Config().statisticEventConfig.ArenaRank;
            int value=Data.prog;
            if(rank)
            {
                int target=Data.number;int sign=value>>31;int negativeAbs=unchecked(sign-(value^sign));
                bool ready=target<=negativeAbs;int shown=ready?SourceAbs(target):0;
                Target.text=string.Format("{0}/{1}",shown,SourceAbs(Data.number));Fill.fillAmount=ready?1:0;Claim.gameObject.SetActive(ready);return;
            }
            if(Data.number<value){Data.prog=Data.number;value=Data.number;}
            Target.text=string.Format("{0}/{1}",value,Data.number);Fill.fillAmount=(float)Data.prog/(float)Data.number;
            Claim.gameObject.SetActive(Data.prog>=Data.number);
        }
        void RefreshTop()=>services.RefreshTopInfo();
        static int SourceAbs(int value){int sign=value>>31;return unchecked((value+sign)^sign);}
        void Click()
        {
            Claim.enabled=false;RewardId=Data.rewards[0].datas[0];int kind=services.GoodsType(RewardId);MinimumCount=0;
            if(kind==1)services.Effects().FlyMoney(Data.rewards[0].datas[1],null,Icon.transform.position,false,RefreshTop,true);
            else if(kind==2)services.Effects().FlyDiamonds(Data.rewards[0].datas[1],null,Icon.transform.position,false,RefreshTop,true);
            else
            {
                if(RewardId==0){var reward=services.RandomReward(kind,1);RewardId=reward.Key;MinimumCount=reward.Value;}
                if(RewardId!=1001)
                {
                    try{services.ShowAward(RewardId,1);}
                    catch(Exception e){services.Error(new object[]{RewardId.ToString()+"\n"+e});}
                }
                else services.Effects().FlyMoney(Data.rewards[0].datas[1],null,Icon.transform.position,false,RefreshTop,true);
            }
            services.MoveX(Lifetime.Transform,1600,.7f,3,CompleteSlide);
        }
        public void CompleteSlide()
        {Claim.enabled=true;ClaimReward();Lifetime.GameObject.SetActive(ActionCache);}
        public void ClaimReward()
        {
            if(IsClaim)return;int reported;
            if(Type==OutgameTaskType.DailyTask)
            {reported=RewardId;Data.rewards[0].datas[0]=RewardId;var first=Data.rewards[0].datas;first[1]=Math.Max(MinimumCount,Data.rewards[0].datas[1]);}
            else{Data.rewards[0].datas[0]=RewardId;reported=Data.rewards[0].datas[0];}
            var callback=ClaimedCallback;if(callback!=null)callback(Data.id,Data.Uid);
            services.ReportActivityReward(Type.ToString(),Data.id.ToString(),reported.ToString(),Data.rewards[0].datas[1].ToString());
        }
        public void Dispose(){Lifetime.DestroyObject(Lifetime.GameObject);Lifetime.Dispose();}
    }
}
