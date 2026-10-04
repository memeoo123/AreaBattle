using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AreaBattle.EditorTools
{
    // Creates editable UGUI objects from serialized source fields. No original MonoBehaviours execute.
    public static class RecoveredHudImporter
    {
        static string Destination="Assets/AreaBattle/Resources/Recovered/Hud";
        static string manifestRelative="generated/presentation-prepared/hud-import.json";
        static string target,workspace;
        static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
        static readonly Dictionary<string,Font> fonts=new Dictionary<string,Font>();
        static readonly List<string> skipped=new List<string>();
        [Serializable] class Manifest { public Prefab[] prefabs;public SpriteSource[] sprites;public FontSource[] fonts; }
        [Serializable] class Prefab { public string name,rootPath;public Node[] nodes; }
        [Serializable] class Node { public string path,name,sourceId,transformSourceId;public bool active;public RectData transform;public ComponentSource[] components; }
        [Serializable] class RectData { public Vector3 m_LocalPosition,m_LocalScale;public Quaternion m_LocalRotation;public Vector2 m_AnchorMin,m_AnchorMax,m_AnchoredPosition,m_SizeDelta,m_Pivot; }
        [Serializable] class ComponentSource { public string className,sourceId,data,spriteId,fontId,materialId,targetGraphicId,fillRectId,handleRectId; }
        [Serializable] class SpriteSource { public string id,name,path,sha256;public Vector2 pivot;public Vector4 border;public float ppu; }
        [Serializable] class FontSource { public string id,name,path,sha256,fallbackPath,fallbackSourceId; }
        [Serializable] class FontData { public int m_FontSize,m_FontStyle,m_MinSize,m_MaxSize,m_Alignment,m_HorizontalOverflow,m_VerticalOverflow;public bool m_BestFit,m_AlignByGeometry,m_RichText;public float m_LineSpacing; }
        [Serializable] class Padding { public int m_Left,m_Right,m_Top,m_Bottom; }
        [Serializable] class Colors { public Color m_NormalColor,m_HighlightedColor,m_PressedColor,m_SelectedColor,m_DisabledColor;public float m_ColorMultiplier,m_FadeDuration; }
        [Serializable] class NavigationData { public int m_Mode;public bool m_WrapAround; }
        [Serializable] class GridData {public Padding m_Padding;public int m_ChildAlignment,m_StartCorner,m_StartAxis,m_Constraint,m_ConstraintCount,m_Enabled=1;public Vector2 m_CellSize,m_Spacing;}
        [Serializable] class ToggleCall {public ObjectPointer m_Target;public string m_MethodName;public int m_Mode,m_CallState;}
        [Serializable] class ToggleCalls {public ToggleCall[] m_Calls;}
        [Serializable] class ToggleEvent {public ToggleCalls m_PersistentCalls;}
        [Serializable] class ToggleData {public ObjectPointer graphic,m_Group;public ToggleEvent onValueChanged;public int m_AllowSwitchOff;}
        [Serializable] class Data
        {
            public int videoID,AutoCallBtnShow=1;public ToggleEvent m_OnClick;public AnimationTriggers m_AnimationTriggers;
            public int m_Enabled=1,m_Type,m_FillMethod,m_FillOrigin,m_ChildAlignment,m_StartCorner,m_StartAxis,m_Constraint,m_ConstraintCount;
            public bool m_RaycastTarget,m_Maskable=true,m_PreserveAspect,m_FillCenter=true,m_FillClockwise=true,m_UseSpriteMesh,m_UseGraphicAlpha=true,m_Interactable=true;
            public bool m_ChildForceExpandWidth,m_ChildForceExpandHeight,m_ChildControlWidth,m_ChildControlHeight,m_ChildScaleWidth,m_ChildScaleHeight,m_ReverseArrangement;
            public float m_FillAmount=1,m_PixelsPerUnitMultiplier=1,m_Spacing;
            public Color m_Color=Color.white,m_EffectColor;public Vector2 m_EffectDistance,m_CellSize;
            public string m_Text;public FontData m_FontData;public Padding m_Padding;public Colors m_Colors;
            public bool m_IsOn;public int toggleTransition;public int m_Transition,m_Direction;public ObjectPointer m_TargetGraphic,m_FillRect,m_HandleRect;
            public float m_MinValue,m_MaxValue=1,m_Value;public bool m_WholeNumbers;public NavigationData m_Navigation;
        }
        [Serializable] class CanvasData
        {
            public int m_Enabled,m_RenderMode,m_AdditionalShaderChannelsFlag,m_SortingLayerID,m_SortingOrder,m_TargetDisplay;
            public float m_PlaneDistance,m_SortingBucketNormalizedSize;
            public bool m_PixelPerfect,m_ReceivesEvents,m_OverrideSorting,m_OverridePixelPerfect,m_VertexColorAlwaysGammaSpace;
            public ObjectPointer m_Camera;
        }
        [Serializable] class RaycastData { public int m_Enabled,m_IgnoreReversedGraphics,m_BlockingObjects;public MaskBits m_BlockingMask; }
        [Serializable] class MaskBits { public int m_Bits; }
        [Serializable] class BangsData {public int IsNeedAdaptiveBangs,IsDoubleEnded,m_Enabled;}
        [Serializable] class RendererData { public bool m_CullTransparentMesh; }
        [Serializable] class ScrollData { public int m_Enabled=1,m_Horizontal,m_Vertical,m_Inertia,m_MovementType,m_HorizontalScrollbarVisibility,m_VerticalScrollbarVisibility; public float m_Elasticity,m_DecelerationRate,m_ScrollSensitivity,m_HorizontalScrollbarSpacing,m_VerticalScrollbarSpacing;public ObjectPointer m_Content,m_Viewport,m_HorizontalScrollbar,m_VerticalScrollbar; }
        [Serializable] class FitData { public int m_Enabled=1,m_HorizontalFit,m_VerticalFit,m_ShowMaskGraphic; }
        [Serializable] class RectMaskData {public int m_Enabled=1;public Vector4 m_Padding;public Vector2Int m_Softness;}
        [Serializable] class ObjectPointer {public int m_FileID;public long m_PathID;}
        [Serializable] class Report {public string status;public int prefabs,sprites,fonts;public string[] skippedComponents;public string limitation;}

        [MenuItem("AreaBattle/Import recovered HUD")]
        public static void Import()=>ImportManifest("generated/presentation-prepared/hud-import.json","Assets/AreaBattle/Resources/Recovered/Hud",7,"analysis/unity-hud-import-report.json");
        [MenuItem("AreaBattle/Import recovered outgame UI")]
        public static void ImportOutgame()=>ImportManifest("generated/outgame/ui-import.json","Assets/AreaBattle/Resources/Recovered/Outgame",4,"analysis/unity-outgame-ui-import-report.json");
        public static void ImportLoadingBatch()
        {
            try{ImportManifest("generated/outgame/loading-ui-import.json","Assets/AreaBattle/Resources/Recovered/Loading",1,"analysis/unity-loading-import-report.json");OutgameLoadingImport.Attach();
                string reportPath=Path.Combine(BattleBuild.Workspace,"analysis/unity-loading-import-report.json");var imported=JsonUtility.FromJson<Report>(File.ReadAllText(reportPath));
                imported.status="imported-source-loading-page-with-runtime-bindings";imported.skippedComponents=new string[0];
                imported.limitation="Original Resources loading-page hierarchy, decoded UGUI images/font/outlets, SpriteRenderer and six-frame looping Animator restored; reachable source loading progress/timeout and parent Canvas sorting adapter attached. Requires Bind before Start. Full UIControl/Main ownership and original matched screenshot acceptance remain pending.";
                File.WriteAllText(reportPath,JsonUtility.ToJson(imported,true));AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportTopInfoBatch()
        {
            try{ImportManifest("generated/outgame/top-info-ui-import.json","Assets/AreaBattle/Resources/Recovered/TopInfo",1,"analysis/unity-top-info-import-report.json");
                string reportPath=Path.Combine(BattleBuild.Workspace,"analysis/unity-top-info-import-report.json");var imported=JsonUtility.FromJson<Report>(File.ReadAllText(reportPath));
                imported.limitation="Original TopInfoUI hierarchy, graphics and source fonts imported. Runtime outlets and part rendering are separate recovered adapters; account/avatar/WXButton/text ellipsis and full page callbacks remain to be connected. No visual-match or complete lobby claim.";
                File.WriteAllText(reportPath,JsonUtility.ToJson(imported,true));AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportOutgameBatch()
        {
            try{ImportOutgame();AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportTaskPanelBatch()
        {
            try{ImportManifest("generated/outgame/task-panel-ui-import.json","Assets/AreaBattle/Resources/Recovered/TaskPanel",2,"analysis/unity-task-panel-import-report.json");
                string path=Path.Combine(workspace,"analysis/unity-task-panel-import-report.json");var report=JsonUtility.FromJson<Report>(File.ReadAllText(path));
                report.limitation="Original task panel and shared task row static UGUI geometry, sprites/fonts/outlets. Task row runtime is separately bound. Full daily/achievement tabs, page lifecycle, preview, source particle/animation effects and original audiovisual matching remain pending.";
                File.WriteAllText(path,JsonUtility.ToJson(report,true));AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportLimitTaskBatch()
        {
            try{ImportManifest("generated/outgame/limit-task-ui-import.json","Assets/AreaBattle/Resources/Recovered/LimitTask",4,"analysis/unity-limit-task-import-report.json");
                string path=Path.Combine(workspace,"analysis/unity-limit-task-import-report.json");var report=JsonUtility.FromJson<Report>(File.ReadAllText(path));
                report.limitation="Original limited-task page and task/day/reward prefabs with source UGUI fields, sprites/fonts/outlets imported. DynamicList and task/accumulator/preview controllers are separate required recovered bindings. Static hierarchy import alone does not claim complete page interactions or original audiovisual matching.";
                File.WriteAllText(path,JsonUtility.ToJson(report,true));AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportGuideBookBatch()
        {
            try{ImportManifest("generated/outgame/guide-book-ui-import.json","Assets/AreaBattle/Resources/Recovered/GuideBook",2,"analysis/unity-guide-book-import-report.json");
                string path=Path.Combine(workspace,"analysis/unity-guide-book-import-report.json");var report=JsonUtility.FromJson<Report>(File.ReadAllText(path));
                report.limitation="Original GuideBookUI and GuideBookItem static UGUI hierarchy, sprite/font payloads and viewport masks imported. DynamicList, tabs, UI lifetime, popup/item controllers and YD_0 Spine binding remain pending; skipped components are explicitly listed. Reward binding is a separate implementation. No full page or visual-match claim.";
                File.WriteAllText(path,JsonUtility.ToJson(report,true));AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportFestActivityBatch()
        {
            try{ImportManifest("generated/outgame/fest-ui-import.json","Assets/AreaBattle/Resources/Recovered/FestActivity",1,"analysis/unity-fest-activity-import-report.json");AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportTipUiBatch()
        {
            try{ImportManifest("generated/outgame/tip-ui-import.json","Assets/AreaBattle/Resources/Recovered/FirstPack/TipUI",1,"analysis/unity-tip-ui-import-report.json");OutgameTipUiAnimationImport.Attach();AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static void ImportUiRootBatch()
        {
            try
            {
                ImportManifest("generated/outgame/ui-root-import.json","Assets/AreaBattle/Resources/Recovered/UiRoot",1,"analysis/unity-ui-root-import-report.json");
                ValidateUiRoot();
                AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static void ValidateUiRoot()
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(target,manifestRelative)));
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Destination+"/UIRoot.prefab");
            if(prefab.GetComponentsInChildren<Transform>(true).Length!=13)throw new InvalidDataException("UIRoot node count");
            int canvases=0,raycasters=0,images=0;
            foreach(var node in manifest.prefabs[0].nodes)
            {
                var t=node.path==""?prefab.transform:prefab.transform.Find(node.path);
                if(t==null||t.gameObject.activeSelf!=node.active)throw new InvalidDataException("UIRoot node state: "+node.path);
                var rt=(RectTransform)t;var d=node.transform;
                if(rt.localScale!=d.m_LocalScale||rt.anchorMin!=d.m_AnchorMin||rt.anchorMax!=d.m_AnchorMax||rt.sizeDelta!=d.m_SizeDelta||rt.pivot!=d.m_Pivot||rt.anchoredPosition!=d.m_AnchoredPosition)
                    throw new InvalidDataException("UIRoot transform: "+node.path);
                foreach(var c in node.components)
                {
                    if(c.className=="Canvas")
                    {
                        canvases++;var source=JsonUtility.FromJson<CanvasData>(c.data);var canvas=t.GetComponent<Canvas>();
                        if(canvas==null||canvas.sortingLayerID!=source.m_SortingLayerID||canvas.sortingOrder!=source.m_SortingOrder||new SerializedObject(canvas).FindProperty("m_OverrideSorting").boolValue!=source.m_OverrideSorting||canvas.renderMode!=(RenderMode)source.m_RenderMode)
                            throw new InvalidDataException("UIRoot Canvas: "+node.path);
                    }
                    if(c.className=="GraphicRaycaster")
                    {
                        raycasters++;var source=JsonUtility.FromJson<RaycastData>(c.data);var ray=t.GetComponent<GraphicRaycaster>();
                        if(ray==null||ray.blockingMask.value!=source.m_BlockingMask.m_Bits||ray.ignoreReversedGraphics!=(source.m_IgnoreReversedGraphics!=0))throw new InvalidDataException("UIRoot raycaster: "+node.path);
                    }
                    if(c.className=="Image")
                    {
                        images++;var source=JsonUtility.FromJson<Data>(c.data);var image=t.GetComponent<Image>();
                        if(image==null||image.color!=source.m_Color||image.raycastTarget!=source.m_RaycastTarget||image.sprite!=null)throw new InvalidDataException("UIRoot mask: "+node.path);
                    }
                }
            }
            if(canvases!=9||raycasters!=8||images!=3)throw new InvalidDataException("UIRoot component coverage");
            File.WriteAllText(Path.Combine(workspace,"analysis/unity-ui-root-validation.json"),"{\"passed\":true,\"nodes\":13,\"canvases\":9,\"raycasters\":8,\"images\":3,\"scope\":\"Reloaded static prefab against source manifest; AdaptiveBangs and production startup pending\"}");
        }
        public static void ImportFlyCurrencyBatch()
        {
            try{ImportManifest("generated/outgame/fly-ui-import.json","Assets/AreaBattle/Resources/Recovered/FlyCurrency",3,"analysis/unity-fly-currency-import-report.json");AssetDatabase.SaveAssets();if(Application.isBatchMode)EditorApplication.Exit(0);}
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        static void ImportManifest(string sourceRelative,string destination,int expectedCount,string reportRelative)
        {
            Destination=destination;manifestRelative=sourceRelative;
            if(sourceRelative!="generated/outgame/ui-root-import.json"&&sourceRelative!="generated/outgame/tip-ui-import.json"&&sourceRelative!="generated/outgame/loading-ui-import.json"&&sourceRelative!="generated/outgame/guide-book-ui-import.json")RecoveredGuideSpineImporter.Import();
            workspace=Directory.GetParent(Path.GetFullPath(Path.Combine(Application.dataPath,".."))).FullName;
            target=Path.Combine(workspace,"analysis/targets/wxcf1394487200e48f/43");
            var source=Path.Combine(target,manifestRelative);
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(source));
            if(manifest.prefabs==null || manifest.prefabs.Length!=expectedCount)throw new InvalidDataException("Unexpected recovered UI prefab count.");
            sprites.Clear();fonts.Clear();skipped.Clear();
            Directory.CreateDirectory(Destination+"/Sprites");Directory.CreateDirectory(Destination+"/Fonts");AssetDatabase.Refresh();
            foreach(var s in manifest.sprites)
            {
                CheckHash(s.path,s.sha256);
                string path=Destination+"/Sprites/"+Safe(s.id)+".png";File.Copy(Path.Combine(target,s.path),path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=s.ppu;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=s.pivot;importer.SetTextureSettings(settings);
                importer.spriteBorder=s.border;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.sRGBTexture=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;
                importer.userData=s.id+" | "+s.path;importer.SaveAndReimport();
                sprites.Add(s.id,AssetDatabase.LoadAssetAtPath<Sprite>(path));
            }
            foreach(var f in manifest.fonts)
            {
                CheckHash(f.path,f.sha256);
                // Retain the original empty font metrics; replacing it with its fallback changes UGUI best-fit sizing.
                string resolved=f.path;
                string path=Destination+"/Fonts/"+Safe(f.id)+".ttf";File.Copy(Path.Combine(target,resolved),path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TrueTypeFontImporter)AssetImporter.GetAtPath(path);
                if(!string.IsNullOrEmpty(f.fallbackPath))
                {
                    string fallbackPath=Destination+"/Fonts/"+Safe(f.fallbackSourceId)+".ttf";
                    File.Copy(Path.Combine(target,f.fallbackPath),fallbackPath,true);AssetDatabase.ImportAsset(fallbackPath,ImportAssetOptions.ForceSynchronousImport);
                    importer.fontReferences=new[]{AssetDatabase.LoadAssetAtPath<Font>(fallbackPath)};
                    importer.fontNames=new[]{"?????","Nowar Warcraft Rounded CN"};
                }
                importer.userData=f.id+" | original empty font metrics and glyph fallback="+f.fallbackSourceId+" | "+resolved;importer.SaveAndReimport();
                var font=AssetDatabase.LoadAssetAtPath<Font>(path);if(font==null)throw new InvalidDataException("Recovered font failed to import: "+resolved);fonts.Add(f.id,font);
            }
            RecoveredUiAnimationImporter.Begin(target,workspace);
            foreach(var p in manifest.prefabs)Build(p);
            RecoveredUiAnimationImporter.Finish();
            File.Copy(source,Destination+"/hud-import.json",true);AssetDatabase.ImportAsset(Destination+"/hud-import.json",ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.Combine(workspace,reportRelative),JsonUtility.ToJson(new Report{status="imported-static-hierarchy",prefabs=manifest.prefabs.Length,sprites=sprites.Count,fonts=fonts.Count,skippedComponents=skipped.Distinct().ToArray(),limitation=sourceRelative=="generated/outgame/tip-ui-import.json"?"Original TipUI static hierarchy, sprites, fonts and outlet paths imported; disabled native animation attachment has a separate validation report. Business callbacks and matched appearance pending.":sourceRelative=="generated/outgame/ui-root-import.json"?"Original 13-node UIRoot, 9 Canvas layers and source AdaptiveBangs component imported; production module/bootstrap and matched runtime appearance remain pending.":sourceRelative=="generated/outgame/fest-ui-import.json"?"Original Valentine activity hierarchy, pixels and localized text imported. Source CanvasGroup fields are unavailable in the exported schema; typography custom scripts, complete activity lifecycle and matched runtime appearance remain unverified.":sourceRelative=="generated/outgame/fly-ui-import.json"?"Original three fly currency prefabs imported with source transforms and sprite identities. Animation and matched runtime appearance remain unverified.":expectedCount==4?"Original outgame static hierarchy imported. Source custom scripts, runtime page binding, effects and matched visual verification remain incomplete.":"Guide skeleton/hand presentation and tower line decorators are source-bound; native Slider fill references restored. Source result default autoplay curves use explicit scaled presentation time, without secondary-clip chaining. defaultNullFont resolves through its original recovered first fallback. Victory normal-claim button is a local continuation adapter labeled 下一关, with no currency claim or advertisements; source graphic and button position retained. Rankings, multiplied rewards and lobby panels are excluded. Runtime appearance requires matched original verification."},true));
        }
        static void Build(Prefab source)
        {
            var nodes=new Dictionary<string,GameObject>();GameObject root=null;
            var graphicSources=new Dictionary<string,Graphic>();var extraGraphics=new List<KeyValuePair<GameObject,ComponentSource>>();
            try
            {
                foreach(var n in source.nodes)
                {
                    var go=new GameObject(n.name,typeof(RectTransform));go.layer=5;nodes.Add(n.path,go);
                    if(n.path=="")root=go;else{int slash=n.path.LastIndexOf('/');string parent=slash<0?"":n.path.Substring(0,slash);go.transform.SetParent(nodes[parent].transform,false);}
                    var rt=(RectTransform)go.transform;var d=n.transform;
                    rt.anchorMin=d.m_AnchorMin;rt.anchorMax=d.m_AnchorMax;rt.pivot=d.m_Pivot;rt.sizeDelta=d.m_SizeDelta;rt.anchoredPosition=d.m_AnchoredPosition;
                    rt.localScale=d.m_LocalScale;rt.localRotation=d.m_LocalRotation;var pos=rt.localPosition;pos.z=d.m_LocalPosition.z;rt.localPosition=pos;
                    foreach(var c in n.components)
                    {
                        bool graphic=c.className=="Image"||c.className=="Text";
                        // Serialized source permits duplicate Graphics; Unity AddComponent rejects them.
                        // Preserve the first owner and defer extra full-rect children until original siblings exist.
                        if(graphic&&go.GetComponent<Graphic>()!=null){extraGraphics.Add(new KeyValuePair<GameObject,ComponentSource>(go,c));continue;}
                        AddComponent(go,c);if(graphic)graphicSources[c.sourceId]=go.GetComponent<Graphic>();
                    }
                    go.SetActive(n.active);
                }
                if(root==null)throw new InvalidDataException(source.name+" root missing");
                // Selectable may tint a sibling graphic (SkillItem.btn_normal targets bg).
                foreach(var entry in extraGraphics)
                {
                    var go=new GameObject("RecoveredGraphic_"+entry.Value.sourceId.Split(':').Last(),typeof(RectTransform));go.layer=entry.Key.layer;
                    var rect=(RectTransform)go.transform;rect.SetParent(entry.Key.transform,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
                    AddComponent(go,entry.Value);graphicSources[entry.Value.sourceId]=go.GetComponent<Graphic>();
                    skipped.Add("Duplicate source Graphic represented by full-rect child | "+entry.Value.sourceId);
                }
                var transformSources=new Dictionary<string,RectTransform>();
                foreach(var n in source.nodes)
                    if(!string.IsNullOrEmpty(n.transformSourceId))transformSources.Add(n.transformSourceId,(RectTransform)nodes[n.path].transform);
                foreach(var n in source.nodes)foreach(var c in n.components)if(c.className=="Button"||c.className=="UIVideoBtn"||c.className=="Slider"||c.className=="Toggle")
                {
                    var data=JsonUtility.FromJson<Data>(c.data);var pointer=data.m_TargetGraphic;
                    if(pointer!=null&&pointer.m_PathID!=0)
                    {
                        string sourceId=c.targetGraphicId;
                        if(string.IsNullOrEmpty(sourceId)&&pointer.m_FileID==0)sourceId=c.sourceId.Substring(0,c.sourceId.LastIndexOf(':')+1)+pointer.m_PathID;
                        if(string.IsNullOrEmpty(sourceId)||!graphicSources.TryGetValue(sourceId,out var graphic))
                            throw new InvalidDataException("Unresolved source selectable targetGraphic: "+c.sourceId+" -> "+sourceId);
                        nodes[n.path].GetComponent<Selectable>().targetGraphic=graphic;
                    }
                    if(c.className=="Slider")
                    {
                        var slider=nodes[n.path].GetComponent<Slider>();
                        slider.fillRect=ResolveRect(c,data.m_FillRect,c.fillRectId,transformSources,"fillRect");
                        slider.handleRect=ResolveRect(c,data.m_HandleRect,c.handleRectId,transformSources,"handleRect");
                        slider.SetValueWithoutNotify(data.m_Value);
                    }
                }
                var sourceObjects=source.nodes.ToDictionary(n=>n.sourceId,n=>nodes[n.path]);
                var sourceGroups=new Dictionary<string,ToggleGroup>();
                foreach(var n in source.nodes)foreach(var c in n.components)if(c.className=="ToggleGroup")sourceGroups.Add(c.sourceId,nodes[n.path].GetComponent<ToggleGroup>());
                foreach(var n in source.nodes)foreach(var c in n.components)if(c.className=="Toggle")
                {
                    var toggle=nodes[n.path].GetComponent<Toggle>();var d=JsonUtility.FromJson<ToggleData>(c.data);
                    string prefix=c.sourceId.Substring(0,c.sourceId.LastIndexOf(':')+1);
                    Func<ObjectPointer,string> key=pointer=>{
                        if(pointer.m_FileID!=0)throw new InvalidDataException("External Toggle reference requires resolution: "+c.sourceId);
                        return prefix+pointer.m_PathID;
                    };
                    if(d.graphic!=null&&d.graphic.m_PathID!=0)toggle.graphic=graphicSources[key(d.graphic)];
                    if(d.m_Group!=null&&d.m_Group.m_PathID!=0)toggle.group=sourceGroups[key(d.m_Group)];
                    if(d.onValueChanged?.m_PersistentCalls?.m_Calls!=null)foreach(var call in d.onValueChanged.m_PersistentCalls.m_Calls)
                    {
                        if(call.m_MethodName!="SetActive"||call.m_Mode!=0)throw new InvalidDataException("Unsupported Toggle persistent call: "+c.sourceId+" "+call.m_MethodName);
                        var target=sourceObjects[key(call.m_Target)];
                        UnityEditor.Events.UnityEventTools.AddPersistentListener<bool>(toggle.onValueChanged,target.SetActive);
                        toggle.onValueChanged.SetPersistentListenerState(toggle.onValueChanged.GetPersistentEventCount()-1,(UnityEngine.Events.UnityEventCallState)call.m_CallState);
                    }
                }
                foreach(var n in source.nodes)foreach(var c in n.components)if(c.className=="ScrollRect")
                {
                    var d=JsonUtility.FromJson<ScrollData>(c.data);var scroll=nodes[n.path].GetComponent<ScrollRect>();
                    scroll.content=ResolveRect(c,d.m_Content,null,transformSources,"content");scroll.viewport=ResolveRect(c,d.m_Viewport,null,transformSources,"viewport");
                    if((d.m_HorizontalScrollbar!=null&&d.m_HorizontalScrollbar.m_PathID!=0)||(d.m_VerticalScrollbar!=null&&d.m_VerticalScrollbar.m_PathID!=0))
                        throw new InvalidDataException("Scrollbars require explicit component binding: "+c.sourceId);
                }
                RecoveredUiAnimationImporter.AddToPrefab(source.name,root);
                if(source.name=="GuideUI")RecoveredGuideSpineImporter.AttachTo(root);
                string path=Destination+"/"+source.name+".prefab";PrefabUtility.SaveAsPrefabAsset(root,path,out bool success);
                if(!success)throw new InvalidDataException("HUD prefab save failed: "+source.name);
                var importer=AssetImporter.GetAtPath(path);importer.userData="wxcf1394487200e48f/43 | "+source.rootPath+" | "+manifestRelative;importer.SaveAndReimport();
            }
            finally{if(root!=null)UnityEngine.Object.DestroyImmediate(root);}
        }
        static void AddComponent(GameObject go,ComponentSource c)
        {
            if(c.className=="AdaptiveBangs")
            {var source=JsonUtility.FromJson<BangsData>(c.data);var bangs=go.AddComponent<OutgameAdaptiveBangs>();bangs.IsNeedAdaptiveBangs=source.IsNeedAdaptiveBangs!=0;bangs.IsDoubleEnded=source.IsDoubleEnded!=0;bangs.enabled=source.m_Enabled!=0;return;}
            if(c.className=="CanvasRenderer")
            {var renderer=go.GetComponent<CanvasRenderer>();if(renderer==null)renderer=go.AddComponent<CanvasRenderer>();renderer.cullTransparentMesh=JsonUtility.FromJson<RendererData>(c.data).m_CullTransparentMesh;return;}
            if(c.className=="Canvas")
            {
                var source=JsonUtility.FromJson<CanvasData>(c.data);
                if(source.m_Camera!=null&&source.m_Camera.m_PathID!=0)throw new InvalidDataException("Canvas camera requires resolved binding: "+c.sourceId);
                var canvas=go.AddComponent<Canvas>();canvas.enabled=source.m_Enabled!=0;canvas.renderMode=(RenderMode)source.m_RenderMode;
                canvas.planeDistance=source.m_PlaneDistance;canvas.pixelPerfect=source.m_PixelPerfect;var serializedCanvas=new SerializedObject(canvas);serializedCanvas.FindProperty("m_ReceivesEvents").boolValue=source.m_ReceivesEvents;serializedCanvas.ApplyModifiedPropertiesWithoutUndo();
                canvas.overrideSorting=source.m_OverrideSorting;canvas.overridePixelPerfect=source.m_OverridePixelPerfect;
                canvas.normalizedSortingGridSize=source.m_SortingBucketNormalizedSize;canvas.vertexColorAlwaysGammaSpace=source.m_VertexColorAlwaysGammaSpace;
                canvas.additionalShaderChannels=(AdditionalCanvasShaderChannels)source.m_AdditionalShaderChannelsFlag;
                canvas.sortingLayerID=source.m_SortingLayerID;canvas.sortingOrder=source.m_SortingOrder;canvas.targetDisplay=source.m_TargetDisplay;
                serializedCanvas.Update();serializedCanvas.FindProperty("m_OverrideSorting").boolValue=source.m_OverrideSorting;serializedCanvas.ApplyModifiedPropertiesWithoutUndo();return;
            }
            if(c.className=="GraphicRaycaster")
            {
                var source=JsonUtility.FromJson<RaycastData>(c.data);
                if(source.m_BlockingMask==null)throw new InvalidDataException("Partial GraphicRaycaster schema: "+c.sourceId);
                var ray=go.AddComponent<GraphicRaycaster>();ray.enabled=source.m_Enabled!=0;ray.ignoreReversedGraphics=source.m_IgnoreReversedGraphics!=0;
                ray.blockingObjects=(GraphicRaycaster.BlockingObjects)source.m_BlockingObjects;ray.blockingMask=source.m_BlockingMask.m_Bits;return;
            }
            if(c.className=="GridLayoutGroup")
            {
                var g=JsonUtility.FromJson<GridData>(c.data);var grid=go.AddComponent<GridLayoutGroup>();grid.cellSize=g.m_CellSize;grid.spacing=g.m_Spacing;
                grid.startCorner=(GridLayoutGroup.Corner)g.m_StartCorner;grid.startAxis=(GridLayoutGroup.Axis)g.m_StartAxis;grid.constraint=(GridLayoutGroup.Constraint)g.m_Constraint;grid.constraintCount=g.m_ConstraintCount;
                grid.childAlignment=(TextAnchor)g.m_ChildAlignment;grid.enabled=g.m_Enabled!=0;if(g.m_Padding!=null)grid.padding=new RectOffset(g.m_Padding.m_Left,g.m_Padding.m_Right,g.m_Padding.m_Top,g.m_Padding.m_Bottom);return;
            }
            if(c.className=="ScrollRect")
            {
                var source=JsonUtility.FromJson<ScrollData>(c.data);var scroll=go.AddComponent<ScrollRect>();
                scroll.horizontal=source.m_Horizontal!=0;scroll.vertical=source.m_Vertical!=0;scroll.movementType=(ScrollRect.MovementType)source.m_MovementType;
                scroll.inertia=source.m_Inertia!=0;scroll.elasticity=source.m_Elasticity;scroll.decelerationRate=source.m_DecelerationRate;scroll.scrollSensitivity=source.m_ScrollSensitivity;
                scroll.horizontalScrollbarVisibility=(ScrollRect.ScrollbarVisibility)source.m_HorizontalScrollbarVisibility;scroll.verticalScrollbarVisibility=(ScrollRect.ScrollbarVisibility)source.m_VerticalScrollbarVisibility;
                scroll.horizontalScrollbarSpacing=source.m_HorizontalScrollbarSpacing;scroll.verticalScrollbarSpacing=source.m_VerticalScrollbarSpacing;scroll.enabled=source.m_Enabled!=0;return;
            }
            if(c.className=="Mask")
            {var source=JsonUtility.FromJson<FitData>(c.data);var mask=go.AddComponent<Mask>();mask.showMaskGraphic=source.m_ShowMaskGraphic!=0;mask.enabled=source.m_Enabled!=0;return;}
            if(c.className=="RectMask2D")
            {var source=JsonUtility.FromJson<RectMaskData>(c.data);var mask=go.AddComponent<RectMask2D>();mask.padding=source.m_Padding;mask.softness=source.m_Softness;mask.enabled=source.m_Enabled!=0;return;}
            if(c.className=="ContentSizeFitter")
            {var source=JsonUtility.FromJson<FitData>(c.data);var fit=go.AddComponent<ContentSizeFitter>();fit.horizontalFit=(ContentSizeFitter.FitMode)source.m_HorizontalFit;fit.verticalFit=(ContentSizeFitter.FitMode)source.m_VerticalFit;fit.enabled=source.m_Enabled!=0;return;}
            var d=JsonUtility.FromJson<Data>(c.data);
            switch(c.className)
            {
                case "Image":
                    var image=go.AddComponent<Image>();image.color=d.m_Color;image.raycastTarget=d.m_RaycastTarget;image.maskable=d.m_Maskable;
                    image.sprite=string.IsNullOrEmpty(c.spriteId)?null:sprites[c.spriteId];image.type=(Image.Type)d.m_Type;image.preserveAspect=d.m_PreserveAspect;
                    image.fillCenter=d.m_FillCenter;image.fillMethod=(Image.FillMethod)d.m_FillMethod;image.fillAmount=d.m_FillAmount;image.fillClockwise=d.m_FillClockwise;image.fillOrigin=d.m_FillOrigin;image.useSpriteMesh=d.m_UseSpriteMesh;image.pixelsPerUnitMultiplier=d.m_PixelsPerUnitMultiplier;image.enabled=d.m_Enabled!=0;break;
                case "Text":
                    var text=go.AddComponent<Text>();text.font=string.IsNullOrEmpty(c.fontId)?null:fonts[c.fontId];text.text=d.m_Text;text.color=d.m_Color;text.raycastTarget=d.m_RaycastTarget;text.maskable=d.m_Maskable;
                    var fd=d.m_FontData;text.fontSize=fd.m_FontSize;text.fontStyle=(FontStyle)fd.m_FontStyle;text.resizeTextForBestFit=fd.m_BestFit;text.resizeTextMinSize=fd.m_MinSize;text.resizeTextMaxSize=fd.m_MaxSize;text.alignment=(TextAnchor)fd.m_Alignment;text.alignByGeometry=fd.m_AlignByGeometry;text.supportRichText=fd.m_RichText;text.horizontalOverflow=(HorizontalWrapMode)fd.m_HorizontalOverflow;text.verticalOverflow=(VerticalWrapMode)fd.m_VerticalOverflow;text.lineSpacing=fd.m_LineSpacing;text.enabled=d.m_Enabled!=0;break;
                case "Outline":case "Shadow":
                    var shadow=c.className=="Outline"?(Shadow)go.AddComponent<Outline>():go.AddComponent<Shadow>();shadow.effectColor=d.m_EffectColor;shadow.effectDistance=d.m_EffectDistance;shadow.useGraphicAlpha=d.m_UseGraphicAlpha;shadow.enabled=d.m_Enabled!=0;break;
                case "UIVideoBtn":
                    if(d.m_OnClick?.m_PersistentCalls?.m_Calls?.Length>0)throw new InvalidDataException("Unrestored persistent video button event: "+c.sourceId);
                    var video=go.AddComponent<OutgameVideoButton>();ConfigureSelectable(video,d);video.videoID=d.videoID;video.AutoCallBtnShow=d.AutoCallBtnShow!=0;
                    if(d.m_AnimationTriggers!=null)video.animationTriggers=d.m_AnimationTriggers;
                    break;
                case "Button":
                    var button=go.AddComponent<Button>();ConfigureSelectable(button,d);break;
                case "ToggleGroup":
                    go.AddComponent<ToggleGroup>().allowSwitchOff=JsonUtility.FromJson<ToggleData>(c.data).m_AllowSwitchOff!=0;break;
                case "Toggle":
                    var toggle=go.AddComponent<Toggle>();ConfigureSelectable(toggle,d);toggle.toggleTransition=(Toggle.ToggleTransition)d.toggleTransition;toggle.SetIsOnWithoutNotify(d.m_IsOn);break;
                case "Slider":
                    var slider=go.AddComponent<Slider>();ConfigureSelectable(slider,d);slider.direction=(Slider.Direction)d.m_Direction;
                    slider.minValue=d.m_MinValue;slider.maxValue=d.m_MaxValue;slider.wholeNumbers=d.m_WholeNumbers;slider.SetValueWithoutNotify(d.m_Value);break;
                case "HorizontalLayoutGroup":case "VerticalLayoutGroup":
                    var layout=c.className=="HorizontalLayoutGroup"?(HorizontalOrVerticalLayoutGroup)go.AddComponent<HorizontalLayoutGroup>():go.AddComponent<VerticalLayoutGroup>();
                    layout.spacing=d.m_Spacing;layout.childForceExpandWidth=d.m_ChildForceExpandWidth;layout.childForceExpandHeight=d.m_ChildForceExpandHeight;layout.childControlWidth=d.m_ChildControlWidth;layout.childControlHeight=d.m_ChildControlHeight;layout.childScaleWidth=d.m_ChildScaleWidth;layout.childScaleHeight=d.m_ChildScaleHeight;layout.reverseArrangement=d.m_ReverseArrangement;Layout(layout,d);break;
                case "GridLayoutGroup":var grid=go.AddComponent<GridLayoutGroup>();grid.cellSize=d.m_CellSize;grid.startCorner=(GridLayoutGroup.Corner)d.m_StartCorner;grid.startAxis=(GridLayoutGroup.Axis)d.m_StartAxis;grid.constraint=(GridLayoutGroup.Constraint)d.m_Constraint;grid.constraintCount=d.m_ConstraintCount;Layout(grid,d);break;
                default:skipped.Add(c.className+" | "+c.sourceId);break;
            }
        }
        static RectTransform ResolveRect(ComponentSource source,ObjectPointer pointer,string resolvedId,Dictionary<string,RectTransform> transforms,string field)
        {
            if(pointer==null||pointer.m_PathID==0)return null;
            if(string.IsNullOrEmpty(resolvedId)&&pointer.m_FileID==0)resolvedId=source.sourceId.Substring(0,source.sourceId.LastIndexOf(':')+1)+pointer.m_PathID;
            if(string.IsNullOrEmpty(resolvedId)||!transforms.TryGetValue(resolvedId,out var value))throw new InvalidDataException("Unresolved source Slider "+field+": "+source.sourceId+" -> "+resolvedId);
            return value;
        }
        static void ConfigureSelectable(Selectable selectable,Data d)
        {
            selectable.interactable=d.m_Interactable;selectable.transition=(Selectable.Transition)d.m_Transition;
            if(d.m_Colors!=null){var cb=selectable.colors;cb.normalColor=d.m_Colors.m_NormalColor;cb.highlightedColor=d.m_Colors.m_HighlightedColor;cb.pressedColor=d.m_Colors.m_PressedColor;cb.selectedColor=d.m_Colors.m_SelectedColor;cb.disabledColor=d.m_Colors.m_DisabledColor;cb.colorMultiplier=d.m_Colors.m_ColorMultiplier;cb.fadeDuration=d.m_Colors.m_FadeDuration;selectable.colors=cb;}
            if(d.m_Navigation!=null){var navigation=selectable.navigation;navigation.mode=(Navigation.Mode)d.m_Navigation.m_Mode;navigation.wrapAround=d.m_Navigation.m_WrapAround;selectable.navigation=navigation;}
            selectable.enabled=d.m_Enabled!=0;
        }
        static void Layout(LayoutGroup layout,Data d){var p=d.m_Padding;if(p!=null)layout.padding=new RectOffset(p.m_Left,p.m_Right,p.m_Top,p.m_Bottom);layout.childAlignment=(TextAnchor)d.m_ChildAlignment;layout.enabled=d.m_Enabled!=0;}
        static string Safe(string text)=>text.Replace(':','_').Replace('/','_');
        static void CheckHash(string path,string expected){using(var sha=SHA256.Create()){string actual=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(target,path)))).Replace("-","").ToLowerInvariant();if(actual!=expected)throw new InvalidDataException("HUD source hash mismatch: "+path);}}
    }
}


