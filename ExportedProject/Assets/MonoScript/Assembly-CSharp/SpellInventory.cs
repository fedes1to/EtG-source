using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/Examples/Actionbar/Spell Inventory")]
public class SpellInventory : MonoBehaviour
{
	[SerializeField]
	protected string spellName = string.Empty;

	private bool needRefresh = true;

	public string Spell
	{
		get
		{
			return spellName;
		}
		set
		{
			spellName = value;
			refresh();
		}
	}

	private void OnEnable()
	{
		refresh();
		dfControl component = base.gameObject.GetComponent<dfControl>();
		component.SizeChanged += delegate
		{
			needRefresh = true;
		};
	}

	private void LateUpdate()
	{
		if (needRefresh)
		{
			needRefresh = false;
			refresh();
		}
	}

	public void OnResolutionChanged()
	{
		needRefresh = true;
	}

	private void refresh()
	{
		dfControl component = base.gameObject.GetComponent<dfControl>();
		dfScrollPanel dfScrollPanel2 = component.Parent as dfScrollPanel;
		if (dfScrollPanel2 != null)
		{
			component.Width = dfScrollPanel2.Width - (float)dfScrollPanel2.ScrollPadding.horizontal;
		}
		SpellSlot componentInChildren = component.GetComponentInChildren<SpellSlot>();
		dfLabel dfLabel2 = component.Find<dfLabel>("lblCosts");
		dfLabel dfLabel3 = component.Find<dfLabel>("lblName");
		dfLabel dfLabel4 = component.Find<dfLabel>("lblDescription");
		if (dfLabel2 == null)
		{
			throw new Exception("Not found: lblCosts");
		}
		if (dfLabel3 == null)
		{
			throw new Exception("Not found: lblName");
		}
		if (dfLabel4 == null)
		{
			throw new Exception("Not found: lblDescription");
		}
		SpellDefinition spellDefinition = SpellDefinition.FindByName(Spell);
		if (spellDefinition == null)
		{
			componentInChildren.Spell = string.Empty;
			dfLabel2.Text = string.Empty;
			dfLabel3.Text = string.Empty;
			dfLabel4.Text = string.Empty;
		}
		else
		{
			componentInChildren.Spell = spellName;
			dfLabel3.Text = spellDefinition.Name;
			dfLabel2.Text = string.Format("{0}/{1}/{2}", spellDefinition.Cost, spellDefinition.Recharge, spellDefinition.Delay);
			dfLabel4.Text = spellDefinition.Description;
			float a = dfLabel4.RelativePosition.y + dfLabel4.Size.y;
			float b = dfLabel2.RelativePosition.y + dfLabel2.Size.y;
			component.Height = Mathf.Max(a, b) + 5f;
		}
	}
}
