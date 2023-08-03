using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Diagnostic
{
    /// <summary>
    /// Controls the behavior of the cheat console.
    /// </summary>
    public class CheatConsoleController : MonoBehaviour, IConsoleOutput
    {
        private static CheatConsoleController s_singleton = null;
        public GameObject ConsoleParent;
        public InputField CheatInputField;
        public Text ConsoleOutput;
        public ScrollRect ConsoleScroll;
        private static int _maxCharacters = 50000;
        private static int _maxHistory = 100;
        bool _isOpen = false;
        List<string> _history = new List<string>();
        int _currentHistory = -1;

        private void Awake()
        {
            if (s_singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            CheatInputField.onValueChanged.AddListener(OnTextChanged);
            s_singleton = this;
            ConsoleParent.SetActive(false);
            DontDestroyOnLoad(this);
        }

        void OnTextChanged(string value)
        {
            _currentHistory = -1;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                ToggleConsole();
            if (!_isOpen)
                return;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Submit();
            if (Input.GetKeyDown(KeyCode.UpArrow) && CheatInputField.isFocused)
            {
                HistoryUp();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && CheatInputField.isFocused)
            {
                HistoryDown();
            }
        }

        void AddToHistory(string text)
        {
            _history.Insert(0, text);
            if (_history.Count > _maxHistory)
                _history.RemoveRange(_maxHistory, _history.Count - _maxHistory);
        }

        void HistoryUp()
        {
            CheatInputField.onValueChanged.RemoveListener(OnTextChanged);
            if (_history.Count > 0)
            {
                _currentHistory++;
                if (_currentHistory < 0)
                    _currentHistory = _history.Count - 1;
                if (_currentHistory >= _history.Count)
                    _currentHistory = 0;
                CheatInputField.text = _history[_currentHistory];
            }
            CheatInputField.onValueChanged.AddListener(OnTextChanged);
        }

        void HistoryDown()
        {
            CheatInputField.onValueChanged.RemoveListener(OnTextChanged);
            if (_history.Count > 0)
            {
                _currentHistory--;
                if (_currentHistory < 0)
                    _currentHistory = _history.Count - 1;
                if (_currentHistory >= _history.Count)
                    _currentHistory = 0;
                CheatInputField.text = _history[_currentHistory];
            }
            CheatInputField.onValueChanged.AddListener(OnTextChanged);
        }

        void ToggleConsole()
        {
            if (!_isOpen)
                OpenConsole();
            else
                CloseConsole();
        }

        void OpenConsole()
        {
            _isOpen = true;
            ConsoleParent.SetActive(true);
            CheatInputField.Select();
            CheatInputField.ActivateInputField();
        }

        void CloseConsole()
        {
            _isOpen = false;
            ConsoleParent.SetActive(false);
        }

        void Submit()
        {
            Log($"> {CheatInputField.text}");
            AddToHistory(CheatInputField.text);
            try
            {
                CheatSystem.Execute(CheatInputField.text, this);
            }
            catch(Exception e)
            {
                Log(e.ToString());
                Debug.LogException(e);
            }
            CheatInputField.text = "";
            StartCoroutine(ActivateInput());
        }

        void Truncate()
        {
            if (ConsoleOutput.text.Length > _maxCharacters)
            {
                ConsoleOutput.text = ConsoleOutput.text.Substring(ConsoleOutput.text.Length-_maxCharacters,_maxCharacters);
            }
        }

        public void Log(string str)
        {
            ConsoleOutput.text += $"{str}{Environment.NewLine}";
            Truncate();
            StartCoroutine(ScrollToBottom());
        }

        public void Clear()
        {
            ConsoleOutput.text = "";
            StartCoroutine(ScrollToBottom());
        }

        IEnumerator ScrollToBottom()
        {
            yield return null;
            ConsoleScroll.verticalNormalizedPosition = 0;
        }

        IEnumerator ActivateInput()
        {
            yield return null;
            CheatInputField.Select();
            CheatInputField.ActivateInputField();
        }
    }
}