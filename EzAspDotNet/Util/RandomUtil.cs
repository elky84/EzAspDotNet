using System;
using System.Collections.Generic;
using System.Text;

namespace WebShared.Util
{
    public class RandomUtil
    {
        private Random random = new Random();

        public int Get(int max)
        {
            return random.Next(max);
        }

        public int Get(int min, int max)
        {
            return random.Next(min, max);
        }

        public double Get(double max)
        {
            return random.NextDouble() * max;
        }

        public double Get(double min, double max)
        {
            return random.NextDouble() * (max - min) + min;
        }

        public List<T> Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
