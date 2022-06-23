using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/3D/Replace GUI Camera")]
public class dfReplaceGUICamera : MonoBehaviour
{
	public Camera mainCamera;

	public void OnEnable()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}
		dfGUIManager component = GetComponent<dfGUIManager>();
		if (component == null)
		{
			Debug.LogError("This script should be attached to a dfGUIManager instance", this);
			base.enabled = false;
		}
		else
		{
			mainCamera.cullingMask |= 1 << base.gameObject.layer;
			component.OverrideCamera = true;
			component.RenderCamera = mainCamera;
		}
	}
}
