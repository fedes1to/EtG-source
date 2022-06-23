using UnityEngine;

public class StoutBulletsItem : PassiveItem
{
	public float RangeCap = 7f;

	public float MaxDamageIncrease = 1.75f;

	public float MinDamageIncrease = 1.125f;

	public float ScaleIncrease = 1.5f;

	public float DescaleAmount = 0.5f;

	public float DamageCutOnDescale = 2f;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			m_player = player;
			base.Pickup(player);
			player.PostProcessProjectile += PostProcessProjectile;
			player.PostProcessBeam += PostProcessBeam;
		}
	}

	private void PostProcessBeam(BeamController obj)
	{
		if ((bool)obj)
		{
			Projectile projectile = obj.projectile;
			if ((bool)projectile)
			{
				PostProcessProjectile(projectile, 1f);
			}
		}
	}

	private void PostProcessProjectile(Projectile obj, float effectChanceScalar)
	{
		float num = Mathf.Max(0f, obj.baseData.range - RangeCap);
		float num2 = Mathf.Lerp(MinDamageIncrease, MaxDamageIncrease, Mathf.Clamp01(num / 15f));
		obj.OnPostUpdate += HandlePostUpdate;
		obj.AdditionalScaleMultiplier *= ScaleIncrease;
		obj.baseData.damage *= num2;
	}

	private void HandlePostUpdate(Projectile proj)
	{
		if ((bool)proj && proj.GetElapsedDistance() > RangeCap)
		{
			proj.RuntimeUpdateScale(DescaleAmount);
			proj.baseData.damage /= DamageCutOnDescale;
			proj.OnPostUpdate -= HandlePostUpdate;
		}
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		m_player = null;
		debrisObject.GetComponent<StoutBulletsItem>().m_pickedUpThisRun = true;
		player.PostProcessProjectile -= PostProcessProjectile;
		player.PostProcessBeam -= PostProcessBeam;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)m_player)
		{
			m_player.PostProcessProjectile -= PostProcessProjectile;
			m_player.PostProcessBeam -= PostProcessBeam;
		}
	}
}
