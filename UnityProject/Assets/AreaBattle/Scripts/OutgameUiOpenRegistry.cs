using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Source type resolution is supplied by the recovered class-name catalog, since
    // local adapter CLR names differ from original Assembly-CSharp page names.
    public sealed class OutgameUiType<T> where T:class
    {
        public readonly string Name;public readonly Func<T> Create;
        public OutgameUiType(string name,Func<T> create){Name=name;Create=create;}
    }
    // UIModule.Open27398; dictionary48 is shared with CloseForName27401.
    public sealed class OutgameUiOpenRegistry<T> where T:class
    {
        readonly IDictionary<string,T> pages;readonly Func<string,OutgameUiType<T>> resolve;
        readonly Action<T,object[]> open;readonly Action<string> error;
        public OutgameUiOpenRegistry(IDictionary<string,T> pages,Func<string,OutgameUiType<T>> resolve,Action<T,object[]> open,Action<string> error)
        {this.pages=pages;this.resolve=resolve;this.open=open;this.error=error;}
        public T Open(string nameSpace,string name,object[] arguments)
        {
            var type=resolve(nameSpace+"."+name);string key=type.Name;
            T page=null;
            if(pages.ContainsKey(key)){page=pages[key];if(page!=null)return page;}
            try{page=type.Create();pages.Add(key,page);open(page,arguments);}
            catch(Exception ex){error(ex.Message+ex.StackTrace);}
            return page;
        }
    }
    public static class OutgameUiWindowIndex
    {
        // GetWindowNum27404 returns max field92, not the number of dictionary entries.
        public static int Maximum<T>(IEnumerable<T> pages,Func<T,int> layer,Func<T,GameObject> root,Func<T,int> index)
        {
            int maximum=0;
            foreach(var page in pages)if(layer(page)==1&&root(page)!=null){int value=index(page);if(value>maximum)maximum=value;}
            return maximum;
        }
    }
}
