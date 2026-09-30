using System;
namespace AreaBattle
{
    // FileManager ReadWebGLFile f5084 / SaveWebGLFile f3362 -> ISDK slots304/303
    // -> Bridge_WX_XYXFunction -> WXBase string storage. Keys are passed unchanged.
    public sealed class OutgameWebGlFileStorage
    {
        readonly OutgameSdkStringStorage storage;
        readonly Func<bool> loggingEnabled;
        readonly Action<string> log;
        public OutgameWebGlFileStorage(OutgameSdkStringStorage storage,Func<bool> loggingEnabled,Action<string> log)
        {this.storage=storage??throw new ArgumentNullException(nameof(storage));this.loggingEnabled=loggingEnabled??throw new ArgumentNullException(nameof(loggingEnabled));this.log=log??throw new ArgumentNullException(nameof(log));}
        public string Read(string key)
        {
            string value=storage.GetString(key,"");
            if(loggingEnabled())log("调用小游戏读取数据：key["+key+"] value["+value+"]");
            return value;
        }
        public void Write(string key,string value)
        {
            if(loggingEnabled())log("调用小游戏存储数据：key["+key+"] value["+value+"]");
            storage.SetString(key,value);
        }
        public OutgameUserPreferences CreatePreferences(Func<bool> saveDisabled,Action<string> preferenceLog,Action<string> warning,Action<int,string> reportError,Action<string> reportSave)
        {return new OutgameUserPreferences(Read,Write,saveDisabled,preferenceLog,warning,reportError,reportSave);}
    }
}
