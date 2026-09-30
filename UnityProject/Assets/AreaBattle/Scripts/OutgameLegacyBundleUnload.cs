using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // AssetBundleManager.UnloadAssetBundle23821, UnloadDependencies, UnloadAssetBundleInternal and cleanup23851.
    public sealed class OutgameLegacyBundleUnload
    {
        readonly IDictionary<string,OutgameLegacyBundleResult> loaded;readonly IDictionary<string,string[]> dependencies;
        readonly OutgameLegacyBundleLookup lookup;readonly IList<OutgameLegacyBundleResult> pending;
        readonly List<OutgameLegacyBundleResult> expired=new List<OutgameLegacyBundleResult>();
        readonly Func<bool> disabled,debug;readonly Func<int> delay;readonly Func<float> delta;
        readonly Action<string> log;readonly Action<AssetBundle,bool> unload;
        public OutgameLegacyBundleUnload(IDictionary<string,OutgameLegacyBundleResult> loaded,IDictionary<string,string[]> dependencies,
            OutgameLegacyBundleLookup lookup,IList<OutgameLegacyBundleResult> pending,Func<bool> disabled,Func<int> delay,
            Func<float> delta,Func<bool> debug,Action<string> log,Action<AssetBundle,bool> unload=null)
        {this.loaded=loaded;this.dependencies=dependencies;this.lookup=lookup;this.pending=pending;this.disabled=disabled;this.delay=delay;this.delta=delta;this.debug=debug;this.log=log;this.unload=unload??((bundle,all)=>bundle.Unload(all));}
        public void Unload(string name)
        {
            UnloadInternal(name);
            if(dependencies.TryGetValue(name,out var names))foreach(var dependency in names)UnloadInternal(dependency);
        }
        void UnloadInternal(string name)
        {
            if(disabled())return;
            var item=lookup(name,out var error,out var missing);
            if(missing==1)loaded.TryGetValue(name,out item);
            if(item==null)return;
            item.ReferenceCount--;
            if(item.ReferenceCount!=0)return;
            item.UnloadRemaining=delay();pending.Add(item);
        }
        public void Update()
        {
            if(expired.Count>0)expired.Clear();
            for(int i=pending.Count-1;i>=0;i--)
            {
                pending[i].UnloadRemaining-=delta();
                if(pending[i].UnloadRemaining<=0)expired.Add(pending[i]);
            }
            foreach(var item in expired)
            {
                unload(item.Bundle,true);loaded.Remove(item.BundleName);
                if(debug())log(item.BundleName+" has been unloaded successfully");
                pending.Remove(item);
            }
        }
    }
}
