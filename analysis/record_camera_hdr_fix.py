"""Record the source-backed GameCamera HDR/MSAA restoration and fresh validation."""
import json,hashlib,subprocess
from pathlib import Path
from datetime import datetime,timezone
W=Path(__file__).resolve().parent.parent;T=W/'analysis/targets/wxcf1394487200e48f/43';G=T/'generated';V=G/'video-20260928'
def read(p):return json.loads(p.read_text(encoding='utf-8'))
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
camera=G/'assets/gameplay.unity_6a7134d1d26bb7354ce61871131a7a4c/Camera__46.json';source=read(camera);assert source['m_HDR'] is False and source['m_AllowMSAA'] is False and source['m_GameObject']['m_PathID']==8
assert read(camera.parent/'GameObject__8.json')['m_Name']=='GameCamera'
checks=read(W/'analysis/unity-integrated-validation.json');build=read(W/'analysis/unity-build-report.json');smoke=read(W/'analysis/player-smoke.json');assert checks['passed'] and len(checks['checks'])==262 and build['result']=='Succeeded' and smoke['passed'] and smoke['frames']==600
assert read(V/'full-replay-diagnostic.json')==read(V/'full-replay-before-skin102.json')
metrics=read(V/'ice-camera-fix-metrics.json');assert metrics['afterMAE']<metrics['beforeMAE'] and metrics['afterNearWhite']<metrics['beforeNearWhite']
out=dict(atUtc=datetime.now(timezone.utc).isoformat(),status='source-camera-flags-restored',sourceCamera=str(camera.relative_to(T)),sourceSha256=hashlib.sha256(camera.read_bytes()).hexdigest(),sourceGameObject='GameCamera / pathID8',sourceFlags=dict(allowHDR=False,allowMSAA=False),implementation='BattleView initializes both flags explicitly; previously relied on new Camera defaults. UI camera already had both false.',shaderAndMaterialChanged=False,numericReplayUnchanged=True,metrics=metrics,validation=dict(integratedChecks=262,build=build,smokeFrames=600,resourceIntegrityPassed=True),comparison='generated/video-20260928/ice-camera-fix-comparison.png',limitations=['Residual ice particle, edge and sampling differences remain; no full visual pass.','Original RNG and pre-recording state remain unknown; full numeric match is still incomplete.'],references=[dict(url='https://learn.microsoft.com/en-us/windows/win32/direct3d11/d3d10-graphics-programming-guide-output-merger-stage',role='Background: normalized versus floating-point render-target blend clamping. The local source camera flags, not this document, determine restored settings.')])
write(V/'camera-hdr-fix-audit.json',out)
p=G/'RESTORE_SPEC.json';v=read(p);v['originalVideoReference']['cameraHdrFix']='generated/video-20260928/camera-hdr-fix-audit.json';write(p,v)
p=T/'BATTLEFIELD_RESTORE_STATE.json';v=read(p);m=dict(id='source-game-camera-hdr-msaa-restored',status=out['status'],atUtc=out['atUtc'],evidence=['generated/video-20260928/camera-hdr-fix-audit.json']);v['milestones']=[x for x in v.get('milestones',[]) if x.get('id')!=m['id']]+[m];write(p,v)
p=W/'analysis/VALIDATION_MANIFEST.json';previous=read(p);subprocess.run(['python',str(W/'analysis/record_validation_manifest.py')],check=True);v=read(p)
for key,value in previous.items():
 if key not in v:v[key]=value
v['cameraHdrFixAudit']=out;v['pendingFullGoal']=['Full original numeric replay and visual/audio timing including remaining ice edges/particles','Unobserved resource-load callback races and daily pool initialization'];write(p,v)
p=V/'visual-review-audit.json';v=read(p);v['iceObservation']['resolvedBy']='camera-hdr-fix-audit.json';v['iceObservation']['currentStatus']='Large white highlight mismatch reduced by restoring source camera HDR=false and MSAA=false; residual visual differences remain.';write(p,v)
p=V/'visual-review.html';s=p.read_text(encoding='utf-8');s=s.replace('36.385秒：四座敌塔冰晶均出现，本地晶体大面积发白，原片更透明。混合状态、HDR颜色和动画曲线已有源资源对应；具体渲染原因未确认，未用调色拟合。','36.385秒：已按原始GameCamera关闭HDR和MSAA，大片发白明显改善。材质、Shader和玩法数值未改；粒子及边缘细节仍有差异。');s=s.replace('<p><a href="full-replay-result-plus700ms.png">','<p><a href="ice-camera-fix-comparison.png">冰晶修复前后对照</a> · <a href="full-replay-result-plus700ms.png">');p.write_text(s,encoding='utf-8')
marker='\n\n## 修复原始 GameCamera 的 HDR/MSAA 设置遗漏'
note=marker+"\n\n原始GameObject8为GameCamera，Camera46的m_HDR=false、m_AllowMSAA=false；BattleView此前新建相机未设置这两项。现显式恢复源设置，未修改冰晶Shader、材质颜色、透明度或伤害。UI相机本来已关闭两项。\n\n36.385秒固定右下塔冰晶区域[534,775,638,898]：接近纯白像素（RGB各通道均>248）原片10，修复前2883，修复后177；该区域RGB平均绝对误差25.599→17.474。区域同时包含塔身、背景和粒子，仅用于局部前后对比，不是全局视觉验收。见 generated/video-20260928/ice-camera-fix-comparison.png 和 camera-hdr-fix-audit.json。\n\n完整录像数值报告与既有基线逐项一致。重新执行262项集成检查全部通过；Windows构建成功0错误、5条既有Spine废弃API警告；新二进制600帧烟雾测试通过，重试前后均正常出兵。资源审计240文件、1363 GUID引用通过。边缘、粒子、完整随机会话和其它未验证关内路径仍待核对，目标继续保持未完成。\n"
for p in [W/'analysis/VALIDATION_REPORT.md',W/'analysis/VIDEO_VALIDATION_REPORT.md',W/'RESTORE_PROGRESS.md',T/'REVERSE_PROGRESS.md']:
 p.write_text(p.read_text(encoding='utf-8').split(marker)[0]+note,encoding='utf-8')
print('Recorded source camera fix,262 checks,fresh build/smoke and local visual metrics.')
