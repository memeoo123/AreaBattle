using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Spine.Unity;

namespace AreaBattle
{
    // UGUI presentation adapter; simulation and skill targeting remain authoritative outside this class.
    public sealed class BattleHud : MonoBehaviour
    {
        [Serializable] sealed class Manifest { public CanvasData canvas;public SpriteEntry[] sprites;public SkillIcon[] skillIcons; }
        [Serializable] sealed class CanvasData { public Vector2 referenceResolution;public float match,planeDistance,cameraSize,cameraDepth,nearClip,farClip;public Vector3 cameraPosition;public Quaternion cameraRotation; }
        [Serializable] sealed class SpriteEntry { public string id,name; }
        [Serializable] sealed class SkillIcon { public int skill;public string name; }
        sealed class TowerLabel
        {
            public RectTransform Root;public Text Score;public readonly Image[] Slots=new Image[3],Insets=new Image[3];
            public int CachedCamp=int.MinValue,InitialGrade;public bool GradeChanged;public Color CampColor;
        }
        BattleView view;
        GameObject owner,guidePanel,pausePanel,topBar,victoryPanel,defeatPanel;
        RectTransform towerLayer,skillPanel;
        readonly Dictionary<int,TowerLabel> labels=new Dictionary<int,TowerLabel>();
        readonly Dictionary<string,Sprite> spriteByName=new Dictionary<string,Sprite>();
        readonly Dictionary<int,string> skillIcons=new Dictionary<int,string>();
        readonly Dictionary<int,int> shipKinds=new Dictionary<int,int>();
        readonly RectTransform[] skillItems=new RectTransform[3];
        Slider bossSlider;
        RecoveredGuidePresentation guidePresentation;
        BattleSimulation lastSimulation;
        BattlePhase lastPresentationPhase;
        SkeletonAnimation commander;
        bool commanderCasting,battleHudClosed;
        CanvasGroup skillFade;
        float skillCloseElapsed;
        const float SkillCloseDuration=.3f;
        public SkeletonAnimation Commander=>commander;
        public Canvas Canvas { get; private set; }
        public Camera UICamera { get; private set; }
        public bool Initialized { get { return Canvas!=null; } }
        // View supplies the same targeting operation already used by its input system.
        public event Action<int,Vector2> SkillPointerDown;
        public event Action<int,Vector2> SkillPointerUp;
        public event Action RestartRequested;
        public event Action ResumeRequested;
        public event Action PauseRequested;
        public event Action NextRequested;

