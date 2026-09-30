# Skill presentation contract

18 ordinary skills: static calls, transforms, lifetimes and minimum asset bindings are in `skill-presentation-contract.json`. The resource-only handoff is `skill-presentation-minimum-resources.json`.

21 unique external effect prefab bundles; dependencies still required. Skills1/4/5 use embedded tower children. Skill6 has no dedicated visual. Skill14 uses current player type1 skin, default entity1000 soldier_100.

| Skill | External effect / direct pooled prefab |
|---|---|
| 1 | embedded tower children |
| 2 | Hdzd_Effect_Wy_Fire, 404, 408 |
| 3 | 413 |
| 4 | embedded tower children |
| 5 | embedded tower children |
| 6 | none dedicated |
| 7 | hdzd_eff_ZHG04_01 |
| 8 | 418 |
| 9 | 421, hdzd_eff_ZHG04_03 |
| 10 | hdzd_eff_ZHG03_01 |
| 11 | 420 |
| 12 | 417, 419 |
| 13 | 511 |
| 14 | 521 |
| 15 | 532, 531 |
| 16 | 611 |
| 17 | 612 |
| 18 | 633, hdzd_eff_wyys04, 631 |

Direct pooled objects2/7/9/10/18 do not use similarly named EffectConfig expiry. Skill18 ground631 and drag633 are manually instantiated too. Attached skills13/16 effects survive skill end until soldier clear; skill11 End resets tower state but does not clear existing soldier effects.

Boss bindings are separately recorded in `combat-boss-presentation-bindings.json`: camps5/6 models801/802, camps7..9 reference missing entity rows803..805. Soldiers11/12 use entities9001/9002;13..15 use3008. Missing rows are not replaced with invented models.
