using System;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // ProcessControl30372..30379 and ABProcessConst34450.
    public sealed class OutgameProcessControl:IOutgameLogicControl
    {
        readonly Func<OutgameLegacyConfigManager> config;
        readonly Func<int> getChannel;
        readonly Func<bool> isInstallVersion;
        readonly Action<int> setChannel;
        readonly Action<Color,string> log;
        readonly Action<string> warning;
        readonly Func<int,string> channelName;
        readonly OutgameControllerRegistry registry;
        public bool Flag8;
        public bool ActiveUpdate {get;set;}
        public int SDK_CompleteLevelCount,SDK_StartLevelCount;
        public ChannelProcedureConfig ProcedureConfig {get;private set;}
        public bool IsBChannel=>ProcedureConfig.id==2;
        public OutgameProcessControl(Func<OutgameLegacyConfigManager> config,Func<int> getChannel,Func<bool> isInstallVersion,Action<int> setChannel,Action<Color,string> log,Action<string> warning,Func<int,string> channelName,OutgameControllerRegistry registry)
        {this.config=config;this.getChannel=getChannel;this.isInstallVersion=isInstallVersion;this.setChannel=setChannel;this.log=log;this.warning=warning;this.channelName=channelName;this.registry=registry;}
        public ChannelProcedureConfig GetChannelProcedureConfig(int channel,int releasePlatform)
        {
            // Read destination table before obtaining channel row, as in each source branch.
            if(channel< -1||channel>4)return null;
            var procedures=config().dicChannelProcedure;
            var row=config().dicChannel[releasePlatform];
            int id;
            switch(channel){case -1:id=row.noAB;break;case 0:id=row.channelA;break;case 1:id=row.channelB;break;case 2:id=row.channelC;break;case 3:id=row.channelD;break;default:id=row.channelE;break;}
            return procedures[id];
        }
        public void OnInit(){SDK_CompleteLevelCount=0;SDK_StartLevelCount=0;Flag8=false;InitProcedure();}
        public void InitProcedure()
        {
            int channel=getChannel();ProcedureConfig=GetChannelProcedureConfig(channel,1);
            if(!isInstallVersion())
            {
                log(Color.green,"当前为老用户");setChannel(-1);ProcedureConfig=config().dicChannelProcedure[1];
            }
            log(Color.green,"当前获取到的AB测试: "+channelName(channel));
            log(Color.green,"当前渠道: RPAndroid");
            if(ProcedureConfig!=null)log(Color.green,"当前流程id: "+ProcedureConfig.id);
        }
        public bool JudgeChannel(params int[] channels)
        {
            if(ProcedureConfig==null)return false;
            bool result=false;
            for(int i=0;i<channels.Length;i++)if(ProcedureConfig.id==unchecked(channels[i]+1)){result=true;break;}
            warning(string.Format("当前渠道id为{0},判断结果为{1}",ProcedureConfig.id,result));return result;
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} // Original30378.
        public void OnDispose(){Flag8=false;registry.Clear(3903);}
    }
}
