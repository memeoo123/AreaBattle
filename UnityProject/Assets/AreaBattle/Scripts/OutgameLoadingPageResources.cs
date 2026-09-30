using System;
using UnityEngine;
namespace AreaBattle
{
    // Explicit provenance mapping from original Resources path to its reconstructed prefab.
    public sealed class OutgameLoadingPageResources
    {
        readonly OutgameScalarTweenRunner tweens;readonly IOutgameLoadingPageHost host;
        public OutgameLoadingPageResources(OutgameScalarTweenRunner tweens,IOutgameLoadingPageHost host){this.tweens=tweens;this.host=host;}
        public OutgameLoadingPage Instantiate(string sourcePath)
        {
            if(sourcePath!="FirstScreenRes/Proj_xqzdLoadingUI")throw new ArgumentOutOfRangeException(nameof(sourcePath),sourcePath,"No recovered loading resource mapping");
            var page=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Loading/Proj_xqzdLoadingUI")).GetComponent<OutgameLoadingPage>();page.Bind(tweens,host);return page;
        }
    }
}
