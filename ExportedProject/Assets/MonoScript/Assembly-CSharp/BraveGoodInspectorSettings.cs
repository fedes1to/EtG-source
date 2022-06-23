using FullInspector;

public class BraveGoodInspectorSettings : fiSettingsProcessor
{
	public void Process()
	{
		fiSettings.SerializeAutoProperties = false;
		fiSettings.RootDirectory = "Assets/Libraries/FullInspector2/";
	}
}
