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
using System.IO;

namespace OpenTS2.Engine.Tests
{
    public class ReiaTest : MonoBehaviour
    {
        public string reiaPath;
        public RawImage image;

        public bool reload = false;
        public bool stream = true;

        public bool oneAtATime = true;

        ReiaFile reia = null;
        float frameCounter = 0f;

        public float speedMultiplier = 1f;

        private void Start()
        {
            var realPath = Filesystem.GetRealPath(reiaPath);
            var streamFs = File.OpenRead(realPath);
            reia = ReiaFile.Read(streamFs, stream);
        }

        void Reload()
        {
            reia.Dispose();
            var realPath = Filesystem.GetRealPath(reiaPath);
            var streamFs = File.OpenRead(realPath);
            reia = ReiaFile.Read(streamFs, stream);
            reload = false;
            frameCounter = 0f;
        }

        private void Update()
        {
            if (reia == null)
                return;
            if (reload)
            {
                Reload();
                return;
            }
            frameCounter += Time.deltaTime * speedMultiplier;
            var framesPast = Mathf.Floor(frameCounter * reia.FramesPerSecond);
            frameCounter -= framesPast / reia.FramesPerSecond;
            if (oneAtATime && framesPast > 1f)
            {
                framesPast = 1f;
            }
            for(var i=0;i<framesPast;i++)
            {
                reia.MoveNextFrame();
            }
            image.texture = reia.GetCurrentFrame().Image;
        }
    }
}
