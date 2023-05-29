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
        /// <summary>
        /// Splits string by newline, takes strings inside quotes into account.
        /// </summary>
        /// <param name="str">String to split.</param>
        /// <returns>Split string.</returns>
        List<string> SplitNewlines(string str)
        {
            var lines = new List<string>();
            var inString = false;
            var stringSoFar = "";
            for(var i=0;i<str.Length;i++)
            {
                var currentChar = str[i];
                if (currentChar == '\n' && !inString)
                {
                    lines.Add(stringSoFar);
                    stringSoFar = "";
                }
                else
                {
                    if (currentChar == '"')
                    {
                        inString = !inString;
                    }
                    stringSoFar += currentChar;
                }
            }
            lines.Add(stringSoFar);
            return lines;
        }
        public override AbstractAsset Deserialize(byte[] bytes, ResourceKey tgi, DBPFFile sourceFile)
        {
            var asset = new UILayout();
            // Current element, if this is not null then parsed elements are children of this.
            UIElement currentElement = null;
            // Last parsed element.
            UIElement lastElement = null;
            var lines = SplitNewlines(Encoding.UTF8.GetString(bytes));
            foreach (var line in lines)
            {
                var currentLine = line.Trim();
                if (string.IsNullOrEmpty(currentLine))
                    continue;
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
                var properties = new UIProperties(propertiesString);
                var element = UIElementFactory.CreateFromProperties(properties);
                element.Parent = currentElement;

                if (currentElement == null)
                    asset.Elements.Add(element);
                else
                    currentElement.Children.Add(element);
                lastElement = element;
            }
            return asset;
        }
    }
}
