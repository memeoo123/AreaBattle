using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle.EditorTools
{
    public static class OutgameCommanderViewValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Dynamic source cards and real unlock/upgrade buttons with explicit held profile fixture. 3D/platform/statistics consumers and full startup remain incomplete."};
            GameObject root=null;
            try
            {
                string cfg=BattleView.ReadText("Data/Outgame/CommanderConfig"),skills=BattleView.ReadText("Data/AllSkillConfig");
                var profile=new OutgameProfile();OutgameProfileInitialization.Initialize(profile,cfg,skills);profile.inventory.goldNum=10250;
                var inventory=new OutgameLocalInventory(profile.inventory,skills);var rules=new OutgameCommanderProgression(cfg,BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
                string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"view.json");var store=new OutgameProfileStore(path);
                var effects=new OutgameCommanderActionValidation.Effects{save=()=>store.Save(profile)};
                var actions=new OutgameCommanderActions(profile,rules,inventory.Count,(id,d)=>Require(inventory.Change(id,d),"debit"),effects,BattleView.ReadText("Data/Outgame/StatisticEventConfig"));
                root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Recovered/Outgame/CommanderUI"));
                Require(new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig")).Chinese("CommanderUI.UnlockSkillLevel").Contains("\n"),"source LangModule.Get expands escaped newline");
                profile.levelID=6;var levels=new OutgameLevelProgression(profile,new OutgameLevelProgressionValidation.Effects()){SpecialState=1,SpecialLevel=999};var view=root.AddComponent<OutgameCommanderView>();view.Bind(profile,rules,actions,()=>levels.CurrentLevel,inventory.Count,new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig")).Chinese,1,()=>levels.RealCurrentLevel);
                view.Card(2).GetComponent<Button>().onClick.Invoke();Require(actions.SelectedId==2&&profile.usedCommanderId==1&&view.Card(2).Find("objLock").gameObject.activeSelf,"locked card preview only");
                string info="objCommander/objCommanderInfo/";
                Require(!root.transform.Find(info+"objSkills/objSkillDetail").gameObject.activeSelf,"source Awake hides detail panel");
                Require(root.transform.Find(info+"objUpgrade/goUpgrade/textNum1").GetComponent<Text>().text=="10000","original unlock amount");
                Require(root.transform.Find(info+"imgTitle/textCommanderName").GetComponent<Text>().text=="时光守护者","original Chinese name");
                var skillCards=root.GetComponent<OutgameCommanderSkillCards>();
                Require(!skillCards.Card(0).Find("imgLock").gameObject.activeSelf&&skillCards.Card(1).Find("imgLock").gameObject.activeSelf,"real level6 unlocks first skill only despite normal fixture level999");
                skillCards.Card(1).GetComponent<Button>().onClick.Invoke();
                Require(root.transform.Find(info+"objSkills/objSkillDetail").gameObject.activeSelf&&root.transform.Find(info+"objSkills/objSkillDetail/textSkillTips").gameObject.activeSelf,"locked skill click opens source tip and details");
                Require(root.transform.Find(info+"objSkills/objSkillDetail/textSkillDetail").GetComponent<Text>().text.Contains("%"),"source speed description uses formatted percentage");
                view.CloseSkillDetail();profile.levelID=14;view.Refresh();Require(!skillCards.Card(1).Find("imgLock").gameObject.activeSelf&&skillCards.Card(2).Find("imgLock").gameObject.activeSelf,"real level14 second skill gate");
                var upgrade=root.transform.Find("objCommander/objCommanderInfo/objUpgrade/btnUpgrade").GetComponent<Button>();upgrade.onClick.Invoke();
                Require(profile.usedCommanderId==2&&profile.commanders.Find(c=>c.id==2).level==1&&inventory.Count(1001)==250,"real unlock button updates held account");
                Require(view.Card(2).Find("objUnlock/imgUsed").gameObject.activeInHierarchy&&!view.Card(2).Find("objLock").gameObject.activeSelf,"visible ownership/use refresh");
                Require(root.transform.Find(info+"objUpgrade/goUpgrade/textNum1").GetComponent<Text>().text=="250","next upgrade price replaces unlock cost");
                upgrade.onClick.Invoke();var saved=store.Load();
                Require(saved.inventory.goldNum==0&&saved.usedCommanderId==2&&saved.commanders.Find(c=>c.id==2).level==2,"second button action durably saves upgraded/used commander");
                Require(skillCards.Card(0).Find("imgTextBg/textSkillLv").GetComponent<Text>().text=="等级2"&&skillCards.Card(1).Find("imgUpgradeTip").gameObject.activeSelf,"upgraded skill level and next-slot hint refreshed");
                report.checks.Add(new BattleBuild.Check{id="outgame-skill-cards-real-level-gates-and-rotation",result="pass"});
                var descriptions=new OutgameSkillDescription(skills,BattleView.ReadText("Data/SkillConfig"),new OutgameLocalization(BattleView.ReadText("Data/Outgame/LanguageConfig")).Chinese);
                Require(descriptions.Description(1,1).Contains(" 5 ")&&descriptions.Description(1,2).Contains(" 6 "),"source duration changes with skill level");
                report.checks.Add(new BattleBuild.Check{id="outgame-skill-detail-click-and-source-values",result="pass"});
                Require(descriptions.Comparison(1,1)[1]=="5 <color=#FFFE18>+1</color>"&&descriptions.Comparison(1,10)[1]=="14","duration next level and max comparison");
                Require(descriptions.Comparison(5,1)[4]=="30% <color=#FFFE18>+10%</color>"&&descriptions.Comparison(5,10)[4]=="70%","decreasing speed uses positive percent delta and max current percent");
                Require(descriptions.Comparison(4,1)[1]=="10"&&descriptions.Comparison(4,1)[5]=="80% <color=#FFFE18>+10%</color>","unchanged duration has no delta and increasing speed transforms");
                view.ShowSkillDetail(1);var grid=root.transform.Find(info+"objSkills/objSkillDetail/objSkillData");
                Require(grid.gameObject.activeSelf&&grid.Find("ImgData7").gameObject.activeSelf&&!grid.Find("ImgData5").gameObject.activeSelf&&!grid.Find("ImgData6").gameObject.activeSelf,"only configured comparison rows visible");
                Require(grid.Find("ImgData7/textSlowPercent").GetComponent<Text>().text=="30% <color=#FFFE18>+10%</color>","native grid has actual source comparison");
                report.checks.Add(new BattleBuild.Check{id="outgame-skill-detail-comparison-grid",result="pass"});
                view.Card(1).GetComponent<Button>().onClick.Invoke();
                Require(root.transform.Find(info+"objSkills/objSkillDetail").gameObject.activeSelf&&grid.Find("ImgData4").gameObject.activeSelf&&!grid.Find("ImgData7").gameObject.activeSelf,"switch commander retains detail slot and refreshes rows");
                Require(root.transform.Find(info+"objSkills/objSkillDetail/textSkillName").GetComponent<Text>().text==descriptions.Name(2)&&skillCards.Card(1).Find("imgSelected").gameObject.activeSelf,"retained slot resolves new commander skill");
                view.CloseSkillDetail();view.Card(2).GetComponent<Button>().onClick.Invoke();
                Require(!root.transform.Find(info+"objSkills/objSkillDetail").gameObject.activeSelf,"hidden detail stays hidden on commander click");
                report.checks.Add(new BattleBuild.Check{id="outgame-skill-detail-retained-slot-on-commander-change",result="pass"});
                Require(view.Card(2).Find("objUnlock/textLv").GetComponent<Text>().text.EndsWith("2"),"rendered level refreshed");
                Require(view.Card(1).Find("imgIcon").GetComponent<Image>().sprite!=view.Card(2).Find("imgIcon").GetComponent<Image>().sprite,"distinct original portraits");
                report.checks.Add(new BattleBuild.Check{id="outgame-view-dynamic-card-unlock-upgrade-disk",result="pass"});
                view.Card(5).GetComponent<Button>().onClick.Invoke();
                Require(root.transform.Find(info+"objUpgrade/goUpgrade/textNum1").GetComponent<Text>().text=="500","diamond unlock price");
                profile.commanders.Find(c=>c.id==5).level=28;view.Refresh();
                Require(!root.transform.Find(info+"objUpgrade/goUpgrade").gameObject.activeSelf&&root.transform.Find(info+"objUpgrade/textMaxLevel").gameObject.activeSelf,"max hides price and shows max label");
                report.checks.Add(new BattleBuild.Check{id="outgame-view-localized-price-and-max",result="pass"});
                root.transform.Find(info+"objSkills/objSkillDetail").gameObject.SetActive(true);root.SetActive(false);typeof(OutgameCommanderView).GetMethod("OnDisable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(view,null);
                Require(!root.transform.Find(info+"objSkills/objSkillDetail").gameObject.activeSelf,"explicit editor invocation of disable callback closes skill detail");
                report.checks.Add(new BattleBuild.Check{id="outgame-view-detail-hidden-on-enter-and-leave",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-view-dynamic-card-unlock-upgrade-disk",result="fail",detail=e.ToString()});}
            finally{if(root)UnityEngine.Object.DestroyImmediate(root);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-commander-view-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}


