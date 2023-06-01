using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.Reia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
    [RequireComponent(typeof(RawImage))]
    public class ReiaPlayer : MonoBehaviour
    {
        public float Speed = 1f;
        public ReiaFile Reia => _reia;
        private ReiaFile _reia;
        private RawImage _rawImage;
        private float _frameCounter = 0f;

        private void Start()
        {
            _rawImage = GetComponent<RawImage>();
        }

        public void Stop()
        {
            _reia?.Dispose();
            _reia = null;
            Speed = 0f;
        }

        public void SetReia(Stream stream, bool streamed)
        {
            _reia = ReiaFile.Read(stream, streamed);
            _frameCounter = 0f;
        }

        public void SetReia(ResourceKey key, bool stream)
        {
            var contentProvider = ContentProvider.Get();
            var bytes = contentProvider.GetEntry(key).GetBytes();
            var memStream = new MemoryStream(bytes);
            SetReia(memStream, stream);
        }

        private void Update()
        {
            if (_reia == null)
                return;
            _frameCounter += Time.deltaTime * Speed;
            var framesPast = Mathf.Floor(_frameCounter * _reia.FramesPerSecond);
            _frameCounter -= framesPast / _reia.FramesPerSecond;
            for (var i = 0; i < framesPast; i++)
            {
                _reia.MoveNextFrame();
            }
            _rawImage.texture = _reia.GetCurrentFrame().Image;
        }

        private void OnDestroy()
        {
            _reia?.Dispose();
        }
    }
}
