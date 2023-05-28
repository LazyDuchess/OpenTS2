using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenTS2.Files.Formats.DBPF;
using UnityEditor;
using OpenTS2.Common;

namespace OpenTS2.Editor
{
    class IDGenerator : EditorWindow
    {
        bool _hasResult = false;
        ResourceKey _result = default(ResourceKey);
        string _filename;
        void OnGUI()
        {
            _filename = EditorGUILayout.TextField("Resource filename: ", _filename);

            if (GUILayout.Button("OK"))
            {
                _hasResult = true;
                _result = new ResourceKey(_filename, _filename, 0);
            }

            if (GUILayout.Button("Cancel"))
                Close();

            if (_hasResult)
            {
                EditorGUILayout.LabelField(_filename+":");
                EditorGUILayout.LabelField("Instance: 0x" + _result.InstanceID.ToString("X8"));
                EditorGUILayout.LabelField("Instance(High): 0x" + _result.InstanceHigh.ToString("X8"));
                EditorGUILayout.LabelField("Group: 0x" + _result.GroupID.ToString("X8"));
            }
        }
        [MenuItem("OpenTS2/Resource Key/From Filename")]
        private static void GenerateInstanceID()
        {
            IDGenerator window = new IDGenerator();
            window.ShowUtility();
        }
    }
}
