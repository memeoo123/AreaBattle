using System;
using DispatchRow=AreaBattle.OriginalConfig.DispatchConfig;
namespace AreaBattle
{
    // BattleControl4060: keep the three config row references captured by source OnInit31249.
    public sealed class OutgameBattleControl:IOutgameLogicControl
    {
        readonly Func<OutgameLegacyConfigManager> config;
        readonly OutgameControllerRegistry registry;
        DispatchRow first,second,third;
        public OutgameBattleControl(Func<OutgameLegacyConfigManager> config,OutgameControllerRegistry registry)
        {this.config=config;this.registry=registry;}
        public void OnInit()
        {
            first=config().dicDispatch[1];
            second=config().dicDispatch[2];
            third=config().dicDispatch[3];
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Source31246 is empty.
        public void OnDispose()=>registry.Clear(4060); // Source31250 retains all three row fields.
        DispatchRow Grade(int grade)=>grade==0?first:grade==1?second:third;
        public int GetDispatchLineNum(int grade)=>Grade(grade).maxLine;
        public int GetDispatchScoreNum(int grade)=>Grade(grade).scoreLimit;
        public float GetDispatchAddScoreTime(int grade)=>(float)Grade(grade).addSpace/1000f;
        public float GetSpawnTime(int grade,int lines)
        {
            var row=Grade(grade);
            return (float)(lines==1?row.swanpSpaceOne:lines==2?row.swanpSpaceTwo:row.swanpSpaceThree)/1000f;
        }
    }
}
