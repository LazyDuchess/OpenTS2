using OpenTS2.Common;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class CASManager
    {
        public static CASManager Instance { get; private set; }
        public bool InCAS = false;
        private const string CASLotName = "CAS!";
        private static ResourceKey CASLotKey = new ResourceKey(0, CASLotName, TypeIDs.BASE_LOT_INFO);

        public CASManager()
        {
            Instance = this;
        }

        public void EnterCAS()
        {
            if (NeighborhoodManager.Instance.CurrentNeighborhood == null)
                throw new Exception("Must be in a neighborhood to enter CAS!");
            InCAS = true;
            LotManager.Instance.EnterLot(ContentManager.Instance.GetAsset<BaseLotInfoAsset>(CASLotKey));
        }
    }
}
