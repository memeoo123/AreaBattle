using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
namespace AreaBattle
{
    // LoadedAssetBundle reference and its source m_AssetBundle field; acquisition remains the bundle manager's responsibility.
    public sealed class OutgameLoadedAtlasBundle {public AssetBundle Bundle;}
    // AtlasLoader22833. Firstpack key membership suppresses the bundle-manager fallback, even for a failed LoadAsset.
    public sealed class OutgameAtlasCache
    {
        readonly IDictionary<string,AssetBundle> firstpack;
        readonly Func<string,OutgameLoadedAtlasBundle> loaded;
        readonly Func<bool> debugEnabled;readonly Action<string> warning;
        public OutgameAtlasCache(IDictionary<string,AssetBundle> firstpack,Func<string,OutgameLoadedAtlasBundle> loaded,Func<bool> debugEnabled,Action<string> warning)
        {this.firstpack=firstpack;this.loaded=loaded;this.debugEnabled=debugEnabled;this.warning=warning;}
        public SpriteAtlas Find(string bundleName,string atlasName)
        {
            if(firstpack.ContainsKey(bundleName))
            {
                var bundle=firstpack[bundleName];
                // Source constructs this string under the debug flag and discards it; retain the bundle.name access.
                if(debugEnabled())_ = "触发图集firstpackLoad:["+bundleName+"],bundle["+bundle.name+",AtlasName["+atlasName+"]]";
                return bundle.LoadAsset<SpriteAtlas>(atlasName);
            }
            var resource=loaded(bundleName);
            if(resource==null){warning("图集资源包未加载："+bundleName);return null;}
            SpriteAtlas atlas=null;
            // Original checks the managed pointer here, not Unity's overloaded object-null comparison.
            if(!ReferenceEquals(resource.Bundle,null))atlas=resource.Bundle.LoadAsset<SpriteAtlas>(atlasName);
            if(atlas==null){warning("图集资源加载失败："+atlasName);return null;}
            return atlas;
        }
    }
}
