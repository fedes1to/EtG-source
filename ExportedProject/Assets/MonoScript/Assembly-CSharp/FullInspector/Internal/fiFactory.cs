using System;
using System.Collections.Generic;

namespace FullInspector.Internal
{
	public class fiFactory<T> where T : new()
	{
		private Stack<T> _reusable = new Stack<T>();

		private Action<T> _reset;

		public fiFactory(Action<T> reset)
		{
			_reset = reset;
		}

		public T GetInstance()
		{
			if (_reusable.Count == 0)
			{
				return new T();
			}
			return _reusable.Pop();
		}

		public void ReuseInstance(T instance)
		{
			_reset(instance);
			_reusable.Push(instance);
		}
	}
}
