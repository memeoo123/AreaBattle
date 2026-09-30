using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AreaBattle.EditorTools
{
    public static class RecoveredWayLineImporter
    {
        const string Destination="Assets/AreaBattle/Resources/Recovered/WayLines";
        [Serializable] class Manifest {public float width,moveSpeed,tilingPerUnit;public Color[] campColors;public TextureSource[] textures;public MaterialSource[] materials;public PrefabSource[] prefabs;}
        [Serializable] class TextureSource {public string id,name,path,sha256;public int width,height;public bool mipmap,srgb;public TextureSettings settings;}
        [Serializable] class TextureSettings {public int m_FilterMode,m_Aniso,m_WrapU,m_WrapV,m_WrapW;public float m_MipBias;}
        [Serializable] class MaterialSource {public string id,name,textureId;public Vector2 scale,offset;public float Vspeed,Uspeed,opacity;public Color color;}
        [Serializable] class PrefabSource {public string name,sourceId;public int layer;public RendererSource[] renderers;}
        [Serializable] class RendererSource {public string name,materialId;public bool active,enabled,receiveShadows,worldSpace,loop;public int layer,sortingOrder,sortingLayerID,castShadows;public Vector3[] positions;public TransformSource transform;public Parameters parameters;}
        [Serializable] class TransformSource {public Vector3 m_LocalPosition,m_LocalScale;public Quaternion m_LocalRotation;}
        [Serializable] class Parameters {public float widthMultiplier,shadowBias;public Curve widthCurve;public GradientSource colorGradient;public int numCornerVertices,numCapVertices,alignment,textureMode;public bool generateLightingData;}
        [Serializable] class Curve {public Key[] m_Curve;public int m_PreInfinity,m_PostInfinity;}
        [Serializable] class Key {public float time,value,inSlope,outSlope,inWeight,outWeight;public int weightedMode;}
        [Serializable] class GradientSource {public Color key0,key1;public int ctime0,ctime1,atime0,atime1,m_Mode;}

        [MenuItem("AreaBattle/Import recovered WayLine")]
        public static void Import()
        {
            string target=Path.Combine(BattleBuild.Workspace,"analysis/targets/wxcf1394487200e48f/43");
            var source=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,"generated/wayline-runtime-data.json")));
            Directory.CreateDirectory(Destination);
            string shaderPath=Destination+"/RecoveredWayLine.shader";
            File.WriteAllText(shaderPath,ShaderSource);AssetDatabase.ImportAsset(shaderPath,ImportAssetOptions.ForceSynchronousImport);
            var shader=AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if(shader==null)throw new InvalidDataException("Recovered WayLine shader failed import.");
            var textures=new Dictionary<string,Texture2D>();
            foreach(var item in source.textures)
            {
                string original=Path.Combine(target,item.path);
                using(var hash=SHA256.Create())if(BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(original))).Replace("-","").ToLowerInvariant()!=item.sha256)
                    throw new InvalidDataException("WayLine source texture hash changed: "+item.path);
                string path=Destination+"/"+item.name+".png";File.Copy(original,path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.sRGBTexture=item.srgb;importer.alphaSource=TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency=false;importer.mipmapEnabled=item.mipmap;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=(FilterMode)item.settings.m_FilterMode;
                importer.anisoLevel=item.settings.m_Aniso;importer.mipMapBias=item.settings.m_MipBias;
                importer.wrapModeU=(TextureWrapMode)item.settings.m_WrapU;importer.wrapModeV=(TextureWrapMode)item.settings.m_WrapV;importer.wrapModeW=(TextureWrapMode)item.settings.m_WrapW;
                importer.SaveAndReimport();textures.Add(item.id,AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            }
            var materials=new Dictionary<string,Material>();
            foreach(var item in source.materials)
            {
                string path=Destination+"/"+item.name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(shader);AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
                material.SetTexture("_Tex",textures[item.textureId]);material.SetTextureScale("_Tex",item.scale);material.SetTextureOffset("_Tex",item.offset);
                material.SetFloat("_Vspeed",item.Vspeed);material.SetFloat("_Uspeed",item.Uspeed);material.SetFloat("_opacity",item.opacity);
                material.SetFloat("_BattleVisualTime",-1);material.SetColor("_color",item.color);EditorUtility.SetDirty(material);materials.Add(item.id,material);
            }
            foreach(var item in source.prefabs)
            {
                var root=new GameObject(item.name){layer=item.layer};
                try
                {
                    var visual=root.AddComponent<RecoveredWayLineVisual>();visual.SourceWidth=source.width;visual.SourceMoveSpeed=source.moveSpeed;visual.TilingPerUnit=source.tilingPerUnit;
                    visual.CampColors=source.campColors;
                    visual.CutCollider=root.AddComponent<BoxCollider>();visual.CutCollider.size=new Vector3(.1f,1,1);
                    foreach(var r in item.renderers)
                    {
                        var obj=new GameObject(r.name){layer=r.layer};obj.transform.SetParent(root.transform,false);obj.SetActive(r.active);
                        obj.transform.localPosition=r.transform.m_LocalPosition;obj.transform.localRotation=r.transform.m_LocalRotation;obj.transform.localScale=r.transform.m_LocalScale;
                        var lr=obj.AddComponent<LineRenderer>();lr.enabled=r.enabled;lr.sharedMaterial=materials[r.materialId];lr.useWorldSpace=r.worldSpace;lr.loop=r.loop;
                        lr.positionCount=r.positions.Length;lr.SetPositions(r.positions);lr.sortingLayerID=r.sortingLayerID;lr.sortingOrder=r.sortingOrder;
                        lr.shadowCastingMode=(ShadowCastingMode)r.castShadows;lr.receiveShadows=r.receiveShadows;lr.lightProbeUsage=LightProbeUsage.Off;lr.reflectionProbeUsage=ReflectionProbeUsage.Off;
                        lr.widthMultiplier=r.parameters.widthMultiplier;
                        var curve=r.parameters.widthCurve;
                        lr.widthCurve=new AnimationCurve(curve.m_Curve.Select(k=>new Keyframe(k.time,k.value,k.inSlope,k.outSlope,k.inWeight,k.outWeight){weightedMode=(WeightedMode)k.weightedMode}).ToArray())
                            {preWrapMode=(WrapMode)curve.m_PreInfinity,postWrapMode=(WrapMode)curve.m_PostInfinity};
                        var g=r.parameters.colorGradient;var gradient=new Gradient{mode=(GradientMode)g.m_Mode};
                        gradient.SetKeys(new[]{new GradientColorKey(g.key0,g.ctime0/65535f),new GradientColorKey(g.key1,g.ctime1/65535f)},new[]{new GradientAlphaKey(g.key0.a,g.atime0/65535f),new GradientAlphaKey(g.key1.a,g.atime1/65535f)});lr.colorGradient=gradient;
                        lr.numCornerVertices=r.parameters.numCornerVertices;lr.numCapVertices=r.parameters.numCapVertices;lr.alignment=(LineAlignment)r.parameters.alignment;
                        lr.textureMode=(LineTextureMode)r.parameters.textureMode;lr.shadowBias=r.parameters.shadowBias;lr.generateLightingData=r.parameters.generateLightingData;
                        if(r.name=="renderer_arrow_1")visual.Arrow1=lr;else if(r.name=="renderer_arrow_2")visual.Arrow2=lr;
                        else if(r.name=="renderer_bg_1")visual.Background1=lr;else if(r.name=="renderer_bg_2")visual.Background2=lr;
                    }
                    PrefabUtility.SaveAsPrefabAsset(root,Destination+"/"+item.name+".prefab");
                }finally{UnityEngine.Object.DestroyImmediate(root);}
            }
            AssetDatabase.SaveAssets();
        }

        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original geometry/texture/shader equations checked; matched original-frame rendering remains pending."};
            Action<string,Action> check=(id,fn)=>{try{fn();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.ToString()});}};
            check("wayline-shader-import",()=>{var s=Shader.Find("AreaBattle/Recovered WayLine");Require(s!=null,"shader");Require(!ShaderUtil.GetShaderMessages(s).Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),"compile");});
            CheckPrefab(check,"wayline-source-single",false,v=>{
                v.SetLine(Vector3.zero,Vector3.right,Color.red,Color.blue,true,true,2);
                Require(v.Arrow1.GetPosition(0)==new Vector3(0,.021f,0)&&v.Arrow1.GetPosition(1)==new Vector3(1,.021f,0),"positions");
                Require(Mathf.Abs(v.Arrow1.startWidth-.1f)<1e-6f&&v.Arrow1.widthCurve.length==1,"original width setter");
                Require(v.Arrow1.material.GetTextureScale("_Tex")==new Vector2(16,1)&&!v.Background1.enabled,"tiling/bg");
            });
            CheckPrefab(check,"wayline-source-double",true,v=>{
                v.SetLine(Vector3.zero,Vector3.forward*4,Color.red,Color.blue,true,true,0);
                Require(v.Arrow1.GetPosition(1)==new Vector3(0,.021f,2)&&v.Arrow2.GetPosition(0)==new Vector3(0,.021f,4)&&v.Arrow2.GetPosition(1)==new Vector3(0,.021f,2),"half-line geometry");
                Require(v.Arrow1.material.GetTextureScale("_Tex")==new Vector2(32,1)&&v.Arrow2.material.GetTextureScale("_Tex")==new Vector2(32,1),"half tiling");
                Require(v.transform.position==new Vector3(0,.02f,2)&&v.CutCollider.size==new Vector3(.1f,.1f,4),"cut collider");
            });
            CheckPrefab(check,"wayline-reversed-direction",false,v=>{
                var a=new TowerState{Position=Vector3.zero,Camp=1};var b=new TowerState{Position=Vector3.right,Camp=2};
                v.Synchronize(new LineState{Direction=2},a,b,Color.red,Color.blue,0);
                Require(v.Arrow1.GetPosition(0)==new Vector3(1,.021f,0),"reversed start");
                var block=new MaterialPropertyBlock();v.Arrow1.GetPropertyBlock(block);Require(block.GetColor("_color")==Color.blue,"source camp color");
            });
            CheckPrefab(check,"wayline-preview-bg",false,v=>{v.SetLine(Vector3.zero,Vector3.right,Color.red,Color.blue,false,false,0);Require(!v.Arrow1.enabled&&v.Background1.enabled&&!v.CutCollider.enabled,"preview flags");Require(v.Background1.GetPosition(0)==new Vector3(0,.02f,0),"preview height");});
            return report;
        }
        static void CheckPrefab(Action<string,Action> check,string id,bool dual,Action<RecoveredWayLineVisual> test)
        {check(id,()=>{var prefab=Resources.Load<GameObject>("Recovered/WayLines/LineRander_"+(dual?"double":"single"));Require(prefab!=null,"prefab");var obj=UnityEngine.Object.Instantiate(prefab);try{test(obj.GetComponent<RecoveredWayLineVisual>());}finally{UnityEngine.Object.DestroyImmediate(obj);}});}
        static void Require(bool valid,string message){if(!valid)throw new Exception(message);}

        // Direct translation of the extracted GLES2 program, including its signed time modulo.
        const string ShaderSource=@"Shader ""AreaBattle/Recovered WayLine"" {
