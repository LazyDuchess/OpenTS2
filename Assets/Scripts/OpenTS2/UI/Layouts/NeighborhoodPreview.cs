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
        private Neighborhood _neighborhood;
        public Action OnHide;
        public Action OnShow;
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49001021, 0xA99D8A11, TypeIDs.UI);

        public NeighborhoodPreview() : this(MainCanvas)
        {

        }

        public NeighborhoodPreview(Transform parent) : base(parent)
        {
            var root = Components[0];
            root.SetAnchor(UIComponent.AnchorType.Center);
            var closeButton = root.GetChildByID<UIButtonComponent>(0xB3);
            var description = root.GetChildByID<UITextEditComponent>(0x2002);
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
            var name = root.GetChildByID<UITextComponent>(0x2003);
            var description = root.GetChildByID<UITextEditComponent>(0x2002);
            var thumbnail = root.GetChildByID<UIBMPComponent>(0x2009);
            name.Text = _neighborhood.GetLocalizedName();
            description.Text = _neighborhood.GetLocalizedDescription();
            thumbnail.Texture = _neighborhood.Thumbnail;
            thumbnail.Color = Color.white;
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
