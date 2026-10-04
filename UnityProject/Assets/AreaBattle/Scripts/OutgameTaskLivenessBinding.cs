using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameTaskLivenessServices
    {
        public Func<OutgameDailyTaskView> Daily;
        public Func<OutgameLegacyConfigManager> Config;
        public Func<int,int> GoodsType;
        public Func<int,bool,string> FormatNumber;
        public Func<int,Transform,int> ShowEffect;
        public Action<int> CloseEffect;
        public Func<IOutgameShopCurrencyEffects> Effects;
        public Action<int,int> ShowAward;
        public Action RefreshTopInfo,SetDailyTabReddot;
        public Func<WaitForEndOfFrame,Task> EndOfFrame=WaitFrame;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        static async Task WaitFrame(WaitForEndOfFrame instruction){await OutgameUnityAwait.Await(instruction);}
    }
    // TaskPanelUI4416 methods33877/33881/33882/33883 plus closures4414 and async4415.
    // Full page owns this binding, preview, tab selection and effect cleanup.
    public sealed class OutgameTaskLivenessBinding
    {
        readonly OutgameTaskLivenessServices services;
        public readonly Transform ProgressBackground;
        public readonly OutgameLivenessPreviewItem Preview;
        public int Layer=2,EffectHandle;
        public Task LastClaim {get;private set;}
        public OutgameTaskLivenessBinding(Transform progressBackground,OutgameLivenessPreviewItem preview,OutgameTaskLivenessServices services)
        {ProgressBackground=progressBackground;Preview=preview;this.services=services;}
        public void Refresh(bool initial)
        {
            var items=services.Daily().LivenessItems;var layout=ProgressBackground.GetChild(1);int value=services.Daily().Liveness;
            for(int i=0;i<layout.childCount;i++)
            {
                var node=layout.GetChild(i);var item=items[i];var button=node.GetComponent<Button>();var text=node.GetComponentInChildren<Text>();
                text.text=services.FormatNumber(item.livenessValue,false);var box=node.GetChild(0);int state=item.state;
                box.GetChild(0).gameObject.SetActive(state==0);box.GetChild(1).gameObject.SetActive(state!=0);
                if(item.livenessValue<=value)
                {
                    if(item.state==0)
                    {
                        button.onClick.RemoveAllListeners();OutgameUiClick.Add(button,()=>BeginClaim(node,item),services.Messages);
                        var glow=node.GetChild(0).GetChild(2);glow.gameObject.SetActive(true);glow.GetComponent<Canvas>().sortingLayerName=OutgameUiLayerNames.Get(Layer);
                    }
                }
                else if(initial)OutgameUiClick.Add(button,()=>ShowPreview(node,item),services.Messages);
            }
        }
        public void RefreshPointDailyLivenessBar()
        {
            services.SetDailyTabReddot();var items=services.Daily().LivenessItems;int value=services.Daily().Liveness;
            if(items.Min(item=>item.livenessValue)<=value)Refresh(false);
        }
        public void ShowPreview(Transform node,OutgameTaskLivenessItemData item)
        {
            Preview.Lifetime.GameObject.SetActive(true);Preview.SetVisible(true);Preview.Lifetime.Transform.position=node.position;
            var reward=item.Config.rewards[0].datas;var config=services.Config().dicGameItem[reward[0]];
            Preview.SetData(config,reward[1]);Preview.Refresh();
        }
        void BeginClaim(Transform node,OutgameTaskLivenessItemData item){LastClaim=ClaimAsync(node,item);Observe(LastClaim);}
        static async void Observe(Task task)=>await task;
        void RefreshTop()=>services.RefreshTopInfo();
        public async Task ClaimAsync(Transform box,OutgameTaskLivenessItemData item)
        {
            int id=item.Rewards[0].itemId;int kind=services.GoodsType(id);int count=unchecked((int)item.Rewards[0].itemCount);
            box.GetChild(0).GetChild(2).gameObject.SetActive(false);EffectHandle=services.ShowEffect(1016,box);
            var animation=box.GetComponentInChildren<Animation>();if(!ReferenceEquals(animation,null))animation.Play();
            await services.EndOfFrame(new WaitForEndOfFrame());
            switch(kind)
            {
                case 1:services.Effects().FlyMoney(count,null,box.position,false,RefreshTop,true);break;
                case 2:services.Effects().FlyDiamonds(count,null,box.position,false,RefreshTop,true);break;
                default:services.ShowAward(id,1);break;
            }
            box.GetComponent<Button>().enabled=false;services.Daily().ClaimLiveness(item.id);services.SetDailyTabReddot();
        }
        // Source33880 closes current handle before toggling tab content. No implicit disposal guard.
        public void CloseCurrentEffect(){if(EffectHandle!=0){services.CloseEffect(EffectHandle);EffectHandle=0;}}
    }
}
