using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // UIModule.CloseForName27401, field48 Dictionary<string,BaseUI>.
    // T is the recovered page owner; close starts its existing async state machine.
    public sealed class OutgameUiCloseRegistry<T>
    {
        readonly IDictionary<string,T> pages;readonly Action<T> close;
        public OutgameUiCloseRegistry(IDictionary<string,T> pages,Action<T> close)
        {this.pages=pages;this.close=close;}
        public void CloseForName(string name)
        {
            if(!pages.ContainsKey(name))return;
            T page=pages[name];pages.Remove(name);close(page);
        }
        // BaseUI.CloseSelf27326. Resolve the module and original runtime type name
        // after CloseAction, since it may change registry ownership. Source names
        // must come from recovered page metadata, not the C# adapter's type name.
        public static void CloseSelf(Action closeAction,Func<OutgameUiCloseRegistry<T>> module,Func<string> sourceTypeName)
        {
            closeAction?.Invoke();var owner=module();owner.CloseForName(sourceTypeName());
        }
    }
}
