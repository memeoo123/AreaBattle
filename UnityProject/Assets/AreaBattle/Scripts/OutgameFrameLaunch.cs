using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AreaBattle
{
    public interface IOutgameFrameAds
    {string SourceName12{set;}void HideBanner();}
    public sealed class OutgameFrameLaunchServices
    {
        public Action InitializeSdk,SetCulture;
        public Func<IOutgameFrameAds> Ads;
        public Func<string> MineGameName,SettingString48;
        public Action<Action> HideTransition;
        public Func<OutgameMessageDispatcher> Messages;
        public Action<string> SetGlobalGameName;
        public Func<float> ReadDpi=()=>Screen.dpi;
        public Action<int> SetTargetFrameRate=value=>Application.targetFrameRate=value;
        public Action<int> SetSleepTimeout=value=>Screen.sleepTimeout=value;
        public Action<string> LoadScene=name=>SceneManager.LoadScene(name);
    }
    public sealed partial class OutgameFrameEntry
    {
        public static float Dpi;
        public OutgameFrameLaunchServices Launch;
        // Source26439; original Converter.DPI fallback is96, with <=0 leaving NaN untouched.
        public void CommonSettings()
        {
            Launch.InitializeSdk();Dpi=Launch.ReadDpi();if(Dpi<=0)Dpi=96;
            Launch.SetTargetFrameRate(60);Launch.SetSleepTimeout(-1);Launch.SetCulture();
            var ads=Launch.Ads();string name=Launch.MineGameName();string suffix=Launch.SettingString48();
            ads.SourceName12=string.Concat(name,",",suffix);
            OutgameFrameWorkMono.Instance.ConfigureNetworkSampling();
        }
        // Source26438/26453: state/message/banner/scene changes happen only in transition callback.
        public void StartGame(string gameName,string menuScene)
        {
            Launch.HideTransition(()=>
            {
                if(!string.IsNullOrEmpty(menuScene))GameMenuSceneName=menuScene;
                Launch.Messages().SendMessage("ReadyExitGame");
                var ads=Launch.Ads();if(ads!=null)ads.HideBanner();
                IsGoGameMenu=false;Launch.SetGlobalGameName(gameName);Launch.LoadScene("GameFrameworkLoad");
            });
        }
        public void PauseGame(bool pause,bool changeTimeScale)
        {if(changeTimeScale)Time.timeScale=pause?0:1;OutgameFrameWorkMono.Instance.ActivePause(pause);}
        public bool IsPauseGame()=>OutgameFrameWorkMono.Instance.PauseState==1;
    }
}
