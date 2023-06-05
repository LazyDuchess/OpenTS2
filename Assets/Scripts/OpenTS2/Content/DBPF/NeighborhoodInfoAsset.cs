using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// Basic neighborhood metadata.
    /// </summary>
    public class NeighborhoodInfoAsset : AbstractAsset
    {
        /// <summary>
        /// Main Neighborhood Prefix. Commonly "NXXX" e.g. N001.
        /// </summary>
        public string MainPrefix;
        public uint ID;
        /// <summary>
        /// Type of neighborhood.
        /// </summary>
        public Neighborhood.Type NeighborhoodType = Neighborhood.Type.Main;
        /// <summary>
        /// Sub Neighborhood Prefix. For example, U001 for a Campus.
        /// </summary>
        public string SubPrefix;
    }
}
