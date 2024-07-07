using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI.Skia
{
    // TODO - Include builds for different architectures.
    public static class PFBFontConverter
    {
        private const string TXPath = "afdko/tx.exe";
        private const string MakeOTFPath = "afdko/makeotfexe.exe";
        private static string TempPath = Path.Combine(Application.persistentDataPath, "Temp");
        
        private static void RunProcess(string path, string args)
        {
            var proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();
        }
        
        public static bool ConvertFiles(string pfbPath, string otfPath)
        {
            var txPath = Path.Combine(Environment.CurrentDirectory, TXPath);
            var makeOtfPath = Path.Combine(Environment.CurrentDirectory, MakeOTFPath);
            if (!File.Exists(txPath) || !File.Exists(makeOtfPath))
                return false;
            Directory.CreateDirectory(TempPath);
            var temp1Path = Path.Combine(TempPath, "temp.font");
            var temp2Path = Path.Combine(TempPath, "temp.font2");
            RunProcess(txPath, $"-cff -Z +b \"{pfbPath}\" \"{temp1Path}\"");
            RunProcess(txPath, $"-t1 \"{temp1Path}\" \"{temp2Path}\"");
            RunProcess(makeOtfPath, $"-f \"{temp2Path}\" -o \"{otfPath}\"");

            if (File.Exists(temp1Path))
                File.Delete(temp1Path);
            if (File.Exists(temp2Path))
                File.Delete(temp2Path);

            return File.Exists(otfPath);
        }

        public static byte[] ConvertData(byte[] pfbData)
        {
            Directory.CreateDirectory(TempPath);
            byte[] output = null;
            var sourcePath = Path.Combine(TempPath, "temp_input.pfb");
            var outputPath = Path.Combine(TempPath, "temp_output.otf");

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
