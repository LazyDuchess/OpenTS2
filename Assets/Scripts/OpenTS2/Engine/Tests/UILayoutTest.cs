using OpenTS2.Client;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Files.Formats.DBPF;
using OpenTS2.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine.Tests
{
    public class UILayoutTest : MonoBehaviour
    {
        public Languages Language = Languages.USEnglish;
        public string Key = "0x49001017";
        public bool Reload = false;
        public bool LoadPackagesFromAllEPs = true;
        private readonly string RelativeUIPackagePath = "Res/UI/ui.package";
        private List<UIComponent> _instances = new List<UIComponent>();
        void LoadAllUIPackages()
        {
            EPManager.Get().InstalledProducts = 0x3EFFF;
            ContentLoading.LoadContentStartup();
        }

        void LoadBGUIPackage()
        {
            EPManager.Get().InstalledProducts = (int)ProductFlags.BaseGame;
            ContentLoading.LoadContentStartup();
        }

        void CreateUI()
        {
            try
            {
                var settings = Settings.Get();
                settings.Language = Language;
                foreach (var instance in _instances)
                {
                    Destroy(instance.gameObject);
                }
                _instances.Clear();
                var contentProvider = ContentProvider.Get();
                var key = new ResourceKey(Convert.ToUInt32(Key, 16), 0xA99D8A11, TypeIDs.UI);
                var uiLayout = contentProvider.GetAsset<UILayout>(key);
                _instances.AddRange(uiLayout.Instantiate(UIManager.MainCanvas.transform));
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var currentCursor = CursorController.Cursor;
                currentCursor++;
                if ((int)currentCursor >= Enum.GetValues(typeof(CursorController.CursorType)).Length)
                    currentCursor = 0;
                CursorController.Cursor = currentCursor;
            }
            if (Reload)
            {
                CreateUI();
                Reload = false;
            }
        }

        private void Start()
        {
            var settings = Settings.Get();
            settings.CustomContentEnabled = false;
            if (LoadPackagesFromAllEPs)
                LoadAllUIPackages();
            else
                LoadBGUIPackage();
            CreateUI();
            /*
            var contentProvider = ContentProvider.Get();
            // Main Menu
            //var key = new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);
            // Neighborhood View
            var key = new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001010, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49060005, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001024, 0xA99D8A11, TypeIDs.UI);
            var mainMenuUILayout = contentProvider.GetAsset<UILayout>(key);
            _instances.AddRange(mainMenuUILayout.Instantiate(Canvas));*/
        }
    }
}
