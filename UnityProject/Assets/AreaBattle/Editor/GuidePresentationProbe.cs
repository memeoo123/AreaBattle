using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class GuidePresentationProbe
    {
        public static void ImportAndRun() { RecoveredGuideSpineImporter.Import(); RecoveredHudImporter.Import(); Run(); }
        public static void Run()
        {
            var report=Validate();
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/guide-presentation-probe.json"),JsonUtility.ToJson(report,true));
            if(!report.passed)throw new InvalidOperationException("Guide presentation probe failed");Debug.Log("GUIDE_PRESENTATION_PROBE_PASS");
        }
        public static BattleBuild.Report Validate()
        {
            var report = new BattleBuild.Report { passed = true, unityVersion = Application.unityVersion };
            Action<string,Action> check = (id, test) => { try { test(); report.checks.Add(new BattleBuild.Check { id=id,result="pass" }); } catch(Exception e) { report.passed=false;report.checks.Add(new BattleBuild.Check { id=id,result="fail",detail=e.ToString() }); } };
            var prefab=Resources.Load<GameObject>("Recovered/Hud/GuideUI");
            if(prefab==null)throw new InvalidOperationException("Missing imported GuideUI");
            var go=UnityEngine.Object.Instantiate(prefab);go.SetActive(true);
            try {
                var p=go.GetComponent<RecoveredGuidePresentation>();
                if(p==null) { RecoveredGuideSpineImporter.AttachTo(go);p=go.GetComponent<RecoveredGuidePresentation>(); }
                p.Initialize();
                check("guide-original-spine-persistent-atlas",()=>{
                    var s=p.Demonstration;var d=s.skeletonDataAsset.GetSkeletonData(false);
                    Require(d.Version=="4.1.16"&&d.Bones.Count==2&&d.Slots.Count==1,"source skeleton topology/version");
                    Require(d.FindAnimation("YD")!=null&&Mathf.Abs(d.FindAnimation("YD").Duration-1.5333f)<.00001f,"source YD duration");
                    Require(s.skeletonDataAsset.atlasAssets.All(a=>a!=null),"persistent original atlas");
                    foreach(var a in s.skeletonDataAsset.atlasAssets)foreach(var page in a.GetAtlas().Pages)Require(page.rendererObject is Material&&((Material)page.rendererObject).mainTexture!=null,"source atlas page binding");
                    s.gameObject.SetActive(true);s.Update(0);s.LateUpdate();
                    Require(s.GetComponent<MeshRenderer>().sharedMaterials.All(m=>m!=null&&m.shader.name=="Spine/Skeleton"&&m.shader.isSupported),"original visible Spine shader");
                });
                check("guide-production-spine-explicit-clock",()=>{
                    var s=p.Demonstration;s.gameObject.SetActive(true);string first=s.Skeleton.Slots.Items[0].Attachment.Name;float t=s.AnimationState.GetCurrent(0).TrackTime;
                    p.Advance(.2f);Require(!s.enabled&&Mathf.Abs(s.AnimationState.GetCurrent(0).TrackTime-t-.2f)<.00001f,"one explicit tick");
                    Require(s.Skeleton.Slots.Items[0].Attachment.Name!=first,"real original attachment timeline");
                    p.Advance(0);Require(Mathf.Abs(s.AnimationState.GetCurrent(0).TrackTime-t-.2f)<.00001f,"scaled pause");
                });
                check("guide-production-hand-move-loop-pause",()=>{
                    p.StartHand(GuideHandMode.Connect,Vector3.zero,new Vector3(4,0,0));p.Advance(1);Near(p.HandPosition,new Vector3(3,0,0));p.Advance(0);Near(p.HandPosition,new Vector3(3,0,0));
                    p.Advance(1);Near(p.HandPosition,new Vector3(4,0,0));p.Advance(.5f);Near(p.HandPosition,new Vector3(1.75f,0,0));Require(p.HandVisible,"int max source loops");
                });
                check("guide-production-hand-pulse-completion",()=>{
                    Vector3 start=p.HandScale;p.StartHand(GuideHandMode.SkillPulse,Vector3.zero,Vector3.one*1.2f);p.Advance(.5f);Near(p.HandScale,Vector3.LerpUnclamped(start,Vector3.one*1.2f,.75f));p.Advance(14.5f);Require(!p.HandVisible,"15 source scale loops completed");Near(p.HandScale,Vector3.one*1.2f);
                });
                check("guide-production-stage-visibility-and-cut",()=>{
                    var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_0"));
                    var world=new BattleSimulation(layout,BattleView.ReadConfig(),4305,(a,b)=>true){AIEnabled=false};
                    using(var guide=new BattleGuide(world,0)) {
                        Func<int,Vector3> point=id=>new Vector3(id,2,0);
                        p.Synchronize(guide,point,point);Require(p.Demonstration.gameObject.activeSelf&&!p.HandVisible,"stage1 popup demonstration only");
                        guide.Tick(0,1);Require(guide.Confirm(),"prompt confirm");p.Synchronize(guide,point,point);Require(p.HandVisible&&!p.Demonstration.gameObject.activeInHierarchy,"hand after confirmation, mainbg hidden");
                        Require(guide.TryConnect(guide.HighlightSourceId,guide.HighlightTargetId),"tutorial connection");var target=world.Tower(guide.HighlightTargetId);world.ChangeScore(target.Id,1,-target.Score);guide.Tick(BattleGuide.TransitionDelay,0);p.Synchronize(guide,point,point);
                        Require(guide.HandMode==GuideHandMode.Cut,"cut stage");var a=point(guide.HighlightSourceId)-Vector3.up;var b=point(guide.HighlightTargetId)-Vector3.up;var mid=(a+b)*.5f;Near(p.HandPosition,mid+Quaternion.AngleAxis(-90,Vector3.forward)*(a-mid));
                    }
                });
                check("guide-source-pitch-and-tip-lifecycle",()=>{
                    foreach(int level in new[]{7,9,6,14,21,25}) {
                        var layout=JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_"+(level==25?501:level)));
                        var world=new BattleSimulation(layout,BattleView.ReadConfig(),4305,(a,b)=>true){AIEnabled=false};
                        using(var guide=new BattleGuide(world,level)) {
                            Func<int,Vector3> point=id=>new Vector3(id,2,0);p.Synchronize(guide,point,point);
                            var tip=go.transform.Find("mainbg/textTipAdd").GetComponent<UnityEngine.UI.Text>();var pitch=go.transform.Find("mainbg/guideContent/Pitch");
                            Require(tip.gameObject.activeInHierarchy&&tip.text.Length>0,"source popup-only rich text");
                            Require(tip.transform.parent.name=="mainbg","original text parent retained");
                            bool hasPitch=level==7||level==9;Require(pitch.gameObject.activeSelf==hasPitch,"stage7/8 Pitch only");
                            Require(tip.alignment==(hasPitch?TextAnchor.MiddleLeft:TextAnchor.MiddleCenter),"source text alignment");
                            if(hasPitch) {string[] values=level==7?new[]{"2","2","1","2"}:new[]{"2","2","2","1"};for(int i=0;i<4;i++)Require(pitch.GetChild(i).GetChild(2).GetComponent<UnityEngine.UI.Text>().text==values[i],"GuideConfig param child order");}
                            guide.Tick(0,1);Require(guide.Confirm(),"confirm source popup");p.Synchronize(guide,point,point);Require(!tip.gameObject.activeSelf&&tip.text==string.Empty&&!pitch.gameObject.activeInHierarchy,"OnOK clears and hides source tip/Pitch");
                        }
                    }
                });
            } finally {UnityEngine.Object.DestroyImmediate(go);Time.timeScale=1;}
            return report;
        }
        static void Require(bool value,string text){if(!value)throw new InvalidOperationException(text);}
        static void Near(Vector3 a,Vector3 b){Require(Vector3.Distance(a,b)<.0001f,"position/scale "+a+" != "+b);}
    }
}
