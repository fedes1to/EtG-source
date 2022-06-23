using UnityEngine;

public class GunAttachmentUnbreaker : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		if (base.gameObject.transform.position.y < 0f)
		{
			base.gameObject.transform.position = new Vector3(base.transform.position.x, Mathf.Abs(base.gameObject.transform.position.y), base.transform.position.z);
		}
	}
}
