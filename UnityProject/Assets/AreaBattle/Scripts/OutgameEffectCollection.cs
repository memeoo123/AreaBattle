using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgameCollectionEffect {void SetActive(bool active);}
    public interface IOutgameCollectionEffectModule
    {
        IOutgameCollectionEffect Get(int handle);
        int Show(int effectId,Vector3 position,Transform parent,int sourceSortingLayer,int sourceOption);
        void Close(int handle);
    }
    // Original EffectCellection4221. Misspelling retained on source Destory API.
    public sealed class OutgameEffectCollection
    {
        readonly Func<IOutgameCollectionEffectModule> module;readonly Action<GameObject> destroy;
        public Transform Root,Entity;
        public readonly Dictionary<int,int> Handles=new Dictionary<int,int>();
        public OutgameEffectCollection(Func<IOutgameCollectionEffectModule> module,Action<GameObject> destroy=null)
        {this.module=module;this.destroy=destroy??(go=>UnityEngine.Object.Destroy(go));}
        public void SetEntity(Transform entity)
        {
            Entity=entity;Root=null;
            if(Entity.childCount>=2)Root=Entity.Find("EffectCellection");
            if(Root==null){var created=new GameObject("EffectCellection");created.transform.SetParent(Entity,false);Root=created.transform;}
        }
        public void Add(int effectId,int handle)=>Handles.Add(effectId,handle);
        public void Display(int effectId)=>module().Get(Handles[effectId]).SetActive(true);
        public void Spawn(int effectId,int sourceOption)
        {
            if(Handles.ContainsKey(effectId)){Display(effectId);return;}
            if(Root!=null)Add(effectId,module().Show(effectId,Vector3.zero,Root,0,sourceOption));
        }
        public void Remove(int effectId)
        {if(Handles.TryGetValue(effectId,out int handle)){module().Close(handle);Handles.Remove(effectId);}}
        public void Clear()
        {
            if(Root==null)return;
            foreach(var pair in Handles)module().Close(pair.Value);
            Handles.Clear();
            for(int i=Root.childCount-1;i>=0;i--){var child=Root.GetChild(i).gameObject;if(child!=null)destroy(child);}
        }
        public void Destory()
        {Clear();if(Root!=null&&Root.gameObject!=null)destroy(Root.gameObject);Root=null;}
    }
}
