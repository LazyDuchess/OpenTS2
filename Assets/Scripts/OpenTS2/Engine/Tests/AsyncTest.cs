using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Engine.Audio;
using OpenTS2.Files;
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
        public AudioSource musicSource;
        public Text progressText;
        public Text objectsText;

        //public List<string> packageList;
        public bool async = true;
        Task asyncTask;
        Stopwatch stopW;
        // Start is called before the first frame update
        void Start()
        {
            var contentManager = ContentManager.Get();
            contentManager.LoadContentStartup();
            musicSource.clip = AudioManager.SplashAudio.Clip;
            musicSource.Play();
            stopW = new Stopwatch();
            stopW.Start();
            if (async)
                asyncTask = contentManager.LoadGameContentAsync().ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            else
            {
                contentManager.LoadGameContentSync();
                OnFinishLoading();
            }
        }

        void Update()
        {
            var contentManager = ContentManager.Get();
            progressText.text = Mathf.Round(contentManager.ContentLoadProgress.percentage * 100f).ToString() + "%";
        }

        void OnFinishLoading()
        {
            var contentManager = ContentManager.Get();
            var oMgr = ObjectManager.Get();
            oMgr.Initialize();
            stopW.Stop();
            UnityEngine.Debug.Log("Done loading packages!");
            UnityEngine.Debug.Log(contentManager.Provider.ContentEntries.Count + " packages loaded.");
            UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
            var objectStr = "Object Amount: " + oMgr.Objects.Count + System.Environment.NewLine;
            for(var i=0;i<200;i++)
            {
                if (i >= oMgr.Objects.Count)
                    break;
                var element = oMgr.Objects[(oMgr.Objects.Count-1)-i];
                objectStr += element.Definition.filename + " (" + "0x"+element.GUID.ToString("X8") + ")" + System.Environment.NewLine;
            }
            objectsText.text = objectStr;
        }
    }
}