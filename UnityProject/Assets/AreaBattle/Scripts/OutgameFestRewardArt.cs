using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    // Source34376 image-name lookup; local adapter binds already imported, source-identified sprites.
    public sealed class OutgameFestRewardArt
    {
        [Serializable] sealed class Skins {public Skin[] Datas;}
        [Serializable] sealed class Skin {public int id;public string iconName;}
        [Serializable] sealed class ArtManifest {public Art[] sprites;}
        [Serializable] sealed class Art {public string atlas,name,path;}
        readonly OutgameFestRewardSelection selection;readonly Func<string> soldiers,scenes;
        readonly Dictionary<string,string> paths=new Dictionary<string,string>();
        public OutgameFestRewardArt(OutgameFestRewardSelection selection,Func<string> soldiers,Func<string> scenes,string manifest)
        {
            this.selection=selection;this.soldiers=soldiers;this.scenes=scenes;
            foreach(var art in JsonUtility.FromJson<ArtManifest>(manifest).sprites)paths.Add(art.atlas+"/"+art.name,art.path);
        }
        public (string icon,string atlas) GetAwardImage(int kind)
        {
            if(selection.GetAwardId(kind)==-1)return ("","");
            switch(kind)
            {
                case 4:int soldierId=selection.GetAwardId(4);return (Array.Find(JsonUtility.FromJson<Skins>(soldiers()).Datas,x=>x.id==soldierId).iconName,"SkinAndToolIcon");
                case 5:int sceneId=selection.GetAwardId(5);return (Array.Find(JsonUtility.FromJson<Skins>(scenes()).Datas,x=>x.id==sceneId).iconName,"SceneSkin");
                default:return ("","");
            }
        }
        public void SetImportedSprite(Image image,string name,string atlas)
        {
            var sprite=Resources.Load<Sprite>(paths[atlas+"/"+name]);
            if(!sprite)throw new InvalidOperationException("Missing recovered activity sprite: "+atlas+"/"+name);
            image.sprite=sprite;
        }
    }
}
