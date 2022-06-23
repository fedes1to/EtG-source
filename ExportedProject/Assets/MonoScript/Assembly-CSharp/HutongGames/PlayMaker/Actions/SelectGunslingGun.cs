using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(".NPCs")]
	public class SelectGunslingGun : BraveFsmStateAction
	{
		[Tooltip("Loot table to choose an item from.")]
		public GenericLootTable lootTable;

		[NonSerialized]
		public GameObject SelectedObject;

		public override void Reset()
		{
			lootTable = null;
		}

		public override void OnEnter()
		{
			if (SelectedObject == null)
			{
				SelectedObject = lootTable.SelectByWeightWithoutDuplicatesFullPrereqs(null, false);
			}
			if (SelectedObject == null)
			{
				SelectedObject = lootTable.defaultItemDrops.elements[UnityEngine.Random.Range(0, lootTable.defaultItemDrops.elements.Count)].gameObject;
			}
			EncounterTrackable component = SelectedObject.GetComponent<EncounterTrackable>();
			if (component != null)
			{
				SetReplacementString(component.journalData.GetPrimaryDisplayName());
			}
			Finish();
		}
	}
}
