namespace FullInspector.Internal
{
	public interface fiIValueProxyAPI
	{
		object Value { get; set; }

		void SaveState();

		void LoadState();
	}
}
