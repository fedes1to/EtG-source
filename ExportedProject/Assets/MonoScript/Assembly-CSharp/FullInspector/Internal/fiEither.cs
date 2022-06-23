using System;

namespace FullInspector.Internal
{
	public struct fiEither<TA, TB>
	{
		private TA _valueA;

		private TB _valueB;

		private bool _hasA;

		public TA ValueA
		{
			get
			{
				if (!IsA)
				{
					throw new InvalidOperationException("Either does not contain value A");
				}
				return _valueA;
			}
		}

		public TB ValueB
		{
			get
			{
				if (!IsB)
				{
					throw new InvalidOperationException("Either does not contain value B");
				}
				return _valueB;
			}
		}

		public bool IsA
		{
			get
			{
				return _hasA;
			}
		}

		public bool IsB
		{
			get
			{
				return !_hasA;
			}
		}

		public fiEither(TA valueA)
		{
			_hasA = true;
			_valueA = valueA;
			_valueB = default(TB);
		}

		public fiEither(TB valueB)
		{
			_hasA = false;
			_valueA = default(TA);
			_valueB = valueB;
		}
	}
}
