using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Runtime adapter for the recovered JSON string arrays. Not a replacement for all LitJson behavior.
    public static class OutgameLegacyPackListJson
    {
        [Serializable] sealed class Envelope { public string[] items; }
        public static List<string> Deserialize(string text)
        {
            var value=JsonUtility.FromJson<Envelope>("{\"items\":"+text+"}");
            return value.items==null?null:new List<string>(value.items);
        }
    }
}
