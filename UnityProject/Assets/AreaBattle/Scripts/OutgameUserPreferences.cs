using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
namespace AreaBattle
{
    [DataContract] public sealed class OutgameUserVariable
    {
        [DataMember(Order=0)] public string Key;
        [DataMember(Order=1)] public string StrVar;
        [DataMember(Order=2)] public int IntVar;
        [DataMember(Order=3)] public float FloatVar;
    }
    [DataContract] sealed class OutgameUserPreferencesRecord
    {
        [DataMember(Order=0)] public List<OutgameUserVariable> UserDataList;
    }
    // Managed UserDataPrefs branch. Platform-owned initialization is explicitly deferred.
    // The supplied file functions implement original FileManager UserData.txt routing.
    public sealed class OutgameUserPreferences
    {
        readonly Func<string,string> read;
        readonly Action<string,string> write;
        readonly Action<string> log,warning,reportSave;
        readonly Action<int,string> reportError;
        readonly Func<bool> saveDisabled;
        Dictionary<string,OutgameUserVariable> values;
        bool initialized;
        string lastDigest="";
        public OutgameUserPreferences(Func<string,string> read,Action<string,string> write,Func<bool> saveDisabled,Action<string> log,Action<string> warning,Action<int,string> reportError,Action<string> reportSave)
        {
            this.read=read??throw new ArgumentNullException(nameof(read));this.write=write??throw new ArgumentNullException(nameof(write));
            this.saveDisabled=saveDisabled??throw new ArgumentNullException(nameof(saveDisabled));this.log=log??throw new ArgumentNullException(nameof(log));
            this.warning=warning??throw new ArgumentNullException(nameof(warning));this.reportError=reportError??throw new ArgumentNullException(nameof(reportError));this.reportSave=reportSave??throw new ArgumentNullException(nameof(reportSave));
        }
        public void OnInit(bool platformOwnsInitialization)
        {
            if(platformOwnsInitialization){log("UNITY_WEBGL下 UserDataPrefs 初始化由 otherPlatformSdk 控制");return;}
            if(initialized){warning("UserDataPrefs已经初始化");return;}
            initialized=true;values=new Dictionary<string,OutgameUserVariable>();
            string text=read("UserData.txt");
            var record=string.IsNullOrEmpty(text)?null:Decode(text);
            if(record==null){reportError(1001,"数据解析失败");return;}
            if(record.UserDataList!=null)foreach(var value in record.UserDataList)
                if(value!=null&&!string.IsNullOrEmpty(value.Key))values[value.Key]=value;
        }
        OutgameUserVariable Find(string key)
        {return values!=null&&values.TryGetValue(key,out var value)?value:null;}
        public int GetInt(string key,int fallback=0){var value=Find(key);return value==null?fallback:value.IntVar;}
        public float GetFloat(string key,float fallback=0){var value=Find(key);return value==null?fallback:value.FloatVar;}
        public string GetString(string key,string fallback=""){return Find(key)?.StrVar??fallback;}
        OutgameUserVariable ForSet(string key)
        {
            if(values==null||string.IsNullOrEmpty(key))return null;
            if(!values.TryGetValue(key,out var value)||value==null){value=new OutgameUserVariable{Key=key};values[key]=value;}return value;
        }
        public void SetInt(string key,int value){var entry=ForSet(key);if(entry!=null)entry.IntVar=value;}
        public void SetFloat(string key,float value){var entry=ForSet(key);if(entry!=null)entry.FloatVar=value;}
        public void SetString(string key,string value){if(value==null)return;var entry=ForSet(key);if(entry!=null)entry.StrVar=value;}
        public void OnSave()
        {
            if(saveDisabled()){warning("UserDataPrefs数据存储功能被禁用了");return;}
            if(values==null)return;
            string text=Encode(new OutgameUserPreferencesRecord{UserDataList=new List<OutgameUserVariable>(values.Values)});
            string digest;using(var hash=MD5.Create())digest=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant();
            if(lastDigest==digest){log("UserData数据没有发生变化，不存储");return;}
            lastDigest=digest;write("UserData.txt",text);
            string user="空";if(values.ContainsKey("GF_LoginUserID")){user=values["GF_LoginUserID"].StrVar;if(string.IsNullOrEmpty(user))user="userId空";}reportSave(user);
        }
        static OutgameUserPreferencesRecord Decode(string text)
        {using(var stream=new MemoryStream(Encoding.UTF8.GetBytes(text)))return (OutgameUserPreferencesRecord)new DataContractJsonSerializer(typeof(OutgameUserPreferencesRecord)).ReadObject(stream);}
        static string Encode(OutgameUserPreferencesRecord record)
        {using(var stream=new MemoryStream()){new DataContractJsonSerializer(typeof(OutgameUserPreferencesRecord)).WriteObject(stream,record);return Encoding.UTF8.GetString(stream.ToArray());}}
    }
    public sealed class OutgameNewDay
    {
        readonly OutgameUserPreferences preferences;
        readonly Func<int> getDaysByFirstLaunch;
        readonly Action<string,object[]> send;
        public OutgameNewDay(OutgameUserPreferences preferences,Func<int> getDaysByFirstLaunch,Action<string,object[]> send)
        {this.preferences=preferences??throw new ArgumentNullException(nameof(preferences));this.getDaysByFirstLaunch=getDaysByFirstLaunch??throw new ArgumentNullException(nameof(getDaysByFirstLaunch));this.send=send??throw new ArgumentNullException(nameof(send));}
        // GlobalData.CheckNewDay f6029 rereads both values inside the changed branch.
        public void Check()
        {
            if(preferences.GetInt("Day",0)==getDaysByFirstLaunch())return;
            int previous=preferences.GetInt("Day",0);
            preferences.SetInt("Day",getDaysByFirstLaunch());
            send("GameEnterNewDay",new object[]{preferences.GetInt("Day",0),previous});
        }
    }
}
