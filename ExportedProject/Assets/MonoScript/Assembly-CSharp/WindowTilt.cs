using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Window Tilt")]
public class WindowTilt : MonoBehaviour
{
	private dfControl control;

	private void Start()
	{
		control = GetComponent<dfControl>();
		if (control == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		Camera camera = control.GetCamera();
		Vector3 center = control.GetCenter();
		Vector3 vector = camera.WorldToViewportPoint(center);
		control.transform.localRotation = Quaternion.Euler(0f, (vector.x * 2f - 1f) * 20f, 0f);
	}
}
