using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // The concrete source action classes implement this boundary; it does not synthesize successful playback.
    public interface IOutgameAudioAction
    {
        int Guid{get;}OutgameVoiceType VoiceType{get;}AudioSource AudioSource{get;}
        void Play();void Stop();void Pause(bool paused);
    }
    public delegate IOutgameAudioAction OutgameNewAudio(GameObject root,int actionType,int audioId,Action<int> started,Action<int> ended);
    public abstract class OutgameAudioNode:IOutgameAudioNode
    {
        public readonly int NodeId;public GameObject Root{get;private set;}
        protected readonly Func<int,int> getAudioType;protected readonly OutgameNewAudio newAudio;protected readonly Func<OutgameMessageDispatcher> messages;readonly Action<UnityEngine.Object> destroy;
        protected OutgameAudioNode(GameObject parent,int id,string name,Func<int,int> getAudioType,OutgameNewAudio newAudio,Func<OutgameMessageDispatcher> messages,Action<UnityEngine.Object> destroy=null)
        {
            this.getAudioType=getAudioType;this.newAudio=newAudio;this.messages=messages;this.destroy=destroy??UnityEngine.Object.Destroy;
            // Utils.NewGameObject28192/SetParent: reset local transform and inherit parent layer only with a native parent.
            Root=new GameObject();if(!string.IsNullOrEmpty(name))Root.name=name;
            if(parent){Root.transform.SetParent(parent.transform,false);Root.transform.localPosition=Vector3.zero;Root.transform.localRotation=Quaternion.identity;Root.transform.localScale=Vector3.one;Root.layer=parent.layer;}
            NodeId=id;
        }
        protected void DestroyRoot()=>destroy(Root);
        protected void Started(int guid)=>messages().SendMessage("AudioPlayStart",new object[]{guid});
        protected void Ended(int guid)=>messages().SendMessage("AudioPlayEnd",new object[]{guid});
        public virtual void StopByGuid(int guid){} // Source AudioCompositeBase26383 empty.
        public abstract int[] Play(int[] ids);public abstract void Stop(OutgameVoiceType voice);public abstract void Pause(bool paused);public abstract void Destroy();
    }
    public sealed class OutgameAudioParallel:OutgameAudioNode
    {
        public Dictionary<int,IOutgameAudioAction> Actions=new Dictionary<int,IOutgameAudioAction>();
        public OutgameAudioParallel(GameObject parent,int id,string name,Func<int,int> getType,OutgameNewAudio create,Func<OutgameMessageDispatcher> messages,Action<UnityEngine.Object> destroy=null):base(parent,id,name,getType,create,messages,destroy){}
        public override int[] Play(int[] ids){if(ids==null||ids.Length==0)return null;var handles=new int[ids.Length];for(int i=0;i<ids.Length;i++)handles[i]=PlayOneAudio(ids[i]);return handles;}
        public int PlayOneAudio(int id)
        {
            int type=getAudioType(id);IOutgameAudioAction action=null;
            action=newAudio(Root,type,id,guid=>{if(action!=null&&Actions!=null)Actions[guid]=action;Started(guid);},OnEnded);
            action.Play();return action.Guid;
        }
        void OnEnded(int guid){if(Actions!=null&&Actions.ContainsKey(guid))Actions.Remove(guid);Ended(guid);}
        public override void StopByGuid(int guid){if(Actions!=null&&Actions.ContainsKey(guid))Actions[guid].Stop();}
        public override void Stop(OutgameVoiceType voice)
        {
            if(Actions==null||Actions.Count<1)return;
            lock(Actions){var keys=new List<int>(Actions.Keys);for(int i=0;i<keys.Count;i++){
                IOutgameAudioAction action=null;if(Actions!=null&&Actions.ContainsKey(keys[i]))action=Actions[keys[i]];
                if(action==null||(voice!=0&&action.VoiceType!=voice))continue;
                action.Stop();Actions.Remove(i); // Source26390 @008cb3f7 passes loop index, not the captured key.
            }}
        }
        public override void Pause(bool paused){if(Actions==null||Actions.Count<1)return;foreach(var pair in Actions){var action=pair.Value;if(action!=null&&action.AudioSource)action.Pause(paused);}}
        public override void Destroy(){Actions=null;DestroyRoot();}
    }
    public sealed class OutgameAudioSequence:OutgameAudioNode
    {
        public Queue<IOutgameAudioAction> Actions=new Queue<IOutgameAudioAction>();public bool Stopping;
        public OutgameAudioSequence(GameObject parent,int id,string name,Func<int,int> getType,OutgameNewAudio create,Func<OutgameMessageDispatcher> messages,Action<UnityEngine.Object> destroy=null):base(parent,id,name,getType,create,messages,destroy){}
        IOutgameAudioAction Dequeue()=>Actions!=null&&Actions.Count>=1?Actions.Dequeue():null;
        IOutgameAudioAction Peek()=>Actions!=null&&Actions.Count>=1?Actions.Peek():null;
        public override int[] Play(int[] ids){if(ids!=null&&ids.Length>0)for(int i=0;i<ids.Length;i++)PlayOneAudio(ids[i]);return null;}
        public void PlayOneAudio(int id)
        {
            int type=getAudioType(id);var action=newAudio(Root,type==2?1:type,id,Started,OnEnded);
            if(Actions!=null&&Actions.Count<1)action.Play();
            if(action!=null)Actions.Enqueue(action); // Source26412 starts before enqueue, including synchronous completion.
        }
        void OnEnded(int guid){if(!Stopping)Dequeue();Ended(guid);if(!Stopping)Peek()?.Play();}
        public override void Stop(OutgameVoiceType voice)
        {
            if(Actions!=null){Stopping=true;IOutgameAudioAction action;while((action=Dequeue())!=null)if(action.AudioSource)action.Stop();}
            Destroy(); // Voice filter is ignored by source sequence stop.
        }
        public override void Pause(bool paused){if(Actions==null||Actions.Count<1)return;foreach(var action in Actions)if(action!=null&&action.AudioSource)action.Pause(paused);}
        public override void Destroy(){Actions=null;Stopping=false;DestroyRoot();}
    }
    public sealed class OutgameAudioSingle:OutgameAudioNode
    {
        public IOutgameAudioAction Current,Pending;
        public OutgameAudioSingle(GameObject parent,int id,string name,Func<int,int> getType,OutgameNewAudio create,Func<OutgameMessageDispatcher> messages,Action<UnityEngine.Object> destroy=null):base(parent,id,name,getType,create,messages,destroy){}
        public override int[] Play(int[] ids)
        {
            int id=ids!=null&&ids.Length>0?ids[0]:0;var action=newAudio(Root,getAudioType(id),id,Started,OnEnded);
            if(Current!=null){Pending=action;Current.Stop();}else{Current=action;Current.Play();}return null;
        }
        void OnEnded(int guid){Current=null;Ended(guid);if(Pending!=null){Current=Pending;Current.Play();Pending=null;}}
        public override void Stop(OutgameVoiceType voice){Current?.Stop();Destroy();}
        public override void Pause(bool paused){if(Current!=null&&Current.AudioSource)Current.Pause(paused);}
        public override void Destroy(){Current=null;DestroyRoot();} // Pending is retained by source26419.
    }
}
