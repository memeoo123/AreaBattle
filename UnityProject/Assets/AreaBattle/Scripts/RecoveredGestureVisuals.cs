using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    // Original WayLineCircle.DrawParaLine f2745; WayLineControl.MoveCut f6778;
    // ArrowTower projectile callbacks. See generated/gesture-visual-evidence.json.
    public sealed class RecoveredGestureVisuals : MonoBehaviour
    {
        public GameObject RingRoot {get;private set;}
        public GameObject CutRoot {get;private set;}
        public RecoveredWayLineVisual PreviewLine {get;private set;}
        public static readonly Color ValidRingTint=new Color(0,.2745098174f,.631372571f,1);
        public IReadOnlyDictionary<int,GameObject> ProjectileObjects=>projectiles;
        readonly Dictionary<int,GameObject> projectiles=new Dictionary<int,GameObject>();
        ParticleSystemRenderer ringRenderer;
        MaterialPropertyBlock tint;

        public struct PreviewResolution
        {
            public bool Valid;
            public GameObject Target;
            public Vector3 End;
        }
        // f9378/f9377: the nearest collider along the entire drag segment can snap the endpoint,
        // even when the screen pointer is over empty ground. Equal distances select the later hit.
        public static GameObject ClosestCandidate(GameObject source,IReadOnlyList<Collider> colliders)
        {
            float closest=1000f;GameObject candidate=null;
            foreach(var collider in colliders)
            {
                if(collider.gameObject==source)continue;
                float distance=Vector3.Distance(source.transform.position,collider.transform.position);
                if(distance<=closest){closest=distance;candidate=collider.gameObject;}
            }
            return candidate;
        }
        public static PreviewResolution ResolvePreview(GameObject source,Vector3 point,
            Func<GameObject,bool> isTower,GameObject directHit=null)
        {
            bool directTower=directHit!=null&&isTower(directHit);
            Vector3 queryEnd=directTower?directHit.transform.position:point;
            var colliders=Physics.OverlapCapsule(source.transform.position,queryEnd,.03f,1<<8);
            var nearest=ClosestCandidate(source,colliders);
            if(nearest==null)return new PreviewResolution{Valid=true,End=queryEnd};
            if(!isTower(nearest))return new PreviewResolution{Valid=false,End=point};
            if(!directTower||nearest!=directHit)
                foreach(var hit in Physics.OverlapCapsule(source.transform.position,nearest.transform.position,.03f,1<<8))
                    if(hit.gameObject!=source&&hit.gameObject!=nearest)
                        return new PreviewResolution{Valid=false,End=queryEnd};
            return new PreviewResolution{Valid=true,Target=nearest,End=nearest.transform.position};
        }

        GameObject Create(string name)
        {
            var prefab=Resources.Load<GameObject>("Recovered/Gestures/"+name);
            if(prefab==null)throw new InvalidOperationException("Missing original gesture prefab: "+name);
            var obj=Instantiate(prefab,transform,false);obj.name=name;
            PrepareParticles(obj);return obj;
        }
        static void PrepareParticles(GameObject obj)
        {
            // One clock: the production adapter calls Tick; native Update must not also advance PS.
            foreach(var ps in obj.GetComponentsInChildren<ParticleSystem>(true))ps.Pause(false);
        }
        static void Show(GameObject obj)
        {
            if(obj.activeSelf)return;
            obj.SetActive(true);PrepareParticles(obj);
        }
        public void DrawPreview(Vector3 start,Vector3 end,Color playerLineColor,bool valid,
            Transform target=null,bool expandTaggedTarget=false,float visualTime=-1)
        {
            if(RingRoot==null)
            {
                RingRoot=Create("LineArrow");ringRenderer=RingRoot.GetComponentInChildren<ParticleSystemRenderer>(true);
                tint=new MaterialPropertyBlock();
            }
            if(PreviewLine==null)
            {
                var prefab=Resources.Load<GameObject>("Recovered/WayLines/LineRander_single");
                if(prefab==null)throw new InvalidOperationException("Missing original preview single line");
                PreviewLine=Instantiate(prefab,transform,false).GetComponent<RecoveredWayLineVisual>();
            }
            Show(RingRoot);Show(PreviewLine.gameObject);
            float scale=1.25f;
            if(valid&&target!=null)
            {
                end=target.position;scale*=target.localScale.x;
                // Caller supplies original IsBossLv (LevelControl+53) && target Tag2 condition.
                if(expandTaggedTarget)scale*=1.3f;
            }
            RingRoot.transform.position=end+Vector3.up*.02f;
            ringRenderer.transform.localScale=Vector3.one*scale;
            Color lineColor=valid?playerLineColor:Color.red;
            PreviewLine.SetLine(start,end,lineColor,lineColor,false,false,visualTime);
            ringRenderer.GetPropertyBlock(tint);tint.SetColor("_TintColor",valid?ValidRingTint:Color.red);ringRenderer.SetPropertyBlock(tint);
        }
        public void MoveCut(Vector3 localPoint,bool show)
        {
            if(CutRoot==null)CutRoot=Create("LineRanderCut");
            CutRoot.transform.localPosition=localPoint+Vector3.up*.02f;
            CutRoot.SetActive(show);
        }
        public void ClearPreview()
        {
            if(RingRoot!=null)RingRoot.SetActive(false);
            if(PreviewLine!=null)PreviewLine.gameObject.SetActive(false);
        }
        public void EndGesture()
        {
            ClearPreview();if(CutRoot!=null)CutRoot.SetActive(false);
        }
        public void SynchronizeArrows(IReadOnlyList<ArrowState> arrows)
        {
            var present=new HashSet<int>();
            foreach(var arrow in arrows)
            {
                if(!arrow.Active)continue;present.Add(arrow.Id);
                if(!projectiles.TryGetValue(arrow.Id,out var obj))
                {
                    obj=Create("hdzd_eff_SphereTrails");obj.transform.localScale=Vector3.one*.5f;
                    obj.transform.eulerAngles=new Vector3(0,90,0);obj.transform.position=arrow.Start;
                    // f8258 @0x3bd9cd..0x3bd9fd selects exactly child index == source CampID.
                    for(int i=0;i<5;i++)obj.transform.GetChild(i).gameObject.SetActive(i==arrow.LaunchCamp);
                    PrepareParticles(obj);
                    projectiles.Add(arrow.Id,obj);
                }
                obj.transform.position=arrow.Position;
            }
            var remove=new List<int>();
            foreach(var pair in projectiles)if(!present.Contains(pair.Key))
            {
                // Original completion returns the object to its inactive pool immediately.
                pair.Value.SetActive(false);DestroyObject(pair.Value);remove.Add(pair.Key);
            }
            foreach(int id in remove)projectiles.Remove(id);
        }
        public void Tick(float scaledDelta)
        {
            if(scaledDelta<0)throw new ArgumentOutOfRangeException(nameof(scaledDelta));
            foreach(var ps in GetComponentsInChildren<ParticleSystem>(false))
            {
                ps.Simulate(scaledDelta,false,false,false);ps.Pause(false);
            }
        }
        public void ResetVisuals()
        {
            EndGesture();foreach(var pair in projectiles)DestroyObject(pair.Value);projectiles.Clear();
        }
        static void DestroyObject(GameObject obj)
        {if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
    }
}
