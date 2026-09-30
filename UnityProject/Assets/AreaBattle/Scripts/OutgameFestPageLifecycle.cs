using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // FestActUI Awake34026, close34034, MoreSkin34019, OpenLater34033.
    public sealed class OutgameFestPageLifecycle
    {
        readonly Transform page;readonly OutgameFestActivityState state;readonly OutgameFestActManager manager;
        readonly OutgameLevelProgression levels;readonly Func<string> rewards;
        readonly OutgameFestRewardItems items;readonly OutgameFestRewardVisibility visibility;readonly OutgameFestSpecialLevelButtons special;
        readonly Action<int,int> voice;readonly Action closeSelf;readonly Func<Action> findMenuSkinPage;
        readonly Action<string,string,string,string> reportEnter;
        public OutgameFestPageLifecycle(Transform page,OutgameFestActivityState state,OutgameFestActManager manager,OutgameLevelProgression levels,
            Func<string> rewards,OutgameFestRewardItems items,OutgameFestRewardVisibility visibility,OutgameFestSpecialLevelButtons special,
            Action<int,int> voice,Action closeSelf,Func<Action> findMenuSkinPage,Action<string,string,string,string> reportEnter)
        {
            this.page=page;this.state=state;this.manager=manager;this.levels=levels;this.rewards=rewards;this.items=items;this.visibility=visibility;this.special=special;
            this.voice=voice;this.closeSelf=closeSelf;this.findMenuSkinPage=findMenuSkinPage;this.reportEnter=reportEnter;
        }
        void Bind(string path,Action callback)=>page.Find(path).GetComponent<Button>().onClick.AddListener(()=>callback());
        public void Awake()
        {
            page.Find("go_Notice").gameObject.SetActive(state.Status<4);
            page.Find("go_Main").gameObject.SetActive(state.Status>3);
            Bind("go_Notice/btn_Read",closeSelf);Bind("go_Notice/btn_NoticeClose",closeSelf);
            Bind("go_Main/CloseBtn",CloseWithVoice);
            new OutgameFestDailyRewardBinding(page,manager,rewards,visibility.Refresh,items.Refresh);
            Bind("go_Main/Area2/MoreSkin",()=>{CloseWithVoice();findMenuSkinPage()?.Invoke();});
            var start=OutgameItemTimestamp.ToDateTime(state.StartTimeStamp);var end=OutgameItemTimestamp.ToDateTime(state.EndTimeStamp);
            var text=page.Find("go_Main/Title/Time").GetComponent<Text>();text.text=start.ToString("yyyy.MM.dd")+"-"+end.ToString("yyyy.MM.dd");
            page.Find("go_Notice/GameObject/txtNoticeTime").GetComponent<Text>().text=text.text;
            reportEnter(state.ActivityName,levels.CurrentLevel.ToString(),null,null);
            items.Refresh();visibility.Refresh();
        }
        void CloseWithVoice(){voice(1,2001);closeSelf();}
        public void OpenLater()=>special.OpenLater();
    }
}
