using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public partial class BattleSimulation
    {
        float evolutionReaction, evolutionPlanning;
        void TickEvolutionAI(float dt)
        {
            if(!BasicTowerExperiment||!AIEnabled||State!=BattlePhase.Running)return;
            evolutionReaction-=dt;
            if(evolutionReaction<=0){
                evolutionReaction=.6f;
                foreach(var source in Towers){
                    if(!EvolutionAISource(source))continue;
                    foreach(var attacker in Incoming(source.Id)){
                        if(attacker.Camp==source.Camp)continue;
                        var line=FindLine(source.Id,attacker.Id);
                        if(line.IsFrom(source.Id))continue;
                        if(!source.CanAddLine){
                            var replace=Outgoing(source.Id).Find(l=>!l.IsFrom(l.Other(source.Id)));
                            if(replace==null)break;
                            SetDirection(replace,replace.Direction&~(replace.SmallTowerId==source.Id?1:2));
                        }
                        if(source.CanAddLine)Connect(source.Id,attacker.Id,true);
                    }
                }
            }
            evolutionPlanning-=dt;
            if(evolutionPlanning>0)return;
            evolutionPlanning=1.5f;
            var camps=new HashSet<int>();
            foreach(var tower in Towers)if(EvolutionAISource(tower))camps.Add(tower.Camp);
            foreach(int camp in camps)PlanEvolutionAI(camp);
        }
        bool EvolutionAISource(TowerState t)=>t.Active&&t.Camp!=0&&t.Camp!=PlayerCampID&&!t.IsBoss&&!t.IsArrow&&t.Mode!=4;
        void PlanEvolutionAI(int camp)
        {
            var sources=Towers.FindAll(t=>t.Camp==camp&&EvolutionAISource(t));
            sources.Sort((a,b)=>b.Score.CompareTo(a.Score));
            foreach(var source in sources){
                if(!source.CanAddLine)continue;
                // Keep one young rear tower growing, while the rest expand or defend.
                if(sources.Count>1&&source==sources[0]&&source.Score<10&&Incoming(source.Id).TrueForAll(t=>t.Camp==camp))continue;
                TowerState best=null;float priority=float.MaxValue;
                foreach(var line in adjacency[source.Id]){
                    var target=Tower(line.Other(source.Id));
                    if(!target.Active||line.IsFrom(source.Id))continue;
                    bool support=target.Camp==camp;
                    if(support&&(target.Score>=10||Incoming(target.Id).TrueForAll(t=>t.Camp==camp)||line.Direction!=0))continue;
                    float score=(support?-20:target.Score)+(target.Position-source.Position).magnitude*3;
                    if(score<priority){priority=score;best=target;}
                }
                if(best!=null&&Connect(source.Id,best.Id,true))return;
            }
        }
    }
}
