using OpenTS2.Content;
using OpenTS2.Files;
using OpenTS2.Files.Formats.Font;
using OpenTS2.Files.Formats.Ini;
using OpenTS2.UI.Skia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI
{
    public class FontStyles
    {
        public static FontStyles Instance { get; private set; }
        private static Dictionary<string, SkiaFont> _skiaFontByName = new Dictionary<string, SkiaFont>();
        private static Dictionary<string, FontStyle> _fontStyleByName = new Dictionary<string, FontStyle>();

        public FontStyles()
        {
            Instance = this;
            LoadFonts();
            LoadFontStyles();
        }

        public FontStyle GetFontStyle(string name)
        {
            if (_fontStyleByName.TryGetValue(name, out var result))
                return result;
            return _fontStyleByName["Default"];
        }

        private string GetValue(string val)
        {
            return val.Trim().Replace("\"", "");
        }

        private void LoadFontStyles()
        {
            var fontStylePath = Filesystem.GetLatestFilePath("Res/UI/Fonts/FontStyle.ini");
            if (string.IsNullOrEmpty(fontStylePath)) return;
            var iniFile = new IniFile(fontStylePath);
            var fontStyles = iniFile.GetSection("Font Styles");
            foreach(var keyValue in fontStyles.KeyValues)
            {
                var values = keyValue.Value.Split(',');

                var style = keyValue.Key;
                var fontFamily = GetValue(values[0]);
                var fontSize = int.Parse(GetValue(values[1]));
                var fontModifiers = GetValue(values[2]);
                var parsedFontModifiers = fontModifiers.Split('|');

                var fontName = fontFamily;
                if (parsedFontModifiers.Contains("bold"))
                    fontName += " (bold)";

                var fontStyle = new FontStyle(_skiaFontByName[fontName], fontSize);

                _fontStyleByName[style] = fontStyle;
            }
        }

        private void LoadFonts()
        {
            var epManager = EPManager.Instance;
            var products = Filesystem.GetProductDirectories();

            foreach(var product in products)
            {
                var fontsPath = Path.Combine(product, "TSData/Res/UI/Fonts");
                if (!Directory.Exists(fontsPath)) continue;
                var fontFiles = Directory.GetFiles(fontsPath, "*.*", SearchOption.TopDirectoryOnly);
                foreach(var fontFile in fontFiles)
                {
                    var extension = Path.GetExtension(fontFile).ToLowerInvariant();
                    byte[] fontData = null;
                    switch (extension)
                    {
                        case ".mxf":
                            var mxf = new MXFFile(fontFile);
                            fontData = mxf.DecodedData;
                            if (PFBFontConverter.IsPFB(fontData))
                                fontData = PFBFontConverter.ConvertData(fontData);
                            break;

                        case ".pfb":
                            fontData = PFBFontConverter.ConvertData(File.ReadAllBytes(fontFile));
                            break;

                        case ".otf":
                        case ".ttf":
                            fontData = File.ReadAllBytes(fontFile);
                            break;
                    }
                    if (fontData != null)
                    {
                        _skiaFontByName[Path.GetFileNameWithoutExtension(fontFile)] = new SkiaFont(fontData);
                    }
                }
            }
        }
    }
}
