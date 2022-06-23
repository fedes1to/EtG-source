using UnityEngine;

public class HealPlayerItem : PlayerItem
{
	public float healingAmount = 1f;

	public GameObject healVFX;

	public bool HealsBothPlayers;

	public bool DoesRevive;

	public bool ProvidesTemporaryDamageBuff;

	public float TemporaryDamageMultiplier = 2f;

	public bool IsOrange;

	public bool HasHealingSynergy;

	[LongNumericEnum]
	public CustomSynergyType HealingSynergyRequired;

	[ShowInInspectorIf("HasHealingSynergy", false)]
	public float synergyHealingAmount = 5f;

	protected PlayerController m_buffedTarget;

	protected StatModifier m_temporaryModifier;

	public override bool CanBeUsed(PlayerController user)
	{
		if (DoesRevive && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && GameManager.Instance.PrimaryPlayer.healthHaver.IsAlive && GameManager.Instance.SecondaryPlayer.healthHaver.IsAlive)
		{
			return false;
		}
		return base.CanBeUsed(user);
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
		if (base.transform.childCount > 0)
		{
			SimpleSpriteRotator[] componentsInChildren = GetComponentsInChildren<SimpleSpriteRotator>(true);
			if (componentsInChildren.Length > 0)
			{
				componentsInChildren[0].gameObject.SetActive(true);
			}
		}
	}

	public override void Pickup(PlayerController player)
	{
		if (base.transform.childCount > 0)
		{
			SimpleSpriteRotator componentInChildren = GetComponentInChildren<SimpleSpriteRotator>();
			if ((bool)componentInChildren)
			{
				componentInChildren.gameObject.SetActive(false);
			}
		}
		base.Pickup(player);
	}

	private void RemoveTemporaryBuff(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		m_buffedTarget.healthHaver.OnDamaged -= RemoveTemporaryBuff;
		m_buffedTarget.ownerlessStatModifiers.Remove(m_temporaryModifier);
		m_buffedTarget.stats.RecalculateStats(m_buffedTarget);
		m_temporaryModifier = null;
		m_buffedTarget = null;
	}

	private float GetHealingAmount(PlayerController user)
	{
		if (HasHealingSynergy && user.HasActiveBonusSynergy(HealingSynergyRequired))
		{
			return synergyHealingAmount;
		}
		return healingAmount;
	}

	protected override void DoEffect(PlayerController user)
	{
		if (DoesRevive && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(user);
			if (otherPlayer.healthHaver.IsDead)
			{
				otherPlayer.ResurrectFromBossKill();
			}
		}
		if (IsOrange)
		{
			StatModifier statModifier = new StatModifier();
			statModifier.amount = 1f;
			statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
			statModifier.statToBoost = PlayerStats.StatType.Health;
			user.ownerlessStatModifiers.Add(statModifier);
			user.stats.RecalculateStats(user);
			AkSoundEngine.PostEvent("Play_OBJ_orange_love_01", base.gameObject);
		}
		if (ProvidesTemporaryDamageBuff && m_temporaryModifier == null)
		{
			m_buffedTarget = user;
			m_temporaryModifier = new StatModifier();
			m_temporaryModifier.statToBoost = PlayerStats.StatType.Damage;
			m_temporaryModifier.amount = TemporaryDamageMultiplier;
			m_temporaryModifier.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
			m_temporaryModifier.isMeatBunBuff = true;
			user.ownerlessStatModifiers.Add(m_temporaryModifier);
			user.stats.RecalculateStats(user);
			user.healthHaver.OnDamaged += RemoveTemporaryBuff;
		}
		float num = GetHealingAmount(user);
		if (!(num > 0f))
		{
			return;
		}
		if (HealsBothPlayers)
		{
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if (GameManager.Instance.AllPlayers[i].healthHaver.IsAlive)
				{
					GameManager.Instance.AllPlayers[i].healthHaver.ApplyHealing(num);
					GameManager.Instance.AllPlayers[i].PlayEffectOnActor(healVFX, Vector3.zero);
				}
			}
		}
		else
		{
			user.healthHaver.ApplyHealing(num);
			if (healVFX != null)
			{
				user.PlayEffectOnActor(healVFX, Vector3.zero);
			}
		}
		AkSoundEngine.PostEvent("Play_OBJ_med_kit_01", base.gameObject);
	}

	private void LateUpdate()
	{
		if (IsOrange)
		{
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unpixelated"));
			base.sprite.renderer.enabled = false;
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
