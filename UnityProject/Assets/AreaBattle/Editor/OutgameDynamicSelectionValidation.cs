using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgameDynamicSelectionValidation
    {
        public sealed class Row:IOutgameSelectData
        {public int Id,Hash;public bool Selected{get;set;}public override int GetHashCode()=>Hash;}
        public sealed class Item:IOutgameDynamicItem,IOutgameDynamicRenderSelect
        {
            public readonly List<string> Trace;public OutgameDynamicListProviderSelection<Row> Provider;public int Index;
            public Action<Item> Select,DeSelect,Render;
            public Item(List<string> trace)=>Trace=trace;
            public void OnCreate(IOutgameDynamicListProvider data)=>Provider=(OutgameDynamicListProviderSelection<Row>)data;
            public void InstantiateNoNewItem(GameObject root){}
            public void OnRenderer(int index){Index=index;Trace.Add("render:"+index);Render?.Invoke(this);}
            public void OnSelect(){Trace.Add("select:"+Index);Select?.Invoke(this);}
            public void OnDeSelect(){Trace.Add("deselect:"+Index);DeSelect?.Invoke(this);}
            public void Dispose(){}
        }
        public sealed class Fixture:IDisposable
        {
            public readonly OutgameDynamicListValidation.Fixture Native=new OutgameDynamicListValidation.Fixture();
            public readonly OutgameDynamicListProviderSelection<Row> Data=new OutgameDynamicListProviderSelection<Row>();
            public readonly List<string> Trace=new List<string>();
            public Fixture(int count=40)
            {
                Native.List.InitRendererList(Data,()=>new Item(Trace));for(int i=0;i<count;i++)Data.Data.Add(new Row{Id=i,Hash=i});Data.UpdateList();Native.ScrollTo(0);Trace.Clear();
            }
            public Item At(int i)=>(Item)Native.List.GetItem(i).BaseItem;
            public void Dispose()=>Native.Dispose();
        }
        static void Require(bool value,string why){if(!value)throw new Exception(why);}
        static void Throws<T>(Action body)where T:Exception{try{body();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);}
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Original shared selection provider/renderer through actual DynamicList, including source hash comparison, callback ordering and recycling. Native original day-list checks are separate; full page/Main/business acceptance pending."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id="dynamic-selection-"+id,result="pass"});}catch(Exception e){r.passed=false;r.checks.Add(new BattleBuild.Check{id="dynamic-selection-"+id,result="fail",detail=e.ToString()});}};
            check("visible-offscreen-and-render-selected-after-scroll",()=>{
                using(var f=new Fixture()){
                    f.Data.SetSelect(1);Require(f.Data.Data[1].Selected&&string.Join(",",f.Trace)=="select:1","visible selection notifies immediately");
                    f.Trace.Clear();f.Data.SetSelect(1);Require(f.Trace.Count==0,"same selected row does not notify twice");
                    f.Data.SetSelect(10);Require(!f.Data.Data[1].Selected&&f.Data.Data[10].Selected&&string.Join(",",f.Trace)=="deselect:1","offscreen selection changes data before clearing old visible row");
                    f.Trace.Clear();f.Native.ScrollTo(125);Require(f.Native.List.GetItem(10)!=null&&f.Trace.IndexOf("render:10")<f.Trace.IndexOf("select:10"),"reused slot renders before replaying selected event");
                    f.Trace.Clear();f.Data.UpdateItemData(10);Require(string.Join(",",f.Trace)=="render:10,select:10","refresh replays selected callback even though data already true");
                }
            });
            check("source-hash-collision-retains-multiple-selected-rows",()=>{
                using(var f=new Fixture()){
                    f.Data.Data[0].Hash=77;f.Data.Data[1].Hash=77;f.Data.SetSelect(0);f.Trace.Clear();f.Data.SetSelect(1);
                    Require(f.Data.Data[0].Selected&&f.Data.Data[1].Selected&&string.Join(",",f.Trace)=="select:1","GetHashCode equality, not reference identity, suppresses deselection");
                    f.Trace.Clear();f.Data.KillSelect();Require(!f.Data.Data[0].Selected&&!f.Data.Data[1].Selected&&string.Join(",",f.Trace)=="deselect:0,deselect:1","kill iterates every selected row regardless of equal hash");
                }
            });
            check("callback-mutation-captures-post-select-count-and-live-data",()=>{
                using(var f=new Fixture()){
                    var original=f.Data.Data[1];f.At(1).Select=item=>f.Data.Data.Add(new Row{Id=40,Hash=40,Selected=true});
                    f.Data.SetSelect(1);Require(original.Selected&&!f.Data.Data[40].Selected,"count captured after select includes newly appended offscreen row");
                    f.Data.Data[2].Selected=true;f.At(2).DeSelect=item=>f.Data.Data[3].Selected=true;f.Trace.Clear();f.Data.SetSelect(1);
                    Require(!f.Data.Data[2].Selected&&!f.Data.Data[3].Selected&&string.Join(",",f.Trace)=="deselect:2,deselect:3","later row reread sees state changed by earlier callback");
                }
            });
            check("selection-and-deselection-exceptions-keep-written-flags",()=>{
                using(var f=new Fixture()){
                    f.Data.SetSelect(0);f.At(1).Select=item=>throw new InvalidOperationException("select callback");Throws<InvalidOperationException>(()=>f.Data.SetSelect(1));
                    Require(f.Data.Data[0].Selected&&f.Data.Data[1].Selected,"new flag written before callback; previous row untouched on failure");
                    f.At(0).DeSelect=item=>throw new InvalidOperationException("deselect callback");Throws<InvalidOperationException>(()=>f.Data.KillSelect());
                    Require(!f.Data.Data[0].Selected&&f.Data.Data[1].Selected,"kill writes false before callback and stops at first failure");
                }
            });
            check("negative-empty-and-invalid-positive-index-contract",()=>{
                var unbound=new OutgameDynamicListProviderSelection<Row>();unbound.SetSelect(-1,true);unbound.KillSelect();
                using(var f=new Fixture()){
                    f.Data.SetSelect(1);f.Data.SetSelect(-9,true);Require(f.Data.Data[1].Selected,"negative index is no-op, not clear");
                    Throws<ArgumentOutOfRangeException>(()=>f.Data.SetSelect(40));Require(f.Data.Data[1].Selected,"invalid positive index fails before selection mutation");
                    f.Data.Data[2]=null;Throws<NullReferenceException>(()=>f.Data.SetSelect(3));Require(f.Data.Data[3].Selected&&!f.Data.Data[1].Selected,"null row fails partway through deselection scan");
                }
            });
            check("renderer-rereads-region-after-render-and-missing-selection-interface",()=>{
                using(var f=new Fixture()){
                    var renderer=(OutgameDynamicSelectionRenderer<Row>)f.Native.List.GetItem(0);f.Data.Data[0].Selected=true;
                    f.At(0).Render=item=>renderer.Region=null;renderer.Refresh();Require(string.Join(",",f.Trace)=="render:0","cleared region after render prevents selected callback");
                    var missing=new OutgameDynamicSelectionRenderer<Row>{SelectionProvider=f.Data};var row=new Row();
                    Throws<NullReferenceException>(()=>missing.Select(row));Require(row.Selected,"missing selection receiver fails only after setting flag");
                }
            });
            return r;
        }
    }
}
