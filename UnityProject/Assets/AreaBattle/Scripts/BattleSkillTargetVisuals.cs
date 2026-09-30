using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // SkillControl uses entity14 for friendly/any-tower targets and15 for enemy targets.
    public sealed class BattleSkillTargetVisuals : MonoBehaviour
    {
        RecoveredEffectVisual blue,red;
        BattleView view;
        readonly Dictionary<long,RecoveredEffectVisual> towerMarks=new Dictionary<long,RecoveredEffectVisual>();
        public int TargetId {get;private set;}
        public int TargetMode {get;private set;}
        public void Initialize(BattleView owner){view=owner;}
        public void BeginDrag(int targetType){TargetMode=targetType==0?1:targetType==2?2:targetType==8?3:0;Hide();}
        public void EndDrag(){TargetMode=0;Hide();}
        public void ResetForRetry(){EndDrag();foreach(var mark in towerMarks.Values)if(mark!=null){mark.gameObject.SetActive(false);mark.Step(0);}}
        public GameObject TowerMark(int id,bool enemy)=>towerMarks.TryGetValue(((long)id<<1)|(enemy?1L:0L),out var v)?v.gameObject:null;
        public static bool ValidTarget(int targetType,TowerState tower)
        {
            if(tower==null||!tower.Active)return false;
            return targetType==8||(targetType==0&&tower.Camp==BattleSimulation.PlayerCampID)||(targetType==2&&tower.Camp!=BattleSimulation.PlayerCampID);
        }
        RecoveredEffectVisual Create(bool enemy)
        {
            string name=enemy?"LineArrow_hero_red":"LineArrow_hero_blue";
            var prefab=Resources.Load<GameObject>("Recovered/SkillTargets/"+name);
            if(prefab==null)throw new InvalidOperationException("Missing original skill targeting indicator "+name);
            var obj=Instantiate(prefab,transform,false);return RecoveredEffectVisual.Attach(obj);
        }
        public bool ShowTarget(int targetType,TowerState tower)
        {
            if(!ValidTarget(targetType,tower)){Hide();return false;}
            bool enemy=targetType==2;
            if(enemy&&blue!=null)blue.gameObject.SetActive(false);
            if(!enemy&&red!=null)red.gameObject.SetActive(false);
            var visual=enemy?(red??(red=Create(true))):(blue??(blue=Create(false)));
            visual.gameObject.SetActive(true);visual.transform.position=tower.Position+Vector3.up*.01f;
            TargetId=tower.Id;return true;
        }
        public void Hide(){TargetId=0;if(blue!=null)blue.gameObject.SetActive(false);if(red!=null)red.gameObject.SetActive(false);}
        void ActivateMark(TowerState tower,bool enemy)
        {
            long key=((long)tower.Id<<1)|(enemy?1L:0L);
            if(!towerMarks.TryGetValue(key,out var marker))
            {
                string name=enemy?"hero_sanjiao_red":"hero_sanjiao_blue";
                string path=(tower.IsBoss?"Recovered/BossEmbedded/":"Recovered/SkillEffects/")+name;
                var prefab=Resources.Load<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Missing original tower target marker "+path);
                var obj=Instantiate(prefab,view.TowerPresentationTransform(tower.Id),false);obj.SetActive(false);
                marker=RecoveredEffectVisual.Attach(obj);towerMarks[key]=marker;
            }
            marker.gameObject.SetActive(true);
        }
        public void Step(float dt)
        {
            if(blue!=null)blue.Step(dt);if(red!=null)red.Step(dt);
            if(view!=null&&view.Simulation.State==BattlePhase.Running)
            {
                // Tower.Update precedes its camp!=0 speed poll. Unmatched branches do
                // not actively hide a prior marker; mode3 also has no activation branch.
                foreach(var tower in view.Simulation.Towers)if(tower.Active)
                {
                    if(TargetMode==0)
                    {var a=TowerMark(tower.Id,false);var b=TowerMark(tower.Id,true);if(a!=null)a.SetActive(false);if(b!=null)b.SetActive(false);}
                    else if(TargetMode==2&&tower.Camp!=BattleSimulation.PlayerCampID)ActivateMark(tower,true);
                    else if(TargetMode==1&&tower.Camp==BattleSimulation.PlayerCampID)ActivateMark(tower,false);
                }
            }
            foreach(var mark in towerMarks.Values)if(mark!=null)mark.Step(dt);
        }
    }
}
