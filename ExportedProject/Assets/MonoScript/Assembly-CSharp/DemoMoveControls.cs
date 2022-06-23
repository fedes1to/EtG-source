using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Add-Remove Controls/Move Child Control")]
public class DemoMoveControls : MonoBehaviour
{
	public dfScrollPanel from;

	public dfScrollPanel to;

	public void OnClick()
	{
		from.SuspendLayout();
		to.SuspendLayout();
		while (from.Controls.Count > 0)
		{
			dfControl dfControl2 = from.Controls[0];
			from.RemoveControl(dfControl2);
			dfControl2.ZOrder = -1;
			to.AddControl(dfControl2);
		}
		from.ResumeLayout();
		to.ResumeLayout();
		from.ScrollPosition = Vector2.zero;
	}
}
