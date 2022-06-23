using UnityEngine;

public class MoveAmmoToClipItem : PassiveItem
{
	public int BulletsToMove = 1;

	public bool TriggerOnRoll;

	public float ActivationChance = 1f;

	public NumericSynergyMultiplier[] moveMultipliers;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			if (TriggerOnRoll)
			{
				player.OnRollStarted += HandleRollStarted;
			}
			base.Pickup(player);
		}
	}

	private void HandleRollStarted(PlayerController arg1, Vector2 arg2)
	{
		DoEffect(arg1);
	}

	private int GetBulletsToMove(PlayerController source)
	{
		float num = BulletsToMove;
		for (int i = 0; i < moveMultipliers.Length; i++)
		{
			if ((bool)source && source.HasActiveBonusSynergy(moveMultipliers[i].RequiredSynergy))
			{
				num *= moveMultipliers[i].SynergyMultiplier;
			}
		}
		return Mathf.RoundToInt(num);
	}

	private void DoEffect(PlayerController source)
	{
		if (Random.value < ActivationChance && source.CurrentGun != null)
		{
			source.CurrentGun.MoveBulletsIntoClip(GetBulletsToMove(source));
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		player.OnRollStarted -= HandleRollStarted;
		debrisObject.GetComponent<MoveAmmoToClipItem>().m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_owner != null)
		{
			m_owner.OnRollStarted -= HandleRollStarted;
		}
	}
}
