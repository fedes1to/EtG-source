using System;
using UnityEngine;

public class BurningHandModifier : MonoBehaviour
{
	public float MinDamageMultiplier = 1f;

	public float MaxDamageMultiplier = 10f;

	public float MinScale = 0.5f;

	public float MaxScale = 2.5f;

	public float MaxRoll = 13f;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.PostProcessProjectile = (Action<Projectile>)Delegate.Combine(gun.PostProcessProjectile, new Action<Projectile>(HandleProjectileMod));
	}

	private void HandleProjectileMod(Projectile p)
	{
		int num = UnityEngine.Random.Range(1, 7) + UnityEngine.Random.Range(1, 7);
		int num2 = 0;
		if (m_gun.CurrentOwner is PlayerController)
		{
			switch ((m_gun.CurrentOwner as PlayerController).characterIdentity)
			{
			case PlayableCharacters.Robot:
				num2 = 1;
				break;
			case PlayableCharacters.Bullet:
				num2 = -1;
				break;
			}
		}
		int num3 = Mathf.Clamp(num + num2, 1, 100);
		int count = 0;
		if (PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.LOADED_DICE, out count))
		{
			num3 = Mathf.Max(12, num3);
		}
		float num4 = Mathf.Lerp(MinScale, MaxScale, Mathf.Clamp01((float)num3 / MaxRoll));
		float num5 = Mathf.Lerp(MinDamageMultiplier, MaxDamageMultiplier, Mathf.Clamp01((float)num3 / MaxRoll));
		p.AdditionalScaleMultiplier *= num4;
		p.baseData.damage *= num5;
	}
}
