using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle
{
    // AssetbundleModule._LoadAsset26239/26253. Public generic loader26236 is a separate route.
    public sealed class OutgameAssetbundleAsyncLoader
    {
        readonly Func<bool> checkLegacy;readonly Func<string,string,Type,OutgameLegacyAssetOperation> load;
        readonly Func<IEnumerator,Task> wait;readonly Action<object[]> error;
        public OutgameAssetbundleAsyncLoader(Func<bool> checkLegacy,Func<string,string,Type,OutgameLegacyAssetOperation> load,Action<object[]> error,Func<IEnumerator,Task> wait=null)
        {this.checkLegacy=checkLegacy;this.load=load;this.error=error;this.wait=wait??WaitNative;}
        static async Task WaitNative(IEnumerator operation){await OutgameUnityAwait.Await((OutgameLegacyAssetOperation)operation);}
        public async Task<T> LoadAsset<T>(string bundleName,string assetName)where T:UnityEngine.Object
        {
            checkLegacy(); // Source logs misuse but deliberately ignores the returned bool.
            var operation=load(bundleName,assetName,typeof(T));
            if(operation==null)return null;
            await wait(operation);
            var asset=operation.GetAsset<T>();
            if(asset==null)error(new object[]{"加载资源失败:"+bundleName+" AssName:"+assetName});
            return asset;
        }
    }
}
