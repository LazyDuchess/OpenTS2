using OpenTS2.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Audio
{
    public class MusicCategory
    {
        public string Name { get; private set; }
        public uint Hash { get; private set; }

        public MusicCategory(string name)
        {
            Name = name;
            Hash = FileUtils.LowHash(name);
        }
    }
}
