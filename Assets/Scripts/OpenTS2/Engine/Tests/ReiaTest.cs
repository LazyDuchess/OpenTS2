using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Files;
using OpenTS2.Files.Formats.Reia;

namespace OpenTS2.Engine.Tests
{
    public class ReiaTest : MonoBehaviour
    {
        public string reiaPath;
        public RawImage image;

        ReiaFile reia = null;
        float frameCounter = 0f;
        int currentFrame = 0;
        private void Start()
        {
            var stream = Filesystem.OpenRead(reiaPath);
            reia = ReiaFile.Read(stream);
            //image.texture = reia.Frames[0].Image;
        }

        private void Update()
        {
            if (reia == null)
                return;
            var frameDelta = 1f / reia.FramesPerSecond;
            if (frameCounter > frameDelta)
            {
                frameCounter = 0f;
                currentFrame++;
                if (currentFrame >= reia.Frames.Count)
                    currentFrame = 0;
            }
            else
                frameCounter += Time.deltaTime;
            image.texture = reia.Frames[currentFrame].Image;
        }
    }
}
