using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ResourcesModule29877/29878. Supply the same dictionary to PackageLoad and AtlasCache.
    public sealed class OutgameLegacyPackRegistry
    {
        readonly IDictionary<string,AssetBundle> packs;
        readonly Action<string> error;
        public OutgameLegacyPackRegistry(IDictionary<string,AssetBundle> packs,Action<string> error)
        {this.packs=packs;this.error=error;}
        public void Add(string assetBundleName,AssetBundle pack)
        {
            if(packs.ContainsKey(assetBundleName))
            {
                bool same=packs[assetBundleName]==pack;
                string packName=pack.name;
                if(same)error("资源[{aseetBundleName}]已经添加到pack["+packName+"]中，无需添加");
                else error(string.Format("错误的资源引用：资源[{{aseetBundleName}}]存在于多个pack中:[{0}] [{1}]",packName,packs[assetBundleName]));
                return;
            }
            packs.Add(assetBundleName,pack);
        }
        public void AddAll(List<string> names,AssetBundle pack)
        {for(int i=0;i<names.Count;i++)Add(names[i],pack);}
    }
    // ResourcesModule.LoadFirstPack29876 and iterator29913; completion reads the current callback.
    public sealed class OutgameLegacyFirstPack
    {
        readonly Func<string,IOutgameLegacyBundleOperation> load;
        readonly Func<IEnumerator,object> start;
        readonly OutgameLegacyPackRegistry registry;
        readonly Action<string> log,error;
        readonly Action completed;
        public OutgameLegacyFirstPack(Func<string,IOutgameLegacyBundleOperation> load,Func<IEnumerator,object> start,
            OutgameLegacyPackRegistry registry,Action<string> log,Action<string> error,Action completed)
        {this.load=load;this.start=start;this.registry=registry;this.log=log;this.error=error;this.completed=completed;}
        public IEnumerator Run(List<string> names)
        {
            log("加载firstpack");
            var operation=load("firstpack.unity3d");
            if(operation==null){error("FirstPackError，请检测首包资源是否存在");yield break;}
            yield return start(operation);
            var bundle=operation.GetAssetBundle().Bundle;
            registry.AddAll(names,bundle);
            log("ResourcesModule初始化完成");
            completed();
        }
    }
}
