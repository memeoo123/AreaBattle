using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // ConfigRead26612 Unity JSON path and26625 OnceAudio table. Load is the real config resource boundary.
    public sealed class OutgameAudioConfigReader
    {
        [Serializable] sealed class Rows{public List<OutgameAudioData> Datas;}
        [Serializable] sealed class OnceRows{public List<OnceRow> AudioInfos;}
        [Serializable] sealed class OnceRow{public string Name,Length;}
        readonly Func<string,TextAsset> load;readonly Func<bool> hasConfig,newResources;readonly Action<object[]> error;
        public OutgameAudioConfigReader(Func<string,TextAsset> load,Func<bool> hasConfig,Func<bool> newResources,Action<object[]> error)
        {this.load=load;this.hasConfig=hasConfig;this.newResources=newResources;this.error=error;}
        public List<OutgameAudioData> ReadAudio()
        {var asset=load("AudioConfig");if(asset){string text=asset.text;return JsonUtility.FromJson<Rows>(text.Substring(text.IndexOf("{",StringComparison.Ordinal))).Datas;}error(new object[]{"配置文件不存在AudioConfig"});return null;}
        public void ReadOnce(Dictionary<string,float> destination)
        {
            if(!hasConfig()&&!newResources())error(new object[]{"ResourcesInfo为NULL"});
            var asset=load("OnceAudioConfig");if(asset){foreach(var row in JsonUtility.FromJson<OnceRows>(asset.text).AudioInfos)destination.Add(row.Name,float.Parse(row.Length));}
            else error(new object[]{"OnceAudioConfig不存在,请点击Assets/生成AudioOnce配置并提交"});
        }
    }
    public sealed class OutgameAudioManagerServices
    {
        public Func<bool> HasConfigResource,UseNewResources;public Func<List<OutgameAudioData>> ReadAudioConfig;
        public Action<Dictionary<string,float>> ReadOnceConfig;public Func<Camera> UiCamera;
        public OutgameAudioActionServices Actions;
        public Func<int,int,int> RandomRange=UnityEngine.Random.Range;
        public Action<UnityEngine.Object> Destroy=UnityEngine.Object.Destroy,Persist=UnityEngine.Object.DontDestroyOnLoad;
        public Action<object[]> Log=args=>Debug.Log(args[0]),Error=args=>Debug.LogError(args[0]),Warning=args=>Debug.LogWarning(args[0]);
    }
    // Source3421 manager ownership, separate from its audio action resource-loader services.
    public sealed class OutgameAudioManager
    {
        readonly OutgameAudioManagerServices services;readonly OutgameAudioActionFactory factory;
        public readonly OutgameUiAudioManagerSlot UiSlot;
        public AudioListener Listener;public GameObject Root;
        public Dictionary<int,OutgameAudioNode> Nodes=new Dictionary<int,OutgameAudioNode>();
        public Dictionary<int,OutgameAudioData> AudioData;public Dictionary<string,float> OnceDurations;
        public Dictionary<int,string> AndroidIds=new Dictionary<int,string>();
        public bool UseOnceAudio,Initialized;public string Field36="";
        public OutgameAudioManager(OutgameAudioManagerServices services)
        {this.services=services;services.Actions.GetData=GetAudioData;factory=new OutgameAudioActionFactory(services.Actions);UiSlot=new OutgameUiAudioManagerSlot((mode,parent)=>CreateNode(mode,parent?parent.gameObject:null),services.Log);}
        public OutgameAudioData GetAudioData(int id){OutgameAudioData data=null;AudioData?.TryGetValue(id,out data);return data;}
        public void InitAudioManager()
        {
            if(Initialized)return;
            if(!services.HasConfigResource()&&!services.UseNewResources()){services.Error(new object[]{"Audio:配置文件AB资源为null"});return;}
            services.Log(new object[]{"Audio:设置音频数据"});AudioData=new Dictionary<int,OutgameAudioData>();var rows=services.ReadAudioConfig();
            if(rows!=null)for(int i=0;i<rows.Count;i++){
                if(AudioData.ContainsKey(rows[i].id))services.Error(new object[]{string.Format("表[音效表]中有相同键({0})",rows[i].id)});else AudioData.Add(rows[i].id,rows[i]);
                if(rows[i].ResPath.ToLower().Contains("audio/once")&&!UseOnceAudio)UseOnceAudio=true;
            }
            OnceDurations=new Dictionary<string,float>();
            if(UseOnceAudio){services.Log(new object[]{"检测到音频表中使用了OnceAudio音频开启OnceAudio配置加载"});services.ReadOnceConfig(OnceDurations);}
            Initialized=true;EnsureRootAndListener();
        }
        public void EnsureRoot(){if(Root==null){Root=new GameObject();Root.name="AudioRoot";services.Persist(Root);}}
        public void EnsureListener(){if(Listener==null){var go=new GameObject();go.name="AudioListener";Listener=go.AddComponent<AudioListener>();}}
        public void EnsureRootAndListener(){EnsureRoot();MoveAudioListen(1);}
        public void MoveAudioListen(int type){var camera=services.UiCamera();EnsureListener();if(type==1&&camera!=null)Listener.transform.SetParent(camera.transform);}
        public int RandomId(int min,int max,List<int> excluded)
        {int id=services.RandomRange(min,max);if(excluded!=null)for(int i=0;i<excluded.Count;i++)if(excluded[i]==id)RandomId(min,max,excluded);return id;} // Source Utils6831 discards recursive result.
        public OutgameAudioNode CreateNode(int type,GameObject parent=null)
        {
            if(Root==null)EnsureRootAndListener();List<int> excluded=null;if(Nodes!=null&&Nodes.Count>=1)excluded=new List<int>(Nodes.Keys);
            int id=RandomId(1,10000,excluded);if(parent==null)parent=Root;OutgameAudioNode node=null;
            if(type==1)node=new OutgameAudioParallel(parent,id,"parallel_"+id,factory.GetAudioType,factory.Create,services.Actions.Messages,services.Destroy);
            else if(type==2)node=new OutgameAudioSequence(parent,id,"sequence_"+id,factory.GetAudioType,factory.Create,services.Actions.Messages,services.Destroy);
            else if(type==3)node=new OutgameAudioSingle(parent,id,"single_"+id,factory.GetAudioType,factory.Create,services.Actions.Messages,services.Destroy);
            if(node!=null&&Nodes!=null)Nodes[id]=node;return node;
        }
        public void ClearNode(){if(Nodes!=null){foreach(var pair in Nodes)pair.Value.Stop(0);Nodes.Clear();}AudioData?.Clear();Initialized=false;}
        public void DestroyRoot(){if(Root!=null)services.Destroy(Root);Root=null;}
        public void DestroyListener(){if(Listener!=null){services.Destroy(Listener.gameObject);Listener=null;}}
        public void Destroy(){ClearNode();DestroyRoot();DestroyListener();UiSlot.Instance.Destroy();} // Source leaves AudioManager singleton and Once/Android caches alive.
        public string GetAndroidAudioID(int id)=>AndroidIds.ContainsKey(id)?AndroidIds[id]:"";
        public float GetOnceAudioDuration(string name)
        {if(!UseOnceAudio){services.Error(new object[]{"isUseOnceAudio is false，should not be here OnceAudioName="+name});return 0;}if(OnceDurations.ContainsKey(name))return OnceDurations[name];services.Warning(new object[]{"OnceAudioName not exist name=="+name});return 0;}
        public void SetMusicGameVolume(OutgameVoiceType voice,float value)=>services.Actions.Settings().SetMusicGameVolume(voice,value);
    }
    public sealed class OutgameAudioManagerSlot
    {
        readonly Func<OutgameAudioManagerServices> services;OutgameAudioManager instance;
        public OutgameAudioManagerSlot(Func<OutgameAudioManagerServices> services){this.services=services;}
        public OutgameAudioManager Instance=>instance??(instance=new OutgameAudioManager(services()));
    }
}
