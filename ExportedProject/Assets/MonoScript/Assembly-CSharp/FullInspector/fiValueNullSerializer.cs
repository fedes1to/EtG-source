using FullInspector.Internal;

namespace FullInspector
{
	public abstract class fiValueNullSerializer<T> : fiValueProxyEditor, fiIValueProxyAPI
	{
		public T Value;

		object fiIValueProxyAPI.Value
		{
			get
			{
				return Value;
			}
			set
			{
				Value = (T)value;
			}
		}

		void fiIValueProxyAPI.SaveState()
		{
		}

		void fiIValueProxyAPI.LoadState()
		{
		}
	}
}