        public void Initialize(BattleView battleView)
        {
            if(battleView==null)throw new ArgumentNullException(nameof(battleView));
            Clear();view=battleView;
            var asset=Resources.Load<TextAsset>("Recovered/Hud/hud-import");
            if(asset==null)throw new InvalidOperationException("Run RecoveredHudImporter.Import before creating BattleHud.");
            var manifest=JsonUtility.FromJson<Manifest>(asset.text);var c=manifest.canvas;
            owner=new GameObject("Recovered in-level UGUI");owner.transform.SetParent(transform,false);
            var cameraObject=new GameObject("Recovered UICamera",typeof(Camera));cameraObject.transform.SetParent(owner.transform,false);
            cameraObject.transform.localPosition=c.cameraPosition;cameraObject.transform.localRotation=c.cameraRotation;
            UICamera=cameraObject.GetComponent<Camera>();UICamera.orthographic=true;UICamera.orthographicSize=c.cameraSize;
            UICamera.nearClipPlane=c.nearClip;UICamera.farClipPlane=c.farClip;UICamera.depth=c.cameraDepth;UICamera.clearFlags=CameraClearFlags.Depth;UICamera.cullingMask=1<<5;UICamera.allowHDR=false;UICamera.allowMSAA=false;
            var canvasObject=new GameObject("Recovered UICanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvasObject.layer=5;canvasObject.transform.SetParent(owner.transform,false);
            Canvas=canvasObject.GetComponent<Canvas>();Canvas.renderMode=RenderMode.ScreenSpaceCamera;Canvas.worldCamera=UICamera;Canvas.planeDistance=c.planeDistance;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=c.referenceResolution;scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=c.match;scaler.referencePixelsPerUnit=100;
            towerLayer=Layer("StarInfoRoot",canvasObject.transform);
            topBar=Clone("PlayTopBar",canvasObject.transform);
            Wire(topBar,"LeftContainer/PauseBtn",()=>{if(view.Guide!=null&&view.Guide.PromptVisible)return;if(PauseRequested!=null)PauseRequested();else view.Simulation.Pause(true);});
            Wire(topBar,"LeftContainer/btn_again",()=>{if(RestartRequested!=null)RestartRequested();else view.RestartCurrentLevel();});
            Show(topBar.transform,"LeftContainer/btn_swift",false);Show(topBar.transform,"LeftContainer/btn_jump",false);
            // PlayUI.f11010 explicitly hides the ordinary camp bar. Boss slider has its own score-event consumer.
            Show(topBar.transform,"ProgressSlider/go_normal",false);Show(topBar.transform,"ProgressSlider/slider_boss",false);
            bossSlider=topBar.transform.Find("ProgressSlider/slider_boss").GetComponent<Slider>();
            if(bossSlider==null)throw new InvalidOperationException("Reimport recovered HUD to restore the original Boss Slider component.");
            foreach(var ship in BattleView.ReadConfig().Ships)shipKinds[ship.id]=ship.shipType;
            skillPanel=(RectTransform)Clone("SkillUI",canvasObject.transform).transform;
            skillFade=skillPanel.GetComponent<CanvasGroup>();
            if(skillFade==null)skillFade=skillPanel.gameObject.AddComponent<CanvasGroup>();
            skillFade.alpha=1;skillCloseElapsed=0;
            battleHudClosed=false;CreateCommander();
            var layout=(RectTransform)skillPanel.Find("main/layout");var template=skillPanel.Find("SkillItem").gameObject;
            for(int i=0;i<3;i++)
            {
                int slot=i;var item=Instantiate(template,layout,false);item.name="SkillItem_"+i;item.SetActive(true);skillItems[i]=(RectTransform)item.transform;
                // Advertisement, purchase and original world MeshCollider handlers are outside the in-level fixture.
                foreach(string path in new[]{"btn_ad","ad_mask","btn_zshi","go_toolValue","meshCollider"})Show(item.transform,path,false);
                var button=item.transform.Find("btn_normal").gameObject;
                var trigger=button.AddComponent<EventTrigger>();
                AddTrigger(trigger,EventTriggerType.PointerDown,e=>SkillPointerDown?.Invoke(slot,((PointerEventData)e).position));
                AddTrigger(trigger,EventTriggerType.PointerUp,e=>SkillPointerUp?.Invoke(slot,((PointerEventData)e).position));
            }
            template.SetActive(false);
            victoryPanel=Clone("VictoryUI",canvasObject.transform);defeatPanel=Clone("DefeatUI",canvasObject.transform);
            Show(victoryPanel.transform,"objBtn/go_common",true);
            Show(victoryPanel.transform,"objBtn/go_common/btn_normalGold2/Image",false);
            Wire(victoryPanel,"objBtn/go_common/btn_normalGold2",()=>
            {
                if(view.Progress!=null && view.Progress.Special){if(RestartRequested!=null)RestartRequested();else view.RestartCurrentLevel();}
                else NextRequested?.Invoke();
            });
            Show(defeatPanel.transform,"ResBtn/BtnAD",false);
            Wire(defeatPanel,"ResBtn/btn_again",()=>{if(RestartRequested!=null)RestartRequested();else view.RestartCurrentLevel();});
            guidePanel=Clone("GuideUI",canvasObject.transform);pausePanel=Clone("PauseUI",canvasObject.transform);
            // Their source BaseUI.UINode is 2 (EUINode.UIPopup); retain the group sorting layer
            // so the original UIPopup commander renderer participates in modal depth sorting.
            foreach(var popup in new[]{guidePanel,pausePanel})
            {
                var popupCanvas=popup.AddComponent<Canvas>();popupCanvas.overrideSorting=true;popupCanvas.sortingLayerName="UIPopup";
                popup.AddComponent<GraphicRaycaster>();
            }
            guidePresentation=guidePanel.GetComponent<RecoveredGuidePresentation>();
            if(guidePresentation==null)throw new InvalidOperationException("Reimport recovered HUD to restore original Guide presentation.");
            guidePresentation.Initialize();
            Wire(guidePanel,"mainbg/OKBtn",()=>{if(view.Guide!=null && view.Guide.CanConfirm)view.Guide.Confirm();});
            Wire(pausePanel,"main/btnRoot/RunBtn",()=>{if(ResumeRequested!=null)ResumeRequested();else view.Simulation.Pause(false);});
            Wire(pausePanel,"main/btnRoot/btn_again",()=>{if(RestartRequested!=null)RestartRequested();else view.RestartCurrentLevel();});
            Wire(pausePanel,"main/btn_sound",()=>{if(view.Audio!=null){view.Audio.Play(2001,null,false,true);view.Audio.SetSound(!view.Audio.SoundEnabled);}});
            Wire(pausePanel,"main/btn_music",()=>{if(view.Audio!=null){view.Audio.Play(2001,null,false,true);view.Audio.SetMusic(!view.Audio.MusicEnabled);}});
            foreach(string path in new[]{"main/btnRoot/btn_home","main/btn_shock"})
            {var b=pausePanel.transform.Find(path)?.GetComponent<Button>();if(b!=null)b.interactable=false;}
            foreach(var entry in manifest.sprites)
            {
                string path="Recovered/Hud/Sprites/"+entry.id.Replace(':','_').Replace('/','_');
                if(!spriteByName.ContainsKey(entry.name))spriteByName.Add(entry.name,Resources.Load<Sprite>(path));
            }
            foreach(var icon in manifest.skillIcons)skillIcons[icon.skill]=icon.name;
            if(EventSystem.current==null){var events=new GameObject("Recovered EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(owner.transform,false);}
            UnityEngine.Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            Synchronize();
        }
        public void AdvancePresentation(float scaledDelta)
        {
            if(!Initialized)return;
            Synchronize();
            foreach(var animation in owner.GetComponentsInChildren<RecoveredUiAnimation>(true))animation.Advance(scaledDelta);
            guidePresentation.Advance(scaledDelta);
            if(battleHudClosed&&skillPanel.gameObject.activeSelf)
            {
                skillCloseElapsed+=scaledDelta;
                // SkillUI.CloseAnim=2. UIModule default fade is .3s; DOTween default OutQuad.
                float remaining=1-Mathf.Clamp01(skillCloseElapsed/SkillCloseDuration);
                skillFade.alpha=remaining*remaining;
                if(skillCloseElapsed>=SkillCloseDuration)skillPanel.gameObject.SetActive(false);
            }
            if(!battleHudClosed){commander.Update(scaledDelta);commander.LateUpdate();}
        }
        void CreateCommander()
        {
            if(commander!=null)DestroyHudObject(commander.gameObject);
            var prefab=Resources.Load<GameObject>("Recovered/Commanders/HD_ZHG_00"+view.CommanderMode);
            if(prefab==null)throw new InvalidOperationException("Import original in-level commander resources first.");
            var obj=Instantiate(prefab,skillPanel.Find("main/commandRoot"),false);
            // SkillUI callback f15922; a reopened source SkillUI has a fresh skeleton/state.
            obj.transform.localPosition=new Vector3(0,0,3);obj.transform.localScale=new Vector3(70,70,0);
            commander=obj.GetComponent<SkeletonAnimation>();commander.Initialize(false);commander.enabled=false;
            commanderCasting=false;commander.Update(0);commander.LateUpdate();
        }
        void SetBattleHudVisible(bool visible)
        {
            // f7630 closes PlayUI on either result; PlayUI.CloseBefore f18063 closes SkillUI.
            // Retain the preloaded UGUI objects, but recreate commander state when reopened.
            if(visible&&battleHudClosed)
            {
                CreateCommander();skillCloseElapsed=0;skillFade.alpha=1;
                skillFade.interactable=true;skillFade.blocksRaycasts=true;
            }
            if(!visible&&!battleHudClosed)
            {
                // SkillUI.CloseBefore f15926 hides its Spine object before UIUtils.ObjectAnim.
                commander.gameObject.SetActive(false);skillCloseElapsed=0;
                skillFade.interactable=false;skillFade.blocksRaycasts=false;
            }
            battleHudClosed=!visible;
            topBar.SetActive(visible);towerLayer.gameObject.SetActive(visible);
            if(visible)skillPanel.gameObject.SetActive(true);
            if(!visible&&guidePanel!=null)guidePanel.SetActive(false);
        }
        static RectTransform Layer(string name,Transform parent)
        {var go=new GameObject(name,typeof(RectTransform));go.layer=5;var rt=(RectTransform)go.transform;rt.SetParent(parent,false);rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;return rt;}
        static GameObject Clone(string name,Transform parent)
        {var prefab=Resources.Load<GameObject>("Recovered/Hud/"+name);if(prefab==null)throw new InvalidOperationException("Missing recovered HUD prefab "+name);var go=Instantiate(prefab,parent,false);go.name=name;go.SetActive(true);return go;}
        static void AddTrigger(EventTrigger trigger,EventTriggerType type,Action<BaseEventData> action)
        {var entry=new EventTrigger.Entry{eventID=type};entry.callback.AddListener(e=>action(e));trigger.triggers.Add(entry);}
        static void Wire(GameObject root,string path,Action action)
        {var button=root.transform.Find(path)?.GetComponent<Button>();if(button==null)throw new InvalidOperationException("Missing source button "+path);button.onClick.AddListener(()=>action());}
        static void Show(Transform root,string path,bool value){var t=root.Find(path);if(t!=null)t.gameObject.SetActive(value);}
        static void TextAt(Transform root,string path,string value){var t=root.Find(path)?.GetComponent<Text>();if(t!=null)t.text=value;}
        void LateUpdate(){if(Initialized)Synchronize();}
        public Vector2 SkillScreenPosition(int slot)
        {if(slot<0||slot>2)throw new ArgumentOutOfRangeException(nameof(slot));return RectTransformUtility.WorldToScreenPoint(UICamera,skillItems[slot].position);}
        public Transform SkillItemTransform(int slot)
        {if(slot<0||slot>2)throw new ArgumentOutOfRangeException(nameof(slot));return skillItems[slot];}
        public Vector3 SkillProjectileOrigin(int slot)
        {var p=SkillScreenPosition(slot);return view.BattleCamera.ScreenToWorldPoint(new Vector3(p.x,p.y,5));}
        public void MoveSkillArtwork(int slot,Vector2 pointer,bool dragging)
        {
            if(slot<0||slot>=skillItems.Length||skillItems[slot]==null)return;
            var cover=skillItems[slot].Find("img_cover");
            // SkillControl pointer handlers project (x,y+50,5) through the UI camera.
            // SkillItem.SetCoverPosition(false) writes world; release(true) restores local zero.
            if(dragging)cover.position=UICamera.ScreenToWorldPoint(new Vector3(pointer.x,pointer.y+50f,5f));
            else cover.localPosition=Vector3.zero;
        }
        public void Synchronize()
        {
            if(view==null || view.Simulation==null || view.BattleCamera==null)return;
            var sim=view.Simulation;
            SetBattleHudVisible(sim.State!=BattlePhase.Victory&&sim.State!=BattlePhase.Defeat);
            bool special=view.Progress!=null&&view.Progress.Special;
            int displayedLevel=view.Progress!=null?view.Progress.SelectedLevel:view.LevelId;
            TextAt(topBar.transform,"ProgressSlider/LevelBg/LevelText",special?"挑战关":"关卡 "+displayedLevel);
            Show(topBar.transform,"ProgressSlider/LevelBg",special||displayedLevel>=1);
            victoryPanel.SetActive(sim.State==BattlePhase.Victory);defeatPanel.SetActive(sim.State==BattlePhase.Defeat);
            if(lastPresentationPhase!=sim.State)
            {
                lastPresentationPhase=sim.State;
                if(sim.State==BattlePhase.Victory||sim.State==BattlePhase.Defeat)
                    foreach(var animation in (sim.State==BattlePhase.Victory?victoryPanel:defeatPanel).GetComponentsInChildren<RecoveredUiAnimation>(true))animation.ResetPlayback();
            }
            TextAt(victoryPanel.transform,"objBtn/go_common/btn_normalGold2/txt_rewardGold2",special?"重新挑战":"下一关");
            topBar.transform.Find("LeftContainer/PauseBtn").GetComponent<Button>().interactable=sim.State==BattlePhase.Running&&(view.Guide==null||!view.Guide.PromptVisible);
            if(lastSimulation!=sim)
            {
                if(lastSimulation!=null)lastSimulation.Event-=OnBattleEvent;
                ClearTowerLabels();lastSimulation=sim;lastSimulation.Event+=OnBattleEvent;RefreshBossSlider();
            }
            var visible=new HashSet<int>();if(view.Guide!=null)foreach(int id in view.Guide.VisibleTowerIds)visible.Add(id);
            foreach(var tower in sim.Towers)
            {
                // Original PlayUI.f11012 excludes Boss before allocating a TowerCanvas.
                if(tower.IsBoss)
                {
                    if(labels.TryGetValue(tower.Id,out var previous)){DestroyHudObject(previous.Root.gameObject);labels.Remove(tower.Id);}
                    continue;
                }
                if(!labels.TryGetValue(tower.Id,out var label))
                {
                    var root=Clone("TowerCanvas",towerLayer);root.name="TowerCanvas_"+tower.Id;
                    label=new TowerLabel{Root=(RectTransform)root.transform,Score=root.transform.Find("ScoreNum").GetComponent<Text>(),InitialGrade=tower.Grade};labels.Add(tower.Id,label);
                    int kind;shipKinds.TryGetValue(tower.ShipID,out kind);
                    string selected=kind==1?"normal":kind==2?"defense":kind==3?"attack":null;
                    foreach(string family in new[]{"normal","defense","attack"})Show(root.transform,family,family==selected);
                    Show(root.transform,"Image",false);
                    if(selected!=null)
                    {
                        var family=root.transform.Find(selected);
                        for(int i=0;i<3;i++)
                        {
                            label.Slots[i]=family.GetChild(i).GetComponent<Image>();
                            if(kind!=1)label.Insets[i]=family.GetChild(i).GetChild(0).GetComponent<Image>();
                        }
                    }
                }
                label.Root.gameObject.SetActive(tower.Active && (view.Guide==null || view.Guide.Stage==0 || visible.Contains(tower.Id)));
                // Init f10061/f9712 offsets in UI world space; later grade changes use f10060's battle-world offset.
                if(tower.Grade!=label.InitialGrade)label.GradeChanged=true;
                var world=tower.Position+Vector3.up*(.3f+(label.GradeChanged?.1f+(sim.GetDispatchLineNum(tower.Grade)-1)*.05f:0f));
                var screen=view.BattleCamera.WorldToScreenPoint(world);
                if(!label.GradeChanged)screen.y+=(.1f+label.InitialGrade*.15f)*UICamera.pixelHeight/(2f*UICamera.orthographicSize);
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(towerLayer,screen,UICamera,out var point))label.Root.anchoredPosition=point;
                label.Score.text=tower.Score>=tower.MaxScore?"Max":Mathf.Max(0,(int)tower.Score).ToString();
                RefreshCapacity(label,tower,label.CachedCamp!=tower.Camp);
            }
            if(view.SkillInput!=null)
            {
                for(int i=0;i<3;i++)
                {
                    var item=skillItems[i];bool unlocked=view.SkillInput.Unlocked(i);bool inUse=view.SkillInput.InUse(i);
                    // Source img_cover is the persistent skill artwork, not a cooldown overlay.
                    Show(item,"img_lock",!unlocked);Show(item,"img_cover",true);Show(item,"img_progress",true);Show(item,"go_num",unlocked&&view.SkillInput.Count(i)>0);
                    // SkillItem f10347 shows the native cost footer when using generic item points.
                    Show(item,"go_toolValue",unlocked&&view.SkillInput.Count(i)<=0&&view.SkillInput.GenericStock>0);
                    var skill=sim.FindSkill(view.SkillInput.SkillId(i));
                    item.Find("img_progress").GetComponent<Image>().fillAmount=inUse&&skill.Parameters.duration>0?skill.Elapsed/skill.Parameters.duration:0;
                    TextAt(item,"go_num/txt_num",view.SkillInput.Count(i).ToString());TextAt(item,"img_lock/txt_lockTips","第 "+view.SkillInput.Rule(i).unLockLevel+" 关解锁");
                    var button=item.Find("btn_normal").GetComponent<Button>();button.interactable=unlocked&&!inUse;
                    if(skillIcons.TryGetValue(view.SkillInput.SkillId(i),out string name) && spriteByName.TryGetValue(name,out var icon))item.Find("img_cover").GetComponent<Image>().sprite=icon;
                }
                TextAt(skillPanel,"main/Item/textItem","道具点："+view.SkillInput.GenericStock);
            }
            var guide=view.Guide;bool prompt=guide!=null&&guide.PromptVisible&&guide.Prompt!=null;
            guidePanel.SetActive(!battleHudClosed&&guide!=null&&guide.Stage>0);pausePanel.SetActive(sim.State==BattlePhase.Pause&&!prompt);
            guidePresentation.Synchronize(guide,id=>labels[id].Root.position,index=>skillItems[index].position);
            if(view.Audio!=null)
            {
                Show(pausePanel.transform,"main/btn_sound/img_sound_on",view.Audio.SoundEnabled);
                Show(pausePanel.transform,"main/btn_sound/img_sound_off",!view.Audio.SoundEnabled);
                Show(pausePanel.transform,"main/btn_music/img_music_on",view.Audio.MusicEnabled);
                Show(pausePanel.transform,"main/btn_music/img_music_off",!view.Audio.MusicEnabled);
            }
            if(prompt)
            {
                TextAt(guidePanel.transform,"mainbg/TitleBG/guideTittle",guide.Prompt.Title);TextAt(guidePanel.transform,"mainbg/guideContent",guide.Stage==7||guide.Stage==8?string.Empty:guide.Prompt.Description);
                TextAt(guidePanel.transform,"mainbg/OKBtn/OKText",guide.CanConfirm?guide.Prompt.Button:guide.Prompt.Button+"("+Mathf.CeilToInt(guide.ConfirmSecondsRemaining)+")");
                guidePanel.transform.Find("mainbg/OKBtn").GetComponent<Button>().interactable=guide.CanConfirm;
                if(spriteByName.TryGetValue(guide.Prompt.Picture,out var picture)){var img=guidePanel.transform.Find("mainbg/guideIcon").GetComponent<Image>();if(img!=null)img.sprite=picture;}
            }
        }
        static void RefreshCapacity(TowerLabel label,TowerState tower,bool refreshColor)
        {
            for(int i=0;i<3;i++)
            {
                var image=label.Slots[i];if(image==null)continue;
                image.gameObject.SetActive(tower.OutgoingCount>i||tower.MaxLines>i);
                var color=tower.OutgoingCount>i?label.CampColor:Color.white;image.color=color;
                if(label.Insets[i]!=null)
                {
                    var delta=new Vector4(color.r-1,color.g-1,color.b-1,color.a-1);
                    label.Insets[i].color=delta.sqrMagnitude<9.99999944e-11f?new Color(.42f,.46f,.48f,1):Color.white;
                }
            }
            // Source RefreshStarCanvas calls SetLineNum before replacing the cached camp color.
            if(refreshColor){label.CachedCamp=tower.Camp;label.CampColor=BattleView.CampColor(tower.Camp);}
        }
        void RefreshBossSlider()
        {
            TowerState current=null;foreach(var tower in lastSimulation.Towers)if(tower.IsBoss)current=tower;
            bossSlider.gameObject.SetActive(current!=null);
            if(current!=null){bossSlider.maxValue=current.MaxScore;bossSlider.value=current.MaxScore;}
        }
        void OnBattleEvent(BattleEvent e)
        {
            if(e.Kind=="phase"&&(e.Phase==BattlePhase.Victory||e.Phase==BattlePhase.Defeat))SetBattleHudVisible(false);
            if(e.Kind=="skill-start"&&e.Camp==BattleSimulation.PlayerCampID&&!battleHudClosed&&!commanderCasting&&commander!=null)
            {
                commanderCasting=true;
                // SkillUI f15918 registers Complete when a cast starts, not on prefab load.
                // Before the first cast, the original idle track loops without being replaced.
                commander.AnimationState.Complete+=OnCommanderComplete;
                if(view.Simulation.State==BattlePhase.Running)commander.AnimationState.SetAnimation(0,"skill",false);
            }
            if(e.Kind=="restart"){ClearTowerLabels();RefreshBossSlider();return;}
            var tower=lastSimulation.Tower(e.TowerId);if(tower==null)return;
            if(e.Kind=="score"&&tower.IsBoss)bossSlider.value=(int)e.Value;
            if((e.Kind=="score"||e.Kind=="capture")&&labels.TryGetValue(tower.Id,out var label))RefreshCapacity(label,tower,e.Kind=="capture");
        }
        void OnCommanderComplete(Spine.TrackEntry entry)
        {
            commanderCasting=false;
            if(view.Simulation.State==BattlePhase.Running)commander.AnimationState.SetAnimation(0,"idle",true);
        }
        static void DestroyHudObject(GameObject obj){if(Application.isPlaying)Destroy(obj);else DestroyImmediate(obj);}
        void ClearTowerLabels(){foreach(var label in labels.Values)DestroyHudObject(label.Root.gameObject);labels.Clear();}
        void OnDestroy(){Clear();}
        void Clear(){if(lastSimulation!=null)lastSimulation.Event-=OnBattleEvent;if(owner!=null){owner.SetActive(false);DestroyHudObject(owner);}owner=null;Canvas=null;UICamera=null;bossSlider=null;labels.Clear();spriteByName.Clear();skillIcons.Clear();shipKinds.Clear();lastSimulation=null;}
    }
}
