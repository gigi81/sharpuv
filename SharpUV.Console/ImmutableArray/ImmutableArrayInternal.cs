#region License
//*****************************************************************************/
// Copyright (c) 2010 - 2012 Luigi Grilli
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//*****************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace SharpUV.ImmutableArray
{
	internal class ImmutableArrayInternal<T> where T : struct, IComparable, IEquatable<T>, IConvertible
	{
		private static readonly T[] EmptyArray = new T[0];

		/// <summary>
		/// Separator used between array items when converting an immutable array to string
		/// </summary>
		internal const string SEPARATOR = "-";

		/// <summary>
		/// Maximum number of elements used to calculate hash code
		/// </summary>
		internal const int HASH_MAX_ELEMENTS = 10;

		private int _offset = 0;
		private T[] _data = EmptyArray;
		private int _length = 0;

		#region Constructors
		public ImmutableArrayInternal(T[] data)
			: this(data, data.Length)
		{
		}

		public ImmutableArrayInternal(T[] data, int length)
			: this(0, data, length)
		{
		}

		public ImmutableArrayInternal(int sourceIndex, T[] data, int length)
		{
			if (sourceIndex < 0)
				throw new IndexOutOfRangeException("Index cannot be less than zero");

			if (data == null)
				throw new ArgumentNullException("data", "Array cannot be null");

			if ((sourceIndex + length) > data.Length)
				throw new ArgumentOutOfRangeException("The sourceIndex and length specified overcome the source ByteArray length");

			_offset = 0;
			_data = ImmutableArrayInternal<T>.AllocInternalArray(sourceIndex, data, length);
			_length = length;
		}

		public ImmutableArrayInternal(ImmutableArrayInternal<T> data)
			: this(data, data.Length)
		{
		}

		public ImmutableArrayInternal(ImmutableArrayInternal<T> data, int length)
			: this(0, data, length)
		{
		}

		public ImmutableArrayInternal(int sourceIndex, ImmutableArrayInternal<T> data, int length)
		{
			if ((sourceIndex < 0) || (sourceIndex >= data.Length))
				throw new IndexOutOfRangeException("Index must be between 0 and upper array bound");

			if (data == null)
				throw new ArgumentNullException("data", "Array cannot be null");

			if ((sourceIndex + length) > data.Length)
				throw new ArgumentOutOfRangeException("The sourceIndex and length specified overcome the source ByteArray length");

			_offset = data.Offset + sourceIndex;
			_data = data.Data;
			_length = length;
		}

		public ImmutableArrayInternal(ICollection<T> array)
		{
			_offset = 0;
			_data = AllocInternalArray(array);
			_length = array.Count;
		}
		#endregion

		#region Public methods
		public T this[int index]
		{
			get
			{
				return _data[_offset + index];
			}
		}

		public int Length
		{
			get { return _length; }
		}

		public ImmutableArrayInternal<T> SubArrayInternal(int sourceIndex)
		{
			return this.SubArrayInternal(sourceIndex, this.Length - sourceIndex);
		}

		public ImmutableArrayInternal<T> SubArrayInternal(int sourceIndex, int length)
		{
			if ((sourceIndex == 0) && (length == this.Length))
				return this;

			return new ImmutableArrayInternal<T>(sourceIndex, this, length);
		}

		public void CopyTo(Array array, int index)
		{
			this.CopyTo(array, index, this.Length);
		}

		public void CopyTo(Array array, int index, int length)
		{
			Array.Copy(_data, _offset, array, index, length);
		}
		#endregion

		#region Protected and Private method/properties
		protected int Offset
		{
			get { return _offset; }
		}

		protected T[] Data
		{
			get { return _data; }
		}
		#endregion


		#region Object overrides
		public override bool Equals(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("Cannot compare object with a null instance");

			if (obj is T[])
				return this.EqualsTo((T[])obj);

			if (obj is ImmutableArrayInternal<T>)
				return this.EqualsTo((ImmutableArrayInternal<T>)obj);

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			int step = this.Length / ImmutableArrayInternal<T>.HASH_MAX_ELEMENTS;
			if (step <= 0)
				step = 1;

			int hash = 0;

			for (int i = 0; i < this.Length; i += step)
				hash += this[i].GetHashCode();

			return hash;
		}

		public override string ToString()
		{
			if (this.Length <= 0)
				return String.Empty;

			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < (this.Length - 1); i++)
			{
				builder.Append(this[i].ToString());
				builder.Append(ImmutableArrayInternal<T>.SEPARATOR);
			}
			builder.Append(this[this.Length - 1].ToString());

			return builder.ToString();
		}
		#endregion

		#region EqualsTo methods
		protected bool EqualsTo(T[] array)
		{
			if (this.Length != array.Length)
				return false;

			for (int i = 0; i < this.Length; i++)
				if (!this[i].Equals(array[i]))
					return false;

			return true;
		}

		protected bool EqualsTo(ImmutableArrayInternal<T> array)
		{
			if (this.Length != array.Length)
				return false;

			//fast compare
			if (this.GetHashCode() != array.GetHashCode())
				return false;

			//slow compare
            int sourceIndex = this._offset, destIndex = array._offset;
			for (int i = 0; i < this.Length; i++)
				if (!this._data[sourceIndex++].Equals(array._data[destIndex++]))
					return false;

			return true;
		}
		#endregion

		#region Static private methods
		/// <summary>
		/// This method allocates a mutable array
		/// </summary>
		/// <param name="length">Length of the new array</param>
		/// <returns>Allocated array</returns>
		private static T[] AllocInternalArray(int length)
		{
			return new T[length];
		}

		/// <summary>
		/// This methods allocates a mutable array and copies all the colletion elements to it
		/// </summary>
		/// <param name="collection">Collection to copy elements from</param>
		/// <returns>Array</returns>
		private static T[] AllocInternalArray(ICollection<T> collection)
		{
			T[] ret = ImmutableArrayInternal<T>.AllocInternalArray(collection.Count);
			collection.CopyTo(ret, 0);
			return ret;
		}

		/// <summary>
		/// This method allocates a copy of the specified array
		/// </summary>
		/// <param name="sourceIndex">Index to start the copy</param>
		/// <param name="data">Array of data to be copied</param>
		/// <param name="length">Length od data to topy</param>
		/// <returns>A partial/complete clone of the specified array</returns>
		private static T[] AllocInternalArray(int sourceIndex, T[] data, int length)
		{
			if (length == 0)
				return EmptyArray;

			T[] ret = ImmutableArrayInternal<T>.AllocInternalArray(length);
			Array.Copy(data, sourceIndex, ret, sourceIndex, length);
			return ret;
		}
		#endregion
	}
}
