using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle
{
    // Explicit shared BigNumExtension state written by SetBigNumSymbol (wasm8189).
    public sealed class OutgameBigNumberSymbols
    {
        public List<string> Symbols {get;private set;}
        public Dictionary<int,string> ByMagnitude {get;private set;}
        public int FirstMagnitude {get;private set;}
        public void Set(Dictionary<int,string> symbols)
        {
            Symbols=symbols.Values.ToList();ByMagnitude=symbols;FirstMagnitude=ByMagnitude.First().Key;
        }
        // BigNumExtension.BigToString27612. Keep source truncation, symbol saturation,
        // current-culture raw digits and the final-fraction-digit zero rule.
        public string Format(System.Numerics.BigInteger value,Action<object[]> warning)
        {
            if(Symbols==null){warning(new object[]{"大数据表未配置"});return string.Empty;}
            string raw=value.ToString(),suffix="";
            if(value==System.Numerics.BigInteger.Zero)return "0";
            string text;
            if(raw.Length<=FirstMagnitude+2){
                text=raw.PadLeft(2,'0');text=text.Insert(text.Length-2,".");
                text=raw.Length>=4?text.Substring(0,4):text.PadLeft(4,'0');
            }else{
                string integer=(value/100).ToString();int group=integer.Length/FirstMagnitude;
                if(integer.Length%FirstMagnitude==0)group--;
                int index=Symbols.Count<=group-1?Symbols.Count-1:group-1;
                suffix=Symbols[index];text=integer.Insert(integer.Length-group*FirstMagnitude,".").Substring(0,4);
            }
            int fraction=text.IndexOf(".")+1;
            if(value.ToString().Length<=5){
                bool lastZero=false;for(int i=fraction;i<text.Length;i++)lastZero=text[i]=='0';
                if(lastZero)text=text.Substring(0,fraction-1);
            }
            if(text.EndsWith("."))text=text.Substring(0,text.Length-1);
            return text+suffix;
        }
        // ConfigMgr.GetLargNum30432 allocates a fresh map and uses Add, preserving enumeration order.
        public static Dictionary<int,string> FromConfig(Dictionary<object,LargeNumConfig> configs)
        {
            var result=new Dictionary<int,string>();foreach(var pair in configs)result.Add(pair.Value.mag,pair.Value.magName);return result;
        }
    }
    public sealed class OutgamePreGameSettings
    {
        readonly Action<int> setReportMode,setFrameRate;readonly Action<bool> setMultiTouch;
        readonly Func<Dictionary<int,string>> getLargeNumbers;readonly OutgameBigNumberSymbols symbols;
        public OutgamePreGameSettings(Action<int> setReportMode,Func<Dictionary<int,string>> getLargeNumbers,
            OutgameBigNumberSymbols symbols,Action<bool> setMultiTouch=null,Action<int> setFrameRate=null)
        {
            this.setReportMode=setReportMode;this.getLargeNumbers=getLargeNumbers;this.symbols=symbols;
            this.setMultiTouch=setMultiTouch??(value=>Input.multiTouchEnabled=value);
            this.setFrameRate=setFrameRate??(value=>Application.targetFrameRate=value);
        }
        // MineGameMain30628; fail at the original operation, with no rollback/guard.
        public void Initialize(){setReportMode(2);setMultiTouch(false);symbols.Set(getLargeNumbers());setFrameRate(60);}
    }
}
