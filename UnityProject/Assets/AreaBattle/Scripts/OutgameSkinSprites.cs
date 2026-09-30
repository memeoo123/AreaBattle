using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Atlas-qualified original Sprite ids avoid the unrelated guide atlas tubiao01..04 names.
    public sealed class OutgameSkinSprites
    {
        [Serializable] sealed class Manifest {public Row[] skinIcons;}
        [Serializable] sealed class Row {public int skinId;public string name,id,atlas;}
        readonly Dictionary<int,Sprite> sprites=new Dictionary<int,Sprite>();
        public OutgameSkinSprites()
        {
            var manifest=JsonUtility.FromJson<Manifest>(Resources.Load<TextAsset>("Recovered/Outgame/hud-import").text);
            foreach(var row in manifest.skinIcons)
            {
                var sprite=Resources.Load<Sprite>("Recovered/Outgame/Sprites/"+row.id.Replace(':','_').Replace('/','_'));
                if(sprite==null)throw new InvalidOperationException("Missing original skin sprite: "+row.skinId+" "+row.id);
                sprites.Add(row.skinId,sprite);
            }
        }
        public Sprite ForSkin(int id)=>sprites[id];
    }
}
