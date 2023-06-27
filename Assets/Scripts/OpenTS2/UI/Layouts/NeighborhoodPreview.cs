using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI.Layouts
{
    /// <summary>
    /// Neighborhood preview, displayed in main menu upon clicking a neighborhood icon.
    /// </summary>
    public class NeighborhoodPreview : UILayoutInstance
    {
        public Action OnTryEnterActiveNeighborhood;
        public Action OnHide;
        public Action OnShow;
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49001021, 0xA99D8A11, TypeIDs.UI);
        private const uint PlayButtonID = 0xB2;
        private const uint CloseButtonID = 0xB3;
        private const uint DescriptionTextEditID = 0x2002;
        private const uint ThumbnailBMPID = 0x2009;
        private const uint NameTextID = 0x2003;
        private Neighborhood _neighborhood;

        public NeighborhoodPreview() : this(MainCanvas)
        {

        }

        public NeighborhoodPreview(Transform parent) : base(parent)
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Center);
            var playButton = root.GetChildByID<UIButtonComponent>(PlayButtonID);
            var closeButton = root.GetChildByID<UIButtonComponent>(CloseButtonID);
            var description = root.GetChildByID<UITextEditComponent>(DescriptionTextEditID);
            playButton.OnClick += Play;
            closeButton.OnClick += Hide;
            description.OnTextEdited += OnDescriptionEdited;
        }

        void OnDescriptionEdited(string newName)
        {
            if (_neighborhood == null)
                return;
            _neighborhood.SetDescription(newName);
        }

        public void SetNeighborhood(Neighborhood neighborhood)
        {
            _neighborhood = neighborhood;
            var root = Components[0];
            var name = root.GetChildByID<UITextComponent>(NameTextID);
            var description = root.GetChildByID<UITextEditComponent>(DescriptionTextEditID);
            var thumbnail = root.GetChildByID<UIBMPComponent>(ThumbnailBMPID);
            name.Text = _neighborhood.GetLocalizedName();
            description.Text = _neighborhood.GetLocalizedDescription();
            thumbnail.Texture = _neighborhood.Thumbnail;
            thumbnail.Color = Color.white;
        }

        void Play()
        {
            if (_neighborhood == null)
                return;
            if (NeighborhoodManager.CurrentNeighborhood == _neighborhood)
            {
                Hide();
                OnTryEnterActiveNeighborhood?.Invoke();
                return;
            }
            NeighborhoodManager.EnterNeighborhood(_neighborhood);
        }

        public void Hide()
        {
            Components[0].gameObject.SetActive(false);
            OnHide?.Invoke();
        }

        public void Show()
        {
            Components[0].gameObject.SetActive(true);
            OnShow?.Invoke();
        }
    }
}
