using System;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
namespace AreaBattle
{
    public interface IOutgameGameSceneResources
    {
        void LoadAsset(int id,Action<GameObject> loaded,object[] arguments=null);
        void LoadTexture(string name,Action<Texture2D> loaded,object[] arguments=null);
        void LoadPrefab(string name,Action<GameObject> loaded,object[] arguments=null);
    }
    // GameControl4064: one owner for native scene references, selection and both resource caches.
    public sealed class OutgameGameControl:IOutgameLogicControl
    {
        readonly Func<OutgameLegacyConfigManager> config;
        readonly OutgameControllerRegistry registry;
        readonly IOutgameGameSceneResources resources;
        readonly Func<int> usedScene,width,height;
        readonly Func<float> heightAdjustment;
        readonly Action<int> notifyShop;
        readonly Func<string,GameObject[]> findTagged;
        public bool UseAnimationIns;
        public OutgameGameSceneMono SceneRoot;
        public Camera GameCamera,HomeCamera,CommanderCamera,UpgradeCamera;
        public Texture2D[] HomeTextures;
        public float Aspect;
        public float Boss3Skill1First,Boss3Skill1Hit;
        public readonly Dictionary<int,Sprite> GameSprites=new Dictionary<int,Sprite>(10);
        public SpriteRenderer GameBackground;
        public Transform GameBackgroundTransform;
        public Dictionary<int,GameObject> SceneEffects;
        public int CurrentSceneId=-1;
        public static readonly Vector3 BaseGameScale=new Vector3(.25f,0,.4f); // Original31268.
        public OutgameGameControl(Func<OutgameLegacyConfigManager> config,OutgameControllerRegistry registry,IOutgameGameSceneResources resources,Func<int> usedScene,Func<float> heightAdjustment,Action<int> notifyShop,Func<string,GameObject[]> findTagged=null,Func<int> width=null,Func<int> height=null)
        {this.config=config;this.registry=registry;this.resources=resources;this.usedScene=usedScene;this.heightAdjustment=heightAdjustment;this.notifyShop=notifyShop;this.findTagged=findTagged??GameObject.FindGameObjectsWithTag;this.width=width??(()=>Screen.width);this.height=height??(()=>Screen.height);}
        public void OnInit(){UseAnimationIns=false;HomeTextures=new Texture2D[config().dicSceneSkin.Count];}
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Original31254 is empty.
        public void OnDispose(){registry.Clear(4064);} // Source retains object fields and native resources.
        public void InitScene()
        {
            foreach(var found in findTagged("Tag10"))if(found.name=="objSceneRoot"){SceneRoot=found.GetComponent<OutgameGameSceneMono>();break;}
            GameCamera=SceneRoot.GameCamera;HomeCamera=SceneRoot.HomeCamera;CommanderCamera=SceneRoot.CommanderCamera;UpgradeCamera=SceneRoot.UpgradeCamera;
            SceneRoot.Scene_game.gameObject.SetActive(true);
            Aspect=width()/(height()+heightAdjustment());
            if(Aspect>.5625f){float scale=Aspect/.5625f;SceneRoot.Scene_home_CJroot.transform.localScale*=scale;SceneRoot.soldierRoot.GetChild(1).localScale*=scale;}
            else if(Aspect<.5625f){float scale=.5625f/Aspect;HomeCamera.orthographicSize=scale*.89f;SceneRoot.Scene_home_CJroot.transform.localScale*=scale;SceneRoot.soldierRoot.GetChild(1).localScale=SceneRoot.Scene_home_CJroot.transform.localScale;}
            ChangeSceneStyle(usedScene(),false);
        }
        public void HideSceneEffect(int offset)
        {if(SceneEffects!=null&&SceneEffects.TryGetValue(unchecked(offset+CurrentSceneId),out var effect))effect.SetActive(false);}
        public void ShowSceneEffect(int offset,Transform parent,bool game)
        {
            if(!config().dicSceneEffect.TryGetValue(CurrentSceneId,out var row))return;
            if(SceneEffects==null)SceneEffects=new Dictionary<int,GameObject>();
            int key=unchecked(offset+CurrentSceneId);
            if(SceneEffects.TryGetValue(key,out var cached)){cached.SetActive(true);return;}
            resources.LoadPrefab(game?row.gameEffect:row.idleEffect,effect=>{
                effect.transform.SetParent(parent,false);effect.SetActive(true);
                if(effect.TryGetComponent<SkeletonAnimation>(out var skeleton)){skeleton.Initialize(false,false);skeleton.AnimationState.SetAnimation(0,"animation",true);}
                SceneEffects.Add(key,effect);
            },new object[]{key,parent});
        }
        public void ChangeSceneStyle(int id,bool suppressShopRefresh)
        {
            if(CurrentSceneId!=id){
                HideSceneEffect(0);CurrentSceneId=id;int index=id-1;
                if(HomeTextures[index]==null)resources.LoadTexture(config().dicSceneSkin[id].idleIconName,texture=>{HomeTextures[index]=texture;ApplyHomeTexture(texture);},Array.Empty<object>());
                else ApplyHomeTexture(HomeTextures[index]);
            }
            if(!suppressShopRefresh)notifyShop(id);
        }
        void ApplyHomeTexture(Texture2D texture){SceneRoot.Scene_home_CJroot.sharedMaterial.mainTexture=texture;ShowSceneEffect(0,SceneRoot.Scene_home_CJroot.transform,false);}
        public void LoadGameScene(int id)
        {
            CurrentSceneId=id;
            if(GameBackground==null)resources.LoadAsset(11001,loaded=>{
                loaded.transform.SetParent(SceneRoot.Scene_game);loaded.transform.localPosition=Vector3.zero;loaded.transform.localRotation=Quaternion.identity;loaded.transform.localScale=Vector3.one;
                GameBackground=loaded.GetComponentInChildren<SpriteRenderer>();GameBackgroundTransform=GameBackground.transform;ApplyGameScene(GameBackground,id);
            },Array.Empty<object>());
            else ApplyGameScene(GameBackground,id);
        }
        void ApplyGameScene(SpriteRenderer renderer,int id)
        {
            if(!GameSprites.TryGetValue(CurrentSceneId,out var sprite))resources.LoadTexture(config().dicSceneSkin[id].gameIconName,texture=>{
                var loaded=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f));
                // Source callback caches under current selection at completion, not requested id.
                GameSprites.Add(CurrentSceneId,loaded);renderer.sprite=loaded;ShowSceneEffect(100,GameBackgroundTransform,true);
            },Array.Empty<object>());
            else{renderer.sprite=sprite;ShowSceneEffect(100,GameBackgroundTransform,true);}
        }
        public void PrepareHomeScene()
        {
            HideSceneEffect(100);SceneRoot.Scene_game.gameObject.SetActive(false);SceneRoot.Scene_home.gameObject.SetActive(true);
            HomeCamera.gameObject.SetActive(true);CommanderCamera.gameObject.SetActive(true);CommanderCamera.gameObject.SetActive(true);GameCamera.gameObject.SetActive(false);
        }
        public void PrepareGameScene()
        {
            SceneRoot.Scene_game.gameObject.SetActive(true);SceneRoot.Scene_home.gameObject.SetActive(false);
            HomeCamera.gameObject.SetActive(false);CommanderCamera.gameObject.SetActive(false);GameCamera.gameObject.SetActive(true);CommanderCamera.gameObject.SetActive(false);
        }
        public void MoveCamera(bool shop,float offset)
        {
            if(shop){var target=Vector3.up*offset;var p=SceneRoot.Scene_home.position;if(p.x==target.x&&p.y==target.y&&p.z==target.z)return;
                SceneRoot.soldierRoot.GetChild(1).gameObject.SetActive(true);SceneRoot.soldierRoot.localPosition=target;SceneRoot.Scene_home_CJroot.gameObject.SetActive(false);
            }else{SceneRoot.soldierRoot.GetChild(1).gameObject.SetActive(false);SceneRoot.soldierRoot.localPosition=Vector3.zero;SceneRoot.Scene_home_CJroot.gameObject.SetActive(true);}
        }
        public void SetCameraSize()
        {
            float ratio=width()/(float)height(),scale=.5625f/ratio;GameCamera.orthographicSize=scale*2.1f;var size=BaseGameScale;
            if(ratio<.5625f)size*=scale;else if(ratio>.5625f)size*=ratio*GameCamera.orthographicSize*2/2.5875f;
            SceneRoot.Scene_game.localScale=size;
        }
        public void ClearRenderModel(){for(int i=SceneRoot.objUnlockCommanderRoot.childCount-1;i>=0;i--)UnityEngine.Object.Destroy(SceneRoot.objUnlockCommanderRoot.GetChild(i).gameObject);}
    }
}
