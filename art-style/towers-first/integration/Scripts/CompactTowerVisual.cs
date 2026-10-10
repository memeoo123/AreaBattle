using System;
using UnityEngine;
namespace AreaBattle
{
    // Evolution presentation only. No simulation, colliders, or save state is changed.
    public sealed class CompactTowerVisual : MonoBehaviour
    {
        static Sprite[] sprites;
        static Material material;
        static float referenceWidth;
        MaterialPropertyBlock properties;
        Vector3 groundAnchor;
        bool initialized;
        const float Width=.2616f;
        public static Sprite ForTower(TowerState tower)
        {
            EnsureAssets();return sprites[tower.IsArrow?3:tower.Specialization==TowerSpecialization.Single?1:tower.Specialization==TowerSpecialization.Split?2:0];
        }
        static Sprite Slice(Texture2D tex,int x0,int x1,string name)
        {
            int minX=x1,minY=tex.height,maxX=-1,maxY=-1;
            var pixels=tex.GetPixels32();
            for(int y=0;y<tex.height;y++)for(int x=x0;x<x1;x++)if(pixels[y*tex.width+x].a>32)
            {minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}
            if(maxX<minX)throw new InvalidOperationException("Empty tower sprite: "+name);
            var sprite=Sprite.Create(tex,new Rect(minX,minY,maxX-minX+1,maxY-minY+1),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);sprite.name=name;return sprite;
        }
        static void EnsureAssets()
        {
            if(sprites!=null&&sprites[0]!=null&&material!=null)return;
            var sheet=Resources.Load<Texture2D>("ArtStyles/Compact/compact-towers");
            var basic=Resources.Load<Texture2D>("ArtStyles/Compact/basic-plain");
            material=Resources.Load<Material>("ArtStyles/Compact/TowerCamp");
            if(sheet==null||basic==null||material==null)throw new InvalidOperationException("Compact tower resources are missing");
            // Original first cell is used only to retain the approved relative scale of advanced towers.
            var calibration=Slice(sheet,0,sheet.width/4,"scale reference");referenceWidth=calibration.bounds.size.x;
            if(Application.isPlaying)Destroy(calibration);else DestroyImmediate(calibration);
            sprites=new[]{Slice(basic,0,basic.width,"基础塔·简朴"),Slice(sheet,sheet.width/4,sheet.width/2,"突击塔"),Slice(sheet,sheet.width/2,sheet.width*3/4,"分流塔"),Slice(sheet,sheet.width*3/4,sheet.width,"箭塔")};
        }
        public void Synchronize(TowerState tower,SpriteRenderer renderer,Camera camera)
        {
            if(!initialized)
            {
                // Existing recovered sprites include padding. Their visible bottom is near the tower's ground point.
                groundAnchor=transform.position+new Vector3(0f,.01f,0f);
                properties=new MaterialPropertyBlock();initialized=true;
            }
            var sprite=ForTower(tower);bool basic=sprite==sprites[0];
            float scale=Mathf.Min(Width/referenceWidth,Width/sprite.bounds.size.x,Width*(basic?1f:1.25f)/sprite.bounds.size.y);
            renderer.sprite=sprite;renderer.sharedMaterial=material;renderer.color=Color.white;
            renderer.transform.rotation=camera.transform.rotation;renderer.transform.localScale=Vector3.one*scale;
            renderer.transform.position=groundAnchor;
            renderer.sortingOrder=0;
            renderer.GetPropertyBlock(properties);properties.SetColor("_CampColor",BattleView.CampColor(tower.Camp));renderer.SetPropertyBlock(properties);
        }
    }
}
