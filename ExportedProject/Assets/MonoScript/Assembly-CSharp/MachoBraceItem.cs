using System.Collections;
using UnityEngine;

public class MachoBraceItem : PassiveItem
{
	public float DamageMultiplier = 1.25f;

	public float DamageBoostDuration = 1.5f;

	public GameObject DustUpVFX;

	public GameObject BurstVFX;

	public GameObject OverheadVFX;

	private bool m_hasUsedShot;

	private float m_beamTickElapsed;

	private StatModifier m_damageStat;

	private GameObject m_instanceVFX;

	private int m_destroyVFXSemaphore;

	private void Awake()
	{
		m_damageStat = new StatModifier();
		m_damageStat.statToBoost = PlayerStats.StatType.Damage;
		m_damageStat.modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE;
		m_damageStat.amount = DamageMultiplier;
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			player.OnRollStarted += HandleRollStarted;
			player.PostProcessProjectile += HandleProjectileFired;
			player.PostProcessBeamTick += HandleBeamTick;
			base.Pickup(player);
		}
	}

	private void HandleBeamTick(BeamController arg1, SpeculativeRigidbody arg2, float arg3)
	{
		if (!m_hasUsedShot)
		{
			m_beamTickElapsed += BraveTime.DeltaTime;
			if (m_beamTickElapsed > 1f)
			{
				m_hasUsedShot = true;
			}
		}
	}

	private void HandleProjectileFired(Projectile firedProjectile, float arg2)
	{
		if (m_destroyVFXSemaphore <= 0)
		{
			return;
		}
		firedProjectile.AdjustPlayerProjectileTint(new Color(1f, 0.9f, 0f), 50);
		if (!m_hasUsedShot)
		{
			m_hasUsedShot = true;
			if ((bool)base.Owner && (bool)DustUpVFX)
			{
				base.Owner.PlayEffectOnActor(DustUpVFX, new Vector3(0f, -0.625f, 0f), false);
				AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Trigger_01", base.gameObject);
			}
			if ((bool)base.Owner && (bool)BurstVFX)
			{
				base.Owner.PlayEffectOnActor(BurstVFX, new Vector3(0f, 0.375f, 0f), false);
			}
		}
	}

	private void HandleRollStarted(PlayerController source, Vector2 arg2)
	{
		source.StartCoroutine(HandleDamageBoost(source));
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject result = base.Drop(player);
		player.OnRollStarted -= HandleRollStarted;
		return result;
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.OnRollStarted -= HandleRollStarted;
		}
		base.OnDestroy();
	}

	public void EnableVFX(PlayerController target)
	{
		if (m_destroyVFXSemaphore == 0)
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", new Color(99f, 99f, 0f));
			}
			if ((bool)OverheadVFX && !m_instanceVFX)
			{
				m_instanceVFX = target.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
			}
		}
	}

	public void DisableVFX(PlayerController target)
	{
		if (m_destroyVFXSemaphore == 0)
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
			}
			if (!m_hasUsedShot)
			{
			}
			if ((bool)m_instanceVFX)
			{
				SpawnManager.Despawn(m_instanceVFX);
				m_instanceVFX = null;
			}
		}
	}

	public void ForceTrigger(PlayerController target)
	{
		target.StartCoroutine(HandleDamageBoost(target));
	}

	private IEnumerator HandleDamageBoost(PlayerController target)
	{
		EnableVFX(target);
		m_destroyVFXSemaphore++;
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Active_01", base.gameObject);
		}
		m_hasUsedShot = false;
		while (target.IsDodgeRolling)
		{
			yield return null;
		}
		m_beamTickElapsed = 0f;
		float elapsed = 0f;
		target.ownerlessStatModifiers.Add(m_damageStat);
		target.stats.RecalculateStats(target);
		while (elapsed < DamageBoostDuration && !m_hasUsedShot)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		target.ownerlessStatModifiers.Remove(m_damageStat);
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Fade_01", base.gameObject);
		}
		target.stats.RecalculateStats(target);
		m_destroyVFXSemaphore--;
		if (m_hasUsedShot)
		{
			m_destroyVFXSemaphore = 0;
		}
		DisableVFX(target);
	}
}
