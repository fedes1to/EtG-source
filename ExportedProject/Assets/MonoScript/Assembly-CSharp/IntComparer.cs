using System;

public class IntComparer : IComparable
{
	public int m_value { get; private set; }

	public IntComparer(int value)
	{
		m_value = value;
	}

	int IComparable.CompareTo(object ob)
	{
		IntComparer intComparer = (IntComparer)ob;
		return m_value - intComparer.m_value;
	}
}
