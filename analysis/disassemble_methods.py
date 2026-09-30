"""Bounded static disassembly of selected already-resolved WASM functions."""
import json
import struct
from pathlib import Path
ROOT=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
methods=json.loads((ROOT/'generated/gameplay-method-map.json').read_text(encoding='utf-8'))['methods']
names={}
for m in methods:
    if 'function' in m:names.setdefault((m['module'],m['function']),[]).append(m['class']+'.'+m['method'])
ops={0:'unreachable',1:'nop',2:'block',3:'loop',4:'if',5:'else',11:'end',12:'br',13:'br_if',14:'br_table',15:'return',16:'call',17:'call_indirect',26:'drop',27:'select',32:'local.get',33:'local.set',34:'local.tee',35:'global.get',36:'global.set',40:'i32.load',41:'i64.load',42:'f32.load',43:'f64.load',44:'i32.load8_s',45:'i32.load8_u',46:'i32.load16_s',47:'i32.load16_u',48:'i64.load8_s',49:'i64.load8_u',50:'i64.load16_s',51:'i64.load16_u',52:'i64.load32_s',53:'i64.load32_u',54:'i32.store',55:'i64.store',56:'f32.store',57:'f64.store',58:'i32.store8',59:'i32.store16',60:'i64.store8',61:'i64.store16',62:'i64.store32',63:'memory.size',64:'memory.grow',65:'i32.const',66:'i64.const',67:'f32.const',68:'f64.const'}
numeric='i32.eqz i32.eq i32.ne i32.lt_s i32.lt_u i32.gt_s i32.gt_u i32.le_s i32.le_u i32.ge_s i32.ge_u i64.eqz i64.eq i64.ne i64.lt_s i64.lt_u i64.gt_s i64.gt_u i64.le_s i64.le_u i64.ge_s i64.ge_u f32.eq f32.ne f32.lt f32.gt f32.le f32.ge f64.eq f64.ne f64.lt f64.gt f64.le f64.ge i32.clz i32.ctz i32.popcnt i32.add i32.sub i32.mul i32.div_s i32.div_u i32.rem_s i32.rem_u i32.and i32.or i32.xor i32.shl i32.shr_s i32.shr_u i32.rotl i32.rotr i64.clz i64.ctz i64.popcnt i64.add i64.sub i64.mul i64.div_s i64.div_u i64.rem_s i64.rem_u i64.and i64.or i64.xor i64.shl i64.shr_s i64.shr_u i64.rotl i64.rotr f32.abs f32.neg f32.ceil f32.floor f32.trunc f32.nearest f32.sqrt f32.add f32.sub f32.mul f32.div f32.min f32.max f32.copysign f64.abs f64.neg f64.ceil f64.floor f64.trunc f64.nearest f64.sqrt f64.add f64.sub f64.mul f64.div f64.min f64.max f64.copysign i32.wrap_i64 i32.trunc_f32_s i32.trunc_f32_u i32.trunc_f64_s i32.trunc_f64_u i64.extend_i32_s i64.extend_i32_u i64.trunc_f32_s i64.trunc_f32_u i64.trunc_f64_s i64.trunc_f64_u f32.convert_i32_s f32.convert_i32_u f32.convert_i64_s f32.convert_i64_u f32.demote_f64 f64.convert_i32_s f64.convert_i32_u f64.convert_i64_s f64.convert_i64_u f64.promote_f32 i32.reinterpret_f32 i64.reinterpret_f64 f32.reinterpret_i32 f64.reinterpret_i64'.split()
ops.update({i+0x45:n for i,n in enumerate(numeric)})
class R:
    def __init__(self,b,p):self.b,self.p=b,p
    def leb(self,signed=False):
        v=shift=0
        while True:
            c=self.b[self.p];self.p+=1;v|=(c&127)<<shift;shift+=7
            if c<128:break
        return v-(1<<shift) if signed and c&64 else v
selected=[m for m in methods if m['class']=='BattleControl' or m['class']=='Tower' and m['method'] in ('get_SpawnTime','SoldierCollision','ChangeScore','RefreshTowerGrade','CheckGradeChange') or m['class']=='LevelControl' and m['method']=='get_PlayerCampID']
out=ROOT/'generated/disassembly';out.mkdir(exist_ok=True)
for m in selected:
    if 'body' not in m:continue
    blob=(ROOT/f'generated/wasm/{m["module"]}.wasm').read_bytes();body=m['body'];r=R(blob,body['offset']);end=body['offset']+body['size']
    local=[]
    for _ in range(r.leb()):
        count=r.leb();typ=blob[r.p];r.p+=1;local.append((count,typ))
    lines=[f'; {m["class"]}.{m["method"]}; {m["module"]} function {m["function"]}; offset={body["offset"]} size={body["size"]}',f'; signature={m["signature"]}; localGroups={local}']
    depth=0
    while r.p<end:
        pos=r.p;op=blob[r.p];r.p+=1;assert op in ops,(m['method'],pos,hex(op));name=ops[op];args=[];comment=''
        if op in (2,3,4):args=[r.leb(True)]
        elif op in (12,13,16,32,33,34,35,36,63,64):args=[r.leb()]
        elif op==14:args=[r.leb() for _ in range(r.leb()+1)]
        elif op==17:args=[r.leb(),r.leb()]
        elif 40<=op<=62:args=[r.leb(),r.leb()]
        elif op in (65,66):args=[r.leb(True)]
        elif op in (67,68):
            fmt='<f' if op==67 else '<d';args=[struct.unpack_from(fmt,blob,r.p)[0]];r.p+=struct.calcsize(fmt)
        if op==16:
            comment=' ; '+', '.join(names.get((m['module'],args[0]),[]))
        if op in (5,11):depth=max(depth-1,0)
        lines.append(f'{pos:08x}  '+('  '*depth)+name+' '+str(args)+comment)
        if op in (2,3,4,5):depth+=1
    dest=out/f'{m["class"]}-{m["method"].replace("/","_")}.txt'
    dest.write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(dest.name,body['size'])
