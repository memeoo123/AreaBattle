import sys
from pathlib import Path
s=Path('analysis/asset_skill_roundtrip_validate.py').read_text(encoding='utf8').replace('skill-effects-20260928','boss-entities-20260928')
s=s.replace("Path('analysis/skill-effect-roundtrip')","Path('analysis/boss-embedded-roundtrip' if embedded else 'analysis/boss-roundtrip')")
exec(compile(s,'boss_roundtrip','exec'))
