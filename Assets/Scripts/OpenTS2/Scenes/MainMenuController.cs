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
using OpenTS2.Lua;
using OpenTS2.Engine;

namespace OpenTS2.Scenes
{
    public class MainMenuController : MonoBehaviour
    {
        public float FadeOutTime = 1f;
        public UIBMPComponent InitialLoadScreenBackgroundImage;
        public UIBMPComponent InitialLoadScreenLogoImage;
        public ReiaPlayer InitialLoadScreenReiaPlayer;
        private ResourceKey _initialLoadScreenReiaKey = new ResourceKey(0x8DA3ADE7, 0x499DB772, TypeIDs.UI);
        private ResourceKey _initialLoadScreenBackgroundKey = new ResourceKey(0xCCC9AF80, 0x499DB772, TypeIDs.IMG);
        private ResourceKey _initialLoadScreenLogoKey = new ResourceKey(0x8CBB9323, 0x499DB772, TypeIDs.IMG);
        private LoadProgress _loadProgress = new LoadProgress();
        private int _currentReiaFrame = 0;

        private void Start()
        {
            var contentManager = ContentManager.Instance;
            ContentLoading.LoadContentStartup();
            PluginSupport.Initialize();
            if (InitialLoadScreenReiaPlayer != null)
            {
                InitialLoadScreenReiaPlayer.SetReia(_initialLoadScreenReiaKey, true);
                InitialLoadScreenReiaPlayer.Speed = 0f;
            }
            if (InitialLoadScreenBackgroundImage != null)
            {
                var bgAsset = contentManager.GetAsset<TextureAsset>(_initialLoadScreenBackgroundKey);
                if (bgAsset != null)
                {
                    InitialLoadScreenBackgroundImage.SetTexture(bgAsset);
                    InitialLoadScreenBackgroundImage.SetNativeSize();
                }
            }
            if (InitialLoadScreenLogoImage != null)
            {
                var fgAsset = contentManager.GetAsset<TextureAsset>(_initialLoadScreenLogoKey);
                if (fgAsset != null)
                {
                    InitialLoadScreenLogoImage.SetTexture(fgAsset);
                    InitialLoadScreenLogoImage.SetNativeSize();
                }
            }
            ContentLoading.LoadGameContentAsync(_loadProgress).ContinueWith((task) => { OnFinishLoading(); }, TaskScheduler.FromCurrentSynchronizationContext());
            Core.OnBeginLoading?.Invoke();
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
            try
            {
                CursorController.Cursor = CursorController.CursorType.Hourglass;
                Core.OnFinishedLoading?.Invoke();
                CursorController.Cursor = CursorController.CursorType.Default;
                FadeOutLoading();
                var mainMenu = new MainMenu();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
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
