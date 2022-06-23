using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Actionbar/Hover Events")]
public class ActionbarsHoverEvents : MonoBehaviour
{
	private dfControl actionBar;

	private dfControl lastTarget;

	private dfControl target;

	private bool isTooltipVisible;

	public void Start()
	{
		actionBar = GetComponent<dfControl>();
	}

	public void OnMouseHover(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (isTooltipVisible)
		{
			return;
		}
		if (actionBar.Controls.Contains(mouseEvent.Source))
		{
			target = mouseEvent.Source;
			if (target == lastTarget)
			{
				return;
			}
			lastTarget = target;
			isTooltipVisible = true;
			SpellSlot componentInChildren = target.GetComponentInChildren<SpellSlot>();
			if (!string.IsNullOrEmpty(componentInChildren.Spell))
			{
				SpellDefinition spellDefinition = SpellDefinition.FindByName(componentInChildren.Spell);
				if (spellDefinition != null)
				{
					ActionbarsTooltip.Show(spellDefinition);
				}
			}
		}
		else
		{
			lastTarget = null;
		}
	}

	public void OnMouseDown()
	{
		isTooltipVisible = false;
		ActionbarsTooltip.Hide();
		target = null;
	}

	public void OnMouseLeave()
	{
		if (!(target == null))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.y = (float)Screen.height - mousePosition.y;
			if (!target.GetScreenRect().Contains(mousePosition, true))
			{
				isTooltipVisible = false;
				ActionbarsTooltip.Hide();
				target = null;
			}
		}
	}
}
