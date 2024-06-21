using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.UI.Skia
{
    public class SkiaInputField : Selectable
    {
        private bool Selected => true;
        [SerializeField]
        private RectTransform _caret;
        [SerializeField]
        private SkiaLabel _label;

        [SerializeField]
        private int _selectedCharacter = 0;

        private void Update()
        {
            if (!Application.isPlaying) return;
            SelectedUpdate();
        }

        private void SelectedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                MoveCaretLeft();
            if (Input.GetKeyDown(KeyCode.RightArrow))
                MoveCaretRight();

            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (_selectedCharacter >= 0)
                {
                    var strBuilder = new StringBuilder(_label.Text);
                    strBuilder.Remove(_selectedCharacter, 1);
                    _label.Text = strBuilder.ToString();
                    MoveCaretLeft();
                }
            }

            ValidateSelection();
            UpdateCaretPosition();
        }

        private void MoveCaretLeft()
        {
            _selectedCharacter--;
            if (_selectedCharacter < -1)
            {
                _selectedCharacter = -1;
            }
        }

        private void MoveCaretRight()
        {
            _selectedCharacter++;
            if (_selectedCharacter >= _label.Text.Length)
                _selectedCharacter = _label.Text.Length - 1;
        }

        private void UpdateCaretPosition()
        {
            _caret.sizeDelta = new Vector2(1, _label.FontSize);
            if (_selectedCharacter < 0)
            {
                _caret.anchoredPosition = new Vector2(0f, 0f);
                return;
            }
            var rect = _label.GetCharacterRect(_selectedCharacter);
            _caret.anchoredPosition = new Vector2(rect.Rect.x + rect.Rect.width, rect.Rect.y);
        }

        private void ValidateSelection()
        {
            _selectedCharacter = Mathf.Clamp(_selectedCharacter, -1, _label.Text.Length - 1);
        }
    }
}
