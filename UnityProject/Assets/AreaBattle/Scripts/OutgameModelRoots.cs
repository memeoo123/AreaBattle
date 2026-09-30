using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameModelRoots:MonoBehaviour
    {
        public Transform Normal,Defense,Attack;
        public Transform SceneHome,SoldierRoot,ModelBackdrop,HomeBackdrop;
        public Camera HomeCamera,GameCamera,CommanderCamera;
        public Transform SceneGame;
        public OutgameHomeScenePresentation CreateHomeScenePresentation()
            =>new OutgameHomeScenePresentation(SceneHome,SceneGame,HomeCamera,CommanderCamera,GameCamera);
        public IReadOnlyDictionary<int,Transform> Categories()=>new Dictionary<int,Transform>{{1,Normal},{2,Defense},{3,Attack}};
        public OutgamePlayerModels Connect(OutgameSkinCatalog skins,string config,System.Func<int> currentCamp)
        {
            if(currentCamp==null)throw new System.ArgumentNullException(nameof(currentCamp));
            var colors=OutgameSoldierColor.FromRecovered();var assets=new OutgameModelAssets();
            var animation=new OutgamePlayerAnimation(skins,model=>colors.Apply(model,currentCamp()),model=>model.GetComponent<OutgameBakedAnimator>(),message=>Debug.LogError(message));
            var models=CreateModels(config,assets.Load,assets.Load,animation);
            for(int type=1;type<=3;type++)models.ChangeSkinUse(type,skins.UsedSkin(type));
            return models;
        }
        // PlayerControl constructor: source uniform scale1 and local position Vector3.zero.
        public OutgamePlayerModels CreateModels(string config,System.Action<int,System.Action<GameObject>> entity,System.Action<int,System.Action<GameObject>> asset,OutgamePlayerAnimation animation)
            =>new OutgamePlayerModels(config,Categories(),Vector3.zero,1,entity,asset,animation.Refresh);
    }
}
