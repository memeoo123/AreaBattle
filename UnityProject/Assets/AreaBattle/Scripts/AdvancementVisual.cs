using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace AreaBattle
{
    // Small code-native equipment silhouettes complement the recovered animated artwork.
    // Camp color stays on the original body; equipment shape communicates the route.
    public sealed class AdvancementVisual : MonoBehaviour
    {
        GameObject artwork;
        Mesh mesh;
        Material material;
        int signature = -1;
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<int> triangles = new List<int>();
        readonly List<Color> colors = new List<Color>();

        public void Synchronize(TowerSpecialization route, int doctrine, int tier, int focus, int camp, Vector3 position, Camera camera, bool soldier)
        {
            if(route==TowerSpecialization.None){if(artwork!=null)artwork.SetActive(false);return;}
            if(artwork==null)
            {
                artwork=new GameObject("Route equipment",typeof(MeshFilter),typeof(MeshRenderer));artwork.transform.SetParent(transform,false);
                mesh=new Mesh{name="Route equipment silhouette"};artwork.GetComponent<MeshFilter>().sharedMesh=mesh;
                material=new Material(Shader.Find("Sprites/Default"));var renderer=artwork.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.sortingOrder=30;
            }
            artwork.SetActive(true);
            int key=(int)route*100000+(doctrine+1)*1000+tier*100+focus*10+camp;
            if(key!=signature){signature=key;Build(route,doctrine,tier,focus,camp,soldier);}
            artwork.transform.rotation=camera.transform.rotation;
            float size=soldier?.043f:.105f;
            var parentScale=transform.lossyScale;
            artwork.transform.localScale=new Vector3(size/Mathf.Max(.001f,Mathf.Abs(parentScale.x)),size/Mathf.Max(.001f,Mathf.Abs(parentScale.y)),size/Mathf.Max(.001f,Mathf.Abs(parentScale.z)));
            artwork.transform.position=position+Vector3.up*(soldier?.085f:.16f)+camera.transform.right*(soldier?.024f:.20f)-camera.transform.forward*.08f;
        }
        void Polygon(Color color, params Vector2[] points)
        {
            int start=vertices.Count;
            foreach(var point in points){vertices.Add(new Vector3(point.x,point.y,-start*.0001f));colors.Add(color);}
            for(int i=1;i<points.Length-1;i++){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}
        }
        void Shape(Color color, params Vector2[] points)
        {
            Vector2 center=Vector2.zero;foreach(var point in points)center+=point;center/=points.Length;
            var outline=new Vector2[points.Length];for(int i=0;i<points.Length;i++)outline[i]=center+(points[i]-center)*1.18f;
            Polygon(new Color(.12f,.15f,.19f),outline);Polygon(color,points);
        }
        void Box(Color c,float x,float y,float w,float h)
        {Shape(c,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));}
        void Build(TowerSpecialization route,int doctrine,int tier,int focus,int camp,bool soldier)
        {
            vertices.Clear();triangles.Clear();colors.Clear();mesh.Clear();
            Color steel=new Color(.84f,.88f,.91f),gold=new Color(1f,.77f,.30f),body=BattleView.CampColor(camp);
            int variant=doctrine<0?-1:doctrine%3;
            if(route==TowerSpecialization.Single)
            {
                Box(body,-.12f,-.75f,.24f,1.2f);
                Shape(steel,new Vector2(-.55f,.35f),new Vector2(.55f,.35f),new Vector2(0,1f));
            }
            else if(route==TowerSpecialization.Split)
            {
                int count=doctrine==3||(doctrine<0&&tier==1)?2:3;
                Box(body,-.12f,-.8f,.24f,.75f);Box(steel,-.7f,-.12f,1.4f,.2f);
                for(int i=0;i<count;i++){
                    float x=count==2?-.65f+i*1.3f:-.65f+i*.65f;
                    Box(body,x-.08f,0,.16f,.4f);
                    Shape(steel,new Vector2(x-.28f,.35f),new Vector2(x+.28f,.35f),new Vector2(x,.8f));
                }
            }
            else if(route==TowerSpecialization.Arrow)
            {
                Box(body,-.55f,-.12f,1.1f,.24f);Box(steel,-.08f,-.65f,.16f,1.2f);
                Shape(gold,new Vector2(-.32f,.4f),new Vector2(.32f,.4f),new Vector2(0,.95f));
            }
            else
            {
                for(int i=0;i<2;i++){
                    float y=i*.7f-.65f;
                    Shape(steel,new Vector2(-.65f,y),new Vector2(0,y+.65f),new Vector2(.65f,y),new Vector2(.35f,y-.15f),new Vector2(0,y+.2f),new Vector2(-.35f,y-.15f));
                }
                Box(body,-.08f,-.85f,.16f,.55f);
            }
            if(tier>=3)Box(gold,-.65f,-.95f,1.3f,.12f);
            // Rank pips make later evolution visible even when the numeric score falls.
            if(!soldier)for(int i=0;i<tier;i++)Box(gold,-.75f+i*.3f,-1.12f,.18f,.18f);
            else if(tier>=4)Box(gold,-.6f,-.9f,1.2f,.14f);
            mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
        }
        void OnDestroy()
        {
            if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}
            if(material!=null){if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
        }
    }
}
