using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskLivenessValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameTaskRowsValidation.Fixture Rows;
            public readonly OutgameLivenessPreviewServices PreviewServices;
            public readonly OutgameTaskLivenessServices Services;
            public readonly OutgameLivenessPreviewItem Preview;
            public readonly OutgameTaskLivenessBinding Binding;
            public readonly List<Action> Added=new List<Action>(),Removed=new List<Action>();
            public readonly List<TaskCompletionSource<bool>> PreviewGates=new List<TaskCompletionSource<bool>>(),ClaimGates=new List<TaskCompletionSource<bool>>();
            public readonly List<WaitForEndOfFrame> PreviewInstructions=new List<WaitForEndOfFrame>(),ClaimInstructions=new List<WaitForEndOfFrame>();
            public readonly List<int> Closed=new List<int>();public int Shows,Red,PopupId;public RectTransform PopupRect;public Action ShowCallback,RedCallback,SpriteCallback;public GameObject Selected;
            public string Icon,Atlas;public bool NativeSize;public Func<OriginalConfig.Lang,string> Language=l=>"奖励名称";
            public Transform Layout=>Binding.ProgressBackground.GetChild(1);
            public OutgameTaskLivenessItemData Item(int index)=>Rows.Daily.LivenessItems[index];
            public Button Button(int index)=>Layout.GetChild(index).GetComponent<Button>();
            public GameObject Glow(int index)=>Layout.GetChild(index).GetChild(0).GetChild(2).gameObject;
            public Fixture(bool native=false,string path=null)
            {
                Rows=new OutgameTaskRowsValidation.Fixture(native,path);
                PreviewServices=new OutgameLivenessPreviewServices{SetSprite=(image,icon,atlas,size)=>{Icon=icon;Atlas=atlas;NativeSize=size;SpriteCallback?.Invoke();},Language=l=>Language(l),PopItemInfo=(rect,id)=>{PopupRect=rect;PopupId=id;},SelectedObject=()=>Selected,
                    AddUpdate=a=>{Added.Add(a);return native?OutgameUpdateManager.AddHandle(a):Added.Count;},RemoveUpdate=a=>{Removed.Add(a);if(native)OutgameUpdateManager.RemoveHandle(a);},Destroy=Rows.Services.Destroy};
                if(!native)PreviewServices.EndOfFrame=w=>{PreviewInstructions.Add(w);var gate=new TaskCompletionSource<bool>();PreviewGates.Add(gate);return gate.Task;};
                Preview=new OutgameLivenessPreviewItem(Rows.Page.transform.Find("Content/LivenessPreviewItem").gameObject,PreviewServices);Selected=Preview.Touch.gameObject;
                Services=new OutgameTaskLivenessServices{Daily=()=>Rows.Daily,Config=()=>Rows.Config,GoodsType=Rows.Services.GoodsType,FormatNumber=(n,b)=>{Require(!b,"source format flag false");return n.ToString();},
                    ShowEffect=(id,node)=>{Require(id==1016&&node.parent==Layout,"source1016 and original tier node");ShowCallback?.Invoke();return ++Shows;},CloseEffect=Closed.Add,Effects=()=>Rows.Fx,ShowAward=Rows.Services.ShowAward,RefreshTopInfo=Rows.Services.RefreshTopInfo,
                    SetDailyTabReddot=()=>{Red++;RedCallback?.Invoke();},Messages=()=>Rows.Messages};
                if(!native)Services.EndOfFrame=w=>{ClaimInstructions.Add(w);var gate=new TaskCompletionSource<bool>();ClaimGates.Add(gate);return gate.Task;};
                Binding=new OutgameTaskLivenessBinding(Rows.Page.transform.Find("Content/Content/TaskContent/ProgressBg"),Preview,Services);Binding.Refresh(true);
            }
            public void Ready(int value=30){Rows.Tasks.Child.ModuleData.ext.livenessValue=value;Binding.RefreshPointDailyLivenessBar();}
            public Task Refresh()=>WithoutContext(Preview.RefreshAsync);
            public Task Claim(int index=0)=>WithoutContext(()=>Binding.ClaimAsync(Layout.GetChild(index),Item(index)));
            public static Task WithoutContext(Func<Task> body){var previous=SynchronizationContext.Current;SynchronizationContext.SetSynchronizationContext(null);try{return body();}finally{SynchronizationContext.SetSynchronizationContext(previous);}}
            public static void Release(TaskCompletionSource<bool> gate,Exception error=null){var previous=SynchronizationContext.Current;SynchronizationContext.SetSynchronizationContext(null);try{if(error==null)gate.SetResult(true);else gate.SetException(error);}finally{SynchronizationContext.SetSynchronizationContext(previous);}}
            public static void Finish(Task task,TaskCompletionSource<bool> gate){Release(gate);Require(task.IsCompleted,"controlled continuation completed");task.GetAwaiter().GetResult();}
            public void Dispose()=>Rows.Dispose();
        }
        public static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original LivenessPreviewItem and task tier bindings with actual daily strategy/economy, source Animation components and controlled frame endpoints. Native Unity awaits/animations separate. Effect1016/popup/sprite/language/report hosts observed; full task page, Achievement, Main and audiovisual/Player acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id="task-liveness-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-liveness-"+id,result="fail",detail=e.ToString()});}};
            check("original-animation-components-clips-and-tier-wiring",()=>{
                using(var f=new Fixture()){
                    var animations=f.Rows.Page.GetComponentsInChildren<Animation>(true);Require(animations.Length==6&&animations.All(a=>a.enabled&&a.clip&&a.clip.legacy&&a.GetClipCount()==1),"six original enabled legacy players");
                    Require(animations.Count(a=>a.playAutomatically)==3&&animations.Count(a=>!a.playAutomatically)==3,"only original glow auto-plays");
                    Require(f.Layout.childCount==3&&f.Button(0).GetComponentInChildren<Text>().text=="30"&&f.Button(1).GetComponentInChildren<Text>().text=="60"&&f.Button(2).GetComponentInChildren<Text>().text=="100","original threshold labels30/60/100 and three tier nodes");
                    Require(animations.Where(a=>!a.playAutomatically).All(a=>Mathf.Abs(a.clip.length-1.7f)<.0001f)&&animations.Where(a=>a.playAutomatically).All(a=>Mathf.Abs(a.clip.length-1.5f)<.0001f),"original clip durations preserved");
                }
            });
            check("preview-initialization-visibility-selection-and-pointer-details",()=>{
                using(var f=new Fixture()){
                    Require(f.Added.Count==1&&f.Preview.Visible&&!f.Preview.Lifetime.GameObject.activeSelf,"visibility registers before Awake directly deactivates root");
                    f.Preview.SetVisible(true);Require(f.Added.Count==2&&f.Preview.Lifetime.Transform.localScale==Vector3.one,"repeated visible registers same source handle");
                    f.Added[0]();Require(f.Preview.Visible,"touch exact name kept");f.Selected=f.Preview.IconBackground;f.Added[0]();Require(f.Preview.Visible,"IconBg exact name kept");
                    var selected=new GameObject("xNodey");try{f.Selected=selected;f.Added[0]();Require(f.Preview.Visible,"case-sensitive Node substring kept");selected.name="node";f.Added[0]();Require(!f.Preview.Visible&&f.Preview.Lifetime.GameObject.activeSelf&&f.Preview.Lifetime.Transform.localScale==Vector3.zero,"outside selection hides by scale");}finally{UnityEngine.Object.DestroyImmediate(selected);}
                    f.Preview.SetData(f.Rows.Config.dicGameItem[1002],4);int clicks=0;f.Preview.OnClick=p=>clicks++;OutgameUiPointerClick.Get(f.Preview.Lifetime.GameObject).OnPointerClick(null);OutgameUiPointerClick.Get(f.Preview.IconBackground).OnPointerClick(null);
                    Require(clicks==1&&f.PopupId==1002&&f.PopupRect==f.Preview.IconBackground.GetComponent<RectTransform>(),"root invokes callback; IconBg forwards live config ID and its own rect");
                }
            });
            check("preview-publishes-snapshots-before-frame-and-grow-only-sizing",()=>{
                using(var f=new Fixture()){
                    var data=f.Rows.Config.dicGameItem[1001];f.Preview.SetData(data,-7);data.ItemIcon="changed later";var original=f.Preview.SpriteName;
                    float w=f.Preview.PreviousNameWidth,left=f.Preview.BackgroundLeft.sizeDelta.x,right=f.Preview.BackgroundRight.sizeDelta.x,root=f.Preview.RootRect.sizeDelta.x;
                    var task=f.Refresh();Require(!task.IsCompleted&&f.Icon==original&&f.Atlas==data.atlasName&&!f.NativeSize&&f.Preview.CountText.text=="x-7"&&f.Preview.NameText.text=="奖励名称","SetData snapshots ItemIcon/atlas/name then Refresh displays synchronously");
                    f.Preview.NameRect.sizeDelta=new Vector2(w+80,f.Preview.NameRect.sizeDelta.y);Fixture.Finish(task,f.PreviewGates[0]);Require(f.Preview.PreviousNameWidth==w+80&&f.Preview.BackgroundLeft.sizeDelta.x==left+40&&f.Preview.BackgroundRight.sizeDelta.x==right+40&&f.Preview.RootRect.sizeDelta.x==root+80,"post-frame name width growth expands each half and root exactly");
                    task=f.Refresh();f.Preview.NameRect.sizeDelta=new Vector2(w,f.Preview.NameRect.sizeDelta.y);Fixture.Finish(task,f.PreviewGates[1]);Require(f.Preview.PreviousNameWidth==w+80&&f.Preview.RootRect.sizeDelta.x==root+80&&ReferenceEquals(f.PreviewInstructions[0],f.PreviewInstructions[1]),"shrinking text does not shrink frame or cached maximum; frame instruction reused");
                }
            });
            check("preview-reentry-overlap-fault-and-dispose-during-await",()=>{
                using(var f=new Fixture()){
                    var first=f.Rows.Config.dicGameItem[1001];var second=f.Rows.Config.dicGameItem[1002];f.Preview.SetData(first,1);f.SpriteCallback=()=>f.Preview.SetData(second,23);var a=f.Refresh();
                    Require(f.Icon==first.ItemIcon&&f.Preview.Data==second&&f.Preview.CountText.text=="x23","sprite request captures old strings; later fields use callback-updated data");f.SpriteCallback=null;var b=f.Refresh();float w=f.Preview.PreviousNameWidth,root=f.Preview.RootRect.sizeDelta.x;f.Preview.NameRect.sizeDelta=new Vector2(w+20,f.Preview.NameRect.sizeDelta.y);
                    Fixture.Finish(a,f.PreviewGates[0]);Fixture.Finish(b,f.PreviewGates[1]);Require(f.Preview.RootRect.sizeDelta.x==root+20,"overlapping continuations share current width maximum without doubling");
                    var error=f.Refresh();Fixture.Release(f.PreviewGates[2],new InvalidOperationException("frame"));Throws<InvalidOperationException>(()=>error.GetAwaiter().GetResult());
                    var pending=f.Refresh();int removed=f.Removed.Count;f.Preview.OnClick=p=>{};f.Preview.Dispose();Fixture.Finish(pending,f.PreviewGates[3]);Require(f.Preview.Lifetime.IsDisposed&&f.Preview.OnClick==null&&f.Preview.Data==second&&f.Removed.Count==removed,"dispose during wait exits resize, retains data and original non-removal of update listener");
                }
            });
            check("preview-button-opens-before-config-failure-and-only-first-reward",()=>{
                using(var f=new Fixture()){
                    f.Button(0).onClick.Invoke();Require(f.Preview.Visible&&f.Preview.Lifetime.GameObject.activeSelf&&f.Preview.Data.id==1001&&f.Preview.Amount==50&&f.PreviewGates.Count==1,"unready tier button shows first configured reward at original node");
                    Fixture.Release(f.PreviewGates[0]);f.Item(0).Config.rewards[0].datas[0]=987654;Throws<KeyNotFoundException>(()=>f.Binding.ShowPreview(f.Layout.GetChild(0),f.Item(0)));
                    Require(f.Preview.Visible&&f.Preview.Lifetime.Transform.position==f.Layout.GetChild(0).position&&f.Preview.Data.id==1001,"missing config throws after preview activation/position before SetData");
                }
            });
            check("ready-replaces-all-listeners-and-shows-canvas-layer",()=>{
                using(var f=new Fixture()){
                    int external=0;f.Button(0).onClick.AddListener(()=>external++);f.Ready();Require(f.Red==1&&f.Glow(0).activeSelf&&f.Glow(0).GetComponent<Canvas>().sortingLayerName==OutgameUiLayerNames.Get(2),"minimum threshold refresh lights exact source canvas layer");
                    var task=f.Claim();Require(!task.IsCompleted&&f.Button(0).enabled&&!f.Glow(0).activeSelf&&f.Shows==1&&f.Binding.EffectHandle==1,"effect and hidden glow precede frame; Button still enabled");Fixture.Finish(task,f.ClaimGates[0]);
                    Require(!f.Button(0).enabled&&f.Item(0).state==1&&f.Rows.Tasks.Items.Global.GetItemCount(1001)==50&&f.Red==2,"post-frame effect then disable, actual reward/bit state and red refresh");
                    Require(f.Rows.Fx.Kind==1&&f.Rows.Fx.Amount==50&&!f.Rows.Fx.Apply&&f.Rows.Fx.Root==null&&external==0,"currency effect requests no economic award");f.Rows.Fx.Completion();Require(f.Rows.TopRefresh==1,"fly completion invokes top refresh");
                    f.Binding.CloseCurrentEffect();Require(f.Closed.SequenceEqual(new[]{1})&&f.Binding.EffectHandle==0,"tab cleanup closes stored handle and clears after success");
                }
            });
            check("no-invented-repeat-guard-before-end-of-frame",()=>{
                using(var f=new Fixture()){
                    f.Ready();var a=f.Claim();var b=f.Claim();Require(f.Shows==2&&f.Button(0).enabled&&!ReferenceEquals(f.ClaimInstructions[0],f.ClaimInstructions[1]),"source allows overlapping claims and allocates frame instruction per claim");
                    Fixture.Finish(a,f.ClaimGates[0]);Fixture.Finish(b,f.ClaimGates[1]);Require(f.Rows.Tasks.Items.Global.GetItemCount(1001)==100&&f.Item(0).state==1&&f.Binding.EffectHandle==2,"original unguarded liveness endpoint awards both pending callbacks; no fabricated deduplication");
                }
            });
            check("effect-frame-fly-and-red-error-order",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.Binding.EffectHandle=77;f.ShowCallback=()=>throw new InvalidOperationException("effect");var task=f.Claim();Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(!f.Glow(0).activeSelf&&f.Binding.EffectHandle==77&&f.ClaimGates.Count==0&&f.Button(0).enabled,"Show failure retains old handle and prevents wait/disable/award");
                    f.ShowCallback=null;task=f.Claim();Fixture.Release(f.ClaimGates[0],new InvalidOperationException("wait"));Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(f.Rows.Tasks.Items.Global.GetItemCount(1001)==0&&f.Button(0).enabled,"await failure prevents economic work");
                    f.Rows.Fx.Call=()=>throw new InvalidOperationException("fly");task=f.Claim();Fixture.Release(f.ClaimGates[1]);Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(f.Button(0).enabled&&f.Item(0).state==0,"fly failure occurs before disable and award");
                    f.Rows.Fx.Call=null;f.RedCallback=()=>throw new InvalidOperationException("red");task=f.Claim();Fixture.Release(f.ClaimGates[2]);Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(!f.Button(0).enabled&&f.Item(0).state==1&&f.Rows.Tasks.Items.Global.GetItemCount(1001)==50,"post-award red failure preserves inventory/state/disabled button");
                }
            });
            check("reward-snapshot-before-wait-and-live-id-after-host-reentry",()=>{
                using(var f=new Fixture()){
                    f.Ready(100);var item=f.Item(0);var task=f.Claim();f.Rows.Fx.Call=()=>item.id=2;Fixture.Finish(task,f.ClaimGates[0]);Require(f.Rows.Fx.Amount==50&&f.Rows.Tasks.Items.Global.GetItemCount(1001)==100&&f.Item(1).state==1,"effect uses pre-wait count50; actual claim reads callback-changed ID2 and awards100");
                }
            });
            check("item-award-quantity-one-before-all-configured-rewards",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.Services.GoodsType=id=>6;var task=f.Claim();Fixture.Finish(task,f.ClaimGates[0]);Require(f.Rows.Trace.Contains("award:1001:1")&&f.Rows.Tasks.Items.Global.GetItemCount(1001)==50,"noncurrency popup shows one while real strategy grants configured50");
                    f.Services.CloseEffect=id=>throw new InvalidOperationException("close");Throws<InvalidOperationException>(f.Binding.CloseCurrentEffect);Require(f.Binding.EffectHandle==1,"close failure retains handle");
                }
            });
            check("refreshed-state-does-not-clear-old-ready-callback-or-glow",()=>{
                using(var f=new Fixture()){
                    f.Ready();f.Item(0).state=1;f.Binding.Refresh(false);Require(f.Glow(0).activeSelf&&f.Button(0).enabled,"source state1 branch leaves glow and old ready listener untouched");
                    f.Rows.Tasks.Child.ModuleData.ext.livenessValue=0;f.Binding.RefreshPointDailyLivenessBar();Require(f.Red==2&&f.Glow(0).activeSelf,"below minimum refresh updates red only");
                }
            });
            check("real-liveness-award-save-and-independent-tier-state-restart",()=>{
                string path=Path.Combine(Path.GetTempPath(),"AreaBattleLiveness-"+Guid.NewGuid().ToString("N"));
                using(var f=new Fixture(false,path)){f.Ready(100);var task=f.Claim(2);Fixture.Finish(task,f.ClaimGates[0]);Require(f.Rows.Tasks.Items.Global.GetItemCount(1002)==20,"third tier actual diamonds");f.Rows.Tasks.Manager.OnSave();}
                using(var f=new Fixture(false,path)){Require(f.Rows.Daily.Liveness==100&&f.Item(2).state==1&&f.Layout.GetChild(2).GetChild(0).GetChild(1).gameObject.activeSelf,"independent ext mask restores third tier opened state");}
            });
            return report;
        }
    }
}
