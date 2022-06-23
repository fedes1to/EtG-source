using System;
using UnityEngine;

public class StealthOnReloadPressed : MonoBehaviour
{
	public GameObject poofVfx;

	public bool OnlyOnClipEmpty = true;

	[Header("Synergues")]
	public bool SynergyContingent;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	private Gun m_gun;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		if (OnlyOnClipEmpty)
		{
			Gun gun = m_gun;
			gun.OnAutoReload = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnAutoReload, new Action<PlayerController, Gun>(HandleReloadPressedSimple));
		}
		else
		{
			Gun gun2 = m_gun;
			gun2.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun2.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
		}
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		BreakStealth(m_gun.CurrentOwner as PlayerController);
	}

	private void BreakStealth(PlayerController obj)
	{
		obj.PlayEffectOnActor(poofVfx, Vector3.zero, false, true);
		obj.ChangeSpecialShaderFlag(1, 0f);
		obj.OnDidUnstealthyAction -= BreakStealth;
		obj.healthHaver.OnDamaged -= OnDamaged;
		obj.SetIsStealthed(false, "box");
		obj.SetCapableOfStealing(false, "StealthOnReloadPressed");
	}

	private void HandleReloadPressedSimple(PlayerController user, Gun sourceGun)
	{
		HandleReloadPressed(user, sourceGun, false);
	}

	private void HandleReloadPressed(PlayerController user, Gun sourceGun, bool actual)
	{
		if (!SynergyContingent || user.HasActiveBonusSynergy(RequiredSynergy))
		{
			if (SynergyContingent)
			{
				sourceGun.CanSneakAttack = true;
				sourceGun.SneakAttackDamageMultiplier = 4f;
			}
			if (OnlyOnClipEmpty || !m_gun.IsFiring)
			{
				user.PlayEffectOnActor(poofVfx, Vector3.zero, false, true);
				user.ChangeSpecialShaderFlag(1, 1f);
				user.OnDidUnstealthyAction += BreakStealth;
				user.healthHaver.OnDamaged += OnDamaged;
				user.SetIsStealthed(true, "box");
				user.SetCapableOfStealing(true, "StealthOnReloadPressed");
			}
		}
	}
}
