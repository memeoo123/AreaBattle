using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using AreaBattle.OriginalConfig;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameRankHomeServices
    {
        public OutgameControllerRegistry Registry;
        public Func<bool> HasCurrent;
        public Func<OutgameUserInfoControl> UserInfo;
        public OutgamePlayerCountry Country;
        public OutgameAiNames AiNames;
        public OutgameReferencePool References=OutgameReferencePool.Shared;
        public Action<Color,object[]> Log;
        public Action<string> Error;
    }
    public sealed class OutgameRankRandomObject {public int Weight,Value;}
    public sealed class OutgameRankHeadWeight {public int Weight;public HeadBoxConfig Config;}
    // Original RankControl4134: full constructor, home/settlement list and lifecycle.
    public sealed partial class OutgameRankControl:IOutgameLogicControl
    {
        public BigInteger RememberedAIScore; // Source obfuscated field16.
        public Color playerInfoColor,aiInfoColor_N,aiInfoColor_R;
        public readonly List<OutgameRankHeadWeight> HeadWeights=new List<OutgameRankHeadWeight>();
        public int HomeRankShowNum,upPlayerScoreNum,lowPlayerScoreNum,rankGapMax,rankGapMin;
        public List<OutgameRankRandomObject> defaultScoreRandList;
        public int lastRank;
        public float playerItemScaleT,playerItemLargeScaleT,playerItemSmallScaleT,playerItemLargeScaleN,playerItemScaleV;
        public int minReGap,maxReGap;
        public OutgameDynamicListProvider<OutgameRankItemData> listRankItemData=new OutgameDynamicListProvider<OutgameRankItemData>();
        public int playerListIndex;
        public Dictionary<int,string> dic_userui=new Dictionary<int,string>();
        OutgameRankHomeServices Home=>scoreServices.Home;
        public string PlayerName=>Home.UserInfo().Manager.Name;
        public void OnInit()
        {
            ColorUtility.TryParseHtmlString("#FCFFB6",out playerInfoColor);
            ColorUtility.TryParseHtmlString("#6D69A1",out aiInfoColor_N);
            ColorUtility.TryParseHtmlString("#00BFCF",out aiInfoColor_R);
            upPlayerScoreNum=scoreServices.Config().newRankSettingConfig.HM_UpPlayerNum;
            lowPlayerScoreNum=scoreServices.Config().newRankSettingConfig.HM_LowPlayerNum;
            HomeRankShowNum=scoreServices.Config().newRankSettingConfig.HM_RankShowNum;
            rankGapMin=scoreServices.Config().newRankSettingConfig.HM_RankGapRag[0];
            rankGapMax=scoreServices.Config().newRankSettingConfig.HM_RankGapRag[1];
            playerItemScaleT=scoreServices.Config().newRankSettingConfig.playerItemScaleT/1000f;
            playerItemLargeScaleT=scoreServices.Config().newRankSettingConfig.playerItemLargeScaleT/1000f;
            playerItemSmallScaleT=scoreServices.Config().newRankSettingConfig.playerItemSmallScaleT/1000f;
            playerItemLargeScaleN=scoreServices.Config().newRankSettingConfig.playerItemLargeScaleN/10f;
            playerItemScaleV=scoreServices.Config().newRankSettingConfig.playerItemscaleV/10f;
            minReGap=scoreServices.Config().newRankSettingConfig.GM_RankGapRe[0];
            maxReGap=scoreServices.Config().newRankSettingConfig.GM_RankGapRe[1];
            InitializeHeadWeights();InitDefaultRankList();InitPlayerRank();InitHomeInfo();
        }
        public void Updata(float deltaTime,float unscaledDeltaTime){} //31672 empty.
        public void OnDispose()=>Home.Registry.Clear(4134); //31682 clears slot only.
        public void InitializeHeadWeights()
        {
            foreach(var pair in scoreServices.Config().dicHeadBox){var row=pair.Value;int weight=pair.Value.weight;HeadWeights.Add(new OutgameRankHeadWeight{Config=row,Weight=weight});}
        }
        public HeadBoxConfig RandomHeadBox()=>OutgameRankWeightedRandom.SelectOne(HeadWeights,row=>row.Weight,scoreServices.Random,Home.Error).Config;
        public int CalWeight(int value)=>unchecked(rankGapMax-rankGapMin+2-value);
        public void InitDefaultRankList()
        {
            if(defaultScoreRandList==null)defaultScoreRandList=new List<OutgameRankRandomObject>();
            defaultScoreRandList.Clear();for(int i=rankGapMin;i<=rankGapMax;i=unchecked(i+1))defaultScoreRandList.Add(new OutgameRankRandomObject{Weight=CalWeight(i),Value=unchecked(i*100)});
        }
        public void SortScoreWeights(List<OutgameRankRandomObject> rows,bool descending)
        {
            for(int i=1;i<rows.Count;i++){
                var current=rows[i];int j=i-1;
                while(j>=0&&(descending?rows[j].Value<current.Value:rows[j].Value>current.Value)){rows[j+1]=rows[j];j--;}
                rows[j+1]=current;
            }
        }
        public string GetCurPlayerCountryInfo()
        {
            int id=Home.Country.PlayerCountryID;
            if(id==-1)id=Home.Country.GetPlayerCountry();else id=Home.Country.PlayerCountryID;
            if(!Home.HasCurrent())return string.Empty;
            if(id==-1)return "country_com";
            return scoreServices.Config().dicCountryConfig[id].countryFlagName;
        }
        public OutgameRankItemData GetPlayerRankData(int rank,BigInteger score)
        {
            var row=Home.References.Acquire<OutgameRankItemData>();row.rankIndex=rank;
            row.countryN=GetCurPlayerCountryInfo();row.name=PlayerName;row.score=scoreServices.Format(score);return row;
        }
        public Dictionary<int,string> GetRandomRankInfo_HM()
        {
            var result=new Dictionary<int,string>();var names=Home.AiNames.Generate(unchecked(HomeRankShowNum-1),true);
            Home.Log(Color.green,new object[]{"随机到的ai信息数据个数："+names.Count});
            int above=Math.Min(unchecked(CurPlayerRank-1),50);
            if(above>=1)foreach(var pair in RandomRankAIScore(true,names,above,CurPlayerRank,CurPlayerScore,BigInteger.Zero,false))result.Add(pair.Key,pair.Value);
            var builder=new StringBuilder(10);int country=Home.Country.GetPlayerCountry();
            if(!Home.HasCurrent())return null;
            Home.Log(Color.green,new object[]{"国籍："+country});
            builder.Append(";");builder.Append(PlayerName);builder.Append(";");builder.Append(scoreServices.Format(CurPlayerScore));
            playerListIndex=result.Count;result.Add(CurPlayerRank,builder.ToString());
            int low=lowPlayerScoreNum,remaining=unchecked(HomeRankShowNum-above-1);
            foreach(var pair in RandomRankAIScore(false,names,Math.Max(low,remaining),CurPlayerRank,CurPlayerScore,BigInteger.Zero,false))result.Add(pair.Key,pair.Value);
            return result;
        }
        public void InitHomeInfo()
        {
            dic_userui=scoreServices.Current().GetRandomRankInfo_HM();
            foreach(var row in listRankItemData.Data)if(row!=null)Home.References.Release(row);
            listRankItemData.Data.Clear();
            foreach(var pair in dic_userui){
                var row=Home.References.Acquire<OutgameRankItemData>();row.rankIndex=pair.Key;var parts=pair.Value.Split(';');
                row.countryN=parts[0];row.name=parts[1];row.score=parts[2];row.headBoxId=RandomHeadBox().id;listRankItemData.Data.Add(row);
            }
            dic_userui.Clear();
        }
        public List<OutgameRankItemData> GetOverUIRankAIData(int count,bool above,int rank,BigInteger score,BigInteger bound)
        {
            var result=new List<OutgameRankItemData>();var names=Home.AiNames.Generate(count,true);
            foreach(var pair in RandomRankAIScore(above,names,count,rank,score,bound,true)){
                var row=Home.References.Acquire<OutgameRankItemData>();row.rankIndex=pair.Key;var parts=pair.Value.Split(';');
                row.countryN=parts[0];row.name=parts[1];row.score=parts[2];row.headBoxId=RandomHeadBox().id;result.Add(row);
            }
            return result;
        }
        public static int CompareAboveGap(int a,int b)=>a>b?-1:1; //31688 returns1 even for equality.
        public Dictionary<int,string> RandomRankAIScore(bool above,List<string> names,int count,int rank,BigInteger baseScore,BigInteger bound,bool over)
        {
            var result=new Dictionary<int,string>();var builder=new StringBuilder();var gaps=new List<int>();
            if(over){
                int running=minReGap;if(lastRank==rank&&above)running=unchecked(running+1);
                for(int i=0;i<count;i++){
                    int r=scoreServices.Random.Inclusive(running,unchecked(running+maxReGap-1));int candidate=unchecked(r+running);int next;
                    if(bound==BigInteger.Zero||BigInteger.Parse(unchecked(candidate*100).ToString(CultureInfo.InvariantCulture))<bound)next=unchecked(running+r);
                    else{
                        next=0;
                        if(running>=maxReGap){int min=minReGap,max=maxReGap;next=scoreServices.Random.Inclusive(lastRank==rank&&above?unchecked(min+1):min,max);}
                    }
                    running=next;gaps.Add(unchecked(running*100));
                }
            }else{
                var selected=OutgameRankWeightedRandom.SelectMany(defaultScoreRandList,count,row=>row.Weight,scoreServices.Random)??new List<OutgameRankRandomObject>();
                SortScoreWeights(selected,above);foreach(var row in selected)gaps.Add(row.Value);
            }
            if(above)gaps.Sort((a,b)=>CompareAboveGap(a,b));else gaps.Sort();
            for(int i=0;i<gaps.Count;i++){
                int key=above?unchecked(i+rank-count):unchecked(rank+i+1);builder.Clear();
                if(names.Count<=0)Home.Log(Color.green,new object[]{"随机的ai个数错误："+i});
                builder.Append(names[0]);builder.Append(";");var gap=BigInteger.Parse(gaps[i].ToString(CultureInfo.InvariantCulture));BigInteger score;
                if(above)score=baseScore+gap;
                else{score=gap>=baseScore?BigInteger.Zero:baseScore-gap;if(score<BigInteger.Zero)score=BigInteger.Zero;}
                if(RememberedAIScore==BigInteger.Zero&&lastRank==rank&&i==0)RememberedAIScore=score;
                builder.Append(scoreServices.Format(score));builder.Append(";");builder.Append(scoreServices.Random.Inclusive(0,3));builder.Append(";");builder.Append(scoreServices.Random.Inclusive(0,1));
                names.RemoveAt(0);result.Add(key,builder.ToString());
            }
            return result;
        }
    }
}
