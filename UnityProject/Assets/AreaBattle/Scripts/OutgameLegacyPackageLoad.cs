using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameLegacyBundleResult
    {public AssetBundle Bundle;public int ReferenceCount;public string BundleName;public float LoadedAt;public float UnloadRemaining;}
    public interface IOutgameLegacyBundleOperation:IEnumerator
    {OutgameLegacyBundleResult GetAssetBundle();}
    // AssetResLoader.LoadFromPackage29855 and iterator29858. Manager transport remains explicit.
    public sealed class OutgameLegacyPackageLoad
    {
        readonly IDictionary<string,AssetBundle> firstPack;
        readonly Func<string,IOutgameLegacyBundleOperation> load;
        readonly Func<IEnumerator,object> start;
        readonly Action<string> log,error;readonly OutgameMessageDispatcher messages;
        public OutgameLegacyPackageLoad(IDictionary<string,AssetBundle> firstPack,Func<string,IOutgameLegacyBundleOperation> load,
            Func<IEnumerator,object> start,Action<string> log,Action<string> error,OutgameMessageDispatcher messages)
        {this.firstPack=firstPack;this.load=load;this.start=start;this.log=log;this.error=error;this.messages=messages;}
        public void Start(OutgameLegacyAssetLoader loader)=>start(Run(loader));
        public IEnumerator Run(OutgameLegacyAssetLoader loader)
        {
            if(loader.State==2)yield break;
            if(firstPack.ContainsKey(loader.BundleName))
            {
                loader.PendingBundle=firstPack[loader.BundleName];
                log("触发firstpackLoad:["+loader.BundleName+"],bundle["+loader.PendingBundle.name+"]");
                loader.Complete();yield break;
            }
            var operation=load(loader.BundleName);
            if(operation==null){loader.Error();yield break;}
            yield return start(operation);
            try
            {
                loader.PendingBundle=operation.GetAssetBundle().Bundle;
                if(loader.PendingBundle==null)loader.Error();
            }
            catch(Exception ex)
            {
                messages.SendMessage("GF_ResLoadError",new object[]{loader.BundleName});
                error("加载资源发生错误"+ex);
            }
            loader.Complete();
        }
    }
}
