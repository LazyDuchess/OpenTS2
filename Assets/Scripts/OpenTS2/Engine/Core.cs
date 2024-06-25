using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    public class Core : MonoBehaviour
    {
        public static Action OnBeginLoadingScreen;
        public static Action OnFinishedLoading;
        public static Action OnNeighborhoodEntered;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
