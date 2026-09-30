using System;
namespace AreaBattle
{
    // BundleAssetInfo storage (type3817); constructors are supplied by the legacy services.
    public sealed class OutgameLegacyBundleLocation
    {
        public string BundleName, ObfuscatorName, RemoteURL, RandomSeed;
        public int Version;
        public bool IsInApp=true;
        public string LocalPath;
        public bool IsEncrypAB;
    }
    public interface IOutgameLegacyBundleServices
    {
        OutgameLegacyBundleLocation GetAssetBundleInfo(string name);
    }
    // AssetBundleManager23813: query the current ResourcesModule service on every call.
    public sealed class OutgameLegacyBundleResolver
    {
        readonly Func<IOutgameLegacyBundleServices> services;
        readonly Action<string> warning;
        readonly Action<ulong> writeOffset;
        public OutgameLegacyBundleResolver(Func<IOutgameLegacyBundleServices> services,Action<ulong> writeOffset,Action<string> warning)
        {this.services=services;this.writeOffset=writeOffset;this.warning=warning;}
        public OutgameLegacyBundleLocation Resolve(string name)
        {
            var record=services().GetAssetBundleInfo(name);
            ulong offset=record.IsEncrypAB?16UL:0UL;
            writeOffset(offset);
            if(string.IsNullOrEmpty(record.LocalPath))
            {warning(string.Format("[{0}]本地资源未找到 offset={1}",name,offset));return null;}
            return record;
        }
        public string ResolveUrl(string name)=>Resolve(name)?.LocalPath;
    }
    public static class OutgameLegacyResourceDefaults
    {
        // Utility(type3130)..cctor23872. Runtime writers are still being audited.
        public const int InitialAssetUnloadInterval=60;
    }
}
