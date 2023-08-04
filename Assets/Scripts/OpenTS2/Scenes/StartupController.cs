using OpenTS2.Audio;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.Game;
using OpenTS2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using OpenTS2.UI.Layouts;

namespace OpenTS2.Scenes
{
    public class StartupController : MonoBehaviour
    {
        public static bool GameLoaded = false;
        public float FadeOutTime = 1f;
        public UIBMPComponent InitialLoadScreenBackgroundImage;
        public UIBMPComponent InitialLoadScreenLogoImage;
        public ReiaPlayer InitialLoadScreenReiaPlayer;
        public bool EnableReia = true;
        public bool LoadObjects = true;
        public bool StreamReia = true;
        private ResourceKey _initialLoadScreenReiaKey = new ResourceKey(0x8DA3ADE7, 0x499DB772, TypeIDs.UI);
        private ResourceKey _initialLoadScreenBackgroundKey = new ResourceKey(0xCCC9AF80, 0x499DB772, TypeIDs.IMG);
        private ResourceKey _initialLoadScreenLogoKey = new ResourceKey(0x8CBB9323, 0x499DB772, TypeIDs.IMG);
        private LoadProgress _loadProgress = new LoadProgress();
        private int _currentReiaFrame = 0;

        private void Start()
        {
            if (!GameLoaded)
            {
                var contentProvider = ContentProvider.Get();
                ContentLoading.LoadContentStartup();
                PluginSupport.Initialize();
                if (EnableReia)
                {
                    if (InitialLoadScreenReiaPlayer != null)
                    {
                        InitialLoadScreenReiaPlayer.SetReia(_initialLoadScreenReiaKey, StreamReia);
                        InitialLoadScreenReiaPlayer.Speed = 0f;
                    }
                }
                else
                    InitialLoadScreenReiaPlayer.gameObject.SetActive(false);
                if (InitialLoadScreenBackgroundImage != null)
                {
                    var bgAsset = contentProvider.GetAsset<TextureAsset>(_initialLoadScreenBackgroundKey);
                    if (bgAsset != null)
                    {
                        InitialLoadScreenBackgroundImage.SetTexture(bgAsset);
                        InitialLoadScreenBackgroundImage.SetNativeSize();
                    }
                }
                if (InitialLoadScreenLogoImage != null)
                {
                    var fgAsset = contentProvider.GetAsset<TextureAsset>(_initialLoadScreenLogoKey);
                    if (fgAsset != null)
                    {
                        InitialLoadScreenLogoImage.SetTexture(fgAsset);
                        InitialLoadScreenLogoImage.SetNativeSize();
                    }
                }
                ContentLoading.LoadGameContentAsync(_loadProgress).ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                OnFinishLoading();
                if (InitialLoadScreenBackgroundImage != null)
                    Destroy(InitialLoadScreenBackgroundImage.gameObject);
                if (InitialLoadScreenLogoImage != null)
                    Destroy(InitialLoadScreenLogoImage.gameObject);
                if (InitialLoadScreenReiaPlayer != null)
                    Destroy(InitialLoadScreenReiaPlayer.gameObject);
            }
            MusicController.PlayMusic(AudioManager.SplashAudio);
        }

        private void Update()
        {
            if (InitialLoadScreenReiaPlayer != null)
            {
                var reia = InitialLoadScreenReiaPlayer.Reia;
                if (reia != null)
                {
                    var targetFrame = Mathf.FloorToInt(_loadProgress.Percentage * (reia.NumberOfFrames - 1));
                    if (targetFrame > _currentReiaFrame)
                    {
                        var difference = targetFrame - _currentReiaFrame;
                        _currentReiaFrame = targetFrame;
                        for(var i=0;i<difference;i++)
                        {
                            reia.MoveNextFrame();
                        }
                    }
                }
            }
        }

        private void OnFinishLoading()
        {
            CursorController.Cursor = CursorController.CursorType.Hourglass;
            if (LoadObjects && !GameLoaded)
            {
                var oMgr = ObjectManager.Get();
                oMgr.Initialize();

                EffectsManager.Get().Initialize();
            }
            if (!GameLoaded)
                NeighborhoodManager.Initialize();
            CursorController.Cursor = CursorController.CursorType.Default;
            Debug.Log("All loaded");
            FadeOutLoading();
            try
            {
                var mainMenu = new MainMenu();
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
            GameLoaded = true;
        }

        void FadeOutLoading()
        {
            if (InitialLoadScreenBackgroundImage != null)
                StartCoroutine(FadeOut(InitialLoadScreenBackgroundImage.RawImageComponent, FadeOutTime));
            if (InitialLoadScreenLogoImage != null)
                StartCoroutine(FadeOut(InitialLoadScreenLogoImage.RawImageComponent, FadeOutTime));
            if (InitialLoadScreenReiaPlayer != null)
                StartCoroutine(FadeOut(InitialLoadScreenReiaPlayer.GetComponent<RawImage>(), FadeOutTime));
        }

        IEnumerator FadeOut(RawImage image, float seconds)
        {
            var color = image.color;
            while (color.a > 0f)
            {
                color.a -= seconds * Time.deltaTime;
                image.color = color;
                yield return null;
            }
            Destroy(image.gameObject);
        }
    }
}
