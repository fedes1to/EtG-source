using UnityEngine;

public class MotionTriggeredStatSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public StatModifier Stat;

	public float TimeRequiredMoving = 2f;

	private Gun m_gun;

	private bool m_isActive;

	private PlayerController m_cachedPlayer;

	private float m_elapsedMoving;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
	}

	private void Update()
	{
		if ((bool)m_gun.CurrentOwner)
		{
			m_cachedPlayer = m_gun.CurrentOwner as PlayerController;
			if (m_cachedPlayer.specRigidbody.Velocity.magnitude > 0.05f)
			{
				m_elapsedMoving += BraveTime.DeltaTime;
			}
			else
			{
				m_elapsedMoving = 0f;
			}
		}
		else
		{
			m_elapsedMoving = 0f;
		}
		bool flag = (bool)m_cachedPlayer && m_cachedPlayer.HasActiveBonusSynergy(RequiredSynergy);
		if (flag && m_elapsedMoving > TimeRequiredMoving && !m_isActive)
		{
			m_isActive = true;
			m_cachedPlayer.ownerlessStatModifiers.Add(Stat);
			m_cachedPlayer.stats.RecalculateStats(m_cachedPlayer);
			EnableVFX(m_cachedPlayer);
		}
		else if (m_isActive && (!flag || m_elapsedMoving < TimeRequiredMoving))
		{
			m_isActive = false;
			if ((bool)m_cachedPlayer)
			{
				DisableVFX(m_cachedPlayer);
				m_cachedPlayer.ownerlessStatModifiers.Remove(Stat);
				m_cachedPlayer.stats.RecalculateStats(m_cachedPlayer);
				m_cachedPlayer = null;
			}
		}
	}

	private void OnDisable()
	{
		if (m_isActive)
		{
			m_isActive = false;
			if ((bool)m_cachedPlayer)
			{
				DisableVFX(m_cachedPlayer);
				m_cachedPlayer.ownerlessStatModifiers.Remove(Stat);
				m_cachedPlayer.stats.RecalculateStats(m_cachedPlayer);
				m_cachedPlayer = null;
			}
		}
	}

	private void OnDestroy()
	{
		if (m_isActive)
		{
			m_isActive = false;
			if ((bool)m_cachedPlayer)
			{
				DisableVFX(m_cachedPlayer);
				m_cachedPlayer.ownerlessStatModifiers.Remove(Stat);
				m_cachedPlayer.stats.RecalculateStats(m_cachedPlayer);
				m_cachedPlayer = null;
			}
		}
	}

	public void EnableVFX(PlayerController target)
	{
		Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
		if (outlineMaterial != null)
		{
			outlineMaterial.SetColor("_OverrideColor", new Color(99f, 99f, 0f));
		}
	}

	public void DisableVFX(PlayerController target)
	{
		Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
		if (outlineMaterial != null)
		{
			outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
		}
	}
}
