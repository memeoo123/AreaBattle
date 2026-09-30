using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // AssetBundleManager.LoadDependencies23828. Manifest callback is GetAllDependencies.
    public sealed class OutgameLegacyBundleDependencies
    {
        readonly IDictionary<string,string[]> cache;readonly Func<bool> hasManifest,debug;
        readonly Func<string,string[]> getAll;readonly Func<string,string> remap;
        readonly Func<string,bool,bool> load;readonly Action<string> log,error;
        public OutgameLegacyBundleDependencies(IDictionary<string,string[]> cache,Func<bool> hasManifest,Func<string,string[]> getAll,
            Func<string,string> remap,Func<string,bool,bool> load,Func<bool> debug,Action<string> log,Action<string> error)
        {this.cache=cache;this.hasManifest=hasManifest;this.getAll=getAll;this.remap=remap;this.load=load;this.debug=debug;this.log=log;this.error=error;}
        public void Load(string name)
        {
            if(!hasManifest()){error("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");return;}
            if(!cache.TryGetValue(name,out var names))
            {
                names=getAll(name);if(names.Length==0)return;
                for(int i=0;i<names.Length;i++)names[i]=remap(names[i]);
                cache.Add(name,names);
            }
            foreach(var dependency in names)
            {
                if(debug())log("加载资源包["+name+"]的依赖资源["+dependency+"]");
                load(dependency,false);
            }
        }
    }
    // AssetBundleManager.RemapVariantName23871. Source compares dot segments0/1, not final extensions.
    public sealed class OutgameLegacyBundleVariants
    {
        readonly Func<string[]> variants,active;readonly Action<string> warning;
        public OutgameLegacyBundleVariants(Func<string[]> variants,Func<string[]> active,Action<string> warning)
        {this.variants=variants;this.active=active;this.warning=warning;}
        public string Remap(string name)
        {
            var candidates=variants();var split=name.Split('.');
            if(candidates==null)return name;
            int best=int.MaxValue,chosen=-1;
            for(int i=0;i<candidates.Length;i++)
            {
                var candidate=candidates[i].Split('.');if(candidate[0]!=split[0])continue;
                int rank=Array.IndexOf(active(),candidate[1]);if(rank==-1)rank=int.MaxValue-1;
                if(rank<best){best=rank;chosen=i;}
            }
            if(best==int.MaxValue-1)warning("Ambigious asset bundle variant chosen because there was no matching active variant: "+candidates[chosen]);
            return chosen==-1?name:candidates[chosen];
        }
    }
}
