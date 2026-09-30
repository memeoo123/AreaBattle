using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace AreaBattle
{
    // Original Spine 4.1.16 skeletons, original transforms and native components.
    // Host advances this explicit clock; Unity component Update is disabled.
    public sealed class RecoveredBossVisual : MonoBehaviour
    {
        public const float SoldierScale=.01600000076f;
        public SkeletonAnimation MainSkeleton { get; private set; }
        public IReadOnlyList<SkeletonAnimation> Skeletons => skeletons;
        SkeletonAnimation[] skeletons;
        RecoveredEffectVisual nativeEffects;
        ParticleSystem[] originalParticles;
        bool initialized,dying;
        float deadElapsed,deadDuration;
        Vector3 deathStart,deathDrift;
        int action,burstWaits;
        float actionWait,burstInterval;
        bool waitingFirst,rainSeen;
        RecoveredEffectVisual shadowClock;

        public static string ResourceForEntity(int entity)
        {
            switch(entity){case 801:return "boss01";case 802:return "boss02";case 9001:return "bossSoldier1";case 9002:return "bossSoldier2";default:throw new ArgumentOutOfRangeException(nameof(entity));}
        }
        public static RecoveredBossVisual Create(int entity,Transform parent=null)
        {
            string name=ResourceForEntity(entity);
            var prefab=Resources.Load<GameObject>("Recovered/Bosses/"+name);
            if(prefab==null)throw new InvalidOperationException("Original Boss model is not imported: "+name);
            var obj=Instantiate(prefab,parent,false);
            var visual=obj.AddComponent<RecoveredBossVisual>();visual.Initialize();return visual;
        }
        public void Initialize()
        {
            if(initialized)return;initialized=true;
            skeletons=GetComponentsInChildren<SkeletonAnimation>(true);
            foreach(var s in skeletons){s.Initialize(false);s.enabled=false;s.Update(0f);s.LateUpdate();if(s.transform.name=="mesh")MainSkeleton=s;}
            if(MainSkeleton==null)throw new InvalidOperationException("Original main Spine skeleton missing");
            originalParticles=GetComponentsInChildren<ParticleSystem>(true);nativeEffects=RecoveredEffectVisual.Attach(gameObject);
        }
        public void ConfigureBossShadow(Color originalCampColor)
        {
            var shadow=transform.Find("bossshadow");
            if(shadow!=null){var sprite=shadow.GetComponent<SpriteRenderer>();sprite.color=originalCampColor;sprite.sortingOrder=-6;}
        }
        public void AttachSoldierShadow()
        {
            var parent=transform.Find("shadowRoot");if(parent==null||parent.Find("shadow")!=null)return;
            var prefab=Resources.Load<GameObject>("Recovered/BossEmbedded/QBDyShadow");
            if(prefab==null)throw new InvalidOperationException("Original Boss soldier shadow9034 is not imported");
            var shadow=Instantiate(prefab,parent,false);shadow.name="shadow";shadow.transform.localPosition=Vector3.zero;shadow.transform.localScale=Vector3.one;
            shadowClock=RecoveredEffectVisual.Attach(shadow);
        }
        public void BeginBossAction(int sourceAction,BossParameters config)
        {
            // BossUseSkill always cancels its previous coroutine, including no-op actions.
            action=sourceAction;actionWait=0;waitingFirst=false;burstWaits=0;
            if(action==1||action==2||action==5||action==6)Play("attack",false);
            if(action==1||action==6)actionWait=1f;
            if(action==2){actionWait=1f;waitingFirst=true;burstWaits=Mathf.CeilToInt(config.skill2_first);burstInterval=config.skill2_first>0?config.skill2_duration/config.skill2_first:0;}
        }
        public void ResetBossForRetry()
        {
            Initialize();action=burstWaits=0;actionWait=burstInterval=0;waitingFirst=rainSeen=false;
            var rain=transform.Find("hdzd_effect_boss05_01_1");if(rain!=null)rain.gameObject.SetActive(false);
            foreach(var p in originalParticles)p.Stop(false,ParticleSystemStopBehavior.StopEmittingAndClear);
            nativeEffects.Step(0);Play("idle",true);
        }
        public void StepBossCoroutines(float unityScaledDelta)
        {
            if(action!=1&&action!=2&&action!=6)return;
            actionWait-=Mathf.Max(0,unityScaledDelta);if(actionWait>0)return;
            if(action==2&&burstWaits>0){if(!waitingFirst)burstWaits--;waitingFirst=false;if(burstWaits>0){actionWait=burstInterval;return;}}
            action=0;Play("idle",true);
        }
        public void SynchronizeRain(bool raining,Vector3 sourceSelectedPoint)
        {
            var rain=transform.Find("hdzd_effect_boss05_01_1");if(rain==null)return;
            if(raining){rain.position=sourceSelectedPoint;float z=rain.localPosition.z;var drops=rain.Find("Rain");var cloud=rain.Find("cloud");var p=drops.localPosition;p.y=1.2f-.696f*z;drops.localPosition=p;p=cloud.localPosition;p.y=.7f-.696f*z;cloud.localPosition=p;rainSeen=true;}
            else if(rainSeen){rainSeen=false;Play("idle",true);}
            rain.gameObject.SetActive(raining);
        }
        public void Play(string animation,bool loop)
        {
            Initialize();if(MainSkeleton.Skeleton.Data.FindAnimation(animation)==null)throw new ArgumentException("Animation absent from original skeleton: "+animation);
            MainSkeleton.AnimationState.SetAnimation(0,animation,loop);MainSkeleton.Update(0);MainSkeleton.LateUpdate();
        }
        public void Step(float unityScaledDelta)
        {
            Initialize();float dt=Mathf.Max(0,unityScaledDelta);
            foreach(var s in skeletons)if(s.gameObject.activeInHierarchy){s.Update(dt);s.LateUpdate();}
            nativeEffects.Step(dt);
            if(shadowClock!=null)shadowClock.Step(dt);
            if(dying){deadElapsed+=dt;float t=Mathf.Clamp01(deadElapsed/Mathf.Max(.001f,deadDuration));transform.position=deathStart+deathDrift*(t*(2-t));}
        }
        public void SynchronizeSoldier(SoldierState soldier,Camera camera)
        {
            Initialize();if(!dying)transform.position=soldier.Position;
            transform.localScale=Vector3.one*SoldierScale;
            // Original InitPosInfo reverses only shipType<=3; types11/12 retain camera tilt.
            transform.localEulerAngles=new Vector3(camera.transform.localEulerAngles.x,0,0);
            var renderer=MainSkeleton.GetComponent<MeshRenderer>();renderer.sortingLayerName="Default";renderer.sortingOrder=-5;
        }
        public void BeginSoldierDeath(Vector3 sourceRandomDrift)
        {
            if(dying)return;Initialize();dying=true;deadElapsed=0;deathStart=transform.position;deathDrift=sourceRandomDrift;
            // Soldier.W... chooses independent X/Z in [-.1,.1], drift tween duration dead-.05.
            deadDuration=Mathf.Max(0,MainSkeleton.Skeleton.Data.FindAnimation("dead").Duration-.05f);Play("dead",false);
        }
        public bool SoldierDeathFinished=>dying&&deadElapsed>=deadDuration;
        public bool IsDying=>dying;
    }
}
