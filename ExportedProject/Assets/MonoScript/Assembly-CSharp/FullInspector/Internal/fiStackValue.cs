using System.Collections.Generic;

namespace FullInspector.Internal
{
	public class fiStackValue<T>
	{
		private readonly Stack<T> _stack = new Stack<T>();

		public T Value
		{
			get
			{
				return _stack.Peek();
			}
			set
			{
				Pop();
				Push(value);
			}
		}

		public void Push(T value)
		{
			_stack.Push(value);
		}

		public T Pop()
		{
			if (_stack.Count > 0)
			{
				return _stack.Pop();
			}
			return default(T);
		}
	}
}
