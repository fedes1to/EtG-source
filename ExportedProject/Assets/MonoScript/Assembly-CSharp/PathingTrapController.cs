using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PathingTrapController : TrapController
{
	public float damage;

	public bool ignoreInvulnerabilityFrames;

	public float knockbackStrength;

	public bool hitsEnemies;

	public float enemyDamage;

	public float enemyKnockbackStrength;

	[TogglesProperty("bloodyAnimation", "Bloody Animation")]
	public bool usesBloodyAnimation;

	[HideInInspector]
	public string bloodyAnimation;

	public bool usesDirectionalAnimations;

	[ShowInInspectorIf("usesDirectionalAnimations", true)]
	public string northAnimation;

	[ShowInInspectorIf("usesDirectionalAnimations", true)]
	public string eastAnimation;

	[ShowInInspectorIf("usesDirectionalAnimations", true)]
	public string southAnimation;

	[ShowInInspectorIf("usesDirectionalAnimations", true)]
	public string westAnimation;

	public bool usesDirectionalShadowAnimations;

	[ShowInInspectorIf("usesDirectionalShadowAnimations", true)]
	public tk2dSpriteAnimator shadowAnimator;

	[ShowInInspectorIf("usesDirectionalShadowAnimations", true)]
	public string northShadowAnimation;

	[ShowInInspectorIf("usesDirectionalShadowAnimations", true)]
	public string eastShadowAnimation;

	[ShowInInspectorIf("usesDirectionalShadowAnimations", true)]
	public string southShadowAnimation;

	[ShowInInspectorIf("usesDirectionalShadowAnimations", true)]
	public string westShadowAnimation;

	public bool pauseAnimationOnRest = true;

	[Header("Sawblade Options")]
	public Transform Sparks_A;

	public Transform Sparks_B;

	private Vector3 m_sparksAStartPos;

	private Vector3 m_sparksBStartPos;

	private bool m_IsSoundPlaying;

	private RoomHandler m_parentRoom;

	protected Vector2 m_cachedSparkVelocity;

	private bool m_isBloodied;

	private bool m_isAnimating;

	private PathMover m_pathMover;

	private tk2dSpriteAnimationClip m_startingAnimation;

	private tk2dSpriteAnimationClip m_startingShadowAnimation;

	public override void Start()
	{
		base.Start();
		m_pathMover = GetComponent<PathMover>();
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnPathTargetReached = (Action)Delegate.Combine(speculativeRigidbody2.OnPathTargetReached, new Action(OnPathTargetReached));
			List<CollisionData> list = new List<CollisionData>();
			if (PhysicsEngine.Instance.OverlapCast(base.specRigidbody, list, false, true, null, null, false, null, null))
			{
				for (int i = 0; i < list.Count; i++)
				{
					SpeculativeRigidbody otherRigidbody = list[i].OtherRigidbody;
					if ((bool)otherRigidbody && (bool)otherRigidbody.minorBreakable)
					{
						otherRigidbody.minorBreakable.Break();
					}
				}
			}
		}
		m_startingAnimation = base.spriteAnimator.CurrentClip;
		if ((bool)shadowAnimator)
		{
			m_startingShadowAnimation = base.spriteAnimator.CurrentClip;
		}
		if (GameManager.Instance.PlayerIsNearRoom(m_parentRoom))
		{
			AkSoundEngine.PostEvent("Play_ENV_trap_active", base.gameObject);
			m_IsSoundPlaying = true;
		}
		m_isAnimating = true;
		if (Sparks_A != null)
		{
			m_sparksAStartPos = Sparks_A.localPosition;
		}
		if (Sparks_B != null)
		{
			m_sparksBStartPos = Sparks_B.localPosition;
		}
	}

	protected void UpdateSparks()
	{
		float t = 5f;
		float t2 = 5f;
		if (m_pathMover != null)
		{
			t2 = Vector2.Distance(base.specRigidbody.Position.UnitPosition, m_pathMover.GetCurrentTargetPosition());
			t = Vector2.Distance(base.specRigidbody.Position.UnitPosition, m_pathMover.GetPreviousTargetPosition());
		}
		if (m_pathMover.Path.wrapMode != SerializedPath.SerializedPathWrapMode.Loop)
		{
			if (m_pathMover.CurrentIndex == 0 || m_pathMover.CurrentIndex == m_pathMover.Path.nodes.Count - 1)
			{
				t2 = 1f;
			}
			if (m_pathMover.PreviousIndex == 0 || m_pathMover.PreviousIndex == m_pathMover.Path.nodes.Count - 1)
			{
				t = 1f;
			}
		}
		if (base.specRigidbody.Velocity == Vector2.zero)
		{
			return;
		}
		if (Sparks_A != null)
		{
			Vector2 cachedSparkVelocity = ((!(base.specRigidbody.Velocity == Vector2.zero)) ? base.specRigidbody.Velocity : m_cachedSparkVelocity);
			if (base.specRigidbody.Velocity != Vector2.zero)
			{
				m_cachedSparkVelocity = cachedSparkVelocity;
			}
			if (Mathf.Abs(cachedSparkVelocity.x) > Mathf.Abs(cachedSparkVelocity.y))
			{
				if (!Sparks_A.gameObject.activeSelf)
				{
					Sparks_A.gameObject.SetActive(true);
				}
				Sparks_A.localPosition = ((!(cachedSparkVelocity.x > 0f)) ? m_sparksBStartPos : m_sparksAStartPos);
				Vector3 a = ((!(m_pathMover.GetPreviousSourcePosition().y > base.specRigidbody.Position.UnitPosition.y)) ? new Vector3(0f, m_sparksAStartPos.x, 0f) : new Vector3(0f, m_sparksBStartPos.x, 0f));
				Sparks_A.localPosition = Vector3.Lerp(a, Sparks_A.localPosition, t);
			}
			else
			{
				if (!Sparks_A.gameObject.activeSelf)
				{
					Sparks_A.gameObject.SetActive(true);
				}
				Sparks_A.localPosition = ((!(cachedSparkVelocity.y > 0f)) ? new Vector3(0f, m_sparksBStartPos.x, 0f) : new Vector3(0f, m_sparksAStartPos.x, 0f));
				Vector3 a2 = ((!(m_pathMover.GetPreviousSourcePosition().x < base.specRigidbody.Position.UnitPosition.x)) ? m_sparksBStartPos : m_sparksAStartPos);
				Sparks_A.localPosition = Vector3.Lerp(a2, Sparks_A.localPosition, t);
			}
		}
		if (!(Sparks_B != null))
		{
			return;
		}
		Vector2 cachedSparkVelocity2 = ((!(base.specRigidbody.Velocity == Vector2.zero)) ? base.specRigidbody.Velocity : m_cachedSparkVelocity);
		if (base.specRigidbody.Velocity != Vector2.zero)
		{
			m_cachedSparkVelocity = cachedSparkVelocity2;
		}
		if (Mathf.Abs(cachedSparkVelocity2.x) > Mathf.Abs(cachedSparkVelocity2.y))
		{
			if (!Sparks_B.gameObject.activeSelf)
			{
				Sparks_B.gameObject.SetActive(true);
			}
			Sparks_B.localPosition = ((!(cachedSparkVelocity2.x > 0f)) ? m_sparksAStartPos : m_sparksBStartPos);
			Vector3 a3 = ((!(m_pathMover.GetNextTargetPosition().y > base.specRigidbody.Position.UnitPosition.y)) ? new Vector3(0f, m_sparksAStartPos.x, 0f) : new Vector3(0f, m_sparksBStartPos.x, 0f));
			Sparks_B.localPosition = Vector3.Lerp(a3, Sparks_B.localPosition, t2);
		}
		else
		{
			if (!Sparks_B.gameObject.activeSelf)
			{
				Sparks_B.gameObject.SetActive(true);
			}
			Sparks_B.localPosition = ((!(cachedSparkVelocity2.y > 0f)) ? new Vector3(0f, m_sparksAStartPos.x, 0f) : new Vector3(0f, m_sparksBStartPos.x, 0f));
			Vector3 a4 = ((!(m_pathMover.GetNextTargetPosition().x > base.specRigidbody.Position.UnitPosition.x)) ? m_sparksAStartPos : m_sparksBStartPos);
			Sparks_B.localPosition = Vector3.Lerp(a4, Sparks_B.localPosition, t2);
		}
	}

	public virtual void Update()
	{
		if (m_IsSoundPlaying)
		{
			if (!GameManager.Instance.PlayerIsNearRoom(m_parentRoom) || (pauseAnimationOnRest && base.specRigidbody.Velocity == Vector2.zero))
			{
				m_IsSoundPlaying = false;
				AkSoundEngine.PostEvent("Stop_ENV_trap_active", base.gameObject);
			}
		}
		else if (GameManager.Instance.PlayerIsNearRoom(m_parentRoom) && (!pauseAnimationOnRest || !(base.specRigidbody.Velocity == Vector2.zero)))
		{
			m_IsSoundPlaying = true;
			AkSoundEngine.PostEvent("Play_ENV_trap_active", base.gameObject);
		}
		UpdateSparks();
		if (base.specRigidbody.Velocity == Vector2.zero)
		{
			if (pauseAnimationOnRest && m_isAnimating)
			{
				base.spriteAnimator.Stop();
				if ((bool)shadowAnimator)
				{
					shadowAnimator.Stop();
				}
				m_isAnimating = false;
			}
			return;
		}
		m_isAnimating = true;
		if (base.spriteAnimator != null)
		{
			base.spriteAnimator.Sprite.UpdateZDepth();
		}
		if (usesDirectionalAnimations)
		{
			IntVector2 intMajorAxis = BraveUtility.GetIntMajorAxis(base.specRigidbody.Velocity);
			if (intMajorAxis == IntVector2.North)
			{
				base.spriteAnimator.Play(northAnimation);
			}
			else if (intMajorAxis == IntVector2.East)
			{
				base.spriteAnimator.Play(eastAnimation);
			}
			else if (intMajorAxis == IntVector2.South)
			{
				base.spriteAnimator.Play(southAnimation);
			}
			else if (intMajorAxis == IntVector2.West)
			{
				base.spriteAnimator.Play(westAnimation);
			}
			if (usesDirectionalShadowAnimations)
			{
				if (intMajorAxis == IntVector2.North)
				{
					shadowAnimator.Play(northShadowAnimation);
				}
				else if (intMajorAxis == IntVector2.East)
				{
					shadowAnimator.Play(eastShadowAnimation);
				}
				else if (intMajorAxis == IntVector2.South)
				{
					shadowAnimator.Play(southShadowAnimation);
				}
				else if (intMajorAxis == IntVector2.West)
				{
					shadowAnimator.Play(westShadowAnimation);
				}
			}
		}
		else
		{
			if (m_startingAnimation != null)
			{
				base.spriteAnimator.Play(m_startingAnimation);
			}
			if ((bool)shadowAnimator && m_startingShadowAnimation != null)
			{
				shadowAnimator.Play(m_startingShadowAnimation);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override GameObject InstantiateObject(RoomHandler targetRoom, IntVector2 loc, bool deferConfiguration = false)
	{
		m_markCellOccupied = false;
		return base.InstantiateObject(targetRoom, loc, deferConfiguration);
	}

	private void OnTriggerCollision(SpeculativeRigidbody rigidbody, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (rigidbody.gameActor is PlayerController)
		{
			if (!(rigidbody.gameActor as PlayerController).IsEthereal)
			{
				Damage(rigidbody, damage, knockbackStrength);
			}
			return;
		}
		if (hitsEnemies && (bool)rigidbody.aiActor)
		{
			Damage(rigidbody, enemyDamage, enemyKnockbackStrength);
			return;
		}
		Chest component = rigidbody.GetComponent<Chest>();
		if (component != null && !component.IsBroken && !component.TemporarilyUnopenable)
		{
			component.majorBreakable.Break(source.Velocity);
		}
	}

	private void OnPathTargetReached()
	{
		if (m_IsSoundPlaying)
		{
			AkSoundEngine.PostEvent("Play_ENV_trap_turn", base.gameObject);
		}
	}

	protected virtual void Damage(SpeculativeRigidbody rigidbody, float damage, float knockbackStrength)
	{
		if (damage <= 0f)
		{
			return;
		}
		if (knockbackStrength > 0f && (bool)rigidbody.knockbackDoer)
		{
			rigidbody.knockbackDoer.ApplySourcedKnockback(rigidbody.UnitCenter - base.specRigidbody.UnitCenter, knockbackStrength, base.gameObject);
		}
		if (rigidbody.healthHaver.IsVulnerable || ignoreInvulnerabilityFrames)
		{
			HealthHaver obj = rigidbody.healthHaver;
			Vector2 zero = Vector2.zero;
			string enemiesString = StringTableManager.GetEnemiesString("#TRAP");
			bool flag = ignoreInvulnerabilityFrames;
			obj.ApplyDamage(damage, zero, enemiesString, CoreDamageTypes.None, DamageCategory.Normal, flag);
			if (!m_isBloodied && usesBloodyAnimation && !string.IsNullOrEmpty(bloodyAnimation))
			{
				base.spriteAnimator.Play(bloodyAnimation);
			}
			m_isBloodied = true;
		}
	}
}
