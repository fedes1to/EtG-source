using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class KthuliberProjectileController : MonoBehaviour
{
	public float BossDamage = 50f;

	public float SoulSpeed = 3f;

	public float SlowDuration = 0.3f;

	public float DamageToRoom = 5f;

	public GameObject SuckVFX;

	public GameObject PickupVFX;

	public GameObject ExplodeVFX;

	public GameObject OverheadVFX;

	private Projectile m_projectile;

	private SpeculativeRigidbody m_soulEnemy;

	private GameObject m_overheadVFX;

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		Projectile projectile = m_projectile;
		projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void Update()
	{
		if ((bool)m_soulEnemy && (bool)m_projectile && m_projectile.OverrideMotionModule != null && m_projectile.OverrideMotionModule is OrbitProjectileMotionModule)
		{
			m_projectile.DieInAir();
		}
	}

	private void HandleHitEnemy(Projectile source, SpeculativeRigidbody target, bool fatal)
	{
		if (fatal || !target)
		{
			return;
		}
		AIActor component = target.GetComponent<AIActor>();
		if ((bool)component && component.IsNormalEnemy)
		{
			if ((bool)SuckVFX)
			{
				Vector2 vector = Vector2.Lerp(m_projectile.specRigidbody.UnitCenter, target.UnitCenter, 0.5f);
				SpawnManager.SpawnVFX(SuckVFX, vector, Quaternion.identity);
				AkSoundEngine.PostEvent("Play_WPN_kthulu_soul_01", base.gameObject);
			}
			if ((bool)OverheadVFX)
			{
				m_overheadVFX = component.PlayEffectOnActor(OverheadVFX, new Vector3(0.0625f, component.specRigidbody.HitboxPixelCollider.UnitDimensions.y, 0f), true, false, true);
			}
			m_soulEnemy = target;
			m_projectile.allowSelfShooting = true;
			m_projectile.collidesWithEnemies = false;
			m_projectile.collidesWithPlayer = true;
			m_projectile.UpdateCollisionMask();
			m_projectile.SetNewShooter(target);
			m_projectile.spriteAnimator.Play("kthuliber_full_projectile");
			m_projectile.specRigidbody.PrimaryPixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Circle;
			m_projectile.specRigidbody.PrimaryPixelCollider.ManualOffsetX = -8;
			m_projectile.specRigidbody.PrimaryPixelCollider.ManualOffsetY = -8;
			m_projectile.specRigidbody.PrimaryPixelCollider.ManualDiameter = 16;
			m_projectile.specRigidbody.PrimaryPixelCollider.Regenerate(m_projectile.transform);
			m_projectile.specRigidbody.Reinitialize();
			int count = -1;
			if (PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.KALIBER_KPOW, out count))
			{
				Projectile projectile = m_projectile;
				projectile.ModifyVelocity = (Func<Vector2, Vector2>)Delegate.Combine(projectile.ModifyVelocity, new Func<Vector2, Vector2>(HomeTowardPlayer));
			}
			SpeculativeRigidbody specRigidbody = m_projectile.specRigidbody;
			specRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(HandlePreRigidbodySoulCollision));
			StartCoroutine(FrameDelayedPostProcessing(source));
			StartCoroutine(SlowDownOverTime(source));
		}
	}

	private Vector2 HomeTowardPlayer(Vector2 inVel)
	{
		PlayerController activePlayerClosestToPoint = GameManager.Instance.GetActivePlayerClosestToPoint(m_projectile.LastPosition.XY(), true);
		if ((bool)activePlayerClosestToPoint)
		{
			float num = Vector2.Distance(activePlayerClosestToPoint.CenterPosition, m_projectile.LastPosition.XY());
			if (num < 10f)
			{
				Vector2 vector = activePlayerClosestToPoint.CenterPosition - m_projectile.LastPosition.XY();
				return inVel.magnitude * vector.normalized;
			}
		}
		return inVel;
	}

	private void HandlePreRigidbodySoulCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		if (!otherRigidbody)
		{
			return;
		}
		PlayerController component = otherRigidbody.GetComponent<PlayerController>();
		if (!component)
		{
			return;
		}
		PhysicsEngine.SkipCollision = true;
		if ((bool)m_soulEnemy)
		{
			HealthHaver component2 = m_soulEnemy.GetComponent<HealthHaver>();
			if ((bool)component2 && !component2.IsBoss)
			{
				component2.ApplyDamage(component2.GetMaxHealth(), Vector2.zero, "Soul Burn", CoreDamageTypes.Void, DamageCategory.Unstoppable, true);
			}
			else if ((bool)component2 && component2.IsBoss)
			{
				component2.ApplyDamage(BossDamage, Vector2.zero, "Soul Burn", CoreDamageTypes.Void, DamageCategory.Unstoppable, false, null, true);
				if ((bool)m_overheadVFX)
				{
					UnityEngine.Object.Destroy(m_overheadVFX);
					m_overheadVFX = null;
				}
			}
			if (component.CurrentRoom != null)
			{
				List<AIActor> activeEnemies = component.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
				if (activeEnemies != null)
				{
					for (int i = 0; i < activeEnemies.Count; i++)
					{
						if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver)
						{
							activeEnemies[i].healthHaver.ApplyDamage(DamageToRoom, Vector2.zero, "Soul Burn", CoreDamageTypes.Void, DamageCategory.Unstoppable);
						}
					}
				}
			}
			if ((bool)ExplodeVFX)
			{
				AIActor component3 = m_soulEnemy.GetComponent<AIActor>();
				if ((bool)component3)
				{
					component3.PlayEffectOnActor(ExplodeVFX, Vector3.zero, false, true);
					AkSoundEngine.PostEvent("Play_WPN_kthulu_blast_01", base.gameObject);
				}
			}
		}
		if ((bool)PickupVFX)
		{
			component.PlayEffectOnActor(PickupVFX, Vector3.zero, false, true);
		}
		m_projectile.DieInAir();
	}

	private IEnumerator SlowDownOverTime(Projectile p)
	{
		float elapsed = 0f;
		float duration = SlowDuration;
		float startSpeed = p.baseData.speed;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			if (!p)
			{
				break;
			}
			p.baseData.speed = Mathf.Lerp(startSpeed, SoulSpeed, t);
			p.UpdateSpeed();
			yield return null;
		}
	}

	private IEnumerator FrameDelayedPostProcessing(Projectile p)
	{
		yield return null;
		if ((bool)p)
		{
			PierceProjModifier component = p.GetComponent<PierceProjModifier>();
			if ((bool)component)
			{
				component.BeastModeLevel = PierceProjModifier.BeastModeStatus.NOT_BEAST_MODE;
				component.penetration = 0;
				UnityEngine.Object.Destroy(component);
			}
			HomingModifier component2 = p.GetComponent<HomingModifier>();
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
		}
	}
}
