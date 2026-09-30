using System;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameFlyCurrencyAssets
    {
        public static GameObject Load(string sourcePath)
        {
            string name;
            switch(sourcePath){case "model/entity/golditem":name="goldItem";break;case "model/entity/diamonditem":name="diamondItem";break;case "model/entity/strengthitem":name="strengthItem";break;default:throw new ArgumentOutOfRangeException(nameof(sourcePath),sourcePath,"Unknown source fly currency asset");}
            return Resources.Load<GameObject>("Recovered/FlyCurrency/"+name)??throw new InvalidOperationException("Missing recovered fly currency prefab: "+name);
        }
    }
}
