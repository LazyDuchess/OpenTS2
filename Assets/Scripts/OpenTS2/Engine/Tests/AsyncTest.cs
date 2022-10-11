using OpenTS2.Content;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class AsyncTest : MonoBehaviour
    {
        public List<string> packageList;
        public bool async = true;
        Task asyncTask;
        Stopwatch stopW;
        // Start is called before the first frame update
        void Start()
        {
            stopW = new Stopwatch();
            stopW.Start();
            if (async)
                asyncTask = ContentManager.Get.Provider.AddPackagesAsync(packageList);
            else
            {
                foreach(var element in packageList)
                {
                    ContentManager.Get.Provider.AddPackage(element);
                }
                stopW.Stop();
                UnityEngine.Debug.Log("Done loading packages!");
                UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
            }
        }
        private void Update()
        {
            if (!async)
                return;
            if (asyncTask.IsCompleted)
            {
                stopW.Stop();
                UnityEngine.Debug.Log("Done loading packages!");
                UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
                async = false;
            }
        }
    }
}