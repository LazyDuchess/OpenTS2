using OpenTS2.Game;
using System;
using System.Collections.Generic;
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
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var simulator = target as Simulator;
            var vm = simulator.VirtualMachine;
            if (vm == null) return;
            var entities = vm.Entities;
            if (entities == null) return;
            GUILayout.Label("Entities");
            GUILayout.BeginVertical("box");
            foreach(var entity in entities)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"{entity.ID} - {entity.ObjectDefinition.FileName}");
                GUILayout.EndVertical();
                if (GUILayout.Button("Kill"))
                {
                    entity.Delete();
                }
            }
            GUILayout.EndVertical();
        }
    }
}
