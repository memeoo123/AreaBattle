using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using BigInteger=System.Numerics.BigInteger;
namespace AreaBattle.EditorTools
{
    public static class OutgameTopInfoAssetsValidation
    {
        public const string Gold="hdzd_effect_jinbiglow",Diamond="hdzd_eff_Diamond02";
        static AssetBundle bundle;
        public static AssetBundle Bundle
        {
            get{
                if(bundle)return bundle;
                string folder=Path.Combine(BattleBuild.Workspace,"analysis/top-info-effects-native-bundle");Directory.CreateDirectory(folder);
                var build=new AssetBundleBuild{assetBundleName="top-info-effects",assetNames=new[]{
                    "Assets/AreaBattle/Resources/Recovered/TopInfoEffects/"+Gold+".prefab",
                    "Assets/AreaBattle/Resources/Recovered/TopInfoEffects/"+Diamond+".prefab",
                    "Assets/AreaBattle/Resources/Recovered/FirstPack/Config/EffectConfig.bytes"},addressableNames=new[]{Gold,Diamond,"EffectConfig.bytes"}};
                if(!BuildPipeline.BuildAssetBundles(folder,new[]{build},BuildAssetBundleOptions.ChunkBasedCompression,EditorUserBuildSettings.activeBuildTarget))throw new Exception("top effects bundle build");
                bundle=AssetBundle.LoadFromFile(Path.Combine(folder,"top-info-effects"));if(!bundle)throw new Exception("top effects bundle load");return bundle;
            }
        }
        public static void LoadPrepared(){bundle=AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b=>b.name=="top-info-effects");if(!bundle)bundle=AssetBundle.LoadFromFile(Path.Combine(BattleBuild.Workspace,"analysis/top-info-effects-native-bundle/top-info-effects"));if(!bundle)throw new Exception("prepared top effects bundle");}
        public sealed class Fixture:IDisposable
        {
            public readonly GameObject Root,Top;public readonly OutgameTopInfoPage Page;
            public readonly OutgameEffectModule Module;public readonly OutgameEffectModuleServices Services;
            public readonly Dictionary<string,OutgameAssetProvider> Providers=new Dictionary<string,OutgameAssetProvider>();
            public readonly List<string> Loads=new List<string>(),LegacyPaths=new List<string>(),Errors=new List<string>();
            public readonly List<OutgameEffectModuleValidation.Wait> Waits=new List<OutgameEffectModuleValidation.Wait>();
            public readonly OutgameAssetbundlePrefabLoader Legacy;readonly bool native;
            public bool Modern=true;public Camera Camera;
            public Transform GoldParent=>Top.transform.Find("objTopInfo/goldInfo/imgIcon");
            public Transform DiamondParent=>Top.transform.Find("objTopInfo/diamondInfo/imgDiamondIcon");
            public Fixture(bool native=false)
            {
                this.native=native;Root=new GameObject("TopInfoAssetValidation",typeof(RectTransform));
                Top=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/TopInfo/TopInfoUI"),Root.transform,false);
                Page=OutgameTopInfoPage.FromOriginal(Top,()=>new OutgameMessageDispatcher());Page.OpenParts();
                foreach(string name in new[]{Gold,Diamond})Providers.Add(name,new OutgameAssetProvider(()=>false,Errors.Add){AssetObject=Bundle.LoadAsset<GameObject>(name),Status=4});
                var modern=new OutgameNewPrefabAsyncHelper(()=>Modern,Load,Errors.Add);
                Legacy=new OutgameAssetbundlePrefabLoader(()=>!Modern,(path,name)=>{LegacyPaths.Add(path);return Task.FromResult(Bundle.LoadAsset<GameObject>(name));},new Dictionary<string,UnityEngine.Object>());
                var effects=new OutgameEffectServices{UseNewResources=()=>Modern,LoadNewPrefab=modern.LoadPrefabAsync,LoadLegacyPrefab=path=>Legacy.LoadPrefab(path)};
                if(!native){effects.Destroy=UnityEngine.Object.DestroyImmediate;effects.Wait=instruction=>{var w=new OutgameEffectModuleValidation.Wait{Instruction=instruction};Waits.Add(w);return w.Completion.Task;};}
                var config=new OutgameEffectConfigReader(name=>Bundle.LoadAsset<TextAsset>(name+".bytes"),Errors.Add);
                Services=new OutgameEffectModuleServices{HasConfigResource=()=>Bundle!=null,UseNewResources=()=>Modern,ReadConfig=config.Read,CanvasRoot=()=>Root.transform,UiRoot=()=>Root.transform,UiCamera=()=>Camera,Effects=effects,Log=s=>{},Error=Errors.Add};
                if(!native){Services.Destroy=UnityEngine.Object.DestroyImmediate;Services.Persist=o=>{};}
                Module=new OutgameEffectModule(Services);Module.Initialize();
            }
            async Task<OutgameAssetHandle> Load(string path)
            {
                Loads.Add(path);string name=path.Substring("Effect/UI/".Length);var p=Providers[name];
                if(native){var op=Bundle.LoadAssetAsync<GameObject>(name);await OutgameUnityAwait.Await(op);p.AssetObject=op.asset;}
                return p.CreateHandle(path,()=>false);
            }
            public void Dispose(){Module.Shutdown();if(native)UnityEngine.Object.Destroy(Root);else UnityEngine.Object.DestroyImmediate(Root);}
        }
        static void Require(bool b,string why){if(!b)throw new Exception(why);}
        static T Throws<T>(Action a)where T:Exception{try{a();}catch(T e){return e;}throw new Exception("Expected "+typeof(T).Name);}
        static OutgameBigNumberSymbols Symbols(){var s=new OutgameBigNumberSymbols();s.Set(new Dictionary<int,string>{{3,"K"},{6,"M"}});return s;}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="TopInfo original effect1007/1019 assets, real EffectModule/provider ownership and BigNumExtension27612 formatting. Original complete TopInfo Awake/profile/auth/account actions and production Main assembly remain pending. Native render/lifetime checks are separate; no original screenshot equivalence claim."};
            Action<string,Action> check=(id,a)=>{try{a();r.checks.Add(new BattleBuild.Check{id="top-info-assets-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="top-info-assets-"+id,result="fail",detail=e.ToString()});}};
            check("format-unconfigured-warning-before-zero-and-propagation",()=>{var s=new OutgameBigNumberSymbols();int n=0;Require(s.Format(BigInteger.Zero,a=>{Require(a.Length==1&&(string)a[0]=="大数据表未配置","source diagnostic");n++;})==""&&n==1,"warning even for zero");Throws<InvalidOperationException>(()=>s.Format(1,a=>throw new InvalidOperationException()));});
            check("format-hundredths-truncation-and-final-zero-rule",()=>{var s=Symbols();long[] input={0,1,10,20,99,100,120,123,150,999,1234,9999,10000,99999};string[] expected={"0","0.01","0","0","0.99","1","1","1.23","1","9.99","12.3","99.9","100","999"};for(int i=0;i<input.Length;i++)Require(s.Format(input[i],null)==expected[i],"source digits "+input[i]);});
            check("format-symbol-boundaries-truncate-without-rounding",()=>{var s=Symbols();long[] input={100000,123456,999999,1000000,99999999,100000000};string[] expected={"1.00K","1.23K","9.99K","10.0K","999K","1.00M"};for(int i=0;i<input.Length;i++)Require(s.Format(input[i],null)==expected[i],"magnitude "+input[i]);});
            check("format-arbitrary-precision-saturates-symbol-only",()=>{var s=Symbols();Require(s.Format(BigInteger.Pow(10,100),null)=="100M","last symbol reused with original group still increasing");s.Symbols[0]="X";Require(s.Format(123456,null)=="1.23X","live public symbol list");s.ByMagnitude[3]="Y";Require(s.Format(123456,null)=="1.23X","Set copied values; dictionary mutation does not rebuild symbols");});
            check("format-negative-source-text-and-current-culture",()=>{var s=Symbols();Require(s.Format(-23,null)=="-.23"&&s.Format(-10,null)=="-"&&s.Format(-120,null)=="-1.2","minus sign participates in source string operations");var before=System.Globalization.CultureInfo.CurrentCulture;try{System.Globalization.CultureInfo.CurrentCulture=new System.Globalization.CultureInfo("fr-FR");Require(s.Format(123,null)=="1.23","source dot literal regardless of decimal separator");}finally{System.Globalization.CultureInfo.CurrentCulture=before;}});
            check("format-empty-and-zero-magnitude-fail-without-fallback",()=>{var s=Symbols();Throws<InvalidOperationException>(()=>s.Set(new Dictionary<int,string>()));Require(s.Symbols.Count==0&&s.FirstMagnitude==3&&s.Format(0,null)=="0","partial Set retained, zero precedes symbol indexing");Throws<ArgumentOutOfRangeException>(()=>s.Format(100000,null));s.Set(new Dictionary<int,string>{{0,"X"}});Throws<DivideByZeroException>(()=>s.Format(12345,null));});
            check("native-prefabs-original-particles-child-scale-and-bindings",()=>{var gold=Bundle.LoadAsset<GameObject>(Gold);var diamond=Bundle.LoadAsset<GameObject>(Diamond);Require(gold.transform is RectTransform&&diamond.transform is RectTransform&&gold.transform.localScale==Vector3.one&&diamond.transform.localScale==Vector3.one,"original roots");Require(gold.GetComponentsInChildren<ParticleSystem>(true).Length==1&&diamond.GetComponentsInChildren<ParticleSystem>(true).Length==4&&gold.transform.GetChild(0).localScale==Vector3.one*200,"five original particle systems and gold child scale200");Require(!diamond.GetComponent<ParticleSystemRenderer>().enabled&&diamond.GetComponent<ParticleSystemRenderer>().sharedMaterial==null,"original disabled root renderer has null material");foreach(var p in new[]{gold,diamond})Require(p.GetComponentsInChildren<MonoBehaviour>(true).Length==0&&p.GetComponentsInChildren<ParticleSystemRenderer>(true).Where(x=>x.enabled).All(x=>x.sharedMaterial&&x.sharedMaterial.mainTexture&&x.sharedMaterial.shader.name!="Hidden/InternalErrorShader"),"native source materials/textures and no original executable script");});
            check("source-config-zero-duration-and-modern-case-sensitive-paths",()=>{using(var f=new Fixture()){int a=f.Module.Show(1007,f.GoldParent),b=f.Module.Show(1019,f.DiamondParent);var x=f.Module.Get(a);var y=f.Module.Get(b);Require(x.Config.type==0&&y.Config.type==0&&x.Config.duration==0&&y.Config.duration==0&&x.GameObject.name==Gold&&y.GameObject.name==Diamond,"original IDs/type/duration/name");Require(f.Loads.SequenceEqual(new[]{"Effect/UI/"+Gold,"Effect/UI/"+Diamond})&&f.Errors.Count==0&&x.Parent==f.GoldParent&&y.Parent==f.DiamondParent,"original path case and parent images");}});
            check("source-looping-effects-have-no-positive-duration-timer",()=>{using(var f=new Fixture()){f.Module.Show(1007,f.GoldParent);f.Module.Show(1019,f.DiamondParent);Require(f.Waits.Count==2&&f.Waits.All(w=>w.Instruction is WaitUntil),"only load waits");foreach(var w in f.Waits.ToArray())w.Complete();Require(f.Waits.Count==2&&f.Module.Effects.Count==2&&f.Providers.Values.All(p=>p.RefCount==1),"zero-duration effects remain module-owned after load");}});
            check("modern-close-releases-only-selected-effect-handle",()=>{using(var f=new Fixture()){int a=f.Module.Show(1007,f.GoldParent),b=f.Module.Show(1019,f.DiamondParent);var x=f.Module.Get(a);f.Module.Close(a);Require(x.IsDisposed&&!x.GameObject&&f.Providers[Gold].RefCount==0&&f.Providers[Diamond].RefCount==1&&f.Module.Get(b)!=null,"independent handle ownership");f.Module.Close(a);f.Module.Close(-1);Require(f.Module.Effects.Count==1,"missing source handle no-op");}});
            check("legacy-lowercase-bundle-distinct-prefab-name-and-cache",()=>{using(var f=new Fixture()){f.Modern=false;int a=f.Module.Show(1019,f.DiamondParent),b=f.Module.Show(1019,f.DiamondParent);Require(f.LegacyPaths.SequenceEqual(new[]{"effect/ui/hdzd_eff_diamond02.prefab.unity3d"})&&f.Legacy.Loaded.Count==1&&f.Module.Get(a).GameObject!=f.Module.Get(b).GameObject&&f.Module.Get(a).GameObject.name==Diamond+"(Clone)"&&f.Providers.Values.All(p=>p.RefCount==0),"original lowercase path, retained-case name, cached asset/distinct instances");}});
            check("module-shutdown-releases-both-persistent-effects",()=>{using(var f=new Fixture()){var a=f.Module.Get(f.Module.Show(1007,f.GoldParent));var b=f.Module.Get(f.Module.Show(1019,f.DiamondParent));f.Module.Shutdown();Require(a.IsDisposed&&b.IsDisposed&&!a.GameObject&&!b.GameObject&&f.Module.Effects.Count==0&&f.Providers.Values.All(p=>p.RefCount==0),"explicit shutdown owns persistent effect cleanup");}});
            return r;
        }
    }
}
