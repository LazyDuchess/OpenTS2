using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.UI.Skia
{
    public static class PFBFontConverter
    {
        private const string TXPath = "tx.exe";
        private const string MakeOTFPath = "makeotfexe.exe";
        public static bool Convert(string pfbPath, string otfPath)
        {
            var proc = Process.Start(TXPath, $"-cff -Z +b \"{pfbPath}\" temp.font");
            proc.WaitForExit();
            proc = Process.Start(TXPath, "-t1 temp.font temp.font2");
            proc.WaitForExit();
            proc = Process.Start(MakeOTFPath, $"-f temp.font2 -o \"{otfPath}\"");
            proc.WaitForExit();

            if (File.Exists("temp.font"))
                File.Delete("temp.font");
            if (File.Exists("temp.font2"))
                File.Delete("temp.font2");

            return File.Exists(otfPath);
        }
    }
}
