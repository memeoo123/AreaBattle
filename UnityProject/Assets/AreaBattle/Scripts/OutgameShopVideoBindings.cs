using System;
using UnityEngine;
namespace AreaBattle
{
    // Binds the original ShopUI serialized video components before page activation.
    public static class OutgameShopVideoBindings
    {
        public static OutgameShopEventHost Bind(GameObject page,IOutgameVideoButtonHost videoHost,Action<object> stopInitialization,Action selectCard,Func<OutgameMessageDispatcher> messages=null,OutgameVideoPlayCooldown cooldown=null)
        {
            if(page.activeInHierarchy)throw new InvalidOperationException("Bind shop video buttons before page activation.");
            foreach(var button in page.GetComponentsInChildren<OutgameVideoButton>(true))button.Bind(videoHost,cooldown);
            var gold=page.transform.Find("btn_addGold").GetComponent<OutgameVideoButton>();
            var diamonds=page.transform.Find("btn_addDiamond").GetComponent<OutgameVideoButton>();
            return new OutgameShopEventHost(callback=>gold.AddVideoPlayCallBack(callback.Invoke),callback=>diamonds.AddVideoPlayCallBack(callback.Invoke),stopInitialization,selectCard,messages);
        }
    }
}
