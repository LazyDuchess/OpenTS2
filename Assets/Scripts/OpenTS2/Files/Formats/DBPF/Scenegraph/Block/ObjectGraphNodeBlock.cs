using System;
using OpenTS2.Files.Utils;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph.Block
{
    /// <summary>
    /// This header is used in non-leaf scenegraph objects (cObjectGraphNode).
    ///
    /// It contains the Scenegraph files that this one depends on, allowing you to graph out their dependencies.
    /// </summary>
    public class ObjectGraphNodeBlock
    {
        private readonly PersistTypeInfo _typeInfo;

        public ObjectReference[] Extensions;

        /// <summary>
        /// A tag on the graph, usually represents the name of the object represented by this graph.
        /// </summary>
        public string Tag { get; }

        private ObjectGraphNodeBlock(PersistTypeInfo typeInfo, ObjectReference[] extensions, string tag) =>
            (_typeInfo, Extensions, Tag) = (typeInfo, extensions, tag);

        public static ObjectGraphNodeBlock Deserialize(IoBuffer reader)
        {
            var typeInfo = PersistTypeInfo.Deserialize(reader);
            Debug.Assert(typeInfo.Name == "cObjectGraphNode");

            var numberOfExtensions = reader.ReadUInt32();
            var extensions = new ObjectReference[numberOfExtensions];
            for (var i = 0; i < numberOfExtensions; i++)
            {
                extensions[i] = ObjectReference.Deserialize(reader);
            }

            var tagString = "";
            if (typeInfo.Version >= 4)
            {
                tagString = reader.ReadVariableLengthPascalString();
            }

            return new ObjectGraphNodeBlock(typeInfo, extensions, tagString);
        }
    }
}