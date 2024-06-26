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
using OpenTS2.Game;
using UnityEngine.UI;

namespace OpenTS2.Engine.Tests
{
    public class AsyncTest : MonoBehaviour
    {
        LoadProgress _loadProgress = new LoadProgress();

        public AudioSource MusicSource;
        public Text ProgressText;
        public Text ObjectsText;
        public RawImage PopupBackgroundImage;
        public RawImage BackgroundImage;

        //public List<string> packageList;
        public bool async = true;
        Stopwatch stopW;
        // Start is called before the first frame update
        void Start()
        {
            ContentLoading.LoadContentStartup();
            PluginSupport.Initialize();
            //MusicSource.clip = MusicManager.SplashAudio.Clip;
            //MusicSource.Play();
            stopW = new Stopwatch();
            stopW.Start();
            if (async)
                ContentLoading.LoadGameContentAsync(_loadProgress).ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            else
            {
                ContentLoading.LoadGameContentSync();
                OnFinishLoading();
            }
        }

        void Update()
        {
            ProgressText.text = Mathf.Round(_loadProgress.Percentage * 100f).ToString() + "%";
        }

        void OnFinishLoading()
        {
            var contentProvider = ContentProvider.Get();
            stopW.Stop();
            UnityEngine.Debug.Log("Done loading packages!");
            UnityEngine.Debug.Log(contentProvider.ContentEntries.Count + " packages loaded.");
            UnityEngine.Debug.Log("Package loading took " + (stopW.ElapsedTicks * 1000000 / Stopwatch.Frequency) + " microseconds");
            PopupBackgroundImage.texture = contentProvider.GetAsset<TextureAsset>(new ResourceKey(0xA9600400, 0x499DB772, 0x856DDBAC)).Texture;
            BackgroundImage.texture = contentProvider.GetAsset<TextureAsset>(new ResourceKey(0xCCC9AF70, 0x499DB772, 0x856DDBAC)).Texture;
        }
    }
}