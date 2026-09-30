using System;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameSceneTextures
    {
        public static void Load(string name,Action<Texture> done)
        {
            var texture=Resources.Load<Texture2D>("Recovered/Outgame/SceneTextures/"+name);
            if(texture==null)throw new InvalidOperationException("Original scene texture missing: "+name);
            done(texture);
        }
    }
}
