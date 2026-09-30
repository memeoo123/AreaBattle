using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameToolValidation
    {
        sealed class Effects:IOutgameToolEffects
        {
            public readonly List<string> Trace=new List<string>();public bool HasEntity;
            public void RefreshTopInfo()=>Trace.Add("top");
            public void GoldSpent(int id,long amount)=>Trace.Add("gold-spent:"+amount);
            public void ToolChanged(int id)=>Trace.Add("changed:"+id);
            public void UnlockScene(int id)=>Trace.Add("scene:"+id);
            public void UnlockSoldier(int id)=>Trace.Add("soldier:"+id);
            public bool ApplyItemEntity(int id,long delta){Trace.Add("entity:"+id+":"+delta);return HasEntity;}
            public void MissingItemEntity(int id)=>Trace.Add("missing:"+id);
            public void ReportGet(int id,int category,int amount,int balance,string reason)=>Trace.Add("get:"+id+":"+category+":"+amount+":"+balance+":"+reason);
            public void ReportCost(int id,int category,int amount,int balance)=>Trace.Add("cost:"+id+":"+category+":"+amount+":"+balance);
            public void Save()=>Trace.Add("save");
        }
        static void Require(bool yes,string why){if(!yes)throw new Exception(why);}
        static void Trace(Effects e,params string[] expected)=>Require(string.Join("|",e.Trace)==string.Join("|",expected),"trace="+string.Join("|",e.Trace));
        static OutgameToolDispatcher Create(Effects e,OutgameLocalInventoryState state)=>new OutgameToolDispatcher(new OutgameLocalInventory(state,BattleView.ReadText("Data/AllSkillConfig")),e,
            BattleView.ReadText("Data/Outgame/GameItemConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"),BattleView.ReadText("Data/Outgame/SkinConfig"));
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Tool dispatch verified with observed side-effect sink; real store, skin/item handlers, analytics transport and UI integration pending."};
            Action<string,Action> check=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=e.Message});}};
            check("outgame-tool-gold-debit-orders-statistics-notification-save",()=>{
                var e=new Effects();var s=new OutgameLocalInventoryState{goldNum=10};var d=Create(e,s);
                Require(d.Change(1001,-4)&&s.goldNum==6,"accepted debit");Trace(e,"top","gold-spent:4","changed:1001","cost:1001:1:4:6","save");
            });
            check("outgame-tool-failed-debit-still-notifies-and-saves",()=>{
                var e=new Effects();var s=new OutgameLocalInventoryState{goldNum=3};var d=Create(e,s);
                Require(!d.Change(1001,-4)&&s.goldNum==3,"failed debit retains balance");Trace(e,"top","changed:1001","save");
            });
            check("outgame-tool-skill-duplicate-notification-is-source-behavior",()=>{
                var e=new Effects();var d=Create(e,new OutgameLocalInventoryState());
                Require(!d.Change(2001,-3),"insufficient initial2");Trace(e,"changed:2001","changed:2001","save");e.Trace.Clear();
                Require(d.Change(2001,1,true,"reward"),"gain");Trace(e,"changed:2001","changed:2001","get:2001:3:1:3:reward","save");
            });
            check("outgame-tool-disabled-report-keeps-skill-local-event",()=>{
                var e=new Effects();var d=Create(e,new OutgameLocalInventoryState());
                Require(d.Change(2001,-1,false,"",false),"skill spend");Trace(e,"changed:2001","save");e.Trace.Clear();
                Require(d.Change(1005,1,false,"",false),"generic gain");Trace(e,"save");
            });
            check("outgame-tool-positive-overflow-source-report-not-success",()=>{
                var e=new Effects();var s=new OutgameLocalInventoryState{goldNum=int.MaxValue};var d=Create(e,s);
                Require(!d.Change(1001,1)&&s.goldNum==int.MaxValue,"mutation rejected");Trace(e,"top","changed:1001","get:1001:1:1:2147483647:","save");
            });
            check("outgame-tool-unknown-id-source-no-op-success-save",()=>{
                var e=new Effects();var d=Create(e,new OutgameLocalInventoryState());
                Require(d.GoodsType(999999)==0&&d.Change(999999,1),"source unknown branch retains default true");Trace(e,"save");
            });
            check("outgame-tool-generic-item-delegates-not-local-balance",()=>{
                var e=new Effects();var s=new OutgameLocalInventoryState();var d=Create(e,s);
                Require(d.GoodsType(1003)==6&&d.Change(1003,1),"package follows item entity, not strength");
                Require(s.strengthsNum==0,"no invented scalar package handling");Trace(e,"entity:1003:1","missing:1003","changed:1003","get:1003:3:1:0:","save");
                e.Trace.Clear();e.HasEntity=true;Require(d.Change(1003,-1,false,"",false),"signed entity delta");Trace(e,"entity:1003:-1","save");
            });
            check("outgame-tool-source-type-boundaries",()=>{
                var d=Create(new Effects(),new OutgameLocalInventoryState());
                foreach(int id in new[]{1,99})Require(d.GoodsType(id)==5,"scene interval");
                foreach(int id in new[]{100,399})Require(d.GoodsType(id)==4,"soldier interval");
                foreach(int id in new[]{2001,2999})Require(d.GoodsType(id)==3,"skill interval");
                Require(d.GoodsType(1001)==1&&d.GoodsType(1002)==2&&d.GoodsType(1005)==7,"explicit scalar overrides");
            });
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-tool-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
