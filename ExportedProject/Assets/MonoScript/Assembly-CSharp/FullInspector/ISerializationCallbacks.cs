namespace FullInspector
{
	public interface ISerializationCallbacks
	{
		void OnBeforeSerialize();

		void OnAfterSerialize();

		void OnBeforeDeserialize();

		void OnAfterDeserialize();
	}
}
