using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // Fields consumed by the recovered adapter lifecycle; names match original JSON.
    [Serializable] public sealed class OutgameAdIdsInfo
    {public int platformId,priority;public string idVals;}
    [Serializable] public sealed class OutgameAdZoneConfig
    {public string zkey,customParam;public float rotaTimeout,banRefreshTime,reqInterTime;public List<OutgameAdIdsInfo> idsInfo;}
    [Serializable] public sealed class OutgameAdCustomParam
    {public string ad_bidding_cache_nextday_clear_enable,banner_error_report_enable,video_ad_timeout;}
}
