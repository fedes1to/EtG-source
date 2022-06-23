using System.Collections.Generic;
using UnityEngine;

public class PowderSkullSpinBulletsBehavior : BehaviorBase
{
	private class ProjectileContainer
	{
		public Projectile projectile;

		public float angle;

		public float distFromCenter;
	}

	public string OverrideBulletName;

	public GameObject ShootPoint;

	public int NumBullets;

	public float BulletMinRadius;

	public float BulletMaxRadius;

	public int BulletCircleSpeed;

	public bool BulletsIgnoreTiles;

	public float RegenTimer;

	private readonly List<ProjectileContainer> m_projectiles = new List<ProjectileContainer>();

	private AIBulletBank m_bulletBank;

	private bool m_cachedCharm;

	private float m_regenTimer;

	public override void Start()
	{
		base.Start();
		m_bulletBank = m_gameObject.GetComponent<AIBulletBank>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_regenTimer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		float num = float.MaxValue;
		if ((bool)m_aiActor && (bool)m_aiActor.TargetRigidbody)
		{
			num = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox)).magnitude;
		}
		for (int i = 0; i < NumBullets; i++)
		{
			float num2 = Mathf.Lerp(BulletMinRadius, BulletMaxRadius, (float)i / ((float)NumBullets - 1f));
			if (num2 * 2f > num)
			{
				m_projectiles.Add(new ProjectileContainer
				{
					projectile = null,
					angle = 0f,
					distFromCenter = num2
				});
				m_projectiles.Add(new ProjectileContainer
				{
					projectile = null,
					angle = 180f,
					distFromCenter = num2
				});
				continue;
			}
			GameObject gameObject = m_bulletBank.CreateProjectileFromBank(GetBulletPosition(0f, num2), 90f, OverrideBulletName);
			Projectile component = gameObject.GetComponent<Projectile>();
			component.specRigidbody.Velocity = Vector2.zero;
			component.ManualControl = true;
			if (BulletsIgnoreTiles)
			{
				component.specRigidbody.CollideWithTileMap = false;
			}
			m_projectiles.Add(new ProjectileContainer
			{
				projectile = component,
				angle = 0f,
				distFromCenter = num2
			});
			gameObject = m_bulletBank.CreateProjectileFromBank(GetBulletPosition(180f, num2), -90f, OverrideBulletName);
			component = gameObject.GetComponent<Projectile>();
			component.specRigidbody.Velocity = Vector2.zero;
			component.ManualControl = true;
			if (BulletsIgnoreTiles)
			{
				component.specRigidbody.CollideWithTileMap = false;
			}
			m_projectiles.Add(new ProjectileContainer
			{
				projectile = component,
				angle = 180f,
				distFromCenter = num2
			});
		}
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuousInClass;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if ((bool)m_aiActor)
		{
			bool flag = m_aiActor.CanTargetEnemies && !m_aiActor.CanTargetPlayers;
			if (m_cachedCharm != flag)
			{
				for (int i = 0; i < m_projectiles.Count; i++)
				{
					if (m_projectiles[i] != null && (bool)m_projectiles[i].projectile && m_projectiles[i].projectile.gameObject.activeSelf)
					{
						m_projectiles[i].projectile.DieInAir(false, false);
						m_projectiles[i].projectile = null;
					}
				}
				m_cachedCharm = flag;
			}
		}
		for (int j = 0; j < m_projectiles.Count; j++)
		{
			if (!m_projectiles[j].projectile || !m_projectiles[j].projectile.gameObject.activeSelf)
			{
				m_projectiles[j].projectile = null;
			}
		}
		for (int k = 0; k < m_projectiles.Count; k++)
		{
			float angle = m_projectiles[k].angle + m_deltaTime * (float)BulletCircleSpeed;
			m_projectiles[k].angle = angle;
			Projectile projectile = m_projectiles[k].projectile;
			if ((bool)projectile)
			{
				Vector2 bulletPosition = GetBulletPosition(angle, m_projectiles[k].distFromCenter);
				projectile.specRigidbody.Velocity = (bulletPosition - (Vector2)projectile.transform.position) / BraveTime.DeltaTime;
				if (projectile.shouldRotate)
				{
					projectile.transform.rotation = Quaternion.Euler(0f, 0f, 180f + (Quaternion.Euler(0f, 0f, 90f) * (ShootPoint.transform.position.XY() - bulletPosition)).XY().ToAngle());
				}
				projectile.ResetDistance();
			}
			else
			{
				if (!(m_regenTimer <= 0f))
				{
					continue;
				}
				Vector2 bulletPosition2 = GetBulletPosition(m_projectiles[k].angle, m_projectiles[k].distFromCenter);
				if (GameManager.Instance.Dungeon.CellExists(bulletPosition2) && !GameManager.Instance.Dungeon.data.isWall((int)bulletPosition2.x, (int)bulletPosition2.y))
				{
					GameObject gameObject = m_bulletBank.CreateProjectileFromBank(bulletPosition2, 0f, OverrideBulletName);
					projectile = gameObject.GetComponent<Projectile>();
					projectile.specRigidbody.Velocity = Vector2.zero;
					projectile.ManualControl = true;
					if (BulletsIgnoreTiles)
					{
						projectile.specRigidbody.CollideWithTileMap = false;
					}
					m_projectiles[k].projectile = projectile;
					m_regenTimer = RegenTimer;
				}
			}
		}
		for (int l = 0; l < m_projectiles.Count; l++)
		{
			if (m_projectiles[l] != null && (bool)m_projectiles[l].projectile)
			{
				bool flag2 = (bool)m_aiActor && m_aiActor.CanTargetEnemies;
				m_projectiles[l].projectile.collidesWithEnemies = m_projectiles[l].projectile.collidesWithEnemies || flag2;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		DestroyProjectiles();
		m_updateEveryFrame = false;
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		DestroyProjectiles();
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	private Vector2 GetBulletPosition(float angle, float distFromCenter)
	{
		return ShootPoint.transform.position.XY() + BraveMathCollege.DegreesToVector(angle, distFromCenter);
	}

	private void DestroyProjectiles()
	{
		for (int i = 0; i < m_projectiles.Count; i++)
		{
			Projectile projectile = m_projectiles[i].projectile;
			if (projectile != null)
			{
				projectile.DieInAir();
			}
		}
		m_projectiles.Clear();
	}
}
