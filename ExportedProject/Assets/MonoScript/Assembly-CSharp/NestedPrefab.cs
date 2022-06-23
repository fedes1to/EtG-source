using UnityEngine;

public class NestedPrefab : BraveBehaviour
{
	public Vector3 localPosition = Vector3.zero;

	public Vector3 localRotation = Vector3.zero;

	public Vector3 localScale = Vector3.one;

	public GameObject prefab;

	public void Awake()
	{
		GameObject gameObject = Object.Instantiate(prefab, base.transform.position, Quaternion.identity);
		gameObject.transform.parent = base.transform;
		if (localScale != Vector3.zero)
		{
			gameObject.transform.localScale = localScale;
		}
		if (localRotation != Vector3.zero)
		{
			gameObject.transform.localRotation = Quaternion.Euler(localRotation);
		}
		if (localScale != Vector3.one)
		{
			gameObject.transform.localScale = localScale;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
