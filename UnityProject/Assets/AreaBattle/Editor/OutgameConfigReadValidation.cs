using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using AreaBattle.OriginalConfig;
namespace AreaBattle.EditorTools
{
    public static class OutgameConfigReadValidation
    {
        [Serializable] sealed class Roster {public Row[] rows;}
        [Serializable] sealed class Row {public string sourceName,kind;public int rowCount;}
        public static BattleBuild.Report Run()
        {
            var report=new BattleBuild.Report{passed=true,unityVersion=Application.unityVersion};
            Action<string,Action> test=(id,action)=>{try{action();report.checks.Add(new BattleBuild.Check{id=id,result="pass",detail=""});}catch(Exception ex){report.passed=false;report.checks.Add(new BattleBuild.Check{id=id,result="fail",detail=ex.ToString()});}};
            test("original-config-native-64-types-2724-rows",()=>{
                var bundle=AssetBundle.LoadFromFile(Path.Combine(OutgameFirstPackBundleBuild.Folder,"firstpack.unity3d"));
                try{Require(bundle!=null,"Native firstpack required");var reader=new OutgameLegacyConfigRead(name=>bundle.LoadAsset<TextAsset>(name),message=>throw new Exception(message));
                    var roster=JsonUtility.FromJson<Roster>(File.ReadAllText(Path.Combine(BattleBuild.Target,"generated/outgame/CONFIG_INITIALIZATION_ROSTER.json")));
                    int total=0,values=0;foreach(var row in roster.rows){
                        var type=typeof(ChannelConfig).Assembly.GetType("AreaBattle.OriginalConfig."+row.sourceName);Require(type!=null,"type "+row.sourceName);
                        if(row.kind=="table"){
                            var dictionary=(IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(object),type));
                            typeof(OutgameLegacyConfigRead).GetMethod("ReadTable").MakeGenericMethod(type).Invoke(reader,new object[]{dictionary});
                            Require(dictionary.Count==row.rowCount,"row count "+row.sourceName);total+=dictionary.Count;
                            foreach(DictionaryEntry item in dictionary)Require(item.Key is int&&Equals(item.Key,((IOutgameConfigRow)item.Value).UniqueID),"original boxed integer key");
                        }else{Require(typeof(OutgameLegacyConfigRead).GetMethod("ReadValue").MakeGenericMethod(type).Invoke(reader,null)!=null,"value "+row.sourceName);values++;}
                    }
                    Require(total==2724&&values==6&&reader.ReadCount==58,"original roster and read counter");
                }finally{if(bundle!=null)bundle.Unload(true);}
            });
            test("original-config-duplicates-preserve-first-and-counter",()=>{
                var asset=new TextAsset("prefix {\"Datas\":[{\"id\":4,\"noAB\":1},{\"id\":4,\"noAB\":9},{\"id\":5,\"noAB\":2}]}");
                try{var errors=new List<string>();var reader=new OutgameLegacyConfigRead(name=>asset,errors.Add);var rows=new Dictionary<object,ChannelConfig>();reader.ReadTable(rows);
                    Require(rows.Count==2&&rows[4].noAB==1&&errors.Count==1&&errors[0]=="表[ChannelConfig]中有相同键(4)"&&reader.ReadCount==1,"duplicate behavior");
                    reader.ReadTable(rows);Require(rows.Count==2&&errors.Count==4&&reader.ReadCount==2,"destination is not cleared");
                }finally{UnityEngine.Object.DestroyImmediate(asset);}
            });
            test("original-config-missing-malformed-and-value-counter",()=>{
                var errors=new List<string>();var reader=new OutgameLegacyConfigRead(name=>null,errors.Add);reader.ReadTable(new Dictionary<object,ChannelConfig>());
                Require(reader.ReadValue<FuncSettingConfig>()==null&&reader.ReadCount==1&&errors.Count==2,"missing table counts, value does not");
                var asset=new TextAsset("no opening brace");try{
                    reader=new OutgameLegacyConfigRead(name=>asset,errors.Add);bool failed=false;try{reader.ReadTable(new Dictionary<object,ChannelConfig>());}catch(ArgumentOutOfRangeException){failed=true;}
                    Require(failed&&reader.ReadCount==0,"parse failure precedes count increment");
                }finally{UnityEngine.Object.DestroyImmediate(asset);}
            });
            test("original-config-online-override-read-and-write-order",()=>{
                var keys=new[]{"Proj_hdzd_HeroSetting","Proj_hdzd_BossLevel","Proj_hdzd_HeroPreUnlockLevel","Proj_hdzd_ValentineSetting","DontShowInsertAfterSeeToolVideo","DontShowInsertIfSeeOverVideo","ShowInsertADPerXLevel","NoRemoveAds"};
                var values=new[]{" FALSE "," -4 ","0","12","23","34","45","not parsed"};var config=new FuncSettingConfig{BossLevel=90,HeroSetting=true,SummerSetting=7,useNewUIMask=true};int index=0;
                OutgameConfigOnlineOverrides.Apply(config,key=>{Require(key==keys[index]&&config.BossLevel==90&&config.HeroSetting,"all reads precede mutations");return values[index++];});
                Require(index==8&&config.BossLevel==-4&&!config.HeroSetting&&config.HeroPreUnlockLevel==0&&config.ValentineSetting==12&&config.dontShowInsertAfterSeeToolVideo==23&&config.dontShowInsertIfSeeOverVideo==34&&config.ShowInsertADPerXLevel==45,"source online field mapping");
                Require(config.SummerSetting==7&&config.useNewUIMask,"unassigned fields preserved");
                OutgameConfigOnlineOverrides.Apply(null,key=>null);OutgameConfigOnlineOverrides.Apply(null,key=>"");
            });
            test("original-config-online-override-partial-parse-failure",()=>{
                var config=new FuncSettingConfig{BossLevel=90,HeroSetting=true,HeroPreUnlockLevel=11};int reads=0;bool failed=false;
                try{OutgameConfigOnlineOverrides.Apply(config,key=>{reads++;return key=="Proj_hdzd_BossLevel"?"4":key=="Proj_hdzd_HeroSetting"?"1":"8";});}catch(FormatException){failed=true;}
                Require(failed&&reads==8&&config.BossLevel==4&&config.HeroSetting&&config.HeroPreUnlockLevel==11,"Boolean.Parse rejects numeric1 after boss mutation, later writes skipped");
                failed=false;try{OutgameConfigOnlineOverrides.Apply(config,key=>key=="Proj_hdzd_BossLevel"?"   ":null);}catch(FormatException){failed=true;}
                Require(failed&&config.BossLevel==4,"whitespace is parsed rather than treated as absent");
            });
            test("original-config-online-provider-failure-before-mutation",()=>{
                var config=new FuncSettingConfig{BossLevel=90};int reads=0;bool failed=false;
                try{OutgameConfigOnlineOverrides.Apply(config,key=>{reads++;if(key=="NoRemoveAds")throw new InvalidOperationException("provider");return "4";});}catch(InvalidOperationException){failed=true;}
                Require(failed&&reads==8&&config.BossLevel==90,"even discarded final read can abort before mutations");
            });
            test("original-config-scene-index-partial-duplicate",()=>{
                var skins=new Dictionary<object,SceneSkinConfig>{{1,new SceneSkinConfig{idleIconName="idle",gameIconName="game"}},{2,new SceneSkinConfig{idleIconName="idle2",gameIconName="idle"}}};
                var resources=new Dictionary<string,int>{{"old",1}};bool failed=false;
                try{OutgameConfigDerivedIndexes.RebuildSceneResources(skins,resources);}catch(ArgumentException){failed=true;}
                Require(failed&&!resources.ContainsKey("old")&&resources.Count==3&&resources["idle"]==12&&resources["game"]==12&&resources["idle2"]==12,"source clear/add order and partial failure");
            });
            test("original-config-guide-index-and-name-append",()=>{
                var guides=new Dictionary<object,GuideConfig>{{1,new GuideConfig{id=1,guildLv=0}},{2,new GuideConfig{id=2,guildLv=-1}},{3,new GuideConfig{id=3,guildLv=0}},{4,new GuideConfig{id=4,guildLv=-2}}};
                var levels=new Dictionary<int,int>{{99,99}};OutgameConfigDerivedIndexes.RebuildGuideLevels(levels,()=>{Require(levels.Count==0,"clear before singleton source lookup");return guides;});
                Require(levels.Count==2&&levels[0]==3&&levels[-2]==4,"only minus1 skipped, last same level wins");
                var names=new Dictionary<object,AINameConfig>{{1,new AINameConfig{zh_cn="one"}},{2,new AINameConfig{zh_cn=null}}};var list=new List<string>{"existing"};
                OutgameConfigDerivedIndexes.AppendChineseNames(names,list);OutgameConfigDerivedIndexes.AppendChineseNames(names,list);
                Require(list.Count==5&&list[0]=="existing"&&list[1]=="one"&&list[2]==null&&list[3]=="one"&&list[4]==null,"source names append including null and repeated calls");
            });
            test("original-config-global-values-original-data-and-partial-failure",()=>{
                var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),message=>throw new Exception(message));var values=new Dictionary<object,GlobalValueConfig>();reader.ReadTable(values);
                var globals=new OutgameConfigGlobalValues();globals.Initialize(values,()=>200);
                Require(globals.GameTimeScale==1f&&globals.BaseGameTimeScale==1f&&globals.ShipTimeScale==.8f&&globals.StarScale==1f&&globals.LineRendererWide==.1f&&globals.HalfLineRendererWide==.05f&&globals.LineRendererMoveSpeed==-.1f&&globals.BaseLineRendererMoveSpeed==-.1f&&globals.LineRendererTilingScale==16f&&globals.ShareURL==values[200].Content1,"all original global assignments");
                values[101].Content1="2";values[113].Content1="0.4;-0.3;broken";bool failed=false;
                try{globals.Initialize(values,()=>throw new Exception("must not reach share key"));}catch(FormatException){failed=true;}
                Require(failed&&globals.GameTimeScale==2f&&globals.LineRendererWide==.4f&&globals.LineRendererMoveSpeed==-.6f&&globals.BaseLineRendererMoveSpeed==-.6f&&globals.LineRendererTilingScale==16f&&globals.HalfLineRendererWide==.05f,"partial float assignments preserve source order");
            });
            test("original-config-camp-colors-before-material-requests",()=>{
                var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),message=>throw new Exception(message));var camps=new Dictionary<object,CampConfig>();reader.ReadTable(camps);
                var lines=new Dictionary<int,Color>{{99,Color.magenta}};var soldiers=new Dictionary<int,Color[]>();int requests=0;
                OutgameConfigCampPresentation.Initialize(camps,()=>9,lines,soldiers,(path,args)=>{
                    Require(lines.Count==11&&soldiers.Count==10,"all colors precede first material load and old unrelated keys persist");
                    Require((int)args[0]==requests&&path==camps[requests].BasitionMaterial&&(string)args[1]==path,"original material path and argument order");requests++;
                });
                Require(requests==10&&lines[99]==Color.magenta,"inclusive original camp count0 through9");
                for(int i=0;i<=9;i++){Color c;ColorUtility.TryParseHtmlString("#"+camps[i].LineColor,out c);Require(lines[i]==c&&soldiers[i].Length==3,"native source color mapping");}
                camps[0].LineColor="invalid";OutgameConfigCampPresentation.Initialize(camps,()=>0,lines,soldiers,(path,args)=>{});Color invalid;ColorUtility.TryParseHtmlString("#invalid",out invalid);Require(lines[0]==invalid,"TryParse result ignored, out color preserved");
            });
            test("original-config-manager-native-composition-and-repeat",()=>{
                var bundle=AssetBundle.LoadFromFile(Path.Combine(OutgameFirstPackBundleBuild.Folder,"firstpack.unity3d"));
                try{
                    var state=new OutgameLegacyConfigReadState(message=>throw new Exception(message));OutgameLegacyConfigManager manager=null;int onlineReads=0;
                    var callbacks=new List<Action<OutgameLegacyPrefabResource>>();var arguments=new List<object[]>();
                    manager=new OutgameLegacyConfigManager(state,new OutgameConfigGlobalValues(),key=>{onlineReads++;Require(state.Reader.ReadCount==58&&manager.newRankSettingConfig!=null&&!manager.IsInitialized,"all64 reads precede online processing");return null;},()=>9,()=>200,()=>manager.dicGuide,
                        (path,callback,args)=>{Require(!manager.IsInitialized&&manager.SoldierColors.Count==10,"color pass precedes async material requests");callbacks.Add(callback);arguments.Add(args);});
                    manager.SetConfigABRes(new OutgameLegacyPrefabResource(bundle));manager.Initialize();
                    Require(manager.IsInitialized&&state.UseUnityJson&&!state.UseMemoryPack&&callbacks.Count==10&&manager.CampMaterials.Count==0&&manager.SceneResources.Count==30&&manager.GuideLevels.Count>0&&manager.ChineseNames.Count==manager.dicAIName.Count&&manager.Globals.ShipTimeScale==.8f,"ordered native config composition completes without waiting for materials");
                    callbacks[3](new OutgameLegacyPrefabResource(null){Arguments=arguments[3]});Require(manager.CampMaterials.ContainsKey(3)&&manager.CampMaterials[3]==null,"late material completion retains null by source contract");
                    state.UseUnityJson=false;state.UseMemoryPack=true;manager.Initialize();Require(state.UseUnityJson&&!state.UseMemoryPack&&onlineReads==8&&callbacks.Count==10&&state.Reader.ReadCount==58,"repeat reapplies read flags and skips all initialization work");
                }finally{if(bundle!=null)bundle.Unload(true);}
            });
            test("original-resource-arguments-filename-and-typed-load",()=>{
                var resource=new OutgameLegacyPrefabResource(null);Require(resource.GetArg<int>(0)==0&&resource.GetArg<string>(1)==null&&resource.LoadAsset<Material>("missing")==null,"missing argument and native bundle defaults");
                resource.Arguments=new object[]{3,"folder/a.b.mat"};Require(resource.GetArg<int>(0)==3&&resource.GetArg<int>(2)==0,"cast and upper bound");bool failed=false;try{resource.GetArg<int>(1);}catch(InvalidCastException){failed=true;}Require(failed,"wrong argument type throws");
                Require(OutgameConfigMaterialCompletion.ToFileName("folder/a.b.mat")=="a"&&OutgameConfigMaterialCompletion.ToFileName("folder/")==""&&OutgameConfigMaterialCompletion.ToFileName(@"folder\a.mat")==@"folder\a","original slash/first-dot rules");
                var mats=new Dictionary<int,Material>();OutgameConfigMaterialCompletion.Loaded(resource,mats);Require(mats.ContainsKey(3)&&mats[3]==null,"null material cached");
            });
            test("original-main-config-startup-order-and-failed-init",()=>{
                var order=new List<string>();Action<OutgameLegacyPrefabResource> callback=null;
                var state=new OutgameLegacyConfigReadState(message=>{order.Add("missing");});
                var config=new OutgameLegacyConfigManager(state,new OutgameConfigGlobalValues(),key=>null,()=>9,()=>200,()=>null,(path,ready,args)=>{});
                var main=new OutgameMainConfigStartup(order.Add,()=>order.Add("sdk"),(path,ready,args)=>{Require(path=="data/config"&&args.Length==0,"source config path and empty arguments");order.Add("load");callback=ready;},()=>{order.Add("manager");return config;},()=>order.Add("advance"));
                main.LoadingShown();Require(string.Join(",",order)=="LoadingUIShow,sdk,load"&&!main.ConfigLoaded,"loading shown sequencing");
                bool failed=false;try{callback(null);}catch(NullReferenceException){failed=true;}
                Require(failed&&!main.ConfigLoaded&&!config.IsInitialized&&string.Join(",",order)=="LoadingUIShow,sdk,load,ConfigLoadOver,manager,manager,missing","failed config initialization cannot set Main completion or advance");
            });
            test("original-pre-game-settings-native-and-number-symbols",()=>{
                var reader=new OutgameLegacyConfigRead(name=>Resources.Load<TextAsset>("Recovered/FirstPack/Config/"+name),message=>throw new Exception(message));var configs=new Dictionary<object,LargeNumConfig>();reader.ReadTable(configs);
                var symbols=new OutgameBigNumberSymbols();bool oldTouch=Input.multiTouchEnabled;int oldRate=Application.targetFrameRate,mode=0;try{
                    // Desktop backends can ignore the touch flag. Check the call/order and the
                    // backend's actual readback separately, without requiring mobile behavior.
                    Input.multiTouchEnabled=false;bool disabledReadback=Input.multiTouchEnabled;Input.multiTouchEnabled=oldTouch;
                    bool touchCalled=false;
                    var settings=new OutgamePreGameSettings(value=>mode=value,()=>{Require(mode==2&&touchCalled&&Input.multiTouchEnabled==disabledReadback,"report then native touch request precede symbols");return OutgameBigNumberSymbols.FromConfig(configs);},symbols,value=>{Require(!value&&mode==2,"source requests false after report mode");Input.multiTouchEnabled=value;touchCalled=true;});settings.Initialize();
                    report.limitations+=" Native multiTouchEnabled=false readback on "+Application.platform+": "+disabledReadback+".";
                    Require(Application.targetFrameRate==60&&symbols.FirstMagnitude==3&&symbols.ByMagnitude[3]=="K"&&symbols.Symbols[0]=="K"&&symbols.Symbols.Count==configs.Count,"native frame setting and original symbol map");
                }finally{Input.multiTouchEnabled=oldTouch;Application.targetFrameRate=oldRate;}
            });
            test("original-number-symbol-order-empty-failure-and-alias",()=>{
                var symbols=new OutgameBigNumberSymbols();var map=new Dictionary<int,string>{{9,"B"},{3,"K"}};symbols.Set(map);
                Require(symbols.FirstMagnitude==9&&symbols.Symbols[0]=="B"&&ReferenceEquals(symbols.ByMagnitude,map),"first enumerated entry, not minimum key");map[9]="changed";Require(symbols.ByMagnitude[9]=="changed"&&symbols.Symbols[0]=="B","dictionary retained while value list copied");
                var order=new List<string>();bool failed=false;try{new OutgamePreGameSettings(value=>order.Add("mode"),()=>{order.Add("get");return new Dictionary<int,string>();},symbols,value=>order.Add("touch"),value=>order.Add("fps")).Initialize();}catch(InvalidOperationException){failed=true;}
                Require(failed&&string.Join(",",order)=="mode,touch,get"&&symbols.Symbols.Count==0&&symbols.ByMagnitude.Count==0&&symbols.FirstMagnitude==9,"empty map leaves partial state and never sets frame rate");
            });return report;
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
