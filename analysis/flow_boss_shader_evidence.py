from pathlib import Path
s=Path('analysis/asset_embedded_shader_evidence.py').read_text(encoding='utf8').replace('skill-effects-20260928','boss-entities-20260928')
s=s.replace("R/'generated/asset-evidence.json'","S/'asset-evidence-incremental.json'").replace("('Effect/bingzhui','Effect/Fire Ruan Dissolves Noise','Effect/Blend')","('Spine/Skeleton',)").replace('embedded-shader-evidence','spine-shader-evidence')
exec(compile(s,'spine_shader_evidence','exec'))
