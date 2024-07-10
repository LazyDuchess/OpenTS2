using OpenTS2.Game;
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
    [CustomEditor(typeof(Simulator))]
    public class SimulatorEditor : Editor
    {
        private bool _showGlobals = false;
        private bool _showEntities = false;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var simulator = target as Simulator;
            var vm = simulator.VirtualMachine;
            if (vm == null) return;
            var entities = vm.Entities;
            if (entities == null) return;

            _showGlobals = EditorGUILayout.Foldout(_showGlobals, "Globals");
            if (_showGlobals)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < vm.GlobalState.Length; i++)
                {
                    GUILayout.BeginVertical("box");
                    var globalName = ((SimAntics.VMGlobals)i).ToString();
                    GUILayout.BeginHorizontal();
                    var editedGlobal = EditorGUILayout.TextField(globalName, vm.GlobalState[i].ToString(CultureInfo.InvariantCulture));
                    if (short.TryParse(editedGlobal,NumberStyles.Integer, CultureInfo.InvariantCulture, out var res))
                    {
                        if (res != vm.GlobalState[i])
                            vm.SetGlobal((ushort)i, res);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _showEntities = EditorGUILayout.Foldout(_showEntities, "Entities");
            if (_showEntities)
            {
                EditorGUI.indentLevel++;
                foreach (var entity in entities)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{entity.ID} - {entity.ObjectDefinition.FileName}");
                    EditorGUILayout.Separator();
                    if (GUILayout.Button("Kill"))
                    {
                        entity.Delete();
                    }
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
