using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameTaskBoxEffectValidation
    {
        public const string PrefabPath="Assets/AreaBattle/Resources/Recovered/TaskBoxEffect/hdzd_eff_bxGlow.prefab";
        static AssetBundle bundle;
        public static AssetBundle Bundle
        {
            get{
                if(bundle)return bundle;string folder=Path.Combine(BattleBuild.Workspace,"analysis/task-box-effect-native-bundle");Directory.CreateDirectory(folder);
                var build=new AssetBundleBuild{assetBundleName="task-box-effect",assetNames=new[]{PrefabPath,"Assets/AreaBattle/Resources/Recovered/FirstPack/Config/EffectConfig.bytes"},addressableNames=new[]{"hdzd_eff_bxGlow","EffectConfig.bytes"}};
                if(!BuildPipeline.BuildAssetBundles(folder,new[]{build},BuildAssetBundleOptions.ChunkBasedCompression,EditorUserBuildSettings.activeBuildTarget))throw new Exception("effect bundle build");
                bundle=AssetBundle.LoadFromFile(Path.Combine(folder,"task-box-effect"));if(!bundle)throw new Exception("effect bundle load");return bundle;
            }
        }
        public static void Prepare(){var b=Bundle;OutgameAccountRewardsValidation.PrepareConfig();}
        public static void LoadPrepared()
        {
            bundle=AssetBundle.GetAllLoadedAssetBundles().FirstOrDefault(b=>b.name=="task-box-effect");if(!bundle)bundle=AssetBundle.LoadFromFile(Path.Combine(BattleBuild.Workspace,"analysis/task-box-effect-native-bundle/task-box-effect"));if(!bundle)throw new Exception("prepared original effect bundle");OutgameAccountRewardsValidation.LoadPreparedConfig();
        }
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameAccountRewardsValidation.Fixture Account;
            public readonly OutgameEffectModule Module;public readonly OutgameEffectModuleServices Services;
            public readonly OutgameAssetProvider Provider;public readonly OutgameEffectConfigReader Config;
            public readonly OutgameAssetbundlePrefabLoader Legacy;
            public readonly List<string> Loads=new List<string>(),Errors=new List<string>(),Warnings=new List<string>();
            public readonly List<OutgameEffectModuleValidation.Wait> Waits=new List<OutgameEffectModuleValidation.Wait>();
            public bool Modern=true;public int LegacyLoads;public readonly bool Native;public Camera Camera;
            public Fixture(bool native=false,string path=null)
            {
                Native=native;Account=new OutgameAccountRewardsValidation.Fixture(native,path);
                Provider=new OutgameAssetProvider(()=>false,Warnings.Add){AssetObject=Bundle.LoadAsset<GameObject>("hdzd_eff_bxGlow"),Status=4};
                Config=new OutgameEffectConfigReader(n=>Bundle.LoadAsset<TextAsset>(n+".bytes"),Errors.Add);
                var modern=new OutgameNewPrefabAsyncHelper(()=>Modern,LoadModern,Errors.Add);
                Legacy=new OutgameAssetbundlePrefabLoader(()=>{if(Modern)Errors.Add("legacy used while modern");return !Modern;},(pathName,name)=>{LegacyLoads++;if(pathName!="effect/ui/hdzd_eff_bxglow.prefab.unity3d"||name!="hdzd_eff_bxGlow")throw new Exception("original legacy path");return Task.FromResult(Bundle.LoadAsset<GameObject>(name));},new Dictionary<string,UnityEngine.Object>());
                var effects=new OutgameEffectServices{UseNewResources=()=>Modern,LoadNewPrefab=modern.LoadPrefabAsync,LoadLegacyPrefab=pathName=>Legacy.LoadPrefab(pathName)};
                if(!native){effects.Destroy=UnityEngine.Object.DestroyImmediate;effects.Wait=instruction=>{var wait=new OutgameEffectModuleValidation.Wait{Instruction=instruction};Waits.Add(wait);return wait.Completion.Task;};}
                Services=new OutgameEffectModuleServices{HasConfigResource=()=>Bundle!=null,UseNewResources=()=>Modern,ReadConfig=Config.Read,CanvasRoot=()=>Account.Page.Root.transform,UiRoot=()=>Account.Page.Root.transform,UiCamera=()=>Camera,Effects=effects,Log=s=>{},Error=Errors.Add};
                if(!native){Services.Destroy=UnityEngine.Object.DestroyImmediate;Services.Persist=o=>{};}
                Module=new OutgameEffectModule(Services);Module.Initialize();OutgameEffectBinding.BindTask(()=>Module,Account.Page.Services.Liveness);
            }
            async Task<OutgameAssetHandle> LoadModern(string path)
            {
                Loads.Add(path);if(path!="Effect/UI/hdzd_eff_bxGlow")throw new Exception("original new resource path");
                if(Native){var operation=Bundle.LoadAssetAsync<GameObject>("hdzd_eff_bxGlow");await OutgameUnityAwait.Await(operation);Provider.AssetObject=operation.asset;}
                return Provider.CreateHandle(path,()=>false);
            }
            public void Start(int liveness=30){Account.Tasks.Child.ModuleData.ext.livenessValue=liveness;Account.Page.Start();}
            public Transform Box(int index=0)=>Account.Page.Page.ProgressBackground.GetChild(1).GetChild(index);
            public OutgameBaseEffect Effect=>Module.Get(Account.Page.Page.Liveness.EffectHandle);
            public Task Claim(int index=0)=>Account.Page.Page.Liveness.ClaimAsync(Box(index),Account.Page.Subviews.Daily.I.LivenessItems[index]);
            public void Dispose(){Module.Shutdown();Account.Dispose();}
        }
        static void Require(bool b,string why){if(!b)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original task effect1016 prefab/particles/materials, native AssetBundle, actual EffectModule/handle provider, original TaskPanel/liveness and actual account rewards/storage. Edit-mode async timing explicitly controlled; native page/render/particle validation separate. Provider acquisition is a local native-bundle fixture; full production Main/resource catalog/EffectControl/platform and original-frame audiovisual acceptance pending."};
            Action<string,Action> check=(id,a)=>{try{a();report.checks.Add(new BattleBuild.Check{id="task-box-effect-"+id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="task-box-effect-"+id,result="fail",detail=e.ToString()});}};
            check("original-prefab-four-particles-source-scale-and-materials",()=>{
                var p=Bundle.LoadAsset<GameObject>("hdzd_eff_bxGlow");Require(p&&p.transform is RectTransform&&p.transform.childCount==3&&p.transform.localScale==Vector3.one*100&&p.GetComponentsInChildren<ParticleSystem>(true).Length==4,"original four-node particle hierarchy and scale100");
                Require(p.GetComponentsInChildren<ParticleSystemRenderer>(true).All(r=>r.sharedMaterial&&r.sharedMaterial.mainTexture&&r.sharedMaterial.shader&&r.sharedMaterial.shader.name!="Hidden/InternalErrorShader")&&p.GetComponentsInChildren<MonoBehaviour>(true).Length==0,"all original material/texture references valid and no original executable scripts");
            });
            check("original-config-native-bundle-specialized-reader",()=>{using(var f=new Fixture()){
                var rows=f.Config.Read();var c=rows.Single(r=>r.id==1016);Require(c.type==0&&c.res=="hdzd_eff_bxGlow"&&c.duration==5,"source1016 config loaded from original bytes through actual bundle");f.Module.ReadEffectData();Require(f.Module.Configs.Count==rows.Count&&f.Errors.Count==0,"full effect config retained");
            }});
            check("task-claim-actual-effect-and-shared-account-reward",()=>{using(var f=new Fixture()){
                f.Start();f.Claim().GetAwaiter().GetResult();var e=f.Effect;
                Require(e!=null&&e.GameObject.name=="hdzd_eff_bxGlow"&&e.GameObject.transform.parent==f.Box()&&e.GameObject.GetComponentsInChildren<ParticleSystem>(true).Length==4&&f.Provider.RefCount==1,"real original asset under source box with owned handle");
                Require(f.Account.Global.GetItemCount(1001)==50&&f.Account.Account.Local.GoldNum==50&&f.Account.Tasks.Child.ModuleData.ext.livenessAward==1&&!f.Box().GetComponent<UnityEngine.UI.Button>().enabled,"source claim after frame awards both actual economic records and marks first bit");
                Require(f.Loads.Single()=="Effect/UI/hdzd_eff_bxGlow"&&e.GameObject.transform.localScale==Vector3.one*100,"original path and prefab scale retained through module");
            }});
            check("tab-change-closes-current-effect-before-content-change",()=>{using(var f=new Fixture()){
                f.Start();f.Claim().GetAwaiter().GetResult();var e=f.Effect;f.Account.Page.Page.ToggleChanged(true,f.Account.Page.Page.AchievementTab);
                Require(e.IsDisposed&&!e.GameObject&&f.Provider.RefCount==0&&f.Module.Effects.Count==0&&f.Account.Page.Page.Liveness.EffectHandle==0&&f.Account.Page.Page.AchievementContent.activeSelf,"actual tab routing releases source effect and clears current handle");
            }});
            check("natural-completion-keeps-page-stale-handle-until-tab-change",()=>{using(var f=new Fixture()){
                f.Start();f.Claim().GetAwaiter().GetResult();int id=f.Effect.UID;f.Waits[0].Complete();Require(f.Waits[1].Instruction is WaitForSeconds,"original positive duration");f.Waits[1].Complete();Require(f.Module.Get(id)==null&&f.Provider.RefCount==0&&f.Account.Page.Page.Liveness.EffectHandle==id,"module completion does not rewrite page handle");f.Account.Page.Page.ToggleChanged(true,f.Account.Page.Page.AchievementTab);Require(f.Account.Page.Page.Liveness.EffectHandle==0,"close missing handle is safe source no-op");
            }});
            check("legacy-source-cache-instantiates-distinct-original-prefabs",()=>{using(var f=new Fixture()){
                f.Modern=false;f.Start();f.Claim().GetAwaiter().GetResult();var first=f.Effect;int second=f.Module.Show(1016,f.Box());var next=f.Module.Get(second);
                Require(first.GameObject!=next.GameObject&&first.GameObject.name=="hdzd_eff_bxGlow(Clone)"&&f.LegacyLoads==1&&f.Legacy.Loaded.Count==1&&f.Provider.RefCount==0,"source legacy cache owns prefab, each call instantiates and no modern handle acquired");
                f.Module.Close(first.UID);Require(!first.GameObject&&next.GameObject&&f.Legacy.Loaded.Count==1,"closing instance retains shared cached prefab");
            }});
            check("overlapping-claims-retain-earlier-effect-source-ownership",()=>{using(var f=new Fixture()){
                f.Start(100);f.Claim(0).GetAwaiter().GetResult();var first=f.Effect;f.Claim(1).GetAwaiter().GetResult();var second=f.Effect;
                Require(first.UID!=second.UID&&f.Module.Effects.Count==2&&f.Provider.RefCount==2&&first.GameObject&&second.GameObject,"each award owns a distinct instance and only latest handle is stored by page");f.Account.Page.Page.Liveness.CloseCurrentEffect();Require(f.Module.Get(first.UID)==first&&f.Module.Get(second.UID)==null&&f.Provider.RefCount==1,"page closes only latest; older timer remains module-owned");
            }});
            check("new-loader-route-gate-and-config-missing-diagnostic",()=>{
                int loads=0;var errors=new List<string>();var helper=new OutgameNewPrefabAsyncHelper(()=>false,path=>{loads++;return Task.FromResult<OutgameAssetHandle>(null);},errors.Add);Require(helper.LoadPrefabAsync("source").GetAwaiter().GetResult()==null&&loads==0&&errors.Count==1,"wrong modern route returns null without provider acquisition");var reader=new OutgameEffectConfigReader(n=>null,errors.Add);Require(reader.Read()==null&&errors[1]=="配置文件不存在EffectConfig","missing config uses source diagnostic and null");
            });
            return report;
        }
    }
}
