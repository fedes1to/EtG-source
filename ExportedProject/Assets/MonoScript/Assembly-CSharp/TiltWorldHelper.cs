using UnityEngine;

public class TiltWorldHelper : BraveBehaviour
{
	public float HeightOffGround;

	public bool DoForceLayer;

	public string ForceLayer = "Unoccluded";

	private void Update()
	{
		base.transform.position = base.transform.position.WithZ(base.transform.position.y - HeightOffGround);
		base.transform.rotation = Quaternion.identity;
		if (DoForceLayer)
		{
			base.gameObject.layer = LayerMask.NameToLayer(ForceLayer);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
