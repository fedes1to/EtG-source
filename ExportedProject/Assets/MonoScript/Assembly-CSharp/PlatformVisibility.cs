using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Platform-based Visibility")]
public class PlatformVisibility : MonoBehaviour
{
	public bool HideOnWeb;

	public bool HideOnMobile;

	public bool HideInEditor;

	private void Start()
	{
		dfControl component = GetComponent<dfControl>();
		if (!(component == null) && ((HideInEditor && Application.isEditor) ? true : false))
		{
			component.Hide();
		}
	}
}
