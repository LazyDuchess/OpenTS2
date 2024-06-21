using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace OpenTS2
{
    public class SkiaBuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var buildPath = Path.GetDirectoryName(report.summary.outputPath);
            var projectName = PlayerSettings.productName;
            var pluginsPath = Path.Combine(buildPath, $"{projectName}_Data", "Plugins");
            var skiaPaths = Directory.GetFiles(pluginsPath, "libSkiaSharp.dll", SearchOption.AllDirectories);
            
            if (skiaPaths.Length <= 0)
                return;

            var targetPath = Path.Combine(buildPath, "libSkiaSharp.dll");

            if (File.Exists(targetPath))
                File.Delete(targetPath);
            File.Move(skiaPaths[0], targetPath);
        }
    }
}
