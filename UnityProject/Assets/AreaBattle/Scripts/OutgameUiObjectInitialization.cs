using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // UIObject.initGameObject27428 with Normalize27429 and constructor27443.
    public sealed class OutgameUiObjectInitialization
    {
        readonly OutgameUiLifetime lifetime;readonly OutgameUiPage page;
        readonly Func<GameObject,IEnumerable<KeyValuePair<string,object>>> outlets;
        readonly Action normalize,initializeComponent,initialize,initializeSkin,awake;
        public bool IsInitialized {get;private set;}
        public Dictionary<string,GameObject> Objects=>(Dictionary<string,GameObject>)lifetime.ObjectList;
        public OutgameUiObjectInitialization(OutgameUiLifetime lifetime,OutgameUiPage page,
            Func<GameObject,IEnumerable<KeyValuePair<string,object>>> outlets,
            Action initializeComponent,Action initialize,Action initializeSkin,Action awake,Action normalize=null)
        {
            this.lifetime=lifetime;this.page=page;this.outlets=outlets;this.initializeComponent=initializeComponent;
            this.initialize=initialize;this.initializeSkin=initializeSkin;this.awake=awake;
            this.normalize=normalize??(()=>lifetime.Transform.localScale=Vector3.one);
            lifetime.ObjectList=new Dictionary<string,GameObject>();
        }
        public void Init(GameObject root)
        {
            lifetime.Attach(root,normalize);
            foreach(var outlet in outlets(root))Objects.Add(outlet.Key,outlet.Value as GameObject);
            lifetime.GameObject.SetActive(true);IsInitialized=true;
            initializeComponent();initialize();page.SetVisible(page.Visible);initializeSkin();awake();
        }
    }
}
