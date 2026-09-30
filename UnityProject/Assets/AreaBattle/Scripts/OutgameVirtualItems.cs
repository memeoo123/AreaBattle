using System;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameCommanderItemRefresh {void RefreshCommanderItem(int id);}
    // UserInfoControl.UnlockHeadBox32198 + manager32228/32229 only.
    // The owning UserInfoManager must supply its actual record's head-box list.
    public sealed class OutgameHeadBoxInventory
    {
        readonly Func<List<int>> headBoxes;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameHeadBoxInventory(Func<List<int>> headBoxes,Func<OutgameMessageDispatcher> messages){this.headBoxes=headBoxes;this.messages=messages;}
        public bool HaveHeadBox(int id)=>headBoxes().Contains(id);
        public void AddHeadBox(int id){headBoxes().Add(id);messages().SendMessage("UserInfo_HeadBoxUnlock",new object[]{id});}
        public void UnlockHeadBox(int id){if(!HaveHeadBox(id))AddHeadBox(id);}
    }
    // DiceGameControl.AddPoint31102 and DiceGameDataManager31135/31144.
    // ChangePoint does not add the requested quantity: shared item11001 already changed.
    public sealed class OutgameDicePoints
    {
        readonly Func<OutgameGlobalItemLifecycle> global;readonly Func<OutgameMessageDispatcher> messages;
        public OutgameDicePoints(Func<OutgameGlobalItemLifecycle> global,Func<OutgameMessageDispatcher> messages){this.global=global;this.messages=messages;}
        public int GetPoint()=>unchecked((int)global().GetItemCount(11001));
        public int ChangePoint(int ignored){int count=GetPoint();int result=count>0?count:0;messages().SendMessage("Mxtz_DataChange",new object[]{1,result});return result;}
        public void AddPoint(int ignored){ChangePoint(0);}
    }
    public sealed class OutgameVirtualItemServices
    {
        public Func<OutgameCommanderManager> Commander;
        public Func<IOutgameCommanderItemRefresh> CommanderUI;
        public Func<OutgameHeadBoxInventory> HeadBoxes;
        public Func<OutgameDicePoints> DicePoints;
    }
    public class OutgameCommanderItem:OutgameVirtualItemBase
    {
        public OutgameCommanderItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count)=>Use(count);
        public override void AddItemOnlyModel(long count)=>Use(count);
        public override void Use(long count){services.Virtual.Commander().UnlockCommander(ItemConfig.paramInt);services.Virtual.CommanderUI().RefreshCommanderItem(ItemConfig.paramInt);}
    }
    public class OutgameHeadBoxItem:OutgameVirtualItemBase
    {
        public OutgameHeadBoxItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count)=>Use(count);
        public override void AddItemOnlyModel(long count)=>Use(count);
        public override void Use(long count)=>services.Virtual.HeadBoxes().UnlockHeadBox(ItemConfig.paramInt);
    }
    public sealed class OutgameHeroPieceItem:OutgameVirtualItemBase
    {
        public OutgameHeroPieceItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count)=>base.AddItem(count);
        public override void AddItemOnlyModel(long count)=>base.AddItemOnlyModel(count);
        public override void Use(long count)=>base.Use(count);
    }
    public sealed class OutgameStrengthItem:OutgameVirtualItemBase
    {
        public OutgameStrengthItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count){base.AddItem(count);services.Messages().SendMessage("SP_CHANGE");}
        public override void Use(long count){base.Use(count);services.Messages().SendMessage("SP_CHANGE");}
    }
    public sealed class OutgameGamePointsItem:OutgameVirtualItemBase
    {
        public OutgameGamePointsItem(int id,OutgameItemEntityServices services):base(id,services){}
        public override void AddItem(long count){base.AddItem(count);services.Virtual.DicePoints().AddPoint(unchecked((int)count));}
        public override void AddItemOnlyModel(long count){base.AddItemOnlyModel(count);services.Virtual.DicePoints().AddPoint(unchecked((int)count));}
    }
}
