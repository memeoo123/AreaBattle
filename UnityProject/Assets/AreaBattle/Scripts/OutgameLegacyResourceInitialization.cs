using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ResourcesModule.Initialize29875. The manager accessor preserves its production owner boundary.
    public sealed class OutgameLegacyResourceInitialization:IOutgameStartupModule
    {
        readonly Action checkRoute;readonly Func<object> manager;
        readonly Func<GameObject> createMono;readonly Func<IOutgameLegacyBundleServices> services;
        readonly Func<string,string> readText;readonly Func<string,List<string>> deserialize;
        readonly Func<List<string>,IEnumerator> firstPack;readonly Action<IEnumerator> start;
        readonly Action<string> log;
        public bool IsInitialized {get;private set;}
        public Action Initialized {get;set;}
        public object BundleManager {get;private set;}
        public GameObject MonoObject {get;private set;}
        public OutgameLegacyResourcesMono Mono {get;private set;}
        public OutgameLegacyResourceInitialization(Action checkRoute,Func<object> manager,Func<IOutgameLegacyBundleServices> services,
            Func<string,List<string>> deserialize,Func<List<string>,IEnumerator> firstPack,Action<IEnumerator> start,Action<string> log,
            Func<string,string> readText=null,Func<GameObject> createMono=null)
        {this.checkRoute=checkRoute;this.manager=manager;this.services=services;this.deserialize=deserialize??OutgameLegacyPackListJson.Deserialize;this.firstPack=firstPack;this.start=start;this.log=log;
            this.readText=readText??(name=>Resources.Load<TextAsset>(name).text);
            this.createMono=createMono??(()=>new GameObject("ResourcesMono",typeof(OutgameLegacyResourcesMono)));}
        public void Initialize()
        {
            if(IsInitialized){Initialized?.Invoke();return;}
            checkRoute();BundleManager=manager();MonoObject=createMono();
            if(MonoObject!=null)Mono=MonoObject.GetComponent<OutgameLegacyResourcesMono>();
            IsInitialized=true;
            var path=services().GetAssetBundleInfo("firstpack.unity3d").LocalPath;
            if(!string.IsNullOrEmpty(path))
            {
                var names=deserialize(readText("firstpack"));
                start(firstPack(names));return;
            }
            log("ResourcesModule初始化完成");Initialized?.Invoke();
        }
    }
}
