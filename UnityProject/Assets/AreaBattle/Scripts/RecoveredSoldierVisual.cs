using System;
using UnityEngine;
namespace AreaBattle
{
    // Parameters and transforms from presentation-binding-evidence.json.
    public sealed class RecoveredSoldierVisual : MonoBehaviour
    {
        public float RunStart,RunFrames,RunSeconds,SourceScale;
        public float DeadStart,DeadFrames,DeadSeconds;
        public Color[] CampColors;
        MeshRenderer target;
        MaterialPropertyBlock properties;
        float started;
        bool dying;
        Vector3 deathPosition,deathDrift;
        public bool DeathFinished(float clock)=>dying&&clock-started>=Mathf.Max(0,DeadSeconds-.05f);
        public void BeginDeath(float clock)
        {
            if(dying)return;dying=true;started=clock;deathPosition=transform.position;
            deathDrift=new Vector3(UnityEngine.Random.Range(-.1f,.1f),0,UnityEngine.Random.Range(-.1f,.1f));
        }
        public void Begin(float clock)
        {started=clock;target=GetComponent<MeshRenderer>();properties=new MaterialPropertyBlock();}
        public void Synchronize(SoldierState soldier,Camera camera,float clock)
        {
            if(target==null)Begin(clock);
            if(dying){float t=Mathf.Clamp01((clock-started)/Mathf.Max(.001f,DeadSeconds-.05f));transform.position=deathPosition+deathDrift*(t*(2-t));}
            else transform.position=soldier.Position;
            bool reverse=soldier.ShipType<=3&&soldier.LegStart.x<soldier.LegEnd.x;
            float tilt=camera.transform.localEulerAngles.x;
            transform.localEulerAngles=reverse?new Vector3(-tilt,-180,0):new Vector3(tilt,0,0);
            properties.SetColor("_Color",soldier.Camp>=1&&soldier.Camp<CampColors.Length?CampColors[soldier.Camp]:Color.white);
            properties.SetVector("_AnimTime",dying?new Vector4(DeadStart,DeadFrames,1f/Mathf.Max(.01f,DeadSeconds),started):new Vector4(RunStart,RunFrames,1f/Mathf.Max(.01f,RunSeconds),started));
            properties.SetFloat("_AnimLoop",dying?0f:1f);
            properties.SetFloat("_BattleVisualTime",clock);
            target.SetPropertyBlock(properties);
        }
    }
}
