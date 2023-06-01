using OpenTS2.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Content
{
    public static class NeighborhoodManager
    {
        public static List<Neighborhood> Neighborhoods => _neighborHoods;
        static List<Neighborhood> _neighborHoods = new List<Neighborhood>();
        public static void Initialize()
        {
            _neighborHoods.Clear();
            var neighborhoodsFolder = Path.Combine(Filesystem.GetUserPath(),"Neighborhoods");
            var directories = Directory.GetDirectories(neighborhoodsFolder, "*", SearchOption.TopDirectoryOnly);
            foreach(var directory in directories)
            {
                var neighborhood = Neighborhood.Load(Path.GetFileName(directory));
                if (neighborhood != null)
                {
                    _neighborHoods.Add(neighborhood);
                }
            }
        }
    }
}
