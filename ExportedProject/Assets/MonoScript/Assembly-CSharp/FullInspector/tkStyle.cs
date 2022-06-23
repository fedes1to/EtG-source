namespace FullInspector
{
	public abstract class tkStyle<T, TContext>
	{
		public abstract void Activate(T obj, TContext context);

		public abstract void Deactivate(T obj, TContext context);
	}
}
