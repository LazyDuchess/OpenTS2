using System;
using System.Text;

namespace OpenTS2.Content.DBPF
{
    public interface IArrayAsset
    {
        Type ArrayType();
    }

    /// <summary>
    /// Contains a 2D Array of data.
    /// </summary>
    public class _2DArrayAsset<T> : AbstractAsset, IArrayAsset
    {
        public int Width { get; }
        public int Height { get; }
        public int Type { get; }
        public T[] Data { get; }

        public _2DArrayAsset(int width, int height, int type, T[] data)
        {
            Width = width;
            Height = height;
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            int padding = typeof(T) == typeof(float) ? 6 : 3;
            int i = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    result.Append(Data[i++].ToString().PadRight(padding) + " ");
                }

                result.AppendLine();
            }

            return $"Array {TGI.InstanceID}: \n {result}";
        }

        public Type ArrayType()
        {
            return typeof(T);
        }
    }

}