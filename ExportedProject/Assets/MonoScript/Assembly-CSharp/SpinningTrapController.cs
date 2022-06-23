using UnityEngine;

public class SpinningTrapController : TrapController
{
	public GameObject baseObject;

	public GameObject spinningObject;

	public float secondsPerRotation;

	public bool doQuantize;

	public float multiplesOf;

	private float m_rotation;

	public void Update()
	{
		m_rotation += 360f * BraveTime.DeltaTime / secondsPerRotation;
		float z = m_rotation;
		if (doQuantize)
		{
			z = BraveMathCollege.QuantizeFloat(m_rotation, multiplesOf);
		}
		spinningObject.transform.rotation = Quaternion.Euler(0f, 0f, z);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
