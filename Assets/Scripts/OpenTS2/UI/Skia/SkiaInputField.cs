using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OpenTS2.UI.Skia
{
    public class SkiaInputField : Selectable, IUpdateSelectedHandler
    {
        public string Text
        {
            get
            {
                return Label.Text;
            }

            set
            {
                Label.Text = value;
            }
        }
        private bool Selected => EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        public RectTransform Caret;
        public SkiaLabel Label;

        private int _selectedCharacter = -1;
        private float _caretTimer = 0f;
        private float CaretTime => WinUtils.GetCaretBlinkTimer();
        private Event _processingEvent = new Event();
        public Action OnTextEdited;

        private void FireTextEdited()
        {
            OnTextEdited?.Invoke();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            if (Selected)
                SelectedUpdate();
            else
                DeselectedUpdate();
        }

        private void DeselectedUpdate()
        {
            if (Caret.gameObject.activeSelf)
                Caret.gameObject.SetActive(false);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying) return;
            if (Selected)
                SelectedGUIUpdate();
        }

        private string SanitizeText(string text)
        {
            text = text.Replace('\t'.ToString(), "");
            if (Label.SingleLine)
            {
                text = text.Replace('\n'.ToString(), "");
                text = text.Replace('\r'.ToString(), "");
            }
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
            _caretTimer += Time.unscaledDeltaTime;
            if (_caretTimer > CaretTime * 2f)
            {
                _caretTimer = 0f;
            }

            if (_caretTimer > caretTime)
            {
                if (Caret.gameObject.activeSelf)
                    Caret.gameObject.SetActive(false);
            }
            else
            {
                if (!Caret.gameObject.activeSelf)
                    Caret.gameObject.SetActive(true);
            }
        }

        private void TypeString(string str)
        {
            _caretTimer = 0f;
            var strBuilder = new StringBuilder(Label.Text);
            var pointInsertion = _selectedCharacter + 1;
            strBuilder.Insert(pointInsertion, str);
            Label.Text = strBuilder.ToString();
            _selectedCharacter += str.Length;
            FireTextEdited();
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
                var strBuilder = new StringBuilder(Label.Text);
                strBuilder.Remove(_selectedCharacter, 1);
                Label.Text = strBuilder.ToString();
                MoveCaretLeft();
                FireTextEdited();
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
            if (_selectedCharacter >= Label.Text.Length)
                _selectedCharacter = Label.Text.Length - 1;
        }

        private void UpdateCaretPosition()
        {
            Caret.sizeDelta = new Vector2(1, Label.FontSize + ((Label.LineSpacing - 1f) * Label.FontSize));
            if (_selectedCharacter < 0)
            {
                var firstLineY = -Label.GetLineY(0) + Label.FontSize;
                Caret.anchoredPosition = new Vector2(0f, firstLineY);
                return;
            }
            var rect = Label.GetCharacterRect(_selectedCharacter);
            Caret.anchoredPosition = new Vector2(rect.Rect.x + rect.Rect.width, rect.Rect.y);
        }

        private void ValidateSelection()
        {
            _selectedCharacter = Mathf.Clamp(_selectedCharacter, -1, Label.Text.Length - 1);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            _selectedCharacter = Label.Text.Length - 1;
        }

        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (Selected)
            {
                while (Event.PopEvent(_processingEvent))
                {
                    var commandName = _processingEvent.commandName;
                    if (commandName != null)
                    {
                        switch (commandName)
                        {
                            case "Paste":
                                PasteClipboard();
                                break;
                        }
                    }
                }
                eventData.Use();
            }
        }
    }
}
