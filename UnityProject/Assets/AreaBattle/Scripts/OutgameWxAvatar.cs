using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace AreaBattle
{
    public interface IOutgameAvatarRequest
    {
        object Send();
        bool Succeeded {get;}
        string Error {get;}
        Texture2D GetContent();
    }
    public sealed class OutgameUnityAvatarRequest:IOutgameAvatarRequest
    {
        public readonly UnityWebRequest Request;
        public OutgameUnityAvatarRequest(string url){Request=UnityWebRequestTexture.GetTexture(url);}
        public object Send()=>Request.SendWebRequest();
        public bool Succeeded=>Request.result==UnityWebRequest.Result.Success;
        public string Error=>Request.error;
        public Texture2D GetContent()=>DownloadHandlerTexture.GetContent(Request);
        // Original iterator Dispose is empty; ownership is not changed with a using block.
    }
    public sealed class OutgameWxAvatarServices
    {
        public Func<string,Sprite> LoadSprite=Resources.Load<Sprite>;
        public Func<string,IOutgameAvatarRequest> Request=url=>new OutgameUnityAvatarRequest(url);
        public Action<IEnumerator> StartCoroutine;
        public Func<OutgameMessageDispatcher> Messages;
        public Action<string> Log=Debug.Log;
    }
    // Original WXAvatar9188. Failed URLs remain in Downloading, including their callbacks.
    public sealed class OutgameWxAvatar:IOutgameAvatarSource
    {
        readonly OutgameWxAvatarServices services;
        public bool Initialized {get;private set;}
        public readonly Dictionary<string,Texture2D> Textures=new Dictionary<string,Texture2D>();
        public readonly Dictionary<string,Sprite> Sprites=new Dictionary<string,Sprite>();
        public readonly Dictionary<string,Action<Texture2D>> TextureCallbacks=new Dictionary<string,Action<Texture2D>>();
        public readonly Dictionary<string,Action<Sprite>> SpriteCallbacks=new Dictionary<string,Action<Sprite>>();
        public readonly List<string> Downloading=new List<string>();
        public OutgameWxAvatar(OutgameWxAvatarServices services){this.services=services;}
        public void InitAtStartGame()
        {
            if(Initialized)return;
            var sprite=services.LoadSprite("EnterGameUI/defaultIcon");var textures=Textures;
            var texture=sprite.texture;textures.Add("default",texture);Sprites.Add("default",sprite);Initialized=true;
        }
        public void GetAvatar(string url,Action<Sprite> callback)=>TryGetIconSprite(url,callback);
        public void TryGetIconSprite(string url,Action<Sprite> callback)
        {
            if(Sprites.ContainsKey(url)){callback?.Invoke(Sprites[url]);return;}
            if(SpriteCallbacks.ContainsKey(url))SpriteCallbacks[url]+=callback;else SpriteCallbacks.Add(url,callback);
            DownLoadTexture(url);
        }
        public void DownLoadTexture(string url)
        {
            if(Downloading.Contains(url)){services.Log("downLoadingList 重复下载:"+url);return;}
            Downloading.Add(url);services.StartCoroutine(DownloadImage(url));
        }
        public IEnumerator DownloadImage(string url)
        {
            services.Log("开始下载:"+url);var request=services.Request(url);yield return request.Send();
            if(!request.Succeeded){services.Log("Error downloading image: "+request.Error);yield break;}
            try
            {
                var texture=request.GetContent();services.Log("下载成功:"+url);
                if(!texture)texture=Textures["default"];
                if(!Textures.ContainsKey(url))Textures.Add(url,texture);
                if(!Sprites.ContainsKey(url))Sprites.Add(url,Sprite.Create(Textures[url],new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f)));
            }
            catch(Exception)
            {
                var texture=Textures["default"];
                if(!Textures.ContainsKey(url))Textures.Add(url,texture);
                if(!Sprites.ContainsKey(url))Sprites.Add(url,Sprite.Create(Textures[url],new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f)));
            }
            if(TextureCallbacks.ContainsKey(url)){TextureCallbacks[url]?.Invoke(Textures[url]);TextureCallbacks.Remove(url);}
            if(SpriteCallbacks.ContainsKey(url)){SpriteCallbacks[url]?.Invoke(Sprites[url]);SpriteCallbacks.Remove(url);}
            services.Messages().SendMessage("Avatar_Refresh");
        }
    }
    // The original global singleton is represented by one account/platform composition owner.
    public sealed class OutgameWxAvatarRuntime
    {
        readonly OutgameWxAvatarServices services;readonly Func<string,string,string> getString;
        OutgameWxAvatar instance;
        public OutgameWxAvatar Instance=>instance??(instance=new OutgameWxAvatar(services));
        public OutgameWxAvatarRuntime(OutgameWxAvatarServices services,Func<string,string,string> getString)
        {this.services=services;this.getString=getString;}
        public string GetMyNickname()=>getString("myNickName","����ǳ�");
        public string GetMyAvatarUrl()=>getString("myAvatarUrl","default");
        public void InitAtStartGame()=>Instance.InitAtStartGame();
        public void BindTopInfo(OutgameTopInfoServices top,Func<OutgameUserPreferences> userPreferences)
        {top.MyNickname=GetMyNickname;top.MyAvatarUrl=GetMyAvatarUrl;top.Avatars=()=>Instance;top.SaveUserData=()=>userPreferences().OnSave();}
    }
    // Utils_PlayerPrefs55731/55732/55739 string path. Reads are not inserted into the
    // mixed-type cache; writes update it before calling the replaceable platform backend.
    public sealed class OutgamePlatformStringPreferences
    {
        public readonly Dictionary<string,object> Cache=new Dictionary<string,object>();
        public Func<string,string,string> Read;
        public Action<string,string> Write;
        public OutgamePlatformStringPreferences(Func<string,string,string> read,Action<string,string> write)
        {Read=read;Write=write;}
        public static OutgamePlatformStringPreferences Native()=>new OutgamePlatformStringPreferences(PlayerPrefs.GetString,PlayerPrefs.SetString);
        public string GetString(string key,string fallback)
        {if(Cache.ContainsKey(key)&&Cache[key] is string value)return value;return Read(key,fallback);}
        public void SetString(string key,string value){Cache[key]=value;Write(key,value);}
        public void ClearCache()=>Cache.Clear();
    }
}
