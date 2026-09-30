import hashlib
import json
from datetime import datetime, timezone
from pathlib import Path

BASE=Path(__file__).parent
ROOT=BASE/'targets/wxcf1394487200e48f/43'
def read(rel): return json.loads((ROOT/rel).read_text(encoding='utf-8-sig'))
def save(rel,data): (ROOT/rel).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
manifest=read('manifest.json')
schema=read('generated/table-schemas.json')
catalog=read('generated/resource-catalog.json')
symbol=read('generated/gameplay-symbols.json')
assets=read('evidence/asset-inventory.json')
wasm=read('evidence/wasm-decode.json')
cache=read('evidence/cache-inventory.json')
probe=read('evidence/temp-data-probe.json')

checks=[]
for item in manifest['sourceArtifacts']:
    checks.append({'check':'source-and-copy-sha256','path':item['copy'],'passed':sha(Path(item['path'])).lower()==sha(ROOT/item['copy']).lower()==item['sha256'].lower()})
for report_path in sorted((ROOT/'evidence').glob('unpack-*.json')):
    report=json.loads(report_path.read_text(encoding='utf-8'))
    for e in report['entries']:
        p=Path(report['outputRoot'])/e['path']
        assert p.stat().st_size==e['size'] and sha(p)==e['sha256'],p
    checks.append({'check':'unpacked-entry-hashes','report':report_path.name,'count':report['fileCount'],'passed':True})
for entry in cache:
    assert sha(ROOT/'work/cache'/entry['relativePath']).lower()==entry['sha256'].lower()
checks.append({'check':'cached-resource-copy-hashes','count':len(cache),'passed':True})
assert probe['decodedSize']==15103186
checks.append({'check':'unity-data-size-matches-loader','expected':15103186,'actual':probe['decodedSize'],'passed':True})
assert all(x['valid'] for x in wasm)
checks.append({'check':'wasm-validation','count':len(wasm),'passed':True})
table_checks=[]
for t in schema:
    data=read(t['output'])
    rows=data[t['rowKey']] if t['rowKey'] else [data]
    assert len(rows)==t['rowCount']
    table_checks.append({'name':t['name'],'rows':len(rows),'duplicateIds':t['duplicateIds'],'multipleRecordSchemaCheck':len(rows)>=2})
checks.append({'check':'configuration-JSON-roundtrip','count':len(table_checks),'passed':True})
soldiers=read('generated/tables/SoldierConfig.json')['Datas']
entities={r['id'] for r in read('generated/tables/EntityModelConfig.json')['Datas']}
missing=[r['EntityID'] for r in soldiers if r['EntityID'] not in entities]
checks.append({'check':'soldier-EntityID-foreign-keys','checked':len(soldiers),'missing':missing,'passed':not missing})
save('evidence/validation.json',{'checks':checks,'tables':table_checks,'allChecksPassed':all(c['passed'] for c in checks)})

unknowns=[
 {'id':'U-006','subject':'soldier-entity-reference-mapping','status':'unknown','reason':'SoldierConfig EntityID 501, 650, 601 do not directly match EntityModelConfig.id. Five of eight rows do match.','verification':'Trace skin/model remapping in Soldier.Init and GetSoldierObj before treating these as missing assets.'},
 {'id':'U-001','subject':'representative-level-layout','status':'unknown','reason':'639 catalogued level layout bundles; none found in copied game file cache','verification':'Obtain catalogued level_0/1/2 bundles, parse TextAssets, verify CampInfoCfgs/StarInfoCfgs/ObstacleInfoCfgs and coordinates against the original game.'},
 {'id':'U-002','subject':'exact-combat-and-spawn-equations','status':'unknown','reason':'IL2CPP metadata contains types/methods and some obfuscated names; it contains no native method bodies. WASM has been decompressed, not decompiled.','verification':'Resolve metadata tokens to primary/secondary WASM functions; trace Tower.SoldierCollision, ChangeScore, BattleControl.GetSpawnTime; verify three numeric runtime cases.'},
 {'id':'U-003','subject':'active-LevelConfig-versus-LevelBConfig','status':'unknown','reason':'Both tables contain 561 records; active selection has not been traced.','verification':'Trace configuration selection and LevelControl.InitGameByLevelConfig/OnLevelCfgLoad.'},
 {'id':'U-004','subject':'runtime-complete-code-and-assets','status':'unknown','reason':'Two code packages cached; wasmcode2 declared but absent. Resource catalog has 2040 entries and only partial local coverage.','verification':'Inspect feature-controlled wasmcode2 loading for this desktop mode and build the dependency closure for one representative level.'},
 {'id':'U-005','subject':'win-loss-state-transitions','status':'unknown','reason':'LevelControl and gameplay state symbols available, branch behavior not yet reconstructed.','verification':'Trace OnGamePlayState/SetPlaySate and confirm original gameplay success/failure transitions.'}
]
save('generated/analysis-status.json',{'schemaVersion':'1.0','target':{'appId':'wxcf1394487200e48f','version':'43','windowTitle':'冲向那座塔','engine':'unity','engineVersion':'2021.3.56f2'},'currentStage':'static-logic-and-schema-partial','implementationReady':False,'unknowns':unknowns,'outputs':{'tables':'table-schemas.json','symbols':'gameplay-symbols.json','resources':'resource-catalog.json'},'goldenCases':'not-created: exact runtime outcomes not verified'})
cdn='https://gameoss-hz.entermore.cn/app-3/Release/Proj_hdzd/XYX/weixin/hcrzd/1.36'
downloads=[]
for i in range(3):
    row=next(x for x in catalog['entries'] if x['name']==f'data/levelcfg/level_{i}.json.unity3d')
    downloads.append({'logicalName':row['name'],'catalogLine':row['line'],'expectedBytes':row['size'],'catalogMd5':row['md5'],'candidateUrl':cdn+'/StreamingAssets/WebGL/Proj_hdzd/'+row['file'],'urlStatus':'derived-from-CDN-root-and-catalog; HTTP response not checked','destination':'work/remote/'+row['file']})
