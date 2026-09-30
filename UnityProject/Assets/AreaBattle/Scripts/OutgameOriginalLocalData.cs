using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // LocalDataManager.UpdateDataCallBack/DealUnsafeStr/DeadOldData only.
    // Keep the original payload for the separate skin/commander/other legacy consumers.
    public static class OutgameOriginalLocalData
    {
        [Serializable] sealed class Local
        {
            public int LevelID,goldNum,diamondsNum,strengthsNum,ToolValue,collectNum,collectAdNum;
            public List<OutgameToolCount> toolCounts=new List<OutgameToolCount>();
        }
        [Serializable] sealed class OldTools
        {
            public int iceNum=2,supportNum=2,toolnum_4=2,fireNum=2,upgradeNum=2,toolnum_5=2;
            public int Get(int id)
            {
                switch(id){case 2001:return iceNum;case 2002:return supportNum;case 2003:return toolnum_4;case 2004:return fireNum;case 2005:return upgradeNum;case 2006:return toolnum_5;default:return -1;}
            }
        }
        public static void Apply(OutgameProfile profile,string sourceJson,string allSkillJson)
        {
            if(profile==null)throw new ArgumentNullException(nameof(profile));
            var data=new Local();OldTools legacy=null;
            if(!string.IsNullOrEmpty(sourceJson)&&sourceJson.Trim()!="null")
            {
                string normalized=sourceJson.Replace("bank","collect");
                JsonUtility.FromJsonOverwrite(normalized,data);
                if(data.toolCounts==null)throw new InvalidOperationException("Original toolCounts is null; source expects a list.");
                if(data.toolCounts.Count==0){legacy=new OldTools();JsonUtility.FromJsonOverwrite(normalized,legacy);}
            }
            var inventory=new OutgameLocalInventoryState{goldNum=data.goldNum,diamondsNum=data.diamondsNum,strengthsNum=data.strengthsNum,ToolValue=data.ToolValue,collectNum=data.collectNum,collectAdNum=data.collectAdNum,toolCounts=data.toolCounts};
            new OutgameLocalInventory(inventory,allSkillJson);
            if(legacy!=null)foreach(var row in inventory.toolCounts){int value=legacy.Get(row.id);if(value!=-1)row.count=value;}
            // Commit only after parsing and source initialization succeed.
            profile.inventory=inventory;profile.levelID=data.LevelID;profile.sourceLocalDataJson=sourceJson;
        }
    }
}
