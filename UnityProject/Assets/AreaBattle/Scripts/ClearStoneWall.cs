using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Rebuild only presentation, inside each original box collider's local envelope.
    public sealed class ClearStoneWall : MonoBehaviour
    {
        Mesh mesh;
        public void Build(GameObject visual, Material material)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
            var boxes=visual.transform.parent.GetComponentsInChildren<BoxCollider>(true);
            int segment=0;
            foreach(var box in boxes){
                if(!box.enabled||!box.gameObject.activeInHierarchy)continue;
                var size=box.size;var basePoint=box.center-Vector3.up*size.y*.5f;
                var matrix=visual.transform.worldToLocalMatrix*box.transform.localToWorldMatrix;
                // Two staggered courses, broad coping and sparse large merlons.
                float width=size.x*.94f,length=size.z,gap=.008f;
                // Recessed dark stone core keeps mortar gaps solid instead of exposing ground.
                AddBlock(vertices,triangles,colors,matrix,basePoint+Vector3.up*(size.y*.34f),new Vector3(width*.94f,size.y*.66f,length*.996f),.006f,.66f);
                for(int row=0;row<2;row++){
                    float[] cuts=row==0?new[]{-.5f,0f,.5f}:new[]{-.5f,-.25f,.25f,.5f};
                    for(int j=0;j<cuts.Length-1;j++){
                        float z=(cuts[j]+cuts[j+1])*.5f*length;
                        var center=basePoint+new Vector3(0,size.y*(.18f+row*.33f),z);
                        var dims=new Vector3(width,size.y*.33f-gap,(cuts[j+1]-cuts[j])*length-gap);
                        AddBlock(vertices,triangles,colors,matrix,center,dims,.014f,1f-.025f*((segment+row+j)%3));
                    }
                }
                AddBlock(vertices,triangles,colors,matrix,basePoint+new Vector3(0,size.y*.735f,0),new Vector3(size.x*.98f,size.y*.14f,length-.004f),.012f,1.035f);
                if(segment%2==0||boxes.Length==1)
                    AddBlock(vertices,triangles,colors,matrix,basePoint+new Vector3(0,size.y*.90f,0),new Vector3(width,size.y*.20f,length*.56f),.015f,1.02f);
                segment++;
            }
            mesh=new Mesh{name="Clear stone wall beveled blocks"};
            mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var child=new GameObject("Clear stone blocks");child.transform.SetParent(visual.transform,false);
            child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        static void AddBlock(List<Vector3> verts,List<int> tris,List<Color> colors,Matrix4x4 matrix,Vector3 center,Vector3 size,float bevel,float tint)
        {
            var h=size*.5f;float b=Mathf.Min(bevel,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.35f);
            // Central faces, edge bevels, and triangular corner bevels form a closed solid.
            for(int axis=0;axis<3;axis++)for(int sign=-1;sign<=1;sign+=2){
                int u=(axis+1)%3,v=(axis+2)%3;var points=new List<Vector3>();
                float[] us={-h[u]+b,h[u]-b,h[u],h[u],h[u]-b,-h[u]+b,-h[u],-h[u]};
                float[] vs={-h[v],-h[v],-h[v]+b,h[v]-b,h[v],h[v],h[v]-b,-h[v]+b};
                for(int i=0;i<8;i++){var p=Vector3.zero;p[axis]=sign*h[axis];p[u]=us[i];p[v]=vs[i];points.Add(p);}
                Face(verts,tris,colors,matrix,center,points,tint);
            }
            for(int axis=0;axis<3;axis++)for(int su=-1;su<=1;su+=2)for(int sv=-1;sv<=1;sv+=2){
                int u=(axis+1)%3,v=(axis+2)%3;var points=new List<Vector3>();
                for(int i=0;i<4;i++){var p=Vector3.zero;p[axis]=(i<2?-1:1)*(h[axis]-b);p[u]=su*(h[u]-(i==1||i==2?b:0));p[v]=sv*(h[v]-(i==0||i==3?b:0));points.Add(p);}
                Face(verts,tris,colors,matrix,center,points,tint);
            }
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){
                var points=new List<Vector3>();for(int axis=0;axis<3;axis++){var p=new Vector3(x*(h.x-b),y*(h.y-b),z*(h.z-b));p[axis]=axis==0?x*h.x:axis==1?y*h.y:z*h.z;points.Add(p);}Face(verts,tris,colors,matrix,center,points,tint);
            }
        }
        static void Face(List<Vector3> verts,List<int> tris,List<Color> colors,Matrix4x4 matrix,Vector3 center,List<Vector3> points,float tint)
        {
            var normal=Vector3.Cross(points[1]-points[0],points[2]-points[0]);var outward=Vector3.zero;foreach(var p in points)outward+=p;
            if(Vector3.Dot(normal,outward)<0)points.Reverse();int start=verts.Count;
            foreach(var p in points){verts.Add(matrix.MultiplyPoint3x4(center+p));colors.Add(new Color(tint,tint,tint,1));}
            for(int i=1;i<points.Count-1;i++){tris.Add(start);tris.Add(start+i);tris.Add(start+i+1);}
        }
        void OnDestroy(){if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
    }
}
