using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Containers/Auto-Arrange Item")]
public class AutoArrangeDemoItem : MonoBehaviour
{
	private dfButton control;

	private dfAnimatedVector2 size;

	private bool isExpanded;

	private void Start()
	{
		control = GetComponent<dfButton>();
		size = new dfAnimatedVector2(control.Size, control.Size, 0.33f);
		control.Text = "#" + (control.ZOrder + 1);
	}

	private void Update()
	{
		control.Size = size.Value.RoundToInt();
	}

	private void OnClick()
	{
		Toggle();
	}

	public void Expand()
	{
		size.StartValue = size.EndValue;
		size.EndValue = new Vector2(128f, 96f);
		isExpanded = true;
	}

	public void Collapse()
	{
		size.StartValue = size.EndValue;
		size.EndValue = new Vector2(48f, 48f);
		isExpanded = false;
	}

	public void Toggle()
	{
		if (isExpanded)
		{
			Collapse();
		}
		else
		{
			Expand();
		}
	}
}
