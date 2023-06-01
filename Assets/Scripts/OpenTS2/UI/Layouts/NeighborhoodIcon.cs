using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Layouts
{
    public class NeighborhoodIcon : UILayoutInstance
    {
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49001018, 0xA99D8A11, TypeIDs.UI);
        public NeighborhoodIcon() : this(MainCanvas)
        {

        }
        public NeighborhoodIcon(Transform parent) : base(parent)
        {

        }
    }
}
