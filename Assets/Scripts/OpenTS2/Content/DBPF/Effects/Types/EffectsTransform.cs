using OpenTS2.Files.Formats.DBPF.Types;
using OpenTS2.Files.Utils;

namespace OpenTS2.Content.DBPF.Effects.Types
{
    public class EffectsTransform
    {
        public static EffectsTransform Deserialize(IoBuffer reader)
        {
            reader.ReadByte();
            reader.ReadFloat();

            // Matrix of 3x3 floats.
            reader.ReadBytes(sizeof(float) * 3 * 3);
            Vector3Serializer.Deserialize(reader);

            return new EffectsTransform();
        }
    }
}