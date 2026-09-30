"""Recover fixed-size startup UI MonoBehaviour payloads from local binary and metadata."""
from pathlib import Path
import json,struct,hashlib
helper=Path(__file__).with_name('asset_focus.py');n={'__file__':str(helper.resolve())}
exec(helper.read_text(encoding='utf8').split('roots=[]')[0],n)
p=Path(__file__).with_name('recover_outgame_manager_registry.py');m={'__file__':str(p.resolve())}
exec(p.read_text(encoding='utf8').split('rows=[]')[0],m)
fields={}
for ti,t in enumerate(m['ts']):
 name=m['ms'](t[0])
 if name not in ['AppSetting','CanvasScaler','CanvasAdaptive','AtlasLoader','AdaptiveBangs']:continue
 fields[name]=[]
 for j in range(t[18]):
  f=struct.unpack_from('<3i',m['b'],m['pairs'][11][0]+12*(t[8]+j));ptr=m['u'](200288+4*f[1])
  fields[name].append({'name':m['ms'](f[0]),'offset':m['u'](m['u'](3823136+4*ti)+4*j),'typeBits':hex(m['u'](ptr+4))})
layouts={
 'CanvasScaler':('<iff2fififff?',48,['m_UiScaleMode','m_ReferencePixelsPerUnit','m_ScaleFactor','referenceWidth','referenceHeight','m_ScreenMatchMode','m_MatchWidthOrHeight','m_PhysicalUnit','m_FallbackScreenDPI','m_DefaultSpriteDPI','m_DynamicPixelsPerUnit','m_PresetInfoIsWorld']),
 'GraphicRaycaster':('<?3xiI',12,['m_IgnoreReversedGraphics','m_BlockingObjects','m_BlockingMask']),
 'CanvasAdaptive':('<fff?',16,['whRatioConst','widthControlsHeightFactor','heightControlsWidthFactor','isHeightCtrWidthFixedWidth']),
 'AspectRatioFitter':('<if',8,['m_AspectMode','m_AspectRatio']),
 'AtlasLoader':('<?',4,['isAutoLoaderAtlas'])}
rows=[]
for root in ['level0:19','resources.assets:244']:
 for component in n['hierarchy'](n['objs'][root])['components']:
  name=(component.get('script') or {}).get('m_ClassName')
  if name not in layouts:continue
  obj=n['objs'][component['id']];data=(n['ROOT']/obj['outputs']['raw']).read_bytes()
  fmt,size,names=layouts[name]
  assert len(data)==32+size,(component['id'],len(data))
  assert data[28:32]==bytes(4),'empty aligned MonoBehaviour name required'
  values=struct.unpack_from(fmt,data,32)
  assert data[32+struct.calcsize(fmt):]==bytes(size-struct.calcsize(fmt)),'unexpected trailing fields'
  rows.append({'object':component['id'],'owner':root,'class':name,'rawPath':obj['outputs']['raw'],'sha256':hashlib.sha256(data).hexdigest(),'format':fmt,'payloadOffset':32,'fields':dict(zip(names,values))})
result={'status':'fixed-size source payloads decoded; runtime behavior separate','metadataFields':fields,'components':rows,'remaining':['Original CanvasAdaptive methods74278-74284','AdaptiveBangs Start28034 and called helpers','AtlasLoader subscription/resource behavior','Bootstrap asset importer with source tag and camera references']}
(n['ROOT']/'generated/outgame/ui-bootstrap-schema.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(rows,ensure_ascii=False))
