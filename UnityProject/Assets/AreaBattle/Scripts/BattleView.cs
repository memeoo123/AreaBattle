using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaBattle
{
    // Presentation adapter only. All scores, production, AI and outcomes live in BattleSimulation.
    public sealed class BattleView : MonoBehaviour
    {
        [Serializable] sealed class ShipTable { public ShipConfig[] Datas; }
        [Serializable] sealed class AITable { public AIConfig[] Datas; }
        [Serializable] sealed class DispatchTable { public DispatchConfig[] Datas; }
        [Serializable] sealed class SkillTable { public SkillParameters[] Datas; }
        [Serializable] sealed class BossTable { public BossParameters[] Datas; }
        [Serializable] sealed class SpecialMap {public int id,SceneId;}
        [Serializable] sealed class SpecialMaps {public SpecialMap[] Datas;}
        [Serializable] sealed class AgentSkills {public AgentSkillUseConfig[] Datas;}
        [Serializable] sealed class PositionRecord { public float x, y, z; public Vector3 World => new Vector3(x,y,z)/100f; }
        [Serializable] sealed class StarRecord { public PositionRecord pos; public int CampID, StartScore, ShipID; }
        [Serializable] sealed class GeometryLayout { public StarRecord[] StarInfoCfgs; }

        public int LevelId = 5;
        public int Seed = 4305;
        // Original RandomHelper..cctor creates a single parameterless System.Random.
        // Do not seed or replace this stream on retry, scene initialization or a new view.
        public System.Random RandomSourceOverride { get; set; }
        public int RequestedNormalLevel = -1;
        public int CommanderMode = 1;
        // Explicit local cosmetic fixture for ordinary type1 units in selected camps.
        // Source player/enemy account skin selection is separate and not inferred here.
        public int OrdinarySoldierSkinId = 100;
        public int OrdinarySoldierSkinCampMask = (1 << 1) | (1 << 2);
        public int ControlledSkillStock = 3;
        public BattleSkillInput SkillInput { get; private set; }
        public BattleProgress Progress { get; private set; }
        public BattleLoadout Loadout { get; set; }
        public BattleHud Hud { get; private set; }
        public BattleAudio Audio { get; private set; }
        public BattleSkillPresentation SkillPresentation { get; private set; }
        public RecoveredGestureVisuals Gestures { get; private set; }
        public BattleSkillTargetVisuals SkillTargets {get;private set;}
        public float VisualClock=>visualClock;
        public Transform TowerPresentationTransform(int id)=>towerObjects.TryGetValue(id,out var obj)?obj.transform:null;
        public Transform SoldierPresentationTransform(int id)=>soldierObjects.TryGetValue(id,out var obj)?obj.transform:null;
        public Shader UnitShader;
        public BattleSimulation Simulation { get; private set; }
        public BattleGuide Guide { get; private set; }
        public Camera BattleCamera { get; private set; }
        public bool Initialized { get; private set; }
        readonly Dictionary<int, GameObject> towerObjects = new Dictionary<int, GameObject>();
        readonly Dictionary<int, RecoveredBossVisual> bossVisuals=new Dictionary<int, RecoveredBossVisual>();
        readonly Dictionary<int, BossParameters> bossVisualParameters=new Dictionary<int, BossParameters>();
        readonly Dictionary<int, SpriteRenderer> towerSprites = new Dictionary<int, SpriteRenderer>();
        readonly Dictionary<int, TextMesh> scoreTexts = new Dictionary<int, TextMesh>();
        readonly Dictionary<int, LineRenderer> lineObjects = new Dictionary<int, LineRenderer>();
        readonly Dictionary<int, RecoveredWayLineVisual> recoveredLines = new Dictionary<int, RecoveredWayLineVisual>();
        readonly Dictionary<int, GameObject> soldierObjects = new Dictionary<int, GameObject>();
        readonly Dictionary<int, LineRenderer> arrowObjects = new Dictionary<int, LineRenderer>();
        readonly List<RecoveredSpriteEffect> spriteEffects=new List<RecoveredSpriteEffect>();
        readonly Dictionary<int,float> lastTowerHit=new Dictionary<int,float>();
        readonly List<GameObject> ownedObjects = new List<GameObject>();
        readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        readonly Dictionary<int, Material> campMaterials = new Dictionary<int, Material>();
        Material lineMaterial, soldierMaterial, obstacleMaterial;
        Font font;
        int selectedTower;
        bool drawing, cutting;
        float visualClock;
        int pressedSkill=-1;
        Vector3 previousGround;
        LineRenderer preview;
        string error;
        GUIStyle textStyle, buttonStyle;
        static readonly string[] CampSuffix = { "灰", "蓝", "红", "绿", "黄", "青" };
        static readonly string[] Family = { "", "普通塔", "防御塔", "进攻塔", "箭塔" };
        static readonly string[] CampHex = { "939393", "2885ff", "ff5252", "00ca64", "ffe255", "B10038", "3AD2FF", "726A84", "64B913", "45379D" };

        void Start()
        {
            var args=Environment.GetCommandLineArgs();int arg=Array.IndexOf(args,"-battle-level");
            bool smoke=Array.IndexOf(args,"-battle-smoke")>=0;
            int skinArg=Array.IndexOf(args,"-battle-skin");
            if(skinArg>=0&&skinArg+1<args.Length&&int.TryParse(args[skinArg+1],out int skin)&&(skin==100||skin==102))OrdinarySoldierSkinId=skin;
            if(!smoke && Loadout==null)
            {
                try{Loadout=BattleLoadout.LoadOrCreate(ProfilePath,BattleLoadout.CreateFixture(Math.Max(0,RequestedNormalLevel),CommanderMode,1,ControlledSkillStock));CommanderMode=Loadout.CommanderId;}
                catch(Exception ex){error=ex.ToString();Debug.LogException(ex);return;}
            }
            int modeArg=Array.IndexOf(args,"-battle-commander");
            if(modeArg>=0 && modeArg+1<args.Length && int.TryParse(args[modeArg+1],out int mode)){CommanderMode=Mathf.Clamp(mode,1,6);if(Loadout!=null)Loadout.SetCommander(CommanderMode);}
            int layoutArg=Array.IndexOf(args,"-battle-layout");
            int pvpArg=Array.IndexOf(args,"-battle-pvp-map");
            if(smoke){RequestedNormalLevel=-1;InitializeScene(5);}
            else if(pvpArg>=0&&pvpArg+1<args.Length&&int.TryParse(args[pvpArg+1],out int map))
            {
                int progress=Loadout==null?0:Loadout.NormalLevel;
                if(arg>=0&&arg+1<args.Length&&int.TryParse(args[arg+1],out int selectedProgress))progress=selectedProgress;
                if(progress<23){error="竞技关需要普通关卡进度达到 23。";return;}
                InitializePvPMap(map,progress,1,1,new[]{1,1,1});
            }
            else if(layoutArg>=0 && layoutArg+1<args.Length && int.TryParse(args[layoutArg+1],out int layout))InitializeSpecialScene(layout);
            else if(arg>=0 && arg+1<args.Length && int.TryParse(args[arg+1],out int requested))InitializeNormalLevel(requested);
            if (!Initialized && RequestedNormalLevel>=0)
            {
                int selected=Loadout==null?RequestedNormalLevel:Loadout.NormalLevel;
                InitializeNormalLevel(selected);
            }
            if(!Initialized)InitializeScene(LevelId);
            if(Initialized){Audio=gameObject.AddComponent<BattleAudio>();Audio.Initialize();Audio.ApplyPhase(Simulation.State);}
            if(smoke)gameObject.AddComponent<BattlePlayerSmoke>();
        }
        void OnDestroy()
        {
            if(Guide!=null)Guide.Dispose();
            foreach(var material in campMaterials.Values)if(material!=null)DestroyOwned(material);
            if(lineMaterial!=null)DestroyOwned(lineMaterial);
            if(soldierMaterial!=null)DestroyOwned(soldierMaterial);
            if(obstacleMaterial!=null)DestroyOwned(obstacleMaterial);
        }

        public void InitializeNormalLevel(int level)
        {
            var catalog=new BattleLevelCatalog(ReadText("Data/LevelConfig"));
            var row=catalog.Resolve(level);RequestedNormalLevel=level;
            InitializeScene(row.SceneId,level);
        }
        public void InitializeSpecialScene(int layout)
        {
            // Source SpecialLevel bypasses normal SceneId mapping and normal progress writes.
            // SkillItem.Init still reads CurLevel, which is the direct layout ID in this mode.
            RequestedNormalLevel=-1;
            InitializeScene(layout);
        }
        public void InitializePvPMap(int mapId,int normalLevelIdentity,int enemyCommander,int agentSkillUseId,int[] enemySkillLevels)
        {
            var maps=JsonUtility.FromJson<SpecialMaps>(ReadText("Data/SpecialLevelConfig")).Datas;
            var row=Array.Find(maps,x=>x.id==mapId)??Array.Find(maps,x=>x.id==1);
            var agents=JsonUtility.FromJson<AgentSkills>(ReadText("Data/AgentSkillUseConfig")).Datas;
            var agent=Array.Find(agents,x=>x.id==agentSkillUseId);
            if(row==null||agent==null)throw new ArgumentException("Recovered PvP map or agent skill configuration missing");
            RequestedNormalLevel=-1;InitializeScene(row.SceneId,normalLevelIdentity);
            if(Initialized)Simulation.ConfigurePvP(enemyCommander,enemySkillLevels,agent);
        }

        public static BattleConfigData ReadConfig()
        {
            return new BattleConfigData {
                Ships = JsonUtility.FromJson<ShipTable>(ReadText("Data/SoldierConfig")).Datas,
                AIs = JsonUtility.FromJson<AITable>(ReadText("Data/AIConfig")).Datas,
                Dispatch = JsonUtility.FromJson<DispatchTable>(ReadText("Data/DispatchConfig")).Datas,
                GameTimeScale = 1f, ShipTimeScale = .8f
            };
        }
        public static string ReadText(string path)
        {
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null) throw new InvalidOperationException("Missing recovered data: " + path);
            return asset.text;
        }
        public static Color CampColor(int camp)
        {
            Color color;
            ColorUtility.TryParseHtmlString("#" + CampHex[Mathf.Clamp(camp, 0, CampHex.Length-1)], out color);
            return color;
        }

        public void InitializeScene(int level,int guideLevel=-1)
        {
            LevelId = level;
            try
            {
                if(Guide!=null)Guide.Dispose();
                foreach (var obj in ownedObjects) if (obj != null) DestroyOwned(obj);
                ownedObjects.Clear(); towerObjects.Clear(); towerSprites.Clear(); scoreTexts.Clear(); lineObjects.Clear();recoveredLines.Clear(); soldierObjects.Clear();arrowObjects.Clear();
                spriteEffects.Clear();lastTowerHit.Clear();bossVisuals.Clear();bossVisualParameters.Clear();
                foreach(var cfg in JsonUtility.FromJson<BossTable>(ReadText("Data/BossConfig")).Datas)bossVisualParameters[cfg.id]=cfg;
                foreach(var mat in campMaterials.Values) if(mat!=null)DestroyOwned(mat);
                campMaterials.Clear();
                if(lineMaterial!=null)DestroyOwned(lineMaterial);
                if(soldierMaterial!=null)DestroyOwned(soldierMaterial);
                if(obstacleMaterial!=null)DestroyOwned(obstacleMaterial);
                Time.timeScale=1f;
                var text = ReadText("Data/Levels/level_" + level);
                var geometry = JsonUtility.FromJson<GeometryLayout>(text);
                var layout = JsonUtility.FromJson<LevelLayout>(text);
                font = Resources.Load<Font>("Recovered/Fonts/HYZhuZiMuTouRenW");
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
                soldierMaterial = new Material(UnitShader!=null?UnitShader:Shader.Find("Unlit/Color"));
                obstacleMaterial = new Material(soldierMaterial){color=new Color(.45f,.5f,.55f)};

                var cameraObject = Own(new GameObject("Battle Camera"));
                BattleCamera = cameraObject.AddComponent<Camera>();
                BattleCamera.transform.position = new Vector3(0f,6.800000190734863f,-7f);
                BattleCamera.transform.rotation = new Quaternion(.30070576071739197f,0f,0f,.9537169933319092f);
                BattleCamera.orthographic = true;
                // Original gameplay camera46 disables HDR and MSAA.
                BattleCamera.allowHDR = false; BattleCamera.allowMSAA = false;
                BattleCamera.orthographicSize = 2.1f * (.5625f / (Screen.width / (float)Math.Max(1,Screen.height)));
                BattleCamera.nearClipPlane = .3f; BattleCamera.farClipPlane = 1000f;
                BattleCamera.clearFlags = CameraClearFlags.SolidColor;
                BattleCamera.backgroundColor = Color.white;
                BattleCamera.tag = "MainCamera";
                if(Application.isPlaying)cameraObject.AddComponent<AudioListener>();
                BattleCamera.cullingMask &= ~(1<<5);

                var background = Resources.Load<GameObject>("Recovered/Background/HD4_CJ_1");
                if (background != null)
                {
                    var sceneRoot=Own(new GameObject("Recovered Scene_game"));
                    sceneRoot.transform.position=new Vector3(0f,0f,2.265000104904175f);
                    Instantiate(background,sceneRoot.transform,false);
                    backgroundRoot=sceneRoot.transform;
                    SynchronizeBackground(BattleCamera.aspect);
                }

                for (int i=0; i<geometry.StarInfoCfgs.Length; i++)
                {
                    int id=i+1;
                    var data=geometry.StarInfoCfgs[i];
                    if(layout.StarInfoCfgs[i].isBoss) {
                        int camp=layout.StarInfoCfgs[i].CampID;
                        if(camp!=5&&camp!=6)throw new InvalidOperationException("Current source has no recovered Boss model for camp "+camp);
                        var boss=RecoveredBossVisual.Create(camp==5?801:802);var bossRoot=Own(boss.gameObject);bossRoot.name="Tower "+id;bossRoot.layer=8;bossRoot.transform.position=data.pos.World;bossRoot.transform.rotation=Quaternion.identity;
                        boss.ConfigureBossShadow(CampColor(camp));bossVisuals[id]=boss;towerObjects.Add(id,bossRoot);continue;
                    }
                    var root=Own(new GameObject("Tower " + id));
                    root.layer=8; root.transform.position=data.pos.World;
                    var collider=root.AddComponent<BoxCollider>();
                    collider.size=new Vector3(.23f,.4f,.23f); collider.center=new Vector3(0f,.1f,0f);
                    towerObjects.Add(id,root);
                    var visual=new GameObject("Tower Art"); visual.transform.SetParent(root.transform,false);
                    visual.transform.localPosition=new Vector3(0f,.213f,.038f);
                    visual.transform.localRotation=new Quaternion(.3007057607f,0f,0f,.9537169933f);
                    visual.transform.localScale=Vector3.one*.22f;
                    towerSprites[id]=visual.AddComponent<SpriteRenderer>();
                    var label=new GameObject("Score"); label.transform.SetParent(root.transform,false);
                    label.transform.localPosition=new Vector3(0f,.15f,-.2f);
                    label.transform.rotation=BattleCamera.transform.rotation;
                    label.transform.localScale=Vector3.one*.045f;
                    var tm=label.AddComponent<TextMesh>(); tm.font=font; tm.fontSize=48; tm.anchor=TextAnchor.MiddleCenter;
                    tm.characterSize=.65f; tm.color=new Color(.1f,.15f,.22f);
                    label.GetComponent<MeshRenderer>().sharedMaterial=font.material;
                    label.GetComponent<MeshRenderer>().sortingOrder=20;
                    scoreTexts[id]=tm;
                }
                var obstacles=BattleObstacles.Create(layout,transform,null);
                RecoveredObstacleVisual.Attach(layout,obstacles);
                foreach(var obstacle in obstacles)ownedObjects.Add(obstacle);
                Physics.SyncTransforms();
                Simulation=new BattleSimulation(layout,ReadConfig(),Seed,ClearPath,RandomSourceOverride ?? GameRandomSource.Shared.Managed);
                ConfigureSkills(Simulation);
                Simulation.PvPFireballOrigin=()=>SkillProjectileOrigin(1);
                int normal=guideLevel<0?level:guideLevel;
                if(Loadout!=null)CommanderMode=Loadout.CommanderId;
                Guide=new BattleGuide(Simulation,normal,0,CommanderMode);
                var stock=new Dictionary<int,int>();for(int id=1;id<=18;id++)stock[2000+id]=ControlledSkillStock;
                SkillInput=Loadout!=null?Loadout.CreateSkillInput(Simulation,ReadText("Data/AllSkillConfig"),normal):new BattleSkillInput(Simulation,ReadText("Data/AllSkillConfig"),CommanderMode,normal,1,stock);
                Progress=new BattleProgress(normal,RequestedNormalLevel<0);
                Simulation.Event+=OnBattleEvent;
                SkillPresentation=Own(new GameObject("Recovered skill presentation")).AddComponent<BattleSkillPresentation>();SkillPresentation.Initialize(this);
                Guide.Confirmed+=SkillPresentation.OnGuideConfirmed;
                Gestures=Own(new GameObject("Recovered gestures")).AddComponent<RecoveredGestureVisuals>();
                SkillTargets=Own(new GameObject("Recovered skill targets")).AddComponent<BattleSkillTargetVisuals>();
                SkillTargets.Initialize(this);
                Time.timeScale=Simulation.State==BattlePhase.Pause?0f:1f;
                preview=MakeLine("Drag preview", .015f);
                preview.enabled=false;
                selectedTower=0;pressedSkill=-1; drawing=cutting=false; error=null; Initialized=true;
                if(Resources.Load<TextAsset>("Recovered/Hud/hud-import")!=null)
                {
                    Hud=Own(new GameObject("Battle HUD")).AddComponent<BattleHud>();Hud.Initialize(this);
                    Hud.ResumeRequested+=()=>SetPaused(false);Hud.RestartRequested+=RestartCurrentLevel;
                    Hud.PauseRequested+=()=>SetPaused(true);
                    Hud.NextRequested+=()=>InitializeNormalLevel(Progress.SavedLevel);
                }
                RefreshPresentation();
                // LevelControl.OnGamePlayState f7630 calls ShowMyCampEffect when no guide takes over entry.
                if(Guide.Stage==0)SkillPresentation.ShowPlayerCampEffect();
                if(Audio!=null){Audio.BeginBattleMusic();Audio.ApplyPhase(Simulation.State);}
            }
            catch (Exception ex) { error=ex.ToString(); Debug.LogException(ex); Initialized=false; }
        }
        void OnBattleEvent(BattleEvent e)
        {
            if(Audio!=null)Audio.OnBattleEvent(e,Simulation);
            if(e.Kind=="boss-action"&&bossVisuals.TryGetValue(e.TowerId,out var boss)&&bossVisualParameters.TryGetValue(Simulation.Tower(e.TowerId).BossSkillId,out var cfg))boss.BeginBossAction((int)e.Value,cfg);
            // Original framework UIObject.Awake runs when each result window is created.
            // The local panels are preloaded, so their equivalent creation point is result entry.
            if(e.Kind=="phase"&&Audio!=null)
            {if(e.Phase==BattlePhase.Victory)Audio.Play(2009);else if(e.Phase==BattlePhase.Defeat)Audio.Play(2010);}
            if(e.Kind=="restart"){Progress.Retry();lastTowerHit.Clear();foreach(var retryBoss in bossVisuals.Values)retryBoss.ResetBossForRetry();}
            if(e.EffectId==105)ShowSpriteEffect("LevelUp",e.TowerId);
            if(e.Kind=="score"&&e.Amount!=0&&e.SourceCamp!=e.PreviousCamp)
            {
                lastTowerHit.TryGetValue(e.TowerId,out float last);
                if(visualClock-last>.5f){lastTowerHit[e.TowerId]=visualClock;ShowSpriteEffect("hit",e.TowerId);}
            }
            if(e.Kind=="phase"&&Progress.OpenResult(e.Phase))
            {
                // Local reconstruction profile only, adjacent to the playable project/build.
                if(Loadout!=null)Loadout.SetNormalLevel(Progress.SavedLevel);
            }
        }
        void ShowSpriteEffect(string name,int towerId)
        {
            var prefab=Resources.Load<GameObject>("Recovered/Effects/"+name);var tower=Simulation.Tower(towerId);
            if(prefab==null||tower==null)return;
            var obj=Own(Instantiate(prefab));obj.transform.position=tower.Position+Vector3.up*.258f;obj.transform.eulerAngles=Vector3.zero;
            if(name=="LevelUp")obj.GetComponent<SpriteRenderer>().sortingOrder=1;
            var effect=obj.GetComponent<RecoveredSpriteEffect>();effect.Begin(visualClock);spriteEffects.Add(effect);
        }
        static string ProfilePath=>System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../LocalProfile.json"));
        public void RestartCurrentLevel()
        {
            // Source retry reuses tower entities; restore colliders before reconstructing topology.
            foreach(var tower in towerObjects.Values)tower.SetActive(true);
            Physics.SyncTransforms();Time.timeScale=1f;
            if(Hud!=null)for(int slot=0;slot<3;slot++)Hud.MoveSkillArtwork(slot,Vector2.zero,false);
            drawing=cutting=false;selectedTower=0;pressedSkill=-1;preview.enabled=false;
            Simulation.Restart();
            if(Gestures!=null)Gestures.ResetVisuals();
            if(SkillTargets!=null)SkillTargets.ResetForRetry();
            Time.timeScale=Simulation.State==BattlePhase.Pause?0f:1f;
            RefreshPresentation();
            // Original Again(10) re-enters states 3 -> 4 -> 5; non-guide state 5
            // calls ShowMyCampEffect again before entering Running (f7630).
            if(Guide.Stage==0&&SkillPresentation!=null)SkillPresentation.ShowPlayerCampEffect();
        }

        bool ClearPath(int source, int target)
        {
            var a=towerObjects[source]; var b=towerObjects[target];
            foreach (var c in Physics.OverlapCapsule(a.transform.position,b.transform.position,.03f,1<<8))
                if(c.gameObject!=a && c.gameObject!=b) return false;
            return true;
        }
        GameObject Own(GameObject obj) { obj.transform.SetParent(transform,true);ownedObjects.Add(obj); return obj; }
        static void DestroyOwned(UnityEngine.Object obj)
        {
            // Destroy is deferred in a player. Disable old colliders before rebuilding topology.
            if(obj is GameObject go)go.SetActive(false);
            if(Application.isPlaying) Destroy(obj); else DestroyImmediate(obj);
        }

        LineRenderer MakeLine(string name,float width)
        {
            var lr=Own(new GameObject(name)).AddComponent<LineRenderer>();
            lr.sharedMaterial=lineMaterial; lr.positionCount=2; lr.useWorldSpace=true;
            lr.startWidth=lr.endWidth=width; lr.numCapVertices=3; lr.sortingOrder=-5;
            return lr;
        }
        void Update()
        {
            if (!Initialized) return;
            BattleCamera.orthographicSize=2.1f*(.5625f/(Screen.width/(float)Math.Max(1,Screen.height)));
            SynchronizeBackground(BattleCamera.aspect);
            bool skillInput=HandleSkillInput();
            if(!skillInput)HandleInput();
            AdvanceFrame(Time.deltaTime,Time.unscaledDeltaTime);
        }
        // Replay and player share this exact clock/order adapter.
        public void AdvanceFrame(float scaledDelta,float unscaledDelta)
        {
            if(!Initialized)return;
            visualClock+=scaledDelta;
            Shader.SetGlobalFloat("_RecoveredEffectTime",visualClock);
            Guide.Tick(scaledDelta,unscaledDelta);
            foreach(var boss in bossVisuals.Values)boss.StepBossCoroutines(scaledDelta);
            Simulation.TickSkillCoroutines(scaledDelta);
            Simulation.TickSkillProjectiles(scaledDelta);
            Simulation.Tick(scaledDelta);
            Time.timeScale=Simulation.State==BattlePhase.Pause?0f:1f;
            RefreshPresentation();
            if(Hud!=null)Hud.AdvancePresentation(scaledDelta);
            if(SkillPresentation!=null)SkillPresentation.Step(scaledDelta);
            if(Gestures!=null)Gestures.Tick(scaledDelta);
            if(SkillTargets!=null)SkillTargets.Step(scaledDelta);
            foreach(var pair in bossVisuals){if(Simulation.TryGetBossRain(pair.Key,out var point,out bool raining))pair.Value.SynchronizeRain(raining,point);pair.Value.Step(scaledDelta);}
            foreach(var obj in soldierObjects.Values){var bossSoldier=obj.GetComponent<RecoveredBossVisual>();if(bossSoldier!=null)bossSoldier.Step(scaledDelta);}
        }
        // Recovered loaded UICanvas:1080x1920, matchHeight=1; original SkillUI item centers.
        public Vector2 SkillScreenCenter(int slot)
        {
            if(Hud!=null&&Hud.Initialized)return Hud.SkillScreenPosition(slot);
            float[] x={-74.510854f,135.399994f,345.310842f};float scale=Screen.height/1920f;
            return new Vector2(Screen.width*.5f+x[slot]*scale,154f*scale);
        }
        public bool SkillContainsPointer(int slot,Vector2 pointer)
        {
            if(Hud!=null&&Hud.Initialized)
            {
                var rect=Hud.SkillItemTransform(slot).Find("btn_normal") as RectTransform;
                return rect!=null&&rect.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(rect,pointer,Hud.UICamera);
            }
            float size=190f*Screen.height/1920f;var c=SkillScreenCenter(slot);
            return new Rect(c.x-size/2,c.y-size/2,size,size).Contains(pointer);
        }
        Transform backgroundRoot;
        public void SynchronizeBackground(float aspect)
        {
            if(backgroundRoot==null)return;
            // GameControl..cctor f19607 base scale; SetCameraSize f7819 aspect branches.
            float factor=aspect<.5625f?.5625f/aspect:aspect>.5625f?(2f*2.1f*.5625f)/2.5875000953674316f:1f;
            backgroundRoot.localScale=new Vector3(.25f,0f,.4f)*factor;
        }
        Rect SkillScreenRect(int slot)
        {float size=190f*Screen.height/1920f;var c=SkillScreenCenter(slot);return new Rect(c.x-size/2,c.y-size/2,size,size);}
        Vector3 SkillProjectileOrigin(int slot)
        {var c=SkillScreenCenter(slot);return BattleCamera.ScreenToWorldPoint(new Vector3(c.x,c.y,5f));}
        public bool TryUseSkillSlot(int slot,int targetId=0,Vector3? groundPoint=null)
        {return SkillInput.TryUse(slot,targetId,groundPoint,SkillProjectileOrigin(slot));}
        bool HandleSkillInput()
        {
            if(Simulation.State!=BattlePhase.Running)return false;
            var pointer=(Vector2)Input.mousePosition;int over=-1;
            for(int i=0;i<3;i++)if(SkillContainsPointer(i,pointer))over=i;
            if(Input.GetMouseButtonDown(0) && over>=0)
            {
                pressedSkill=over;
                SkillTargets.Hide();
                if(SkillInput.Unlocked(over)&&!SkillInput.InUse(over)&&(SkillInput.Count(over)>0||SkillInput.GenericStock>0)&&SkillInput.Rule(over).useType==1)
                    SkillTargets.BeginDrag(SkillInput.Rule(over).targetType);
                if(SkillInput.Unlocked(over)&&!SkillInput.InUse(over)&&(SkillInput.Count(over)>0||SkillInput.GenericStock>0)&&SkillInput.SkillId(over)==12&&Audio!=null)Audio.Play(2026);
            }
            if(pressedSkill<0)return over>=0;
            bool available=SkillInput.Unlocked(pressedSkill)&&!SkillInput.InUse(pressedSkill)&&(SkillInput.Count(pressedSkill)>0||SkillInput.GenericStock>0);
            if(available&&SkillInput.Rule(pressedSkill).useType==1&&Hud!=null)Hud.MoveSkillArtwork(pressedSkill,pointer,true);
            if(available&&SkillInput.Rule(pressedSkill).useType==1&&Input.GetMouseButton(0))
                SkillTargets.ShowTarget(SkillInput.Rule(pressedSkill).targetType,Simulation.Tower(TowerUnderPointer()));
            if(SkillInput.SkillId(pressedSkill)==18&&SkillInput.Unlocked(pressedSkill)&&!SkillInput.InUse(pressedSkill)&&(SkillInput.Count(pressedSkill)>0||SkillInput.GenericStock>0)&&GroundPoint(out var previewPoint))
                SkillPresentation.PreviewPoison(previewPoint,true);
            if(Input.GetMouseButtonUp(0))
            {
                int slot=pressedSkill;pressedSkill=-1;
                int selectedSkillTarget=SkillTargets.TargetId;SkillTargets.EndDrag();
                if(Hud!=null)Hud.MoveSkillArtwork(slot,pointer,false);
                SkillPresentation.PreviewPoison(Vector3.zero,false);
                if(SkillInput.Rule(slot).useType==0){if(over==slot)TryUseSkillSlot(slot);}
                else
                {
                    Vector3 point;Vector3? ground=GroundPoint(out point)?point:(Vector3?)null;
                    TryUseSkillSlot(slot,selectedSkillTarget,ground);
                }
                return true;
            }
            return true;
        }
        public static void ConfigureSkills(BattleSimulation simulation)
        {
            simulation.ConfigureSkills(JsonUtility.FromJson<SkillTable>(ReadText("Data/SkillConfig")).Datas,
                JsonUtility.FromJson<BossTable>(ReadText("Data/BossConfig")).Datas);
        }
        bool GroundPoint(out Vector3 point)
        {
            var ray=BattleCamera.ScreenPointToRay(Input.mousePosition);
            var plane=new Plane(Vector3.up,Vector3.zero);
            float distance;
            if(plane.Raycast(ray,out distance)){point=ray.GetPoint(distance);return true;}
            point=Vector3.zero;return false;
        }
        int TowerUnderPointer()
        {
            RaycastHit hit;
            if(Physics.Raycast(BattleCamera.ScreenPointToRay(Input.mousePosition),out hit,1000f,1<<8))
                foreach(var pair in towerObjects)if(pair.Value==hit.collider.gameObject)return pair.Key;
            return 0;
        }
        int cachedDragTarget;
        public int CachedDragTarget=>cachedDragTarget;
        // Production input adapter: original down resets target120; move updates it; up submits it.
        public bool BeginTowerDrag(int sourceId)
        {
            cachedDragTarget=0;
            if(Simulation.State!=BattlePhase.Running||!Guide.CanDraw||!towerObjects.ContainsKey(sourceId)||!Guide.IsSourceAllowed(sourceId))return false;
            selectedTower=sourceId;drawing=true;cutting=false;return true;
        }
        public RecoveredGestureVisuals.PreviewResolution MoveTowerDrag(Vector3 groundPoint,int directHitTowerId=0)
        {
            if(!drawing||cutting||selectedTower==0)return default;
            var resolved=RecoveredGestureVisuals.ResolvePreview(towerObjects[selectedTower],groundPoint,
                obj=>towerObjects.ContainsValue(obj),directHitTowerId!=0?towerObjects[directHitTowerId]:null);
            cachedDragTarget=0;
            if(resolved.Valid&&resolved.Target!=null)
                foreach(var pair in towerObjects)if(pair.Value==resolved.Target){cachedDragTarget=pair.Key;break;}
            var sourceLine=Resources.Load<GameObject>("Recovered/WayLines/LineRander_single").GetComponent<RecoveredWayLineVisual>();
            // Boss.Init dynamically assigns Tag2 to the model; this reproduces that semantic marker.
            bool expand=cachedDragTarget!=0&&Simulation.Tower(cachedDragTarget).IsBoss;
            Gestures.DrawPreview(Simulation.Tower(selectedTower).Position,resolved.End,sourceLine.GetCampColor(1),resolved.Valid,
                resolved.Target!=null?resolved.Target.transform:null,expand,visualClock);
            return resolved;
        }
        public bool EndTowerDrag()
        {
            bool connected=drawing&&!cutting&&selectedTower!=0&&cachedDragTarget!=0&&Guide.TryConnect(selectedTower,cachedDragTarget);
            drawing=false;cutting=false;selectedTower=0;cachedDragTarget=0;preview.enabled=false;Gestures.EndGesture();return connected;
        }
        void HandleInput()
        {
            if(Simulation.State!=BattlePhase.Running || !Guide.CanDraw)return;
            Vector3 point;
            if(!GroundPoint(out point))return;
            if(Input.GetMouseButtonDown(0))
            {
                if(Input.mousePosition.y>Screen.height-90 || Input.mousePosition.y<60)return;
                selectedTower=TowerUnderPointer();
                if(selectedTower!=0 && !Guide.IsSourceAllowed(selectedTower))return;
                cachedDragTarget=0;
                if(selectedTower!=0&&!BeginTowerDrag(selectedTower))return;
                if(selectedTower!=0&&Audio!=null)Audio.Play(2015);
                drawing=true;cutting=selectedTower==0;previousGround=point;
            }
            if(drawing && Input.GetMouseButton(0))
            {
                if(cutting)
                {
                    foreach(var line in Simulation.Lines)
                    {
                        var a=Simulation.Tower(line.SmallTowerId).Position;var b=Simulation.Tower(line.LargeTowerId).Position;
                        if(SegmentsCross(previousGround,point,a,b))Guide.TryCut(line.Id);
                    }
                    previousGround=point;
                    Gestures.MoveCut(point,true);
                }
                else
                {
                    MoveTowerDrag(point,TowerUnderPointer());
                }
            }
            if(drawing && Input.GetMouseButtonUp(0))
            {
                EndTowerDrag();
            }
        }
        static bool SegmentsCross(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {
            float rX=b.x-a.x,rZ=b.z-a.z,sX=d.x-c.x,sZ=d.z-c.z;
            float den=rX*sZ-rZ*sX;if(Mathf.Abs(den)<1e-8f)return false;
            float t=((c.x-a.x)*sZ-(c.z-a.z)*sX)/den,u=((c.x-a.x)*rZ-(c.z-a.z)*rX)/den;
            return t>=0 && t<=1 && u>=0 && u<=1;
        }
        public void RefreshPresentation()
        {
            if(Simulation==null)return;
            foreach(var tower in Simulation.Towers)
            {
                towerObjects[tower.Id].SetActive(Guide==null || Guide.IsTowerVisible(tower.Id));
                if(tower.IsBoss)continue;
                string key=Family[Mathf.Clamp(tower.ShipID,1,4)]+(tower.Grade+1)+CampSuffix[Mathf.Clamp(tower.Camp,0,5)];
                Sprite sprite;
                if(!spriteCache.TryGetValue(key,out sprite)){sprite=Resources.Load<Sprite>("Recovered/Towers/"+key);spriteCache[key]=sprite;}
                towerSprites[tower.Id].sprite=sprite;
                towerSprites[tower.Id].enabled=Guide==null || Guide.IsTowerVisible(tower.Id);
                scoreTexts[tower.Id].gameObject.SetActive(Hud==null&&(Guide==null || Guide.IsTowerVisible(tower.Id)));
                scoreTexts[tower.Id].text=((int)tower.Score).ToString();
            }
            foreach(var line in Simulation.Lines)
            {
                var source=Simulation.Tower(line.SmallTowerId);var destination=Simulation.Tower(line.LargeTowerId);
                bool two=RecoveredWayLineVisual.IsDouble(line,source,destination);
                var originalPrefab=Resources.Load<GameObject>("Recovered/WayLines/LineRander_"+(two?"double":"single"));
                if(originalPrefab!=null)
                {
                    if(recoveredLines.TryGetValue(line.Id,out var old)&&old.Double!=two){ownedObjects.Remove(old.gameObject);DestroyOwned(old.gameObject);recoveredLines.Remove(line.Id);}
                    if(!recoveredLines.TryGetValue(line.Id,out var recovered)){recovered=Own(Instantiate(originalPrefab)).GetComponent<RecoveredWayLineVisual>();recoveredLines[line.Id]=recovered;}
                    recovered.Synchronize(line,source,destination,visualClock);
                    continue;
                }
                LineRenderer lr;
                if(!lineObjects.TryGetValue(line.Id,out lr)){lr=MakeLine("Line "+line.Id,.015f);lineObjects[line.Id]=lr;}
                lr.enabled=line.Direction!=0;
                var a=Simulation.Tower(line.SmallTowerId);var b=Simulation.Tower(line.LargeTowerId);
                lr.SetPosition(0,a.Position+Vector3.up*.015f);lr.SetPosition(1,b.Position+Vector3.up*.015f);
                lr.startColor=CampColor(line.Direction==2?b.Camp:a.Camp);
                lr.endColor=CampColor(line.Direction==1?a.Camp:b.Camp);
            }
            var live=new HashSet<int>();
            foreach(var soldier in Simulation.Soldiers)
            {
                if(!soldier.Active)
                {
                    if(soldier.PlayDeathAnimation&&soldierObjects.TryGetValue(soldier.Id,out var dyingObject))
                    {
                        var bossDying=dyingObject.GetComponent<RecoveredBossVisual>();
                        if(bossDying!=null){if(!bossDying.IsDying)bossDying.BeginSoldierDeath(new Vector3(UnityEngine.Random.Range(-.1f,.1f),0,UnityEngine.Random.Range(-.1f,.1f)));bossDying.SynchronizeSoldier(soldier,BattleCamera);if(!bossDying.SoldierDeathFinished)live.Add(soldier.Id);continue;}
                        var dying=dyingObject.GetComponent<RecoveredSoldierVisual>();
                        if(dying!=null){dying.BeginDeath(visualClock);dying.Synchronize(soldier,BattleCamera,visualClock);if(!dying.DeathFinished(visualClock))live.Add(soldier.Id);}
                    }
                    continue;
                }
                live.Add(soldier.Id);
                GameObject visual;
                if(!soldierObjects.TryGetValue(soldier.Id,out visual))
                {
                    int visualSkin=soldier.ShipType==1&&OrdinarySoldierSkinId==102&&(OrdinarySoldierSkinCampMask&(1<<soldier.Camp))!=0?102:soldier.ShipType*100;
                    var prefab=soldier.ShipType>=1&&soldier.ShipType<=3?Resources.Load<GameObject>("Recovered/Soldiers/soldier_"+visualSkin):null;
                    if(soldier.ShipType==11||soldier.ShipType==12){var bossSoldier=RecoveredBossVisual.Create(soldier.ShipType==11?9001:9002);bossSoldier.AttachSoldierShadow();visual=Own(bossSoldier.gameObject);}
                    else if(prefab!=null){visual=Own(Instantiate(prefab));visual.GetComponent<RecoveredSoldierVisual>().Begin(visualClock);}
                    else{
                        visual=Own(GameObject.CreatePrimitive(PrimitiveType.Sphere));
                        DestroyOwned(visual.GetComponent<Collider>());visual.transform.localScale=new Vector3(.065f,.065f,.065f);
                        Material mat;
                        if(!campMaterials.TryGetValue(soldier.Camp,out mat)){mat=new Material(soldierMaterial);mat.color=CampColor(soldier.Camp);campMaterials[soldier.Camp]=mat;}
                        visual.GetComponent<Renderer>().sharedMaterial=mat;
                    }
                    visual.name="Soldier "+soldier.Id;
                    soldierObjects[soldier.Id]=visual;
                }
                var animated=visual.GetComponent<RecoveredSoldierVisual>();
                if(animated!=null)animated.Synchronize(soldier,BattleCamera,visualClock);
                else if(visual.TryGetComponent<RecoveredBossVisual>(out var bossSoldier))bossSoldier.SynchronizeSoldier(soldier,BattleCamera);
                else visual.transform.position=soldier.Position+Vector3.up*.05f;
            }
            var gone=new List<int>();foreach(var pair in soldierObjects)if(!live.Contains(pair.Key))gone.Add(pair.Key);
            foreach(int id in gone){ownedObjects.Remove(soldierObjects[id]);DestroyOwned(soldierObjects[id]);soldierObjects.Remove(id);}
            if(Gestures!=null)Gestures.SynchronizeArrows(Simulation.Arrows);
            if(Hud!=null)Hud.Synchronize();
            for(int i=spriteEffects.Count-1;i>=0;i--)
                if(spriteEffects[i]==null||!spriteEffects[i].Advance(visualClock))
                {if(spriteEffects[i]!=null){ownedObjects.Remove(spriteEffects[i].gameObject);DestroyOwned(spriteEffects[i].gameObject);}spriteEffects.RemoveAt(i);}
        }
        void SetPaused(bool value){if(Guide.PromptVisible)return;Simulation.Pause(value);Time.timeScale=value?0f:1f;}
        void OnGUI()
        {
            if(textStyle==null){textStyle=new GUIStyle(GUI.skin.label){font=font,fontSize=24,alignment=TextAnchor.MiddleCenter};textStyle.normal.textColor=new Color(.12f,.18f,.25f);buttonStyle=new GUIStyle(GUI.skin.button){font=font,fontSize=20};}
            if(error!=null){GUI.Label(new Rect(10,10,Screen.width-20,Screen.height-20),error);return;}
            if(!Initialized)return;
            if(Hud!=null)return;
            GUI.Label(new Rect(0,10,Screen.width,45),"关卡 "+LevelId,textStyle);
            if(GUI.Button(new Rect(14,16,80,42),Simulation.State==BattlePhase.Pause?"继续":"暂停",buttonStyle))SetPaused(Simulation.State!=BattlePhase.Pause);
            if(GUI.Button(new Rect(Screen.width-94,16,80,42),"重试",buttonStyle))RestartCurrentLevel();
            GUI.Label(new Rect(0,Screen.height-50,Screen.width,40),"拖动蓝塔连线 · 从空地划过线路断开",new GUIStyle(textStyle){fontSize=17});
            for(int slot=0;Hud==null&&slot<3;slot++)
            {
                var rect=SkillScreenRect(slot);rect.y=Screen.height-rect.yMax;
                string name=SkillInput.Rule(slot).useType==1?"拖动施放":"点击施放";
                string caption="技能 "+SkillInput.SkillId(slot)+"\n"+(SkillInput.Unlocked(slot)?(SkillInput.InUse(slot)?"生效中":name+" ×"+SkillInput.Count(slot)):"第"+SkillInput.Rule(slot).unLockLevel+"关解锁");
                GUI.Box(rect,caption,new GUIStyle(buttonStyle){fontSize=15,wordWrap=true});
                if(Guide.HighlightSkillIndex==slot)GUI.Label(new Rect(rect.x,rect.y-35,rect.width,30),"↓",textStyle);
            }
            if(Guide.PromptVisible)
            {
                if(Hud!=null)return;
                var rect=new Rect(30,Screen.height*.3f,Screen.width-60,310);GUI.Box(rect,"");
                GUI.Label(new Rect(rect.x+15,rect.y+10,rect.width-30,50),Guide.Prompt.Title,textStyle);
                GUI.Label(new Rect(rect.x+25,rect.y+65,rect.width-50,145),Guide.Prompt.Description,new GUIStyle(textStyle){wordWrap=true,fontSize=22,richText=true});
                GUI.enabled=Guide.CanConfirm;
                if(GUI.Button(new Rect(rect.x+45,rect.y+230,rect.width-90,50),Guide.CanConfirm?Guide.Prompt.Button:Mathf.CeilToInt(Guide.ConfirmSecondsRemaining).ToString(),buttonStyle))
                {Guide.Confirm();Time.timeScale=Simulation.State==BattlePhase.Pause?0f:1f;}
                GUI.enabled=true;return;
            }
            string outcome=Simulation.State==BattlePhase.Victory?"胜利":Simulation.State==BattlePhase.Defeat?"失败":Simulation.State==BattlePhase.Pause?"已暂停":null;
            if(outcome!=null && !(Hud!=null&&Simulation.State==BattlePhase.Pause))
            {
                var rect=new Rect(Screen.width*.2f,Screen.height*.4f,Screen.width*.6f,140);
                GUI.Box(rect,"");GUI.Label(new Rect(rect.x,rect.y+12,rect.width,55),outcome,textStyle);
                if(GUI.Button(new Rect(rect.x+30,rect.y+80,rect.width-60,42),Simulation.State==BattlePhase.Pause?"继续":"重新挑战",buttonStyle))
                {if(Simulation.State==BattlePhase.Pause)SetPaused(false);else RestartCurrentLevel();}
                if(Simulation.State==BattlePhase.Victory&&!Progress.Special&&GUI.Button(new Rect(rect.x+30,rect.y+140,rect.width-60,42),"下一关",buttonStyle))
                    InitializeNormalLevel(Progress.SavedLevel);
            }
        }
    }
}


