using UnityEngine;

public class SimpleRenamer : MonoBehaviour
{
	public string OverrideName;

	public void Start()
	{
		base.name = OverrideName;
	}
}
