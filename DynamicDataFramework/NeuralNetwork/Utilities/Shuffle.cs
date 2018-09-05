using System;
using System.Linq;
using System.Collections.Generic;

namespace DynamicData
{
    public static class Shuffle
    {
        /// <summary>
        /// Used to shuffle lists into random orders to prevent networks from becoming biased to ordering.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Usually a PairBit List. </param>
        public static void ShuffleList<T>(this IList<T> list)
        {
            Random rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
