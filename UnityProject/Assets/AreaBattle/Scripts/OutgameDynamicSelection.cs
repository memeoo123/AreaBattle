namespace AreaBattle
{
    // Source interfaces4560 and4559.
    public interface IOutgameSelectData {bool Selected{get;set;}}
    public interface IOutgameDynamicRenderSelect {void OnSelect();void OnDeSelect();}
    // Source4554. Selection lives on data, not on a separate selected-index field.
    public sealed class OutgameDynamicListProviderSelection<T>:OutgameDynamicListProvider<T> where T:IOutgameSelectData
    {
        public override OutgameDynamicRenderItem CreateRenderer()=>new OutgameDynamicSelectionRenderer<T>{Provider=this,SelectionProvider=this};
        public void SetSelect(int index,bool center=false,float duration=0)
        {
            if(index<0)return;
            T selected=GetData(index);
            var renderer=DynamicList.GetItem(index);
            if(renderer!=null)((OutgameDynamicSelectionRenderer<T>)renderer).Select(selected);
            else selected.Selected=true;
            int count=GetListCount();
            for(int i=0;i<count;i++)
            {
                T data=GetData(i);
                if(data.Selected&&selected.GetHashCode()!=data.GetHashCode())
                {
                    var other=DynamicList.GetItem(i);
                    if(other!=null)((OutgameDynamicSelectionRenderer<T>)other).DeSelect(data);
                    else data.Selected=false;
                }
            }
            if(center)DynamicList.CenteredWithIndex(index,duration);
        }
        public void KillSelect()
        {
            int count=GetListCount();
            for(int i=0;i<count;i++)
            {
                T data=GetData(i);
                if(data.Selected)
                {
                    var renderer=DynamicList.GetItem(i);
                    if(renderer!=null)((OutgameDynamicSelectionRenderer<T>)renderer).DeSelect(data);
                    else data.Selected=false;
                }
            }
        }
    }
    // Source4553 is nested in the generic provider; it has its own typed provider reference.
    public sealed class OutgameDynamicSelectionRenderer<T>:OutgameDynamicRenderItem where T:IOutgameSelectData
    {
        IOutgameDynamicRenderSelect selection;
        public OutgameDynamicListProviderSelection<T> SelectionProvider;
        protected override void OnBaseItemChanged()=>selection=BaseItem as IOutgameDynamicRenderSelect;
        public void OnSelect()=>selection.OnSelect();
        public void OnDeSelect()=>selection.OnDeSelect();
        public void Select(T data){if(!data.Selected){data.Selected=true;OnSelect();}}
        public void DeSelect(T data){if(data.Selected){data.Selected=false;OnDeSelect();}}
        public override void Refresh()
        {
            base.Refresh();
            if(Region!=null&&SelectionProvider.GetData(Region.Index).Selected)OnSelect();
        }
    }
}
