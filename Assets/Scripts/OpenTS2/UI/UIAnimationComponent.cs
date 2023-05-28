using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI
{
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
        public void SetTexture(Texture2D texture, int width)
        {
            _frames = UIUtils.SplitTextureHorizontalSequence(texture, width);
            _currentFrame = 0;
            _timer = 0f;
            UpdateFrame();
            var rawImage = RawImageComponent;
            rawImage.SetNativeSize();
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
