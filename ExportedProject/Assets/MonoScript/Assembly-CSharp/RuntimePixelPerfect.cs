using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Pixel-Perfect Platform Settings")]
public class RuntimePixelPerfect : MonoBehaviour
{
	public bool PixelPerfectInEditor;

	public bool PixelPerfectAtRuntime = true;

	private void Awake()
	{
		dfGUIManager component = GetComponent<dfGUIManager>();
		if (component == null)
		{
			throw new MissingComponentException("dfGUIManager instance not found");
		}
		if (Application.isEditor)
		{
			component.PixelPerfectMode = PixelPerfectInEditor;
		}
		else
		{
			component.PixelPerfectMode = PixelPerfectAtRuntime;
		}
	}
}
