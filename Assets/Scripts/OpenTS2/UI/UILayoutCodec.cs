using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTS2.Common;
using OpenTS2.Content;
using OpenTS2.Files.Formats.DBPF;
using UnityEngine;

namespace OpenTS2.UI
{
    [Codec(TypeIDs.UI)]
    public class UILayoutCodec : AbstractCodec
    {
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new UILayout();
            // Current element, if this is not null then parsed elements are children of this.
            UIElement currentElement = null;
            // Last parsed element.
            UIElement lastElement = null;
            using (var reader = new StringReader(Encoding.UTF8.GetString(bytes)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var currentLine = line.Trim();
                    // skip comments.
                    if (line.StartsWith("#"))
                        continue;
                    // Remove start < and end >
                    currentLine = currentLine.Substring(1, currentLine.Length - 2);
                    var split = currentLine.Split(' ');
                    var type = split[0];
                    if (type == "CHILDREN")
                    {
                        currentElement = lastElement;
                        continue;
                    }
                    if (type == "/CHILDREN")
                    {
                        currentElement = currentElement.Parent;
                        continue;
                    }
                    if (type != "LEGACY")
                        continue;
                    // Get current line without the type at the beginning (eg LEGACY), just the properties.
                    var propertiesString = currentLine.Substring(type.Length).Trim();
                    var properties = ParseProperties(propertiesString);
                    var element = new UIElement();

                    if (properties.TryGetValue("area", out string area))
                        element.Area = ParseRect(area);

                    if (properties.TryGetValue("fillcolor", out string fillcolor))
                        element.FillColor = ParseColor32(fillcolor);

                    if (properties.TryGetValue("caption", out string caption))
                        element.Caption = caption;

                    if (properties.TryGetValue("id", out string id))
                        element.ID = Convert.ToUInt32(id, 16);

                    if (properties.TryGetValue("image", out string image))
                        element.Image = ParseImageKey(image);

                    element.Parent = currentElement;

                    if (currentElement == null)
                        asset.Elements.Add(element);
                    else
                        currentElement.Children.Add(element);
                    lastElement = element;
                }
            }
            return asset;
        }

        ResourceKey ParseImageKey(string value)
        {
            var uintArray = ParseIDArray(value);
            return new ResourceKey(uintArray[1], uintArray[0], TypeIDs.IMG);
        }

        Color32 ParseColor32(string value)
        {
            var intArray = ParseFloatArray(value);
            return new Color32((byte)intArray[0], (byte)intArray[1], (byte)intArray[2], 0);
        }

        Rect ParseRect(string value)
        {
            var floatArray = ParseFloatArray(value);
            return new Rect(floatArray[0], floatArray[1], floatArray[2], floatArray[3]);
        }

        List<uint> ParseIDArray(string value)
        {
            // Remove start ( and end )
            value = value.Substring(1, value.Length - 2);
            var split = value.Split(',');
            var uintList = new List<uint>();
            foreach (var element in split)
            {
                uintList.Add(Convert.ToUInt32(element, 16));
            }
            return uintList;
        }

        List<float> ParseFloatArray(string value)
        {
            // Remove start ( and end )
            value = value.Substring(1, value.Length - 2);
            var split = value.Split(',');
            var floatList = new List<float>();
            foreach(var element in split)
            {
                floatList.Add(float.Parse(element));
            }
            return floatList;
        }

        private const int READING_NONE = 0;
        private const int READING_KEY = 1;
        private const int READING_VALUE = 2;

        Dictionary<string, string> ParseProperties(string props)
        {
            var keysAndValues = new Dictionary<string, string>();
            var currentlyReading = READING_NONE;
            var currentKey = "";
            var currentValue = "";
            var inString = false;
            for(var i=0;i<props.Length;i++)
            {
                var currentChar = props[i];
                if (currentlyReading == READING_NONE)
                {
                    if (currentChar == ' ')
                        continue;
                    else
                    {
                        currentlyReading = READING_KEY;
                        currentKey = "";
                    }
                }

                if (currentlyReading == READING_VALUE)
                {
                    if (currentChar == '"')
                    {
                        if (!inString)
                            inString = true;
                        else
                        {
                            inString = false;
                            currentlyReading = READING_NONE;
                            keysAndValues[currentKey.Trim()] = currentValue.Trim();
                        }
                    }
                    else
                    {
                        if (currentChar == ' ' && !inString)
                        {
                            currentlyReading = READING_NONE;
                            keysAndValues[currentKey.Trim()] = currentValue.Trim();
                        }
                        else
                        {
                            currentValue += currentChar;
                        }
                    }
                }

                if (currentlyReading == READING_KEY)
                {
                    if (currentChar == '=')
                    {
                        currentlyReading = READING_VALUE;
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
            return keysAndValues;
        }
    }
}
