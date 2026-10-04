"""Record original launch/settings, framework Mono focus/pause/sampler and public domain lookup."""
from pathlib import Path
import hashlib,json,struct
helper=Path(__file__).resolve().parent/"recover_outgame_manager_registry.py";c={"__file__":str(helper)}
exec(helper.read_text().split("rows=[]")[0],c)
out=c["p"]/"generated/outgame";methods=[]
selected={26436,26437,26438,26439,26453,26479,26484,26487,26489,26500,26502,26503,26504,26507,26512,26514,26519,26527,26528,26529,26530,26531}
for row in json.loads((out/"method-map.json").read_text()):
    if row["metadata"] in selected:methods.append(dict(metadata=row["metadata"],sourceClass=row["cls"],method=row["method"],path=row["path"],sha256=hashlib.sha256((out/row["path"]).read_bytes()).hexdigest()))
assert len(methods)==22
fields=[]
for owner in (3433,3439,3440,3442,3443,3444):
    for k in range(c["ts"][owner][18]):
        n,t,token=struct.unpack_from("<3i",c["b"],c["pairs"][11][0]+12*(c["ts"][owner][8]+k));ptr=c["u"](200288+4*t)
        fields.append(dict(owner=owner,name=c["ms"](n),offset=c["u"](c["u"](3823136+4*owner)+4*k),typeCode=(c["u"](ptr+4)>>16)&255,typeData=c["u"](ptr)))
encoded=c["u"](4000760);assert encoded>>29==6
spec=struct.unpack_from("<3i",c["mem"],483008+12*((encoded&0x1ffffffe)>>1));method=c["md"][spec[0]]
singleton=dict(usage=4000760,methodSpec=list(spec),sourceClass=c["ms"](c["ts"][method[1]][0]),method=c["ms"](method[0]))
assert singleton["method"]=="AddComponent"
assert struct.unpack('<f',struct.pack('<I',1119879168))[0]==96
report=dict(metadataSha256=hashlib.sha256(c["b"]).hexdigest(),memorySha256=hashlib.sha256(c["mem"]).hexdigest(),methods=methods,fields=fields,singletonGeneric=singleton,
    findings=[
        "CommonSettings26439 initializes DBTSDK, stores Screen.dpi in Converter.DPI, if<=0 writes96 (NaN remains), sets targetFrameRate60 and sleepTimeout-1, sets culture. Captures AdsManager instance before reading MineGameName and AppSetting static string48; assigns gameName+comma+suffix to ads field12. Null ads fails only after both strings read. Then singleton normal26484->26500 sampler.",
        "StartGame26438 captures gameName/menuScene and calls TransitionAnima.Hide(callback).26453 updates menu scene only when nonempty; sends ordinary ReadyExitGame with null args; rereads optional AdsManager and hides banner; sets IsGoGameMenu=false; sets GlobalData string4; synchronously SceneManager.LoadScene(GameFrameworkLoad). Failure stops at the exact committed prefix. No completion fabricated.",
        "PauseGame26436 changes Time.timeScale to0/1 only when second flag true, before obtaining framework Mono and calling normal active-pause entry26507. TimeScale can change even if Mono is not initialized and discards pause input. IsPauseGame26437 tests Mono enum field12==1.",
        "Mono26512 finds GameFrameWorkMono root, creates/DontDestroyOnLoad only when absent; always AddComponent even when root already has one. Proven generic4000760 resolves GameObject.AddComponent, not GetComponent. Destroyed singleton component recreates on surviving root. No invented destruction cleanup.",
        "Mono constructor26503 defaults field21 sampling flag=true, field28 focus=true and allocates DomainData. Init26479 ReleaseLog then singleton field20=true, sets actual managed thread id, and only in release mode calls SDK IsSupportLogStatic endpoint, ignoring result.",
        "Normal OnApplicationFocus26502 gates on field20, then26489 only on focus change stores28 and sends GF_GameFocus [bool], then SetPause(!inputFocus,AppFocus0). Focus listener failure leaves changed focus and prevents subsequent pause handling. OnApplicationPause26504 uses AppPause1; active26507 uses ActiveTriggerPause2, both gated on initialization.",
        "Pause handler26519: if requested pause and state==0, store source then state1 and publish. If state nonzero, only upgrade source when incoming signed value higher; no event. Resume only when state1 and incoming source>=current; sets state0, logs exact enum-formatted reason, then publishes latest state/source after possible log reentry. Resume does not replace current source. Publish26487 sends [state==1, boxed source enum].",
        "Sampler26500 reads persisted value; only-1 generates shared RandomHelper inclusive1..100, assigns before UserDataPrefs.OnSave. Log sample, query net_status_game_statistic_filter, default threshold20; nonempty config int.TryParse can set0 on failure. Log threshold then if threshold<=sample set flag21=false; no branch sets it back true.",
        "DomainData26531 allocates List<DomainConfig3444>. Public GetValue26530 starts string.Empty, scans live list and matches GN/BN with string equality; type2 assigns TD and type3 RD. Last matching record wins including null value; other types keep empty. D field is not used by this public path. Alternate obfuscated domain helpers26527..29 are evidence only, not substituted.",
        "Native tests use actual component singleton/Init/Unity SendMessage, Time.timeScale and recovered TimeModule through real frame Update. Loop timer flushes via GF_NewGamePause, remains frozen under scaled pause, remains focus-gated after scale resume, resumes on focus message, and continues advancing when pause flag changes with changeTimeScale=false."
    ],boundaries=[
        "SDK/banner/transition/global-name/scene endpoints are explicit required host services; tests provide deliberate inputs and do not claim original transition visuals, real platform responses or full GameFrameworkLoad/Main scene wiring.",
        "Unity SendMessage proves actual component routing, not operating-system focus event delivery across platforms. Uncalled obfuscated GameFrameWorkMono/domain alternatives are not restored or claimed equivalent.",
        "Actual account/SDK/HTTP/permission/config owners, remaining20 controllers, full activities/business/reward return, Player and original audiovisual acceptance remain pending."
    ])
(out/"FRAME_LAUNCH_SOURCE_EVIDENCE.json").write_text(json.dumps(report,ensure_ascii=False,indent=2)+"\n")
print(json.dumps(dict(methods=len(methods),fields=len(fields),singleton=singleton)))
