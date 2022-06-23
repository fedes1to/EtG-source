using System;
using FullInspector;
using Pathfinding;
using UnityEngine;

public class TeleportBehavior : BasicAttackBehavior
{
	public enum ShadowSupport
	{
		None,
		Fade,
		Animate
	}

	private enum TeleportState
	{
		None,
		TeleportOut,
		Gone,
		GoneBehavior,
		TeleportIn
	}

	public bool AttackableDuringAnimation;

	public bool AvoidWalls;

	public bool StayOnScreen = true;

	public float MinDistanceFromPlayer = 4f;

	public float MaxDistanceFromPlayer = -1f;

	public float GoneTime = 1f;

	[InspectorCategory("Conditions")]
	public bool OnlyTeleportIfPlayerUnreachable;

	[InspectorCategory("Attack")]
	public BulletScriptSelector teleportOutBulletScript;

	[InspectorCategory("Attack")]
	public BulletScriptSelector teleportInBulletScript;

	[InspectorCategory("Attack")]
	public AttackBehaviorBase goneAttackBehavior;

	[InspectorCategory("Attack")]
	public bool AllowCrossRoomTeleportation;

	[InspectorCategory("Visuals")]
	public string teleportOutAnim = "teleport_out";

	[InspectorCategory("Visuals")]
	public string teleportInAnim = "teleport_in";

	[InspectorCategory("Visuals")]
	public bool teleportRequiresTransparency;

	[InspectorCategory("Visuals")]
	public bool hasOutlinesDuringAnim = true;

	[InspectorCategory("Visuals")]
	public ShadowSupport shadowSupport;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowShadowAnimationNames")]
	public string shadowOutAnim;

	[InspectorShowIf("ShowShadowAnimationNames")]
	[InspectorCategory("Visuals")]
	public string shadowInAnim;

	public bool ManuallyDefineRoom;

	[InspectorShowIf("ManuallyDefineRoom")]
	[InspectorIndent]
	public Vector2 roomMin;

	[InspectorShowIf("ManuallyDefineRoom")]
	[InspectorIndent]
	public Vector2 roomMax;

	private tk2dBaseSprite m_shadowSprite;

	private Shader m_cachedShader;

	private float m_timer;

	private bool m_shouldFire;

	private TeleportState m_state;

	private TeleportState State
	{
		get
		{
			return m_state;
		}
		set
		{
			EndState(m_state);
			m_state = value;
			BeginState(m_state);
		}
	}

	private bool ShowShadowAnimationNames()
	{
		return shadowSupport == ShadowSupport.Animate;
	}

