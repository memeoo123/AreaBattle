'use strict';
// Static text extraction and Brotli decoding only; never execute target code.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const zlib = require('node:zlib');
const root = path.join(__dirname, 'targets/wxcf1394487200e48f/43');
const beautifyPath = 'E:/Projects/weichatAnalysis/shoucheng/tools/vendor/js-beautify/node_modules/js-beautify/js/lib/beautify.js';
const beautify = require(beautifyPath).js_beautify;
const hash = b => crypto.createHash('sha256').update(b).digest('hex');
const sourcePath = 'work/unpacked/__WITHOUT_MULTI_PLUGINCODE__/game.js';
const source = fs.readFileSync(path.join(root, sourcePath), 'utf8');
const matches = [...source.matchAll(/define\("([^"\n]+)", function\(require, module, exports\)\{/g)];
const selected = new Set(['game.js', 'unity-namespace.js', 'wasm-split.js', 'plugin-config.js', 'texture-config.js', 'webgl.wasm.framework.unityweb.js']);
fs.mkdirSync(path.join(root, 'generated/modules'), {recursive:true});
const modules = matches.map((m, i) => {
  const start = m.index;
  const end = i + 1 < matches.length ? matches[i+1].index : source.length;
  const item = {name:m[1], source:sourcePath, charStart:start, charEnd:end, originalLine:source.slice(0,start).split('\n').length};
  if (selected.has(m[1])) {
    const output = `generated/modules/${m[1]}`;
    const text = beautify(source.slice(start,end), {indent_size:2});
    fs.writeFileSync(path.join(root,output),text);
    item.output = output;
    item.sha256 = hash(text);
  }
  return item;
});
fs.writeFileSync(path.join(root,'evidence/module-index.json'),JSON.stringify({sourceSha256:hash(source),formatter:{path:beautifyPath,sha256:hash(fs.readFileSync(beautifyPath)),version:'2.0.3'},modules},null,2));
const decoded = [];
fs.mkdirSync(path.join(root,'generated/wasm'),{recursive:true});
for (const [folder, name] of [['_wasmcode_','wasmcode'],['_wasmcode1_','wasmcode1']]) {
  const rel = `work/unpacked/${folder}/${name}/8cf71c9e452b70bc.webgl.wasm.code.unityweb.wasm.br`;
  const bytes = fs.readFileSync(path.join(root,rel));
  try {
    const plain = zlib.brotliDecompressSync(bytes);
    const out = `generated/wasm/${name}.wasm`;
    fs.writeFileSync(path.join(root,out),plain);
    decoded.push({source:rel,sourceSha256:hash(bytes),output:out,size:plain.length,sha256:hash(plain),header:plain.subarray(0,16).toString('hex'),valid:WebAssembly.validate(plain)});
  } catch(error) { decoded.push({source:rel,error:error.message,header:bytes.subarray(0,32).toString('hex')}); }
}
fs.writeFileSync(path.join(root,'evidence/wasm-decode.json'),JSON.stringify(decoded,null,2));
console.log(JSON.stringify({moduleCount:modules.length,selected:modules.filter(x=>x.output).map(x=>x.output),decoded},null,2));
