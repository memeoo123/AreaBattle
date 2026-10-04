using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameSevendayAccPreviewValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTaskRowsValidation.Fixture Rows;
            public readonly OutgameSevendayAccPreviewServices Services;
            public readonly OutgameSevendayAccPreviewItem Item;
            public readonly List<Action> Added=new List<Action>(),Removed=new List<Action>();
            public readonly List<TaskCompletionSource<bool>> Gates=new List<TaskCompletionSource<bool>>();
            public GameObject Selected;
            public Fixture(bool native=false,string path=null)
            {
                Rows=new OutgameLimitTaskRowsValidation.Fixture(native,path);
                Services=new OutgameSevendayAccPreviewServices{Rewards=Rows.Services.Rewards,Destroy=Rows.Services.Destroy,
                    SelectedObject=()=>Selected,AddUpdate=a=>{Added.Add(a);return Added.Count;},RemoveUpdate=a=>Removed.Add(a),
                    EndOfFrame=instruction=>{Require(instruction!=null,"source frame instruction");var gate=new TaskCompletionSource<bool>();Gates.Add(gate);return gate.Task;}};
                Item=new OutgameSevendayAccPreviewItem(Rows.Days.Page.transform.Find("SevendayAccPreviewItem").gameObject,Services);
                Item.SetData(Rows.Child.AccRewards[0].RewardsData);Selected=Item.Touch.gameObject;
            }
            public Task Refresh()
            {
                // Deterministic injected await endpoint; native runner verifies Unity frame/context separately.
                var previous=SynchronizationContext.Current;SynchronizationContext.SetSynchronizationContext(null);
                try{return Item.RefreshAsync();}finally{SynchronizationContext.SetSynchronizationContext(previous);}
            }
            public void Release(int index,Exception error=null)
            {
                var previous=SynchronizationContext.Current;SynchronizationContext.SetSynchronizationContext(null);
                try{if(error==null)Gates[index].SetResult(true);else Gates[index].SetException(error);}finally{SynchronizationContext.SetSynchronizationContext(previous);}
            }
            public void Finish(int index,Task task){Release(index);Require(task.IsCompleted,"injected continuation completed");task.GetAwaiter().GetResult();}
            public void Dispose()=>Rows.Dispose();
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original preview hierarchy and source async ordering using controlled frame endpoint; native Unity await/update checks separate. Full activity page/Main/platform/Player acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="sevenday-acc-preview-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="sevenday-acc-preview-"+id,result="fail",detail=e.ToString()});}};
            check("visibility-registers-every-call-and-original-scale",()=>{
                using(var f=new Fixture()){
                    Require(f.Added.Count==1&&f.Item.Visible&&f.Item.Lifetime.GameObject.activeSelf,"initial visible registers update");
                    f.Item.SetVisible(true);Require(f.Added.Count==2&&f.Added[0]==f.Added[1],"repeated visible registers same callback again");
                    f.Item.SetVisible(false);f.Item.SetVisible(false);Require(f.Removed.Count==2&&!f.Item.Visible&&f.Item.Lifetime.GameObject.activeSelf&&f.Item.Lifetime.Transform.localScale==Vector3.zero,"repeated hidden queues removal and source zero scale");
                    f.Item.SetVisible(true);Require(f.Item.Lifetime.Transform.localScale==Vector3.one,"source restore scale");
                }
            });
            check("selected-object-exact-touch-and-case-sensitive-substrings",()=>{
                using(var f=new Fixture()){
                    f.Added[0]();Require(f.Item.Visible,"exact touch name remains visible");
                    var selected=new GameObject("xxNodeyy");try{
                        f.Selected=selected;f.Added[0]();Require(f.Item.Visible,"Node substring accepted");
                        selected.name=f.Item.RewardTemplate.name+"(Clone)";f.Added[0]();Require(f.Item.Visible,"reward template substring accepted");
                        selected.name="node";f.Added[0]();Require(!f.Item.Visible,"lowercase node rejected");
                        f.Item.SetVisible(true);selected.name=f.Item.Touch.name+"x";f.Added[0]();Require(!f.Item.Visible,"touch requires exact name");
                        f.Item.SetVisible(true);f.Selected=null;f.Added[0]();Require(!f.Item.Visible,"null selection dismisses");
                    }finally{UnityEngine.Object.DestroyImmediate(selected);}
                }
            });
            check("setdata-only-then-live-data-after-frame-and-original-reward-click",()=>{
                using(var f=new Fixture()){
                    Require(f.Gates.Count==0&&f.Item.Items.Count==0,"SetData does not refresh");var task=f.Refresh();Require(!task.IsCompleted&&f.Item.Items.Count==0,"wait before clone creation");
                    var replacement=new List<OutgameItemReward>{new OutgameItemReward{itemId=1002,itemCount=37}};f.Item.SetData(replacement);f.Finish(0,task);
                    var reward=f.Item.Items[0];Require(f.Item.Items.Count==1&&ReferenceEquals(reward.Data,replacement[0])&&reward.Count.text=="37"&&reward.Lifetime.Transform.parent==f.Item.RewardParent,"post-await live data, original parent and count");
                    OutgameUiPointerClick.Get(reward.Lifetime.GameObject).OnPointerClick(null);Require(f.Rows.Trace.Contains("popup:1002"),"actual reward pointer forwards live item detail id");
                    var old=reward;task=f.Refresh();Require(old.Lifetime.IsDisposed&&f.Item.Items.Count==0&&!task.IsCompleted,"clear children happens before next await");f.Finish(1,task);
                }
            });
            check("overlapping-refresh-appends-and-hidden-preview-still-builds",()=>{
                using(var f=new Fixture()){
                    var first=f.Refresh();var second=f.Refresh();f.Item.SetVisible(false);f.Finish(0,first);int n=f.Item.Data.Count;f.Finish(1,second);
                    Require(f.Item.Items.Count==2*n&&!f.Item.Visible,"source has no cancellation, coalescing or visibility gate");
                }
            });
            check("dispose-during-await-exits-and-keeps-source-listener-contract",()=>{
                using(var f=new Fixture()){
                    var first=f.Refresh();f.Finish(0,first);var old=f.Item.Items[0];int n=f.Item.Items.Count;f.Item.OnClick=item=>{};f.Item.Dispose();
                    Require(f.Item.Lifetime.IsDisposed&&f.Item.OnClick==null&&f.Item.Items.Count==n&&!old.Lifetime.IsDisposed&&f.Removed.Count==0,"Dispose destroys native root, retains item objects and update callback as source");
                }
                using(var f=new Fixture()){
                    var task=f.Refresh();f.Item.Dispose();f.Finish(0,task);Require(f.Item.Items.Count==0,"destroyed root stops pending continuation");
                }
            });
            check("await-and-sprite-failures-preserve-source-partial-work",()=>{
                using(var f=new Fixture()){
                    var first=f.Refresh();f.Finish(0,first);var old=f.Item.Items[0];var task=f.Refresh();f.Release(1,new InvalidOperationException("frame failure"));
                    Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(old.Lifetime.IsDisposed&&f.Item.Items.Count==0,"await failure after old items cleared");
                    f.Rows.SpriteCallback=()=>throw new InvalidOperationException("sprite failure");int before=f.Item.RewardParent.childCount;task=f.Refresh();f.Release(2);
                    Throws<InvalidOperationException>(()=>task.GetAwaiter().GetResult());Require(f.Item.Items.Count==0&&f.Item.RewardParent.childCount==before+1,"clone remains untracked when SetData throws before append");
                }
            });
            check("preview-pointer-only-calls-hook-without-dismissal",()=>{
                using(var f=new Fixture()){
                    int calls=0;f.Item.OnClick=item=>{Require(ReferenceEquals(item,f.Item),"self callback");calls++;};OutgameUiPointerClick.Get(f.Item.Lifetime.GameObject).OnPointerClick(null);
                    Require(calls==1&&f.Item.Visible&&f.Removed.Count==0,"source preview click does not close automatically");
                }
            });
            return r;
        }
    }
}
