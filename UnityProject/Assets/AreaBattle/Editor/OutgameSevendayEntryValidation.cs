using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameSevendayEntryValidation
    {
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameLimitTaskPageValidation.Fixture Page;
            public readonly GameObject Main;
            public readonly OutgameSevendayEntryServices Services;
            public readonly OutgameSevendayEntryBinding Entry;
            public readonly List<string> Trace=new List<string>();
            public Action DuringControl,DuringVoice;public int ControlReads;public object LastButton;
            public Fixture(bool native=false,string path=null)
            {
                Page=new OutgameLimitTaskPageValidation.Fixture(native,path);
                Main=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/Proj_xqzdStartUI"),Page.Layer,false);Main.SetActive(true);
                Services=new OutgameSevendayEntryServices{Control=()=>{ControlReads++;DuringControl?.Invoke();return Page.Sevenday;},Pages=()=>{Trace.Add("page-owner");return Page.OpenRegistry;},Messages=()=>Page.Messages,
                    PlayVoice=(group,id)=>{Trace.Add("voice:"+group+":"+id);DuringVoice?.Invoke();}};
                Entry=new OutgameSevendayEntryBinding(Main.transform,Services);
                Page.Messages.AddListener("GF_UIButtonClick",args=>{LastButton=args[0];Trace.Add("button");});
            }
            public void ClaimAllPrerequisites(){foreach(var list in Page.Child.DayTasks.Values)foreach(var task in list)task.state=1;foreach(var reward in Page.Child.AccRewards)reward.state=1;}
            public void Dispose(){Entry.Dispose();UnityEngine.Object.DestroyImmediate(Main);Page.Dispose();}
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original main-page seven-day entry slice connected to actual control, messages and composed page registry. Full Proj_xqzdStartUI/Main lifecycle assembly remains pending. Voice and other page effect/resource/report hosts are explicit fixtures; native flow separate."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="sevenday-entry-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="sevenday-entry-"+id,result="fail",detail=e.ToString()});}};
            check("original-button-voice-page-registry-and-button-message-order",()=>{
                using(var f=new Fixture()){
                    f.Entry.Initialize();Require(f.Entry.Button.gameObject.activeSelf&&f.Entry.Button.transform.parent.name=="RigthBar","original entry hierarchy and unlock query");
                    f.Trace.Clear();f.Entry.Button.onClick.Invoke();Require(string.Join(",",f.Trace)=="voice:1:2001,page-owner,button"&&ReferenceEquals(f.LastButton,f.Entry.Button),"voice before actual page open and original GF_UIButtonClick afterward");
                    var p=f.Page.Page;Require(p!=null&&p.Lifetime.Arguments is object[] args&&args.Length==0&&f.Page.Loads.Count==1,"typed original CommonLimitTimeTaskUI route and empty arguments");
                    f.Entry.Button.onClick.Invoke();Require(f.Page.Page==p&&f.Page.Loads.Count==1,"registry prevents duplicate page while entry repeats source sound/button event");f.Page.CompleteLoad();f.Page.Tick();Require(p.AccItems.Count==8,"entry opens complete actual page components");
                }
            });
            check("locked-initialization-skips-listeners-and-later-unlock-message",()=>{
                using(var f=new Fixture()){
                    f.Page.Sevenday.IsInActivity();f.Page.Now=f.Page.Sevenday.GetActDate(false);f.Entry.Initialize();Require(!f.Entry.Button.gameObject.activeSelf,"closed interval hides entry");
                    f.Page.Now=f.Page.Sevenday.GetActDate(true)+1;int reads=f.ControlReads;f.Page.Messages.SendMessage("SevendayUnlock");f.Entry.Button.onClick.Invoke();
                    Require(!f.Entry.Button.gameObject.activeSelf&&f.ControlReads==reads&&f.Trace.Count==0,"source inactive initialization has neither click nor unlock listener; message cannot invent registration");
                }
            });
            check("three-source-messages-refresh-real-task-and-accumulator-red",()=>{
                using(var f=new Fixture()){
                    f.ClaimAllPrerequisites();f.Entry.Initialize();Require(!f.Entry.RedDot.activeSelf,"all claimed has no red");
                    var task=f.Page.Child.FindTask(1301101);task.state=0;foreach(var c in task.conditions)c.value=long.MaxValue;
                    foreach(string key in new[]{"SevendayUnlock","SevendayFinishTask","SevendayGetAccReward"}){f.Entry.RedDot.SetActive(false);f.Page.Messages.SendMessage(key,new object[]{"payload ignored"});Require(f.Entry.RedDot.activeSelf,key+" checks actual source red predicate");}
                    task.state=1;f.Page.Messages.SendMessage("SevendayFinishTask");Require(!f.Entry.RedDot.activeSelf,"task claimed removes red");
                    f.Page.Child.AccRewards[0].state=0;f.Page.Messages.SendMessage("SevendayGetAccReward");Require(f.Entry.RedDot.activeSelf,"actual eligible accumulator independently lights entry");
                }
            });
            check("end-message-updates-visibility-only-without-gating-existing-click",()=>{
                using(var f=new Fixture()){
                    f.Entry.Initialize();f.Entry.RedDot.SetActive(true);f.Page.Now=f.Page.Sevenday.GetActDate(false);f.Page.Messages.SendMessage("SevendayClose",new object[]{1});
                    Require(!f.Entry.Button.gameObject.activeSelf&&f.Entry.RedDot.activeSelf,"end checks current unlock but leaves red state untouched");f.Entry.Button.onClick.Invoke();Require(f.Page.Page!=null,"programmatic existing click has no added time guard");
                }
            });
            check("initial-visibility-uses-captured-unlock-despite-red-query-callback",()=>{
                using(var f=new Fixture()){
                    f.DuringControl=()=>{if(f.ControlReads==2)f.Page.Now=f.Page.Sevenday.GetActDate(false);};f.Entry.Initialize();
                    Require(f.Entry.Button.gameObject.activeSelf&&!f.Entry.RedDot.activeSelf,"initial true unlock captured before red callback moves clock to end");f.DuringControl=null;
                    f.Page.Messages.SendMessage("SevendayClose");Require(!f.Entry.Button.gameObject.activeSelf,"later visibility event uses current interval");
                }
            });
            check("voice-and-open-failures-stop-later-button-message",()=>{
                using(var f=new Fixture()){
                    f.Entry.Initialize();f.DuringVoice=()=>throw new InvalidOperationException("voice");Throws<InvalidOperationException>(()=>f.Entry.Button.onClick.Invoke());Require(f.Page.Page==null&&!f.Trace.Contains("button"),"voice failure precedes page lookup and button message");
                    f.DuringVoice=null;f.Trace.Clear();f.Services.Pages=()=>throw new InvalidOperationException("ui owner");Throws<InvalidOperationException>(()=>f.Entry.Button.onClick.Invoke());Require(string.Join(",",f.Trace)=="voice:1:2001","owner failure follows sound, prevents final button message");
                }
            });
            check("dispose-current-unlock-gate-and-retained-button-handler",()=>{
                using(var f=new Fixture()){
                    f.Entry.Initialize();f.Entry.Dispose();int reads=f.ControlReads;f.Page.Messages.SendMessage("SevendayFinishTask");f.Page.Messages.SendMessage("SevendayClose");Require(f.ControlReads==reads,"unlocked disposal removes all entry messages");f.Entry.Button.onClick.Invoke();Require(f.Page.Page!=null,"source Dispose does not remove native button callback");
                }
                using(var f=new Fixture()){
                    f.Entry.Initialize();f.Page.Now=f.Page.Sevenday.GetActDate(false);f.Entry.Dispose();int reads=f.ControlReads;f.Page.Messages.SendMessage("SevendayFinishTask");Require(f.ControlReads==reads+1&&!f.Entry.RedDot.activeSelf,"ended disposal skips removal, retained listener still runs");
                }
            });
            check("repeat-initialize-duplicates-and-removes-one-registration",()=>{
                using(var f=new Fixture()){
                    f.Entry.Initialize();f.Entry.Initialize();int reads=f.ControlReads;f.Page.Messages.SendMessage("SevendayUnlock");Require(f.ControlReads==reads+2,"no invented initialize guard or listener replacement");
                    f.Entry.Dispose();reads=f.ControlReads;f.Page.Messages.SendMessage("SevendayUnlock");Require(f.ControlReads==reads+1,"source event removal removes one delegate occurrence");
                    f.Trace.Clear();f.Entry.Button.onClick.Invoke();Require(f.Trace.FindAll(s=>s=="voice:1:2001").Count==2&&f.Page.Loads.Count==1,"duplicate button delegates repeat sound but module deduplicates page");
                }
            });
            check("red-managed-null-guard-and-captured-native-object",()=>{
                using(var f=new Fixture()){
                    f.Entry.RedDot=null;int reads=f.ControlReads;f.Entry.RefreshRed(null);Require(f.ControlReads==reads,"managed null skips control query");
                    var old=f.Entry.Button.transform.Find("imgReddot").gameObject;var replacement=new GameObject("replacement");try{
                        f.Entry.RedDot=old;f.ClaimAllPrerequisites();replacement.SetActive(true);f.DuringControl=()=>f.Entry.RedDot=replacement;f.Entry.RefreshRed(null);Require(!old.activeSelf&&replacement.activeSelf,"red callback holds original object through current control lookup");f.DuringControl=null;
                        UnityEngine.Object.DestroyImmediate(old);f.Entry.RedDot=old;Throws<MissingReferenceException>(()=>f.Entry.RefreshRed(null));
                    }finally{f.DuringControl=null;UnityEngine.Object.DestroyImmediate(replacement);f.Entry.RedDot=null;}
                }
            });
            return r;
        }
    }
}
