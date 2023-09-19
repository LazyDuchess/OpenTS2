using OpenTS2.Files.Utils;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace OpenTS2.Content.DBPF
{
    public interface I3DArrayView : IArrayView
    {
        void Resize(int newDepth);
    }

    /// <summary>
    /// A typed view into a 3D array.
    /// This is essentially a copy of the base data. It should be committed to save back to the parent array.
    /// </summary>
    /// <typeparam name="T">Array element type</typeparam>
    public class _3DArrayView<T> : IArrayView where T : unmanaged
    {
        public _3DArrayAsset Parent { get; }
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; private set; }
        public T[][] Data => _data;

        private T[][] _data;

        public _3DArrayView(_3DArrayAsset asset)
        {
            if (UnsafeUtility.SizeOf<T>() != asset.ElementSize)
            {
                throw new InvalidOperationException($"3D array view made of type {typeof(T).Name} with expected element size {UnsafeUtility.SizeOf<T>()}, but source data is for element size {asset.ElementSize}.");
            }

            Parent = asset;
            Width = asset.Width;
            Height = asset.Height;
            Depth = asset.Depth;
            _data = new T[Depth][];

            for (int i = 0; i < Depth; i++)
            {
                _data[i] = new T[Width * Height];
            }

            Update();
        }

        /// <summary>
        /// Resize the array view with a new depth.
        /// This will apply to the parent when Commit is called, but can be reset by Update.
        /// </summary>
        /// <param name="newDepth">The new depth for the 3d array</param>
        public void Resize(int newDepth)
        {
            Array.Resize(ref _data, newDepth);

            for (int i = Depth; i < newDepth; i++)
            {
                _data[i] = new T[Width * Height];
            }

            Depth = newDepth;
        }

        /// <summary>
        /// Take values from the parent 2D array.
        /// </summary>
        public void Update()
        {
            // Ensure this array has the same depth as the source.

            if (Depth != Parent.Depth)
            {
                Resize(Parent.Depth);
            }

            for (int i = 0; i < Depth; i++)
            {
                ReinterpretCast.Copy(Parent.Data[i], _data[i], _data[i].Length * Parent.ElementSize);
            }
        }

        /// <summary>
        /// Commit changes back to the parent 2D array.
        /// </summary>
        public void Commit()
        {
            // Ensure the source array has the same depth as this.

            if (Depth != Parent.Depth)
            {
                Parent.Resize(Depth);
            }

            for (int i = 0; i < Depth; i++)
            {
                ReinterpretCast.Copy(_data[i], Parent.Data[i], _data[i].Length * Parent.ElementSize);
            }
        }

        /// <summary>
        /// Get the type of the array view.
        /// </summary>
        /// <returns>The array view type</returns>
        public Type ArrayType()
        {
            return typeof(T);
        }
    }

    /// <summary>
    /// Contains a 3D Array of data.
    /// </summary>
    public class _3DArrayAsset : AbstractAsset
    {
        public int Width { get; }
        public int Height { get; }
        public int ElementSize { get; private set; }
        public int Depth { get; private set; }

        /// <summary>
        /// Note: data is encoded in consecutive vertical strips, unlike 2d array which is horizontal
        /// </summary>
        public byte[][] Data => _data;

        private byte[][] _data;

        private IArrayView _autoCommitView;

        public _3DArrayAsset(int width, int height, int depth, int elementSize, byte[][] data)
        {
            Width = width;
            Height = height;
            Depth = depth;
            ElementSize = elementSize;
            _data = data;
        }

        public void Resize(int newDepth)
        {
            Array.Resize(ref _data, newDepth);

            for (int i = Depth; i < newDepth; i++)
            {
                _data[i] = new byte[Width * Height * ElementSize];
            }

            Depth = newDepth;
        }

        public _3DArrayView<T> GetView<T>(bool autoCommit) where T : unmanaged
        {
            if (ElementSize == 0)
            {
                // Element size may not be known if the depth is 0.
                // Inherit the first view's size if that is the case.
                ElementSize = UnsafeUtility.SizeOf<T>();
            }

            if (autoCommit && _autoCommitView != null)
            {
                if (typeof(T) == _autoCommitView.ArrayType())
                {
                    return (_3DArrayView<T>)_autoCommitView;
                }

                throw new InvalidOperationException("Cannot have more than one view of a 3d array with auto commit enabled.");
            }

            var view = new _3DArrayView<T>(this);

            if (autoCommit)
            {
                _autoCommitView = view;
            }

            return view;
        }

        public void AutoCommit()
        {
            if (_autoCommitView != null)
            {
                _autoCommitView.Commit();
            }
        }
    }

}