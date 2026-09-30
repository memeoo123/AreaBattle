using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class RecoveredAudioImporter
    {
        [Serializable] sealed class Manifest {public Clip[] clips;}
        [Serializable] sealed class Clip {public int id;public string path;}
        [Serializable] sealed class Report {public bool passed;public int clips;public string scope="Original locally recovered AAC decoded to PCM float32; unacquired clip IDs remain explicit in source manifest.";}
        public static void Import()
        {
            string path=Path.Combine(BattleBuild.Target,"generated/presentation-prepared/audio-runtime.json");
            var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(path));const string dest="Assets/AreaBattle/Resources/Recovered/Audio";Directory.CreateDirectory(dest);
            File.Copy(path,dest+"/audio-runtime.json",true);int count=0;
            foreach(var clip in data.clips)
            {
                string output=dest+"/"+clip.id+".wav";File.Copy(Path.Combine(BattleBuild.Target,clip.path),output,true);
                AssetDatabase.ImportAsset(output,ImportAssetOptions.ForceSynchronousImport);
                var asset=AssetDatabase.LoadAssetAtPath<AudioClip>(output);if(asset==null||asset.samples<=0)throw new InvalidDataException("Audio import failed: "+output);
                count++;
            }
            AssetDatabase.Refresh();File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/audio-import-report.json"),JsonUtility.ToJson(new Report{passed=true,clips=count},true));
        }
    }
}