Properties {
 _Tex (""Tex"",2D)=""white"" {} _Vspeed (""Vspeed"",Float)=0 _Uspeed (""Uspeed"",Float)=0
 [HDR] _color (""color"",Color)=(1,1,1,1) _opacity (""opacity"",Range(0,5))=3
 _BattleVisualTime (""Host scaled clock (-1 uses Unity time)"",Float)=-1
}
SubShader { Tags { ""IgnoreProjector""=""True"" ""Queue""=""Transparent"" ""RenderType""=""Transparent"" }
 Pass { Name ""FORWARD"" Tags { ""LightMode""=""ForwardBase"" } Cull Off ZWrite Off ZTest LEqual
 Blend SrcAlpha OneMinusSrcAlpha
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include ""UnityCG.cginc""
 struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
 struct v2f { float4 pos:SV_POSITION; half2 uv:TEXCOORD0; };
 sampler2D _Tex; float4 _Tex_ST; half4 _color; half _Vspeed,_Uspeed,_opacity; float _BattleVisualTime;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 half4 frag(v2f i):SV_Target {
  float clock=_BattleVisualTime<0?_Time.y:_BattleVisualTime;
  float t=clock*4.99999987e-06; t=sign(t)*frac(abs(t))*200000.0;
  float2 uv=(i.uv+t*float2(_Vspeed,_Uspeed))*_Tex_ST.xy+_Tex_ST.zw;
  half4 c=tex2D(_Tex,uv);return half4(c.rgb*_color.rgb,saturate(c.a*_opacity));
 }
 ENDCG
 }
}
}";
    }
}
