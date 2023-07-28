using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenTS2.Diagnostic
{
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        [Range(0f,10f)] [SerializeField] float _updateRate = 1f;
        Text _text;
        float _updateCounter = 0f;
        // Start is called before the first frame update
        void Awake()
        {
            _text = GetComponent<Text>();
            UpdateFPS();
        }

        void Update()
        {
            _updateCounter += Time.deltaTime;
            if (_updateCounter >= _updateRate)
            {
                UpdateFPS();
                _updateCounter = 0f;
            }
        }

        // Update is called once per frame
        void UpdateFPS()
        {
            _text.text = "FPS: " + Mathf.Floor(1f / Time.deltaTime).ToString() + Environment.NewLine;
            _text.text += "Frame time: " + (Time.deltaTime * 1000f).ToString("#.##") + "ms";
        }
    }
}
