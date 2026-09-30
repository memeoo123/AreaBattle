using System;
using UnityEngine;

namespace AreaBattle
{
    // Source: generated/wayline-runtime-evidence.json, original LineRendererEntity.SetLine.
    public sealed class RecoveredWayLineVisual : MonoBehaviour
    {
        public LineRenderer Arrow1,Arrow2,Background1,Background2;
        public BoxCollider CutCollider;
        public float SourceWidth=.1f,SourceMoveSpeed=-.1f,TilingPerUnit=16f;
        public Color[] CampColors;
        public bool Double=>Arrow2!=null;
        MaterialPropertyBlock properties;
        Material[] instances;

        public static bool IsDouble(LineState line,TowerState small,TowerState large)
            =>line.Direction==3&&small.Camp!=large.Camp;
        public Color GetCampColor(int camp)=>CampColors!=null&&camp>=0&&camp<CampColors.Length?CampColors[camp]:Color.green;
        public void Synchronize(LineState line,TowerState small,TowerState large,float visualTime)
            =>Synchronize(line,small,large,GetCampColor(small.Camp),GetCampColor(large.Camp),visualTime);

        void Initialize()
        {
            if(properties!=null)return;
            properties=new MaterialPropertyBlock();
            var renderers=new[]{Arrow1,Arrow2,Background1,Background2};
            instances=new Material[renderers.Length];
            for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)
            {
                // Intentionally use the original startWidth setter on the recovered curve.
                renderers[i].startWidth=SourceWidth;
                instances[i]=renderers[i].material;
            }
            properties.SetFloat("_Vspeed",SourceMoveSpeed);
        }

        public void Synchronize(LineState line,TowerState small,TowerState large,
            Color smallColor,Color largeColor,float visualTime)
        {
            bool active=line.Direction!=0;
            if(gameObject.activeSelf!=active)gameObject.SetActive(active);
            if(!active)return;
            if(Double!=IsDouble(line,small,large))throw new InvalidOperationException("WayLine prefab topology changed; select the matching single/double prefab.");
            bool reverse=line.Direction==2;
            SetLine(reverse?large.Position:small.Position,reverse?small.Position:large.Position,
                reverse?largeColor:smallColor,reverse?smallColor:largeColor,
                small.Camp==BattleSimulation.PlayerCampID||large.Camp==BattleSimulation.PlayerCampID,true,visualTime);
        }

        // visualTime is the same scaled global presentation clock used for other recovered effects.
        // A negative value lets the shader use Unity's _Time.y directly.
        public void SetLine(Vector3 from,Vector3 to,Color fromColor,Color toColor,
            bool allowCut,bool showArrow,float visualTime=-1f)
        {
            Initialize();
            from+=Vector3.up*.02f;to+=Vector3.up*.02f;
            Vector3 middle=(from+to)*.5f,arrowOffset=Vector3.up*.001f;
            Segment(Arrow1,from+arrowOffset,(Double?middle:to)+arrowOffset,fromColor,visualTime);
            Segment(Background1,from,Double?middle:to,fromColor,visualTime);
            Background1.enabled=!showArrow;
            if(Double)
            {
                Segment(Arrow2,to+arrowOffset,middle+arrowOffset,toColor,visualTime);
                Segment(Background2,to,middle,toColor,visualTime);
                Background2.enabled=!showArrow;
                // Original SetLine does not change Arrow2.enabled (prefab defaults to true).
            }
            Arrow1.enabled=showArrow;
            CutCollider.enabled=allowCut;
            if(allowCut)
            {
                transform.position=middle;
                if((to-middle).sqrMagnitude>0)transform.LookAt(to);
                CutCollider.size=new Vector3(.1f,.1f,Vector3.Distance(from,to));
            }
        }

        void Segment(LineRenderer renderer,Vector3 from,Vector3 to,Color color,float time)
        {
            renderer.positionCount=2;renderer.SetPosition(0,from);renderer.SetPosition(1,to);
            renderer.material.SetTextureScale("_Tex",new Vector2(Vector3.Distance(from,to)*TilingPerUnit,1));
            properties.SetColor("_color",color);
            properties.SetFloat("_BattleVisualTime",time);
            renderer.SetPropertyBlock(properties);
        }

        void OnDestroy()
        {
            if(instances==null)return;
            foreach(var material in instances)if(material!=null)
                if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);
        }
    }
}
