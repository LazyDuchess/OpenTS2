using OpenTS2.Files.Utils;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace OpenTS2.Content.DBPF
{
    public interface IArrayView
    {
        void Update();
        void Commit();
        Type ArrayType();
    }

    /// <summary>
    /// A typed view into a 2D array.
    /// This is essentially a copy of the base data. It should be committed to save back to the parent array.
    /// </summary>
    /// <typeparam name="T">Array element type</typeparam>
    public class _2DArrayView<T> : IArrayView where T : unmanaged
    {
        public _2DArrayAsset Parent { get; }
        public int Width { get; }
        public int Height { get; }
        public T[] Data { get; }

        public _2DArrayView(_2DArrayAsset asset)
        {
            if (UnsafeUtility.SizeOf<T>() != asset.ElementSize)
            {
                throw new InvalidOperationException($"2D array view made of type {typeof(T).Name} with expected element size {UnsafeUtility.SizeOf<T>()}, but source data is for element size {asset.ElementSize}.");
            }

            Parent = asset;
            Width = asset.Width;
            Height = asset.Height;
            Data = new T[Width * Height];

            Update();
        }

        /// <summary>
        /// Take values from the parent 2D array.
        /// </summary>
        public void Update()
        {
            ReinterpretCast.Copy(Parent.Data, Data, Data.Length * Parent.ElementSize);
        }

        /// <summary>
        /// Commit changes back to the parent 2D array.
        /// </summary>
        public void Commit()
        {
            ReinterpretCast.Copy(Data, Parent.Data, Data.Length * Parent.ElementSize);
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
    /// Contains a 2D Array of data.
    /// </summary>
    public class _2DArrayAsset : AbstractAsset
    {
        public int Width { get; }
        public int Height { get; }
        public int ElementSize { get; }
        public byte[] Data { get; }

        private IArrayView _autoCommitView;

        public _2DArrayAsset(int width, int height, int elementSize, byte[] data)
        {
            Width = width;
            Height = height;
            ElementSize = elementSize;
            Data = data;
        }

        public _2DArrayView<T> GetView<T>(bool autoCommit) where T : unmanaged
        {
            if (autoCommit && _autoCommitView != null)
            {
                if (typeof(T) == _autoCommitView.ArrayType())
                {
                    return (_2DArrayView<T>)_autoCommitView;
                }

                throw new InvalidOperationException("Cannot have more than one view of a 2d array with auto commit enabled.");
            }

            var view = new _2DArrayView<T>(this);

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