	public override void Start()
	{
		base.Start();
		tk2dSpriteAnimator spriteAnimator = m_aiActor.spriteAnimator;
		spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimationEventTriggered));
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_timer);
		if (goneAttackBehavior != null)
		{
			goneAttackBehavior.Upkeep();
		}
	}

	public override bool IsReady()
	{
		if (OnlyTeleportIfPlayerUnreachable && m_aiActor.GetAbsoluteParentRoom() == GameManager.Instance.BestActivePlayer.CurrentRoom && m_aiActor.Path != null && m_aiActor.Path.WillReachFinalGoal)
		{
			return false;
		}
		return base.IsReady();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (m_shadowSprite == null)
		{
			m_shadowSprite = m_aiActor.ShadowObject.GetComponent<tk2dBaseSprite>();
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		State = TeleportState.TeleportOut;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State == TeleportState.TeleportOut)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f - m_aiAnimator.CurrentClipProgress);
			}
			if (!m_aiAnimator.IsPlaying(teleportOutAnim))
			{
				State = TeleportState.Gone;
			}
		}
		else if (State == TeleportState.Gone)
		{
			if (m_timer <= 0f)
			{
				State = TeleportState.GoneBehavior;
			}
		}
		else if (State == TeleportState.GoneBehavior)
		{
			if (goneAttackBehavior.ContinuousUpdate() == ContinuousBehaviorResult.Finished)
			{
				State = TeleportState.TeleportIn;
			}
		}
		else if (State == TeleportState.TeleportIn)
		{
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(m_aiAnimator.CurrentClipProgress);
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TeleportBehavior");
			}
			if (!m_aiAnimator.IsPlaying(teleportInAnim))
			{
				State = TeleportState.None;
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (teleportRequiresTransparency && (bool)m_cachedShader)
		{
			m_aiActor.sprite.usesOverrideMaterial = false;
			m_aiActor.renderer.material.shader = m_cachedShader;
			m_cachedShader = null;
		}
		m_aiActor.sprite.renderer.enabled = true;
		if ((bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "teleport");
		}
		m_aiActor.specRigidbody.CollideWithOthers = true;
		m_aiActor.IsGone = false;
		if ((bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "TeleportBehavior");
		}
		if (!hasOutlinesDuringAnim)
		{
			SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
		}
		if (goneAttackBehavior != null && State == TeleportState.GoneBehavior)
		{
			goneAttackBehavior.EndContinuousUpdate();
		}
		m_aiAnimator.EndAnimationIf(teleportOutAnim);
		m_aiAnimator.EndAnimationIf(teleportInAnim);
		if (shadowSupport == ShadowSupport.Fade)
		{
			m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
		}
		else if (shadowSupport == ShadowSupport.Animate)
		{
			tk2dSpriteAnimationClip clipByName = m_shadowSprite.spriteAnimator.GetClipByName(shadowInAnim);
			m_shadowSprite.spriteAnimator.Play(clipByName, clipByName.frames.Length - 1, clipByName.fps);
		}
		m_state = TeleportState.None;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		if (goneAttackBehavior != null)
		{
			goneAttackBehavior.Init(gameObject, aiActor, aiShooter);
		}
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		if (goneAttackBehavior != null)
		{
			goneAttackBehavior.SetDeltaTime(deltaTime);
		}
	}

	public override bool UpdateEveryFrame()
	{
		if (goneAttackBehavior != null && m_state == TeleportState.GoneBehavior)
		{
			return goneAttackBehavior.UpdateEveryFrame();
		}
		return base.UpdateEveryFrame();
	}

	public void AnimationEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frame)
	{
		if (m_shouldFire && clip.GetFrame(frame).eventInfo == "fire")
		{
			if (State == TeleportState.TeleportIn)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, teleportInBulletScript);
			}
			else if (State == TeleportState.TeleportOut)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, teleportOutBulletScript);
			}
			m_shouldFire = false;
		}
		else if (State == TeleportState.TeleportOut && clip.GetFrame(frame).eventInfo == "teleport_collider_off")
		{
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
		}
	}

	private void BeginState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
			if (teleportOutBulletScript != null && !teleportOutBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			if (teleportRequiresTransparency)
			{
				m_cachedShader = m_aiActor.renderer.material.shader;
				m_aiActor.sprite.usesOverrideMaterial = true;
				m_aiActor.renderer.material.shader = ShaderCache.Acquire("Brave/LitBlendUber");
			}
			m_aiAnimator.PlayUntilCancelled(teleportOutAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowOutAnim, m_aiAnimator.CurrentClipLength);
			}
			if ((bool)m_aiActor.knockbackDoer)
			{
				m_aiActor.knockbackDoer.SetImmobile(true, "teleport");
			}
			m_aiActor.ClearPath();
			if (!AttackableDuringAnimation)
			{
				m_aiActor.specRigidbody.CollideWithOthers = false;
				m_aiActor.IsGone = true;
			}
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TeleportBehavior");
			}
			if (!hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			}
			return;
		case TeleportState.Gone:
			if (GoneTime <= 0f)
			{
				State = TeleportState.GoneBehavior;
				return;
			}
			m_timer = GoneTime;
			m_aiActor.specRigidbody.CollideWithOthers = false;
			m_aiActor.IsGone = true;
			m_aiActor.sprite.renderer.enabled = false;
			return;
		}
		if (State == TeleportState.GoneBehavior)
		{
			if (goneAttackBehavior == null)
			{
				State = TeleportState.TeleportIn;
				return;
			}
			BehaviorResult behaviorResult = goneAttackBehavior.Update();
			if (behaviorResult != BehaviorResult.RunContinuous && behaviorResult != BehaviorResult.RunContinuousInClass)
			{
				State = TeleportState.TeleportIn;
			}
		}
		else if (state == TeleportState.TeleportIn)
		{
			if (teleportInBulletScript != null && !teleportInBulletScript.IsNull)
			{
				m_shouldFire = true;
			}
			DoTeleport();
			m_aiAnimator.PlayUntilFinished(teleportInAnim, true);
			if (shadowSupport == ShadowSupport.Animate)
			{
				m_shadowSprite.spriteAnimator.PlayAndForceTime(shadowInAnim, m_aiAnimator.CurrentClipLength);
			}
			m_shadowSprite.renderer.enabled = true;
			if (AttackableDuringAnimation)
			{
				m_aiActor.specRigidbody.CollideWithOthers = true;
				m_aiActor.IsGone = false;
			}
			m_aiActor.sprite.renderer.enabled = true;
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "TeleportBehavior");
			}
			if (hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			}
		}
	}

	private void EndState(TeleportState state)
	{
		switch (state)
		{
		case TeleportState.TeleportOut:
			m_shadowSprite.renderer.enabled = false;
			if (hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, false);
			}
			if (teleportOutBulletScript != null && !teleportOutBulletScript.IsNull && m_shouldFire)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, teleportOutBulletScript);
				m_shouldFire = false;
			}
			break;
		case TeleportState.TeleportIn:
			if (teleportRequiresTransparency && (bool)m_cachedShader)
			{
				m_aiActor.sprite.usesOverrideMaterial = false;
				m_aiActor.renderer.material.shader = m_cachedShader;
				m_cachedShader = null;
			}
			if (shadowSupport == ShadowSupport.Fade)
			{
				m_shadowSprite.color = m_shadowSprite.color.WithAlpha(1f);
			}
			if ((bool)m_aiActor.knockbackDoer)
			{
				m_aiActor.knockbackDoer.SetImmobile(false, "teleport");
			}
			m_aiActor.specRigidbody.CollideWithOthers = true;
			m_aiActor.IsGone = false;
			if ((bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(true, "TeleportBehavior");
			}
			if (teleportInBulletScript != null && !teleportInBulletScript.IsNull && m_shouldFire)
			{
				SpawnManager.SpawnBulletScript(m_aiActor, teleportInBulletScript);
				m_shouldFire = false;
			}
			if (!hasOutlinesDuringAnim)
			{
				SpriteOutlineManager.ToggleOutlineRenderers(m_aiActor.sprite, true);
			}
			break;
		}
	}

	private void DoTeleport()
	{
		float minDistanceFromPlayerSquared = MinDistanceFromPlayer * MinDistanceFromPlayer;
		float maxDistanceFromPlayerSquared = MaxDistanceFromPlayer * MaxDistanceFromPlayer;
		Vector2 playerLowerLeft = Vector2.zero;
		Vector2 playerUpperRight = Vector2.zero;
		bool hasOtherPlayer = false;
		Vector2 otherPlayerLowerLeft = Vector2.zero;
		Vector2 otherPlayerUpperRight = Vector2.zero;
		bool hasDistChecks = (MinDistanceFromPlayer > 0f || MaxDistanceFromPlayer > 0f) && (bool)m_aiActor.TargetRigidbody;
		if (hasDistChecks)
		{
			playerLowerLeft = m_aiActor.TargetRigidbody.HitboxPixelCollider.UnitBottomLeft;
			playerUpperRight = m_aiActor.TargetRigidbody.HitboxPixelCollider.UnitTopRight;
			PlayerController playerController = m_behaviorSpeculator.PlayerTarget as PlayerController;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && (bool)playerController)
			{
				PlayerController otherPlayer = GameManager.Instance.GetOtherPlayer(playerController);
				if ((bool)otherPlayer && otherPlayer.healthHaver.IsAlive)
				{
					hasOtherPlayer = true;
					otherPlayerLowerLeft = otherPlayer.specRigidbody.HitboxPixelCollider.UnitBottomLeft;
					otherPlayerUpperRight = otherPlayer.specRigidbody.HitboxPixelCollider.UnitTopRight;
				}
			}
		}
		IntVector2 bottomLeft = IntVector2.Zero;
		IntVector2 topRight = IntVector2.Zero;
		if (StayOnScreen)
		{
			bottomLeft = Vector2Extensions.ToIntVector2(BraveUtility.ViewportToWorldpoint(new Vector2(0f, 0f), ViewportType.Gameplay), VectorConversions.Ceil);
			topRight = Vector2Extensions.ToIntVector2(BraveUtility.ViewportToWorldpoint(new Vector2(1f, 1f), ViewportType.Gameplay), VectorConversions.Floor) - IntVector2.One;
		}
		CellValidator cellValidator = delegate(IntVector2 c)
		{
			for (int i = 0; i < m_aiActor.Clearance.x; i++)
			{
				int num = c.x + i;
				for (int j = 0; j < m_aiActor.Clearance.y; j++)
				{
					int num2 = c.y + j;
					if (GameManager.Instance.Dungeon.data.isTopWall(num, num2))
					{
						return false;
					}
					if (ManuallyDefineRoom && ((float)num < roomMin.x || (float)num > roomMax.x || (float)num2 < roomMin.y || (float)num2 > roomMax.y))
					{
						return false;
					}
				}
			}
			if (hasDistChecks)
			{
				PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
				Vector2 vector2 = new Vector2((float)c.x + 0.5f * ((float)m_aiActor.Clearance.x - hitboxPixelCollider.UnitWidth), c.y);
				Vector2 aMax = vector2 + hitboxPixelCollider.UnitDimensions;
				if (MinDistanceFromPlayer > 0f)
				{
					if (BraveMathCollege.AABBDistanceSquared(vector2, aMax, playerLowerLeft, playerUpperRight) < minDistanceFromPlayerSquared)
					{
						return false;
					}
					if (hasOtherPlayer && BraveMathCollege.AABBDistanceSquared(vector2, aMax, otherPlayerLowerLeft, otherPlayerUpperRight) < minDistanceFromPlayerSquared)
					{
						return false;
					}
				}
				if (MaxDistanceFromPlayer > 0f)
				{
					if (BraveMathCollege.AABBDistanceSquared(vector2, aMax, playerLowerLeft, playerUpperRight) > maxDistanceFromPlayerSquared)
					{
						return false;
					}
					if (hasOtherPlayer && BraveMathCollege.AABBDistanceSquared(vector2, aMax, otherPlayerLowerLeft, otherPlayerUpperRight) > maxDistanceFromPlayerSquared)
					{
						return false;
					}
				}
			}
			if (StayOnScreen && (c.x < bottomLeft.x || c.y < bottomLeft.y || c.x + m_aiActor.Clearance.x - 1 > topRight.x || c.y + m_aiActor.Clearance.y - 1 > topRight.y))
			{
				return false;
			}
			if (AvoidWalls)
			{
				int num3 = -1;
				int k;
				for (k = -1; k < m_aiActor.Clearance.y + 1; k++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num3, c.y + k))
					{
						return false;
					}
				}
				num3 = m_aiActor.Clearance.x;
				for (k = -1; k < m_aiActor.Clearance.y + 1; k++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num3, c.y + k))
					{
						return false;
					}
				}
				k = -1;
				for (num3 = -1; num3 < m_aiActor.Clearance.x + 1; num3++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num3, c.y + k))
					{
						return false;
					}
				}
				k = m_aiActor.Clearance.y;
				for (num3 = -1; num3 < m_aiActor.Clearance.x + 1; num3++)
				{
					if (GameManager.Instance.Dungeon.data.isWall(c.x + num3, c.y + k))
					{
						return false;
					}
				}
			}
			return true;
		};
		Vector2 vector = m_aiActor.specRigidbody.UnitBottomCenter - m_aiActor.transform.position.XY();
		IntVector2? intVector = null;
		intVector = ((!AllowCrossRoomTeleportation) ? m_aiActor.ParentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator) : GameManager.Instance.BestActivePlayer.CurrentRoom.GetRandomAvailableCell(m_aiActor.Clearance, m_aiActor.PathableTiles, false, cellValidator));
		if (intVector.HasValue)
		{
			m_aiActor.transform.position = Pathfinder.GetClearanceOffset(intVector.Value, m_aiActor.Clearance).WithY(intVector.Value.y) - vector;
			m_aiActor.specRigidbody.Reinitialize();
		}
		else
		{
			Debug.LogWarning("TELEPORT FAILED!", m_aiActor);
		}
	}
}
