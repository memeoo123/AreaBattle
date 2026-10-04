using System;
namespace AreaBattle
{
    public enum OutgameDomainType {RD=1,TD=2,RTD=3}
    public sealed class OutgameNetToolServices
    {
        public Func<string,string> OnlineConfig;
        public Func<string,OutgameDomainType,string,string> GameBizDomain;
        public Action<object[]> Warning;
    }
    // One original static NetTool domain cache per installed platform session.
    // The platform supplies its real domain query; an empty result remains retryable.
    public sealed class OutgameNetTool
    {
        readonly OutgameNetToolServices services;
        string baseUrl;
        public OutgameNetTool(OutgameNetToolServices services){this.services=services;}
        public string BaseUrl
        {
            get{
                if(string.IsNullOrEmpty(baseUrl)){
                    int dtype=1;string configured=services.OnlineConfig("NetDtype");
                    if(!string.IsNullOrEmpty(configured)&&configured!="0")int.TryParse(configured,out dtype);
                    services.Warning(new object[]{"NetDtype",(OutgameDomainType)dtype});
                    string domain=services.GameBizDomain("hcrzd-u-cn-wx",(OutgameDomainType)dtype,"default");
                    if(dtype==1)domain="https://"+domain+":20150";
                    baseUrl=domain;services.Warning(new object[]{"NetTool URL",baseUrl});
                }
                return baseUrl;
            }
        }
    }
}
