using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ShopUI source skin lists; provider binding is required for every instantiated card.
    public sealed class OutgameShopSkinLists:MonoBehaviour
    {
        readonly Dictionary<int,OutgameSkinItemView> cards=new Dictionary<int,OutgameSkinItemView>();
        readonly Dictionary<int,List<OutgameSkinItemView>> soldierGroups=new Dictionary<int,List<OutgameSkinItemView>>();
        readonly List<OutgameSkinItemView> sceneCards=new List<OutgameSkinItemView>();
        public OutgameSkinItemView Card(int id)=>cards[id];
        public int Count=>cards.Count;
        public void Build(OutgameSkinCatalog skins,int selectedSoldierType,Action<OutgameSkinItemView,int,int> bind)
        {
            if(bind==null)throw new ArgumentNullException(nameof(bind));
            foreach(var card in cards.Values){card.gameObject.SetActive(false);if(Application.isPlaying)Destroy(card.gameObject);else DestroyImmediate(card.gameObject);}cards.Clear();soldierGroups.Clear();sceneCards.Clear();
            var template=transform.Find("bottom/SkinItem");template.gameObject.SetActive(false);transform.Find("bottom/sceneSkinItem").gameObject.SetActive(false);
            var parents=new Dictionary<int,Transform>{
                {1,transform.Find("bottom/skinGroup_normal/Viewport/Content_normal")},
                {2,transform.Find("bottom/skinGroup_defense/Viewport/Content_defense")},
                {3,transform.Find("bottom/skinGroup_attack/Viewport/Content_attack")},
                {4,transform.Find("bottom/skinGroup_scene/Viewport/Content_scene")}};
            Action<OutgameSkinData> create=skin=>{
                var node=Instantiate(template,parents[skin.skinType],false);node.name=(skin.skinType==4?"SceneSkinItem_":"SkinItem_")+skin.s;
                var view=node.gameObject.AddComponent<OutgameSkinItemView>();cards.Add(skin.s,view);
                if(skin.skinType==4)sceneCards.Add(view);
                else{if(!soldierGroups.TryGetValue(skin.skinType,out var group))soldierGroups.Add(skin.skinType,group=new List<OutgameSkinItemView>());group.Add(view);}
                bind(view,skin.s,skin.skinType);node.gameObject.SetActive(true);
            };
            foreach(var skin in skins.OrderedSoldiers)if(skin.skinType==selectedSoldierType)create(skin);
            foreach(var skin in skins.OrderedSoldiers)if(skin.skinType!=selectedSoldierType)create(skin);
            foreach(var skin in skins.OrderedScenes)create(skin);
        }
        public void RefreshCategory(int type){foreach(var card in cards.Values)if(card.SkinType==type)card.Refresh();}
        public void Refresh(){foreach(var group in soldierGroups.Values)foreach(var card in group)card.Refresh();foreach(var card in sceneCards)card.Refresh();}
        // ShopUI.RefreshSkinStatus33798: reset all three first-ad references before refreshing any card.
        public void RefreshSkinStatus(Action<int> initializeFirstAdsItem)
        {if(initializeFirstAdsItem==null)throw new ArgumentNullException(nameof(initializeFirstAdsItem));initializeFirstAdsItem(0);Refresh();}
    }
}