save('generated/next-download-plan.json',{'status':'not-executed','scope':'Three catalogued level data bundles only; no dependency install or execution of downloaded code','expectedBytes':sum(x['expectedBytes'] for x in downloads),'items':downloads})

lang=read('generated/tables/LanguageConfig.json')['Datas']
strategy=[{'id':x['id'],'text':x['zh_cn'],'status':'confirmed-as-in-game-help-text; runtime-unverified'} for x in lang if str(x['id']).startswith('StrategyDes_')]
save('generated/gameplay-help-evidence.json',strategy)

report=f'''# 冲向那座塔：本地逆向分析报告

## 目标与范围

- [已确认] 当前运行进程 AppID：`wxcf1394487200e48f`，本地缓存版本 `43`。微信小游戏窗口标题为“冲向那座塔”。定位记录：`evidence/locator-handoff.json`。
- 授权依据：用户要求“用这个skill ，执行一下当前运行的微信小游戏”。仅处理该目标本地包、资源缓存及 Unity 数据；源文件保留。
- 当前结果：包解密/解包、引擎识别、加载路径分析、配置表恢复和 IL2CPP 符号提取已完成；精确玩法还原未完成。

## 已确认结果

| 项目 | 结果 | 证据 |
|---|---|---|
| 引擎 | Unity 2021.3.56f2，竖屏 | `generated/modules/unity-namespace.js:16`；`app-config.json:1`；`evidence/webdata-unpack.json` 中 data.unity3d 的 UnityFS 版本头 |
| 微信插件 | UnityPlugin 1.2.83；与引擎版本分开记录 | `work/unpacked/__WITHOUT_MULTI_PLUGINCODE__/app-config.json:1` |
| 解包 | 3 个 wxapkg，共 27 个文件 | `evidence/unpack-*.json`，包含源包与各条目的 SHA-256、偏移和大小 |
| WASM | 主分片 13,867,396 字节，次分片 23,108,695 字节，均通过格式校验 | `evidence/wasm-decode.json`；未执行 WASM |
| Unity 数据包 | 从目标 temp 文件恢复 15,103,186 字节，和加载入口声明一致 | `evidence/temp-data-probe.json`；`generated/modules/game.js:20` |
| 元数据 | IL2CPP v31；11,003 类型、75,896 方法、46,852 字段；提取 420 个 Proj_hdzd 类型 | `generated/gameplay-symbols.json`，含二进制偏移、方法 token 与字段引用 |
| 本地资源 | 122 个缓存文件；121 个 Unity 资源包解析成功，盘点 6,244 个对象 | `evidence/cache-inventory.json`、`evidence/asset-inventory.json` |
| 配置 | 导出 89 个 TextAsset；整理 83 份 Config JSON，包含 76 张记录表和 7 个单例配置对象 | `generated/table-schemas.json`，含来源包、PathID、哈希、字段类型与样例 |
| 资源清单 | 2,040 条资源映射；639 个关卡布局文件，本地布局命中 0 | `generated/resource-catalog.json` |

Unity 数据包中的 `data.unity3d` 另盘点 4,109 个对象，详见 `evidence/data-unity3d-inventory.json`；此数量不与缓存对象数量混为去重总数。

## 核心配置

| 表 | 行数 | 已确认内容 |
|---|---:|---|
| LevelConfig | 561 | 关卡 ID、LevelType、SceneId 至 SceneId10 的映射 |
| LevelBConfig | 561 | 另一组映射；当前运行采用哪组仍待追踪，不能据此宣称有 1,122 个不同关卡 |
| SoldierConfig | 8 | EntityID、shipType、hp、attack、occupy、reinforce、voyage、param1/2 |
| AIConfig | 212 | AI 参数记录；字段含义与使用方式需要结合方法体确认 |
| CampConfig | 10 | 阵营 ID、颜色、模型参数 |
| EntityModelConfig | 155 | 8 个兵种中 5 条 EntityID 可直接匹配；501、650、601 未直接匹配，需追踪皮肤/模型重映射 |
| GlobalValueConfig | 9 | 原始全局参数；尚不推断时间单位或精确计算公式 |

例如 SoldierConfig 的 id=1 记录为 hp=1、attack=1、occupy=1、reinforce=1、voyage=5；id=2 为 2、2、1、2、5。这些是配置值，不是已验证的完整伤害公式。

## 玩法证据与追踪入口

[已确认：游戏内说明] LanguageConfig 的 StrategyDes 系列描述出兵连线、塔等级影响出兵速度、小兵碰撞、友军支援、攻城和箭塔。说明文本中写有每座城堡最多 3 条出兵路线。原始条目另存 `generated/gameplay-help-evidence.json`；尚未将说明文本等同于全部运行时边界行为。

[已确认：符号] `Tower` 包含 `SoldierCollision`、`ChangeScore`、`ChangeCamp`、`get_SpawnTime` 等方法；`BattleControl` 包含 `GetSpawnTime`、`GetDispatchLineNum`；`LevelControl` 包含 `InitGameByLevelConfig`、`OnLevelCfgLoad`、`OnGamePlayState`。部分字段和私有方法名已混淆。元数据提供声明，不提供方法实现。

[已确认：布局结构声明] `LevelInfoCfg` 含 `CampInfoCfgs`、`StarInfoCfgs`、`ObstacleInfoCfgs`。`StarInfoCfg` 含 pos、angle、scale、CampID、StartScore、ShipID、isBoss、bossSkillId；类型记录位于 global-metadata.dat 偏移 7,784,976，长度 88 字节。此结构不等于已获取关卡实例。

## 还原门槛与下一步

`implementationReady=false`。尚未生成可用于实现的 RESTORE_SPEC.json 或伪造数值 golden cases。

1. 补齐代表关卡布局，先取清单中的 level_0、level_1、level_2，预计合计 {sum(x['expectedBytes'] for x in downloads):,} 字节。确切文件名、清单行号、推导 URL 和目标路径已列入 `generated/next-download-plan.json`，尚未联网。
2. 将元数据 token 对应到 WASM 方法体，恢复碰撞扣值、占领、出兵间隔、升级阈值和胜负条件。
3. 对照原游戏验证代表关卡与至少三组数值结果，再输出 RESTORE_SPEC 和 golden cases。

## 验证与复现

`evidence/validation.json` 记录源包/副本哈希、27 个解包条目哈希、122 个缓存副本哈希、两个 WASM 格式校验、Unity 数据长度匹配、83 份配置 JSON 校验和兵种实体外键检查。文件完整性检查通过；兵种模型的直接外键匹配发现上述 3 个待查 ID，因此不能声明全部语义校验通过。

分析脚本位于 `analysis/`：`inspect_target.js`、`inspect_temp.js`、`inventory_assets.py`、`recover_schema.py`、`finalize_analysis.py`。工具版本及命令记录见 manifest.json。所有脚本均静态读取与导出，不启动目标游戏代码。
'''
(ROOT/'REVERSE_REPORT.md').write_text(report,encoding='utf-8')
progress=f'''# REVERSE_PROGRESS

- 目标：冲向那座塔 / wxcf1394487200e48f / 43
- 引擎：Unity 2021.3.56f2
- 授权：用户要求分析当前运行小游戏；范围为目标本地包及资源。
- 更新时间：{datetime.now(timezone.utc).isoformat()}
- 当前阶段：静态逻辑与 Schema 已取得部分结果；精确玩法待继续。

| 阶段 | 状态 | 输出 |
|---|---|---|
| 输入盘点 | 完成 | manifest.json、evidence/cache-inventory.json |
| 解密/解包 | 完成 | 3 包 / 27 文件；Unity WebData 6 条目 |
| 分包重建 | 部分完成 | 两个 WASM 分片有效；wasmcode2 声明但未缓存，运行时必要性待查 |
| 引擎识别 | 完成 | Unity 2021.3.56f2，多处独立证据 |
| 静态逻辑 | 部分完成 | 86 个 JS 模块索引，420 个玩法类型；WASM 方法体未恢复 |
| 数据 Schema | 部分完成 | 76 张记录表、7 个配置对象、布局类字段；639 个布局实例缺失 |
| 还原交接 | 未达门槛 | generated/analysis-status.json，implementationReady=false |

## 已确认结论

见 REVERSE_REPORT.md；配置、符号、资源各自带来源与哈希。原始包未修改。

## 意义明确的失败与限制

- 普通权限 CIM 进程读取拒绝访问；提升权限的只读 AppID/窗口标题查询成功。
- 未缓存实际关卡布局；尚未请求远程 CDN。无需重新解包已有结果。
- 元数据存在部分符号混淆；不把名称推测写成公式。
- 首次配置验证发现部分 JSON 是单例对象或 AudioInfos 数组，已修正 schema 提取器，保留原始结构。
- 兵种 EntityID 的直接外键匹配有 3 个未匹配值（501、650、601），需要追踪运行时映射。

## 下一步

1. 根据 generated/next-download-plan.json 获取代表关卡布局；执行下载前遵守所用技能的网络下载授权要求。
2. 追踪 Tower/BattleControl/LevelControl 的 WASM 方法体。
3. 验证代表关卡坐标、出兵与碰撞数值，更新 implementation gate。
'''
(ROOT/'REVERSE_PROGRESS.md').write_text(progress,encoding='utf-8')

