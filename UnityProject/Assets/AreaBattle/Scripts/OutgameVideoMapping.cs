using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    [Serializable] public sealed class OutgameVideoPlacement
    {public int Id;public int PlacementId;public string ReportLable;}
    [Serializable] sealed class OutgameVideoPlacementTable
    {public OutgameVideoPlacement[] Datas;}
    // ProcedureStarGame.OnEnter builds ReportLable -> PlacementId using Dictionary.Add.
    // Bridge_WX_XYXFunction.SetVideoMapping retains that dictionary by reference.
    public sealed class OutgameVideoMapping
    {
        Dictionary<string,int> mapping;
        readonly Action<string> error;
        public OutgameVideoMapping(Action<string> error){this.error=error??throw new ArgumentNullException(nameof(error));}
        public static Dictionary<string,int> Build(IEnumerable<OutgameVideoPlacement> configValues)
        {
            var result=new Dictionary<string,int>();foreach(var row in configValues)result.Add(row.ReportLable,row.PlacementId);return result;
        }
        public static OutgameVideoPlacement[] ReadOriginalTable(string text)
        {return JsonUtility.FromJson<OutgameVideoPlacementTable>(text).Datas;}
        public void SetVideoMapping(Dictionary<string,int> values){mapping=values;}
        // The placement-resolution part of OnNewEvent after video_name extraction.
        // Event filtering, JSON conversion, ad delivery and rewards are separate source paths.
        public int ResolvePlacement(string videoName)
        {
            int placement=0;
            if(mapping.ContainsKey(videoName))
            {
                placement=mapping[videoName];if(placement>0)return placement;
                error("WXAMS,视频名称映射为0或负数,video_name:"+videoName+",placementId:"+placement);
            }
            else error("WXAMS,未设置视频名称映射,video_name:"+videoName+",placementId:"+placement);
            return 68;
        }
    }
}
