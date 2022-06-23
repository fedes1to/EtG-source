using System;
using System.Collections.Generic;
using Dungeonator;
using Pathfinding;
using UnityEngine;

public class KeybulletFleeBehavior : MovementBehaviorBase
{
	private enum State
	{
		Idle,
		Fleeing,
		WaitingToDisappear,
		Disappearing
	}

	private const float c_screenXBuffer = 1f / 30f;

	private const float c_screenYBuffer = 2f / 27f;

	public float PathInterval = 0.25f;

	public float TimeOnScreenToFlee = 1.25f;

	public float FleeMoveSpeed = 9.5f;

	public float PreDisappearTime = 1f;

	public string DisappearAnimation;

	public bool ChangeColliderOnDisappear = true;

	public float BlackPhantomMultiplier = 1f;

	public float MinGoalDistFromPlayer = 10f;

	private float m_repathTimer;

	private float m_timer;

	private float m_onScreenTime;

	private float m_awakeTime;

	private IntVector2 m_targetPos;

	private Shader m_cachedShader;

	private tk2dSprite m_shadowSprite;

	private State m_state;

	private Vector2 m_playerPos;

	private Vector2? m_player2Pos;

	public override void Start()
	{
		base.Start();
		m_aiActor.healthHaver.OnDamaged += HandleDamaged;
		m_aiActor.healthHaver.OnPreDeath += OnPreDeath;
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(spriteAnimator.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
		tk2dSpriteAnimator spriteAnimator2 = m_aiActor.spriteAnimator;
		spriteAnimator2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(OnAnimationEvent));
		m_aiActor.DoDustUps = false;
		m_aiActor.IsWorthShootingAt = true;
	}

