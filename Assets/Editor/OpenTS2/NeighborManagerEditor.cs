using OpenTS2.Content;
using OpenTS2.Game;
using OpenTS2.SimAntics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace OpenTS2
{
    [CustomEditor(typeof(NeighborManager))]
    public class NeighborManagerEditor : Editor
    {
        private bool _showNeighbors = false;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var neighborManager = target as NeighborManager;
            _showNeighbors = EditorGUILayout.Foldout(_showNeighbors, "Neighbors");
            if (_showNeighbors)
            {
                var neighbors = neighborManager.Neighbors;
                EditorGUI.indentLevel++;
                foreach(var neighbor in neighbors)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{neighbor.ObjectDefinition.FileName}");
                    GUILayout.Label($"Neighbor ID {neighbor.Id}");
                    GUILayout.Label($"Object GUID 0x{neighbor.GUID:X8}");
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
