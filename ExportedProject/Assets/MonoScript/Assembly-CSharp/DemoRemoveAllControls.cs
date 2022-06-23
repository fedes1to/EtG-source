using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Add-Remove Controls/Remove Child Controls")]
public class DemoRemoveAllControls : MonoBehaviour
{
	public dfControl target;

	public void Start()
	{
		if (target == null)
		{
			target = GetComponent<dfControl>();
		}
	}

	public void OnClick()
	{
		while (target.Controls.Count > 0)
		{
			dfControl dfControl2 = target.Controls[0];
			target.RemoveControl(dfControl2);
			Object.DestroyImmediate(dfControl2.gameObject);
		}
	}
}
