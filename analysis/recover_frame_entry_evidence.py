"""Record source module-dispatch/teardown core and concrete statistics runtime composition."""
from pathlib import Path
import hashlib,json,struct
helper=Path(__file__).resolve().parent/"recover_outgame_manager_registry.py";c={"__file__":str(helper)}
exec(helper.read_text().split("rows=[]")[0],c)
out=c["p"]/"generated/outgame";methods=[]
for row in json.loads((out/"method-map.json").read_text()):
    if row["cls"] in ("GameFrameEntry","GameFrameModule") or row["metadata"] in (26452,26453,26740,31631,31731):
        methods.append(dict(metadata=row["metadata"],sourceClass=row["cls"],method=row["method"],path=row["path"],sha256=hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()))
generic=json.loads((out/"frame-entry-generics.json").read_text());proofs=[]
for row in generic["methods"]:proofs.append(dict(row,sha256=hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()))
fields=[]
for owner in (3433,3436,3467):
    for k in range(c["ts"][owner][18]):
        n,t,token=struct.unpack_from("<3i",c["b"],c["pairs"][11][0]+12*(c["ts"][owner][8]+k));ptr=c["u"](200288+4*t)
        fields.append(dict(owner=owner,name=c["ms"](n),offset=c["u"](c["u"](3823136+4*owner)+4*k),typeCode=(c["u"](ptr+4)>>16)&255,typeData=c["u"](ptr)))
report=dict(metadataSha256=hashlib.sha256(c["b"]).hexdigest(),memorySha256=hashlib.sha256(c["mem"]).hexdigest(),methods=methods,generics=proofs,fields=fields,
    findings=[
        "GameFrameEntry3433 holds static LinkedList<GameFrameModule> at0, started byte4, menu scene8, go-menu byte12, disposableActions16. Restored as explicit application-owned frame domain, permitting isolated validation domains without changing per-domain dispatch order.",
        "GetModule26446 resolves typeof(T) directly then26449 searches from Last by exact GetType equality. HaveModule26447 enumerates First-to-Last, returns typed instance or default, never constructs. Unused type-name resolver26448 has different GameFramework namespace validation and is not silently substituted into GetModule.",
        "Create26450 Activator/base-cast, logs before publication, scans First-to-Last comparing new.Priority > current.Priority and inserts before first lower entry; otherwise AddLast. Equal-priority insertion is stable. A failed cast dereferences null before nominal exception construction. Explicit dependency factories replace only reflection construction in restored modules.",
        "GetAll26440 returns copied list of live module references. Initialize26441 overwrites initialized callback before forwarding exact object[] args to virtual Initialize. No readiness flag set by frame itself.",
        "Start26442 sets culture, foreach calls every module Start without individual readiness gate, then sets started=true. Enumeration mutation and callback failures propagate; existing true flag is not reset before repeated Start.",
        "Update26444 checks only frame started, traverses linked nodes First/Next read after callback, invokes every Update(delta,unscaled). No per-module readiness gate or catch. ResourcesModuleUpdate26443 checks AppSetting static36 suppression then frame started before lazy Resources lookup and null-conditional update.",
        "Shutdown26445 traverses Last/Previous read after callback. Initialized modules Shutdown; others log type Name+模块没有初始化，不进行Shutdown操作. After traversal: clear modules, InputManager singleton Shutdown, permission cleanup, ConfigRead static0=false, invoke captured disposableActions. No reset of started/delegate field and no per-module catch.",
        "DataManagerPool26740 closes static save-ready flag4, calls SaveData, then if dictionary nonnull foreach live Values calls virtual OnRelease slot7 and clears dictionary. SaveData retains prior per-manager exception handling; OnRelease exceptions propagate and prevent Clear. Dictionary is retained, not nulled. Null rows are skipped by SaveData but fail during OnRelease.",
        "LocalDataManager31631 and SkinManager31731 OnRelease bodies are source nop; restored empty methods satisfy actual data-pool virtual cleanup without inventing data deletion.",
        "Existing concrete Logic/FSM/Procedure/Time modules implement shared frame contract using their recovered source priorities12/80/90/60 and lifecycle methods. Time ignores supplied frame delta and reads Unity time as its original Update does.",
        "StatisticsRuntime composes actual control/manager/offnet, storage/download host, source expansion, shared common-message singleton, UpdateManager singleton and TimeToRefresh singleton; statistics and refresh use the same frame disposable field. SDK clock/mode uses existing controller-platform owner and HTTP remains required explicit service. No fake server completion.",
        "Native composed shutdown uses actual LogicModule->DataManagerPool release before input/global tail and statistics disposal; original manager saves/releases first, controller removes queued update and clears common singleton next, refresh awaits scaled0.1 seconds. Independent runtime recreation reads compressed file and preserves pre-init source providers."
    ],boundaries=[
        "This restores frame module core and statistics runtime composition, not complete GameFrameEntry.CommonSettings/StartGame/PauseGame/GameFrameWorkMono or production Main/account/SDK/HTTP/permission owner graph. Those extracted bodies remain pending.",
        "ActivityControl/Manager/InitData and GameFrameWorkMono source extraction adds125 indexed methods for next implementation; extraction is not implementation or acceptance.",
        "Native platform, account/download and permission/config reset endpoints are controlled fixtures; actual source modules/statistics/file persistence and frame ordering run in Unity. Full activity/business, remaining20 roster controllers, Player and audiovisual acceptance are still open."
    ])
(out/"FRAME_ENTRY_SOURCE_EVIDENCE.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
print(json.dumps(dict(methods=len(methods),generics=len(proofs),fields=len(fields))))
