using System;
namespace AreaBattle
{
    // EffectControl coroutine31241 initialization and per-icon arrival31238.
    // Inventory was already changed by NewFlyTool; collection changes only the displayed value.
    public sealed class OutgameFlyToolCollection
    {
        readonly OutgameFlyToolRequest request;readonly Action<int> setDisplayedValue;readonly Action playTargetTween;
        public int Count {get;private set;}
        public int CreateCount {get;}
        public int DisplayedValue {get;private set;}
        public int EndValue {get;}
        public int Step {get;}
        public string AssetPath {get;}
        public static int SourceCreateCount(int amount)
        {
            int remaining=unchecked(amount-100);
            int value=remaining>0?6+(int)((uint)remaining/50):5;
            return value<10?value:10;
        }
        public OutgameFlyToolCollection(OutgameFlyToolRequest request,string displayedValue,Func<int,int> currentInventory,
            Action<int> setDisplayedValue,Action playTargetTween)
        {
            this.request=request;this.setDisplayedValue=setDisplayedValue;this.playTargetTween=playTargetTween;
            int start=int.Parse(displayedValue);
            if((request.ItemId==1001||request.ItemId==1002)&&start==currentInventory(request.ItemId))start=unchecked(start-request.Amount);
            DisplayedValue=start;EndValue=unchecked(start+request.Amount);CreateCount=SourceCreateCount(request.Amount);Step=request.Amount/CreateCount;
            switch(request.ItemId){case 1001:AssetPath="model/entity/golditem";break;case 1002:AssetPath="model/entity/diamonditem";break;case 1004:AssetPath="model/entity/strengthitem";break;}
        }
        public void Arrived(Action returnToPool)
        {
            returnToPool();Count=unchecked(Count+1);playTargetTween();
            if(request.UpdateDisplayedValue)
            {
                DisplayedValue=Count==CreateCount?EndValue:unchecked(DisplayedValue+Step);
                setDisplayedValue(DisplayedValue);
            }
            if(Count==CreateCount)request.Completion?.Invoke();
        }
    }
}
