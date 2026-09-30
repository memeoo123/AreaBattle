using System;
using System.Collections;
using UnityEngine;
namespace AreaBattle
{
    // UIModule.LoadUI27391 and callbacks27409/27411, wait2Fram27418.
    // Original resource acquisition and the legacy branch stay explicit module seams.
    public sealed class OutgameUiModernLoader:IOutgameUiLoader
    {
        readonly Func<string,Action<OutgameAssetHandle>,OutgameAssetHandle> loadOriginal;
        readonly Func<string,Transform> node;readonly Action<IEnumerator> start;
        readonly Action<string,string,Action<GameObject,object>> legacy;
        public OutgameUiModernLoader(Func<string,Action<OutgameAssetHandle>,OutgameAssetHandle> loadOriginal,
            Func<string,Transform> node,Action<IEnumerator> start,Action<string,string,Action<GameObject,object>> legacy)
        {this.loadOriginal=loadOriginal;this.node=node;this.start=start;this.legacy=legacy;}
        public OutgameAssetHandle LoadModern(string path,string layer,Action<GameObject,OutgameAssetHandle> complete)
        {
            return loadOriginal("UI/"+path,handle=>{
                var instance=handle.Instantiate();instance.SetActive(false);
                instance.transform.SetParent(node(layer));
                var rect=instance.GetComponent<RectTransform>();
                rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                start(WaitTwoFrames(()=>complete?.Invoke(instance,handle)));
            });
        }
        public void LoadLegacy(string path,string layer,Action<GameObject,object> complete)=>legacy(path,layer,complete);
        public static IEnumerator WaitTwoFrames(Action complete){yield return null;yield return null;complete?.Invoke();}
    }
}
