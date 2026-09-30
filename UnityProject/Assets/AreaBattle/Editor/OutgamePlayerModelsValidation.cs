using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle.EditorTools
{
    public static class OutgamePlayerModelsValidation
    {
        sealed class AnimatorFixture:IOutgameModelAnimator
        {
            public readonly List<string> calls=new List<string>();
            public Action OnComplete {get;set;}
            public void Reset(){calls.Add("reset");}
            public void Play(string name,bool loop){calls.Add(name+":"+loop);}
        }
        static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion,limitations="Isolated source model-load lifecycle; production model assets, root placement and animation adapter remain pending."};
            var objects=new List<GameObject>();
            GameObject Make(string name){var obj=new GameObject(name);objects.Add(obj);return obj;}
            try
            {
                var root=Make("source-root");root.layer=7;
                var entities=new List<Action<GameObject>>();var assets=new List<Action<GameObject>>();var entityIds=new List<int>();var assetIds=new List<int>();int refresh=0;
                var models=new OutgamePlayerModels(@"{""Datas"":[{""id"":100,""skinType"":1,""prefabId"":22},{""id"":101,""skinType"":1,""prefabId"":23},{""id"":300,""skinType"":3,""prefabId"":24}]}",new Dictionary<int,Transform>{{1,root.transform},{3,root.transform}},new Vector3(1,2,3),2,
                    (id,callback)=>{entityIds.Add(id);entities.Add(callback);},(id,callback)=>{assetIds.Add(id);assets.Add(callback);},(cache,type)=>refresh++);
                models.ChangeSkinUse(1,100);Require(refresh==1&&models.Cached(1,100)==null,"pending request refreshes immediately");
                var a=Make("model100");var child=Make("inactive-child");child.transform.SetParent(a.transform);child.SetActive(false);entities[0](a);
                Require(a.activeSelf&&a.transform.parent==root.transform&&a.transform.localPosition==new Vector3(1,2,3)&&a.transform.localScale==Vector3.one*2&&a.transform.localRotation==Quaternion.identity&&child.layer==7,"entity placement and recursive layer");
                Require(models.Cached(1,100)==null&&assetIds[0]==9033,"cache waits for auxiliary asset");
                var aux=Make("aux");aux.transform.localScale=Vector3.one*3;assets[0](aux);
                Require(models.Cached(1,100)==a&&refresh==2&&aux.transform.parent==a.transform&&aux.transform.localPosition==Vector3.zero&&aux.transform.localScale==Vector3.one*3,"auxiliary completion caches and refreshes without changing auxiliary scale");
                models.ChangeSkinUse(1,100);Require(entities.Count==1&&refresh==3,"cached request does not reload");
                report.checks.Add(new BattleBuild.Check{id="outgame-player-model-two-stage-load-placement-cache",result="pass"});
                models.ChangeSkinUse(1,101);Require(!a.activeSelf,"pending uncached selection hides previous model");
                models.ChangeSkinUse(1,100);Require(a.activeSelf,"cached reselection shows old model while new load pending");
                var b=Make("model101");entities[1](b);assets[1](Make("aux101"));
                Require(b.activeSelf&&!a.activeSelf,"late source callback displays its requested ID without race suppression");
                models.ChangeSkinUse(3,300);Require(entityIds[2]==1024,"attack prefab adds1000");entities[2](Make("attack"));Require(assetIds[2]==9034,"attack auxiliary differs");assets[2](Make("attack-aux"));
                int count=entities.Count;bool completed=false;models.LoadModel(999,true,()=>completed=true);Require(entities.Count==count&&!completed,"missing config has no callback");
                models.ChangeSkinUse(1,999);Require(!a.activeSelf&&!b.activeSelf,"missing requested ID still performs immediate visibility pass");
                report.checks.Add(new BattleBuild.Check{id="outgame-player-model-late-callback-and-attack-routing",result="pass"});
                var skins=OutgameSkinCatalog.FromOriginal(null,BattleView.ReadText("Data/Outgame/SkinConfig"),BattleView.ReadText("Data/Outgame/SceneSkinConfig"));
                int colors=0,finds=0;var errors=new List<string>();var aa=new AnimatorFixture();var bb=new AnimatorFixture();
                var animation=new OutgamePlayerAnimation(skins,obj=>colors++,obj=>{finds++;return obj==a?aa:obj==b?bb:null;},errors.Add);
                var display=new Dictionary<int,GameObject>{{100,a},{101,b}};skins.SetUsedSkin(1,100);
                a.SetActive(false);animation.Refresh(display,1);Require(colors==1&&finds==0,"color precedes inactive guard");
                a.SetActive(true);animation.Refresh(display,1);Require(string.Join("|",aa.calls)=="reset|relax:False","reset slot11 precedes nonlooping relax slot5");
                var completion=aa.OnComplete;skins.SetUsedSkin(1,101);b.SetActive(true);animation.Refresh(display,1);completion();
                Require(bb.calls[bb.calls.Count-1]=="idle:True"&&aa.calls.Count==2,"completion reads current equipped cached animator");
                skins.SetUsedSkin(1,102);completion();Require(aa.calls[aa.calls.Count-1]=="idle:True","missing current animator falls back to captured animator");
                skins.SetUsedSkin(1,100);animation.Refresh(display,1);Require(finds==2,"animator lookup cached by skin ID");
                report.checks.Add(new BattleBuild.Check{id="outgame-player-animation-reset-relax-current-equipment-idle",result="pass"});
                var missing=Make("missing-animator");display[102]=missing;skins.SetUsedSkin(1,102);animation.Refresh(display,1);animation.Refresh(display,1);
                Require(errors.Count==2&&errors[0]=="missing-animator 没有SpineAnimtor"&&finds==3,"missing animator is cached and original error emitted on each refresh");
                report.checks.Add(new BattleBuild.Check{id="outgame-player-animation-color-active-guard-missing-cache",result="pass"});
                models.ChangeSkinUse(1,102); // Missing config does not enter cache.
                var duplicate=new OutgamePlayerModels(@"{""Datas"":[{""id"":100,""skinType"":1,""prefabId"":22}]}",new Dictionary<int,Transform>{{1,root.transform}},Vector3.zero,1,(id,cb)=>entities.Add(cb),(id,cb)=>assets.Add(cb),(c,t)=>{});
                count=entities.Count;duplicate.ChangeSkinUse(1,100);duplicate.ChangeSkinUse(1,100);Require(entities.Count==count+2,"in-flight requests are not deduplicated");
                var first=Make("duplicate-first");var last=Make("duplicate-last");entities[count](first);var firstAux=assets[assets.Count-1];entities[count+1](last);var lastAux=assets[assets.Count-1];firstAux(Make("first-aux"));lastAux(Make("last-aux"));Require(duplicate.Cached(1,100)==last,"dictionary setter replaces earlier cached object");
                report.checks.Add(new BattleBuild.Check{id="outgame-player-model-duplicate-pending-cache-replacement",result="pass"});
            }
            catch(Exception e){report.passed=false;report.checks.Add(new BattleBuild.Check{id="outgame-player-models",result="fail",detail=e.ToString()});}
            finally{for(int i=objects.Count-1;i>=0;i--)if(objects[i]!=null)UnityEngine.Object.DestroyImmediate(objects[i]);}
            File.WriteAllText(Path.Combine(BattleBuild.Workspace,"analysis/outgame-player-models-validation.json"),JsonUtility.ToJson(report,true));return report;
        }
    }
}
