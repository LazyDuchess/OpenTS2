using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI.Layouts
{
    /// <summary>
    /// Clickable icons for neighborhoods in main menu.
    /// </summary>
    public class NeighborhoodIcon : UILayoutInstance
    {
        public Action<Neighborhood> OnClick;
        private Neighborhood _neighborhood;
        protected override ResourceKey UILayoutResourceKey => new ResourceKey(0x49001018, 0xA99D8A11, TypeIDs.UI);
        public NeighborhoodIcon() : this(MainCanvas)
        {

        }
        public NeighborhoodIcon(Transform parent) : base(parent)
        {
            var button = Components[0].GetChildByID<UIButtonComponent>(0xA2);
            button.OnClick += OnButtonClick;
        }

        void OnButtonClick()
        {
            OnClick?.Invoke(_neighborhood);
        }

        public void SetNeighborhood(Neighborhood neighborhood)
        {
            _neighborhood = neighborhood;
            var root = Components[0];
            var thumbnailBMP = root.GetChildByID<UIBMPComponent>(0xA1);
            var nameText = root.GetChildByID<UITextComponent>(0xA5);
            nameText.Text = _neighborhood.GetLocalizedName();
            thumbnailBMP.Color = Color.white;
            thumbnailBMP.RawImageComponent.texture = _neighborhood.Thumbnail;
            var reiaPlayer = thumbnailBMP.gameObject.GetComponent<ReiaPlayer>();
            if (reiaPlayer == null)
                reiaPlayer = thumbnailBMP.gameObject.AddComponent<ReiaPlayer>();
            var reiaPath = _neighborhood.ReiaPath;
            reiaPlayer.Stop();
            FlipTransformForBMP(thumbnailBMP.RectTransformComponent);
            if (File.Exists(reiaPath))
            {
                FlipTransformForReia(thumbnailBMP.RectTransformComponent);
                var reiaStream = File.OpenRead(reiaPath);
                reiaPlayer.SetReia(reiaStream, true);
                reiaPlayer.Speed = 1f;
            }
        }

        void FlipTransformForReia(RectTransform transform)
        {
            if (transform.localScale.y == -1f)
                return;
            transform.localScale = new Vector3(1f, -1f, 1f);
            transform.position -= new Vector3(0f, transform.sizeDelta.y, 0f);
        }

        void FlipTransformForBMP(RectTransform transform)
        {
            if (transform.localScale.y == 1f)
                return;
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.position += new Vector3(0f, transform.sizeDelta.y, 0f);
        }
    }
}
