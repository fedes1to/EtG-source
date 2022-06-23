using System.Collections;
using UnityEngine;

public class RagePassiveItem : PassiveItem
{
	public float Duration = 3f;

	public float DamageMultiplier = 2f;

	public Color flatColorOverride = new Color(0.5f, 0f, 0f, 0.75f);

	public GameObject OverheadVFX;

	private bool m_isRaged;

	private float m_elapsed;

	private GameObject instanceVFX;

	private PlayerController m_player;

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			player.OnReceivedDamage += HandleReceivedDamage;
			m_player = player;
		}
	}

	private void HandleReceivedDamage(PlayerController obj)
	{
		if (m_isRaged)
		{
			if ((bool)OverheadVFX && !instanceVFX)
			{
				instanceVFX = m_player.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
			}
			m_elapsed = 0f;
		}
		else
		{
			obj.StartCoroutine(HandleRage());
		}
	}

	private IEnumerator HandleRage()
	{
		m_isRaged = true;
		instanceVFX = null;
		if ((bool)OverheadVFX)
		{
			instanceVFX = m_player.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
		}
		StatModifier damageStat = new StatModifier
		{
			amount = DamageMultiplier,
			modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
			statToBoost = PlayerStats.StatType.Damage
		};
		m_player.ownerlessStatModifiers.Add(damageStat);
		m_player.stats.RecalculateStats(m_player);
		if (m_player.CurrentGun != null)
		{
			m_player.CurrentGun.ForceImmediateReload();
		}
		m_elapsed = 0f;
		float particleCounter = 0f;
		while (m_elapsed < Duration)
		{
			m_elapsed += BraveTime.DeltaTime;
			m_player.baseFlatColorOverride = flatColorOverride.WithAlpha(Mathf.Lerp(flatColorOverride.a, 0f, Mathf.Clamp01(m_elapsed - (Duration - 1f))));
			if ((bool)instanceVFX && m_elapsed > 1f)
			{
				instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
				instanceVFX = null;
			}
			if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && (bool)m_player && m_player.IsVisible && !m_player.IsFalling)
			{
				particleCounter += BraveTime.DeltaTime * 40f;
				if (particleCounter > 1f)
				{
					int num = Mathf.FloorToInt(particleCounter);
					particleCounter %= 1f;
					GlobalSparksDoer.DoRandomParticleBurst(num, m_player.sprite.WorldBottomLeft.ToVector3ZisY(), m_player.sprite.WorldTopRight.ToVector3ZisY(), Vector3.up, 90f, 0.5f, null, null, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
				}
			}
			yield return null;
		}
		if ((bool)instanceVFX)
		{
			instanceVFX.GetComponent<tk2dSpriteAnimator>().PlayAndDestroyObject("rage_face_vfx_out");
		}
		m_player.ownerlessStatModifiers.Remove(damageStat);
		m_player.stats.RecalculateStats(m_player);
		m_isRaged = false;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<RagePassiveItem>().m_pickedUpThisRun = true;
		player.OnReceivedDamage -= HandleReceivedDamage;
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		if (m_player != null)
		{
			m_player.OnReceivedDamage -= HandleReceivedDamage;
		}
		base.OnDestroy();
	}
}
