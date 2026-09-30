"""Audit original controller slots, including explicit interface implementations."""
from pathlib import Path
import json, re, hashlib
base = Path(__file__).resolve().parent
helper = base / 'recover_outgame_manager_registry.py'
env = {'__file__': str(helper)}
exec(helper.read_text(encoding='utf8').split('rows=[]')[0], env)
ts, md, ms, u = (env[k] for k in ('ts', 'md', 'ms', 'u'))
out = env['p'] / 'generated/outgame'
roster = json.loads((out / 'PRE_GAME_REGISTRATION_ROSTER.json').read_text(encoding='utf8'))
methods = {m['metadata']: m for m in json.loads((out / 'method-map.json').read_text(encoding='utf8'))}
def decode_type(pointer):
    data, code = u(pointer), (u(pointer + 4) >> 16) & 255
    if code in (18, 17, 28):
        return data, {'typePointer': pointer, 'code': code, 'typeIndex': data}
    if code == 21:
        definition, evidence = decode_type(u(data))
        return definition, {'typePointer': pointer, 'code': code, 'genericClass': data,
                            'definition': evidence, 'classInst': u(data + 4)}
    raise ValueError((pointer, code))
def ancestry(index):
    result = []
    while index >= 0:
        assert index not in [x['typeIndex'] for x in result], index
        entry = {'typeIndex': index, 'name': ms(ts[index][0])}
        result.append(entry)
        parent = ts[index][4]
        if parent < 0:
            break
        index, entry['parentEvidence'] = decode_type(u(200288 + 4 * parent))
    return result
implementations = {
    3875: 'OutgameItemModuleControl.cs',
    4025: 'OutgameBuffControl.cs', 4256: 'OutgameToolControl.cs',
    4561: 'OutgamePrefabPoolControl.cs', 4034: 'OutgameServerTimeControl.cs',
    4027: 'OutgameControllerRegistry.cs', 3903: 'OutgameProcessControl.cs',
    4118: 'OutgameLocalDataControl.cs', 4296: 'OutgameUiControl.cs', 4064: 'OutgameGameControl.cs', 4117: 'OutgameLoadPrefabControl.cs',
    4107: 'OutgameLevelControl.cs', 4462: 'OutgamePlayerControl.cs', 3904: 'OutgameAudioControl.cs',
}
rows = []
for controller in roster['controllers']:
    index = controller['typeIndex']
    chain = ancestry(index)
    slots = []
    for slot, name in ((4, 'OnInit'), (5, 'Updata'), (6, 'OnDispose')):
        candidates = []
        for ancestor in chain:
            candidates = [(i, m) for i, m in enumerate(md)
                          if m[1] == ancestor['typeIndex'] and m[-2] == slot]
            if candidates:
                break
        assert len(candidates) == 1, (index, slot, candidates)
        metadata, method = candidates[0]
        assert ms(method[0]).split('.')[-1] == name
        mapping = methods[metadata]
        path = out / mapping['path']
        text = path.read_text(encoding='utf8')
        instructions = [m.group(1).strip() for line in text.splitlines()
                        if (m := re.match(r'^[0-9a-f]+\s+(.+)', line))]
        empty = instructions == ['nop []', 'end []']
        slots.append({'slot': slot, 'contract': name, 'metadata': metadata,
                      'declaringType': method[1], 'methodName': ms(method[0]),
                      'explicitInterface': '.' in ms(method[0]), 'inherited': method[1] != index,
                      'sourceBodyEmpty': empty, 'module': mapping['module'],
                      'function': mapping['function'], 'body': mapping['body'],
                      'evidence': mapping['path'], 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                      'namedCalls': list(dict.fromkeys(re.findall(r'call(?:_indirect)? .*? ; (.+)', text))),
                      'implementationStatus': 'implemented-in-core-controller-bindings' if index in implementations else 'not-yet-bound-to-production-resolver',
                      'implementation': implementations.get(index)})
    rows.append({**controller, 'ancestry': chain, 'lifecycle': slots})
assert len(rows) == 38 and sum(len(r['lifecycle']) for r in rows) == 114
result = {'status': 'evidence-audited-production-binding-incomplete',
          'sourceMethod': 30608, 'metadataSha256': hashlib.sha256(env['b']).hexdigest(),
          'memorySha256': hashlib.sha256(env['mem']).hexdigest(),
          'methodCount': 114, 'implementedLifecycleControllers': len(implementations), 'binding': 'OutgameCoreControllerBindings.cs', 'emptyBodies': sum(x['sourceBodyEmpty'] for r in rows for x in r['lifecycle']),
          'correction': 'Type4561 declares all three slots explicitly as GameFramework.IControl.*; these are not inherited ControlBase lifecycle methods. ControlBase`1 declares only get_I and ctor.',
          'controllers': rows,
          'limits': ['Empty lifecycle body does not imply empty business controller.',
                     'Named calls are an inventory, not a complete semantic reconstruction.',
                     'No full production composition or Player validation claimed.']}
(out / 'CONTROLLER_LIFECYCLE_MATRIX.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf8')
print(json.dumps({k: result[k] for k in ('status', 'methodCount', 'emptyBodies', 'correction')}, ensure_ascii=False))
