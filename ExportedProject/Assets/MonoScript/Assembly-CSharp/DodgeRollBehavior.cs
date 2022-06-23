using System.Collections.Generic;
using UnityEngine;

public class DodgeRollBehavior : OverrideBehaviorBase
{
	public float Cooldown = 1f;

	public float timeToHitThreshold = 0.25f;

	public float dodgeChance = 0.5f;

	public string dodgeAnim = "dodgeroll";

	public float rollDistance = 3f;

	private float m_cooldownTimer;

	private List<Projectile> m_consideredProjectiles = new List<Projectile>();

	private bool? m_cachedShouldDodge;

	private Vector2? m_cachedRollDirection;

	public override void Start()
	{
		base.Start();
		m_updateEveryFrame = true;
		m_ignoreGlobalCooldown = true;
		StaticReferenceManager.ProjectileAdded += ProjectileAdded;
		StaticReferenceManager.ProjectileRemoved += ProjectileRemoved;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_cooldownTimer);
		m_cachedShouldDodge = null;
		m_cachedRollDirection = null;
	}

	public override bool OverrideOtherBehaviors()
	{
		if (m_cooldownTimer > 0f)
		{
			return false;
		}
		Vector2 rollDirection;
		return ShouldDodgeroll(out rollDirection);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_cooldownTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		Vector2 rollDirection;
		if (ShouldDodgeroll(out rollDirection))
		{
			m_aiAnimator.LockFacingDirection = true;
			m_aiAnimator.FacingDirection = rollDirection.ToAngle();
			m_aiAnimator.PlayUntilFinished(dodgeAnim);
			float currentClipLength = m_aiAnimator.CurrentClipLength;
			m_aiActor.BehaviorOverridesVelocity = true;
			m_aiActor.BehaviorVelocity = rollDirection * (rollDistance / currentClipLength);
			return BehaviorResult.RunContinuous;
		}
		return BehaviorResult.Continue;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_aiAnimator.IsPlaying(dodgeAnim))
		{
			m_aiActor.BehaviorOverridesVelocity = false;
			m_aiAnimator.LockFacingDirection = false;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void Destroy()
	{
		StaticReferenceManager.ProjectileAdded -= ProjectileAdded;
		StaticReferenceManager.ProjectileRemoved -= ProjectileRemoved;
		base.Destroy();
	}

	private void ProjectileAdded(Projectile p)
	{
		if ((bool)p && (bool)p.specRigidbody && p.specRigidbody.CanCollideWith(m_aiActor.specRigidbody))
		{
			m_consideredProjectiles.Add(p);
		}
	}

	private void ProjectileRemoved(Projectile p)
	{
		m_consideredProjectiles.Remove(p);
	}

	private bool ShouldDodgeroll(out Vector2 rollDirection)
	{
		if (m_cachedShouldDodge.HasValue)
		{
			rollDirection = m_cachedRollDirection.Value;
			return m_cachedShouldDodge.Value;
		}
		PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
		for (int i = 0; i < m_consideredProjectiles.Count; i++)
		{
			Projectile projectile = m_consideredProjectiles[i];
			float num = Vector2.Distance(projectile.specRigidbody.UnitCenter, hitboxPixelCollider.UnitCenter) / projectile.Speed;
			if (num > timeToHitThreshold)
			{
				continue;
			}
			IntVector2 pixelsToMove = PhysicsEngine.UnitToPixel(projectile.specRigidbody.Velocity * timeToHitThreshold * 1.1f);
			CollisionData result;
			bool flag = PhysicsEngine.Instance.RigidbodyCast(projectile.specRigidbody, pixelsToMove, out result);
			CollisionData.Pool.Free(ref result);
			if (flag && !(result.OtherRigidbody == null) && !(result.OtherRigidbody != m_aiActor.specRigidbody) && !(Random.value > dodgeChance))
			{
				List<Vector2> list = new List<Vector2>();
				Vector2 unitCenter = m_aiActor.specRigidbody.UnitCenter;
				for (int j = 0; j < 8; j++)
				{
					bool flag2 = false;
					bool flag3 = false;
					Vector2 normalized = IntVector2.CardinalsAndOrdinals[j].ToVector2().normalized;
					RaycastResult result2;
					flag2 = PhysicsEngine.Instance.Raycast(unitCenter, normalized, 3f, out result2, true, true, int.MaxValue, CollisionLayer.EnemyCollider, false, null, m_aiActor.specRigidbody);
					RaycastResult.Pool.Free(ref result2);
					float num2 = 0.25f;
					for (float num3 = num2; num3 <= rollDistance; num3 += num2)
					{
						if (flag3)
						{
							break;
						}
						if (flag2)
						{
							break;
						}
						if (GameManager.Instance.Dungeon.ShouldReallyFall(unitCenter + num3 * normalized))
						{
							flag3 = true;
						}
					}
					if (!flag2 && !flag3)
					{
						list.Add(normalized);
					}
				}
				if (list.Count != 0)
				{
					m_cachedShouldDodge = true;
					m_cachedRollDirection = BraveUtility.RandomElement(list);
					rollDirection = m_cachedRollDirection.Value;
					return m_cachedShouldDodge.Value;
				}
			}
			m_consideredProjectiles.Remove(projectile);
		}
		m_cachedShouldDodge = false;
		m_cachedRollDirection = Vector2.zero;
		rollDirection = m_cachedRollDirection.Value;
		return m_cachedShouldDodge.Value;
	}
}
