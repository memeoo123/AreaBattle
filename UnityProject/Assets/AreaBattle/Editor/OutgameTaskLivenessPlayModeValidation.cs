using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Fixture=AreaBattle.EditorTools.OutgameTaskLivenessValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameTaskLivenessPlayModeValidation
    {
        const string Pending="AreaBattle.TaskLivenessNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original task tier/preview hierarchy and native legacy animation, real EventSystem selection/Button pointer callbacks, UpdateManager visibility and WaitForEndOfFrame at timeScale0, actual reward/mask/autosave/independent restart. Source Effect1016, fly/sprite/localization/popup/report endpoints observed; full task page/Main/Achievement and original audiovisual/Player acceptance pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture f;static Report report;static string path;static int phase,frame;static double began;static bool stopped;static float width;static Animation animation;
        static OutgameTaskLivenessPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run(){SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string message){if(!value)throw new Exception(message);report.checks.Add(message);}
        static void Phase(int next){phase=next;frame=Time.frameCount;}
        static void Click(GameObject root)=>ExecuteEvents.Execute(root,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        static void Canvas(){var c=f.Rows.Root.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;f.Rows.Root.AddComponent<GraphicRaycaster>();}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                new GameObject("EventSystem",typeof(EventSystem));new GameObject("Camera",typeof(Camera)).transform.position=new Vector3(0,0,-10);
                path=Path.Combine(Path.GetTempPath(),"AreaBattleLivenessNative-"+Guid.NewGuid().ToString("N"));f=new Fixture(true,path);Canvas();f.PreviewServices.SelectedObject=()=>EventSystem.current.currentSelectedGameObject;
                EventSystem.current.SetSelectedGameObject(f.Button(0).gameObject);Click(f.Button(0).gameObject);
                Check(f.Preview.Visible&&f.Preview.Data.id==1001&&f.Preview.Amount==50,"Original unready tier pointer opens first reward preview");Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>55)throw new TimeoutException("Task liveness native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    Check(f.Preview.Visible&&f.Preview.CountText.text=="x50"&&f.Preview.NameText.text=="奖励名称","Native selection named Node preserves rendered preview through UpdateManager frames");
                    EventSystem.current.SetSelectedGameObject(f.Preview.IconBackground);Click(f.Preview.IconBackground);Check(f.PopupId==1001&&f.PopupRect==f.Preview.IconBackground.GetComponent<RectTransform>(),"Native IconBg pointer sends exact reward detail anchor/id");
                    width=f.Preview.RootRect.sizeDelta.x;f.Preview.NameRect.sizeDelta=new Vector2(f.Preview.PreviousNameWidth+60,f.Preview.NameRect.sizeDelta.y);f.Preview.Refresh();Phase(1);return;
                }
                if(phase==1)
                {
                    Check(f.Preview.RootRect.sizeDelta.x==width+60,"Real WaitForEndOfFrame expands original preview by new name width");
                    EventSystem.current.SetSelectedGameObject(f.Rows.Root);Phase(2);return;
                }
                if(phase==2)
                {
                    Check(!f.Preview.Visible&&f.Preview.Lifetime.GameObject.activeSelf&&f.Preview.Lifetime.Transform.localScale==Vector3.zero,"Actual outside EventSystem selection hides preview via recovered update handle");
                    f.Ready();Check(f.Glow(0).activeSelf&&f.Glow(0).GetComponent<Animation>().clip.name=="hdzd_eff_taskNode","Ready threshold shows original glow with restored native clip");
                    Time.timeScale=0;EventSystem.current.SetSelectedGameObject(f.Button(0).gameObject);Click(f.Button(0).gameObject);animation=f.Layout.GetChild(0).GetComponentInChildren<Animation>();
                    Check(f.Shows==1&&!f.Glow(0).activeSelf&&f.Button(0).enabled&&f.Item(0).state==0,"Source effect request precedes frame boundary; Button not disabled until later");Phase(3);return;
                }
                if(phase==3)
                {
                    if(f.Binding.LastClaim==null||!f.Binding.LastClaim.IsCompleted)return;f.Binding.LastClaim.GetAwaiter().GetResult();
                    Check(f.Rows.Tasks.Items.Global.GetItemCount(1001)==50&&f.Item(0).state==1&&!f.Button(0).enabled,"End-of-frame continues at timeScale0 and awards real tier inventory/mask before red refresh");
                    Check(animation.isPlaying&&Mathf.Abs(animation[animation.clip.name].time)<.001f,"Original 1.7-second box animation remains paused while source one-frame reward already completed");
                    int shown=f.Shows;Click(f.Button(0).gameObject);Check(f.Shows==shown,"Native disabled tier Button rejects later pointer callbacks");Time.timeScale=1;Phase(4);return;
                }
                if(phase==4)
                {
                    if(animation.isPlaying||f.Rows.Tasks.Parent.Dirty)return;
                    Check(f.Rows.Tasks.Stored.Length>0,"Native resumed frames finish source box animation and save actual liveness award");
                    f.Binding.CloseCurrentEffect();Check(f.Closed.Count==1&&f.Closed[0]==1&&f.Binding.EffectHandle==0,"Recovered cleanup closes stored effect endpoint once");
                    f.Dispose();f=new Fixture(true,path);Canvas();Check(f.Item(0).state==1&&f.Rows.Daily.Liveness==30&&f.Layout.GetChild(0).GetChild(0).GetChild(1).gameObject.activeSelf,"Independent storage owner restores original opened tier and accumulated liveness");
                    Check(f.Rows.Page.GetComponentsInChildren<Animation>(true).Length==6,"Restarted original panel retains all six animation components");Finish(null);
                }
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);Time.timeScale=1;report.passed=error==null;report.error=error?.ToString();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/task-liveness-native-validation.json"),JsonUtility.ToJson(report,true));if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
