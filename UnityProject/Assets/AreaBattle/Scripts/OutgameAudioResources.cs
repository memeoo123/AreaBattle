using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // Audio helper3424:26373/26376,26374/26378,26375. Shared pack dictionary is owned by ResourcesModule.
    public sealed class OutgameAudioResources
    {
        readonly Func<bool> useNewResources;
        readonly Func<IDictionary<string,AssetBundle>> packs;
        readonly Func<string,Task<OutgameAssetHandle>> loadNew;
        readonly Func<string,string,Task<AudioClip>> loadLegacy;
        readonly Action<string> unload,debug;
        readonly Action<object[]> log;
        public OutgameAudioResources(Func<bool> useNewResources,Func<IDictionary<string,AssetBundle>> packs,
            Func<string,Task<OutgameAssetHandle>> loadNew,Func<string,string,Task<AudioClip>> loadLegacy,
            Action<string> unload,Action<string> debug=null,Action<object[]> log=null)
        {this.useNewResources=useNewResources;this.packs=packs;this.loadNew=loadNew;this.loadLegacy=loadLegacy;this.unload=unload;this.debug=debug??(s=>Debug.Log(s));this.log=log??(a=>Debug.Log(a[0]));}
        public async Task<AudioClip> LoadAudioClip(string path)
        {
            var assetName=OutgameResourcePaths.ToFileName(path);
            var lower=path.ToLower();
            if(useNewResources())return (await loadNew(lower)).MainObject as AudioClip;
            debug("加载音频："+lower);
            var bundleName=lower+".unity3d";
            debug("加载音频："+assetName);
            if(packs().ContainsKey(bundleName))
            {
                log(new object[]{"触发firstpackLoad:+"+bundleName});
                var clip=packs()[bundleName].LoadAsset<AudioClip>(assetName);
                log(new object[]{"音频文件:+"+(ReferenceEquals(clip,null)?null:clip.ToString())});
                return clip;
            }
            return await loadLegacy(bundleName,assetName);
        }
        public async Task<OutgameAssetHandle> LoadAudioClipForNewResource(string path)
        {
            OutgameResourcePaths.ToFileName(path); // Evaluated by source although the basename is unused.
            return await loadNew(path.ToLower());
        }
        public void UnloadAudioClip(string path)
        {var bundleName=path.ToLower()+".unity3d";if(!useNewResources())unload(bundleName);}
        public void Bind(OutgameAudioActionServices services)
        {services.UseNewResourceLoader=useNewResources;services.LoadAudioClip=LoadAudioClip;services.LoadAudioClipForNewResource=LoadAudioClipForNewResource;services.UnloadClip=UnloadAudioClip;}
    }
    // NewResLoadHelper26176/26186. The module's typed provider acquisition is an explicit boundary.
    public sealed class OutgameNewResourceAsyncHelper
    {
        readonly Func<bool> useNewResources;readonly Func<string,Type,Task<OutgameAssetHandle>> load;readonly Action<object[]> error;
        public OutgameNewResourceAsyncHelper(Func<bool> useNewResources,Func<string,Type,Task<OutgameAssetHandle>> load,Action<object[]> error)
        {this.useNewResources=useNewResources;this.load=load;this.error=error;}
        public async Task<OutgameAssetHandle> LoadAssetAsync<T>(string path)where T:UnityEngine.Object
        {
            if(!useNewResources()){error(new object[]{"当前不是新资源加载，请确认是否添加UseNewRes宏开启"});return null;}
            return await load(path,typeof(T));
        }
    }
}
