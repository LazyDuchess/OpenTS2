using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpenTS2
{
    public class ShaderBuildPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var allshaders = Directory.GetFiles("Assets/Shaders", "*.shader", SearchOption.AllDirectories);
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            var serializedObject = new SerializedObject(graphicsSettings);
            var alwaysIncludedShadersProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");

            foreach (var shaderPath in allshaders)
            {
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                if (shader == null) 
                    continue;
                var alreadyInList = false;
                for (var i = 0; i < alwaysIncludedShadersProp.arraySize; i++)
                {
                    var elem = alwaysIncludedShadersProp.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue == shader)
                    {
                        alreadyInList = true;
                        break;
                    }
                }
                if (alreadyInList)
                    continue;
                alwaysIncludedShadersProp.InsertArrayElementAtIndex(0);
                var arrayElem = alwaysIncludedShadersProp.GetArrayElementAtIndex(0);
                arrayElem.objectReferenceValue = shader;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}