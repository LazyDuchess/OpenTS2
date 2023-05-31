using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Common.Utils
{
    public static class RandomUtils
    {
        public static T RandomFromList<T>(List<T> list)
        {
            if (list.Count <= 0)
                return default;
            return list[Random.Range(0, list.Count)];
        }
    }
}
