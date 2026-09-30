# 关内技能覆盖清单

目标 wxcf1394487200e48f / 43；本文件是静态覆盖，不是运行验收。

|技能|角色|代表配置 duration/data1/data2/data3|分支|机制|残余未知|
|---|---|---|---|---|---|
|1 冰冻|1|101: 5.0/0.0/0.0/0.0|activebranch|confirmed|运行验证|
|2 火焰|1|201: 5.0/10.0/2.0/2.0|activebranch|confirmed|RandomHelper upper-bound convention and interruption timing not runtime-tested|
|3 闪电|1|301: 0.7/20.0/0.0/0.0|activebranch|confirmed|运行验证|
|4 加速|2|401: 10.0/1.8/0.0/0.0|activebranch|confirmed|运行验证|
|5 减速|2|501: 10.0/0.7/0.0/0.0|activebranch|confirmed|运行验证|
|6 发展|2|601: 0.7/20.0/0.0/0.0|activebranch|confirmed|运行验证|
|7 夜魔侵袭|3|701: 6.0/0.0/0.0/0.0|activebranch|confirmed|Exact bat initial travel timing|
|8 血族契约|3|801: 6.0/0.0/0.0/0.0|activebranch|confirmed|运行验证|
|9 吸血|3|901: 1.0/5.0/0.0/0.0|activebranch|confirmed|运行验证|
|10 箭雨|4|1001: 3.0/60.0/2.0/0.0|activebranch|confirmed|Visual coroutine end-boundary versus active timer ordering|
|11 迅捷|4|1101: 10.0/1.8/1.8/0.0|activebranch|confirmed|运行验证|
|12 瞄准|4|1201: 3.0/5.0/0.0/0.0|activebranch|confirmed|Async selected-target visual travel and exact duration start|
|13 鼓舞|5|1301: 6.0/100.0/0.0/0.0|activebranch|confirmed|运行验证|
|14 募兵|5|1401: 5.0/25.0/0.0/0.0|activebranch|confirmed|randomPoint distribution, scene-unit step on pause|
|15 降临|5|1501: 1.7/30.0/0.0/0.0|activebranch|confirmed|AI selection strategy and invalid selection flow|
|16 虚弱|6|1601: 5.0/0.0/0.0/0.0|activebranch|confirmed|运行验证|
|17 恢复|6|1701: 8.0/3.0/0.0/0.0|activebranch|confirmed|运行验证|
|18 毒药|6|1801: 7.0/2.0/0.0/0.0|activebranch|confirmed|Bottle arrival versus duration start and pause timing|

全部18技能都有具体代码与配置绑定。角色5/6 usePercent=0不能证明玩家不能使用，因此不据此删去技能13–18。

## Boss 实际配置覆盖

扫描 639 份布局；只有 2 个 Star.isBoss=true（配置 [998, 999]）。CampInfo.BossActionId 不等于已激活调度。

- Config 1: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 2: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 3: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 4: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 5: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 6: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 7: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 8: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 9: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 10: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 11: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 12: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 13: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 14: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 15: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 16: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 17: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 18: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 19: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 20: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 21: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 22: **unknown**；Star引用0，CampInfo引用1。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 23: **unknown**；Star引用0，CampInfo引用3。Only CampInfo.BossActionId references it; confirmed initializer does not consume that field. Special-mode mutation not excluded.
- Config 24: **unusedconfig**；Star引用0，CampInfo引用0。No isBoss Star or CampInfo references in cached layouts. Do not require solely because table row exists.
- Config 999: **activebranch**；Star引用1，CampInfo引用0。isBoss=true Star references this bossSkillId
- Config 998: **activebranch**；Star引用1，CampInfo引用0。isBoss=true Star references this bossSkillId

Boss action1/2/5/6有实现；3/4/7–10在当前BossUseSkill中只停止前一协程，不执行新动作。特殊模式是否动态改写isBoss/bossSkillId仍为unknown。

## TowerBuff 与 Bastion

TowerBuff字段/叠加公式confirmed，但创建源unknown，当前初始化方法为空、GetBuff只读字典。不得凭字段存在加上任意概率或倍率。Bastion仅发现材质字段，不能臆造独立机制。

详细规则、精确二进制偏移和函数引用见 combat-evidence.json；本文件每个技能内已复制证据引用。
