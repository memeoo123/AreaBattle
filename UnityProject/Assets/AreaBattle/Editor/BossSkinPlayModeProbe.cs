using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    // Diagnostic real player-loop capture; never changes the production skinning path.
    public static class BossSkinPlayModeProbe
    {
        const string Key="AreaBattle.BossSkinPlayModeProbe";
        static BattleView view;static int frames;static GameObject host;static SkinnedMeshRenderer skin;
        [InitializeOnLoadMethod] static void Resume()
        {if(SessionState.GetBool(Key,false)){EditorApplication.playModeStateChanged-=Changed;EditorApplication.playModeStateChanged+=Changed;}}
        public static void Run()
        {
            RecoveredSkillEffectImporter.Import();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SessionState.SetBool(Key,true);Resume();EditorApplication.EnterPlaymode();
        }
        static void Changed(PlayModeStateChange state)
        {
            if(state==PlayModeStateChange.EnteredPlayMode)
            {
                host=new GameObject("Real player-loop Boss skin probe");view=host.AddComponent<BattleView>();view.enabled=false;view.InitializeSpecialScene(99999);
                view.Simulation.CastSkill(1,1);view.AdvanceFrame(0,0);var boss=view.Simulation.Towers.Single(t=>t.IsBoss);
                skin=view.TowerPresentationTransform(boss.Id).Find("skill_TM").GetComponentInChildren<SkinnedMeshRenderer>();frames=0;
                EditorApplication.update+=Frame;
            }
            if(state==PlayModeStateChange.EnteredEditMode){SessionState.SetBool(Key,false);EditorApplication.Exit(0);}
        }
        static void Frame()
        {
            if(view==null)return;
            view.AdvanceFrame(1f/60f,1f/60f);frames++;
            if(frames==30||frames==60||frames==120||frames==360)
            {
                Debug.Log("BOSS_REAL_FRAME "+frames+" supported="+skin.sharedMaterial.shader.isSupported+" bounds="+skin.bounds+" active="+skin.gameObject.activeInHierarchy);
                BattleBuild.Capture(view,"boss-ice-playmode-"+frames+"frames.png");
            }
            if(frames>=361){EditorApplication.update-=Frame;UnityEngine.Object.Destroy(host);EditorApplication.ExitPlaymode();}
            else EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
