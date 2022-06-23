namespace FullInspector
{
	public class UpdateFullInspectorRootDirectory : fiSettingsProcessor
	{
		public void Process()
		{
			fiSettings.RootDirectory = "Assets/Libraries/FullInspector2/";
		}
	}
}
