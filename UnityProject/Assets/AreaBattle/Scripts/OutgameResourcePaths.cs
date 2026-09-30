using System;
namespace AreaBattle
{
    // Original Path32611/32612 and UtilitHelper32693, including suffix spelling and first-dot split.
    public static class OutgameResourcePaths
    {
        public static string GetPath(string name,int type)
        {
            switch(type){
                case 0:return "Scene/"+name;
                case 1:return "Model/WayLine/"+name+".prefab";
                case 2:return "Model/Entity/"+name+".prefab";
                case 3:return "Model/Hero/"+name+".prefab";
                case 4:return "Data/LevelCfg/"+name+".Json";
                case 5:return "Materials/materials/"+name+".mat";
                case 6:return "Materials/Model/"+name+".mat";
                case 7:return "Animation/Soldier/"+name+".controller";
                case 8:return "Model/Scene/"+name+".prefab";
                case 9:return "Data/AnimationTexture/"+name+".bytes";
                case 10:return "Model/SoldierCommon/"+name+".prefab";
                case 11:return "Model/SoldierAnimationIns/"+name;
                case 12:return "Textures/skinscene/"+name+".png";
                default:return name;
            }
        }
        public static string ToFileName(string path)=>path.Substring(path.LastIndexOf("/")+1).Split('.')[0];
    }
    // ResLoadHelper26213/26214/26217/26220. This legacy route logs and returns when modern is active.
    public sealed class OutgameResLoadHelper
    {
        readonly Func<bool> modern;readonly Func<OutgameLegacyResourceScheduler> scheduler;readonly Action<string> error;
        public OutgameResLoadHelper(Func<bool> modern,Func<OutgameLegacyResourceScheduler> scheduler,Action<string> error){this.modern=modern;this.scheduler=scheduler;this.error=error;}
        public bool CheckUseRightResLoad(){bool value=modern();if(value)error("当前使用的是新资源加载，请使用NewResLoadHelper");return !value;}
        public OutgameLegacyResLoader LoadAsset(string path,Action<OutgameLegacyPrefabResource> complete,object[] args=null)
            =>CheckUseRightResLoad()?scheduler().LoadAsset(path,complete,args):null;
        public void LoadPrefab(string path,Action<OutgameLegacyPrefabResource> complete,object[] args=null)
        {if(CheckUseRightResLoad())LoadPrefabOri("Prefabs/"+path,complete,args);}
        public void LoadPrefabOri(string path,Action<OutgameLegacyPrefabResource> complete,object[] args=null)
        {if(CheckUseRightResLoad())scheduler().LoadPrefab(path,complete,args);}
    }
}
