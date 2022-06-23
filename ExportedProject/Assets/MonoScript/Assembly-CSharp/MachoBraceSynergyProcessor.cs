using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class MachoBraceSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public float DamageMultiplier = 1.25f;

	public GameObject DustUpVFX;

	public GameObject BurstVFX;

	public GameObject OverheadVFX;

	public bool TriggersOnStandingStill;

	public float StandStillTimer = 3f;

	public bool TriggersOnTableFlip;

	public float FlipDuration = 3f;

	public bool TriggersOnAimRotation;

	private float m_lastGunAngle;

	private float m_cumulativeGunRotation;

	private float m_zeroRotationTime;

	private float m_standStillTimer;

	private PassiveItem m_item;

	private bool m_initialized;

	private PlayerController m_lastOwner;

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
		m_item = GetComponent<PassiveItem>();
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || !PhysicsEngine.HasInstance)
		{
			return;
		}
		bool flag = (bool)m_item.Owner && m_item.Owner.HasActiveBonusSynergy(RequiredSynergy);
		if (flag && !m_initialized)
		{
			Initialize(m_item.Owner);
		}
		else if (m_initialized && !flag)
		{
			if ((bool)m_lastOwner)
			{
				m_lastOwner.PostProcessProjectile -= HandleProjectileFired;
				m_lastOwner.PostProcessBeamTick -= HandleBeamTick;
				PlayerController lastOwner = m_lastOwner;
				lastOwner.OnTableFlipped = (Action<FlippableCover>)Delegate.Remove(lastOwner.OnTableFlipped, new Action<FlippableCover>(HandleTableFlip));
			}
			m_initialized = false;
			m_lastOwner = null;
		}
		else
		{
			if (!m_initialized || !flag)
			{
				return;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
			{
				m_hasUsedShot = true;
				return;
			}
			if (TriggersOnStandingStill && m_destroyVFXSemaphore <= 0 && m_item.Owner.Velocity.magnitude < 0.05f)
			{
				m_standStillTimer += BraveTime.DeltaTime;
				if (m_standStillTimer > StandStillTimer)
				{
					ForceTrigger(m_item.Owner);
					m_standStillTimer = 0f;
				}
			}
			if (!TriggersOnAimRotation || !m_item.Owner.CurrentGun)
			{
				return;
			}
			float value = Vector2.SignedAngle(BraveMathCollege.DegreesToVector((m_item.Owner.unadjustedAimPoint.XY() - m_item.Owner.CenterPosition).ToAngle()), BraveMathCollege.DegreesToVector(m_lastGunAngle));
			value = Mathf.Clamp(value, -90f, 90f);
			if (Mathf.Abs(value) < 120f * BraveTime.DeltaTime)
			{
				m_zeroRotationTime += Time.deltaTime;
				if (m_zeroRotationTime < 0.0333f)
				{
					return;
				}
				value = 0f;
				m_cumulativeGunRotation = 0f;
			}
			else
			{
				m_zeroRotationTime = 0f;
			}
			m_lastGunAngle = (m_item.Owner.unadjustedAimPoint.XY() - m_item.Owner.CenterPosition).ToAngle();
			m_cumulativeGunRotation += value;
			if (m_cumulativeGunRotation > 360f)
			{
				m_cumulativeGunRotation = 0f;
				ForceTrigger(m_item.Owner);
			}
			else if (m_cumulativeGunRotation < -360f)
			{
				m_cumulativeGunRotation = 0f;
				ForceTrigger(m_item.Owner);
			}
		}
	}

	public void Initialize(PlayerController player)
	{
		m_initialized = true;
		player.PostProcessProjectile += HandleProjectileFired;
		player.PostProcessBeamTick += HandleBeamTick;
		player.OnTableFlipped = (Action<FlippableCover>)Delegate.Combine(player.OnTableFlipped, new Action<FlippableCover>(HandleTableFlip));
		m_lastOwner = player;
		if ((bool)player.CurrentGun)
		{
			m_lastGunAngle = player.CurrentGun.CurrentAngle;
		}
	}

	private void HandleTableFlip(FlippableCover obj)
	{
		if (TriggersOnTableFlip && (bool)m_item.Owner)
		{
			ForceTrigger(m_item.Owner);
		}
	}

	private void HandleBeamTick(BeamController arg1, SpeculativeRigidbody arg2, float arg3)
	{
		if ((bool)m_item.Owner && !m_hasUsedShot)
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
		if (!m_item.Owner || m_destroyVFXSemaphore <= 0)
		{
			return;
		}
		firedProjectile.AdjustPlayerProjectileTint(new Color(1f, 0.9f, 0f), 50);
		if (!m_hasUsedShot)
		{
			m_hasUsedShot = true;
			if ((bool)m_item.Owner && (bool)DustUpVFX)
			{
				m_item.Owner.PlayEffectOnActor(DustUpVFX, new Vector3(0f, -0.625f, 0f), false);
				AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Trigger_01", base.gameObject);
			}
			if ((bool)m_item.Owner && (bool)BurstVFX)
			{
				m_item.Owner.PlayEffectOnActor(BurstVFX, new Vector3(0f, 0.375f, 0f), false);
			}
		}
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
		if (m_destroyVFXSemaphore > 5)
		{
			yield break;
		}
		EnableVFX(target);
		m_destroyVFXSemaphore++;
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Active_01", base.gameObject);
		}
		m_hasUsedShot = false;
		m_beamTickElapsed = 0f;
		float elapsed = 0f;
		if ((bool)target)
		{
			target.ownerlessStatModifiers.Add(m_damageStat);
			target.stats.RecalculateStats(target);
		}
		if (TriggersOnStandingStill)
		{
			while ((bool)target && target.specRigidbody.Velocity.magnitude < 0.05f && !m_hasUsedShot)
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
		}
		else if (TriggersOnTableFlip || TriggersOnAimRotation)
		{
			while ((bool)target && elapsed < FlipDuration && !m_hasUsedShot)
			{
				elapsed += BraveTime.DeltaTime;
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.ownerlessStatModifiers.Remove(m_damageStat);
		}
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Fade_01", base.gameObject);
		}
		if ((bool)target)
		{
			target.stats.RecalculateStats(target);
		}
		m_destroyVFXSemaphore--;
		if (m_hasUsedShot)
		{
			m_destroyVFXSemaphore = 0;
		}
		if ((bool)target)
		{
			DisableVFX(target);
		}
	}
}
