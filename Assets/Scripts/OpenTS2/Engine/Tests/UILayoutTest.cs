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
        private List<UIComponent> _instances = new List<UIComponent>();
        void LoadAllUIPackages()
        {
            EPManager.Instance.InstalledProducts = 0x3EFFF;
            ContentLoading.LoadContentStartup();
        }

        void LoadBGUIPackage()
        {
            EPManager.Instance.InstalledProducts = (int)ProductFlags.BaseGame;
            ContentLoading.LoadContentStartup();
        }

        void CreateUI()
        {
            var globals = GameGlobals.Instance;
            globals.Language = Language;
            foreach (var instance in _instances)
            {
                Destroy(instance.gameObject);
            }
            _instances.Clear();
            var contentManager = ContentManager.Instance;
            var key = new ResourceKey(Convert.ToUInt32(Key, 16), 0xA99D8A11, TypeIDs.UI);
            var uiLayout = contentManager.GetAsset<UILayout>(key);
            _instances.AddRange(uiLayout.Instantiate(UIManager.MainCanvas.transform));
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
            GameGlobals.allowCustomContent = false;
            if (LoadPackagesFromAllEPs)
                LoadAllUIPackages();
            else
                LoadBGUIPackage();
            Core.OnFinishedLoading?.Invoke();
            CreateUI();
            /*
            var contentManager = ContentManager.Get();
            // Main Menu
            //var key = new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);
            // Neighborhood View
            var key = new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001010, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49060005, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001024, 0xA99D8A11, TypeIDs.UI);
            var mainMenuUILayout = contentManager.GetAsset<UILayout>(key);
            _instances.AddRange(mainMenuUILayout.Instantiate(Canvas));*/
        }
    }
}
