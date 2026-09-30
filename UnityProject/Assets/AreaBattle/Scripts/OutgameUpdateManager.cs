using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameUpdateData
    {
        public string ActionName,Handle;
        public OutgameUpdateData(string actionName,string handle){ActionName=actionName;Handle=handle;}
    }
    // Source2920: public API ->22788/22768/22782 and Unity messages22802/22785.
    // Obfuscated alternate entry points are not silently substituted for these traced bodies.
    public sealed class OutgameUpdateManager:MonoBehaviour
    {
        static OutgameUpdateManager instance;
        Dictionary<int,Action> indexDict;List<Action> handles,forceHandles,fixedHandles;
        public bool IsPause;public int NextIndex;public int TimePreFrame{get;set;}=16;
        public List<int> RemoveIds=new List<int>();public List<Action> RemoveHandles=new List<Action>();
        public Dictionary<int,OutgameUpdateData> UpdateData=new Dictionary<int,OutgameUpdateData>();
        public Dictionary<int,Action> IndexDict{get=>indexDict??(indexDict=new Dictionary<int,Action>());set=>indexDict=value;}
        public List<Action> HandleList{get=>handles??(handles=new List<Action>());set=>handles=value;}
        public List<Action> ForceHandleList{get=>forceHandles??(forceHandles=new List<Action>());set=>forceHandles=value;}
        public List<Action> FixedUpdateHandle{get=>fixedHandles??(fixedHandles=new List<Action>());set=>fixedHandles=value;}
        public static OutgameUpdateManager Instance
        {
            get{if(instance==null){var root=GameObject.Find("UpdateManager");if(root==null){root=new GameObject();root.name="UpdateManager";}instance=root.GetComponent<OutgameUpdateManager>();if(instance==null)instance=root.AddComponent<OutgameUpdateManager>();DontDestroyOnLoad(root);}return instance;}
        }
        public static int AddHandle(Action callback)=>Instance.Register(callback);
        public static int AddFixedHandle(Action callback)=>Instance.RegisterFixed(callback);
        public static int AddForceHandle(Action callback)=>Instance.RegisterForce(callback);
        public static void RemoveHandle(Action callback)=>Instance.QueueRemove(callback);
        public static void RemoveHandleById(int id)=>Instance.QueueRemove(id);
        public static void AddUpdateData(int id,string actionName,string handle)=>Instance.RegisterData(id,actionName,handle,args=>Debug.LogError(args[0]));
        public static void BindAudio(OutgameAudioActionServices services){services.AddUpdate=AddHandle;services.RemoveUpdate=RemoveHandleById;}
        public int GetNextId()
        {int id=NextIndex;do{id=unchecked(id+1);}while(IndexDict.ContainsKey(id));NextIndex=id>2147483645?0:id;return id;}
        public int Register(Action callback){int id=GetNextId();HandleList.Add(callback);IndexDict.Add(id,callback);return id;}
        public int RegisterFixed(Action callback){int id=GetNextId();FixedUpdateHandle.Add(callback);IndexDict.Add(id,callback);return id;}
        public int RegisterForce(Action callback){int id=GetNextId();ForceHandleList.Add(callback);IndexDict.Add(id,callback);return id;}
        public void RegisterData(int id,string actionName,string handle,Action<object[]> error)
        {var data=new OutgameUpdateData(actionName,handle);if(UpdateData.ContainsKey(id)){error(new object[]{"重复添加："+id});return;}UpdateData.Add(id,data);}
        public void QueueRemove(Action callback){if(RemoveHandles.IndexOf(callback)<=-1)RemoveHandles.Add(callback);}
        public void QueueRemove(int id){if(RemoveIds.IndexOf(id)<=-1)RemoveIds.Add(id);}
        // Source22806 removes first normal OR force occurrence, plus first fixed occurrence.
        // Index lookup follows delegate equality, not the id requested by the caller.
        public void RemoveNow(Action callback)
        {
            int index=HandleList.IndexOf(callback);if(index>=0)HandleList.RemoveAt(index);else{index=ForceHandleList.IndexOf(callback);if(index>=0)ForceHandleList.RemoveAt(index);}
            index=FixedUpdateHandle.IndexOf(callback);if(index>=0)FixedUpdateHandle.RemoveAt(index);
            if(IndexDict.ContainsValue(callback)){int id=-1;foreach(var pair in IndexDict){if(pair.Value!=callback)continue;id=pair.Key;break;}if(id>=0)IndexDict.Remove(id);}
        }
        public void RemoveNow(int id){if(!IndexDict.ContainsKey(id))return;RemoveNow(IndexDict[id]);if(UpdateData.ContainsKey(id))UpdateData.Remove(id);}
        public void ProcessRemovals()
        {if(RemoveIds.Count>=1){for(int i=0;i<RemoveIds.Count;i++)RemoveNow(RemoveIds[i]);RemoveIds=new List<int>();}if(RemoveHandles.Count>0){for(int i=0;i<RemoveHandles.Count;i++)RemoveNow(RemoveHandles[i]);RemoveHandles=new List<Action>();}}
        public void Update()
        {if(IsPause)return;ProcessRemovals();int count=ForceHandleList.Count;for(int i=0;i<count;i++)ForceHandleList[i]();count=HandleList.Count;for(int i=0;i<count;i++)HandleList[i]();}
        public void FixedUpdate()
        {if(IsPause)return;int count=FixedUpdateHandle.Count;for(int i=0;i<count;i++){var callback=FixedUpdateHandle[i];if(callback!=null)callback();}}
    }
}
