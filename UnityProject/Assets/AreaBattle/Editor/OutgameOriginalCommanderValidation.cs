using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameOriginalCommanderValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Provided source legacy commander records only; full login, skin migration and startup remain pending."};GameObject root=null;
            try
            {
                string cfg=BattleView.ReadText("Data/Outgame/CommanderConfig"),skills=BattleView.ReadText("Data/AllSkillConfig");
                var p=new OutgameProfile{usedCommanderId=5};OutgameProfileInitialization.Initialize(p,cfg,skills);p.commanders.Find(c=>c.id==2).level=20;p.inventory.goldNum=100000;
                string json=@"{""UsedCommanderId"":2,""commanderDatas"":[{""Id"":2,""curLevel"":2,""isNew"":true,""skillsData"":[{""skillId"":5,""skillLevel"":8},{""skillId"":4,""skillLevel"":7},{""skillId"":6,""skillLevel"":6}]},{""Id"":999,""curLevel"":3,""skillsData"":[]}]}";
                Require(OutgameOriginalCommanders.ApplyLegacy(p,json,cfg),"nonempty legacy data accepted");var held=p.commanders.Find(c=>c.id==2);
                Require(held.level==2&&held.isNew&&held.skillIds[0]==5&&held.skillLevels[0]==8&&p.usedCommanderId==2,"source replaces even lower level and preserves skill order/selection");
                Require(p.commanders.Find(c=>c.id==1).level==0&&p.commanders.Find(c=>c.id==1).skillLevels[0]==1&&p.commanders.Exists(c=>c.id==999)&&p.inventory.goldNum==100000,"missing defaults, unknown record and unrelated inventory survive");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-commander-replace-and-preserve-records",result="pass"});
                var old=p.commanders;Require(!OutgameOriginalCommanders.ApplyLegacy(p,null,cfg)&&!OutgameOriginalCommanders.ApplyLegacy(p,"{}",cfg)&&!OutgameOriginalCommanders.ApplyLegacy(p,"{\"commanderDatas\":[]}",cfg)&&ReferenceEquals(old,p.commanders),"empty legacy does not reset held commanders");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-commander-empty-keeps-held",result="pass"});
                var rules=new OutgameCommanderProgression(cfg,BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));var inventory=new OutgameLocalInventory(p.inventory,skills);
                Require(rules.TryUpgrade(held,inventory.Count,(id,d)=>inventory.Change(id,d),out int slot)&&slot==1&&held.skillIds[1]==4&&held.skillLevels[1]==8,"upgrade targets retained source skill slot");
                string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"commander-original.json");var store=new OutgameProfileStore(path);store.Save(p);var loaded=store.Load().commanders.Find(c=>c.id==2);
                Require(loaded.isNew&&loaded.skillIds[0]==5&&loaded.skillLevels[1]==8,"original identity and levels survive restart");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-commander-upgrade-and-restart",result="pass"});
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/CommanderUI"));var view=root.AddComponent<OutgameCommanderView>();
                var actions=new OutgameCommanderActions(p,rules,inventory.Count,(id,d)=>inventory.Change(id,d),new OutgameCommanderActionValidation.Effects(),BattleView.ReadText("Data/Outgame/StatisticEventConfig"));
                var localize=new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig"));view.Bind(p,rules,actions,()=>30,inventory.Count,localize.Chinese,2,()=>30);view.ShowSkillDetail(0);
                Require(root.transform.Find("objCommander/objCommanderInfo/objSkills/objSkillDetail/textSkillName").GetComponent<Text>().text==localize.Chinese("Commander.SkillName.5"),"view reads held skill ID instead of commander default");
                Require(root.GetComponent<OutgameCommanderSkillCards>().Card(0).Find("imgTextBg/textSkillLv").GetComponent<Text>().text.EndsWith("8"),"card retains held skill level");
                report.checks.Add(new BattleBuild.Check{id="outgame-original-commander-view-held-skill-identity",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-original-commander",result="fail",detail=e.ToString()});}
            finally{if(root)UnityEngine.Object.DestroyImmediate(root);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-original-commander-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
