using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Source4251, actual Awake/OnDestroy and native Button ownership.
    public sealed class OutgameTabButton:MonoBehaviour
    {
        public Action<bool> SelectChanged;
        public Action<OutgameTabButton> Click;
        public GameObject UnSelectGo,SelectGo;
        Button button;
        public bool IsSelect {get;private set;}
        void Awake()
        {button=GetComponent<Button>();if(button)button.onClick.AddListener(OnClick);else Debug.LogError("缺少Button组件");}
        void OnClick()=>Click?.Invoke(this);
        public void SetSelect(bool value){IsSelect=value;SelectChanged?.Invoke(value);Render(value);}
        public void SetSelectWithoutNotify(bool value){IsSelect=value;Render(value);}
        void Render(bool value){UnSelectGo.SetActive(!value);SelectGo.SetActive(value);}
        void OnDestroy(){if(button)button.onClick.RemoveListener(OnClick);}
    }
}
