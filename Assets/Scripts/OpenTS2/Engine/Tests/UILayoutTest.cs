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
        public bool LoadPackagesFromAllEPs = true;
        [HideInInspector]
        public int CurrentLayout = 0;
        public List<ResourceKey> UILayouts;
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

        private void LoadUILayouts()
        {
            UILayouts = ContentManager.Instance.ResourceMap.Keys.Where(x => {
                if (x.TypeID != TypeIDs.UI)
                    return false;
                var bytes = ContentManager.Instance.GetEntry(x).GetBytes();
                var magic = Encoding.UTF8.GetString(bytes, 0, 4);
                if (magic == "RIFF")
                    return false;
                return true;
            }).ToList();
            UILayouts.Sort((k1, k2) => k1.InstanceID.CompareTo(k2.InstanceID));
        }

        public void Next()
        {
            CurrentLayout++;
            if (CurrentLayout >= UILayouts.Count)
                CurrentLayout = 0;
            CreateUI();
        }

        public void Previous()
        {
            CurrentLayout--;
            if (CurrentLayout < 0)
                CurrentLayout = UILayouts.Count - 1;
            CreateUI();
        }

        void CreateUI()
        {
            var canvas = UIManager.MainCanvas.transform;
            var canvasChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach(var child in canvasChildren)
            {
                if (child == canvas) continue;
                Destroy(child.gameObject);
            }
            var globals = GameGlobals.Instance;
            globals.Language = Language;
            var key = UILayouts[CurrentLayout];
            var contentManager = ContentManager.Instance;
            var uiLayout = contentManager.GetAsset<UILayout>(key);
            uiLayout.Instantiate(UIManager.MainCanvas.transform);
        }

        private void Start()
        {
            GameGlobals.allowCustomContent = false;
            if (LoadPackagesFromAllEPs)
                LoadAllUIPackages();
            else
                LoadBGUIPackage();
            LoadUILayouts();
            Core.OnFinishedLoading?.Invoke();
            CreateUI();
        }
    }
}
