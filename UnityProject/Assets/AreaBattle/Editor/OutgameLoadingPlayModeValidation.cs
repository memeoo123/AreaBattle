using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLoadingPlayModeValidation
    {
        const string Pending="AreaBattle.LoadingNative";
        [Serializable] sealed class Report {public bool passed;public string error;public string scope="Original loading Resources prefab, native UGUI/SpriteRenderer/Animator and ScriptableObject-independent source progress/timeout callbacks. Scaled scalar tweens stepped deterministically. Full Main/UIControl composition and original matched visual replay are not covered.";public List<string> checks=new List<string>();}
        sealed class Host:IOutgameLoadingPageHost
        {
            public Action Retry,Cancel,Hidden;public int Tips,State=-1;public bool Option,SingletonClearedAtState;public string Text,Title;
            public void ShowCommonTip(string text,Action retry,Action cancel,string title){Tips++;Text=text;Retry=retry;Cancel=cancel;Title=title;}
            public void SetPlayState(int state,bool option){State=state;Option=option;SingletonClearedAtState=OutgameLoadingPage.Instance==null;}
            public void HideTransition(Action complete){Hidden=complete;}
        }
        static Report report;static bool stopped;static int phase;static double began;static GameObject parent;static OutgameScalarTweenRunner tweens;static OutgameLoadingPage closing,retry,cancel,newer;static Host retryHost,cancelHost;static OutgameUiControl ui;static OutgameLoadingPage replaced,owned;
        static OutgameLoadingPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){if(!Application.isBatchMode)throw new InvalidOperationException("Isolated batch only");SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange change){if(change==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool yes,string label){if(!yes)throw new Exception(label);report.checks.Add(label);}
        static bool Near(float a,float b)=>Mathf.Abs(a-b)<.0002f;
        static OutgameLoadingPage Page(Host host)
        {
            var go=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Loading/Proj_xqzdLoadingUI"),parent.transform,false);var page=go.GetComponent<OutgameLoadingPage>();page.enabled=false;page.Bind(tweens,host,(min,max)=>{Check(Near(min,.6f)&&Near(max,.8f),"Source initial fill requests random range0.6..0.8");return .7f;});return page;
        }
        static void Start()
        {
            report=new Report();stopped=false;phase=0;began=EditorApplication.timeSinceStartup;
            try{
                parent=new GameObject("native-loading-canvas",typeof(RectTransform),typeof(Canvas));var canvas=parent.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingLayerID=-939575283;canvas.sortingOrder=42;
                tweens=new GameObject("loading-scalar-tweens").AddComponent<OutgameScalarTweenRunner>();tweens.enabled=false;
                closing=Page(new Host());var renderer=closing.AnimationAnchor.GetComponent<SpriteRenderer>();var animator=closing.AnimationAnchor.GetComponent<Animator>();
                Check(closing.LoadingText.font&&closing.Mask.sprite&&renderer.sprite&&animator.runtimeAnimatorController&&closing.transform.childCount==4,"Original loading hierarchy and native font/image/renderer/controller references are populated");
                var auto=closing.LoadingText.GetComponent<OutgameAutoCanvasLayer>();auto.enabled=false;auto.Apply();var nested=closing.LoadingText.GetComponent<Canvas>();Check(nested.overrideSorting&&nested.sortingLayerID==canvas.sortingLayerID&&nested.sortingOrder==67,"AutoCanvasLayer finds ancestor canvas and adds original OrderOffer25");
                var clip=animator.runtimeAnimatorController.animationClips[0];var keys=AnimationUtility.GetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(SpriteRenderer),"m_Sprite"));Check(clip.isLooping&&Near(clip.frameRate,12)&&Near(clip.length,.5f)&&keys.Length==6,"Original six sprite frames at12fps loop through0.5 seconds (loop="+clip.isLooping+", fps="+clip.frameRate+", duration="+clip.length+", keys="+keys.Length+")");
                animator.Update(0);var first=renderer.sprite;animator.Update(.09f);Check(first==keys[0].value&&renderer.sprite==keys[1].value,"Native Animator advances original first and second sprite frames");animator.enabled=false;
                closing.StartPage();var anchor=closing.OriginalAnchor;closing.Close();Check(closing.CloseRequested&&!closing.InitialFillFinished&&tweens.Count==1,"Early Close defers final fill until initial tween completes");
                tweens.Advance(.5f);Check(Near(closing.Mask.fillAmount,.525f)&&Near(closing.AnimationAnchor.anchoredPosition.x,anchor.x+.525f*825f),"Default OutQuad drives native Image and825-unit animation anchor");
                tweens.Advance(.5f);Check(closing.InitialFillFinished&&closing.LoadingText.text=="70%"&&Near(closing.Mask.fillAmount,.7f)&&tweens.Count==1&&OutgameLoadingPage.Instance==closing,"Initial completion queues final tween without consuming current step twice");
                tweens.Advance(.3f);Check(Near(closing.Mask.fillAmount,1)&&closing&&OutgameLoadingPage.Instance==null&&tweens.Count==0,"Final completion clears singleton immediately and schedules deferred native destruction");
                retryHost=new Host();retry=Page(retryHost);retry.Elapsed=4.5f;retry.SetSpine();Check(retry.TimeoutEnabled&&Near(retry.TimeoutSeconds,5)&&Near(retry.Elapsed,4.5f),"SetSpine enables five-second timeout without resetting accumulated time");
                retry.AdvanceTimeout(.5f);Check(retryHost.Tips==0&&Near(retry.Elapsed,5),"Exact timeout boundary does not open retry tip");retry.AdvanceTimeout(.01f);
                Check(retryHost.Tips==1&&retryHost.Text=="资源下载失败，是否重新下载？"&&retryHost.Title==""&&!retry.TimeoutEnabled&&retry.Elapsed==0,"Timeout strictly greater than threshold resets timer and opens source retry/cancel tip once");
                retry.AdvanceTimeout(100);Check(retryHost.Tips==1,"Disabled timeout cannot emit repeated tips");retryHost.Retry();Check(retryHost.State==10&&!retryHost.Option&&retryHost.SingletonClearedAtState&&retry,"Retry destroys loading page before state10 callback; native destruction remains deferred");
                cancelHost=new Host();cancel=Page(cancelHost);cancel.Elapsed=3;cancel.ResetTime();Check(cancel.Elapsed==0,"Successful preparation resets actual page timer");cancel.Cancel();Check(cancelHost.Hidden!=null&&cancelHost.State==-1&&OutgameLoadingPage.Instance==cancel,"Cancel waits for transition completion before destroying page or changing state");
                newer=Page(new Host());cancelHost.Hidden();Check(cancelHost.State==11&&!cancelHost.Option&&cancelHost.SingletonClearedAtState&&newer&&OutgameLoadingPage.Instance==null,"Cancel completion clears singleton unconditionally even when a newer page exists, then emits state11");
                var resources=new OutgameLoadingPageResources(tweens,new Host());var registry=new OutgameControllerRegistry();
                var nodes=new OutgameUiNodes(new Dictionary<string,Transform>(),()=>parent.GetComponent<RectTransform>(),()=>parent,Debug.LogError);
                var story=new GameObject("UIStory",typeof(RectTransform));story.transform.SetParent(parent.transform,false);
                OutgameCoreControllerBindings.BindUi(registry,null,null,new OutgameUiControlGlobals(),null,null,null,null,null,resources.Instantiate,nodes.Get);ui=(OutgameUiControl)registry.Resolve(4296);
                replaced=ui.OpenLoadingUI();owned=ui.OpenLoadingUI();replaced.enabled=false;owned.enabled=false;
                Check(replaced&&owned&&replaced!=owned&&ui.LoadingPage==owned&&owned.transform.parent==story.transform&&owned.transform.localScale==Vector3.one,"UIControl creates original-path loading resource for each open and parents with worldPositionStays=false under UIStory");
                owned.Elapsed=4;ui.ResetLoadingTime();Check(owned.Elapsed==0,"UIControl resets held page field44");ui.CloseLoadingUI();Check(ui.LoadingPage==null&&owned.CloseRequested&&owned&&replaced,"UIControl Close clears held reference immediately while native page completion is still pending");
                replaced.DestroyPage();owned.DestroyPage();
                EditorApplication.update+=Poll;
            }catch(Exception ex){Finish(ex);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>20)throw new TimeoutException("Native loading page destruction timed out");
                if(phase==0){if(closing||retry||cancel||replaced||owned)return;ui.LoadingPage=owned;owned.Elapsed=8;ui.ResetLoadingTime();Check(owned.Elapsed==0,"Destroyed native owner retains source CLR field reset semantics");ui.LoadingPage=null;Check(newer,"Native frame destroys only scheduled pages and preserves newer loading object");UnityEngine.Object.Destroy(newer.gameObject);phase=1;return;}
                if(newer)return;Check(OutgameLoadingPage.Instance==null,"No fabricated singleton reassignment on delayed destroy");Finish(null);
            }catch(Exception ex){Finish(ex);}
        }
        static void Finish(Exception ex){if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);report.passed=ex==null;report.error=ex?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/loading-page-native-validation.json"),JsonUtility.ToJson(report,true));if(ex!=null)Debug.LogException(ex);EditorApplication.Exit(ex==null?0:1);}
    }
}
