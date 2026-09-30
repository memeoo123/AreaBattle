using System;
using System.Collections.Generic;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public static class OutgameConfigDerivedIndexes
    {
        // ConfigMgr30398: Add throws on duplicate/null names; no deduplication or rollback.
        public static void RebuildSceneResources(Dictionary<object,SceneSkinConfig> skins,Dictionary<string,int> resources)
        {
            resources.Clear();
            foreach(var pair in skins){resources.Add(pair.Value.idleIconName,12);resources.Add(pair.Value.gameIconName,12);}
        }
        // ConfigMgr30426: destination belongs to this manager; source comes from current Instance.
        public static void RebuildGuideLevels(Dictionary<int,int> levels,Func<Dictionary<object,GuideConfig>> currentGuides)
        {
            levels.Clear();
            foreach(var guide in currentGuides().Values)if(guide.guildLv!=-1)levels[guide.guildLv]=guide.id;
        }
        // ConfigMgr30428 appends on every call without clearing existing names.
        public static void AppendChineseNames(Dictionary<object,AINameConfig> names,List<string> destination)
        {foreach(var pair in names)destination.Add(pair.Value.zh_cn);}
    }
}
