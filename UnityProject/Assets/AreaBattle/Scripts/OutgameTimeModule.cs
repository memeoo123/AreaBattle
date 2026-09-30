using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Source3535: elapsed resets before periodic callback, after game-pause callback.
    public sealed class OutgameLoopTimer
    {
        public int Id,Step;public float FloatStep,Elapsed,TotalTime;
        public bool IsFloat,UseGamePause,UseTimeScale,Paused;
        public Action<int> IntCallback;public Action<float> FloatCallback;
        public OutgameLoopTimer(Action<int> callback){IntCallback+=callback;}
        public OutgameLoopTimer(Action<float> callback){FloatCallback+=callback;}
        public void ClearElapsed()=>Elapsed=0;
        public void Pause(bool value)=>Paused=value;
        public void ResetTimer(bool clearTotal){Elapsed=0;if(clearTotal)TotalTime=0;}
        public void Destroy(){IntCallback=null;FloatCallback=null;}
        public void GamePause(){if(!UseGamePause)return;if(IsFloat)FloatCallback?.Invoke(Elapsed);else IntCallback?.Invoke(OutgameTimeClock.ToInt(Elapsed));Elapsed=0;}
        public void AddTimes(float delta,float unscaled)
        {if(Paused||!OutgameTimeClock.IsFocus)return;if(!UseTimeScale){delta=unscaled;if(delta>1)return;}Elapsed+=delta;TotalTime+=delta;}
        public void Check()
        {if(IsFloat){if(!(Elapsed>=FloatStep))return;Elapsed=0;FloatCallback?.Invoke(FloatStep);}else{if(!(Elapsed>=Step))return;Elapsed=0;IntCallback?.Invoke(Step);}}
    }
    // Source3536: Unity elapsed callbacks precede completion and can mutate the live state.
    public sealed class OutgameUnityTimer
    {
        public int Id;public float Duration,Elapsed;public bool UseTimeScale,Loop,IsComplete;
        public Action Complete;public Action<float> Every;
        public OutgameUnityTimer(int id,float duration,bool useTimeScale,bool loop,Action complete,Action<float> every)
        {Complete=complete;Loop=loop;UseTimeScale=useTimeScale;Duration=duration;Id=id;Every=every;}
        public void AddTimer()=>AddTimer(Time.deltaTime,Time.unscaledDeltaTime);
        public void AddTimer(float delta,float unscaled)
        {if(IsComplete||!OutgameTimeClock.IsFocus)return;if(UseTimeScale)Elapsed+=delta;else{if(unscaled>1)return;Elapsed+=unscaled;}Every?.Invoke(Elapsed);if(Elapsed>=Duration)CompleteTimer();}
        public void CompleteTimer(){Complete?.Invoke();if(Loop)Elapsed-=Duration;else IsComplete=true;}
    }
    // TimeModule3534. This module is ready for the source startup owner; full app composition is separate.
    public sealed class OutgameTimeModule:OutgameTimeCountdowns,IOutgameStartupModule
    {
        readonly Func<OutgameMessageDispatcher> messages;readonly Action<object[]> log,warning;
        public Action Initialized{get;set;}public bool IsInitialized;public int Priority=>60;
        public Dictionary<int,OutgameLoopTimer> LoopTimers;
        public readonly List<OutgameLoopTimer> AddLoopTimerData=new List<OutgameLoopTimer>();
        public readonly List<int> LoopTimerRemoveIds=new List<int>();
        public Dictionary<int,OutgameUnityTimer> UnityTimers,EverySecondTimers,EveryMinuteTimers;
        public readonly List<OutgameUnityTimer> NewUnityTimers=new List<OutgameUnityTimer>(),NewEverySecond=new List<OutgameUnityTimer>(),NewEveryMinute=new List<OutgameUnityTimer>();
        public readonly List<int> CompleteUnityTimers=new List<int>(),CashCompleteUnityTimers=new List<int>();
        public OutgameTimeModule(Func<OutgameMessageDispatcher> messages=null,Action<object[]> log=null,Action<object[]> warning=null,Func<DateTime> localNow=null):base(localNow)
        {this.messages=messages??(()=>OutgameMessageDispatcher.Shared);this.log=log??(args=>Debug.Log(args[0]));this.warning=warning??(args=>Debug.LogWarning(args[0]));}
        public override void Initialize()
        {InitializeCountdownMaps();LoopTimers=new Dictionary<int,OutgameLoopTimer>();UnityTimers=new Dictionary<int,OutgameUnityTimer>();EverySecondTimers=new Dictionary<int,OutgameUnityTimer>();EveryMinuteTimers=new Dictionary<int,OutgameUnityTimer>();ResetTimeFields();IsInitialized=true;Initialized?.Invoke();}
        public void Start(){messages().AddListener("GF_NewGamePause",GamePause);messages().AddListener("GF_GameFocus",GameFocus);}
        public void Update(){OutgameTimeClock.Now=localNow();CheckTime(OutgameTimeClock.Now,Time.realtimeSinceStartup);CheckLoopTimer(Time.deltaTime,Time.unscaledDeltaTime);CheckUnityTimer();}
        public void GamePause(object[] args){if((bool)args[0])foreach(var timer in LoopTimers.Values)timer.GamePause();}
        public void GameFocus(object[] args)=>OutgameTimeClock.IsFocus=(bool)args[0];
        public void Shutdown(){messages().RemoveListener("GF_NewGamePause",GamePause);messages().RemoveListener("GF_GameFocus",GameFocus);ClearTimer();IsInitialized=false;}
        public void ClearTimer()
        {ClearCountdownMaps();LoopTimerRemoveIds.Clear();LoopTimers.Clear();UnityTimers.Clear();EverySecondTimers.Clear();EveryMinuteTimers.Clear();ResetTimeFields();}
        public int AddLoopTimer(int step,bool useGamePause,bool useTimeScale,bool autoPlay,Action<int> callback)
        {int id=GetTimeId();var timer=new OutgameLoopTimer(callback){UseTimeScale=useTimeScale,UseGamePause=useGamePause,IsFloat=false,Step=step,Id=id};AddLoopTimerData.Add(timer);if(autoPlay)timer.Paused=false;log(new object[]{"当前循环计时器个数："+LoopTimers.Count});return id;}
        public int AddLoopTimer(float step,bool useGamePause,bool useTimeScale,bool autoPlay,Action<float> callback)
        {int id=GetTimeId();var timer=new OutgameLoopTimer(callback){UseTimeScale=useTimeScale,UseGamePause=useGamePause,IsFloat=true,FloatStep=step,Id=id};AddLoopTimerData.Add(timer);if(autoPlay)timer.Paused=false;log(new object[]{"当前循环计时器个数："+LoopTimers.Count});return id;}
        public int AddLoopTimer(int step,Action<int> callback,bool autoPlay,bool useTimeScale)=>AddLoopTimer(step,false,useTimeScale,autoPlay,callback);
        public int AddLoopTimer(int step,bool useGamePause,Action<int> callback,bool autoPlay,bool useTimeScale)=>AddLoopTimer(step,useGamePause,useTimeScale,autoPlay,callback);
        public void CheckLoopTimer(float delta,float unscaled)
        {if(LoopTimerRemoveIds.Count>=1){foreach(int id in LoopTimerRemoveIds)PRemoveLoopTimer(id);LoopTimerRemoveIds.Clear();}if(AddLoopTimerData.Count>=1){foreach(var timer in AddLoopTimerData)LoopTimers.Add(timer.Id,timer);AddLoopTimerData.Clear();}foreach(var timer in LoopTimers.Values)timer.AddTimes(delta,unscaled);foreach(var timer in LoopTimers.Values)timer.Check();}
        public void PlayLoopTimer(int id){if(!LoopTimers.TryGetValue(id,out var timer)){warning(new object[]{"PlayLoopTimer错误，未找到对应ID["+id+"]的数据类"});return;}timer.Pause(false);}
        public void PauseLoopTimer(int id,Action<int> callback){if(!LoopTimers.TryGetValue(id,out var timer)){warning(new object[]{"PauseLoopTimer，未找到对应ID["+id+"]的数据类"});return;}timer.Pause(true);callback?.Invoke(OutgameTimeClock.ToInt(timer.Elapsed));}
        public void ResetLoopTimer(int id,bool clearTotal){if(!LoopTimers.TryGetValue(id,out var timer)){warning(new object[]{"ResetLoopTimer，未找到对应ID["+id+"]的数据类"});return;}timer.ResetTimer(clearTotal);}
        public int GetLoopTimerValue(int id){if(!LoopTimers.TryGetValue(id,out var timer)){warning(new object[]{"GetLoopTimerValue，未找到对应ID["+id+"]的数据类"});return 0;}return OutgameTimeClock.ToInt(timer.TotalTime);}
        public void RemoveLoopTimer(int id,Action<int> callback){LoopTimerRemoveIds.Add(id);if(LoopTimers.ContainsKey(id)){var timer=LoopTimers[id];callback?.Invoke(OutgameTimeClock.ToInt(timer.Elapsed));}}
        public void PRemoveLoopTimer(int id){if(LoopTimers.ContainsKey(id)){LoopTimers[id].Destroy();LoopTimers.Remove(id);}else warning(new object[]{"RemoveLoopTimer，未找到对应ID["+id+"]的数据类"});}
        public int AddUnityTimer(float duration,bool useTimeScale,Action complete,Action<float> every,Action everySecond)
        {int id=GetTimeId();NewUnityTimers.Add(new OutgameUnityTimer(id,duration,useTimeScale,false,complete,every));if(everySecond!=null)NewEverySecond.Add(new OutgameUnityTimer(id,1,useTimeScale,true,everySecond,null));return id;}
        public int AddUnityEverySecondTimer(bool useTimeScale,Action callback)
        {int id=GetTimeId();NewEverySecond.Add(new OutgameUnityTimer(id,1,useTimeScale,true,callback,null));return id;}
        public void AddNewUnityTimer()
        {if(NewUnityTimers.Count>=1){for(int i=0;i<NewUnityTimers.Count;i++)UnityTimers.Add(NewUnityTimers[i].Id,NewUnityTimers[i]);NewUnityTimers.Clear();}if(NewEverySecond.Count>=1){for(int i=0;i<NewEverySecond.Count;i++)EverySecondTimers.Add(NewEverySecond[i].Id,NewEverySecond[i]);NewEverySecond.Clear();}if(NewEveryMinute.Count>=1){for(int i=0;i<NewEveryMinute.Count;i++)EveryMinuteTimers.Add(NewEveryMinute[i].Id,NewEveryMinute[i]);NewEveryMinute.Clear();}}
        public void RemoveUnityTimer(int id)=>CashCompleteUnityTimers.Add(id);
        public void PriRemoveUnityTimer(int id){if(UnityTimers.ContainsKey(id))UnityTimers.Remove(id);if(EverySecondTimers.ContainsKey(id))EverySecondTimers.Remove(id);if(EveryMinuteTimers.ContainsKey(id))EveryMinuteTimers.Remove(id);}
        public void CheckUnityTimer()=>CheckUnityTimer(timer=>timer.AddTimer());
        public void CheckUnityTimer(float delta,float unscaled)=>CheckUnityTimer(timer=>timer.AddTimer(delta,unscaled));
        void CheckUnityTimer(Action<OutgameUnityTimer> tick)
        {if(CashCompleteUnityTimers.Count>=1){for(int i=0;i<CashCompleteUnityTimers.Count;i++)if(!CompleteUnityTimers.Contains(CashCompleteUnityTimers[i]))CompleteUnityTimers.Add(CashCompleteUnityTimers[i]);CashCompleteUnityTimers.Clear();}foreach(int id in CompleteUnityTimers)PriRemoveUnityTimer(id);CompleteUnityTimers.Clear();AddNewUnityTimer();foreach(var timer in EverySecondTimers.Values)tick(timer);foreach(var timer in UnityTimers.Values){tick(timer);if(timer.IsComplete)CompleteUnityTimers.Add(timer.Id);}foreach(var timer in EveryMinuteTimers.Values)tick(timer);}
    }
}
