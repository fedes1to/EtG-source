using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Coverflow/Item Info")]
public class DEMO_CoverflowItemInfo : MonoBehaviour
{
	public dfCoverflow scroller;

	public string[] descriptions;

	private dfLabel label;

	public void Start()
	{
		label = GetComponent<dfLabel>();
	}

	private void Update()
	{
		if (!(scroller == null) && descriptions != null && descriptions.Length != 0)
		{
			int num = Mathf.Max(0, Mathf.Min(descriptions.Length - 1, scroller.selectedIndex));
			label.Text = descriptions[num];
		}
	}
}
