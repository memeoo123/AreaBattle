using System;
using UnityEngine;
namespace AreaBattle
{
    // VisibleImp overrides33678/33793/32828. Data refresh remains owned by each bound page controller.
    public sealed class OutgameMenuPageVisibility:OutgameUiPage
    {
        readonly OutgameMenuPage page;
        readonly Action refresh,refreshSelected;
        readonly GameObject permit,ads,detail;
        public OutgameMenuPageVisibility(GameObject target,OutgameMenuPage page,Action refresh,Action refreshSelected=null,Func<OutgameMessageDispatcher> messages=null):base(target,messages)
        {
            this.page=page;this.refresh=refresh??throw new ArgumentNullException(nameof(refresh));this.refreshSelected=refreshSelected;
            if(page==OutgameMenuPage.Main){permit=Find(target,"btnPermit");ads=Find(target,"RigthBar/btn_ads");}
            else if(page==OutgameMenuPage.Commander)detail=Find(target,"objCommander/objCommanderInfo/objSkills/objSkillDetail");
            else if(page!=OutgameMenuPage.Skins)throw new ArgumentOutOfRangeException(nameof(page));
        }
        static GameObject Find(GameObject root,string name)
        {
            // Paths resolved from original OutletInfos object IDs in ui-import.json, not a name search.
            var found=root.transform.Find(name);
            if(!found)throw new InvalidOperationException("Missing original UI outlet: "+name);return found.gameObject;
        }
        protected override void VisibleImp(bool visible)
        {
            if(page==OutgameMenuPage.Main)
            {
                if(!GameObject)return;
                base.VisibleImp(visible);
                if(visible){permit.SetActive(false);ads.SetActive(false);refresh();}
                return;
            }
            if(page==OutgameMenuPage.Skins)
            {if(GameObject)GameObject.SetActive(visible);refresh();return;}
            if(GameObject)
            {
                GameObject.SetActive(visible);
                if(!visible){detail.SetActive(false);return;}
            }
            if(visible){refreshSelected?.Invoke();refresh();}
        }
    }
}
