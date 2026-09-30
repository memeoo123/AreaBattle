using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // FestActUI.OpenLater34033 and closure34037, using original child indices.
    public sealed class OutgameFestSpecialLevelButtons
    {
        readonly Transform gifts;readonly OutgameFestActManager manager;readonly OutgameFestSpecialLevelStart start;readonly Func<string,string> language;
        public OutgameFestSpecialLevelButtons(Transform page,OutgameFestActManager manager,OutgameFestSpecialLevelStart start,Func<string,string> language)
        {gifts=page.Find("go_Main/Area2/Main/SkinGifts");this.manager=manager;this.start=start;this.language=language;}
        public void OpenLater()
        {
            for(int i=0;i<manager.LimitSkinCount;i++)
            {
                if(((uint)manager.Data.limetSkinStatus>>i&1)!=0)continue;
                int index=i;var button=gifts.GetChild(i).GetChild(3);
                button.GetChild(0).GetComponent<Text>().text=language("ValentineUI.Play");
                button.GetComponent<Button>().onClick.AddListener(()=>start.Start(index));
            }
        }
    }
}
