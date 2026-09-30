using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameUiImportValidation
    {
        [Serializable] sealed class Manifest {public Page[] prefabs;}
        [Serializable] sealed class Page {public string name;public Node[] nodes;}
        [Serializable] sealed class Node {public string path,name,transformSourceId;public bool active;public RectData transform;public Component[] components;}
        [Serializable] sealed class Component {public string className,data,sourceId;}
        [Serializable] sealed class Pointer {public long m_PathID;}
        [Serializable] sealed class UiData {public int m_Horizontal,m_Vertical,m_Inertia,m_ShowMaskGraphic,m_HorizontalFit,m_VerticalFit,m_MovementType;public float m_Elasticity,m_DecelerationRate,m_ScrollSensitivity;public Pointer m_Content,m_Viewport;}
        [Serializable] sealed class RectData {public Vector2 m_AnchorMin,m_AnchorMax,m_AnchoredPosition,m_SizeDelta,m_Pivot;}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Imported source hierarchy/rect geometry only. Runtime controllers, dynamic content and visual matching remain incomplete."};
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/ui-import.json")));
            foreach(var page in manifest.prefabs)
            {
                string id="outgame-ui-original-layout-"+page.name;
                try
                {
                    var prefab=Resources.Load<GameObject>("Recovered/Outgame/"+page.name);
                    if(!prefab)throw new Exception("Missing prefab");
                    var resolved=new Dictionary<string,Transform>();var sourceTransforms=new Dictionary<string,Transform>();var childIndices=new Dictionary<string,int>();
                    foreach(var node in page.nodes)
                    {
                        Transform t;
                        if(string.IsNullOrEmpty(node.path))t=prefab.transform;
                        else
                        {
                            int slash=node.path.LastIndexOf('/');string parent=slash<0?"":node.path.Substring(0,slash);
                            childIndices.TryGetValue(parent,out int index);t=resolved[parent].GetChild(index);childIndices[parent]=index+1;
                        }
                        resolved.Add(node.path,t);sourceTransforms.Add(node.transformSourceId,t);
                        if(t.name!=node.name)throw new Exception("Source sibling order/name changed: "+node.path);
                        if(!t)throw new Exception("Missing source node "+node.path);
                        var rect=t as RectTransform;var source=node.transform;
                        if(!rect||Vector2.Distance(rect.anchorMin,source.m_AnchorMin)>0.01f||Vector2.Distance(rect.anchorMax,source.m_AnchorMax)>0.01f||Vector2.Distance(rect.pivot,source.m_Pivot)>0.01f)
                            throw new Exception("Source rect anchors/pivot changed: "+node.path);
                        if(t.gameObject.activeSelf!=node.active)throw new Exception("Source active state changed: "+node.path);
                    }
                    int controls=0;
                    foreach(var node in page.nodes)foreach(var c in node.components)
                    {
                        var t=resolved[node.path];var d=JsonUtility.FromJson<UiData>(c.data);
                        if(c.className=="ScrollRect")
                        {
                            var scroll=t.GetComponent<ScrollRect>();string prefix=c.sourceId.Substring(0,c.sourceId.LastIndexOf(':')+1);
                            if(!scroll||scroll.content!=sourceTransforms[prefix+d.m_Content.m_PathID]||scroll.viewport!=sourceTransforms[prefix+d.m_Viewport.m_PathID]||scroll.horizontal!=(d.m_Horizontal!=0)||scroll.vertical!=(d.m_Vertical!=0)||scroll.inertia!=(d.m_Inertia!=0)||Mathf.Abs(scroll.decelerationRate-d.m_DecelerationRate)>0.0001f||Mathf.Abs(scroll.elasticity-d.m_Elasticity)>0.0001f)
                                throw new Exception("Source scrolling contract mismatch: "+node.path);
                            controls++;
                        }
                        if(c.className=="Mask")
                        {var mask=t.GetComponent<Mask>();if(!mask||mask.showMaskGraphic!=(d.m_ShowMaskGraphic!=0))throw new Exception("Source mask mismatch: "+node.path);controls++;}
                        if(c.className=="ContentSizeFitter")
                        {var fit=t.GetComponent<ContentSizeFitter>();if(!fit||(int)fit.horizontalFit!=d.m_HorizontalFit||(int)fit.verticalFit!=d.m_VerticalFit)throw new Exception("Source content fitting mismatch: "+node.path);controls++;}
                    }
                    report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=page.nodes.Length+" source nodes and "+controls+" scroll/mask/fitter controls retained"});
                }
                catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.Message});}
            }
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-ui-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
