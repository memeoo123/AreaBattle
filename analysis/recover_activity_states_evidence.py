"""Index original activity base/state/widget bodies and field layouts; no new disassembly."""
from pathlib import Path
import hashlib,json,struct
helper=Path(__file__).resolve().parent/'recover_outgame_manager_registry.py'
c={'__file__':str(helper)};exec(helper.read_text().split('rows=[]')[0],c)
out=c['p']/'generated/outgame';index=json.loads((out/'method-map.json').read_text())
classes={'ActivityBase','ActivityStateBase','ActivityCloseState','ActivityNoticeState','ActivityLaunchState','ActivityOverState'}
methods=[dict(metadata=r['metadata'],sourceClass=r['cls'],method=r['method'],path=r['path'],sha256=hashlib.sha256((out/r['path']).read_bytes()).hexdigest(),status='factory-pending' if r['metadata']==35162 else 'implemented-or-prior-shared-base') for r in index if r['cls'] in classes or r['metadata']==27246]
assert len(methods)==77
fields=[]
for owner in (4629,4645,4662,4663,4664,4665,4667):
    for k in range(c['ts'][owner][18]):
        name,typ,token=struct.unpack_from('<3i',c['b'],c['pairs'][11][0]+12*(c['ts'][owner][8]+k));ptr=c['u'](200288+4*typ)
        fields.append(dict(owner=owner,name=c['ms'](name),offset=c['u'](c['u'](3823136+4*owner)+4*k),typeCode=(c['u'](ptr+4)>>16)&255,typeData=c['u'](ptr)))
report=dict(metadataSha256=hashlib.sha256(c['b']).hexdigest(),memorySha256=hashlib.sha256(c['mem']).hexdigest(),methods=methods,fields=fields,
    findings=[
        'ActivityBase35164 sets ID/data before null return, clears first-init flag before OnInit, resolves config using record.id, refreshes FSM then live child list. Missing data retains prior config/FSM. BaseUpdate35171 calls each child.Update, not child.BaseUpdate. Dispose35183 destroys FSM, recursively disposes children, clears both collections then virtual Dispose; stored FSM/data/config/flags survive.',
        'RefreshFSM35167 creates Close/Notice/Launch/Over in that order. Exactly open==1 selects saved states1..4; unknown saved state leaves FSM unstarted. Other open values start Close. Existing FSM only receives event1 when open==1. Conditions35369 independently accept any nonzero open.',
        'Base OnEnter35358 compares config uniqueId to record uniqueId, writes new ID then dirty, clears LaunchTimeStamp only then virtual ResetProgress. It subscribes events1/2 and concrete statistics callbacks. Failures preserve already executed changes.',
        'Close enters state1 and hides widgets before choosing notice then launch then over, blocked by close condition. Direct initial Notice hook precedes transition; direct Launch/Over hooks follow transition. Closed callback executes after nested transitions too. When no choice succeeds, launchPop then noticePop clear.',
        'Launch enters3 then prioritizes over before close; otherwise shows buttons and hides text. Source35334 removes OverEvent from closeType and CloseEvent from overType, opposite registration35333. Dormant listeners remain; IsLeave gates callbacks. This original mismatch is preserved and tested.',
        'Notice enters2, prioritizes launch/over/close; otherwise binds notice UI and clock10000. Warm timestamp only initializes when still state2 and zero, after nested transitions. Notice event1 to launch transitions, clears launchPop and shows launch widgets again, allowing source double popup request. DateUpdate caches string[] size once and does not resize on config growth.',
        'Over enters4, marks launchPop true, resets progress before a close transition, otherwise hides buttons. All state exit flags change after base unsubscribe; Notice removes10000 before flag. Source event1 flags and callback order are retained.',
        'Widgets use actual UGUI Button/Text arrays. Show removes all runtime onClick listeners then binds type-based notice/launch UI host; description visibility follows state. Type10000 text subtracts current statistics clock from cached target, divides1000 and converts to signed int. TimeHelper27246 returns first three segments: d/h/m when >=86400, otherwise h/m/s; negative remainder is not clamped.',
        'Pop requests store config, actual Type and empty object[] before queue callback, then send common Warm/LaunchOnceOnEnterState message. OpenBindNoticeUI35178 ignores its parameter and reads stored NoticePopEvent after resolving UI. No invented popup acknowledgement or flags; ActivityControl bookkeeping/queue ownership remains a required host.'
    ],boundaries=[
        'ActivityFactoryBase35162 and generic GetChildActivity35163 remain pending; neither is claimed complete. Config getter/setter are represented by public reference storage. Condition35369 and FSM inherited empty lifecycle/event cleanup were implemented in prior milestones.',
        'Actual activity control/config owner, concrete parent/child activities, popup queue UI, SevenDay/ProcedurePreLoad/Main and original page resources are not production assembled. Services expose required dependencies without fallback success.',
        'Editor fixtures and native Button pointer dispatch/automatic statistics clock test the recovered state/widget path. No rendered original-page, Player or platform acceptance; controller registry remains18/38.'
    ])
(out/'ACTIVITY_STATES_SOURCE_EVIDENCE.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(dict(methods=len(methods),fields=len(fields),indexedMethods=len(index))))
