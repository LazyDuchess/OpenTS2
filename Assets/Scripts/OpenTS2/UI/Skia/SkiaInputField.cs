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

        private int _selectedCharacter = 0;
        private float _caretTimer = 0f;
        private float CaretTime => WinUtils.GetCaretBlinkTimer();

        private void Update()
        {
            if (!Application.isPlaying) return;
            SelectedUpdate();
        }

        private void OnGUI()
        {
            SelectedGUIUpdate();
        }

        private string SanitizeText(string text)
        {
            text = text.Replace('\t'.ToString(), " ");
            return text;
        }

        private void SelectedGUIUpdate()
        {
            var currentInput = "";
            var currentKey = KeyCode.None;
            var ev = Event.current;
            if (ev.type != EventType.KeyDown) return;
            if (ev.isKey)
            {
                if (ev.character != 0)
                {
                    currentInput = SanitizeText(ev.character.ToString());
                }
                currentKey = ev.keyCode;
            }

            switch (currentKey)
            {
                case KeyCode.LeftArrow:
                    MoveCaretLeft();
                    break;

                case KeyCode.RightArrow:
                    MoveCaretRight();
                    break;

                case KeyCode.Backspace:
                    Backspace();
                    break;

                case KeyCode.V:
                    if (Input.GetKey(KeyCode.LeftControl))
                        PasteClipboard();
                    break;
            }

            if (currentInput != "")
            {
                TypeString(currentInput);
            }
            ev.Use();
        }

        private void SelectedUpdate()
        {
            ValidateSelection();
            UpdateCaretPosition();
            DoCaretAnimation();
        }

        private void DoCaretAnimation()
        {
            var caretTime = CaretTime;
            _caretTimer += Time.deltaTime;
            if (_caretTimer > CaretTime * 2f)
            {
                _caretTimer = 0f;
            }

            if (_caretTimer > caretTime)
            {
                if (_caret.gameObject.activeSelf)
                    _caret.gameObject.SetActive(false);
            }
            else
            {
                if (!_caret.gameObject.activeSelf)
                    _caret.gameObject.SetActive(true);
            }
        }

        private void TypeString(string str)
        {
            _caretTimer = 0f;
            var strBuilder = new StringBuilder(_label.Text);
            var pointInsertion = _selectedCharacter + 1;
            strBuilder.Insert(pointInsertion, str);
            _label.Text = strBuilder.ToString();
            _selectedCharacter += str.Length;
        }

        private void PasteClipboard()
        {
            var clipboard = SanitizeText(GUIUtility.systemCopyBuffer);
            TypeString(clipboard);
        }

        private void Backspace()
        {
            if (_selectedCharacter >= 0)
            {
                var strBuilder = new StringBuilder(_label.Text);
                strBuilder.Remove(_selectedCharacter, 1);
                _label.Text = strBuilder.ToString();
                MoveCaretLeft();
            }
        }

        private void MoveCaretLeft()
        {
            _caretTimer = 0f;
            _selectedCharacter--;
            if (_selectedCharacter < -1)
            {
                _selectedCharacter = -1;
            }
        }

        private void MoveCaretRight()
        {
            _caretTimer = 0f;
            _selectedCharacter++;
            if (_selectedCharacter >= _label.Text.Length)
                _selectedCharacter = _label.Text.Length - 1;
        }

        private void UpdateCaretPosition()
        {
            _caret.sizeDelta = new Vector2(1, _label.FontSize + ((_label.LineSpacing - 1f) * _label.FontSize));
            if (_selectedCharacter < 0)
            {
                var firstLineY = -_label.GetLineY(0) + _label.FontSize;
                _caret.anchoredPosition = new Vector2(0f, firstLineY);
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
