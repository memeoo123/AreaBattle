using System;
using UnityEngine;
using UnityEngine.Rendering;
namespace AreaBattle
{
    // Recovered SpineAnimator (Type4523), backed by the original packed mesh animation shader.
    [RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public sealed class OutgameBakedAnimator:MonoBehaviour,IOutgameModelAnimator
    {
        [Serializable] public sealed class Clip
        {
            public string name;public float startFrame,lengthFrames,lengthSeconds;
            public bool isNeedFix;public Mesh mesh;
        }
        public Clip[] Clips;public string AutoPlayAnimation;
        public Action OnComplete {get;set;}
        public Clip Current {get;private set;}
        public bool IsAnimationPaused {get;private set;}
        public bool Looping {get;private set;}
        public Func<float> Clock;
        MeshRenderer target;MeshFilter filter;Mesh originalMesh;MaterialPropertyBlock properties;
        float started,pausedPhase;string pending;
        float Now=>Clock!=null?Clock():GraphicsSettings.defaultRenderPipeline!=null?Time.time:Time.timeSinceLevelLoad;
        void Awake(){Initialize();}
        void Initialize()
        {
            if(properties!=null)return;
            target=GetComponent<MeshRenderer>();filter=GetComponent<MeshFilter>();originalMesh=filter.sharedMesh;properties=new MaterialPropertyBlock();
        }
        void Start(){if(Current==null&&!string.IsNullOrEmpty(AutoPlayAnimation))Play(AutoPlayAnimation,true);}
        public void Play(string name,bool loop)
        {
            if(IsAnimationPaused){pending=name;return;}
            Initialize();Looping=loop;started=Now;
            Current=Array.Find(Clips,clip=>clip.name==name)??Clips[0];Apply();
        }
        public void Reset()
        {
            pausedPhase=0;pending=null;started=0;Current=null;IsAnimationPaused=false;Looping=false;
        }
        public void Pause()
        {
            pending=Current.name;IsAnimationPaused=true;pausedPhase=(Now-started)%Current.lengthSeconds;
        }
        public void Resume()
        {
            pausedPhase=0;IsAnimationPaused=false;
            if(!string.IsNullOrEmpty(pending)){Play(pending,true);pending=null;}
        }
        void Apply()
        {
            if(Current==null)return;
            filter.sharedMesh=Current.isNeedFix?Current.mesh:originalMesh;
            target.GetPropertyBlock(properties);
            properties.SetFloat("_AnimLoop",Looping?1:0);
            properties.SetVector("_AnimTime",new Vector4(Current.startFrame,Current.lengthFrames,1/Mathf.Max(Current.lengthSeconds,.01f),started));
            properties.SetFloat("_BattleVisualTime",Now);target.SetPropertyBlock(properties);
        }
        public void Tick()
        {
            Initialize();
            if(IsAnimationPaused){started=Now-pausedPhase;Apply();}
            // Recovered shader uses an explicit clock in place of the original shader's _Time.y.
            target.GetPropertyBlock(properties);properties.SetFloat("_BattleVisualTime",Now);target.SetPropertyBlock(properties);
            if(Current==null||Looping||!(Now>(float)(started+Current.lengthSeconds)))return;
            Reset();OnComplete?.Invoke();OnComplete=null;
            if(Current==null&&!string.IsNullOrEmpty(AutoPlayAnimation))Play(AutoPlayAnimation,true);
        }
        void Update(){Tick();}
    }
}
