using OpenTS2.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OpenTS2.Content.DBPF;

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
        private bool _held = false;
        private TextureAsset[] _textures;
        public RawImage RawImageComponent => GetComponent<RawImage>();
        public void SetTexture(TextureAsset texture)
        {
            if (texture == null)
                return;
            var _textureSequence = UIUtils.SplitTextureHorizontalSequence(texture.Texture, texture.Texture.width / 4);
            _textures = new TextureAsset[4];
            for(var i=0;i<4;i++)
            {
                _textures[i] = new TextureAsset(_textureSequence[i]);
            }
            UpdateTexture();
        }

        void Update()
        {
            UpdateTexture();
        }

        void UpdateTexture()
        {
            if (_textures == null || _textures.Length < 4)
                return;
            if (GreyedOut)
                RawImageComponent.texture = _textures[0].Texture;
            else
            {
                RawImageComponent.texture = _textures[1].Texture;
                if (_hovering && !UIManager.AnyMouseButtonHeld)
                    RawImageComponent.texture = _textures[3].Texture;
                if (_held)
                    RawImageComponent.texture = _textures[2].Texture;
            }
        }

        private void OnDisable()
        {
            _hovering = false;
            _held = false;
        }

        private void OnEnable()
        {
            _hovering = false;
            _held = false;
            UpdateTexture();
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
            if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right)
                return;
            _held = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left && eventData.button != PointerEventData.InputButton.Right)
                return;
            if (_held && _hovering)
            {
                Click();
            }
            _held = false;
        }

        void Click()
        {
            OnClick?.Invoke();
        }
    }
}
