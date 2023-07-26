using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Components
{
    public class AssetReferenceComponent : MonoBehaviour
    {
        public List<AbstractAsset> References = new List<AbstractAsset>();

        public void AddReference(params AbstractAsset[] assets)
        {
            References.AddRange(assets);
        }
    }
}
