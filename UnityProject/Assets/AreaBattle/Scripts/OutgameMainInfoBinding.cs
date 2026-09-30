using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Main RefreshUIInfo33720 and its level-label/Easter visibility helpers. Original outlet paths.
    public sealed class OutgameMainInfoBinding:IDisposable
    {
        readonly OutgameMenuView menu;readonly Transform root;
        readonly Func<int> level,piggy,easterStatus,designMode;readonly Func<string,string> language;
        readonly Action refreshEasterTime;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameMainInfoBinding(OutgameMenuView menu,Transform root,Func<int> level,Func<string,string> language,
            Func<int> piggy,Func<int> easterStatus,Action refreshEasterTime,Func<int> designMode,Func<OutgameMessageDispatcher> messages=null)
        {
            this.menu=menu;this.root=root;this.level=level;this.language=language;this.piggy=piggy;this.easterStatus=easterStatus;
            this.refreshEasterTime=refreshEasterTime;this.designMode=designMode;this.messages=messages??(()=>OutgameMessageDispatcher.Shared);
            menu.PageRefreshRequested+=OnRefresh;
        }
        public OutgameMainInfoBinding(OutgameMenuView menu,Transform root,OutgameLevelProgression levels,Func<string,string> language,
            OutgamePiggyBank bank,Func<int> easterStatus,Action refreshEasterTime,Func<int> designMode,Func<OutgameMessageDispatcher> messages=null)
            :this(menu,root,()=>levels.CurrentLevel,language,bank.GetBankState,easterStatus,refreshEasterTime,designMode,messages){}
        void OnRefresh(OutgameMenuPage page){if(page==OutgameMenuPage.Main)Refresh();}
        Transform Node(string path)=>root.Find(path)??throw new InvalidOperationException("Missing original main outlet: "+path);
        void Active(string path,bool value)=>Node(path).gameObject.SetActive(value);
        void Text(string path,string value)=>Node(path).GetComponent<Text>().text=value;
        public void Refresh()
        {
            bool ordinary=level()!=0;
            Active("objNewStartpanel/btnNewStart/TextNormal",ordinary);Active("objNewStartpanel/btnNewStart/TextGuide",!ordinary);
            if(level()!=0)
            {
                Text("objNewStartpanel/btnNewStart/txt_level",language("startUI/level")+" "+level());
                Text("objStartpanel/btnStart/txt_startLevel",language("startUI/level")+" "+level());
            }
            else{Text("objNewStartpanel/btnNewStart/txt_level","");Text("objStartpanel/btnStart/txt_startLevel","");}
            RefreshEasterVisibility();
            int bank=piggy();Node("RigthBar/pigBank/btnPiggy_normal").parent.gameObject.SetActive(bank!=0);
            if(bank!=0){Active("RigthBar/pigBank/btnPiggy_normal",bank==1);Active("RigthBar/pigBank/btnPiggy_full",bank==2);}
            messages().SendMessage("LoadStartingUI");
            Active("objNewStartpanel",false);Active("objStartpanel",true);
            // Source repeats these same assignments in mode1; it does not select the new panel.
            if(designMode()==1){Active("objNewStartpanel",false);Active("objStartpanel",true);}
        }
        void RefreshEasterVisibility()
        {
            if(!root)return;
            if(easterStatus()>=3&&easterStatus()<=4)
            {
                Active("RigthBar/btn_Easter",true);Node("RigthBar/btn_Easter/Image/textEasterTime").parent.gameObject.SetActive(true);refreshEasterTime();
            }
            else if(easterStatus()<=5)Active("RigthBar/btn_Easter",false);
        }
        public void Dispose(){menu.PageRefreshRequested-=OnRefresh;}
    }
}
