using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // GameControl31256 / helper31255. References are the original scene owner's objects.
    public sealed class OutgameHomeScenePresentation
    {
        readonly Transform home,game;readonly Camera homeCamera,commanderCamera,gameCamera;
        public Dictionary<int,GameObject> SourceObjects56;
        public int SourceOffset60;
        public OutgameHomeScenePresentation(Transform home,Transform game,Camera homeCamera,Camera commanderCamera,Camera gameCamera)
        {this.home=home;this.game=game;this.homeCamera=homeCamera;this.commanderCamera=commanderCamera;this.gameCamera=gameCamera;}
        public void PrepareHomeScene()
        {
            if(SourceObjects56!=null&&SourceObjects56.TryGetValue(unchecked(100+SourceOffset60),out var prior))prior.SetActive(false);
            game.gameObject.SetActive(false);home.gameObject.SetActive(true);
            homeCamera.gameObject.SetActive(true);commanderCamera.gameObject.SetActive(true);
            commanderCamera.gameObject.SetActive(true);gameCamera.gameObject.SetActive(false);
        }
    }
}
