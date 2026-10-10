from pathlib import Path
import base64, json
root=Path(__file__).parent
frames=['data:image/png;base64,'+base64.b64encode(p.read_bytes()).decode() for p in sorted((root/'frames').glob('*.png'))]
assert len(frames)==24
page='''<!doctype html><html lang="zh"><meta charset="utf-8"><title>普通士兵 · 待确认样板</title>
<style>body{margin:32px auto;max-width:900px;padding:0 20px;background:#f7f5ef;color:#293228;font:16px/1.7 system-ui}h1{font-size:25px}button,select{padding:9px 16px;margin-right:10px;font:inherit}figure{margin:18px 0}img{display:block;max-width:100%;border-radius:8px}small{color:#596353}#state{font-weight:600}input{width:280px}details img{width:540px}</style>
<h1>普通士兵 · 待确认样板</h1><p>当前游戏素材与渲染代码的 Unity 录制。上排放大 7 倍，下排约 18 像素高（接近 540×960 战场尺寸）。页面缩放会影响显示大小。</p>
<p>蓝方朝右，红方朝左。循环：四帧跑动 → 0.65 秒倒下淡出 → 重新播放。</p>
<figure><img id="animation" width="720" height="360"><figcaption>上排：放大造型　　下排：战场尺寸参照</figcaption></figure>
<p><button id="toggle">暂停</button><button id="restart">重播</button><select id="mode"><option value="all">跑动 + 死亡</option><option value="run">仅跑动</option><option value="death">仅死亡</option></select><span id="state"></span></p>
<p><input id="scrub" type="range" min="0" max="23" value="0"><small>拖动逐帧查看</small></p>
<p>请确认：造型是否合适、实际大小是否清楚、跑动是否自然、死亡表现是否接受。当前四帧步态是初版，尚未获得视觉认可。</p>
<details><summary>查看战场组合截图（展示布置）</summary><img src="../../../..//analysis/captures/clear-a-level116-sample.png"></details>
<script>const frames=FRAMES;let index=0,playing=true;const img=document.querySelector('#animation'),slider=document.querySelector('#scrub'),mode=document.querySelector('#mode');frames.forEach(s=>{const i=new Image();i.src=s});function draw(){img.src=frames[index];slider.value=index;document.querySelector('#state').textContent=index<12?'跑动':index<19?'倒下 / 淡出':'死亡结束';document.querySelector('#toggle').textContent=playing?'暂停':'播放'}document.querySelector('#toggle').onclick=()=>{playing=!playing;draw()};document.querySelector('#restart').onclick=()=>{index=mode.value==='death'?12:0;playing=true;draw()};slider.oninput=()=>{playing=false;index=+slider.value;draw()};mode.onchange=()=>{index=mode.value==='death'?12:0;draw()};setInterval(()=>{if(!playing)return;index++;if(mode.value==='run'&&index>=12)index=0;else if(index>=24)index=mode.value==='death'?12:0;draw()},100);draw();</script></html>'''
(root/'index.html').write_text(page.replace('FRAMES',json.dumps(frames)),encoding='utf-8')
