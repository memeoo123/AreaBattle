using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // AppSetting fields0/8 shared by CanvasAdaptive and UIModule root completion.
    public sealed class OutgameUiDisplaySettings
    {
        public static readonly OutgameUiDisplaySettings Shared=new OutgameUiDisplaySettings();
        public bool IsPortrait,HeightControlsWidthFixedWidth,IsBangs;
        public float BangsPixel;
    }
    // CanvasAdaptive74278-74283, including the diagnostic coroutine10721.MoveNext.
    public sealed class OutgameCanvasAdaptation
    {
        readonly GameObject owner;readonly OutgameUiDisplaySettings settings;
        readonly Func<int> width,height;readonly Action<IEnumerator> start;
        readonly Action<string> log;readonly Func<OutgameMessageDispatcher> messages;
        CanvasScaler canvas;AspectRatioFitter aspect;
        public float WhRatioConst,WidthControlsHeightFactor,HeightControlsWidthFactor=1f;
        public bool IsHeightControlsWidthFixedWidth=true;
        public float AspectWhRatio {get;private set;}
        public OutgameCanvasAdaptation(GameObject owner,OutgameUiDisplaySettings settings,Func<int> width,Func<int> height,Action<IEnumerator> start,Action<string> log,Func<OutgameMessageDispatcher> messages)
        {this.owner=owner;this.settings=settings;this.width=width;this.height=height;this.start=start;this.log=log;this.messages=messages;}
        public void Initialize()
        {
            canvas=owner.GetComponent<CanvasScaler>();aspect=owner.GetComponent<AspectRatioFitter>();
            if(aspect==null)aspect=owner.AddComponent<AspectRatioFitter>();
            WhRatioConst=canvas.referenceResolution.x/canvas.referenceResolution.y;
            settings.IsPortrait=canvas.referenceResolution.y>canvas.referenceResolution.x;
            settings.HeightControlsWidthFixedWidth=IsHeightControlsWidthFixedWidth;
            log("当前分辨率比值："+((float)width()/height()).ToString());
            Adapt();
        }
        public void Adapt()
        {
            float ratio=(float)width()/height();
            if(settings.IsPortrait&&!(WhRatioConst>=ratio))HeightControlsWidth(ratio);
            else WidthControlsHeight(ratio);
            AspectWhRatio=ratio;
            start(PrintDesignResolutionInfo());
        }
        public void WidthControlsHeight(float ratio)
        {canvas.matchWidthOrHeight=WidthControlsHeightFactor;aspect.aspectMode=AspectRatioFitter.AspectMode.WidthControlsHeight;aspect.aspectRatio=ratio;}
        public void HeightControlsWidth(float ratio)
        {canvas.matchWidthOrHeight=HeightControlsWidthFactor;aspect.aspectMode=AspectRatioFitter.AspectMode.HeightControlsWidth;aspect.aspectRatio=ratio;}
        public void Update()
        {
            float currentRatio=(float)((float)width()/height());
            if(AspectWhRatio!=currentRatio)
            {
                Adapt();
                log("屏幕分辨率变动，触发适配"+width().ToString()+"_"+height().ToString());
                messages().SendMessage("GameAspectChange");
            }
        }
        public IEnumerator PrintDesignResolutionInfo()
        {
            yield return new WaitForSeconds(1f);
            log(string.Format("DesignResolutionInfo:{{designWidth={0}, designHeight={1} , scaleX=0，scaleY=0，scaleWidth={2}, scaleHeight={3}}}",canvas.referenceResolution.x,canvas.referenceResolution.y,width(),height()));
        }
    }
}
