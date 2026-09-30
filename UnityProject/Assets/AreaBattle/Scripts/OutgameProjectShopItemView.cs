using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameProjectShopItemView:MonoBehaviour,IOutgameProjectShopItemView
    {
        Func<string,string,Sprite> icon;
        Action<int,int> voice;
        Action<int,int,int,int,string> report;
        public OutgameProjectShopItem Controller {get;private set;}
        Transform Node(string path)=>transform.Find(path)??throw new InvalidOperationException("Missing original ShopItem node: "+path);
        public void Bind(OutgameProjectProduct data,OutgameToolDispatcher tools,OutgameLocalInventory inventory,
            Func<string,string,Sprite> icon,Action<int,int> voice,Action<int,int,int,int,string> report,Func<string> coinCostReason)
        {
            this.icon=icon??throw new ArgumentNullException(nameof(icon));
            this.voice=voice??throw new ArgumentNullException(nameof(voice));
            this.report=report??throw new ArgumentNullException(nameof(report));
            Controller=new OutgameProjectShopItem(tools,inventory,this,coinCostReason);
            var node=Node("btn_choose");var button=node.GetComponent<Button>()??node.gameObject.AddComponent<Button>();
            if(button.targetGraphic==null)button.targetGraphic=node.GetComponent<Graphic>();
            button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>Controller.Click());
            Controller.SetData(data);
        }
        public void SetActive(bool active)=>gameObject.SetActive(active);
        public void SetIcon(string name,string atlas)=>Node("imgIcon").GetComponent<Image>().sprite=icon(name,atlas);
        public void SetPriceText(string text)=>Node("img_gold/txt_gold").GetComponent<Text>().text=text;
        public void SetQuantityText(string text)=>Node("txt_info").GetComponent<Text>().text=text;
        public void PlayVoice(int group,int id)=>voice(group,id);
        public void ReportToolGet(int item,int category,int amount,int balance,string reason)=>report(item,category,amount,balance,reason);
        public void Refresh(){} // Original ShopItem.Refresh33332 is empty.
    }
}
