using System;
namespace AreaBattle
{
    // Exact recovered XYXCommon_DBT_ISDK + Bridge_WX_XYXFunction preference branch.
    // SDK slot203 inherits ISDK.GetDaysByFirstLaunch => 0.
    // SDK slot302 -> Bridge slot54 inherits IXYXFunction.CanReadLocalData => true.
    // This composition restores those classes, not account/login success or other platforms.
    public sealed class OutgameWxPreferenceSession
    {
        public OutgameUserPreferences Preferences {get;}
        readonly OutgameNewDay newDay;
        public OutgameWxPreferenceSession(OutgameWebGlFileStorage files,Func<bool> saveDisabled,Action<string> log,Action<string> warning,Action<int,string> reportError,Action<string> reportSave,Action<string,object[]> send)
        {
            if(files==null)throw new ArgumentNullException(nameof(files));
            Preferences=files.CreatePreferences(saveDisabled,log,warning,reportError,reportSave);
            newDay=new OutgameNewDay(Preferences,()=>0,send);
        }
        public void Initialize(){Preferences.OnInit(false);}
        public void CheckNewDay(){newDay.Check();}
        public void Save(){Preferences.OnSave();}
    }
}
