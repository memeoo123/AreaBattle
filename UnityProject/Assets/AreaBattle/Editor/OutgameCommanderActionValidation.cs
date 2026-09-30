using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameCommanderActionValidation
    {
        internal sealed class Effects:IOutgameCommanderEffects
        {
            public readonly List<string> calls=new List<string>();public Action save;
            public void AddStatistic(int id,long delta)=>calls.Add("add:"+id+":"+delta);
            public void SetCommanderStatistic(int id,int c,int level)=>calls.Add("set:"+id+":"+c+":"+level);
            public void SaveUserPreferences()=>calls.Add("prefs");
            public void SaveManagers(){calls.Add("save");save?.Invoke();}
            public void ReportUse(int c,int l)=>calls.Add("use:"+c+":"+l);
            public void ReportUnlock(int c,int l)=>calls.Add("unlock:"+c+":"+l);
            public void ReportLevelUp(int c,int l)=>calls.Add("levelup:"+c+":"+l);
            public void ShowUnlock(int c)=>calls.Add("show:"+c);
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Commander action source flow and real disk save callback. Platform reporting, actual statistics manager and page rendering not integrated."};
            Action<string,Action> check=(id,body)=>{try{body();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            string cfg=BattleView.ReadText("Data/Outgame/CommanderConfig"),skills=BattleView.ReadText("Data/AllSkillConfig");
            Func<OutgameProfile> fresh=()=>{var p=new OutgameProfile();OutgameProfileInitialization.Initialize(p,cfg,skills);return p;};
            var rules=new OutgameCommanderProgression(cfg,BattleView.ReadText("Data/Outgame/CommanderUpgradeConfig"));
            Func<OutgameProfile,Effects,OutgameCommanderActions> open=(p,e)=>{var inv=new OutgameLocalInventory(p.inventory,skills);return new OutgameCommanderActions(p,rules,inv.Count,(id,d)=>Require(inv.Change(id,d),"debit"),e,BattleView.ReadText("Data/Outgame/StatisticEventConfig"));};
            check("outgame-commander-select-preview-versus-equip",()=>{
                var p=fresh();p.commanders.Find(c=>c.id==1).level=3;var e=new Effects();var a=open(p,e);
                a.Select(2);Require(p.usedCommanderId==1&&e.calls.Count==0,"locked preview does not equip or report");
                a.Select(1);Require(p.usedCommanderId==1&&e.calls[0]=="use:1:3"&&!a.Select(1)&&e.calls.Count==1,"same selected item ignored");
            });
            check("outgame-commander-ui-level-gate-before-free-unlock",()=>{
                var p=fresh();var e=new Effects();var a=open(p,e);a.Select(1);
                Require(a.ClickUpgrade(5,out _)==OutgameCommanderActionResult.LevelLocked&&p.commanders[0].level==0&&e.calls.Count==0,"zero cost does not bypass level6");
                Require(a.ClickUpgrade(6,out _)==OutgameCommanderActionResult.Success&&p.commanders[0].level==1,"unlock at source boundary");
            });
            check("outgame-commander-unlock-save-before-autoequip",()=>{
                var p=fresh();p.commanders.Find(c=>c.id==1).level=4;p.inventory.goldNum=10000;
                string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"action.json");var store=new OutgameProfileStore(path);
                var e=new Effects{save=()=>store.Save(p)};var a=open(p,e);a.Select(2);
                Require(a.ClickUpgrade(999,out _)==OutgameCommanderActionResult.Success,"unlock");
                Require(string.Join(",",e.calls)=="add:120003:1,set:210003:2:1,add:220007:1,prefs,save,levelup:2:4,show:2,unlock:2:1,use:2:1","source statistics/save/report order including used commander level");
                var saved=store.Load();Require(saved.inventory.goldNum==0&&saved.commanders.Find(c=>c.id==2).level==1&&saved.usedCommanderId==1&&p.usedCommanderId==2,"source save precedes UI auto-equip; later lifecycle save still required");
            });
            check("outgame-commander-ui-insufficient-no-mutation",()=>{
                var p=fresh();p.inventory.goldNum=9999;var e=new Effects();var a=open(p,e);a.Select(2);
                Require(a.ClickUpgrade(999,out _)==OutgameCommanderActionResult.Insufficient&&p.inventory.goldNum==9999&&e.calls.Count==0,"no partial charge or save");
            });
            check("outgame-commander-ui-upgrade-does-not-show-unlock",()=>{
                var p=fresh();p.commanders.Find(c=>c.id==1).level=1;p.inventory.goldNum=250;var e=new Effects();var a=open(p,e);a.Select(1);e.calls.Clear();
                Require(a.ClickUpgrade(0,out int slot)==OutgameCommanderActionResult.Success&&slot==0,"owned upgrades do not recheck unlock level");
                Require(e.calls.Count==6&&e.calls[5]=="levelup:1:2"&&p.commanders[0].skillLevels[0]==2,"upgrade report after save; no unlock popup");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-commander-action-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
