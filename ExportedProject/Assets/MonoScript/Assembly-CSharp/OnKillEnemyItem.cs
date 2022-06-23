using UnityEngine;

public class OnKillEnemyItem : PassiveItem
{
	public enum ActivationStyle
	{
		RANDOM_CHANCE,
		EVERY_X_ENEMIES
	}

	public ActivationStyle activationStyle;

	[ShowInInspectorIf("activationStyle", 0, false)]
	public float chanceOfActivating = 1f;

	[ShowInInspectorIf("activationStyle", 1, false)]
	public int numEnemiesBeforeActivation = 3;

	public int ammoToGain = 1;

	public float healthToGain;

	private int m_activations;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnKilledEnemy += OnKilledEnemy;
		}
	}

	public void OnKilledEnemy(PlayerController source)
	{
		m_activations++;
		if (activationStyle == ActivationStyle.RANDOM_CHANCE)
		{
			if (Random.value < chanceOfActivating)
			{
				DoEffect(source);
			}
		}
		else if (activationStyle == ActivationStyle.EVERY_X_ENEMIES && m_activations % numEnemiesBeforeActivation == 0)
		{
			DoEffect(source);
		}
	}

	private void DoEffect(PlayerController source)
	{
		if (ammoToGain > 0 && source.CurrentGun != null)
		{
			source.CurrentGun.GainAmmo(Mathf.Max(1, (int)((float)ammoToGain * 0.01f * (float)source.CurrentGun.AdjustedMaxAmmo)));
		}
		if (healthToGain > 0f)
		{
			source.healthHaver.ApplyHealing(healthToGain);
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<OnKillEnemyItem>().m_pickedUpThisRun = true;
		player.OnKilledEnemy -= OnKilledEnemy;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
