using System;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameSceneSkinEffects:IOutgameSkinItemEffects
    {
        void ChangeSceneStyle(int id,bool sourceFlag);
        void RefreshSceneCard(int id);
    }
    public sealed class OutgameSceneSkinActions
    {
        [Serializable] sealed class Statistics {public int BuyScene;}
        readonly OutgameSkinItemActions items;readonly OutgameToolDispatcher tools;
        readonly OutgameSkinCatalog skins;readonly IOutgameSceneSkinEffects effects;readonly Statistics statistics;
        public OutgameSceneSkinActions(OutgameSkinItemActions items,OutgameToolDispatcher tools,OutgameSkinCatalog skins,IOutgameSceneSkinEffects effects,string statisticsJson)
        {
            this.items=items??throw new ArgumentNullException(nameof(items));this.tools=tools??throw new ArgumentNullException(nameof(tools));
            this.skins=skins??throw new ArgumentNullException(nameof(skins));this.effects=effects??throw new ArgumentNullException(nameof(effects));statistics=JsonUtility.FromJson<Statistics>(statisticsJson);
        }
        public bool Buy(int id)
        {
            var row=items.Configuration(id);
            if(!tools.Change(row.castType,unchecked(-items.Price(id)))){effects.ShowMissingCurrency(row.castType);return false;}
            effects.PlayVoice(2017);effects.AddStatistic(statistics.BuyScene,1);
            skins.SetUsedSkin(4,id);effects.ChangeSceneStyle(id,false);
            tools.Change(id,1);effects.SetRedDot(id,skins.CheckSkinNew2Modify(id));effects.RefreshSceneCard(id);return true;
        }
        public void Select(int id)
        {
            if(skins.UsedSkin(4)==id)return;
            effects.PlayVoice(2001);skins.SetUsedSkin(4,id);effects.ChangeSceneStyle(id,false);
            effects.SetRedDot(id,skins.CheckSkinNew2Modify(id));effects.ReportToolUse(id,3);
        }
    }
}
