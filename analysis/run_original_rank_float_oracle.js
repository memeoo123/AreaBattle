// Execute the reviewed original FloatToInt32605 body in isolation, without imports.
// Never feed NaN/infinity: the original source has no termination guard for them.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.join(__dirname,'targets/wxcf1394487200e48f/43');
const rows=JSON.parse(fs.readFileSync(path.join(root,'generated/outgame/method-map.json')));
const method=rows.find(r=>r.metadata===32605);
const source=fs.readFileSync(path.join(root,'generated/wasm',method.module+'.wasm'));
const body=source.subarray(method.body.offset,method.body.offset+method.body.size);
const dis=fs.readFileSync(path.join(root,'generated/outgame',method.path),'utf8');
if(/\b(call|call_indirect|global\.(get|set)|memory\.grow)\b/.test(dis))throw Error('Original function not isolated');
const leb=n=>{const r=[];do{let b=n&127;n>>>=7;if(n)b|=128;r.push(b);}while(n);return r;};
const str=s=>[...leb(Buffer.byteLength(s)),...Buffer.from(s)],sec=(id,a)=>[id,...leb(a.length),...a];
const bytes=Buffer.from([0,97,115,109,1,0,0,0,...sec(1,[1,96,3,125,127,127,1,127]),...sec(3,[1,0]),...sec(5,[1,1,1,1]),...sec(7,[2,...str('run'),0,0,...str('memory'),2,0]),...sec(10,[1,...leb(body.length),...body])]);
const moduleObject=new WebAssembly.Module(bytes);if(WebAssembly.Module.imports(moduleObject).length)throw Error('Unexpected imports');
const e=new WebAssembly.Instance(moduleObject,{}).exports,memory=new DataView(e.memory.buffer);
const cases=[0,1.23,1.25,1.28,1.38,1.2345,-1.25].map(input=>{memory.setInt32(128,-99,true);const integer=e.run(input,128,0);return{input,f32Input:Math.fround(input),integer,decimals:memory.getInt32(128,true)};});
const anomaly=cases.find(r=>r.input===1.28);if(anomaly.integer!==12799999||anomaly.decimals!==7)throw Error('Unexpected original float oracle');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
fs.writeFileSync(path.join(root,'generated/outgame/rank-float-oracle.json'),JSON.stringify({sourceMethod:method,sourceSha256:hash(source),bodySha256:hash(body),harnessSha256:hash(bytes),imports:[],memoryPages:1,runtime:process.version,cases,scope:'Unaltered original arithmetic body only; source manager string conversion separately reviewed.'},null,2)+'\n');
console.log(JSON.stringify({originalBodyExecuted:true,cases:cases.length,anomaly,imports:0}));
