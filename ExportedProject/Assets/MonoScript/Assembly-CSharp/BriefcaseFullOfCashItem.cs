using System.Collections.Generic;
using UnityEngine;

public class BriefcaseFullOfCashItem : PassiveItem
{
	public int CurrencyAmount = 200;

	public int MetaCurrencyAmount = 3;

	private bool m_hasTriggered;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (m_pickedUpThisRun)
			{
				m_hasTriggered = true;
			}
			if (!m_pickedUpThisRun && !m_hasTriggered)
			{
				m_hasTriggered = true;
				player.carriedConsumables.Currency += CurrencyAmount;
				LootEngine.SpawnCurrency(player.CenterPosition, MetaCurrencyAmount, true, Vector2.down, 45f, 0.5f, 0.25f);
			}
			base.Pickup(player);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<BriefcaseFullOfCashItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp)
		{
		}
		base.OnDestroy();
	}

	public override void MidGameSerialize(List<object> data)
	{
		base.MidGameSerialize(data);
		data.Add(m_hasTriggered);
	}

	public override void MidGameDeserialize(List<object> data)
	{
		base.MidGameDeserialize(data);
		if (data.Count == 1)
		{
			m_hasTriggered = (bool)data[0];
		}
	}
}
