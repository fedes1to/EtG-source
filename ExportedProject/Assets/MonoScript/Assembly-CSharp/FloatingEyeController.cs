using Dungeonator;
using UnityEngine;

public class FloatingEyeController : BraveBehaviour
{
	private BeholsterController m_beholster;

	private bool m_beholsterKilled;

	public void Awake()
	{
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.OnSpawnCompleted += OnSpawnCompleted;
		}
		base.aiActor.PreventAutoKillOnBossDeath = true;
	}

	public void Start()
	{
		m_beholster = Object.FindObjectOfType<BeholsterController>();
		if ((bool)m_beholster)
		{
			m_beholster.healthHaver.OnDamaged += OnBeholsterDamaged;
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_beholster)
		{
			m_beholster.healthHaver.OnDamaged -= OnBeholsterDamaged;
		}
		if ((bool)base.aiAnimator)
		{
			base.aiAnimator.OnSpawnCompleted -= OnSpawnCompleted;
		}
		base.OnDestroy();
	}

	private void OnBeholsterDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (resultValue <= 0f)
		{
			m_beholsterKilled = true;
			StartCrying();
		}
	}

	private void OnSpawnCompleted()
	{
		if ((bool)base.aiActor)
		{
			base.aiActor.PathableTiles |= CellTypes.PIT;
		}
		if (m_beholsterKilled || ((bool)m_beholster && m_beholster.healthHaver.IsDead))
		{
			StartCrying();
		}
	}

	private void StartCrying()
	{
		base.aiActor.ClearPath();
		base.behaviorSpeculator.enabled = false;
		base.aiShooter.enabled = false;
		base.aiShooter.ToggleGunAndHandRenderers(false, "Cry");
		base.aiActor.IgnoreForRoomClear = true;
		base.aiAnimator.PlayUntilCancelled("cry");
	}
}
