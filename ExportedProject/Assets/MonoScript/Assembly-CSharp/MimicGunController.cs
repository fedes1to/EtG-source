using System;
using System.Collections;
using UnityEngine;

public class MimicGunController : MonoBehaviour
{
	public float DamageRequired = 300f;

	public GameObject BecomeMimicVFX;

	public GameObject UnbecomeMimicVfx;

	[Header("Audio")]
	public string AcquisitionAudioEvent;

	public string RefillingAmmoAudioEvent;

	private Gun m_gun;

	private bool m_initialized;

	private float m_damageDealt;

	private Gun m_sourceGun;

	private bool m_selfRefilling;

	private bool m_isClearing;

	public void Initialize(PlayerController p, Gun sourceGun)
	{
		m_initialized = true;
		m_gun = GetComponent<Gun>();
		p.inventory.GunLocked.AddOverride("mimic gun");
		p.OnDealtDamage += HandleDealtDamage;
		Gun gun = m_gun;
		gun.OnAmmoChanged = (Action<PlayerController, Gun>)Delegate.Combine(gun.OnAmmoChanged, new Action<PlayerController, Gun>(HandleAmmoChanged));
		m_sourceGun = sourceGun;
		if (!string.IsNullOrEmpty(AcquisitionAudioEvent))
		{
			AkSoundEngine.PostEvent(AcquisitionAudioEvent, base.gameObject);
		}
		if ((bool)BecomeMimicVFX)
		{
			SpawnManager.SpawnVFX(BecomeMimicVFX, m_gun.GetSprite().WorldCenter, Quaternion.identity);
		}
		m_gun.OverrideAnimations = true;
		StartCoroutine(HandleDeferredAnimationOverride(1f));
		m_gun.spriteAnimator.PlayForDuration("mimic_gun_intro", 1f, m_gun.idleAnimation);
	}

	private void Update()
	{
		if (!m_isClearing && (bool)m_gun && m_gun.ammo <= 0)
		{
			if (!string.IsNullOrEmpty(RefillingAmmoAudioEvent))
			{
				AkSoundEngine.PostEvent(RefillingAmmoAudioEvent, base.gameObject);
			}
			m_gun.OverrideAnimations = true;
			StartCoroutine(HandleDeferredAnimationOverride(3f));
			m_gun.spriteAnimator.PlayForDuration("mimic_gun_laugh", 3f, m_gun.idleAnimation);
			m_selfRefilling = true;
			m_gun.GainAmmo(m_gun.AdjustedMaxAmmo);
			m_selfRefilling = false;
		}
	}

	public void OnDestroy()
	{
		if ((bool)m_gun)
		{
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if ((bool)playerController)
			{
				playerController.OnDealtDamage -= HandleDealtDamage;
				playerController.inventory.GunLocked.RemoveOverride("mimic gun");
			}
		}
	}

	private IEnumerator HandleDeferredAnimationOverride(float t)
	{
		yield return new WaitForSeconds(t);
		if ((bool)m_gun)
		{
			m_gun.OverrideAnimations = false;
		}
	}

	private void HandleDealtDamage(PlayerController source, float dmg)
	{
		if (!m_isClearing)
		{
			m_damageDealt += dmg;
			if (m_damageDealt >= DamageRequired)
			{
				ClearMimic();
			}
		}
	}

	private void HandleAmmoChanged(PlayerController sourcePlayer, Gun sourceGun)
	{
		if (!m_isClearing && sourceGun == m_gun && !m_selfRefilling && sourceGun.ammo >= sourceGun.AdjustedMaxAmmo)
		{
			ForceClearMimic();
		}
	}

	public void ForceClearMimic(bool instant = false)
	{
		if (m_isClearing)
		{
			return;
		}
		m_damageDealt = 10000f;
		if (instant)
		{
			if ((bool)m_gun && (bool)m_gun.CurrentOwner)
			{
				if ((bool)UnbecomeMimicVfx)
				{
					SpawnManager.SpawnVFX(UnbecomeMimicVfx, m_gun.GetSprite().WorldCenter, Quaternion.identity);
				}
				PlayerController playerController = m_gun.CurrentOwner as PlayerController;
				playerController.OnDealtDamage -= HandleDealtDamage;
				playerController.inventory.GunLocked.RemoveOverride("mimic gun");
				playerController.inventory.DestroyGun(m_gun);
				playerController.ChangeToGunSlot(playerController.inventory.AllGuns.IndexOf(m_sourceGun), true);
			}
		}
		else
		{
			ClearMimic();
		}
	}

	private IEnumerator HandleClearMimic()
	{
		if (m_isClearing)
		{
			yield break;
		}
		m_isClearing = true;
		m_gun.OverrideAnimations = true;
		m_gun.spriteAnimator.Play("mimic_gun_outro");
		while (m_gun.spriteAnimator.IsPlaying("mimic_gun_outro"))
		{
			yield return null;
		}
		if ((bool)m_gun && (bool)m_gun.CurrentOwner)
		{
			if ((bool)UnbecomeMimicVfx)
			{
				SpawnManager.SpawnVFX(UnbecomeMimicVfx, m_gun.GetSprite().WorldCenter, Quaternion.identity);
			}
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			playerController.OnDealtDamage -= HandleDealtDamage;
			playerController.inventory.GunLocked.RemoveOverride("mimic gun");
			playerController.inventory.DestroyGun(m_gun);
			playerController.ChangeToGunSlot(playerController.inventory.AllGuns.IndexOf(m_sourceGun), true);
		}
	}

	private void ClearMimic()
	{
		if (!m_isClearing)
		{
			StartCoroutine(HandleClearMimic());
		}
	}
}
