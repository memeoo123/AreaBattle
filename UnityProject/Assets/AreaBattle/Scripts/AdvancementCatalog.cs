using System;
namespace AreaBattle
{
    // Progression changes connection efficiency, never the original tower's soldier combat role.
    public static class AdvancementCatalog
    {
        static readonly string[] Names={"快线","急行","休整","双线","三线","远送","直送","分送","稳运"};
        static readonly string[][] Stages={
            new[]{"高效派遣","轻装行军","驻塔整备"},
            new[]{"快速投送","后方保障","持续派遣"},
            new[]{"密集输送","长途快运","恢复保障"},
            new[]{"极限派遣","极速通路","稳固后方"}
        };
        static AdvancementOption Node(string id,string name,string description,AdvancementStats bonus,int doctrine=-1,int focus=0)
        {return new AdvancementOption{Id=id,Name=name,Description=description,Bonus=bonus,Doctrine=doctrine,ArtFocus=focus};}
        public static AdvancementOption[] Options(TowerState t)
        {
            if(t.AdvancementSpent==0)return new[]{
                new AdvancementOption{Id="root_single",Name="单线",Route=TowerSpecialization.Single,Description="仅1条出线；单线出兵速度为基础的135%"},
                new AdvancementOption{Id="root_split",Name="分流",Route=TowerSpecialization.Split,Description="最多3条出线；每条出兵速度为基础的70%"},
                new AdvancementOption{Id="root_relay",Name="中继",Route=TowerSpecialization.Relay,Description="最多2条；未满也转发；自身出兵速度为基础的50%"}
            };
            if(t.AdvancementSpent==1)
            {
                if(t.Specialization==TowerSpecialization.Single)return new[]{
                    Node("0_0","快线","单线出兵速度 +15%",new AdvancementStats{Spawn=15},0),
                    Node("1_0","急行","本塔新兵移速 +25%",new AdvancementStats{Speed=25},1,1),
                    Node("2_0","休整","断线时自然增长量 +50%",new AdvancementStats{Regen=50},2,2)};
                if(t.Specialization==TowerSpecialization.Split)return new[]{
                    Node("3_0","双线","收拢为最多2条出线；出兵速度 +35%",new AdvancementStats{LineLimit=2,Spawn=35},3),
                    Node("4_0","三线","保留3条出线；出兵速度 +10%",new AdvancementStats{LineLimit=3,Spawn=10},4,1),
                    Node("5_0","远送","保留3条出线；本塔新兵移速 +25%",new AdvancementStats{Speed=25},5,2)};
                return new[]{
                    Node("6_0","直送","收拢为1条出线；新兵及转发兵移速 +30%",new AdvancementStats{LineLimit=1,Speed=30},6),
                    Node("7_0","分送","扩展为3条出线；新兵及转发兵移速 +10%",new AdvancementStats{LineLimit=3,Speed=10},7,1),
                    Node("8_0","稳运","保留2条出线；断线时自然增长量 +50%",new AdvancementStats{Regen=50},8,2)};
            }
            if(t.Doctrine<0||t.AdvancementSpent>=6)return Array.Empty<AdvancementOption>();
            int stage=t.AdvancementSpent-2;
            AdvancementStats[] bonuses;
            string[] descriptions;
            switch(stage)
            {
                case 0:
                    bonuses=new[]{new AdvancementStats{Spawn=15},new AdvancementStats{Speed=20},new AdvancementStats{Regen=30}};
                    descriptions=new[]{"出兵速度 +15%","移速 +20%","断线时自然增长量 +30%"};break;
                case 1:
                    bonuses=new[]{new AdvancementStats{Spawn=10,Speed=15},new AdvancementStats{Speed=15,Regen=25},new AdvancementStats{Spawn=20}};
                    descriptions=new[]{"出兵速度 +10%；移速 +15%","移速 +15%；断线自然增长量 +25%","出兵速度 +20%"};break;
                case 2:
                    bonuses=new[]{new AdvancementStats{Spawn=25},new AdvancementStats{Speed=30},new AdvancementStats{Spawn=10,Regen=40}};
                    descriptions=new[]{"出兵速度 +25%","移速 +30%","出兵速度 +10%；断线自然增长量 +40%"};break;
                default:
                    bonuses=new[]{new AdvancementStats{Spawn=30,Speed=10},new AdvancementStats{Speed=40,Spawn=10},new AdvancementStats{Regen=60,Spawn=15}};
                    descriptions=new[]{"出兵速度 +30%；移速 +10%","移速 +40%；出兵速度 +10%","断线自然增长量 +60%；出兵速度 +15%"};break;
            }
            var options=new AdvancementOption[3];
            for(int i=0;i<3;i++)options[i]=Node(t.Doctrine+"_"+(1+stage*3+i),Names[t.Doctrine]+"·"+Stages[stage][i],descriptions[i]+(t.Specialization==TowerSpecialization.Relay&&bonuses[i].Speed>0?"（转发兵同享移速）":""),bonuses[i],-1,i);
            return options;
        }
        public static string DoctrineName(int doctrine)=>doctrine<0?"":Names[doctrine];
    }
}
