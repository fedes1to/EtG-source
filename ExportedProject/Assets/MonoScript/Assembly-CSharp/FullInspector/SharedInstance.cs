namespace FullInspector
{
	public class SharedInstance<TInstance, TSerializer> : BaseScriptableObject<TSerializer> where TSerializer : BaseSerializer
	{
		public TInstance Instance;
	}
	public abstract class SharedInstance<T> : SharedInstance<T, FullSerializerSerializer>
	{
	}
}
