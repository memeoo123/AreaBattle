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
    }
}
