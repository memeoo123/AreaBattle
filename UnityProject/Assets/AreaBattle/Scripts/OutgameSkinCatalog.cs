using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameSkinData {public int s,skinType,sortNo,special,castType;public bool u,isNew;}
    [Serializable] public sealed class OutgameUsedSkin {public int t,id;}
    [Serializable] public sealed class OutgameSkinState
    {
        public List<OutgameSkinData> skins=new List<OutgameSkinData>();
        public List<int> newSkins=new List<int>();
        public List<OutgameUsedSkin> usedSkin=new List<OutgameUsedSkin>();
    }
    public sealed class OutgameSkinCatalog
    {
        [Serializable] sealed class StoredSkin {public int s;public bool u;}
        [Serializable] sealed class StoredData
        {public List<StoredSkin> skins=new List<StoredSkin>();public List<int> newSkins;public List<OutgameUsedSkin> usedSkin;}
        // Original private s/u have SerializeField; all other SkinData fields are NonSerialized.
        public string ToOriginalJson()
        {
            PrepareSave();var data=new StoredData{newSkins=State.newSkins,usedSkin=State.usedSkin};
            foreach(var skin in State.skins)data.skins.Add(new StoredSkin{s=skin.s,u=skin.u});
            return JsonUtility.ToJson(data);
        }
        [Serializable] sealed class Rows {public Config[] Datas;}
        [Serializable] sealed class Config {public int id,skinType,sortNo,special,castType,lockState;}
        [Serializable] sealed class Old {public List<Soldier> list_playerskin=new List<Soldier>();public List<Scene> sceneMapDatas=new List<Scene>();}
        [Serializable] sealed class Soldier {public int skinId;public bool isUnlock;}
        [Serializable] sealed class Scene {public int sceneId;public bool isUnlock;}
        readonly Config[] soldiers,maps;
        readonly Dictionary<int,OutgameSkinData> soldierData=new Dictionary<int,OutgameSkinData>(),mapData=new Dictionary<int,OutgameSkinData>();
        readonly Dictionary<int,int> used=new Dictionary<int,int>();
        public OutgameSkinState State {get;}
        public IReadOnlyList<OutgameSkinData> OrderedSoldiers {get;}
        public IReadOnlyList<OutgameSkinData> OrderedScenes {get;}
        public static int SourceOrder(OutgameSkinData skin)
        {
            int derived=skin.s%100+(skin.skinType==4?0:1);
            return derived==1?-99:skin.sortNo!=0?skin.sortNo:derived;
        }
        public OutgameSkinCatalog(OutgameSkinState held,string soldierJson,string sceneJson,bool initializeUsedDefaults=false)
        {
            State=held??throw new ArgumentNullException(nameof(held));soldiers=JsonUtility.FromJson<Rows>(soldierJson).Datas;maps=JsonUtility.FromJson<Rows>(sceneJson).Datas;
            if(initializeUsedDefaults)for(int type=1;type<5;type++)State.usedSkin.Add(new OutgameUsedSkin{t=type,id=FreeSkin(type)});
            foreach(var row in State.usedSkin)used[row.t]=row.id;
            Initialize(soldiers,soldierData);Initialize(maps,mapData);
            // Source OrderBy(GetOrder).ToDictionary(key,value): stable ties retain held/config insertion order.
            OrderedSoldiers=Array.AsReadOnly(soldierData.Values.OrderBy(SourceOrder).ToArray());
            OrderedScenes=Array.AsReadOnly(mapData.Values.OrderBy(SourceOrder).ToArray());
        }
        public static OutgameSkinCatalog FromOriginal(string sourceJson,string soldierJson,string sceneJson)
        {
            bool fresh=string.IsNullOrEmpty(sourceJson)||sourceJson.Trim()=="null";var state=new OutgameSkinState();
            if(!fresh)JsonUtility.FromJsonOverwrite(sourceJson,state);
            return new OutgameSkinCatalog(state,soldierJson,sceneJson,fresh);
        }
        void Initialize(Config[] rows,Dictionary<int,OutgameSkinData> index)
        {
            var configs=new Dictionary<int,Config>();foreach(var row in rows)configs.Add(row.id,row);
            foreach(var held in State.skins)if(configs.ContainsKey(held.s)){held.isNew=State.newSkins.Contains(held.s);index.Add(held.s,held);}
            foreach(var config in rows)
            {
                if(!index.TryGetValue(config.id,out var held)){held=new OutgameSkinData{s=config.id,u=config.lockState==2};State.skins.Add(held);index.Add(held.s,held);}
                held.skinType=config.skinType;held.sortNo=config.sortNo;held.special=config.special;held.castType=config.castType;
            }
        }
        public int FreeSkin(int type)
        {
            foreach(var row in type==4?maps:soldiers)if(row.lockState==2&&(type==4||row.skinType==type))return row.id;
            return 0;
        }
        public int UsedSkin(int type)=>used[type];
        public OutgameSkinData Skin(int id)=>SoldierSkin(id)??SceneSkin(id)??throw new ArgumentOutOfRangeException(nameof(id),id,"Skin id does not exist");
        public bool CheckSkinNew2Modify(int id){Skin(id).isNew=false;return false;}
        public void SetUsedSkin(int type,int id){used[type]=id;}
        public int UnlockedSoldierCount()
        {
            int count=0;foreach(var skin in soldierData.Values)if(skin.skinType<=3&&skin.u)count++;return count;
        }
        // SkinManager.OnSave projects transient flags and equipment before serializing manager data.
        public void PrepareSave()
        {
            State.newSkins.Clear();foreach(var skin in State.skins)if(skin.isNew)State.newSkins.Add(skin.s);
            State.usedSkin.Clear();foreach(var pair in used)State.usedSkin.Add(new OutgameUsedSkin{t=pair.Key,id=pair.Value});
        }
        public OutgameSkinData SoldierSkin(int id)=>soldierData.TryGetValue(id,out var row)?row:null;
        public OutgameSkinData SceneSkin(int id)=>mapData.TryGetValue(id,out var row)?row:null;
        // Source DealOldData changes only matching u flags, including true->false. No equip/new/save side effects.
        public void ApplyLegacy(string originalLocalJson)
        {
            if(originalLocalJson==null||originalLocalJson.Trim()=="null")return;
            var old=new Old();JsonUtility.FromJsonOverwrite(originalLocalJson,old);
            foreach(var row in old.list_playerskin)if(soldierData.TryGetValue(row.skinId,out var held))held.u=row.isUnlock;
            foreach(var row in old.sceneMapDatas)if(mapData.TryGetValue(row.sceneId,out var held))held.u=row.isUnlock;
        }
    }
}
