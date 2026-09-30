using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameVideoButtonData
    {
        public int id,VideFlag,showType,AudioId,AmsId;
        public string Video_param1,Video_param2,ReportLable;
    }
    // Concrete SDK/report implementations must be supplied by the recovered account/platform owner.
    public interface IOutgameVideoButtonHost
    {
        OutgameVideoButtonData GetData(int id);
        bool IsOpen(int function,bool refresh);
        void BindFunctionButton(int function,OutgameVideoButton button);
        void CheckVideoIsReady();
        bool IsVideoReady();
        void PlayAudio(int group,int audio);
        void Log(string message);
        void TrackVideo(int state);
        void Report(string kind,string label,string param1,string param2);
        string GameName {get;}
        void ReportEvent(string name,string value,bool once);
        void ShowVideo(int flag,Action<string,bool> callback,int videoId);
    }
    // UIVideoBtn10741. Bind on the inactive reconstructed page before its first activation.
    public class OutgameVideoButton:Button,IOutgameVideoClickHost,IOutgameVideoButtonState
    {
        public int videoID;
        public bool AutoCallBtnShow=true;
        public int ButtonState {get;set;}
        public OutgameVideoButtonData Data {get;private set;}
        public bool CanShowAds {get;private set;}=true;
        public string ReportLabel {get;private set;}
        public string Param1 {get;private set;}
        public string Param2 {get;private set;}
        public int ReportState=>completion.ReportState;
        IOutgameVideoButtonHost host;
        OutgameVideoButtonCompletion completion;
        OutgameVideoClick click;
        bool lockedData;
        float nextEnable;
        PointerEventData pointer;
        public void Bind(IOutgameVideoButtonHost host,OutgameVideoPlayCooldown cooldown=null)
        {
            if(gameObject.activeInHierarchy)throw new InvalidOperationException("Bind video button on inactive page before activation.");
            if(this.host!=null)throw new InvalidOperationException("Video button is already bound.");
            this.host=host??throw new ArgumentNullException(nameof(host));
            completion=gameObject.AddComponent<OutgameVideoButtonCompletion>();
            completion.Bind(()=>Data.showType,n=>StartCoroutine(OutgameVideoButtonCompletion.DelayReport(n,CreateVideoReport,()=>host.IsOpen(15,false),VideoIsReadyReport)),ReportVideoPlayOver,()=>Report("fail"));
            click=new OutgameVideoClick(this,cooldown);
            GetVideoBtnData();
            if(Data!=null&&Data.showType==0)gameObject.SetActive(false);
        }
        public void SetVideoBtnData(string label,string param1,string param2)
        {ReportLabel=label;Param1=param1;Param2=param2;lockedData=true;}
        public void GetVideoBtnData()
        {
            CanShowAds=host.IsOpen(5,false);
            if(lockedData)return;
            Data=host.GetData(videoID);
            if(Data!=null){ReportLabel=Data.ReportLable;Param1=Data.Video_param1;Param2=Data.Video_param2;}
        }
        public void ResetVideoBtnData(int id)
        {
            if(videoID==id){host.Log(string.Format("设置的ID与已有的ID是一样的=[{0}]",videoID));return;}
            videoID=id;GetVideoBtnData();
            if(!CanShowAds||Data==null)gameObject.SetActive(false);
        }
        protected override void OnEnable()
        {
            base.OnEnable();if(host==null)return;
            if(Time.time<nextEnable){host.Log("激励视频多次激活，CD未好");return;}
            nextEnable=Time.time+.5f;
            if(!CanShowAds||Data==null){gameObject.SetActive(false);return;}
            if(AutoCallBtnShow)VideoBtnShow(true);
            if(Data.showType!=0)StartCoroutine(OutgameVideoButtonCompletion.DelayReport(1,CreateVideoReport,()=>host.IsOpen(15,false),VideoIsReadyReport));
        }
        protected override void Start()
        {
            if(host==null)return;
            if(!CanShowAds||Data==null){gameObject.SetActive(false);return;}
            if(Data.showType!=0){host.BindFunctionButton(5,this);host.CheckVideoIsReady();}
        }
        void Update(){click?.Update();}
        public override void OnPointerClick(PointerEventData eventData)
        {
            var prior=pointer;pointer=eventData;
            try{click?.OnPointerClick();}finally{pointer=prior;}
        }
        public bool CheckCanShowVideo()
        {
            GetVideoBtnData();
            if(!CanShowAds||Data==null)return false;
            if(Data.showType!=0)return true;
            CreateVideoReport();bool open=host.IsOpen(15,true);
            gameObject.SetActive(open);ButtonState=open?2:1;
            if(open){VideoIsReadyReport();return true;}return false;
        }
        public virtual void SetButtonState(int state)
        {
            if(Data==null)return;ButtonState=state;
            switch(state)
            {
                case 0:gameObject.SetActive(false);break;
                case 1:SetGay(true);break;
                case 2:SetGay(false);if(completion.ReportState==1)VideoIsReadyReport();break;
            }
        }
        public virtual void SetGay(bool gray){} // Original base method is empty.
        public virtual void PlayAudio(){if(Data.AudioId!=-1)host.PlayAudio(1,Data.AudioId);}
        public void CreateVideoReport(){host.TrackVideo(1);completion.ReportState=1;}
        public void VideoIsReadyReport(){host.TrackVideo(2);completion.ReportState=2;}
        public void VideoClickReport(){host.TrackVideo(3);Report("click");}
        void Report(string kind)=>host.Report(kind,ReportLabel,Param1,Param2);
        public void ReportVideoPlayOver()
        {
            Report("success");string label=Data.ReportLable;string name="",scoped="";
            switch(Data.VideFlag)
            {
                case 0:name="videoreward";scoped="videoreward_"+host.GameName;break;
                case 1:name="videoalive";scoped="videoalive_"+host.GameName;break;
                case 2:name="videounlock";scoped="videounlock_"+host.GameName;break;
            }
            host.ReportEvent(name,host.GameName,true);
            if(!string.IsNullOrEmpty(label))host.ReportEvent(scoped,label,true);else CustomizeReport();
        }
        public virtual void CustomizeReport(){} // Original base method is empty.
        public virtual void VideoBtnShow(bool show)
        {
            bool ready=host.IsVideoReady();
            if(show&&ready)Report("show");
            else host.Report(show?"no-ads":"unable",ReportLabel,null,null);
        }
        public void AddVideoBtnEvent(UnityAction<bool> callback)=>completion.AddVideoBtnEvent(callback);
        public void AddVideoPlayCallBack(UnityAction<bool> callback)=>completion.AddVideoPlayCallBack(callback);
        public void RemoveVideoBtnListeners()=>completion.RemoveVideoBtnListeners();
        public void VideoCallBack(string name,bool success)=>completion.VideoCallBack(name,success);
        bool IOutgameVideoClickHost.HasData=>Data!=null;
        float IOutgameVideoClickHost.UnscaledTime=>Time.unscaledTime;
        void IOutgameVideoClickHost.Log(string message)=>host.Log(message);
        void IOutgameVideoClickHost.InvokeClick(bool ready)=>completion.InvokeClick(ready);
        void IOutgameVideoClickHost.BasePointerClick()=>base.OnPointerClick(pointer);
        void IOutgameVideoClickHost.ReportVideoPlay()=>Report("play");
        void IOutgameVideoClickHost.ShowVideo()=>host.ShowVideo(Data.VideFlag,VideoCallBack,videoID);
    }
}
