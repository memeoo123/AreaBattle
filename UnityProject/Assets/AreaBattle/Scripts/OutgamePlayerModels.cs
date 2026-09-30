using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // PlayerControl f4886/f11065/f11067/f7427; callbacks f15166/f18946.
    // Providers must return instantiated objects, matching original GetEntity/LoadAsset callbacks.
    public sealed class OutgamePlayerModels
    {
        [Serializable] sealed class Rows { public Row[] Datas; }
        [Serializable] sealed class Row { public int id,skinType,prefabId; }
        readonly Dictionary<int,Row> configs=new Dictionary<int,Row>();
        Dictionary<int,Dictionary<int,GameObject>> cache=new Dictionary<int,Dictionary<int,GameObject>>();
        readonly IReadOnlyDictionary<int,Transform> roots;
        readonly Action<int,Action<GameObject>> entity,asset;
        readonly Action<IReadOnlyDictionary<int,GameObject>,int> refreshEquippedAnimation;
        readonly Vector3 position;readonly float scale;
        public Func<Vector3> LivePosition;public Func<float> LiveScale;
        public void UninitializeModels()=>cache=null;
        public void ResetModels()=>cache=new Dictionary<int,Dictionary<int,GameObject>>();
        public void RefreshType(int type)=>refreshEquippedAnimation(cache[type],type);
        public OutgamePlayerModels(string soldierConfig,IReadOnlyDictionary<int,Transform> roots,Vector3 position,float scale,
            Action<int,Action<GameObject>> entity,Action<int,Action<GameObject>> asset,
            Action<IReadOnlyDictionary<int,GameObject>,int> refreshEquippedAnimation)
        {
            this.roots=roots??throw new ArgumentNullException(nameof(roots));this.position=position;this.scale=scale;
            this.entity=entity??throw new ArgumentNullException(nameof(entity));this.asset=asset??throw new ArgumentNullException(nameof(asset));
            this.refreshEquippedAnimation=refreshEquippedAnimation??throw new ArgumentNullException(nameof(refreshEquippedAnimation));
            foreach(var row in JsonUtility.FromJson<Rows>(soldierConfig).Datas)configs.Add(row.id,row);
        }
        Dictionary<int,GameObject> Category(int type)
        {
            if(!cache.TryGetValue(type,out var models)){models=new Dictionary<int,GameObject>();cache.Add(type,models);}return models;
        }
        public GameObject Cached(int type,int id)=>cache.TryGetValue(type,out var models)&&models.TryGetValue(id,out var model)?model:null;
        public void ChangeSkinUse(int type,int id)
        {
            var models=Category(type);
            if(!models.ContainsKey(id))LoadModel(id,true,()=>Display(models,id,type));
            Display(models,id,type);
        }
        public void LoadModel(int id,bool active,Action complete)
        {
            if(!configs.TryGetValue(id,out var row))return;
            var parent=roots[row.skinType];int prefab=row.prefabId+(row.skinType==3?1000:0),aux=row.skinType==3?9034:9033;
            entity(prefab,model=>
            {
                model.SetActive(active);model.transform.SetParent(parent,false);model.transform.localPosition=LivePosition!=null?LivePosition():position;
                model.transform.localEulerAngles=Vector3.zero;model.transform.localScale=Vector3.one*(LiveScale!=null?LiveScale():scale);
                foreach(var child in model.GetComponentsInChildren<Transform>(true))child.gameObject.layer=parent.gameObject.layer;
                asset(aux,attachment=>
                {
                    attachment.transform.SetParent(model.transform,false);attachment.transform.localPosition=Vector3.zero;
                    Category(row.skinType)[id]=model;complete?.Invoke();
                });
            });
        }
        void Display(Dictionary<int,GameObject> models,int id,int type)
        {
            foreach(var pair in models)pair.Value.SetActive(pair.Key==id);
            refreshEquippedAnimation(models,type);
        }
    }
}
