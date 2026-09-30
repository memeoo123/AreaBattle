using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameManagerRegistrationCatalog
    {
        [Serializable] sealed class Catalog {public Entry[] registrations;}
        [Serializable] sealed class Entry {public int sourceTypeIndex;public string sourceClass,gameName;public bool autoSyn,compressData;}
        public static IEnumerable<OutgameManagerRegistration> Read(string json,Func<int,IOutgameDataManager> create)
        {
            if(create==null)throw new ArgumentNullException(nameof(create));
            var catalog=JsonUtility.FromJson<Catalog>(json);
            foreach(var entry in catalog.registrations)
            {
                int sourceType=entry.sourceTypeIndex;
                yield return new OutgameManagerRegistration(sourceType,entry.gameName,entry.autoSyn,entry.compressData,()=>create(sourceType));
            }
        }
    }
}
