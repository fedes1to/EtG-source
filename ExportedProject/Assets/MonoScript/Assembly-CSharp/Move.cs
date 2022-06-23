using UnityEngine;

public class Move : MonoBehaviour
{
	private void Update()
	{
		Vector3 zero = Vector3.zero;
		float axis = Input.GetAxis("Horizontal");
		zero = Vector3.right * axis;
		base.transform.Translate(zero * BraveTime.DeltaTime * 5f, Space.World);
		float axis2 = Input.GetAxis("Vertical");
		zero = Vector3.forward * axis2;
		base.transform.Translate(zero * BraveTime.DeltaTime * 5f, Space.World);
	}
}
