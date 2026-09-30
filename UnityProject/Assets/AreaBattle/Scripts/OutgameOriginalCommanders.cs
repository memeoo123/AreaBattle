using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameOriginalCommanders
    {
        [Serializable] sealed class Data {public List<Commander> commanderDatas=new List<Commander>();public int UsedCommanderId=1;}
        [Serializable] sealed class Commander {public bool isNew;public int Id,curLevel;public List<Skill> skillsData=new List<Skill>();}
        [Serializable] sealed class Skill {public int skillId,skillLevel;}
        public static void ReadCurrent(OutgameProfile profile,string json,string commanderConfigJson)
        {
            var data=new Data();
            if(!string.IsNullOrEmpty(json)&&json.Trim()!="null")JsonUtility.FromJsonOverwrite(json,data);
            profile.usedCommanderId=data.UsedCommanderId;profile.commanders=new List<OutgameCommanderState>();
            foreach(var old in data.commanderDatas)
            {
                var held=new OutgameCommanderState{id=old.Id,level=old.curLevel,isNew=old.isNew};
                if(old.skillsData==null){held.skillIds=null;held.skillLevels=null;}
                else
                {
                    held.skillIds=new int[old.skillsData.Count];held.skillLevels=new int[old.skillsData.Count];
                    for(int i=0;i<held.skillLevels.Length;i++){held.skillIds[i]=old.skillsData[i].skillId;held.skillLevels[i]=old.skillsData[i].skillLevel;}
                }
                profile.commanders.Add(held);
            }
            OutgameProfileInitialization.InitializeCommanders(profile,commanderConfigJson);
        }
        public static string WriteCurrent(int usedCommanderId,IEnumerable<OutgameCommanderState> commanders)
        {
            var data=new Data{UsedCommanderId=usedCommanderId};
            foreach(var held in commanders)
            {
                var row=new Commander{Id=held.id,curLevel=held.level,isNew=held.isNew};
                if(held.skillIds==null)row.skillsData=null;
                else for(int i=0;i<held.skillIds.Length;i++)row.skillsData.Add(new Skill{skillId=held.skillIds[i],skillLevel=held.skillLevels[i]});
                data.commanderDatas.Add(row);
            }
            return JsonUtility.ToJson(data);
        }
        // CommanderManager.DealOldData replaces its held data only for a nonempty old list.
        // No merge-by-max, equip normalization, spending or save is performed by the source method.
        public static bool ApplyLegacy(OutgameProfile profile,string originalJson,string commanderConfigJson)
        {
            if(profile==null)throw new ArgumentNullException(nameof(profile));
            if(originalJson==null||originalJson.Trim()=="null")return false;
            var data=new Data();JsonUtility.FromJsonOverwrite(originalJson,data);
            if(data.commanderDatas==null)throw new InvalidOperationException("Original commanderDatas is null; source expects a list.");
            if(data.commanderDatas.Count==0)return false;
            var candidate=new OutgameProfile{usedCommanderId=data.UsedCommanderId};
            foreach(var old in data.commanderDatas)
            {
                if(old==null||old.skillsData==null)throw new InvalidOperationException("Invalid original commander record.");
                var held=new OutgameCommanderState{id=old.Id,level=old.curLevel,isNew=old.isNew,skillIds=new int[old.skillsData.Count],skillLevels=new int[old.skillsData.Count]};
                for(int i=0;i<held.skillLevels.Length;i++){held.skillIds[i]=old.skillsData[i].skillId;held.skillLevels[i]=old.skillsData[i].skillLevel;}
                candidate.commanders.Add(held);
            }
            OutgameProfileInitialization.InitializeCommanders(candidate,commanderConfigJson);
            profile.commanders=candidate.commanders;profile.usedCommanderId=candidate.usedCommanderId;return true;
        }
    }
}
