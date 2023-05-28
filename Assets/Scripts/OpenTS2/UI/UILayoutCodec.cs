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
                    var properties = new UIProperties(propertiesString);
                    var element = UIElementFactory.CreateFromProperties(properties);
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
    }
}
