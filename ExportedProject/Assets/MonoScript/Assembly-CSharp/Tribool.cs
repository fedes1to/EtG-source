using System;

public struct Tribool
{
	public static Tribool Complete = new Tribool(2);

	public static Tribool Ready = new Tribool(1);

	public static Tribool Unready = new Tribool(0);

	public int value;

	public Tribool(int v)
	{
		value = v;
	}

	public override string ToString()
	{
		return string.Format("[Tribool] " + value);
	}

	public static bool operator true(Tribool a)
	{
		return a.value == 1;
	}

	public static bool operator false(Tribool a)
	{
		return a.value == 0;
	}

	public static bool operator !(Tribool a)
	{
		return a.value == 0;
	}

	public static bool operator ==(Tribool a, Tribool b)
	{
		return a.value == b.value;
	}

	public static bool operator !=(Tribool a, Tribool b)
	{
		return a.value != b.value;
	}

	public static Tribool operator ++(Tribool a)
	{
		return new Tribool(Math.Min(2, a.value + 1));
	}

	public override bool Equals(object obj)
	{
		if (obj is Tribool)
		{
			return this == (Tribool)obj;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return value;
	}
}
