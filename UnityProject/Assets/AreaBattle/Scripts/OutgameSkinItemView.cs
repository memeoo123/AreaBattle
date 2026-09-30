using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Source ShopUI clones SkinItem for both soldier and scene controllers.
    public sealed class OutgameSkinItemView:MonoBehaviour
    {
        OutgameSkinItemActions actions;OutgameSkinCatalog skins;OutgameSkinItemActions.Config config;
        Func<bool> sevenDayActive;Func<int> activityStatus;
        Func<string,string> language;Action<int,Action<bool>> requestVideo;Action activityClick;
        string videoRewardReason;int skinType;OutgameSceneSkinActions sceneActions;
        public int SkinType=>skinType;
        public int CurrentItemState {get;private set;}
        public event Action Selected;
        Transform Node(string path)=>transform.Find(path)??throw new InvalidOperationException("Missing original SkinItem node: "+path);
        void Active(string path,bool active)=>Node(path).gameObject.SetActive(active);
        void TextAt(string path,string text)=>Node(path).GetComponent<Text>().text=text;
        void Click(string path,Action action)
        {
            var node=Node(path);var button=node.GetComponent<Button>()??node.gameObject.AddComponent<Button>();
            if(button.targetGraphic==null)button.targetGraphic=node.GetComponent<Graphic>();
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>action());
        }
        public void Bind(int id,int sourceSkinType,OutgameSkinItemActions actions,OutgameSkinCatalog skins,
            Func<string,string> localize,Func<string,Sprite> icon,Func<bool> sevenDayActive,Func<int> activityStatus,
            Action<int,Action<bool>> requestVideo,string originalVideoRewardReason,Action activityClick,OutgameSceneSkinActions sceneActions=null)
        {
            this.actions=actions??throw new ArgumentNullException(nameof(actions));this.skins=skins??throw new ArgumentNullException(nameof(skins));
            language=localize??throw new ArgumentNullException(nameof(localize));this.sevenDayActive=sevenDayActive??throw new ArgumentNullException(nameof(sevenDayActive));
            this.activityStatus=activityStatus??throw new ArgumentNullException(nameof(activityStatus));this.requestVideo=requestVideo??throw new ArgumentNullException(nameof(requestVideo));
            this.activityClick=activityClick??throw new ArgumentNullException(nameof(activityClick));videoRewardReason=originalVideoRewardReason??throw new ArgumentNullException(nameof(originalVideoRewardReason));
            config=actions.Configuration(id);skinType=sourceSkinType;this.sceneActions=sceneActions;
            Node("img_poster").GetComponent<Image>().sprite=icon(config.iconName);
            Active("btn_goldUnlock/img_goldUnlock",config.castType==1001);Active("btn_goldUnlock/img_DiamondUnlock",config.castType==1002);
            TextAt("btn_goldUnlock/img_goldUnlock/txt_gold",actions.Price(id).ToString());TextAt("btn_goldUnlock/img_DiamondUnlock/txt_diamond",actions.Price(id).ToString());
            Active("imgTag",!string.IsNullOrEmpty(config.tag));TextAt("imgTag/txt_tag",language(config.tag));
            Click("btn_choose",()=>{if(this.sceneActions==null)actions.Select(id,CurrentItemState,()=>Selected?.Invoke());else this.sceneActions.Select(id);Refresh();});
            Click("btn_goldUnlock",()=>{if(this.sceneActions==null)actions.Buy(id);else this.sceneActions.Buy(id);Refresh();});
            Click("btn_adUnlock",()=>requestVideo(skinType+1001,success=>{actions.VideoCompleted(id,success,videoRewardReason);Refresh();}));
            Click("btn_actLimit",()=>this.activityClick());Click("btn_spceilLimit",()=>this.activityClick());
            Refresh();
        }
        public void Refresh()
        {
            var skin=skins.Skin(config.id);Active("imgRedDot",skin.isNew);
            if(skin.u)CurrentItemState=skins.UsedSkin(skinType)==config.id?1:0;
            else switch(config.special)
            {
                case 0:case 1:CurrentItemState=3;break;
                case 2:CurrentItemState=(config.actNo==1301?sevenDayActive():activityStatus()!=5)?4:3;break;
                case 3:CurrentItemState=5;break;
                // Source default retains prior item state.
            }
            Active("btn_actLimit",false);Active("img_using",false);Active("btn_adUnlock",false);Active("btn_goldUnlock",false);Active("btn_spceilLimit",false);
            switch(CurrentItemState)
            {
                case 0:break;
                case 1:Active("img_using",true);break;
                case 3:Active("btn_goldUnlock",true);break;
                case 4:Active("btn_actLimit",true);break;
                case 5:Active("btn_spceilLimit",true);TextAt("btn_spceilLimit/txt_getway",language("ShopUI.ActivityGetWay"));break;
                default:Active("btn_adUnlock",true);break;
            }
        }
    }
}
