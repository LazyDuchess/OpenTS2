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
        public Transform Canvas;
        void LoadAllUIPackages()
        {
            var contentProvider = ContentProvider.Get();
            var relativePackagePath = "Res/UI/ui.package";
            var products = EPManager.Get().GetInstalledProducts();
            foreach(var product in products)
            {
                var fullPath = Path.Combine(Filesystem.PathProvider.GetDataPathForProduct(product), relativePackagePath);
                contentProvider.AddPackage(fullPath);
            }
        }
        private void Start()
        {
            LoadAllUIPackages();
            var contentProvider = ContentProvider.Get();
            // Main Menu
            var key = new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);
            // Neighborhood View
            //var key = new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001010, 0xA99D8A11, TypeIDs.UI);
            var mainMenuUILayout = contentProvider.GetAsset<UILayout>(key);
            mainMenuUILayout.Instantiate(Canvas);
        }
    }
}
