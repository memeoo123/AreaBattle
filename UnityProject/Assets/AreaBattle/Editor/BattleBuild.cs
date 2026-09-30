using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AreaBattle.EditorTools
{
    public static class BattleBuild
    {
        public static void ValidateMechanicsOnly()
        {
            try
            {
                PrepareData();
                var report=ValidateIntegrated();
                Debug.Log("AREABATTLE_INTEGRATED_"+(report.passed?"PASS":"FAIL")+" cases="+report.checks.Count);
                if(Application.isBatchMode)EditorApplication.Exit(report.passed?0:1);
            }
            catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
        }
        public static Report ValidateIntegrated()
        {
            var report=Validate();
            report.limitations="Static-source ordinary/arrow/guide/obstacle/skill/PVP mechanics suite. Full presentation, unresolved special-mode branches and original matched replay remain incomplete.";
            foreach(string name in new[]{"OutgameGlobalItemSlotValidation", "OutgamePackageItemsValidation", "OutgameVirtualItemsValidation", "OutgameGlobalItemRewardsValidation", "OutgameItemModuleValidation", "OutgameItemConfigValidation", "OutgameBuffToolControlValidation", "OutgameAudioResourceValidation", "OutgameAudioManagerValidation", "OutgameUpdateManagerValidation", "OutgameTimeModuleValidation", "OutgameTimeCountdownValidation", "OutgameAudioFadeValidation", "OutgameAudioPlaybackValidation", "OutgameAudioActionValidation", "OutgameAudioNodeValidation", "OutgameAudioControlValidation", "OutgameInputManagerValidation", "OutgamePlayerControlValidation", "OutgameLevelQueryValidation", "OutgameLevelReuseValidation", "OutgameLevelControlValidation", "OutgameLevelResourceValidation", "OutgamePrefabLoaderValidation", "OutgameGameControlValidation", "OutgameUiControlValidation", "OutgameControllerLifecycleValidation", "OutgameMainPreGameValidation", "OutgameMainFocusValidation", "OutgameMainLifecycleValidation", "OutgameMainExitValidation", "OutgameExitGameValidation", "OutgameStarGameValidation", "OutgameLegacyCommanderRepairValidation", "OutgamePreLoadValidation", "OutgameFsmValidation", "OutgameLogicModuleValidation","OutgameConfigReadValidation","OutgameLoginSyncValidation","OutgameBakedAnimationValidation","OutgamePlayerModelsValidation","OutgameSkinItemValidation","OutgameSkinValidation","OutgameOriginalCommanderValidation","OutgameOriginalLocalDataValidation","OutgameLevelProgressionValidation","OutgameCommanderViewValidation","OutgameMenuViewValidation","OutgameUiImportValidation","OutgameCommanderActionValidation","OutgameMenuValidation","OutgameProfileValidation","OutgameToolValidation","OutgameInventoryValidation","OutgameCommanderValidation","AIWithdrawalValidation","OriginalAITimingValidation","OriginalVideoValidation","OriginalReferenceValidation","RandomSourceValidation","ArrowValidation","GuideValidation","GuidePresentationValidation","ObstacleValidation","LevelCatalogValidation","SkillValidation","SkillInputValidation","PvPValidation","LoadoutValidation","AudioPresentationValidation","HudPresentationValidation","RecoveredWayLineImporter","RecoveredObstacleImporter","RecoveredGestureImporter","RecoveredBossValidation","BossProjectileValidation","RepresentativeReplayValidation"})
            {
                var type=typeof(BattleBuild).Assembly.GetType("AreaBattle.EditorTools."+name);
                if(type==null)throw new InvalidOperationException("Missing required validator: "+name);
                var sub=(Report)type.GetMethod("Run").Invoke(null,null);
                report.checks.AddRange(sub.checks);report.passed &= sub.passed;
            }
            File.WriteAllText(Path.Combine(Workspace,"analysis/unity-integrated-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
        public static string Workspace => Directory.GetParent(Application.dataPath).Parent.FullName;
        public static string Target => Path.Combine(Workspace,"analysis/targets/wxcf1394487200e48f/43");
        [Serializable] public sealed class CaseInput { public int grade, lines; }
        [Serializable] public sealed class OracleCase { public string id, method; public CaseInput input; public float expected; }
        [Serializable] public sealed class Oracle { public OracleCase[] cases; }
        [Serializable] public sealed class Check { public string id, result, detail; }
        [Serializable] public sealed class Report { public string schemaVersion="1.0",unityVersion;public bool passed;public List<Check> checks=new List<Check>();public string limitations="Ordinary slice validation only. Original visual replay, special content, and full gameplay acceptance remain open."; }

        [MenuItem("AreaBattle/Import, Validate and Build")]
        public static void BuildAndValidate()
        {
            bool batch=Application.isBatchMode;
            try
            {
                PrepareData();
                RecoveredAssetImporter.Import();
                RecoveredCommanderImporter.Import();
                RecoveredSoldierImporter.Import();
                RecoveredSpriteEffectImporter.Import();
                RecoveredHudImporter.Import();
                RecoveredWayLineImporter.Import();
                RecoveredAudioImporter.Import();
                RecoveredObstacleImporter.Import();
                RecoveredGestureImporter.Import();
                RecoveredSkillEffectImporter.Import();
                RecoveredBossImporter.Import();
                RecoveredBossEmbeddedImporter.Import();
                AssetDatabase.Refresh();
                PlayerSettings.colorSpace=ColorSpace.Gamma;
                PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;
                PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
                PlayerSettings.runInBackground=true;
                PlayerSettings.companyName="Local Restoration";PlayerSettings.productName="AreaBattle";
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var obj=new GameObject("Battle");var view=obj.AddComponent<BattleView>();view.LevelId=0;view.RequestedNormalLevel=0;view.UnitShader=Shader.Find("Unlit/Color");
                Directory.CreateDirectory("Assets/AreaBattle/Scenes");
                string scenePath="Assets/AreaBattle/Scenes/Battle.unity";
                EditorSceneManager.SaveScene(scene,scenePath);
                EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
                AssetDatabase.SaveAssets();
                var report=ValidateIntegrated();
                if(!report.passed)throw new InvalidOperationException("Mechanism checks failed; see analysis/unity-mechanics-validation.json");
                view.InitializeScene(5);
                if(!view.Initialized)throw new InvalidOperationException("Battle view did not initialize");
                Capture(view,"ordinary-level5-initial.png");
                // Keep the editable scene minimal; runtime reconstructs the recovered data.
                EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
                string buildRoot=Path.Combine(Workspace,"Build/Windows");Directory.CreateDirectory(buildRoot);
                var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes=new[]{scenePath},locationPathName=Path.Combine(buildRoot,"AreaBattle.exe"),
                    target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development
                });
                File.WriteAllText(Path.Combine(Workspace,"analysis/unity-build-report.json"),
                    JsonUtility.ToJson(new BuildSummaryData{result=build.summary.result.ToString(),errors=build.summary.totalErrors,warnings=build.summary.totalWarnings,size=build.summary.totalSize.ToString()},true));
                if(build.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Standalone build failed");
                Debug.Log("AREABATTLE_VALIDATION_AND_BUILD_PASS");
                if(batch)EditorApplication.Exit(0);
            }
            catch(Exception ex){Debug.LogException(ex);if(batch)EditorApplication.Exit(1);else throw;}
        }
        [Serializable] sealed class BuildSummaryData {public string result,size;public int errors,warnings;}
        public static void PrepareData()
        {
            string dest="Assets/AreaBattle/Resources/Data";Directory.CreateDirectory(dest+"/Levels");
            foreach(string name in new[]{"SoldierConfig","DispatchConfig","AIConfig","CampConfig","LevelConfig","SettingConfig","SkillConfig","BossConfig","AllSkillConfig","SpecialLevelConfig","AgentSkillUseConfig","EffectConfig"})
                File.Copy(Path.Combine(Target,"generated/tables",name+".json"),dest+"/"+name+".json",true);
            foreach(string path in Directory.GetFiles(Path.Combine(Target,"generated/all-levels"),"level_*.json"))File.Copy(path,dest+"/Levels/"+Path.GetFileName(path),true);
            File.Copy(Path.Combine(Target,"generated/obstacle-runtime.json"),dest+"/ObstacleGeometry.json",true);
            AssetDatabase.Refresh();
        }
        static BattleSimulation World(int id=0)
        { return new BattleSimulation(JsonUtility.FromJson<LevelLayout>(BattleView.ReadText("Data/Levels/level_"+id)),BattleView.ReadConfig(),4305,(a,b)=>true); }
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
        static void Near(float actual,float expected,string label){Require(Mathf.Abs(actual-expected)<=1e-6f,label+": expected "+expected+", got "+actual);}
        public static Report Validate()
        {
            var report=new Report{unityVersion=Application.unityVersion,passed=true};
            Action<string,Action> check=(id,test)=>{try{test();report.checks.Add(new Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new Check{id=id,result="fail",detail=e.Message});}};
            var oracle=JsonUtility.FromJson<Oracle>(File.ReadAllText(Path.Combine(Target,"generated/original-numeric-cases.json")));
            foreach(var sample in oracle.cases)
            {
                var c=sample;
                check(c.id,()=>{
                    var world=World();float actual;
                    switch(c.method){case "GetSpawnTime":actual=world.GetSpawnTime(c.input.grade,c.input.lines);break;
                        case "GetDispatchLineNum":actual=world.GetDispatchLineNum(c.input.grade);break;
                        case "GetDispatchAddScoreTime":actual=world.GetDispatchAddScoreTime(c.input.grade);break;
                        case "GetDispatchScoreNum":actual=world.GetDispatchScoreNum(c.input.grade);break;
                        default:throw new Exception("Unknown oracle method "+c.method);}
                    Near(actual,c.expected,c.method);
                });
            }
            check("layout0-source-data",()=>{var w=World();Require(w.Towers.Count==4,"tower count");Near(w.Tower(4).Score,10,"source score");Require(w.Tower(4).Grade==1 && w.Tower(4).MaxLines==2,"source grade/lines");Near(w.Tower(1).Position.z,4.07f,"pos/100");});
            check("exact-zero-capture-retains-incoming",()=>{var w=World();Require(w.Connect(4,1),"connect");for(int i=0;i<3;i++)w.ChangeScore(1,1,-1);Near(w.Tower(1).Score,0,"capture score");Require(w.Tower(1).Camp==1,"exact-zero capture");Require(w.Tower(4).OutgoingCount==1,"incoming direction must survive");});
            check("capture-overflow",()=>{var w=World();w.ChangeScore(1,1,-4);Require(w.Tower(1).Camp==1,"camp");Near(w.Tower(1).Score,1,"overkill retained");});
            check("noncapturing-damage",()=>{var w=World();w.ChangeScore(1,1,-10,true);Near(w.Tower(1).Score,0,"clamp0");Require(w.Tower(1).Camp==0,"preventCapture");});
            check("grade-boundaries",()=>{float[] scores={9,9.9f,10,29,29.9f,30,65};int[] grades={0,0,1,1,1,2,2};for(int i=0;i<scores.Length;i++){var w=World();w.ChangeScore(4,1,scores[i]-10);Require(w.Tower(4).Grade==grades[i],"grade at "+scores[i]);}});
            check("friendly-cap",()=>{var w=World();w.ChangeScore(4,1,54);w.ChangeScore(4,1,2);Near(w.Tower(4).Score,65,"cap");});
            check("regen-two-second-boundary",()=>{var w=World();w.Tick(2);Near(w.Tower(4).Score,11,"one regen at >=2");Near(w.Tower(1).Score,3,"neutral no regen");});
            check("outgoing-capacity",()=>{var w=World();Require(w.Connect(4,1)&&w.Connect(4,2),"two allowed");Require(!w.Connect(4,3),"third rejected at grade1");});
            check("pause-no-simulation",()=>{var w=World();w.Connect(4,1);w.Pause(true);w.Tick(10);Near(w.Tower(4).Score,10,"pause score");Require(w.Soldiers.Count==0,"pause spawn");w.Pause(false);w.Tick(1.1f);Require(w.Soldiers.Count>0,"resume spawn");});
            check("normal-battle-victory-retry",()=>{
                var w=World();Require(w.Connect(4,1)&&w.Connect(4,2),"initial directions");
                for(int frame=0;frame<1800 && w.State==BattlePhase.Running;frame++){
                    if(w.Tower(2).Camp==1)w.Connect(2,3);
                    w.Tick(1f/60f);
                }
                Require(w.State==BattlePhase.Victory,"all towers captured through production/arrival");
                w.Restart();Require(w.State==BattlePhase.Running,"restart Running");Near(w.Tower(4).Score,10,"restart source");Near(w.Tower(1).Score,3,"restart neutral");Require(w.Tower(1).Camp==0,"restart camp");Require(w.Soldiers.Count==0,"restart units");
            });
            check("defeat-outcome",()=>{var w=World();w.ChangeScore(4,2,-11);w.Tick(1.01f);Require(w.State==BattlePhase.Defeat,"player no towers -> defeat");});
            check("level5-ai-produces-lines",()=>{var w=World(5);for(int i=0;i<3600 && w.State==BattlePhase.Running;i++)w.Tick(1f/60f);Require(w.Lines.Exists(l=>l.Direction!=0),"AI should activate directions");});
            check("spawn-strict-boundary-and-no-debit",()=>{var w=World();w.Connect(4,1);float interval=w.GetSpawnTime(1,1);w.Tick(interval);Require(w.Soldiers.Count==0,"strict > boundary");w.Tick(.001f);Require(w.Soldiers.Count==1,"one spawn after boundary");Near(w.Tower(4).Score,10,"source score is unchanged");Require(w.Soldiers[0].ShipID==1 && w.Soldiers[0].HP==1 && w.Soldiers[0].Occupy==1,"spawn config identity");});
            check("spawn-no-catchup",()=>{var w=World();w.Connect(4,1);w.Tick(5);Require(w.Soldiers.Count==1,"one batch per frame");Near(w.FindLine(4,1).LargeSpawnTimer,0,"discard timer overshoot");});
            check("L0-neutral-capture",()=>{var w=World();w.Connect(4,1);for(int i=0;i<3;i++){var unit=w.SpawnSoldier(4,1);w.ArriveSoldier(unit,w.Tower(1));Near(w.Tower(1).Score,2-i,"arrival score");Require(w.Tower(1).Camp==(i==2?1:0),"arrival camp");Require(!unit.Active,"arrival consumes soldier");}});
            check("equal-hp-contact",()=>{var w=World();var a=new SoldierState{HP=1,Attack=1,Camp=1};var b=new SoldierState{HP=1,Attack=1,Camp=2};w.ResolveSoldierContact(a,b);Require(a.HP==0&&b.HP==0&&!a.Active&&!b.Active,"simultaneous lethal attacks");});
            check("tank-contact",()=>{var w=World();var a=new SoldierState{HP=2,Attack=2,Camp=1};var b=new SoldierState{HP=1,Attack=1,Camp=2};w.ResolveSoldierContact(a,b);Require(a.HP==1&&b.HP==-1&&a.Active&&!b.Active,"simultaneous asymmetric attacks");});
            check("forward-budget",()=>{var w=World();w.ChangeScore(1,1,-3);w.ChangeScore(1,1,65);w.Connect(4,1);var s=w.SpawnSoldier(4,1);s.Voyage=1;w.ArriveSoldier(s,w.Tower(1));Require(s.Voyage==0&&!s.Active,"voyage exhausted");Near(w.Tower(1).Score,65,"full tower score unchanged");});
            check("cut-keeps-enemy-direction",()=>{var w=World(1);w.AIEnabled=false;Require(w.Connect(3,4),"player line");Require(w.Connect(4,3,true),"enemy reverse line");var l=w.FindLine(3,4);Require(l.Direction==3,"bidirectional");w.CutPlayerLine(l.Id);Require(l.Direction==2,"enemy direction survives");});
            check("physics-topology-and-view-restart",()=>{
                var go=new GameObject("Physics integration fixture");
                try{
                    var view=go.AddComponent<BattleView>();view.InitializeScene(1);Require(view.Initialized,"initialize view");
                    Require(view.Simulation.FindLine(1,2)==null,"third tower collider blocks the left-right pair");
                    Require(view.Simulation.FindLine(3,4)!=null,"unobstructed pair exists");
                    view.InitializeScene(1);Require(view.Simulation.FindLine(3,4)!=null,"restart does not leave stale colliders");
                    var before=view.Simulation;view.RestartCurrentLevel();Require(ReferenceEquals(before,view.Simulation),"retry retains simulation identities");
                    Require(view.Simulation.FindLine(3,4)!=null,"retry rebuilds topology before hiding guide towers");
                }finally{UnityEngine.Object.DestroyImmediate(go);Physics.SyncTransforms();}
            });
            check("AI-activation-versus-player-reverse",()=>{var w=World();w.ChangeScore(1,1,-3);w.ChangeScore(1,1,10);Require(w.Connect(4,1),"existing friendly direction");Require(!w.TryActivateLine(1,4),"AI AddActiveLine refuses friendly reverse");Require(w.Connect(1,4),"player SetLineState reverses friendly direction");Require(w.FindLine(1,4).Direction==1,"reversed direction");});
            check("AI-unlinkable-rejected",()=>{var w=World();w.Tower(4).Mode=4;Require(!w.TryActivateLine(4,1),"unlinkable source");});
            check("demotion-keeps-existing-lines",()=>{var w=World();w.ChangeScore(4,1,20);w.Connect(4,1);w.Connect(4,2);w.Connect(4,3);w.ChangeScore(4,2,-21);Require(w.Tower(4).Grade==0 && w.Tower(4).MaxLines==1 && w.Tower(4).OutgoingCount==3,"no forced pruning on downgrade");});
            check("cut-retains-inflight-soldier-and-clock",()=>{var w=World();w.Connect(4,1);var unit=w.SpawnSoldier(4,1);w.Tick(.5f);var line=w.FindLine(4,1);float old=line.LargeSpawnTimer;w.CutPlayerLine(line.Id);w.Tick(2);Require(!unit.Active && w.Tower(1).Score==2,"existing soldier arrives after cut");Require(w.Soldiers.Count==1,"cut stops new spawn");Near(line.LargeSpawnTimer,old,"disabled timer retained");});
            Directory.CreateDirectory(Path.Combine(Workspace,"analysis"));
            File.WriteAllText(Path.Combine(Workspace,"analysis/unity-mechanics-validation.json"),JsonUtility.ToJson(report,true));
            return report;
        }
        public static void Capture(BattleView view,string name)
        {
            var camera=view.BattleCamera;camera.aspect=.5625f;camera.orthographicSize=2.1f;
            var rt=new RenderTexture(540,960,24);camera.targetTexture=rt;
            if(view.Hud!=null){view.Hud.UICamera.targetTexture=rt;Canvas.ForceUpdateCanvases();view.Hud.Synchronize();}
            camera.Render();if(view.Hud!=null)view.Hud.UICamera.Render();
            RenderTexture.active=rt;var image=new Texture2D(540,960,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,540,960),0,0);image.Apply();
            var dest=Path.Combine(Workspace,"analysis/captures");Directory.CreateDirectory(dest);File.WriteAllBytes(Path.Combine(dest,name),image.EncodeToPNG());
            camera.targetTexture=null;if(view.Hud!=null)view.Hud.UICamera.targetTexture=null;
            RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}







