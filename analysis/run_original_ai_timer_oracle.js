// Execute the UNALTERED AICamp.Updata body with explicit deterministic config/action
// stubs. No original initializer or account state; no host imports/network/files in WASM.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.join(__dirname,'targets/wxcf1394487200e48f/43');
const methods=JSON.parse(fs.readFileSync(path.join(root,'generated/gameplay-method-map.json'))).methods;
const m=methods.find(x=>x.class==='AICamp'&&x.method==='Updata');
const source=fs.readFileSync(path.join(root,'generated/wasm/wasmcode.wasm'));
const original=source.subarray(m.body.offset,m.body.offset+m.body.size);
const leb=n=>{let a=[];do{let b=n&127;n>>>=7;if(n)b|=128;a.push(b);}while(n);return a;};
const str=s=>[...leb(Buffer.byteLength(s)),...Buffer.from(s)];
const sec=(id,a)=>[id,...leb(a.length),...a];
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const f32=n=>{const b=Buffer.alloc(4);b.writeFloatLE(n);return [...b];};
const types=[ [[],[]], [[127],[]], [[127],[127]], [[127,127,127],[125]], [[127,127,127,127],[]], [[127,125,125,127],[]] ];
const indices=new Array(m.function+1).fill(0),bodies=indices.map(()=>[0,11]);
function stub(i,t,ops){indices[i]=t;bodies[i]=[0,...ops,11];}
stub(900,1,[]); // metadata init: guard is already set, never reached
stub(935,2,[65,...leb(1024)]); // controller singleton
stub(1060,2,[65,...leb(512)]); // config singleton
stub(12796,3,[35,0]); // explicit interval global
stub(13413,4,[35,1,65,1,106,36,1]); // count action invocations, no invented AI actions
indices[m.function]=5;bodies[m.function]=[...original];
let code=[...leb(bodies.length)];for(const b of bodies)code.push(...leb(b.length),...b);
const bytes=Buffer.from([0,97,115,109,1,0,0,0,
 ...sec(1,[...leb(types.length),...types.flatMap(([a,r])=>[96,...leb(a.length),...a,...leb(r.length),...r])]),
 ...sec(3,[...leb(indices.length),...indices]),...sec(5,[1,1,80,80]),
 ...sec(6,[2,125,1,67,...f32(.2),11,127,1,65,0,11]),
 ...sec(7,[4,...str('run'),0,...leb(m.function),...str('memory'),2,0,...str('interval'),3,0,...str('actions'),3,1]),...sec(10,code)]);
const mod=new WebAssembly.Module(bytes);if(WebAssembly.Module.imports(mod).length)throw Error('Unexpected imports');
const e=new WebAssembly.Instance(mod,{}).exports,mem=new DataView(e.memory.buffer),self=128;
mem.setUint8(5010199,1);mem.setInt32(3953304,1024,true);mem.setUint8(self+8,1);mem.setInt32(self+12,99,true);mem.setInt32(self+16,2,true);
const cases=[];
for(const [id,delay,timer,dt,interval] of [
 ['delay-positive',.4,0,.1,.2],['delay-crosses-zero',.1,0,.2,.2],['delay-exact-zero',.1,0,.1,.2],
 ['timer-exact-zero',0,.125,.125,.2],['first-running-frame',0,0,1/60,.2],['negative-delay-next-frame',-.1,0,1/60,.4],
 ['timer-residual',0,.1,.15,.4],['large-delta-one-action',0,0,.7,.2],['negative-timer-zero-delta',0,-.3,0,.2]]) {
 mem.setFloat32(self+20,delay,true);mem.setFloat32(self+24,timer,true);e.interval.value=interval;e.actions.value=0;
 e.run(self,dt,dt,0);
 cases.push({id,delay,timer,dt,interval,expectedDelay:mem.getFloat32(self+20,true),expectedTimer:mem.getFloat32(self+24,true),expectedActions:e.actions.value});
}
const out={status:'isolated-original-scheduler-oracle',runtime:process.version,originalBodyUnchanged:true,
 source:{function:m.function,offset:m.body.offset,size:m.body.size,bodySha256:hash(original),sourceSha256:hash(source),harnessSha256:hash(bytes)},
 stubs:{900:'metadata guard already set',935:'controller pointer',1060:'config pointer',12796:'fixture interval, does not exercise RNG',13413:'count invocation only, does not execute AI policy'},
 limitations:['Scheduler arithmetic/control flow only; not original random stream, AI policy or complete initialization.'],cases};
fs.writeFileSync(path.join(root,'generated/original-ai-timer-cases.json'),JSON.stringify(out,null,2)+'\n');
console.log(JSON.stringify(out));
