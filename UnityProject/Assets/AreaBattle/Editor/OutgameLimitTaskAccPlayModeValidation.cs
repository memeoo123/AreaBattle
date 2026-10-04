using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using Fixture=AreaBattle.EditorTools.OutgameLimitTaskAccValidation.Fixture;
namespace AreaBattle.EditorTools
{
    [InitializeOnLoad] public static class OutgameLimitTaskAccPlayModeValidation
    {
        const string Pending="AreaBattle.LimitTaskAccNative";
        [Serializable] sealed class Report
        {
            public bool passed;public string error;
            public string scope="Original accumulator and preview prefabs, actual pointer reward claim, real WaitForEndOfFrame and UpdateManager selection dismissal, automatic activity save and independent disk restart. Currency animation/skin UI/localization/sprite/detail delivery remain observed required hosts. Inventory persistence, full page lifecycle/menu/Main/platform/Player/audiovisual acceptance pending.";
            public List<string> checks=new List<string>();
        }
        static Fixture fixture;static OutgameSevendayAccPreviewItem preview;static Report report;static Task first,second;static GameObject oldReward;static int phase,frame;static double began;static bool stopped;static string path;
        static OutgameLimitTaskAccPlayModeValidation(){EditorApplication.playModeStateChanged+=Changed;}
        public static void Run()
        {SessionState.SetBool(Pending,true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);if(!Application.isBatchMode)EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.EnterPlaymode();}
        static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))Start();}
        static void Check(bool value,string text){if(!value)throw new Exception(text);report.checks.Add(text);}
        static void Phase(int value){phase=value;frame=Time.frameCount;}
        static void Start()
        {
            report=new Report();stopped=false;began=EditorApplication.timeSinceStartup;Time.timeScale=1;
            try{
                var camera=new GameObject("ValidationCamera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-10);
                new GameObject("EventSystem",typeof(EventSystem));path=Path.Combine(Path.GetTempPath(),"AreaBattleOriginalAcc-"+Guid.NewGuid().ToString("N"));
                fixture=new Fixture(true,path);
                preview=new OutgameSevendayAccPreviewItem(fixture.Rows.Days.Page.transform.Find("SevendayAccPreviewItem").gameObject,new OutgameSevendayAccPreviewServices{Rewards=fixture.Rows.Services.Rewards});
                EventSystem.current.SetSelectedGameObject(preview.Touch.gameObject);
                fixture.PreviewCallback=()=>{preview.SetData(fixture.Previewed.RewardsData);first=preview.RefreshAsync();};
                OutgameLimitTaskUiItemsValidation.Click(fixture.ItemRoot);
                Check(fixture.PreviewCount==1&&preview.Items.Count==0&&!first.IsCompleted,"Unready original accumulator pointer opens actual preview and waits for frame end before reward creation");
                Phase(0);EditorApplication.update+=Poll;
            }catch(Exception e){Finish(e);}
        }
        static void Poll()
        {
            if(stopped)return;
            try{
                if(EditorApplication.timeSinceStartup-began>45)throw new TimeoutException("LimitTask accumulator native phase "+phase);
                if(Time.frameCount<=frame+3)return;
                if(phase==0)
                {
                    first.GetAwaiter().GetResult();Check(first.IsCompleted&&preview.Visible&&preview.Items.Count==2&&preview.Items[0].Count.text=="50","Actual Unity end-of-frame continuation builds original configured preview rewards while touch selection keeps it visible");
                    OutgameLimitTaskUiItemsValidation.Click(preview.Items[0].Lifetime.GameObject);Check(fixture.Rows.Trace.Contains("popup:1001"),"Original native preview reward pointer reaches detail host with item1001");
                    oldReward=preview.Items[0].Lifetime.GameObject;first=preview.RefreshAsync();second=preview.RefreshAsync();
                    Check(preview.Items.Count==0&&oldReward,"Refresh clears managed rows before deferred native destruction");Phase(1);return;
                }
                if(phase==1)
                {
                    first.GetAwaiter().GetResult();second.GetAwaiter().GetResult();Check(!oldReward&&preview.Items.Count==4,"Two actual frame awaits append both reward sets; old native clone destroyed at frame end");
                    EventSystem.current.SetSelectedGameObject(null);Phase(2);return;
                }
                if(phase==2)
                {
                    Check(!preview.Visible&&preview.Lifetime.GameObject.activeSelf&&preview.Lifetime.Transform.localScale==Vector3.zero,"Actual UpdateManager checks EventSystem selection and hides preview on null");
                    fixture.Ready();fixture.EffectsHost.Trace.Clear();OutgameLimitTaskUiItemsValidation.Click(fixture.ItemRoot);
                    Check(fixture.Item.Data.state==1&&fixture.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==50&&fixture.EffectsHost.Amount==50&&fixture.CompletionMessages==0,"Actual accumulator pointer awards gold once before deferred currency completion");
                    OutgameLimitTaskUiItemsValidation.Click(fixture.ItemRoot);fixture.EffectsHost.Completion();
                    Check(fixture.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==50&&fixture.CompletionMessages==1&&fixture.Item.BoxUnlocked.gameObject.activeSelf,"Duplicate native pointer keeps single award; effect callback delivers original red-refresh message");
                    Phase(3);return;
                }
                if(phase==3)
                {
                    Check(!fixture.Rows.Days.Tasks.Parent.Dirty&&fixture.Rows.Days.Tasks.Stored.Length>0,"Actual UpdateManager automatically saves accumulator claimed bit through activity parent");
                    preview=null;fixture.Dispose();fixture=new Fixture(true,path);
                    Check(fixture.Item.Data.state==1&&fixture.Child.ModuleData.ext.AccProgress>=80,"Independent original activity reconstruction restores claimed accumulator and task-derived liveness from disk");
                    Phase(4);return;
                }
                OutgameLimitTaskUiItemsValidation.Click(fixture.ItemRoot);
                Check(fixture.Item.BoxUnlocked.gameObject.activeSelf&&fixture.Rows.Days.Tasks.Items.Host.Adds==0&&fixture.Rows.Days.Tasks.Items.Global.GetItemCount(1001)==0,"Reopened original claimed accumulator renders unlocked and prevents another award; inventory host remains independently reset");
                Finish(null);
            }catch(Exception e){Finish(e);}
        }
        static void Finish(Exception error)
        {
            if(stopped)return;stopped=true;EditorApplication.update-=Poll;SessionState.SetBool(Pending,false);
            try{fixture?.Dispose();fixture=null;}catch(Exception e){if(error==null)error=e;}
            report.passed=error==null;report.error=error?.ToString();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/limit-task-acc-native-validation.json"),JsonUtility.ToJson(report,true));
            if(error!=null)Debug.LogException(error);EditorApplication.Exit(error==null?0:1);
        }
    }
}
