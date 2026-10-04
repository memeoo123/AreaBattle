using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameFlyCleanupHost
    {
        float Time {get;}
        int SubIdCount(int id);
        void KillTween(string id,bool complete);
        void StopCoroutine(object coroutine);
        void RemoveSubIds(int id);
    }
    // EffectControl.StopEffect31221, Updata31223, removal31219 and tween kill31209.
    public sealed class OutgameFlyToolCleanup
    {
        readonly IOutgameFlyCleanupHost host;
        Dictionary<int,object> coroutines;
        Dictionary<int,float> started;
        readonly List<int> pending=new List<int>();
        public float NextCheck {get;private set;}
        public int ActiveCount=>coroutines.Count;
        public int PendingCount=>pending.Count;
        public OutgameFlyToolCleanup(IOutgameFlyCleanupHost host,bool initialize=true){this.host=host;if(initialize)Initialize();}
        // EffectControl31213 recreates tracking dictionaries; pending ids and NextCheck survive.
        public void Initialize(){coroutines=new Dictionary<int,object>();started=new Dictionary<int,float>();}
        public void TrackCoroutine(int id,object coroutine)=>coroutines.Add(id,coroutine);
        public void TrackTime(int id,float time)=>started.Add(id,time);
        public void StopEffect(int id){if(coroutines.ContainsKey(id))pending.Add(id);}
        public void Update()
        {
            if(pending.Count>=1)
            {
                foreach(int id in pending)
                {
                    int count=host.SubIdCount(id);
                    for(int sub=0;sub<count;sub++)host.KillTween(string.Format("fly_{0}_{1}",id,sub),true);
                    host.StopCoroutine(coroutines[id]);coroutines.Remove(id);started.Remove(id);host.RemoveSubIds(id);
                }
                pending.Clear();
            }
            if(started.Count>0&&host.Time>NextCheck)
            {
                foreach(int id in started.Keys){float began=started[id];if(host.Time-began>4f)StopEffect(id);}
                NextCheck=NextCheck+(host.Time+1f);
            }
        }
    }
}
