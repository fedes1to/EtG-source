using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUICamera")]
public class tk2dUICamera : MonoBehaviour
{
	[SerializeField]
	private LayerMask raycastLayerMask = -1;

	public LayerMask FilteredMask
	{
		get
		{
			return (int)raycastLayerMask & GetComponent<Camera>().cullingMask;
		}
	}

	public Camera HostCamera
	{
		get
		{
			return GetComponent<Camera>();
		}
	}

	public void AssignRaycastLayerMask(LayerMask mask)
	{
		raycastLayerMask = mask;
	}

	private void OnEnable()
	{
		if (GetComponent<Camera>() == null)
		{
			Debug.LogError("tk2dUICamera should only be attached to a camera.");
			base.enabled = false;
		}
		else
		{
			tk2dUIManager.RegisterCamera(this);
		}
	}

	private void OnDisable()
	{
		tk2dUIManager.UnregisterCamera(this);
	}
}
