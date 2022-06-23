using System.Collections.Generic;
using UnityEngine;

public class GunderfuryController : MonoBehaviour
{
	public static int[] expTiers = new int[6] { 0, 800, 2100, 3750, 5500, 7500 };

	[SerializeField]
	public List<GunderfuryTier> tiers;

	public tk2dSpriteAnimator idleVFX;

	private Gun m_gun;

	private bool m_initialized;

	private PlayerController m_player;

	private int m_currentTier;

	private float m_sparkTimer;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		idleVFX.gameObject.SetActive(false);
	}

	public static int GetCurrentTier()
	{
		if (!Application.isPlaying)
		{
			return 0;
		}
		int currentAccumulatedGunderfuryExperience = GameStatsManager.Instance.CurrentAccumulatedGunderfuryExperience;
		int num = 0;
		for (int i = 0; i < expTiers.Length; i++)
		{
			if (expTiers[i] <= currentAccumulatedGunderfuryExperience && expTiers[i] > expTiers[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static int GetCurrentLevel()
	{
		if (!Application.isPlaying)
		{
			return 0;
		}
		int currentTier = GetCurrentTier();
		int num = currentTier * 10;
		int num2 = num + 10;
		if (currentTier < 5)
		{
			int currentAccumulatedGunderfuryExperience = GameStatsManager.Instance.CurrentAccumulatedGunderfuryExperience;
			float num3 = (float)(currentAccumulatedGunderfuryExperience - expTiers[currentTier]) / (float)(expTiers[currentTier + 1] - expTiers[currentTier]);
			num2 += Mathf.FloorToInt(num3 * 10f);
		}
		return num2;
	}

	private void Update()
	{
		if ((bool)m_gun.CurrentOwner && !m_initialized)
		{
			m_player = m_gun.CurrentOwner as PlayerController;
			m_player.OnKilledEnemyContext += HandleKilledEnemy;
			m_initialized = true;
		}
		else if (!m_gun.CurrentOwner && m_initialized)
		{
			m_initialized = false;
			if ((bool)m_player)
			{
				m_player.OnKilledEnemyContext -= HandleKilledEnemy;
			}
			m_player = null;
		}
		int currentTier = GetCurrentTier();
		if (m_currentTier != currentTier)
		{
			m_currentTier = currentTier;
			m_gun.CeaseAttack();
			m_gun.TransformToTargetGun(PickupObjectDatabase.GetById(tiers[currentTier].GunID) as Gun);
			if (string.IsNullOrEmpty(tiers[currentTier].IdleVFX))
			{
				idleVFX.gameObject.SetActive(false);
			}
			else
			{
				idleVFX.gameObject.SetActive(true);
				idleVFX.Play(tiers[currentTier].IdleVFX);
			}
		}
		if (currentTier >= 5 && GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
		{
			m_sparkTimer += BraveTime.DeltaTime * 30f;
			int num = Mathf.FloorToInt(m_sparkTimer);
			if (num > 0)
			{
				m_sparkTimer -= num;
				GlobalSparksDoer.DoRadialParticleBurst(num, m_gun.PrimaryHandAttachPoint.position, m_gun.barrelOffset.position, 360f, 4f, 4f, null, 0.5f, Color.white, GlobalSparksDoer.SparksType.DARK_MAGICKS);
			}
		}
		if (idleVFX.gameObject.activeSelf && (bool)m_gun && (bool)m_gun.sprite)
		{
			idleVFX.sprite.FlipY = m_gun.sprite.FlipY;
			idleVFX.renderer.enabled = m_gun.renderer.enabled;
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_player)
		{
			m_player.OnKilledEnemyContext -= HandleKilledEnemy;
		}
	}

	private void HandleKilledEnemy(PlayerController sourcePlayer, HealthHaver killedEnemy)
	{
		if (GameStatsManager.Instance.CurrentAccumulatedGunderfuryExperience <= 10000000)
		{
			if (!killedEnemy || killedEnemy.GetMaxHealth() < 0f)
			{
				GameStatsManager.Instance.CurrentAccumulatedGunderfuryExperience++;
			}
			else
			{
				GameStatsManager.Instance.CurrentAccumulatedGunderfuryExperience += Mathf.Max(1, Mathf.CeilToInt(killedEnemy.GetMaxHealth() / 10f));
			}
		}
	}
}
