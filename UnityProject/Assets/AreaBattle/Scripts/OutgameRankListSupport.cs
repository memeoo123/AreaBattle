using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Original IReference4138 and ReferencePool4142; separate from the similarly
    // named framework pool3449 and from the GameObject pools.
    public interface IOutgameReference {void Clear();}
    public sealed class OutgameReferencePool
    {
        public static readonly OutgameReferencePool Shared=new OutgameReferencePool();
        readonly Dictionary<Type,Collection> collections=new Dictionary<Type,Collection>();
        public bool StrictChecks; // Original static default is false.
        sealed class Collection
        {
            public readonly Queue<IOutgameReference> Queue=new Queue<IOutgameReference>();
            public readonly Type Type;
            public int Using,AcquireCount,ReleaseCount,AddCount,RemoveCount;
            public Collection(Type type){Type=type;}
        }
        Collection Get(Type type)
        {
            if(type==null)throw new Exception("ReferenceType is invalid.");
            lock(collections){if(!collections.TryGetValue(type,out var value)){value=new Collection(type);collections.Add(type,value);}return value;}
        }
        public T Acquire<T>()where T:class,IOutgameReference,new()
        {
            var c=Get(typeof(T));if(typeof(T)!=c.Type)throw new Exception("Type is invalid.");
            c.Using=unchecked(c.Using+1);c.AcquireCount=unchecked(c.AcquireCount+1);
            lock(c.Queue){if(c.Queue.Count>0)return (T)c.Queue.Dequeue();}
            c.AddCount=unchecked(c.AddCount+1);return new T();
        }
        public void Release(IOutgameReference value)
        {
            if(value==null)throw new Exception("Reference is invalid.");
            var type=value.GetType();
            if(StrictChecks){
                if(!type.IsClass||type.IsAbstract)throw new Exception("Reference type is not a non-abstract class type.");
                if(!typeof(IOutgameReference).IsAssignableFrom(type))throw new Exception(string.Format("Reference type '{0}' is invalid.",type.FullName));
            }
            var c=Get(type);value.Clear(); // Clear precedes lock and duplicate check.
            lock(c.Queue){if(StrictChecks&&c.Queue.Contains(value))throw new Exception("The reference has been released.");c.Queue.Enqueue(value);}
            c.ReleaseCount=unchecked(c.ReleaseCount+1);c.Using=unchecked(c.Using-1);
        }
    }
    // RankItemData4260: construction leaves strings null; Release clears to Empty.
    public sealed class OutgameRankItemData:IOutgameReference
    {
        public int rankIndex;public string countryN,name,score;public int headBoxId;
        public void Clear(){rankIndex=0;countryN=string.Empty;name=string.Empty;score=string.Empty;headBoxId=0;}
    }
    public static class OutgameRankWeightedRandom
    {
        // Shared generic26201: random priority + weight, not repeated weighted draws.
        // Requesting all returns the input reference and consumes no random numbers.
        public static List<T> SelectMany<T>(List<T> rows,int count,Func<T,int> weight,GameRandomSource random)
        {
            if(rows==null||count<1||count>rows.Count)return null;
            if(count==rows.Count)return rows;
            int total=0;for(int i=0;i<rows.Count;i++)total=unchecked(total+weight(rows[i])+1);
            var priorities=new List<KeyValuePair<int,int>>();
            for(int i=0;i<rows.Count;i++){
                int w=weight(rows[i]);int score=unchecked(random.Managed.Next(0,total)+w+1);priorities.Add(new KeyValuePair<int,int>(i,score));
            }
            priorities.Sort((a,b)=>unchecked(b.Value-a.Value)); //26206, including overflow.
            var result=new List<T>();for(int i=0;i<count;i++)result.Add(rows[priorities[i].Key]);return result;
        }
        public static T SelectOne<T>(List<T> rows,Func<T,int> weight,GameRandomSource random,Action<string> error)where T:class
        {
            if(rows==null){error("权重随机集合不能为空");return null;}
            int total=0;foreach(var row in rows)total=unchecked(total+weight(row));
            int roll=random.Managed.Next(0,total),cumulative=0;
            for(int i=0;i<rows.Count;i++){cumulative=unchecked(cumulative+weight(rows[i]));if(roll<cumulative)return rows[i];}
            return rows[0];
        }
    }
    // ConfigHelper32594/32591. The app-country port remains asynchronous; a pending
    // request returns -1. No inferred locale or synthetic platform callback.
    public sealed class OutgamePlayerCountry
    {
        readonly Func<OutgameLegacyConfigManager> config;
        readonly Action<Action<string>> request;
        readonly Action<Color,object[]> log;
        public int PlayerCountryID=-1;public bool Requested;
        public OutgamePlayerCountry(Func<OutgameLegacyConfigManager> config,Action<Action<string>> request,Action<Color,object[]> log)
        {this.config=config;this.request=request;this.log=log;}
        public int GetPlayerCountry()
        {
            PlayerCountryID=-1;Requested=false;
            if(!Requested){Requested=true;request(OnCountryCode);}
            return PlayerCountryID;
        }
        void OnCountryCode(string code)
        {
            log(Color.green,new object[]{"countryCode:"+code});
            foreach(var pair in config().dicCountryConfig){
                for(int i=0;i<pair.Value.countryCode.Length;i++)if(pair.Value.countryCode[i]==code){PlayerCountryID=pair.Value.id;break;}
                if(PlayerCountryID!=-1)break;
            }
            Requested=true;
        }
    }
}
