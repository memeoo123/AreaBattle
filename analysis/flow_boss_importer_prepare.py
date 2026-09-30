"""Historical importer bootstrap. Maintained C# includes later persistence fixes; do not overwrite."""
from pathlib import Path
if Path('UnityProject/Assets/AreaBattle/Editor/RecoveredBossImporter.cs').exists():
 raise SystemExit('Boss importer already exists; maintain it directly. This bootstrap predates typed ScriptableObject persistence fixes.')
p=Path('UnityProject/Assets/AreaBattle/Editor/RecoveredSkillEffectImporter.cs')
s=p.read_text(encoding='utf8').replace('RecoveredSkillEffectImporter','RecoveredBossImporter').replace('SkillEffects','Bosses').replace('recovered skill effects','recovered Boss Spine models').replace('skill-effects-20260928','boss-entities-20260928').replace('skill-effect-roundtrip','boss-roundtrip').replace('unity-skill-effect-import-report','unity-boss-import-report')
s=s.replace('using UnityEngine;','using UnityEngine;\nusing Spine.Unity;')
s=s.replace('sourceMesh,sourceSha256,templatePath,shaderName,restoredShaderName,fileId;','sourceMesh,sourceSha256,templatePath,shaderName,restoredShaderName,fileId,scriptPath,scriptClass;')
s=s.replace('Dest+"/Shaders"','Dest+"/Shaders",Dest+"/Spine"',1)
s=s.replace('else if(r.type=="Mesh")ImportMesh(r);','''else if(r.type=="Mesh")ImportMesh(r);
                else if(r.type=="MonoScript") { imported[r.index]=AssetDatabase.LoadAssetAtPath<MonoScript>(r.scriptPath);if(imported[r.index]==null)throw new InvalidDataException("Missing official Spine script "+r.scriptPath); }
                else if(r.type=="TextAsset") {CheckHash(r.path,r.sha256);string path=Dest+"/Spine/"+Safe(r.id)+".txt";File.Copy(Path.Combine(target,r.path),path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);imported[r.index]=AssetDatabase.LoadAssetAtPath<TextAsset>(path);}''')
s=s.replace('foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="AnimationClip"))','''foreach(string script in new[]{"SpineAtlasAsset","SkeletonDataAsset"})
                foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="MonoBehaviour"&&x.scriptClass==script))
                    ImportNativeResource(r,Dest+"/Spine/"+Safe(r.id)+".asset",script=="SpineAtlasAsset"?typeof(SpineAtlasAsset):typeof(SkeletonDataAsset));
            foreach(var r in manifest.resources.Where(x=>!x.builtin&&x.type=="AnimationClip"))''')
s=s.replace('Type type=typeof(Transform).Assembly','Type type=source.type=="MonoBehaviour"?typeof(SkeletonAnimation):typeof(Transform).Assembly')
s=s.replace('if(prefab.GetComponentsInChildren<MonoBehaviour>(true).Length!=0)throw new InvalidDataException("Original custom executable component survived import");','''if(prefab.GetComponentsInChildren<MonoBehaviour>(true).Any(x=>!(x is SkeletonAnimation)))throw new InvalidDataException("Original custom executable component survived import");
                foreach(var skeleton in prefab.GetComponentsInChildren<SkeletonAnimation>(true)) {
                    var data=skeleton.skeletonDataAsset.GetSkeletonData(false);
                    if(data==null||data.Version!="4.1.16"||data.FindAnimation(skeleton.AnimationName)==null)throw new InvalidDataException("Original Spine data unavailable "+p.originalName);
                }''')
s=s.replace('r.type=="Shader"||r.type=="Texture2D"||r.type=="Sprite"?3:2','r.type=="Shader"||r.type=="Texture2D"||r.type=="Sprite"||r.type=="MonoScript"||r.type=="TextAsset"?3:2')
s=s.replace('Native source components/curves/renderer fields; original Bullet code excluded. Custom shaders translated from retained original GLES. Current-engine mip generation is reported in source manifest.','Four original models, native source components and Spine 4.1.16 skeleton/atlas data. Pinned official Spine 4.1 runtime; original game executable components excluded. Frame comparison remains pending.')
s=s.replace('original code excluded','original game code excluded; official Spine runtime bound')
Path('UnityProject/Assets/AreaBattle/Editor/RecoveredBossImporter.cs').write_text(s,encoding='utf8')
