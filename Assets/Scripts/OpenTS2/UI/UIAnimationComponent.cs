using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenTS2.Engine;

namespace OpenTS2.UI
{
    /// <summary>
    /// Animated UI element, amount of frames is texture width divided by Area width.
    /// </summary>
    public class UIAnimationComponent : UIBMPComponent
    {
        public float Speed = 0.04f;
        public int CurrentFrame { get
            {
                return _currentFrame;
            }
            set
            {
                _currentFrame = value;
                UpdateFrame();
            }
        }
        private int _currentFrame = 0;
        private Texture2D[] _frames;
        private float _timer = 0f;

        /// <summary>
        /// Set this UI element's animated texture.
        /// </summary>
        /// <param name="texture">Texture</param>
        /// <param name="width">Width of each frame</param>
        public void SetTexture(Texture2D texture, int width)
        {
            DisposeFrames();
            _frames = UIUtils.SplitTextureHorizontalSequence(texture, width);
            _currentFrame = 0;
            _timer = 0f;
            UpdateFrame();
        }
        private void OnDestroy()
        {
            DisposeFrames();
        }
        protected void DisposeFrames()
        {
            if (_frames == null)
                return;
            foreach(var frame in _frames)
            {
                frame.Free();
            }
        }
        protected void UpdateFrame()
        {
            if (_currentFrame >= _frames.Length)
                _currentFrame = _frames.Length - 1;
            if (_currentFrame < 0)
                _currentFrame = 0;
            var rawImage = RawImageComponent;
            rawImage.texture = _frames[_currentFrame];
        }

        private void Update()
        {
            UpdateFrame();
            if (Speed <= 0f)
                return;
            _timer += Time.deltaTime;
            if (_timer >= Speed)
            {
                var amountPassed = Mathf.FloorToInt(_timer / Speed);
                _currentFrame += amountPassed;
                _timer -= amountPassed * Speed;
            }
            if (_currentFrame >= _frames.Length)
                _currentFrame -= Mathf.FloorToInt(_currentFrame / _frames.Length) * _frames.Length;
        }
    }
}
