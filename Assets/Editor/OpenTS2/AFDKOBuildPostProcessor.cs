using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace OpenTS2
{
    public class AFDKOBuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var buildPath = Path.GetDirectoryName(report.summary.outputPath);
            var afdkoFiles = Directory.GetFiles("afdko");
            var afdkoOutputPath = Path.Combine(buildPath, "afdko");
            if (Directory.Exists(afdkoOutputPath))
                Directory.Delete(afdkoOutputPath, true);
            var dir = Directory.CreateDirectory(afdkoOutputPath);
            foreach(var file in afdkoFiles)
            {
                File.Copy(file, Path.Combine(afdkoOutputPath, Path.GetFileName(file)));
            }
        }
    }
}
