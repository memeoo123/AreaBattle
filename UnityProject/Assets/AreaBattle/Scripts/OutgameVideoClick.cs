using System;
namespace AreaBattle
{
    // UIVideoBtn74414/74415: shared play cooldown and per-button click cooldown.
    public sealed class OutgameVideoPlayCooldown
    {
        public static readonly OutgameVideoPlayCooldown Shared=new OutgameVideoPlayCooldown();
        public bool Playing;
        public float Until;
    }
    public interface IOutgameVideoClickHost
    {
        bool HasData {get;}
        int ButtonState {get;}
        float UnscaledTime {get;}
        void PlayAudio();
        void Log(string message);
        void VideoClickReport();
        void InvokeClick(bool videoReady);
        void BasePointerClick();
        void ReportVideoPlay();
        void ShowVideo();
    }
    public sealed class OutgameVideoClick
    {
        readonly IOutgameVideoClickHost host;
        readonly OutgameVideoPlayCooldown shared;
        public bool Clicking {get;private set;}
        public float ClickUntil {get;private set;}
        public OutgameVideoClick(IOutgameVideoClickHost host,OutgameVideoPlayCooldown shared=null)
        {this.host=host;this.shared=shared??OutgameVideoPlayCooldown.Shared;}
        public void Update()
        {
            if(Clicking&&host.UnscaledTime>ClickUntil)Clicking=false;
            if(shared.Playing&&host.UnscaledTime>shared.Until)shared.Playing=false;
        }
        public void OnPointerClick()
        {
            if(!host.HasData)return;
            host.PlayAudio();
            if(shared.Playing){host.Log(string.Format("目前正处于视频播放CD中:{0}",shared.Until-host.UnscaledTime));return;}
            if(Clicking){host.Log(string.Format("目前正处于视频点击CD中{0}",ClickUntil-host.UnscaledTime));return;}
            Clicking=true;ClickUntil=host.UnscaledTime+1f;
            host.VideoClickReport();
            host.InvokeClick(host.ButtonState==2);
            host.BasePointerClick();
            if(host.ButtonState!=2)return;
            shared.Playing=true;shared.Until=host.UnscaledTime+5f;
            host.ReportVideoPlay();
            host.ShowVideo();
        }
    }
}
