using System.Collections;
using UnityEngine;

public class KiPulseItem : PlayerItem
{
	public string IdleAnimation;

	public string CooldownAnimation;

	public string InactiveAnimation;

	public string ActiveAnimation;

	public float DetectionRadius = 0.75f;

	public float PreTriggerPeriod;

	public float TriggerPeriod = 0.25f;

	public float SynergyTriggerPeriod;

	public GameObject KiOverheadVFX;

	private GameObject m_extantVFX;

	private int m_activated;

	public override void Pickup(PlayerController player)
	{
		base.Pickup(player);
		m_activated = 0;
		player.OnIsRolling += HandleRollFrame;
		player.OnDodgedBeam += HandleDodgedBeam;
	}

	private void HandleDodgedBeam(BeamController beam, PlayerController player)
	{
		if (!base.IsOnCooldown && player.CurrentRollState == PlayerController.DodgeRollState.InAir)
		{
			StartCoroutine(Activate());
		}
	}

	private void HandleRollFrame(PlayerController obj)
	{
		if (base.IsOnCooldown || m_activated > 0 || obj.CurrentRollState != PlayerController.DodgeRollState.InAir)
		{
			return;
		}
		Vector2 centerPosition = obj.CenterPosition;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.Owner is AIActor)
			{
				float sqrMagnitude = (projectile.transform.position.XY() - centerPosition).sqrMagnitude;
				if (sqrMagnitude < DetectionRadius)
				{
					StartCoroutine(Activate());
					break;
				}
			}
		}
	}

	public override bool CanBeUsed(PlayerController user)
	{
		return base.CanBeUsed(user) && m_activated > 0 && !base.IsOnCooldown;
	}

	private void HandleDodgedProjectile(Projectile obj)
	{
		if (!base.IsOnCooldown)
		{
			StartCoroutine(Activate());
		}
	}

	private IEnumerator Activate()
	{
		if (PreTriggerPeriod > 0f)
		{
			yield return new WaitForSeconds(PreTriggerPeriod);
		}
		m_activated++;
		float modifiedTriggerPeriod = ((!(LastOwner != null) || !LastOwner.HasActiveBonusSynergy(CustomSynergyType.GUON_UPGRADE_WHITE)) ? TriggerPeriod : SynergyTriggerPeriod);
		yield return new WaitForSeconds(modifiedTriggerPeriod);
		m_activated--;
		m_activated = Mathf.Max(m_activated, 0);
	}

	private void LateUpdate()
	{
		bool flag = false;
		if (!m_pickedUp)
		{
			if (!base.spriteAnimator.IsPlaying(IdleAnimation))
			{
				base.spriteAnimator.Play(IdleAnimation);
			}
		}
		else if (base.IsOnCooldown)
		{
			if (!base.spriteAnimator.IsPlaying(CooldownAnimation))
			{
				base.spriteAnimator.Play(CooldownAnimation);
			}
		}
		else if (m_activated <= 0)
		{
			if (!base.spriteAnimator.IsPlaying(InactiveAnimation))
			{
				base.spriteAnimator.Play(InactiveAnimation);
			}
		}
		else
		{
			flag = true;
			if (!base.spriteAnimator.IsPlaying(ActiveAnimation))
			{
				base.spriteAnimator.Play(ActiveAnimation);
			}
		}
		if (flag && (bool)LastOwner)
		{
			if (!m_extantVFX)
			{
				m_extantVFX = LastOwner.PlayEffectOnActor(KiOverheadVFX, new Vector3(-0.0625f, 1.25f, 0f));
			}
		}
		else if (!flag && (bool)m_extantVFX && !m_extantVFX.GetComponent<tk2dSpriteAnimator>().Playing)
		{
			Object.Destroy(m_extantVFX);
			m_extantVFX = null;
		}
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		user.ForceBlank();
		if ((bool)m_extantVFX)
		{
			m_extantVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject(string.Empty);
		}
	}

	protected override void AfterCooldownApplied(PlayerController user)
	{
		base.AfterCooldownApplied(user);
		if (user.HasActiveBonusSynergy(CustomSynergyType.GUON_UPGRADE_WHITE))
		{
			base.CurrentDamageCooldown /= 2f;
		}
	}

	protected override void OnPreDrop(PlayerController user)
	{
		base.OnPreDrop(user);
		user.OnIsRolling -= HandleRollFrame;
		user.OnDodgedProjectile -= HandleDodgedProjectile;
		user.OnDodgedBeam -= HandleDodgedBeam;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)LastOwner)
		{
			LastOwner.OnIsRolling -= HandleRollFrame;
			LastOwner.OnDodgedProjectile -= HandleDodgedProjectile;
			LastOwner.OnDodgedBeam -= HandleDodgedBeam;
		}
	}
}
