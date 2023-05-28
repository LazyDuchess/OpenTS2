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
        private void Start()
        {
            var contentProvider = ContentProvider.Get();
            var uiPackageLocation = Path.Combine(Filesystem.PathProvider.GetDataPathForProduct(ProductFlags.BaseGame), "Res/UI/ui.package");
            contentProvider.AddPackage(uiPackageLocation);
            uiPackageLocation = Path.Combine(Filesystem.PathProvider.GetDataPathForProduct(ProductFlags.Mansion), "Res/UI/ui.package");
            contentProvider.AddPackage(uiPackageLocation);
            // Main Menu
            //var key = new ResourceKey(0x49001017, 0xA99D8A11, TypeIDs.UI);
            // Neighborhood View
            var key = new ResourceKey(0x49000000, 0xA99D8A11, TypeIDs.UI);
            //var key = new ResourceKey(0x49001010, 0xA99D8A11, TypeIDs.UI);
            var mainMenuUILayout = contentProvider.GetAsset<UILayout>(key);
            mainMenuUILayout.Instantiate(Canvas);
        }
    }
}