manifest['target']['engine']='unity'
manifest['target']['engineVersion']='2021.3.56f2'
manifest['updatedAtUtc']=datetime.now(timezone.utc).isoformat()
manifest['currentStage']='static-logic-and-schema-partial'
manifest['implementationReady']=False
manifest['engineEvidence']=['generated/modules/unity-namespace.js:16','work/unpacked/__WITHOUT_MULTI_PLUGINCODE__/app-config.json:1','evidence/webdata-unpack.json: data.unity3d UnityFS header']
manifest['additionalSourceInventories']=['evidence/cache-inventory.json','evidence/temp-data-probe.json']
manifest['toolchain']={'node':'24.14.0','python':'3.14.2','assetPython':'3.12.14','UnityPy':'1.25.2','js-beautify':'2.0.3','scripts':[{'path':str(p),'sha256':sha(p)} for p in sorted(BASE.glob('*')) if p.suffix in {'.py','.js'}],'localCopiedTools':[{'path':p.relative_to(ROOT).as_posix(),'sha256':sha(p)} for p in sorted((ROOT/'work/tools').glob('*')) if p.is_file()]}
manifest['commands']=[
 {'command':'Get-CimInstance Win32_Process; filtered WeChatAppEx AppID and window title','exitCode':0,'note':'Initial sandbox query denied; read-only escalated query succeeded'},
 {'command':'python <skill>/scripts/init_target_workspace.py --analysis-root E:/Projects/AreaBattle/analysis --app-id wxcf1394487200e48f --version 43','exitCode':0},
 *[{'command':f'node work/tools/unpack_wxapkg.js --input {x["copy"]} --output work/unpacked/{Path(x["copy"]).stem} --wxid wxcf1394487200e48f --report evidence/unpack-{Path(x["copy"]).stem}.json','exitCode':0} for x in manifest['sourceArtifacts']],
 {'command':'node analysis/inspect_target.js','exitCode':0},
 {'command':'<bundled-python-3.12.14> -B analysis/inventory_assets.py','exitCode':0},
 {'command':'node analysis/inspect_temp.js','exitCode':0},
 {'command':'python -B work/tools/unpack_tuanjie_webdata.py generated/webgl.data --output-root work/webdata --report evidence/webdata-unpack.json','exitCode':0},
 {'command':'<bundled-python-3.12.14> -B work/tools/inventory_unity_bundle.py work/webdata/data.unity3d --vendor-root <UnityPy-1.25.2> --output evidence/data-unity3d-inventory.json','exitCode':0},
 {'command':'python -B analysis/recover_schema.py','exitCode':0},
 {'command':'python -B analysis/finalize_analysis.py','exitCode':0}
]
manifest['generatedArtifacts']=['REVERSE_REPORT.md','generated/analysis-status.json','generated/table-schemas.json','generated/gameplay-symbols.json','generated/resource-catalog.json','generated/gameplay-help-evidence.json','generated/next-download-plan.json','evidence/validation.json']
save('manifest.json',manifest)
index=json.loads((BASE/'REVERSE_TARGETS.json').read_text(encoding='utf-8'))
for row in index['targets']:
    if row['key']=='wxcf1394487200e48f/43': row.update(engine='unity',currentStage=manifest['currentStage'],updatedAtUtc=manifest['updatedAtUtc'])
(BASE/'REVERSE_TARGETS.json').write_text(json.dumps(index,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'report':str(ROOT/'REVERSE_REPORT.md'),'validationPassed':all(c['passed'] for c in checks),'tables':len(schema),'networkDownloads':0,'implementationReady':False},ensure_ascii=False,indent=2))
