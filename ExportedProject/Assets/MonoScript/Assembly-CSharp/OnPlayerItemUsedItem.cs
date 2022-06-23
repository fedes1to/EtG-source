using UnityEngine;

public class OnPlayerItemUsedItem : PassiveItem
{
	public float ActivationChance = 1f;

	public bool TriggersBlank;

	public bool TriggersRadialBulletBurst;

	[ShowInInspectorIf("TriggersRadialBulletBurst", false)]
	public RadialBurstInterface RadialBurstSettings;

	public float InternalCooldown = 10f;

	private float m_lastUsedTime = -1000f;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnUsedPlayerItem += DoEffect;
		}
	}

	private void DoEffect(PlayerController usingPlayer, PlayerItem usedItem)
	{
		if (Time.realtimeSinceStartup - m_lastUsedTime < InternalCooldown)
		{
			return;
		}
		m_lastUsedTime = Time.realtimeSinceStartup;
		if (Random.value < ActivationChance)
		{
			if (TriggersBlank)
			{
				usingPlayer.ForceBlank();
			}
			if (TriggersRadialBulletBurst)
			{
				RadialBurstSettings.DoBurst(usingPlayer);
			}
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		OnPlayerItemUsedItem component = debrisObject.GetComponent<OnPlayerItemUsedItem>();
		player.OnUsedPlayerItem -= DoEffect;
		component.m_pickedUpThisRun = true;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
