using System.Collections;
using UnityEngine;

public class TowerBossIrisController : BraveBehaviour
{
	public TowerBossController tower;

	public bool fuseAlive = true;

	public float openDuration = 10f;

	private tk2dSprite m_sprite;

	public bool IsOpen
	{
		get
		{
			return base.healthHaver.IsVulnerable;
		}
	}

	private void Start()
	{
		m_sprite = GetComponentInChildren<tk2dSprite>();
		m_sprite.IsPerpendicular = false;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.IsVulnerable = false;
		base.healthHaver.OnDamaged += Damaged;
		base.healthHaver.OnDeath += Die;
	}

	private void Update()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void Open()
	{
		base.healthHaver.IsVulnerable = true;
		base.spriteAnimator.Play("tower_boss_leftPanel_open");
		StartCoroutine(TimedClose());
	}

	private IEnumerator TimedClose()
	{
		float elapsed = 0f;
		while (elapsed < openDuration && fuseAlive)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (fuseAlive)
		{
			Close();
		}
	}

	public void Close()
	{
		base.healthHaver.IsVulnerable = false;
		base.spriteAnimator.Play("tower_boss_rightPanel_open");
	}

	private void Damaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
	}

	private void Die(Vector2 finalDamageDirection)
	{
		fuseAlive = false;
		if (tower.currentPhase == TowerBossController.TowerBossPhase.PHASE_ONE)
		{
			tower.NotifyFuseDestruction(this);
			base.healthHaver.FullHeal();
			base.healthHaver.IsVulnerable = false;
		}
		else
		{
			tower.NotifyFuseDestruction(this);
			base.healthHaver.IsVulnerable = false;
		}
	}
}
