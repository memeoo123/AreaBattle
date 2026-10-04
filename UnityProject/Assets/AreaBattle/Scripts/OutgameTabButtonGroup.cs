using System;
using UnityEngine;
namespace AreaBattle
{
    // Source4252. Programmatic selection visits all tabs; clicks visit only previous/current.
    public sealed class OutgameTabButtonGroup:MonoBehaviour
    {
        public Action<OutgameTabButton,OutgameTabButton> SelectionChanging;
        public OutgameTabButton[] Buttons;
        public OutgameTabButton Previous,Current;
        void Awake()=>Buttons=transform.GetComponentsInChildren<OutgameTabButton>();
        void Start(){if(Buttons!=null)for(int i=0;i<Buttons.Length;i++)Buttons[i].Click+=OnClick;}
        void OnClick(OutgameTabButton value)
        {
            Current=value;
            if(Previous!=Current)
            {
                SelectionChanging?.Invoke(Previous,Current);
                if(Previous)Previous.SetSelect(false);
                Current.SetSelect(true);
            }
            Previous=Current;
        }
        public void SetSelect(OutgameTabButton value)
        {Current=value;if(Buttons!=null)for(int i=0;i<Buttons.Length;i++){bool selected=Buttons[i]==value;Buttons[i].SetSelect(selected);}Previous=Current;}
        public void SetSelectWithoutNotify(OutgameTabButton value)
        {Current=value;if(Buttons!=null)for(int i=0;i<Buttons.Length;i++){bool selected=Buttons[i]==value;Buttons[i].SetSelectWithoutNotify(selected);}Previous=Current;}
        void OnDestroy()
        {if(Buttons!=null){for(int i=0;i<Buttons.Length;i++)Buttons[i].Click-=OnClick;Buttons=null;}Previous=null;Current=null;}
    }
}
