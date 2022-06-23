public static class Tuple
{
	public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 second)
	{
		return new Tuple<T1, T2>(item1, second);
	}
}
public sealed class Tuple<T1, T2>
{
	public T1 First;

	public T2 Second;

	public Tuple(T1 first, T2 second)
	{
		First = first;
		Second = second;
	}

	public override string ToString()
	{
		return string.Format("[{0}, {1}]", First, Second);
	}
}
