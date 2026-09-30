using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AreaBattle
{
    public sealed class OutgameSkillDescription
    {
        [Serializable] sealed class Skills {public Skill[] Datas;}
        [Serializable] sealed class Values {public Value[] Datas;}
        [Serializable] sealed class Skill {public int id,unLockLevel,useType,targetType;public int[] describeData,dataType;}
        [Serializable] sealed class Value {public int id;public double duration,data1,data2,data3;public double At(int index){switch(index){case 0:return duration;case 1:return data1;case 2:return data2;case 3:return data3;default:throw new ArgumentOutOfRangeException(nameof(index));}}}
        readonly Dictionary<int,Skill> skills=new Dictionary<int,Skill>();readonly Dictionary<int,Value> values=new Dictionary<int,Value>();readonly Func<string,string> language;
        public OutgameSkillDescription(string skillsJson,string valuesJson,Func<string,string> localize)
        {language=localize;foreach(var row in JsonUtility.FromJson<Skills>(skillsJson).Datas)skills.Add(row.id,row);foreach(var row in JsonUtility.FromJson<Values>(valuesJson).Datas)values.Add(row.id,row);}
        public int UnlockLevel(int skill)=>skills[skill].unLockLevel;
        public string Name(int skill)=>language("Commander.SkillName."+skill);
        public string Target(int skill)=>language("Commander.Target."+skills[skill].targetType);
        public string Operating(int skill)=>language("Commander.Operating."+skills[skill].useType);
        public Dictionary<int,string> Comparison(int skill,int level)
        {
            // CommanderUI f12846 iterates dataType, not describeData. Zero suppresses a row.
            var result=new Dictionary<int,string>();var config=skills[skill];var current=values[skill*100+level];
            var next=level<=9?values[skill*100+level+1]:null;
            for(int field=0;field<config.dataType.Length;field++)
            {
                int type=config.dataType[field];if(type==0)continue;
                double value=current.At(field);bool percent=(type&~1)==4;
                double delta=next==null?0:Math.Abs(next.At(field)-value);
                if(percent)value=Math.Abs(value-1)*100;
                if(delta!=0)
                {
                    delta=Math.Round(percent?delta*100:delta,2,MidpointRounding.ToEven);
                    result[type]=string.Format(CultureInfo.InvariantCulture,percent?"{0}% <color=#FFFE18>+{1}%</color>":"{0} <color=#FFFE18>+{1}</color>",value,delta);
                }
                else result[type]=(percent?Math.Round(value,2,MidpointRounding.ToEven):value).ToString(CultureInfo.InvariantCulture)+(percent?"%":"");
            }
            return result;
        }
        public string Description(int skill,int level)
        {
            var config=skills[skill];var row=values[skill*100+level];var args=new object[config.describeData.Length];
            for(int i=0;i<args.Length;i++)
            {
                int field=config.describeData[i];double value=row.At(field);
                int kind=field<config.dataType.Length?config.dataType[field]&~1:-2;
                if(kind==4)value=Math.Abs(value-1)*100;
                args[i]=" "+Math.Round(value,1,MidpointRounding.ToEven).ToString(CultureInfo.InvariantCulture)+" ";
            }
            return string.Format(language("Commander.SkillDetail."+skill),args);
        }
    }
}
