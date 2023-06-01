using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace OpenTS2.UI
{
    public class UIButtonComponent : UIComponent, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Triggers when this button is clicked. Boolean argument specifies whether it was a double click.
        /// </summary>
        public Action OnClick;
        public bool GreyedOut = false;
        private bool _hovering = false;
        private Texture2D[] _textures;
        public RawImage RawImageComponent => GetComponent<RawImage>();
        public void SetTexture(Texture2D texture)
        {
            if (texture == null)
                return;
            _textures = UIUtils.SplitTextureHorizontalSequence(texture, texture.width / 4);
            UpdateTexture();
        }

        void Update()
        {
            if (UIManager.HeldComponent == this && GreyedOut)
                UIManager.HeldComponent = null;
            UpdateTexture();
        }

        void UpdateTexture()
        {
            if (_textures == null || _textures.Length < 4)
                return;
            if (GreyedOut)
                RawImageComponent.texture = _textures[0];
            else
            {
                RawImageComponent.texture = _textures[1];
                if (UIManager.HeldComponent == this)
                {
                    RawImageComponent.texture = _textures[2];
                    return;
                }
                else
                {
                    if (UIManager.HeldComponent != null)
                        return;
                }
                if (_hovering)
                    RawImageComponent.texture = _textures[3];
            }
        }

        private void OnDisable()
        {
            if (UIManager.HeldComponent == this)
                UIManager.HeldComponent = null;
            _hovering = false;
        }

        private void OnEnable()
        {
            if (UIManager.HeldComponent == this)
                UIManager.HeldComponent = null;
            _hovering = false;
        }

        private void OnDestroy()
        {
            if (UIManager.HeldComponent == this)
                UIManager.HeldComponent = null;
            if (_textures == null)
                return;
            foreach (var texture in _textures)
            {
                if (texture != null)
                    texture.Free();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UIManager.HeldComponent = this;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (UIManager.HeldComponent == this && _hovering)
            {
                Click();
            }
        }

        void Click()
        {
            OnClick?.Invoke();
        }
    }
}
