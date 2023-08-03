using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Diagnostic
{
    public class CheatConsoleController : MonoBehaviour
    {
        public GameObject ConsoleParent;
        public InputField CheatInputField;
        bool _isOpen = false;

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
            try
            {
                CheatSystem.Execute(CheatInputField.text);
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
            CheatInputField.text = "";
            CloseConsole();
        }
    }
}