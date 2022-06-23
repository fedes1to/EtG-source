using System;
using System.Diagnostics;
using System.Threading;

public class ObjectPool<T> where T : class
{
	[DebuggerDisplay("{Value,nq}")]
	private struct Element
	{
		public T Value;
	}

	public delegate T Factory();

	public delegate void Cleanup(T obj);

	private T _firstItem;

	private readonly Element[] _items;

	private readonly Factory _factory;

	private readonly Cleanup _cleanup;

	public ObjectPool(Factory factory, Cleanup cleanup = null)
		: this(factory, Environment.ProcessorCount * 2, cleanup)
	{
	}

	public ObjectPool(Factory factory, int size, Cleanup cleanup = null)
	{
		_factory = factory;
		_items = new Element[size - 1];
		_cleanup = cleanup;
	}

	private T CreateInstance()
	{
		return _factory();
	}

	public T Allocate()
	{
		T val = _firstItem;
		if (val == null || val != Interlocked.CompareExchange(ref _firstItem, (T)null, val))
		{
			val = AllocateSlow();
		}
		return val;
	}

	private T AllocateSlow()
	{
		Element[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			T value = items[i].Value;
			if (value != null && value == Interlocked.CompareExchange(ref items[i].Value, (T)null, value))
			{
				return value;
			}
		}
		return CreateInstance();
	}

	public void Clear()
	{
		_firstItem = (T)null;
		Element[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Value = (T)null;
		}
	}

	public void Free(ref T obj)
	{
		if (obj == null)
		{
			return;
		}
		if (_firstItem == null)
		{
			if (_cleanup != null)
			{
				_cleanup(obj);
			}
			_firstItem = obj;
		}
		else
		{
			FreeSlow(obj);
		}
		obj = (T)null;
	}

	private void FreeSlow(T obj)
	{
		Element[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Value == null)
			{
				if (_cleanup != null)
				{
					_cleanup(obj);
				}
				items[i].Value = obj;
				break;
			}
		}
	}
}
