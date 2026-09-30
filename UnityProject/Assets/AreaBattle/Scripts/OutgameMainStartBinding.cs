using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace AreaBattle
{
    // Proj_xqzdStartUI.OpenLater33679 binds fields228/164, both to33685.
    // InitializeComponent33681 resolves those fields as btnStart / btnNewStart.
    public sealed class OutgameMainStartBinding:IDisposable
    {
        readonly Button first,second;readonly UnityAction clicked;
        public OutgameMainStartBinding(Transform page,Action<int,bool> setPlayState,Action<int,int> playVoice)
        {
            first=Find(page,"btnStart");second=Find(page,"btnNewStart");
            clicked=()=>{setPlayState(3,false);playVoice(1,2001);};
            first.onClick.AddListener(clicked);second.onClick.AddListener(clicked);
        }
        static Button Find(Transform page,string name)
        {
            Transform found=null;
            foreach(var item in page.GetComponentsInChildren<Transform>(true))if(item.name==name)
            {if(found!=null)throw new InvalidOperationException("Ambiguous original start button: "+name);found=item;}
            if(found==null)throw new InvalidOperationException("Missing original start button: "+name);
            return found.GetComponent<Button>()??found.gameObject.AddComponent<Button>();
        }
        public void Dispose(){if(first)first.onClick.RemoveListener(clicked);if(second)second.onClick.RemoveListener(clicked);}
    }
}
