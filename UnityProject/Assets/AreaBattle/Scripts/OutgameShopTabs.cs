using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // ShopUI.X|}y{vW and wsNkyjR; content visibility belongs to the original toggle-group controller.
    public sealed class OutgameShopTabs:MonoBehaviour
    {
        readonly string[] names={"normal","defense","attack","MapSkin","Shop"};
        readonly string[] keys={"ShopUI.Infantry","ShopUI.Mauler","ShopUI.Cavalry","ShopUI.Scene","ShopUI.Shop"};
        readonly string[] dots={"objNormalRedDot","objDefenseRedDot","objAttackRedDot","objSceneRedDot","objShopRedDot"};
        OutgameSkinCatalog skins;Func<string,string> language;Action<int> voice,rotate;
        public int SelectedSoldierType {get;private set;}
        public int SelectedTab {get;private set;}
        public void Bind(OutgameSkinCatalog skins,int heldSoldierType,Func<string,string> language,Action<int> voice,Action<int> rotate)
        {
            this.skins=skins??throw new ArgumentNullException(nameof(skins));this.language=language??throw new ArgumentNullException(nameof(language));
            this.voice=voice??throw new ArgumentNullException(nameof(voice));this.rotate=rotate??throw new ArgumentNullException(nameof(rotate));SelectedSoldierType=heldSoldierType;
            for(int i=0;i<names.Length;i++){int tab=i+1;transform.Find("bottom/ToggleArr/tog_"+names[i]).GetComponent<Toggle>().onValueChanged.AddListener(on=>Changed(tab,on));}
        }
        public void InitializeSourceSelection()
        {
            foreach(string group in new[]{"normal","defense","attack","scene","Shop"})transform.Find("bottom/skinGroup_"+group).gameObject.SetActive(false);
            int tab=SelectedSoldierType==2?2:SelectedSoldierType==3?3:1;
            if(tab==1)SelectedSoldierType=1;
            transform.Find("bottom/ToggleArr/tog_"+names[tab-1]).GetComponent<Toggle>().isOn=true;
            if(tab==1)
            {
                // Source explicitly enables normal content and repeats its change callback.
                transform.Find("bottom/skinGroup_normal").gameObject.SetActive(true);Changed(1,true);
            }
            RefreshRedDots(tab);
        }
        public void Changed(int tab,bool on)
        {
            if(!on)return;if(tab<1||tab>5)throw new ArgumentOutOfRangeException(nameof(tab));
            voice(2001);SelectedTab=tab;if(tab<=3)SelectedSoldierType=tab;
            RefreshRedDots(tab);rotate(SelectedSoldierType);
            transform.Find("bottom/bg_top/txt_shopinfo").GetComponent<Text>().text=language(keys[tab-1]);
        }
        public void RefreshRedDots(int hiddenTab)
        {
            for(int i=0;i<names.Length;i++)
            {
                int type=i+1;bool visible=type<=3?skins.OrderedSoldiers.Any(s=>s.skinType==type&&s.isNew):type==4&&skins.OrderedScenes.Any(s=>s.isNew);
                transform.Find("bottom/ToggleArr/tog_"+names[i]+"/"+dots[i]).gameObject.SetActive(visible&&type!=hiddenTab);
            }
        }
    }
}
