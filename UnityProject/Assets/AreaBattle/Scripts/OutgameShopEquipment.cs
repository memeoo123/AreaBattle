using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ShopUI ChooseSkin listener f15997. SceneSkinItem uses its separate direct scene path.
    public sealed class OutgameShopEquipment
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id,skinType;}
        readonly Dictionary<int,int> types=new Dictionary<int,int>();
        readonly OutgameSkinCatalog skins;readonly Action<int> resetFirstAd,refreshCategory;
        readonly Action<int,int> changeModel;
        public OutgameShopEquipment(OutgameSkinCatalog skins,string soldierConfig,Action<int> resetFirstAd,Action<int> refreshCategory,Action<int,int> changeModel)
        {
            this.skins=skins??throw new ArgumentNullException(nameof(skins));this.resetFirstAd=resetFirstAd??throw new ArgumentNullException(nameof(resetFirstAd));
            this.refreshCategory=refreshCategory??throw new ArgumentNullException(nameof(refreshCategory));this.changeModel=changeModel??throw new ArgumentNullException(nameof(changeModel));
            foreach(var row in JsonUtility.FromJson<Rows>(soldierConfig).Datas)types.Add(row.id,row.skinType);
        }
        public void ChooseSkin(int id)
        {
            if(!types.TryGetValue(id,out int type))return;
            skins.SetUsedSkin(type,id);resetFirstAd(type);refreshCategory(type);changeModel(type,id);
        }
    }
}
