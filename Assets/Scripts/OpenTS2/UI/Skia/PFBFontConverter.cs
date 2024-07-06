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
        private const string TXPath = "afdko/tx.exe";
        private const string MakeOTFPath = "afdko/makeotfexe.exe";
        public static bool ConvertFiles(string pfbPath, string otfPath)
        {
            var txPath = Path.Combine(Environment.CurrentDirectory, TXPath);
            var makeOtfPath = Path.Combine(Environment.CurrentDirectory, MakeOTFPath);
            if (!File.Exists(txPath) || !File.Exists(makeOtfPath))
                return false;
            var proc = Process.Start(txPath, $"-cff -Z +b \"{pfbPath}\" temp.font");
            proc.WaitForExit();
            proc = Process.Start(txPath, "-t1 temp.font temp.font2");
            proc.WaitForExit();
            proc = Process.Start(makeOtfPath, $"-f temp.font2 -o \"{otfPath}\"");
            proc.WaitForExit();

            if (File.Exists("temp.font"))
                File.Delete("temp.font");
            if (File.Exists("temp.font2"))
                File.Delete("temp.font2");

            return File.Exists(otfPath);
        }

        public static byte[] ConvertData(byte[] pfbData)
        {
            byte[] output = null;
            var sourcePath = Path.Combine(Environment.CurrentDirectory, "afdko/temp_input.pfb");
            var outputPath = Path.Combine(Environment.CurrentDirectory, "afdko/temp_output.otf");

            File.WriteAllBytes(sourcePath, pfbData);

            if (ConvertFiles(sourcePath, outputPath))
                output = File.ReadAllBytes(outputPath);

            if (File.Exists(sourcePath))
                File.Delete(sourcePath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);

            return output;
        }
    }
}
