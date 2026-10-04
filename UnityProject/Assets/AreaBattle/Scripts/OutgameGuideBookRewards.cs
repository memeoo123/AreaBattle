using System;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    public interface IOutgameGuideBookRewardPage
    {
        GuidebookConfig SelectedBook {get;}
        int SelectedIndex {get;}
        Transform Root {get;}
        Transform BookRewardButton {get;}
        void PlayVoice(int id);
        void RefreshRedDots();
        void RefreshBookItem(int index);
        void SetBookRewardVisible(bool visible);
    }
    // GuideBookUI33032/33049. Inventory mutation precedes effects and claim save.
    public sealed class OutgameGuideBookRewards
    {
        readonly IOutgameGuideBookRewardPage page;
        readonly Func<OutgameGuideBookControl> control;readonly Func<OutgameToolControl> tools;
        readonly Func<IOutgameShopCurrencyEffects> effects;
        public OutgameGuideBookRewards(IOutgameGuideBookRewardPage page,Func<OutgameGuideBookControl> control,
            Func<OutgameToolControl> tools,Func<IOutgameShopCurrencyEffects> effects)
        {this.page=page;this.control=control;this.tools=tools;this.effects=effects;}
        public void ClaimBook()
        {
            page.PlayVoice(2001);
            if(page.SelectedBook==null||control().IsGetBookReward(page.SelectedBook.id))return;
            int id=page.SelectedBook.reward[0],amount=page.SelectedBook.reward[1];
            tools().ToolChange(id,amount,false,"guidebook",true); // Source ignores the returned bool.
            int particles=Math.Max(Math.Min(amount,100),10);
            if(id==1001)effects().FlyMoney(particles,page.Root,page.BookRewardButton.position,false,null,true);
            else if(id==1002)effects().FlyDiamonds(particles,page.Root,page.BookRewardButton.position,false,null,true);
            control().GetBookRrward(page.SelectedBook.id); // Re-read selection after external callbacks.
            page.RefreshRedDots();page.RefreshBookItem(page.SelectedIndex);page.SetBookRewardVisible(false);
        }
        public void ClaimTip(GuideTipsConfig tip,Vector3 position)
        {
            if(tip==null||control().IsGetTipReward(tip.id))return;
            int id=tip.reward[0],amount=tip.reward[1];
            tools().ToolChange(id,amount,false,"guidetip",true);
            // Source performs four parent accesses even though it discards the result.
            var unused=page.Root.parent.parent.parent.parent;
            int particles=Math.Max(Math.Min(amount,100),10);
            if(id==1001)effects().FlyMoney(particles,page.Root,position,false,null,true);
            else if(id==1002)effects().FlyDiamonds(particles,page.Root,position,false,null,true);
            control().GetTipReward(tip.id);page.RefreshRedDots();
        }
    }
}
