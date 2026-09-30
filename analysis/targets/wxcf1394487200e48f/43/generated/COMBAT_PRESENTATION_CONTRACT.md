# Combat presentation event contract

Static local evidence; target wxcf1394487200e48f/43. Full details, source offsets and asset paths are in `combat-presentation-contract.json`.

Death: contact deaths, arrow hits and ClearInSkill rain/poison retain the existing model, play dead without looping, drift locally by independent random X/Z in [-0.1,+0.1], and remove after clip length minus 0.05 seconds. Arrival and skill1 removal are immediate. Dying soldiers leave combat lists immediately.

Audio: every voice request respects IsSoundEffect. IDs2012/2013 separately throttle on integer wall seconds, permitting equality. Capture2005/2006 uses a per-tower strict inequality and only player-involved camp changes. Clock base is local 1970-01-01 08:00:00.

104: nonzero score change from a different camp, strict Time.time difference >0.5; camp comparison happens before capture. 105: grade increase only. Both are at tower position +up*0.258, zero Euler rotation. 106 has no confirmed effect caller.

lastHit160 resets during Tower.Init, but Unity Time.time does not reset on restart; no audited reset of lastAudio168. Effect initialization preserves prefab localScale, sets parent(false), localPosition and zero localEulerAngles. Type3 defaults to WorldEffectRoot. Configured duration is a scaled WaitForSeconds after the model is ready.

| Model | Dead clip seconds | Removal seconds |
|---|---:|---:|
| soldier_100 | 0.7000000 | 0.6500000 |
| soldier_200 | 0.6667000 | 0.6167000 |
| soldier_300 | 0.8000000 | 0.7500000 |

| Audio ID | Config path | Export |
|---|---|---|
| 1001 | Audio/Loop/hcrzd_GameBGM.wav | .m4a |
| 2005 | Audio/once/game/tower_occupy01.mp3 | .m4a |
| 2006 | Audio/once/game/tower_occupy02.mp3 | .m4a |
| 2007 | Audio/once/game/connectSuccess.mp3 | .m4a |
| 2008 | Audio/once/game/connectCut.mp3 | .m4a |
| 2009 | Audio/once/ui/overui_victory.mp3 | .m4a |
| 2010 | Audio/once/ui/failUI_fail.mp3 | missing |
| 2012 | Audio/once/game/tower_soldier_collider.mp3 | .m4a |
| 2013 | Audio/once/game/soldier_soldier_collider.mp3 | .m4a |
| 2014 | Audio/once/game/tower_upgrade.mp3 | .m4a |
| 2015 | Audio/once/game/startConnectLine.mp3 | .m4a |
