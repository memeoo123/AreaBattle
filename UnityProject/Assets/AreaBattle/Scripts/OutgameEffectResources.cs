using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // NewResLoadHelper.LoadPrefabAsync26177/26188. Module acquisition is explicit.
    public sealed class OutgameNewPrefabAsyncHelper
    {
        readonly Func<bool> modern;readonly Func<string,Task<OutgameAssetHandle>> load;readonly Action<string> error;
        public OutgameNewPrefabAsyncHelper(Func<bool> modern,Func<string,Task<OutgameAssetHandle>> load,Action<string> error){this.modern=modern;this.load=load;this.error=error;}
        public async Task<OutgameAssetHandle> LoadPrefabAsync(string path)
        {
            if(!modern()){error("当前不是新资源加载，请确认是否添加UseNewRes宏开启");return null;}
            return await load(path);
        }
    }
    // AssetbundleModule.LoadPrefab26237/26249, shared object cache26234/26235.
    public sealed class OutgameAssetbundlePrefabLoader
    {
        readonly Func<bool> checkLegacy;readonly Func<string,string,Task<GameObject>> load;
        public readonly Dictionary<string,UnityEngine.Object> Loaded;
        public OutgameAssetbundlePrefabLoader(Func<bool> checkLegacy,Func<string,string,Task<GameObject>> load,Dictionary<string,UnityEngine.Object> loaded)
        {this.checkLegacy=checkLegacy;this.load=load;Loaded=loaded;}
        public async Task<GameObject> LoadPrefab(string path,string name=null)
        {
            checkLegacy(); // Source emits the wrong-route diagnostic but continues.
            if(string.IsNullOrEmpty(name))name=path.Substring(path.LastIndexOf("/",StringComparison.Ordinal)+1);
            path=(path+".prefab").ToLower()+".unity3d";string key=path+name;
            Loaded.TryGetValue(key,out var cached);GameObject prefab;
            if(cached!=null)prefab=cached as GameObject;
            else {prefab=await load(path,name);if(Loaded.ContainsKey(key))Loaded[key]=prefab;else Loaded.Add(key,prefab);}
            return UnityEngine.Object.Instantiate(prefab);
        }
    }
    // ConfigRead26612 Unity JSON route specialized to original EffectData.
    // Alternate JSON/MemoryPack and configuration acquisition are explicit host routes.
    public sealed class OutgameEffectConfigReader
    {
        [Serializable] sealed class Rows {public List<OutgameEffectData> Datas;}
        readonly Func<string,TextAsset> load;readonly Action<string> error;
        public OutgameEffectConfigReader(Func<string,TextAsset> load,Action<string> error){this.load=load;this.error=error;}
        public List<OutgameEffectData> Read()
        {
            var asset=load("EffectConfig");if(asset){string text=asset.text;return JsonUtility.FromJson<Rows>(text.Substring(text.IndexOf("{",StringComparison.Ordinal))).Datas;}
            error("配置文件不存在EffectConfig");return null;
        }
    }
    public static class OutgameEffectBinding
    {
        // Bind before TaskPanelUI.Awake copies the page's liveness services.
        public static void BindTask(Func<OutgameEffectModule> module,OutgameTaskLivenessServices services)
        {services.ShowEffect=(id,parent)=>module().Show(id,parent);services.CloseEffect=id=>module().Close(id);}
    }
}
