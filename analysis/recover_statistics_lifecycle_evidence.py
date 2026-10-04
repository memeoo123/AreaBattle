"""Record source owner layout and runtime call evidence for statistics lifecycle restoration."""
from pathlib import Path
import hashlib
import json
import struct

helper = Path(__file__).resolve().parent / "recover_outgame_manager_registry.py"
context = {"__file__": str(helper)}
exec(helper.read_text().split("rows=[]")[0], context)
u, metadata, memory, pairs, types, methods, name = [context[k] for k in ("u", "b", "mem", "pairs", "ts", "md", "ms")]
out = context["p"] / "generated/outgame"
classes = []
for index in (3433, 3464, 3465, 4617, 4618, 4620, 4624):
    fields = []
    for field in range(types[index][18]):
        field_name, type_index, token = struct.unpack_from("<3i", metadata, pairs[11][0] + 12 * (types[index][8] + field))
        pointer = u(200288 + 4 * type_index)
        fields.append(dict(name=name(field_name), offset=u(u(3823136 + 4 * index) + 4 * field),
            typeCode=(u(pointer + 4) >> 16) & 255, typeData=u(pointer)))
    classes.append(dict(typeIndex=index, name=name(types[index][0]), fields=fields))
calls = []
for address in (3995064, 3995096, 3989352):
    encoded = u(address)
    assert encoded >> 29 == 6
    spec = struct.unpack_from("<3i", memory, 483008 + 12 * ((encoded & 0x1ffffffe) >> 1))
    method = methods[spec[0]]
    calls.append(dict(usageAddress=address, methodSpec=list(spec), sourceClass=name(types[method[1]][0]), method=name(method[0])))
source = []
for row in json.loads((out / "method-map.json").read_text()):
    if (35006 <= row["metadata"] <= 35035 or 35043 <= row["metadata"] <= 35060 or 35099 <= row["metadata"] <= 35127):
        source.append(dict(metadata=row["metadata"], sourceClass=row["cls"], method=row["method"], path=row["path"],
            sha256=hashlib.sha256((out / row["path"]).read_bytes()).hexdigest()))
runtime = []
for path, claim in (
    ("disassembly/StatisticsPoolGeneric-26734.txt", "Shared DataManagerPool.AddModel: runtime type key, AutoSyn from source attribute only (absent=false), dictionary.Add before optional OnInit, duplicate logs and returns supplied argument."),
    ("disassembly/StatisticsPoolBridge-8028.txt", "Statistics OnInit bridge supplies initialize=true to shared AddModel12658."),
    ("disassembly/StatisticsCast-1428.txt", "RegisterFromMessage table1428 maps to runtime function938: null accepted, incompatible non-null type throws; this is an explicit cast, not as.")):
    runtime.append(dict(path=path, sha256=hashlib.sha256((out / path).read_bytes()).hexdigest(), claim=claim))
slots = [dict(metadata=i, method=name(method[0]), slot=method[-2]) for i, method in enumerate(methods)
    if method[1] == 4624]
registration = next(row for row in json.loads((out / "data-manager-registration-roster.json").read_text())["managers"] if row["typeIndex"] == 4617)
report = dict(metadataSha256=hashlib.sha256(metadata).hexdigest(), memorySha256=hashlib.sha256(memory).hexdigest(),
    classes=classes, methods=source, strategySlots=slots, genericCalls=calls, runtimeEvidence=runtime, registration=registration,
    providerRegistrationOrder=[10000,10015,10011,10901,10900,10800,10003,10002,10001,10902,10008,10007,20000,10500,10501,10502,10600,10700,10013],
    findings=[
        "Manager creates offnet strategy before adding two ordinary/game message listeners. OnInit itself does not load. InitStrategy passes RefreshData to strategy slot4.",
        "RefreshData publishes Data then logs null without returning, indexes without clearing old keys, binds19 virtual delegates in source order with first-registration-wins, clears first-init flag before resolving/invoking completion Action.",
        "Manager callback calls strategy.LoadData before CommonModule_StatisticsData_Refresh with null args. SaveData forwards exact text to inherited storage. OnSave invokes static SaveDataDel, then rereads Strategy/Data/Datas; OnRelease removes game messages, disposes strategy, clears dictionary/Data, resets first-init flag but retains strategy.",
        "StatisticsControl35047 setter and35060 get_initOver both access field16 Action. The different bool field20 is initialized true by ctor; treating initOver as that bool is incorrect.",
        "Control initialization keeps existing providers, stores completion, gets4617 from pool or manually adds a new manager with initialize=true, calls InitStrategy before UpdateManager.AddHandle and GameFrameEntry.disposableActions subscription.",
        "Control Update unconditionally calls manager.Update before evaluating pool.IsEnableSaveData and dirty. It does not clear dirty. Dispose clears providers, queues update removal, removes itself from GameFrameEntry.disposableActions, replaces common dispatcher, clears completion; manager, singleton and flags remain.",
        "GameFrameEntry3433 static offset16 is disposableActions, not an application-quit SDK action. This is a required host property until full entry composition.",
        "RegisterFromMessage casts provider before unboxing eventId; wrong provider type throws, null registers. SetFromMessage first skips existing providers, then calls value.ToString and Int64.TryParse; failed parse evaluates ToString again for Int32.TryParse. Only arities2/3 dispatch, child index unboxed as Int32.",
        "Expansion wrappers resolve current controller again after notifications, then mark dirty. Parent set10000 alone omits dirty write. GameValue catches only custom provider execution; Exception.get_Message slot5 (metadata3571) supplies log text, not Exception.ToString slot3.",
        "GameValue without provider: null/empty args use parent count; one int-parsable argument uses parent if0, otherwise child; invalid or unsupported arity logs and returns0. Parsing/formatting/record errors outside custom-provider invocation propagate.",
        "Strategy base InitData stores callback, resolves manager from pool, registers CommonModule_Activity_Lunch. Update passes Array.Empty<object> to virtual UpdateDate without a readiness gate. Activity handler increments child event20000 by1. Dispose removes listener before virtual cleanup.",
        "Strategy base calendar providers use GameValue10000 and original TimeHelper epoch. Value10600 accepts null/empty and returns0, Value10700 returns0 only for non-null empty arrays; null fails. Value10013 reads parent count. Base LoadData/OnSave are source-empty virtual bodies;13 event providers/UpdateDate/cleanup remain abstract."
    ], boundaries=[
        "Concrete GameStatisticsOffNetStrategy4623 and StatistUtils codec/clock/daily readiness coroutine are still pending. No fallback strategy, successful account state or platform result is supplied.",
        "Manager/control/base are exercised with an explicitly isolated Probe strategy; this does not establish complete statistics startup, offline persistence/day reset, production Main or a Player.",
        "Manual UpdateManager invocation verifies delegate scheduling/removal order, not native PlayMode time progression. Statistics controller is outside38 LogicModule roster; count remains18/38."
    ])
(out / "STATISTICS_LIFECYCLE_SOURCE_EVIDENCE.json").write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
print(json.dumps(dict(methods=len(source), runtimeEvidence=len(runtime), fields=sum(len(row["fields"]) for row in classes))))
