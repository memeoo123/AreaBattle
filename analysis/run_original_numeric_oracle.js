// Execute only reviewed, import-free original arithmetic function bodies.
// No original initializer, host imports, network access, or account state is used.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const root = path.join(__dirname, 'targets/wxcf1394487200e48f/43');
const methods = JSON.parse(fs.readFileSync(path.join(root,'generated/gameplay-method-map.json'))).methods;
const rows = JSON.parse(fs.readFileSync(path.join(root,'generated/tables/DispatchConfig.json'))).Datas.slice(0,3);
const leb = n => { const r=[]; do {let b=n&127;n>>>=7;if(n)b|=128;r.push(b);}while(n);return r; };
const str = s => [...leb(Buffer.byteLength(s)),...Buffer.from(s)];
const section = (id,a) => [id,...leb(a.length),...a];
const hash = a => crypto.createHash('sha256').update(a).digest('hex');
const cases=[], sources=[];
for(const name of ['GetSpawnTime','GetDispatchLineNum','GetDispatchAddScoreTime','GetDispatchScoreNum']) {
  const m=methods.find(x=>x.class==='BattleControl'&&x.method===name);
  if(!m) throw Error('Unresolved method '+name);
  const source=fs.readFileSync(path.join(root,'generated/wasm',m.module+'.wasm'));
  const body=source.subarray(m.body.offset,m.body.offset+m.body.size);
  const dis=fs.readFileSync(path.join(root,'generated/disassembly','BattleControl-'+name+'.txt'),'utf8');
  if(/\b(call|call_indirect|global\.(get|set)|memory\.grow)\b/.test(dis)) throw Error('Not a pure reviewed function: '+name);
  const sig=m.signature;
  const bytes=Buffer.from([0,97,115,109,1,0,0,0,
    ...section(1,[1,96,...leb(sig.args.length),...sig.args,...leb(sig.returns.length),...sig.returns]),
    ...section(3,[1,0]), ...section(5,[1,1,1,1]),
    ...section(7,[2,...str('run'),0,0,...str('memory'),2,0]),
    ...section(10,[1,...leb(body.length),...body])]);
  const module=new WebAssembly.Module(bytes);
  if(WebAssembly.Module.imports(module).length) throw Error('Unexpected imports');
  const e=new WebAssembly.Instance(module,{}).exports;
  const mem=new DataView(e.memory.buffer), self=128;
  rows.forEach((row,i)=>{const ptr=256+i*64;mem.setInt32(self+8+i*4,ptr,true);
    ['id','scoreLimit','maxLine','addSpace','swanpSpaceOne','swanpSpaceTwo','swanpSpaceThree'].forEach((key,j)=>mem.setInt32(ptr+8+j*4,row[key],true));});
  sources.push({method:name,module:m.module,function:m.function,offset:m.body.offset,size:m.body.size,sourceSha256:hash(source),bodySha256:hash(body),harnessSha256:hash(bytes),imports:[],memoryPages:1});
  for(let grade=0;grade<3;grade++) {
    for(const lines of name==='GetSpawnTime'?[1,2,3]:[null]) {
      const args=lines===null?[self,grade,0]:[self,grade,lines,0];
      const expected=e.run(...args);
      cases.push({id:'original-'+name+'-'+grade+(lines===null?'':'-'+lines),method:name,input:{grade,...(lines===null?{}:{lines})},expected,provenance:'unaltered-original-wasm-body',sourceMethod:name});
    }
  }
}
const result={schemaVersion:'1.0',target:{appId:'wxcf1394487200e48f',version:'43'},generatedAt:new Date().toISOString(),mode:'isolated original arithmetic; not full-game runtime validation',runtime:process.version,sources,cases};
fs.writeFileSync(path.join(root,'generated/original-numeric-cases.json'),JSON.stringify(result,null,2)+'\n');
console.log(JSON.stringify({methods:sources.length,cases:cases.length,sourceBodiesExecuted:true,hostImports:0}));
