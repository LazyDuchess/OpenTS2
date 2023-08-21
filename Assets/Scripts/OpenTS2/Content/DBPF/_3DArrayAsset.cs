using System;
using System.Text;

namespace OpenTS2.Content.DBPF
{
    public interface ID2ArrayAsset
    {
        Type ArrayType();
    }

    /// <summary>
    /// Contains a 2D Array of data.
    /// </summary>
    public class _3DArrayAsset<T> : AbstractAsset, IArrayAsset
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public int Type { get; }

        /// <summary>
        /// Note: data is encoded in consecutive vertical strips, unlike 2d array which is horizontal
        /// </summary>
        public T[][] Data { get; }

        public _3DArrayAsset(int width, int height, int depth, int type, T[][] data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            int padding = typeof(T) == typeof(float) ? 6 : 3;

            for (int z = 0; z < Depth; z++)
            {
                var data = Data[z];

                int i = 0;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        var stringValue = $"{data[i++]}";
                        result.Append(stringValue.PadRight(padding) + " ");
                    }

                    result.AppendLine();
                }

                result.AppendLine();
            }

            return $"Array {TGI.InstanceID} ({Width}x{Height}x{Depth}): \n {result}";
        }

        public Type ArrayType()
        {
            return typeof(T);
        }
    }

}