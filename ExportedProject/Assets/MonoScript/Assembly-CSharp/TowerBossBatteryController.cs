using UnityEngine;

public class TowerBossBatteryController : DungeonPlaceableBehaviour
{
	public TowerBossController tower;

	public TowerBossIrisController linkedIris;

	public float cycleTime = 5f;

	private tk2dSprite m_sprite;

	public bool IsVulnerable
	{
		get
		{
			return base.healthHaver.IsVulnerable;
		}
		set
		{
			if (m_sprite == null)
			{
				m_sprite = GetComponentInChildren<tk2dSprite>();
			}
			base.healthHaver.IsVulnerable = value;
			if (value)
			{
				m_sprite.renderer.enabled = true;
			}
			else
			{
				m_sprite.renderer.enabled = false;
			}
		}
	}

	private void Start()
	{
		m_sprite = GetComponentInChildren<tk2dSprite>();
		base.healthHaver.IsVulnerable = false;
		base.healthHaver.persistsOnDeath = true;
		base.healthHaver.OnDamaged += Damaged;
		base.healthHaver.OnDeath += Die;
	}

	private void Damaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
	}

	private void Die(Vector2 finalDamageDirection)
	{
		linkedIris.Open();
		base.healthHaver.FullHeal();
		base.healthHaver.IsVulnerable = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
