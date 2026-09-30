"""Recover the source targeting indicators selected by SkillControl entity14/15."""
from pathlib import Path
s=Path('analysis/flow_gesture_prefabs.py').read_text(encoding='utf8')
s=s.replace("'generated/gesture-visuals'","'generated/skill-target-visuals'")
s=s.replace("['hdzd_eff_spheretrails','/linerandercut.prefab','/linearrow.prefab']","['/linearrow_hero_blue.prefab','/linearrow_hero_red.prefab']")
exec(compile(s,'skill target prefab inventory','exec'))
