using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // View-only consumption of source-backed events. Never changes scores, targets or clocks.
    public sealed class BattleSkillPresentation : MonoBehaviour
    {
        [Serializable] sealed class Effects {public Effect[] Datas;}
        [Serializable] sealed class Effect {public int id;public string res;public float duration;}
        sealed class Instance
        {
            public GameObject Object;public RecoveredEffectVisual Clock;public SkillVisualEvent Source;
            public RecoveredSoldierVisual Recruit;public SoldierState RecruitPose;
            public RecoveredBossProjectilePool BossPool;
            public float Age;public bool Managed;public int CreatedStep;
        }
        BattleView view;BattleSimulation simulation;
        readonly Dictionary<int,string> effectNames=new Dictionary<int,string>();
        readonly Dictionary<int,Instance> instances=new Dictionary<int,Instance>();
        readonly Dictionary<long,Instance> attachments=new Dictionary<long,Instance>();
        readonly Dictionary<long,RecoveredEffectVisual> embedded=new Dictionary<long,RecoveredEffectVisual>();
        readonly Dictionary<int,float> towerVisualPoll=new Dictionary<int,float>();
        readonly List<SkillVisualEvent> pending=new List<SkillVisualEvent>();
        readonly HashSet<int> audioHandles=new HashSet<int>();
        public int MissingResourceCount {get;private set;}
        public int ActiveVisualCount=>instances.Count+attachments.Count;
        public GameObject VisualObject(int id)=>instances.TryGetValue(id,out var item)?item.Object:null;
        int nextGuideVisual=-100000;
        int stepSerial;
        public void OnGuideConfirmed(int stage)
        {
            if(stage!=1&&stage!=4&&stage!=5&&stage!=7&&stage!=8&&stage!=9&&stage!=10&&stage!=11)return;
            ShowPlayerCampEffect();
        }
        public void ShowPlayerCampEffect()
        {
            // GuideUI.OnOK -> LevelControl.ShowMyCampEffect -> EffectModule.Show107.
            foreach(var tower in simulation.Towers)
            {
                var parent=view.TowerPresentationTransform(tower.Id);
                if(tower.Camp!=BattleSimulation.PlayerCampID||parent==null)continue;
                var e=new SkillVisualEvent{Kind="effect-show",EffectId=107,VisualId=nextGuideVisual--,Parent="world",Position=parent.position,Duration=3};
                var item=Spawn(e);if(item==null)continue;
                foreach(var renderer in item.Object.GetComponentsInChildren<Renderer>(true)){renderer.sortingLayerID=0;renderer.sortingOrder=-20;}
                instances[e.VisualId]=item;
            }
        }
        const int PoisonPreviewKey=int.MinValue;
        public void PreviewPoison(Vector3 point,bool visible)
        {
            if(!instances.TryGetValue(PoisonPreviewKey,out var item))
            {
                if(!visible)return;
                item=Spawn(new SkillVisualEvent{Kind="poison-drag-preview",SkillId=18,Camp=1,EffectId=633,Parent="world",Position=point,OverrideScale=true,LocalScale=Vector3.one*.25f});
                if(item==null)return;instances[PoisonPreviewKey]=item;
            }
            item.Object.SetActive(visible);if(visible)item.Object.transform.position=point;
        }
        public void Initialize(BattleView source)
        {
            view=source;simulation=view.Simulation;
            var data=Resources.Load<TextAsset>("Data/EffectConfig");
            if(data!=null)foreach(var e in JsonUtility.FromJson<Effects>(data.text).Datas)effectNames[e.id]=e.res;
            simulation.SkillVisual+=Observe;
        }
        void Observe(SkillVisualEvent value){pending.Add(value);}
        static long AttachmentKey(int soldier,int effect)=>((long)soldier<<32)|(uint)effect;
        RecoveredEffectVisual EmbeddedTowerEffect(int towerId,int kind)
        {
            long key=AttachmentKey(towerId,kind);
            if(embedded.TryGetValue(key,out var existing)&&existing!=null)return existing;
            var tower=simulation.Tower(towerId);var parent=view.TowerPresentationTransform(towerId);
            if(tower==null||parent==null)return null;
            string name=kind==1?"Hdzd_Effect_icicle":kind==4?"Hdzd_Effect_Wy_speedUp":"Hdzd_Effect_Wy_speedDown";
            if(tower.IsBoss)
            {
                if(kind==1)name="skill_TM"; // Boss.Init field92/96 differs from ordinary Tower.Init.
                // Boss.Init loads its own entity821 hierarchy, with different source TRS.
                var bossPrefab=Resources.Load<GameObject>("Recovered/BossEmbedded/"+name);
                if(bossPrefab==null){MissingResourceCount++;throw new InvalidOperationException("Missing original Boss embedded effect "+name);}
                var bossEffect=Instantiate(bossPrefab,parent,false);bossEffect.name=name;bossEffect.SetActive(false);
                var original=RecoveredEffectVisual.Attach(bossEffect);embedded[key]=original;return original;
            }
            var prefab=Resources.Load<GameObject>("Recovered/SkillEffects/"+name);
            if(prefab==null){MissingResourceCount++;throw new InvalidOperationException("Missing recovered tower effect "+name);}
            var obj=Instantiate(prefab,parent,false);obj.name=name;obj.SetActive(false);
            var effect=RecoveredEffectVisual.Attach(obj);embedded[key]=effect;return effect;
        }
        void SetTowerSpeedEffect(TowerState tower,int kind,bool show)
        {
            long key=AttachmentKey(tower.Id,kind);
            if(!show&&!embedded.ContainsKey(key))return;
            var effect=EmbeddedTowerEffect(tower.Id,kind);if(effect!=null)effect.gameObject.SetActive(show);
        }
        static int VisualKey(SkillVisualEvent e)=>e.Kind.StartsWith("poison-ground-",StringComparison.Ordinal)?-1000-e.Camp:e.VisualId;
        Instance Spawn(SkillVisualEvent e,bool recruit=false)
        {
            if(e.SkillId==102&&e.Kind=="projectile-spawn")
            {
                var owner=view.TowerPresentationTransform(e.SourceTowerId);
                if(owner==null)return null;
                var pool=RecoveredBossProjectilePool.For(owner);var fire=pool.Acquire(e.Start);
                return new Instance{Object=fire,Clock=fire.GetComponent<RecoveredEffectVisual>(),Source=e,BossPool=pool};
            }
            string name=e.Path;
            if(string.IsNullOrEmpty(name)&&effectNames.TryGetValue(e.EffectId,out var mapped))name=mapped;
            if(!string.IsNullOrEmpty(name)){name=name.Replace('\\','/');name=name.Substring(name.LastIndexOf('/')+1);if(name.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase))name=name.Substring(0,name.Length-7);}
            string resource=recruit?"Recovered/Soldiers/soldier_100":"Recovered/SkillEffects/"+name;
            var prefab=Resources.Load<GameObject>(resource);
            if(prefab==null){MissingResourceCount++;Debug.LogWarning("Unresolved original skill visual: "+resource);return null;}
            Transform parent=transform;
            if(e.Parent=="tower")parent=view.TowerPresentationTransform(e.TowerId);
            if(e.Parent=="soldier")parent=view.SoldierPresentationTransform(e.SoldierId);
            // GetSkillParent returns the actual UI SkillItem transform, including Canvas
            // world scale. Original bottle150 and arrow-rain4 are local scales under it.
            if(e.Parent=="skill")parent=view.Hud!=null?view.Hud.SkillItemTransform((e.SkillId-1)%3):null;
            if(parent==null)return null; // A unit already cleared in this same frame has no remaining visual lifetime.
            var obj=Instantiate(prefab,parent,false);obj.name="Skill "+e.SkillId+" / "+prefab.name;
            if(e.PositionIsLocal)obj.transform.localPosition=e.Position;else obj.transform.position=e.Position;
            if(e.OverrideScale)obj.transform.localScale=e.LocalScale;
            if(e.OverrideEuler)obj.transform.localEulerAngles=e.Euler;
            if(e.LookAt&&(e.End-e.Start).sqrMagnitude>0)obj.transform.LookAt(e.End);
            var item=new Instance{Object=obj,Source=e,Clock=RecoveredEffectVisual.Attach(obj),Managed=e.Kind=="effect-show",CreatedStep=stepSerial};
            if(recruit)
            {
                item.Recruit=obj.GetComponent<RecoveredSoldierVisual>();item.RecruitPose=new SoldierState{Camp=e.Camp,ShipType=1,Active=true};
                item.Recruit.Begin(view.VisualClock);
            }
            return item;
        }
        public void Flush()
        {
            foreach(var e in pending)
            {
                int visualKey=VisualKey(e);
                switch(e.Kind)
                {
                    case "audio-play":case "audio-stop":
                        if(e.AudioId==2041&&e.Kind=="audio-play")audioHandles.Add(e.HandleId);
                        if(e.Kind=="audio-stop")audioHandles.Remove(e.HandleId);
                        if(view.Audio!=null)view.Audio.OnSkillAudio(e);break;
                    case "tower-ice-begin":case "tower-ice-melt":case "tower-ice-stop":
                        var ice=EmbeddedTowerEffect(e.TowerId,1);
                        if(ice!=null)
                        {
                            if(e.Kind=="tower-ice-stop")ice.gameObject.SetActive(false);
                            else
                            {
                                if(e.Kind=="tower-ice-begin")ice.gameObject.SetActive(true);
                                ice.GetComponentInChildren<Animator>(true).SetInteger("skill",e.Kind=="tower-ice-melt"?1:0);
                            }
                        }
                        break;
                    case "effect-show":case "projectile-spawn":case "recruit-spawn":case "poison-ground-prepare":
                        Remove(visualKey);
                        var item=Spawn(e,e.Kind=="recruit-spawn");
                        if(item!=null){instances[visualKey]=item;if(e.Kind=="poison-ground-prepare")item.Object.SetActive(false);}
                        break;
                    case "effect-close":case "projectile-hide":case "recruit-hide":case "poison-ground-hide":Remove(visualKey);break;
                    case "poison-ground-show":
                        if(instances.TryGetValue(visualKey,out var ground))
                        {
                            if(ground.Object.transform.childCount>0)
                                foreach(var ps in ground.Object.transform.GetChild(0).GetComponentsInChildren<ParticleSystem>(true))
                                {var main=ps.main;main.simulationSpeed=ground.Source.ParticleSpeed;}
                            ground.Object.SetActive(true);
                        }
                        break;
                    case "soldier-effect-add":
                        long key=AttachmentKey(e.SoldierId,e.EffectId);
                        if(!attachments.ContainsKey(key)){var attached=Spawn(e);if(attached!=null)attachments[key]=attached;}
                        break;
                    case "soldier-effect-remove":
                        key=AttachmentKey(e.SoldierId,e.EffectId);if(attachments.TryGetValue(key,out var old)){Dispose(old);attachments.Remove(key);}break;
                    case "battle-reset":ResetBattleVisuals();break;
                }
            }
            pending.Clear();
        }
        public void Step(float dt)
        {
            stepSerial++;Flush();var remove=new List<int>();
            foreach(var pair in instances)
            {
                var item=pair.Value;if(item.Object==null){remove.Add(pair.Key);continue;}
                // BaseEffect.Start (f9898) creates WaitForSeconds only after model readiness.
                // Events flushed during this Step became ready now, after the elapsed frame.
                if(!item.Managed||item.CreatedStep!=stepSerial)item.Age+=dt;
                if(item.Managed&&item.Source.Duration>0&&item.Age>=item.Source.Duration){remove.Add(pair.Key);continue;}
                if(item.Source.Kind=="projectile-spawn")
                {
                    var projectile=simulation.SkillProjectiles.Find(p=>p.VisualId==pair.Key);
                    if(projectile!=null)item.Object.transform.position=simulation.SkillProjectileVisualPosition(projectile);
                    else if(item.Source.SkillId==18)
                    {
                        float t=Mathf.Clamp01(item.Age/Mathf.Max(.0001f,item.Source.Duration)),u=1-t;
                        item.Object.transform.position=u*u*item.Source.Start+2*u*t*item.Source.ControlPoint+t*t*item.Source.End;
                    }
                    if(item.Source.RotationSpeed!=0)
                    {
                        // Bullet.Update passes this unnormalized vector to Rotate's local Euler overload.
                        var axis=Quaternion.AngleAxis(-90,Vector3.up)*(item.Source.End-item.Source.Start);
                        item.Object.transform.Rotate(axis*(dt*item.Source.RotationSpeed),Space.Self);
                    }
                }
                if(item.Recruit!=null)
                {
                    var recruit=simulation.RecruitedSoldiers.Find(s=>s.VisualId==pair.Key);
                    if(recruit==null||!recruit.Active){remove.Add(pair.Key);continue;}
                    item.RecruitPose.Position=recruit.Position;item.RecruitPose.LegStart=recruit.Position;item.RecruitPose.LegEnd=recruit.Position+recruit.Direction;
                    item.Recruit.Synchronize(item.RecruitPose,view.BattleCamera,view.VisualClock);
                }
                if(item.Object.activeInHierarchy)item.Clock.Step(dt);
            }
            foreach(int id in remove)Remove(id);
            var gone=new List<long>();
            foreach(var pair in attachments)
            {
                int id=(int)(pair.Key>>32);var soldier=simulation.Soldiers.Find(s=>s.Id==id);
                if(pair.Value.Object==null||soldier==null||!soldier.Active){Dispose(pair.Value);gone.Add(pair.Key);}
                else pair.Value.Clock.Step(dt);
            }
            foreach(long key in gone)attachments.Remove(key);
            bool speedUp=simulation.IsSkillActive(4),speedDown=simulation.IsSkillActive(5);
            foreach(var tower in simulation.Towers)if(tower.Active&&tower.Camp!=0&&simulation.State==BattlePhase.Running)
            {
                towerVisualPoll.TryGetValue(tower.Id,out float elapsed);elapsed+=dt;
                if(elapsed>.2f)
                {
                    elapsed-=.2f;
                    if(!tower.IsBoss)SetTowerSpeedEffect(tower,4,speedUp&&tower.Camp==BattleSimulation.PlayerCampID);
                    SetTowerSpeedEffect(tower,5,speedDown&&tower.Camp!=BattleSimulation.PlayerCampID);
                }
                towerVisualPoll[tower.Id]=elapsed;
            }
            foreach(var item in embedded.Values)if(item!=null)item.Step(dt);
        }
        void Remove(int id){if(instances.TryGetValue(id,out var item)){Dispose(item);instances.Remove(id);}}
        void ResetBattleVisuals()
        {
            // Skill2 Reset stops future launches but leaves already emitted fireballs.
            // Skill12 explicitly closes managed effects; other timed world effects continue.
            var remove=new List<int>();foreach(var pair in instances)
            {
                bool retainedFireball=pair.Value.Source.SkillId==2&&pair.Value.Source.Kind=="projectile-spawn"&&
                    simulation.SkillProjectiles.Exists(p=>p.VisualId==pair.Key&&p.Active&&p.SkillId==2);
                if(!retainedFireball&&(!pair.Value.Managed||pair.Value.Source.SkillId==12))remove.Add(pair.Key);
            }
            foreach(int id in remove)Remove(id);
            foreach(var tower in simulation.Towers)if(tower.IsBoss)
            {
                var root=view.TowerPresentationTransform(tower.Id);
                var pool=root!=null?root.GetComponentInChildren<RecoveredBossProjectilePool>(true):null;
                if(pool!=null)pool.ClearSourcePool();
            }
            foreach(var item in attachments.Values)Dispose(item);attachments.Clear();
            foreach(var item in embedded.Values)if(item!=null){item.gameObject.SetActive(false);item.Step(0);}
            towerVisualPoll.Clear();
        }
        void Dispose(Instance item){if(item.BossPool!=null){item.BossPool.Release(item.Object);return;}if(item.Object!=null){item.Object.SetActive(false);if(Application.isPlaying)Destroy(item.Object);else DestroyImmediate(item.Object);}}
        void Clear()
        {
            foreach(var item in instances.Values)Dispose(item);instances.Clear();foreach(var item in attachments.Values)Dispose(item);attachments.Clear();
            foreach(var item in embedded.Values)if(item!=null)Dispose(new Instance{Object=item.gameObject});embedded.Clear();
        }
        void OnDestroy()
        {
            if(simulation!=null)simulation.SkillVisual-=Observe;
            if(view!=null&&view.Audio!=null)foreach(int id in audioHandles)view.Audio.OnSkillAudio(new SkillVisualEvent{Kind="audio-stop",HandleId=id,AudioId=2041});
            audioHandles.Clear();Clear();
        }
    }
}
