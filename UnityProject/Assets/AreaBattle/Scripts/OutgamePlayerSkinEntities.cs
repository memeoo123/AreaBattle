using System;
namespace AreaBattle
{
    // PlayerControl34200/34203. No skin selection or randomization is invented here.
    public sealed class OutgamePlayerSkinEntities
    {
        readonly Func<OutgameSkinCatalog> skins;readonly Func<OutgameLegacyConfigManager> config;
        readonly Func<OutgameLevelResourceState> level;readonly Func<OutgameGameControl> game;
        public OutgamePlayerSkinEntities(Func<OutgameSkinCatalog> skins,Func<OutgameLegacyConfigManager> config,Func<OutgameLevelResourceState> level,Func<OutgameGameControl> game)
        {this.skins=skins;this.config=config;this.level=level;this.game=game;}
        public int GetSkinEntityByType(int type)
        {
            int id=skins().UsedSkin(type);
            int entity=config().dicSkin.TryGetValue(id,out var row)?row.prefabId:unchecked(type*1000);
            return game().UseAnimationIns?unchecked(entity+3000):entity;
        }
        public int GetSkinEntityByRandomType(int type,int camp)
        {
            int id=0;int[] indices=null;int start=0;
            switch(type){case 1:indices=level().EnemyNormal;start=100;break;case 2:indices=level().EnemyDefense;start=200;break;case 3:indices=level().EnemyAttack;start=300;break;}
            if(type>=1&&type<=3)id=unchecked(start+indices[Math.Max(2,Math.Min(camp,4))-2]);
            int entity=config().dicSkin.TryGetValue(id,out var row)?row.prefabId:unchecked(type*1000);
            return game().UseAnimationIns?unchecked(entity+3000):entity;
        }
    }
}
