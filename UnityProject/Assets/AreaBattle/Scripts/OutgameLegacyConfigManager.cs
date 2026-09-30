using System;
using System.Collections.Generic;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // Explicit shared legacy ConfigRead/AppSetting state; modern route is not represented here.
    public sealed class OutgameLegacyConfigReadState
    {
        public OutgameLegacyPrefabResource ConfigResource;
        public bool UseUnityJson,UseMemoryPack;
        readonly Action<string> error;
        public readonly OutgameLegacyConfigRead Reader;
        public OutgameLegacyConfigReadState(Action<string> error)
        {this.error=error;Reader=new OutgameLegacyConfigRead(Load,error);}
        TextAsset Load(string name)
        {
            if(ConfigResource==null)error("ResourcesInfo为NULL");
            return ConfigResource.LoadAsset<TextAsset>(name);
        }
    }
    // ConfigMgr30413/30427/30401. Supply shared state and actual service callbacks at composition.
    public sealed partial class OutgameLegacyConfigManager
    {
        readonly OutgameLegacyConfigReadState state;readonly OutgameLegacyConfigRead reader;
        readonly Func<string,string> online;readonly Func<int> campCount,shareKey;
        readonly Func<Dictionary<object,GuideConfig>> currentGuides;
        readonly Action<string,Action<OutgameLegacyPrefabResource>,object[]> loadMaterial;
        public bool IsInitialized {get;private set;}
        public readonly OutgameConfigGlobalValues Globals;
        public readonly Dictionary<int,Color> LineColors=new Dictionary<int,Color>();
        public readonly Dictionary<int,Color[]> SoldierColors=new Dictionary<int,Color[]>();
        public readonly Dictionary<int,Material> CampMaterials=new Dictionary<int,Material>();
        public readonly Dictionary<string,int> SceneResources=new Dictionary<string,int>();
        public readonly Dictionary<int,int> GuideLevels=new Dictionary<int,int>();
        public readonly List<string> ChineseNames=new List<string>();
        public OutgameLegacyConfigManager(OutgameLegacyConfigReadState state,OutgameConfigGlobalValues globals,
            Func<string,string> online,Func<int> campCount,Func<int> shareKey,
            Func<Dictionary<object,GuideConfig>> currentGuides,
            Action<string,Action<OutgameLegacyPrefabResource>,object[]> loadMaterial)
        {this.state=state;reader=state.Reader;Globals=globals;this.online=online;this.campCount=campCount;this.shareKey=shareKey;this.currentGuides=currentGuides;this.loadMaterial=loadMaterial;}
        public void SetConfigABRes(OutgameLegacyPrefabResource resource)=>state.ConfigResource=resource;
        public void Initialize()
        {
            state.UseUnityJson=true;state.UseMemoryPack=false;
            if(IsInitialized)return;
            ReadOriginalConfigs();
            OutgameConfigOnlineOverrides.Apply(funcSettingConfig,online);
            OutgameConfigCampPresentation.Initialize(dicCamp,campCount,LineColors,SoldierColors,
                (path,args)=>loadMaterial(path,resource=>OutgameConfigMaterialCompletion.Loaded(resource,CampMaterials),args));
            Globals.Initialize(dicGlobalValue,shareKey);
            OutgameConfigDerivedIndexes.RebuildSceneResources(dicSceneSkin,SceneResources);
            OutgameConfigDerivedIndexes.RebuildGuideLevels(GuideLevels,currentGuides);
            OutgameConfigDerivedIndexes.AppendChineseNames(dicAIName,ChineseNames);
            IsInitialized=true;
        }
    }
}
