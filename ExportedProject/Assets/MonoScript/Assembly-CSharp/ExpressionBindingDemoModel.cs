using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Data Binding/Expression Binding Model")]
public class ExpressionBindingDemoModel : MonoBehaviour
{
	private dfListbox list;

	public List<string> SpellsLearned { get; set; }

	public SpellDefinition SelectedSpell
	{
		get
		{
			return SpellDefinition.FindByName(SpellsLearned[list.SelectedIndex]);
		}
	}

	private void Awake()
	{
		list = GetComponentInChildren<dfListbox>();
		list.SelectedIndex = 0;
		SpellsLearned = (from x in SpellDefinition.AllSpells
			orderby x.Name
			select x.Name).ToList();
	}
}
