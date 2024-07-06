using OpenTS2.Content;
using OpenTS2.Engine;
using OpenTS2.Game;
using OpenTS2.UI.Layouts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenTS2.Scenes
{
    public class NeighborhoodController : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Core.OnNeighborhoodEntered?.Invoke();
            var hud = new NeighborhoodHUD();
            var simulator = Simulator.Instance;
            if (simulator != null)
                simulator.Kill();
            var nhoodSimulator = Simulator.Create(Simulator.Context.Neighborhood);
        }
    }
}
