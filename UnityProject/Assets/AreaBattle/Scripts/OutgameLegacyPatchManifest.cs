using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public sealed class OutgameLegacyPatchElement
    {
        public string EName,ObfuscatorName,MD5;
        public ulong Size;
        public int Version;
    }
    // PatchManifest29838-29840: retain untrimmed strings and source exception behavior.
    public sealed class OutgameLegacyPatchManifest
    {
        public bool IsEncrypAB;
        public string RandomSeed,ResourceVersionData,GFVersion;
        public int ResourceVersion;
        readonly Dictionary<string,OutgameLegacyPatchElement> elements=new Dictionary<string,OutgameLegacyPatchElement>();
        public OutgameLegacyPatchManifest(string text,Action<string> error)
        {
            if(string.IsNullOrEmpty(text)){error("清单文件为空");return;}
            var lines=text.Split('\n');var header=lines[0].Split('|');
            ResourceVersion=Convert.ToInt32(header[0]);GFVersion=header[2];
            if(header.Length>=4){IsEncrypAB=true;RandomSeed=header[3];}
            ResourceVersionData=header[1];
            for(int i=1;i<lines.Length;i++)
            {
                var row=lines[i].Split('|');if(row.Length<4)continue;
                var element=new OutgameLegacyPatchElement{EName=row[0],ObfuscatorName=row[1],MD5=row[2],Size=Convert.ToUInt64(row[3]),
                    Version=row.Length==5?Convert.ToInt32(row[4]):ResourceVersion};
                elements.Add(element.EName,element);
            }
        }
        public bool TryGetValue(string name,out OutgameLegacyPatchElement element)=>elements.TryGetValue(name,out element);
    }
    // Shared record construction from services3818/3820 and BundleAssetInfo29813/29818.
    public abstract class OutgameLegacyManifestServices:IOutgameLegacyBundleServices
    {
        protected OutgameLegacyPatchManifest Manifest;
        readonly Action<string> warning,log;
        readonly Func<bool> debug;
        protected OutgameLegacyManifestServices(Action<string> warning,Func<bool> debug,Action<string> log)
        {this.warning=warning;this.debug=debug;this.log=log;}
        protected abstract string Prefix {get;}
        protected abstract bool IsInApp {get;}
        public OutgameLegacyBundleLocation GetAssetBundleInfo(string name)
        {
            if(Manifest!=null&&Manifest.TryGetValue(name,out var element))
            {
                var record=new OutgameLegacyBundleLocation{BundleName=name,LocalPath=Prefix+element.ObfuscatorName,RemoteURL=string.Empty,
                    Version=element.Version,IsEncrypAB=Manifest.IsEncrypAB,ObfuscatorName=element.ObfuscatorName,RandomSeed=Manifest.RandomSeed,IsInApp=IsInApp};
                if(debug())log(string.Format("CreateBundleAssetInfo:BundleName=[{0}] LocalPath=[{1}] RemoteURL=[{2}] Version=[{3}]  IsEncrypAB=[{4}] obfuscatorName[{5}] randomSeed[{6}] isInApp[{7}]",
                    record.BundleName,record.LocalPath,record.RemoteURL,record.Version,record.IsEncrypAB,record.ObfuscatorName,record.RandomSeed,record.IsInApp));
                return record;
            }
            warning("Not found element in patch manifest : "+name);
            return new OutgameLegacyBundleLocation{BundleName=name,LocalPath=string.Empty,RemoteURL=string.Empty};
        }
        public string GetGFVersion()=>Manifest==null?"empty":Manifest.GFVersion.Split(',')[0];
        public string GetUACBuildID()
        {if(Manifest==null)return "empty";var parts=Manifest.GFVersion.Split(',');return parts.Length>=2?parts[1]:"empty";}
        public bool GetIsDeepObf()
        {if(Manifest==null)return false;var parts=Manifest.GFVersion.Split(',');return parts.Length>=3&&parts[2].Trim()=="1";}
    }
    public sealed class OutgameLegacyPatchBundleServices:OutgameLegacyManifestServices
    {
        readonly string prefix;readonly int mode;
        public OutgameLegacyPatchBundleServices(string text,string root,int mode,Action<string> error,Action<string> warning,Func<bool> debug,Action<string> log):base(warning,debug,log)
        {Manifest=new OutgameLegacyPatchManifest(text,error);prefix=root+"/";this.mode=mode;}
        protected override string Prefix=>prefix;
        protected override bool IsInApp=>mode==1;
    }
    public sealed class OutgameLegacyStreamingBundleServices:OutgameLegacyManifestServices
    {
        readonly Func<string> streamingPath;readonly Action<string> error;
        public OutgameLegacyStreamingBundleServices(Func<string> streamingPath,Action<string> error,Action<string> warning,Func<bool> debug,Action<string> log):base(warning,debug,log)
        {this.streamingPath=streamingPath;this.error=error;}
        public void ReadStreamingPatchManifest(string text)=>Manifest=new OutgameLegacyPatchManifest(text,error);
        protected override string Prefix=>streamingPath();
        protected override bool IsInApp=>true;
    }
}
