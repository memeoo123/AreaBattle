from pathlib import Path
s=Path('analysis/presentation_disassemble.py').read_text(encoding='utf8')
s=s.replace("classes={'Soldier','SpineAnimator','TowerCanvas','TowerUpgradeConfig','ShipConfig','ColorHelper','MPBExtension'}","classes={'GameSceneControl','SceneControl','SceneSkinConfig','SceneSkinControl','SceneController','SkinControl','SceneSkinData','GameSceneMono','GameControl','SkinManager','SceneSkinItem'}")
s=s.replace('generated/presentation-binding-evidence.json','generated/scene-binding-evidence.json')
s=s.replace("m['token'] in tokens]","m['token'] in tokens or m['token'] in (100664522,100664524,100664526)]")
exec(compile(s,'scene_disassemble','exec'))


