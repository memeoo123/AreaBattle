using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Fixture=AreaBattle.EditorTools.OutgameDynamicListValidation.Fixture;
namespace AreaBattle.EditorTools
{
    public static class OutgameDynamicCenterValidation
    {
        static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
        static Vector2 Position(Fixture f)=>((RectTransform)f.List.transform).anchoredPosition;
        public static BattleBuild.Report Run()
        {
            var r=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Source single/pair positioning, index validation and first coroutine yield. Real frame timing and concurrent coroutine behavior require the separate native suite."};
            Action<string,Action> check=(id,body)=>{try{body();r.checks.Add(new BattleBuild.Check{id=id,result="pass"});}catch(Exception ex){r.passed=false;r.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            check("dynamic-center-single-clamps-stops-velocity-and-dirties",()=>{
                using(var f=new Fixture()){
                    f.Init(40);f.ScrollTo(0);f.List.Scroll.velocity=new Vector2(7,9);f.List.CenteredWithIndex(10);
                    Require(Position(f)==new Vector2(0,112.5f)&&f.List.Scroll.velocity==Vector2.zero&&f.List.IsDirty,"single center and StopMovement");
                    f.List.Centered2Bottom();Require(Position(f).y==450,"bottom clamps content boundary");
                    f.Data.Centered2Top();Require(Position(f).y==0,"provider forwards top to index0");
                    f.List.CenteredWithIndex(999);Require(Position(f).y==450,"invalid high index warns then clamps");
                    f.List.CenteredWithIndex(-99);Require(Position(f).y==0,"negative index warns then clamps");
                    f.Data.Data.Clear();f.Data.UpdateList();f.List.Centered2Bottom();Require(Position(f).y==0,"empty bottom clamps via index0 without invented early return");
                }
            });
            check("dynamic-center-single-inverse-horizontal-and-inclusive-spacing",()=>{
                using(var f=new Fixture()){
                    f.List.Inverse=true;f.List.SetSpaceList(new[]{2f,3f});f.Init(40);f.List.CenteredWithIndex(10);
                    Require(Position(f).y==-167.5f,"inverse adds half viewport then negates with inclusive space");
                    f.Data.Data.RemoveRange(1,39);f.Data.UpdateList();f.List.CenteredWithIndex(0);Require(Position(f).y==-30,"inverted bounds preserve original clamp branch on short content");
                }
                using(var f=new Fixture(100,100,50,25)){
                    f.List.Direction=1;f.Init(40);f.List.CenteredWithIndex(20);Require(Position(f).x==-225,"single horizontal center negates target");
                    f.List.Centered2Bottom();Require(Position(f).x==-400,"horizontal endpoint clamps");
                }
            });
            check("dynamic-center-two-index-retains-distinct-source-edges",()=>{
                using(var f=new Fixture()){
                    f.Init(40);f.ScrollTo(0);f.List.CenteredWithTwoIndex(10,14,0);Require(Position(f).y==137.5f&&!f.List.IsDirty,"pair averages row centers without explicit dirty flag");
                    f.List.CenteredWithTwoIndex(0,0,0);Require(Position(f).y==-12.5f,"pair does not clamp negative average");
                    f.List.CenteredWithTwoIndex(0,2,0);Require(Position(f).y==0,"pair crossing half visible line count snaps top");
                    f.List.SetSpaceList(new[]{100f});f.List.Inverse=true;f.List.CenteredWithTwoIndex(10,14,0);Require(Position(f).y==137.5f,"pair intentionally ignores spacing/inverse");
                    var prior=Position(f);try{f.List.CenteredWithTwoIndex(-1,2,0);throw new Exception("missing index error");}catch(Exception ex){Require(ex.Message=="Locate Index Error -1"&&Position(f)==prior,"pair invalid first throws before mutation");}
                    try{f.List.CenteredWithTwoIndex(0,99,0);throw new Exception("missing index error");}catch(Exception ex){Require(ex.Message=="Locate Index Error 99"&&Position(f)==prior,"pair invalid second throws before mutation");}
                }
                using(var f=new Fixture(100,100,50,25)){
                    f.List.Direction=1;f.Init(40);f.List.CenteredWithTwoIndex(20,24,0);Require(Position(f).x==250,"pair horizontal uses positive target, unlike single");
                    f.List.CenteredWithTwoIndex(39,39,0);Require(Position(f).x==425,"strict last-half comparison does not clamp equal boundary");
                }
            });
            check("dynamic-center-coroutine-first-yield-does-not-move",()=>{
                using(var f=new Fixture()){
                    f.Init(40);f.ScrollTo(0);
                    var routine=(IEnumerator)typeof(OutgameDynamicList).GetMethod("MovePosition",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(f.List,new object[]{Vector2.zero,new Vector2(0,100),1f});
                    Require(routine.MoveNext()&&routine.Current is WaitForEndOfFrame&&Position(f)==Vector2.zero&&!f.List.IsDirty,"source iterator delays first delta and write until first frame end");
                    ((IDisposable)routine).Dispose();
                }
            });
            return r;
        }
    }
}
