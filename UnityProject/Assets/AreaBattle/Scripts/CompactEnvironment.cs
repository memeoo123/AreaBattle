using System;
using UnityEngine;
namespace AreaBattle
{
    // Screen-filling visual ground only. Gameplay remains on the original world plane.
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public sealed class CompactEnvironment : MonoBehaviour
    {
        Camera target;
        SpriteRenderer ground;
        Sprite sprite;
        public void Configure()
        {
            target=GetComponent<Camera>();
            var texture=Resources.Load<Texture2D>("ArtStyles/ClearA/ground");
            if(texture==null)throw new InvalidOperationException("Compact ground missing");
            var go=new GameObject("Compact battlefield ground");go.transform.SetParent(transform,false);
            go.transform.localPosition=new Vector3(0,0,50);ground=go.AddComponent<SpriteRenderer>();
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);
            ground.sprite=sprite;ground.sortingOrder=-1000;FitToCamera();
        }
        void OnPreCull(){FitToCamera();}
        public void FitToCamera()
        {
            if(ground==null||target==null)return;
            float height=2*target.orthographicSize;
            float scale=Mathf.Max(height/sprite.bounds.size.y,height*target.aspect/sprite.bounds.size.x);
            ground.transform.localScale=Vector3.one*scale;
        }
        void OnDestroy(){if(sprite!=null){if(Application.isPlaying)Destroy(sprite);else DestroyImmediate(sprite);}}
        public static void RestyleWall(GameObject visual)
        {
            if(visual.GetComponent<ClearStoneWall>()!=null)return;
            var mat=Resources.Load<Material>("ArtStyles/Compact/WarmStoneWall");
            if(mat==null)throw new InvalidOperationException("Compact wall material missing");
            foreach(var renderer in visual.GetComponentsInChildren<MeshRenderer>(true)){
                var materials=renderer.sharedMaterials;for(int i=0;i<materials.Length;i++)materials[i]=mat;renderer.sharedMaterials=materials;renderer.enabled=false;
            }
            visual.AddComponent<ClearStoneWall>().Build(visual,mat);
        }
    }
}
