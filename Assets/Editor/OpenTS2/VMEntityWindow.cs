using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
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
    public class VMEntityWindow : EditorWindow
    {
        public VMEntity Entity;
        private ContentManager _contentManager;
        private List<string> _attributeNames;
        private List<string> _semiAttributeNames;
        private Vector2 _scrollbarPosition = Vector2.zero;
        private bool _showTemps = false;
        private bool _showAttributes = false;
        private bool _showSemiAttributes = false;
        private bool _showObjectData = false;
        public static void Show(VMEntity entity)
        {
            var window = CreateWindow<VMEntityWindow>(typeof(VMEntityWindow));
            window.Initialize(entity);
        }

        public void Initialize(VMEntity entity)
        {
            _attributeNames = new List<string>();
            _semiAttributeNames = new List<string>();
            _contentManager = ContentManager.Instance;
            Entity = entity;
            titleContent = new GUIContent($"VM Entity: {Entity.ObjectDefinition.FileName} ({Entity.ID})");

            var attrStringSet = _contentManager.GetAsset<StringSetAsset>(new ResourceKey(0x100, Entity.PrivateGroupID, TypeIDs.STR));
            var semiAttrStringSet = _contentManager.GetAsset<StringSetAsset>(new ResourceKey(0x100, Entity.SemiGlobalGroupID, TypeIDs.STR));

            if (attrStringSet != null)
            {
                foreach (var strValue in attrStringSet.StringData.Strings[Languages.USEnglish])
                    _attributeNames.Add(strValue.Value);
            }

            if (semiAttrStringSet != null)
            {
                foreach (var strValue in semiAttrStringSet.StringData.Strings[Languages.USEnglish])
                    _semiAttributeNames.Add(strValue.Value);
            }
        }

        private void DisplayEditableField(string name, ref short value)
        {
            var editedValue = EditorGUILayout.TextField(name, value.ToString(CultureInfo.InvariantCulture));
            if (short.TryParse(editedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out short res))
            {
                if (res != value)
                    value = res;
            }
        }

        private void OnGUI()
        {
            _scrollbarPosition = EditorGUILayout.BeginScrollView(_scrollbarPosition);

            _showObjectData = EditorGUILayout.Foldout(_showObjectData, "Object Data");
            if (_showObjectData)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < Entity.ObjectData.Length; i++)
                {
                    GUILayout.BeginVertical("box");
                    DisplayEditableField(((VMObjectData)i).ToString(), ref Entity.ObjectData[i]);
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _showAttributes = EditorGUILayout.Foldout(_showAttributes, "Attributes");
            if (_showAttributes)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < Entity.Attributes.Length; i++)
                {
                    var attributeName = $"Attribute {i}";
                    if (_attributeNames.Count > i && !string.IsNullOrEmpty(_attributeNames[i]))
                    {
                        attributeName = _attributeNames[i];
                    }
                    GUILayout.BeginVertical("box");
                    DisplayEditableField(attributeName, ref Entity.Attributes[i]);
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _showSemiAttributes = EditorGUILayout.Foldout(_showSemiAttributes, "Semi-Attributes");
            if (_showSemiAttributes)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < Entity.SemiAttributes.Length; i++)
                {
                    var attributeName = $"Attribute {i}";
                    if (_semiAttributeNames.Count > i && !string.IsNullOrEmpty(_semiAttributeNames[i]))
                    {
                        attributeName = _semiAttributeNames[i];
                    }
                    GUILayout.BeginVertical("box");
                    DisplayEditableField(attributeName, ref Entity.SemiAttributes[i]);
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _showTemps = EditorGUILayout.Foldout(_showTemps, "Temps");
            if (_showTemps)
            {
                EditorGUI.indentLevel++;
                for (var i = 0; i < Entity.Temps.Length; i++)
                {
                    GUILayout.BeginVertical("box");
                    DisplayEditableField($"Temp {i}", ref Entity.Temps[i]);
                    GUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
