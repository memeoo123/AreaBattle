using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    // UIModule.LoadUI27392, callbacks27413/27415 and wait2Fram27418.
    public sealed class OutgameUiLegacyLoader
    {
        readonly Action<string,Action<OutgameLegacyPrefabResource>> load;
        readonly Func<string,Transform> node;
        readonly Action<IEnumerator> start;
        readonly Action<string> log;
        public OutgameUiLegacyLoader(Action<string,Action<OutgameLegacyPrefabResource>> load,
            Func<string,Transform> node,Action<IEnumerator> start,Action<string> log)
        {this.load=load;this.node=node;this.start=start;this.log=log;}
        public void Load(string path,string layer,Action<GameObject,object> complete)
        {
            load("UI/"+path,resource=>{
                log(string.Format("LoadPrefab{0}完成[{1}]",path,resource.Bundle));
                var name=path.Substring(path.LastIndexOf("/")+1);
                var instance=resource.Instantiate(name,false);
                instance.transform.SetParent(node(layer));
                var rect=instance.GetComponent<RectTransform>();
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
                rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                start(OutgameUiModernLoader.WaitTwoFrames(()=>complete?.Invoke(instance,resource)));
            });
        }
    }
    // Shares the same ResourcesModule acquisition route with root initialization27385.
    public sealed class OutgameUiLegacyRootResources:IOutgameUiRootResources
    {
        readonly Action<string,Action<OutgameLegacyPrefabResource>> load;
        readonly Action<float,float> update;
        public OutgameUiLegacyRootResources(Action<string,Action<OutgameLegacyPrefabResource>> load,Action<float,float> update)
        {this.load=load;this.update=update;}
        public void LoadPrefab(string path,Action<Func<string,bool,GameObject>> loaded)
            =>load(path,resource=>loaded(resource.Instantiate));
        public void Update(float deltaTime,float unscaledDeltaTime)=>update(deltaTime,unscaledDeltaTime);
    }
}
