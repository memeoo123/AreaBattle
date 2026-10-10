# 多阶段任务的最小记录

优先复用项目已有进度文件。没有约定时，可在项目的 `art-style/<style-id>/` 记录以下三项；短咨询无需创建文件。路径用项目相对路径，临时工具输出转为项目资源后再登记。

## STYLE_BRIEF.md

记录范围/排除项、用户给定参考及其用途、已选方向或未定选项、镜头/色板/光照/字体规则、核心状态的辨识方式、样板内容与当前要求的终点。区分“用户决定”和“建议/暂定”，不得把建议自动记成确认。

必须记录当前阶段（inventory / style-selection / sample / rollout / acceptance）、style_decision（pending / selected / delegated）、选择依据（用户原话或指定参考）、样板检查证据、下一关卡。重启时追加新状态并将旧方案标为历史试验；旧通过记录不得自动解锁新方案。只有 selected 或明确 delegated 才进入生产样板。

## ASSET_MANIFEST.json

仅记录实际存在的条目；不提前编造路径或通过结果。可按项目简化字段：

```json
{
  "style_id": "candidate-a",
  "phase": "inventory",
  "assets": [
    {
      "id": "tower.basic",
      "render_type": "sprite",
      "source": null,
      "evidence": [],
      "target": null,
      "status": "inventoried",
      "variants": [],
      "spec": {},
      "generation": null,
      "checks": [],
      "blockers": []
    }
  ]
}
```

实际使用时 source/evidence 指向检查过的资源或代码；spec 记录尺寸、锚点、alpha、层级、动作等必要约束。generation 记录工具、提示词文件、参考图和输出。每个 check 包含检查内容、结果与证据路径/测量值；未做就是未做。

状态采用 inventoried / specified / generated / imported / runtime-checked / accepted，按真实进度更新。用户要求验收的任务中 accepted 不能只由“图片已生成”推导；用户未评审时另注明。

## PROGRESS.md

记录当前分支/基线、已完成范围、保留的用户改动、下一步具体入口、阻塞、相关输出和验证日期。历史通过只适用于记录的版本。概念图、引擎截图、编译检查、交互检查和性能数据分别列出，避免把一种证据代替另一种。

继续执行前读取记录并检查 diff：资源或代码变更影响到的检查需重做，未受影响的不重复运行。不要重新生成已选资源或覆盖用户已修改版本。
