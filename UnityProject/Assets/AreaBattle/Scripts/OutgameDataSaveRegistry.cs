using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // One DataManagerSave session: source lazy singleton dictionaries, not a save trigger.
    public sealed class OutgameDataSaveRegistry
    {
        Dictionary<string,string> strings;
        Dictionary<string,int> hashes;
        public IReadOnlyDictionary<string,int> Hashes=>hashes;
        public void RegisterSaveData(IOutgameVersionManager manager,string text)
        {
            if(strings==null)strings=new Dictionary<string,string>();
            if(hashes==null)hashes=new Dictionary<string,int>();
            strings[manager.DataKey]=text;
            string key=manager.DataKey;
            // Original calls text.GetHashCode after storing the raw text: null leaves a partial update.
            if(text==null)throw new NullReferenceException();
            hashes[key]=OriginalStringHash(text);
        }
        public string GetRegStringValue(IOutgameVersionManager manager)
        {return strings==null?null:strings[manager.DataKey];}
        // Original String.GetLegacyNonRandomizedHashCode f10222, UTF-16 and zero-terminated.
        public static int OriginalStringHash(string text)
        {
            if(text==null)throw new ArgumentNullException(nameof(text));
            unchecked
            {
                int first=5381,second=5381;
                for(int i=0;i<text.Length;i+=2)
                {
                    if(text[i]==0)break;first=((first<<5)+first)^text[i];
                    if(i+1==text.Length||text[i+1]==0)break;second=((second<<5)+second)^text[i+1];
                }
                return first+second*1566083941;
            }
        }
    }
    public static class OutgameLegacyMigration
    {
        // ProcedureStarGame f11032 after page setup, before prefab init and game-scene entry.
        public static void Apply(OutgameDataSaveRegistry registry,OutgameLocalDataManager local,OutgameSkinManager skins,OutgameCommanderManager commanders)
        {
            skins.ApplyLegacy(registry.GetRegStringValue(local));
            commanders.ApplyLegacy(registry.GetRegStringValue(local));
        }
    }
}
