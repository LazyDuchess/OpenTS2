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
        public GameObject ConsoleParent;
        public InputField CheatInputField;
        public Text ConsoleOutput;
        public ScrollRect ConsoleScroll;
        [ConsoleProperty("ots2_consoleMaxChars")]
        private static int _maxCharacters = 50000;
        bool _isOpen = false;
        string _lastCheat = "";

        private void Awake()
        {
            ConsoleParent.SetActive(false);
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                ToggleConsole();
            if (!_isOpen)
                return;
            if (Input.GetKeyDown(KeyCode.Return))
                Submit();
            if (Input.GetKeyDown(KeyCode.UpArrow) && !string.IsNullOrEmpty(_lastCheat) && CheatInputField.isFocused)
                CheatInputField.text = _lastCheat;
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
            _lastCheat = CheatInputField.text;
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
            ConsoleOutput.text += $"  {str}{Environment.NewLine}";
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