using System;
using UnityEngine;
using UnityEngine.U2D;
namespace AreaBattle
{
    public interface IOutgameAtlasResource {SpriteAtlas LoadAsset(string name);}
    // AtlasLoader22819/22842/22811 and async closure22847. Source uses one shared callback slot.
    public sealed class OutgameAtlasLoader
    {
        readonly Func<bool> modern,disabled;
        readonly Action<Action<string,Action<SpriteAtlas>>> subscribe,unsubscribe;
        readonly Func<string,string,SpriteAtlas> lookup;
        readonly Action<string,Action<IOutgameAtlasResource>> load;
        readonly Action<string> requestLog,log,warning;
        Action<SpriteAtlas> callback;
        public bool AutoLoadAtlas=true;
        public OutgameAtlasLoader(Func<bool> modern,Func<bool> disabled,Action<Action<string,Action<SpriteAtlas>>> subscribe,Action<Action<string,Action<SpriteAtlas>>> unsubscribe,Func<string,string,SpriteAtlas> lookup,Action<string,Action<IOutgameAtlasResource>> load,Action<string> requestLog,Action<string> log,Action<string> warning)
        {this.modern=modern;this.disabled=disabled;this.subscribe=subscribe;this.unsubscribe=unsubscribe;this.lookup=lookup;this.load=load;this.requestLog=requestLog;this.log=log;this.warning=warning;}
        public void OnEnable(){if(!modern())subscribe(Request);}
        public void OnDisable(){if(!modern())unsubscribe(Request);}
        public void Request(string tag,Action<SpriteAtlas> complete)
        {
            SpriteAtlas atlas=null;
            if(!disabled())
            {
                requestLog("RequestAtlas:"+tag);
                atlas=lookup(("UIAtlas/"+tag+".spriteatlas.unity3d").ToLower(),tag);
                if(atlas==null&&AutoLoadAtlas)
                {
                    callback=null;callback=complete;
                    log(tag+"图集未找到，重新加载");
                    load("UIAtlas/"+tag+".spriteatlas",resource=>{
                        if(resource==null)warning("图集加载失败");
                        log("图集名称"+tag);
                        atlas=resource.LoadAsset(tag);
                        log("图集重新加载成功："+atlas.name);
                        callback?.Invoke(atlas);
                    });
                    return;
                }
            }
            complete(atlas);
        }
    }
}
