using System;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/LeapBehaviour")]
public class GatlingGullLeapBehavior : BasicAttackBehavior
{
	public enum LeapState
	{
		Jump,
		TrackFromAbove,
		ShadowFall,
		Fall,
		Smug
	}

	public float AirSpeed = 1f;

	public float MinAirtime = 0.8f;

	public float DamageRadius = 3f;

	public float Damage = 1f;

	public float Force = 1f;

	public ScreenShakeSettings HitScreenShake;

	public ScreenShakeSettings MissScreenShake;

	public GameObject ImpactDustUp;

	public float SmugTime = 1f;

	[NonSerialized]
	[HideInInspector]
	public bool ShouldSmug = true;

	[NonSerialized]
	[HideInInspector]
	public Vector2? OverridePosition;

	[NonSerialized]
	[HideInInspector]
	public float SpeedMultiplier = 1f;

	private Vector3 m_startPosition;

	private Vector3 m_targetLandPosition;

	private tk2dSprite m_sprite;

	private SpeculativeRigidbody m_specRigidbody;

	private Vector3 m_offset;

	private float m_timer;

	private float m_totalAirTime;

	private tk2dSpriteAnimator m_animator;

	private tk2dSpriteAnimator m_shadowAnimator;

	private LeapState m_state;

	public override void Start()
	{
		base.Start();
		m_sprite = m_gameObject.GetComponent<tk2dSprite>();
		m_specRigidbody = m_gameObject.GetComponent<SpeculativeRigidbody>();
		SpeculativeRigidbody specRigidbody = m_specRigidbody;
		specRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleMajorBreakableDestruction));
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	protected void HandleMajorBreakableDestruction(CollisionData rigidbodyCollision)
	{
		MajorBreakable majorBreakable = rigidbodyCollision.OtherRigidbody.GetComponent<MajorBreakable>();
		if (majorBreakable == null)
		{
			majorBreakable = rigidbodyCollision.OtherRigidbody.GetComponentInParent<MajorBreakable>();
		}
		if (rigidbodyCollision.Overlap && majorBreakable != null)
		{
			majorBreakable.Break(Vector2.zero);
		}
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!m_animator)
		{
			m_animator = m_aiActor.spriteAnimator;
		}
		if (!m_shadowAnimator)
		{
			m_shadowAnimator = m_aiActor.ShadowObject.GetComponent<tk2dSpriteAnimator>();
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_startPosition = m_gameObject.transform.position;
		m_aiActor.ClearPath();
		m_state = LeapState.Jump;
		m_aiAnimator.enabled = false;
		tk2dSpriteAnimationClip clipByName = m_animator.GetClipByName("jump");
		m_animator.Play(clipByName, 0f, clipByName.fps * SpeedMultiplier);
		m_updateEveryFrame = true;
		m_offset = (m_aiActor.specRigidbody.UnitCenter.ToVector3ZUp() - m_aiActor.transform.position).WithZ(0f);
		tk2dSpriteAnimator animator = m_animator;
		animator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(animator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		UpdateTargetPosition();
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		m_timer -= m_deltaTime * SpeedMultiplier;
		if (m_state == LeapState.Jump)
		{
			if (!m_animator.IsPlaying("jump"))
			{
				UpdateTargetPosition();
				m_totalAirTime = Mathf.Max(MinAirtime / SpeedMultiplier, (m_targetLandPosition - m_startPosition).magnitude / AirSpeed);
				m_timer = m_totalAirTime;
				m_state = LeapState.TrackFromAbove;
				m_specRigidbody.enabled = false;
				m_sprite.renderer.enabled = false;
				SpriteOutlineManager.ToggleOutlineRenderers(m_sprite, false);
			}
		}
		else if (m_state == LeapState.TrackFromAbove)
		{
			UpdateTargetPosition();
			m_gameObject.transform.position = Vector3.Lerp(m_targetLandPosition, m_startPosition, Mathf.Clamp01(m_timer / m_totalAirTime));
			if (m_timer <= 0f)
			{
				m_state = LeapState.ShadowFall;
				AkSoundEngine.PostEvent("Play_ANM_gull_descend_01", m_gameObject);
				tk2dSpriteAnimationClip clipByName = m_shadowAnimator.GetClipByName("shadow_out");
				m_shadowAnimator.Play(clipByName, 0f, clipByName.fps * SpeedMultiplier);
			}
		}
		else if (m_state == LeapState.ShadowFall)
		{
			if (!m_shadowAnimator.IsPlaying("shadow_out"))
			{
				m_state = LeapState.Fall;
				m_gameObject.transform.position = m_targetLandPosition;
				m_specRigidbody.enabled = true;
				m_specRigidbody.Reinitialize();
				m_sprite.renderer.enabled = true;
				SpriteOutlineManager.ToggleOutlineRenderers(m_sprite, true);
				tk2dSpriteAnimationClip clipByName2 = m_animator.GetClipByName("land");
				m_animator.Play(clipByName2, 0f, clipByName2.fps * SpeedMultiplier);
			}
		}
		else if (m_state == LeapState.Fall)
		{
			if (!m_animator.IsPlaying("land"))
			{
				m_state = LeapState.Smug;
				m_aiAnimator.enabled = true;
				if (ShouldSmug)
				{
					m_aiAnimator.PlayForDuration("smug", SmugTime, true);
					m_timer = SmugTime;
				}
			}
		}
		else if (m_state == LeapState.Smug && (m_timer <= 0f || !ShouldSmug))
		{
			ShouldSmug = false;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_updateEveryFrame = false;
		tk2dSpriteAnimator animator = m_animator;
		animator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(animator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (frame.eventInfo == "start_shadow_animation")
		{
			m_shadowAnimator.Play("shadow_in");
		}
		else
		{
			if (!(frame.eventInfo == "land_impact"))
			{
				return;
			}
			if ((bool)ImpactDustUp)
			{
				tk2dSprite component = SpawnManager.SpawnVFX(ImpactDustUp).GetComponent<tk2dSprite>();
				tk2dSprite component2 = m_aiActor.ShadowObject.GetComponent<tk2dSprite>();
				component.transform.position = m_targetLandPosition + m_offset;
				component2.AttachRenderer(component);
				component.HeightOffGround = 0.01f;
			}
			bool flag = false;
			SpeculativeRigidbody targetRigidbody = m_aiActor.TargetRigidbody;
			if ((bool)targetRigidbody)
			{
				Vector2 direction = m_aiActor.TargetRigidbody.UnitCenter - m_aiActor.specRigidbody.UnitCenter;
				if (direction.magnitude < DamageRadius)
				{
					if (Mathf.Approximately(direction.magnitude, 0f))
					{
						direction = UnityEngine.Random.insideUnitCircle;
					}
					if ((bool)targetRigidbody.healthHaver)
					{
						targetRigidbody.healthHaver.ApplyDamage(Damage, direction.normalized, m_aiActor.GetActorName());
					}
					if ((bool)targetRigidbody.knockbackDoer)
					{
						targetRigidbody.knockbackDoer.ApplyKnockback(direction, Force);
					}
					targetRigidbody.RegisterGhostCollisionException(m_aiActor.specRigidbody);
					flag = true;
					ShouldSmug = true;
					GameManager.Instance.MainCameraController.DoScreenShake(HitScreenShake, m_aiActor.specRigidbody.UnitCenter);
				}
			}
			if (!flag)
			{
				GameManager.Instance.MainCameraController.DoScreenShake(MissScreenShake, m_aiActor.specRigidbody.UnitCenter);
			}
		}
	}

	private void UpdateTargetPosition()
	{
		if (!m_aiActor.TargetRigidbody)
		{
			return;
		}
		Vector2 target = ((!OverridePosition.HasValue) ? m_aiActor.TargetRigidbody.UnitCenter : OverridePosition.Value);
		Vector2 vector = m_aiActor.specRigidbody.UnitDimensions / 2f;
		Dungeon dungeon = GameManager.Instance.Dungeon;
		RoomHandler roomFromPosition = dungeon.data.GetRoomFromPosition(target.ToIntVector2(VectorConversions.Floor));
		if (roomFromPosition != null)
		{
			Vector2 min = roomFromPosition.area.basePosition.ToVector2() + vector + Vector2.one * PhysicsEngine.Instance.HalfPixelUnitWidth;
			Vector2 max = (roomFromPosition.area.basePosition + roomFromPosition.area.dimensions).ToVector2() - vector - Vector2.one * PhysicsEngine.Instance.HalfPixelUnitWidth;
			target = Vector2Extensions.Clamp(target, min, max);
		}
		Vector2 vector2 = target + new Vector2(0f - vector.x, vector.y);
		Vector2 vector3 = target + new Vector2(vector.x, vector.y);
		Vector2 vector4 = target + new Vector2(0f - vector.x, 0f - vector.y);
		Vector2 vector5 = target + new Vector2(vector.x, 0f - vector.y);
		CellData cellData = dungeon.data[vector2.ToIntVector2(VectorConversions.Floor)];
		CellData cellData2 = dungeon.data[vector3.ToIntVector2(VectorConversions.Floor)];
		CellData cellData3 = dungeon.data[vector4.ToIntVector2(VectorConversions.Floor)];
		CellData cellData4 = dungeon.data[vector5.ToIntVector2(VectorConversions.Floor)];
		bool flag = cellData.type != CellType.FLOOR;
		bool flag2 = cellData2.type != CellType.FLOOR;
		bool flag3 = cellData3.type != CellType.FLOOR || cellData3.IsTopWall();
		bool flag4 = cellData4.type != CellType.FLOOR || cellData4.IsTopWall();
		int num = 0;
		if (flag)
		{
			num++;
		}
		if (flag2)
		{
			num++;
		}
		if (flag3)
		{
			num++;
		}
		if (flag4)
		{
			num++;
		}
		switch (num)
		{
		case 1:
			if (flag)
			{
				AdjustTarget(ref target, vector, IntVector2.Down, IntVector2.Right);
			}
			if (flag2)
			{
				AdjustTarget(ref target, vector, IntVector2.Down, IntVector2.Left);
			}
			if (flag3)
			{
				AdjustTarget(ref target, vector, IntVector2.Up, IntVector2.Right);
			}
			if (flag4)
			{
				AdjustTarget(ref target, vector, IntVector2.Up, IntVector2.Left);
			}
			break;
		case 2:
			if (flag3 && flag4)
			{
				AdjustTarget(ref target, vector, IntVector2.Up);
			}
			if (flag && flag2)
			{
				AdjustTarget(ref target, vector, IntVector2.Down);
			}
			if (flag2 && flag4)
			{
				AdjustTarget(ref target, vector, IntVector2.Left);
			}
			if (flag && flag3)
			{
				AdjustTarget(ref target, vector, IntVector2.Right);
			}
			break;
		case 3:
			if (!flag4)
			{
				AdjustTarget(ref target, vector, IntVector2.Down, IntVector2.Right);
			}
			if (!flag3)
			{
				AdjustTarget(ref target, vector, IntVector2.Down, IntVector2.Left);
			}
			if (!flag2)
			{
				AdjustTarget(ref target, vector, IntVector2.Up, IntVector2.Right);
			}
			if (!flag)
			{
				AdjustTarget(ref target, vector, IntVector2.Up, IntVector2.Left);
			}
			break;
		case 4:
			if (dungeon.data[vector4.ToIntVector2(VectorConversions.Floor) + new IntVector2(0, 2)].type == CellType.FLOOR)
			{
				AdjustTarget(ref target, vector, IntVector2.Up, IntVector2.Up);
			}
			else if (dungeon.data[vector2.ToIntVector2(VectorConversions.Floor) + new IntVector2(2, 0)].type == CellType.FLOOR)
			{
				AdjustTarget(ref target, vector, IntVector2.Right, IntVector2.Right);
			}
			else if (dungeon.data[vector3.ToIntVector2(VectorConversions.Floor) + new IntVector2(0, -2)].type == CellType.FLOOR)
			{
				AdjustTarget(ref target, vector, IntVector2.Down, IntVector2.Down);
			}
			else if (dungeon.data[vector5.ToIntVector2(VectorConversions.Floor) + new IntVector2(-2, 0)].type == CellType.FLOOR)
			{
				AdjustTarget(ref target, vector, IntVector2.Left, IntVector2.Left);
			}
			break;
		}
		m_targetLandPosition = target.ToVector3ZUp() - m_offset;
		m_targetLandPosition.z = m_targetLandPosition.y;
	}

	private void AdjustTarget(ref Vector2 target, Vector2 extents, params IntVector2[] dir)
	{
		for (int i = 0; i < dir.Length; i++)
		{
			if (dir[i] == IntVector2.Up)
			{
				target.y = (float)((int)(target.y - extents.y) + 1) + extents.y + PhysicsEngine.Instance.PixelUnitWidth;
			}
			if (dir[i] == IntVector2.Down)
			{
				target.y = (float)(int)(target.y + extents.y) - extents.y - PhysicsEngine.Instance.PixelUnitWidth;
			}
			if (dir[i] == IntVector2.Left)
			{
				target.x = (float)(int)(target.x + extents.x) - extents.x - PhysicsEngine.Instance.PixelUnitWidth;
			}
			if (dir[i] == IntVector2.Right)
			{
				target.x = (float)((int)(target.x - extents.x) + 1) + extents.x + PhysicsEngine.Instance.PixelUnitWidth;
			}
		}
	}
}
