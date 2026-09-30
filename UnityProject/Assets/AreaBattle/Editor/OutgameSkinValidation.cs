using System;
using System.IO;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameSkinValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source skin records, migration, unlock/equip mutations and save projection. Ordering, shop acquisition/payment, UI and startup pending."};
            try
            {
                string soldiers=BattleView.ReadText("Data/Outgame/SkinConfig"),maps=BattleView.ReadText("Data/Outgame/SceneSkinConfig");
                var catalog=OutgameSkinCatalog.FromOriginal(null,soldiers,maps);
                Require(catalog.UsedSkin(1)==100&&catalog.UsedSkin(2)==200&&catalog.UsedSkin(3)==300&&catalog.UsedSkin(4)==1,"four source default equip categories");
                Require(catalog.SoldierSkin(100).u&&!catalog.SoldierSkin(101).u&&catalog.SceneSkin(1).u&&!catalog.SceneSkin(2).u,"only lockState2 seeds unlocked");
                var empty=OutgameSkinCatalog.FromOriginal("{}",soldiers,maps);bool missing=false;try{empty.UsedSkin(1);}catch(System.Collections.Generic.KeyNotFoundException){missing=true;}
                Require(missing,"existing empty manager is not treated as missing manager payload");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-source-defaults-and-existing-empty-distinction",result="pass"});
                string held=@"{""skins"":[{""s"":100,""u"":false,""skinType"":99},{""s"":101,""u"":true},{""s"":9999,""u"":true}],""newSkins"":[101],""usedSkin"":[{""t"":1,""id"":101}]}";
                catalog=OutgameSkinCatalog.FromOriginal(held,soldiers,maps);
                Require(!catalog.SoldierSkin(100).u&&catalog.SoldierSkin(100).skinType==1&&catalog.SoldierSkin(101).isNew&&catalog.UsedSkin(1)==101,"held ownership/equip/new flags preserved and config fields refreshed");
                Require(catalog.SoldierSkin(9999)==null&&catalog.State.skins.Exists(s=>s.s==9999),"unknown saved skin retained but excluded from config index");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-held-records-and-config-metadata",result="pass"});
                catalog.ApplyLegacy(@"{""list_playerskin"":[{""skinId"":100,""isUnlock"":true},{""skinId"":101,""isUnlock"":false},{""skinId"":99999,""isUnlock"":true}],""sceneMapDatas"":[{""sceneId"":2,""isUnlock"":true},{""sceneId"":1,""isUnlock"":false}]}");
                Require(catalog.SoldierSkin(100).u&&!catalog.SoldierSkin(101).u&&catalog.SceneSkin(2).u&&!catalog.SceneSkin(1).u,"old flags overwrite both directions in correct categories");
                Require(catalog.SoldierSkin(101).isNew&&catalog.UsedSkin(1)==101,"migration does not normalize equipped locked skin or new flag");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-legacy-bidirectional-flags-only",result="pass"});
                var profile=new OutgameProfile{skins=catalog.State};string path=Path.Combine(BattleBuild.Workspace,"analysis/outgame-store-tests",Guid.NewGuid().ToString("N"),"skins.json");var store=new OutgameProfileStore(path);store.Save(profile);
                var loaded=new OutgameSkinCatalog(store.Load().skins,soldiers,maps);
                Require(loaded.SceneSkin(2).u&&!loaded.SceneSkin(1).u&&!loaded.SoldierSkin(101).u&&loaded.SoldierSkin(101).isNew&&loaded.UsedSkin(1)==101,"migrated skin state survives isolated restart");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-migration-disk-restart",result="pass"});
                catalog=OutgameSkinCatalog.FromOriginal(null,soldiers,maps);int eventCount=0;
                var actions=new OutgameSkinActions(catalog,(eventId,value)=>{
                    Require(eventId==210001&&value==4&&catalog.SoldierSkin(101).u&&catalog.SoldierSkin(101).isNew,"statistic observes committed unlock and includes three free skins");eventCount++;
                },BattleView.ReadText("Data/Outgame/StatisticEventConfig"));
                actions.UnlockSoldier(101,true);actions.UnlockSoldier(101,false);actions.UnlockSoldier(99999,true);
                Require(eventCount==1&&catalog.SoldierSkin(101).isNew&&catalog.UsedSkin(1)==100,"repeated/missing soldier unlock is inert and never auto-equips");
                actions.UnlockScene(2,true);actions.UnlockScene(2,false);actions.UnlockScene(99999,true);
                Require(eventCount==1&&catalog.SceneSkin(2).isNew&&catalog.UsedSkin(4)==1,"scene unlock has no statistic or auto-equip and repeat preserves new flag");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-unlock-source-effects-and-idempotence",result="pass"});
                catalog.SetUsedSkin(1,102);catalog.SetUsedSkin(9,99999);
                Require(catalog.UsedSkin(1)==102&&!catalog.SoldierSkin(102).u&&catalog.UsedSkin(9)==99999,"equip setter does not validate ownership/category/id");
                Require(!catalog.CheckSkinNew2Modify(101)&&!catalog.SoldierSkin(101).isNew&&!catalog.CheckSkinNew2Modify(101),"mark-seen always returns false");
                bool invalidSkin=false;try{catalog.CheckSkinNew2Modify(99999);}catch(ArgumentOutOfRangeException){invalidSkin=true;}
                Require(invalidSkin&&catalog.Skin(2)==catalog.SceneSkin(2),"skin lookup falls back to scene and missing throws");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-equip-and-new-flag-source-semantics",result="pass"});
                catalog.State.skins.Add(new OutgameSkinData{s=9998,isNew=true});catalog.State.newSkins.Add(7777);
                catalog.PrepareSave();profile.skins=catalog.State;store.Save(profile);
                loaded=new OutgameSkinCatalog(store.Load().skins,soldiers,maps);
                Require(loaded.UsedSkin(1)==102&&loaded.UsedSkin(9)==99999&&loaded.SceneSkin(2).isNew&&!loaded.SoldierSkin(101).isNew,"projected equipment and flags survive restart");
                Require(loaded.State.newSkins.Contains(9998)&&!loaded.State.newSkins.Contains(7777)&&loaded.State.newSkins.Count==2,"save rebuilds new list from all held records including unknown skins");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-save-projection-and-disk-restart",result="pass"});
                Require(OutgameSkinCatalog.SourceOrder(new OutgameSkinData{s=100,skinType=1,sortNo=900})==-99&&OutgameSkinCatalog.SourceOrder(new OutgameSkinData{s=1,skinType=4,sortNo=900})==-99,"default sentinel overrides explicit sort");
                Require(OutgameSkinCatalog.SourceOrder(new OutgameSkinData{s=102,skinType=1})==3&&OutgameSkinCatalog.SourceOrder(new OutgameSkinData{s=2,skinType=4})==2,"soldier/scene fallback offset differs");
                Require(OutgameSkinCatalog.SourceOrder(new OutgameSkinData{s=103,skinType=1,sortNo=-7})==-7,"nonzero explicit sort wins");
                var ordered=OutgameSkinCatalog.FromOriginal(@"{""skins"":[{""s"":300},{""s"":200},{""s"":100}]}",soldiers,maps);
                Require(ordered.OrderedSoldiers[0].s==300&&ordered.OrderedSoldiers[1].s==200&&ordered.OrderedSoldiers[2].s==100,"equal default sort keys preserve held dictionary insertion order");
                for(int i=1;i<ordered.OrderedSoldiers.Count;i++)Require(OutgameSkinCatalog.SourceOrder(ordered.OrderedSoldiers[i-1])<=OutgameSkinCatalog.SourceOrder(ordered.OrderedSoldiers[i]),"source ascending order");
                Require(ordered.OrderedScenes[0].s==1&&ordered.State.skins[0].s==300,"ordered index does not reorder saved list");
                report.checks.Add(new BattleBuild.Check{id="outgame-skin-source-order-stable-held-ties",result="pass"});


            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-skin-records",result="fail",detail=e.ToString()});}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-skin-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
