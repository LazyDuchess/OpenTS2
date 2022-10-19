using OpenTS2.Content;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Engine.Tests
{
    public class AsyncTest : MonoBehaviour
    {
        LoadProgress progress = new LoadProgress();

        public Text progressText;
        public Text objectsText;

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
                asyncTask = ContentManager.Provider.AddPackagesAsync(packageList, progress).ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            else
            {
                foreach(var element in packageList)
                {
                    ContentManager.Provider.AddPackage(element);
                }
                OnFinishLoading();
            }
        }

        void Update()
        {
            progressText.text = Mathf.Round(progress.percentage * 100f).ToString() + "%";
        }

        void OnFinishLoading()
        {
            var oMgr = ObjectManager.Get();
            oMgr.Initialize();
            stopW.Stop();
            UnityEngine.Debug.Log("Done loading packages!");
            UnityEngine.Debug.Log(ContentManager.Provider.ContentEntries.Count + " packages loaded.");
            UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
            var objectStr = "Object Amount: " + oMgr.Objects.Count + System.Environment.NewLine;
            for(var i=0;i<200;i++)
            {
                if (i >= oMgr.Objects.Count)
                    break;
                var element = oMgr.Objects[i];
                objectStr += element.Definition.filename + " (" + "0x"+element.GUID.ToString("X8") + ")" + System.Environment.NewLine;
            }
            objectsText.text = objectStr;
        }
    }
}