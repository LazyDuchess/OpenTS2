using OpenTS2.Content.DBPF;
using OpenTS2.Engine;
using OpenTS2.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class LotManager
    {
        public static LotManager Instance { get; private set; }

        public LotManager()
        {
            Instance = this;
        }

        public void EnterLot(BaseLotInfoAsset baseLotInfo)
        {
            if (NeighborhoodManager.Instance.CurrentNeighborhood == null)
                throw new Exception("Must be in a neighborhood to enter a lot");
            ContentManager.Instance.Changes.SaveChanges();
            Core.OnLotLoaded?.Invoke();
            var simulator = Simulator.Instance;
            if (simulator != null)
                simulator.Kill();
            var lotSimulator = Simulator.Create(Simulator.Context.Lot);
        }
    }
}
