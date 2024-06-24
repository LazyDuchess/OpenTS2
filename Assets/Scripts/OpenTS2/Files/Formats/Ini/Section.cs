using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Files.Formats.Ini
{
    /// <summary>
    /// INI Section with keys and values.
    /// </summary>
    public class Section
    {
        public Dictionary<string, string> KeyValues = new Dictionary<string, string>();
    }
}
