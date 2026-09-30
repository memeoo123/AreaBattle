using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameLegacyCommanderRepairValidation
    {
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,run)=>{try{run();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-103-repair-first-charge-stats-and-skill-distribution",()=>{
                var record=new OutgameLocalRecord{firstChargeData=new OutgameFirstChargeRecord{hasCharge=true,rewardState=3}};
                var gift=new OutgameCommanderState{id=3,level=0,skillLevels=new[]{7,8,9}};var high=new OutgameCommanderState{id=4,level=2,skillLevels=new[]{99,99,99,77}};var small=new OutgameCommanderState{id=5,level=0,skillLevels=new[]{8,9,10}};
                var values=new Dictionary<int,long>{{3,0},{4,long.MaxValue},{5,2}};var trace=new List<string>();int key=100;
                var repair=new OutgameLegacyCommanderRepair(()=>false,()=>record,id=>gift,()=>new[]{gift,high,small},()=>key,(e,id,v)=>{Require(gift.level==1&&record.dealOldComm==1,"mutations before statistics write");values[id]=v;key=101;trace.Add("set:"+e);},(e,args)=>{Require(e==101,"key captured after first-charge callback");return values[(int)args[0]];},message=>trace.Add(message));
                repair.Handle103VersionBug();Require(gift.level==1&&gift.skillLevels[0]==7,"gift no extra skill rewrite when statistic equals level");Require(high.level==28&&string.Join(",",high.skillLevels)=="10,10,10,77","cap28 reset three skills then distribute27 upgrades");Require(small.level==2&&string.Join(",",small.skillLevels)=="2,1,10","small level leaves untouched third skill");Require(trace.Count==2&&trace[1]=="set:100","source warning then first-charge statistic");
                string json=record.ToOriginalJson();Require(OutgameLocalRecord.Read(json).dealOldComm==1,"processed marker survives source serialization");
            });
            test("original-103-repair-guards-and-early-marker",()=>{
                var record=new OutgameLocalRecord();int reads=0;
                var repair=new OutgameLegacyCommanderRepair(()=>true,()=>{reads++;return record;},null,null,null,null,null,null);repair.Handle103VersionBug();Require(reads==0,"installation guard before record access");
                record.dealOldComm=1;repair=new OutgameLegacyCommanderRepair(()=>false,()=>record,null,null,null,null,null,null);repair.Handle103VersionBug();
                record.dealOldComm=2;repair=new OutgameLegacyCommanderRepair(()=>false,()=>record,null,null,null,null,null,m=>throw new InvalidOperationException("warning"));bool failed=false;try{repair.Handle103VersionBug();}catch(InvalidOperationException){failed=true;}Require(failed&&record.dealOldComm==1,"only exact1 skips; marker written before warning failure");
            });
            test("original-103-repair-partial-failure-and-negative-statistic",()=>{
                var record=new OutgameLocalRecord();var negative=new OutgameCommanderState{id=1,level=-5,skillLevels=null};var invalid=new OutgameCommanderState{id=2,level=0,skillLevels=new[]{9}};
                var repair=new OutgameLegacyCommanderRepair(()=>false,()=>record,null,()=>new[]{negative,invalid},()=>8,null,(e,a)=>(int)a[0]==1?-2:4,m=>{});bool failed=false;try{repair.Handle103VersionBug();}catch(IndexOutOfRangeException){failed=true;}
                Require(failed&&record.dealOldComm==1&&negative.level==0&&invalid.level==4&&invalid.skillLevels[0]==1,"clamp lower bound and preserve partial writes on invalid skills");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
