using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenTS2.Engine
{
    public class Core : MonoBehaviour
    {
        public string TargetScene;
        public static Action OnBeginLoadingScreen;
        public static Action OnFinishedLoading;
        public static Action OnNeighborhoodEntered;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (!string.IsNullOrEmpty(TargetScene))
                SceneManager.LoadScene(TargetScene);
        }
    }
}