	private void OnAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (clip.GetFrame(frame).eventInfo == "blackPhantomPoof" && (bool)m_aiActor && m_aiActor.IsBlackPhantom)
		{
			DoBlackPhantomPoof();
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_repathTimer);
		DecrementTimer(ref m_timer);
		m_awakeTime += m_deltaTime;
	}

	public override BehaviorResult Update()
	{
		if (m_shadowSprite == null)
		{
			m_shadowSprite = m_aiActor.ShadowObject.GetComponent<tk2dSprite>();
		}
		if (m_state == State.Idle)
		{
			PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			if ((bool)mainCameraController && hitboxPixelCollider != null && (BraveUtility.PointIsVisible(hitboxPixelCollider.UnitTopCenter, -2f / 27f, ViewportType.Gameplay) || BraveUtility.PointIsVisible(hitboxPixelCollider.UnitBottomCenter, -2f / 27f, ViewportType.Gameplay) || BraveUtility.PointIsVisible(hitboxPixelCollider.UnitCenterLeft, -1f / 30f, ViewportType.Gameplay) || BraveUtility.PointIsVisible(hitboxPixelCollider.UnitCenterRight, -1f / 30f, ViewportType.Gameplay)))
			{
				m_onScreenTime += m_deltaTime;
				m_aiActor.ClearPath();
			}
			if (m_onScreenTime > TimeOnScreenToFlee || (m_onScreenTime > 0f && m_awakeTime < 1.5f))
			{
				Flee();
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
		}
		else if (m_state == State.Fleeing)
		{
			if (m_aiActor.PathComplete)
			{
				m_timer = PreDisappearTime;
				m_state = State.WaitingToDisappear;
				m_aiActor.SetResistance(EffectResistanceType.Freeze, 1f);
				m_aiActor.behaviorSpeculator.ImmuneToStun = true;
				if ((bool)m_aiActor.knockbackDoer)
				{
					m_aiActor.knockbackDoer.SetImmobile(true, "My people need me");
				}
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
			if (m_repathTimer <= 0f)
			{
				m_aiActor.PathfindToPosition(m_targetPos.ToCenterVector2());
				m_repathTimer = PathInterval;
			}
		}
		else if (m_state == State.WaitingToDisappear)
		{
			if (m_timer <= 0f)
			{
				if (!string.IsNullOrEmpty(DisappearAnimation))
				{
					if (m_aiActor.IsBlackPhantom)
					{
						m_aiAnimator.FpsScale *= BlackPhantomMultiplier;
					}
					m_aiAnimator.PlayUntilFinished(DisappearAnimation, true);
				}
				if (ChangeColliderOnDisappear)
				{
					List<PixelCollider> pixelColliders = m_aiActor.specRigidbody.PixelColliders;
					for (int i = 0; i < pixelColliders.Count; i++)
					{
						PixelCollider pixelCollider = pixelColliders[i];
						if (pixelCollider.Enabled && pixelCollider.CollisionLayer == CollisionLayer.EnemyHitBox)
						{
							pixelCollider.Enabled = false;
							break;
						}
					}
					for (int num = pixelColliders.Count - 1; num >= 0; num--)
					{
						PixelCollider pixelCollider2 = pixelColliders[num];
						if (!pixelCollider2.Enabled && pixelCollider2.CollisionLayer == CollisionLayer.EnemyHitBox)
						{
							pixelCollider2.Enabled = true;
							break;
						}
					}
				}
				if (!m_aiActor.IsBlackPhantom)
				{
					m_cachedShader = m_aiActor.renderer.material.shader;
					m_aiActor.sprite.usesOverrideMaterial = true;
					m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
					m_aiActor.renderer.material.SetFloat("_VertexColor", 1f);
				}
				m_aiActor.sprite.HeightOffGround = 1f;
				m_aiActor.sprite.UpdateZDepth();
				m_state = State.Disappearing;
			}
		}
		else if (m_state == State.Disappearing)
		{
			if (!m_aiActor.IsBlackPhantom)
			{
				float alpha = Mathf.Clamp01(Mathf.Lerp(1.5f, 0f, m_aiAnimator.CurrentClipProgress));
				m_aiActor.sprite.color = m_aiActor.sprite.color.WithAlpha(alpha);
				if ((bool)m_shadowSprite)
				{
					m_shadowSprite.color = m_aiActor.sprite.color.WithAlpha(alpha);
				}
			}
			if (!m_aiAnimator.IsPlaying(DisappearAnimation))
			{
				m_aiActor.EraseFromExistence(true);
			}
		}
		return (m_state != 0 || !(m_onScreenTime <= 0f)) ? BehaviorResult.SkipRemainingClassBehaviors : BehaviorResult.Continue;
	}

	private void HandleDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (m_state == State.Idle)
		{
			Flee();
		}
	}

	private void OnPreDeath(Vector2 obj)
	{
		m_aiActor.sprite.HeightOffGround = 1f;
		m_aiActor.sprite.UpdateZDepth();
		if (m_state == State.Disappearing && !m_aiActor.IsBlackPhantom)
		{
			m_aiActor.sprite.usesOverrideMaterial = false;
			m_aiActor.renderer.material.shader = m_cachedShader;
		}
	}

	private void OnAnimationCompleted(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip)
	{
		if (m_state == State.Disappearing)
		{
			m_aiActor.EraseFromExistence(true);
		}
	}

	private void Flee()
	{
		m_aiActor.ClearPath();
		m_aiActor.DoDustUps = true;
		IntVector2? fleePoint = GetFleePoint();
		if (fleePoint.HasValue)
		{
			m_targetPos = fleePoint.Value;
			if (FleeMoveSpeed > 0f)
			{
				m_aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(FleeMoveSpeed);
			}
			m_aiActor.PathfindToPosition(m_targetPos.ToCenterVector2());
			m_repathTimer = PathInterval;
		}
		m_state = State.Fleeing;
	}

	private IntVector2? GetFleePoint()
	{
		PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
		m_playerPos = bestActivePlayer.specRigidbody.UnitCenter;
		m_player2Pos = null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(bestActivePlayer);
			if ((bool)otherPlayer && (bool)otherPlayer.healthHaver && otherPlayer.healthHaver.IsAlive)
			{
				m_player2Pos = otherPlayer.specRigidbody.UnitCenter;
			}
		}
		FloodFillUtility.PreprocessContiguousCells(m_aiActor.ParentRoom, m_aiActor.specRigidbody.UnitCenter.ToIntVector2(VectorConversions.Floor));
		IntVector2? intVector = null;
		RoomHandler parentRoom = m_aiActor.ParentRoom;
		intVector = parentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, CellValidator);
		if (!intVector.HasValue)
		{
			intVector = parentRoom.GetRandomWeightedAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, null, CellWeighter);
		}
		return intVector;
	}

	private bool CellValidator(IntVector2 c)
	{
		if (!FloodFillUtility.WasFilled(c))
		{
			return false;
		}
		bool flag = false;
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 0; i < m_aiActor.Clearance.x; i++)
		{
			if (flag)
			{
				break;
			}
			for (int j = 0; j < m_aiActor.Clearance.y; j++)
			{
				if (flag)
				{
					break;
				}
				if (data.isWall(c.x + i - 1, c.y + j))
				{
					flag = true;
					break;
				}
				if (data.isWall(c.x + i + 1, c.y + j))
				{
					flag = true;
					break;
				}
				if (data.isWall(c.x + i, c.y + j + 1))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		Vector2 clearanceOffset = Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance);
		if (Vector2.Distance(clearanceOffset, m_playerPos) < MinGoalDistFromPlayer)
		{
			return false;
		}
		Vector2? player2Pos = m_player2Pos;
		if (player2Pos.HasValue && Vector2.Distance(clearanceOffset, m_player2Pos.Value) < MinGoalDistFromPlayer)
		{
			return false;
		}
		return true;
	}

	private float CellWeighter(IntVector2 c)
	{
		if (!FloodFillUtility.WasFilled(c))
		{
			return 0f;
		}
		bool flag = false;
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 0; i < m_aiActor.Clearance.x; i++)
		{
			if (flag)
			{
				break;
			}
			for (int j = 0; j < m_aiActor.Clearance.y; j++)
			{
				if (flag)
				{
					break;
				}
				if (data.isWall(c.x + i - 1, c.y + j))
				{
					flag = true;
					break;
				}
				if (data.isWall(c.x + i + 1, c.y + j))
				{
					flag = true;
					break;
				}
				if (data.isWall(c.x + i, c.y + j + 1))
				{
					flag = true;
					break;
				}
			}
		}
		bool flag2 = false;
		if (!flag)
		{
			for (int k = 0; k < m_aiActor.Clearance.x; k++)
			{
				if (flag)
				{
					break;
				}
				for (int l = 0; l < m_aiActor.Clearance.y; l++)
				{
					if (flag)
					{
						break;
					}
					if (data.isPit(c.x + k - 1, c.y + l))
					{
						flag2 = true;
						break;
					}
					if (data.isPit(c.x + k + 1, c.y + l))
					{
						flag2 = true;
						break;
					}
					if (data.isPit(c.x + k, c.y + l + 1))
					{
						flag2 = true;
						break;
					}
					if (data.isPit(c.x + k, c.y + l - 1))
					{
						flag2 = true;
						break;
					}
				}
			}
		}
		Vector2 clearanceOffset = Pathfinder.GetClearanceOffset(c, m_aiActor.Clearance);
		float num = Vector2.Distance(clearanceOffset, m_playerPos);
		Vector2? player2Pos = m_player2Pos;
		if (player2Pos.HasValue)
		{
			num = Mathf.Min(num, Vector2.Distance(clearanceOffset, m_player2Pos.Value));
		}
		return num + (float)(flag ? 100 : (flag2 ? 50 : 0));
	}

	private void DoBlackPhantomPoof()
	{
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW)
		{
			Vector3 vector = m_aiActor.specRigidbody.HitboxPixelCollider.UnitBottomLeft.ToVector3ZisY();
			Vector3 vector2 = m_aiActor.specRigidbody.HitboxPixelCollider.UnitTopRight.ToVector3ZisY();
			vector.z = vector.y - 6f;
			vector2.z = vector2.y - 6f;
			float num = (vector2.y - vector.y) * (vector2.x - vector.x);
			int num2 = (int)(50f * num);
			int num3 = num2;
			Vector3 minPosition = vector;
			Vector3 maxPosition = vector2;
			Vector3 direction = Vector3.up / 2f;
			float angleVariance = 120f;
			float magnitudeVariance = 0.2f;
			float? startLifetime = UnityEngine.Random.Range(1f, 1.65f);
			GlobalSparksDoer.DoRandomParticleBurst(num3, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
			num3 = num2;
			direction = vector;
			maxPosition = vector2;
			minPosition = Vector3.up / 2f;
			magnitudeVariance = 120f;
			angleVariance = 0.2f;
			startLifetime = UnityEngine.Random.Range(1f, 1.65f);
			GlobalSparksDoer.DoRandomParticleBurst(num3, direction, maxPosition, minPosition, magnitudeVariance, angleVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.DARK_MAGICKS);
			if (UnityEngine.Random.value < 0.5f)
			{
				num3 = 1;
				minPosition = vector;
				maxPosition = vector2.WithY(vector.y + 0.1f);
				direction = Vector3.right / 2f;
				angleVariance = 25f;
				magnitudeVariance = 0.2f;
				startLifetime = UnityEngine.Random.Range(1f, 1.65f);
				GlobalSparksDoer.DoRandomParticleBurst(num3, minPosition, maxPosition, direction, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
			}
			else
			{
				num3 = 1;
				direction = vector;
				maxPosition = vector2.WithY(vector.y + 0.1f);
				minPosition = Vector3.left / 2f;
				magnitudeVariance = 25f;
				angleVariance = 0.2f;
				startLifetime = UnityEngine.Random.Range(1f, 1.65f);
				GlobalSparksDoer.DoRandomParticleBurst(num3, direction, maxPosition, minPosition, magnitudeVariance, angleVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
			}
		}
	}
}
