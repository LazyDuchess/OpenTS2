using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using OpenTS2.Engine.Tests;
using OpenTS2.Game;

namespace OpenTS2
{
    [CustomEditor(typeof(UILayoutTest))]
    public class UILayoutTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (Application.isPlaying)
            {
                var layoutTest = target as UILayoutTest;
                GUILayout.BeginVertical("box");
                GUILayout.Label(layoutTest.UILayouts[layoutTest.CurrentLayout].ToString());
                EditorGUILayout.Separator();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Previous"))
                {
                    layoutTest.Previous();
                }
                if (GUILayout.Button("Next"))
                {
                    layoutTest.Next();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }
    }
}
