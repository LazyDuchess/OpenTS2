using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using OpenTS2.Files.Formats.DBPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.UI
{
    /// <summary>
    /// Represents properties for UI elements, such as class id, id, style, etc.
    /// </summary>
    public class UIProperties
    {
        public struct StringReference
        {
            public ResourceKey StringSet;
            public int StringID;
            public StringReference(ResourceKey stringSet, int stringId)
            {
                StringSet = stringSet;
                StringID = stringId;
            }
            public string GetLocalizedString()
            {
                var contentProvider = ContentProvider.Get();
                var stringSet = contentProvider.GetAsset<StringSetAsset>(StringSet);
                if (stringSet == null)
                    return "";
                return stringSet.GetString(StringID);
            }
        }
        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        public bool HasProperty(string property)
        {
            return _properties.ContainsKey(property);
        }

        public StringReference GetStringSetProperty(string property)
        {
            var stringArray = GetStringListProperty(property);
            var group = Convert.ToUInt32(stringArray[0], 16);
            var strAndID = stringArray[1];
            var strInstance = Convert.ToUInt32(strAndID.Substring(0,4), 16);
            var stringId = Convert.ToInt32(strAndID.Substring(4), 16) - 1;
            return new StringReference(new ResourceKey(strInstance, group, TypeIDs.STR), stringId);
        }

        public uint GetHexProperty(string property)
        {
            var value = GetProperty(property);
            if (value == null)
                return 0;
            return Convert.ToUInt32(value, 16);
        }
        public ResourceKey GetImageKeyProperty(string property)
        {
            var uintArray = GetHexListProperty(property);
            if (uintArray.Count < 2)
                return default;
            return new ResourceKey(uintArray[1], uintArray[0], TypeIDs.IMG);
        }
        public Color32 GetColorProperty(string property)
        {
            var floatArray = GetFloatListProperty(property);
            return new Color32((byte)floatArray[0], (byte)floatArray[1], (byte)floatArray[2], 255);
        }
        public Rect GetRectProperty(string property)
        {
            var floatArray = GetFloatListProperty(property);
            var x = floatArray[0];
            var y = floatArray[1];
            var width = floatArray[2] - x;
            var height = floatArray[3] - y;
            return new Rect(x, y, width, height);
        }

        public List<string> GetStringListProperty(string property)
        {
            var list = new List<string>();
            var value = GetProperty(property);
            if (value == null)
                return list;
            // Remove start ( and end )
            value = value.Substring(1, value.Length - 2);
            var split = value.Split(',');
            foreach (var element in split)
            {
                list.Add(element);
            }
            return list;
        }

        public List<uint> GetHexListProperty(string property)
        {
            var list = new List<uint>();
            var value = GetProperty(property);
            if (value == null)
                return list;
            // Remove start ( and end )
            value = value.Substring(1, value.Length - 2);
            var split = value.Split(',');
            foreach (var element in split)
            {
                list.Add(Convert.ToUInt32(element, 16));
            }
            return list;
        }
        public List<float> GetFloatListProperty(string property)
        {
            var list = new List<float>();
            var value = GetProperty(property);
            if (value == null)
                return list;
            // Remove start ( and end )
            value = value.Substring(1, value.Length - 2);
            var split = value.Split(',');
            foreach (var element in split)
            {
                list.Add(float.Parse(element));
            }
            return list;
        }

        public bool GetBoolProperty(string property)
        {
            if (_properties.TryGetValue(property, out string value))
                return value == "yes";
            return false;
        }
        public string GetProperty(string property)
        {
            if (_properties.TryGetValue(property, out string value))
                return value;
            return null;
        }
        private enum Reading
        {
            None,
            Key,
            Value
        }
        public override string ToString()
        {
            var str = "";
            foreach(var prop in _properties)
            {
                str += $"{prop.Key} = {prop.Value} | ";
            }
            return str;
        }
        public UIProperties(string properties)
        {
            var currentlyReading = Reading.None;
            var currentKey = "";
            var currentValue = "";
            var inString = false;
            for (var i = 0; i < properties.Length; i++)
            {
                var currentChar = properties[i];
                if (currentlyReading == Reading.None)
                {
                    if (currentChar == ' ')
                        continue;
                    else
                    {
                        currentlyReading = Reading.Key;
                        currentKey = "";
                    }
                }

                if (currentlyReading == Reading.Value)
                {
                    if (currentChar == '"')
                    {
                        if (!inString)
                            inString = true;
                        else
                        {
                            inString = false;
                            currentlyReading = Reading.None;
                            _properties[currentKey.Trim()] = currentValue.Trim();
                        }
                    }
                    else
                    {
                        if (currentChar == ' ' && !inString)
                        {
                            currentlyReading = Reading.None;
                            _properties[currentKey.Trim()] = currentValue.Trim();
                        }
                        else
                        {
                            currentValue += currentChar;
                        }
                    }
                }

                if (currentlyReading == Reading.Key)
                {
                    if (currentChar == '=')
                    {
                        currentlyReading = Reading.Value;
                        currentValue = "";
                        inString = false;
                    }
                    else
                    {
                        if (currentChar != ' ')
                            currentKey += currentChar;
                    }
                }
            }
            if (currentlyReading == Reading.Value)
                _properties[currentKey.Trim()] = currentValue.Trim();
        }
    }
}
