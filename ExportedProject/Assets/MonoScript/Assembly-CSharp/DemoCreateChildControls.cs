using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Add-Remove Controls/Create Child Control")]
public class DemoCreateChildControls : MonoBehaviour
{
	public dfScrollPanel target;

	private int colorNum;

	private Color32[] colors = new Color32[4]
	{
		Color.white,
		Color.red,
		Color.green,
		Color.black
	};

	public void Start()
	{
		if (target == null)
		{
			target = GetComponent<dfScrollPanel>();
		}
	}

	public void OnClick()
	{
		for (int i = 0; i < 10; i++)
		{
			dfButton dfButton2 = target.AddControl<dfButton>();
			dfButton2.NormalBackgroundColor = colors[colorNum % colors.Length];
			dfButton2.BackgroundSprite = "button-normal";
			dfButton2.Text = string.Format("Button {0}", dfButton2.ZOrder);
			dfButton2.Anchor = dfAnchorStyle.Left | dfAnchorStyle.Right;
			dfButton2.Width = target.Width - (float)target.ScrollPadding.horizontal;
		}
		colorNum++;
	}

	public void OnDoubleClick()
	{
		OnClick();
	}
}
