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
