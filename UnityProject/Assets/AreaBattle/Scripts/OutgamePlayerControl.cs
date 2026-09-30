using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public interface IOutgamePlayerInput
    {
        bool MouseDown {get;}bool MouseHeld {get;}bool MouseUp {get;}
        Vector3 MousePosition {get;}int ScreenHeight {get;}
        void AddTouchBegin(Action<float,float> callback);void RemoveTouchBegin(Action<float,float> callback);
    }
    // Native polling and shared source InputManager subscriptions. Explicit adapters remain available for validation.
    public sealed class OutgamePlayerInput:IOutgamePlayerInput
    {
        readonly Action<Action<float,float>> add,remove;
        public OutgamePlayerInput():this(OutgameInputManager.AddTouchBeginListener,OutgameInputManager.RemoveTouchBeginListener){}
        public OutgamePlayerInput(Action<Action<float,float>> add,Action<Action<float,float>> remove){this.add=add;this.remove=remove;}
        public bool MouseDown=>Input.GetMouseButtonDown(0);public bool MouseHeld=>Input.GetMouseButton(0);public bool MouseUp=>Input.GetMouseButtonUp(0);
        public Vector3 MousePosition=>Input.mousePosition;public int ScreenHeight=>Screen.height;
        public void AddTouchBegin(Action<float,float> callback)=>add(callback);public void RemoveTouchBegin(Action<float,float> callback)=>remove(callback);
    }
    public sealed class OutgamePlayerControl:IOutgameLogicControl
    {
        readonly OutgameControllerRegistry registry;readonly Func<OutgameGameControl> game;
        readonly Func<OutgameLevelControl> level;readonly Func<OutgameUiControl> ui;readonly Func<OutgameSkinCatalog> skins;
        readonly Func<OutgameMessageDispatcher> messages;readonly IOutgamePlayerInput input;readonly Action<object[]> warning;
        public readonly Dictionary<int,Transform> Roots=new Dictionary<int,Transform>();
        public readonly OutgameModelRotation Rotation;public readonly OutgamePlayerModels Models;public readonly OutgamePlayerAnimation Animation;
        public readonly OutgamePlayerSkinEntities SkinEntities;
        public int FirstAds1,FirstAds2,FirstAds3;
        public float Timer1,Timer2,Timer3,ModelScale=1f;
        public Vector3 ScreenPoint,SourceVector188;public RaycastHit LastHit;
        public OutgamePlayerControl(OutgameControllerRegistry registry,Func<OutgameGameControl> game,Func<OutgameLevelControl> level,Func<OutgameUiControl> ui,Func<OutgameSkinCatalog> skins,Func<OutgameMessageDispatcher> messages,IOutgamePlayerInput input,Func<OutgameLoadPrefabControl> loader,Func<OutgameLegacyConfigManager> config,string soldierConfig,Action<GameObject> color,Action<string> error,Action<object[]> warning)
        {
            this.registry=registry;this.game=game;this.level=level;this.ui=ui;this.skins=skins;this.messages=messages;this.input=input;this.warning=warning;
            Rotation=new OutgameModelRotation(Roots);
            Animation=new OutgamePlayerAnimation(skins,color,go=>go.GetComponent<OutgameBakedAnimator>(),error);
            Models=new OutgamePlayerModels(soldierConfig,Roots,Vector3.zero,1,(id,complete)=>loader().GetEntity(id,(go,args)=>complete(go)),(id,complete)=>loader().LoadAsset(id,(go,args)=>complete(go)),Animation.Refresh);
            Models.UninitializeModels();Models.LivePosition=()=>SourceVector188;Models.LiveScale=()=>ModelScale;
            SkinEntities=new OutgamePlayerSkinEntities(skins,config,()=>level().Resources,game);
        }
        public void OnInit()
        {messages().AddListener("LoadGameScreen",OnLoadGameScreen);messages().AddListener("RotateSoldier",OnRotateSoldier);input.AddTouchBegin(OnTouchBegin);}
        public void OnDispose()
        {registry.Clear(4462);Animation.ClearCache();messages().RemoveListener("LoadGameScreen",OnLoadGameScreen);messages().RemoveListener("RotateSoldier",OnRotateSoldier);input.RemoveTouchBegin(OnTouchBegin);}
        void OnLoadGameScreen(object[] args){game().InitScene();InitLoadModel();}
        void OnRotateSoldier(object[] args)=>Rotation.Select((int)args[0]);
        public void InitLoadModel()
        {
            var normal=game().SceneRoot.NormalGroup;var defense=game().SceneRoot.DefenseGroup;var attack=game().SceneRoot.AttackGroup;
            Models.ResetModels();Roots.Add(1,normal);Roots.Add(2,defense);Roots.Add(3,attack);
            Rotation.Capture(normal,attack,defense);
            Models.ChangeSkinUse(1,skins().UsedSkin(1));Models.ChangeSkinUse(2,skins().UsedSkin(2));Models.ChangeSkinUse(3,skins().UsedSkin(3));
        }
        public void Updata(float deltaTime,float unscaledDeltaTime)
        {
            if(level().State.PlayState==2)
            {
                Timer1+=deltaTime;Timer2+=deltaTime;Timer3+=deltaTime;
                if(Timer1>11.7f){Timer1=0;RefreshAnimation(1);}if(Timer2>13.7f){Timer2=0;RefreshAnimation(2);}if(Timer3>14.7f){Timer3=0;RefreshAnimation(3);}
            }
            if(ui().CurrentPage==2)Rotation.UpdateDrag(input,selected=>messages().SendMessage("ChooseSoldier",new object[]{selected}));
        }
        public void RefreshAnimation(int type)
        {try{Models.RefreshType(type);}catch(Exception ex){warning(new object[]{"类型：",type,ex.ToString()});}}
        public void InitFirstAdsItem(int type)
        {switch(type){case 0:FirstAds1=FirstAds2=FirstAds3=0;break;case 1:FirstAds1=0;break;case 2:FirstAds2=0;break;case 3:FirstAds3=0;break;}}
        public void SetSoldierActive(bool active){foreach(var pair in Roots)pair.Value.gameObject.SetActive(active);}
        public void ChangeSkinUse(int type,int id)=>Models.ChangeSkinUse(type,id);
        public int GetSkinEntityByType(int type)=>SkinEntities.GetSkinEntityByType(type);
        public int GetSkinEntityByRandomType(int type,int camp)=>SkinEntities.GetSkinEntityByRandomType(type,camp);
        public bool Raycast(float x,float y,Camera camera,string layer)
        {ScreenPoint.x=x;ScreenPoint.y=y;var ray=camera.ScreenPointToRay(ScreenPoint);int mask=1<<LayerMask.NameToLayer(layer);return Physics.Raycast(ray,out LastHit,float.MaxValue,mask);}
        public void OnTouchBegin(float x,float y)
        {
            if(level().State.PlayState!=2||ui().CurrentPage!=3)return;
            if(!Raycast(x,y,game().HomeCamera,"Scenes11"))return;
            if(LastHit.transform.GetComponent<Animator>()!=null)LastHit.transform.GetComponent<Animator>().SetTrigger("Trigger");
        }
    }
}
