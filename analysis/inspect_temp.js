const fs = require('node:fs');
const path = require('node:path');
const zlib = require('node:zlib');
const crypto = require('node:crypto');
const root = path.join(__dirname,'targets/wxcf1394487200e48f/43');
const source = 'C:/Users/jiachengwei/AppData/Roaming/Tencent/xwechat/radium/users/d833ae57d25e1087edac741082077974/applet/local/wxcf1394487200e48f/temp/1a0e5e8fbe8_a1f';
const bytes = fs.readFileSync(source);
const report = {source,size:bytes.length,sha256:crypto.createHash('sha256').update(bytes).digest('hex'),headerHex:bytes.subarray(0,32).toString('hex')};
try {
  const decoded = zlib.brotliDecompressSync(bytes);
  report.decodedSize=decoded.length;
  report.decodedHeaderHex=decoded.subarray(0,32).toString('hex');
  if (decoded.subarray(0,32).toString().startsWith('UnityWebData1.0\0') || decoded.subarray(0,32).toString().startsWith('TuanjieWebData1.0\0')) {
    fs.writeFileSync(path.join(root,'work/input/data-cache.br'),bytes);
    fs.writeFileSync(path.join(root,'generated/webgl.data'),decoded);
    report.output='generated/webgl.data';
    report.decodedSha256=crypto.createHash('sha256').update(decoded).digest('hex');
  }
}catch(e){report.decodeError=e.message;}
fs.writeFileSync(path.join(root,'evidence/temp-data-probe.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify(report,null,2));
