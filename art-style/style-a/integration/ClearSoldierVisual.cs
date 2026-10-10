using System;
using UnityEngine;
namespace AreaBattle
{
    // One ordinary infantry sample. All animation uses the paused battle clock.
    public sealed class ClearSoldierVisual : MonoBehaviour
    {
        static Sprite[] frames;
        static Material material;
        static float scale;
        SpriteRenderer target;
        MaterialPropertyBlock properties;
        float started, deathStarted;
        bool dying;
        Vector3 deathPosition;
        int deathFrame;
        const float DeathSeconds=.65f;
        public bool DeathFinished(float clock)=>dying&&clock-deathStarted>=DeathSeconds;
        public void Begin(float clock)
        {
            EnsureAssets();started=clock;
            target=gameObject.AddComponent<SpriteRenderer>();target.sharedMaterial=material;
            target.sortingOrder=1;properties=new MaterialPropertyBlock();
            transform.localScale=Vector3.one*scale;
        }
        static void EnsureAssets()
        {
            if(frames!=null&&frames[0]!=null&&material!=null)return;
            var tex=Resources.Load<Texture2D>("ArtStyles/ClearA/soldier-run");
            material=Resources.Load<Material>("ArtStyles/Compact/TowerCamp");
            if(tex==null||material==null)throw new InvalidOperationException("Clear infantry resources missing");
            var pixels=tex.GetPixels32();frames=new Sprite[4];float maxHeight=0;
            for(int i=0;i<4;i++){
                int left=i*tex.width/4,right=(i+1)*tex.width/4,minX=right,maxX=-1,minY=tex.height,maxY=-1;
                for(int y=0;y<tex.height;y++)for(int x=left;x<right;x++)if(pixels[y*tex.width+x].a>32){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
                if(maxX<minX)throw new InvalidOperationException("Empty infantry frame");
                frames[i]=Sprite.Create(tex,new Rect(minX,minY,maxX-minX+1,maxY-minY+1),new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
                frames[i].name="Clear infantry run "+i;maxHeight=Mathf.Max(maxHeight,frames[i].bounds.size.y);
            }
            // ~21px at 540x960: 20% larger than the initial infantry sample.
            scale=.09f/maxHeight;
        }
        public void BeginDeath(float clock)
        {
            if(dying)return;
            dying=true;deathStarted=clock;deathPosition=transform.position;
            deathFrame=Mathf.FloorToInt(Mathf.Max(0,clock-started)/.15f)%4;
        }
        public void Synchronize(SoldierState soldier,Camera camera,float clock)
        {
            if(target==null)Begin(clock);
            float t=dying?Mathf.Clamp01((clock-deathStarted)/DeathSeconds):0;
            target.sprite=frames[dying?deathFrame:Mathf.FloorToInt(Mathf.Max(0,clock-started)/.15f)%4];
            target.flipX=soldier.LegEnd.x<soldier.LegStart.x;
            transform.position=dying?deathPosition:soldier.Position+Vector3.up*.006f;
            // A short fall and fade has a defined end, without changing combat/death timing.
            transform.rotation=camera.transform.rotation*Quaternion.Euler(0,0,(target.flipX?1:-1)*80*Mathf.SmoothStep(0,1,t));
            target.color=new Color(1,1,1,1-t);
            properties.SetColor("_CampColor",BattleView.CampColor(soldier.Camp));target.SetPropertyBlock(properties);
        }
    }
}
