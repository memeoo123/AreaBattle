using System;
using UnityEngine.UI;
namespace AreaBattle
{
    // Source activity controller4504 RefreshRemainingTime34377. Server timestamp unit is milliseconds.
    public sealed class OutgameActivityCountdown
    {
        readonly Func<int> status;readonly Func<long> now;
        public long StartTimeStamp,EndTimeStamp;
        public int DayTime {get;private set;}public int HourTime {get;private set;}
        public int MinTime {get;private set;}public int SecTime {get;private set;}
        public int Status=>status();
        public OutgameActivityCountdown(Func<int> status,Func<long> serverTimestamp)
        {this.status=status??throw new ArgumentNullException(nameof(status));now=serverTimestamp??throw new ArgumentNullException(nameof(serverTimestamp));}
        public void RefreshRemainingTime()
        {
            int seconds=0;
            if(Status==3)seconds=unchecked((int)(unchecked(StartTimeStamp-now())/1000));
            else if(Status==4)seconds=unchecked((int)(unchecked(EndTimeStamp-now())/1000));
            SecTime=seconds%60;DayTime=seconds/86400;
            int hours=seconds/3600;HourTime=hours%24;
            MinTime=unchecked((short)(unchecked((short)(seconds-hours*3600))/60));
        }
    }
    // Proj_xqzdStartUI.RefreshEasterTime: guard first, then refresh and format original language keys.
    public sealed class OutgameMainActivityTimer
    {
        readonly Text text;readonly OutgameActivityCountdown countdown;readonly Func<string,string> language;
        public OutgameMainActivityTimer(Text text,OutgameActivityCountdown countdown,Func<string,string> language)
        {this.text=text;this.countdown=countdown;this.language=language;}
        public void Refresh()
        {
            if(!text||!text.gameObject||countdown.Status<2)return;
            countdown.RefreshRemainingTime();
            if(countdown.DayTime>=1)text.text=string.Format(language("ServerTimeModule.DhTime"),countdown.DayTime,countdown.HourTime);
            else if(countdown.HourTime>=1)text.text=string.Format(language("ServerTimeModule.HmTime"),countdown.HourTime,countdown.MinTime);
            else text.text=string.Format(language("ServerTimeModule.MsTime"),countdown.MinTime,countdown.SecTime);
        }
    }
}
