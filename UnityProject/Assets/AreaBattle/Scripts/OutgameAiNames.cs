using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace AreaBattle
{
    // ConfigHelper.RandAIInfo32592. Country selection uses Unity's RNG; distinct
    // name sampling and list insertion use the original shared managed RNG.
    public sealed class OutgameAiNames
    {
        readonly Func<OutgameLegacyConfigManager> config;
        readonly Func<int,int,int> unityRange;
        readonly GameRandomSource random;
        readonly Action<object[]> log;
        public OutgameAiNames(Func<OutgameLegacyConfigManager> config,Action<object[]> log,
            Func<int,int,int> unityRange=null,GameRandomSource random=null)
        {this.config=config;this.log=log;this.unityRange=unityRange??UnityEngine.Random.Range;this.random=random??GameRandomSource.Shared;}
        public List<string> Generate(int count,bool includeCountry)
        {
            var result=new List<string>();var text=new StringBuilder(10);
            var countries=config().dicCountryConfig;
            var names=config().dicAIName;
            var counts=new Dictionary<int,int>();var keys=countries.Keys.ToList();
            int nameCount=config().dicAIName.Count;
            for(int i=0;i<count;i++){
                while(true){
                    int index=unityRange(0,keys.Count),country=(int)keys[index];
                    if(country==1||country==10)country++;
                    if(counts.ContainsKey(country)){
                        if(counts[country]<nameCount-1){counts[country]++;break;}
                        keys.RemoveAt(index);
                    }else{counts.Add(country,1);break;}
                }
            }
            foreach(var pair in counts){
                int country=pair.Key;
                var selected=random.DistinctRange(pair.Value,1,nameCount,log);
                for(int i=0;i<selected.Length;i++){
                    text.Clear();var name=names[selected[i]];
                    // The original country lookup also executes without a prefix.
                    var ignored=countries[country];
                    string value=name.zh_cn;
                    if(includeCountry){text.Append(countries[country].countryFlagName);text.Append(";");} //signEnum2/32606
                    text.Append(value);
                    if(result.Count<1)result.Add(text.ToString());
                    else result.Insert(random.Inclusive(0,result.Count-1),text.ToString());
                }
            }
            return result;
        }
    }
}
