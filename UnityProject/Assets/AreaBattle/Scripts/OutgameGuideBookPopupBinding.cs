using System;
using UnityEngine;
using UnityEngine.UI;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public sealed class OutgameGuideBookPopupServices
    {
        public Func<OutgameLegacyConfigManager> Config;
        public Func<OutgameGuideBookControl> Control;
        // Source ControlBase<SkillControl>.I.CurCommanderId (type4165, field108).
        public Func<int> CurrentCommanderId;
        public Func<string,string> Language;
        public Action<Image,string,string,bool> SetSprite;
        public Action<int> Voice;
        public Action ClosePage;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
    }
    // Source4328 popup33036/33039/33041/33043/33044/33050.
    public sealed class OutgameGuideBookPopupBinding:MonoBehaviour
    {
        OutgameGuideBookRewardBinding rewards;OutgameGuideBookPopupServices services;
        public GameObject Popup {get;private set;}
        public GameObject Demonstration {get;private set;}
        public GameObject Pitch {get;private set;}
        public Text Title {get;private set;}
        public Text Content {get;private set;}
        public Text AdditionalTip {get;private set;}
        public Image Icon {get;private set;}
        public void Bind(OutgameGuideBookRewardBinding rewards,OutgameGuideBookPopupServices services)
        {
            this.rewards=rewards;this.services=services;
            Popup=transform.Find("guidePop").gameObject;
            Demonstration=transform.Find("guidePop/mainbg/guideIcon/YD_0").gameObject;
            Pitch=transform.Find("guidePop/mainbg/guideContent/Pitch").gameObject;
            Title=transform.Find("guidePop/mainbg/TitleBG/guideTittle").GetComponent<Text>();
            Content=transform.Find("guidePop/mainbg/guideContent").GetComponent<Text>();
            AdditionalTip=transform.Find("guidePop/mainbg/textTipAdd").GetComponent<Text>();
            Icon=transform.Find("guidePop/mainbg/guideIcon").GetComponent<Image>();
            OutgameUiClick.Add(transform.Find("btnClose").GetComponent<Button>(),ClosePage,services.Messages);
            OutgameUiClick.Add(transform.Find("guidePop/imgPopMask").GetComponent<Image>(),ClosePopup);
            OutgameUiClick.Add(transform.Find("guidePop/mainbg/OKBtn").GetComponent<Button>(),ClosePopup,services.Messages);
        }
        void ClosePage(){services.Voice(2001);services.ClosePage();}
        public void ClosePopup(){services.Voice(2001);Popup.SetActive(false);rewards.SelectedBook=null;}
        public void OnItemClick(int index,GuidebookConfig row)
        {services.Voice(2001);rewards.SelectedBook=row;rewards.SelectedIndex=index;ShowPopup();Refresh(row);}
        public void ShowPopup()=>Popup.SetActive(true);
        public void Refresh(GuidebookConfig book)
        {
            if(!services.Config().dicGuide.ContainsKey(book.guideId))return;
            var row=services.Config().dicGuide[book.guideId];
            string picture=row.picture,explain=row.explain,title=row.title;
            if(row.id>=9&&row.id<=11)
            {
                int number=unchecked(row.id-8+(services.CurrentCommanderId()-1)*3);
                if(number!=-1)
                {
                    title=string.Format(row.title,number);explain=string.Format(row.explain,number);
                    picture=string.Format(row.picture,services.CurrentCommanderId());
                }
            }
            Title.text=services.Language(title);Content.text=services.Language(explain);
            services.SetSprite(Icon,picture,"GuideSprite",false);
            if(row.id==1){Demonstration.SetActive(true);Icon.sprite=null;}else Demonstration.SetActive(false);
            if(row.id==7||row.id==8)
            {
                Pitch.SetActive(true);string[] values=row.param.Split(';');
                for(int i=0;i<values.Length;i++)Pitch.transform.GetChild(i).GetChild(2).GetComponent<Text>().text=values[i];
                Content.text="";
            }
            else Pitch.SetActive(false);
            RefreshAdditionalTip(AdditionalTip,row.id,services.Language);
            var rewardObject=rewards.BookRewardButton.gameObject;rewardObject.SetActive(!services.Control().IsGetBookReward(book.id));
        }
        public static void RefreshAdditionalTip(Text text,int guideId,Func<string,string> language)
        {
            // Source br_table offsets:7/8/12 return without changing alignment;6/9/10/11 set MiddleCenter.
            switch(guideId)
            {
                case 7:text.text=language("GuideUI.tipAdd.defanse");return;
                case 8:text.text=language("GuideUI.tipAdd.attack");return;
                case 12:text.text=language("GuideUI.tipAdd.arrow");return;
                case 6:text.text=language("GuideUI.tipAdd.max");break;
                case 9:text.text=language("GuideUI.tipAdd.ice");break;
                case 10:text.text=language("GuideUI.tipAdd.fire");break;
                case 11:text.text=language("GuideUI.tipAdd.lighting");break;
                default:text.text="";return;
            }
            text.alignment=TextAnchor.MiddleCenter;
        }
    }
}
