# 现有障碍盘点

依据：`analysis/targets/wxcf1394487200e48f/43/generated/obstacle-visual-runtime.json`、RecoveredObstacleImporter.cs、RecoveredObstacleVisual.cs 及 `analysis/captures/obstacles-layout104-original-meshes.png`。资源目录共有15种障碍Prefab，均为墙体组合，共享墙体网格与材质。此为资源盘点，未重新运行Unity。

| Entity ID | 源名称 | 网格节点数 |
|---|---|---:|
|81|Wall_1|1|
|82|Wall_2|2|
|83|Wall_3|3|
|84|Wall_4|4|
|85|Wall_circle_8_180|5|
|86|Wall_circle_8_360|8|
|87|Wall_circle_16_180|8|
|88|Wall_circle_16_360|16|
|89|Wall_square_3_180|7|
|90|Wall_square_3_360|12|
|91|Wall_square_5_180|11|
|92|Wall_square_5_360|20|
|94|Wall_triangle_2_120|4|
|96|Wall_triangle_3_180|9|
|98|Wall_triangle_5_180|15|

概念总览选直墙、圆弧、方形开口与三角折线代表，完整落地需保留以上15种结构及其变换、开口和碰撞边界，不能用概念图形状替代实际布局。闭合圆环和闭合方形等变体也在完整范围内。当前没有证据需要新增树木/岩石障碍。

塔种范围：基础、突击、分流、箭塔；中立作为阵营状态，不新增玩法塔种。等级/阵营变化在后续单体生产时补齐。
