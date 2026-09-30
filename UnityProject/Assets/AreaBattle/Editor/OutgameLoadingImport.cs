using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Animations;
namespace AreaBattle.EditorTools
{
    public static class OutgameLoadingImport
    {
        const string Folder="Assets/AreaBattle/Resources/Recovered/Loading";
        [Serializable] sealed class Manifest {public AnimationSource animation;}
        [Serializable] sealed class AnimationSource {public Key[] keys;public float duration,frameRate;public bool loop;public string path,stateName;public RendererSource rendererSource;}
        [Serializable] sealed class Key {public float time;public string spriteId;}
        [Serializable] sealed class RendererSource {public int m_SortingLayerID,m_SortingOrder,m_DrawMode,m_MaskInteraction,m_SpriteSortPoint;public Color m_Color;public bool m_FlipX,m_FlipY;public Vector2 m_Size;}
        static Sprite Sprite(string id)=>AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/Sprites/"+id.Replace(':','_')+".png");
        public static void Attach()
        {
            var source=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/loading-ui-import.json"))).animation;
            string clipPath=Folder+"/lodingSoldier.anim",controllerPath=Folder+"/Pulbic_man001.controller";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if(!clip){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,clipPath);}
            clip.name="lodingSoldier";clip.frameRate=source.frameRate;
            var keys=source.keys.Select(k=>new ObjectReferenceKeyframe{time=k.time,value=Sprite(k.spriteId)}).ToList();
            if(keys.Any(k=>k.value==null))throw new InvalidDataException("Missing original loading frame sprite");
            // Native PPtr clips hold each key for one sample; six keys at12fps already span0.5 seconds.
            AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"),keys.ToArray());
            var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=source.loop;settings.startTime=0;settings.stopTime=source.duration;AnimationUtility.SetAnimationClipSettings(clip,settings);EditorUtility.SetDirty(clip);
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if(!controller)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var machine=controller.layers[0].stateMachine;var state=machine.states.FirstOrDefault(s=>s.state.name==source.stateName).state;
            if(state==null)state=machine.AddState(source.stateName);state.motion=clip;state.speed=1;state.writeDefaultValues=true;machine.defaultState=state;
            string prefabPath=Folder+"/Proj_xqzdLoadingUI.prefab";var root=PrefabUtility.LoadPrefabContents(prefabPath);
            try{
                var page=root.GetComponent<OutgameLoadingPage>();if(!page)page=root.AddComponent<OutgameLoadingPage>();
                page.LoadingText=root.transform.Find("objs/LoadingProcess/txt_loading").GetComponent<Text>();page.Mask=root.transform.Find("objs/LoadingProcess/MASK").GetComponent<Image>();page.AnimationAnchor=(RectTransform)root.transform.Find(source.path);
                var renderer=page.AnimationAnchor.GetComponent<SpriteRenderer>();if(!renderer)renderer=page.AnimationAnchor.gameObject.AddComponent<SpriteRenderer>();var r=source.rendererSource;
                renderer.sprite=Sprite(source.keys[0].spriteId);renderer.sortingLayerID=r.m_SortingLayerID;renderer.sortingOrder=r.m_SortingOrder;renderer.color=r.m_Color;renderer.flipX=r.m_FlipX;renderer.flipY=r.m_FlipY;renderer.drawMode=(SpriteDrawMode)r.m_DrawMode;renderer.size=r.m_Size;renderer.maskInteraction=(SpriteMaskInteraction)r.m_MaskInteraction;renderer.spriteSortPoint=(SpriteSortPoint)r.m_SpriteSortPoint;
                var animator=page.AnimationAnchor.GetComponent<Animator>();if(!animator)animator=page.AnimationAnchor.gameObject.AddComponent<Animator>();animator.runtimeAnimatorController=controller;
                var auto=page.LoadingText.GetComponent<OutgameAutoCanvasLayer>();if(!auto)auto=page.LoadingText.gameObject.AddComponent<OutgameAutoCanvasLayer>();auto.OrderOffer=25;
                if(page.LoadingText.font==null||!page.Mask.sprite)throw new InvalidDataException("Loading text font and mask sprite must come from decoded source payloads");
                PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
            AssetDatabase.SaveAssets();
        }
    }
}
