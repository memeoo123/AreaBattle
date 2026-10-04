using System;
using System.Collections.Generic;
namespace AreaBattle
{
    // RandomHelper owns one parameterless System.Random across game modules.
    public sealed class GameRandomSource
    {
        public static readonly GameRandomSource Shared=new GameRandomSource(new Random());
        public readonly Random Managed;
        public GameRandomSource(Random managed){Managed=managed;}
        public int Inclusive(int min,int max)=>Managed.Next(min,unchecked(max+1));
        public int Element(List<int> values)=>values[Managed.Next(values.Count)];
        public T Element<T>(List<T> values)=>values[Managed.Next(values.Count)];
        // RandomHelper.Randoms26196: upper bound is exclusive; selection uses the
        // same inclusive RNG as other game modules, removing each selected entry.
        public int[] DistinctRange(int count,int min,int max,Action<object[]> log)
        {
            if(count>unchecked(max-min)){log(new object[]{string.Format("随机次数[{0}]大于区间值[{1}]",count,unchecked(max-min))});return null;}
            if(min>max){log(new object[]{string.Format("随机区间值错误[{0}]小于[{1}]",max,min)});return null;}
            var candidates=new List<int>();for(int value=min;value<max;value++)candidates.Add(value);
            var selected=new List<int>();
            for(int i=0;i<count;i++){int index=Inclusive(0,candidates.Count-1);selected.Add(candidates[index]);candidates.RemoveAt(index);}
            return selected.ToArray();
        }
    }
}
