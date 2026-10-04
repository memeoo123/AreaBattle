using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Local recovered-asset lookup; source async atlas loading remains the resource module's responsibility.
    public sealed class OutgameGuideBookSprites
    {
        [Serializable] sealed class Manifest{public Row[] sprites;}
        [Serializable] sealed class Row{public string name,id;}
        readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        public OutgameGuideBookSprites()
        {
            var manifest=JsonUtility.FromJson<Manifest>(Resources.Load<TextAsset>("Recovered/GuideBook/guide-sprites").text);
            foreach(var row in manifest.sprites)
            {
                var sprite=Resources.Load<Sprite>("Recovered/GuideBook/Sprites/"+row.id.Replace(':','_').Replace('/','_'));
                if(!sprite)throw new InvalidOperationException("Missing recovered GuideSprite payload: "+row.id);
                sprites.Add(row.name,sprite);
            }
        }
        public Sprite Find(string name)=>name!=null&&sprites.TryGetValue(name,out var sprite)?sprite:null;
    }
}
