using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Files;
using OpenTS2.Files.Formats.Reia;
using System.Diagnostics;

namespace OpenTS2.Engine.Tests
{
    public class ReiaTest : MonoBehaviour
    {
        public string reiaPath;
        public RawImage image;

        ReiaFile reia = null;
        Stopwatch frameCounter;

        private void Start()
        {
            var stream = Filesystem.OpenRead(reiaPath);
            reia = ReiaFile.Read(stream);
            frameCounter = new Stopwatch();
            frameCounter.Start();
        }

        private void Update()
        {
            if (reia == null)
                return;
            var frame = (int)Mathf.Floor((float)frameCounter.Elapsed.TotalSeconds * reia.FramesPerSecond);
            frame -= (int)(Mathf.Floor((float)frame / reia.Frames.Count) * reia.Frames.Count);
            image.texture = reia.Frames[frame].Image;
        }
    }
}
