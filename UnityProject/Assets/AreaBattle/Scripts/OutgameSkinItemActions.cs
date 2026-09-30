using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameSkinItemEffects
    {
        void PlayVoice(int id);
        void AddStatistic(int eventId,long delta);
        void ReportToolUse(int skinId,int category);
        void ChooseSkin(int skinId);
        void ShowMissingCurrency(int currencyId);
        void SetRedDot(int skinId,bool visible);
    }
    // SkinItem purchase/select/video callbacks. Button visibility supplies the source UI gates.
    public sealed class OutgameSkinItemActions
    {
        [Serializable] public sealed class Config {public int id,skinType,castType,price,DiamondPrice,special,actNo;public string iconName,tag;}
        [Serializable] sealed class Rows {public Config[] Datas;}
        [Serializable] sealed class Statistics {public int BuySkin;}
        readonly Dictionary<int,Config> configs=new Dictionary<int,Config>();
        readonly OutgameToolDispatcher tools;
        readonly OutgameSkinCatalog skins;
        readonly IOutgameSkinItemEffects effects;
        readonly Statistics statistics;
        public OutgameSkinItemActions(OutgameToolDispatcher tools,OutgameSkinCatalog skins,IOutgameSkinItemEffects effects,string soldierJson,string sceneJson,string statisticsJson)
        {
            this.tools=tools??throw new ArgumentNullException(nameof(tools));this.skins=skins??throw new ArgumentNullException(nameof(skins));this.effects=effects??throw new ArgumentNullException(nameof(effects));
            foreach(var row in JsonUtility.FromJson<Rows>(soldierJson).Datas)configs.Add(row.id,row);
            foreach(var row in JsonUtility.FromJson<Rows>(sceneJson).Datas)configs.Add(row.id,row);
            statistics=JsonUtility.FromJson<Statistics>(statisticsJson);
        }
        public Config Configuration(int id)=>configs[id];
        public int Price(int id){var row=configs[id];return row.castType==1001?row.price:row.castType==1002?row.DiamondPrice:0;}
        void ClearNew(int id)=>effects.SetRedDot(id,skins.CheckSkinNew2Modify(id));
        public bool Buy(int id)
        {
            var row=configs[id];
            if(!tools.Change(row.castType,unchecked(-Price(id)))){effects.ShowMissingCurrency(row.castType);return false;}
            effects.PlayVoice(2017);effects.AddStatistic(statistics.BuySkin,1);effects.ReportToolUse(id,3);
            tools.Change(id,1);effects.ChooseSkin(id);ClearNew(id);return true;
        }
        public void Select(int id,int currentItemState,Action onClick=null)
        {
            if(currentItemState!=0)return;
            effects.PlayVoice(2001);effects.ChooseSkin(id);effects.ReportToolUse(id,3);onClick?.Invoke();ClearNew(id);
        }
        public void VideoCompleted(int id,bool success,string originalRewardReason)
        {
            if(!success)return;
            effects.ReportToolUse(id,3);tools.Change(id,1,true,originalRewardReason);effects.ChooseSkin(id);ClearNew(id);
        }
    }
}
