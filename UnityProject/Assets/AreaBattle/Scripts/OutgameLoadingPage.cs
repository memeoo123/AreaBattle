using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public interface IOutgameLoadingPageHost
    {
        void ShowCommonTip(string text,Action retry,Action cancel,string title);
        void SetPlayState(int state,bool option);
        void HideTransition(Action complete);
    }
    // Reachable Proj_xqzdLoadingUI4387 entrypoints and callbacks; decoy numeric variants excluded.
    public sealed class OutgameLoadingPage:MonoBehaviour,IOutgameLevelStartLoading
    {
        public static OutgameLoadingPage Instance;
        public Text LoadingText;public Image Mask;public RectTransform AnimationAnchor;
        public Vector2 OriginalAnchor;
        public bool InitialFillFinished,CloseRequested,TimeoutEnabled;
        public float TimeoutSeconds=10f,Elapsed;
        OutgameScalarTweenRunner tweens;IOutgameLoadingPageHost host;Func<float,float,float> random;
        public bool SourceFlag48 {set=>TimeoutEnabled=value;}
        public float SourceValue40 {set=>TimeoutSeconds=value;}
        public void Bind(OutgameScalarTweenRunner tweens,IOutgameLoadingPageHost host,Func<float,float,float> random=null)
        {this.tweens=tweens;this.host=host;this.random=random??UnityEngine.Random.Range;}
        void Awake()=>Instance=this;
        void Start()=>StartPage();
        public void StartPage()
        {
            Mask.fillAmount=0;OriginalAnchor=AnimationAnchor.anchoredPosition;
            AnimateFill(random(.6f,.8f),1f,()=>{InitialFillFinished=true;if(CloseRequested)AnimateFill(1f,1f-Mask.fillAmount,DestroyPage);});
        }
        void AnimateFill(float end,float duration,Action complete)
        {
            float value=Mask.fillAmount;
            tweens.To(()=>value,next=>{value=next;ApplyFill(next);},end,duration,complete);
        }
        public void ApplyFill(float value)
        {Mask.fillAmount=value;LoadingText.text=string.Format("{0:N0}%",value*100f);AnimationAnchor.anchoredPosition=OriginalAnchor+Vector2.right*(Mathf.Clamp01(value)*825f);}
        public void Close()
        {CloseRequested=true;if(InitialFillFinished)AnimateFill(1f,1f-Mask.fillAmount,DestroyPage);}
        public void SetSpine(){TimeoutEnabled=true;TimeoutSeconds=5f;}
        public void ResetTime()=>Elapsed=0;
        public void AdvanceTimeout(float scaledDelta)
        {
            if(!TimeoutEnabled)return;Elapsed+=scaledDelta;
            if(Elapsed>TimeoutSeconds)
            {Elapsed=0;TimeoutEnabled=false;host.ShowCommonTip("资源下载失败，是否重新下载？",Retry,Cancel,string.Empty);}
        }
        void Update()=>AdvanceTimeout(Time.deltaTime);
        public void Retry(){DestroyPage();host.SetPlayState(10,false);}
        public void Cancel()=>host.HideTransition(()=>{DestroyPage();host.SetPlayState(11,false);});
        public void DestroyPage(){Instance=null;TimeoutEnabled=false;Canvas.ForceUpdateCanvases();Destroy(gameObject);}
    }
}
