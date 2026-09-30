"""Narrow parent-authorized Boss branches; fail if shared adapter changed unexpectedly."""
from pathlib import Path
p=Path('UnityProject/Assets/AreaBattle/Scripts/BattleView.cs');s=p.read_text(encoding='utf8')
def replace(a,b):
 global s
 assert s.count(a)==1,(a[:100],s.count(a));s=s.replace(a,b)
replace('        readonly Dictionary<int, SpriteRenderer> towerSprites', '        readonly Dictionary<int, RecoveredBossVisual> bossVisuals=new Dictionary<int, RecoveredBossVisual>();\n        readonly Dictionary<int, BossParameters> bossVisualParameters=new Dictionary<int, BossParameters>();\n        readonly Dictionary<int, SpriteRenderer> towerSprites')
replace('                spriteEffects.Clear();lastTowerHit.Clear();','                spriteEffects.Clear();lastTowerHit.Clear();bossVisuals.Clear();bossVisualParameters.Clear();\n                foreach(var cfg in JsonUtility.FromJson<BossTable>(ReadText("Data/BossConfig")).Datas)bossVisualParameters[cfg.id]=cfg;')
replace('                    var root=Own(new GameObject("Tower " + id));','''                    if(layout.StarInfoCfgs[i].isBoss) {
                        int camp=layout.StarInfoCfgs[i].CampID;
                        if(camp!=5&&camp!=6)throw new InvalidOperationException("Current source has no recovered Boss model for camp "+camp);
                        var boss=RecoveredBossVisual.Create(camp==5?801:802);var bossRoot=Own(boss.gameObject);bossRoot.name="Tower "+id;bossRoot.layer=8;bossRoot.transform.position=data.pos.World;bossRoot.transform.rotation=Quaternion.identity;
                        boss.ConfigureBossShadow(CampColor(camp));bossVisuals[id]=boss;towerObjects.Add(id,bossRoot);continue;
                    }
                    var root=Own(new GameObject("Tower " + id));''')
replace('            if(Audio!=null)Audio.OnBattleEvent(e,Simulation);','''            if(Audio!=null)Audio.OnBattleEvent(e,Simulation);
            if(e.Kind=="boss-action"&&bossVisuals.TryGetValue(e.TowerId,out var boss)&&bossVisualParameters.TryGetValue(Simulation.Tower(e.TowerId).BossSkillId,out var cfg))boss.BeginBossAction((int)e.Value,cfg);''')
replace('            Simulation.TickSkillCoroutines(scaledDelta);','''            foreach(var boss in bossVisuals.Values)boss.StepBossCoroutines(scaledDelta);
            Simulation.TickSkillCoroutines(scaledDelta);''')
replace('            if(Gestures!=null)Gestures.Tick(scaledDelta);','''            if(Gestures!=null)Gestures.Tick(scaledDelta);
            foreach(var pair in bossVisuals){if(Simulation.TryGetBossRain(pair.Key,out var point,out bool raining))pair.Value.SynchronizeRain(raining,point);pair.Value.Step(scaledDelta);}
            foreach(var obj in soldierObjects.Values){var bossSoldier=obj.GetComponent<RecoveredBossVisual>();if(bossSoldier!=null)bossSoldier.Step(scaledDelta);}''')
replace('                string key=Family[Mathf.Clamp(tower.ShipID,1,4)]','                if(tower.IsBoss)continue;\n                string key=Family[Mathf.Clamp(tower.ShipID,1,4)]')
replace('                        var dying=dyingObject.GetComponent<RecoveredSoldierVisual>();','''                        var bossDying=dyingObject.GetComponent<RecoveredBossVisual>();
                        if(bossDying!=null){if(!bossDying.IsDying)bossDying.BeginSoldierDeath(new Vector3(UnityEngine.Random.Range(-.1f,.1f),0,UnityEngine.Random.Range(-.1f,.1f)));bossDying.SynchronizeSoldier(soldier,BattleCamera);if(!bossDying.SoldierDeathFinished)live.Add(soldier.Id);continue;}
                        var dying=dyingObject.GetComponent<RecoveredSoldierVisual>();''')
replace('                    if(prefab!=null){visual=Own(Instantiate(prefab));visual.GetComponent<RecoveredSoldierVisual>().Begin(visualClock);}','''                    if(soldier.ShipType==11||soldier.ShipType==12){var bossSoldier=RecoveredBossVisual.Create(soldier.ShipType==11?9001:9002);bossSoldier.AttachSoldierShadow();visual=Own(bossSoldier.gameObject);}
                    else if(prefab!=null){visual=Own(Instantiate(prefab));visual.GetComponent<RecoveredSoldierVisual>().Begin(visualClock);}''')
replace('                else visual.transform.position=soldier.Position+Vector3.up*.05f;','''                else if(visual.TryGetComponent<RecoveredBossVisual>(out var bossSoldier))bossSoldier.SynchronizeSoldier(soldier,BattleCamera);
                else visual.transform.position=soldier.Position+Vector3.up*.05f;''')
p.write_text(s,encoding='utf8')
