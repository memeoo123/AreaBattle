using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
namespace AreaBattle
{
    // GameControl f5063, callback f19605 and hide f7817.
    public sealed class OutgameSceneEffects
    {
        [Serializable] sealed class Rows { public Row[] Datas; }
        [Serializable] sealed class Row { public int sceneId; public string idleEffect,gameEffect; }
        readonly Dictionary<int,Row> rows=new Dictionary<int,Row>();
        readonly Dictionary<int,GameObject> cache=new Dictionary<int,GameObject>();
        readonly Action<string,Action<GameObject>> load;
        public OutgameSceneEffects(string config,Action<string,Action<GameObject>> load)
        {
            this.load=load??throw new ArgumentNullException(nameof(load));
            foreach(var row in JsonUtility.FromJson<Rows>(config).Datas)rows.Add(row.sceneId,row);
        }
        public void Hide(int sceneId,int offset=0)
        {
            if(cache.TryGetValue(unchecked(offset+sceneId),out var effect))effect.SetActive(false);
        }
        public void Show(int sceneId,Transform parent,int offset=0,bool game=false)
        {
            if(!rows.TryGetValue(sceneId,out var row))return;
            int key=unchecked(offset+sceneId);
            if(cache.TryGetValue(key,out var cached)){cached.SetActive(true);return;}
            load(game?row.gameEffect:row.idleEffect,effect=>
            {
                effect.transform.SetParent(parent,false);
                effect.SetActive(true);
                if(effect.TryGetComponent<SkeletonAnimation>(out var skeleton))
                {
                    skeleton.Initialize(false,false);
                    skeleton.AnimationState.SetAnimation(0,"animation",true);
                }
                // Original Add deliberately preserves duplicate pending-load failures.
                cache.Add(key,effect);
            });
        }
    }
}
