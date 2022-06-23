using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/Examples/3D/Follow Object (3D)")]
public class dfFollowObject3D : MonoBehaviour
{
	public Transform attachedTo;

	public bool liveUpdate;

	[HideInInspector]
	[SerializeField]
	protected Vector3 designTimePosition;

	private dfControl control;

	private bool lastLiveUpdateValue;

	public void OnEnable()
	{
		control = GetComponent<dfControl>();
		Update();
	}

	public void Update()
	{
		if (lastLiveUpdateValue != liveUpdate)
		{
			lastLiveUpdateValue = liveUpdate;
			if (!liveUpdate)
			{
				control.RelativePosition = designTimePosition;
				control.transform.localScale = Vector3.one;
				control.transform.localRotation = Quaternion.identity;
			}
			else
			{
				designTimePosition = control.RelativePosition;
			}
			control.Invalidate();
		}
		if (liveUpdate || Application.isPlaying)
		{
			updatePosition3D();
		}
	}

	public void OnDrawGizmos()
	{
		if (control == null)
		{
			control = GetComponent<dfControl>();
		}
		Vector3 size = (Vector3)control.Size * control.PixelsToUnits();
		Gizmos.matrix = Matrix4x4.TRS(attachedTo.position, attachedTo.rotation, attachedTo.localScale);
		Gizmos.color = Color.clear;
		Gizmos.DrawCube(Vector3.zero, size);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3.zero, size);
	}

	public void OnDrawGizmosSelected()
	{
		OnDrawGizmos();
	}

	private void updatePosition3D()
	{
		if (!(attachedTo == null))
		{
			control.transform.position = attachedTo.position;
			control.transform.rotation = attachedTo.rotation;
			control.transform.localScale = attachedTo.localScale;
		}
	}
}
