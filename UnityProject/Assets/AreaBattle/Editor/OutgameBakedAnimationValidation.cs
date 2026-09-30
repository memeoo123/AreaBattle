using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameBakedAnimationValidation
    {
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Five available soldier prefabs, baked animator timings and actual renderer parameters. Remaining models, shadows and production scene wiring pending."};
            GameObject go=null;
            try
            {
                foreach(string name in new[]{"soldier_100","soldier_102","soldier_200","soldier_300","soldier_400"})
                {
                    var prefab=Resources.Load<GameObject>("Recovered/Outgame/Models/"+name);Require(prefab!=null,"original model imported");
                    Require(prefab.GetComponent<OutgameBakedAnimator>()!=null&&prefab.GetComponent<RecoveredSoldierVisual>()==null,"separate outgame animator preserves battle prefab");
                    Require(prefab.GetComponent<MeshFilter>().sharedMesh==Resources.Load<GameObject>((name=="soldier_400"?"Recovered/Outgame/ModelAssets/":"Recovered/Soldiers/")+name).GetComponent<MeshFilter>().sharedMesh,"original shared geometry retained");
                }
                var attack=Resources.Load<GameObject>("Recovered/Outgame/Models/soldier_400");var attackAnimator=attack.GetComponent<OutgameBakedAnimator>();
                Require(attack.GetComponent<MeshFilter>().sharedMesh.vertexCount==256&&attack.transform.localScale==Vector3.one,"original attack display geometry and prefab scale");
                Require(attackAnimator.Clips.Length==2&&attackAnimator.Clips[0].name=="idle"&&attackAnimator.Clips[1].name=="relax"&&attackAnimator.Clips[1].startFrame==121&&attackAnimator.Clips[1].lengthFrames==73,"attack display uses original idle/relax clip table");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-attack-display-model400",result="pass"});
                var backdropTest=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/BossEmbedded/meshHomeScene_4"));
                var tempMaterial=new Material(backdropTest.GetComponent<MeshRenderer>().sharedMaterial);backdropTest.GetComponent<MeshRenderer>().sharedMaterial=tempMaterial;
                var tex1=new Texture2D(1,1);var tex2=new Texture2D(1,1);
                try
                {
                    var textureCallbacks=new System.Collections.Generic.List<Action<Texture>>();var events=new System.Collections.Generic.List<string>();
                    var style=new OutgameSceneStyle(BattleView.ReadText("Data/Outgame/SceneSkinConfig"),backdropTest.GetComponent<MeshRenderer>(),(name,done)=>{events.Add("load:"+name);textureCallbacks.Add(done);},id=>events.Add("hide:"+id),(id,parent)=>events.Add("effect:"+id),id=>events.Add("shop:"+id));
                    style.Change(1,false);Require(string.Join("|",events)=="hide:-1|load:scene_skin_idle1|shop:1","shop refresh occurs before pending texture callback");
                    style.Change(2,true);textureCallbacks[1](tex2);Require(tempMaterial.mainTexture==tex2&&events[events.Count-1]=="effect:2","new selection callback applies texture and effect");
                    textureCallbacks[0](tex1);Require(tempMaterial.mainTexture==tex1&&style.CurrentId==2&&events[events.Count-1]=="effect:2","late callback writes old texture while effects read current ID");
                    events.Clear();style.Change(2,false);Require(string.Join("|",events)=="shop:2"&&tempMaterial.mainTexture==tex1,"same selection skips texture/effect work but refreshes shop");
                    events.Clear();style.Change(1,false);Require(string.Join("|",events)=="hide:2|effect:1|shop:1"&&textureCallbacks.Count==2&&tempMaterial.mainTexture==tex1,"cached texture path is synchronous");
                    report.checks.Add(new BattleBuild.Check{id="outgame-scene-style-cache-late-callback-shop-order",result="pass"});
                    var realStyle=new OutgameSceneStyle(BattleView.ReadText("Data/Outgame/SceneSkinConfig"),backdropTest.GetComponent<MeshRenderer>(),OutgameSceneTextures.Load,id=>{},(id,parent)=>{},id=>{});
                    for(int id=1;id<=15;id++)
                    {
                        realStyle.Change(id,true);string name="scene_skin_idle"+(id>=10?id+1:id);
                        Require(tempMaterial.mainTexture==Resources.Load<Texture2D>("Recovered/Outgame/SceneTextures/"+name),"actual source scene ID to idle texture mapping");
                        Require(tempMaterial.mainTexture.width>0&&tempMaterial.mainTexture.height>0,"native texture loaded");
                    }
                    report.checks.Add(new BattleBuild.Check{id="outgame-all15-original-scene-texture-resource-binding",result="pass"});

                }
                finally{UnityEngine.Object.DestroyImmediate(backdropTest);UnityEngine.Object.DestroyImmediate(tempMaterial);UnityEngine.Object.DestroyImmediate(tex1);UnityEngine.Object.DestroyImmediate(tex2);}
                var effectParent=new GameObject("effect-parent");var otherParent=new GameObject("other-parent");
                var effectObjects=new System.Collections.Generic.List<GameObject>();
                try
                {
                    var pendingEffects=new System.Collections.Generic.List<Action<GameObject>>();var effectPaths=new System.Collections.Generic.List<string>();
                    var effects=new OutgameSceneEffects(BattleView.ReadText("Data/Outgame/SceneEffectConfig"),(path,done)=>{effectPaths.Add(path);pendingEffects.Add(done);});
                    effects.Show(1,effectParent.transform);effects.Hide(-1);Require(pendingEffects.Count==0,"unconfigured scene has no effect");
                    effects.Show(7,effectParent.transform);effects.Show(7,otherParent.transform);
                    Require(pendingEffects.Count==2&&effectPaths[0]=="effect/eff_idle_GuBao","pending requests are not deduplicated");
                    var first=new GameObject("first");effectObjects.Add(first);first.SetActive(false);first.transform.localPosition=new Vector3(2,3,4);first.transform.localScale=Vector3.one*2;
                    effects.Hide(7);pendingEffects[0](first);
                    Require(first.activeSelf&&first.transform.parent==effectParent.transform&&first.transform.localPosition==new Vector3(2,3,4)&&first.transform.localScale==Vector3.one*2,"late source callback preserves local transform and activates");
                    effects.Hide(7);Require(!first.activeSelf,"hide existing effect");effects.Show(7,otherParent.transform);
                    Require(first.activeSelf&&first.transform.parent==effectParent.transform&&pendingEffects.Count==2,"cached effect reactivates without reparent or reload");
                    var duplicate=new GameObject("duplicate");effectObjects.Add(duplicate);bool duplicateFailed=false;
                    try{pendingEffects[1](duplicate);}catch(ArgumentException){duplicateFailed=true;}
                    Require(duplicateFailed&&duplicate.activeSelf&&duplicate.transform.parent==otherParent.transform,"duplicate completion activates then fails dictionary Add");
                    report.checks.Add(new BattleBuild.Check{id="outgame-scene-effect-cache-late-callback-duplicate-add",result="pass"});
                    effects.Show(7,otherParent.transform,100,true);Require(effectPaths[2]=="effect/eff_game_GuBao","game effect uses offset cache and source game path");
                    var battleEffect=new GameObject("game");effectObjects.Add(battleEffect);pendingEffects[2](battleEffect);effects.Hide(7);
                    Require(!first.activeSelf&&battleEffect.activeSelf,"offset caches hide independently");effects.Hide(7,100);Require(!battleEffect.activeSelf,"offset hide uses same key");
                    effects.Show(8,effectParent.transform);effects.Show(9,effectParent.transform);
                    Require(effectPaths[3]=="effect/idle_eff_HuoShan"&&effectPaths[4]=="effect/eff_idle_ZhaoZe","all original scene effect names retained");
                    report.checks.Add(new BattleBuild.Check{id="outgame-scene-effect-source-config-offset-cache",result="pass"});
                }
                finally{foreach(var effect in effectObjects)UnityEngine.Object.DestroyImmediate(effect);UnityEngine.Object.DestroyImmediate(effectParent);UnityEngine.Object.DestroyImmediate(otherParent);}
                var actualEffectParent=new GameObject("original-effect-validation");
                try
                {
                    var nativeEffects=new OutgameSceneEffects(BattleView.ReadText("Data/Outgame/SceneEffectConfig"),OutgameSceneEffectAssets.Load);
                    foreach(int id in new[]{7,8,9})nativeEffects.Show(id,actualEffectParent.transform);
                    Require(actualEffectParent.transform.childCount==3,"three exact native effect prefabs loaded");
                    var skeletons=actualEffectParent.GetComponentsInChildren<Spine.Unity.SkeletonAnimation>(true);
                    Require(skeletons.Length==2,"two original root Spine components");
                    foreach(var skeleton in skeletons)
                    {
                        Require(skeleton.Skeleton.Data.Version=="4.1.16"&&skeleton.AnimationState.GetCurrent(0).Animation.Name=="animation"&&skeleton.AnimationState.GetCurrent(0).Loop,"source root initialization selects animation loop");
                        skeleton.Update(.5f);skeleton.LateUpdate();
                        Require(skeleton.GetComponent<MeshFilter>().sharedMesh.vertexCount>0,"original skeleton generates rendered geometry");
                    }
                    Require(actualEffectParent.GetComponentsInChildren<ParticleSystem>(true).Length==6,"all six source volcano particle systems retained");
                    foreach(var renderer in actualEffectParent.GetComponentsInChildren<Renderer>(true))foreach(var material in renderer.sharedMaterials)
                        Require(material!=null&&material.shader!=null&&material.shader.name!="Hidden/InternalErrorShader","native effect renderer material and shader resolved");
                    nativeEffects.Hide(7);nativeEffects.Hide(8);nativeEffects.Hide(9);
                    foreach(Transform effect in actualEffectParent.transform)Require(!effect.gameObject.activeSelf,"loaded source effects deactivate by source ID");
                    nativeEffects.Show(7,actualEffectParent.transform);
                    Require(actualEffectParent.transform.childCount==3&&skeletons[0].gameObject.activeSelf,"real asset cache reuses imported instance");
                    report.checks.Add(new BattleBuild.Check{id="outgame-three-native-scene-effects-spine-particles-cache",result="pass"});
                }
                finally{UnityEngine.Object.DestroyImmediate(actualEffectParent);}
                var roots=Resources.Load<GameObject>("Recovered/Outgame/OriginalModelRoots").GetComponent<OutgameModelRoots>();
                Require(roots.Normal.localPosition==new Vector3(-4,.2f,0)&&roots.Defense.localPosition==new Vector3(4,.2f,0)&&roots.Attack.localPosition==new Vector3(0,-1,0),"original three model positions");
                Require(roots.Normal.localScale==Vector3.one*.85f&&roots.Attack.localScale==Vector3.one&&roots.Normal.gameObject.layer==31,"original scales and layer");
                Require(roots.Categories()[1]==roots.Normal&&roots.Categories()[2]==roots.Defense&&roots.Categories()[3]==roots.Attack,"GameSceneMono category references");
                Require(roots.HomeCamera.orthographic&&roots.HomeCamera.orthographicSize==.89f&&roots.HomeCamera.cullingMask==unchecked((int)3221225216),"original home camera projection and layer mask");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-model-roots-home-camera-bindings",result="pass"});
                foreach(var backdrop in new[]{roots.HomeBackdrop,roots.ModelBackdrop})
                {
                    var renderer=backdrop.GetComponent<MeshRenderer>();var mesh=backdrop.GetComponent<MeshFilter>().sharedMesh;
                    Require(renderer!=null&&mesh!=null&&mesh.name=="Quad","original built-in background mesh bound");
                    Require(renderer.sharedMaterial==UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/AreaBattle/Resources/Recovered/BossEmbedded/Materials/CAB-d416eeb0298976ad43e5b162291c08b7_-5357574502611821897.mat")&&renderer.sharedMaterial.mainTexture==UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AreaBattle/Resources/Recovered/BossEmbedded/Textures/CAB-91c0726113b3596120e43286294a0aee_915550828357872149.png"),"native source material and original idle1 background texture");
                }
                Require(roots.HomeBackdrop.GetComponent<MeshRenderer>().sharedMaterial==roots.ModelBackdrop.GetComponent<MeshRenderer>().sharedMaterial,"both native background nodes share original material asset");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-background-native-mesh-material-texture",result="pass"});

                var display=UnityEngine.Object.Instantiate(roots.gameObject);
                try
                {
                    var bindings=display.GetComponent<OutgameModelRoots>();string config=BattleView.ReadText("Data/Outgame/SkinConfig");
                    var skins=OutgameSkinCatalog.FromOriginal(null,config,BattleView.ReadText("Data/Outgame/SceneSkinConfig"));
                    var scene=new OutgameScenePresentation(bindings);scene.MoveCamera(true,.5f);
                    Require(bindings.SoldierRoot.localPosition==Vector3.up*.5f&&bindings.ModelBackdrop.gameObject.activeSelf&&!bindings.HomeBackdrop.gameObject.activeSelf,"source enter-shop scene toggles");
                    scene.MoveCamera(false,99);Require(bindings.SoldierRoot.localPosition==Vector3.zero&&!bindings.ModelBackdrop.gameObject.activeSelf&&bindings.HomeBackdrop.gameObject.activeSelf,"source return-home ignores offset");
                    scene.MoveCamera(true,0);Require(!bindings.ModelBackdrop.gameObject.activeSelf,"source guard reads Scene_home position, so target zero returns before toggling");
                    scene.MoveCamera(true,.000001f);Require(bindings.ModelBackdrop.gameObject.activeSelf&&bindings.SoldierRoot.localPosition.y==.000001f,"exact float guard does not use approximate Vector3 equality");
                    report.checks.Add(new BattleBuild.Check{id="outgame-source-scene-enter-exit-exact-guard",result="pass"});
                    var bg=bindings.HomeBackdrop.localScale;var modelBg=bindings.ModelBackdrop.localScale;float size=bindings.HomeCamera.orthographicSize;
                    scene.ApplyViewport(540,960,0);Require(bindings.HomeBackdrop.localScale==bg&&bindings.ModelBackdrop.localScale==modelBg&&bindings.HomeCamera.orthographicSize==size,"exact9to16 leaves scene sizing untouched");
                    scene.ApplyViewport(1080,960,0);Require(bindings.HomeBackdrop.localScale==bg*2&&bindings.ModelBackdrop.localScale==modelBg*2&&bindings.HomeCamera.orthographicSize==size,"wide screen scales both backgrounds independently");
                    bindings.HomeBackdrop.localScale=bg;bindings.ModelBackdrop.localScale=modelBg;
                    scene.ApplyViewport(270,960,0);Require(bindings.HomeBackdrop.localScale==bg*2&&bindings.ModelBackdrop.localScale==bg*2&&bindings.HomeCamera.orthographicSize==1.78f,"narrow screen copies main background scale to model background and expands camera");
                    bindings.HomeBackdrop.localScale=bg;bindings.ModelBackdrop.localScale=modelBg;bindings.HomeCamera.orthographicSize=size;
                    scene.ApplyViewport(540,480,480);Require(bindings.HomeBackdrop.localScale==bg,"source additive height adjustment participates in aspect");
                    scene.MoveCamera(false,0);
                    report.checks.Add(new BattleBuild.Check{id="outgame-source-scene-wide-narrow-aspect-branches",result="pass"});
                    var models=bindings.Connect(skins,config,()=>1);
                    foreach(int type in new[]{1,2,3})
                    {
                        var model=models.Cached(type,skins.UsedSkin(type));
                        Require(model!=null&&model.activeSelf&&model.transform.parent==bindings.Categories()[type]&&model.transform.localPosition==Vector3.zero&&model.transform.localScale==Vector3.one,"source initial equipped model/placement");
                        Require(model.CompareTag("Tag5")&&model.GetComponent<OutgameBakedAnimator>().Current.name=="relax","real color and animation adapter invoked");
                        string shadow=type==3?"QBDyShadow":"BBDyShadow";Require(model.transform.Find(shadow)!=null,"correct original auxiliary resource parented");
                    }
                    var rotation=new OutgameModelRotation(bindings);var mid=bindings.Attack.localPosition;var left=bindings.Normal.localPosition;var right=bindings.Defense.localPosition;
                    rotation.Select(1);Require(rotation.SelectedType==1&&bindings.Normal.localPosition==mid&&bindings.Attack.localPosition==right&&bindings.Defense.localPosition==left&&bindings.Normal.localScale==Vector3.one,"source next rotation wraps3to1");
                    rotation.Select(3);Require(rotation.SelectedType==3&&bindings.Attack.localPosition==mid&&bindings.Normal.localPosition==left&&bindings.Defense.localPosition==right,"source previous wrap1to3");
                    rotation.Select(2);Require(rotation.SelectedType==2&&bindings.Defense.localPosition==mid&&bindings.Attack.localPosition==left&&bindings.Normal.localPosition==right,"direct selection uses opposite direction");
                    rotation.Select(99);Require(rotation.SelectedType==2&&bindings.Defense.localPosition==mid,"invalid source event exits without slot changes");
                    rotation.Select(2);Require(bindings.Defense.localPosition==mid,"same selection no-op");
                    report.checks.Add(new BattleBuild.Check{id="outgame-source-model-category-rotation-slots",result="pass"});
                    Require(models.Cached(3,300).name=="soldier_400","display attack selects400 instead of battle300");
                    Require(models.Cached(1,100).transform.Find("BBDyShadow").GetComponent<Animator>().runtimeAnimatorController!=null,"native shadow animation controller retained");
                    skins.SetUsedSkin(1,102);models.ChangeSkinUse(1,102);Require(!models.Cached(1,100).activeSelf&&models.Cached(1,102).activeSelf,"real imported alternate skin changes model visibility");
                    report.checks.Add(new BattleBuild.Check{id="outgame-default-models-native-roots-shadows-color-animation",result="pass"});
                }
                finally{UnityEngine.Object.DestroyImmediate(display);}

                go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/Models/soldier_100"));var a=go.GetComponent<OutgameBakedAnimator>();float now=10;a.Clock=()=>now;
                var target=go.GetComponent<MeshRenderer>();var block=new MaterialPropertyBlock();block.SetColor("_Color",Color.red);target.SetPropertyBlock(block);
                a.Play("relax",false);target.GetPropertyBlock(block);var clip=a.Current;
                Require(clip.name=="relax"&&block.GetVector("_AnimTime")==new Vector4(164,134,1/clip.lengthSeconds,10)&&block.GetFloat("_AnimLoop")==0&&block.GetColor("_Color")==Color.red,"actual original relax clip and preserved tint");
                var colors=OutgameSoldierColor.FromRecovered();colors.Apply(go,2);target.GetPropertyBlock(block);
                var originalPalette=Resources.Load<GameObject>("Recovered/Soldiers/soldier_100").GetComponent<RecoveredSoldierVisual>().CampColors;
                Require(go.CompareTag("Tag6")&&block.GetColor("_Color")==originalPalette[2]&&target.sortingOrder==-5&&block.GetVector("_AnimTime").x==164,"source tag, color, sorting and preserved animation block");
                target.sortingOrder=12;block.SetColor("_Color",Color.magenta);target.SetPropertyBlock(block);colors.Apply(go,2);target.GetPropertyBlock(block);
                Require(target.sortingOrder==12&&block.GetColor("_Color")==Color.magenta,"matching source camp tag skips all mutations");
                colors.Apply(go,0);target.GetPropertyBlock(block);Require(go.CompareTag("Tag9")&&block.GetColor("_Color")==originalPalette[1]&&target.sortingOrder==-5,"out-of-range tag fallback and lower-bound color index");
                Require(OutgameSoldierColor.CampTag(6)=="Tag10"&&OutgameSoldierColor.CampTag(7)=="Tag9"&&OutgameSoldierColor.CampTag(-1)=="Tag9","source tag switch fallback");
                colors.Apply((MeshRenderer)null,1);
                report.checks.Add(new BattleBuild.Check{id="outgame-source-soldier-color-tag-short-circuit-renderer",result="pass"});
                int callbacks=0;a.OnComplete=()=>{Require(a.Current==null,"reset before callback");callbacks++;a.Play("idle",true);a.OnComplete=()=>callbacks+=100;};
                now=10+clip.lengthSeconds;a.Tick();Require(callbacks==0,"strict completion boundary");now+=.001f;a.Tick();Require(callbacks==1&&a.Current.name=="idle"&&a.Looping&&a.OnComplete==null,"callback-selected animation persists and callback assignment is cleared after invocation");
                report.checks.Add(new BattleBuild.Check{id="outgame-baked-original-prefabs-clip-renderer-completion",result="pass"});
                now=20;a.Play("relax",false);now=20.5f;a.Pause();now=30;a.Tick();target.GetPropertyBlock(block);Require(Mathf.Abs(block.GetVector("_AnimTime").w-29.5f)<.0001f,"pause maintains phase by shifting start timestamp");
                a.Play("run",false);Require(a.Current.name=="relax","paused play only queues name");a.Resume();Require(a.Current.name=="run"&&a.Looping&&!a.IsAnimationPaused,"resume restarts queued animation looping");
                a.Play("unknown",false);Require(a.Current==a.Clips[0],"unknown animation uses first clip");a.OnComplete=()=>callbacks++;a.Reset();Require(a.OnComplete!=null&&a.Current==null,"reset preserves completion callback");
                a.OnComplete=null;now=40;a.Play("relax",false);now=50;a.Tick();Require(a.Current.name=="idle"&&a.Looping,"default autoplay after nonloop completion without callback");
                report.checks.Add(new BattleBuild.Check{id="outgame-baked-pause-resume-fallback-and-autoplay",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-baked-animation",result="fail",detail=e.ToString()});}
            finally{if(go!=null)UnityEngine.Object.DestroyImmediate(go);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-baked-animation-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
