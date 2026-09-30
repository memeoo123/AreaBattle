using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // PlayerControl f11064 and completion f13441. Source SpineAnimator is a baked-mesh player.
    public interface IOutgameModelAnimator
    {
        void Reset();
        void Play(string name,bool loop);
        Action OnComplete { get; set; }
    }
    public sealed class OutgamePlayerAnimation
    {
        readonly Func<OutgameSkinCatalog> skins;
        readonly Action<GameObject> color;
        readonly Func<GameObject,IOutgameModelAnimator> find;
        readonly Action<string> error;
        readonly Dictionary<int,IOutgameModelAnimator> animators=new Dictionary<int,IOutgameModelAnimator>();
        public OutgamePlayerAnimation(OutgameSkinCatalog skins,Action<GameObject> color,Func<GameObject,IOutgameModelAnimator> find,Action<string> error)
:this(()=>skins,color,find,error){}
        public OutgamePlayerAnimation(Func<OutgameSkinCatalog> skins,Action<GameObject> color,Func<GameObject,IOutgameModelAnimator> find,Action<string> error)
        {
            this.skins=skins??throw new ArgumentNullException(nameof(skins));this.color=color??throw new ArgumentNullException(nameof(color));
            this.find=find??throw new ArgumentNullException(nameof(find));this.error=error??throw new ArgumentNullException(nameof(error));
        }
        public void ClearCache()=>animators.Clear();
        public int CachedCount=>animators.Count;
        public void Refresh(IReadOnlyDictionary<int,GameObject> models,int type)
        {
            int id=skins().UsedSkin(type);
            if(!models.TryGetValue(id,out var model))return;
            color(model);
            if(!model.activeSelf)return;
            if(!animators.TryGetValue(id,out var animator)){animator=find(model);animators.Add(id,animator);}
            if(animator==null||(animator is UnityEngine.Object native&&native==null)){error(model.name+" 没有SpineAnimtor");return;}
            animator.Reset();animator.Play("relax",false);
            animator.OnComplete=()=>
            {
                int equipped=skins().UsedSkin(type);
                (animators.TryGetValue(equipped,out var current)?current:animator).Play("idle",true);
            };
        }
    }
}
