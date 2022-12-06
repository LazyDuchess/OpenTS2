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

        public bool reload = false;
        public bool stream = true;

        ReiaFile reia = null;
        float frameCounter = 0f;

        public float speedMultiplier = 1f;

        private void Start()
        {
            var streamFs = Filesystem.OpenRead(reiaPath);
            reia = ReiaFile.Read(streamFs, stream);
        }

        void Reload()
        {
            reia.Dispose();
            var streamFs = Filesystem.OpenRead(reiaPath);
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
            for(var i=0;i<framesPast;i++)
            {
                reia.MoveNextFrame();
            }
            frameCounter -= framesPast / reia.FramesPerSecond;
            image.texture = reia.GetCurrentFrame().Image;
        }
    }
}
