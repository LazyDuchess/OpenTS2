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

        private ObjectGraphNodeBlock(PersistTypeInfo typeInfo, ObjectReference[] extensions) =>
            (_typeInfo, Extensions) = (typeInfo, extensions);

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

            if (typeInfo.Version >= 4)
            {
                var tagString = reader.ReadVariableLengthPascalString();
                Debug.Log($"  tagString: {tagString}");
            }

            return new ObjectGraphNodeBlock(typeInfo, extensions);
        }
    }
}