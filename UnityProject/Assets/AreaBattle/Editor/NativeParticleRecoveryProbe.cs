using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class NativeParticleRecoveryProbe
    {
        public static void Run()
        {
            GameObject obj=null;
            try{
                string path="Assets/AreaBattle/Experimental/NativeParticleProbe.prefab";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new Exception("Native prefab import failed");
                obj=UnityEngine.Object.Instantiate(prefab);var particle=obj.GetComponent<ParticleSystem>();
                File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/particle-json-roundtrip.json"),EditorJsonUtility.ToJson(particle,true));
                if(Mathf.Abs(particle.main.duration-3)>.00001f||particle.main.loop||
                    particle.main.startLifetime.mode!=ParticleSystemCurveMode.TwoConstants||
                    Mathf.Abs(particle.main.startLifetime.constantMax-1.7f)>.00001f||
                    Mathf.Abs(particle.main.startSpeed.constantMin-4)>.00001f)
                    throw new Exception("Native particle source serialization was not applied");
                var clock=RecoveredEffectVisual.Attach(obj);clock.Step(.3f);
                if(Mathf.Abs(particle.time-.3f)>.0001f||particle.isPlaying)throw new Exception("Explicit source particle clock failed");
                int count=particle.particleCount;clock.Step(0);
                if(Mathf.Abs(particle.time-.3f)>.0001f||particle.particleCount!=count)throw new Exception("Paused particle clock advanced");
                clock.Step(.3f);if(Mathf.Abs(particle.time-.6f)>.0001f)throw new Exception("Particle failed to resume on explicit clock");
                Debug.Log("AREABATTLE_NATIVE_PARTICLE_YAML_PASS");
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }catch(Exception ex){Debug.LogException(ex);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{UnityEngine.Object.DestroyImmediate(obj);}
        }
    }
}
