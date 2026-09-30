using System;
using System.Security.Cryptography;
using System.Text;
namespace AreaBattle
{
    public interface IOutgameDataStorageHost
    {
        int SourceLoginProgress {get;}
        bool LoginProcedureFlag8 {get;}
        bool LoginStaticFlag4 {get;}
        bool IsUseServer {get;}
        string MineGameName {get;}
        bool HasToast {get;}
        void Log(string text);
        void Error(string text);
        void Toast(string text);
        string Compress(string dataKey,string text);
        string Decompress(string dataKey,string text);
        void QueueUpload(string dataKey,string text);
    }
    // Original DataManagerBase storage path f20028/f8029/f20029. All account flags
    // are required from the active login host; no synthetic successful login default.
    public sealed class OutgameDataManagerStorage
    {
        readonly Func<string> key;
        readonly IOutgameDataStorageHost host;
        readonly OutgameSdkStringStorage storage;
        readonly OutgameDataVersionState versions;
        public bool SaveLocal=true,AutoSyn=true,CompressData;
        public string LastDigest {get;private set;}="";
        bool keyValidated;
        public OutgameDataManagerStorage(Func<string> key,IOutgameDataStorageHost host,OutgameSdkStringStorage storage,OutgameDataVersionState versions)
        {this.key=key??throw new ArgumentNullException(nameof(key));this.host=host??throw new ArgumentNullException(nameof(host));this.storage=storage??throw new ArgumentNullException(nameof(storage));this.versions=versions??throw new ArgumentNullException(nameof(versions));}
        public string ReadLocalData()
        {
            string text=storage.GetString(host.MineGameName+key(),"");
            return CompressData?host.Decompress(key(),text):text;
        }
        public void SaveLocalData(string text)
        {
            if(!keyValidated)
            {
                bool checkedKey=false;
                try{string name=key();host.Log("校验关键字：["+name+"]");keyValidated=IsValidDataKey(name);checkedKey=true;}
                catch(Exception exception){host.Error("校验发生异常:"+exception);}
                if(checkedKey&&!keyValidated)
                {const string message="严重告警：数据模块命名不符合规范，会有丢失数据风险！！！！！";host.Error(message);if(host.HasToast)host.Toast(message);}
            }
            if(!host.LoginProcedureFlag8)
            {
                string digest=Digest(text);
                if(string.Equals(LastDigest,digest)){host.Log("["+key()+"]数据没有发生变化，不存储");return;}
                LastDigest=digest;
            }
            if(CompressData)text=host.Compress(key(),text);
            bool forced=host.LoginProcedureFlag8&&host.LoginStaticFlag4;
            if(SaveLocal)
            {
                if(host.SourceLoginProgress>=10)storage.SetString(host.MineGameName+key(),text);
                if((host.IsUseServer||forced)&&AutoSyn)versions.AddGameVersion(key());
            }
            if((host.IsUseServer||forced)&&AutoSyn)host.QueueUpload(key(),text);
        }
        string Digest(string text)
        {
            try
            {
                using(var md5=MD5.Create())
                {var bytes=md5.ComputeHash(Encoding.UTF8.GetBytes(text));var result=new StringBuilder(32);foreach(byte b in bytes)result.Append(Convert.ToString(b,16).PadLeft(2,'0'));return result.ToString();}
            }
            catch(Exception exception){host.Error(string.Format("MD5计算发生异常[{0}] [{1}]",text,exception));return "";}
        }
        // Original identifier check f13674; limit is UTF-16 characters, not bytes.
        public static bool IsValidDataKey(string name)
        {
            if(string.IsNullOrEmpty(name)||name.Length>511)return false;
            int index=0;if(name[0]=='@'){index=1;if(name.Length==1)return false;}
            char c=name[index];if(!char.IsLetter(c)&&c!='_')return false;
            for(index++;index<name.Length;index++)if(!char.IsLetterOrDigit(name[index])&&name[index]!='_')return false;
            return true;
        }
    }
}
