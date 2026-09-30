// Generated from original metadata by analysis/generate_shared_item_schemas.py.
using System;
using System.Collections.Generic;
namespace AreaBattle.SharedItemConfig
{
    // Original type 3452
    [Serializable] public sealed class ListArrayInt
    {
        public int[] datas;
    }
    // Original type 3490
    [Serializable] public sealed class Lang
    {
        public string key;
    }
    // Original type 4527
    [Serializable] public sealed class GameItemConfig : IOutgameConfigRow
    {
        public int id;
        public int type1;
        public int type2;
        public string icon;
        public string useFeatureParam;
        public int quality;
        public Lang name;
        public Lang detail;
        public string item_game;
        public int paramInt;
        public string paramString;
        public int activeParam;
        public string useParam1;
        public string useParam2;
        public string useParam3;
        public int effectHours;
        public object UniqueID => id;
    }
    // Original type 4529
    [Serializable] public sealed class GamePackageConfig : IOutgameConfigRow
    {
        public int packageRewardId;
        public int packageId;
        public int rewardId;
        public int rewardRandom;
        public int rewardItemCount;
        public object UniqueID => packageRewardId;
    }
    // Original type 4531
    [Serializable] public sealed class GameProductConfig : IOutgameConfigRow
    {
        public int id;
        public int itemId;
        public Lang name;
        public Lang detail;
        public string icon;
        public int itemCount;
        public int[] buyTypeOrder;
        public Lang iapPrice;
        public string IapGameKey;
        public string price;
        public int videoBtnId;
        public int iapBuyTimes;
        public int adsBuyTimes;
        public int freeBuyTimes;
        public int[] costItemPriceTypes;
        public List<ListArrayInt> costItemPriceParams;
        public int[] costItemBuyTimesArray;
        public int buyLimit;
        public int storeID;
        public int groupID;
        public int order;
        public int fixedTag;
        public int forceRefreshTag;
        public int refreshType;
        public string[] refreshConditionParam;
        public int randomWeight;
        public int unEnoughShow;
        public Lang tag;
        public int[] refreshPeriod;
        public int Expired;
        public string param;
        public int getType;
        public int buyLimitParam;
        public int priceType;
        public int[] priceCalParam;
        public object UniqueID => id;
    }
    // Original type 4533
    [Serializable] public sealed class GameRewardConfig : IOutgameConfigRow
    {
        public int rewardItemId;
        public int rewardId;
        public int itemId;
        public int itemCountMin;
        public int itemCountMax;
        public int weight;
        public int rewardOrder;
        public object UniqueID => rewardItemId;
    }
}
