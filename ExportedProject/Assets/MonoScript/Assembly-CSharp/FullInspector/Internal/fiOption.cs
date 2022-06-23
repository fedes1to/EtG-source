using System;

namespace FullInspector.Internal
{
	public static class fiOption
	{
		public static fiOption<T> Just<T>(T value)
		{
			return new fiOption<T>(value);
		}
	}
	public struct fiOption<T>
	{
		private bool _hasValue;

		private T _value;

		public static fiOption<T> Empty = new fiOption<T>
		{
			_hasValue = false,
			_value = default(T)
		};

		public bool HasValue
		{
			get
			{
				return _hasValue;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return !_hasValue;
			}
		}

		public T Value
		{
			get
			{
				if (!HasValue)
				{
					throw new InvalidOperationException("There is no value inside the option");
				}
				return _value;
			}
		}

		public fiOption(T value)
		{
			_hasValue = true;
			_value = value;
		}
	}
}
