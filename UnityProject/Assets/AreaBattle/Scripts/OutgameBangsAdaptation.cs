using System;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameBangsState
    {
        public static readonly OutgameBangsState Shared=new OutgameBangsState();
        public int WebGlHeight=-1,ExtraOffset;
        public float CurrentScreenRatio;
        public void SetBangsPixel(int pixel)=>WebGlHeight=pixel;
    }
    // AdaptiveBangs Start28034 ->28028 ->28022 ->28012/28029.
    public sealed class OutgameBangsAdaptation
    {
        readonly RectTransform rect;readonly OutgameUiDisplaySettings settings;readonly OutgameBangsState state;
        readonly Func<RectTransform> moduleCanvas;readonly Action<string> log;
        Vector2 offsetMax,offsetMin;int screenWidth,screenHeight;
        public bool IsNeedAdaptiveBangs,IsDoubleEnded;
        public OutgameBangsAdaptation(RectTransform rect,OutgameUiDisplaySettings settings,OutgameBangsState state,Func<RectTransform> moduleCanvas,Action<string> log)
        {this.rect=rect;this.settings=settings;this.state=state;this.moduleCanvas=moduleCanvas;this.log=log;}
        public void Start(int width,int height)
        {screenWidth=width;screenHeight=height;offsetMax=rect.offsetMax;offsetMin=rect.offsetMin;Apply();}
        public void Apply()
        {
            rect.offsetMax=offsetMax;rect.offsetMin=offsetMin;
            int pixel=ResolvePixel();log("刘海屏适配高度:"+pixel);
            settings.BangsPixel=pixel;settings.IsBangs=settings.BangsPixel!=0f;
            if(!IsNeedAdaptiveBangs)return;
            if(settings.IsPortrait)
            {
                var max=rect.offsetMax;rect.offsetMax=new Vector2(max.x,max.y-pixel);
                if(IsDoubleEnded){var min=rect.offsetMin;rect.offsetMin=new Vector2(min.x,min.y+pixel);}
            }
            else
            {
                var min=rect.offsetMin;rect.offsetMin=new Vector2(min.x+pixel,min.y);
                if(IsDoubleEnded){var max=rect.offsetMax;rect.offsetMax=new Vector2(max.x-pixel,max.y);}
            }
        }
        int ResolvePixel()
        {
            int pixel=0;
            if(state.WebGlHeight>=0)
            {
                var size=moduleCanvas().sizeDelta;
                float value=settings.IsPortrait?(float)(state.WebGlHeight*1f*size.y/screenHeight):(float)(state.WebGlHeight*1f*size.x/screenWidth);
                double ceiling=Mathf.Ceil(value);
                pixel=Math.Abs(ceiling)<2147483648d?(int)ceiling:int.MinValue;
            }
            else
            {
                float ratio=settings.IsPortrait?(float)((float)screenHeight/screenWidth):(float)((float)screenWidth/screenHeight);
                log("宽高比："+ratio);state.CurrentScreenRatio=ratio;
                if(ratio>2f)pixel=85;
            }
            log(string.Format("webGlHeight --> {0} _> {1}",state.WebGlHeight,pixel));return pixel;
        }
    }
}
