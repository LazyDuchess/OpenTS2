using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace OpenTS2.Files.Formats.DBPF.Scenegraph
{
    /// <summary>
    /// This is a utility class to dump OpenTS2 scenegraph objects into a json format for debugging purposes.
    /// </summary>
    public class ScenegraphJsonDumper
    {
        public static string DumpCollection(ScenegraphResourceCollection resourceCollection)
        {
            return JsonConvert.SerializeObject(resourceCollection, Formatting.None, new Vector3Converter(),
                new QuaternionConverter());
        }
    }

    internal class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    internal class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}