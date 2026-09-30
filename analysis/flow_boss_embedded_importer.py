from pathlib import Path
s=Path('UnityProject/Assets/AreaBattle/Editor/RecoveredSkillEffectImporter.cs').read_text(encoding='utf8')
s=s.replace('RecoveredSkillEffectImporter','RecoveredBossEmbeddedImporter').replace('Resources/Recovered/SkillEffects','Resources/Recovered/BossEmbedded').replace('skill-effects-20260928','boss-entities-20260928').replace('skill-effect-roundtrip','boss-embedded-roundtrip')
s=s.replace('            ImportManifest("prepared/native-import.json","unity-skill-effect-import-report.json");\n','')
s=s.replace('unity-embedded-effect-import-report.json','unity-boss-embedded-import-report.json').replace('Import recovered skill effects','Import recovered Boss status and shadow')
Path('UnityProject/Assets/AreaBattle/Editor/RecoveredBossEmbeddedImporter.cs').write_text(s,encoding='utf8')
