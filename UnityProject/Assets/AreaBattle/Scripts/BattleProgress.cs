using System;

namespace AreaBattle
{
    // Proj_xqzdOverUI.OpenLater advances on opening a normal victory result, once.
    public sealed class BattleProgress
    {
        public int SelectedLevel { get; private set; }
        public int SavedLevel { get; private set; }
        public bool Special { get; private set; }
        bool resultOpened;
        public BattleProgress(int level,bool special=false)
        {SelectedLevel=SavedLevel=level;Special=special;}
        public bool OpenResult(BattlePhase phase)
        {
            if(resultOpened || phase!=BattlePhase.Victory)return false;
            resultOpened=true;
            if(Special)return false;
            SavedLevel=checked(SelectedLevel+1);return true;
        }
        public void Retry(){resultOpened=false;}
    }
}
